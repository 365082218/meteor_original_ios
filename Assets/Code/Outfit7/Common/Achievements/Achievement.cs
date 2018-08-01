//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//
using Outfit7.Util;

namespace Outfit7.Achievements {

    public class Achievement {

        public virtual string Tag {
            get {
                if (tag == null) {
                    tag = GetType().Name;
                }
                return tag;
            }
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public bool IsUnlocked { get; set; }

        protected string tag;

        public Achievement(int id, string name) {
            Id = id;
            Name = name;
        }

        public virtual bool Unlock() {
            if (IsUnlocked) {
                O7Log.WarnT(Tag, "Unlocking achievement - already unlocked! {0}", this);
                return false;
            }

            IsUnlocked = true;
            O7Log.DebugT(Tag, "Unlocked achievement: {0}", this);
            return true;
        }

        public override string ToString() {
            return string.Format("{0}: Id = {1}, Name = {2}, IsUnlocked = {3}", Tag, Id, Name, IsUnlocked);
        }
    }

    public class Achievement<T> : Achievement {

        public T Data { get; set; }

        public Achievement(int id, string name) : this(id, name, default(T)) {
        }

        public Achievement(int id, string name, T data) : base(id, name) {
            Data = data;
        }

    }
}

