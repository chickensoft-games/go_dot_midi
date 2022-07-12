using System;
using System.IO;
using System.Runtime.CompilerServices;
using Godot;
using MeltySynth;

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

  [Export(PropertyHint.File, hintString: "Resource path to sound font file")]
  public string SoundFontPath { get; set; } = "";

  [Export(PropertyHint.File, hintString: "Resource path to midi file")]
  public string MidiFilePath { get; set; } = "";

  protected MidiFile _midiFile = null!;
  protected SoundFont _soundFont = null!;
  protected Synthesizer _synthesizer = null!;
  protected MidiFileSequencer _sequencer = null!;
  protected AudioStreamGeneratorPlayback _playback = null!;
  protected bool _started = false;

  protected int _bufferHead = 0;

  protected float[] _left = null!;
  protected float[] _right = null!;

  public override void _Ready() {
    _soundFont = new SoundFont(ReadFile(SoundFontPath));
    _midiFile = new MidiFile(ReadFile(MidiFilePath));
    _synthesizer = new Synthesizer(_soundFont, new SynthesizerSettings(SAMPLE_RATE) {
      BlockSize = 128,
      MaximumPolyphony = 256,
      EnableReverbAndChorus = true
    });
    _sequencer = new MidiFileSequencer(_synthesizer);
    _synthesizer.MasterVolume = 1.0f;

    _playback = (AudioStreamGeneratorPlayback)GetStreamPlayback();

    _left = new float[(int)(SAMPLE_RATE * _midiFile.Length.TotalSeconds)];
    _right = new float[(int)(SAMPLE_RATE * _midiFile.Length.TotalSeconds)];

    var a = new int[] { 1 };
    var b = new int[10];
    Array.Copy(a, b, 1);
    GD.Print("b" + b.ToString());
  }

  public override void _Process(float delta) {
    if (!_started) {
      _started = true;
      _sequencer.Play(_midiFile, true);
      _sequencer.Render(_left, _right);
      Play();
      GD.Print("Is Playing? " + Playing.ToString());
    }
    if (_started) {
      Buffer();
    }
  }

  public void Buffer() {
    var bufferLength = _left.Length;

    var framesUsed = MAX_FRAMES_AVAILABLE - _playback.GetFramesAvailable();
    while (framesUsed < BUFFER_SIZE) {
      if (_bufferHead >= _left.Length) {
        _bufferHead = 0;
      }
      var length = Mathf.Min(bufferLength - _bufferHead, BUFFER_SIZE);
      var buffer = new Vector2[length];
      ConvertToGodotAudioFrames(_left, _right, _bufferHead, length, buffer);

      _playback.PushBuffer(buffer);

      _bufferHead += length;
      framesUsed = MAX_FRAMES_AVAILABLE - _playback.GetFramesAvailable();
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected static void ConvertToGodotAudioFrames(
    float[] left, float[] right, int start, int length, Vector2[] buffer
  ) {
    var n = 0;
    for (var i = start; i < start + length; i++, n++) {
      buffer[n] = new Vector2(left[i], right[i]);
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
