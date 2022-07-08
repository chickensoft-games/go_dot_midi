namespace AudioSynthesis.Sf2.Chunks {
  using System;
  using System.IO;
  using AudioSynthesis.Util.Riff;

  public class GeneratorChunk : Chunk {
    public Generator[] Generators { get; set; }

    public GeneratorChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      if (size % 4 != 0) {
        throw new Exception("Invalid SoundFont. The presetzone chunk was invalid.");
      }

      Generators = new Generator[(size / 4) - 1];
      for (var x = 0; x < Generators.Length; x++) {
        Generators[x] = new Generator(reader);
      }

      new Generator(reader); //terminal record
    }
  }
}
