namespace AudioSynthesis.Bank.Components.Effects {
  using System;

  public class Delay : IAudioEffect {
    private readonly float[] _buffer1;
    private readonly float[] _buffer2;
    private int _position1;
    private int _position2;

    public Delay(int sampleRate, double delay) {
      _buffer1 = new float[(int)(sampleRate * delay)];
      _position1 = 0;
      _buffer2 = null!;
      _position2 = 0;
    }
    public Delay(int sampleRate, double delay1, double delay2) {
      _buffer1 = new float[(int)(sampleRate * delay1)];
      _position1 = 0;
      _buffer2 = new float[(int)(sampleRate * delay2)];
      _position2 = 0;
    }
    public void ApplyEffect(float[] source) {
      int x = 0, end = _buffer1.Length - 1;
      while (x < source.Length) {
        if (source.Length - x >= end) {
          while (_position1 < end) {
            _buffer1[_position1++] = source[x];
            source[x++] = _buffer1[_position1];
          }
          _buffer1[_position1] = source[x];
          _position1 = 0;
          source[x++] = _buffer1[_position1];
        }
        else {
          while (x < source.Length) {
            _buffer1[_position1++] = source[x];
            source[x++] = _buffer1[_position1];
          }
        }
      }
    }
    public void ApplyEffect(float[] source1, float[] source2) {
      int x, end;
      //source1
      x = 0;
      end = _buffer1.Length - 1;
      while (x < source1.Length) {
        if (source1.Length - x >= end) {
          while (_position1 < end) {
            _buffer1[_position1++] = source1[x];
            source1[x++] = _buffer1[_position1];
          }
          _buffer1[_position1] = source1[x];
          _position1 = 0;
          source1[x++] = _buffer1[_position1];
        }
        else {
          while (x < source1.Length) {
            _buffer1[_position1++] = source1[x];
            source1[x++] = _buffer1[_position1];
          }
        }
      }
      //source2
      x = 0;
      end = _buffer2.Length - 1;
      while (x < source2.Length) {
        if (source2.Length - x >= end) {
          while (_position2 < end) {
            _buffer2[_position2++] = source2[x];
            source2[x++] = _buffer2[_position2];
          }
          _buffer2[_position2] = source2[x];
          _position2 = 0;
          source2[x++] = _buffer2[_position2];
        }
        else {
          while (x < source2.Length) {
            _buffer2[_position2++] = source2[x];
            source2[x++] = _buffer2[_position2];
          }
        }
      }
    }
    public void Reset() {
      _position1 = 0;
      _position2 = 0;
      Array.Clear(_buffer1, 0, _buffer1.Length);
      if (_buffer2 != null) {
        Array.Clear(_buffer2, 0, _buffer2.Length);
      }
    }
  }
}
