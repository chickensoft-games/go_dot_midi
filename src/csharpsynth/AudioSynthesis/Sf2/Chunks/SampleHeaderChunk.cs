namespace AudioSynthesis.Sf2.Chunks {
  using System;
  using System.IO;
  using AudioSynthesis.Util.Riff;

  public class SampleHeaderChunk : Chunk {
    public SampleHeader[] SampleHeaders { get; set; }

    public SampleHeaderChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      if (size % 46 != 0) {
        throw new Exception("Invalid SoundFont. The sample header chunk was invalid.");
      }

      SampleHeaders = new SampleHeader[(size / 46) - 1];
      for (var x = 0; x < SampleHeaders.Length; x++) {
        SampleHeaders[x] = new SampleHeader(reader);
      }
      new SampleHeader(reader); //read terminal record
    }
  }
}
