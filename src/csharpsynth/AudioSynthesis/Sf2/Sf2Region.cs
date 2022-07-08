namespace AudioSynthesis.Sf2 {
  public class Sf2Region {
    public short[] Generators { get; }

    public Sf2Region() => Generators = new short[61];
    public void ApplyDefaultValues() {
      Generators[0] = 0; //startAddrsOffset
      Generators[1] = 0; //endAddrsOffset
      Generators[2] = 0; //startloopAddrsOffset
      Generators[3] = 0; //endloopAddrsOffset
      Generators[4] = 0; //startAddrsCoarseOffset
      Generators[5] = 0; //modLfoToPitch
      Generators[6] = 0; //vibLfoToPitch
      Generators[7] = 0; //modEnvToPitch
      Generators[8] = 13500; //initialFilterFc
      Generators[9] = 0; //initialFilterQ
      Generators[10] = 0; //modLfoToFilterFc
      Generators[11] = 0; //modEnvToFilterFc
      Generators[12] = 0; //endAddrsCoarseOffset
      Generators[13] = 0; //modLfoToVolume
      Generators[15] = 0; //chorusEffectsSend
      Generators[16] = 0; //reverbEffectsSend
      Generators[17] = 0; //pan
      Generators[21] = -12000; //delayModLFO
      Generators[22] = 0; //freqModLFO
      Generators[23] = -12000; //delayVibLFO
      Generators[24] = 0; //freqVibLFO
      Generators[25] = -12000; //delayModEnv
      Generators[26] = -12000; //attackModEnv
      Generators[27] = -12000; //holdModEnv
      Generators[28] = -12000; //decayModEnv
      Generators[29] = 0; //sustainModEnv
      Generators[30] = -12000; //releaseModEnv
      Generators[31] = 0; //keynumToModEnvHold
      Generators[32] = 0; //keynumToModEnvDecay
      Generators[33] = -12000; //delayVolEnv
      Generators[34] = -12000; //attackVolEnv
      Generators[35] = -12000; //holdVolEnv
      Generators[36] = -12000; //decayVolEnv
      Generators[37] = 0; //sustainVolEnv
      Generators[38] = -12000; //releaseVolEnv
      Generators[39] = 0; //keynumToVolEnvHold
      Generators[40] = 0; //keynumToVolEnvDecay
      Generators[43] = 0x7F00;//keyRange
      Generators[44] = 0x7F00;//velRange
      Generators[45] = 0; //startloopAddrsCoarseOffset
      Generators[46] = -1; //keynum
      Generators[47] = -1; //velocity
      Generators[48] = 0; //initialAttenuation
      Generators[50] = 0; //endloopAddrsCoarseOffset
      Generators[51] = 0; //coarseTune
      Generators[52] = 0; //fineTune
      Generators[54] = 0; //sampleModes
      Generators[56] = 100; //scaleTuning
      Generators[57] = 0; //exclusiveClass
      Generators[58] = -1; //overridingRootKey
    }
  }
}
