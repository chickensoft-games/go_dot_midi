namespace AudioSynthesis.Util.Riff {
  using System.IO;

  public class DataChunk : Chunk {
    //--Properties
    public byte[] RawSampleData { get; }
    //--Methods
    public DataChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      RawSampleData = reader.ReadBytes(size);
      if (size % 2 == 1 && reader.PeekChar() == 0) {
        reader.ReadByte();
      }
    }
  }
}
