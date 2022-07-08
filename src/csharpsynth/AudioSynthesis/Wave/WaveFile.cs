namespace AudioSynthesis.Wave {
  using AudioSynthesis.Util.Riff;

  public class WaveFile {
    //--Properties
    public DataChunk Data { get; }
    public FormatChunk Format { get; }
    public Chunk[] Chunks { get; }
    //--Methods
    public WaveFile(Chunk[] chunks) {
      Chunks = chunks;
      Data = FindChunk<DataChunk>();
      Format = FindChunk<FormatChunk>();
    }
    public T FindChunk<T>(int startIndex = 0) where T : Chunk {
      for (var x = startIndex; x < Chunks.Length; x++) {
        if (Chunks[x] is T t) {
          return t;
        }
      }
      return default!;
    }
  }
}
