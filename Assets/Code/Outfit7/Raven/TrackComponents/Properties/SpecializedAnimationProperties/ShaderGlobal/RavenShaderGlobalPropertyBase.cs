using Starlite.Raven.Compiler;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenShaderGlobalPropertyBase<T> : RavenAnimationPropertyBase<T> {

        [SerializeField]
        protected string m_TargetShaderProperty = string.Empty;
    }
}