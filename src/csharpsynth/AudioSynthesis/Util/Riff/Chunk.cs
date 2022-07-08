namespace AudioSynthesis.Util.Riff {
  public abstract class Chunk {
    //--Fields
    protected string id;
    protected int size;
    //--Properties
    public string ChunkId => id;
    public int ChunkSize => size;
    //--Methods
    public Chunk(string id, int size) {
      this.id = id;
      this.size = size;
    }
  }
}
