namespace AudioSynthesis.Bank.Components.Generators {
  using System;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Synthesis;
  using AudioSynthesis.Util;
  using AudioSynthesis.Wave;

  public class SampleGenerator : Generator {
    public PcmData Samples { get; set; } = null!;

    public SampleGenerator()
        : base(new GeneratorDescriptor()) { }
    public SampleGenerator(GeneratorDescriptor description, AssetManager assets)
        : base(description) {
      var sample = assets.FindSample(IOHelper.GetFileNameWithoutExtension(description.AssetName));
      if (sample == null) {
        throw new Exception("Could not find asset: (" + description.AssetName + ").");
      }

      Samples = sample.SampleData;
      _freq = sample.SampleRate;
      if (_end < 0) {
        _end = sample.End;
      }

      if (_start < 0) {
        _start = sample.Start;
      }

      if (_loopEnd < 0) {
        if (sample.LoopEnd < 0) {
          _loopEnd = _end;
        }
        else {
          _loopEnd = sample.LoopEnd;
        }
      }
      if (_loopStart < 0) {
        if (sample.LoopStart < 0) {
          _loopStart = _start;
        }
        else {
          _loopStart = sample.LoopStart;
        }
      }
      if (_genPeriod < 0) {
        _genPeriod = 1;
      }

      if (_root < 0) {
        _root = sample.RootKey;
        if (_tuneCents == 0) {
          _tuneCents = sample.Tune;
        }
      }
      //check sample end and loop end for consistency
      if (_end > Samples.Length) {
        _end = Samples.Length;
      }

      if (_loopEnd > _end) {
        _loopEnd = _end;
      }
    }
    public override float GetValue(double phase) => Samples[(int)phase];
    public override void GetValues(GeneratorParameters generatorParams, float[] blockBuffer, double increment) {
      var processed = 0;
      do {
        var samplesAvailable = (int)Math.Ceiling((generatorParams.CurrentEnd - generatorParams.Phase) / increment);
        if (samplesAvailable > blockBuffer.Length - processed) {
          Interpolate(generatorParams, blockBuffer, increment, processed, blockBuffer.Length);
          return; //processed = blockBuffer.Length;
        }
        else {
          var endProcessed = processed + samplesAvailable;
          Interpolate(generatorParams, blockBuffer, increment, processed, endProcessed);
          processed = endProcessed;
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
            default:
              break;
          }
        }
      }
      while (processed < blockBuffer.Length);
    }

    private void Interpolate(GeneratorParameters generatorParams, float[] blockBuffer, double increment, int start, int end) {
      switch (Synthesizer.InterpolationMode) {
        case InterpolationEnum.Linear:
          #region Linear
        {
            var end2 = generatorParams.CurrentState == GeneratorStateEnum.Loop ? _loopEnd - 1 : _end - 1;
            int index;
            float s1, s2, mu;
            while (start < end && generatorParams.Phase < end2)//do this until we reach an edge case or fill the buffer
            {
              index = (int)generatorParams.Phase;
              s1 = Samples[index];
              s2 = Samples[index + 1];
              mu = (float)(generatorParams.Phase - index);
              blockBuffer[start++] = s1 + (mu * (s2 - s1));
              generatorParams.Phase += increment;
            }
            while (start < end)//edge case, if in loop wrap to loop start else use duplicate sample
            {
              index = (int)generatorParams.Phase;
              s1 = Samples[index];
              if (generatorParams.CurrentState == GeneratorStateEnum.Loop) {
                s2 = Samples[(int)generatorParams.CurrentStart];
              }
              else {
                s2 = s1;
              }

              mu = (float)(generatorParams.Phase - index);
              blockBuffer[start++] = s1 + (mu * (s2 - s1));
              generatorParams.Phase += increment;
            }
          }
          #endregion
          break;
        case InterpolationEnum.Cosine:
          #region Cosine
        {
            var end3 = generatorParams.CurrentState == GeneratorStateEnum.Loop ? _loopEnd - 1 : _end - 1;
            int index;
            float s1, s2, mu;
            while (start < end && generatorParams.Phase < end3)//do this until we reach an edge case or fill the buffer
            {
              index = (int)generatorParams.Phase;
              s1 = Samples[index];
              s2 = Samples[index + 1];
              mu = (1f - (float)Math.Cos((generatorParams.Phase - index) * Math.PI)) * 0.5f;
              blockBuffer[start++] = (s1 * (1f - mu)) + (s2 * mu);
              generatorParams.Phase += increment;
            }
            while (start < end)//edge case, if in loop wrap to loop start else use duplicate sample
            {
              index = (int)generatorParams.Phase;
              s1 = Samples[index];
              if (generatorParams.CurrentState == GeneratorStateEnum.Loop) {
                s2 = Samples[(int)generatorParams.CurrentStart];
              }
              else {
                s2 = s1;
              }

              mu = (1f - (float)Math.Cos((generatorParams.Phase - index) * Math.PI)) * 0.5f;
              blockBuffer[start++] = (s1 * (1f - mu)) + (s2 * mu);
              generatorParams.Phase += increment;
            }
          }
          #endregion
          break;
        case InterpolationEnum.CubicSpline:
          #region CubicSpline
        {
            var end4 = generatorParams.CurrentState == GeneratorStateEnum.Loop ? _loopStart + 1 : _start + 1;
            int index;
            float s0, s1, s2, s3, mu;
            while (start < end && generatorParams.Phase < end4)//edge case, wrap to endpoint or duplicate sample
            {
              index = (int)generatorParams.Phase;
              if (generatorParams.CurrentState == GeneratorStateEnum.Loop) {
                s0 = Samples[(int)generatorParams.CurrentEnd - 1];
              }
              else {
                s0 = Samples[index];
              }

              s1 = Samples[index];
              s2 = Samples[index + 1];
              s3 = Samples[index + 2];
              mu = (float)(generatorParams.Phase - index);
              blockBuffer[start++] = (((-0.5f * s0) + (1.5f * s1) - (1.5f * s2) + (0.5f * s3)) * mu * mu * mu) + ((s0 - (2.5f * s1) + (2f * s2) - (0.5f * s3)) * mu * mu) + (((-0.5f * s0) + (0.5f * s2)) * mu) + s1;
              generatorParams.Phase += increment;
            }
            end4 = generatorParams.CurrentState == GeneratorStateEnum.Loop ? _loopEnd - 2 : _end - 2;
            while (start < end && generatorParams.Phase < end4) {
              index = (int)generatorParams.Phase;
              s0 = Samples[index - 1];
              s1 = Samples[index];
              s2 = Samples[index + 1];
              s3 = Samples[index + 2];
              mu = (float)(generatorParams.Phase - index);
              blockBuffer[start++] = (((-0.5f * s0) + (1.5f * s1) - (1.5f * s2) + (0.5f * s3)) * mu * mu * mu) + ((s0 - (2.5f * s1) + (2f * s2) - (0.5f * s3)) * mu * mu) + (((-0.5f * s0) + (0.5f * s2)) * mu) + s1;
              generatorParams.Phase += increment;
            }
            end4 += 1;
            while (start < end)//edge case, wrap to start point or duplicate sample
            {
              index = (int)generatorParams.Phase;
              s0 = Samples[index - 1];
              s1 = Samples[index];
              if (generatorParams.Phase < end4) {
                s2 = Samples[index + 1];
                if (generatorParams.CurrentState == GeneratorStateEnum.Loop) {
                  s3 = Samples[(int)generatorParams.CurrentStart];
                }
                else {
                  s3 = s2;
                }
              }
              else {
                if (generatorParams.CurrentState == GeneratorStateEnum.Loop) {
                  s2 = Samples[(int)generatorParams.CurrentStart];
                  s3 = Samples[(int)generatorParams.CurrentStart + 1];
                }
                else {
                  s2 = s1;
                  s3 = s1;
                }
              }
              mu = (float)(generatorParams.Phase - index);
              blockBuffer[start++] = (((-0.5f * s0) + (1.5f * s1) - (1.5f * s2) + (0.5f * s3)) * mu * mu * mu) + ((s0 - (2.5f * s1) + (2f * s2) - (0.5f * s3)) * mu * mu) + (((-0.5f * s0) + (0.5f * s2)) * mu) + s1;
              generatorParams.Phase += increment;
            }
          }
          #endregion
          break;
        case InterpolationEnum.None:
          break;
        default:
          #region None
                {
            while (start < end) {
              blockBuffer[start++] = Samples[(int)generatorParams.Phase];
              generatorParams.Phase += increment;
            }
          }
          #endregion
          break;
      }
    }
  }
}
