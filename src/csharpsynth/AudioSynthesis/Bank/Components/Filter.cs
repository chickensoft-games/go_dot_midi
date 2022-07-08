namespace AudioSynthesis.Bank.Components {
  using System;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Synthesis;

  public class Filter {
    private float a1, a2, b1, b2;
    private float m1, m2, m3;
    private double cutOff;
    private double resonance;

    public FilterTypeEnum FilterMethod { get; private set; }
    public double Cutoff {
      get => cutOff;
      set {
        if (cutOff != value) { cutOff = value; CoeffNeedsUpdating = true; }
      }
    }
    public double Resonance {
      get => resonance;
      set {
        if (value != resonance) { resonance = value; CoeffNeedsUpdating = true; }
      }
    }
    public bool CoeffNeedsUpdating { get; private set; }
    public bool Enabled => FilterMethod != FilterTypeEnum.None;

    public void Disable() => FilterMethod = FilterTypeEnum.None;
    public void QuickSetup(int sampleRate, int note, float velocity, FilterDescriptor filterInfo) {
      CoeffNeedsUpdating = false;
      cutOff = filterInfo.CutOff;
      resonance = filterInfo.Resonance;
      FilterMethod = filterInfo.FilterMethod;
      a1 = 0;
      a2 = 0;
      b1 = 0;
      b2 = 0;
      m1 = 0f;
      m2 = 0f;
      m3 = 0f;
      if (cutOff <= 0 || resonance <= 0) {
        FilterMethod = FilterTypeEnum.None;
      }
      if (FilterMethod != FilterTypeEnum.None) {
        cutOff *= SynthHelper.CentsToPitch(((note - filterInfo.RootKey) * filterInfo.KeyTrack) + (int)(velocity * filterInfo.VelTrack));
        UpdateCoeff(sampleRate);
      }
    }
    public float ApplyFilter(float sample) {
      switch (FilterMethod) {
        case FilterTypeEnum.BiquadHighpass:
        case FilterTypeEnum.BiquadLowpass:
          m3 = sample - (a1 * m1) - (a2 * m2);
          sample = (b2 * (m3 + m2)) + (b1 * m1);
          m2 = m1;
          m1 = m3;
          return sample;
        case FilterTypeEnum.OnePoleLowpass:
          m1 += a1 * (sample - m1);
          return m1;
        case FilterTypeEnum.None:
        default:
          return 0f;
      }
    }
    public void ApplyFilter(float[] data) {
      switch (FilterMethod) {
        case FilterTypeEnum.BiquadHighpass:
        case FilterTypeEnum.BiquadLowpass:
          for (var x = 0; x < data.Length; x++) {
            m3 = data[x] - (a1 * m1) - (a2 * m2);
            data[x] = (b2 * (m3 + m2)) + (b1 * m1);
            m2 = m1;
            m1 = m3;
          }
          break;
        case FilterTypeEnum.OnePoleLowpass:
          for (var x = 0; x < data.Length; x++) {
            m1 += a1 * (data[x] - m1);
            data[x] = m1;
          }
          break;
        case FilterTypeEnum.None:
          break;
        default:
          break;
      }
    }
    public void ApplyFilterInterp(float[] data, int sampleRate) {
      var ic = GenerateFilterCoeff(cutOff / sampleRate, resonance);
      var a1_inc = (ic[0] - a1) / data.Length;
      var a2_inc = (ic[1] - a2) / data.Length;
      var b1_inc = (ic[2] - b1) / data.Length;
      var b2_inc = (ic[3] - b2) / data.Length;
      switch (FilterMethod) {
        case FilterTypeEnum.BiquadHighpass:
        case FilterTypeEnum.BiquadLowpass:
          for (var x = 0; x < data.Length; x++) {
            a1 += a1_inc;
            a2 += a2_inc;
            b1 += b1_inc;
            b2 += b2_inc;
            m3 = data[x] - (a1 * m1) - (a2 * m2);
            data[x] = (b2 * (m3 + m2)) + (b1 * m1);
            m2 = m1;
            m1 = m3;
          }
          a1 = ic[0];
          a2 = ic[1];
          b1 = ic[2];
          b2 = ic[3];
          break;
        case FilterTypeEnum.OnePoleLowpass:
          for (var x = 0; x < data.Length; x++) {
            a1 += a1_inc;
            m1 += a1 * (data[x] - m1);
            data[x] = m1;
          }
          a1 = ic[0];
          break;
        case FilterTypeEnum.None:
          break;
        default:
          break;
      }
      CoeffNeedsUpdating = false;
    }
    public void UpdateCoeff(int sampleRate) {
      var coeff = GenerateFilterCoeff(cutOff / sampleRate, resonance);
      a1 = coeff[0];
      a2 = coeff[1];
      b1 = coeff[2];
      b2 = coeff[3];
      CoeffNeedsUpdating = false;
    }
    public override string ToString() {
      if (Enabled) {
        return string.Format("Type: {0}, CutOff: {1}Hz, Resonance: {2}", FilterMethod, cutOff, resonance);
      }
      else {
        return "Disabled";
      }
    }

    //--helper methods for coeff update
    private float[] GenerateFilterCoeff(double fc, double q) {
      fc = SynthHelper.Clamp(fc, Synthesizer.DENORM_LIMIT, .49);
      var coeff = new float[4];
      switch (FilterMethod) {
        case FilterTypeEnum.BiquadLowpass: {
            var w0 = Synthesizer.TWO_PI * fc;
            var cosw0 = Math.Cos(w0);
            var alpha = Math.Sin(w0) / (2.0 * q);
            var a0inv = 1.0 / (1.0 + alpha);
            coeff[0] = (float)(-2.0 * cosw0 * a0inv);
            coeff[1] = (float)((1.0 - alpha) * a0inv);
            coeff[2] = (float)((1.0 - cosw0) * a0inv * (1.0 / Math.Sqrt(q)));
            coeff[3] = b1 * 0.5f;
          }
          break;
        case FilterTypeEnum.BiquadHighpass: {
            var w0 = Synthesizer.TWO_PI * fc;
            var cosw0 = Math.Cos(w0);
            var alpha = Math.Sin(w0) / (2.0 * q);
            var a0inv = 1.0 / (1.0 + alpha);
            var qinv = 1.0 / Math.Sqrt(q);
            coeff[0] = (float)(-2.0 * cosw0 * a0inv);
            coeff[1] = (float)((1.0 - alpha) * a0inv);
            coeff[2] = (float)((-1.0 - cosw0) * a0inv * qinv);
            coeff[3] = (float)((1.0 + cosw0) * a0inv * qinv * 0.5);
          }
          break;
        case FilterTypeEnum.OnePoleLowpass:
          coeff[0] = 1.0f - (float)Math.Exp(-2.0 * Math.PI * fc);
          break;
        case FilterTypeEnum.None:
          break;
        default:
          break;
      }
      return coeff;
    }

  }
}
