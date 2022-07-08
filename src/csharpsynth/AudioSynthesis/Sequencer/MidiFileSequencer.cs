/*
 *    ______   __ __     _____             __  __
 *   / ____/__/ // /_   / ___/__  ______  / /_/ /_
 *  / /    /_  _  __/   \__ \/ / / / __ \/ __/ __ \
 * / /___ /_  _  __/   ___/ / /_/ / / / / /_/ / / /
 * \____/  /_//_/     /____/\__, /_/ /_/\__/_/ /_/
 *                         /____/
 * Midi File Sequencer
 *  Used for situations where the whole midi is available in file format locally or over a network stream.
 *  Loads the midi and calculates the timing before hand so when sequencing no BPM calculation is needed.
 */

namespace AudioSynthesis.Sequencer {
  using System;
  using System.IO;
  using AudioSynthesis.Midi;
  using AudioSynthesis.Midi.Event;
  using AudioSynthesis.Synthesis;
  public class MidiFileSequencer {
    private MidiMessage[] _mdata = null!;
    private readonly bool[] _blockList;
    private double _playbackRate = 1.0; // 1/8 to 8
    private int _eventIndex;

    //--Public Properties
    public Synthesizer Synth { get; set; }
    public bool IsPlaying { get; private set; } = false;
    public bool IsMidiLoaded => _mdata != null;
    public int CurrentTime { get; private set; }
    public int EndTime { get; private set; }
    public double PlaySpeed {
      get => _playbackRate;
      set => _playbackRate = SynthHelper.Clamp(value, .125, 8.0);
    }

    //--Public Methods
    public MidiFileSequencer(Synthesizer synth) {
      Synth = synth;
      _blockList = new bool[Synthesizer.DEFAULT_CHANNEL_COUNT];
    }
    public bool LoadMidi(Stream midiFileStream) {
      if (IsPlaying) {
        return false;
      }

      LoadMidiFile(new MidiFile(midiFileStream));
      return true;
    }
    public bool LoadMidi(MidiFile midiFile) {
      if (IsPlaying) {
        return false;
      }

      LoadMidiFile(midiFile);
      return true;
    }
    public bool UnloadMidi() {
      if (IsPlaying) {
        return false;
      }

      _mdata = null!;
      return true;
    }
    public void Play() {
      if (IsPlaying || _mdata == null) {
        return;
      }

      IsPlaying = true;
    }
    public void Stop() {
      IsPlaying = false;
      CurrentTime = 0;
      _eventIndex = 0;
    }
    public bool IsChannelMuted(int channel) => _blockList[channel];
    public void MuteAllChannels() {
      for (var x = 0; x < _blockList.Length; x++) {
        _blockList[x] = true;
      }
    }
    public void UnMuteAllChannels() => Array.Clear(_blockList, 0, _blockList.Length);
    public void SetMute(int channel, bool muteValue) => _blockList[channel] = muteValue;
    public void Seek(TimeSpan time) {
      var targetSampleTime = (int)(Synth.SampleRate * time.TotalSeconds);
      if (targetSampleTime > CurrentTime) {//process forward
        SilentProcess(targetSampleTime - CurrentTime);
      }
      else if (targetSampleTime < CurrentTime) {//we have to restart the midi to make sure we get the right state: instruments, volume, pan, etc
        CurrentTime = 0;
        _eventIndex = 0;
        Synth.NoteOffAll(true);
        Synth.ResetPrograms();
        Synth.ResetSynthControls();
        SilentProcess(targetSampleTime);
      }
    }
    public void FillMidiEventQueue() {
      if (!IsPlaying || Synth.MidiEventQueue.Count != 0) {
        return;
      }

      if (CurrentTime >= EndTime) {
        CurrentTime = 0;
        _eventIndex = 0;
        IsPlaying = false;
        Synth.NoteOffAll(true);
        Synth.ResetPrograms();
        Synth.ResetSynthControls();
        return;
      }
      var newMSize = (int)(Synth.MicroBufferSize * _playbackRate);
      for (var x = 0; x < Synth.MidiEventCounts.Length; x++) {
        CurrentTime += newMSize;
        while (_eventIndex < _mdata.Length && _mdata[_eventIndex].Delta < CurrentTime) {
          if (_mdata[_eventIndex].Command != 0x90 || _blockList[_mdata[_eventIndex].Channel] == false) {
            Synth.MidiEventQueue.Enqueue(_mdata[_eventIndex]);
            Synth.MidiEventCounts[x]++;
          }
          _eventIndex++;
        }
      }
    }
    //--Private Methods
    private void LoadMidiFile(MidiFile midiFile) {
      //Converts midi to sample based format for easy sequencing
      var bpm = 120.0;
      //Combine all tracks into 1 track that is organized from lowest to highest absolute time
      if (midiFile.Tracks.Length > 1 || midiFile.Tracks[0].EndTime == 0) {
        midiFile.CombineTracks();
      }

      _mdata = new MidiMessage[midiFile.Tracks[0].MidiEvents.Length];
      //Convert delta time to sample time
      _eventIndex = 0;
      CurrentTime = 0;
      //Calculate sample based time using double counter and round down to nearest integer sample.
      var absDelta = 0.0;
      for (var x = 0; x < _mdata.Length; x++) {
        var mEvent = midiFile.Tracks[0].MidiEvents[x];
        _mdata[x] = new MidiMessage((byte)mEvent.Channel, (byte)mEvent.Command, (byte)mEvent.Data1, (byte)mEvent.Data2);
        absDelta += Synth.SampleRate * mEvent.DeltaTime * (60.0 / (bpm * midiFile.Division));
        _mdata[x].Delta = (int)absDelta;
        //Update tempo
        if (mEvent.Command == 0xFF && mEvent.Data1 == 0x51) {
          bpm = Math.Round(MidiHelper.MicroSecondsPerMinute / (double)((MetaNumberEvent)mEvent).Value, 2);
        }
      }
      //Set total time to proper value
      EndTime = _mdata[^1].Delta;
    }
    private void SilentProcess(int amount) {
      while (_eventIndex < _mdata.Length && _mdata[_eventIndex].Delta < (CurrentTime + amount)) {
        if (_mdata[_eventIndex].Command != 0x90) {
          var m = _mdata[_eventIndex];
          Synth.ProcessMidiMessage(m.Channel, m.Command, m.Data1, m.Data2);
        }
        _eventIndex++;
      }
      CurrentTime += amount;
    }
  }
}
