using System.Collections.Generic;
using System.IO;
using System.Text;
using AudioSynthesis.Bank.Components;
using AudioSynthesis.Util;

namespace AudioSynthesis.Sfz {
  public class SfzReader {
    private string name;
    private SfzRegion[] regionList;

    public string Name {
      get { return name; }
      set { name = value; }
    }
    public SfzRegion[] Regions {
      get { return regionList; }
    }

    public SfzReader(Stream stream, string name) {
      this.name = IOHelper.GetFileNameWithoutExtension(name);
      Load(stream);
    }

    private void Load(Stream reader) {
      List<SfzRegion> regions = new List<SfzRegion>();
      using (reader) {
        SfzRegion group = new SfzRegion(true);
        SfzRegion global = new SfzRegion(true);
        SfzRegion master = new SfzRegion(true);
        string[] regionText = new string[2];
        ReadNextString(reader, '<'); //skip everything before the first region
        while (ReadNextRegion(reader, regionText)) {
          switch (regionText[0].ToLower()) {
            case "global":
              ToRegion(regionText[1], global);
              break;
            case "master":
              ToRegion(regionText[1], master);
              break;
            case "group":
              ToRegion(regionText[1], group);
              break;
            case "region":
              SfzRegion r = new SfzRegion(false);
              r.ApplyGlobal(global);
              r.ApplyGlobal(master);
              r.ApplyGlobal(group);
              ToRegion(regionText[1], r);
              if (!r.Sample.Equals(string.Empty))
                regions.Add(r);
              break;
            default:
              break;
          }
        }
      }
      regionList = regions.ToArray();
    }
    private string ReadNextString(Stream reader, char marker) {
      StringBuilder sbuild = new StringBuilder();
      int i = reader.ReadByte();
      while (true) {
        if (i == -1 || i == marker)
          break;
        else if (i == '/') {
          i = reader.ReadByte();
          if (i == '/') {
            do { i = reader.ReadByte(); } while (i != '\n' && i != -1);
            i = reader.ReadByte();
          }
          else
            sbuild.Append('/');
        }
        else {
          sbuild.Append((char)i);
          i = reader.ReadByte();
        }
      }
      return sbuild.ToString();
    }
    private bool ReadNextRegion(Stream reader, string[] regionData) {
      regionData[0] = ReadNextString(reader, '>');
      regionData[1] = ReadNextString(reader, '<');
      return !regionData[0].Equals(string.Empty) || !regionData[1].Equals(string.Empty);
    }
    private void ToRegion(string regionText, SfzRegion region) {
      string[] param = new string[2];
      int index = ReadNextParam(0, regionText, param);
      while (index != -1) {
        string command = param[0].ToLower();
        string parameter = param[1].ToLower();
        switch (command) {
          case "sample":
            if (IOHelper.GetExtension(parameter).Equals(string.Empty))
              parameter += ".wav";
            region.Sample = parameter;
            break;
          case "lochan":
            region.LoChan = (byte)(int.Parse(parameter) - 1);
            break;
          case "hichan":
            region.HiChan = (byte)(int.Parse(parameter) - 1);
            break;
          case "lokey":
            region.LoKey = NoteNameToValue(parameter);
            break;
          case "hikey":
            region.HiKey = NoteNameToValue(parameter);
            break;
          case "key":
            region.LoKey = NoteNameToValue(parameter);
            region.HiKey = region.LoKey;
            region.PitchKeyCenter = region.LoKey;
            break;
          case "lovel":
            region.LoVel = byte.Parse(parameter);
            break;
          case "hivel":
            region.HiVel = byte.Parse(parameter);
            break;
          case "lobend":
            region.LoBend = short.Parse(parameter);
            break;
          case "hibend":
            region.HiBend = short.Parse(parameter);
            break;
          case "lochanaft":
            region.LoChanAft = byte.Parse(parameter);
            break;
          case "hichanaft":
            region.HiChanAft = byte.Parse(parameter);
            break;
          case "lopolyaft":
            region.LoPolyAft = byte.Parse(parameter);
            break;
          case "hipolyaft":
            region.HiPolyAft = byte.Parse(parameter);
            break;
          case "group":
            region.Group = int.Parse(parameter);
            break;
          case "off_by":
            region.OffBy = int.Parse(parameter);
            break;
          case "off_mode":
            region.OffMode = parameter.Equals("fast") ? SfzRegion.OffModeEnum.Fast : SfzRegion.OffModeEnum.Normal;
            break;
          case "delay":
            region.Delay = float.Parse(parameter);
            break;
          case "offset":
            region.Offset = int.Parse(parameter);
            break;
          case "end":
            region.End = int.Parse(parameter);
            break;
          case "count":
            region.Count = int.Parse(parameter);
            region.LoopMode = LoopModeEnum.OneShot;
            break;
          case "loop_mode":
            switch (parameter) {
              case "no_loop":
                region.LoopMode = LoopModeEnum.NoLoop;
                break;
              case "one_shot":
                region.LoopMode = LoopModeEnum.OneShot;
                break;
              case "loop_continuous":
                region.LoopMode = LoopModeEnum.Continuous;
                break;
              case "loop_sustain":
                region.LoopMode = LoopModeEnum.LoopUntilNoteOff;
                break;
              default:
                break;
            }
            break;
          case "loop_start":
            region.LoopStart = int.Parse(parameter);
            break;
          case "loop_end":
            region.LoopEnd = int.Parse(parameter);
            break;
          case "transpose":
            region.Transpose = short.Parse(parameter);
            break;
          case "tune":
            region.Tune = short.Parse(parameter);
            break;
          case "pitch_keycenter":
            region.PitchKeyCenter = NoteNameToValue(parameter);
            break;
          case "pitch_keytrack":
            region.PitchKeyTrack = short.Parse(parameter);
            break;
          case "pitch_veltrack":
            region.PitchVelTrack = short.Parse(parameter);
            break;
          case "pitcheg_delay":
            region.PitchEGDelay = float.Parse(parameter);
            break;
          case "pitcheg_start":
            region.PitchEGStart = float.Parse(parameter);
            break;
          case "pitcheg_attack":
            region.PitchEGAttack = float.Parse(parameter);
            break;
          case "pitcheg_hold":
            region.PitchEGHold = float.Parse(parameter);
            break;
          case "pitcheg_decay":
            region.PitchEGDecay = float.Parse(parameter);
            break;
          case "pitcheg_sustain":
            region.PitchEGSustain = float.Parse(parameter);
            break;
          case "pitcheg_release":
            region.PitchEGRelease = float.Parse(parameter);
            break;
          case "pitcheg_depth":
            region.PitchEGDepth = short.Parse(parameter);
            break;
          case "pitcheg_vel2delay":
            region.PitchEGVel2Delay = float.Parse(parameter);
            break;
          case "pitcheg_vel2attack":
            region.PitchEGVel2Attack = float.Parse(parameter);
            break;
          case "pitcheg_vel2hold":
            region.PitchEGVel2Hold = float.Parse(parameter);
            break;
          case "pitcheg_vel2decay":
            region.PitchEGVel2Decay = float.Parse(parameter);
            break;
          case "pitcheg_vel2sustain":
            region.PitchEGVel2Sustain = float.Parse(parameter);
            break;
          case "pitcheg_vel2release":
            region.PitchEGVel2Release = float.Parse(parameter);
            break;
          case "pitcheg_vel2depth":
            region.PitchEGVel2Depth = short.Parse(parameter);
            break;
          case "pitchlfo_delay":
            region.PitchLfoDelay = float.Parse(parameter);
            break;
          case "pitchlfo_freq":
            region.PitchLfoFrequency = float.Parse(parameter);
            break;
          case "pitchlfo_depth":
            region.PitchLfoDepth = short.Parse(parameter);
            break;
          case "fil_type":
            switch (parameter) {
              case "lpf_1p":
                region.FilterType = FilterTypeEnum.OnePoleLowpass;
                break;
              case "hpf_1p":
                region.FilterType = FilterTypeEnum.None;//unsupported
                break;
              case "lpf_2p":
                region.FilterType = FilterTypeEnum.BiquadLowpass;
                break;
              case "hpf_2p":
                region.FilterType = FilterTypeEnum.BiquadHighpass;
                break;
              case "bpf_2p":
                region.FilterType = FilterTypeEnum.None;//unsupported
                break;
              case "brf_2p":
                region.FilterType = FilterTypeEnum.None;//unsupported
                break;
              default:
                break;
            }
            break;
          case "cutoff":
            region.CutOff = float.Parse(parameter);
            break;
          case "resonance":
            region.Resonance = float.Parse(parameter);
            break;
          case "fil_keytrack":
            region.FilterKeyTrack = short.Parse(parameter);
            break;
          case "fil_keycenter":
            region.FilterKeyCenter = byte.Parse(parameter);
            break;
          case "fil_veltrack":
            region.FilterVelTrack = short.Parse(parameter);
            break;
          case "fileg_delay":
            region.FilterEGDelay = float.Parse(parameter);
            break;
          case "fileg_start":
            region.FilterEGStart = float.Parse(parameter);
            break;
          case "fileg_attack":
            region.FilterEGAttack = float.Parse(parameter);
            break;
          case "fileg_hold":
            region.FilterEGHold = float.Parse(parameter);
            break;
          case "fileg_decay":
            region.FilterEGDecay = float.Parse(parameter);
            break;
          case "fileg_sustain":
            region.FilterEGSustain = float.Parse(parameter);
            break;
          case "fileg_release":
            region.FilterEGRelease = float.Parse(parameter);
            break;
          case "fileg_depth":
            region.FilterEGDepth = short.Parse(parameter);
            break;
          case "fileg_vel2delay":
            region.FilterEGVel2Delay = float.Parse(parameter);
            break;
          case "fileg_vel2attack":
            region.FilterEGVel2Attack = float.Parse(parameter);
            break;
          case "fileg_vel2hold":
            region.FilterEGVel2Hold = float.Parse(parameter);
            break;
          case "fileg_vel2decay":
            region.FilterEGVel2Decay = float.Parse(parameter);
            break;
          case "fileg_vel2sustain":
            region.FilterEGVel2Sustain = float.Parse(parameter);
            break;
          case "fileg_vel2release":
            region.FilterEGVel2Release = float.Parse(parameter);
            break;
          case "fileg_vel2depth":
            region.FilterEGVel2Depth = short.Parse(parameter);
            break;
          case "fillfo_delay":
            region.FilterLfoDelay = float.Parse(parameter);
            break;
          case "fillfo_freq":
            region.FilterLfoFrequency = float.Parse(parameter);
            break;
          case "fillfo_depth":
            region.FilterLfoDepth = float.Parse(parameter);
            break;
          case "volume":
            region.Volume = float.Parse(parameter);
            break;
          case "pan":
            region.Pan = float.Parse(parameter);
            break;
          case "amp_keytrack":
            region.AmpKeyTrack = float.Parse(parameter);
            break;
          case "amp_keycenter":
            region.AmpKeyCenter = byte.Parse(parameter);
            break;
          case "amp_veltrack":
            region.AmpVelTrack = float.Parse(parameter);
            break;
          case "ampeg_delay":
            region.AmpEGDelay = float.Parse(parameter);
            break;
          case "ampeg_start":
            region.AmpEGStart = float.Parse(parameter);
            break;
          case "ampeg_attack":
            region.AmpEGAttack = float.Parse(parameter);
            break;
          case "ampeg_hold":
            region.AmpEGHold = float.Parse(parameter);
            break;
          case "ampeg_decay":
            region.AmpEGDecay = float.Parse(parameter);
            break;
          case "ampeg_sustain":
            region.AmpEGSustain = float.Parse(parameter);
            break;
          case "ampeg_release":
            region.AmpEGRelease = float.Parse(parameter);
            break;
          case "ampeg_vel2delay":
            region.AmpEGVel2Delay = float.Parse(parameter);
            break;
          case "ampeg_vel2attack":
            region.AmpEGVel2Attack = float.Parse(parameter);
            break;
          case "ampeg_vel2hold":
            region.AmpEGVel2Hold = float.Parse(parameter);
            break;
          case "ampeg_vel2decay":
            region.AmpEGVel2Decay = float.Parse(parameter);
            break;
          case "ampeg_vel2sustain":
            region.AmpEGVel2Sustain = float.Parse(parameter);
            break;
          case "ampeg_vel2release":
            region.AmpEGVel2Release = float.Parse(parameter);
            break;
          case "amplfo_delay":
            region.AmpLfoDelay = float.Parse(parameter);
            break;
          case "amplfo_freq":
            region.AmpLfoFrequency = float.Parse(parameter);
            break;
          case "amplfo_depth":
            region.AmpLfoDepth = float.Parse(parameter);
            break;
          default:
            break;
        }
        index = ReadNextParam(index, regionText, param);
      }
    }
    private int ReadNextParam(int index, string regionText, string[] result) {
      int i1, i2;
      i1 = regionText.IndexOf('=', index);
      if (i1 < 0 || i1 == regionText.Length - 1)
        return -1;
      result[0] = regionText.Substring(index, i1 - index).Trim();
      i2 = regionText.IndexOf('=', i1 + 1);
      if (i2 < 0)
        i2 = regionText.Length - 1;
      else {
        while (char.IsWhiteSpace(regionText[i2]))
          i2--;
        while (!char.IsWhiteSpace(regionText[i2]))
          i2--;
      }
      i1++;
      result[1] = regionText.Substring(i1, i2 - i1).Trim();
      return i2;
    }
    private byte NoteNameToValue(string name) {
      int value = 0, i;
      if (int.TryParse(name, out value))
        return (byte)value;
      const string notes = "cdefgab";
      int[] noteValues = { 0, 2, 4, 5, 7, 9, 11 };
      name = name.Trim().ToLower();

      for (i = 0; i < name.Length; i++) {
        int index = notes.IndexOf(name[i]);
        if (index >= 0) {
          value = noteValues[index];
          i++;
          break;
        }
      }
      while (i < name.Length) {
        if (name[i] == '#') {
          value--;
          i++;
          break;
        }
        else if (name[i] == 'b') {
          value--;
          i++;
          break;
        }
        i++;
      }
      string digit = string.Empty;
      while (i < name.Length) {
        if (char.IsDigit(name[i])) {
          digit += name[i];
          i++;
        }
        else
          break;
      }
      if (digit.Equals(string.Empty))
        digit = "0";
      return (byte)((int.Parse(digit) + 1) * 12 + value);
    }
  }
}
