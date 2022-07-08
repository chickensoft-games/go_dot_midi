namespace AudioSynthesis.Sf2 {
  public class PresetHeader {
    private ushort _patchNumber;
    private ushort _bankNumber;

    public string? Name { get; set; }
    public int PatchNumber {
      get => _patchNumber;
      set => _patchNumber = (ushort)value;
    }
    public int BankNumber {
      get => _bankNumber;
      set => _bankNumber = (ushort)value;
    }
    public int Library { get; set; }
    public int Genre { get; set; }
    public int Morphology { get; set; }
    public Zone[]? Zones { get; set; }

    public override string ToString() => string.Format("{0}-{1} {2}", _bankNumber, _patchNumber, Name);
  }
}
