namespace AudioSynthesis.Bank.Patches {
  using AudioSynthesis.Synthesis;

  /// <summary>
  /// A patch allows a user to create different instrument types. They typically contain components such as generators,
  /// filters, and envelopes that can be connected or used in many different ways. A patch should not change any of
  /// it's fields after being loaded. Instead any changes should be implemented inside the abstract methods Process()
  /// and Start() and only effect a single voice instance.
  /// </summary>
  public abstract class Patch {
    protected string _patchName;
    protected int _exTarget;
    protected int _exGroup;
    //properties
    public int ExclusiveGroupTarget {
      get => _exTarget;
      set => _exTarget = value;
    }
    public int ExclusiveGroup {
      get => _exGroup;
      set => _exGroup = value;
    }
    public string Name => _patchName;
    //methods
    protected Patch(string name) {
      _patchName = name;
      _exTarget = 0;
      _exGroup = 0;
    }
    public abstract void Process(VoiceParameters voiceparams, int startIndex, int endIndex);
    public abstract bool Start(VoiceParameters voiceparams);
    public abstract void Stop(VoiceParameters voiceparams);
    public abstract void Load(DescriptorList description, AssetManager assets);
    public override string ToString() => string.Format("PatchName: {0}", _patchName);
  }
}
