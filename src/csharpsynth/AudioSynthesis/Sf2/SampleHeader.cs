namespace AudioSynthesis.Sf2 {
  using System.IO;
  using AudioSynthesis.Util;

  public class SampleHeader {
    private readonly uint start;
    private readonly uint end;
    private readonly uint startLoop;
    private readonly uint endLoop;
    private readonly uint sampleRate;
    private readonly sbyte pitchCorrection;
    private readonly ushort sampleLink;
    private readonly SFSampleLink soundFontSampleLink;

    public string Name { get; }
    public int Start => (int)start;
    public int End => (int)end;
    public int StartLoop => (int)startLoop;
    public int EndLoop => (int)endLoop;
    public int SampleRate => (int)sampleRate;
    public byte RootKey { get; }
    public short Tune => pitchCorrection;

    public SampleHeader(BinaryReader reader) {
      Name = IOHelper.Read8BitString(reader, 20);
      start = reader.ReadUInt32();
      end = reader.ReadUInt32();
      startLoop = reader.ReadUInt32();
      endLoop = reader.ReadUInt32();
      sampleRate = reader.ReadUInt32();
      RootKey = reader.ReadByte();
      pitchCorrection = reader.ReadSByte();
      sampleLink = reader.ReadUInt16();
      soundFontSampleLink = (SFSampleLink)reader.ReadUInt16();
    }

    public override string ToString() => Name;
  }
}
