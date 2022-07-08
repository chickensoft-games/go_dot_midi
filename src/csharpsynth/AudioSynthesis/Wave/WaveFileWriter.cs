namespace AudioSynthesis.Wave {
  using System;
  using System.IO;

  public sealed class WaveFileWriter : IDisposable {
    //--Fields
    private int _length;
    private readonly int _sRate;
    private readonly int _channels;
    private readonly int _bits;
    private readonly IResource _tempR;
    private BinaryWriter _writer;
    private readonly IResource _wavR;

    //--Methods
    public WaveFileWriter(int sampleRate, int channels, int bitsPerSample, IResource tempFile, IResource waveFile) {
      _sRate = sampleRate;
      _channels = channels;
      _bits = bitsPerSample;
      if (!tempFile.WriteAllowed() || !tempFile.ReadAllowed() || !tempFile.DeleteAllowed()) {
        throw new Exception("A valid temporary file with read/write/and delete access is required.");
      }

      _tempR = tempFile;
      _writer = new BinaryWriter(_tempR.OpenResourceForWrite());
      if (!waveFile.WriteAllowed()) {
        throw new Exception("A valid wave file with write access is required.");
      }

      _wavR = waveFile;
    }
    public void Write(byte[] buffer) {
      _writer.Write(buffer);
      _length += buffer.Length;
    }
    public void Write(float[] buffer) => Write(WaveHelper.ConvertToPcm(buffer, _bits));
    public void Write(float[][] buffer) => Write(WaveHelper.ConvertToPcm(buffer, _bits));
    public void Close() {
      if (_writer == null) {
        return;
      }

      _writer.Close();
      _writer = null!;
      using (var bw2 = new BinaryWriter(_wavR.OpenResourceForWrite())) {
        bw2.Write(1179011410);
        bw2.Write(44 + _length - 8);
        bw2.Write(1163280727);
        bw2.Write(544501094);
        bw2.Write(16);
        bw2.Write((short)1);
        bw2.Write((short)_channels);
        bw2.Write(_sRate);
        bw2.Write(_sRate * _channels * (_bits / 8));
        bw2.Write((short)(_channels * (_bits / 8)));
        bw2.Write((short)_bits);
        bw2.Write(1635017060);
        bw2.Write(_length);
        using var br = new BinaryReader(_tempR.OpenResourceForRead());
        var buffer = new byte[1024];
        var count = br.Read(buffer, 0, buffer.Length);
        while (count > 0) {
          bw2.Write(buffer, 0, count);
          count = br.Read(buffer, 0, buffer.Length);
        }
      }
      _tempR.DeleteResource();
    }
    public void Dispose() {
      if (_writer == null) {
        return;
      }

      Close();
    }
  }
}
