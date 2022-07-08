namespace AudioSynthesis.Bank.Components {
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Bank.Descriptors;

  public class Lfo {
    private double phase;
    private double increment;
    private int delayTime;
    private Generator? generator;

    public double Frequency { get; private set; }
    public LfoStateEnum CurrentState { get; private set; }
    public double Value { get; set; }
    public double Depth { get; set; }

    public void QuickSetup(int sampleRate, LfoDescriptor lfoInfo) {
      generator = lfoInfo.Generator;
      delayTime = (int)(sampleRate * lfoInfo.DelayTime);
      Frequency = lfoInfo.Frequency;
      increment = generator.Period * Frequency / sampleRate;
      Depth = lfoInfo.Depth;
      Reset();
    }
    public void Increment(int amount) {
      if (CurrentState == LfoStateEnum.Delay) {
        phase -= amount;
        if (phase <= 0.0) {
          phase = generator.LoopStartPhase + (increment * -phase);
          Value = generator.GetValue(phase);
          CurrentState = LfoStateEnum.Sustain;
        }
      }
      else {
        phase += increment * amount;
        if (phase >= generator.LoopEndPhase) {
          phase = generator.LoopStartPhase + ((phase - generator.LoopEndPhase) % (generator.LoopEndPhase - generator.LoopStartPhase));
        }

        Value = generator.GetValue(phase);
      }
    }
    public void Reset() {
      Value = 0;
      if (delayTime > 0) {
        phase = delayTime;
        CurrentState = LfoStateEnum.Delay;
      }
      else {
        phase = generator.LoopStartPhase;
        CurrentState = LfoStateEnum.Sustain;
      }
    }
    public override string ToString() => string.Format("State: {0}, Frequency: {1}Hz, Value: {2:0.00}", CurrentState, Frequency, Value);
  }
}
