using System.Collections.Generic;

namespace AudioSynthesis.Synthesis {
  internal class VoiceManager {
    public class VoiceNode {
      public Voice Value = null!;
      public VoiceNode Next = null!;
    }
    //--Variables
    public VoiceStealEnum StealingMethod;
    public int Polyphony;
    public LinkedList<Voice> FreeVoices;
    public LinkedList<Voice> ActiveVoices;
    public VoiceNode[,] Registry;
    private readonly Voice[] _voicePool;
    private readonly Stack<VoiceNode> _vnodes;

    //--Public Methods
    public VoiceManager(int voiceCount) {
      StealingMethod = VoiceStealEnum.Quietest;
      Polyphony = voiceCount;
      //initialize voice containers
      _voicePool = new Voice[voiceCount];
      VoiceNode[] nodes = new VoiceNode[voiceCount];
      for (int x = 0; x < _voicePool.Length; x++) {
        _voicePool[x] = new Voice();
        nodes[x] = new VoiceNode();
      }
      _vnodes = new Stack<VoiceNode>(nodes);
      //free voice list
      FreeVoices = new LinkedList<Voice>(_voicePool);
      ActiveVoices = new LinkedList<Voice>();
      Registry = new VoiceNode[Synthesizer.DefaultChannelCount, Synthesizer.DefaultKeyCount];
    }
    public Voice GetFreeVoice() {
      if (FreeVoices.Count > 0) {
        Voice voice = FreeVoices.First.Value;
        FreeVoices.RemoveFirst();
        return voice;
      }
      switch (StealingMethod) {
        case VoiceStealEnum.Oldest:
          return StealOldest();
        case VoiceStealEnum.Quietest:
          return StealQuietestVoice();
        default:
          return null;
      }
    }
    public void AddToRegistry(Voice voice) {
      VoiceNode node = _vnodes.Pop();
      node.Value = voice;
      node.Next = Registry[voice.VoiceParams.Channel, voice.VoiceParams.Note];
      Registry[voice.VoiceParams.Channel, voice.VoiceParams.Note] = node;
    }
    public void RemoveFromRegistry(int channel, int note) {
      VoiceNode node = Registry[channel, note];
      while (node != null) {
        _vnodes.Push(node);
        node = node.Next;
      }
      Registry[channel, note] = null;
    }
    public void RemoveFromRegistry(Voice voice) {
      VoiceNode node = Registry[voice.VoiceParams.Channel, voice.VoiceParams.Note];
      if (node == null)
        return;
      if (node.Value == voice) {
        Registry[voice.VoiceParams.Channel, voice.VoiceParams.Note] = node.Next;
        _vnodes.Push(node);
        return;
      }
      else {
        VoiceNode node2 = node;
        node = node.Next;
        while (node != null) {
          if (node.Value == voice) {
            node2.Next = node.Next;
            _vnodes.Push(node);
            return;
          }
          node2 = node;
          node = node.Next;
        }
      }
    }
    public void ClearRegistry() {
      LinkedListNode<Voice> node = ActiveVoices.First;
      while (node != null) {
        VoiceNode vnode = Registry[node.Value.VoiceParams.Channel, node.Value.VoiceParams.Note];
        while (vnode != null) {
          _vnodes.Push(vnode);
          vnode = vnode.Next;
        }
        Registry[node.Value.VoiceParams.Channel, node.Value.VoiceParams.Note] = null;
        node = node.Next;
      }
    }
    public void UnloadPatches() {
      for (int x = 0; x < _voicePool.Length; x++) {
        _voicePool[x].Configure(0, 0, 0, null, null);
        foreach (VoiceNode node in _vnodes)
          node.Value = null;
      }
    }

    private Voice StealOldest() {
      LinkedListNode<Voice> node = ActiveVoices.First;
      //first look for a voice that is not playing
      while (node != null && node.Value.VoiceParams.State == VoiceStateEnum.Playing)
        node = node.Next;
      //if no stopping voice is found use the oldest
      if (node == null)
        node = ActiveVoices.First;
      //check and remove from registry
      RemoveFromRegistry(node.Value);
      ActiveVoices.Remove(node);
      //stop voice if it is not already
      node.Value.VoiceParams.State = VoiceStateEnum.Stopped;
      return node.Value;
    }
    private Voice StealQuietestVoice() {
      float voice_volume = 1000f;
      LinkedListNode<Voice> quietest = null;
      LinkedListNode<Voice> node = ActiveVoices.First;
      while (node != null) {
        if (node.Value.VoiceParams.State != VoiceStateEnum.Playing) {
          float volume = node.Value.VoiceParams.CombinedVolume;
          if (volume < voice_volume) {
            quietest = node;
            voice_volume = volume;
          }
        }
        node = node.Next;
      }
      if (quietest == null)
        quietest = ActiveVoices.First;
      //check and remove from registry
      RemoveFromRegistry(quietest.Value);
      ActiveVoices.Remove(quietest);
      //stop voice if it is not already
      quietest.Value.VoiceParams.State = VoiceStateEnum.Stopped;
      return quietest.Value;
    }
    private Voice StealLowestScore() {
      LinkedListNode<Voice> node = ActiveVoices.First;
      LinkedListNode<Voice> lowest = null;
      int lowScore = int.MaxValue;
      while (node != null) {
        int score = 0;
        if (node.Value.VoiceParams.State == VoiceStateEnum.Stopped) {
          lowest = node;
          break;
        }
        else if (node.Value.VoiceParams.State == VoiceStateEnum.Stopping)
          score -= 50;
        if (node.Value.VoiceParams.Channel == Midi.MidiHelper.DrumChannel)
          score -= 20;

        if (score < lowScore) {
          lowScore = score;
          lowest = node;
        }
        node = node.Next;
      }
      //check and remove from registry
      RemoveFromRegistry(lowest!.Value);
      ActiveVoices.Remove(lowest);
      //stop voice if it is not already
      lowest.Value.VoiceParams.State = VoiceStateEnum.Stopped;
      return lowest.Value;
    }
  }
}
