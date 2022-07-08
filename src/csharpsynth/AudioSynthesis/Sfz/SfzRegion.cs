namespace AudioSynthesis.Sfz {
  using AudioSynthesis.Bank.Components;

  public class SfzRegion {
    public enum OffModeEnum { Fast, Normal }

    //Sample Definition
    public string Sample = string.Empty;
    //Input Controls
    public byte LoChan = 0;
    public byte HiChan = 15;
    public byte LoKey = 0;
    public byte HiKey = 127;
    public byte LoVel = 0;
    public byte HiVel = 127;
    public short LoBend = -8192;
    public short HiBend = 8192;
    public byte LoChanAft = 0;
    public byte HiChanAft = 127;
    public byte LoPolyAft = 0;
    public byte HiPolyAft = 127;
    public int Group = 0;
    public int OffBy = 0;
    public OffModeEnum OffMode = OffModeEnum.Fast;
    //Sample Player
    public float Delay = 0;
    public int Offset = 0;
    public int End = 0;
    public int Count = 0;
    public LoopModeEnum LoopMode = LoopModeEnum.NoLoop;
    public int LoopStart = -1;
    public int LoopEnd = -1;
    //Pitch
    public short Transpose = 0;
    public short Tune = 0;
    public short PitchKeyCenter = 60;
    public short PitchKeyTrack = 100;
    public short PitchVelTrack = 0;
    //Pitch EG
    public float PitchEGDelay = 0;
    public float PitchEGStart = 0;
    public float PitchEGAttack = 0;
    public float PitchEGHold = 0;
    public float PitchEGDecay = 0;
    public float PitchEGSustain = 100f;
    public float PitchEGRelease = 0;
    public short PitchEGDepth = 0;
    public float PitchEGVel2Delay = 0;
    public float PitchEGVel2Attack = 0;
    public float PitchEGVel2Hold = 0;
    public float PitchEGVel2Decay = 0;
    public float PitchEGVel2Sustain = 0;
    public float PitchEGVel2Release = 0;
    public short PitchEGVel2Depth = 0;
    //Pitch Lfo
    public float PitchLfoDelay = 0;
    public float PitchLfoFrequency = 0;
    public short PitchLfoDepth = 0;
    //Filter
    public FilterTypeEnum FilterType = FilterTypeEnum.BiquadLowpass;
    public float CutOff = -1;
    public float Resonance = 0;
    public short FilterKeyTrack = 0;
    public byte FilterKeyCenter = 60;
    public short FilterVelTrack = 0;
    //Filter EG
    public float FilterEGDelay = 0;
    public float FilterEGStart = 0;
    public float FilterEGAttack = 0;
    public float FilterEGHold = 0;
    public float FilterEGDecay = 0;
    public float FilterEGSustain = 100f;
    public float FilterEGRelease = 0;
    public short FilterEGDepth = 0;
    public float FilterEGVel2Delay = 0;
    public float FilterEGVel2Attack = 0;
    public float FilterEGVel2Hold = 0;
    public float FilterEGVel2Decay = 0;
    public float FilterEGVel2Sustain = 0;
    public float FilterEGVel2Release = 0;
    public short FilterEGVel2Depth = 0;
    //Filter Lfo
    public float FilterLfoDelay = 0;
    public float FilterLfoFrequency = 0;
    public float FilterLfoDepth = 0;
    //Amplifier
    public float Volume = 0;
    public float Pan = 0;
    public float AmpKeyTrack = 0;
    public byte AmpKeyCenter = 60;
    public float AmpVelTrack = 1;
    //Amplifier EG
    public float AmpEGDelay = 0;
    public float AmpEGStart = 0;
    public float AmpEGAttack = 0;
    public float AmpEGHold = 0;
    public float AmpEGDecay = 0;
    public float AmpEGSustain = 100f;
    public float AmpEGRelease = 0;
    public float AmpEGVel2Delay = 0;
    public float AmpEGVel2Attack = 0;
    public float AmpEGVel2Hold = 0;
    public float AmpEGVel2Decay = 0;
    public float AmpEGVel2Sustain = 0;
    public float AmpEGVel2Release = 0;
    //Amplifier Lfo
    public float AmpLfoDelay = 0;
    public float AmpLfoFrequency = 0;
    public float AmpLfoDepth = 0;

    public SfzRegion(bool isGlobal) {
      if (isGlobal) {
        //Sample Definition
        Sample = null!;
        //Input Controls
        LoChan = 255;
        HiChan = 255;
        LoKey = 255;
        HiKey = 255;
        LoVel = 255;
        HiVel = 255;
        LoBend = short.MaxValue;
        HiBend = short.MaxValue;
        LoChanAft = 255;
        HiChanAft = 255;
        LoPolyAft = 255;
        HiPolyAft = 255;
        Group = int.MaxValue;
        OffBy = int.MaxValue;
        OffMode = (OffModeEnum)int.MaxValue;
        //Sample Player
        Delay = float.MaxValue;
        Offset = int.MaxValue;
        End = int.MaxValue;
        Count = int.MaxValue;
        LoopMode = (LoopModeEnum)int.MaxValue;
        LoopStart = int.MaxValue;
        LoopEnd = int.MaxValue;
        //Pitch
        Transpose = short.MaxValue;
        Tune = short.MaxValue;
        PitchKeyCenter = short.MaxValue;
        PitchKeyTrack = short.MaxValue;
        PitchVelTrack = short.MaxValue;
        //Pitch EG
        PitchEGDelay = float.MaxValue;
        PitchEGStart = float.MaxValue;
        PitchEGAttack = float.MaxValue;
        PitchEGHold = float.MaxValue;
        PitchEGDecay = float.MaxValue;
        PitchEGSustain = float.MaxValue;
        PitchEGRelease = float.MaxValue;
        PitchEGDepth = short.MaxValue;
        PitchEGVel2Delay = float.MaxValue;
        PitchEGVel2Attack = float.MaxValue;
        PitchEGVel2Hold = float.MaxValue;
        PitchEGVel2Decay = float.MaxValue;
        PitchEGVel2Sustain = float.MaxValue;
        PitchEGVel2Release = float.MaxValue;
        PitchEGVel2Depth = short.MaxValue;
        //Pitch Lfo
        PitchLfoDelay = float.MaxValue;
        PitchLfoFrequency = float.MaxValue;
        PitchLfoDepth = short.MaxValue;
        //Filter
        FilterType = FilterTypeEnum.None;
        CutOff = float.MaxValue;
        Resonance = float.MaxValue;
        FilterKeyTrack = short.MaxValue;
        FilterKeyCenter = 255;
        FilterVelTrack = short.MaxValue;
        //Filter EG
        FilterEGDelay = float.MaxValue;
        FilterEGStart = float.MaxValue;
        FilterEGAttack = float.MaxValue;
        FilterEGHold = float.MaxValue;
        FilterEGDecay = float.MaxValue;
        FilterEGSustain = float.MaxValue;
        FilterEGRelease = float.MaxValue;
        FilterEGDepth = short.MaxValue;
        FilterEGVel2Delay = float.MaxValue;
        FilterEGVel2Attack = float.MaxValue;
        FilterEGVel2Hold = float.MaxValue;
        FilterEGVel2Decay = float.MaxValue;
        FilterEGVel2Sustain = float.MaxValue;
        FilterEGVel2Release = float.MaxValue;
        FilterEGVel2Depth = short.MaxValue;
        //Filter Lfo
        FilterLfoDelay = float.MaxValue;
        FilterLfoFrequency = float.MaxValue;
        FilterLfoDepth = float.MaxValue;
        //Amplifier
        Volume = float.MaxValue;
        Pan = float.MaxValue;
        AmpKeyTrack = float.MaxValue;
        AmpKeyCenter = 255;
        AmpVelTrack = float.MaxValue;
        //Amplifier EG
        AmpEGDelay = float.MaxValue;
        AmpEGStart = float.MaxValue;
        AmpEGAttack = float.MaxValue;
        AmpEGHold = float.MaxValue;
        AmpEGDecay = float.MaxValue;
        AmpEGSustain = float.MaxValue;
        AmpEGRelease = float.MaxValue;
        AmpEGVel2Delay = float.MaxValue;
        AmpEGVel2Attack = float.MaxValue;
        AmpEGVel2Hold = float.MaxValue;
        AmpEGVel2Decay = float.MaxValue;
        AmpEGVel2Sustain = float.MaxValue;
        AmpEGVel2Release = float.MaxValue;
        //Amplifier Lfo
        AmpLfoDelay = float.MaxValue;
        AmpLfoFrequency = float.MaxValue;
        AmpLfoDepth = float.MaxValue;
      }
    }
    public void ApplyGlobal(SfzRegion globalRegion) {
      if (globalRegion.Sample != null) {
        Sample = globalRegion.Sample;
      }

      if (globalRegion.LoChan != 255) {
        LoChan = globalRegion.LoChan;
      }

      if (globalRegion.HiChan != 255) {
        HiChan = globalRegion.HiChan;
      }

      if (globalRegion.LoKey != 255) {
        LoKey = globalRegion.LoKey;
      }

      if (globalRegion.HiKey != 255) {
        HiKey = globalRegion.HiKey;
      }

      if (globalRegion.LoVel != 255) {
        LoVel = globalRegion.LoVel;
      }

      if (globalRegion.HiVel != 255) {
        HiVel = globalRegion.HiVel;
      }

      if (globalRegion.LoBend != short.MaxValue) {
        LoBend = globalRegion.LoBend;
      }

      if (globalRegion.HiBend != short.MaxValue) {
        HiBend = globalRegion.HiBend;
      }

      if (globalRegion.LoChanAft != 255) {
        LoChanAft = globalRegion.LoChanAft;
      }

      if (globalRegion.HiChanAft != 255) {
        HiChanAft = globalRegion.HiChanAft;
      }

      if (globalRegion.LoPolyAft != 255) {
        LoPolyAft = globalRegion.LoPolyAft;
      }

      if (globalRegion.HiPolyAft != 255) {
        HiPolyAft = globalRegion.HiPolyAft;
      }

      if (globalRegion.Group != int.MaxValue) {
        Group = globalRegion.Group;
      }

      if (globalRegion.OffBy != int.MaxValue) {
        OffBy = globalRegion.OffBy;
      }

      if ((int)globalRegion.OffMode != int.MaxValue) {
        OffMode = globalRegion.OffMode;
      }

      if (globalRegion.Delay != float.MaxValue) {
        Delay = globalRegion.Delay;
      }

      if (globalRegion.Offset != int.MaxValue) {
        Offset = globalRegion.Offset;
      }

      if (globalRegion.End != int.MaxValue) {
        End = globalRegion.End;
      }

      if (globalRegion.Count != int.MaxValue) {
        Count = globalRegion.Count;
      }

      if ((int)globalRegion.LoopMode != int.MaxValue) {
        LoopMode = globalRegion.LoopMode;
      }

      if (globalRegion.LoopStart != int.MaxValue) {
        LoopStart = globalRegion.LoopStart;
      }

      if (globalRegion.LoopEnd != int.MaxValue) {
        LoopEnd = globalRegion.LoopEnd;
      }

      if (globalRegion.Transpose != short.MaxValue) {
        Transpose = globalRegion.Transpose;
      }

      if (globalRegion.Tune != short.MaxValue) {
        Tune = globalRegion.Tune;
      }

      if (globalRegion.PitchKeyCenter != short.MaxValue) {
        PitchKeyCenter = globalRegion.PitchKeyCenter;
      }

      if (globalRegion.PitchKeyTrack != short.MaxValue) {
        PitchKeyTrack = globalRegion.PitchKeyTrack;
      }

      if (globalRegion.PitchVelTrack != short.MaxValue) {
        PitchVelTrack = globalRegion.PitchVelTrack;
      }

      if (globalRegion.PitchEGDelay != float.MaxValue) {
        PitchEGDelay = globalRegion.PitchEGDelay;
      }

      if (globalRegion.PitchEGStart != float.MaxValue) {
        PitchEGStart = globalRegion.PitchEGStart;
      }

      if (globalRegion.PitchEGAttack != float.MaxValue) {
        PitchEGAttack = globalRegion.PitchEGAttack;
      }

      if (globalRegion.PitchEGHold != float.MaxValue) {
        PitchEGHold = globalRegion.PitchEGHold;
      }

      if (globalRegion.PitchEGDecay != float.MaxValue) {
        PitchEGDecay = globalRegion.PitchEGDecay;
      }

      if (globalRegion.PitchEGSustain != float.MaxValue) {
        PitchEGSustain = globalRegion.PitchEGSustain;
      }

      if (globalRegion.PitchEGRelease != float.MaxValue) {
        PitchEGRelease = globalRegion.PitchEGRelease;
      }

      if (globalRegion.PitchEGDepth != short.MaxValue) {
        PitchEGDepth = globalRegion.PitchEGDepth;
      }

      if (globalRegion.PitchEGVel2Delay != float.MaxValue) {
        PitchEGVel2Delay = globalRegion.PitchEGVel2Delay;
      }

      if (globalRegion.PitchEGVel2Attack != float.MaxValue) {
        PitchEGVel2Attack = globalRegion.PitchEGVel2Attack;
      }

      if (globalRegion.PitchEGVel2Hold != float.MaxValue) {
        PitchEGVel2Hold = globalRegion.PitchEGVel2Hold;
      }

      if (globalRegion.PitchEGVel2Decay != float.MaxValue) {
        PitchEGVel2Decay = globalRegion.PitchEGVel2Decay;
      }

      if (globalRegion.PitchEGVel2Sustain != float.MaxValue) {
        PitchEGVel2Sustain = globalRegion.PitchEGVel2Sustain;
      }

      if (globalRegion.PitchEGVel2Release != float.MaxValue) {
        PitchEGVel2Release = globalRegion.PitchEGVel2Release;
      }

      if (globalRegion.PitchEGVel2Depth != short.MaxValue) {
        PitchEGVel2Depth = globalRegion.PitchEGVel2Depth;
      }

      if (globalRegion.PitchLfoDelay != float.MaxValue) {
        PitchLfoDelay = globalRegion.PitchLfoDelay;
      }

      if (globalRegion.PitchLfoFrequency != float.MaxValue) {
        PitchLfoFrequency = globalRegion.PitchLfoFrequency;
      }

      if (globalRegion.PitchLfoDepth != short.MaxValue) {
        PitchLfoDepth = globalRegion.PitchLfoDepth;
      }

      if (globalRegion.FilterType != FilterTypeEnum.None) {
        FilterType = globalRegion.FilterType;
      }

      if (globalRegion.CutOff != float.MaxValue) {
        CutOff = globalRegion.CutOff;
      }

      if (globalRegion.Resonance != float.MaxValue) {
        Resonance = globalRegion.Resonance;
      }

      if (globalRegion.FilterKeyTrack != short.MaxValue) {
        FilterKeyTrack = globalRegion.FilterKeyTrack;
      }

      if (globalRegion.FilterKeyCenter != 255) {
        FilterKeyCenter = globalRegion.FilterKeyCenter;
      }

      if (globalRegion.FilterVelTrack != short.MaxValue) {
        FilterVelTrack = globalRegion.FilterVelTrack;
      }

      if (globalRegion.FilterEGDelay != float.MaxValue) {
        FilterEGDelay = globalRegion.FilterEGDelay;
      }

      if (globalRegion.FilterEGStart != float.MaxValue) {
        FilterEGStart = globalRegion.FilterEGStart;
      }

      if (globalRegion.FilterEGAttack != float.MaxValue) {
        FilterEGAttack = globalRegion.FilterEGAttack;
      }

      if (globalRegion.FilterEGHold != float.MaxValue) {
        FilterEGHold = globalRegion.FilterEGHold;
      }

      if (globalRegion.FilterEGDecay != float.MaxValue) {
        FilterEGDecay = globalRegion.FilterEGDecay;
      }

      if (globalRegion.FilterEGSustain != float.MaxValue) {
        FilterEGSustain = globalRegion.FilterEGSustain;
      }

      if (globalRegion.FilterEGRelease != float.MaxValue) {
        FilterEGRelease = globalRegion.FilterEGRelease;
      }

      if (globalRegion.FilterEGDepth != short.MaxValue) {
        FilterEGDepth = globalRegion.FilterEGDepth;
      }

      if (globalRegion.FilterEGVel2Delay != float.MaxValue) {
        FilterEGVel2Delay = globalRegion.FilterEGVel2Delay;
      }

      if (globalRegion.FilterEGVel2Attack != float.MaxValue) {
        FilterEGVel2Attack = globalRegion.FilterEGVel2Attack;
      }

      if (globalRegion.FilterEGVel2Hold != float.MaxValue) {
        FilterEGVel2Hold = globalRegion.FilterEGVel2Hold;
      }

      if (globalRegion.FilterEGVel2Decay != float.MaxValue) {
        FilterEGVel2Decay = globalRegion.FilterEGVel2Decay;
      }

      if (globalRegion.FilterEGVel2Sustain != float.MaxValue) {
        FilterEGVel2Sustain = globalRegion.FilterEGVel2Sustain;
      }

      if (globalRegion.FilterEGVel2Release != float.MaxValue) {
        FilterEGVel2Release = globalRegion.FilterEGVel2Release;
      }

      if (globalRegion.FilterEGVel2Depth != short.MaxValue) {
        FilterEGVel2Depth = globalRegion.FilterEGVel2Depth;
      }

      if (globalRegion.FilterLfoDelay != float.MaxValue) {
        FilterLfoDelay = globalRegion.FilterLfoDelay;
      }

      if (globalRegion.FilterLfoFrequency != float.MaxValue) {
        FilterLfoFrequency = globalRegion.FilterLfoFrequency;
      }

      if (globalRegion.FilterLfoDepth != float.MaxValue) {
        FilterLfoDepth = globalRegion.FilterLfoDepth;
      }

      if (globalRegion.Volume != float.MaxValue) {
        Volume = globalRegion.Volume;
      }

      if (globalRegion.Pan != float.MaxValue) {
        Pan = globalRegion.Pan;
      }

      if (globalRegion.AmpKeyTrack != float.MaxValue) {
        AmpKeyTrack = globalRegion.AmpKeyTrack;
      }

      if (globalRegion.AmpKeyCenter != 255) {
        AmpKeyCenter = globalRegion.AmpKeyCenter;
      }

      if (globalRegion.AmpVelTrack != float.MaxValue) {
        AmpVelTrack = globalRegion.AmpVelTrack;
      }

      if (globalRegion.AmpEGDelay != float.MaxValue) {
        AmpEGDelay = globalRegion.AmpEGDelay;
      }

      if (globalRegion.AmpEGStart != float.MaxValue) {
        AmpEGStart = globalRegion.AmpEGStart;
      }

      if (globalRegion.AmpEGAttack != float.MaxValue) {
        AmpEGAttack = globalRegion.AmpEGAttack;
      }

      if (globalRegion.AmpEGHold != float.MaxValue) {
        AmpEGHold = globalRegion.AmpEGHold;
      }

      if (globalRegion.AmpEGDecay != float.MaxValue) {
        AmpEGDecay = globalRegion.AmpEGDecay;
      }

      if (globalRegion.AmpEGSustain != float.MaxValue) {
        AmpEGSustain = globalRegion.AmpEGSustain;
      }

      if (globalRegion.AmpEGRelease != float.MaxValue) {
        AmpEGRelease = globalRegion.AmpEGRelease;
      }

      if (globalRegion.AmpEGVel2Delay != float.MaxValue) {
        AmpEGVel2Delay = globalRegion.AmpEGVel2Delay;
      }

      if (globalRegion.AmpEGVel2Attack != float.MaxValue) {
        AmpEGVel2Attack = globalRegion.AmpEGVel2Attack;
      }

      if (globalRegion.AmpEGVel2Hold != float.MaxValue) {
        AmpEGVel2Hold = globalRegion.AmpEGVel2Hold;
      }

      if (globalRegion.AmpEGVel2Decay != float.MaxValue) {
        AmpEGVel2Decay = globalRegion.AmpEGVel2Decay;
      }

      if (globalRegion.AmpEGVel2Sustain != float.MaxValue) {
        AmpEGVel2Sustain = globalRegion.AmpEGVel2Sustain;
      }

      if (globalRegion.AmpEGVel2Release != float.MaxValue) {
        AmpEGVel2Release = globalRegion.AmpEGVel2Release;
      }

      if (globalRegion.AmpLfoDelay != float.MaxValue) {
        AmpLfoDelay = globalRegion.AmpLfoDelay;
      }

      if (globalRegion.AmpLfoFrequency != float.MaxValue) {
        AmpLfoFrequency = globalRegion.AmpLfoFrequency;
      }

      if (globalRegion.AmpLfoDepth != float.MaxValue) {
        AmpLfoDepth = globalRegion.AmpLfoDepth;
      }
    }
    public override string ToString() => string.Format("{0}, Chan: {1}-{2}, Key: {3}-{4}", Sample, LoChan, HiChan, LoKey, HiKey);

  }
}
