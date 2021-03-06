namespace AudioSynthesis.Midi {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using AudioSynthesis.Midi.Event;
  using AudioSynthesis.Util;

  public class MidiFile {
    public enum TrackFormat { SingleTrack, MultiTrack, MultiSong }
    public enum TimeFormat { TicksPerBeat, FamesPerSecond }

    public int Division { get; private set; }
    public TrackFormat MidiFormat { get; private set; }
    public TimeFormat TimingStandard { get; private set; }
    public MidiTrack[] Tracks { get; private set; } = new MidiTrack[0];

    public MidiFile(Stream stream) {
      using var reader = new BinaryReader(stream);
      LoadStream(reader);
    }
    public void CombineTracks() {
      //create a new track of the appropriate size
      var finalTrack = MergeTracks();
      var absevents = new MidiEvent[Tracks.Length][];
      //we have to convert delta times to absolute delta times
      for (var x = 0; x < absevents.Length; x++) {
        absevents[x] = new MidiEvent[Tracks[x].MidiEvents.Length];
        for (int x2 = 0, totalDeltaTime = 0; x2 < absevents[x].Length; x2++) {//create copies
          absevents[x][x2] = Tracks[x].MidiEvents[x2];
          totalDeltaTime += absevents[x][x2].DeltaTime;
          absevents[x][x2].DeltaTime = totalDeltaTime;
        }
      }
      //sort by absolute delta time also makes sure events occur in order of track and when they are received.
      var eventCount = 0;
      var delta = 0;
      var nextDelta = int.MaxValue;
      var counters = new int[absevents.Length];
      while (eventCount < finalTrack.MidiEvents.Length) {
        for (var x = 0; x < absevents.Length; x++) {
          while (counters[x] < absevents[x].Length && absevents[x][counters[x]].DeltaTime == delta) {
            finalTrack.MidiEvents[eventCount] = absevents[x][counters[x]];
            eventCount++;
            counters[x]++;
          }
          if (counters[x] < absevents[x].Length && absevents[x][counters[x]].DeltaTime < nextDelta) {
            nextDelta = absevents[x][counters[x]].DeltaTime;
          }
        }
        delta = nextDelta;
        nextDelta = int.MaxValue;
      }
      //set total time
      finalTrack.EndTime = finalTrack.MidiEvents[^1].DeltaTime;
      //put back into regular delta time
      for (int x = 0, deltaDiff = 0; x < finalTrack.MidiEvents.Length; x++) {
        var oldTime = finalTrack.MidiEvents[x].DeltaTime;
        finalTrack.MidiEvents[x].DeltaTime -= deltaDiff;
        deltaDiff = oldTime;
      }
      Tracks = new MidiTrack[] { finalTrack };
      MidiFormat = TrackFormat.SingleTrack;
    }

    private MidiTrack MergeTracks() {
      var eventCount = 0;
      var notesPlayed = 0;
      var activeChannels = 0;
      var programsUsed = new List<byte>();
      var drumProgramsUsed = new List<byte>();
      //Loop to get track info
      for (var x = 0; x < Tracks.Length; x++) {
        eventCount += Tracks[x].MidiEvents.Length;
        notesPlayed += Tracks[x].NoteOnCount;

        foreach (var p in Tracks[x].Instruments) {
          if (!programsUsed.Contains(p)) {
            programsUsed.Add(p);
          }
        }
        foreach (var p in Tracks[x].DrumInstruments) {
          if (!drumProgramsUsed.Contains(p)) {
            drumProgramsUsed.Add(p);
          }
        }
        activeChannels |= Tracks[x].ActiveChannels;
      }
      var track = new MidiTrack(programsUsed.ToArray(), drumProgramsUsed.ToArray(), new MidiEvent[eventCount]) {
        NoteOnCount = notesPlayed,
        ActiveChannels = activeChannels
      };
      return track;
    }
    private void LoadStream(BinaryReader reader) {
      if (!FindHead(reader, 500)) {
        throw new Exception("Invalid midi file : MThd chunk could not be found.");
      }

      ReadHeader(reader);
      try {
        for (var x = 0; x < Tracks.Length; x++) {
          Tracks[x] = ReadTrack(reader);
        }
      }
      catch (EndOfStreamException ex) {
        System.Diagnostics.Debug.WriteLine(ex.Message + "\nWarning: the midi file may not have one or more invalid tracks.");
        var emptyByte = new byte[0];
        var emptyEvents = new MidiEvent[0];
        for (var x = 0; x < Tracks.Length; x++) {
          if (Tracks[x] == null) {
            Tracks[x] = new MidiTrack(emptyByte, emptyByte, emptyEvents);
          }
        }
      }
    }
    private void ReadHeader(BinaryReader reader) {
      if (BigEndianHelper.ReadInt32(reader) != 6) //midi header should be 6 bytes long
{
        throw new Exception("Midi header is invalid.");
      }

      MidiFormat = (TrackFormat)BigEndianHelper.ReadInt16(reader);
      Tracks = new MidiTrack[BigEndianHelper.ReadInt16(reader)];
      int div = BigEndianHelper.ReadInt16(reader);
      Division = div & 0x7FFF;
      TimingStandard = ((div & 0x8000) > 0) ? TimeFormat.FamesPerSecond : TimeFormat.TicksPerBeat;
    }

    private static MidiTrack ReadTrack(BinaryReader reader) {
      var instList = new List<byte>();
      var drumList = new List<byte>();
      var eventList = new List<MidiEvent>();
      var channelList = 0;
      var noteOnCount = 0;
      var totalTime = 0;
      while (!new string(IOHelper.Read8BitChars(reader, 4)).Equals("MTrk")) {
        var length = BigEndianHelper.ReadInt32(reader);
        while (length > 0) {
          length--;
          reader.ReadByte();
        }
      }
      var endPosition = BigEndianHelper.ReadInt32(reader) + reader.BaseStream.Position;
      byte prevStatus = 0;
      while (reader.BaseStream.Position < endPosition) {
        var delta = ReadVariableLength(reader);
        totalTime += delta;
        var status = reader.ReadByte();
        if (status is >= 0x80 and <= 0xEF) {//voice message
          prevStatus = status;
          eventList.Add(ReadVoiceMessage(reader, delta, status, reader.ReadByte()));
          TrackVoiceStats(eventList[^1], instList, drumList, ref channelList, ref noteOnCount);
        }
        else if (status is >= 0xF0 and <= 0xF7) {//system common message
          prevStatus = 0;
          eventList.Add(ReadSystemCommonMessage(reader, delta, status));
        }
        else if (status is >= 0xF8 and <= 0xFF) {//realtime message
          eventList.Add(ReadRealTimeMessage(reader, delta, status));
        }
        else {//data bytes
          if (prevStatus == 0) {//if no running status continue to next status byte
            while ((status & 0x80) != 0x80) {
              status = reader.ReadByte();
            }

            if (status is >= 0x80 and <= 0xEF) {//voice message
              prevStatus = status;
              eventList.Add(ReadVoiceMessage(reader, delta, status, reader.ReadByte()));
              TrackVoiceStats(eventList[^1], instList, drumList, ref channelList, ref noteOnCount);
            }
            else if (status is >= 0xF0 and <= 0xF7) {//system common message
              eventList.Add(ReadSystemCommonMessage(reader, delta, status));
            }
            else if (status is >= 0xF8 and <= 0xFF) {//realtime message
              eventList.Add(ReadRealTimeMessage(reader, delta, status));
            }
          }
          else {//otherwise apply running status
            eventList.Add(ReadVoiceMessage(reader, delta, prevStatus, status));
            TrackVoiceStats(eventList[^1], instList, drumList, ref channelList, ref noteOnCount);
          }
        }
      }
      if (reader.BaseStream.Position != endPosition) {
        throw new Exception("The track length was invalid for the current MTrk chunk.");
      }

      if (((channelList >> MidiHelper.DRUM_CHANNEL) & 1) == 1) {
        if (!drumList.Contains(0)) {
          drumList.Add(0);
        }
      }
      else {
        if (!instList.Contains(0)) {
          instList.Add(0);
        }
      }
      var track = new MidiTrack(instList.ToArray(), drumList.ToArray(), eventList.ToArray()) {
        NoteOnCount = noteOnCount,
        EndTime = totalTime,
        ActiveChannels = channelList
      };
      return track;
    }
    private static MidiEvent ReadMetaMessage(BinaryReader reader, int delta, byte status) {
      var metaStatus = reader.ReadByte();
      switch (metaStatus) {
        case 0x0://sequence number
            {
            int count = reader.ReadByte();
            if (count == 0) {
              return new MetaNumberEvent(delta, status, metaStatus, -1); //current track
            }
            else if (count == 2) {
              return new MetaNumberEvent(delta, status, metaStatus, reader.ReadInt16());
            }
            else {
              throw new Exception("Invalid sequence number event.");
            }
          }
        case 0x1://text
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x2://copyright
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x3://track name
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x4://inst name
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x5://lyric
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x6://marker
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x7://cue point
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x8://patch name
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x9://port name
          return new MetaTextEvent(delta, status, metaStatus, ReadString(reader));
        case 0x20://midi channel
          if (reader.ReadByte() != 1) {
            throw new Exception("Invalid midi channel event. Expected size of 1.");
          }

          return new MetaEvent(delta, status, metaStatus, reader.ReadByte());
        case 0x21://midi port
          if (reader.ReadByte() != 1) {
            throw new Exception("Invalid midi port event. Expected size of 1.");
          }

          return new MetaEvent(delta, status, metaStatus, reader.ReadByte());
        case 0x2F://end of track
          return new MetaEvent(delta, status, metaStatus, reader.ReadByte());
        case 0x51://tempo
          if (reader.ReadByte() != 3) {
            throw new Exception("Invalid tempo event. Expected size of 3.");
          }

          return new MetaNumberEvent(delta, status, metaStatus, (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte());
        case 0x54://smpte
          if (reader.ReadByte() != 5) {
            throw new Exception("Invalid smpte event. Expected size of 5.");
          }

          return new MetaTextEvent(delta, status, metaStatus, reader.ReadByte() + ":" + reader.ReadByte() + ":" + reader.ReadByte() + ":" + reader.ReadByte() + ":" + reader.ReadByte());
        case 0x58://time sig
          if (reader.ReadByte() != 4) {
            throw new Exception("Invalid time signature event. Expected size of 4.");
          }

          return new MetaTextEvent(delta, status, metaStatus, reader.ReadByte() + ":" + reader.ReadByte() + ":" + reader.ReadByte() + ":" + reader.ReadByte());
        case 0x59://key sig
          if (reader.ReadByte() != 2) {
            throw new Exception("Invalid key signature event. Expected size of 2.");
          }

          return new MetaTextEvent(delta, status, metaStatus, reader.ReadByte() + ":" + reader.ReadByte());
        case 0x7F://seq specific
          return new MetaDataEvent(delta, status, metaStatus, reader.ReadBytes(ReadVariableLength(reader)));
        default:
          break;
      }
      throw new Exception("Not a valid meta message Status: " + status + " Meta: " + metaStatus);
    }

    private static MidiEvent ReadRealTimeMessage(BinaryReader reader, int delta, byte status) => status switch {
      //midi clock
      0xF8 => new RealTimeEvent(delta, status, 0, 0),
      //midi tick
      0xF9 => new RealTimeEvent(delta, status, 0, 0),
      //midi start
      0xFA => new RealTimeEvent(delta, status, 0, 0),
      //midi continue
      0xFB => new RealTimeEvent(delta, status, 0, 0),
      //midi stop
      0xFC => new RealTimeEvent(delta, status, 0, 0),
      //active sense
      0xFE => new RealTimeEvent(delta, status, 0, 0),
      //meta message
      0xFF => ReadMetaMessage(reader, delta, status),
      _ => throw new Exception("The real time message was invalid or unsupported : " + status),
    };

    private static MidiEvent ReadSystemCommonMessage(BinaryReader reader, int delta, byte status) {
      switch (status) {
        case 0xF7://sysEx (either or)
        case 0xF0://sysEx
            {
            short maker = reader.ReadByte();
            if (maker == 0x0) {
              maker = reader.ReadInt16();
            }
            else if (maker == 0xF7) {
              return null!;
            }

            var data = new List<byte>();
            var b = reader.ReadByte();
            while (b != 0xF7) {
              data.Add(b);
              b = reader.ReadByte();
            }
            return new SystemExclusiveEvent(delta, status, maker, data.ToArray());
          }
        case 0xF1://mtc quarter frame
          return new SystemCommonEvent(delta, status, reader.ReadByte(), 0);
        case 0xF2://song position
          return new SystemCommonEvent(delta, status, reader.ReadByte(), reader.ReadByte());
        case 0xF3://song select
          return new SystemCommonEvent(delta, status, reader.ReadByte(), 0);
        case 0xF6://tune request
          return new SystemCommonEvent(delta, status, 0, 0);
        default:
          throw new Exception("The system common message was invalid or unsupported : " + status);
      }
    }

    private static MidiEvent ReadVoiceMessage(BinaryReader reader, int delta, byte status, byte data1) {
      switch (status & 0xF0) {
        case 0x80: //NoteOff
          return new MidiEvent(delta, status, data1, reader.ReadByte());
        case 0x90: //NoteOn
          var velocity = reader.ReadByte();
          if (velocity == 0) //actually a note off event
{
            status = (byte)((status & 0x0F) | 0x80);
          }

          return new MidiEvent(delta, status, data1, velocity);
        case 0xA0: //AfterTouch
          return new MidiEvent(delta, status, data1, reader.ReadByte());
        case 0xB0: //ControlChange
          return new MidiEvent(delta, status, data1, reader.ReadByte());
        case 0xC0: //ProgramChange
          return new MidiEvent(delta, status, data1, 0);
        case 0xD0: //ChannelPressure
          return new MidiEvent(delta, status, data1, 0);
        case 0xE0: //PitchWheel
          return new MidiEvent(delta, status, data1, reader.ReadByte());
        default:
          throw new NotSupportedException("The status provided was not that of a voice message.");
      }
    }

    private static void TrackVoiceStats(MidiEvent midiEvent, List<byte> instList, List<byte> drumList, ref int channelList, ref int noteOnCount) {
      if (midiEvent.Command == 0x90) //note on
      {
        channelList |= 1 << midiEvent.Channel;
        noteOnCount++;
      }
      else if (midiEvent.Command == 0xC0) //prog change
      {
        var prog = (byte)midiEvent.Data1;
        if (midiEvent.Channel == MidiHelper.DRUM_CHANNEL && !drumList.Contains(prog)) {
          drumList.Add(prog);
        }
        else if (!instList.Contains(prog)) {
          instList.Add(prog);
        }
      }
    }

    private static int ReadVariableLength(BinaryReader reader) {
      var value = 0;
      int next;
      do {
        next = reader.ReadByte();
        value <<= 7;
        value |= next & 0x7F;
      } while ((next & 0x80) == 0x80);
      return value;
    }

    private static string ReadString(BinaryReader reader) {
      var length = ReadVariableLength(reader);
      return Encoding.UTF8.GetString(reader.ReadBytes(length), 0, length);
    }

    private static bool FindHead(BinaryReader reader, int attempts) {//Attempts to find the "MThd" midi header in a stream
      var match = 0;
      while (attempts > 0) {
        switch ((char)reader.ReadByte()) {
          case 'M':
            match = 1;
            break;
          case 'T':
            if (match == 1) {
              match = 2;
            }
            else {
              match = 0;
            }

            break;
          case 'h':
            if (match == 2) {
              match = 3;
            }
            else {
              match = 0;
            }

            break;
          case 'd':
            if (match == 3) {
              return true;
            }
            else {
              match = 0;
            }

            break;
          default:
            break;
        }
        attempts--;
      }
      return false;
    }
  }
}
