namespace AudioSynthesis.Util.Riff {
  using System.IO;

  public class InstrumentChunk : Chunk {
    private readonly sbyte instFineTune;
    private readonly sbyte instGain;

    //--Properties
    public byte Note { get; }
    public int FineTuneCents => instFineTune;
    public double Gain => instGain;
    public byte LowNote { get; }
    public byte HighNote { get; }
    public byte LowVelocity { get; }
    public byte HighVelocity { get; }
    //--Methods
    public InstrumentChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      Note = reader.ReadByte();
      instFineTune = reader.ReadSByte();
      instGain = reader.ReadSByte();
      LowNote = reader.ReadByte();
      HighNote = reader.ReadByte();
      LowVelocity = reader.ReadByte();
      HighVelocity = reader.ReadByte();
      reader.ReadByte(); //always read pad
    }
  }
}
