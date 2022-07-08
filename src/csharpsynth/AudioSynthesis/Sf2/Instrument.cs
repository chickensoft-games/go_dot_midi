namespace AudioSynthesis.Sf2 {
  public class Instrument {
    public string? Name { get; set; }
    public Zone[]? Zones { get; set; }

    public override string ToString() => Name;
  }
}
