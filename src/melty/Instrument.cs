namespace MeltySynth {
  using System;
  using System.Collections.Generic;
  using System.IO;

  /// <summary>
  /// Represents an instrument in the SoundFont.
  /// </summary>
  public sealed class Instrument {
    internal static readonly Instrument Default = new();

    private Instrument() {
      Name = "Default";
      RegionArray = Array.Empty<InstrumentRegion>();
    }

    private Instrument(InstrumentInfo info, Zone[] zones, SampleHeader[] samples) {
      Name = info.Name;

      var zoneCount = info.ZoneEndIndex - info.ZoneStartIndex + 1;
      if (zoneCount <= 0) {
        throw new InvalidDataException($"The instrument '{info.Name}' has no zone.");
      }

      var zoneSpan = zones.AsSpan(info.ZoneStartIndex, zoneCount);

      RegionArray = InstrumentRegion.Create(this, zoneSpan, samples);
    }

    internal static Instrument[] Create(InstrumentInfo[] infos, Zone[] zones, SampleHeader[] samples) {
      if (infos.Length <= 1) {
        throw new InvalidDataException("No valid instrument was found.");
      }

      // The last one is the terminator.
      var instruments = new Instrument[infos.Length - 1];

      for (var i = 0; i < instruments.Length; i++) {
        instruments[i] = new Instrument(infos[i], zones, samples);
      }

      return instruments;
    }

    /// <summary>
    /// Gets the name of the instrument.
    /// </summary>
    /// <returns>
    /// The name of the instrument.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// The name of the instrument.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The regions of the instrument.
    /// </summary>
    public IReadOnlyList<InstrumentRegion> Regions => RegionArray;

    // Internally exposes the raw array for fast enumeration.
    internal InstrumentRegion[] RegionArray { get; }
  }
}
