namespace AudioSynthesis.Sf2 {
  using System.IO;

  public class Generator {
    private ushort _rawAmount;

    public Generator(BinaryReader reader) {
      GeneratorType = (GeneratorEnum)reader.ReadUInt16();
      _rawAmount = reader.ReadUInt16();
    }
    public GeneratorEnum GeneratorType { get; set; }
    public short AmountInt16 {
      get => (short)_rawAmount;
      set => _rawAmount = (ushort)value;
    }
    public byte LowByteAmount {
      get => (byte)(_rawAmount & 0x00FF);
      set {
        _rawAmount &= 0xFF00;
        _rawAmount += value;
      }
    }
    public byte HighByteAmount {
      get => (byte)((_rawAmount & 0xFF00) >> 8);
      set {
        _rawAmount &= 0x00FF;
        _rawAmount += (ushort)(value << 8);
      }
    }
    public override string ToString() => string.Format("Generator {0} {1}", GeneratorType, _rawAmount);
  }
}
