
namespace AudioSynthesis.Synthesis {
  using AudioSynthesis.Bank.Patches;
  internal class Voice {
    // Properties
    public Patch Patch { get; private set; } = null!;
    public VoiceParameters VoiceParams { get; }
    // Public
    public Voice() => VoiceParams = new VoiceParameters();
    public void Start() {
      if (VoiceParams.State != VoiceStateEnum.Stopped) {
        return;
      }

      if (Patch.Start(VoiceParams)) {
        VoiceParams.State = VoiceStateEnum.Playing;
      }
    }
    public void Stop() {
      if (VoiceParams.State != VoiceStateEnum.Playing) {
        return;
      }

      VoiceParams.State = VoiceStateEnum.Stopping;
      Patch.Stop(VoiceParams);
    }
    public void StopImmediately() => VoiceParams.State = VoiceStateEnum.Stopped;
    public void Process(int startIndex, int endIndex) {
      //do not process if the voice is stopped
      if (VoiceParams.State == VoiceStateEnum.Stopped) {
        return;
      }
      //process using the patch's algorithm
      Patch.Process(VoiceParams, startIndex, endIndex);
    }
    public void Configure(int channel, int note, int velocity, Patch patch, SynthParameters synthParams) {
      VoiceParams.Reset();
      VoiceParams.Channel = channel;
      VoiceParams.Note = note;
      VoiceParams.Velocity = velocity;
      VoiceParams.SynthParams = synthParams;
      Patch = patch;
    }
    public override string ToString() => VoiceParams.ToString() + ", PatchName: " + (Patch == null ? "null" : Patch.Name);
  }
}
