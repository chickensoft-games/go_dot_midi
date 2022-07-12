namespace MeltySynth {
  using System;
  using System.IO;
  using System.Runtime.InteropServices;

  internal sealed class SoundFontSampleData {
    internal SoundFontSampleData(BinaryReader reader) {
      var chunkId = reader.ReadFourCC();
      if (chunkId != "LIST") {
        throw new InvalidDataException("The LIST chunk was not found.");
      }

      var end = reader.BaseStream.Position + reader.ReadInt32();

      var listType = reader.ReadFourCC();
      if (listType != "sdta") {
        throw new InvalidDataException($"The type of the LIST chunk must be 'sdta', but was '{listType}'.");
      }

      while (reader.BaseStream.Position < end) {
        var id = reader.ReadFourCC();
        var size = reader.ReadInt32();

        switch (id) {
          case "smpl":
            BitsPerSample = 16;
            Samples = new short[size / 2];
            reader.Read(MemoryMarshal.Cast<short, byte>(Samples));
            break;
          case "sm24":
            // 24 bit audio is not supported.
            reader.BaseStream.Position += size;
            break;
          default:
            throw new InvalidDataException($"The INFO list contains an unknown ID '{id}'.");
        }
      }

      if (Samples == null) {
        throw new InvalidDataException("No valid sample data was found.");
      }

      if (!BitConverter.IsLittleEndian) {
        // TODO: Insert the byte swapping code here.
        throw new NotSupportedException("Big endian architectures are not yet supported.");
      }
    }

    public int BitsPerSample { get; }
    public short[] Samples { get; }
  }
}
