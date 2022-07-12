namespace MeltySynth {
  using System;

  internal sealed class ModulationEnvelope {
    private readonly Synthesizer synthesizer;

    private double attackSlope;
    private double decaySlope;
    private double releaseSlope;

    private double attackStartTime;
    private double holdStartTime;
    private double decayStartTime;

    private double decayEndTime;
    private double releaseEndTime;

    private float sustainLevel;
    private float releaseLevel;

    private int processedSampleCount;
    private Stage stage;

    internal ModulationEnvelope(Synthesizer synthesizer) => this.synthesizer = synthesizer;

    public void Start(float delay, float attack, float hold, float decay, float sustain, float release) {
      attackSlope = 1 / attack;
      decaySlope = 1 / decay;
      releaseSlope = 1 / release;

      attackStartTime = delay;
      holdStartTime = attackStartTime + attack;
      decayStartTime = holdStartTime + hold;

      decayEndTime = decayStartTime + decay;
      releaseEndTime = release;

      sustainLevel = SoundFontMath.Clamp(sustain, 0F, 1F);
      releaseLevel = 0;

      processedSampleCount = 0;
      stage = Stage.Delay;
      Value = 0;

      Process(0);
    }

    public void Release() {
      stage = Stage.Release;
      releaseEndTime += (double)processedSampleCount / synthesizer.SampleRate;
      releaseLevel = Value;
    }

    public bool Process() => Process(synthesizer.BlockSize);

    private bool Process(int sampleCount) {
      processedSampleCount += sampleCount;

      var currentTime = (double)processedSampleCount / synthesizer.SampleRate;

      while (stage <= Stage.Hold) {
        var endTime = 0d;
        switch (stage) {
          case Stage.Delay:
            endTime = attackStartTime;
            break;

          case Stage.Attack:
            endTime = holdStartTime;
            break;

          case Stage.Hold:
            endTime = decayStartTime;
            break;
          case Stage.Decay:
            break;
          case Stage.Release:
            break;
          default:
            throw new InvalidOperationException("Invalid envelope stage.");
        }

        if (currentTime < endTime) {
          break;
        }
        else {
          stage++;
        }
      }

      switch (stage) {
        case Stage.Delay:
          Value = 0;
          return true;

        case Stage.Attack:
          Value = (float)(attackSlope * (currentTime - attackStartTime));
          return true;

        case Stage.Hold:
          Value = 1;
          return true;

        case Stage.Decay:
          Value = Math.Max((float)(decaySlope * (decayEndTime - currentTime)), sustainLevel);
          return Value > SoundFontMath.NonAudible;

        case Stage.Release:
          Value = Math.Max((float)(releaseLevel * releaseSlope * (releaseEndTime - currentTime)), 0F);
          return Value > SoundFontMath.NonAudible;

        default:
          throw new InvalidOperationException("Invalid envelope stage.");
      }
    }

    public float Value { get; private set; }



    private enum Stage {
      Delay,
      Attack,
      Hold,
      Decay,
      Release
    }
  }
}
