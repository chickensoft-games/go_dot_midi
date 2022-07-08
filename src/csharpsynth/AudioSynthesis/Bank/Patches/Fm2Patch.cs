
namespace AudioSynthesis.Bank.Patches {
  using System;
  using AudioSynthesis.Bank.Components;
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Synthesis;
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
   * Note: GEN 1 & 2 must also wrap mathematically on its input like Sin() does
   */
  public class Fm2Patch : Patch {
    public enum SyncMode { Soft, Hard };

    private double _feedBack;
    private Generator _cGen = null!, _mGen = null!;
    private EnvelopeDescriptor _cEnv = null!, _mEnv = null!;
    private LfoDescriptor _lfo = null!;

    public SyncMode SynchronizationMethod { get; private set; }
    public double ModulationIndex { get; private set; }
    public double CarrierIndex { get; private set; }

    public Fm2Patch(string name) : base(name) { }
    public override bool Start(VoiceParameters voiceparams) {
      //calculate velocity
      var fVel = voiceparams.Velocity / 127f;
      //reset counters
      voiceparams.PData[0].Double1 = _cGen.LoopStartPhase;
      voiceparams.PData[1].Double1 = _mGen.LoopStartPhase;
      voiceparams.PData[2].Double1 = 0.0;
      //reset envelopes
      voiceparams.Envelopes[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, fVel, _cEnv);
      voiceparams.Envelopes[1].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, fVel, _mEnv);
      //reset lfo (vibra)
      voiceparams.Lfos[0].QuickSetup(voiceparams.SynthParams.Synth.SampleRate, _lfo);
      //calculate initial pitch
      voiceparams.PitchOffset = (int)(100.0 * (voiceparams.SynthParams.MasterCoarseTune + ((voiceparams.SynthParams.MasterFineTune.Combined - 8192.0) / 8192.0)));
      //calc initial volume
      voiceparams.VolOffset = voiceparams.SynthParams.Volume.Combined / 16383f;
      voiceparams.VolOffset *= voiceparams.VolOffset * fVel * voiceparams.SynthParams.Synth.MixGain;
      //check if we have finished before we have begun
      return voiceparams.Envelopes[0].CurrentState != EnvelopeStateEnum.None;
    }
    public override void Stop(VoiceParameters voiceparams) {
      voiceparams.Envelopes[0].Release(Synthesis.Synthesizer.NON_AUDIBLE);
      voiceparams.Envelopes[1].Release(Synthesis.Synthesizer.DENORM_LIMIT);
    }
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) {
      //--Base pitch calculation
      var carrierPitch = SynthHelper.CentsToPitch(((voiceparams.Note - _cGen.RootKey) * _cGen.KeyTrack) + _cGen.Tune + voiceparams.PitchOffset + voiceparams.SynthParams.CurrentPitch)
          * _cGen.Period * _cGen.Frequency * CarrierIndex / voiceparams.SynthParams.Synth.SampleRate;
      var modulatorPitch = SynthHelper.CentsToPitch(((voiceparams.Note - _mGen.RootKey) * _mGen.KeyTrack) + _mGen.Tune + voiceparams.PitchOffset + voiceparams.SynthParams.CurrentPitch)
          * _mGen.Period * _mGen.Frequency * ModulationIndex / voiceparams.SynthParams.Synth.SampleRate;
      //--Base volume calculation
      var baseVolume = voiceparams.VolOffset * voiceparams.SynthParams.CurrentVolume;
      //--Main Loop
      for (var x = startIndex; x < endIndex; x += Synthesizer.DEFAULT_BLOCK_SIZE * voiceparams.SynthParams.Synth.AudioChannels) {
        //--Calculate pitch modifications
        double pitchMod;
        if (voiceparams.SynthParams.ModRange.Combined != 0) {
          voiceparams.Lfos[0].Increment(Synthesizer.DEFAULT_BLOCK_SIZE);
          pitchMod = SynthHelper.CentsToPitch((int)(voiceparams.Lfos[0].Value * voiceparams.SynthParams.CurrentMod));
        }
        else {
          pitchMod = 1;
        }
        //--Get amplitude values for carrier and modulator
        voiceparams.Envelopes[0].Increment(Synthesizer.DEFAULT_BLOCK_SIZE);
        voiceparams.Envelopes[1].Increment(Synthesizer.DEFAULT_BLOCK_SIZE);
        var cAmp = baseVolume * voiceparams.Envelopes[0].Value;
        var mAmp = voiceparams.Envelopes[1].Value;
        //--Interpolator for modulator amplitude
        var linear_m_amp = (mAmp - voiceparams.PData[3].Float1) / Synthesizer.DEFAULT_BLOCK_SIZE;
        //--Process block
        for (var i = 0; i < voiceparams.BlockBuffer.Length; i++) {
          //calculate current modulator amplitude
          voiceparams.PData[3].Float1 += linear_m_amp;
          //calculate sample
          voiceparams.BlockBuffer[i] = _cGen.GetValue(voiceparams.PData[0].Double1 + (voiceparams.PData[3].Float1 * _mGen.GetValue(voiceparams.PData[1].Double1 + (voiceparams.PData[2].Double1 * _feedBack))));
          //store sample for feedback calculation
          voiceparams.PData[2].Double1 = voiceparams.BlockBuffer[i];
          //increment phase counters
          voiceparams.PData[0].Double1 += carrierPitch * pitchMod;
          voiceparams.PData[1].Double1 += modulatorPitch * pitchMod;
        }
        voiceparams.PData[3].Float1 = mAmp;
        //--Mix block based on number of channels
        if (voiceparams.SynthParams.Synth.AudioChannels == 2) {
          voiceparams.MixMonoToStereoInterp(x,
              cAmp * voiceparams.SynthParams.CurrentPan.Left,
              cAmp * voiceparams.SynthParams.CurrentPan.Right);
        }
        else {
          voiceparams.MixMonoToMonoInterp(x, cAmp);
        }
        //--Bounds check
        if (SynchronizationMethod == SyncMode.Soft) {
          if (voiceparams.PData[0].Double1 >= _cGen.LoopEndPhase) {
            voiceparams.PData[0].Double1 = _cGen.LoopStartPhase + ((voiceparams.PData[0].Double1 - _cGen.LoopEndPhase) % (_cGen.LoopEndPhase - _cGen.LoopStartPhase));
          }

          if (voiceparams.PData[1].Double1 >= _mGen.LoopEndPhase) {
            voiceparams.PData[1].Double1 = _mGen.LoopStartPhase + ((voiceparams.PData[1].Double1 - _mGen.LoopEndPhase) % (_mGen.LoopEndPhase - _mGen.LoopStartPhase));
          }
        }
        else {
          if (voiceparams.PData[0].Double1 >= _cGen.LoopEndPhase) {
            voiceparams.PData[0].Double1 = _cGen.LoopStartPhase;
            voiceparams.PData[1].Double1 = _mGen.LoopStartPhase;
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
      var fmConfig = description.FindCustomDescriptor("fm2c");
      CarrierIndex = (double)fmConfig.Objects[0];
      ModulationIndex = (double)fmConfig.Objects[1];
      _feedBack = (double)fmConfig.Objects[2];
      SynchronizationMethod = GetSyncModeFromString((string)fmConfig.Objects[3]);
      if (description.GenDescriptions[0].LoopMethod != LoopModeEnum.Continuous || description.GenDescriptions[1].LoopMethod != LoopModeEnum.Continuous) {
        throw new Exception("Fm2 patches must have continuous generators with wrapping bounds.");
      }

      _cGen = description.GenDescriptions[0].ToGenerator(assets);
      _mGen = description.GenDescriptions[1].ToGenerator(assets);
      _cEnv = description.EnvelopeDescriptions[0];
      _mEnv = description.EnvelopeDescriptions[1];
      _lfo = description.LfoDescriptions[0];
    }
    public override string ToString() => string.Format("Fm2Patch: {0}, GeneratorCount: 2, SyncMode: {1}", _patchName, SynchronizationMethod);

    public static SyncMode GetSyncModeFromString(string value) => value switch {
      "hard" => SyncMode.Hard,
      "soft" => SyncMode.Soft,
      _ => throw new Exception("Invalid sync mode: " + value + "."),
    };
  }
}
