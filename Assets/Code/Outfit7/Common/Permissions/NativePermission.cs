//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using SimpleJSON;
using Outfit7.Util;

namespace Outfit7.Common.Permissions {

    public class NativePermission {

        // DON'T CHANGE THESE VALUES! They get saved to UserPrefs so only add new ones.
        public enum State {
            Check = 0,
            Request = 1,
            Asking = 2,
            Granted = 3,
            Denied = 4
        }

        public const int RequestPermissionTimes = 2;

        private const string PermissionPrefKeyPrefix = "PermissionManager.Permission.";

#region All supported permissions

        /// <summary>
        /// All supported permissions should be specified here (even app specific ones).
        ///
        /// Permission IDs have to be supported on the native side as well (mind their exact values/names).
        /// </summary>
        public static readonly NativePermission Microphone = new NativePermission("Microphone");
        public static readonly NativePermission Camera = new NativePermission("Camera", int.MaxValue);
        #endregion

        #region Permission fields/properties

        public readonly string PrefKey;

        public string Id { get; private set; }

        public State PermissionState  { get; set; }

        public int PermissionRequestsLeft { get; private set; }

#endregion

        private NativePermission(string id) : this(id, RequestPermissionTimes) {
        }

        private NativePermission(string id, int permissionRequestsLeft) {
            Id = id;
            PrefKey = PermissionPrefKeyPrefix + Id;

            PermissionState = State.Check;
            PermissionRequestsLeft = permissionRequestsLeft;
        }

        public virtual bool CanAskForPermission() {
            return PermissionRequestsLeft > 0 && PermissionState != State.Granted;
        }

        /// <summary>
        /// Specify if or when app settings should be opened. Mostly usable for iOS cases where we can show the
        /// permission dialog only once and after that, we might want to redirect the user directly to app settings.
        /// </summary>
        /// <returns><c>true</c>, if app settings should be opened, <c>false</c> otherwise.</returns>
        public virtual bool ShouldOpenAppSettings() {
            return false;
        }

        public int DecPermissionRequestsLeft() {
            return --PermissionRequestsLeft;
        }

        public void ParsePermissionResult(JSONNode json) {
            Assert.IsTrue(Id.Equals(json.Key), "Permission ids don't match! Id = {0}, json.Key = {1}", Id, json.Key);
            PermissionState = json.AsBool ? NativePermission.State.Granted : NativePermission.State.Denied;
        }

        public JSONClass ToJson() {
            JSONClass json = new JSONClass();
            json["Id"] = Id;
            json["PermissionState"].AsInt = (int) PermissionState;
            json["PermissionRequestsLeft"].AsInt = PermissionRequestsLeft;
            return json;
        }

        public void FillDataFromJson(JSONNode json) {
            Assert.IsTrue(Id.Equals(json["Id"]), "Permission ids don't match! Id = {0}, json Id = {1}", Id, json["Id"]);

            PermissionState = (State) json["PermissionState"].AsInt;
            PermissionRequestsLeft = json["PermissionRequestsLeft"].AsInt;
        }

        public override string ToString() {
            return string.Format("[Permission: {0} = {1}, PermissionRequestsLeft = {2}]", Id, PermissionState, PermissionRequestsLeft);
        }
    }
}

