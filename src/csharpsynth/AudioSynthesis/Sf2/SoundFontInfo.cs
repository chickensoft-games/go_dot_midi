namespace AudioSynthesis.Sf2 {
  using System;
  using System.IO;
  using AudioSynthesis.Util;

  public class SoundFontInfo {

    //--Properties
    public short ROMVersionMajor { get; }
    public short ROMVersionMinor { get; }
    public short SFVersionMajor { get; }
    public short SFVersionMinor { get; }
    public string SoundEngine { get; } = string.Empty;
    public string BankName { get; } = string.Empty;
    public string DataROM { get; } = string.Empty;
    public string CreationDate { get; } = string.Empty;
    public string Author { get; } = string.Empty;
    public string TargetProduct { get; } = string.Empty;
    public string Copyright { get; } = string.Empty;
    public string Comments { get; } = string.Empty;
    public string Tools { get; } = string.Empty;

    //--Methods
    public SoundFontInfo(BinaryReader reader) {
      var id = new string(IOHelper.Read8BitChars(reader, 4));
      var size = reader.ReadInt32();
      if (!id.ToLower().Equals("list")) {
        throw new Exception("Invalid soundfont. Could not find INFO LIST chunk.");
      }

      var readTo = reader.BaseStream.Position + size;
      id = new string(IOHelper.Read8BitChars(reader, 4));
      if (!id.ToLower().Equals("info")) {
        throw new Exception("Invalid soundfont. The LIST chunk is not of type INFO.");
      }

      while (reader.BaseStream.Position < readTo) {
        id = new string(IOHelper.Read8BitChars(reader, 4));
        size = reader.ReadInt32();
        switch (id.ToLower()) {
          case "ifil":
            SFVersionMajor = reader.ReadInt16();
            SFVersionMinor = reader.ReadInt16();
            break;
          case "isng":
            SoundEngine = IOHelper.Read8BitString(reader, size);
            break;
          case "inam":
            BankName = IOHelper.Read8BitString(reader, size);
            break;
          case "irom":
            DataROM = IOHelper.Read8BitString(reader, size);
            break;
          case "iver":
            ROMVersionMajor = reader.ReadInt16();
            ROMVersionMinor = reader.ReadInt16();
            break;
          case "icrd":
            CreationDate = IOHelper.Read8BitString(reader, size);
            break;
          case "ieng":
            Author = IOHelper.Read8BitString(reader, size);
            break;
          case "iprd":
            TargetProduct = IOHelper.Read8BitString(reader, size);
            break;
          case "icop":
            Copyright = IOHelper.Read8BitString(reader, size);
            break;
          case "icmt":
            Comments = IOHelper.Read8BitString(reader, size);
            break;
          case "isft":
            Tools = IOHelper.Read8BitString(reader, size);
            break;
          default:
            throw new Exception("Invalid soundfont. The Chunk: " + id + " was not expected.");
        }
      }
    }
    public override string ToString() => "Bank Name: " + BankName + "\nAuthor: " + Author + "\n\nComments:\n" + Comments;
  }
}
