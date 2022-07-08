
namespace AudioSynthesis.Bank.Patches {
  using System;
  using AudioSynthesis.Bank.Components;
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Sf2;
  using AudioSynthesis.Synthesis;
  public class Sf2Patch : Patch {
    private int _iniFilterFc;
    private double _filterQ;
    private float _initialAttn;
    private short _keyOverride;
    private short _velOverride;
    private short _keynumToModEnvHold;
    private short _keynumToModEnvDecay;
    private short _keynumToVolEnvHold;
    private short _keynumToVolEnvDecay;
    private PanComponent _pan;
    private short _modLfoToPitch;
    private short _vibLfoToPitch;
    private short _modEnvToPitch;
    private short _modLfoToFilterFc;
    private short _modEnvToFilterFc;
    private float _modLfoToVolume;
    private Components.Generators.Generator _gen = null!;
    private EnvelopeDescriptor _mod_env = null!, _vel_env = null!;
    private LfoDescriptor _mod_lfo = null!, _vib_lfo = null!;
    private FilterDescriptor _flir = null!;

    public Sf2Patch(string name) : base(name) { }
    public override bool Start(VoiceParameters voiceparams) {
      var note = _keyOverride > -1 ? _keyOverride : voiceparams.Note;
      var vel = _velOverride > -1 ? _velOverride : voiceparams.Velocity;
      //setup generator
      voiceparams.GeneratorParams[0].QuickSetup(_gen);
      //setup envelopes
      voiceparams.Envelopes[0].QuickSetupSf2(voiceparams.SynthParams.synth.SampleRate, note, _keynumToModEnvHold, _keynumToModEnvDecay, false, _mod_env);
      voiceparams.Envelopes[1].QuickSetupSf2(voiceparams.SynthParams.synth.SampleRate, note, _keynumToVolEnvHold, _keynumToVolEnvDecay, true, _vel_env);
      //setup filter
      //voiceparams.pData[0].int1 = iniFilterFc - (int)(2400 * CalculateModulator(SourceTypeEnum.Linear, TransformEnum.Linear, DirectionEnum.MaxToMin, PolarityEnum.Unipolar, voiceparams.velocity, 0, 127));
      //if (iniFilterFc >= 13500 && fltr.Resonance <= 1)
      voiceparams.Filters[0].Disable();
      //else
      //    voiceparams.filters[0].QuickSetup(voiceparams.synthParams.synth.SampleRate, note, 1f, fltr);
      //setup lfos
      voiceparams.Lfos[0].QuickSetup(voiceparams.SynthParams.synth.SampleRate, _mod_lfo);
      voiceparams.Lfos[1].QuickSetup(voiceparams.SynthParams.synth.SampleRate, _vib_lfo);
      //calculate initial pitch
      voiceparams.PitchOffset = ((note - _gen.RootKey) * _gen.KeyTrack) + _gen.Tune;
      voiceparams.PitchOffset += (int)(100.0 * (voiceparams.SynthParams.masterCoarseTune + ((voiceparams.SynthParams.masterFineTune.Combined - 8192.0) / 8192.0)));
      //calculate initial volume
      voiceparams.VolOffset = _initialAttn;
      voiceparams.VolOffset -= 96.0f * (float)CalculateModulator(SourceTypeEnum.Concave, TransformEnum.Linear, DirectionEnum.MaxToMin, PolarityEnum.Unipolar, voiceparams.Velocity, 0, 127);
      voiceparams.VolOffset -= 96.0f * (float)CalculateModulator(SourceTypeEnum.Concave, TransformEnum.Linear, DirectionEnum.MaxToMin, PolarityEnum.Unipolar, voiceparams.SynthParams.volume.Coarse, 0, 127);
      //check if we have finished before we have begun
      return voiceparams.GeneratorParams[0].currentState != GeneratorStateEnum.Finished && voiceparams.Envelopes[1].CurrentState != EnvelopeStateEnum.None;
    }
    public override void Stop(VoiceParameters voiceparams) {
      _gen.Release(voiceparams.GeneratorParams[0]);
      if (_gen.LoopMode != LoopModeEnum.OneShot) {
        voiceparams.Envelopes[0].Release(Synthesis.Synthesizer.DenormLimit);
        voiceparams.Envelopes[1].ReleaseSf2VolumeEnvelope();
      }
    }
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) {
      //--Base pitch calculation
      var basePitch = SynthHelper.CentsToPitch(voiceparams.PitchOffset + voiceparams.SynthParams.currentPitch)
          * _gen.Frequency / voiceparams.SynthParams.synth.SampleRate;
      var baseVolume = voiceparams.SynthParams.currentVolume * voiceparams.SynthParams.synth.MixGain;
      //--Main Loop
      for (var x = startIndex; x < endIndex; x += Synthesizer.DefaultBlockSize * voiceparams.SynthParams.synth.AudioChannels) {
        voiceparams.Envelopes[0].Increment(Synthesizer.DefaultBlockSize);
        voiceparams.Envelopes[1].Increment(Synthesizer.DefaultBlockSize);
        voiceparams.Lfos[0].Increment(Synthesizer.DefaultBlockSize);
        voiceparams.Lfos[1].Increment(Synthesizer.DefaultBlockSize);
        //--Calculate pitch and get next block of samples
        _gen.GetValues(voiceparams.GeneratorParams[0], voiceparams.BlockBuffer, basePitch *
            SynthHelper.CentsToPitch((int)((voiceparams.Envelopes[0].Value * _modEnvToPitch) +
            (voiceparams.Lfos[0].Value * _modLfoToPitch) + (voiceparams.Lfos[1].Value * _vibLfoToPitch))));
        //--Filter
        if (voiceparams.Filters[0].Enabled) {
          var centsFc = voiceparams.PData[0].int1 + (voiceparams.Lfos[0].Value * _modLfoToFilterFc) + (voiceparams.Envelopes[0].Value * _modEnvToFilterFc);
          if (centsFc > 13500) {
            centsFc = 13500;
          }

          voiceparams.Filters[0].Cutoff = SynthHelper.KeyToFrequency(centsFc / 100.0, 69);
          if (voiceparams.Filters[0].CoeffNeedsUpdating) {
            voiceparams.Filters[0].ApplyFilterInterp(voiceparams.BlockBuffer, voiceparams.SynthParams.synth.SampleRate);
          }
          else {
            voiceparams.Filters[0].ApplyFilter(voiceparams.BlockBuffer);
          }
        }
        //--Volume calculation
        var volume = (float)SynthHelper.DBtoLinear(voiceparams.VolOffset + voiceparams.Envelopes[1].Value + (voiceparams.Lfos[0].Value * _modLfoToVolume)) * baseVolume;
        //--Mix block based on number of channels
        if (voiceparams.SynthParams.synth.AudioChannels == 2) {
          voiceparams.MixMonoToStereoInterp(x,
              volume * _pan.Left * voiceparams.SynthParams.currentPan.Left,
              volume * _pan.Right * voiceparams.SynthParams.currentPan.Right);
        }
        else {
          voiceparams.MixMonoToMonoInterp(x, volume);
        }
        //--Check and end early if necessary
        if ((voiceparams.Envelopes[1].CurrentState > EnvelopeStateEnum.Hold && volume <= Synthesizer.NonAudible) || voiceparams.GeneratorParams[0].currentState == GeneratorStateEnum.Finished) {
          voiceparams.State = VoiceStateEnum.Stopped;
          return;
        }
      }
    }
    public override void Load(DescriptorList description, AssetManager assets) => throw new Exception("Sf2 does not load from patch descriptions.");
    public void Load(Sf2Region region, AssetManager assets) {
      exGroup = region.Generators[(int)GeneratorEnum.ExclusiveClass];
      exTarget = exGroup;
      _iniFilterFc = region.Generators[(int)GeneratorEnum.InitialFilterCutoffFrequency];
      _filterQ = SynthHelper.DBtoLinear(region.Generators[(int)GeneratorEnum.InitialFilterQ] / 10.0);
      _initialAttn = -region.Generators[(int)GeneratorEnum.InitialAttenuation] / 10f;
      _keyOverride = region.Generators[(int)GeneratorEnum.KeyNumber];
      _velOverride = region.Generators[(int)GeneratorEnum.Velocity];
      _keynumToModEnvHold = region.Generators[(int)GeneratorEnum.KeyNumberToModulationEnvelopeHold];
      _keynumToModEnvDecay = region.Generators[(int)GeneratorEnum.KeyNumberToModulationEnvelopeDecay];
      _keynumToVolEnvHold = region.Generators[(int)GeneratorEnum.KeyNumberToVolumeEnvelopeHold];
      _keynumToVolEnvDecay = region.Generators[(int)GeneratorEnum.KeyNumberToVolumeEnvelopeDecay];
      _pan = new PanComponent(region.Generators[(int)GeneratorEnum.Pan] / 500f, PanFormulaEnum.Neg3dBCenter);
      _modLfoToPitch = region.Generators[(int)GeneratorEnum.ModulationLFOToPitch];
      _vibLfoToPitch = region.Generators[(int)GeneratorEnum.VibratoLFOToPitch];
      _modEnvToPitch = region.Generators[(int)GeneratorEnum.ModulationEnvelopeToPitch];
      _modLfoToFilterFc = region.Generators[(int)GeneratorEnum.ModulationLFOToFilterCutoffFrequency];
      _modEnvToFilterFc = region.Generators[(int)GeneratorEnum.ModulationEnvelopeToFilterCutoffFrequency];
      _modLfoToVolume = region.Generators[(int)GeneratorEnum.ModulationLFOToVolume] / 10f;
      LoadGen(region, assets);
      LoadEnvelopes(region);
      LoadLfos(region);
      LoadFilter(region);
    }

    private void LoadGen(Sf2Region region, AssetManager assets) {
      var sda = assets.SampleAssetList[region.Generators[(int)GeneratorEnum.SampleID]];
      _gen = new SampleGenerator {
        EndPhase = sda.End + region.Generators[(int)GeneratorEnum.EndAddressOffset] + (32768 * region.Generators[(int)GeneratorEnum.EndAddressCoarseOffset]),
        Frequency = sda.SampleRate,
        KeyTrack = region.Generators[(int)GeneratorEnum.ScaleTuning],
        LoopEndPhase = sda.LoopEnd + region.Generators[(int)GeneratorEnum.EndLoopAddressOffset] + (32768 * region.Generators[(int)GeneratorEnum.EndLoopAddressCoarseOffset])
      };
      switch (region.Generators[(int)GeneratorEnum.SampleModes] & 0x3) {
        case 0x0:
        case 0x2:
          _gen.LoopMode = LoopModeEnum.NoLoop;
          break;
        case 0x1:
          _gen.LoopMode = LoopModeEnum.Continuous;
          break;
        case 0x3:
          _gen.LoopMode = LoopModeEnum.LoopUntilNoteOff;
          break;
        default:
          break;
      }
      _gen.LoopStartPhase = sda.LoopStart + region.Generators[(int)GeneratorEnum.StartLoopAddressOffset] + (32768 * region.Generators[(int)GeneratorEnum.StartLoopAddressCoarseOffset]);
      _gen.Offset = 0;
      _gen.Period = 1.0;
      if (region.Generators[(int)GeneratorEnum.OverridingRootKey] > -1) {
        _gen.RootKey = region.Generators[(int)GeneratorEnum.OverridingRootKey];
      }
      else {
        _gen.RootKey = sda.RootKey;
      }

      _gen.StartPhase = sda.Start + region.Generators[(int)GeneratorEnum.StartAddressOffset] + (32768 * region.Generators[(int)GeneratorEnum.StartAddressCoarseOffset]);
      _gen.Tune = (short)(sda.Tune + region.Generators[(int)GeneratorEnum.FineTune] + (100 * region.Generators[(int)GeneratorEnum.CoarseTune]));
      _gen.VelocityTrack = 0;
      ((SampleGenerator)_gen).Samples = sda.SampleData;
    }
    private void LoadEnvelopes(Sf2Region region) {
      //mod env
      _mod_env = new EnvelopeDescriptor {
        AttackTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.AttackModulationEnvelope] / 1200.0),
        AttackGraph = 3,
        DecayTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.DecayModulationEnvelope] / 1200.0),
        DelayTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.DelayModulationEnvelope] / 1200.0),
        HoldTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.HoldModulationEnvelope] / 1200.0),
        PeakLevel = 1,
        ReleaseTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.ReleaseModulationEnvelope] / 1200.0),
        StartLevel = 0,
        SustainLevel = 1f - (SynthHelper.Clamp(region.Generators[(int)GeneratorEnum.SustainModulationEnvelope], (short)0, (short)1000) / 1000f)
      };
      //checks
      if (_mod_env.AttackTime < 0.001f) {
        _mod_env.AttackTime = 0.001f;
      }
      else if (_mod_env.AttackTime > 100f) {
        _mod_env.AttackTime = 100f;
      }

      if (_mod_env.DecayTime < 0.001f) {
        _mod_env.DecayTime = 0;
      }
      else if (_mod_env.DecayTime > 100f) {
        _mod_env.DecayTime = 100f;
      }

      if (_mod_env.DelayTime < 0.001f) {
        _mod_env.DelayTime = 0;
      }
      else if (_mod_env.DelayTime > 20f) {
        _mod_env.DelayTime = 20f;
      }

      if (_mod_env.HoldTime < 0.001f) {
        _mod_env.HoldTime = 0;
      }
      else if (_mod_env.HoldTime > 20f) {
        _mod_env.HoldTime = 20f;
      }

      if (_mod_env.ReleaseTime < 0.001f) {
        _mod_env.ReleaseTime = 0.001f;
      }
      else if (_mod_env.ReleaseTime > 100f) {
        _mod_env.ReleaseTime = 100f;
      }
      //volume env
      _vel_env = new EnvelopeDescriptor {
        AttackTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.AttackVolumeEnvelope] / 1200.0),
        AttackGraph = 3,
        DecayTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.DecayVolumeEnvelope] / 1200.0),
        DelayTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.DelayVolumeEnvelope] / 1200.0),
        HoldTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.HoldVolumeEnvelope] / 1200.0),
        PeakLevel = 0,
        ReleaseTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.ReleaseVolumeEnvelope] / 1200.0),
        StartLevel = -100,
        SustainLevel = SynthHelper.Clamp(region.Generators[(int)GeneratorEnum.SustainVolumeEnvelope], (short)0, (short)1000) / -10f
      };
      //checks
      if (_vel_env.AttackTime < 0.001f) {
        _vel_env.AttackTime = 0.001f;
      }
      else if (_vel_env.AttackTime > 100f) {
        _vel_env.AttackTime = 100f;
      }

      if (_vel_env.DecayTime < 0.001f) {
        _vel_env.DecayTime = 0;
      }
      else if (_vel_env.DecayTime > 100f) {
        _vel_env.DecayTime = 100f;
      }

      if (_vel_env.DelayTime < 0.001f) {
        _vel_env.DelayTime = 0;
      }
      else if (_vel_env.DelayTime > 20f) {
        _vel_env.DelayTime = 20f;
      }

      if (_vel_env.HoldTime < 0.001f) {
        _vel_env.HoldTime = 0;
      }
      else if (_vel_env.HoldTime > 20f) {
        _vel_env.HoldTime = 20f;
      }

      if (_vel_env.ReleaseTime < 0.001f) {
        _vel_env.ReleaseTime = 0.001f;
      }
      else if (_vel_env.ReleaseTime > 100f) {
        _vel_env.ReleaseTime = 100f;
      }
    }
    private void LoadLfos(Sf2Region region) {
      _mod_lfo = new LfoDescriptor {
        DelayTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.DelayModulationLFO] / 1200.0),
        Frequency = (float)(Math.Pow(2, region.Generators[(int)GeneratorEnum.FrequencyModulationLFO] / 1200.0) * 8.176),
        Generator = AudioSynthesis.Bank.Components.Generators.Generator.DefaultSine
      };
      _vib_lfo = new LfoDescriptor {
        DelayTime = (float)Math.Pow(2, region.Generators[(int)GeneratorEnum.DelayVibratoLFO] / 1200.0),
        Frequency = (float)(Math.Pow(2, region.Generators[(int)GeneratorEnum.FrequencyVibratoLFO] / 1200.0) * 8.176),
        Generator = AudioSynthesis.Bank.Components.Generators.Generator.DefaultSine
      };
    }
    private void LoadFilter(Sf2Region region) => _flir = new FilterDescriptor {
      FilterMethod = FilterTypeEnum.BiquadLowpass,
      CutOff = (float)SynthHelper.KeyToFrequency(region.Generators[(int)GeneratorEnum.InitialFilterCutoffFrequency] / 100.0, 69),
      Resonance = (float)SynthHelper.DBtoLinear(region.Generators[(int)GeneratorEnum.InitialFilterQ] / 10.0)
    };

    private static double CalculateModulator(SourceTypeEnum s, TransformEnum t, DirectionEnum d, PolarityEnum p, int value, int min, int max) {
      double output = 0;
      int i;
      value -= min;
      max -= min;
      if (d == DirectionEnum.MaxToMin) {
        value = max - value;
      }

      switch (s) {
        case SourceTypeEnum.Linear:
          output = value / max;
          break;
        case SourceTypeEnum.Concave:
          i = 127 - value;
          output = -(20.0 / 96.0) * Math.Log10(i * i / (double)(max * max));
          break;
        case SourceTypeEnum.Convex:
          i = value;
          output = 1 + (20.0 / 96.0 * Math.Log10(i * i / (double)(max * max)));
          break;
        case SourceTypeEnum.Switch:
          if (value <= (max / 2)) {
            output = 0;
          }
          else {
            output = 1;
          }

          break;
        default:
          break;
      }
      if (p == PolarityEnum.Bipolar) {
        output = (output * 2) - 1;
      }

      if (t == TransformEnum.AbsoluteValue) {
        output = Math.Abs(output);
      }

      return output;
    }
  }
}
