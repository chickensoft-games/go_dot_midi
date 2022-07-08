using System;
using AudioSynthesis.Bank.Components;
using AudioSynthesis.Bank.Components.Generators;
using AudioSynthesis.Bank.Descriptors;
using AudioSynthesis.Synthesis;

namespace AudioSynthesis.Bank.Patches {
  /* FM 2 Operator Patch
   *   M --> C --> OUT
   *
   *    LFO1              LFO1
   *     |                 |
   *    GEN1 --> ENV1 --> GEN2 --> ENV2 --> OUT
   *
   * LFO1 : Usually generates vibrato. Responds to the MOD Controller (MIDI Controlled).
   * GEN1 : A generator with a continuous loop type. The Modulator.
   * ENV1 : An envelope controlling the amplitude of GEN1.
   * GEN2 : A generator with a continuous loop type. The Carrier.
   * ENV2 : An envelope controlling the amplitude of GEN2.
   *
   * Note: GEN 1 & 2 must also wrap mathmatically on its input like Sin() does
   */
  public class Fm2Patch : Patch {
    public enum SyncMode { Soft, Hard };
    private SyncMode sync;
    private double mIndex, cIndex, feedBack;
    private Generator cGen, mGen;
    private EnvelopeDescriptor cEnv, mEnv;
    private LfoDescriptor lfo;

    public SyncMode SynchronizationMethod {
      get { return sync; }
    }
    public double ModulationIndex {
      get { return mIndex; }
    }
    public double CarrierIndex {
      get { return cIndex; }
    }

    public Fm2Patch(string name) : base(name) { }
    public override bool Start(VoiceParameters voiceparams) {
      //calculate velocity
      float fVel = voiceparams.Velocity / 127f;
      //reset counters
      voiceparams.PData[0].double1 = cGen.LoopStartPhase;
      voiceparams.PData[1].double1 = mGen.LoopStartPhase;
      voiceparams.PData[2].double1 = 0.0;
      //reset envelopes
      voiceparams.Envelopes[0].QuickSetup(voiceparams.SynthParams.synth.SampleRate, fVel, cEnv);
      voiceparams.Envelopes[1].QuickSetup(voiceparams.SynthParams.synth.SampleRate, fVel, mEnv);
      //reset lfo (vibra)
      voiceparams.Lfos[0].QuickSetup(voiceparams.SynthParams.synth.SampleRate, lfo);
      //calculate initial pitch
      voiceparams.PitchOffset = (int)(100.0 * (voiceparams.SynthParams.masterCoarseTune + (voiceparams.SynthParams.masterFineTune.Combined - 8192.0) / 8192.0));
      //calc initial volume
      voiceparams.VolOffset = voiceparams.SynthParams.volume.Combined / 16383f;
      voiceparams.VolOffset *= voiceparams.VolOffset * fVel * voiceparams.SynthParams.synth.MixGain;
      //check if we have finished before we have begun
      return voiceparams.Envelopes[0].CurrentState != EnvelopeStateEnum.None;
    }
    public override void Stop(VoiceParameters voiceparams) {
      voiceparams.Envelopes[0].Release(Synthesis.Synthesizer.NonAudible);
      voiceparams.Envelopes[1].Release(Synthesis.Synthesizer.DenormLimit);
    }
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) {
      //--Base pitch calculation
      double carrierPitch = SynthHelper.CentsToPitch((voiceparams.Note - cGen.RootKey) * cGen.KeyTrack + cGen.Tune + voiceparams.PitchOffset + voiceparams.SynthParams.currentPitch)
          * cGen.Period * cGen.Frequency * cIndex / voiceparams.SynthParams.synth.SampleRate;
      double modulatorPitch = SynthHelper.CentsToPitch((voiceparams.Note - mGen.RootKey) * mGen.KeyTrack + mGen.Tune + voiceparams.PitchOffset + voiceparams.SynthParams.currentPitch)
          * mGen.Period * mGen.Frequency * mIndex / voiceparams.SynthParams.synth.SampleRate;
      //--Base volume calculation
      float baseVolume = voiceparams.VolOffset * voiceparams.SynthParams.currentVolume;
      //--Main Loop
      for (int x = startIndex; x < endIndex; x += Synthesizer.DefaultBlockSize * voiceparams.SynthParams.synth.AudioChannels) {
        //--Calculate pitch modifications
        double pitchMod;
        if (voiceparams.SynthParams.modRange.Combined != 0) {
          voiceparams.Lfos[0].Increment(Synthesizer.DefaultBlockSize);
          pitchMod = SynthHelper.CentsToPitch((int)(voiceparams.Lfos[0].Value * voiceparams.SynthParams.currentMod));
        }
        else {
          pitchMod = 1;
        }
        //--Get amplitude values for carrier and modulator
        voiceparams.Envelopes[0].Increment(Synthesizer.DefaultBlockSize);
        voiceparams.Envelopes[1].Increment(Synthesizer.DefaultBlockSize);
        float c_amp = baseVolume * voiceparams.Envelopes[0].Value;
        float m_amp = voiceparams.Envelopes[1].Value;
        //--Interpolator for modulator amplitude
        float linear_m_amp = (m_amp - voiceparams.PData[3].float1) / Synthesizer.DefaultBlockSize;
        //--Process block
        for (int i = 0; i < voiceparams.BlockBuffer.Length; i++) {
          //calculate current modulator amplitude
          voiceparams.PData[3].float1 += linear_m_amp;
          //calculate sample
          voiceparams.BlockBuffer[i] = cGen.GetValue(voiceparams.PData[0].double1 + voiceparams.PData[3].float1 * mGen.GetValue(voiceparams.PData[1].double1 + voiceparams.PData[2].double1 * feedBack));
          //store sample for feedback calculation
          voiceparams.PData[2].double1 = voiceparams.BlockBuffer[i];
          //increment phase counters
          voiceparams.PData[0].double1 += carrierPitch * pitchMod;
          voiceparams.PData[1].double1 += modulatorPitch * pitchMod;
        }
        voiceparams.PData[3].float1 = m_amp;
        //--Mix block based on number of channels
        if (voiceparams.SynthParams.synth.AudioChannels == 2)
          voiceparams.MixMonoToStereoInterp(x,
              c_amp * voiceparams.SynthParams.currentPan.Left,
              c_amp * voiceparams.SynthParams.currentPan.Right);
        else
          voiceparams.MixMonoToMonoInterp(x, c_amp);
        //--Bounds check
        if (sync == SyncMode.Soft) {
          if (voiceparams.PData[0].double1 >= cGen.LoopEndPhase)
            voiceparams.PData[0].double1 = cGen.LoopStartPhase + (voiceparams.PData[0].double1 - cGen.LoopEndPhase) % (cGen.LoopEndPhase - cGen.LoopStartPhase);
          if (voiceparams.PData[1].double1 >= mGen.LoopEndPhase)
            voiceparams.PData[1].double1 = mGen.LoopStartPhase + (voiceparams.PData[1].double1 - mGen.LoopEndPhase) % (mGen.LoopEndPhase - mGen.LoopStartPhase);
        }
        else {
          if (voiceparams.PData[0].double1 >= cGen.LoopEndPhase) {
            voiceparams.PData[0].double1 = cGen.LoopStartPhase;
            voiceparams.PData[1].double1 = mGen.LoopStartPhase;
          }
        }
        //--Check and end early if necessary
        if (voiceparams.Envelopes[0].CurrentState == EnvelopeStateEnum.None) {
          voiceparams.State = VoiceStateEnum.Stopped;
          return;
        }
      }
    }
    public override void Load(DescriptorList description, AssetManager assets) {
      CustomDescriptor fmConfig = description.FindCustomDescriptor("fm2c");
      cIndex = (double)fmConfig.Objects[0];
      mIndex = (double)fmConfig.Objects[1];
      feedBack = (double)fmConfig.Objects[2];
      sync = GetSyncModeFromString((string)fmConfig.Objects[3]);
      if (description.GenDescriptions[0].LoopMethod != LoopModeEnum.Continuous || description.GenDescriptions[1].LoopMethod != LoopModeEnum.Continuous)
        throw new Exception("Fm2 patches must have continuous generators with wrapping bounds.");
      cGen = description.GenDescriptions[0].ToGenerator(assets);
      mGen = description.GenDescriptions[1].ToGenerator(assets);
      cEnv = description.EnvelopeDescriptions[0];
      mEnv = description.EnvelopeDescriptions[1];
      lfo = description.LfoDescriptions[0];
    }
    public override string ToString() {
      return string.Format("Fm2Patch: {0}, GeneratorCount: 2, SyncMode: {1}", patchName, sync);
    }

    public static SyncMode GetSyncModeFromString(string value) {
      switch (value) {
        case "hard":
          return SyncMode.Hard;
        case "soft":
          return SyncMode.Soft;
        default:
          throw new Exception("Invalid sync mode: " + value + ".");
      }
    }
  }
}
