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
    public EnvelopeDescriptor[] EnvelopeDescriptions;
    public FilterDescriptor[] FilterDescriptions;
    public LfoDescriptor[] LfoDescriptions;
    public GeneratorDescriptor[] GenDescriptions;
    public CustomDescriptor[] CustomDescriptions;

    //--Properties
    public int DescriptorCount {
      get { return EnvelopeDescriptions.Length + FilterDescriptions.Length + LfoDescriptions.Length + GenDescriptions.Length + CustomDescriptions.Length; }
    }

    //--Methods
    public DescriptorList() {
      EnvelopeDescriptions = new EnvelopeDescriptor[0];
      FilterDescriptions = new FilterDescriptor[0];
      LfoDescriptions = new LfoDescriptor[0];
      GenDescriptions = new GeneratorDescriptor[0];
      CustomDescriptions = new CustomDescriptor[0];
    }
    public DescriptorList(StreamReader reader) {
      List<EnvelopeDescriptor> envList = new List<EnvelopeDescriptor>();
      List<FilterDescriptor> fltrList = new List<FilterDescriptor>();
      List<LfoDescriptor> lfoList = new List<LfoDescriptor>();
      List<GeneratorDescriptor> genList = new List<GeneratorDescriptor>();
      List<CustomDescriptor> cList = new List<CustomDescriptor>();
      List<string> descList = new List<string>();
      while (!reader.EndOfStream) {
        string tag = ReadNextTag(reader, descList);
        switch (tag) {
          case "envelope": {
              EnvelopeDescriptor env = new EnvelopeDescriptor();
              env.Read(descList.ToArray());
              envList.Add(env);
              break;
            }
          case "generator": {
              GeneratorDescriptor gen = new GeneratorDescriptor();
              gen.Read(descList.ToArray());
              genList.Add(gen);
              break;
            }
          case "filter": {
              FilterDescriptor fltr = new FilterDescriptor();
              fltr.Read(descList.ToArray());
              fltrList.Add(fltr);
              break;
            }
          case "lfo": {
              LfoDescriptor lfo = new LfoDescriptor();
              lfo.Read(descList.ToArray());
              lfoList.Add(lfo);
              break;
            }
          default:
            if (!tag.Equals(string.Empty)) {
              CustomDescriptor cus = new CustomDescriptor(tag, 0);
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
      List<EnvelopeDescriptor> envList = new List<EnvelopeDescriptor>();
      List<FilterDescriptor> fltrList = new List<FilterDescriptor>();
      List<LfoDescriptor> lfoList = new List<LfoDescriptor>();
      List<GeneratorDescriptor> genList = new List<GeneratorDescriptor>();
      List<CustomDescriptor> cList = new List<CustomDescriptor>();
      int count = reader.ReadInt16();
      for (int x = 0; x < count; x++) {
        string id = new string(IOHelper.Read8BitChars(reader, 4));
        int size = reader.ReadInt32();
        switch (id.ToLower()) {
          case EnvelopeDescriptor.ID: {
              EnvelopeDescriptor env = new EnvelopeDescriptor();
              env.Read(reader);
              envList.Add(env);
              break;
            }
          case GeneratorDescriptor.ID: {
              GeneratorDescriptor gen = new GeneratorDescriptor();
              gen.Read(reader);
              genList.Add(gen);
              break;
            }
          case FilterDescriptor.ID: {
              FilterDescriptor fltr = new FilterDescriptor();
              fltr.Read(reader);
              fltrList.Add(fltr);
              break;
            }
          case LfoDescriptor.ID: {
              LfoDescriptor lfo = new LfoDescriptor();
              lfo.Read(reader);
              lfoList.Add(lfo);
              break;
            }
          default: {
              CustomDescriptor cus = new CustomDescriptor(id, size);
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
      for (int x = 0; x < CustomDescriptions.Length; x++) {
        if (CustomDescriptions[x].ID.Equals(name))
          return CustomDescriptions[x];
      }
      return null;
    }
    public void Write(BinaryWriter writer) {
      for (int x = 0; x < EnvelopeDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, EnvelopeDescriptor.ID, 4);
        writer.Write((int)EnvelopeDescriptor.SIZE);
        EnvelopeDescriptions[x].Write(writer);
      }
      for (int x = 0; x < FilterDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, FilterDescriptor.ID, 4);
        writer.Write((int)FilterDescriptor.SIZE);
        FilterDescriptions[x].Write(writer);
      }
      for (int x = 0; x < LfoDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, LfoDescriptor.ID, 4);
        writer.Write((int)LfoDescriptor.SIZE);
        LfoDescriptions[x].Write(writer);
      }
      for (int x = 0; x < GenDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, GeneratorDescriptor.ID, 4);
        writer.Write((int)GeneratorDescriptor.SIZE);
        GenDescriptions[x].Write(writer);
      }
      for (int x = 0; x < CustomDescriptions.Length; x++) {
        IOHelper.Write8BitString(writer, CustomDescriptions[x].ID, 4);
        writer.Write((int)CustomDescriptions[x].Size);
        CustomDescriptions[x].Write(writer);
      }
    }

    private void LoadSfzEnvelopes(SfzRegion region) {
      EnvelopeDescriptions = new EnvelopeDescriptor[3];
      EnvelopeDescriptions[0] = new EnvelopeDescriptor();
      EnvelopeDescriptions[0].DelayTime = region.PitchEGDelay;
      EnvelopeDescriptions[0].AttackTime = region.PitchEGAttack;
      EnvelopeDescriptions[0].HoldTime = region.PitchEGHold;
      EnvelopeDescriptions[0].DecayTime = region.PitchEGDecay;
      EnvelopeDescriptions[0].SustainLevel = region.PitchEGSustain / 100f;
      EnvelopeDescriptions[0].ReleaseTime = region.PitchEGRelease;
      EnvelopeDescriptions[0].StartLevel = region.PitchEGStart / 100f;
      EnvelopeDescriptions[0].Depth = region.PitchEGDepth;
      EnvelopeDescriptions[0].Vel2Delay = region.PitchEGVel2Delay;
      EnvelopeDescriptions[0].Vel2Attack = region.PitchEGVel2Attack;
      EnvelopeDescriptions[0].Vel2Hold = region.PitchEGVel2Hold;
      EnvelopeDescriptions[0].Vel2Decay = region.PitchEGVel2Decay;
      EnvelopeDescriptions[0].Vel2Sustain = region.PitchEGVel2Sustain;
      EnvelopeDescriptions[0].Vel2Release = region.PitchEGVel2Release;
      EnvelopeDescriptions[0].Vel2Depth = region.PitchEGVel2Depth;
      EnvelopeDescriptions[1] = new EnvelopeDescriptor();
      EnvelopeDescriptions[1].DelayTime = region.FilterEGDelay;
      EnvelopeDescriptions[1].AttackTime = region.FilterEGAttack;
      EnvelopeDescriptions[1].HoldTime = region.FilterEGHold;
      EnvelopeDescriptions[1].DecayTime = region.FilterEGDecay;
      EnvelopeDescriptions[1].SustainLevel = region.FilterEGSustain / 100f;
      EnvelopeDescriptions[1].ReleaseTime = region.FilterEGRelease;
      EnvelopeDescriptions[1].StartLevel = region.FilterEGStart / 100f;
      EnvelopeDescriptions[1].Depth = region.FilterEGDepth;
      EnvelopeDescriptions[1].Vel2Delay = region.FilterEGVel2Delay;
      EnvelopeDescriptions[1].Vel2Attack = region.FilterEGVel2Attack;
      EnvelopeDescriptions[1].Vel2Hold = region.FilterEGVel2Hold;
      EnvelopeDescriptions[1].Vel2Decay = region.FilterEGVel2Decay;
      EnvelopeDescriptions[1].Vel2Sustain = region.FilterEGVel2Sustain;
      EnvelopeDescriptions[1].Vel2Release = region.FilterEGVel2Release;
      EnvelopeDescriptions[1].Vel2Depth = region.FilterEGVel2Depth;
      EnvelopeDescriptions[2] = new EnvelopeDescriptor();
      EnvelopeDescriptions[2].DelayTime = region.AmpEGDelay;
      EnvelopeDescriptions[2].AttackTime = region.AmpEGAttack;
      EnvelopeDescriptions[2].HoldTime = region.AmpEGHold;
      EnvelopeDescriptions[2].DecayTime = region.AmpEGDecay;
      EnvelopeDescriptions[2].SustainLevel = region.AmpEGSustain / 100f;
      EnvelopeDescriptions[2].ReleaseTime = region.AmpEGRelease;
      EnvelopeDescriptions[2].StartLevel = region.AmpEGStart / 100f;
      EnvelopeDescriptions[2].Depth = 1f;
      EnvelopeDescriptions[2].Vel2Delay = region.AmpEGVel2Delay;
      EnvelopeDescriptions[2].Vel2Attack = region.AmpEGVel2Attack;
      EnvelopeDescriptions[2].Vel2Hold = region.AmpEGVel2Hold;
      EnvelopeDescriptions[2].Vel2Decay = region.AmpEGVel2Decay;
      EnvelopeDescriptions[2].Vel2Sustain = region.AmpEGVel2Sustain;
      EnvelopeDescriptions[2].Vel2Release = region.AmpEGVel2Release;
      EnvelopeDescriptions[2].Vel2Depth = 0f;
    }
    private void LoadSfzFilters(SfzRegion region) {
      FilterDescriptions = new FilterDescriptor[1];
      FilterDescriptions[0] = new FilterDescriptor();
      FilterDescriptions[0].FilterMethod = region.FilterType;
      FilterDescriptions[0].CutOff = region.CutOff;
      FilterDescriptions[0].KeyTrack = region.FilterKeyTrack;
      FilterDescriptions[0].Resonance = (float)SynthHelper.DBtoLinear(region.Resonance);
      FilterDescriptions[0].RootKey = region.FilterKeyCenter;
      FilterDescriptions[0].VelTrack = region.FilterVelTrack;
    }
    private void LoadSfzLfos(SfzRegion region) {
      LfoDescriptions = new LfoDescriptor[3];
      LfoDescriptions[0] = new LfoDescriptor();
      LfoDescriptions[0].DelayTime = region.PitchLfoDelay; //make sure pitch lfo is enabled for midi mod event
      LfoDescriptions[0].Frequency = region.PitchLfoFrequency > 0 ? region.PitchLfoFrequency : (float)Synthesizer.DEFAULT_LFO_FREQUENCY;
      LfoDescriptions[0].Depth = region.PitchLfoDepth;
      LfoDescriptions[1] = new LfoDescriptor();
      LfoDescriptions[1].DelayTime = region.FilterLfoDelay;
      LfoDescriptions[1].Frequency = region.FilterLfoFrequency;
      LfoDescriptions[1].Depth = region.FilterLfoDepth;
      LfoDescriptions[2] = new LfoDescriptor();
      LfoDescriptions[2].DelayTime = region.AmpLfoDelay;
      LfoDescriptions[2].Frequency = region.AmpLfoFrequency;
      LfoDescriptions[2].Depth = (float)SynthHelper.DBtoLinear(region.AmpLfoDepth);
    }
    private void LoadSfzGens(SfzRegion region) {
      GenDescriptions = new GeneratorDescriptor[1];
      GenDescriptions[0] = new GeneratorDescriptor();
      GenDescriptions[0].SamplerType = Components.WaveformEnum.SampleData;
      GenDescriptions[0].AssetName = region.Sample;
      //deal with end point
      if (region.End == -1) //-1 is silent region, so set end to 0 and let the generator figure it out later
        GenDescriptions[0].EndPhase = 0;
      else if (region.End == 0) //set end out of range and let the descriptor default it to the proper end value
        GenDescriptions[0].EndPhase = -1;
      else //add one to the value because its inclusive
        GenDescriptions[0].EndPhase = region.End + 1;
      GenDescriptions[0].KeyTrack = region.PitchKeyTrack;
      //deal with loop end
      if (region.LoopEnd < 0)
        GenDescriptions[0].LoopEndPhase = -1;
      else
        GenDescriptions[0].LoopEndPhase = region.LoopEnd + 1;
      GenDescriptions[0].LoopMethod = region.LoopMode;
      if (region.LoopStart < 0)
        GenDescriptions[0].LoopStartPhase = -1;
      else
        GenDescriptions[0].LoopStartPhase = region.LoopStart;
      GenDescriptions[0].Offset = region.Offset;
      GenDescriptions[0].Rootkey = region.PitchKeyCenter;
      GenDescriptions[0].Tune = (short)(region.Tune + region.Transpose * 100);
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
      StringBuilder sbuild = new StringBuilder();
      int c = reader.Read();
      //skip anything outside of the tags
      while (c != -1 && c != '<')
        c = reader.Read();
      //read opening tag
      c = reader.Read();
      while (c != -1 && c != '>') {
        sbuild.Append((char)c);
        c = reader.Read();
      }
      tagName = sbuild.ToString().Trim().ToLower();
      sbuild.Length = 0;
      //read the description
      c = reader.Read();
      while (c != -1 && c != '<') {
        sbuild.Append((char)c);
        c = reader.Read();
      }
      description = sbuild.ToString();
      sbuild.Length = 0;
      //read closing tag
      c = reader.Read();
      while (c != -1 && c != '>') {
        sbuild.Append((char)c);
        c = reader.Read();
      }
      closeTag = sbuild.ToString().Trim().ToLower();
      if (closeTag.Length > 1 && closeTag.StartsWith("/") && closeTag.Substring(1).Equals(tagName)) {
        descList.AddRange(description.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        return tagName;
      }
      else {
        throw new Exception("Invalid tag! <" + tagName + ">...<" + closeTag + ">");
      }
    }
  }
}
