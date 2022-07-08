namespace AudioSynthesis.Sf2 {
  public class Zone {
    public Modulator[]? Modulators { get; set; }
    public Generator[]? Generators { get; set; }

    public override string ToString() => string.Format("Gens:{0} Mods:{1}", Generators == null ? 0 : Generators.Length, Modulators == null ? 0 : Modulators.Length);
  }
}
