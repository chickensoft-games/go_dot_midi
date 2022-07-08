namespace AudioSynthesis.Bank.Components.Generators {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public abstract class Generator {
    //--Fields
    internal static readonly SineGenerator DefaultSine = new(new GeneratorDescriptor());
    internal static readonly SawGenerator DefaultSaw = new(new GeneratorDescriptor());
    internal static readonly SquareGenerator DefaultSquare = new(new GeneratorDescriptor());
    internal static readonly TriangleGenerator DefaultTriangle = new(new GeneratorDescriptor());

    protected LoopModeEnum _loopMethod;
    protected double _loopStart;
    protected double _loopEnd;
    protected double _start;
    protected double _end;
    protected double _startOffset;
    protected double _genPeriod;
    protected double _freq;
    protected short _root;
    protected short _noteTrack;
    protected short _velTrack;
    protected short _tuneCents;

    //--Properties
    public LoopModeEnum LoopMode {
      get => _loopMethod;
      set => _loopMethod = value;
    }
    public double LoopStartPhase {
      get => _loopStart;
      set => _loopStart = value;
    }
    public double LoopEndPhase {
      get => _loopEnd;
      set => _loopEnd = value;
    }
    public double StartPhase {
      get => _start;
      set => _start = value;
    }
    public double EndPhase {
      get => _end;
      set => _end = value;
    }
    public double Offset {
      get => _startOffset;
      set => _startOffset = value;
    }
    public double Period {
      get => _genPeriod;
      set => _genPeriod = value;
    }
    public double Frequency {
      get => _freq;
      set => _freq = value;
    }
    public short RootKey {
      get => _root;
      set => _root = value;
    }
    public short KeyTrack {
      get => _noteTrack;
      set => _noteTrack = value;
    }
    public short VelocityTrack {
      get => _velTrack;
      set => _velTrack = value;
    }
    public short Tune {
      get => _tuneCents;
      set => _tuneCents = value;
    }

    //--Methods
    public Generator(GeneratorDescriptor description) {
      _loopMethod = description.LoopMethod;
      _loopStart = description.LoopStartPhase;
      _loopEnd = description.LoopEndPhase;
      _start = description.StartPhase;
      _end = description.EndPhase;
      _startOffset = description.Offset;
      _genPeriod = description.Period;
      _root = description.Rootkey;
      _noteTrack = description.KeyTrack;
      _velTrack = description.VelTrack;
      _tuneCents = description.Tune;
    }
    public void Release(GeneratorParameters generatorParams) {
      if (_loopMethod == LoopModeEnum.LoopUntilNoteOff) {
        generatorParams.CurrentState = GeneratorStateEnum.PostLoop;
        generatorParams.CurrentStart = _start;
        generatorParams.CurrentEnd = _end;
      }
    }
    public abstract float GetValue(double phase);
    public abstract void GetValues(GeneratorParameters generatorParams, float[] blockBuffer, double increment);
    public override string ToString() => string.Format("LoopMode: {0}, RootKey: {1}, Period: {2:0.00}", _loopMethod, _root, _genPeriod);

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
