namespace AudioSynthesis.Wave {
  using System;
  using AudioSynthesis.Synthesis;
  using AudioSynthesis.Util;

  public static class WaveHelper {
    //--Methods
    public static float[] GetSampleDataInterleaved(WaveFile wave, int expectedChannels) => GetSampleDataInterleaved(wave.Data.RawSampleData, wave.Format.BitsPerSample, wave.Format.ChannelCount, expectedChannels);
    public static float[] GetSampleDataInterleaved(byte[] pcmData, int bitsPerSample, int channelCount, int expectedChannels) {
      var samplesPerChannel = pcmData.Length / (bitsPerSample / 8 * channelCount);
      var channels = Math.Min(expectedChannels, channelCount);
      var sampleData = new float[samplesPerChannel * expectedChannels];
      for (var x = 0; x < channels; x++) {
        ToSamplesFromPcm(pcmData, bitsPerSample, channelCount, sampleData, x, true);
      }

      return sampleData;
    }
    public static float[][] GetSampleDataDeinterleaved(WaveFile wave, int expectedChannels) => GetSampleDataDeinterleaved(wave.Data.RawSampleData, wave.Format.BitsPerSample, wave.Format.ChannelCount, expectedChannels);
    public static float[][] GetSampleDataDeinterleaved(byte[] pcmData, int bitsPerSample, int channelCount, int expectedChannels) {
      var samplesPerChannel = pcmData.Length / (bitsPerSample / 8 * channelCount);
      var channels = Math.Min(expectedChannels, channelCount);
      var sampleData = new float[expectedChannels][];
      for (var x = 0; x < sampleData.Length; x++) {
        sampleData[x] = new float[samplesPerChannel];
      }

      for (var x = 0; x < channels; x++) {
        ToSamplesFromPcm(pcmData, bitsPerSample, channelCount, sampleData[x], x, false);
      }

      return sampleData;
    }
    public static float[][] Deinterleave(float[] data, int channelCount) {
      if (data.Length % channelCount != 0) {
        throw new Exception("The data provided is invalid or channel count is invalid");
      }

      var sampleData = new float[channelCount][];
      var channelSize = data.Length / channelCount;
      for (var x = 0; x < sampleData.Length; x++) {
        sampleData[x] = new float[channelSize];
        var i = x;
        for (var y = 0; y < sampleData[x].Length; y++) {
          sampleData[x][y] = data[i];
          i += channelCount;
        }
      }
      return sampleData;
    }
    public static float[] Interleave(float[][] data) {
      if (data.Length == 0) {
        return new float[0];
      }

      var slen = data[0].Length;
      for (var x = 1; x < data.Length; x++) {//if channels are not the same size the smallest channel size is used
        if (data[x].Length < slen) {
          slen = data[x].Length;
        }
      }
      var sampleData = new float[data.Length * slen];
      for (var x = 0; x < sampleData.Length; x += data.Length) {
        var z = x / data.Length;
        for (var y = 0; y < data.Length; y++) {
          sampleData[x + y] = data[y][z];
        }
      }
      return sampleData;
    }
    public static byte[] ConvertToPcm(float[][] buffer, int bitsPerSample) {
      var slen = buffer[0].Length;
      for (var x = 1; x < buffer.Length; x++) {//if channels are not the same size the smallest channel size is used
        if (buffer[x].Length < slen) {
          slen = buffer[x].Length;
        }
      }
      var output = new byte[buffer.Length * slen * bitsPerSample / 8];
      for (var x = 0; x < buffer.Length; x++) {
        ToPcmFromSamples(buffer[x], bitsPerSample, buffer.Length, output, x * bitsPerSample / 8);
      }

      return output;
    }
    public static byte[] ConvertToPcm(float[] buffer, int bitsPerSample) {
      var output = new byte[buffer.Length * bitsPerSample / 8];
      ToPcmFromSamples(buffer, bitsPerSample, 1, output, 0);
      return output;
    }
    public static byte[] GetChannelPcmData(byte[] pcmData, int bits, int channelCount, int expectedChannels) {
      var bytes = bits / 8;
      var channels = Math.Min(expectedChannels, channelCount);
      var newData = new byte[expectedChannels * (pcmData.Length / channelCount)];
      var inc = bytes * channelCount;
      var len = bytes * channels;
      for (var x = 0; x < pcmData.Length; x += inc) {
        Array.Copy(pcmData, x, newData, x / inc * len, len);
      }

      return newData;
    }
    public static void SwapEndianess(byte[] data, int bits) {
      bits /= 8; //get bytes per sample
      var swapArray = new byte[bits];
      for (var x = 0; x < data.Length; x += bits) {
        Array.Copy(data, x, swapArray, 0, bits);
        Array.Reverse(swapArray);
        Array.Copy(swapArray, 0, data, x, bits);
      }
    }

    //returns raw audio data in little endian form
    private static void ToPcmFromSamples(float[] input, int bitsPerSample, int channels, byte[] output, int index) {
      switch (bitsPerSample) {
        case 8:
          for (var x = 0; x < input.Length; x++) {
            output[index] = (byte)((input[x] + 1f) / 2f * 255f);
            index += channels;
          }
          break;
        case 16:
          for (var x = 0; x < input.Length; x++) {
            LittleEndianHelper.WriteInt16((short)SynthHelper.Clamp(input[x] * 32768f, -32768f, 32767f), output, index);
            index += channels * 2;
          }
          break;
        case 24:
          for (var x = 0; x < input.Length; x++) {
            LittleEndianHelper.WriteInt24((int)SynthHelper.Clamp(input[x] * 8388608f, -8388608f, 8388607f), output, index);
            index += channels * 3;
          }
          break;
        case 32:
          for (var x = 0; x < input.Length; x++) {
            LittleEndianHelper.WriteInt32((int)SynthHelper.Clamp(input[x] * 2147483648f, -2147483648f, 2147483647f), output, index);
            index += channels * 4;
          }
          break;
        default:
          throw new ArgumentException("Invalid bitspersample value. Supported values are 8, 16, 24, and 32.");
      }
    }
    private static void ToSamplesFromPcm(byte[] input, int bitsPerSample, int channelCount, float[] output, int channel, bool interleaved) {
      int x, xc, i, ic;
      if (interleaved) {
        x = channel;
        xc = channelCount;
      }
      else {
        x = 0;
        xc = 1;
      }
      i = channel * bitsPerSample / 8;
      ic = channelCount * bitsPerSample / 8;
      switch (bitsPerSample) {
        case 8:
          while (x < output.Length) {
            output[x] = (input[i] / 255f * 2f) - 1f;
            x += xc;
            i += ic;
          }
          break;
        case 16:
          while (x < output.Length) {
            output[x] = LittleEndianHelper.ReadInt16(input, i) / 32768f;
            x += xc;
            i += ic;
          }
          break;
        case 24:
          while (x < output.Length) {
            output[x] = LittleEndianHelper.ReadInt24(input, i) / 8388608f;
            x += xc;
            i += ic;
          }
          break;
        case 32:
          while (x < output.Length) {
            output[x] = LittleEndianHelper.ReadInt32(input, i) / 2147483648f;
            x += xc;
            i += ic;
          }
          break;
        default:
          throw new Exception("Invalid sample format: PCM " + bitsPerSample + " bit.");
      }
    }
    private static void ToSamplesFromFloat(byte[] input, int bitsPerSample, int channelCount, float[] output, int channel) {
      var buffer = new byte[bitsPerSample / 8];
      switch (bitsPerSample) {
        case 32:
          for (var x = channel; x < output.Length; x += channelCount) {
            Array.Copy(input, x * 4, buffer, 0, 4);
            if (!BitConverter.IsLittleEndian) {
              Array.Reverse(buffer);
            }

            output[x] = BitConverter.ToSingle(buffer, 0);
          }
          break;
        case 64:
          for (var x = channel; x < output.Length; x += channelCount) {
            Array.Copy(input, x * 8, buffer, 0, 8);
            if (!BitConverter.IsLittleEndian) {
              Array.Reverse(buffer);
            }

            output[x] = (float)BitConverter.ToDouble(buffer, 0);
          }
          break;
        default:
          throw new Exception("Invalid sample format: FLOAT " + bitsPerSample + "bps.");
      }
    }
  }
}
