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
  // audio server returns this when all frames are available
  protected const int MAX_FRAMES_AVAILABLE = ushort.MaxValue;

  // num samples to keep in the buffer at all times.
  protected const int BUFFER_SIZE = (int)(SAMPLE_RATE * 0.5f);

  [Export(PropertyHint.File, "*.sf2")]
  public string SoundFontPath { get; set; } = "";

  [Export(PropertyHint.File, "*.mid")]
  public string MidiFilePath { get; set; } = "";

  protected MidiFile _midiFile = null!;
  protected Synthesizer _synthesizer = null!;
  protected MidiFileSequencer _sequencer = null!;
  protected AudioStreamGeneratorPlayback _playback = null!;
  protected bool _started = false;

  protected float[][] _buffer = new float[][] { new float[] { }, new float[] { } };
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
    var bufferLength = _synthesizer.WorkingBufferSize / 2;

    var framesUsed = MAX_FRAMES_AVAILABLE - _playback.GetFramesAvailable();

    while (framesUsed < BUFFER_SIZE) {
      if (_bufferHead >= _buffer[0].Length) {
        _sequencer.FillMidiEventQueue();
        _synthesizer.GetNext();
        _buffer = WaveHelper.Deinterleave(_synthesizer.WorkingBuffer, CHANNELS);
        _bufferHead = 0;
      }
      var length = Mathf.Min(bufferLength - _bufferHead, BUFFER_SIZE);
      var buffer = new Vector2[length];
      ConvertToGodotAudioFrames(_buffer, _bufferHead, _bufferHead + length, buffer);

      _playback.PushBuffer(buffer);

      _bufferHead += length;
      framesUsed = MAX_FRAMES_AVAILABLE - _playback.GetFramesAvailable();
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected static void ConvertToGodotAudioFrames(
    float[][] samples, int start, int length, Vector2[] buffer
  ) {
    for (var i = start; i < length; i++) {
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
