namespace AudioSynthesis.Util.Riff {
  using System.Collections.Generic;
  using System.IO;
  using AudioSynthesis.Util;

  public class CueChunk : Chunk {
    //--Fields
    private readonly CuePoint[] _cues;
    //--Properties
    public IList<CuePoint> CuePoints => _cues;
    //--Methods
    public CueChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      _cues = new CuePoint[reader.ReadInt32()];
      for (var x = 0; x < _cues.Length; x++) {
        _cues[x] = new CuePoint(reader);
      }
    }
    //--Internal classes and structs
    public class CuePoint {
      public int Id { get; }
      public int Position { get; }
      public string DataChunkId { get; }
      public int ChunkStart { get; }
      public int BlockStart { get; }
      public int SampleOffset { get; }

      public CuePoint(BinaryReader reader) {
        Id = reader.ReadInt32();
        Position = reader.ReadInt32();
        DataChunkId = new string(IOHelper.Read8BitChars(reader, 4));
        ChunkStart = reader.ReadInt32();
        BlockStart = reader.ReadInt32();
        SampleOffset = reader.ReadInt32();
      }
    }
  }
}
