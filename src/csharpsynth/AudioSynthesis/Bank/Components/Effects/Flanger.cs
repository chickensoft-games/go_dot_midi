namespace AudioSynthesis.Bank.Components.Effects {
  using System;
  using AudioSynthesis.Bank.Descriptors;

  public class Flanger : IAudioEffect {
    private readonly int baseDelay;
    private readonly int minDelay;
    private readonly float[] inputBuffer1;
    private readonly float[] outputBuffer1;
    private int position1;
    private readonly float[] inputBuffer2;
    private readonly float[] outputBuffer2;
    private int position2;

    public Lfo Lfo { get; set; }
    public float FeedBack { get; set; }
    public float WetMix { get; set; }
    public float DryMix { get; set; }

    public Flanger(int sampleRate, double minDelay, double maxDelay) {
      if (minDelay > maxDelay) {
        (maxDelay, minDelay) = (minDelay, maxDelay);
      }
      var description = new LfoDescriptor();
      Lfo = new Lfo();
      Lfo.QuickSetup(sampleRate, description);

      baseDelay = (int)(sampleRate * (maxDelay - minDelay));
      this.minDelay = (int)(sampleRate * minDelay);

      var size = (int)(sampleRate * maxDelay) + 1;
      inputBuffer1 = new float[size];
      outputBuffer1 = new float[size];
      position1 = 0;

      inputBuffer2 = new float[size];
      outputBuffer2 = new float[size];
      position2 = 0;

      FeedBack = .15f;
      WetMix = .5f;
      DryMix = .5f;
    }
    public void ApplyEffect(float[] source) {
      for (var x = 0; x < source.Length; x++) {
        Lfo.Increment(1);
        var index = position1 - (int)((baseDelay * ((.5 * Lfo.Value) + .5)) + minDelay);

        if (index < 0) {
          index += inputBuffer1.Length;
        }

        inputBuffer1[position1] = source[x];
        outputBuffer1[position1] = (DryMix * inputBuffer1[position1]) + (WetMix * inputBuffer1[index]) + (FeedBack * outputBuffer1[index]);
        source[x] = outputBuffer1[position1++];

        if (position1 == inputBuffer1.Length) {
          position1 = 0;
        }
      }
    }
    public void ApplyEffect(float[] source1, float[] source2) {
      for (int x = 0, index; x < source1.Length; x++) {
        Lfo.Increment(1);
        var lfoValue = (.5 * Lfo.Value) + .5;
        //source 1
        index = position1 - (int)((baseDelay * lfoValue) + minDelay);
        if (index < 0) {
          index += inputBuffer1.Length;
        }

        inputBuffer1[position1] = source1[x];
        outputBuffer1[position1] = (DryMix * inputBuffer1[position1]) + (WetMix * inputBuffer1[index]) + (FeedBack * outputBuffer1[index]);
        source1[x] = outputBuffer1[position1++];
        if (position1 == inputBuffer1.Length) {
          position1 = 0;
        }
        //source 2
        index = position2 - (int)((baseDelay * (1.0 - lfoValue)) + minDelay);
        if (index < 0) {
          index += inputBuffer2.Length;
        }

        inputBuffer2[position2] = source2[x];
        outputBuffer2[position2] = (DryMix * inputBuffer2[position2]) + (WetMix * inputBuffer2[index]) + (FeedBack * outputBuffer2[index]);
        source2[x] = outputBuffer2[position2++];
        if (position2 == inputBuffer2.Length) {
          position2 = 0;
        }
      }
    }
    public void Reset() {
      Lfo.Reset();
      Array.Clear(inputBuffer1, 0, inputBuffer1.Length);
      Array.Clear(outputBuffer1, 0, outputBuffer1.Length);
      Array.Clear(inputBuffer2, 0, inputBuffer2.Length);
      Array.Clear(outputBuffer2, 0, outputBuffer2.Length);
      position1 = 0;
      position2 = 0;
    }
  }
}
