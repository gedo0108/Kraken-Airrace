using System;
using System.Collections.Generic;

namespace KrakenAirrace
{
    // The current race
    public class Airrace
    {
        // The amount of targets
        public Int32 amount;

        // The position of the vessel
        public Int32 position;

        // The amount of rounds
        public Int32 rounds;

        // All targets
        public List<AirraceTargetModule> targets;

        // The time needed to fly the race
        public TimeSpan time;
    }
}