namespace MeltySynth {
  using System.IO;

  internal struct Generator {
    private Generator(BinaryReader reader) {
      Type = (GeneratorType)reader.ReadUInt16();
      Value = reader.ReadUInt16();
    }

    internal static Generator[] ReadFromChunk(BinaryReader reader, int size) {
      if (size % 4 != 0) {
        throw new InvalidDataException("The generator list is invalid.");
      }

      var generators = new Generator[(size / 4) - 1];

      for (var i = 0; i < generators.Length; i++) {
        generators[i] = new Generator(reader);
      }

      // The last one is the terminator.
      new Generator(reader);

      return generators;
    }

    public GeneratorType Type { get; }
    public ushort Value { get; }
  }
}
