namespace AudioSynthesis.Bank.Components.Generators {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public abstract class Generator {
    //--Fields
    internal static readonly SineGenerator DefaultSine = new(new GeneratorDescriptor());
    internal static readonly SawGenerator DefaultSaw = new(new GeneratorDescriptor());
    internal static readonly SquareGenerator DefaultSquare = new(new GeneratorDescriptor());
    internal static readonly TriangleGenerator DefaultTriangle = new(new GeneratorDescriptor());

    protected LoopModeEnum loopMethod;
    protected double loopStart;
    protected double loopEnd;
    protected double start;
    protected double end;
    protected double startOffset;
    protected double genPeriod;
    protected double freq;
    protected short root;
    protected short noteTrack;
    protected short velTrack;
    protected short tuneCents;

    //--Properties
    public LoopModeEnum LoopMode {
      get => loopMethod;
      set => loopMethod = value;
    }
    public double LoopStartPhase {
      get => loopStart;
      set => loopStart = value;
    }
    public double LoopEndPhase {
      get => loopEnd;
      set => loopEnd = value;
    }
    public double StartPhase {
      get => start;
      set => start = value;
    }
    public double EndPhase {
      get => end;
      set => end = value;
    }
    public double Offset {
      get => startOffset;
      set => startOffset = value;
    }
    public double Period {
      get => genPeriod;
      set => genPeriod = value;
    }
    public double Frequency {
      get => freq;
      set => freq = value;
    }
    public short RootKey {
      get => root;
      set => root = value;
    }
    public short KeyTrack {
      get => noteTrack;
      set => noteTrack = value;
    }
    public short VelocityTrack {
      get => velTrack;
      set => velTrack = value;
    }
    public short Tune {
      get => tuneCents;
      set => tuneCents = value;
    }

    //--Methods
    public Generator(GeneratorDescriptor description) {
      loopMethod = description.LoopMethod;
      loopStart = description.LoopStartPhase;
      loopEnd = description.LoopEndPhase;
      start = description.StartPhase;
      end = description.EndPhase;
      startOffset = description.Offset;
      genPeriod = description.Period;
      root = description.Rootkey;
      noteTrack = description.KeyTrack;
      velTrack = description.VelTrack;
      tuneCents = description.Tune;
    }
    public void Release(GeneratorParameters generatorParams) {
      if (loopMethod == LoopModeEnum.LoopUntilNoteOff) {
        generatorParams.currentState = GeneratorStateEnum.PostLoop;
        generatorParams.currentStart = start;
        generatorParams.currentEnd = end;
      }
    }
    public abstract float GetValue(double phase);
    public abstract void GetValues(GeneratorParameters generatorParams, float[] blockBuffer, double increment);
    public override string ToString() => string.Format("LoopMode: {0}, RootKey: {1}, Period: {2:0.00}", loopMethod, root, genPeriod);

    public static WaveformEnum GetWaveformFromString(string value) => value.ToLower().Trim() switch {
      "sine" => WaveformEnum.Sine,
      "square" => WaveformEnum.Square,
      "saw" or "sawtooth" => WaveformEnum.Saw,
      "triangle" => WaveformEnum.Triangle,
      "sample" or "sampledata" => WaveformEnum.SampleData,
      "noise" or "whitenoise" => WaveformEnum.WhiteNoise,
      _ => throw new Exception("No such waveform: " + value),
    };
    public static InterpolationEnum GetInterpolationFromString(string value) => value.ToLower() switch {
      "none" => InterpolationEnum.None,
      "linear" => InterpolationEnum.Linear,
      "cosine" => InterpolationEnum.Cosine,
      "cubic" => InterpolationEnum.CubicSpline,
      _ => throw new Exception("No such interpolation: " + value),
    };
    public static LoopModeEnum GetLoopModeFromString(string value) => value.ToLower() switch {
      "noloop" or "none" => LoopModeEnum.NoLoop,
      "oneshot" => LoopModeEnum.OneShot,
      "continuous" => LoopModeEnum.Continuous,
      "sustain" => LoopModeEnum.LoopUntilNoteOff,
      _ => throw new Exception("No such loop mode: " + value),
    };
  }
}
