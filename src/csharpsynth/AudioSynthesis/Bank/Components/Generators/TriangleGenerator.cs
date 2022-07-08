namespace AudioSynthesis.Bank.Components.Generators {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public class TriangleGenerator : Generator {
    //--Methods
    public TriangleGenerator(GeneratorDescriptor description)
        : base(description) {
      if (_end < 0) {
        _end = 1.25;
      }

      if (_start < 0) {
        _start = 0.25;
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
    public override float GetValue(double phase) => (float)((Math.Abs(phase - Math.Floor(phase + 0.5)) * 4.0) - 1.0);
    public override void GetValues(GeneratorParameters generatorParams, float[] blockBuffer, double increment) {
      var processed = 0;
      do {
        var samplesAvailable = (int)Math.Ceiling((generatorParams.currentEnd - generatorParams.phase) / increment);
        if (samplesAvailable > blockBuffer.Length - processed) {
          while (processed < blockBuffer.Length) {
            blockBuffer[processed++] = (float)((Math.Abs(generatorParams.phase - Math.Floor(generatorParams.phase + 0.5)) * 4.0) - 1.0);
            generatorParams.phase += increment;
          }
        }
        else {
          var endProcessed = processed + samplesAvailable;
          while (processed < endProcessed) {
            blockBuffer[processed++] = (float)((Math.Abs(generatorParams.phase - Math.Floor(generatorParams.phase + 0.5)) * 4.0) - 1.0);
            generatorParams.phase += increment;
          }
          switch (generatorParams.currentState) {
            case GeneratorStateEnum.PreLoop:
              generatorParams.currentStart = _loopStart;
              generatorParams.currentEnd = _loopEnd;
              generatorParams.currentState = GeneratorStateEnum.Loop;
              break;
            case GeneratorStateEnum.Loop:
              generatorParams.phase += generatorParams.currentStart - generatorParams.currentEnd;
              break;
            case GeneratorStateEnum.PostLoop:
              generatorParams.currentState = GeneratorStateEnum.Finished;
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
