#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataComponentBase.h>
#include <TrackComponents/Events/Base/RavenEventTriggerPoint.h>
#include <TrackComponents/Events/Base/RavenContinuousEvent.h>
#include <TrackComponents/Events/Common/RavenBookmarkEvent.h>
#include <Tracks/RavenTrackGroup.h>
#include <Tracks/RavenBookmarkTrack.h>
#include <Other/RavenAutoRegisterComponent.h>
#include <Utility/RavenUtility.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        // TODO: create all trigger points on build instead of runtime
        class RavenSequence : public SceneObjectComponent {
            SCLASS_SEALED(RavenSequence);

        public:
            class IgnoredEventEntry : public MemoryObject<> {
            public:
                void Initialize(Ptr<RavenEvent>& event, int ignoreCount);
                void Reset();

                friend bool operator==(const IgnoredEventEntry& lhs, const IgnoredEventEntry& rhs) {
                    return &lhs == &rhs;
                }

            public:
                Ptr<RavenEvent> m_Event;
                int m_IgnoreCount = 0;
            };

        public:
            RavenSequence();
            bool CanDestroyAnimationData(Ptr<RavenAnimationDataComponentBase> animationData);
            bool GetBoolParameter(int index) const;
            bool GetBoolParameter(const String& name) const;
            bool GetLoop() const;
            bool GetPlaying() const;
            bool GetPlayOnAwake() const;
            bool GetPlayOnEnable() const;
            bool GetRecording() const;
            double GetCurrentTime() const;
            double GetDuration() const;
            double GetFrameDuration() const;
            double GetFrameInterpolationTime() const;
            double GetTimeForFrame(int frame) const;
            double GetTimeScale() const;
            float GetFloatParameter(int index) const;
            float GetFloatParameter(const String& name);
            int GetCurrentFrame() const;
            int GetFps() const;
            int GetIntParameter(int index) const;
            int GetIntParameter(const String& name) const;
            int GetPreviousFrame() const;
            int GetTotalFrameCount() const;
            Int64 GetFrameForTime(double time) const;
            const List<Ref<RavenBookmarkEvent>>& GetSortedBookmarks() const;
            const List<Ref<RavenEvent>>& GetSortedEvents() const;
            const List<Ref<RavenParameter>>& GetParameters() const;
            const List<Ref<RavenTrackGroup>>& GetTrackGroups() const;
            List<Ref<SceneObject>>& GetActorListParameter(int index);
            List<Ref<SceneObject>>& GetActorListParameter(const String& name);
            Ref<Object>& GetObjectParameter(int index);
            Ref<Object>& GetObjectParameter(const String& name);
            Ref<RavenBookmarkTrack>& GetBookmarkTrack();
            RavenParameter* GetParameterAtIndex(int index);
            Vector4 GetVectorParameter(int index) const;
            Vector4 GetVectorParameter(const String& name) const;
            String FindFirstBookmarkAfter(int frame);
            String FindFirstBookmarkBefore(int frame);
            String GetSequenceName() const;
            void AddEventToSortedLists(Ref<RavenEvent>& evnt);
            void AddTrackGroup(Ref<RavenTrackGroup>& trackGroup);
            void EventChanged(Ref<RavenEvent>& evnt);
            void FlagDirty();
            void GoToBookmark(const String& bookmarkName);
            void InitializeEditor();
            void JumpToFrame(int frame);
            void JumpToTime(double time);
            void OnActivate() final;
            void OnDeactivate() final;
            void OnCreate() final;
            void OnDestroy() final;
            void OnPreUpdate(const Timer& timer) final;
            void Pause();

            SFUNCTION(Export : 0, Overload : 1, AlternateName : "Play|System.Boolean,System.Boolean");
            void Play(bool fromStart = true, bool instant = false);

            SFUNCTION(Export : 0, Overload : 1, AlternateName : "Play|System.String,System.Boolean");
            void Play(const char* bookmarkName, bool instant = false);

            void Play(const String& bookmarkName, bool instant = false);

            void RebuildSortedLists();
            void RecalculateFpsChange(int oldFps, int newFps);
            void RegisterRavenComponents(Ref<SceneObject>& targetParent);
            void RemoveEvent(Ref<RavenEvent>& evnt);
            void RemoveTrackGroup(Ref<RavenTrackGroup>& trackGroup);
            void Resume();

            SFUNCTION(Export : 0, Overload : 1, AlternateName : "SetActorListParameter|System.Int32,System.Collections.Generic.List`1[UnityEngine.GameObject]");
            void SetActorListParameter(int index, const List<Ref<SceneObject>>& value);

            SFUNCTION(Export : 0, Overload : 1, AlternateName : "SetActorListParameter|System.String,System.Collections.Generic.List`1[UnityEngine.GameObject]");
            void SetActorListParameter(const String& name, const List<Ref<SceneObject>>& value);

            void SetActorParameter(int index, const Ref<SceneObject>& value);
            void SetActorParameter(const String& name, const Ref<SceneObject>& value);
            void ClearActorListParameter(int index);
            void ClearActorListParameter(const String& name);
            void SetBookmarkTrack(Ref<RavenBookmarkTrack>& value);

            SFUNCTION(Export : 0, Overload : 1, AlternateName : "SetBoolParameter|System.Int32,System.Boolean");
            void SetBoolParameter(int index, bool value);

            SFUNCTION(Export : 0, Overload : 1, AlternateName : "SetBoolParameter|System.String,System.Boolean");
            void SetBoolParameter(const String& name, bool value);

            void SetDuration(double duration, bool reinitialize);
            void SetFloatParameter(int index, float value);
            void SetFloatParameter(const String& name, float value);
            void SetFps(int value);
            void SetIntParameter(int index, int value);
            void SetIntParameter(const String& name, int value);
            void SetLoop(bool value);
            void SetName(const String& value);
            void SetObjectParameter(int index, Ref<Object>& value);
            void SetObjectParameter(const String& name, Ref<Object>& value);
            void SetPlayOnAwake(bool value);
            void SetPlayOnEnable(bool value);
            void SetRecording(bool value);
            void SetTimeScale(double value);
            void SetVectorParameter(int index, Vector4 value);
            void SetVectorParameter(const String& name, Vector4 value);

            SFUNCTION(Export : 0);
            void Stop();

            void SwapTrackGroups(int index1, int index2);
            void TrackGroupsChanged();

        private:
            bool GetInitializedEditor() const;
            bool ShouldWarmup() const;
            int FindParameterIndexByName(const String& name) const;
            int GetAllocatedWarmupFrames() const;
            List<Ptr<RavenEventTriggerPoint>>* GetTriggerPointsForFrame(int frame, SSize_T& startIndex);
            void AddIgnoredEvent(RavenEvent* evnt, int ignoreUpdateCount);
            void CheckIfEventsDirty();
            void Clear();
            void ClearPause();
            void CreateParameterIndexToPropertyMap();
            void CustomUpdate(double deltaTime);
            void Deinitialize();
            void DeinitializeParameters();
            void EndContinuousEvents(int currentFrame, int jumpFrame, bool forceEnd);
            void Evaluate();
            void Initialize();
            void InitializeParameters();
            void OnSetActorList(Size_T parameterIndex, const List<Ref<SceneObject>>& value);
            void PauseContinuousEvents(int pauseFrame);
            void ProcessEvents(int frame, bool isSameFrame, bool isCurrentFrame, double frameInterpolationTime);
            void ProcessPropertyParameters(RavenPropertyComponent* property, int overrideParameterIndex);
            void Reinitialize();
            void SetPlaying(bool value);
            void SetTimeInternal(double time, bool calculateOverflow);
            void UpdatePropertiesWithParameters();
            void UpdateWarmup();

            static SSize_T BinarySearchIndexOfFirst(List<Ref<RavenEvent>>& list, int frame);

        public:
            static const String Tag;
            Delegate<const RavenSequence&> e_OnEndDelegate;

        private:
            SPROPERTY(Access : "private");
            bool m_Loop = false;

            SPROPERTY(Access : "private");
            bool m_PlayOnAwake = false;

            SPROPERTY(Access : "private");
            bool m_PlayOnEnable = false;

            SPROPERTY(Access : "private");
            Double m_Duration = 2.0;

            SPROPERTY(Access : "private");
            Double m_TimeScale = 1.0;

            SPROPERTY(Access : "private");
            int m_Fps = 30;

            SPROPERTY(Access : "private");
            List<Ref<RavenBookmarkEvent>> m_SortedBookmarks;

            SPROPERTY(Access : "private");
            List<Ref<RavenEvent>> m_SortedEvents;

            SPROPERTY(Access : "private");
            List<Ref<RavenParameter>> m_Parameters;

            SPROPERTY(Access : "private");
            List<Ref<RavenTrackGroup>> m_SortedTrackGroups;

            SPROPERTY(Access : "private");
            Ref<RavenBookmarkTrack> m_BookmarkTrack;

            SPROPERTY(CustomAttributes : ["UnityEngine.HideInInspector"], Access : "private");
            String m_CustomName;

            Array<bool> m_AutoRegisteredParameters;
            bool m_EventsDirty = false;
            bool m_ForceWarmup = false;
            bool m_JumpedFrame = false;
            bool m_PausedThisFrame = false;
            bool m_Playing = false;
            bool m_PlayingInstantly = false;
            bool m_Recording = false;
            bool m_StopProcessing = false;
            Double m_CurrentTime = 0;
            Double m_FrameDuration = 0;
            Double m_JumpFrameTime = 0;
            Double m_JumpFrameTimeOverflow = 0;
            Double m_PreviousTime = 0;

            HashSet<Ptr<RavenEvent>> m_IgnoredEvents;
            int m_LastProcessedFrame = -1;
            int m_LastWarmupFrame = 0;
            Size_T m_PausedTriggerPointIndex = (Size_T)-1;
            int m_TotalFrameCount = 0;
            List<Ptr<RavenContinuousEvent>> m_ActiveContinuousEvents;
            List<Ptr<RavenEvent>> m_CachedContinuousEventsForSearch;
            List<Ptr<RavenEventTriggerPoint>> m_TempTriggerPointSortListForSearch;
            List<Ptr<List<Ptr<RavenEventTriggerPoint>>>> m_TriggerPoints;
            List<Ptr<List<Ptr<RavenPropertyComponent>>>> m_ParameterIndexToPropertyMap;
            List<Ptr<RavenSequence::IgnoredEventEntry>> m_IgnoredEventsInternal;
            Ptr<RavenEvent> m_CurrentProcessingEvent;
        };
    } // namespace Raven
} // namespace Starlite
#endif
