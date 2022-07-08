namespace AudioSynthesis.Bank.Patches {
  using System;
  using AudioSynthesis.Bank.Components;
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Synthesis;

  /* A single generator patch with sfz parameters
   *
   * (Pitch)  (Cutoff)  (Volume)
   *    |        |        |
   *   ENV0     ENV1     ENV2
   *    |        |        |
   *   LFO0     LFO1     LFO2
   *    |        |        |
   *   GEN0 --> FLT0 --> MIX --> OUTPUT
   *
   * ENV0 : An envelope that effects pitch
   * ENV1 : An envelope that effects the filter's cutoff
   * ENV2 : An envelope that effects volume
   * LFO0 : LFO used for pitch modulation
   * LFO1 : LFO used to alter the filter's cutoff
   * LFO2 : LFO for tremolo effect on volume
   * GEN0 : A sample generator
   * FLT0 : A filter
   * MIX  : Handles volume mixing (interp and panning)
   */
  public class SfzPatch : Patch {
    private float _sfzVolume;
    private float _ampKeyTrack;
    private float _ampVelTrack;
    private PanComponent _sfzPan;
    private short _ampRootKey;
    private Generator _gen = null!;
    private EnvelopeDescriptor _ptch_env = null!, _fltr_env = null!, _amp_env = null!;
    private LfoDescriptor _ptch_lfo = null!, _fltr_lfo = null!, _amp_lfo = null!;
    private FilterDescriptor _fltr = null!;

    public SfzPatch(string name) : base(name) { }
    public override bool Start(VoiceParameters voiceparams) {
      //calculate velocity
      var fVel = voiceparams.Velocity / 127f;
      //setup generator
      voiceparams.GeneratorParams[0].QuickSetup(_gen);
      //setup envelopes
      voiceparams.Envelopes[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, fVel, _ptch_env);
      voiceparams.Envelopes[1].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, fVel, _fltr_env);
      voiceparams.Envelopes[2].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, fVel, _amp_env);
      //setup lfos
      voiceparams.Lfos[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, _ptch_lfo);
      voiceparams.Lfos[1].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, _fltr_lfo);
      voiceparams.Lfos[2].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, _amp_lfo);
      //setup filter
      voiceparams.Filters[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, voiceparams.Note, fVel, _fltr);
      voiceparams.PData[0].Double1 = voiceparams.Filters[0].Cutoff;
      if (!voiceparams.Filters[0].Enabled) {//disable filter components if necessary
        voiceparams.Envelopes[1].Depth = 0f;
        voiceparams.Lfos[1].Depth = 0f;
      }
      //setup sfz params
      //calculate initial pitch
      voiceparams.PitchOffset = ((voiceparams.Note - _gen.RootKey) * _gen.KeyTrack) + (int)(fVel * _gen.VelocityTrack) + _gen.Tune;
      voiceparams.PitchOffset += (int)(100.0 * (voiceparams.SynthParams.MasterCoarseTune + ((voiceparams.SynthParams.MasterFineTune.Combined - 8192.0) / 8192.0)));
      //calculate initial vol
      voiceparams.VolOffset = voiceparams.SynthParams.Volume.Combined / 16383f;
      voiceparams.VolOffset *= voiceparams.VolOffset * voiceparams.SynthParams.Synth.MixGain;
      var dBVel = -20.0f * (float)Math.Log10(16129.0 / (voiceparams.Velocity * voiceparams.Velocity));
      voiceparams.VolOffset *= (float)SynthHelper.DBtoLinear(((voiceparams.Note - _ampRootKey) * _ampKeyTrack) + (dBVel * _ampVelTrack) + _sfzVolume);
      //check if we have finished before we have begun
      return voiceparams.GeneratorParams[0].CurrentState != GeneratorStateEnum.Finished && voiceparams.Envelopes[2].CurrentState != EnvelopeStateEnum.None;
    }
    public override void Stop(VoiceParameters voiceparams) {
      _gen.Release(voiceparams.GeneratorParams[0]);
      if (_gen.LoopMode != LoopModeEnum.OneShot) {
        voiceparams.Envelopes[0].Release(Synthesis.Synthesizer.DENORM_LIMIT);
        voiceparams.Envelopes[1].Release(Synthesis.Synthesizer.DENORM_LIMIT);
        voiceparams.Envelopes[2].Release(Synthesis.Synthesizer.NON_AUDIBLE);
      }
    }
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) {
      //--Base pitch calculation
      var basePitch = SynthHelper.CentsToPitch(voiceparams.PitchOffset + voiceparams.SynthParams.CurrentPitch)
          * _gen.Frequency / voiceparams.SynthParams.Synth.SampleRate;
      //--Base volume calculation
      var baseVolume = voiceparams.VolOffset * voiceparams.SynthParams.CurrentVolume;
      //--Main Loop
      for (var x = startIndex; x < endIndex; x += Synthesizer.DEFAULT_BLOCK_SIZE * voiceparams.SynthParams.Synth.AudioChannels) {
        //--Envelope Calculations
        if (voiceparams.Envelopes[0].Depth != 0) {
          voiceparams.Envelopes[0].Increment(Synthesizer.DEFAULT_BLOCK_SIZE); //pitch envelope
        }

        if (voiceparams.Envelopes[1].Depth != 0) {
          voiceparams.Envelopes[1].Increment(Synthesizer.DEFAULT_BLOCK_SIZE); //filter envelope
        }

        voiceparams.Envelopes[2].Increment(Synthesizer.DEFAULT_BLOCK_SIZE); //amp envelope (do not skip)
                                                                            //--LFO Calculations
        if (voiceparams.Lfos[0].Depth + voiceparams.SynthParams.CurrentMod != 0) {
          voiceparams.Lfos[0].Increment(Synthesizer.DEFAULT_BLOCK_SIZE); //pitch lfo
        }

        if (voiceparams.Lfos[1].Depth != 0) {
          voiceparams.Lfos[1].Increment(Synthesizer.DEFAULT_BLOCK_SIZE); //filter lfo
        }

        if (voiceparams.Lfos[2].Depth != 1.0)//linear scale 1.0 = 0dB
{
          voiceparams.Lfos[2].Increment(Synthesizer.DEFAULT_BLOCK_SIZE); //amp lfo
        }
        //--Calculate pitch and get next block of samples
        _gen.GetValues(voiceparams.GeneratorParams[0], voiceparams.BlockBuffer, basePitch *
            SynthHelper.CentsToPitch((int)((voiceparams.Envelopes[0].Value * voiceparams.Envelopes[0].Depth) +
            (voiceparams.Lfos[0].Value * (voiceparams.Lfos[0].Depth + voiceparams.SynthParams.CurrentMod)))));
        //--Filter if enabled
        if (voiceparams.Filters[0].Enabled) {
          var cents = (int)(voiceparams.Envelopes[1].Value * voiceparams.Envelopes[1].Depth) + (int)(voiceparams.Lfos[1].Value * voiceparams.Lfos[1].Depth);
          voiceparams.Filters[0].Cutoff = voiceparams.PData[0].Double1 * SynthHelper.CentsToPitch(cents);
          if (voiceparams.Filters[0].CoeffNeedsUpdating) {
            voiceparams.Filters[0].ApplyFilterInterp(voiceparams.BlockBuffer, voiceparams.SynthParams.Synth.SampleRate);
          }
          else {
            voiceparams.Filters[0].ApplyFilter(voiceparams.BlockBuffer);
          }
        }
        //--Volume calculation
        var volume = baseVolume * voiceparams.Envelopes[2].Value * (float)Math.Pow(voiceparams.Lfos[2].Depth, voiceparams.Lfos[2].Value);
        //--Mix block based on number of channels
        if (voiceparams.SynthParams.Synth.AudioChannels == 2) {
          voiceparams.MixMonoToStereoInterp(x,
              volume * _sfzPan.Left * voiceparams.SynthParams.CurrentPan.Left,
              volume * _sfzPan.Right * voiceparams.SynthParams.CurrentPan.Right);
        }
        else {
          voiceparams.MixMonoToMonoInterp(x, volume);
        }
        //--Check and end early if necessary
        if (voiceparams.Envelopes[2].CurrentState == EnvelopeStateEnum.None || voiceparams.GeneratorParams[0].CurrentState == GeneratorStateEnum.Finished) {
          voiceparams.State = VoiceStateEnum.Stopped;
          return;
        }
      }
    }
    public override void Load(DescriptorList description, AssetManager assets) {
      //read in sfz params
      var sfzConfig = description.FindCustomDescriptor("sfzi");
      _exTarget = (int)sfzConfig.Objects[0];
      _exGroup = (int)sfzConfig.Objects[1];
      _sfzVolume = (float)sfzConfig.Objects[2];
      _sfzPan = new PanComponent((float)sfzConfig.Objects[3], PanFormulaEnum.Neg3dBCenter);
      _ampKeyTrack = (float)sfzConfig.Objects[4];
      _ampRootKey = (byte)sfzConfig.Objects[5];
      _ampVelTrack = (float)sfzConfig.Objects[6];
      //read in the generator info
      var gdes = description.GenDescriptions[0];
      if (gdes.SamplerType != WaveformEnum.SampleData) {
        throw new Exception("Sfz can only support sample data generators.");
      }

      _gen = gdes.ToGenerator(assets);
      //read in the envelope info
      _ptch_env = description.EnvelopeDescriptions[0];
      _fltr_env = description.EnvelopeDescriptions[1];
      _amp_env = description.EnvelopeDescriptions[2];
      //read in the lfo info
      _ptch_lfo = description.LfoDescriptions[0];
      _fltr_lfo = description.LfoDescriptions[1];
      _amp_lfo = description.LfoDescriptions[2];
      //read in the filter info
      _fltr = description.FilterDescriptions[0];
    }
  }
}
