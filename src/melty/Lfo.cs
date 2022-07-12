namespace MeltySynth {
  internal sealed class Lfo {
    private readonly Synthesizer synthesizer;

    private bool active;

    private double delay;
    private double period;

    private int processedSampleCount;

    internal Lfo(Synthesizer synthesizer) => this.synthesizer = synthesizer;

    public void Start(float delay, float frequency) {
      if (frequency > 1.0E-3) {
        active = true;

        this.delay = delay;
        period = 1.0 / frequency;

        processedSampleCount = 0;
        Value = 0;
      }
      else {
        active = false;
        Value = 0;
      }
    }

    public void Process() {
      if (!active) {
        return;
      }

      processedSampleCount += synthesizer.BlockSize;

      var currentTime = (double)processedSampleCount / synthesizer.SampleRate;

      if (currentTime < delay) {
        Value = 0;
      }
      else {
        var phase = (currentTime - delay) % period / period;
        if (phase < 0.25) {
          Value = (float)(4 * phase);
        }
        else if (phase < 0.75) {
          Value = (float)(4 * (0.5 - phase));
        }
        else {
          Value = (float)(4 * (phase - 1.0));
        }
      }
    }

    public float Value { get; private set; }
  }
}
