namespace AudioSynthesis.Sf2 {
  using System.IO;

  public class Modulator {
    private readonly ModulatorType _sourceModulationData;
    private readonly GeneratorEnum _destinationGenerator;
    private readonly short _amount;
    private readonly ModulatorType _sourceModulationAmount;
    private readonly TransformEnum _sourceTransform;

    public Modulator(BinaryReader reader) {
      _sourceModulationData = new ModulatorType(reader);
      _destinationGenerator = (GeneratorEnum)reader.ReadUInt16();
      _amount = reader.ReadInt16();
      _sourceModulationAmount = new ModulatorType(reader);
      _sourceTransform = (TransformEnum)reader.ReadUInt16();
    }
    public override string ToString() => string.Format("Modulator {0} : Amount: {1}", _sourceModulationData, _amount);
  }
}
