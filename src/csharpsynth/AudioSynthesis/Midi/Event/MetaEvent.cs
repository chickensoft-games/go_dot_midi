namespace AudioSynthesis.Midi.Event {
  using System;

  public class MetaEvent : MidiEvent {
    public override int Channel => -1;
    public override int Command => message & 0x00000FF;
    public int MetaStatus => Data1;
    public MetaEvent(int delta, byte status, byte data1, byte data2)
            : base(delta, status, data1, data2) { }
    public override string ToString() => "MetaEvent: " + Enum.GetName(typeof(MetaEventTypeEnum), Data1);
  }
}
