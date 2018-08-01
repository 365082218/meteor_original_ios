
//#define VERBOSE

using UnityEngine;
using System.Collections.Generic;
using Outfit7.Util;
using System.Text;
using Outfit7.Logic;

namespace Outfit7.Audio {
    public class AudioEventManager : Manager<AudioEventManager> {
        const string Tag = "AudioEventManager";
        const string AudioEventName = "AudioEvent";
        const int DefaultPoolSize = 16;

        private static bool DebugInfoInternal = false;

        public static bool DebugInfo {
            get {
                return DebugInfoInternal;
            }
            set {
                if (DebugInfoInternal == value) {
                    return;
                }
                //
                DebugInfoInternal = value;
            }
        }

        private List<AudioEvent> AudioEventPoolFree = new List<AudioEvent>();
        private List<AudioEvent> AudioEventPoolUsed = new List<AudioEvent>();
        private List<AudioEvent> ActiveAudioEvents = new List<AudioEvent>();
        private GameObject GroupParent = null;

        public Camera Panning2DCamera = null;
        public AnimationCurve Panning2DStereoCurve = null;
        public AnimationCurve Panning2DVolumeCurve = null;

#if UNITY_EDITOR
        static float EditorOldTime = 0.0f;
#endif

#if UNITY_EDITOR || DEVEL_BUILD
        static StringBuilder InfoStringBuilder = new StringBuilder(64 * 1024);
#endif

#if UNITY_EDITOR
        public static AudioEvent CreateAudioEventForEditor() {
            GameObject go = new GameObject();
            go.name = "Editor Audio Event";
            AudioEvent audioEvent = go.AddComponent<AudioEvent>();
            audioEvent.SourceMain = go.AddComponent<AudioSource>();
            audioEvent.SourceMain.playOnAwake = false;
            audioEvent.SourceAux = go.AddComponent<AudioSource>();
            audioEvent.SourceAux.playOnAwake = false;
            audioEvent.ResetForPool();
            audioEvent.gameObject.hideFlags = HideFlags.DontSave;
            return audioEvent;
        }
#endif

        public AudioEvent CreateAudioEventInstance() {
            GameObject go = new GameObject();
            if (GroupParent == null) {
                GroupParent = new GameObject();
                GameObject.DontDestroyOnLoad(GroupParent);
                GroupParent.name = "AudioEvents";
            }
            GameObject.DontDestroyOnLoad(go);
            go.transform.SetParent(GroupParent.transform);
            go.name = AudioEventName;
            AudioEvent audioEvent = go.AddComponent<AudioEvent>();
            audioEvent.SourceMain = go.AddComponent<AudioSource>();
            audioEvent.SourceMain.playOnAwake = false;
            audioEvent.SourceAux = go.AddComponent<AudioSource>();
            audioEvent.SourceAux.playOnAwake = false;
            audioEvent.ResetForPool();
            return audioEvent;
        }

        public override bool OnInitialize() {
            Cleanup();
            // populate pool
            for (int a = 0; a < DefaultPoolSize; a++) {
                AudioEvent ae = CreateAudioEventInstance();
                AudioEventPoolFree.Add(ae);
            }

            // mixer extension
            AudioMixerExtension.OnInit();

            return base.OnInitialize();
        }

        public override void OnTerminate() {
            Cleanup();
            base.OnTerminate();
        }

        public void Cleanup() {
            // stop all active events
            while (AudioEventPoolUsed.Count > 0) {
                AudioEvent ae = AudioEventPoolUsed[0];
                DeleteAudioEvent(ae);
            }
            // kill all instances
            for (int a = 0; a < AudioEventPoolFree.Count; a++) {
                AudioEvent ae = AudioEventPoolFree[0];
                if (ae != null && ae.gameObject != null) {
                    GameObject.Destroy(ae.gameObject);
                }
            }
            AudioEventPoolFree.Clear();
        }

        public void OnLevelLoaded() {
            for (int a = 0; a < ActiveAudioEvents.Count; a++) {
                AudioEvent ae = ActiveAudioEvents[a];
                if ((ae.EventAttributes & EAudioEventAttributes.StopOnSceneChange) == EAudioEventAttributes.StopOnSceneChange) {
                    DeleteAudioEvent(ae);
                    a--;
                }
            }
        }

        public bool IsAnyAudioPlaying {
            get {
                for (int a = 0; a < ActiveAudioEvents.Count; a++) {
                    AudioEvent ae = ActiveAudioEvents[a];
                    if (ae != null && ae.IsPlaying) {
                        return true;
                    }
                }

                for (int a = 0; a < AudioEventPoolUsed.Count; a++) {
                    AudioEvent ae = AudioEventPoolUsed[a];
                    if (ae != null && ae.IsPlaying) {
                        return true;
                    }
                }

                return false;
            }
        }

        public void RegisterActiveAudioEvent(AudioEvent ae) {
            if (ActiveAudioEvents.Contains(ae)) {
                O7Log.ErrorT(Tag, "AudioEvent instance already in active list!");
            }
            ActiveAudioEvents.Add(ae);
        }

        public void UnregisterActiveAudioEvent(AudioEvent ae) {
            ActiveAudioEvents.Remove(ae);
        }

        public void PlayOneShot(AudioEventData eventData, Transform transform = null) {
            AudioEvent ae = CreateAudioEvent(eventData, transform, EAudioEventAttributes.AutoDelete);
            if (ae) {
                ae.Play();
            }
        }

        public void PlayOneShot(AudioEventData eventData, float delay, Transform transform = null) {
            AudioEvent ae = CreateAudioEvent(eventData, transform, EAudioEventAttributes.AutoDelete);
            if (ae) {
                ae.PlayFullArg(true, 0, null, delay);
            }
        }

        public AudioEvent CreateAudioEvent(AudioEventData eventData, Transform transform = null, EAudioEventAttributes attr = EAudioEventAttributes.None) {
            if (CanPlay(eventData) == false) {
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
                O7Log.DebugT(Tag, "CreateAudioEvent: CanPlay says no.");
#endif
                return null;
            }
            // take from pool if available
            AudioEvent ae = null;
            if (AudioEventPoolFree.Count == 0) {
                if (AudioEventPoolUsed.Count == DefaultPoolSize) {
                    return null;
                } else {
                    ae = CreateAudioEventInstance();
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
                    O7Log.ErrorT(Tag, "CreateAudioEvent: Pool empty, allocating new instance...");
#endif
                }
            } else {
                ae = AudioEventPoolFree[0];
                AudioEventPoolFree.RemoveAt(0);
            }
            // initialize if valid
            if (ae) {
                if (AudioEventPoolUsed.Contains(ae)) {
                    O7Log.ErrorT(Tag, "AudioEvent instance already in used list!");
                }
#if UNITY_EDITOR
                ae.gameObject.name = eventData.name;
#endif
                AudioEventPoolUsed.Add(ae);
                ae.EventAttributes = attr;
                ae.FollowTransfrom = transform;
                ae.Initialize(eventData);
                eventData.InstanceCounter++;
            }
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
            else {
                O7Log.DebugT(Tag, "CreateAudioEvent: instance is null.");
            }
#endif
            //
            return ae;
        }

        public void DeleteAudioEvent(AudioEvent ae) {
            if (ae == null) {
                AudioEventPoolUsed.Remove(ae);
                return;
            }

            // fire callbacks
            if (ae.OnEventDestroy != null) {
                ae.OnEventDestroy(ae);
            }
#if UNITY_EDITOR
            ae.gameObject.name = AudioEventName;
#endif
            // force stop
            ae.Stop(true);
            if (ae.EventDataInternal != null) {
                ae.EventDataInternal.InstanceCounter--;
            }
            // reparent back
            ae.transform.SetParent(GroupParent.transform);
            ae.transform.localPosition = Vector3.zero;
            // reset
            ae.ResetForPool();
            // back to pool
            AudioEventPoolUsed.Remove(ae);
            AudioEventPoolFree.Add(ae);
        }

        public override void OnPreUpdate(float deltaTime) {
            base.OnPreUpdate(deltaTime);
#if UNITY_EDITOR
            deltaTime = Mathf.Clamp(Time.realtimeSinceStartup - EditorOldTime, 0.0001f, 0.0166f);
            EditorOldTime = Time.realtimeSinceStartup;
#endif
            // mixer extension
            AudioMixerExtension.OnUpdate(deltaTime);

            // first, update all active instances, purge nulls
            for (int a = 0; a < ActiveAudioEvents.Count; a++) {
                AudioEvent ae = ActiveAudioEvents[a];
                if (ae == null) {
                    ActiveAudioEvents.RemoveAt(a);
                    continue;
                }
                ae.OnUpdate(deltaTime);
            }
            // second, check all used ones, purge nulls, remove dead
            for (int a = 0; a < AudioEventPoolUsed.Count; a++) {
                AudioEvent ae = AudioEventPoolUsed[a];
                if (ae == null) {
                    ActiveAudioEvents.RemoveAt(a);
                    continue;
                }
                //
                if (ae.IsPlaying == false && ae.IsAutoDelete) {
                    DeleteAudioEvent(ae);
                    a--;
                }
            }
        }

        private bool CanPlay(AudioEventData eventData) {
            // no data, no play
            if (eventData == null) {
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
                O7Log.DebugT(Tag, "CanPlay: data is null.");
#endif
                return false;
            }
            // first check instnace limiting, if this is reached, return false
            if (eventData.InstanceLimit > 0 && eventData.InstanceCounter >= eventData.InstanceLimit) {
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
                O7Log.DebugT(Tag, "CanPlay: limit reached: {0}: {1}/{2}", eventData.name, eventData.InstanceCounter, eventData.InstanceLimit);
#endif
                return false;
            }

            // check overlapping
            if (eventData.OverlappingLimitRelativeLength > 0.0f && eventData.InstanceLimit > 0) {
                // find same data
                for (int a = 0; a < ActiveAudioEvents.Count; a++) {
                    AudioEvent ae = ActiveAudioEvents[a];
                    if (ae.EventDataInternal == eventData) {
                        float st = ae.GetSourceTime();
                        float et = ae.GetSourceLength() * eventData.OverlappingLimitRelativeLength;
                        if (st < et) {
                            return false;
                        }
                    }
                }
            }

            // if free poll is empty
            if (AudioEventPoolFree.Count <= 0) {
                EPriority priority = eventData.Priority;
                int idx = -1;
                // try to kill one with lowest priority - oldest are upfront
                for (int a = 0; a < AudioEventPoolUsed.Count; a++) {
                    AudioEvent ae = AudioEventPoolUsed[a];
                    // ignore instances which have outside handles
                    if ((ae.EventAttributes & EAudioEventAttributes.AutoDelete) != EAudioEventAttributes.AutoDelete) {
                        continue;
                    }
                    // if lower priority, mark index
                    if (ae.EventDataInternal.Priority < priority) {
                        priority = ae.EventDataInternal.Priority;
                        idx = a;
                    }
                }
                // if index was set, kill that instance
                if (idx != -1) {
                    AudioEvent killea = AudioEventPoolUsed[idx];
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
                    O7Log.DebugT(Tag, "CanPlay: killing event: {0}", killea.EventDataInternal.name);
#endif
                    DeleteAudioEvent(killea);
                }
            }
            //
            bool canPlay = AudioEventPoolFree.Count > 0;
#if (UNITY_EDITOR || DEVEL_BUILD) && VERBOSE
            if (!canPlay) {
                O7Log.DebugT(Tag, "CanPlay: free pool empty");
            }
#endif
            return canPlay;
        }

#if UNITY_EDITOR || DEVEL_BUILD
        public string DumpInfo() {
            InfoStringBuilder.Length = 0;
            for (int a = 0; a < ActiveAudioEvents.Count; a++) {
                AudioEvent ae = ActiveAudioEvents[a];
                InfoStringBuilder.Append(ae.ToString());
                if (a < ActiveAudioEvents.Count - 1) {
                    InfoStringBuilder.Append(';');
                }
            }
            return InfoStringBuilder.ToString();
        }
#endif

        public void Set2DPanning(Camera c, AnimationCurve acStereo, AnimationCurve acVolume) {
            Panning2DCamera = c;
            Panning2DStereoCurve = acStereo;
            Panning2DVolumeCurve = acVolume;
        }
    }
}