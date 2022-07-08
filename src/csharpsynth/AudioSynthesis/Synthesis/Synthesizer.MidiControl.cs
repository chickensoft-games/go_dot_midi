
namespace AudioSynthesis.Synthesis {
  using System.Collections.Generic;
  using AudioSynthesis.Bank;
  using AudioSynthesis.Bank.Patches;
  using AudioSynthesis.Midi;
  public partial class Synthesizer {
    internal Queue<MidiMessage> MidiEventQueue;
    internal int[] MidiEventCounts;
    private readonly Patch[] _layerList;

    public IEnumerator<MidiMessage> MidiMessageEnumerator => MidiEventQueue.GetEnumerator();
    /// <summary>
    /// Starts a voice with the given key and velocity.
    /// </summary>
    /// <param name="channel">The midi channel this voice is on.</param>
    /// <param name="note">The key the voice will play in.</param>
    /// <param name="velocity">The volume of the voice.</param>
    public void NoteOn(int channel, int note, int velocity) {
      // Get the correct instrument depending if it is a drum or not
      var sChan = _synthChannels[channel];
      var inst = SoundBank.GetPatch(sChan.BankSelect, sChan.Program);
      if (inst == null) {
        return;
      }
      // A NoteOn can trigger multiple voices via layers
      int layerCount;
      if (inst is MultiPatch patch) {
        layerCount = patch.FindPatches(channel, note, velocity, _layerList);
      }
      else {
        layerCount = 1;
        _layerList[0] = inst;
      }
      // If a key with the same note value exists, stop it
      if (_voiceManager.Registry[channel, note] != null) {
        var node = _voiceManager.Registry[channel, note];
        while (node != null) {
          node.Value.Stop();
          node = node.Next;
        }
        _voiceManager.RemoveFromRegistry(channel, note);
      }
      // Check exclusive groups
      for (var x = 0; x < layerCount; x++) {
        var notseen = true;
        for (var i = x - 1; i >= 0; i--) {
          if (_layerList[x].ExclusiveGroupTarget == _layerList[i].ExclusiveGroupTarget) {
            notseen = false;
            break;
          }
        }
        if (_layerList[x].ExclusiveGroupTarget != 0 && notseen) {
          var node = _voiceManager.ActiveVoices.First;
          while (node != null) {
            if (_layerList[x].ExclusiveGroupTarget == node.Value.Patch.ExclusiveGroup) {
              node.Value.Stop();
              _voiceManager.RemoveFromRegistry(node.Value);
            }
            node = node.Next;
          }
        }
      }
      // Assign a voice to each layer
      for (var x = 0; x < layerCount; x++) {
        var voice = _voiceManager.GetFreeVoice();
        if (voice == null)// out of voices and skipping is enabled
{
          break;
        }

        voice.Configure(channel, note, velocity, _layerList[x], _synthChannels[channel]);
        _voiceManager.AddToRegistry(voice);
        _voiceManager.ActiveVoices.AddLast(voice);
        voice.Start();
      }
      // Clear layer list
      for (var x = 0; x < layerCount; x++) {
        _layerList[x] = null!;
      }
    }
    /// <summary>
    /// Attempts to stop a voice by putting it into its release phase.
    /// If there is no release phase defined the voice will stop immediately.
    /// </summary>
    /// <param name="channel">The channel of the voice.</param>
    /// <param name="note">The key of the voice.</param>
    public void NoteOff(int channel, int note) {
      if (_synthChannels[channel].HoldPedal) {
        var node = _voiceManager.Registry[channel, note];
        while (node != null) {
          node.Value.VoiceParams.NoteOffPending = true;
          node = node.Next;
        }
      }
      else {
        var node = _voiceManager.Registry[channel, note];
        while (node != null) {
          node.Value.Stop();
          node = node.Next;
        }
        _voiceManager.RemoveFromRegistry(channel, note);
      }
    }
    /// <summary>
    /// Stops all voices.
    /// </summary>
    /// <param name="immediate">If true all voices will stop immediately regardless of their release phase.</param>
    public void NoteOffAll(bool immediate) {
      var node = _voiceManager.ActiveVoices.First;
      if (immediate) {//if immediate ignore hold pedals and clear the entire registry
        _voiceManager.ClearRegistry();
        while (node != null) {
          node.Value.StopImmediately();
          var delnode = node;
          node = node.Next;
          _voiceManager.ActiveVoices.Remove(delnode);
          _voiceManager.FreeVoices.AddFirst(delnode);
        }
      }
      else {//otherwise we have to check for hold pedals and double check the registry before removing the voice
        while (node != null) {
          var voiceParams = node.Value.VoiceParams;
          if (voiceParams.State == VoiceStateEnum.Playing) {
            //if hold pedal is enabled do not stop the voice
            if (_synthChannels[voiceParams.Channel].HoldPedal) {
              voiceParams.NoteOffPending = true;
            }
            else {
              node.Value.Stop();
              _voiceManager.RemoveFromRegistry(node.Value);
            }
          }
          node = node.Next;
        }
      }
    }
    /// <summary>
    /// Stops all voices on a given channel.
    /// </summary>
    /// <param name="channel">The midi channel.</param>
    /// <param name="immediate">If true the voices will stop immediately regardless of their release phase.</param>
    public void NoteOffAll(int channel, bool immediate) {
      var node = _voiceManager.ActiveVoices.First;
      while (node != null) {
        if (channel == node.Value.VoiceParams.Channel) {
          if (immediate) {
            node.Value.StopImmediately();
            var delnode = node;
            node = node.Next;
            _voiceManager.ActiveVoices.Remove(delnode);
            _voiceManager.FreeVoices.AddFirst(delnode);
          }
          else {
            //if hold pedal is enabled do not stop the voice
            if (_synthChannels[channel].HoldPedal) {
              node.Value.VoiceParams.NoteOffPending = true;
            }
            else {
              node.Value.Stop();
            }

            node = node.Next;
          }
        }
      }
    }
    /// <summary>
    /// Executes a midi command without queueing it first.
    /// </summary>
    /// <param name="midimsg">A midi message struct.</param>
    public void ProcessMidiMessage(int channel, int command, int data1, int data2) {
      switch (command) {
        case 0x80: //NoteOff
          NoteOff(channel, data1);
          break;
        case 0x90: //NoteOn
          if (data2 == 0) {
            NoteOff(channel, data1);
          }
          else {
            NoteOn(channel, data1, data2);
          }

          break;
        /*case 0xA0: //NoteAftertouch
            synth uses channel after touch instead
            break;*/
        case 0xB0: //Controller
          #region Controller Switch
          switch (data1) {
            case 0x00: //Bank select coarse
              if (channel == MidiHelper.DRUM_CHANNEL) {
                data2 += PatchBank.DrumBank;
              }

              if (SoundBank.IsBankLoaded(data2)) {
                _synthChannels[channel].BankSelect = (byte)data2;
              }
              else {
                _synthChannels[channel].BankSelect = (channel == MidiHelper.DRUM_CHANNEL) ? (byte)PatchBank.DrumBank : (byte)0;
              }

              break;
            case 0x01: //Modulation wheel coarse
              _synthChannels[channel].ModRange.Coarse = (byte)data2;
              _synthChannels[channel].UpdateCurrentMod();
              break;
            case 0x21: //Modulation wheel fine
              _synthChannels[channel].ModRange.Fine = (byte)data2;
              _synthChannels[channel].UpdateCurrentMod();
              break;
            case 0x07: //Channel volume coarse
              _synthChannels[channel].Volume.Coarse = (byte)data2;
              break;
            case 0x27: //Channel volume fine
              _synthChannels[channel].Volume.Fine = (byte)data2;
              break;
            case 0x0A: //Pan coarse
              _synthChannels[channel].Pan.Coarse = (byte)data2;
              _synthChannels[channel].UpdateCurrentPan();
              break;
            case 0x2A: //Pan fine
              _synthChannels[channel].Pan.Fine = (byte)data2;
              _synthChannels[channel].UpdateCurrentPan();
              break;
            case 0x0B: //Expression coarse
              _synthChannels[channel].Expression.Coarse = (byte)data2;
              _synthChannels[channel].UpdateCurrentVolume();
              break;
            case 0x2B: //Expression fine
              _synthChannels[channel].Expression.Fine = (byte)data2;
              _synthChannels[channel].UpdateCurrentVolume();
              break;
            case 0x40: //Hold Pedal
              if (_synthChannels[channel].HoldPedal && !(data2 > 63)) //if hold pedal is released stop any voices with pending release tags
{
                ReleaseHoldPedal(channel);
              }

              _synthChannels[channel].HoldPedal = data2 > 63;
              break;
            case 0x44: //Legato Pedal
              _synthChannels[channel].LegatoPedal = data2 > 63;
              break;
            case 0x63: //NRPN Coarse Select   //fix for invalid DataEntry after unsupported NRPN events
              _synthChannels[channel].Rpn.Combined = 0x3FFF; //todo implement NRPN
              break;
            case 0x62: //NRPN Fine Select     //fix for invalid DataEntry after unsupported NRPN events
              _synthChannels[channel].Rpn.Combined = 0x3FFF; //todo implement NRPN
              break;
            case 0x65: //RPN Coarse Select
              _synthChannels[channel].Rpn.Coarse = (byte)data2;
              break;
            case 0x64: //RPN Fine Select
              _synthChannels[channel].Rpn.Fine = (byte)data2;
              break;
            case 0x78: //All Sounds Off
              NoteOffAll(true);
              break;
            case 0x7B: //All Notes Off
              NoteOffAll(false);
              break;
            case 0x06: //DataEntry Coarse
              switch (_synthChannels[channel].Rpn.Combined) {
                case 0: //change semitone, pitchwheel
                  _synthChannels[channel].PitchBendRangeCoarse = (byte)data2;
                  _synthChannels[channel].UpdateCurrentPitch();
                  break;
                case 1: //master fine tune coarse
                  _synthChannels[channel].MasterFineTune.Coarse = (byte)data2;
                  break;
                case 2: //master coarse tune coarse
                  _synthChannels[channel].MasterCoarseTune = (short)(data2 - 64);
                  break;
                default:
                  break;
              }
              break;
            case 0x26: //DataEntry Fine
              switch (_synthChannels[channel].Rpn.Combined) {
                case 0: //change cents, pitchwheel
                  _synthChannels[channel].PitchBendRangeFine = (byte)data2;
                  _synthChannels[channel].UpdateCurrentPitch();
                  break;
                case 1: //master fine tune fine
                  _synthChannels[channel].MasterFineTune.Fine = (byte)data2;
                  break;
                default:
                  break;
              }
              break;
            case 0x79: //Reset the following controllers, follows midi spec: RP-015
              _synthChannels[channel].Expression.Combined = 0x3FFF;
              _synthChannels[channel].ModRange.Combined = 0;
              if (_synthChannels[channel].HoldPedal) {
                ReleaseHoldPedal(channel);
              }

              _synthChannels[channel].HoldPedal = false;
              _synthChannels[channel].LegatoPedal = false;
              _synthChannels[channel].Rpn.Combined = 0x3FFF;
              _synthChannels[channel].PitchBend.Combined = 0x2000;
              _synthChannels[channel].ChannelAfterTouch = 0;
              _synthChannels[channel].UpdateCurrentPitch(); //because pitchBend was reset
              _synthChannels[channel].UpdateCurrentVolume(); //because expression was reset
              break;
            default:
              return;
          }
          #endregion
          break;
        case 0xC0: //Program Change
          _synthChannels[channel].Program = (byte)data1;
          break;
        case 0xD0: //Channel Aftertouch
          _synthChannels[channel].ChannelAfterTouch = (byte)data2;
          break;
        case 0xE0: //Pitch Bend
          _synthChannels[channel].PitchBend.Coarse = (byte)data2;
          _synthChannels[channel].PitchBend.Fine = (byte)data1;
          _synthChannels[channel].UpdateCurrentPitch();
          break;
        default:
          return;
      }
    }

    //private
    private void ReleaseAllHoldPedals() {
      var node = _voiceManager.ActiveVoices.First;
      while (node != null) {
        if (node.Value.VoiceParams.NoteOffPending) {
          node.Value.Stop();
          _voiceManager.RemoveFromRegistry(node.Value);
        }
        node = node.Next;
      }
    }
    private void ReleaseHoldPedal(int channel) {
      var node = _voiceManager.ActiveVoices.First;
      while (node != null) {
        if (node.Value.VoiceParams.Channel == channel && node.Value.VoiceParams.NoteOffPending) {
          node.Value.Stop();
          _voiceManager.RemoveFromRegistry(node.Value);
        }
        node = node.Next;
      }
    }
  }
}
