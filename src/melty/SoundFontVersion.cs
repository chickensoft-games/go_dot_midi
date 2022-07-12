namespace MeltySynth {
  /// <summary>
  /// Reperesents the version of a SoundFont.
  /// </summary>
  public struct SoundFontVersion {
    internal SoundFontVersion(short major, short minor) {
      Major = major;
      Minor = minor;
    }

    /// <summary>
    /// Gets the string representation of the version.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Major}.{Minor}";

    /// <summary>
    /// The major version.
    /// </summary>
    public short Major { get; }

    /// <summary>
    /// The minor version.
    /// </summary>
    public short Minor { get; }
  }
}
