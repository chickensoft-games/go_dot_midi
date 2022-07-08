namespace AudioSynthesis.Synthesis {
  using System;
  using AudioSynthesis.Bank.Components;
  using AudioSynthesis.Bank.Components.Generators;

  public class VoiceParameters {
    public int Channel;
    public int Note;
    public int Velocity;
    public bool NoteOffPending;
    public VoiceStateEnum State;
    public int PitchOffset;
    public float VolOffset;
    public float[] BlockBuffer;
    public UnionData[] PData;    //used for anything, counters, params, or mixing
    public SynthParameters SynthParams = null!;
    public GeneratorParameters[] GeneratorParams;
    public Envelope[] Envelopes;    //set by parameters (quicksetup)
    public Filter[] Filters;        //set by parameters (quicksetup)
    public Lfo[] Lfos;              //set by parameters (quicksetup)
    private float _mix1, _mix2;

    public float CombinedVolume => _mix1 + _mix2;

    public VoiceParameters() {
      BlockBuffer = new float[Synthesizer.DEFAULT_BLOCK_SIZE];
      //create default number of each component
      PData = new UnionData[Synthesizer.MAX_VOICE_COMPONENTS];
      GeneratorParams = new GeneratorParameters[Synthesizer.MAX_VOICE_COMPONENTS];
      Envelopes = new Envelope[Synthesizer.MAX_VOICE_COMPONENTS];
      Filters = new Filter[Synthesizer.MAX_VOICE_COMPONENTS];
      Lfos = new Lfo[Synthesizer.MAX_VOICE_COMPONENTS];
      //initialize each component
      for (var x = 0; x < Synthesizer.MAX_VOICE_COMPONENTS; x++) {
        GeneratorParams[x] = new GeneratorParameters();
        Envelopes[x] = new Envelope();
        Filters[x] = new Filter();
        Lfos[x] = new Lfo();
      }
    }
    public void Reset() {
      NoteOffPending = false;
      PitchOffset = 0;
      VolOffset = 0;
      Array.Clear(PData, 0, PData.Length);
      _mix1 = 0;
      _mix2 = 0;
    }
    public void MixMonoToMonoInterp(int startIndex, float volume) {
      var inc = (volume - _mix1) / Synthesizer.DEFAULT_BLOCK_SIZE;
      for (var i = 0; i < BlockBuffer.Length; i++) {
        _mix1 += inc;
        SynthParams.Synth.SampleBuffer[startIndex + i] += BlockBuffer[i] * _mix1;
      }
      _mix1 = volume;
    }
    public void MixMonoToStereoInterp(int startIndex, float leftVol, float rightVol) {
      var inc_l = (leftVol - _mix1) / Synthesizer.DEFAULT_BLOCK_SIZE;
      var inc_r = (rightVol - _mix2) / Synthesizer.DEFAULT_BLOCK_SIZE;
      for (var i = 0; i < BlockBuffer.Length; i++) {
        _mix1 += inc_l;
        _mix2 += inc_r;
        SynthParams.Synth.SampleBuffer[startIndex] += BlockBuffer[i] * _mix1;
        SynthParams.Synth.SampleBuffer[startIndex + 1] += BlockBuffer[i] * _mix2;
        startIndex += 2;
      }
      _mix1 = leftVol;
      _mix2 = rightVol;
    }
    public void MixStereoToStereoInterp(int startIndex, float leftVol, float rightVol) {
      var inc_l = (leftVol - _mix1) / Synthesizer.DEFAULT_BLOCK_SIZE;
      var inc_r = (rightVol - _mix2) / Synthesizer.DEFAULT_BLOCK_SIZE;
      for (var i = 0; i < BlockBuffer.Length; i++) {
        _mix1 += inc_l;
        _mix2 += inc_r;
        SynthParams.Synth.SampleBuffer[startIndex + i] += BlockBuffer[i] * _mix1;
        i++;
        SynthParams.Synth.SampleBuffer[startIndex + i] += BlockBuffer[i] * _mix2;
      }
      _mix1 = leftVol;
      _mix2 = rightVol;
    }

    public override string ToString() => string.Format("Channel: {0}, Key: {1}, Velocity: {2}, State: {3}", Channel, Note, Velocity, State);
  }
}
