namespace AudioSynthesis.Sf2 {
  using System.IO;
  using AudioSynthesis.Midi;

  public class ModulatorType {
    private readonly bool _midiContinuousController;
    private readonly ushort _controllerSource;

    public PolarityEnum Polarity { get; set; }
    public DirectionEnum Direction { get; set; }
    public SourceTypeEnum SourceType { get; set; }

    public ModulatorType(BinaryReader reader) {
      var raw = reader.ReadUInt16();

      if ((raw & 0x0200) == 0x0200) {
        Polarity = PolarityEnum.Bipolar;
      }
      else {
        Polarity = PolarityEnum.Unipolar;
      }

      if ((raw & 0x0100) == 0x0100) {
        Direction = DirectionEnum.MaxToMin;
      }
      else {
        Direction = DirectionEnum.MinToMax;
      }

      _midiContinuousController = (raw & 0x0080) == 0x0080;
      SourceType = (SourceTypeEnum)((raw & (0xFC00)) >> 10);
      _controllerSource = (ushort)(raw & 0x007F);
    }
    public bool IsMidiContinuousController() => _midiContinuousController;

    public override string ToString() {
      if (_midiContinuousController) {
        return string.Format("{0} : {1} : {2} : CC {3}", Polarity, Direction, SourceType, (ControllerTypeEnum)_controllerSource);
      }
      else {
        return string.Format("{0} : {1} : {2} : {3}", Polarity, Direction, SourceType, (ControllerSourceEnum)_controllerSource);
      }
    }
  }
}
