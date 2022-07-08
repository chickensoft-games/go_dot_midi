namespace AudioSynthesis.Bank.Components.Effects {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public class Flanger : IAudioEffect {
    private readonly int _baseDelay;
    private readonly int _minDelay;
    private readonly float[] _inputBuffer1;
    private readonly float[] _outputBuffer1;
    private int _position1;
    private readonly float[] _inputBuffer2;
    private readonly float[] _outputBuffer2;
    private int _position2;

    public Lfo Lfo { get; set; }
    public float FeedBack { get; set; }
    public float WetMix { get; set; }
    public float DryMix { get; set; }

    public Flanger(int sampleRate, double minDelay, double maxDelay) {
      if (minDelay > maxDelay) {
        (maxDelay, minDelay) = (minDelay, maxDelay);
      }
      var description = new LfoDescriptor();
      Lfo = new Lfo();
      Lfo.QuickSetup(sampleRate, description);

      _baseDelay = (int)(sampleRate * (maxDelay - minDelay));
      _minDelay = (int)(sampleRate * minDelay);

      var size = (int)(sampleRate * maxDelay) + 1;
      _inputBuffer1 = new float[size];
      _outputBuffer1 = new float[size];
      _position1 = 0;

      _inputBuffer2 = new float[size];
      _outputBuffer2 = new float[size];
      _position2 = 0;

      FeedBack = .15f;
      WetMix = .5f;
      DryMix = .5f;
    }
    public void ApplyEffect(float[] source) {
      for (var x = 0; x < source.Length; x++) {
        Lfo.Increment(1);
        var index = _position1 - (int)((_baseDelay * ((.5 * Lfo.Value) + .5)) + _minDelay);

        if (index < 0) {
          index += _inputBuffer1.Length;
        }

        _inputBuffer1[_position1] = source[x];
        _outputBuffer1[_position1] = (DryMix * _inputBuffer1[_position1]) + (WetMix * _inputBuffer1[index]) + (FeedBack * _outputBuffer1[index]);
        source[x] = _outputBuffer1[_position1++];

        if (_position1 == _inputBuffer1.Length) {
          _position1 = 0;
        }
      }
    }
    public void ApplyEffect(float[] source1, float[] source2) {
      for (int x = 0, index; x < source1.Length; x++) {
        Lfo.Increment(1);
        var lfoValue = (.5 * Lfo.Value) + .5;
        //source 1
        index = _position1 - (int)((_baseDelay * lfoValue) + _minDelay);
        if (index < 0) {
          index += _inputBuffer1.Length;
        }

        _inputBuffer1[_position1] = source1[x];
        _outputBuffer1[_position1] = (DryMix * _inputBuffer1[_position1]) + (WetMix * _inputBuffer1[index]) + (FeedBack * _outputBuffer1[index]);
        source1[x] = _outputBuffer1[_position1++];
        if (_position1 == _inputBuffer1.Length) {
          _position1 = 0;
        }
        //source 2
        index = _position2 - (int)((_baseDelay * (1.0 - lfoValue)) + _minDelay);
        if (index < 0) {
          index += _inputBuffer2.Length;
        }

        _inputBuffer2[_position2] = source2[x];
        _outputBuffer2[_position2] = (DryMix * _inputBuffer2[_position2]) + (WetMix * _inputBuffer2[index]) + (FeedBack * _outputBuffer2[index]);
        source2[x] = _outputBuffer2[_position2++];
        if (_position2 == _inputBuffer2.Length) {
          _position2 = 0;
        }
      }
    }
    public void Reset() {
      Lfo.Reset();
      Array.Clear(_inputBuffer1, 0, _inputBuffer1.Length);
      Array.Clear(_outputBuffer1, 0, _outputBuffer1.Length);
      Array.Clear(_inputBuffer2, 0, _inputBuffer2.Length);
      Array.Clear(_outputBuffer2, 0, _outputBuffer2.Length);
      _position1 = 0;
      _position2 = 0;
    }
  }
}
