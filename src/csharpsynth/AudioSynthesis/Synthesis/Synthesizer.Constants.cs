namespace AudioSynthesis.Synthesis {
  using System;
  using AudioSynthesis.Bank.Components;

  public partial class Synthesizer {
    public static InterpolationEnum InterpolationMode = InterpolationEnum.Linear;

    public const double TWO_PI = 2.0 * Math.PI;      //period constant for sin()
    public const double HALF_PI = Math.PI / 2.0;     //half of pi
    public const double INVERSE_SQRT_OF_TWO = 0.707106781186;
    public const double DEFAULT_LFO_FREQUENCY = 8.0; //lfo frequency
    public const int DEFAULT_MOD_DEPTH = 100;
    public const int DEFAULT_POLYPHONY = 40;     //number of voices used when not specified
    public const int MIN_POLYPHONY = 5;          //Lowest possible number of voices
    public const int MAX_POLYPHONY = 250;        //Highest possible number of voices
    public const int DEFAULT_BLOCK_SIZE = 64;     //determines alignment of samples when using block processing
    public const double MAX_BUFFER_SIZE = 0.05;   //maximum time before updating midi controls is necessary : 50 ms
    public const double MIN_BUFFER_SIZE = 0.001;  //minimum time before updating midi controls is necessary : 1 ms
    public const float DENORM_LIMIT = 1e-38f;    //loose denorm limit
    public const float NON_AUDIBLE = 1e-5f;      //lowest value for volume
    public const int MAX_VOICE_COMPONENTS = 4;    //max number of envelopes, lfos, generators, etc for patches
    public const int DEFAULT_CHANNEL_COUNT = 16;  //The number of synth channels for midi processing. default is 16: 0 - 15
    public const int DEFAULT_KEY_COUNT = 128;     //Then number of keys on a midi keyboard ie: 0-127
  }
}
