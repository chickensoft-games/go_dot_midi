namespace AudioSynthesis.Synthesis {
  using System;
  using System.Runtime.InteropServices;
  using AudioSynthesis.Midi;
  using AudioSynthesis.Util;

  //structs and enum
  public enum VoiceStealEnum { Oldest, Quietest, Skip };
  public enum PanFormulaEnum { Neg3dBCenter, Neg6dBCenter, ZeroCenter }
  public enum VoiceStateEnum { Stopped, Stopping, Playing }

  public struct MidiMessage {
    public int Delta;
    public byte Channel;
    public byte Command;
    public byte Data1;
    public byte Data2;

    public MidiMessage(byte channel, byte command, byte data1, byte data2)
        : this(0, channel, command, data1, data2) { }
    public MidiMessage(int delta, byte channel, byte command, byte data1, byte data2) {
      Delta = delta;
      Channel = channel;
      Command = command;
      Data1 = data1;
      Data2 = data2;
    }
    public override string ToString() {
      if (Command is >= 0x80 and <= 0xEF) {
        return string.Format("Type: {0}, Channel: {1}, P1: {2}, P2: {3}", (MidiEventTypeEnum)(Command & 0xF0), Channel, Data1, Data2);
      }
      else if (Command is >= 0xF0 and <= 0xF7) {
        return "System Common message";
      }
      else if (Command is >= 0xF8 and <= 0xFF) {
        return "Realtime message";
      }
      else {
        return "Unknown midi message";
      }
    }
  }
  public struct PanComponent {
    public float Left;
    public float Right;

    public PanComponent(float value, PanFormulaEnum formula) {
      value = SynthHelper.Clamp(value, -1f, 1f);
      switch (formula) {
        case PanFormulaEnum.Neg3dBCenter: {
            var dvalue = Synthesizer.HALF_PI * (value + 1f) / 2f;
            Left = (float)Math.Cos(dvalue);
            Right = (float)Math.Sin(dvalue);
          }
          break;
        case PanFormulaEnum.Neg6dBCenter: {
            Left = .5f + (value * -.5f);
            Right = .5f + (value * .5f);
          }
          break;
        case PanFormulaEnum.ZeroCenter: {
            var dvalue = Synthesizer.HALF_PI * (value + 1.0) / 2.0;
            Left = (float)(Math.Cos(dvalue) / Synthesizer.INVERSE_SQRT_OF_TWO);
            Right = (float)(Math.Sin(dvalue) / Synthesizer.INVERSE_SQRT_OF_TWO);
          }
          break;
        default:
          throw new Exception("Invalid pan law selected.");
      }
    }
    public PanComponent(float right, float left) {
      Right = right;
      Left = left;
    }
    public override string ToString() => string.Format("Left: {0:0.0}, Right: {1:0.0}", Left, Right);
  }
  public struct CCValue {
    private byte _coarseValue;
    private byte _fineValue;
    private short _combined;

    public byte Coarse {
      get => _coarseValue;
      set { _coarseValue = value; UpdateCombined(); }
    }
    public byte Fine {
      get => _fineValue;
      set { _fineValue = value; UpdateCombined(); }
    }
    public short Combined {
      get => _combined;
      set { _combined = value; UpdateCoarseFinePair(); }
    }

    public CCValue(byte coarse, byte fine) {
      _coarseValue = coarse;
      _fineValue = fine;
      _combined = 0;
      UpdateCombined();
    }
    public override string ToString() => string.Format("7BitValue: {0}, 14BitValue: {1}", _coarseValue, _combined);
    private void UpdateCombined() {
      if (BitConverter.IsLittleEndian) {
        _combined = (short)((_coarseValue << 7) | _fineValue);
      }
      else {
        _combined = (short)((_fineValue << 7) | _coarseValue);
      }
    }
    private void UpdateCoarseFinePair() {
      if (BitConverter.IsLittleEndian) {
        _coarseValue = (byte)(_combined >> 7);
        _fineValue = (byte)(_combined & 0x7F);
      }
      else {
        _fineValue = (byte)(_combined >> 7);
        _coarseValue = (byte)(_combined & 0x7F);
      }
    }
  }
  [StructLayout(LayoutKind.Explicit)]
  public struct UnionData {
    //double values
    [FieldOffset(0)] public double Double1;
    //float values
    [FieldOffset(0)] public float Float1;
    [FieldOffset(4)] public float Float2;
    //int values
    [FieldOffset(0)] public int Int1;
    [FieldOffset(4)] public int Int2;
  }

  //static helper methods
  public static class SynthHelper {
    //Math related calculations
    public static double Clamp(double value, double min, double max) {
      if (value <= min) {
        return min;
      }
      else if (value >= max) {
        return max;
      }
      else {
        return value;
      }
    }
    public static float Clamp(float value, float min, float max) {
      if (value <= min) {
        return min;
      }
      else if (value >= max) {
        return max;
      }
      else {
        return value;
      }
    }
    public static int Clamp(int value, int min, int max) {
      if (value <= min) {
        return min;
      }
      else if (value >= max) {
        return max;
      }
      else {
        return value;
      }
    }
    public static short Clamp(short value, short min, short max) {
      if (value <= min) {
        return min;
      }
      else if (value >= max) {
        return max;
      }
      else {
        return value;
      }
    }

    public static double NearestPowerOfTwo(double value) => Math.Pow(2, Math.Round(Math.Log(value, 2)));
    public static double SamplesFromTime(int sampleRate, double seconds) => sampleRate * seconds;
    public static double TimeFromSamples(int sampleRate, int samples) => samples / (double)sampleRate;

    public static double DBtoLinear(double dBvalue) => Math.Pow(10.0, dBvalue / 20.0);
    public static double LinearToDB(double linearvalue) => 20.0 * Math.Log10(linearvalue);
    public static double CalculateRMS(float[] data, int start, int length) {
      double sum = 0;
      var end = start + length;
      for (var i = start; i < end; i++) {
        double v = data[i];
        sum += v * v;
      }
      return Math.Sqrt(sum / length);
    }

    //Midi Note and Frequency Conversions
    public static double FrequencyToKey(double frequency, int rootkey) => (12.0 * Math.Log(frequency / 440.0, 2.0)) + rootkey;
    public static double KeyToFrequency(double key, int rootkey) => Math.Pow(2.0, (key - rootkey) / 12.0) * 440.0;

    public static double SemitoneToPitch(int key) {//does not return a frequency, only the 2^(1/12) value.
      if (key < -127) {
        key = -127;
      }
      else if (key > 127) {
        key = 127;
      }

      return Tables.SemitoneTable[127 + key];
    }
    public static double CentsToPitch(int cents) {//does not return a frequency, only the 2^(1/12) value.
      var key = cents / 100;
      cents -= key * 100;
      if (key < -127) {
        key = -127;
      }
      else if (key > 127) {
        key = 127;
      }

      return Tables.SemitoneTable[127 + key] * Tables.CentTable[100 + cents];
    }

  }
}
