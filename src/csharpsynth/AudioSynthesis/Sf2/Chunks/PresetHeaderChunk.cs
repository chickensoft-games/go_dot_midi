
namespace AudioSynthesis.Sf2.Chunks {
  using System;
  using System.IO;
  using AudioSynthesis.Util;
  using AudioSynthesis.Util.Riff;
  public class PresetHeaderChunk : Chunk {
    private class RawPreset {
      public string Name = "";
      public ushort PatchNumber;
      public ushort BankNumber;
      public ushort StartPresetZoneIndex;
      public ushort EndPresetZoneIndex;
      public uint Library;
      public uint Genre;
      public uint Morphology;
    }

    private readonly RawPreset[] _rawPresets;

    public PresetHeaderChunk(string id, int size, BinaryReader reader)
        : base(id, size) {
      if (size % 38 != 0) {
        throw new Exception("Invalid SoundFont. The preset chunk was invalid.");
      }

      _rawPresets = new RawPreset[size / 38];
      RawPreset lastPreset = null!;
      for (var x = 0; x < _rawPresets.Length; x++) {
        var p = new RawPreset {
          Name = IOHelper.Read8BitString(reader, 20),
          PatchNumber = reader.ReadUInt16(),
          BankNumber = reader.ReadUInt16(),
          StartPresetZoneIndex = reader.ReadUInt16(),
          Library = reader.ReadUInt32(),
          Genre = reader.ReadUInt32(),
          Morphology = reader.ReadUInt32()
        };
        if (lastPreset != null) {
          lastPreset.EndPresetZoneIndex = (ushort)(p.StartPresetZoneIndex - 1);
        }

        _rawPresets[x] = p;
        lastPreset = p;
      }
    }

    public PresetHeader[] ToPresets(Zone[] presetZones) {
      var presets = new PresetHeader[_rawPresets.Length - 1];
      for (var x = 0; x < presets.Length; x++) {
        var rawPreset = _rawPresets[x];
        var p = new PresetHeader {
          BankNumber = rawPreset.BankNumber,
          Genre = (int)rawPreset.Genre,
          Library = (int)rawPreset.Library,
          Morphology = (int)rawPreset.Morphology,
          Name = rawPreset.Name,
          PatchNumber = rawPreset.PatchNumber,
          Zones = new Zone[rawPreset.EndPresetZoneIndex - rawPreset.StartPresetZoneIndex + 1]
        };
        Array.Copy(presetZones, rawPreset.StartPresetZoneIndex, p.Zones, 0, p.Zones.Length);
        presets[x] = p;
      }
      return presets;
    }
  }
}
