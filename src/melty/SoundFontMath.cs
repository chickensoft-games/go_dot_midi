namespace MeltySynth {
  using System;

  internal static class SoundFontMath {
    public const float HalfPi = MathF.PI / 2;

    public static readonly float NonAudible = 1.0E-3F;

    private static readonly double logNonAudible = Math.Log(1.0E-3);

    public static float TimecentsToSeconds(float x) => MathF.Pow(2F, 1F / 1200F * x);

    public static float CentsToHertz(float x) => 8.176F * MathF.Pow(2F, 1F / 1200F * x);

    public static float CentsToMultiplyingFactor(float x) => MathF.Pow(2F, 1F / 1200F * x);

    public static float DecibelsToLinear(float x) => MathF.Pow(10F, 0.05F * x);

    public static float LinearToDecibels(float x) => 20F * MathF.Log10(x);

    public static float KeyNumberToMultiplyingFactor(int cents, int key) => TimecentsToSeconds(cents * (60 - key));

    public static double ExpCutoff(double x) {
      if (x < logNonAudible) {
        return 0.0;
      }
      else {
        return Math.Exp(x);
      }
    }

    // This is for .NET Framework, which does not have the Math.Clamp method.
    public static float Clamp(float value, float min, float max) {
      if (value < min) {
        return min;
      }
      else if (value > max) {
        return max;
      }
      else {
        return value;
      }
    }
  }
}
