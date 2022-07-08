﻿namespace AudioSynthesis.Util.Riff {
  using System.IO;
  using AudioSynthesis.Util;

  public class NoteChunk : Chunk {
    //--Properties
    public int CuePointId { get; }
    public string Text { get; }
    //--Methods
    public NoteChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      CuePointId = reader.ReadInt32();
      Text = IOHelper.Read8BitString(reader);
      if (size % 2 == 1 && reader.PeekChar() == 0) {
        reader.ReadByte();
      }
    }
  }
}
