using System.Collections.Generic;
using Starlite.Raven.Compiler;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Starlite.Raven {

    public abstract partial class RavenMaterialPropertyBase<T> {
        protected int m_TargetMaterialPropertyId;

        private RavenTriggerPropertyBase2<T, string> m_TriggerPropertyCast;

        private RavenSequence m_Sequence;
        private bool m_IsRenderer;
        private bool m_Played = false;

        public sealed override bool IsCustom {
            get {
                return true;
            }
        }

        public sealed override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_TriggerPropertyCast = m_TriggerProperty as RavenTriggerPropertyBase2<T, string>;
            m_TargetMaterialPropertyId = Shader.PropertyToID(m_TargetMaterialProperty);
            m_IsRenderer = m_TargetComponent is Renderer;
            m_Sequence = sequence;
            if (ShouldRestoreMaterial()) {
                Action<RavenSequence> del = OnSequenceEndCallback;
                sequence.RemoveInternalDelegate(del);
                sequence.AddInternalDelegate(del);
                if (HasOverridenTargetComponents()) {
                    for (int i = 0; i < m_OverridenTargetComponents.Count; ++i) {
                        RavenMaterialTracker.RegisterRenderer(m_OverridenTargetComponents[i]);
                    }
                } else {
                    RavenMaterialTracker.RegisterRenderer(m_TargetComponent);
                }
            }

#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                if (!m_IsRenderer && !m_UseSharedMaterial) {
                    var graphic = m_TargetComponent as Graphic;
                    graphic.material = Instantiate(graphic.material);
                }
#if UNITY_EDITOR
            }
#endif
        }

        public override void OnEnter() {
            base.OnEnter();
            m_Played = true;
            if (ShouldRestoreMaterial()) {
                if (HasOverridenTargetComponents()) {
                    for (int i = 0; i < m_OverridenTargetComponents.Count; ++i) {
                        RavenMaterialTracker.BeginMaterialModification(m_OverridenTargetComponents[i]);
                    }
                } else {
                    RavenMaterialTracker.BeginMaterialModification(m_TargetComponent);
                }
            }
        }

        protected sealed override void PostEvaluateAtTime(double time, double duration, T value, UnityEngine.Object targetComponent) {
            if (m_TriggerPropertyCast != null) {
                if (!m_ExecuteFunctionOnly) {
                    SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast.ManualExecute(value, m_TargetMaterialProperty);
            } else {
                SetValue(value, targetComponent);
            }
        }

        protected sealed override bool IsCustomValid() {
            return m_TargetComponent is Renderer || m_TargetComponent is Graphic;
        }

        protected override void OnSetTargets(List<GameObject> gameObjects) {
            base.OnSetTargets(gameObjects);
            if (!(m_TargetComponent is Renderer) && !m_UseSharedMaterial) {
                for (int i = 0; i < m_OverridenTargetComponents.Count; ++i) {
                    var graphic = m_OverridenTargetComponents[i] as Graphic;
                    graphic.material = Instantiate(graphic.material);
                }
            }
        }

        protected Material GetMaterial(UnityEngine.Object targetComponent) {
            var useSharedMaterial = m_UseSharedMaterial || !Application.isPlaying;
#if UNITY_EDITOR
            if (Application.isPlaying) {
                useSharedMaterial = false;
            }
            m_TargetMaterialPropertyId = Shader.PropertyToID(m_TargetMaterialProperty);
#endif
            var material = m_IsRenderer ? (useSharedMaterial ? (targetComponent as Renderer).sharedMaterials[m_TargetMaterialIndex] : (targetComponent as Renderer).materials[m_TargetMaterialIndex]) : (targetComponent as Graphic).material;
#if !DEVEL_BUILD
            RavenAssert.IsTrue(material != null, "Material is null on {0}, sequence {1}", targetComponent, this);
#endif
            return material;
        }

        protected Material[] GetAllMaterials(UnityEngine.Object targetComponent) {
            var useSharedMaterial = m_UseSharedMaterial || !Application.isPlaying;
#if UNITY_EDITOR
            if (Application.isPlaying) {
                useSharedMaterial = false;
            }
            m_TargetMaterialPropertyId = Shader.PropertyToID(m_TargetMaterialProperty);
#endif
            return m_IsRenderer ? (useSharedMaterial ? (targetComponent as Renderer).sharedMaterials : (targetComponent as Renderer).materials) : new Material[] { (targetComponent as Graphic).material };
        }

        protected bool ShouldRestoreMaterial() {
            return m_RestoreMaterialOnEnd && !m_UseSharedMaterial && m_IsRenderer;
        }

        private void OnSequenceEndCallback(RavenSequence sequence) {
            if (m_Played) {
                m_Played = false;
                if (ShouldRestoreMaterial()) {
                    if (HasOverridenTargetComponents()) {
                        for (int i = 0; i < m_OverridenTargetComponents.Count; ++i) {
                            RavenMaterialTracker.EndMaterialModification(m_OverridenTargetComponents[i]);
                        }
                    } else {
                        RavenMaterialTracker.EndMaterialModification(m_TargetComponent);
                    }
                }
            }
        }

        private void OnDestroy() {
            if (ShouldRestoreMaterial()) {
                m_Sequence.RemoveInternalDelegate(OnSequenceEndCallback);
            }
        }
    }
}