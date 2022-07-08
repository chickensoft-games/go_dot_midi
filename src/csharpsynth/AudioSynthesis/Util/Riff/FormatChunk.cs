namespace AudioSynthesis.Util.Riff {
  using System;
  using System.IO;

  public class FormatChunk : Chunk {
    //--Enum
    public enum CompressionCode {
      Unknown = 0x0000,
      Pcm = 0x0001,
      MicrosoftAdpcm = 0x0002,
      IeeeFloat = 0x0003,
      Alaw = 0x0006,
      Mulaw = 0x0007,
      Extensible = 0xFFFE,
      Experimental = 0xFFFF
    };
    //--Fields
    private readonly int _formatCompressionCode; //WORD
                                                 //--Properties

    public CompressionCode FormatCode {
      get {
        if (Enum.IsDefined(typeof(CompressionCode), _formatCompressionCode)) {
          return (CompressionCode)_formatCompressionCode;
        }

        return CompressionCode.Unknown;
      }
    }
    public short ChannelCount { get; }
    public int SampleRate { get; }
    public int AverageBytesPerSecond { get; }
    public short BlockAlign { get; }
    public short BitsPerSample { get; }
    public byte[] ExtendedData { get; } = null!;
    //--Methods
    public FormatChunk(string id, int size, BinaryReader reader)
        : base(id, size) {
      _formatCompressionCode = reader.ReadUInt16();
      ChannelCount = reader.ReadInt16();
      SampleRate = reader.ReadInt32();
      AverageBytesPerSecond = reader.ReadInt32();
      BlockAlign = reader.ReadInt16();
      BitsPerSample = reader.ReadInt16();
      if (size > 16 && _formatCompressionCode > (int)CompressionCode.Pcm) {
        ExtendedData = new byte[reader.ReadInt16()]; //read cb size
        reader.Read(ExtendedData, 0, ExtendedData.Length);
        if (ExtendedData.Length % 2 == 1 && reader.PeekChar() == 0) {
          reader.ReadByte();
        }
      }
    }
  }
}
