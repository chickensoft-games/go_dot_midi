namespace AudioSynthesis.Bank.Patches {
  using AudioSynthesis.Bank.Components;
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Synthesis;

  /* A simple single generator patch
   *
   *    LFO1
   *     |
   *    GEN1 --> ENV1 --> OUT
   *
   * LFO1 : Usually generates vibrato. Responds to the MOD Controller (MIDI Controlled).
   * GEN1 : Any generator. No restriction on sampler type.
   * ENV1 : An envelope controlling the amplitude of GEN1.
   */
  public class BasicPatch : Patch {
    private Generator _gen = null!;
    private EnvelopeDescriptor _env = null!;
    private LfoDescriptor _lfo = null!;

    public BasicPatch(string name) : base(name) { }
    public override bool Start(VoiceParameters voiceparams) {
      //calculate velocity
      var fVel = voiceparams.Velocity / 127f;
      //reset generator
      voiceparams.GeneratorParams[0].QuickSetup(_gen);
      //reset envelope
      voiceparams.Envelopes[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, fVel, _env);
      //reset lfo (vibra)
      voiceparams.Lfos[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, _lfo);
      //calculate initial pitch
      voiceparams.PitchOffset = ((voiceparams.Note - _gen.RootKey) * _gen.KeyTrack) + (int)(fVel * _gen.VelocityTrack) + _gen.Tune;
      voiceparams.PitchOffset += (int)(100.0 * (voiceparams.SynthParams.MasterCoarseTune + ((voiceparams.SynthParams.MasterFineTune.Combined - 8192.0) / 8192.0)));
      //calculate initial volume
      voiceparams.VolOffset = voiceparams.SynthParams.Volume.Combined / 16383f;
      voiceparams.VolOffset *= voiceparams.VolOffset * fVel * voiceparams.SynthParams.Synth.MixGain;
      //check if we have finished before we have begun
      return voiceparams.GeneratorParams[0].CurrentState != GeneratorStateEnum.Finished && voiceparams.Envelopes[0].CurrentState != EnvelopeStateEnum.None;
    }
    public override void Stop(VoiceParameters voiceparams) {
      _gen.Release(voiceparams.GeneratorParams[0]);
      if (_gen.LoopMode != LoopModeEnum.OneShot) {
        voiceparams.Envelopes[0].Release(Synthesis.Synthesizer.NON_AUDIBLE);
      }
    }
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) {
      //--Base pitch calculation
      var basePitch = SynthHelper.CentsToPitch(voiceparams.PitchOffset + voiceparams.SynthParams.CurrentPitch)
          * _gen.Period * _gen.Frequency / voiceparams.SynthParams.Synth.SampleRate;
      //--Base volume calculation
      var baseVolume = voiceparams.VolOffset * voiceparams.SynthParams.CurrentVolume;
      //--Main Loop
      for (var x = startIndex; x < endIndex; x += Synthesizer.DEFAULT_BLOCK_SIZE * voiceparams.SynthParams.Synth.AudioChannels) {
        //--Volume Envelope
        voiceparams.Envelopes[0].Increment(Synthesizer.DEFAULT_BLOCK_SIZE);
        //--Lfo pitch modulation
        double pitchMod;
        if (voiceparams.SynthParams.ModRange.Combined != 0) {
          voiceparams.Lfos[0].Increment(Synthesizer.DEFAULT_BLOCK_SIZE);
          pitchMod = SynthHelper.CentsToPitch((int)(voiceparams.Lfos[0].Value * voiceparams.SynthParams.CurrentMod));
        }
        else {
          pitchMod = 1;
        }
        //--Get next block of samples
        _gen.GetValues(voiceparams.GeneratorParams[0], voiceparams.BlockBuffer, basePitch * pitchMod);
        //--Mix block based on number of channels
        var volume = baseVolume * voiceparams.Envelopes[0].Value;
        if (voiceparams.SynthParams.Synth.AudioChannels == 2) {
          voiceparams.MixMonoToStereoInterp(x,
              volume * voiceparams.SynthParams.CurrentPan.Left,
              volume * voiceparams.SynthParams.CurrentPan.Right);
        }
        else {
          voiceparams.MixMonoToMonoInterp(x, volume);
        }
        //--Check and end early if necessary
        if (voiceparams.Envelopes[0].CurrentState == EnvelopeStateEnum.None || voiceparams.GeneratorParams[0].CurrentState == GeneratorStateEnum.Finished) {
          voiceparams.State = VoiceStateEnum.Stopped;
          return;
        }
      }
    }
    public override void Load(DescriptorList description, AssetManager assets) {
      _gen = description.GenDescriptions[0].ToGenerator(assets);
      _env = description.EnvelopeDescriptions[0];
      _lfo = description.LfoDescriptions[0];
    }
    public override string ToString() => string.Format("BasicPatch: {0}, GeneratorCount: 1", _patchName);
  }
}
