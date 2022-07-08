namespace AudioSynthesis.Sf2 {
  using System.IO;

  public class Modulator {
    private readonly ModulatorType sourceModulationData;
    private readonly GeneratorEnum destinationGenerator;
    private readonly short amount;
    private readonly ModulatorType sourceModulationAmount;
    private readonly TransformEnum sourceTransform;

    public Modulator(BinaryReader reader) {
      sourceModulationData = new ModulatorType(reader);
      destinationGenerator = (GeneratorEnum)reader.ReadUInt16();
      amount = reader.ReadInt16();
      sourceModulationAmount = new ModulatorType(reader);
      sourceTransform = (TransformEnum)reader.ReadUInt16();
    }
    public override string ToString() => string.Format("Modulator {0} : Amount: {1}", sourceModulationData, amount);
  }
}
