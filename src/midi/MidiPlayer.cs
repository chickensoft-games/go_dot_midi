using System;
using System.IO;
using System.Runtime.CompilerServices;
using AudioSynthesis.Bank;
using AudioSynthesis.Midi;
using AudioSynthesis.Sequencer;
using AudioSynthesis.Synthesis;
using AudioSynthesis.Wave;
using Godot;

/// <summary>
/// Midi player for MeltySynth, based on
/// MeltySynth's MonoGame example at https://tinyurl.com/54mvev9k.
/// <br/>
/// Special thanks to this Godot forum post for describing the C# api for
/// audio stream playback: https://tinyurl.com/3y2wu99s.
/// <br/>
/// Godot audio stream generation example: https://tinyurl.com/yxrjsnf5
/// <br/>
/// Special thanks to this Godot example for showing how to render PCM audio
/// samples to AudioStreamSample: https://tinyurl.com/wbsm99ds.
/// </summary>
public class MidiPlayer : AudioStreamPlayer {
  // Melty Synth and Godot both use this sample rate, so no need to configure.
  protected const int SAMPLE_RATE = 44100;
  protected const int CHANNELS = 2; // stereo

  [Export(PropertyHint.File, hintString: "Resource path to sound font file")]
  public string SoundFontPath { get; set; } = "";

  [Export(PropertyHint.File, hintString: "Resource path to midi file")]
  public string MidiFilePath { get; set; } = "";

  protected MidiFile _midiFile = null!;
  protected Synthesizer _synthesizer = null!;
  protected MidiFileSequencer _sequencer = null!;
  protected AudioStreamGeneratorPlayback _playback = null!;
  protected bool _started = false;

  protected float[][] _buffer = new float[][] { };
  protected int _bufferHead = 0;

  public override void _Ready() {
    // based on https://github.com/n-yoda/unity-midi/blob/master/Assets/UnityMidi/Scripts/MidiPlayer.cs
    // _soundFont = new SoundFont(ReadFile(SoundFontPath));
    _midiFile = new MidiFile(ReadFile(MidiFilePath));
    _synthesizer = new Synthesizer(SAMPLE_RATE, CHANNELS);
    _sequencer = new MidiFileSequencer(_synthesizer);
    _synthesizer.MixGain = 1.0f;
    _synthesizer.LoadBank(
      ReadFile(SoundFontPath),
      System.IO.Path.GetFileName(SoundFontPath),
      PatchBank.PatchBankType.Sf2
    );
    _sequencer.LoadMidi(_midiFile);

    _playback = (AudioStreamGeneratorPlayback)GetStreamPlayback();

    var a = new int[] { 1 };
    var b = new int[10];
    Array.Copy(a, b, 1);
    GD.Print("b" + b.ToString());
  }

  public override void _Process(float delta) {
    if (!_started) {
      _started = true;
      _sequencer.Play();
      Play();
      GD.Print("Is Playing? " + Playing.ToString());
    }
    if (_started) {
      Buffer();
    }
  }

  public void Buffer() {

    var initFrames = _playback.GetFramesAvailable();
    var deinterleaved = new float[][] { };

    GD.Print("Need to buffer " + initFrames.ToString() + " frames");

    for (var frame = 0; frame < initFrames; frame++) {
      if (_bufferHead >= _buffer.Length) {
        _sequencer.FillMidiEventQueue();
        _synthesizer.GetNext();
        _buffer = WaveHelper.Deinterleave(_synthesizer.WorkingBuffer, CHANNELS);
        _bufferHead = 0;
      }
      var length = Mathf.Min(_buffer[0].Length - _bufferHead, initFrames - frame);
      GD.Print("Buffering " + length.ToString() + " frames");
      for (var i = 0; i < length; i++) {
        _playback.PushFrame(new Vector2(_buffer[0][i], _buffer[1][i]));
      }

      _bufferHead += length;
      initFrames -= 1;
    }
  }

  protected MemoryStream ReadFile(string path) {
    var file = new Godot.File();
    var fileError = file.Open(path, Godot.File.ModeFlags.Read);
    if (fileError != Godot.Error.Ok) {
      throw new FileNotFoundException(
        $"Could not open file {path}",
        path
      );
    }
    var data = file.GetBuffer((long)file.GetLen());
    file.Close();
    return new MemoryStream(data);
  }

  // protected AudioStreamSample RenderMidi() {
  //   // For now, render the entire midi file. Godot doesn't seem to allow
  //   // you to buffer up interleaved, 16 bit stereo PCM audio outside the C++
  //   // layer (GDExtension/GDNative).

  //   // Compute number of samples needed to render entire file based on its
  //   // total seconds.
  //   var numSamples = (int)(SAMPLE_RATE * _midiFile.Length.TotalSeconds);

  //   // The output buffer.
  //   // var stereo = new short[numSamples * 2];

  //   var left = new float[numSamples];
  //   var right = new float[numSamples];

  //   // Render the waveform.
  //   // _sequencer.RenderInterleavedInt16(stereo);
  //   _sequencer.Play(_midiFile, false);
  //   _sequencer.Render(left, right);

  //   var lMax = left.Max(x => Math.Abs(x));
  //   var rMax = right.Max(x => Math.Abs(x));
  //   var a = 0.99F / Math.Max(lMax, rMax);

  //   var dataStream = new MemoryStream();
  //   var writer = new BinaryWriter(dataStream);

  //   var foundNonZero = false;

  //   for (var t = 0; t < left.Length; t++) {
  //     if (left[t] != 0) {
  //       foundNonZero = true;
  //     }
  //     if (right[t] != 0) {
  //       foundNonZero = true;
  //     }
  //     WriteSample(writer, a * left[t]);
  //     WriteSample(writer, a * right[t]);
  //   }


  //   GD.Print("Found non-zero values? " + foundNonZero.ToString());

  //   var binaryData = dataStream.ToArray();

  //   var sample = new AudioStreamSample {
  //     Format = AudioStreamSample.FormatEnum.Format16Bits,
  //     LoopMode = AudioStreamSample.LoopModeEnum.Forward,
  //     MixRate = SAMPLE_RATE,
  //     Stereo = true,
  //     LoopBegin = 0,
  //     LoopEnd = numSamples - 1,
  //     Data = binaryData
  //   };

  //   sample.SaveToWav("test.wav");

  //   return sample;
  // }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected void WriteSample(BinaryWriter writer, float sample)
    => writer.Write((ushort)(ushort.MaxValue * sample));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected void FloatToBytes(float value, out byte first, out byte second) {
    // Credit: https://www.reddit.com/r/godot/comments/o2kazd/comment/h2ak3nu/
    var intValue = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
    first = (byte)(intValue & 0xFF);
    second = (byte)((intValue >> 8) & 0xFF);
  }
}
