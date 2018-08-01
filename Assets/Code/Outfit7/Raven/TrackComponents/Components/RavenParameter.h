#ifdef STARLITE
#pragma once

#include <Enums/RavenParameterType.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenParameter : public SerializableObject {
            SCLASS_SEALED(RavenParameter);

        public:
            RavenParameter() = default;

            bool IsEnum();
            bool IsIndexer();
            void AddGameObject(Ref<SceneObject>& c);
            void ClearGameObjectList();
            void ResetTrigger();
            void SetBool(bool b);
            void SetBoolTrigger(void* userData);
            void SetFloat(float f);
            void SetGameObjectList(const List<Ref<SceneObject>>& cl);
            void SetInt(int i);
            void SetObject(Ref<Object>& c);
            void SetVector(Vector4 v);

        public:
            SPROPERTY();
            ERavenParameterType m_ParameterType = ERavenParameterType::Int;

            SPROPERTY();
            float m_ValueFloat = 0;

            SPROPERTY();
            Double m_ValueDouble = 0;

            SPROPERTY();
            int m_ValueInt = 0;

            SPROPERTY();
            List<Ref<SceneObject>> m_ValueGameObjectList;

            SPROPERTY();
            Ref<Object> m_ValueObject;

            SPROPERTY();
            String m_Name;

            SPROPERTY();
            Vector4 m_ValueVector = Vector4::Zero;
        };
    } // namespace Raven
} // namespace Starlite
#endif
