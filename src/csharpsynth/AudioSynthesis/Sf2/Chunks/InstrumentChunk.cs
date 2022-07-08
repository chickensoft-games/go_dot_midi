namespace AudioSynthesis.Sf2.Chunks {
  using System;
  using System.IO;
  using AudioSynthesis.Util;
  using AudioSynthesis.Util.Riff;

  public class InstrumentChunk : Chunk {
    private class RawInstrument {
      public string Name = "";
      public ushort StartInstrumentZoneIndex;
      public ushort EndInstrumentZoneIndex;
    }

    private readonly RawInstrument[] _rawInstruments;

    public InstrumentChunk(string id, int size, BinaryReader reader)
        : base(id, size) {
      if (size % 22 != 0) {
        throw new Exception("Invalid SoundFont. The preset chunk was invalid.");
      }

      _rawInstruments = new RawInstrument[size / 22];
      RawInstrument lastInstrument = null!;
      for (var x = 0; x < _rawInstruments.Length; x++) {
        var i = new RawInstrument {
          Name = IOHelper.Read8BitString(reader, 20),
          StartInstrumentZoneIndex = reader.ReadUInt16()
        };
        if (lastInstrument != null) {
          lastInstrument.EndInstrumentZoneIndex = (ushort)(i.StartInstrumentZoneIndex - 1);
        }

        _rawInstruments[x] = i;
        lastInstrument = i;
      }
    }

    public Instrument[] ToInstruments(Zone[] zones) {
      var inst = new Instrument[_rawInstruments.Length - 1];
      for (var x = 0; x < inst.Length; x++) {
        var rawInst = _rawInstruments[x];
        var i = new Instrument {
          Name = rawInst.Name,
          Zones = new Zone[rawInst.EndInstrumentZoneIndex - rawInst.StartInstrumentZoneIndex + 1]
        };
        Array.Copy(zones, rawInst.StartInstrumentZoneIndex, i.Zones, 0, i.Zones.Length);
        inst[x] = i;
      }
      return inst;
    }

  }
}
