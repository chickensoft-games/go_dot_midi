namespace MeltySynth {
  using System.IO;

  /// <summary>
  /// Represents a sample in the SoundFont.
  /// </summary>
  public sealed class SampleHeader {
    internal static readonly SampleHeader Default = new();
    private readonly ushort link;
    private readonly SampleType type;

    private SampleHeader() => Name = "Default";

    private SampleHeader(BinaryReader reader) {
      Name = reader.ReadFixedLengthString(20);
      Start = reader.ReadInt32();
      End = reader.ReadInt32();
      StartLoop = reader.ReadInt32();
      EndLoop = reader.ReadInt32();
      SampleRate = reader.ReadInt32();
      OriginalPitch = reader.ReadByte();
      PitchCorrection = reader.ReadSByte();
      link = reader.ReadUInt16();
      type = (SampleType)reader.ReadUInt16();
    }

    internal static SampleHeader[] ReadFromChunk(BinaryReader reader, int size) {
      if (size % 46 != 0) {
        throw new InvalidDataException("The sample header list is invalid.");
      }

      var headers = new SampleHeader[(size / 46) - 1];

      for (var i = 0; i < headers.Length; i++) {
        headers[i] = new SampleHeader(reader);
      }

      // The last one is the terminator.
      new SampleHeader(reader);

      return headers;
    }

    /// <summary>
    /// Gets the name of the sample.
    /// </summary>
    /// <returns>
    /// The name of the sample.
    /// </returns>
    public override string ToString() => Name;

    /// <summary>
    /// The name of the sample.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The start point of the sample in the sample data.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// The end point of the sample in the sample data.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// The loop start point of the sample in the sample data.
    /// </summary>
    public int StartLoop { get; }

    /// <summary>
    /// The loop end point of the sample in the sample data.
    /// </summary>
    public int EndLoop { get; }

    /// <summary>
    /// The sample rate of the sample.
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// The key number of the recorded pitch of the sample.
    /// </summary>
    public byte OriginalPitch { get; }

    /// <summary>
    /// The pitch correction in cents that should be applied to the sample on playback.
    /// </summary>
    public sbyte PitchCorrection { get; }
  }
}
