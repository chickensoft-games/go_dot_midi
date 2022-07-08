namespace AudioSynthesis.Sf2 {
  using System;
  using System.IO;
  using AudioSynthesis.Sf2.Chunks;
  using AudioSynthesis.Util;

  public class SoundFontPresets {
    public SampleHeader[] SampleHeaders { get; } = null!;
    public PresetHeader[] PresetHeaders { get; }
    public Instrument[] Instruments { get; }

    public SoundFontPresets(BinaryReader reader) {
      var id = new string(IOHelper.Read8BitChars(reader, 4));
      var size = reader.ReadInt32();
      if (!id.ToLower().Equals("list")) {
        throw new Exception("Invalid soundfont. Could not find pdta LIST chunk.");
      }

      var readTo = reader.BaseStream.Position + size;
      id = new string(IOHelper.Read8BitChars(reader, 4));
      if (!id.ToLower().Equals("pdta")) {
        throw new Exception("Invalid soundfont. The LIST chunk is not of type pdta.");
      }

      Modulator[] presetModulators = null!;
      Generator[] presetGenerators = null!;
      Modulator[] instrumentModulators = null!;
      Generator[] instrumentGenerators = null!;

      ZoneChunk pbag = null!;
      ZoneChunk ibag = null!;
      PresetHeaderChunk phdr = null!;
      InstrumentChunk inst = null!;

      while (reader.BaseStream.Position < readTo) {
        id = new string(IOHelper.Read8BitChars(reader, 4));
        size = reader.ReadInt32();

        switch (id.ToLower()) {
          case "phdr":
            phdr = new PresetHeaderChunk(id, size, reader);
            break;
          case "pbag":
            pbag = new ZoneChunk(id, size, reader);
            break;
          case "pmod":
            presetModulators = new ModulatorChunk(id, size, reader).Modulators;
            break;
          case "pgen":
            presetGenerators = new GeneratorChunk(id, size, reader).Generators;
            break;
          case "inst":
            inst = new InstrumentChunk(id, size, reader);
            break;
          case "ibag":
            ibag = new ZoneChunk(id, size, reader);
            break;
          case "imod":
            instrumentModulators = new ModulatorChunk(id, size, reader).Modulators;
            break;
          case "igen":
            instrumentGenerators = new GeneratorChunk(id, size, reader).Generators;
            break;
          case "shdr":
            SampleHeaders = new SampleHeaderChunk(id, size, reader).SampleHeaders;
            break;
          default:
            throw new Exception("Invalid soundfont. Unrecognized sub chunk: " + id);
        }
      }
      var pZones = pbag.ToZones(presetModulators, presetGenerators);
      PresetHeaders = phdr.ToPresets(pZones);
      var iZones = ibag.ToZones(instrumentModulators, instrumentGenerators);
      Instruments = inst.ToInstruments(iZones);
    }
  }
}
