namespace AudioSynthesis.Sf2.Chunks {
  using System;
  using System.IO;
  using AudioSynthesis.Util.Riff;

  public class ZoneChunk : Chunk {
    private class RawZoneData {
      public ushort GeneratorIndex;
      public ushort ModulatorIndex;
      public ushort GeneratorCount;
      public ushort ModulatorCount;
    }

    private readonly RawZoneData[] _zoneData;

    public ZoneChunk(string id, int size, BinaryReader reader)
        : base(id, size) {
      if (size % 4 != 0) {
        throw new Exception("Invalid SoundFont. The presetzone chunk was invalid.");
      }

      _zoneData = new RawZoneData[size / 4];
      RawZoneData lastZone = null!;
      for (var x = 0; x < _zoneData.Length; x++) {
        var z = new RawZoneData {
          GeneratorIndex = reader.ReadUInt16(),
          ModulatorIndex = reader.ReadUInt16()
        };
        if (lastZone != null) {
          lastZone.GeneratorCount = (ushort)(z.GeneratorIndex - lastZone.GeneratorIndex);
          lastZone.ModulatorCount = (ushort)(z.ModulatorIndex - lastZone.ModulatorIndex);
        }
        _zoneData[x] = z;
        lastZone = z;
      }
    }

    public Zone[] ToZones(Modulator[] modulators, Generator[] generators) {
      var zones = new Zone[_zoneData.Length - 1];
      for (var x = 0; x < zones.Length; x++) {
        var rawZone = _zoneData[x];
        var zone = new Zone {
          Generators = new Generator[rawZone.GeneratorCount]
        };
        Array.Copy(generators, rawZone.GeneratorIndex, zone.Generators, 0, rawZone.GeneratorCount);
        zone.Modulators = new Modulator[rawZone.ModulatorCount];
        Array.Copy(modulators, rawZone.ModulatorIndex, zone.Modulators, 0, rawZone.ModulatorCount);
        zones[x] = zone;
      }
      return zones;
    }
  }
}
