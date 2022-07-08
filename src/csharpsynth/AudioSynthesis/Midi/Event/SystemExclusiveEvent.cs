namespace AudioSynthesis.Midi.Event {
  public class SystemExclusiveEvent : SystemCommonEvent {
    public byte[] Data { get; }
    public int ManufacturerId => _message >> 8;
    public SystemExclusiveEvent(int delta, byte status, short id, byte[] data)
            : base(delta, status, (byte)(id & 0x00FF), (byte)(id >> 8)) => Data = data;
  }
}
