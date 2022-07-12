namespace MeltySynth {
  using System;

  internal sealed class VolumeEnvelope {
    private readonly Synthesizer synthesizer;

    private double attackSlope;
    private double decaySlope;
    private double releaseSlope;

    private double attackStartTime;
    private double holdStartTime;
    private double decayStartTime;
    private double releaseStartTime;

    private float sustainLevel;
    private float releaseLevel;

    private int processedSampleCount;
    private Stage stage;

    internal VolumeEnvelope(Synthesizer synthesizer) => this.synthesizer = synthesizer;

    public void Start(float delay, float attack, float hold, float decay, float sustain, float release) {
      attackSlope = 1 / attack;
      decaySlope = -9.226 / decay;
      releaseSlope = -9.226 / release;

      attackStartTime = delay;
      holdStartTime = attackStartTime + attack;
      decayStartTime = holdStartTime + hold;
      releaseStartTime = 0;

      sustainLevel = SoundFontMath.Clamp(sustain, 0F, 1F);
      releaseLevel = 0;

      processedSampleCount = 0;
      stage = Stage.Delay;
      Value = 0;

      Process(0);
    }

    public void Release() {
      stage = Stage.Release;
      releaseStartTime = (double)processedSampleCount / synthesizer.SampleRate;
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
          Priority = 4F + Value;
          return true;

        case Stage.Attack:
          Value = (float)(attackSlope * (currentTime - attackStartTime));
          Priority = 3F + Value;
          return true;

        case Stage.Hold:
          Value = 1;
          Priority = 2F + Value;
          return true;

        case Stage.Decay:
          Value = Math.Max((float)SoundFontMath.ExpCutoff(decaySlope * (currentTime - decayStartTime)), sustainLevel);
          Priority = 1F + Value;
          return Value > SoundFontMath.NonAudible;

        case Stage.Release:
          Value = (float)(releaseLevel * SoundFontMath.ExpCutoff(releaseSlope * (currentTime - releaseStartTime)));
          Priority = Value;
          return Value > SoundFontMath.NonAudible;

        default:
          throw new InvalidOperationException("Invalid envelope stage.");
      }
    }

    public float Value { get; private set; }
    public float Priority { get; private set; }



    private enum Stage {
      Delay,
      Attack,
      Hold,
      Decay,
      Release
    }
  }
}
