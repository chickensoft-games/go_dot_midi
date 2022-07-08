namespace AudioSynthesis.Sf2 {
  using System.IO;

  public class Generator {
    private ushort rawAmount;

    public Generator(BinaryReader reader) {
      GeneratorType = (GeneratorEnum)reader.ReadUInt16();
      rawAmount = reader.ReadUInt16();
    }
    public GeneratorEnum GeneratorType { get; set; }
    public short AmountInt16 {
      get => (short)rawAmount;
      set => rawAmount = (ushort)value;
    }
    public byte LowByteAmount {
      get => (byte)(rawAmount & 0x00FF);
      set {
        rawAmount &= 0xFF00;
        rawAmount += value;
      }
    }
    public byte HighByteAmount {
      get => (byte)((rawAmount & 0xFF00) >> 8);
      set {
        rawAmount &= 0x00FF;
        rawAmount += (ushort)(value << 8);
      }
    }
    public override string ToString() => string.Format("Generator {0} {1}", GeneratorType, rawAmount);
  }
}
