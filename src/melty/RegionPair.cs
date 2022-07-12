namespace MeltySynth {
  internal struct RegionPair {
    internal RegionPair(PresetRegion preset, InstrumentRegion instrument) {
      this.Preset = preset;
      this.Instrument = instrument;
    }

    private int this[GeneratorType generatortType] => Instrument[generatortType] + Preset[generatortType];

    public PresetRegion Preset { get; }
    public InstrumentRegion Instrument { get; }

    public int SampleStart => Instrument.SampleStart;
    public int SampleEnd => Instrument.SampleEnd;
    public int SampleStartLoop => Instrument.SampleStartLoop;
    public int SampleEndLoop => Instrument.SampleEndLoop;

    public int StartAddressOffset => Instrument.StartAddressOffset;
    public int EndAddressOffset => Instrument.EndAddressOffset;
    public int StartLoopAddressOffset => Instrument.StartLoopAddressOffset;
    public int EndLoopAddressOffset => Instrument.EndLoopAddressOffset;

    public int ModulationLfoToPitch => this[GeneratorType.ModulationLfoToPitch];
    public int VibratoLfoToPitch => this[GeneratorType.VibratoLfoToPitch];
    public int ModulationEnvelopeToPitch => this[GeneratorType.ModulationEnvelopeToPitch];
    public float InitialFilterCutoffFrequency => SoundFontMath.CentsToHertz(this[GeneratorType.InitialFilterCutoffFrequency]);
    public float InitialFilterQ => 0.1F * this[GeneratorType.InitialFilterQ];
    public int ModulationLfoToFilterCutoffFrequency => this[GeneratorType.ModulationLfoToFilterCutoffFrequency];
    public int ModulationEnvelopeToFilterCutoffFrequency => this[GeneratorType.ModulationEnvelopeToFilterCutoffFrequency];

    public float ModulationLfoToVolume => 0.1F * this[GeneratorType.ModulationLfoToVolume];

    public float ChorusEffectsSend => 0.1F * this[GeneratorType.ChorusEffectsSend];
    public float ReverbEffectsSend => 0.1F * this[GeneratorType.ReverbEffectsSend];
    public float Pan => 0.1F * this[GeneratorType.Pan];

    public float DelayModulationLfo => SoundFontMath.TimecentsToSeconds(this[GeneratorType.DelayModulationLfo]);
    public float FrequencyModulationLfo => SoundFontMath.CentsToHertz(this[GeneratorType.FrequencyModulationLfo]);
    public float DelayVibratoLfo => SoundFontMath.TimecentsToSeconds(this[GeneratorType.DelayVibratoLfo]);
    public float FrequencyVibratoLfo => SoundFontMath.CentsToHertz(this[GeneratorType.FrequencyVibratoLfo]);
    public float DelayModulationEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.DelayModulationEnvelope]);
    public float AttackModulationEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.AttackModulationEnvelope]);
    public float HoldModulationEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.HoldModulationEnvelope]);
    public float DecayModulationEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.DecayModulationEnvelope]);
    public float SustainModulationEnvelope => 0.1F * this[GeneratorType.SustainModulationEnvelope];
    public float ReleaseModulationEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.ReleaseModulationEnvelope]);
    public int KeyNumberToModulationEnvelopeHold => this[GeneratorType.KeyNumberToModulationEnvelopeHold];
    public int KeyNumberToModulationEnvelopeDecay => this[GeneratorType.KeyNumberToModulationEnvelopeDecay];
    public float DelayVolumeEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.DelayVolumeEnvelope]);
    public float AttackVolumeEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.AttackVolumeEnvelope]);
    public float HoldVolumeEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.HoldVolumeEnvelope]);
    public float DecayVolumeEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.DecayVolumeEnvelope]);
    public float SustainVolumeEnvelope => 0.1F * this[GeneratorType.SustainVolumeEnvelope];
    public float ReleaseVolumeEnvelope => SoundFontMath.TimecentsToSeconds(this[GeneratorType.ReleaseVolumeEnvelope]);
    public int KeyNumberToVolumeEnvelopeHold => this[GeneratorType.KeyNumberToVolumeEnvelopeHold];
    public int KeyNumberToVolumeEnvelopeDecay => this[GeneratorType.KeyNumberToVolumeEnvelopeDecay];

    // public int KeyRangeStart => this[GeneratorParameterType.KeyRange] & 0xFF;
    // public int KeyRangeEnd => (this[GeneratorParameterType.KeyRange] >> 8) & 0xFF;
    // public int VelocityRangeStart => this[GeneratorParameterType.VelocityRange] & 0xFF;
    // public int VelocityRangeEnd => (this[GeneratorParameterType.VelocityRange] >> 8) & 0xFF;

    public float InitialAttenuation => 0.1F * this[GeneratorType.InitialAttenuation];

    public int CoarseTune => this[GeneratorType.CoarseTune];
    public int FineTune => this[GeneratorType.FineTune] + Instrument.Sample.PitchCorrection;
    public LoopMode SampleModes => Instrument.SampleModes;

    public int ScaleTuning => this[GeneratorType.ScaleTuning];
    public int ExclusiveClass => Instrument.ExclusiveClass;
    public int RootKey => Instrument.RootKey;
  }
}
