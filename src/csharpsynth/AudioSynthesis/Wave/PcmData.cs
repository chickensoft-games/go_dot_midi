namespace AudioSynthesis.Wave {
  using System;

  public abstract class PcmData {
    protected byte[] _data;
    protected byte _bytes;
    protected int _length;

    public int Length => _length;
    public int BytesPerSample => _bytes;
    public int BitsPerSample => _bytes * 8;

    protected PcmData(int bits, byte[] pcmData, bool isDataInLittleEndianFormat) {
      _bytes = (byte)(bits / 8);
      if (pcmData.Length % _bytes != 0) {
        throw new Exception("Invalid PCM format. The PCM data was an invalid size.");
      }

      _data = pcmData;
      _length = _data.Length / _bytes;
      if (BitConverter.IsLittleEndian != isDataInLittleEndianFormat) {
        WaveHelper.SwapEndianess(_data, bits);
      }
    }
    public abstract float this[int index] { get; }

    public static PcmData Create(int bits, byte[] pcmData, bool isDataInLittleEndianFormat) => bits switch {
      8 => new PcmData8Bit(bits, pcmData, isDataInLittleEndianFormat),
      16 => new PcmData16Bit(bits, pcmData, isDataInLittleEndianFormat),
      24 => new PcmData24Bit(bits, pcmData, isDataInLittleEndianFormat),
      32 => new PcmData32Bit(bits, pcmData, isDataInLittleEndianFormat),
      _ => throw new Exception("Invalid PCM format. " + bits + "bit pcm data is not supported."),
    };
  }
  public class PcmData8Bit : PcmData {
    public PcmData8Bit(int bits, byte[] pcmData, bool isDataInLittleEndianFormat) : base(bits, pcmData, isDataInLittleEndianFormat) { }
    public override float this[int index] => (_data[index] / 255f * 2f) - 1f;
  }
  public class PcmData16Bit : PcmData {
    public PcmData16Bit(int bits, byte[] pcmData, bool isDataInLittleEndianFormat) : base(bits, pcmData, isDataInLittleEndianFormat) { }
    public override float this[int index] {
      get { index *= 2; return (((_data[index] | (_data[index + 1] << 8)) << 16) >> 16) / 32768f; }
    }
  }
  public class PcmData24Bit : PcmData {
    public PcmData24Bit(int bits, byte[] pcmData, bool isDataInLittleEndianFormat) : base(bits, pcmData, isDataInLittleEndianFormat) { }
    public override float this[int index] {
      get { index *= 3; return (((_data[index] | (_data[index + 1] << 8) | (_data[index + 2] << 16)) << 12) >> 12) / 8388608f; }
    }
  }
  public class PcmData32Bit : PcmData {
    public PcmData32Bit(int bits, byte[] pcmData, bool isDataInLittleEndianFormat) : base(bits, pcmData, isDataInLittleEndianFormat) { }
    public override float this[int index] {
      get { index *= 4; return (_data[index] | (_data[index + 1] << 8) | (_data[index + 2] << 16) | (_data[index + 3] << 24)) / 2147483648f; }
    }
  }

}
