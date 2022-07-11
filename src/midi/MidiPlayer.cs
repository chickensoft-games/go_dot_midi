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
/// Midi player for Godot, based on https://github.com/n-yoda/unity-midi/.
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
    var deinterleaved = new float[][] { };

    var bufferLength = _synthesizer.WorkingBufferSize / 2;
    if (_bufferHead >= _buffer.Length) {
      _sequencer.FillMidiEventQueue();
      _synthesizer.GetNext();
      _buffer = WaveHelper.Deinterleave(_synthesizer.WorkingBuffer, CHANNELS);
      _bufferHead = 0;
    }
    var initFrames = _playback.GetFramesAvailable();
    var length = Mathf.Min(bufferLength - _bufferHead, initFrames);
    var frames = new Vector2[bufferLength];
    ConvertToGodotAudioFrames(_buffer, frames);

    _playback.PushBuffer(frames);

    _bufferHead += length;
    initFrames -= 1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected static void ConvertToGodotAudioFrames(
    float[][] samples, Vector2[] buffer
  ) {
    for (var i = 0; i < samples[0].Length; i++) {
      buffer[i] = new Vector2(samples[0][i], samples[1][i]);
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
}
