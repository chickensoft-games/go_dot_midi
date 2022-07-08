namespace AudioSynthesis.Util.Riff {
  using System.IO;
  using AudioSynthesis.Util;

  public class RiffTypeChunk : Chunk {
    //--Properties
    public string TypeId { get; }
    //--Methods
    public RiffTypeChunk(string id, int size, BinaryReader reader)
            : base(id, size) => TypeId = new string(IOHelper.Read8BitChars(reader, 4));
  }
}
