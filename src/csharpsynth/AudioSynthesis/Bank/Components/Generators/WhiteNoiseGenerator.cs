namespace AudioSynthesis.Bank.Components.Generators {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public class WhiteNoiseGenerator : Generator {
    private static readonly Random _random = new();
    //--Methods
    public WhiteNoiseGenerator(GeneratorDescriptor description)
        : base(description) {
      if (end < 0) {
        end = 1;
      }

      if (start < 0) {
        start = 0;
      }

      if (loopEnd < 0) {
        loopEnd = end;
      }

      if (loopStart < 0) {
        loopStart = start;
      }

      if (genPeriod < 0) {
        genPeriod = 1;
      }

      if (root < 0) {
        root = 69;
      }

      freq = 440;
    }
    public override float GetValue(double phase) => (float)((_random.NextDouble() * 2.0) - 1.0);
    public override void GetValues(GeneratorParameters generatorParams, float[] blockBuffer, double increment) {
      var processed = 0;
      do {
        var samplesAvailable = (int)Math.Ceiling((generatorParams.currentEnd - generatorParams.phase) / increment);
        if (samplesAvailable > blockBuffer.Length - processed) {
          while (processed < blockBuffer.Length) {
            blockBuffer[processed++] = (float)((_random.NextDouble() * 2.0) - 1.0);
            generatorParams.phase += increment;
          }
        }
        else {
          var endProcessed = processed + samplesAvailable;
          while (processed < endProcessed) {
            blockBuffer[processed++] = (float)((_random.NextDouble() * 2.0) - 1.0);
            generatorParams.phase += increment;
          }
          switch (generatorParams.currentState) {
            case GeneratorStateEnum.PreLoop:
              generatorParams.currentStart = loopStart;
              generatorParams.currentEnd = loopEnd;
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
