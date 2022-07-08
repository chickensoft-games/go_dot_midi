namespace AudioSynthesis.Midi.Event {
  public class MetaNumberEvent : MetaEvent {
    public int Value { get; }
    public MetaNumberEvent(int delta, byte status, byte metaId, int number)
            : base(delta, status, metaId, 0) => Value = number;
  }
}
