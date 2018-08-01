namespace Outfit7.Devel.SceneLocking {
    public class SceneLockingData : LockingData {

        public string SceneName { get; private set; }

        public SceneLockingData(string userName, string lockComment, bool locked, string scenePath, string sceneName) : base(userName, lockComment, locked, scenePath) {
            SceneName = sceneName;
        }

        public override string GetName() {
            return SceneName;
        }
    }
}