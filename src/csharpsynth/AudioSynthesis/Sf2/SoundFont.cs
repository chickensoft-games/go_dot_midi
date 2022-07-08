namespace AudioSynthesis.Sf2 {
  using System;
  using System.IO;
  using AudioSynthesis.Util;

  public class SoundFont {

    //--Properties
    public SoundFontInfo Info { get; private set; } = null!;
    public SoundFontSampleData SampleData { get; private set; } = null!;
    public SoundFontPresets Presets { get; private set; } = null!;


    //--Methods
    public SoundFont(IResource soundFont) {
      if (!soundFont.ReadAllowed()) {
        throw new Exception("A soundFont resource must have read access.");
      }

      Load(soundFont.OpenResourceForRead());
    }
    public SoundFont(Stream stream) => Load(stream);

    private void Load(Stream stream) {
      using var reader = new BinaryReader(stream);
      var id = new string(IOHelper.Read8BitChars(reader, 4));
      var size = reader.ReadInt32();
      if (!id.ToLower().Equals("riff")) {
        throw new Exception("Invalid soundfont. Could not find RIFF header.");
      }

      id = new string(IOHelper.Read8BitChars(reader, 4));
      if (!id.ToLower().Equals("sfbk")) {
        throw new Exception("Invalid soundfont. Riff type is invalid.");
      }

      Info = new SoundFontInfo(reader);
      SampleData = new SoundFontSampleData(reader);
      Presets = new SoundFontPresets(reader);
    }
  }
}
