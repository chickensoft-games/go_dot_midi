namespace AudioSynthesis.Synthesis {
  using System;

  /// <summary>
  /// Parameters for a single synth channel including its program, bank, and cc list.
  /// </summary>
  public class SynthParameters {
    public byte Program; //program number
    public byte BankSelect; //bank number
    public byte ChannelAfterTouch; //channel pressure event
    public CCValue Pan; //(vol) pan positions controlling both right and left output levels
    public CCValue Volume; //(vol) channel volume controller
    public CCValue Expression; //(vol) expression controller
    public CCValue ModRange; //(pitch) mod wheel pitch modifier in partial cents ie. 22.3
    public CCValue PitchBend; //(pitch) pitch bend including both semitones and cents
    public byte PitchBendRangeCoarse; //controls max and min pitch bend range semitones
    public byte PitchBendRangeFine; //controls max and min pitch bend range cents
    public short MasterCoarseTune; //(pitch) transposition in semitones
    public CCValue MasterFineTune; //(pitch) transposition in cents
    public bool HoldPedal; //hold pedal status (true) for active
    public bool LegatoPedal; //legato pedal status (true) for active
    public CCValue Rpn; //registered parameter number
    internal Synthesizer Synth;

    //These are updated whenever a midi event that affects them is received.
    public float CurrentVolume;
    public int CurrentPitch;    //in cents
    public int CurrentMod;      //in cents
    public PanComponent CurrentPan;


    public SynthParameters(Synthesizer synth) {
      Synth = synth;
      ResetControllers();
    }
    /// <summary>
    /// Resets all of the channel's controllers to initial first power on values. Not the same as CC-121.
    /// </summary>
    public void ResetControllers() {
      Program = 0;
      BankSelect = 0;
      ChannelAfterTouch = 0; //Reset Channel Pressure to 0
      Pan.Combined = 0x2000;
      Volume.Fine = 0;
      Volume.Coarse = 100; //Reset Vol Positions back to 90/127 (GM spec)
      Expression.Combined = 0x3FFF; //Reset Expression positions back to 127/127
      ModRange.Combined = 0;
      PitchBend.Combined = 0x2000;
      PitchBendRangeCoarse = 2; //Reset pitch wheel to +-2 semitones (GM spec)
      PitchBendRangeFine = 0;
      MasterCoarseTune = 0;
      MasterFineTune.Combined = 0x2000; //Reset fine tune
      HoldPedal = false;
      LegatoPedal = false;
      Rpn.Combined = 0x3FFF; //Reset rpn
      UpdateCurrentPan();
      UpdateCurrentPitch();
      UpdateCurrentVolume();
    }

    internal void UpdateCurrentVolume() {
      CurrentVolume = Expression.Combined / 16383f;
      CurrentVolume *= CurrentVolume;
    }
    internal void UpdateCurrentPitch() => CurrentPitch = (int)((PitchBend.Combined - 8192.0) / 8192.0 * ((100 * PitchBendRangeCoarse) + PitchBendRangeFine));
    internal void UpdateCurrentMod() => CurrentMod = (int)(Synthesizer.DEFAULT_MOD_DEPTH * (ModRange.Combined / 16383.0));
    internal void UpdateCurrentPan() {
      var value = Synthesizer.HALF_PI * (Pan.Combined / 16383.0);
      CurrentPan.Left = (float)Math.Cos(value);
      CurrentPan.Right = (float)Math.Sin(value);
    }
  }
}
