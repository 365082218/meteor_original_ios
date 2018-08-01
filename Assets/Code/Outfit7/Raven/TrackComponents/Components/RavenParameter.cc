#ifdef STARLITE
#include "RavenParameter.h"
#include "RavenParameter.cs"

namespace Starlite {
    namespace Raven {

        bool RavenParameter::IsEnum() {
            return m_ParameterType == ERavenParameterType::Enum || m_ParameterType == ERavenParameterType::EnumTrigger || m_ParameterType == ERavenParameterType::EnumBitMask;
        }

        bool RavenParameter::IsIndexer() {
            return m_ParameterType == ERavenParameterType::Int || m_ParameterType == ERavenParameterType::IntTrigger || m_ParameterType == ERavenParameterType::Enum ||
                   m_ParameterType == ERavenParameterType::EnumTrigger || m_ParameterType == ERavenParameterType::EnumBitMask;
        }

        void RavenParameter::AddGameObject(Ref<SceneObject>& c) {
            m_ValueGameObjectList.Add(c);
        }

        void RavenParameter::ClearGameObjectList() {
            m_ValueGameObjectList.Clear();
        }

        void RavenParameter::ResetTrigger() {
            m_ValueFloat = 0.0f;
            m_ValueInt = 0;
        }

        void RavenParameter::SetBool(bool b) {
            m_ValueInt = b ? 1 : 0;
        }

        void RavenParameter::SetBoolTrigger(void* userData) {
            SetBool(true);
        }

        void RavenParameter::SetFloat(float f) {
            m_ValueFloat = f;
        }

        void RavenParameter::SetGameObjectList(const List<Ref<SceneObject>>& cl) {
            m_ValueGameObjectList = cl;
        }

        void RavenParameter::SetInt(int i) {
            m_ValueInt = i;
        }

        void RavenParameter::SetObject(Ref<Object>& c) {
            m_ValueObject = c;
        }

        void RavenParameter::SetVector(Vector4 v) {
            m_ValueVector = v;
        }
    } // namespace Raven
} // namespace Starlite
#endif