namespace AudioSynthesis.Bank.Descriptors {
  using System;
  using System.IO;
  using AudioSynthesis.Bank.Components;
  using AudioSynthesis.Bank.Components.Generators;
  using AudioSynthesis.Util;

  public class GeneratorDescriptor : IDescriptor {
    public const string ID = "gen ";
    public const int SIZE = 80;

    public LoopModeEnum LoopMethod;
    public WaveformEnum SamplerType;
    public string AssetName = "";
    public double EndPhase;
    public double StartPhase;
    public double LoopEndPhase;
    public double LoopStartPhase;
    public double Offset;
    public double Period;
    public short Rootkey;
    public short KeyTrack;
    public short VelTrack;
    public short Tune;

    public GeneratorDescriptor() => ApplyDefault();
    public void Read(string[] description) {
      ApplyDefault();
      for (var x = 0; x < description.Length; x++) {
        var index = description[x].IndexOf('=');
        if (index >= 0 && index < description[x].Length) {
          var paramName = description[x][..index].Trim().ToLower();
          var paramValue = description[x][(index + 1)..].Trim();
          switch (paramName) {
            case "loopmode":
              LoopMethod = Generator.GetLoopModeFromString(paramValue.ToLower());
              break;
            case "type":
              SamplerType = Generator.GetWaveformFromString(paramValue.ToLower());
              break;
            case "assetname":
              AssetName = paramValue;
              break;
            case "endphase":
              EndPhase = double.Parse(paramValue);
              break;
            case "startphase":
              StartPhase = double.Parse(paramValue);
              break;
            case "loopendphase":
              LoopEndPhase = double.Parse(paramValue);
              break;
            case "loopstartphase":
              LoopStartPhase = double.Parse(paramValue);
              break;
            case "offset":
              Offset = double.Parse(paramValue);
              break;
            case "period":
              Period = double.Parse(paramValue);
              break;
            case "keycenter":
            case "rootkey":
              Rootkey = short.Parse(paramValue);
              break;
            case "keytrack":
              KeyTrack = short.Parse(paramValue);
              break;
            case "velocitytrack":
              VelTrack = short.Parse(paramValue);
              break;
            case "tune":
              Tune = short.Parse(paramValue);
              break;
            default:
              break;
          }
        }
      }
    }
    public int Read(BinaryReader reader) {
      LoopMethod = (LoopModeEnum)reader.ReadInt16();
      SamplerType = (WaveformEnum)reader.ReadInt16();
      AssetName = IOHelper.Read8BitString(reader, 20);
      EndPhase = reader.ReadDouble();
      StartPhase = reader.ReadDouble();
      LoopEndPhase = reader.ReadDouble();
      LoopStartPhase = reader.ReadDouble();
      Offset = reader.ReadDouble();
      Period = reader.ReadDouble();
      Rootkey = reader.ReadInt16();
      KeyTrack = reader.ReadInt16();
      VelTrack = reader.ReadInt16();
      Tune = reader.ReadInt16();
      return SIZE;
    }
    public int Write(BinaryWriter writer) {
      writer.Write((short)LoopMethod);
      writer.Write((short)SamplerType);
      IOHelper.Write8BitString(writer, AssetName, 20);
      writer.Write(EndPhase);
      writer.Write(StartPhase);
      writer.Write(LoopEndPhase);
      writer.Write(LoopStartPhase);
      writer.Write(Offset);
      writer.Write(Period);
      writer.Write(Rootkey);
      writer.Write(KeyTrack);
      writer.Write(VelTrack);
      writer.Write(Tune);
      return SIZE;
    }
    public Generator ToGenerator(AssetManager assets) => SamplerType switch {
      WaveformEnum.SampleData => new SampleGenerator(this, assets),
      WaveformEnum.Saw => new SawGenerator(this),
      WaveformEnum.Sine => new SineGenerator(this),
      WaveformEnum.Square => new SquareGenerator(this),
      WaveformEnum.Triangle => new TriangleGenerator(this),
      WaveformEnum.WhiteNoise => new WhiteNoiseGenerator(this),
      _ => throw new Exception(string.Format("Unsupported generator: {0}", SamplerType)),
    };

    private void ApplyDefault() {
      LoopMethod = LoopModeEnum.NoLoop;
      SamplerType = WaveformEnum.Sine;
      AssetName = "null";
      EndPhase = -1;
      StartPhase = -1;
      LoopEndPhase = -1;
      LoopStartPhase = -1;
      Offset = 0;
      Period = -1;
      Rootkey = -1;
      KeyTrack = 100;
      VelTrack = 0;
      Tune = 0;
    }
  }
}
