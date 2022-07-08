namespace AudioSynthesis.Midi.Event {
  using System;

  public class MidiEvent {
    protected int time;
    protected int message;

    public int DeltaTime {
      get => time;
      set => time = value;
    }
    public virtual int Channel => message & 0x000000F;
    public virtual int Command => message & 0x00000F0;
    public int Data1 => (message & 0x000FF00) >> 8;
    public int Data2 => (message & 0x0FF0000) >> 16;

    public MidiEvent(int delta, byte status, byte data1, byte data2) {
      time = delta;
      message = status | (data1 << 8) | (data2 << 16);
    }
    public override string ToString() {
      var value = "MidiEvent: " + Enum.GetName(typeof(MidiEventTypeEnum), Command);
      if (Command == 0xB0) {
        value += "(" + Enum.GetName(typeof(ControllerTypeEnum), Data1) + ")";
      }

      return value;
    }
  }
}
