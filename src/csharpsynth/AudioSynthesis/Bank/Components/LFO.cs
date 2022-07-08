namespace AudioSynthesis.Bank.Components {
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Bank.Descriptors;

  public class Lfo {
    private double _phase;
    private double _increment;
    private int _delayTime;
    private Generator _generator = null!;

    public double Frequency { get; private set; }
    public LfoStateEnum CurrentState { get; private set; }
    public double Value { get; set; }
    public double Depth { get; set; }

    public void QuickSetup(int sampleRate, LfoDescriptor lfoInfo) {
      _generator = lfoInfo.Generator;
      _delayTime = (int)(sampleRate * lfoInfo.DelayTime);
      Frequency = lfoInfo.Frequency;
      _increment = _generator.Period * Frequency / sampleRate;
      Depth = lfoInfo.Depth;
      Reset();
    }
    public void Increment(int amount) {
      if (CurrentState == LfoStateEnum.Delay) {
        _phase -= amount;
        if (_phase <= 0.0) {
          _phase = _generator.LoopStartPhase + (_increment * -_phase);
          Value = _generator.GetValue(_phase);
          CurrentState = LfoStateEnum.Sustain;
        }
      }
      else {
        _phase += _increment * amount;
        if (_phase >= _generator.LoopEndPhase) {
          _phase = _generator.LoopStartPhase + ((_phase - _generator.LoopEndPhase) % (_generator.LoopEndPhase - _generator.LoopStartPhase));
        }

        Value = _generator.GetValue(_phase);
      }
    }
    public void Reset() {
      Value = 0;
      if (_delayTime > 0) {
        _phase = _delayTime;
        CurrentState = LfoStateEnum.Delay;
      }
      else {
        _phase = _generator.LoopStartPhase;
        CurrentState = LfoStateEnum.Sustain;
      }
    }
    public override string ToString() => string.Format("State: {0}, Frequency: {1}Hz, Value: {2:0.00}", CurrentState, Frequency, Value);
  }
}
