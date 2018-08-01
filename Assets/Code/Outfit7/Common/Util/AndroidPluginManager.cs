
#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Manager for default activity class of Android application.
    ///
    /// We have two different definitions of class, because default UnityEngine doesn't have switches for different platforms -
    /// so we would have to if-def every call to Android native Java classes (AndroidJavaClass).
    /// I cannot extend AndroidJavaObject due to "exception jni unknown signature".
    /// </summary>
    public class AndroidPluginManager {

        private static AndroidPluginManager instance;

        public static AndroidPluginManager Instance {
            get {
                if (instance == null) {
                    instance = new AndroidPluginManager();
                }
                return instance;
            }
        }

        // Main AndroidJavaClass for connection to Android app
        private readonly AndroidJavaClass ActivityClass;

        private readonly AndroidJavaClass O7ActivityClass;

        // Main AndroidJavaObject for main activity object
        private readonly AndroidJavaObject ActivityObject;

        // Main list for objects
        private readonly Dictionary<string, AndroidJavaObject> ObjectList;

        private AndroidPluginManager() {
            // init activity class
            ActivityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            Assert.NotNull(ActivityClass, "ActivityClass");

            // init currentActivity object
            ActivityObject = ActivityClass.GetStatic<AndroidJavaObject>("currentActivity");
            Assert.NotNull(ActivityObject, "ActivityObject");

            O7ActivityClass = new AndroidJavaClass("com.jinke.demand.JinkeRealize");
            Assert.NotNull(O7ActivityClass, "O7ActivityClass");
            // init list for objects
            ObjectList = new Dictionary<string, AndroidJavaObject>();
        }

        public ReturnType O7CallStaticMethod<ReturnType>(string method, params object[] args)
        {
            return O7ActivityClass.CallStatic<ReturnType>(method, args);
        }

        public void O7CallStaticMethod(string method, params object[] args)
        {
            O7ActivityClass.CallStatic(method, args);
        }

        // Call on main activity object with return type
        public ReturnType ActivityCall<ReturnType>(string methodName, params object[] args) {
            return ActivityObject.Call<ReturnType>(methodName, args);
        }

        // Call on main activity object without return type
        public void ActivityCall(string methodName, params object[] args) {
            ActivityObject.Call(methodName, args);
        }

#region OBJECT_LIST

        // create new AndroidJavaObject and add to list
        public void AddNewObject(string objectID) {
            AndroidJavaObject obj = new AndroidJavaObject(objectID);
            Assert.NotNull(obj, "obj");
            ObjectList.Add(objectID, obj);
        }

        // remove object form list
        public void RemoveObject(string objectID) {
            ObjectList.Remove(objectID);
        }

        //calls a non activity class with a getter on the activity class (caches the object, to speedup subsequent calls)
        public ReturnType CallAnActivityRef<ReturnType>(string activityGetterAndObjectId, string methodName, params object[] objs) {
            AndroidJavaObject obj;
            if (!ObjectList.TryGetValue(activityGetterAndObjectId, out obj)) {
                obj = ActivityCall<AndroidJavaObject>(activityGetterAndObjectId);
                Assert.NotNull(obj, "obj");
                ObjectList.Add(activityGetterAndObjectId, obj);
            }

            return obj.Call<ReturnType>(methodName, objs);
        }

        public ReturnType CallAnActivityRefGetArray<ReturnType>(string activityGetterAndObjectId, string methodName) {
            AndroidJavaObject obj;
            if (!ObjectList.TryGetValue(activityGetterAndObjectId, out obj)) {
                obj = ActivityCall<AndroidJavaObject>(activityGetterAndObjectId);
                Assert.NotNull(obj, "obj");
                ObjectList.Add(activityGetterAndObjectId, obj);
            }

            AndroidJavaObject jObj = obj.Call<AndroidJavaObject>(methodName);

            if (jObj.GetRawObject().ToInt32() != 0) {
                ReturnType r = AndroidJNIHelper.ConvertFromJNIArray<ReturnType>(jObj.GetRawObject());
                jObj.Dispose();
                return r;
            }

            return default(ReturnType);
        }

        //calls a non activity class with a getter on the activity class (caches the object, to speedup subsequent calls)
        public void CallAnActivityRef(string activityGetterAndObjectId, string methodName, params object[] objs) {
            AndroidJavaObject obj;
            if (!ObjectList.TryGetValue(activityGetterAndObjectId, out obj)) {
                obj = ActivityCall<AndroidJavaObject>(activityGetterAndObjectId);
                Assert.NotNull(obj, "obj");
                ObjectList.Add(activityGetterAndObjectId, obj);
            }

            obj.Call(methodName, objs);
        }

        // call static method on object from list (with return type)
        public ReturnType CallStaticMethod<ReturnType>(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject obj;
            if (ObjectList.TryGetValue(objectID, out obj)) {
                return obj.CallStatic<ReturnType>(methodName, objs);
            }
            return default(ReturnType);
        }

        // call static nethod on object from list (without return type)
        public void CallStaticMethod(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject obj;
            if (ObjectList.TryGetValue(objectID, out obj)) {
                obj.CallStatic(methodName, objs);
            }
        }

        // call nethod on object from list (without return type)
        public void CallMethod(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject obj;
            if (ObjectList.TryGetValue(objectID, out obj)) {
                obj.Call(methodName, objs);
            }
        }

        // call method on object from list (with return type)
        public ReturnType CallMethod<ReturnType>(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject obj;
            if (ObjectList.TryGetValue(objectID, out obj)) {
                return obj.Call<ReturnType>(methodName, objs);
            }
            return default(ReturnType);
        }

#endregion

#region STATIC_CALLS

        // call method directly on object
        public static void CallDirect(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject jo = new AndroidJavaObject(objectID);
            jo.Call(methodName, objs);
        }

        // call method directly on object
        public static ReturnType CallDirect<ReturnType>(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject jo = new AndroidJavaObject(objectID);
            return jo.Call<ReturnType>(methodName, objs);
        }

        // call static method directly on object
        public static void CallStaticDirect(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject jo = new AndroidJavaObject(objectID);
            jo.CallStatic(methodName, objs);
        }

        // call method directly on object
        public static ReturnType CallStaticDirect<ReturnType>(string objectID, string methodName, params object[] objs) {
            AndroidJavaObject jo = new AndroidJavaObject(objectID);
            return jo.CallStatic<ReturnType>(methodName, objs);
        }

#endregion
    }
}

#endif
