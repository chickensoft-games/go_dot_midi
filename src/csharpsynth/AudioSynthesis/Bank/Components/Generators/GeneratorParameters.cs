namespace AudioSynthesis.Bank.Components.Generators {
  public class GeneratorParameters {
    public double Phase;
    public double CurrentStart;
    public double CurrentEnd;
    public GeneratorStateEnum CurrentState;

    public void QuickSetup(Generator generator) {
      CurrentStart = generator.StartPhase;
      Phase = CurrentStart + generator.Offset;
      switch (generator.LoopMode) {
        case LoopModeEnum.Continuous:
        case LoopModeEnum.LoopUntilNoteOff:
          if (Phase >= generator.EndPhase) {//phase is greater than the end index so generator is finished
            CurrentState = GeneratorStateEnum.Finished;
          }
          else if (Phase >= generator.LoopEndPhase) {//phase is greater than the loop end point so generator is in post loop
            CurrentState = GeneratorStateEnum.PostLoop;
            CurrentEnd = generator.EndPhase;
          }
          else if (Phase >= generator.LoopStartPhase) {//phase is greater than loop start so we are inside the loop
            CurrentState = GeneratorStateEnum.Loop;
            CurrentEnd = generator.LoopEndPhase;
            CurrentStart = generator.LoopStartPhase;
          }
          else {//phase is less than the loop so generator is in pre loop
            CurrentState = GeneratorStateEnum.PreLoop;
            CurrentEnd = generator.LoopStartPhase;
          }
          break;
        case LoopModeEnum.NoLoop:
          break;
        case LoopModeEnum.OneShot:
          break;
        default:
          CurrentEnd = generator.EndPhase;
          if (Phase >= CurrentEnd) {
            CurrentState = GeneratorStateEnum.Finished;
          }
          else {
            CurrentState = GeneratorStateEnum.PostLoop;
          }

          break;
      }
    }
    public override string ToString() => string.Format("State: {0}, Bounds: {1} to {2}, CurrentIndex: {3:0.00}", CurrentState, CurrentStart, CurrentEnd, Phase);
  }
}
