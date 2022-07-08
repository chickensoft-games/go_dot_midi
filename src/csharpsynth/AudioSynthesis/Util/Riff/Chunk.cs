namespace AudioSynthesis.Util.Riff {
  public abstract class Chunk {
    //--Fields
    protected string _id;
    protected int _size;
    //--Properties
    public string ChunkId => _id;
    public int ChunkSize => _size;
    //--Methods
    public Chunk(string id, int size) {
      _id = id;
      _size = size;
    }
  }
}
