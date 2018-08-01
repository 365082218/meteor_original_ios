using System;

namespace Outfit7.Logic {

    [Serializable]
    public class MessageEvent {
        public int EventId;
        public int IntData0;
        public int IntData1;
        public float FloatData0;
        public float FloatData1;
        public UnityEngine.Object ObjectData0;
        public object ObjectData1;
        public object UserData;
        public object Sender;

        public override string ToString() {
            return string.Format("EventId: {0}; IntDatat0: {1}; IntDatat1: {2}; FloatDatat0: {3}; FloatDatat1: {4}; ObjectData0: {5}; ObjectData1: {6}; UserData: {7}; Sender: {8}",
                EventId, IntData0, IntData1, FloatData0, FloatData1, 
                ObjectData0 != null ? ObjectData0.ToString() : "null", ObjectData1 != null ? ObjectData1.ToString() : "null", 
                UserData != null ? UserData.ToString() : "null", Sender != null ? Sender.ToString() : "null");
        }
    }
}