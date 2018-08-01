//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.Achievements {

    public class IncrementalAchievement : Achievement {

        public int CurrentSteps { get; protected set; }

        public int MaxSteps { get; private set; }

        public float CurrentProgressInPercent { get { return 100f * CurrentSteps / MaxSteps; } }

        public IncrementalAchievement(int id, string name, int maxSteps) : base(id, name) {
            Assert.IsTrue(maxSteps >= 2, "maxSteps must be >= 2 for achievement id: {0}", id);
            MaxSteps = maxSteps;
            CurrentSteps = 0;
        }

        public int IncCurrentSteps() {
            return IncCurrentSteps(1);
        }

        public virtual int IncCurrentSteps(int bySteps) {
            if (bySteps < 1) {
                O7Log.DebugT(Tag, "Not incrementing '{0}': bySteps = {1}", Name, bySteps);
                return CurrentSteps;
            }

            if (CurrentSteps >= MaxSteps) {
                O7Log.VerboseT(Tag, "Not incrementing '{0}'; already at MaxSteps: {1}/{2} ({3}%)", Name, CurrentSteps, MaxSteps, CurrentProgressInPercent);
                return CurrentSteps;
            }

            if (CurrentSteps + bySteps > MaxSteps) {
                CurrentSteps = MaxSteps;
            } else {
                CurrentSteps += bySteps;
            }

            if (CurrentSteps == MaxSteps) {
                Unlock();
            }

            O7Log.VerboseT(Tag, "Achievement '{0}' incremented to: {1}/{2} ({3}%)", Name, CurrentSteps, MaxSteps, CurrentProgressInPercent);
            return CurrentSteps;
        }

        public override bool Unlock() {
            if (CurrentSteps >= MaxSteps) {
                return base.Unlock();
            }
            return false;
        }

        public override string ToString() {
            return string.Format("{0}; CurrentSteps = {1}, MaxSteps = {2}, CurrentProgress = {3}", base.ToString(), CurrentSteps, MaxSteps, CurrentProgressInPercent);
        }
    }

    public class IncrementalAchievement<T> : IncrementalAchievement {

        public T Data { get; protected set; }

        public IncrementalAchievement(int id, string name, int maxSteps) : this(id, name, maxSteps, default(T)) {
        }

        public IncrementalAchievement(int id, string name, int maxSteps, T data) : base(id, name, maxSteps) {
            Data = data;
        }

    }
}

