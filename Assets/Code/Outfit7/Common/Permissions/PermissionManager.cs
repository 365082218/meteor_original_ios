//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Util;
using SimpleJSON;
using System.Collections.Generic;
using System;

namespace Outfit7.Common.Permissions {

    public abstract class PermissionManager {

        public const string Tag = "PermissionManager";

        public PermissionPlugin PermissionPlugin { get; set; }

        protected abstract NativePermission[] InitPermissions { get; }

        private Dictionary<string, NativePermission> PermissionIds;

        public static void ClearPrefs() {
            // remove all permissions as defined in Permission class (crapy implementation because ClearPrefs() has to be static...)
            UserPrefs.Remove(NativePermission.Microphone.PrefKey);
            UserPrefs.Remove(NativePermission.Camera.PrefKey);
        }

        public virtual void Init() {
            PermissionIds = new Dictionary<string, NativePermission>(InitPermissions.Length);

            foreach (var permission in InitPermissions) {
                InitPermission(permission);
            }
        }

        /// <summary>
        /// Checks for the current permission state. Returns cached value or asks the native for permission status if neccessary.
        /// </summary>
        /// <returns>Permission with the current permission state set.</returns>
        /// <param name="permission">Permission.</param>
        public virtual NativePermission CheckPermission(NativePermission permission) {
            if (permission.PermissionState == NativePermission.State.Check) {
                ForceCheckPermission(permission);
            }

            return permission;
        }

        /// <summary>
        /// Simple helper method that checks if the permission is granted.
        /// </summary>
        /// <returns><c>true</c>, if permission granted, <c>false</c> if not.</returns>
        /// <param name="permission">Permission.</param>
        public virtual bool CheckPermissionGranted(NativePermission permission) {
            return CheckPermission(permission).PermissionState == NativePermission.State.Granted;
        }

        /// <summary>
        /// Checks the permission status on the native side and saves the resulting permission state.
        /// </summary>
        /// <returns>Permission with the current permission state set.</returns>
        /// <param name="permission">Permission.</param>
        /// <param name = "saveNewState">Save permission state if changed or not</param>
        public virtual NativePermission ForceCheckPermission(NativePermission permission, bool saveNewState = true) {
            NativePermission.State checkedState = PermissionPlugin.CheckPermission(permission);
            if (permission.PermissionState != checkedState) {
                permission.PermissionState = checkedState;

                if (saveNewState) {
                    SavePermission(permission);
                }
            }
            O7Log.DebugT(Tag, "Permission checked: {0}", permission);

            return permission;
        }

        /// <summary>
        /// Checks the permission status and requests it from the native if neccessary.
        /// </summary>
        /// <returns>Permission with the current permission state set.</returns>
        /// <param name="permission">Permission.</param>
        public virtual NativePermission CheckOrRequestPermission(NativePermission permission) {
            NativePermission.State permissionState = CheckPermission(permission).PermissionState;
            if (permissionState != NativePermission.State.Request) {
                return permission;
            }

            if (permission.CanAskForPermission()) {
                if (permission.ShouldOpenAppSettings()) {
                    O7Log.DebugT(Tag, "Opening app settings for permission: {0}", permission);
                    permission.PermissionState = PermissionPlugin.OpenAppSettings() ? NativePermission.State.Asking : NativePermission.State.Denied;
                } else {
                    permission.PermissionState = PermissionPlugin.RequestPermission(permission);
                }
                permission.DecPermissionRequestsLeft();
                O7Log.DebugT(Tag, "Request direct response: {0}", permission);
            } else {
                permission.PermissionState = NativePermission.State.Denied;
                O7Log.DebugT(Tag, "No more permission requests left - fallback to Denied: {0}", permission);
            }

            SavePermission(permission); // save the new values
            return permission;
        }

        /// <summary>
        /// Simple helper method that checks if the permission is granted. If not, requests permission.
        /// </summary>
        /// <returns><c>true</c>, if permission granted, <c>false</c> if not (and requests permission).</returns>
        /// <param name="permission">Permission.</param>
        public virtual bool CheckOrRequestPermissionGranted(NativePermission permission) {
            return CheckOrRequestPermission(permission).PermissionState == NativePermission.State.Granted;
        }

        /// <summary>
        /// This method is called when the native returns the permission request letting us know if the premission has been Granted or Denied.
        /// </summary>
        /// <param name="permissionResult">Permission result.</param>
        public virtual void OnRequestedPermissionResult(string permissionResult) {
            O7Log.DebugT(Tag, "Got permissions JSON result: {0}", permissionResult);

            JSONNode json = JSONNode.Parse(permissionResult);

            foreach (var item in json.Childs) {
                O7Log.VerboseT(Tag, "json.key: {0}, json.value: {1}", item.Key, item.Value);
                NativePermission permission = GetPermissionById(item.Key);
                permission.ParsePermissionResult(item);
                O7Log.DebugT(Tag, "RequestedPermissionResult: {0}", permission);
                SavePermission(permission);
            }
        }

        public virtual NativePermission GetPermissionById(string permissionId) {
            return PermissionIds[permissionId];
        }

        private void InitPermission(NativePermission permission) {
            O7Log.DebugT(Tag, "Loading permission: {0}", permission);
            PermissionIds.Add(permission.Id, permission);

            JSONNode json = UserPrefs.GetJson(permission.PrefKey, null);
            // if we have already requested this permission (because it's state is not Request state), check it's current state on every app start
            if (json != null) {
                permission.FillDataFromJson(json); // load permission data from json (mainly just PermissionRequestsLeft)
            }

            // execute call only on proper device, since in editor all permissions succeed (PermissionRequestsLeft issue)
#if !(UNITY_EDITOR || NATIVE_SIM)
            ForceCheckPermission(permission); // check with the native for this permission on every app start (also saves permission status)
#endif

            // if we have any permission requests left and this permission still isn't granted, set permission state to ask the user for this permission (again)
            if (permission.CanAskForPermission()) {
                permission.PermissionState = NativePermission.State.Request;
                SavePermission(permission);
            }

            O7Log.DebugT(Tag, "Permission loaded: {0}", permission);
        }

        private void SavePermission(NativePermission permission) {
            O7Log.DebugT(Tag, "Saving permission: {0}", permission);
            UserPrefs.SetJson(permission.PrefKey, permission.ToJson());
            UserPrefs.SaveDelayed();
        }

        public virtual bool OpenAppSettings() {
            return PermissionPlugin.OpenAppSettings();
        }

        public virtual void OnAppResume() {
            // refresh permission states in case the user would grant/denied permissions and then resume the app
            NativePermission.State previousState;
            NativePermission.State newState;

            foreach (var permission in InitPermissions) {
                previousState = permission.PermissionState;
                newState = ForceCheckPermission(permission, false).PermissionState;

                if (newState == previousState) {
                    continue;
                }
                O7Log.DebugT(Tag, "Permission changed on app resume: previousState = {0}, new permission = {1}", previousState, permission);

                switch (newState) {
                    case NativePermission.State.Granted:
                        // do nothing but have this case covered so that the exception is not thrown below (default case)
                        break;

                    case NativePermission.State.Denied:
                        if (previousState != NativePermission.State.Granted) {
                            // if this permission is now Denied but previously wasn't Granted, revert permission state
                            // to the previous one (Denied/Asking/Request/Check)
                            permission.PermissionState = previousState;
                        }
                        break;

                    case NativePermission.State.Request:
                        // force request permission [MTT-12222]
                        permission.PermissionState = PermissionPlugin.RequestPermission(permission);
                        break;

                    default:
                        throw new Exception("Unhandled permission state returned for permission: " + permission);
                }

                // save new/updated permission state
                SavePermission(permission);
            }
        }

    }
}
