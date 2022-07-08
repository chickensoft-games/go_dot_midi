namespace AudioSynthesis.Midi.Event {
  public class MetaTextEvent : MetaEvent {
    public string Text { get; }
    public MetaTextEvent(int delta, byte status, byte metaId, string text)
            : base(delta, status, metaId, 0) => Text = text;
  }
}
