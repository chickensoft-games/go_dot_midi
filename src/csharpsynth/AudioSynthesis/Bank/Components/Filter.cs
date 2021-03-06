namespace AudioSynthesis.Bank.Components {
  using System;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Synthesis;

  public class Filter {
    private float _a1, _a2, _b1, _b2;
    private float _m1, _m2, _m3;
    private double _cutOff;
    private double _resonance;

    public FilterTypeEnum FilterMethod { get; private set; }
    public double Cutoff {
      get => _cutOff;
      set {
        if (_cutOff != value) { _cutOff = value; CoeffNeedsUpdating = true; }
      }
    }
    public double Resonance {
      get => _resonance;
      set {
        if (value != _resonance) { _resonance = value; CoeffNeedsUpdating = true; }
      }
    }
    public bool CoeffNeedsUpdating { get; private set; }
    public bool Enabled => FilterMethod != FilterTypeEnum.None;

    public void Disable() => FilterMethod = FilterTypeEnum.None;
    public void QuickSetup(int sampleRate, int note, float velocity, FilterDescriptor filterInfo) {
      CoeffNeedsUpdating = false;
      _cutOff = filterInfo.CutOff;
      _resonance = filterInfo.Resonance;
      FilterMethod = filterInfo.FilterMethod;
      _a1 = 0;
      _a2 = 0;
      _b1 = 0;
      _b2 = 0;
      _m1 = 0f;
      _m2 = 0f;
      _m3 = 0f;
      if (_cutOff <= 0 || _resonance <= 0) {
        FilterMethod = FilterTypeEnum.None;
      }
      if (FilterMethod != FilterTypeEnum.None) {
        _cutOff *= SynthHelper.CentsToPitch(((note - filterInfo.RootKey) * filterInfo.KeyTrack) + (int)(velocity * filterInfo.VelTrack));
        UpdateCoeff(sampleRate);
      }
    }
    public float ApplyFilter(float sample) {
      switch (FilterMethod) {
        case FilterTypeEnum.BiquadHighpass:
        case FilterTypeEnum.BiquadLowpass:
          _m3 = sample - (_a1 * _m1) - (_a2 * _m2);
          sample = (_b2 * (_m3 + _m2)) + (_b1 * _m1);
          _m2 = _m1;
          _m1 = _m3;
          return sample;
        case FilterTypeEnum.OnePoleLowpass:
          _m1 += _a1 * (sample - _m1);
          return _m1;
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
            _m3 = data[x] - (_a1 * _m1) - (_a2 * _m2);
            data[x] = (_b2 * (_m3 + _m2)) + (_b1 * _m1);
            _m2 = _m1;
            _m1 = _m3;
          }
          break;
        case FilterTypeEnum.OnePoleLowpass:
          for (var x = 0; x < data.Length; x++) {
            _m1 += _a1 * (data[x] - _m1);
            data[x] = _m1;
          }
          break;
        case FilterTypeEnum.None:
          break;
        default:
          break;
      }
    }
    public void ApplyFilterInterp(float[] data, int sampleRate) {
      var ic = GenerateFilterCoeff(_cutOff / sampleRate, _resonance);
      var a1_inc = (ic[0] - _a1) / data.Length;
      var a2_inc = (ic[1] - _a2) / data.Length;
      var b1_inc = (ic[2] - _b1) / data.Length;
      var b2_inc = (ic[3] - _b2) / data.Length;
      switch (FilterMethod) {
        case FilterTypeEnum.BiquadHighpass:
        case FilterTypeEnum.BiquadLowpass:
          for (var x = 0; x < data.Length; x++) {
            _a1 += a1_inc;
            _a2 += a2_inc;
            _b1 += b1_inc;
            _b2 += b2_inc;
            _m3 = data[x] - (_a1 * _m1) - (_a2 * _m2);
            data[x] = (_b2 * (_m3 + _m2)) + (_b1 * _m1);
            _m2 = _m1;
            _m1 = _m3;
          }
          _a1 = ic[0];
          _a2 = ic[1];
          _b1 = ic[2];
          _b2 = ic[3];
          break;
        case FilterTypeEnum.OnePoleLowpass:
          for (var x = 0; x < data.Length; x++) {
            _a1 += a1_inc;
            _m1 += _a1 * (data[x] - _m1);
            data[x] = _m1;
          }
          _a1 = ic[0];
          break;
        case FilterTypeEnum.None:
          break;
        default:
          break;
      }
      CoeffNeedsUpdating = false;
    }
    public void UpdateCoeff(int sampleRate) {
      var coeff = GenerateFilterCoeff(_cutOff / sampleRate, _resonance);
      _a1 = coeff[0];
      _a2 = coeff[1];
      _b1 = coeff[2];
      _b2 = coeff[3];
      CoeffNeedsUpdating = false;
    }
    public override string ToString() {
      if (Enabled) {
        return string.Format("Type: {0}, CutOff: {1}Hz, Resonance: {2}", FilterMethod, _cutOff, _resonance);
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
            coeff[3] = _b1 * 0.5f;
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
