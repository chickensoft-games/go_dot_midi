namespace MeltySynth {
  using System.IO;

  internal sealed class ZoneInfo {
    private ZoneInfo() {
    }

    internal static ZoneInfo[] ReadFromChunk(BinaryReader reader, int size) {
      if (size % 4 != 0) {
        throw new InvalidDataException("The zone list is invalid.");
      }

      var count = size / 4;

      var zones = new ZoneInfo[count];

      for (var i = 0; i < count; i++) {
        var zone = new ZoneInfo {
          GeneratorIndex = reader.ReadUInt16(),
          ModulatorIndex = reader.ReadUInt16()
        };

        zones[i] = zone;
      }

      for (var i = 0; i < count - 1; i++) {
        zones[i].GeneratorCount = zones[i + 1].GeneratorIndex - zones[i].GeneratorIndex;
        zones[i].ModulatorCount = zones[i + 1].ModulatorIndex - zones[i].ModulatorIndex;
      }

      return zones;
    }

    public int GeneratorIndex { get; private set; }
    public int ModulatorIndex { get; private set; }
    public int GeneratorCount { get; private set; }
    public int ModulatorCount { get; private set; }
  }
}
