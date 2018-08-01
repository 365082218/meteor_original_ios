#ifdef STARLITE
#include "RavenSequence.h"
#include "RavenSequence.cs"

#include <Utility/RavenLog.h>
#include <RavenOverseer.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
#define FINDPARAMETER(name)                                                                                                                                                                            \
    int idx = FindParameterIndexByName(name);                                                                                                                                                          \
    if (idx == -1) {                                                                                                                                                                                   \
        pRavenLog->ErrorT(Tag.GetCString(), "Failed to find parameter by name %s", name);                                                                                                              \
    }

        const String RavenSequence::Tag = "Raven";

        struct DoubleHax {
            union {
                Int64 m_LongValue;
                double m_DoubleValue;
            };

            DoubleHax(double value)
                : m_DoubleValue(value) {
            }

            DoubleHax(Int64 value)
                : m_LongValue(value) {
            }

            operator Int64() {
                Int64 i1 = (Int64)m_DoubleValue;
                ++m_LongValue;
                Int64 i2 = (Int64)m_DoubleValue;

                if (i1 != i2) {
                    return i2;
                }

                return i1;
            }
        };

        RavenSequence::RavenSequence() {
        }

        // At this point the event should already be destroyed!
        bool RavenSequence::CanDestroyAnimationData(Ptr<RavenAnimationDataComponentBase> animationData) {
            for (Size_T i = 0; i < m_SortedEvents.Count(); ++i) {
                auto& evnt = m_SortedEvents[i];
                if (evnt->GetAnimationDataEditorOnly() == animationData) {
                    return false;
                }
            }

            return true;
        }

        bool RavenSequence::GetBoolParameter(int index) const {
            return m_Parameters[index]->m_ValueInt != 0;
        }

        bool RavenSequence::GetBoolParameter(const String& name) const {
            FINDPARAMETER(name);
            return GetBoolParameter(idx);
        }

        bool RavenSequence::GetLoop() const {
            return m_Loop;
        }

        bool RavenSequence::GetPlaying() const {
            return m_Playing;
        }

        bool RavenSequence::GetPlayOnAwake() const {
            return m_PlayOnAwake;
        }

        bool RavenSequence::GetPlayOnEnable() const {
            return m_PlayOnEnable;
        }

        bool RavenSequence::GetRecording() const {
            return m_Recording;
        }

        double RavenSequence::GetCurrentTime() const {
            return m_CurrentTime;
        }

        double RavenSequence::GetDuration() const {
            return m_Duration;
        }

        double RavenSequence::GetFrameDuration() const {
            return m_FrameDuration;
        }

        double RavenSequence::GetFrameInterpolationTime() const {
            return (m_CurrentTime - GetCurrentFrame() * m_FrameDuration) / m_FrameDuration;
        }

        double RavenSequence::GetTimeForFrame(int frame) const {
            return frame * m_FrameDuration;
        }

        double RavenSequence::GetTimeScale() const {
            return m_TimeScale;
        }

        float RavenSequence::GetFloatParameter(int index) const {
            return m_Parameters[index]->m_ValueFloat;
        }

        float RavenSequence::GetFloatParameter(const String& name) {
            FINDPARAMETER(name);
            return GetFloatParameter(idx);
        }

        int RavenSequence::GetCurrentFrame() const {
            return (int)GetFrameForTime(m_CurrentTime);
        }

        int RavenSequence::GetFps() const {
            return m_Fps;
        }

        int RavenSequence::GetIntParameter(int index) const {
            return m_Parameters[index]->m_ValueInt;
        }

        int RavenSequence::GetIntParameter(const String& name) const {
            FINDPARAMETER(name);
            return GetIntParameter(idx);
        }

        int RavenSequence::GetPreviousFrame() const {
            return (int)GetFrameForTime(m_PreviousTime);
        }

        int RavenSequence::GetTotalFrameCount() const {
            return m_TotalFrameCount;
        }

        Int64 RavenSequence::GetFrameForTime(double time) const {
            // this works for more frames than we'll ever need (> billion)
            // if we ever need more, this can be reworked
            DoubleHax haxValue(time / m_FrameDuration);
            return haxValue;
        }

        const List<Ref<RavenBookmarkEvent>>& RavenSequence::GetSortedBookmarks() const {
            return m_SortedBookmarks;
        }

        const List<Ref<RavenEvent>>& RavenSequence::GetSortedEvents() const {
            return m_SortedEvents;
        }

        const List<Ref<RavenParameter>>& RavenSequence::GetParameters() const {
            return m_Parameters;
        }

        const List<Ref<RavenTrackGroup>>& RavenSequence::GetTrackGroups() const {
            return m_SortedTrackGroups;
        }

        List<Ref<SceneObject>>& RavenSequence::GetActorListParameter(int index) {
            return m_Parameters[index]->m_ValueGameObjectList;
        }

        List<Ref<SceneObject>>& RavenSequence::GetActorListParameter(const String& name) {
            FINDPARAMETER(name);
            return GetActorListParameter(idx);
        }

        Ref<Object>& RavenSequence::GetObjectParameter(int index) {
            return m_Parameters[index]->m_ValueObject;
        }

        Ref<Object>& RavenSequence::GetObjectParameter(const String& name) {
            FINDPARAMETER(name);
            return GetObjectParameter(idx);
        }

        Ref<RavenBookmarkTrack>& RavenSequence::GetBookmarkTrack() {
            return m_BookmarkTrack;
        }

        RavenParameter* RavenSequence::GetParameterAtIndex(int index) {
            if (index < 0 || index >= m_Parameters.Count()) {
                return nullptr;
            }

            return m_Parameters[index];
        }

        Vector4 RavenSequence::GetVectorParameter(int index) const {
            return m_Parameters[index]->m_ValueVector;
        }

        Vector4 RavenSequence::GetVectorParameter(const String& name) const {
            FINDPARAMETER(name);
            return GetVectorParameter(idx);
        }

        String RavenSequence::FindFirstBookmarkAfter(int frame) {
            for (Size_T i = 0; i < m_SortedBookmarks.Count(); ++i) {
                auto& bookmark = m_SortedBookmarks[i];
                if (bookmark->GetStartFrame() > frame) {
                    return bookmark->GetBookmarkName();
                }
            }

            return String();
        }

        String RavenSequence::FindFirstBookmarkBefore(int frame) {
            for (Size_T i = m_SortedBookmarks.Count() - 1; i < m_SortedBookmarks.Count(); --i) {
                auto& bookmark = m_SortedBookmarks[i];
                if (bookmark->GetStartFrame() < frame) {
                    return bookmark->GetBookmarkName();
                }
            }

            return String();
        }

        String RavenSequence::GetSequenceName() const {
            return m_CustomName;
        }

        void RavenSequence::AddEventToSortedLists(Ref<RavenEvent>& evnt) {
            if (evnt->GetEventType() == ERavenEventType::Bookmark) {
                m_SortedBookmarks.AddSorted(evnt, RavenEvent::Comparer);
            }

            m_SortedEvents.AddSorted(evnt, RavenEvent::Comparer);
            m_EventsDirty = true;
        }

        void RavenSequence::AddTrackGroup(Ref<RavenTrackGroup>& trackGroup) {
            m_SortedTrackGroups.Add(trackGroup);
            TrackGroupsChanged();
        }

        void RavenSequence::EventChanged(Ref<RavenEvent>& evnt) {
            if (evnt->GetEventType() == ERavenEventType::Bookmark) {
                if (m_SortedBookmarks.Remove(evnt)) {
                    m_SortedBookmarks.AddSorted(evnt, RavenBookmarkEvent::Comparer);
                }
            }

            if (m_SortedEvents.Remove(evnt)) {
                m_SortedEvents.AddSorted(evnt, RavenEvent::Comparer);
            }
            m_EventsDirty = true;
        }

        void RavenSequence::FlagDirty() {
            // recalc total duration here anyway for editor
            m_FrameDuration = 1.f / m_Fps;
            m_TotalFrameCount = (int)Math::Ceil(m_Fps * m_Duration);
            m_EventsDirty = true;
        }

        void RavenSequence::GoToBookmark(const String& bookmarkName) {
            RavenBookmarkEvent* selectedBookmark = nullptr;

            for (Size_T i = 0; i < m_SortedBookmarks.Count(); ++i) {
                auto& bookmark = m_SortedBookmarks[i];
                if (bookmark->GetBookmarkName() == bookmarkName) {
                    selectedBookmark = bookmark;
                    break;
                }
            }

            Assert(selectedBookmark != nullptr, "Bookmark %s not found in sequence!", bookmarkName.GetCString());

            JumpToFrame(selectedBookmark->GetStartFrame());
            AddIgnoredEvent(selectedBookmark, 1);
        }

        void RavenSequence::InitializeEditor() {
            if (GetInitializedEditor()) {
                CheckIfEventsDirty();
                return;
            }

            if (m_TriggerPoints.Count() == 0) {
                Initialize();
            }
            m_LastWarmupFrame = 0;
            m_ForceWarmup = true;
            UpdateWarmup();
        }

        void RavenSequence::JumpToFrame(int frame) {
            JumpToTime(GetTimeForFrame(frame));
        }

        void RavenSequence::JumpToTime(double time) {
            SetTimeInternal(time, true);
        }

        void RavenSequence::OnActivate() {
            SceneObjectComponent::OnActivate();
            if (m_PlayOnEnable) {
                Play();
            }
        }

        void RavenSequence::OnDeactivate() {
            SceneObjectComponent::OnDeactivate();
            Stop();
        }

        void RavenSequence::OnCreate() {
            SceneObjectComponent::OnCreate();
            RavenOverseer::RegisterSequence(this);
            Initialize();
            if (m_PlayOnAwake) {
                Play();
            }
        }

        void RavenSequence::OnDestroy() {
            Deinitialize();
            RavenOverseer::UnregisterSequence(this);
        }

        void RavenSequence::OnPreUpdate(const Timer& timer) {
            CustomUpdate(timer.GetDeltaTime());
        }

        void RavenSequence::Pause() {
            if (!m_Playing) {
                return;
            }

            int pauseFrame = m_LastProcessedFrame;
            PauseContinuousEvents(pauseFrame);
            m_PausedThisFrame = true;
            SetPlaying(false);
        }

        void RavenSequence::Play(bool fromStart, bool instant) {
            if (fromStart) {
                JumpToTime(0.f);
            }

            if (IsActive() || instant) {
                SetPlaying(true);
            }

            if (instant) {
                m_PlayingInstantly = true;
                m_CurrentTime = m_Duration;
                Evaluate();
            }
        }

        void RavenSequence::Play(const char* bookmarkName, bool instant) {
            GoToBookmark(bookmarkName);
            Play(false, instant);
        }

        void RavenSequence::Play(const String& bookmarkName, bool instant) {
            Play(bookmarkName.GetCString(), instant);
        }

        void RavenSequence::RebuildSortedLists() {
            m_SortedEvents.Clear();
            m_SortedBookmarks.Clear();

            for (auto& evnt : m_BookmarkTrack->GetEvents()) {
                m_SortedBookmarks.AddSorted(evnt, RavenEvent::Comparer);
                m_SortedEvents.AddSorted(evnt, RavenEvent::Comparer);
            }

            for (auto& trackGroup : m_SortedTrackGroups) {
                for (auto& evnt : trackGroup->m_PropertyTrack->GetEvents()) {
                    m_SortedEvents.AddSorted(evnt, RavenEvent::Comparer);
                }
                for (auto& evnt : trackGroup->m_AudioTrack->GetEvents()) {
                    m_SortedEvents.AddSorted(evnt, RavenEvent::Comparer);
                }
            }
            m_EventsDirty = true;
        }

        void RavenSequence::RecalculateFpsChange(int oldFps, int newFps) {
            Stop();
            Deinitialize();

            double durationFactor = (double)newFps / oldFps;
            for (auto& evnt : m_SortedEvents) {
                evnt->RecalculateFpsChange(durationFactor);
            }

            m_Fps = newFps;
            Initialize();

            m_ForceWarmup = true;
            UpdateWarmup();
        }

        void RavenSequence::RegisterRavenComponents(Ref<SceneObject>& targetParent) {
            List<Ref<RavenAutoRegisterComponent>> autoRegisterComponents;
            targetParent->FindComponentsInChildren<RavenAutoRegisterComponent>(autoRegisterComponents, true);
            if (autoRegisterComponents.Count() > 0) {
                m_AutoRegisteredParameters = Array<bool>(m_Parameters.Count());
                ;
            }
            for (auto& component : autoRegisterComponents) {
                int parameterIndex = FindParameterIndexByName(component->m_Parameter);
                if (parameterIndex >= 0) {
                    auto& parameter = m_Parameters[parameterIndex];
                    Assert(parameter->m_ParameterType == ERavenParameterType::ActorList, "Target parameter at index %d is not an actor list!", parameterIndex);

                    if (!m_AutoRegisteredParameters[parameterIndex]) {
                        parameter->ClearGameObjectList();
                        m_AutoRegisteredParameters[parameterIndex] = true;
                    }
                    parameter->AddGameObject(component->GetSceneObject());
                    // Don't call set parameter here because we'll handle that in UpdatePropertiesWithParameters all at once
                    // instead of one by one
                }
            }

            UpdatePropertiesWithParameters();
        }

        void RavenSequence::RemoveEvent(Ref<RavenEvent>& evnt) {
            if (evnt == nullptr) {
                return;
            }

            if (evnt->GetEventType() == ERavenEventType::Bookmark && m_SortedBookmarks.Remove(evnt)) {
                m_EventsDirty = true;
            }

            if (m_SortedEvents.Remove(evnt)) {
                m_EventsDirty = true;
            }

            evnt->DestroyEditor(this);
        }

        void RavenSequence::RemoveTrackGroup(Ref<RavenTrackGroup>& trackGroup) {
            if (m_SortedTrackGroups.Remove(trackGroup)) {
                TrackGroupsChanged();
            }
        }

        void RavenSequence::Resume() {
            Play(false, false);
        }

        void RavenSequence::SetActorListParameter(int index, const List<Ref<SceneObject>>& value) {
            m_Parameters[index]->SetGameObjectList(value);
            OnSetActorList(index, value);
        }

        void RavenSequence::SetActorListParameter(const String& name, const List<Ref<SceneObject>>& value) {
            FINDPARAMETER(name);
            SetActorListParameter(idx, value);
        }

        void RavenSequence::SetActorParameter(int index, const Ref<SceneObject>& value) {
            auto& parameter = m_Parameters[index];
            parameter->m_ValueGameObjectList.Clear();
            if (value) {
                parameter->m_ValueGameObjectList.Add(value);
            }
            OnSetActorList(index, parameter->m_ValueGameObjectList);
        }

        void RavenSequence::SetActorParameter(const String& name, const Ref<SceneObject>& value) {
            FINDPARAMETER(name);
            SetActorParameter(idx, value);
        }

        void RavenSequence::ClearActorListParameter(int index) {
            auto& parameter = m_Parameters[index];
            parameter->m_ValueGameObjectList.Clear();
            OnSetActorList(index, parameter->m_ValueGameObjectList);
        }

        void RavenSequence::ClearActorListParameter(const String& name) {
            FINDPARAMETER(name);
            ClearActorListParameter(idx);
        }

        void RavenSequence::SetBookmarkTrack(Ref<RavenBookmarkTrack>& value) {
            m_BookmarkTrack = value;
        }

        void RavenSequence::SetBoolParameter(int index, bool value) {
            m_Parameters[index]->SetBool(value);
        }

        void RavenSequence::SetBoolParameter(const String& name, bool value) {
            FINDPARAMETER(name);
            SetBoolParameter(idx, value);
        }

        void RavenSequence::SetDuration(double duration, bool reinitialize) {
            m_Duration = duration;
            if (reinitialize) {
                int frame = (int)GetFrameForTime(duration);
                m_Duration = GetTimeForFrame((int)frame);
                Reinitialize();
            }
        }

        void RavenSequence::SetFloatParameter(int index, float value) {
            m_Parameters[index]->SetFloat(value);
        }

        void RavenSequence::SetFloatParameter(const String& name, float value) {
            FINDPARAMETER(name);
            SetFloatParameter(idx, value);
        }

        void RavenSequence::SetFps(int value) {
            if (m_Fps == value) {
                return;
            }

            RecalculateFpsChange(m_Fps, value);
        }

        void RavenSequence::SetIntParameter(int index, int value) {
            m_Parameters[index]->SetInt(value);
        }

        void RavenSequence::SetIntParameter(const String& name, int value) {
            FINDPARAMETER(name);
            SetIntParameter(idx, value);
        }

        void RavenSequence::SetLoop(bool value) {
            m_Loop = value;
        }

        void RavenSequence::SetName(const String& value) {
            m_CustomName = value;
        }

        void RavenSequence::SetObjectParameter(int index, Ref<Object>& value) {
            m_Parameters[index]->SetObject(value);
        }

        void RavenSequence::SetObjectParameter(const String& name, Ref<Object>& value) {
            FINDPARAMETER(name);
            SetObjectParameter(idx, value);
        }

        void RavenSequence::SetPlayOnAwake(bool value) {
            m_PlayOnAwake = value;
        }

        void RavenSequence::SetPlayOnEnable(bool value) {
            m_PlayOnEnable = value;
        }

        void RavenSequence::SetRecording(bool value) {
            m_Recording = value;
        }

        void RavenSequence::SetTimeScale(double value) {
            m_TimeScale = value;
        }

        void RavenSequence::SetVectorParameter(int index, Vector4 value) {
            m_Parameters[index]->SetVector(value);
        }

        void RavenSequence::SetVectorParameter(const String& name, Vector4 value) {
            FINDPARAMETER(name);
            SetVectorParameter(idx, value);
        }

        void RavenSequence::Stop() {
            if (!m_Playing) {
                return;
            }

            EndContinuousEvents(m_LastProcessedFrame, m_LastProcessedFrame, true);
            SetTimeInternal(0.f, false);
            Clear();
            SetPlaying(false);

            DebugAssert(m_ActiveContinuousEvents.Count() == 0, "%llu active continuous events still left after Stop!", m_ActiveContinuousEvents.Count());
        }

        void RavenSequence::SwapTrackGroups(int index1, int index2) {
            std::swap(m_SortedTrackGroups[index1], m_SortedTrackGroups[index2]);
            TrackGroupsChanged();
        }

        void RavenSequence::TrackGroupsChanged() {
            for (Size_T i = 0; i < m_SortedTrackGroups.Count(); ++i) {
                m_SortedTrackGroups[i]->SetTrackIndex((int)i * RavenTrackGroup::c_TrackCount);
            }
            m_EventsDirty = true;
        }

        bool RavenSequence::GetInitializedEditor() const {
            return ShouldWarmup();
        }

        bool RavenSequence::ShouldWarmup() const {
            return m_LastWarmupFrame < m_TotalFrameCount;
        }

        int RavenSequence::FindParameterIndexByName(const String& name) const {
            for (Size_T i = 0; i < m_Parameters.Count(); ++i) {
                auto& parameter = m_Parameters[i];
                if (parameter->m_Name == name) {
                    return (int)i;
                }
            }

            return -1;
        }

        int RavenSequence::GetAllocatedWarmupFrames() const {
            int maxFrames = m_TotalFrameCount - m_LastWarmupFrame;
            return m_ForceWarmup ? maxFrames : RavenOverseer::GetAllocatedWarmupFrames(this, maxFrames);
        }

        List<Ptr<RavenEventTriggerPoint>>* RavenSequence::GetTriggerPointsForFrame(int frame, SSize_T& startIndex) {
            bool placedBarrier = false;
            if (startIndex >= 0) {
                while (startIndex < (SSize_T)m_SortedEvents.Count() && m_SortedEvents[startIndex]->GetStartFrame() == frame) {
                    auto& evnt = m_SortedEvents[startIndex];
                    auto evntPtr = Ptr<RavenEvent>(evnt.GetObject());
                    if (evnt->IsValid()) {
                        switch (evnt->GetEventType()) {
                        case ERavenEventType::Bookmark:
                            m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(frame, ERavenEventTriggerPointType::Bookmark, evntPtr), RavenEventTriggerPoint::Comparer);
                            evnt->Initialize(this);
                            break;

                        case ERavenEventType::Trigger:
                            m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(frame, ERavenEventTriggerPointType::Start, evntPtr), RavenEventTriggerPoint::Comparer);
                            evnt->Initialize(this);
                            break;

                        case ERavenEventType::Continuous:
                            m_CachedContinuousEventsForSearch.Add(evntPtr);
                            break;
                        }

                        // Add barrier if necessary
                        if (!placedBarrier && evnt->IsBarrier()) {
                            placedBarrier = true;
                            m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(frame, ERavenEventTriggerPointType::Barrier, evntPtr), RavenEventTriggerPoint::Comparer);
                        }
                    }
                    ++startIndex;
                }
            } else {
                startIndex = ~startIndex;
            }

            for (Size_T i = 0; i < m_CachedContinuousEventsForSearch.Count(); ++i) {
                RavenContinuousEvent* continuousEvent = reinterpret_cast<RavenContinuousEvent*>(m_CachedContinuousEventsForSearch[i].GetPointer());
                auto type = ERavenEventTriggerPointType::Start;
                if (continuousEvent->ShouldEndEvent(frame)) {
                    type = ERavenEventTriggerPointType::End;
                    m_CachedContinuousEventsForSearch.RemoveAt(i--);
                } else if (continuousEvent->ShouldProcessEvent(frame)) {
                    type = ERavenEventTriggerPointType::Process;
                } else {
                    // start
                    continuousEvent->Initialize(this);
                }

                auto index = m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(frame, type, continuousEvent), RavenEventTriggerPoint::Comparer);
                // we have to inject process here because we might need to interpolate in the starting frame multiple times
                // but OnEnter callback doesn't allow that
                if (type == ERavenEventTriggerPointType::Start) {
                    m_TempTriggerPointSortListForSearch.Insert(index + 1, RavenOverseer::PopTriggerPoint(frame, ERavenEventTriggerPointType::Process, continuousEvent));
                }

                // continuous events can be barriers too
                if (!placedBarrier && continuousEvent->IsBarrier()) {
                    placedBarrier = true;
                    m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(frame, ERavenEventTriggerPointType::Barrier, continuousEvent), RavenEventTriggerPoint::Comparer);
                }
            }

            // fill the frame list with the resulting trigger points
            List<Ptr<RavenEventTriggerPoint>>* triggerList = nullptr;
            if (m_TempTriggerPointSortListForSearch.Count() > 0) {
                triggerList = RavenOverseer::PopTriggerPointList();
                for (Size_T i = 0; i < m_TempTriggerPointSortListForSearch.Count(); ++i) {
                    triggerList->Add(m_TempTriggerPointSortListForSearch[i]);
                }
                m_TempTriggerPointSortListForSearch.Clear();
            }
            return triggerList;
        }

        void RavenSequence::AddIgnoredEvent(RavenEvent* evnt, int ignoreUpdateCount) {
            if (evnt == nullptr) {
                return;
            }

            auto it = m_IgnoredEvents.Find(evnt);
            if (it != m_IgnoredEvents.end()) {
                IgnoredEventEntry* existingEntry = nullptr;
                for (Size_T i = 0; i < m_IgnoredEventsInternal.Count(); ++i) {
                    auto& ignoredEventEntry = m_IgnoredEventsInternal[i];
                    if (ignoredEventEntry->m_Event.GetPointer() == evnt) {
                        existingEntry = ignoredEventEntry;
                        break;
                    }
                }
                Assert(existingEntry != nullptr, "Ignored event %s was in hashset but not in internal list!", evnt);
                existingEntry->m_IgnoreCount = ignoreUpdateCount;
            } else {
                m_IgnoredEventsInternal.Add(RavenOverseer::PopIgnoredEventEntry(evnt, ignoreUpdateCount));
                m_IgnoredEvents.Add(evnt);
            }
        }

        void RavenSequence::CheckIfEventsDirty() {
            if (m_EventsDirty) {
                bool wasPlaying = m_Playing;
                SetPlaying(false);

                m_EventsDirty = false;
                Reinitialize();

                m_ForceWarmup = true;
                UpdateWarmup();

                if (wasPlaying) {
                    Resume();
                }
            }
        }

        void RavenSequence::Clear() {
            m_LastProcessedFrame = -1;
            ClearPause();
        }

        void RavenSequence::ClearPause() {
            m_PausedThisFrame = false;
            m_PausedTriggerPointIndex = 0;
        }

        void RavenSequence::CreateParameterIndexToPropertyMap() {
            m_ParameterIndexToPropertyMap.SetSize(m_Parameters.Count());
            for (Size_T i = 0; i < m_Parameters.Count(); ++i) {
                m_ParameterIndexToPropertyMap.Add(RavenOverseer::PopPropertyComponentList());
            }

            // first do tracks because they have priority
            // we only override targets for the topmost property here
            // as they can have child properties which may be independent
            for (auto& trackGroup : m_SortedTrackGroups) {
                auto overrideParameterIndex = trackGroup->m_OverrideTargetsParameterIndex;
                auto& propertyTrack = trackGroup->m_PropertyTrack;
                if (propertyTrack != nullptr) {
                    for (auto& evnt : propertyTrack->GetEvents()) {
                        ProcessPropertyParameters(const_cast<RavenPropertyComponent*>(evnt->GetPropertyComponent()), overrideParameterIndex);
                    }
                }

                auto& audioTrack = trackGroup->m_AudioTrack;
                if (audioTrack != nullptr) {
                    for (auto& evnt : audioTrack->GetEvents()) {
                        ProcessPropertyParameters(const_cast<RavenPropertyComponent*>(evnt->GetPropertyComponent()), overrideParameterIndex);
                    }
                }
            }
        }

        void RavenSequence::CustomUpdate(double deltaTime) {
            if (!m_Playing) {
                UpdateWarmup();
                return;
            }

            deltaTime *= m_TimeScale;
            Evaluate();

            // only increment current time if we didn't forcefully stop
            if (!m_StopProcessing) {
                m_CurrentTime += deltaTime;
                if (m_CurrentTime > m_Duration) {
                    m_CurrentTime = m_Duration;
                }
            }
        }

        void RavenSequence::Deinitialize() {
            m_ActiveContinuousEvents.Clear();
            for (auto& list : m_TriggerPoints) {
                // push everything back to pool
                RavenOverseer::PushTriggerPointList(list);
            }
            m_TriggerPoints.Clear();
            DeinitializeParameters();
        }

        void RavenSequence::DeinitializeParameters() {
            for (auto& list : m_ParameterIndexToPropertyMap) {
                RavenOverseer::PushPropertyComponentList(list);
            }
            m_ParameterIndexToPropertyMap.Clear();

            for (Size_T i = 0; i < m_AutoRegisteredParameters.Size(); ++i) {
                if (m_AutoRegisteredParameters[i]) {
                    m_Parameters[i]->ClearGameObjectList();
                }
            }
            m_AutoRegisteredParameters.SetSize(0);
        }

        void RavenSequence::EndContinuousEvents(int currentFrame, int jumpFrame, bool forceEnd) {
#ifdef RAVEN_DEBUG
            // case when we call stop right after play
            if (jumpFrame == -1) {
                Assert(m_ActiveContinuousEvents.Count() == 0, "EndContinuousEvents count %llu > 0 when ending on %d frame", m_ActiveContinuousEvents.Count(), jumpFrame);
            }
#endif

            for (Size_T i = 0; i < m_ActiveContinuousEvents.Count(); ++i) {
                auto& continuousEvent = m_ActiveContinuousEvents[i];
                if (forceEnd || continuousEvent->ShouldEndEventAfterJump(currentFrame)) {
                    // reuse warmup list...
                    // we have to do this because even tho this is sorted by arrival, it doesn't guarantee order onend
                    // as an even that started later but has a higher track priority would get end executed after the one that
                    // started before
                    m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(jumpFrame, ERavenEventTriggerPointType::End, continuousEvent.GetPointer()),
                                                                  RavenEventTriggerPoint::Comparer);
                    m_ActiveContinuousEvents.RemoveAt(i--);
                }
            }

            for (auto& triggerPoint : m_TempTriggerPointSortListForSearch) {
                reinterpret_cast<RavenContinuousEvent*>(triggerPoint->GetRavenEvent().GetPointer())->OnEnd(jumpFrame);
                RavenOverseer::PushTriggerPoint(triggerPoint);
            }
            m_TempTriggerPointSortListForSearch.Clear();
        }

        void RavenSequence::Evaluate() {
            Assert(m_CurrentProcessingEvent == nullptr, "Current processing event not null! %s", m_CurrentProcessingEvent->ToString().GetCString());

            do {
                for (Size_T i = 0; i < m_IgnoredEventsInternal.Count(); ++i) {
                    auto& entry = m_IgnoredEventsInternal[i];
                    --entry->m_IgnoreCount;
                    Assert(entry->m_IgnoreCount >= 0, "Ignore count < 0!");
                }

                int previousFrame = GetPreviousFrame();
                int currentFrame = GetCurrentFrame();
                double frameInterpolationTime = GetFrameInterpolationTime();
                bool isSameFrame = m_LastProcessedFrame == previousFrame && !m_PausedThisFrame && !m_JumpedFrame;

                if (m_JumpedFrame) {
                    EndContinuousEvents(previousFrame, m_LastProcessedFrame, false);
                }

                m_JumpedFrame = false;
                m_StopProcessing = false;

                for (int frame = previousFrame; frame <= currentFrame && frame < m_TotalFrameCount; ++frame) {
#ifdef RAVEN_DEBUG
                    pRavenLog->InfoT(Tag.GetCString(), "Processing frame %d", frame);
#endif
                    bool isCurrentFrame = frame == currentFrame;
                    m_LastProcessedFrame = frame;
                    // This optimization and the one inside can produce different results when playing instantly vs not playing instantly
                    // It can also produce different results when events depend on results from continuous events (e.g. LookAt) due to frame interpolation time
                    // being different in the previous frame on every run. Perhaps invokes should sync/barrier when they're run.
                    // That means process previous frame but only continuous events on frame interpolation time 1.0
                    // We'll solve that by injecting a barrier event at the beginning of a frame in case there's a barrier requesting event in that frame
                    // However, there will still be cases when a continuous event will be processed AFTER barrier and BEFORE barrier event screwing it up slightly
                    // due to frame interpolation time
                    if (!isSameFrame || previousFrame == currentFrame) {
                        ProcessEvents(frame, isSameFrame, isCurrentFrame, frameInterpolationTime);
                    }

                    if (m_StopProcessing) {
                        if (m_JumpedFrame) {
                            m_PreviousTime = m_JumpFrameTime;
                            // If we're playing instantly, force current time to end time so we don't start processing this
                            // as if it wasn't instant
                            m_CurrentTime = m_PlayingInstantly ? m_Duration : (m_JumpFrameTime + m_JumpFrameTimeOverflow);
                        } else {
                            // Set current time to the frame we're ending on
                            m_CurrentTime = GetTimeForFrame(frame);
                        }
                        break;
                    }

                    isSameFrame = false;
                    ClearPause();
                }

                for (Size_T i = 0; i < m_IgnoredEventsInternal.Count(); ++i) {
                    auto entry = m_IgnoredEventsInternal[i];
                    if (entry->m_IgnoreCount == 0) {
                        m_IgnoredEventsInternal.RemoveAt(i--);
                        m_IgnoredEvents.Remove(entry->m_Event);
                        RavenOverseer::PushIgnoredEventEntry(entry);
                    }
                }
            } while (m_JumpedFrame && m_Playing);

            m_PreviousTime = m_CurrentTime;

            if (m_PlayingInstantly) {
                SetPlaying(false);
            }

            if (m_CurrentTime == m_Duration) {
                ProcessEvents(m_TotalFrameCount, false, true, 0);
                // don't loop again if we forcefully stopped it
                if (m_Playing) {
                    SetPlaying(m_Loop);
                }
                JumpToTime(0);
            }
        }

        void RavenSequence::Initialize() {
            m_FrameDuration = 1.0 / m_Fps;
            m_LastWarmupFrame = 0;
            m_TotalFrameCount = (int)Math::Ceil(m_Fps * m_Duration);
            m_TriggerPoints.SetSize(m_TotalFrameCount + 1);
            InitializeParameters();
            Clear();
        }

        void RavenSequence::InitializeParameters() {
            CreateParameterIndexToPropertyMap();
            UpdatePropertiesWithParameters();
        }

        void RavenSequence::OnSetActorList(Size_T parameterIndex, const List<Ref<SceneObject>>& value) {
            for (auto& property : *m_ParameterIndexToPropertyMap[parameterIndex].GetPointer()) {
                property->SetTargets(value);
            }
        }

        void RavenSequence::PauseContinuousEvents(int pauseFrame) {
            for (auto& continuousEvent : m_ActiveContinuousEvents) {
                m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(pauseFrame, ERavenEventTriggerPointType::Pause, continuousEvent.GetPointer()),
                                                              RavenEventTriggerPoint::Comparer);
            }

            for (auto& triggerPoint : m_TempTriggerPointSortListForSearch) {
                reinterpret_cast<RavenContinuousEvent*>(triggerPoint->GetRavenEvent().GetPointer())->OnPause(pauseFrame);
                RavenOverseer::PushTriggerPoint(triggerPoint);
            }
            m_TempTriggerPointSortListForSearch.Clear();
        }

        void RavenSequence::ProcessEvents(int frame, bool isSameFrame, bool isCurrentFrame, double frameInterpolationTime) {
            auto& triggerPoints = m_TriggerPoints[frame];
            if (triggerPoints == nullptr) {
                return;
            }

            for (Size_T i = m_PausedTriggerPointIndex; i < triggerPoints->Count(); ++i) {
                auto& triggerPoint = (*triggerPoints)[i];
                auto& evnt = triggerPoint->GetRavenEvent();
                if (m_IgnoredEventsInternal.Count() > 0 && m_IgnoredEvents.Find(evnt) != m_IgnoredEvents.end()) {
                    continue;
                }

                m_CurrentProcessingEvent = evnt;

                switch (triggerPoint->GetType()) {
                case ERavenEventTriggerPointType::Barrier:
                    // if we paused then we don't need to execute it since pause executes OnProcess on continuous events which is exactly the same
                    if (!isSameFrame && !m_PausedThisFrame) {
#ifdef RAVEN_DEBUG
                        pRavenLog->InfoT(Tag.GetCString(), "Executing barrier at frame %d, owner: %s", frame, evnt.GetPointer());
#endif
                        ProcessEvents(frame, true, true, 0);
#ifdef RAVEN_DEBUG
                        pRavenLog->InfoT(Tag.GetCString(), "Leaving barrier at frame %d, owner: %s", frame, evnt.GetPointer());
#endif
                    }
                    break;

                case ERavenEventTriggerPointType::Start:
                case ERavenEventTriggerPointType::Bookmark:
                    if (!isSameFrame) {
                        if (evnt->GetEventType() == ERavenEventType::Continuous) {
                            auto continuousEvent = reinterpret_cast<RavenContinuousEvent*>(evnt.GetPointer());
                            if (!continuousEvent->GetActive()) {
                                m_ActiveContinuousEvents.Add(continuousEvent);
                                evnt->OnEnter(frame);
                            }
                        } else {
                            evnt->OnEnter(frame);
                        }
                    }
                    break;

                case ERavenEventTriggerPointType::Process:
                    DebugAssert(evnt->GetEventType() == ERavenEventType::Continuous, "Not a continuous event %s", evnt->ToString().GetCString());
                    if (isCurrentFrame) {
                        auto continuousEvent = reinterpret_cast<RavenContinuousEvent*>(evnt.GetPointer());
                        if (!continuousEvent->GetActive()) {
                            m_ActiveContinuousEvents.Add(continuousEvent);
                        }
                        continuousEvent->OnProcess(frame, frameInterpolationTime);
                    }
                    break;

                case ERavenEventTriggerPointType::End:
                    DebugAssert(evnt->GetEventType() == ERavenEventType::Continuous, "Not a continuous event %s", evnt->ToString().GetCString());
                    if (!isSameFrame) {
                        auto continuousEvent = reinterpret_cast<RavenContinuousEvent*>(evnt.GetPointer());
                        if (continuousEvent->GetActive()) {
                            // remove first in case onend calls stop or something that would cause it to be processed again
                            m_ActiveContinuousEvents.Remove(continuousEvent);
                            continuousEvent->OnEnd(frame);
                        }
                    }
                    break;
                }

                m_CurrentProcessingEvent = nullptr;

                if (m_StopProcessing) {
                    if (m_PausedThisFrame) {
                        m_PausedTriggerPointIndex = i + 1;
                    }
                    break;
                }
            }
        }

        void RavenSequence::ProcessPropertyParameters(RavenPropertyComponent* property, int overrideParameterIndex) {
            if (property == nullptr) {
                return;
            }

            if (overrideParameterIndex >= 0 && property->GetParameterIndex() < 0) {
                m_ParameterIndexToPropertyMap[overrideParameterIndex]->Add(property);
            } else if (property->GetParameterIndex() >= 0) {
                m_ParameterIndexToPropertyMap[property->GetParameterIndex()]->Add(property);
            }

            // don't use track's override parameter for child properties because those are decoupled
            auto childProperty = property->GetChildPropertyComponent();
            while (childProperty != nullptr) {
                if (childProperty->GetParameterIndex() >= 0) {
                    m_ParameterIndexToPropertyMap[childProperty->GetParameterIndex()]->Add(const_cast<RavenPropertyComponent*>(childProperty));
                }
                childProperty = childProperty->GetChildPropertyComponent();
            }
        }

        void RavenSequence::Reinitialize() {
            Deinitialize();
            Initialize();
        }

        void RavenSequence::SetPlaying(bool value) {
            if (m_Playing == value) {
                return;
            }

            m_Playing = value;
            if (!m_Playing) {
                m_PlayingInstantly = false;
                m_StopProcessing = true;
                e_OnEndDelegate(*this);
                pRavenLog->InfoT(Tag.GetCString(), "Stopping sequence %s at %f", GetName().GetCString(), m_CurrentTime);
            } else {
                m_ForceWarmup = true;
                RavenOverseer::RegisterSequencePlay(this, ShouldWarmup());
                UpdateWarmup();
                Assert(m_LastWarmupFrame == m_TotalFrameCount, "Warmup not finished! %d/%d", m_LastWarmupFrame, m_TotalFrameCount);
                pRavenLog->InfoT(Tag.GetCString(), "Starting sequence %s at %f", GetName().GetCString(), m_CurrentTime);
            }
        }

        void RavenSequence::SetTimeInternal(double time, bool calculateOverflow) {
            m_JumpedFrame = true;
            m_StopProcessing = true;
            ClearPause();

            m_JumpFrameTime = time;
            if (calculateOverflow) {
                m_JumpFrameTimeOverflow = m_CurrentTime - GetTimeForFrame(m_LastProcessedFrame);
            } else {
                m_JumpFrameTimeOverflow = 0.f;
            }

            // this will get overriden only if we're in the process of executing an event
            // it will get overriden by the above in the main loop (Evaluate)

            m_CurrentTime = time;
            m_PreviousTime = time;
        }

        void RavenSequence::UpdatePropertiesWithParameters() {
            for (Size_T i = 0; i < m_ParameterIndexToPropertyMap.Count(); ++i) {
                auto& parameter = m_Parameters[i];
                if (parameter->m_ParameterType == ERavenParameterType::ActorList) {
                    OnSetActorList(i, parameter->m_ValueGameObjectList);
                }
            }
        }

        void RavenSequence::UpdateWarmup() {
            if (!ShouldWarmup()) {
                return;
            }

            int warmupFrames = GetAllocatedWarmupFrames();
            SSize_T startIndex = BinarySearchIndexOfFirst(m_SortedEvents, m_LastWarmupFrame);
            for (int i = 0; i < warmupFrames; ++i) {
                auto triggerPointList = GetTriggerPointsForFrame(m_LastWarmupFrame, startIndex);
                m_TriggerPoints.Add(triggerPointList);
                ++m_LastWarmupFrame;
            }

            if (m_LastWarmupFrame == m_TotalFrameCount) {
                List<Ptr<RavenEventTriggerPoint>>* endTriggerPoints = nullptr;
                // do we have some leftover events that weren't finished until the end?
                if (m_CachedContinuousEventsForSearch.Count() > 0) {
                    endTriggerPoints = RavenOverseer::PopTriggerPointList();
                    for (Size_T i = 0; i < m_CachedContinuousEventsForSearch.Count(); ++i) {
                        m_TempTriggerPointSortListForSearch.AddSorted(RavenOverseer::PopTriggerPoint(m_LastWarmupFrame, ERavenEventTriggerPointType::End, m_CachedContinuousEventsForSearch[i]),
                                                                      RavenEventTriggerPoint::Comparer);
                    }
                    for (Size_T i = 0; i < m_TempTriggerPointSortListForSearch.Count(); ++i) {
                        endTriggerPoints->Add(m_TempTriggerPointSortListForSearch[i]);
                    }
                    m_TempTriggerPointSortListForSearch.Clear();
                    m_CachedContinuousEventsForSearch.Clear();
                }
                m_TriggerPoints.Add(endTriggerPoints);
            }
        }

        SSize_T RavenSequence::BinarySearchIndexOfFirst(List<Ref<RavenEvent>>& list, int frame) {
            if (list.Count() == 0) {
                return ~0;
            }

            SSize_T lower = 0;
            SSize_T upper = list.Count() - 1;

            while (lower <= upper) {
                SSize_T middle = lower + (upper - lower) / 2;
                int comparisonResult = Compare(frame, list[middle]->GetStartFrame());
                if (comparisonResult == 0) {
                    SSize_T lowest = middle;
                    while (lowest > 0 && frame == list[--lowest]->GetStartFrame()) {
                        middle = lowest;
                    }
                    return middle;
                } else if (comparisonResult < 0) {
                    upper = middle - 1;
                } else {
                    lower = middle + 1;
                }
            }

            return ~lower;
        }

        void RavenSequence::IgnoredEventEntry::Initialize(Ptr<RavenEvent>& event, int ignoreCount) {
            m_Event = event;
            m_IgnoreCount = ignoreCount;
        }

        void RavenSequence::IgnoredEventEntry::Reset() {
            m_Event = nullptr;
            m_IgnoreCount = 0;
        }
    }
} // namespace Starlite

#endif