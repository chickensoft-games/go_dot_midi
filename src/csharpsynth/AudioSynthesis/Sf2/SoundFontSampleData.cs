namespace AudioSynthesis.Sf2 {
  using System;
  using System.IO;
  using AudioSynthesis.Util;

  public class SoundFontSampleData {

    //--Properties
    public int BitsPerSample { get; }
    public byte[] SampleData { get; } = null!;
    //--Methods
    public SoundFontSampleData(BinaryReader reader) {
      if (new string(IOHelper.Read8BitChars(reader, 4)).ToLower().Equals("list") == false) {
        throw new Exception("Invalid soundfont. Could not find SDTA LIST chunk.");
      }

      long readTo = reader.ReadInt32();
      readTo += reader.BaseStream.Position;
      if (new string(IOHelper.Read8BitChars(reader, 4)).Equals("sdta") == false) {
        throw new Exception("Invalid soundfont. List is not of type sdta.");
      }

      BitsPerSample = 0;
      byte[] rawSampleData = null!;
      while (reader.BaseStream.Position < readTo) {
        var subID = new string(IOHelper.Read8BitChars(reader, 4));
        var size = reader.ReadInt32();
        switch (subID.ToLower()) {
          case "smpl":
            BitsPerSample = 16;
            rawSampleData = reader.ReadBytes(size);
            break;
          case "sm24":
            if (rawSampleData == null || size != (int)Math.Ceiling(SampleData.Length / 2.0)) {//ignore this chunk if wrong size or if it comes first
              reader.ReadBytes(size);
            }
            else {
              BitsPerSample = 24;
              SampleData = new byte[rawSampleData.Length + size];
              for (int x = 0, i = 0; x < SampleData.Length; x += 3, i += 2) {
                SampleData[x] = reader.ReadByte();
                SampleData[x + 1] = rawSampleData[i];
                SampleData[x + 2] = rawSampleData[i + 1];
              }
            }
            if (size % 2 == 1 && reader.PeekChar() == 0) {
              reader.ReadByte();
            }

            break;
          default:
            throw new Exception("Invalid soundfont. Unknown chunk id: " + subID + ".");
        }
      }
      if (BitsPerSample == 16) {
        SampleData = rawSampleData!;
      }
      else if (BitsPerSample != 24) {
        throw new NotSupportedException("Only 16 and 24 bit samples are supported.");
      }
    }
  }
}
