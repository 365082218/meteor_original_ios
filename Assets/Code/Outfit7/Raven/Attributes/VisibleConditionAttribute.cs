using System;
using UnityEngine;

namespace Starlite.Raven {

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class VisibleConditionAttribute : PropertyAttribute {
        public readonly string m_DependencyStatement;

        public VisibleConditionAttribute(string dependencyStatement) {
            m_DependencyStatement = dependencyStatement;
        }
    }
}