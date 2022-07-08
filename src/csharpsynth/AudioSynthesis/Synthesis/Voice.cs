using System;
using AudioSynthesis.Bank.Patches;

namespace AudioSynthesis.Synthesis {
  internal class Voice {
    // Variables
    private Patch patch;
    private VoiceParameters voiceparams;
    // Properties
    public Patch Patch {
      get { return patch; }
    }
    public VoiceParameters VoiceParams {
      get { return voiceparams; }
    }
    // Public
    public Voice() {
      voiceparams = new VoiceParameters();
    }
    public void Start() {
      if (voiceparams.State != VoiceStateEnum.Stopped)
        return;
      if (patch.Start(voiceparams))
        voiceparams.State = VoiceStateEnum.Playing;
    }
    public void Stop() {
      if (voiceparams.State != VoiceStateEnum.Playing)
        return;
      voiceparams.State = VoiceStateEnum.Stopping;
      patch.Stop(voiceparams);
    }
    public void StopImmediately() {
      voiceparams.State = VoiceStateEnum.Stopped;
    }
    public void Process(int startIndex, int endIndex) {
      //do not process if the voice is stopped
      if (voiceparams.State == VoiceStateEnum.Stopped)
        return;
      //process using the patch's algorithm
      patch.Process(voiceparams, startIndex, endIndex);
    }
    public void Configure(int channel, int note, int velocity, Patch patch, SynthParameters synthParams) {
      voiceparams.Reset();
      voiceparams.Channel = channel;
      voiceparams.Note = note;
      voiceparams.Velocity = velocity;
      voiceparams.SynthParams = synthParams;
      this.patch = patch;
    }
    public override string ToString() {
      return voiceparams.ToString() + ", PatchName: " + (patch == null ? "null" : patch.Name);
    }
  }
}
