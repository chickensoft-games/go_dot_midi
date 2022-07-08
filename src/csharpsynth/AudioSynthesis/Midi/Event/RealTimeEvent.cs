namespace AudioSynthesis.Midi.Event {
  public class RealTimeEvent : MidiEvent {
    public override int Channel => -1;
    public override int Command => message & 0x00000FF;
    public RealTimeEvent(int delta, byte status, byte data1, byte data2)
            : base(delta, status, data1, data2) { }
  }
}
