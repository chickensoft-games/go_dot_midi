
namespace AudioSynthesis.Bank.Patches {
  using System;
  using AudioSynthesis.Sf2;
  using AudioSynthesis.Synthesis;
  /* Patch containing other patches mapped to channel, velocity, or key ranges.
   * Must not contain any other MultiPatches within. */
  public class MultiPatch : Patch {
    private class PatchInterval {
      public Patch Patch = null!;
      public byte StartChannel = 0;
      public byte StartKey = 0;
      public byte StartVelocity = 0;
      public byte EndChannel = 15;
      public byte EndKey = 127;
      public byte EndVelocity = 127;

      public PatchInterval() {
      }
      public PatchInterval(Patch patch, byte startChannel, byte endChannel, byte startKey, byte endKey, byte startVelocity, byte endVelocity) {
        Patch = patch;
        StartChannel = startChannel;
        EndChannel = endChannel;
        StartKey = startKey;
        EndKey = endKey;
        StartVelocity = startVelocity;
        EndVelocity = endVelocity;
      }
      public bool CheckAllIntervals(int channel, int key, int velocity) => channel >= StartChannel && channel <= EndChannel &&
            key >= StartKey && key <= EndKey &&
            velocity >= StartVelocity && velocity <= EndVelocity;

      public bool CheckChannelAndKey(int channel, int key) => channel >= StartChannel && channel <= EndChannel &&
            key >= StartKey && key <= EndKey;
      public bool CheckKeyAndVelocity(int key, int velocity) => key >= StartKey && key <= EndKey &&
            velocity >= StartVelocity && velocity <= EndVelocity;
      public bool CheckKey(int key) => key >= StartKey && key <= EndKey;

      public override string ToString() => string.Format("{0}, Channel: {1}-{2}, Key: {3}-{4}, Velocity: {5}-{6}", Patch, StartChannel, EndChannel, StartKey, EndKey, StartVelocity, EndVelocity);
    }
    private enum IntervalType { Channel_Key_Velocity, Channel_Key, Key_Velocity, Key };
    private IntervalType _iType;
    private PatchInterval[] _intervalList = null!;

    public MultiPatch(string name) : base(name) { }
    public int FindPatches(int channel, int key, int velocity, Patch[] layers) {
      var count = 0;
      switch (_iType) {
        case IntervalType.Channel_Key_Velocity:
          for (var x = 0; x < _intervalList.Length; x++) {
            if (_intervalList[x].CheckAllIntervals(channel, key, velocity)) {
              layers[count++] = _intervalList[x].Patch;
              if (count == layers.Length) {
                break;
              }
            }
          }
          break;
        case IntervalType.Channel_Key:
          for (var x = 0; x < _intervalList.Length; x++) {
            if (_intervalList[x].CheckChannelAndKey(channel, key)) {
              layers[count++] = _intervalList[x].Patch;
              if (count == layers.Length) {
                break;
              }
            }
          }
          break;
        case IntervalType.Key_Velocity:
          for (var x = 0; x < _intervalList.Length; x++) {
            if (_intervalList[x].CheckKeyAndVelocity(key, velocity)) {
              layers[count++] = _intervalList[x].Patch;
              if (count == layers.Length) {
                break;
              }
            }
          }
          break;
        case IntervalType.Key:
          for (var x = 0; x < _intervalList.Length; x++) {
            if (_intervalList[x].CheckKey(key)) {
              layers[count++] = _intervalList[x].Patch;
              if (count == layers.Length) {
                break;
              }
            }
          }
          break;
        default:
          break;
      }
      return count;
    }
    public override bool Start(VoiceParameters voiceparams) => throw new NotImplementedException();
    public override void Stop(VoiceParameters voiceparams) => throw new NotImplementedException();
    public override void Process(VoiceParameters voiceparams, int startIndex, int endIndex) => throw new NotImplementedException();
    public override void Load(DescriptorList description, AssetManager assets) {
      _intervalList = new PatchInterval[description.CustomDescriptions.Length];
      for (var x = 0; x < _intervalList.Length; x++) {
        if (!description.CustomDescriptions[x].ID.ToLower().Equals("mpat")) {
          throw new Exception(string.Format("The patch: {0} has an invalid descriptor with id {1}", _patchName, description.CustomDescriptions[x].ID));
        }

        var patchName = (string)description.CustomDescriptions[x].Objects[0];
        var pAsset = assets.FindPatch(patchName);
        if (pAsset == null) {
          throw new Exception(string.Format("The patch: {0} could not be found. For multi patches all sub patches must be loaded first.", patchName));
        }

        var sChan = (byte)description.CustomDescriptions[x].Objects[1];
        var eChan = (byte)description.CustomDescriptions[x].Objects[2];
        var sKey = (byte)description.CustomDescriptions[x].Objects[3];
        var eKey = (byte)description.CustomDescriptions[x].Objects[4];
        var sVel = (byte)description.CustomDescriptions[x].Objects[5];
        var eVel = (byte)description.CustomDescriptions[x].Objects[6];
        _intervalList[x] = new PatchInterval(pAsset.Patch, sChan, eChan, sKey, eKey, sVel, eVel);
      }
      DetermineIntervalType();
    }
    //public void LoadSfz(SfzRegion[] regions, AssetManager assets, string directory)
    //{
    //    //Load sub instruments first
    //    intervalList = new PatchInterval[regions.Length];
    //    for (int x = 0; x < intervalList.Length; x++)
    //    {
    //        SfzRegion r = regions[x];
    //        DescriptorList descList = new DescriptorList(r);
    //        assets.LoadSampleAsset(descList.GenDescriptions[0].AssetName, patchName, directory);
    //        SfzPatch sfzPatch = new SfzPatch(patchName + "_" + x);
    //        sfzPatch.Load(descList, assets);
    //        intervalList[x] = new PatchInterval(sfzPatch, r.loChan, r.hiChan, r.loKey, r.hiKey, r.loVel, r.hiVel);
    //    }
    //    DetermineIntervalType();
    //}
    public void LoadSf2(Sf2Region[] regions, AssetManager assets) {
      _intervalList = new PatchInterval[regions.Length];
      for (var x = 0; x < _intervalList.Length; x++) {
        byte loKey;
        byte hiKey;
        byte loVel;
        byte hiVel;
        if (BitConverter.IsLittleEndian) {
          loKey = (byte)(regions[x].Generators[(int)GeneratorEnum.KeyRange] & 0xFF);
          hiKey = (byte)((regions[x].Generators[(int)GeneratorEnum.KeyRange] >> 8) & 0xFF);
          loVel = (byte)(regions[x].Generators[(int)GeneratorEnum.VelocityRange] & 0xFF);
          hiVel = (byte)((regions[x].Generators[(int)GeneratorEnum.VelocityRange] >> 8) & 0xFF);
        }
        else {
          hiKey = (byte)(regions[x].Generators[(int)GeneratorEnum.KeyRange] & 0xFF);
          loKey = (byte)((regions[x].Generators[(int)GeneratorEnum.KeyRange] >> 8) & 0xFF);
          hiVel = (byte)(regions[x].Generators[(int)GeneratorEnum.VelocityRange] & 0xFF);
          loVel = (byte)((regions[x].Generators[(int)GeneratorEnum.VelocityRange] >> 8) & 0xFF);
        }
        var sf2 = new Sf2Patch(_patchName + "_" + x);
        sf2.Load(regions[x], assets);
        _intervalList[x] = new PatchInterval(sf2, 0, 15, loKey, hiKey, loVel, hiVel);
      }
      DetermineIntervalType();
    }
    public override string ToString() => string.Format("MultiPatch: {0}, IntervalCount: {1}, IntervalType: {2}", _patchName, _intervalList.Length, _iType);

    private void DetermineIntervalType() {//see if checks on channel and velocity intervals are necessary
      var checkChannel = false;
      var checkVelocity = false;
      for (var x = 0; x < _intervalList.Length; x++) {
        if (_intervalList[x].StartChannel != 0 || _intervalList[x].EndChannel != 15) {
          checkChannel = true;
          if (checkChannel && checkVelocity) {
            break;
          }
        }
        if (_intervalList[x].StartVelocity != 0 || _intervalList[x].EndVelocity != 127) {
          checkVelocity = true;
          if (checkChannel && checkVelocity) {
            break;
          }
        }
      }
      if (checkChannel & checkVelocity) {
        _iType = IntervalType.Channel_Key_Velocity;
      }
      else if (checkChannel) {
        _iType = IntervalType.Channel_Key;
      }
      else if (checkVelocity) {
        _iType = IntervalType.Key_Velocity;
      }
      else {
        _iType = IntervalType.Key;
      }
    }
  }
}
