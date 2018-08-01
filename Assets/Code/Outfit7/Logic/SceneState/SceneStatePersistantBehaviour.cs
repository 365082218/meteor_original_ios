using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Logic {
    public class SceneStatePersistentBehaviour<T> : SceneStateBehaviour where T : SceneStatePersistentData, new() {

        public T PersistentData { get; private set; }

        public sealed override void InitializePersistentData(ref SceneStatePersistentData persistentData) {
            if (persistentData == null) {
                persistentData = new T();
            }
            PersistentData = (T) persistentData;
        }

    }
}