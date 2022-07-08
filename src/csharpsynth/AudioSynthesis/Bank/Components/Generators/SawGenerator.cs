namespace AudioSynthesis.Bank.Components.Generators {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public class SawGenerator : Generator {
    //--Methods
    public SawGenerator(GeneratorDescriptor description)
        : base(description) {
      if (_end < 0) {
        _end = 1;
      }

      if (_start < 0) {
        _start = 0;
      }

      if (_loopEnd < 0) {
        _loopEnd = _end;
      }

      if (_loopStart < 0) {
        _loopStart = _start;
      }

      if (_genPeriod < 0) {
        _genPeriod = 1;
      }

      if (_root < 0) {
        _root = 69;
      }

      _freq = 440;
    }
    public override float GetValue(double phase) => (float)(2.0 * (phase - Math.Floor(phase + 0.5)));
    public override void GetValues(GeneratorParameters generatorParams, float[] blockBuffer, double increment) {
      var processed = 0;
      do {
        var samplesAvailable = (int)Math.Ceiling((generatorParams.CurrentEnd - generatorParams.Phase) / increment);
        if (samplesAvailable > blockBuffer.Length - processed) {
          while (processed < blockBuffer.Length) {
            blockBuffer[processed++] = (float)(2.0 * (generatorParams.Phase - Math.Floor(generatorParams.Phase + 0.5)));
            generatorParams.Phase += increment;
          }
        }
        else {
          var endProcessed = processed + samplesAvailable;
          while (processed < endProcessed) {
            blockBuffer[processed++] = (float)(2.0 * (generatorParams.Phase - Math.Floor(generatorParams.Phase + 0.5)));
            generatorParams.Phase += increment;
          }
          switch (generatorParams.CurrentState) {
            case GeneratorStateEnum.PreLoop:
              generatorParams.CurrentStart = _loopStart;
              generatorParams.CurrentEnd = _loopEnd;
              generatorParams.CurrentState = GeneratorStateEnum.Loop;
              break;
            case GeneratorStateEnum.Loop:
              generatorParams.Phase += generatorParams.CurrentStart - generatorParams.CurrentEnd;
              break;
            case GeneratorStateEnum.PostLoop:
              generatorParams.CurrentState = GeneratorStateEnum.Finished;
              while (processed < blockBuffer.Length) {
                blockBuffer[processed++] = 0f;
              }

              break;
            case GeneratorStateEnum.Finished:
              break;
            default:
              break;
          }
        }
      }
      while (processed < blockBuffer.Length);
    }
  }
}

