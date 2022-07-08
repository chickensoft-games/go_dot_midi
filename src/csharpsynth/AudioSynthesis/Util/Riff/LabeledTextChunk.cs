namespace AudioSynthesis.Util.Riff {
  using System.IO;
  using AudioSynthesis.Util;

  public class LabeledTextChunk : Chunk {
    //--Properties
    public int CuePointId { get; }
    public int SampleLength { get; }
    public int PurposeId { get; }
    public short Country { get; }
    public short Language { get; }
    public short Dialect { get; }
    public short CodePage { get; }
    public string Text { get; }
    //--Methods
    public LabeledTextChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      CuePointId = reader.ReadInt32();
      SampleLength = reader.ReadInt32();
      PurposeId = reader.ReadInt32();
      Country = reader.ReadInt16();
      Language = reader.ReadInt16();
      Dialect = reader.ReadInt16();
      CodePage = reader.ReadInt16();
      Text = IOHelper.Read8BitString(reader);
      if (size % 2 == 1 && reader.PeekChar() == 0) {
        reader.ReadByte();
      }
    }
  }
}
