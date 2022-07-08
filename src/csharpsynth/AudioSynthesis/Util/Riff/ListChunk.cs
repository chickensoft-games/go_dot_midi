namespace AudioSynthesis.Util.Riff {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AudioSynthesis.Util;

  public class ListChunk : Chunk {
    //--Properties
    public string TypeId { get; }
    public Chunk[] SubChunks { get; }
    //--Methods
    public ListChunk(string id, int size, BinaryReader reader, Func<BinaryReader, Chunk> listCallback)
            : base(id, size) {
      var readTo = reader.BaseStream.Position + size;
      TypeId = new string(IOHelper.Read8BitChars(reader, 4));
      var chunkList = new List<Chunk>();
      while (reader.BaseStream.Position < readTo) {
        var chk = listCallback.Invoke(reader);
        chunkList.Add(chk);
      }
      SubChunks = chunkList.ToArray();
    }
  }
}
