namespace AudioSynthesis.Midi.Event {
  using System;

  public class MidiEvent {
    protected int _time;
    protected int _message;

    public int DeltaTime {
      get => _time;
      set => _time = value;
    }
    public virtual int Channel => _message & 0x000000F;
    public virtual int Command => _message & 0x00000F0;
    public int Data1 => (_message & 0x000FF00) >> 8;
    public int Data2 => (_message & 0x0FF0000) >> 16;

    public MidiEvent(int delta, byte status, byte data1, byte data2) {
      _time = delta;
      _message = status | (data1 << 8) | (data2 << 16);
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
