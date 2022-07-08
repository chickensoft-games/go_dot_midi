using System.Collections.Generic;

namespace AudioSynthesis.Synthesis {
  internal class VoiceManager {
    public class VoiceNode {
      public Voice Value;
      public VoiceNode Next;
    }
    //--Variables
    public VoiceStealEnum stealingMethod;
    public int polyphony;
    public LinkedList<Voice> freeVoices;
    public LinkedList<Voice> activeVoices;
    public VoiceNode[,] registry;
    private Voice[] voicePool;
    private Stack<VoiceNode> vnodes;

    //--Public Methods
    public VoiceManager(int voiceCount) {
      stealingMethod = VoiceStealEnum.Quietest;
      polyphony = voiceCount;
      //initialize voice containers
      voicePool = new Voice[voiceCount];
      VoiceNode[] nodes = new VoiceNode[voiceCount];
      for (int x = 0; x < voicePool.Length; x++) {
        voicePool[x] = new Voice();
        nodes[x] = new VoiceNode();
      }
      vnodes = new Stack<VoiceNode>(nodes);
      //free voice list
      freeVoices = new LinkedList<Voice>(voicePool);
      activeVoices = new LinkedList<Voice>();
      registry = new VoiceNode[Synthesizer.DefaultChannelCount, Synthesizer.DefaultKeyCount];
    }
    public Voice GetFreeVoice() {
      if (freeVoices.Count > 0) {
        Voice voice = freeVoices.First.Value;
        freeVoices.RemoveFirst();
        return voice;
      }
      switch (stealingMethod) {
        case VoiceStealEnum.Oldest:
          return StealOldest();
        case VoiceStealEnum.Quietest:
          return StealQuietestVoice();
        default:
          return null;
      }
    }
    public void AddToRegistry(Voice voice) {
      VoiceNode node = vnodes.Pop();
      node.Value = voice;
      node.Next = registry[voice.VoiceParams.Channel, voice.VoiceParams.Note];
      registry[voice.VoiceParams.Channel, voice.VoiceParams.Note] = node;
    }
    public void RemoveFromRegistry(int channel, int note) {
      VoiceNode node = registry[channel, note];
      while (node != null) {
        vnodes.Push(node);
        node = node.Next;
      }
      registry[channel, note] = null;
    }
    public void RemoveFromRegistry(Voice voice) {
      VoiceNode node = registry[voice.VoiceParams.Channel, voice.VoiceParams.Note];
      if (node == null)
        return;
      if (node.Value == voice) {
        registry[voice.VoiceParams.Channel, voice.VoiceParams.Note] = node.Next;
        vnodes.Push(node);
        return;
      }
      else {
        VoiceNode node2 = node;
        node = node.Next;
        while (node != null) {
          if (node.Value == voice) {
            node2.Next = node.Next;
            vnodes.Push(node);
            return;
          }
          node2 = node;
          node = node.Next;
        }
      }
    }
    public void ClearRegistry() {
      LinkedListNode<Voice> node = activeVoices.First;
      while (node != null) {
        VoiceNode vnode = registry[node.Value.VoiceParams.Channel, node.Value.VoiceParams.Note];
        while (vnode != null) {
          vnodes.Push(vnode);
          vnode = vnode.Next;
        }
        registry[node.Value.VoiceParams.Channel, node.Value.VoiceParams.Note] = null;
        node = node.Next;
      }
    }
    public void UnloadPatches() {
      for (int x = 0; x < voicePool.Length; x++) {
        voicePool[x].Configure(0, 0, 0, null, null);
        foreach (VoiceNode node in vnodes)
          node.Value = null;
      }
    }

    private Voice StealOldest() {
      LinkedListNode<Voice> node = activeVoices.First;
      //first look for a voice that is not playing
      while (node != null && node.Value.VoiceParams.State == VoiceStateEnum.Playing)
        node = node.Next;
      //if no stopping voice is found use the oldest
      if (node == null)
        node = activeVoices.First;
      //check and remove from registry
      RemoveFromRegistry(node.Value);
      activeVoices.Remove(node);
      //stop voice if it is not already
      node.Value.VoiceParams.State = VoiceStateEnum.Stopped;
      return node.Value;
    }
    private Voice StealQuietestVoice() {
      float voice_volume = 1000f;
      LinkedListNode<Voice> quietest = null;
      LinkedListNode<Voice> node = activeVoices.First;
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
        quietest = activeVoices.First;
      //check and remove from registry
      RemoveFromRegistry(quietest.Value);
      activeVoices.Remove(quietest);
      //stop voice if it is not already
      quietest.Value.VoiceParams.State = VoiceStateEnum.Stopped;
      return quietest.Value;
    }
    private Voice StealLowestScore() {
      LinkedListNode<Voice> node = activeVoices.First;
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
      RemoveFromRegistry(lowest.Value);
      activeVoices.Remove(lowest);
      //stop voice if it is not already
      lowest.Value.VoiceParams.State = VoiceStateEnum.Stopped;
      return lowest.Value;
    }
  }
}
