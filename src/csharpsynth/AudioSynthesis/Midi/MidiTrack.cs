namespace AudioSynthesis.Midi {
  using AudioSynthesis.Midi.Event;

  public class MidiTrack {
    public int NoteOnCount { get; set; }
    public int EndTime { get; set; }
    public int ActiveChannels { get; set; }
    public MidiEvent[] MidiEvents { get; }
    public byte[] Instruments { get; }
    public byte[] DrumInstruments { get; }

    public MidiTrack(byte[] instPrograms, byte[] drumPrograms, MidiEvent[] midiEvents) {
      Instruments = instPrograms;
      DrumInstruments = drumPrograms;
      MidiEvents = midiEvents;
      NoteOnCount = 0;
      EndTime = 0;
      ActiveChannels = 0;
    }
    public bool IsChannelActive(int channel) => ((ActiveChannels >> channel) & 1) == 1;
    public override string ToString() => "MessageCount: " + MidiEvents.Length + ", TotalTime: " + EndTime;
  }
}
