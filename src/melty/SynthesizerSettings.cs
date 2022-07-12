﻿namespace MeltySynth {
  using System;

  /// <summary>
  /// Specifies a set of parameters for synthesis.
  /// </summary>
  public sealed class SynthesizerSettings {
    internal static int DefaultBlockSize = 64;
    internal static int DefaultMaximumPolyphony = 64;
    internal static bool DefaultEnableReverbAndChorus = true;

    private int sampleRate;
    private int blockSize;
    private int maximumPolyphony;

    /// <summary>
    /// Initializes a new instance of synthesizer settings.
    /// </summary>
    /// <param name="sampleRate">The sample rate for synthesis.</param>
    public SynthesizerSettings(int sampleRate) {
      CheckSampleRate(sampleRate);

      this.sampleRate = sampleRate;
      blockSize = DefaultBlockSize;
      maximumPolyphony = DefaultMaximumPolyphony;
      EnableReverbAndChorus = DefaultEnableReverbAndChorus;
    }

    private static void CheckSampleRate(int value) {
      if (value is not (>= 16000 and <= 192000)) {
        throw new ArgumentOutOfRangeException("The sample rate must be between 16000 and 192000.");
      }
    }

    private static void CheckBlockSize(int value) {
      if (value is not (>= 8 and <= 1024)) {
        throw new ArgumentOutOfRangeException("The block size must be between 8 and 1024.");
      }
    }

    private static void CheckMaximumPolyphony(int value) {
      if (value is not (>= 8 and <= 256)) {
        throw new ArgumentOutOfRangeException("The maximum number of polyphony must be between 8 and 256.");
      }
    }

    /// <summary>
    /// Gets or sets the sample rate for synthesis.
    /// </summary>
    public int SampleRate {
      get => sampleRate;

      set {
        CheckSampleRate(value);
        sampleRate = value;
      }
    }

    /// <summary>
    /// Gets or sets the block size of waveform rendering.
    /// </summary>
    public int BlockSize {
      get => blockSize;

      set {
        CheckBlockSize(value);
        blockSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the number of maximum polyphony.
    /// </summary>
    public int MaximumPolyphony {
      get => maximumPolyphony;

      set {
        CheckMaximumPolyphony(value);
        maximumPolyphony = value;
      }
    }

    /// <summary>
    /// Gets or sets whether reverb and chorus are enabled.
    /// </summary>
    public bool EnableReverbAndChorus { get; set; }
  }
}