namespace AudioSynthesis.Bank {
  using System;
  using AudioSynthesis.Bank.Patches;

  public class PatchAsset {
    public string Name { get; }
    public Patch Patch { get; }

    public PatchAsset(string name, Patch patch) {
      Name = name ?? throw new ArgumentNullException("An asset must be given a valid name.");
      Patch = patch;
    }
    public override string ToString() {
      if (Patch == null) {
        return "null";
      }

      return Patch.ToString();
    }

  }
}
