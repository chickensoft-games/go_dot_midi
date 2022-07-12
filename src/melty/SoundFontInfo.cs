namespace MeltySynth {
  using System.IO;

  /// <summary>
  /// The information of a SoundFont.
  /// </summary>
  public sealed class SoundFontInfo {
    internal SoundFontInfo(BinaryReader reader) {
      var chunkId = reader.ReadFourCC();
      if (chunkId != "LIST") {
        throw new InvalidDataException("The LIST chunk was not found.");
      }

      var end = reader.BaseStream.Position + reader.ReadInt32();

      var listType = reader.ReadFourCC();
      if (listType != "INFO") {
        throw new InvalidDataException($"The type of the LIST chunk must be 'INFO', but was '{listType}'.");
      }

      while (reader.BaseStream.Position < end) {
        var id = reader.ReadFourCC();
        var size = reader.ReadInt32();

        switch (id) {
          case "ifil":
            Version = new SoundFontVersion(reader.ReadInt16(), reader.ReadInt16());
            break;
          case "isng":
            TargetSoundEngine = reader.ReadFixedLengthString(size);
            break;
          case "INAM":
            BankName = reader.ReadFixedLengthString(size);
            break;
          case "irom":
            RomName = reader.ReadFixedLengthString(size);
            break;
          case "iver":
            RomVersion = new SoundFontVersion(reader.ReadInt16(), reader.ReadInt16());
            break;
          case "ICRD":
            CeationDate = reader.ReadFixedLengthString(size);
            break;
          case "IENG":
            Author = reader.ReadFixedLengthString(size);
            break;
          case "IPRD":
            TargetProduct = reader.ReadFixedLengthString(size);
            break;
          case "ICOP":
            Copyright = reader.ReadFixedLengthString(size);
            break;
          case "ICMT":
            Comments = reader.ReadFixedLengthString(size);
            break;
          case "ISFT":
            Tools = reader.ReadFixedLengthString(size);
            break;
          default:
            throw new InvalidDataException($"The INFO list contains an unknown ID '{id}'.");
        }
      }
    }

    /// <summary>
    /// Gets the name of the SoundFont.
    /// </summary>
    /// <returns>
    /// The name of the SoundFont.
    /// </returns>
    public override string ToString() => BankName;

    /// <summary>
    /// The version of the SoundFont.
    /// </summary>
    public SoundFontVersion Version { get; } = default;

    /// <summary>
    /// The target sound engine of the SoundFont.
    /// </summary>
    public string TargetSoundEngine { get; } = string.Empty;

    /// <summary>
    /// The bank name of the SoundFont.
    /// </summary>
    public string BankName { get; } = string.Empty;

    /// <summary>
    /// The ROM name of the SoundFont.
    /// </summary>
    public string RomName { get; } = string.Empty;

    /// <summary>
    /// The ROM version of the SoundFont.
    /// </summary>
    public SoundFontVersion RomVersion { get; } = default;

    /// <summary>
    /// The creation date of the SoundFont.
    /// </summary>
    public string CeationDate { get; } = string.Empty;

    /// <summary>
    /// The auther of the SoundFont.
    /// </summary>
    public string Author { get; } = string.Empty;

    /// <summary>
    /// The target product of the SoundFont.
    /// </summary>
    public string TargetProduct { get; } = string.Empty;

    /// <summary>
    /// The copyright message for the SoundFont.
    /// </summary>
    public string Copyright { get; } = string.Empty;

    /// <summary>
    /// The comments for the SoundFont.
    /// </summary>
    public string Comments { get; } = string.Empty;

    /// <summary>
    /// The tools used to create the SoundFont.
    /// </summary>
    public string Tools { get; } = string.Empty;
  }
}
