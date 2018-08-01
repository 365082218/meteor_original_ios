using Starlite.Raven.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenPropertyComponent {
        protected List<UnityEngine.Object> m_OverridenTargetComponents = null;

        private bool m_HasOverridenTargetComponents = false;

        public abstract RavenPropertyComponent ChildPropertyComponent {
            get;
        }

        public abstract int ParameterIndex {
            get;
        }

        public virtual void Initialize(RavenSequence sequence) {
        }

        public void SetTargets(List<GameObject> gameObjects) {
            // if it comes here, it will _always_ have override target components (even if null or empty)
            var wasOverriden = m_HasOverridenTargetComponents;
            m_HasOverridenTargetComponents = true;
            if (m_OverridenTargetComponents == null) {
                m_OverridenTargetComponents = new List<Object>();
            }

            if (gameObjects == null || gameObjects.Count == 0) {
                m_OverridenTargetComponents.Clear();
                return;
            }

            var sameTargets = wasOverriden && gameObjects.Count == m_OverridenTargetComponents.Count;
            var targetComponentIsGameObject = m_TargetComponent is GameObject;

            for (int i = 0; i < gameObjects.Count; ++i) {
                if (gameObjects[i] == null) {
                    RavenLog.ErrorT(RavenSequence.Tag, "Object {0}/{1} is null when setting actor list on {2}!", i, gameObjects.Count, this);
                    return;
                } else if (sameTargets) {
                    if (targetComponentIsGameObject) {
                        sameTargets &= m_OverridenTargetComponents[i] == gameObjects[i];
                    } else {
                        sameTargets &= m_OverridenTargetComponents[i] != null && (m_OverridenTargetComponents[i] as Component).gameObject == gameObjects[i];
                    }
                }
            }

            if (sameTargets) {
                return;
            }

            m_OverridenTargetComponents.Clear();
            for (int i = 0; i < gameObjects.Count; ++i) {
                var gameObject = gameObjects[i] as GameObject;
                if (targetComponentIsGameObject) {
                    m_OverridenTargetComponents.Add(gameObject);
                } else {
                    var component = gameObject.GetComponent(m_TargetComponent.GetType());
                    if (component == null) {
                        RavenAssert.IsTrue(false, "Failed to find component of type {0} on {1}!", m_TargetComponent.GetType().ToString(), gameObject);
                    }
                    m_OverridenTargetComponents.Add(component);
                }
            }

            OnSetTargets(gameObjects);
            m_HasOverridenTargetComponents = true;
        }

        public override string ToString() {
            return string.Format("{0} ({1})", base.ToString(), GetInstanceID());
        }

        public abstract void OnEnter();

        protected virtual void OnSetTargets(List<GameObject> gameObjects) {
        }

        protected bool HasOverridenTargetComponents() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return false;
            }
#endif
            return m_HasOverridenTargetComponents;
        }
    }
}