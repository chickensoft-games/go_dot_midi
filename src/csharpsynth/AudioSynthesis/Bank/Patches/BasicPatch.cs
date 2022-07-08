using AudioSynthesis.Bank.Components;
using AudioSynthesis.Bank.Components.Generators;
using AudioSynthesis.Bank.Descriptors;
using AudioSynthesis.Synthesis;

namespace AudioSynthesis.Bank.Patches {
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
    private Generator gen;
    private EnvelopeDescriptor env;
    private LfoDescriptor lfo;

    public BasicPatch(string name) : base(name) { }
    public override bool Start(VoiceParameters voiceparams) {
      //calculate velocity
      float fVel = voiceparams.Velocity / 127f;
      //reset generator
      voiceparams.GeneratorParams[0].QuickSetup(gen);
      //reset envelope
      voiceparams.Envelopes[0].QuickSetup(voiceparams.SynthParams.synth.SampleRate, fVel, env);
      //reset lfo (vibra)
      voiceparams.Lfos[0].QuickSetup(voiceparams.SynthParams.synth.SampleRate, lfo);
      //calculate initial pitch
      voiceparams.PitchOffset = (voiceparams.Note - gen.RootKey) * gen.KeyTrack + (int)(fVel * gen.VelocityTrack) + gen.Tune;
      voiceparams.PitchOffset += (int)(100.0 * (voiceparams.SynthParams.masterCoarseTune + (voiceparams.SynthParams.masterFineTune.Combined - 8192.0) / 8192.0));
      //calculate initial volume
      voiceparams.VolOffset = voiceparams.SynthParams.volume.Combined / 16383f;
      voiceparams.VolOffset *= voiceparams.VolOffset * fVel * voiceparams.SynthParams.synth.MixGain;
      //check if we have finished before we have begun
      return voiceparams.GeneratorParams[0].currentState != GeneratorStateEnum.Finished && voiceparams.Envelopes[0].CurrentState != EnvelopeStateEnum.None;
    }
    public override void Stop(VoiceParameters voiceparams) {
      gen.Release(voiceparams.GeneratorParams[0]);
      if (gen.LoopMode != LoopModeEnum.OneShot)
        voiceparams.Envelopes[0].Release(Synthesis.Synthesizer.NonAudible);
    }
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) {
      //--Base pitch calculation
      double basePitch = SynthHelper.CentsToPitch(voiceparams.PitchOffset + voiceparams.SynthParams.currentPitch)
          * gen.Period * gen.Frequency / voiceparams.SynthParams.synth.SampleRate;
      //--Base volume calculation
      float baseVolume = voiceparams.VolOffset * voiceparams.SynthParams.currentVolume;
      //--Main Loop
      for (int x = startIndex; x < endIndex; x += Synthesizer.DefaultBlockSize * voiceparams.SynthParams.synth.AudioChannels) {
        //--Volume Envelope
        voiceparams.Envelopes[0].Increment(Synthesizer.DefaultBlockSize);
        //--Lfo pitch modulation
        double pitchMod;
        if (voiceparams.SynthParams.modRange.Combined != 0) {
          voiceparams.Lfos[0].Increment(Synthesizer.DefaultBlockSize);
          pitchMod = SynthHelper.CentsToPitch((int)(voiceparams.Lfos[0].Value * voiceparams.SynthParams.currentMod));
        }
        else {
          pitchMod = 1;
        }
        //--Get next block of samples
        gen.GetValues(voiceparams.GeneratorParams[0], voiceparams.BlockBuffer, basePitch * pitchMod);
        //--Mix block based on number of channels
        float volume = baseVolume * voiceparams.Envelopes[0].Value;
        if (voiceparams.SynthParams.synth.AudioChannels == 2)
          voiceparams.MixMonoToStereoInterp(x,
              volume * voiceparams.SynthParams.currentPan.Left,
              volume * voiceparams.SynthParams.currentPan.Right);
        else
          voiceparams.MixMonoToMonoInterp(x, volume);
        //--Check and end early if necessary
        if (voiceparams.Envelopes[0].CurrentState == EnvelopeStateEnum.None || voiceparams.GeneratorParams[0].currentState == GeneratorStateEnum.Finished) {
          voiceparams.State = VoiceStateEnum.Stopped;
          return;
        }
      }
    }
    public override void Load(DescriptorList description, AssetManager assets) {
      gen = description.GenDescriptions[0].ToGenerator(assets);
      env = description.EnvelopeDescriptions[0];
      lfo = description.LfoDescriptions[0];
    }
    public override string ToString() {
      return string.Format("BasicPatch: {0}, GeneratorCount: 1", patchName);
    }
  }
}
