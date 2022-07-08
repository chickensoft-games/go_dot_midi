namespace AudioSynthesis.Sf2 {
  public class PresetHeader {
    private ushort patchNumber;
    private ushort bankNumber;

    public string? Name { get; set; }
    public int PatchNumber {
      get => patchNumber;
      set => patchNumber = (ushort)value;
    }
    public int BankNumber {
      get => bankNumber;
      set => bankNumber = (ushort)value;
    }
    public int Library { get; set; }
    public int Genre { get; set; }
    public int Morphology { get; set; }
    public Zone[]? Zones { get; set; }

    public override string ToString() => string.Format("{0}-{1} {2}", bankNumber, patchNumber, Name);
  }
}
