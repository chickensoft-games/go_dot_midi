/*
 *    ______   __ __     _____             __  __
 *   / ____/__/ // /_   / ___/__  ______  / /_/ /_
 *  / /    /_  _  __/   \__ \/ / / / __ \/ __/ __ \
 * / /___ /_  _  __/   ___/ / /_/ / / / / /_/ / / /
 * \____/  /_//_/     /____/\__, /_/ /_/\__/_/ /_/
 *                         /____/
 * Synthesizer
 *  A synth class that follows the GM spec (for the most part). Use a sequencer to take advantage of easy midi playback, but
 *  the synth can also be used with and external sequencer. See Synthesizer.MidiControl.cs for information about which midi
 *  events are supported.
 */

namespace AudioSynthesis.Synthesis {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using AudioSynthesis.Bank;
  using AudioSynthesis.Bank.Patches;
  using AudioSynthesis.Midi;
  public partial class Synthesizer {
    #region Fields
    //synth variables
    internal float[] SampleBuffer;
    private readonly VoiceManager _voiceManager;
    private bool _littleEndian;
    private float _mainVolume = 1.0f;
    private float _synthGain = .35f;
    private readonly SynthParameters[] _synthChannels;
    #endregion
    #region Properties
    /// <summary>
    /// Controls the method used when stealing voices.
    /// </summary>
    public VoiceStealEnum VoiceStealMethod {
      get => _voiceManager.StealingMethod;
      set => _voiceManager.StealingMethod = value;
    }
    /// <summary>
    /// The number of voices in use.
    /// </summary>
    public int ActiveVoices => _voiceManager.ActiveVoices.Count;
    /// <summary>
    /// The number of voices that are not being used.
    /// </summary>
    public int FreeVoices => _voiceManager.FreeVoices.Count;
    /// <summary>
    /// The size of the individual sub buffers in samples
    /// </summary>
    public int MicroBufferSize { get; }
    /// <summary>
    /// The number of sub buffers
    /// </summary>
    public int MicroBufferCount { get; }
    /// <summary>
    /// The size of the entire buffer in bytes
    /// </summary>
    public int RawBufferSize => SampleBuffer.Length * 2;
    /// <summary>
    /// The size of the entire buffer in samples
    /// </summary>
    public int WorkingBufferSize => SampleBuffer.Length;
    /// <summary>
    /// A buffer which will be filled on GetNext.
    /// </summary>
    public float[] WorkingBuffer => SampleBuffer;
    /// <summary>
    /// The number of voices
    /// </summary>
    public int Polyphony => _voiceManager.Polyphony;
    /// <summary>
    /// Global volume control
    /// </summary>
    public float MasterVolume {
      get => _mainVolume;
      set => _mainVolume = SynthHelper.Clamp(value, 0.0f, 3.0f);
    }
    /// <summary>
    /// The mix volume for each voice
    /// </summary>
    public float MixGain {
      get => _synthGain;
      set => _synthGain = SynthHelper.Clamp(value, .05f, 1f);
    }
    /// <summary>
    /// The number of samples per second produced per channel
    /// </summary>
    public int SampleRate { get; }
    /// <summary>
    /// The number of audio channels
    /// </summary>
    public int AudioChannels { get; private set; }
    /// <summary>
    /// The patch bank that holds all of the currently loaded instrument patches
    /// </summary>
    public PatchBank SoundBank { get; private set; } = null!;
    #endregion
    #region Methods
    public Synthesizer(int sampleRate, int audioChannels)
        : this(sampleRate, audioChannels, (int)(.01 * sampleRate), 3, DEFAULT_POLYPHONY) { }
    public Synthesizer(int sampleRate, int audioChannels, int bufferSize, int bufferCount)
        : this(sampleRate, audioChannels, bufferSize, bufferCount, DEFAULT_POLYPHONY) { }
    public Synthesizer(int sampleRate, int audioChannels, int bufferSize, int bufferCount, int polyphony) {
      const int MIN_SAMPLE_RATE = 8000;
      const int MAX_SAMPLE_RATE = 96000;
      //Setup synth parameters
      if (sampleRate is < MIN_SAMPLE_RATE or > MAX_SAMPLE_RATE) {
        throw new ArgumentException("Invalid parameter: (sampleRate) Valid ranges are " + MIN_SAMPLE_RATE + " to " + MAX_SAMPLE_RATE, "sampleRate");
      }

      SampleRate = sampleRate;
      if (audioChannels is < 1 or > 2) {
        throw new ArgumentException("Invalid parameter: (audioChannels) Valid ranges are " + 1 + " to " + 2, "audioChannels");
      }

      AudioChannels = audioChannels;
      MicroBufferSize = SynthHelper.Clamp(bufferSize, (int)(MIN_BUFFER_SIZE * sampleRate), (int)(MAX_BUFFER_SIZE * sampleRate));
      MicroBufferSize = (int)Math.Ceiling(MicroBufferSize / (double)DEFAULT_BLOCK_SIZE) * DEFAULT_BLOCK_SIZE; //ensure multiple of block size
      MicroBufferCount = Math.Max(1, bufferCount);
      SampleBuffer = new float[MicroBufferSize * MicroBufferCount * audioChannels];
      _littleEndian = true;
      //Setup Controllers
      _synthChannels = new SynthParameters[DEFAULT_CHANNEL_COUNT];
      for (var x = 0; x < _synthChannels.Length; x++) {
        _synthChannels[x] = new SynthParameters(this);
      }
      //Create synth voices
      _voiceManager = new VoiceManager(SynthHelper.Clamp(polyphony, MIN_POLYPHONY, MAX_POLYPHONY));
      //Create midi containers
      MidiEventQueue = new Queue<MidiMessage>();
      MidiEventCounts = new int[MicroBufferCount];
      _layerList = new Patch[15];
    }
    public bool IsLittleEndian() => _littleEndian;
    public void SetEndianMode(bool isLittleEndian) => _littleEndian = isLittleEndian;
    public void LoadBank(Stream bankFile, string name, PatchBank.PatchBankType type) => LoadBank(new PatchBank(bankFile, name, type));
    public void LoadBank(PatchBank bank) {
      UnloadBank();
      SoundBank = bank ?? throw new ArgumentNullException("The parameter bank was null.");
    }
    public void UnloadBank() {
      if (SoundBank != null) {
        NoteOffAll(true);
        _voiceManager.UnloadPatches();
        SoundBank = null!;
      }
    }
    public void ResetSynthControls() {
      for (var x = 0; x < _synthChannels.Length; x++) {
        _synthChannels[x].ResetControllers();
      }
      _synthChannels[MidiHelper.DRUM_CHANNEL].BankSelect = PatchBank.DRUM_BANK;
      ReleaseAllHoldPedals();
    }
    public void ResetPrograms() {
      for (var x = 0; x < _synthChannels.Length; x++) {
        _synthChannels[x].Program = 0;
      }
    }
    public string GetProgramName(int channel) {
      if (SoundBank != null) {
        var sChannel = _synthChannels[channel];
        var inst = SoundBank.GetPatch(sChannel.BankSelect, sChannel.Program);
        if (inst != null) {
          return inst.Name;
        }
      }
      return "Null";
    }
    public Patch GetProgram(int channel) {
      if (SoundBank != null) {
        var sChannel = _synthChannels[channel];
        var inst = SoundBank.GetPatch(sChannel.BankSelect, sChannel.Program);
        if (inst != null) {
          return inst;
        }
      }
      return null!;
    }
    public void SetAudioChannelCount(int channels) {
      channels = SynthHelper.Clamp(channels, 1, 2);
      if (AudioChannels != channels) {
        AudioChannels = channels;
        SampleBuffer = new float[MicroBufferSize * MicroBufferCount * AudioChannels];
      }
    }
    public void GetNext(byte[] buffer) {
      Array.Clear(SampleBuffer, 0, SampleBuffer.Length);
      FillWorkingBuffer();
      ConvertWorkingBuffer(buffer, SampleBuffer);
    }
    public void GetNext() {
      Array.Clear(SampleBuffer, 0, SampleBuffer.Length);
      FillWorkingBuffer();
    }
    #region Getters
    public float GetChannelVolume(int channel) => _synthChannels[channel].Volume.Combined / 16383f;
    public float GetChannelExpression(int channel) => _synthChannels[channel].Expression.Combined / 16383f;
    public float GetChannelPan(int channel) => (_synthChannels[channel].Pan.Combined - 8192.0f) / 8192f;
    public float GetChannelPitchBend(int channel) => (_synthChannels[channel].PitchBend.Combined - 8192.0f) / 8192f;
    public bool GetChannelHoldPedalStatus(int channel) => _synthChannels[channel].HoldPedal;
    #endregion
    // private
    private void FillWorkingBuffer() {
      /*Break the process loop into sections representing the smallest timeframe before the midi controls need to be updated
      the bigger the timeframe the more efficient the process is, but playback quality will be reduced.*/
      var sampleIndex = 0;
      for (var x = 0; x < MicroBufferCount; x++) {
        if (MidiEventQueue.Count > 0) {
          for (var i = 0; i < MidiEventCounts[x]; i++) {
            var m = MidiEventQueue.Dequeue();
            ProcessMidiMessage(m.Channel, m.Command, m.Data1, m.Data2);
          }
        }
        //voice processing loop
        var node = _voiceManager.ActiveVoices.First; //node used to traverse the active voices
        while (node != null) {
          node.Value.Process(sampleIndex, sampleIndex + (MicroBufferSize * AudioChannels));
          //if an active voice has stopped remove it from the list
          if (node.Value.VoiceParams.State == VoiceStateEnum.Stopped) {
            var delnode = node; //node used to remove inactive voices
            node = node.Next;
            _voiceManager.RemoveFromRegistry(delnode.Value);
            _voiceManager.ActiveVoices.Remove(delnode);
            _voiceManager.FreeVoices.AddFirst(delnode);
          }
          else {
            node = node.Next;
          }
        }
        sampleIndex += MicroBufferSize * AudioChannels;
      }
      Array.Clear(MidiEventCounts, 0, MidiEventCounts.Length);
    }
    private void ConvertWorkingBuffer(byte[] to, float[] from) {
      if (_littleEndian) {
        for (int x = 0, i = 0; x < from.Length; x++, i += 2) {
          var sample = (short)SynthHelper.Clamp(from[x] * _mainVolume * 32768f, -32768f, 32767f);
          to[i] = (byte)sample;
          to[i + 1] = (byte)(sample >> 8);
        }
      }
      else {
        for (int x = 0, i = 0; x < from.Length; x++, i += 2) {
          var sample = (short)SynthHelper.Clamp(from[x] * _mainVolume * 32768f, -32768f, 32767f);
          to[i] = (byte)(sample >> 8);
          to[i + 1] = (byte)sample;
        }
      }
    }
    #endregion
  }
}
