namespace AudioSynthesis.Bank.Descriptors {
  using System.IO;

  public interface IDescriptor {
    void Read(string[] description);
    int Read(BinaryReader reader);
    int Write(BinaryWriter writer);
  }
}
