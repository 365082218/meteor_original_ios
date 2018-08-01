//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//
using UnityEngine;

namespace Outfit7.Achievements {

    public class TimedIncrementalAchievement : IncrementalAchievement {

        private const float DefaultIncrementFrequencySeconds = 3f;

        /// <summary>
        /// Gets or sets the increment frequency in seconds (the achievement will increment after every IncrementFrequencySeconds seconds).
        /// </summary>
        /// <value>The increment frequency.</value>
        public float IncrementFrequencySeconds { get; set; }

        public float StartTime { get; private set; }

        public TimedIncrementalAchievement(int id, string name, int maxSteps) : this(id, name, maxSteps, DefaultIncrementFrequencySeconds) {
        }

        public TimedIncrementalAchievement(int id, string name, int maxSteps, float incrementFrequencySeconds) : base(id, name, maxSteps) {
            IncrementFrequencySeconds = incrementFrequencySeconds;
            StopIncrementing();
        }

        /// <summary>
        /// Checks and updates/increments achievement.
        /// </summary>
        /// <returns><c>true</c>, if achievement was incremented, <c>false</c> otherwise.</returns>
        public virtual bool CheckAndUpdateIncrement() {
            float time = Time.time;
            if (StartTime == float.MaxValue) {
                StartTime = time;
                return false;
            }

            // start incrementing only after IncrementFrequencySeconds time has passed since StartTime was set
            if (StartTime + IncrementFrequencySeconds <= time) {
                StartTime = time;
                IncCurrentSteps();
                return true;
            }

            return false;
        }

        public void StopIncrementing() {
            StartTime = float.MaxValue;
        }
    }
}

