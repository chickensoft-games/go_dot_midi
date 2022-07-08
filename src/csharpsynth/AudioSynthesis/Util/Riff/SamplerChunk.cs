namespace AudioSynthesis.Util.Riff {
  using System;
  using System.IO;

  public class SamplerChunk : Chunk {
    private readonly uint _smplMidiPitchFraction;

    //--Properties
    public int Manufacturer { get; }
    public int Product { get; }
    public int SamplePeriod { get; }
    public int UnityNote { get; }
    public double PitchFraction => _smplMidiPitchFraction / (double)0x80000000 / 2.0;
    public int SmpteFormat { get; }
    public int SmpteOffset { get; }
    public SampleLoop[] Loops { get; }
    public byte[] Data { get; }
    //--Methods
    public SamplerChunk(string id, int size, BinaryReader reader)
            : base(id, size) {
      Manufacturer = reader.ReadInt32();
      Product = reader.ReadInt32();
      SamplePeriod = reader.ReadInt32();
      UnityNote = reader.ReadInt32();
      _smplMidiPitchFraction = reader.ReadUInt32();
      SmpteFormat = reader.ReadInt32();
      SmpteOffset = reader.ReadInt32();
      var smplSampleLoops = reader.ReadInt32();
      var smplSamplerData = reader.ReadInt32();
      Loops = new SampleLoop[smplSampleLoops];
      for (var x = 0; x < Loops.Length; x++) {
        Loops[x] = new SampleLoop(reader);
      }
      Data = reader.ReadBytes(smplSamplerData);
      if (size % 2 == 1 && reader.PeekChar() == 0) {
        reader.ReadByte();
      }
    }
    //--Internal classes and structs
    public struct SampleLoop {
      public enum LoopType { Forward = 0, Alternating = 1, Reverse = 2, Unknown = 32 }

      private readonly int _sloopType;
      private readonly uint _sloopFraction;

      //--Properties
      public int CuePointId { get; }
      public LoopType Type {
        get {
          if (Enum.IsDefined(typeof(LoopType), _sloopType)) {
            return (LoopType)_sloopType;
          }

          return LoopType.Unknown;
        }
      }
      public int Start { get; }
      public int End { get; }
      public double Fraction => _sloopFraction / (double)0x80000000 / 2.0;
      public int Count { get; }
      //--Methods
      public SampleLoop(BinaryReader reader) {
        CuePointId = reader.ReadInt32();
        _sloopType = reader.ReadInt32();
        Start = reader.ReadInt32();
        End = reader.ReadInt32();
        _sloopFraction = reader.ReadUInt32();
        Count = reader.ReadInt32();
      }
    }
  }
}
