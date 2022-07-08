namespace AudioSynthesis.Util.Riff {
  using System.IO;

  public class FactChunk : Chunk {
    //--Properties
    public int SampleCount { get; }

    //--Methods
    public FactChunk(string id, int size, BinaryReader reader)
            : base(id, size) => SampleCount = reader.ReadInt32();
  }
}
