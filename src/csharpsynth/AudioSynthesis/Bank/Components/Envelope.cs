/*
 *    ______   __ __     _____             __  __
 *   / ____/__/ // /_   / ___/__  ______  / /_/ /_
 *  / /    /_  _  __/   \__ \/ / / / __ \/ __/ __ \
 * / /___ /_  _  __/   ___/ / /_/ / / / / /_/ / / /
 * \____/  /_//_/     /____/\__, /_/ /_/\__/_/ /_/
 *                         /____/
 * Envelope
 *   An envelope class that follows the six stage DAHDSR model and uses tables for fast calculation.
 */

namespace AudioSynthesis.Bank.Components {
  using System;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Util;

  public class Envelope {
    //--Classes and Enum
    private class EnvelopeStage {
      public int Time;
      public float[] Graph;
      public float Scale;
      public float Offset;
      public bool Reverse;

      public EnvelopeStage() {
        Time = 0;
        Graph = null!;
        Scale = 0;
        Offset = 0;
        Reverse = false;
      }
    }

    private readonly EnvelopeStage[] _stages;
    private EnvelopeStage _stage;
    private int _index;

    //--Properties
    public float Value { get; set; }
    public EnvelopeStateEnum CurrentState { get; private set; }
    public float Depth { get; set; }

    //--Methods
    public Envelope() {
      _stages = new EnvelopeStage[7];
      for (var x = 0; x < _stages.Length; x++) {
        _stages[x] = new EnvelopeStage {
          Graph = Tables.EnvelopeTables[0]
        };
      }
      _stages[3].Reverse = true;
      _stages[5].Reverse = true;
      _stages[6].Time = 100000000;
      CurrentState = EnvelopeStateEnum.None;
      _stage = _stages[(int)CurrentState];
    }
    public void QuickSetup(int sampleRate, float velocity, EnvelopeDescriptor envelopeInfo) {
      Depth = envelopeInfo.Depth + (velocity * envelopeInfo.Vel2Depth);
      //Delay
      _stages[0].Offset = 0;
      _stages[0].Scale = 0;
      _stages[0].Time = Math.Max(0, (int)(sampleRate * (envelopeInfo.DelayTime + (envelopeInfo.Vel2Delay * velocity))));
      //Attack
      _stages[1].Offset = envelopeInfo.StartLevel;
      _stages[1].Scale = envelopeInfo.PeakLevel - envelopeInfo.StartLevel;
      _stages[1].Time = Math.Max(0, (int)(sampleRate * (envelopeInfo.AttackTime + (envelopeInfo.Vel2Attack * velocity))));
      _stages[1].Graph = Tables.EnvelopeTables[envelopeInfo.AttackGraph];
      //Hold
      _stages[2].Offset = 0;
      _stages[2].Scale = envelopeInfo.PeakLevel;
      _stages[2].Time = Math.Max(0, (int)(sampleRate * (envelopeInfo.HoldTime + (envelopeInfo.Vel2Hold * velocity))));
      //Decay
      _stages[3].Offset = envelopeInfo.SustainLevel;
      _stages[3].Scale = envelopeInfo.PeakLevel - envelopeInfo.SustainLevel;
      _stages[3].Time = Math.Max(0, (int)(sampleRate * (envelopeInfo.DecayTime + (envelopeInfo.Vel2Decay * velocity))));
      _stages[3].Graph = Tables.EnvelopeTables[envelopeInfo.DecayGraph];
      //Sustain
      _stages[4].Offset = 0;
      _stages[4].Scale = envelopeInfo.SustainLevel + (envelopeInfo.Vel2Sustain * velocity);
      _stages[4].Time = (int)(sampleRate * envelopeInfo.SustainTime);
      //Release
      _stages[5].Offset = 0;
      _stages[5].Scale = (_stages[3].Time == 0 && _stages[4].Time == 0) ? envelopeInfo.PeakLevel : _stages[4].Scale;
      _stages[5].Time = Math.Max(0, (int)(sampleRate * (envelopeInfo.ReleaseTime + (envelopeInfo.Vel2Release * velocity))));
      _stages[5].Graph = Tables.EnvelopeTables[envelopeInfo.ReleaseGraph];
      //None
      _stages[6].Scale = 0;
      //Reset value, index, and starting state
      _index = 0;
      Value = 0;
      CurrentState = EnvelopeStateEnum.Delay;
      while (_stages[(int)CurrentState].Time == 0) {
        CurrentState++;
      }
      _stage = _stages[(int)CurrentState];
    }
    public void QuickSetupSf2(int sampleRate, int note, short keyNumToHold, short keyNumToDecay, bool isVolumeEnvelope, EnvelopeDescriptor envelopeInfo) {
      Depth = envelopeInfo.Depth;
      //Delay
      _stages[0].Offset = 0;
      _stages[0].Scale = 0;
      _stages[0].Time = Math.Max(0, (int)(sampleRate * envelopeInfo.DelayTime));
      //Attack
      _stages[1].Offset = envelopeInfo.StartLevel;
      _stages[1].Scale = envelopeInfo.PeakLevel - envelopeInfo.StartLevel;
      _stages[1].Time = Math.Max(0, (int)(sampleRate * envelopeInfo.AttackTime));
      _stages[1].Graph = Tables.EnvelopeTables[envelopeInfo.AttackGraph];
      //Hold
      _stages[2].Offset = 0;
      _stages[2].Scale = envelopeInfo.PeakLevel;
      _stages[2].Time = Math.Max(0, (int)(sampleRate * envelopeInfo.HoldTime * Math.Pow(2, (60 - note) * keyNumToHold / 1200.0)));
      //Decay
      _stages[3].Offset = envelopeInfo.SustainLevel;
      _stages[3].Scale = envelopeInfo.PeakLevel - envelopeInfo.SustainLevel;
      if (envelopeInfo.SustainLevel == envelopeInfo.PeakLevel) {
        _stages[3].Time = 0;
      }
      else {
        _stages[3].Time = Math.Max(0, (int)(sampleRate * envelopeInfo.DecayTime * Math.Pow(2, (60 - note) * keyNumToDecay / 1200.0)));
      }

      _stages[3].Graph = Tables.EnvelopeTables[envelopeInfo.DecayGraph];
      //Sustain
      _stages[4].Offset = 0;
      _stages[4].Scale = envelopeInfo.SustainLevel;
      _stages[4].Time = (int)(sampleRate * envelopeInfo.SustainTime);
      //Release
      _stages[5].Scale = _stages[3].Time == 0 && _stages[4].Time == 0 ? envelopeInfo.PeakLevel : _stages[4].Scale;
      if (isVolumeEnvelope) {
        _stages[5].Offset = -100;
        _stages[5].Scale += 100;
        _stages[6].Scale = -100;
      }
      else {
        _stages[5].Offset = 0;
        _stages[6].Scale = 0;
      }
      _stages[5].Time = Math.Max(0, (int)(sampleRate * envelopeInfo.ReleaseTime));
      _stages[5].Graph = Tables.EnvelopeTables[envelopeInfo.ReleaseGraph];
      //Reset value, index, and starting state
      _index = 0;
      Value = 0;
      CurrentState = EnvelopeStateEnum.Delay;
      while (_stages[(int)CurrentState].Time == 0) {
        CurrentState++;
      }
      _stage = _stages[(int)CurrentState];
    }
    public void Increment(int samples) {
      do {
        var neededSamples = _stage.Time - _index;
        if (neededSamples > samples) {
          _index += samples;
          samples = 0;
        }
        else {
          _index = 0;
          if (CurrentState != EnvelopeStateEnum.None) {
            do {
              _stage = _stages[(int)++CurrentState];
            }
            while (_stage.Time == 0);
          }
          samples -= neededSamples;
        }
      }
      while (samples > 0);

      var i = (int)(_stage.Graph.Length * (_index / (double)_stage.Time));
      if (_stage.Reverse) {
        Value = ((1f - _stage.Graph[i]) * _stage.Scale) + _stage.Offset;
      }
      else {
        Value = (_stage.Graph[i] * _stage.Scale) + _stage.Offset;
      }
    }
    public void Release(float lowerLimit) {
      if (Value <= lowerLimit) {
        _index = 0;
        CurrentState = EnvelopeStateEnum.None;
        _stage = _stages[(int)CurrentState];
      }
      else if (CurrentState < EnvelopeStateEnum.Release) {
        _index = 0;
        CurrentState = EnvelopeStateEnum.Release;
        _stage = _stages[(int)CurrentState];
        _stage.Scale = Value;
      }
    }
    public void ReleaseSf2VolumeEnvelope() {
      if (Value <= -100) {
        _index = 0;
        CurrentState = EnvelopeStateEnum.None;
        _stage = _stages[(int)CurrentState];
      }
      else if (CurrentState < EnvelopeStateEnum.Release) {
        _index = 0;
        CurrentState = EnvelopeStateEnum.Release;
        _stage = _stages[(int)CurrentState];
        _stage.Offset = -100;
        _stage.Scale = 100 + Value;
      }
    }
    public override string ToString() => string.Format("State: {0}, Time: {1}%, Value: {2:0.00}", CurrentState, (int)(_index / (float)_stage.Time * 100f), Value);
  }
}
