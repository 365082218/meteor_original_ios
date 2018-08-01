using System.IO;

namespace Outfit7.Devel.SceneLocking {
    public class PrefabLockingData : LockingData {

        public string PrefabName { get; private set; }

        public string PrefabGUID { get; private set; }

        public PrefabLockingData(string userName, string lockComment, bool locked, string prefabPath, string prefabGUID) : base(userName, lockComment, locked, prefabPath) {
            PrefabName = Path.GetFileNameWithoutExtension(prefabPath);
            PrefabGUID = prefabGUID;
        }

        public override string GetName() {
            return string.Format("{0}.prefab", PrefabName);
        }
    }
}