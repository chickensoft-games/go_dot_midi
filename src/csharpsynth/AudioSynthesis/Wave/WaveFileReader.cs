namespace AudioSynthesis.Wave {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AudioSynthesis.Util;
  using AudioSynthesis.Util.Riff;

  public sealed class WaveFileReader : IDisposable {
    //--Fields
    private BinaryReader _reader;
    //--Properties

    //--Methods
    public WaveFileReader(IResource waveFile) {
      if (!waveFile.ReadAllowed()) {
        throw new Exception("The file provided did not have read access.");
      }

      _reader = new BinaryReader(waveFile.OpenResourceForRead());
    }
    public WaveFileReader(Stream stream) => _reader = new BinaryReader(stream);

    public WaveFile ReadWaveFile() => new(WaveFileReader.ReadAllChunks(_reader));
    public Chunk[] ReadAllChunks() => WaveFileReader.ReadAllChunks(_reader);
    public Chunk ReadNextChunk() => WaveFileReader.ReadNextChunk(_reader);
    public void Close() => Dispose();
    public void Dispose() {
      if (_reader == null) {
        return;
      }

      _reader.Close();
      _reader = null!;
    }

    internal static Chunk[] ReadAllChunks(BinaryReader reader) {
      var offset = reader.BaseStream.Position + 8;
      var chunks = new List<Chunk>();
      var head = new RiffTypeChunk(new string(IOHelper.Read8BitChars(reader, 4)), reader.ReadInt32(), reader);
      if (!head.ChunkId.ToLower().Equals("riff") || !head.TypeId.ToLower().Equals("wave")) {
        throw new Exception("The asset could not be loaded because the RIFF chunk was missing or was not of type WAVE.");
      }

      while (reader.BaseStream.Position - offset < head.ChunkSize) {
        var chunk = ReadNextChunk(reader);
        if (chunk != null) {
          chunks.Add(chunk);
        }
      }
      return chunks.ToArray();
    }
    internal static Chunk ReadNextChunk(BinaryReader reader) {
      var id = new string(IOHelper.Read8BitChars(reader, 4));
      var size = reader.ReadInt32();
      return id.ToLower() switch {
        "riff" => new RiffTypeChunk(id, size, reader),
        "fact" => new FactChunk(id, size, reader),
        "data" => new DataChunk(id, size, reader),
        "fmt " => new FormatChunk(id, size, reader),
        "cue " => new CueChunk(id, size, reader),
        "plst" => new PlaylistChunk(id, size, reader),
        "list" => new ListChunk(id, size, reader, new Func<BinaryReader, Chunk>(ReadNextChunk)),
        "labl" => new LabelChunk(id, size, reader),
        "note" => new NoteChunk(id, size, reader),
        "ltxt" => new LabeledTextChunk(id, size, reader),
        "smpl" => new SamplerChunk(id, size, reader),
        "inst" => new InstrumentChunk(id, size, reader),
        _ => new UnknownChunk(id, size, reader),
      };
    }
    internal static WaveFile ReadWaveFile(BinaryReader reader) => new(ReadAllChunks(reader));
  }
}
