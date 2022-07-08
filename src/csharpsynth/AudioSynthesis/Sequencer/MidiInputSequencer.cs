/*
 *    ______   __ __     _____             __  __
 *   / ____/__/ // /_   / ___/__  ______  / /_/ /_
 *  / /    /_  _  __/   \__ \/ / / / __ \/ __/ __ \
 * / /___ /_  _  __/   ___/ / /_/ / / / / /_/ / / /
 * \____/  /_//_/     /____/\__, /_/ /_/\__/_/ /_/
 *                         /____/
 * Midi Input Sequencer
 *  Used for midi input using short messages.
 *  Tempo is calculated by the input device so the messages are all processed at the same time with: FillSequencerQueue(...)
 */
namespace AudioSynthesis.Sequencer {
  using AudioSynthesis.Synthesis;

  public class MidiInputSequencer {
    public Synthesizer Synth { get; set; }

    public MidiInputSequencer(Synthesizer synth) => Synth = synth;
    public void AddMidiEvent(MidiMessage midiMsg) {
      midiMsg.delta = 0;
      Synth.MidiEventQueue.Enqueue(midiMsg);
      Synth.MidiEventCounts[0]++;
    }
  }
}
