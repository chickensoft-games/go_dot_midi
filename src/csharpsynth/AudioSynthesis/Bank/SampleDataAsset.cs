namespace AudioSynthesis.Bank {
  using System;
  using System.IO;
  using AudioSynthesis.Sf2;
  using AudioSynthesis.Util;
  using AudioSynthesis.Util.Riff;
  using AudioSynthesis.Wave;

  public class SampleDataAsset {
    public string Name { get; }
    public int Channels { get; } = 1;
    public int SampleRate { get; }
    public short RootKey { get; } = 60;
    public short Tune { get; } = 0;
    public double Start { get; }
    public double End { get; }
    public double LoopStart { get; } = -1;
    public double LoopEnd { get; } = -1;
    public PcmData SampleData { get; }

    public SampleDataAsset(int size, BinaryReader reader) {
      Name = IOHelper.Read8BitString(reader, 20);
      SampleRate = reader.ReadInt32();
      RootKey = reader.ReadInt16();
      Tune = reader.ReadInt16();
      LoopStart = reader.ReadDouble();
      LoopEnd = reader.ReadDouble();
      var bits = reader.ReadByte();
      var chans = reader.ReadByte();
      var data = reader.ReadBytes(size - 46);
      if (chans != Channels) //reformat to supported channels
{
        data = WaveHelper.GetChannelPcmData(data, bits, chans, Channels);
      }

      SampleData = PcmData.Create(bits, data, true);
      Start = 0;
      End = SampleData.Length;
    }
    public SampleDataAsset(string name, WaveFile wave) {
      Name = name ?? throw new ArgumentNullException("An asset must be given a valid name.");
      var smpl = wave.FindChunk<SamplerChunk>();
      if (smpl != null) {
        SampleRate = (int)(44100.0 * (1.0 / (smpl.SamplePeriod / 22675.0)));
        RootKey = (short)smpl.UnityNote;
        Tune = (short)(smpl.PitchFraction * 100);
        if (smpl.Loops.Length > 0) {
          //--WARNING ASSUMES: smpl.Loops[0].Type == SamplerChunk.SampleLoop.LoopType.Forward
          LoopStart = smpl.Loops[0].Start;
          LoopEnd = smpl.Loops[0].End + smpl.Loops[0].Fraction + 1;
        }
      }
      else {
        SampleRate = wave.Format.SampleRate;
      }
      var data = wave.Data.RawSampleData;
      if (wave.Format.ChannelCount != Channels) //reformat to supported channels
{
        data = WaveHelper.GetChannelPcmData(data, wave.Format.BitsPerSample, wave.Format.ChannelCount, Channels);
      }

      SampleData = PcmData.Create(wave.Format.BitsPerSample, data, true);
      Start = 0;
      End = SampleData.Length;
    }
    public SampleDataAsset(SampleHeader sample, SoundFontSampleData sampleData) {
      Name = sample.Name;
      SampleRate = sample.SampleRate;
      RootKey = sample.RootKey;
      Tune = sample.Tune;
      Start = sample.Start;
      End = sample.End;
      LoopStart = sample.StartLoop;
      LoopEnd = sample.EndLoop;
      SampleData = PcmData.Create(sampleData.BitsPerSample, sampleData.SampleData, true);
    }

    public override string ToString() => string.Format("Name: {0}, SampleRate: {1}Hz, Size: {2} bytes", Name, SampleRate, SampleData.Length);
  }
}
