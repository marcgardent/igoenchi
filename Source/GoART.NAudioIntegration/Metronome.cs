using System;

namespace GoART.NAudioIntegration
{
    public class Metronome
    {
        public TimeSpan Current { get; private set; }
        public TimeSpan Tick { get; set; }
        
        public void Move(int ticks=1)
        {
            Current += TimeSpan.FromTicks(Tick.Ticks * ticks);
        }

        public Metronome(TimeSpan tick)
        {
            Tick = tick;
        }

        public Metronome(int bpm)
        {
            Tick = TimeSpan.FromSeconds(bpm/60.0);
        }
    }
}