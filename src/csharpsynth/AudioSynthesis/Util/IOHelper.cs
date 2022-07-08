namespace AudioSynthesis.Util {
  using System;
  using System.IO;
  using System.Text;

  /// <summary>
  /// Provides methods for reading ascii strings and parsing directory strings.
  /// </summary>
  public static class IOHelper {
    // String type helpers
    public static char Read8BitChar(BinaryReader reader) => (char)reader.ReadByte();
    public static char[] Read8BitChars(BinaryReader reader, int length) {
      var chars = new char[length];
      for (var x = 0; x < chars.Length; x++) {
        chars[x] = (char)reader.ReadByte();
      }

      return chars;
    }
    public static string Read8BitString(BinaryReader reader) {
      var sbuild = new StringBuilder();
      var c = (char)reader.ReadByte();
      while (c != '\0') {
        sbuild.Append(c);
        c = (char)reader.ReadByte();
      }
      return sbuild.ToString();
    }
    public static string Read8BitString(BinaryReader reader, int length) {
      var chars = new char[length];
      for (var x = 0; x < chars.Length; x++) {
        chars[x] = (char)reader.ReadByte();
      }

      var s = new string(chars);
      var i = s.IndexOf('\0');
      if (i >= 0) {
        return s.Remove(i);
      }

      return s;
    }
    public static void Write8BitString(BinaryWriter writer, string str, int length) {
      int x;
      var end = Math.Min(str.Length, length);
      for (x = 0; x < end; x++) {
        writer.Write((byte)str[x]);
      }

      x = length - str.Length;
      while (x > 0) {
        writer.Write((byte)'\0');
        x--;
      }
    }
    // Path namespace emulated below
    public static string GetExtension(string fileName) {
      for (var x = fileName.Length - 1; x >= 0; x--) {
        if (fileName[x] == '.') {
          return fileName[x..];
        }
        else if (fileName[x] is '/' or '\\') {
          break;
        }
      }
      return string.Empty;
    }
    public static string GetFileNameWithExtension(string fileName) {
      for (var x = fileName.Length - 1; x >= 0; x--) {
        if (fileName[x] is '/' or '\\') {
          return fileName[(x + 1)..];
        }
      }
      return fileName;
    }
    public static string GetFileNameWithoutExtension(string fileName) {
      fileName = GetFileNameWithExtension(fileName);
      for (var x = fileName.Length - 1; x >= 0; x--) {
        if (fileName[x] == '.') {
          return fileName[..x];
        }
      }
      return fileName;
    }
  }
  /// <summary>
  /// Provides support for reading and writing data in little endian format.
  /// </summary>
  public static class LittleEndianHelper {
    // Integer type helpers
    public static short ReadInt16(byte[] input, int index) => (short)(input[index] | (input[index + 1] << 8));
    public static short ReadInt16(BinaryReader reader) => (short)(reader.ReadByte() | (reader.ReadByte() << 8));
    public static int ReadInt24(byte[] input, int index) => ((input[index] | (input[index + 1] << 8) | (input[index + 2] << 16)) << 12) >> 12;
    public static int ReadInt24(BinaryReader reader) => ((reader.ReadByte() | (reader.ReadByte() << 8) | (reader.ReadByte() << 16)) << 12) >> 12;
    public static int ReadInt32(byte[] input, int index) => input[index] | (input[index + 1] << 8) | (input[index + 2] << 16) | (input[index + 3] << 24);
    public static int ReadInt32(BinaryReader reader) => reader.ReadByte() | (reader.ReadByte() << 8) | (reader.ReadByte() << 16) | (reader.ReadByte() << 24);

    public static void WriteInt16(short value, byte[] output, int index) {
      var uvalue = (uint)value;
      output[index] = (byte)(uvalue & 0xFF);
      output[index + 1] = (byte)(uvalue >> 8);
    }
    public static void WriteInt16(short value, BinaryWriter writer) {
      var uvalue = (uint)value;
      writer.Write((byte)(uvalue & 0xFF));
      writer.Write((byte)(uvalue >> 8));
    }
    public static void WriteInt24(int value, byte[] output, int index) {
      var uvalue = (uint)value;
      output[index] = (byte)(uvalue & 0xFF);
      output[index + 1] = (byte)((uvalue >> 8) & 0xFF);
      output[index + 2] = (byte)(uvalue >> 16);
    }
    public static void WriteInt24(int value, BinaryWriter writer) {
      var uvalue = (uint)value;
      writer.Write((byte)(uvalue & 0xFF));
      writer.Write((byte)((uvalue >> 8) & 0xFF));
      writer.Write((byte)(uvalue >> 16));
    }
    public static void WriteInt32(int value, byte[] output, int index) {
      var uvalue = (uint)value;
      output[index] = (byte)(uvalue & 0xFF);
      output[index + 1] = (byte)((uvalue >> 8) & 0xFF);
      output[index + 2] = (byte)((uvalue >> 16) & 0xFF);
      output[index + 3] = (byte)(uvalue >> 24);
    }
    public static void WriteInt32(int value, BinaryWriter writer) {
      var uvalue = (uint)value;
      writer.Write((byte)(uvalue & 0xFF));
      writer.Write((byte)((uvalue >> 8) & 0xFF));
      writer.Write((byte)((uvalue >> 16) & 0xFF));
      writer.Write((byte)(uvalue >> 24));
    }
  }
  /// <summary>
  /// Provides support for reading and writing data in big endian format.
  /// </summary>
  public static class BigEndianHelper {
    // Integer type helpers
    public static short ReadInt16(byte[] input, int index) => (short)((input[index] << 8) | input[index + 1]);
    public static short ReadInt16(BinaryReader reader) => (short)((reader.ReadByte() << 8) | reader.ReadByte());
    public static int ReadInt24(byte[] input, int index) => (((input[index] << 16) | (input[index + 1] << 8) | input[index + 2]) << 12) >> 12;
    public static int ReadInt24(BinaryReader reader) => (((reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte()) << 12) >> 12;
    public static int ReadInt32(byte[] input, int index) => (input[index] << 24) | (input[index + 1] << 16) | (input[index + 2] << 8) | input[index + 3];
    public static int ReadInt32(BinaryReader reader) => (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();

    public static void WriteInt16(short value, byte[] output, int index) {
      var uvalue = (uint)value;
      output[index] = (byte)(uvalue >> 8);
      output[index + 1] = (byte)(uvalue & 0xFF);
    }
    public static void WriteInt16(short value, BinaryWriter writer) {
      var uvalue = (uint)value;
      writer.Write((byte)(uvalue >> 8));
      writer.Write((byte)(uvalue & 0xFF));
    }
    public static void WriteInt24(int value, byte[] output, int index) {
      var uvalue = (uint)value;
      output[index] = (byte)(uvalue >> 16);
      output[index + 1] = (byte)((uvalue >> 8) & 0xFF);
      output[index + 2] = (byte)(uvalue & 0xFF);
    }
    public static void WriteInt24(int value, BinaryWriter writer) {
      var uvalue = (uint)value;
      writer.Write((byte)(uvalue >> 16));
      writer.Write((byte)((uvalue >> 8) & 0xFF));
      writer.Write((byte)(uvalue & 0xFF));
    }
    public static void WriteInt32(int value, byte[] output, int index) {
      var uvalue = (uint)value;
      output[index] = (byte)(uvalue >> 24);
      output[index + 1] = (byte)((uvalue >> 16) & 0xFF);
      output[index + 2] = (byte)((uvalue >> 8) & 0xFF);
      output[index + 3] = (byte)(uvalue & 0xFF);
    }
    public static void WriteInt32(int value, BinaryWriter writer) {
      var uvalue = (uint)value;
      writer.Write((byte)(uvalue >> 24));
      writer.Write((byte)((uvalue >> 16) & 0xFF));
      writer.Write((byte)((uvalue >> 8) & 0xFF));
      writer.Write((byte)(uvalue & 0xFF));
    }
  }
}
