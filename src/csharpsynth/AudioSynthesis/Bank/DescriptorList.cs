namespace AudioSynthesis.Bank {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using AudioSynthesis.Bank.Descriptors;
  using AudioSynthesis.Sfz;
  using AudioSynthesis.Synthesis;
  using AudioSynthesis.Util;

  public class DescriptorList {
    //--Fields
    public EnvelopeDescriptor[] EnvelopeDescriptions = null!;
    public FilterDescriptor[] FilterDescriptions = null!;
    public LfoDescriptor[] LfoDescriptions = null!;
    public GeneratorDescriptor[] GenDescriptions = null!;
    public CustomDescriptor[] CustomDescriptions = null!;

    //--Properties
    public int DescriptorCount => EnvelopeDescriptions.Length + FilterDescriptions.Length + LfoDescriptions.Length + GenDescriptions.Length + CustomDescriptions.Length;

    //--Methods
    public DescriptorList() {
      EnvelopeDescriptions = new EnvelopeDescriptor[0];
      FilterDescriptions = new FilterDescriptor[0];
      LfoDescriptions = new LfoDescriptor[0];
      GenDescriptions = new GeneratorDescriptor[0];
      CustomDescriptions = new CustomDescriptor[0];
    }
    public DescriptorList(StreamReader reader) {
      var envList = new List<EnvelopeDescriptor>();
      var fltrList = new List<FilterDescriptor>();
      var lfoList = new List<LfoDescriptor>();
      var genList = new List<GeneratorDescriptor>();
      var cList = new List<CustomDescriptor>();
      var descList = new List<string>();
      while (!reader.EndOfStream) {
        var tag = ReadNextTag(reader, descList);
        switch (tag) {
          case "envelope": {
              var env = new EnvelopeDescriptor();
              env.Read(descList.ToArray());
              envList.Add(env);
              break;
            }
          case "generator": {
              var gen = new GeneratorDescriptor();
              gen.Read(descList.ToArray());
              genList.Add(gen);
              break;
            }
          case "filter": {
              var fltr = new FilterDescriptor();
              fltr.Read(descList.ToArray());
              fltrList.Add(fltr);
              break;
            }
          case "lfo": {
              var lfo = new LfoDescriptor();
              lfo.Read(descList.ToArray());
              lfoList.Add(lfo);
              break;
            }
          default:
            if (!tag.Equals(string.Empty)) {
              var cus = new CustomDescriptor(tag, 0);
              cus.Read(descList.ToArray());
              cList.Add(cus);
            }
            break;
        }
        descList.Clear();
      }
      EnvelopeDescriptions = envList.ToArray();
      FilterDescriptions = fltrList.ToArray();
      LfoDescriptions = lfoList.ToArray();
      GenDescriptions = genList.ToArray();
      CustomDescriptions = cList.ToArray();
    }
    public DescriptorList(BinaryReader reader) {
      var envList = new List<EnvelopeDescriptor>();
      var fltrList = new List<FilterDescriptor>();
      var lfoList = new List<LfoDescriptor>();
      var genList = new List<GeneratorDescriptor>();
      var cList = new List<CustomDescriptor>();
      int count = reader.ReadInt16();
      for (var x = 0; x < count; x++) {
        var id = new string(IOHelper.Read8BitChars(reader, 4));
        var size = reader.ReadInt32();
        switch (id.ToLower()) {
          case EnvelopeDescriptor.ID: {
              var env = new EnvelopeDescriptor();
              env.Read(reader);
              envList.Add(env);
              break;
            }
          case GeneratorDescriptor.ID: {
              var gen = new GeneratorDescriptor();
              gen.Read(reader);
              genList.Add(gen);
              break;
            }
          case FilterDescriptor.ID: {
              var fltr = new FilterDescriptor();
              fltr.Read(reader);
              fltrList.Add(fltr);
              break;
            }
          case LfoDescriptor.ID: {
              var lfo = new LfoDescriptor();
              lfo.Read(reader);
              lfoList.Add(lfo);
              break;
            }
          default: {
              var cus = new CustomDescriptor(id, size);
              cus.Read(reader);
              cList.Add(cus);
              break;
            }
        }
      }
      EnvelopeDescriptions = envList.ToArray();
      FilterDescriptions = fltrList.ToArray();
      LfoDescriptions = lfoList.ToArray();
      GenDescriptions = genList.ToArray();
      CustomDescriptions = cList.ToArray();
    }
    public DescriptorList(SfzRegion region) {
      LoadSfzEnvelopes(region);
      LoadSfzFilters(region);
      LoadSfzLfos(region);
      LoadSfzGens(region);
      LoadSfzCustom(region);
    }

    public CustomDescriptor FindCustomDescriptor(string name) {
      for (var x = 0; x < CustomDescriptions.Length; x++) {
        if (CustomDescriptions[x].ID.Equals(name)) {
          return CustomDescriptions[x];
        }
      }
      return null!;
    }
    public void Write(BinaryWriter writer) {
      for (var x = 0; x < EnvelopeDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, EnvelopeDescriptor.ID, 4);
        writer.Write(EnvelopeDescriptor.SIZE);
        EnvelopeDescriptions[x].Write(writer);
      }
      for (var x = 0; x < FilterDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, FilterDescriptor.ID, 4);
        writer.Write(FilterDescriptor.SIZE);
        FilterDescriptions[x].Write(writer);
      }
      for (var x = 0; x < LfoDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, LfoDescriptor.ID, 4);
        writer.Write(LfoDescriptor.SIZE);
        LfoDescriptions[x].Write(writer);
      }
      for (var x = 0; x < GenDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, GeneratorDescriptor.ID, 4);
        writer.Write(GeneratorDescriptor.SIZE);
        GenDescriptions[x].Write(writer);
      }
      for (var x = 0; x < CustomDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, CustomDescriptions[x].ID, 4);
        writer.Write(CustomDescriptions[x].Size);
        CustomDescriptions[x].Write(writer);
      }
    }

    private void LoadSfzEnvelopes(SfzRegion region) {
      EnvelopeDescriptions = new EnvelopeDescriptor[3];
      EnvelopeDescriptions[0] = new EnvelopeDescriptor {
        DelayTime = region.PitchEGDelay,
        AttackTime = region.PitchEGAttack,
        HoldTime = region.PitchEGHold,
        DecayTime = region.PitchEGDecay,
        SustainLevel = region.PitchEGSustain / 100f,
        ReleaseTime = region.PitchEGRelease,
        StartLevel = region.PitchEGStart / 100f,
        Depth = region.PitchEGDepth,
        Vel2Delay = region.PitchEGVel2Delay,
        Vel2Attack = region.PitchEGVel2Attack,
        Vel2Hold = region.PitchEGVel2Hold,
        Vel2Decay = region.PitchEGVel2Decay,
        Vel2Sustain = region.PitchEGVel2Sustain,
        Vel2Release = region.PitchEGVel2Release,
        Vel2Depth = region.PitchEGVel2Depth
      };
      EnvelopeDescriptions[1] = new EnvelopeDescriptor {
        DelayTime = region.FilterEGDelay,
        AttackTime = region.FilterEGAttack,
        HoldTime = region.FilterEGHold,
        DecayTime = region.FilterEGDecay,
        SustainLevel = region.FilterEGSustain / 100f,
        ReleaseTime = region.FilterEGRelease,
        StartLevel = region.FilterEGStart / 100f,
        Depth = region.FilterEGDepth,
        Vel2Delay = region.FilterEGVel2Delay,
        Vel2Attack = region.FilterEGVel2Attack,
        Vel2Hold = region.FilterEGVel2Hold,
        Vel2Decay = region.FilterEGVel2Decay,
        Vel2Sustain = region.FilterEGVel2Sustain,
        Vel2Release = region.FilterEGVel2Release,
        Vel2Depth = region.FilterEGVel2Depth
      };
      EnvelopeDescriptions[2] = new EnvelopeDescriptor {
        DelayTime = region.AmpEGDelay,
        AttackTime = region.AmpEGAttack,
        HoldTime = region.AmpEGHold,
        DecayTime = region.AmpEGDecay,
        SustainLevel = region.AmpEGSustain / 100f,
        ReleaseTime = region.AmpEGRelease,
        StartLevel = region.AmpEGStart / 100f,
        Depth = 1f,
        Vel2Delay = region.AmpEGVel2Delay,
        Vel2Attack = region.AmpEGVel2Attack,
        Vel2Hold = region.AmpEGVel2Hold,
        Vel2Decay = region.AmpEGVel2Decay,
        Vel2Sustain = region.AmpEGVel2Sustain,
        Vel2Release = region.AmpEGVel2Release,
        Vel2Depth = 0f
      };
    }
    private void LoadSfzFilters(SfzRegion region) {
      FilterDescriptions = new FilterDescriptor[1];
      FilterDescriptions[0] = new FilterDescriptor {
        FilterMethod = region.FilterType,
        CutOff = region.CutOff,
        KeyTrack = region.FilterKeyTrack,
        Resonance = (float)SynthHelper.DBtoLinear(region.Resonance),
        RootKey = region.FilterKeyCenter,
        VelTrack = region.FilterVelTrack
      };
    }
    private void LoadSfzLfos(SfzRegion region) {
      LfoDescriptions = new LfoDescriptor[3];
      LfoDescriptions[0] = new LfoDescriptor {
        DelayTime = region.PitchLfoDelay, //make sure pitch lfo is enabled for midi mod event
        Frequency = region.PitchLfoFrequency > 0 ? region.PitchLfoFrequency : (float)Synthesizer.DEFAULT_LFO_FREQUENCY,
        Depth = region.PitchLfoDepth
      };
      LfoDescriptions[1] = new LfoDescriptor {
        DelayTime = region.FilterLfoDelay,
        Frequency = region.FilterLfoFrequency,
        Depth = region.FilterLfoDepth
      };
      LfoDescriptions[2] = new LfoDescriptor {
        DelayTime = region.AmpLfoDelay,
        Frequency = region.AmpLfoFrequency,
        Depth = (float)SynthHelper.DBtoLinear(region.AmpLfoDepth)
      };
    }
    private void LoadSfzGens(SfzRegion region) {
      GenDescriptions = new GeneratorDescriptor[1];
      GenDescriptions[0] = new GeneratorDescriptor {
        SamplerType = Components.WaveformEnum.SampleData,
        AssetName = region.Sample
      };
      //deal with end point
      if (region.End == -1) //-1 is silent region, so set end to 0 and let the generator figure it out later
{
        GenDescriptions[0].EndPhase = 0;
      }
      else if (region.End == 0) //set end out of range and let the descriptor default it to the proper end value
{
        GenDescriptions[0].EndPhase = -1;
      }
      else //add one to the value because its inclusive
      {
        GenDescriptions[0].EndPhase = region.End + 1;
      }

      GenDescriptions[0].KeyTrack = region.PitchKeyTrack;
      //deal with loop end
      if (region.LoopEnd < 0) {
        GenDescriptions[0].LoopEndPhase = -1;
      }
      else {
        GenDescriptions[0].LoopEndPhase = region.LoopEnd + 1;
      }

      GenDescriptions[0].LoopMethod = region.LoopMode;
      if (region.LoopStart < 0) {
        GenDescriptions[0].LoopStartPhase = -1;
      }
      else {
        GenDescriptions[0].LoopStartPhase = region.LoopStart;
      }

      GenDescriptions[0].Offset = region.Offset;
      GenDescriptions[0].Rootkey = region.PitchKeyCenter;
      GenDescriptions[0].Tune = (short)(region.Tune + (region.Transpose * 100));
      GenDescriptions[0].VelTrack = region.PitchVelTrack;
    }
    private void LoadSfzCustom(SfzRegion region) {
      CustomDescriptions = new CustomDescriptor[1];
      CustomDescriptions[0] = new CustomDescriptor("sfzi", 32,
          new object[] { region.OffBy, region.Group, region.Volume, region.Pan / 100f, region.AmpKeyTrack, region.AmpKeyCenter, region.AmpVelTrack / 100f });
    }

    private static string ReadNextTag(StreamReader reader, List<string> descList) {
      string tagName;
      string closeTag;
      string description;
      var sbuild = new StringBuilder();
      var c = reader.Read();
      //skip anything outside of the tags
      while (c is not (-1) and not '<') {
        c = reader.Read();
      }
      //read opening tag
      c = reader.Read();
      while (c is not (-1) and not '>') {
        sbuild.Append((char)c);
        c = reader.Read();
      }
      tagName = sbuild.ToString().Trim().ToLower();
      sbuild.Length = 0;
      //read the description
      c = reader.Read();
      while (c is not (-1) and not '<') {
        sbuild.Append((char)c);
        c = reader.Read();
      }
      description = sbuild.ToString();
      sbuild.Length = 0;
      //read closing tag
      c = reader.Read();
      while (c is not (-1) and not '>') {
        sbuild.Append((char)c);
        c = reader.Read();
      }
      closeTag = sbuild.ToString().Trim().ToLower();
      if (closeTag.Length > 1 && closeTag.StartsWith("/") && closeTag[1..].Equals(tagName)) {
        descList.AddRange(description.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        return tagName;
      }
      else {
        throw new Exception("Invalid tag! <" + tagName + ">...<" + closeTag + ">");
      }
    }
  }
}
