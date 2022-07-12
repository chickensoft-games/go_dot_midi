namespace MeltySynth {
  using System;
  using System.Collections.Generic;
  using System.IO;

  /// <summary>
  /// Represents a preset in the SoundFont.
  /// </summary>
  public sealed class Preset {
    internal static readonly Preset Default = new();
    private readonly int library;
    private readonly int genre;
    private readonly int morphology;

    private Preset() {
      Name = "Default";
      RegionArray = Array.Empty<PresetRegion>();
    }

    private Preset(PresetInfo info, Zone[] zones, Instrument[] instruments) {
      Name = info.Name;
      PatchNumber = info.PatchNumber;
      BankNumber = info.BankNumber;
      library = info.Library;
      genre = info.Genre;
      morphology = info.Morphology;

      var zoneCount = info.ZoneEndIndex - info.ZoneStartIndex + 1;
      if (zoneCount <= 0) {
        throw new InvalidDataException($"The preset '{info.Name}' has no zone.");
      }

      var zoneSpan = zones.AsSpan(info.ZoneStartIndex, zoneCount);

      RegionArray = PresetRegion.Create(this, zoneSpan, instruments);
    }

    internal static Preset[] Create(PresetInfo[] infos, Zone[] zones, Instrument[] instruments) {
      if (infos.Length <= 1) {
        throw new InvalidDataException("No valid preset was found.");
      }

      // The last one is the terminator.
      var presets = new Preset[infos.Length - 1];

      for (var i = 0; i < presets.Length; i++) {
        presets[i] = new Preset(infos[i], zones, instruments);
      }

      return presets;
    }

    /// <summary>
    /// Gets the name of the preset.
    /// </summary>
    /// <returns>
    /// The name of the preset.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// The name of the preset.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The patch number of the preset.
    /// </summary>
    public int PatchNumber { get; }

    /// <summary>
    /// The bank number of the preset.
    /// </summary>
    public int BankNumber { get; }

    /// <summary>
    /// The regions of the preset.
    /// </summary>
    public IReadOnlyList<PresetRegion> Regions => RegionArray;

    // Internally exposes the raw array for fast enumeration.
    internal PresetRegion[] RegionArray { get; }
  }
}
