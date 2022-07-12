namespace MeltySynth {
  internal sealed class Channel {
    private readonly Synthesizer synthesizer;
    private readonly float[] blockLeft;
    private readonly float[] blockRight;
    private short modulation;
    private short volume;
    private short pan;
    private short expression;
    private byte reverbSend;
    private byte chorusSend;

    private short rpn;
    private short pitchBendRange;
    private short coarseTune;
    private short fineTune;

    private float pitchBend;

    internal Channel(Synthesizer synthesizer, bool isPercussionChannel) {
      this.synthesizer = synthesizer;
      this.IsPercussionChannel = isPercussionChannel;

      blockLeft = new float[synthesizer.BlockSize];
      blockRight = new float[synthesizer.BlockSize];

      Reset();
    }

    public void Reset() {
      BankNumber = IsPercussionChannel ? 128 : 0;
      PatchNumber = 0;

      modulation = 0;
      volume = 100 << 7;
      pan = 64 << 7;
      expression = 127 << 7;
      HoldPedal = false;

      reverbSend = 40;
      chorusSend = 0;

      rpn = -1;
      pitchBendRange = 2 << 7;
      coarseTune = 0;
      fineTune = 8192;

      pitchBend = 0F;
    }

    public void ResetAllControllers() {
      modulation = 0;
      expression = 127 << 7;
      HoldPedal = false;

      rpn = -1;

      pitchBend = 0F;
    }

    public void SetBank(int value) {
      BankNumber = value;

      if (IsPercussionChannel) {
        BankNumber += 128;
      }
    }

    public void SetPatch(int value) => PatchNumber = value;

    public void SetModulationCoarse(int value) => modulation = (short)((modulation & 0x7F) | (value << 7));

    public void SetModulationFine(int value) => modulation = (short)((modulation & 0xFF80) | value);

    public void SetVolumeCoarse(int value) => volume = (short)((volume & 0x7F) | (value << 7));

    public void SetVolumeFine(int value) => volume = (short)((volume & 0xFF80) | value);

    public void SetPanCoarse(int value) => pan = (short)((pan & 0x7F) | (value << 7));

    public void SetPanFine(int value) => pan = (short)((pan & 0xFF80) | value);

    public void SetExpressionCoarse(int value) => expression = (short)((expression & 0x7F) | (value << 7));

    public void SetExpressionFine(int value) => expression = (short)((expression & 0xFF80) | value);

    public void SetHoldPedal(int value) => HoldPedal = value >= 64;

    public void SetReverbSend(int value) => reverbSend = (byte)value;

    public void SetChorusSend(int value) => chorusSend = (byte)value;

    public void SetRpnCoarse(int value) => rpn = (short)((rpn & 0x7F) | (value << 7));

    public void SetRpnFine(int value) => rpn = (short)((rpn & 0xFF80) | value);

    public void DataEntryCoarse(int value) {
      switch (rpn) {
        case 0:
          pitchBendRange = (short)((pitchBendRange & 0x7F) | (value << 7));
          break;

        case 1:
          fineTune = (short)((fineTune & 0x7F) | (value << 7));
          break;

        case 2:
          coarseTune = (short)(value - 64);
          break;
        default:
          break;
      }
    }

    public void DataEntryFine(int value) {
      switch (rpn) {
        case 0:
          pitchBendRange = (short)((pitchBendRange & 0xFF80) | value);
          break;

        case 1:
          fineTune = (short)((fineTune & 0xFF80) | value);
          break;
        default:
          break;
      }
    }

    public void SetPitchBend(int value1, int value2) => pitchBend = 1F / 8192F * ((value1 | (value2 << 7)) - 8192);

    public bool IsPercussionChannel { get; }

    public int BankNumber { get; private set; }
    public int PatchNumber { get; private set; }

    public float Modulation => 50F / 16383F * modulation;
    public float Volume => 1F / 16383F * volume;
    public float Pan => (100F / 16383F * pan) - 50F;
    public float Expression => 1F / 16383F * expression;
    public bool HoldPedal { get; private set; }

    public float ReverbSend => 1F / 127F * reverbSend;
    public float ChorusSend => 1F / 127F * chorusSend;

    public float PitchBendRange => (pitchBendRange >> 7) + (0.01F * (pitchBendRange & 0x7F));
    public float Tune => coarseTune + (1F / 8192F * (fineTune - 8192));

    public float PitchBend => PitchBendRange * pitchBend;
  }
}
