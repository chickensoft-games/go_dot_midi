namespace AudioSynthesis.Bank {
  using System.Collections.Generic;

  public class AssetManager {
    public List<PatchAsset> PatchAssetList { get; }
    public List<SampleDataAsset> SampleAssetList { get; }

    public AssetManager() {
      PatchAssetList = new List<PatchAsset>();
      SampleAssetList = new List<SampleDataAsset>();
    }
    public PatchAsset FindPatch(string name) {
      for (var x = 0; x < PatchAssetList.Count; x++) {
        if (PatchAssetList[x].Name.Equals(name)) {
          return PatchAssetList[x];
        }
      }
      return null!;
    }
    public SampleDataAsset FindSample(string name) {
      for (var x = 0; x < SampleAssetList.Count; x++) {
        if (SampleAssetList[x].Name.Equals(name)) {
          return SampleAssetList[x];
        }
      }
      return null!;
    }
    //public void LoadSampleAsset(string assetName, string patchName, string directory)
    //{
    //    string assetNameWithoutExtension;
    //    string extension;
    //    if (Path.HasExtension(assetName))
    //    {
    //        assetNameWithoutExtension = Path.GetFileNameWithoutExtension(assetName);
    //        extension = Path.GetExtension(assetName).ToLower();
    //    }
    //    else
    //    {
    //        assetNameWithoutExtension = assetName;
    //        assetName += ".wav"; //assume .wav
    //        extension = ".wav";
    //    }
    //    if (FindSample(assetNameWithoutExtension) == null)
    //    {
    //        string waveAssetPath;
    //        if (CrossPlatformHelper.ResourceExists(assetName))
    //            waveAssetPath = assetName; //ex. "asset.wav"
    //        else if (CrossPlatformHelper.ResourceExists(directory + Path.DirectorySeparatorChar + assetName))
    //            waveAssetPath = directory + Path.DirectorySeparatorChar + assetName; //ex. "C:\asset.wav"
    //        else if (CrossPlatformHelper.ResourceExists(directory + "/SAMPLES/" + assetName))
    //            waveAssetPath = directory + "/SAMPLES/" + assetName; //ex. "C:\SAMPLES\asset.wav"
    //        else if (CrossPlatformHelper.ResourceExists(directory + Path.DirectorySeparatorChar + patchName + Path.DirectorySeparatorChar + assetName))
    //            waveAssetPath = directory + Path.DirectorySeparatorChar + patchName + Path.DirectorySeparatorChar + assetName; //ex. "C:\Piano\asset.wav"
    //        else
    //            throw new IOException("Could not find sample asset: (" + assetName + ") required for patch: " + patchName);
    //        using (BinaryReader reader = new BinaryReader(CrossPlatformHelper.OpenResource(waveAssetPath)))
    //        {
    //            switch (extension)
    //            {
    //                case ".wav":
    //                    sampleAssets.Add(new SampleDataAsset(assetNameWithoutExtension, WaveFileReader.ReadWaveFile(reader)));
    //                    break;
    //            }
    //        }
    //    }
    //}
  }
}
