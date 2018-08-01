namespace Outfit7.Devel.SceneLocking {
    public abstract class LockingData {

        public string UserName { get; set; }

        public string LockComment { get; set; }

        public bool Locked { get; set; }

        public string AssetPath { get; private set; }

        protected LockingData(string userName, string lockComment, bool locked, string path) {
            UserName = userName;
            LockComment = lockComment;
            Locked = locked;
            AssetPath = path;
        }

        public abstract string GetName();

        public virtual string GetToolTip() {
            return AssetPath;
        }
    }
}