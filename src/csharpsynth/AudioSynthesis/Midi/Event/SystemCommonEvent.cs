namespace AudioSynthesis.Midi.Event {
  using System;

  public class SystemCommonEvent : MidiEvent {
    public override int Channel => -1;
    public override int Command => _message & 0x00000FF;
    public SystemCommonEvent(int delta, byte status, byte data1, byte data2)
            : base(delta, status, data1, data2) { }
    public override string ToString() => "SystemCommon: " + Enum.GetName(typeof(SystemCommonTypeEnum), Command);
  }
}
