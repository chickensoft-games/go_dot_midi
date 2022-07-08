namespace AudioSynthesis.Midi.Event {
  public class MetaDataEvent : MetaEvent {
    public byte[] Data { get; }
    public MetaDataEvent(int delta, byte status, byte metaId, byte[] data)
            : base(delta, status, metaId, 0) => Data = data;
  }
}
