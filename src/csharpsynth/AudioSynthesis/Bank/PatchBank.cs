namespace AudioSynthesis.Bank {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AudioSynthesis.Bank.Patches;
  using AudioSynthesis.Sf2;
  using AudioSynthesis.Util;
  using AudioSynthesis.Util.Riff;
  public class PatchBank {
    public enum PatchBankType {
      Sf2,
      Bank
    }

    public const float BANK_VERSION = 1.000f;
    public const int DRUM_BANK = 128;
    public const int BANK_SIZE = 128;
    private static readonly Dictionary<string, Type> _patchTypes;
    private readonly Dictionary<int, Patch[]> _bank;
    private readonly AssetManager _assets;

    //patch type mappings
    static PatchBank() {
      _patchTypes = new Dictionary<string, Type>();
      ClearCustomPatchTypes();
    }
    public static void AddCustomPatchType(string id, Type type) {//add a patch type/id pair to the map
      if (!type.IsSubclassOf(typeof(Patch))) {
        throw new Exception("Type must be a subtype of patch.");
      }

      if (_patchTypes.ContainsKey(id)) {
        throw new Exception("The specified id already exists.");
      }

      if (id.Length == 0 || id.Trim().Equals("")) {
        throw new Exception("The specified id is invalid.");
      }

      _patchTypes.Add(id, type);
    }
    public static void ClearCustomPatchTypes() {//clear any custom patch types from the map
      _patchTypes.Clear();
      _patchTypes.Add("mult", typeof(MultiPatch));
      _patchTypes.Add("basc", typeof(BasicPatch));
      _patchTypes.Add("fm2 ", typeof(Fm2Patch));
      _patchTypes.Add("sfz ", typeof(SfzPatch));
    }

    public string Name { get; private set; }
    public string Comments { get; private set; }

    public PatchBank(Stream soundFontFile, string name, PatchBankType type) {
      _bank = new Dictionary<int, Patch[]>();
      _assets = new AssetManager();
      Name = string.Empty;
      Comments = string.Empty;
      LoadBank(soundFontFile, name, type);
    }

    public void LoadBank(Stream soundFontFile, string name, PatchBankType type) {
      _bank.Clear();
      _assets.PatchAssetList.Clear();
      _assets.SampleAssetList.Clear();
      Name = name;
      Comments = string.Empty;
      switch (type) {
        case PatchBankType.Bank:
          LoadMyBank(soundFontFile);
          break;
        case PatchBankType.Sf2:
          LoadSf2(soundFontFile);
          break;
        default:
          throw new Exception("Invalid bank resource was provided. An extension must be included in the resource name.");
      }
      _assets.PatchAssetList.TrimExcess();
      _assets.SampleAssetList.TrimExcess();
    }
    //public void LoadPatch(string patchFile, int bankNumber, int startRange, int endRange)
    //{
    //    string patchName = Path.GetFileNameWithoutExtension(patchFile);
    //    string directory = Path.GetDirectoryName(patchFile);
    //    //check for duplicate patch
    //    PatchAsset patchAsset = assets.FindPatch(patchName);
    //    if (patchAsset != null)
    //    {
    //        AssignPatchToBank(patchAsset.Patch, bankNumber, startRange, endRange);
    //        return;
    //    }
    //    //load patch here
    //    Patch patch;
    //    switch (Path.GetExtension(patchFile).ToLower())
    //    {
    //        case ".patch":
    //            patch = LoadMyPatch(CrossPlatformHelper.OpenResource(patchFile), patchName, directory);
    //            break;
    //        case ".sfz":
    //            patch = LoadSfzPatch(CrossPlatformHelper.OpenResource(patchFile), patchName, directory);
    //            break;
    //        default:
    //            throw new Exception("The patch: " + Path.GetFileName(patchFile) + " is unsupported.");
    //    }
    //    AssignPatchToBank(patch, bankNumber, startRange, endRange);
    //    assets.PatchAssetList.Add(new PatchAsset(patchName, patch));
    //}
    public int[] GetLoadedBanks() {
      var copy = new int[_bank.Keys.Count];
      _bank.Keys.CopyTo(copy, 0);
      return copy;
    }
    public Patch[] GetBank(int bankNumber) {
      if (_bank.ContainsKey(bankNumber)) {
        return _bank[bankNumber];
      }

      return null!;
    }
    public Patch GetPatch(int bankNumber, int patchNumber) {
      if (_bank.ContainsKey(bankNumber)) {
        return _bank[bankNumber][patchNumber];
      }
      return null!;
    }
    public Patch GetPatch(int bankNumber, string name) {
      if (_bank.ContainsKey(bankNumber)) {
        var patches = _bank[bankNumber];
        for (var x = 0; x < patches.Length; x++) {
          if (patches[x] != null && patches[x].Name.Equals(name)) {
            return patches[x];
          }
        }
      }
      return null!;
    }
    public bool IsBankLoaded(int bankNumber) => _bank.ContainsKey(bankNumber);

    //private Patch LoadMyPatch(Stream stream, string patchName, string directory)
    //{
    //    DescriptorList description;
    //    Patch patch;
    //    using (StreamReader reader = new StreamReader(stream))
    //    {
    //        if (!AssertCorrectVersion(ReadNextLine(reader)).Equals("patch"))
    //            throw new FormatException("Invalid patch version. Current version is: v" + string.Format("{0:0.000}", BankVersion) + " Patch ID: " + patchName);
    //        string patchTypeString = ReadNextLine(reader);
    //        Type type;
    //        if (patchTypes.TryGetValue(patchTypeString, out type))
    //            patch = (Patch)Activator.CreateInstance(type, new object[] { patchName });
    //        else
    //            throw new Exception("Invalid patch type \"" + patchTypeString + "\"! Patch ID: " + patchName);
    //        description = new DescriptorList(reader);
    //    }
    //    LoadSampleAssets(patchName, directory, description);
    //    patch.Load(description, assets);
    //    return patch;
    //}
    //private Patch LoadSfzPatch(Stream stream, string patchName, string directory)
    //{
    //    SfzReader sfz = new SfzReader(stream, patchName);
    //    MultiPatch multi = new MultiPatch(patchName);
    //    multi.LoadSfz(sfz.Regions, assets, directory);
    //    return multi;
    //}
    private void LoadMyBank(Stream stream) {
      using var reader = new BinaryReader(stream);
      //read riff chunk
      var id = IOHelper.Read8BitString(reader, 4).ToLower();
      var size = reader.ReadInt32();
      if (!id.Equals("riff")) {
        throw new Exception("Invalid bank file. The riff header is missing.");
      }

      if (!new RiffTypeChunk(id, size, reader).TypeId.ToLower().Equals("bank")) {
        throw new Exception("Invalid bank file. The riff type is incorrect.");
      }
      //read info chunk
      id = IOHelper.Read8BitString(reader, 4).ToLower();
      size = reader.ReadInt32();
      if (!id.Equals("info")) {
        throw new Exception("Invalid bank file. The INFO chunk is missing.");
      }

      if (reader.ReadSingle() != BANK_VERSION) {
        throw new Exception(string.Format("Invalid bank file. The bank version is incorrect, the correct version is {0:0.000}.", BANK_VERSION));
      }

      Comments = IOHelper.Read8BitString(reader, size - 4);
      //read asset list chunk
      id = IOHelper.Read8BitString(reader, 4).ToLower();
      size = reader.ReadInt32();
      if (!id.Equals("list")) {
        throw new Exception("Invalid bank file. The ASET LIST chunk is missing.");
      }

      var readTo = reader.BaseStream.Position + size;
      id = IOHelper.Read8BitString(reader, 4).ToLower();
      if (!id.Equals("aset")) {
        throw new Exception("Invalid bank file. The LIST chunk is not of type ASET.");
      }
      //--read assets
      while (reader.BaseStream.Position < readTo) {
        id = IOHelper.Read8BitString(reader, 4).ToLower();
        size = reader.ReadInt32();
        if (!id.Equals("smpl")) {
          throw new Exception("Invalid bank file. Only SMPL chunks are allowed in the asset list chunk.");
        }

        _assets.SampleAssetList.Add(new SampleDataAsset(size, reader));
      }
      //read instrument list chunk
      id = IOHelper.Read8BitString(reader, 4).ToLower();
      size = reader.ReadInt32();
      if (!id.Equals("list")) {
        throw new Exception("Invalid bank file. The INST LIST chunk is missing.");
      }

      readTo = reader.BaseStream.Position + size;
      id = IOHelper.Read8BitString(reader, 4).ToLower();
      if (!id.Equals("inst")) {
        throw new Exception("Invalid bank file. The LIST chunk is not of type INST.");
      }
      //--read instruments
      while (reader.BaseStream.Position < readTo) {
        id = IOHelper.Read8BitString(reader, 4).ToLower();
        size = reader.ReadInt32();
        if (!id.Equals("ptch")) {
          throw new Exception("Invalid bank file. Only PTCH chunks are allowed in the instrument list chunk.");
        }

        var patchName = IOHelper.Read8BitString(reader, 20);
        var patchType = IOHelper.Read8BitString(reader, 4);
        Patch patch;
        if (_patchTypes.TryGetValue(patchType, out var type)) {
          patch = (Patch)Activator.CreateInstance(type, new object[] { patchName });
        }
        else {
          throw new Exception("Invalid patch type \"" + patchType + "\"! Patch ID: " + patchName);
        }

        patch.Load(new DescriptorList(reader), _assets);
        _assets.PatchAssetList.Add(new PatchAsset(patchName, patch));
        int rangeCount = reader.ReadInt16();
        for (var x = 0; x < rangeCount; x++) {
          int bankNum = reader.ReadInt16();
          int start = reader.ReadByte();
          int end = reader.ReadByte();
          AssignPatchToBank(patch, bankNum, start, end);
        }
      }
    }
    private void LoadSf2(Stream stream) {
      var sf = new SoundFont(stream);
      Name = sf.Info.BankName;
      Comments = sf.Info.Comments;
      //load samples
      for (var x = 0; x < sf.Presets.SampleHeaders.Length; x++) {
        _assets.SampleAssetList.Add(new SampleDataAsset(sf.Presets.SampleHeaders[x], sf.SampleData));
      }
      //create instrument regions first
      var inst = ReadSf2Instruments(sf.Presets.Instruments);
      //load each patch
      foreach (var p in sf.Presets.PresetHeaders) {
        Generator[] globalGens = null!;
        int i;
        if (p.Zones![0].Generators!.Length == 0 || p.Zones![0].Generators![p.Zones[0].Generators!.Length - 1].GeneratorType != GeneratorEnum.Instrument) {
          globalGens = p.Zones![0].Generators!;
          i = 1;
        }
        else {
          i = 0;
        }

        var regionList = new List<Sf2Region>();
        while (i < p.Zones.Length) {
          byte presetLoKey = 0;
          byte presetHiKey = 127;
          byte presetLoVel = 0;
          byte presetHiVel = 127;
          if (p.Zones![i].Generators![0].GeneratorType == GeneratorEnum.KeyRange) {
            if (BitConverter.IsLittleEndian) {
              presetLoKey = (byte)(p.Zones![i].Generators![0].AmountInt16 & 0xFF);
              presetHiKey = (byte)((p.Zones![i].Generators![0].AmountInt16 >> 8) & 0xFF);
            }
            else {
              presetHiKey = (byte)(p.Zones![i].Generators![0].AmountInt16 & 0xFF);
              presetLoKey = (byte)((p.Zones![i].Generators![0].AmountInt16 >> 8) & 0xFF);
            }
            if (p.Zones![i].Generators!.Length > 1 && p.Zones![i].Generators![1].GeneratorType == GeneratorEnum.VelocityRange) {
              if (BitConverter.IsLittleEndian) {
                presetLoVel = (byte)(p.Zones![i].Generators![1].AmountInt16 & 0xFF);
                presetHiVel = (byte)((p.Zones![i].Generators![1].AmountInt16 >> 8) & 0xFF);
              }
              else {
                presetHiVel = (byte)(p.Zones![i].Generators![1].AmountInt16 & 0xFF);
                presetLoVel = (byte)((p.Zones![i].Generators![1].AmountInt16 >> 8) & 0xFF);
              }
            }
          }
          else if (p.Zones![i].Generators![0].GeneratorType == GeneratorEnum.VelocityRange) {
            if (BitConverter.IsLittleEndian) {
              presetLoVel = (byte)(p.Zones![i].Generators![0].AmountInt16 & 0xFF);
              presetHiVel = (byte)((p.Zones![i].Generators![0].AmountInt16 >> 8) & 0xFF);
            }
            else {
              presetHiVel = (byte)(p.Zones![i].Generators![0].AmountInt16 & 0xFF);
              presetLoVel = (byte)((p.Zones![i].Generators![0].AmountInt16 >> 8) & 0xFF);
            }
          }
          if (p.Zones![i].Generators![^1].GeneratorType == GeneratorEnum.Instrument) {
            var insts = inst[p.Zones![i].Generators![^1].AmountInt16];
            for (var x = 0; x < insts.Length; x++) {
              byte instLoKey;
              byte instHiKey;
              byte instLoVel;
              byte instHiVel;
              if (BitConverter.IsLittleEndian) {
                instLoKey = (byte)(insts[x].Generators[(int)GeneratorEnum.KeyRange] & 0xFF);
                instHiKey = (byte)((insts[x].Generators[(int)GeneratorEnum.KeyRange] >> 8) & 0xFF);
                instLoVel = (byte)(insts[x].Generators[(int)GeneratorEnum.VelocityRange] & 0xFF);
                instHiVel = (byte)((insts[x].Generators[(int)GeneratorEnum.VelocityRange] >> 8) & 0xFF);
              }
              else {
                instHiKey = (byte)(insts[x].Generators[(int)GeneratorEnum.KeyRange] & 0xFF);
                instLoKey = (byte)((insts[x].Generators[(int)GeneratorEnum.KeyRange] >> 8) & 0xFF);
                instHiVel = (byte)(insts[x].Generators[(int)GeneratorEnum.VelocityRange] & 0xFF);
                instLoVel = (byte)((insts[x].Generators[(int)GeneratorEnum.VelocityRange] >> 8) & 0xFF);
              }
              if (instLoKey <= presetHiKey && presetLoKey <= instHiKey && instLoVel <= presetHiVel && presetLoVel <= instHiVel) {
                var r = new Sf2Region();
                Array.Copy(insts[x].Generators, r.Generators, r.Generators.Length);
                ReadSf2Region(r, globalGens, p.Zones![i].Generators!, true);
                regionList.Add(r);
              }
            }
          }
          i++;
        }
        var mp = new MultiPatch(p.Name!);
        mp.LoadSf2(regionList.ToArray(), _assets);
        _assets.PatchAssetList.Add(new PatchAsset(mp.Name, mp));
        AssignPatchToBank(mp, p.BankNumber, p.PatchNumber, p.PatchNumber);
      }
    }
    private Sf2Region[][] ReadSf2Instruments(Instrument[] instruments) {
      var regions = new Sf2Region[instruments.Length][];
      for (var x = 0; x < regions.Length; x++) {
        Generator[] globalGens = null!;
        int i;
        if (instruments[x].Zones![0].Generators!.Length == 0 ||
            instruments[x].Zones![0].Generators![instruments![x].Zones![0].Generators!.Length - 1].GeneratorType != GeneratorEnum.SampleID) {
          globalGens = instruments[x].Zones![0].Generators!;
          i = 1;
        }
        else {
          i = 0;
        }

        regions[x] = new Sf2Region[instruments![x].Zones!.Length - i];
        for (var j = 0; j < regions[x].Length; j++) {
          var r = new Sf2Region();
          r.ApplyDefaultValues();
          ReadSf2Region(r, globalGens, instruments![x].Zones![j + i].Generators!, false);
          regions[x][j] = r;
        }
      }
      return regions;
    }
    private void ReadSf2Region(Sf2Region region, Generator[] globals, Generator[] gens, bool isRelative) {
      if (isRelative == false) {
        if (globals != null) {
          for (var x = 0; x < globals.Length; x++) {
            region.Generators[(int)globals[x].GeneratorType] = globals[x].AmountInt16;
          }
        }
        for (var x = 0; x < gens.Length; x++) {
          region.Generators[(int)gens[x].GeneratorType] = gens[x].AmountInt16;
        }
      }
      else {
        var genList = new List<Generator>(gens);
        if (globals != null) {
          for (var x = 0; x < globals.Length; x++) {
            var found = false;
            for (var i = 0; i < genList.Count; i++) {
              if (genList[i].GeneratorType == globals[x].GeneratorType) {
                found = true;
                break;
              }
            }
            if (!found) {
              genList.Add(globals[x]);
            }
          }
        }
        for (var x = 0; x < genList.Count; x++) {
          var value = (int)genList[x].GeneratorType;
          if (value is < 5 or 12 or 45 or 46 or 47 or 50 or 54 or 57 or 58) {
            continue;
          }
          else if (value is 43 or 44) {//calculate intersect
            byte lo_a;
            byte hi_a;
            byte lo_b;
            byte hi_b;
            if (BitConverter.IsLittleEndian) {
              lo_a = (byte)(region.Generators[value] & 0xFF);
              hi_a = (byte)((region.Generators[value] >> 8) & 0xFF);
              lo_b = (byte)(genList[x].AmountInt16 & 0xFF);
              hi_b = (byte)((genList[x].AmountInt16 >> 8) & 0xFF);
            }
            else {
              hi_a = (byte)(region.Generators[value] & 0xFF);
              lo_a = (byte)((region.Generators[value] >> 8) & 0xFF);
              hi_b = (byte)(genList[x].AmountInt16 & 0xFF);
              lo_b = (byte)((genList[x].AmountInt16 >> 8) & 0xFF);
            }
            lo_a = Math.Max(lo_a, lo_b);
            hi_a = Math.Min(hi_a, hi_b);

            if (lo_a > hi_a) {
              throw new Exception("Invalid sf2 region. The range generators do not intersect.");
            }

            if (BitConverter.IsLittleEndian) {
              region.Generators[value] = (short)(lo_a | (hi_a << 8));
            }
            else {
              region.Generators[value] = (short)((lo_a << 8) | hi_a);
            }
          }
          else {
            region.Generators[value] += genList[x].AmountInt16;
          }
        }
      }
    }

    //private void LoadSampleAssets(string patchName, string directory, DescriptorList description)
    //{
    //    for (int x = 0; x < description.GenDescriptions.Length; x++)
    //    {
    //        if (description.GenDescriptions[x].SamplerType == WaveformEnum.SampleData && !description.GenDescriptions[x].AssetName.Equals("null"))
    //        {
    //            assets.LoadSampleAsset(description.GenDescriptions[x].AssetName, patchName, directory);
    //        }
    //    }
    //}
    private void AssignPatchToBank(Patch patch, int bankNumber, int startRange, int endRange) {
      //make sure bank is valid
      if (bankNumber < 0) {
        return;
      }
      //make sure range is valid
      if (startRange > endRange) {
        (endRange, startRange) = (startRange, endRange);
      }
      if (startRange is < 0 or >= BANK_SIZE) {
        throw new ArgumentOutOfRangeException("startRange");
      }

      if (endRange is < 0 or >= BANK_SIZE) {
        throw new ArgumentOutOfRangeException("endRange");
      }
      //create bank if necessary and load assign patches
      Patch[] patches;
      if (_bank.ContainsKey(bankNumber)) {
        patches = _bank[bankNumber];
      }
      else {
        patches = new Patch[BANK_SIZE];
        _bank.Add(bankNumber, patches);
      }
      for (var x = startRange; x <= endRange; x++) {
        patches[x] = patch;
      }
    }

    private static string ReadNextLine(StreamReader reader) {
      while (!reader.EndOfStream) {
        var s = reader.ReadLine();
        var x = s.IndexOf('#');
        if (x >= 0) {
          var y = s.IndexOf('#', x + 1);
          if (y > x) {
            s = s.Remove(x, y - x);
          }
          else {
            s = s.Remove(x, s.Length - x);
          }
        }
        if (!s.Trim().Equals(string.Empty)) {
          return s;
        }
      }
      return string.Empty;
    }
    private static string AssertCorrectVersion(string header) {
      var args = header.Split(new string[] { "v" }, StringSplitOptions.RemoveEmptyEntries);
      if (args.Length != 2 || float.Parse(args[1]) != BANK_VERSION) {
        return string.Empty;
      }

      return args[0].Trim().ToLower();
    }
  }
}
