//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Util;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Common.Permissions {

    public class PermissionPlugin : MonoBehaviour {

        private enum PermissionState {
            Denied = 0,
            Granted = 1,
            Undefined = 2
        }

        public const string Tag = "PermissionPlugin";

        public PermissionManager PermissionManager { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int _CheckPermission(string permissionType);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _RequestPermission(string permissionType);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _OpenAppSettings();
#endif

        public NativePermission.State RequestPermission(NativePermission permission) {
            O7Log.InfoT(Tag, "RequestPermission: {0}", permission);
            NativePermission.State permissionState = NativePermission.State.Granted;

#if UNITY_EDITOR || NATIVE_SIM
            _OnRequestedPermissionResult(FakePermissionResponse(permission.Id));
#elif UNITY_IPHONE
            _RequestPermission(permission.Id);
            permissionState = NativePermission.State.Asking;
#elif UNITY_ANDROID
            AndroidPluginManager.Instance.ActivityCall("requestPermission", permission.Id);
            permissionState = NativePermission.State.Asking;
#elif UNITY_WP8

#endif
            return permissionState;
        }

        public NativePermission.State CheckPermission(NativePermission permission) {
#if UNITY_EDITOR || NATIVE_SIM
            return NativePermission.State.Granted;
#elif UNITY_IPHONE
            PermissionState state = (PermissionState) _CheckPermission(permission.Id);
            O7Log.InfoT(Tag, "CheckPermission: {0}, state = {1}", permission, state);
            switch (state) {
                case PermissionState.Denied:
                    return NativePermission.State.Denied;
                case PermissionState.Granted:
                    return NativePermission.State.Granted;
                case PermissionState.Undefined:
                    return NativePermission.State.Request;
                default:
                    throw new System.Exception("Unknown permission state: " + state);
            }
#elif UNITY_ANDROID
            bool granted = AndroidPluginManager.Instance.ActivityCall<bool>("checkPermission", permission.Id);
            O7Log.InfoT(Tag, "CheckPermission: {0}, granted = {1}", permission, granted);
            return granted ? NativePermission.State.Granted : NativePermission.State.Denied;
#elif UNITY_WP8
            return NativePermission.State.Granted;
#endif
        }

        public bool OpenAppSettings() {
            bool settingsOpened = true;
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            settingsOpened = _OpenAppSettings();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
            O7Log.InfoT(Tag, "OpenAppSettings: settingsOpened = {0}", settingsOpened);
            return settingsOpened;
        }

        public void _OnRequestedPermissionResult(string permissionResult) {
            O7Log.InfoT(Tag, "_OnRequestedPermissionResult: {0}", permissionResult);
            PermissionManager.OnRequestedPermissionResult(permissionResult);
        }

#if UNITY_EDITOR || NATIVE_SIM
        private string FakePermissionResponse(string permission) {
            JSONClass json = new JSONClass();
            json[permission] = "true";
            O7Log.VerboseT("PermissionPlugin", "json: {0}", json.ToString());
            return json.ToString();
        }
#endif
    }

}
