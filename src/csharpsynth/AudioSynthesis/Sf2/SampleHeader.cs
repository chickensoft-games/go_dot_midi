namespace AudioSynthesis.Sf2 {
  using System.IO;
  using AudioSynthesis.Util;

  public class SampleHeader {
    private readonly uint _start;
    private readonly uint _end;
    private readonly uint _startLoop;
    private readonly uint _endLoop;
    private readonly uint _sampleRate;
    private readonly sbyte _pitchCorrection;
    private readonly ushort _sampleLink;
    private readonly SFSampleLink _soundFontSampleLink;

    public string Name { get; }
    public int Start => (int)_start;
    public int End => (int)_end;
    public int StartLoop => (int)_startLoop;
    public int EndLoop => (int)_endLoop;
    public int SampleRate => (int)_sampleRate;
    public byte RootKey { get; }
    public short Tune => _pitchCorrection;

    public SampleHeader(BinaryReader reader) {
      Name = IOHelper.Read8BitString(reader, 20);
      _start = reader.ReadUInt32();
      _end = reader.ReadUInt32();
      _startLoop = reader.ReadUInt32();
      _endLoop = reader.ReadUInt32();
      _sampleRate = reader.ReadUInt32();
      RootKey = reader.ReadByte();
      _pitchCorrection = reader.ReadSByte();
      _sampleLink = reader.ReadUInt16();
      _soundFontSampleLink = (SFSampleLink)reader.ReadUInt16();
    }

    public override string ToString() => Name;
  }
}
