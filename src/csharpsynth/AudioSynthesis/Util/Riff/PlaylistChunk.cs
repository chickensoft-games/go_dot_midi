namespace AudioSynthesis.Util.Riff {
  using System.Collections.Generic;
  using System.IO;

  public class PlaylistChunk : Chunk {
    //--Fields
    private readonly Segment[] _segments;
    //--Properties
    public IList<Segment> SegmentList => _segments;
    //--Methods
    public PlaylistChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      var segCount = reader.ReadInt32();
      _segments = new Segment[segCount];
      for (var x = 0; x < _segments.Length; x++) {
        _segments[x] = new Segment(reader);
      }
    }
    //--Internal classes and structs
    public class Segment {

      //--Properties
      public int CuePointId { get; }
      public int SampleLength { get; }
      public int RepeatCount { get; }
      //--Methods
      public Segment(BinaryReader reader) {
        CuePointId = reader.ReadInt32();
        SampleLength = reader.ReadInt32();
        RepeatCount = reader.ReadInt32();
      }
    }
  }
}
