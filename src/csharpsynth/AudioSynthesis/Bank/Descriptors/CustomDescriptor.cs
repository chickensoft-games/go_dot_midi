namespace AudioSynthesis.Bank.Descriptors {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AudioSynthesis.Util;

  public class CustomDescriptor : IDescriptor {
    //--Properties
    public string ID { get; }
    public int Size { get; private set; }
    public object[] Objects { get; private set; } = null!;
    //--Methods
    public CustomDescriptor(string id, int size, object[] objs) {
      ID = id;
      Size = size;
      Objects = objs;
    }
    public CustomDescriptor(string id, int size) {
      ID = id;
      Size = size;
    }
    public void Read(string[] description) {
      var desc = new Dictionary<string, object>();
      Size = 0;
      for (var x = 0; x < description.Length; x++) {
        var index = description[x].IndexOf('=');
        if (index >= 0 && index < description[x].Length) {
          int sizeInc;
          var paramName = description[x][..index].Trim().ToLower();
          var paramValue = description[x][(index + 1)..].Trim();
          var type = paramValue[^1];
          paramValue = paramValue[..^1];
          object obj = null!;
          switch (type) {
            case 'i':
              obj = int.Parse(paramValue);
              sizeInc = 5;
              break;
            case 's':
              obj = short.Parse(paramValue);
              sizeInc = 3;
              break;
            case 'b':
              obj = byte.Parse(paramValue);
              sizeInc = 2;
              break;
            case 'd':
              obj = double.Parse(paramValue);
              sizeInc = 9;
              break;
            case 'f':
              obj = float.Parse(paramValue);
              sizeInc = 5;
              break;
            case '&':
              obj = paramValue;
              if (paramValue.Length > 255) {
                sizeInc = 2 + 255;
              }
              else {
                sizeInc = 2 + paramValue.Length;
              }

              break;
            default:
              sizeInc = 0;
              break;
          }
          if (obj != null) {
            if (desc.ContainsKey(paramName)) {
              desc[paramName] = obj;
            }
            else {
              Size += sizeInc;
              desc.Add(paramName, obj);
            }
          }
        }
      }
      if (Size % 2 == 1) {
        Size++;
      }

      Objects = new object[desc.Values.Count];
      desc.Values.CopyTo(Objects, 0);
    }
    public int Read(BinaryReader reader) {
      var objList = new List<object>();
      var read = 0;
      while (read < Size) {
        switch ((char)reader.ReadByte()) {
          case '\0':
            read++;
            break;
          case 'i':
            objList.Add(reader.ReadInt32());
            read += 5;
            break;
          case 's':
            objList.Add(reader.ReadInt16());
            read += 3;
            break;
          case 'b':
            objList.Add(reader.ReadByte());
            read += 2;
            break;
          case 'd':
            objList.Add(reader.ReadDouble());
            read += 9;
            break;
          case 'f':
            objList.Add(reader.ReadSingle());
            read += 5;
            break;
          case '&':
            int strLen = reader.ReadByte();
            objList.Add(IOHelper.Read8BitString(reader, strLen));
            read += strLen + 2;
            break;
          default:
            throw new Exception("Invalid custom descriptor: " + ID);
        }
      }
      if (read > Size) {
        throw new Exception("Invalid custom descriptor: " + ID);
      }

      Objects = objList.ToArray();
      return read;
    }
    public int Write(BinaryWriter writer) {
      var written = 0;
      for (var x = 0; x < Objects.Length; x++) {
        if (Objects[x] is int @int) {
          writer.Write((byte)'i');
          writer.Write(@int);
          written += 5;
        }
        else if (Objects[x] is short int1) {
          writer.Write((byte)'s');
          writer.Write(int1);
          written += 3;
        }
        else if (Objects[x] is byte @byte) {
          writer.Write((byte)'b');
          writer.Write(@byte);
          written += 2;
        }
        else if (Objects[x] is double @double) {
          writer.Write((byte)'d');
          writer.Write(@double);
          written += 9;
        }
        else if (Objects[x] is float single) {
          writer.Write((byte)'f');
          writer.Write(single);
          written += 5;
        }
        else if (Objects[x] is string @string) {
          writer.Write((byte)'&');
          var s = @string;
          writer.Write((byte)s.Length);
          IOHelper.Write8BitString(writer, s, s.Length);
          written += s.Length + 2;
        }
      }
      if (written < Size) {
        do {
          writer.Write((byte)0);
          written++;
        } while (written < Size);
      }
      else if (written > Size) {
        throw new Exception("More bytes were written than expected.");
      }
      return written;
    }
  }
}
