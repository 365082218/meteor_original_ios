using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Starlite.Raven.Compiler {

    /// <summary>
    /// This goes through all the sequences and maps function and component names to custom functions that create delegates that drive an object property.
    /// </summary>
    public class RavenCodeGenerator {
        private const string c_FunctionJumpTableName = "s_FunctionJumpTable";
        private const string c_GetDelegateName = "RavenPropertyGetter";
        private const string c_SetDelegateName = "RavenPropertySetter";
        private const string c_CallDelegateName = "RavenFunctionCall";
        private const string c_FunctionCallPropertyType = RavenEditorUtility.c_RavenCompilerNamespace + ".RavenTriggerPropertyBase";
        private const string c_FunctionCallPropertyTypeBaseType = RavenEditorUtility.c_RavenNamespace + ".RavenTriggerPropertyComponentBase";
        private const string c_FunctionCallClassName = "RavenTriggerPropertySpecialization";
        private const string c_SpecializedFunctionClassesFolder = "SpecializedClasses";
        private const string c_Indent = "    ";
        private const string c_NewLine = "\r\n";
#if STARLITE_EDITOR
        private const string c_GeneratedHeaderFile = "#ifdef STARLITE\n#pragma once\n\n#include <TrackComponents/Properties/Base/RavenTriggerPropertyComponentBase.h>\n#include <RavenSequence.h>\n\nnamespace Starlite {\n    namespace Raven {\n        namespace Compiler {\n            class RavenTriggerPropertyBase0 : public RavenTriggerPropertyComponentBase {\n                SCLASS_ABSTRACT(RavenTriggerPropertyBase0);\n\n            public:\n                RavenTriggerPropertyBase0() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), ScopedArray<InvokeParameter>());\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), ScopedArray<InvokeParameter>());\n                    }\n                }\n\n                void ManualExecute() {\n                    OnEnter();\n                }\n\n                bool OnBuild(int pass) final {\n                    return RavenTriggerPropertyComponentBase::OnBuild(pass);\n                }\n            };\n\n            template <class T0> class RavenTriggerPropertyBase1 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase1);\n\n            public:\n                RavenTriggerPropertyBase1() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0) {\n                    FixedArray<InvokeParameter, 1> parameterList{value0};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                FixedArray<InvokeParameter, 1> m_ParameterList;\n            };\n\n            template <class T0, class T1> class RavenTriggerPropertyBase2 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase2);\n\n            public:\n                RavenTriggerPropertyBase2() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1) {\n                    FixedArray<InvokeParameter, 2> parameterList{value0, value1};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                FixedArray<InvokeParameter, 2> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2> class RavenTriggerPropertyBase3 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase3);\n\n            public:\n                RavenTriggerPropertyBase3() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2) {\n                    FixedArray<InvokeParameter, 3> parameterList{value0, value1, value2};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                FixedArray<InvokeParameter, 3> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3> class RavenTriggerPropertyBase4 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase4);\n\n            public:\n                RavenTriggerPropertyBase4() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3) {\n                    FixedArray<InvokeParameter, 4> parameterList{value0, value1, value2, value3};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                FixedArray<InvokeParameter, 4> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3, class T4> class RavenTriggerPropertyBase5 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase5);\n\n            public:\n                RavenTriggerPropertyBase5() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4) {\n                    FixedArray<InvokeParameter, 5> parameterList{value0, value1, value2, value3, value4};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    m_ParameterList[4] = m_Value4;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                SPROPERTY(Access : \"private\");\n                T4 m_Value4;\n\n                FixedArray<InvokeParameter, 5> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3, class T4, class T5> class RavenTriggerPropertyBase6 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase6);\n\n            public:\n                RavenTriggerPropertyBase6() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) {\n                    FixedArray<InvokeParameter, 6> parameterList{value0, value1, value2, value3, value4, value5};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    m_ParameterList[4] = m_Value4;\n                    m_ParameterList[5] = m_Value5;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                SPROPERTY(Access : \"private\");\n                T4 m_Value4;\n\n                SPROPERTY(Access : \"private\");\n                T5 m_Value5;\n\n                FixedArray<InvokeParameter, 6> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3, class T4, class T5, class T6> class RavenTriggerPropertyBase7 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase7);\n\n            public:\n                RavenTriggerPropertyBase7() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6) {\n                    FixedArray<InvokeParameter, 7> parameterList{value0, value1, value2, value3, value4, value5, value6};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    m_ParameterList[4] = m_Value4;\n                    m_ParameterList[5] = m_Value5;\n                    m_ParameterList[6] = m_Value6;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                SPROPERTY(Access : \"private\");\n                T4 m_Value4;\n\n                SPROPERTY(Access : \"private\");\n                T5 m_Value5;\n\n                SPROPERTY(Access : \"private\");\n                T6 m_Value6;\n\n                FixedArray<InvokeParameter, 7> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3, class T4, class T5, class T6, class T7> class RavenTriggerPropertyBase8 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase8);\n\n            public:\n                RavenTriggerPropertyBase8() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7) {\n                    FixedArray<InvokeParameter, 8> parameterList{value0, value1, value2, value3, value4, value5, value6, value7};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    m_ParameterList[4] = m_Value4;\n                    m_ParameterList[5] = m_Value5;\n                    m_ParameterList[6] = m_Value6;\n                    m_ParameterList[7] = m_Value7;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                SPROPERTY(Access : \"private\");\n                T4 m_Value4;\n\n                SPROPERTY(Access : \"private\");\n                T5 m_Value5;\n\n                SPROPERTY(Access : \"private\");\n                T6 m_Value6;\n\n                SPROPERTY(Access : \"private\");\n                T7 m_Value7;\n\n                FixedArray<InvokeParameter, 8> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3, class T4, class T5, class T6, class T7, class T8> class RavenTriggerPropertyBase9 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase9);\n\n            public:\n                RavenTriggerPropertyBase9() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8) {\n                    FixedArray<InvokeParameter, 9> parameterList{value0, value1, value2, value3, value4, value5, value6, value7, value8};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    m_ParameterList[4] = m_Value4;\n                    m_ParameterList[5] = m_Value5;\n                    m_ParameterList[6] = m_Value6;\n                    m_ParameterList[7] = m_Value7;\n                    m_ParameterList[8] = m_Value8;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                SPROPERTY(Access : \"private\");\n                T4 m_Value4;\n\n                SPROPERTY(Access : \"private\");\n                T5 m_Value5;\n\n                SPROPERTY(Access : \"private\");\n                T6 m_Value6;\n\n                SPROPERTY(Access : \"private\");\n                T7 m_Value7;\n\n                SPROPERTY(Access : \"private\");\n                T8 m_Value8;\n\n                FixedArray<InvokeParameter, 9> m_ParameterList;\n            };\n\n            template <class T0, class T1, class T2, class T3, class T4, class T5, class T6, class T7, class T8, class T9> class RavenTriggerPropertyBase10 : public RavenTriggerPropertyComponentBase {\n                SCLASS_TEMPLATE_ABSTRACT(RavenTriggerPropertyBase10);\n\n            public:\n                RavenTriggerPropertyBase10() = default;\n\n                void OnEnter() final {\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), m_ParameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), m_ParameterList);\n                    }\n                }\n\n                void ManualExecute(T0 value0, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9) {\n                    FixedArray<InvokeParameter, 10> parameterList{value0, value1, value2, value3, value4, value5, value6, value7, value8, value9};\n                    if (HasOverridenTargetComponents()) {\n                        for (auto& component : m_OverridenTargetComponents) {\n                            m_Function->Invoke(component.GetObject(), parameterList);\n                        }\n                    } else {\n                        m_Function->Invoke(m_TargetComponent.GetObject(), parameterList);\n                    }\n                }\n\n                bool OnBuild(int pass) final {\n                    if (!RavenTriggerPropertyComponentBase::OnBuild(pass)) {\n                        return false;\n                    }\n\n                    m_ParameterList[0] = m_Value0;\n                    m_ParameterList[1] = m_Value1;\n                    m_ParameterList[2] = m_Value2;\n                    m_ParameterList[3] = m_Value3;\n                    m_ParameterList[4] = m_Value4;\n                    m_ParameterList[5] = m_Value5;\n                    m_ParameterList[6] = m_Value6;\n                    m_ParameterList[7] = m_Value7;\n                    m_ParameterList[8] = m_Value8;\n                    m_ParameterList[9] = m_Value9;\n                    return true;\n                }\n\n            protected:\n                SPROPERTY(Access : \"private\", CustomAttributes : [\"UnityEngine.Header(\\\"Values\\\")\"]);\n                T0 m_Value0;\n\n                SPROPERTY(Access : \"private\");\n                T1 m_Value1;\n\n                SPROPERTY(Access : \"private\");\n                T2 m_Value2;\n\n                SPROPERTY(Access : \"private\");\n                T3 m_Value3;\n\n                SPROPERTY(Access : \"private\");\n                T4 m_Value4;\n\n                SPROPERTY(Access : \"private\");\n                T5 m_Value5;\n\n                SPROPERTY(Access : \"private\");\n                T6 m_Value6;\n\n                SPROPERTY(Access : \"private\");\n                T7 m_Value7;\n\n                SPROPERTY(Access : \"private\");\n                T8 m_Value8;\n\n                SPROPERTY(Access : \"private\");\n                T9 m_Value9;\n\n                FixedArray<InvokeParameter, 10> m_ParameterList;\n            };\n        } // namespace Compiler\n    }     // namespace Raven\n} // namespace Starlite\n#endif";
        private const string c_GeneratedCCFile = "#ifdef STARLITE\n#include \"RavenTriggerPropertyBase.h\"\n#include \"RavenTriggerPropertyBase.cs\"\n#endif";

        private class StarliteCodeCompileUnit {
            public string m_TypeName;
            public StringBuilder m_HeaderContents = new StringBuilder(512);
            public StringBuilder m_CCContents = new StringBuilder(256);
        }
#endif

        public Action<string, int, int> e_OnSceneBeingProcessed;

        private CodeNamespace m_CodeNamespace;
        private CodeNamespace m_CodeNamespacePropertyCompiler;
        private CodeCompileUnit m_CodeCompileUnit;
        private List<object> m_CodeCompileUnitsForSpecializedFunctionClasses;
        private List<object> m_CodeCompileUnitsForBaseFunctionClasses;
        private string m_TargetFolder;

        private Dictionary<ulong, FunctionDeclaration[]> m_TargetHashToFuncMap = new Dictionary<ulong, FunctionDeclaration[]>();
        private Dictionary<string, string> m_TargetClassSpecializationToNameMap = new Dictionary<string, string>();
        private readonly Dictionary<ulong, string> m_TargetHashToNameMap = new Dictionary<ulong, string>();
        private readonly Dictionary<string, ulong> m_NameToTargetHashMap = new Dictionary<string, ulong>();

        private bool m_ReserializeProperties;
        private bool m_SceneUpdate;
        private bool m_ValidateProperties;
        private bool m_DumpFunctionInfo;
        private bool m_RemakeEverything;
        private RavenPropertyComponent m_PropertyToUpdate;
        private List<int> m_GeneratedClassTemplates;
        private string m_OpenScene;
        private List<string> m_Info;
        private List<string> m_Warnings;

        public RavenCodeGenerator(bool reserializeProperties, bool sceneUpdate, RavenPropertyComponent property, bool validateProperties, bool dumpFunctionInfo, bool remakeEverything) {
            m_CodeCompileUnitsForSpecializedFunctionClasses = new List<object>();
            m_CodeCompileUnitsForBaseFunctionClasses = new List<object>();

            m_ReserializeProperties = reserializeProperties;
            m_SceneUpdate = sceneUpdate;
            m_PropertyToUpdate = property;
            m_ValidateProperties = validateProperties;
            m_DumpFunctionInfo = dumpFunctionInfo;
            m_RemakeEverything = remakeEverything;

            m_OpenScene = "Prefabs";
            m_GeneratedClassTemplates = new List<int>();
            m_Info = new List<string>();
            m_Warnings = new List<string>();
        }

        public string[] Run(string file, out string[] warnings, out string[] info) {
            var files = new List<string>() {
                file
            };
            m_TargetFolder = Path.GetDirectoryName(file);

            m_CodeNamespace = new CodeNamespace();
            m_CodeNamespacePropertyCompiler = new CodeNamespace(RavenEditorUtility.c_RavenCompilerNamespace);

            m_CodeCompileUnit = new CodeCompileUnit();
            m_CodeCompileUnit.Namespaces.Add(m_CodeNamespace);
            m_CodeCompileUnit.Namespaces.Add(m_CodeNamespacePropertyCompiler);

            GenerateClass(Path.GetFileNameWithoutExtension(file).TrimEnd('_'));

            GenerateSourceCode(file, m_CodeCompileUnit);

            var specializedClassesFolder = m_TargetFolder + "/" + c_SpecializedFunctionClassesFolder + "/";
            if (!Directory.Exists(specializedClassesFolder)) {
                Directory.CreateDirectory(specializedClassesFolder);
            }
            for (int i = 0; i < m_CodeCompileUnitsForSpecializedFunctionClasses.Count; ++i) {
#if STARLITE_EDITOR
                var codeCompileUnit = m_CodeCompileUnitsForSpecializedFunctionClasses[i] as StarliteCodeCompileUnit;
                var extraFile = specializedClassesFolder + codeCompileUnit.m_TypeName + ".h";
                var extraFile2 = specializedClassesFolder + codeCompileUnit.m_TypeName + ".cc";

                File.WriteAllText(extraFile, codeCompileUnit.m_HeaderContents.ToString());
                File.WriteAllText(extraFile2, codeCompileUnit.m_CCContents.ToString());
                Starlite.SourceCC.Format(extraFile);
                Starlite.SourceCC.Format(extraFile2);

                files.Add(extraFile);
                files.Add(extraFile2);
#else
                var codeCompileUnit = m_CodeCompileUnitsForSpecializedFunctionClasses[i] as CodeCompileUnit;
                var extraFile = specializedClassesFolder + codeCompileUnit.Namespaces[0].Types[0].Name + ".cs";
                GenerateSourceCode(extraFile, codeCompileUnit);
                files.Add(extraFile);
#endif
            }
            for (int i = 0; i < m_CodeCompileUnitsForBaseFunctionClasses.Count; ++i) {
                var codeCompileUnit = m_CodeCompileUnitsForBaseFunctionClasses[i] as CodeCompileUnit;
                var extraFile = m_TargetFolder + "/" + RavenUtility.GetTypeWithoutNamespace(c_FunctionCallPropertyType) + ".logic.cs";
#if STARLITE_EDITOR
                var headerFile = extraFile.Replace(".logic.cs", ".h");
                var ccFile = extraFile.Replace(".logic.cs", ".cc");
                File.WriteAllText(headerFile, c_GeneratedHeaderFile);
                File.WriteAllText(ccFile, c_GeneratedCCFile);
                Starlite.SourceCC.Format(headerFile);
                Starlite.SourceCC.Format(ccFile);
#endif
                GenerateSourceCode(extraFile, codeCompileUnit);
                files.Add(extraFile);
            }

            CleanupOldClasses();
            info = m_Info.ToArray();
            warnings = m_Warnings.ToArray();
            return files.ToArray();
        }

        private CodeNamespace GenerateCodeCompileUnitForSoloClass(List<object> codeCompileUnits) {
            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(RavenEditorUtility.c_RavenCompilerNamespace);
            compileUnit.Namespaces.Add(codeNamespace);

            codeCompileUnits.Add(compileUnit);

            return codeNamespace;
        }

        private void GenerateClass(string className) {
            var klass = m_CodeNamespacePropertyCompiler.DeclareClass(className);
            klass.Attributes |= MemberAttributes.Static;

            GenerateBaseClasses();
            FindPropertiesAndAssignIDs(klass);
            GenerateFields(klass);
            GenerateReflectionMethods(klass);
            GenerateConfigurePropertyPropertyFunction(klass);
            GenerateConfigureTriggerPropertyFunctions(klass);
            DumpFunctionObjects(klass);
        }

        private void GenerateBaseClasses() {
            var nameSpace = GenerateCodeCompileUnitForSoloClass(m_CodeCompileUnitsForBaseFunctionClasses);
            for (int i = 0; i <= RavenUtility.c_MaxFunctionParameters; ++i) {
                GenerateBaseGenericCallerDataClass(i, nameSpace);
            }
        }

        private void GenerateFields(CodeTypeDeclaration klass) {
            klass.DeclareMember("#if UNITY_EDITOR");
            klass.DeclareMember("#region Compiler Maps");
            klass.DeclareMember("public static readonly Dictionary<string, ulong> s_NameToTargetHashMap = new Dictionary<string, ulong>(){2}{1}{{{2}{0}{1}}};", DumpNameToTargetHashMap(), MultiplyString(c_Indent, 2), c_NewLine);
            klass.DeclareMember("public static readonly Dictionary<ulong, FunctionDeclaration[]> s_TargetHashToFuncMap = new Dictionary<ulong, FunctionDeclaration[]>(){2}{1}{{{2}{0}{1}}};", DumpTargetHashToFuncMap(), MultiplyString(c_Indent, 2), c_NewLine);
            klass.DeclareMember("public static readonly Dictionary<string, string> s_TargetClassSpecializationToNameMap = new Dictionary<string, string>(){2}{1}{{{2}{0}{1}}};", DumpTargetClassSpecializationToIntMap(), MultiplyString(c_Indent, 2), c_NewLine);
            klass.DeclareMember("#endregion Compiler Maps");
            klass.DeclareMember("#endif");
            GenerateFunctionJumpTable(klass);
        }

        private void GenerateReflectionMethods(CodeTypeDeclaration klass) {
            GenerateEditorCacheFunction(klass);
            GenerateEditorSpecializedTypeCacheFunction(klass);
        }

        private void GenerateEditorCacheFunction(CodeTypeDeclaration klass) {
            klass.DeclareMethod(typeof(bool), "HasInDatabase", (x) => {
                x.Attributes |= MemberAttributes.Static;
                x.DeclareParameter(typeof(string).ToString(), "targetObject");
                x.DeclareParameter(typeof(string).ToString(), "targetProperty");
                var paramOut1 = new CodeParameterDeclarationExpression(typeof(ulong).ToString(), "outHash");
                paramOut1.Direction = FieldDirection.Out;
                x.Parameters.Add(paramOut1);
                x.Statements.Expr("outHash = RavenUtility.s_InvalidHash");
                x.Statements.IfDef("UNITY_EDITOR",
                    (stmts0) => {
                        stmts0.Expr("var stitchedName = RavenUtility.StitchTypeAndMember(targetObject, targetProperty)");
                        stmts0.IfElse("s_NameToTargetHashMap.TryGetValue(stitchedName, out outHash)".Expr(),
                             (stmts) => {
                                 stmts.Expr("return true");
                             },
                             (stmtsElse) => {
                                 stmtsElse.Expr("outHash = RavenUtility.s_InvalidHash");
                             });
                    });
                x.Statements.Expr("return false");
            });
        }

        private void GenerateEditorSpecializedTypeCacheFunction(CodeTypeDeclaration klass) {
            klass.DeclareMethod(typeof(bool), "HasPropertySpecialization", (x) => {
                x.Attributes |= MemberAttributes.Static;
                x.DeclareParameter(typeof(string).ToString(), "packedTypes");
                var paramOut1 = new CodeParameterDeclarationExpression(typeof(Type).ToString(), "outSpecializationType");
                paramOut1.Direction = FieldDirection.Out;
                x.Parameters.Add(paramOut1);
                x.Statements.Expr("outSpecializationType = null");
                x.Statements.IfDef("UNITY_EDITOR",
                    (stmts) => {
                        stmts.Expr("string type");
                        stmts.If("s_TargetClassSpecializationToNameMap.TryGetValue(packedTypes, out type)".Expr(),
                            (stmtsTrue) => {
                                stmtsTrue.Expr("outSpecializationType = RavenUtility.GetTypeFromLoadedAssemblies(type)");
                                stmtsTrue.Expr("return true");
                            });
                    });
                x.Statements.Expr("return false");
            });
        }

        private void FindPropertiesAndAssignIDs(CodeTypeDeclaration klass) {
            if (ShouldProcessAssets()) {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    throw new Exception("User cancelled saving scene(s).");
                }

                if (!m_RemakeEverything) {
                    RegenerateProperties(klass);
                    RegenerateFunctions(klass);
                    RegenerateClasses();
                }

                var exclusionFilters = RavenPreferences.GetExclusionFilters();
                var scenes = new List<string>();
                FindAllScenesInProject(Application.dataPath, scenes, exclusionFilters);

                CallSceneBeingProcessedDelegate(0, scenes.Count + 1);

                var propertyGUIDs = AssetDatabase.FindAssets("t:gameobject");
                foreach (var guid in propertyGUIDs) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    for (int i = 0; i < exclusionFilters.Length; ++i) {
                        if (assetPath.ToLowerInvariant().Contains(exclusionFilters[i])) {
                            continue;
                        }
                    }
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    var properties = go.GetComponentsInChildren<RavenPropertyComponent>();
                    foreach (var property in properties) {
                        ProcessProperty(property, klass);
                    }
                }

                var currentSceneSetup = EditorSceneManager.GetSceneManagerSetup();

                for (int i = 0; i < scenes.Count; ++i) {
                    var scene = EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Single);
                    bool didWork = false;
                    if (scene.isLoaded) {
                        m_OpenScene = scenes[i];
                        CallSceneBeingProcessedDelegate(i + 1, scenes.Count + 1);
                        var properties = Resources.FindObjectsOfTypeAll<RavenPropertyComponent>();
                        foreach (var property in properties) {
                            didWork |= ProcessProperty(property, klass);
                        }

                        if (didWork) {
                            m_Info.Add(string.Format("Work done in scene {0}.", scene.path));
                            EditorSceneManager.SaveScene(scene);
                        }
                    } else {
                        throw new Exception(string.Format("Failed opening scene {0}!", scenes[i]));
                    }
                }

                if (currentSceneSetup.Length == 0) {
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                } else {
                    EditorSceneManager.RestoreSceneManagerSetup(currentSceneSetup);
                }
            } else {
                m_OpenScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                RegenerateProperties(klass);
                RegenerateFunctions(klass);
                RegenerateClasses();
                if (ShouldProcessCurrentScene()) {
                    var properties = Resources.FindObjectsOfTypeAll<RavenPropertyComponent>();
                    foreach (var property in properties) {
                        ProcessProperty(property, klass);
                    }
                } else {
                    // property to update can't be null here
                    ProcessProperty(m_PropertyToUpdate, klass);
                }
            }
        }

        private void RegenerateProperties(CodeTypeDeclaration klass) {
            foreach (var kvp in PropertyReflectionCompiledOutput.s_NameToTargetHashMap) {
                m_NameToTargetHashMap[kvp.Key] = kvp.Value;
                m_TargetHashToNameMap[kvp.Value] = kvp.Key;
            }

            foreach (var kvp in PropertyReflectionCompiledOutput.s_TargetHashToFuncMap) {
                m_TargetHashToFuncMap[kvp.Key] = kvp.Value;
                // invalidate all imported functions here because we're just copying the references
                // so we're actually flagging these objects when validating them
                foreach (var func in kvp.Value) {
                    func.SetValidated(false);
                }
            }
        }

        private bool ProcessProperty(RavenPropertyComponent property, CodeTypeDeclaration klass) {
            if (property is RavenAnimationPropertyComponentBase) {
                var propProperty = property as RavenAnimationPropertyComponentBase;
                if (propProperty.IsCustom) {
                    return false;
                }
                RecordObject(property);
                return ProcessPropertyProperty(propProperty, klass);
            } else if (property is RavenTriggerPropertyComponentBase) {
                RecordObject(property);
                var trigProperty = property as RavenTriggerPropertyComponentBase;
                return ProcessTrigger(trigProperty, klass);
            }

            return false;
        }

        private bool ProcessPropertyProperty(RavenAnimationPropertyComponentBase propProperty, CodeTypeDeclaration klass) {
            if (string.IsNullOrEmpty(propProperty.MemberName)) {
                return false;
            }

            bool didWork = false;
            if (ShouldReserializeProperties()) {
                UpdateSerializedProperty(propProperty, true);
            }

            if (propProperty.TargetComponent != null && !string.IsNullOrEmpty(propProperty.MemberName)) {
                var componentType = propProperty.TargetComponent.GetType();
                var argType = propProperty.GetType().BaseType.GetGenericArguments()[0];
                if (ValidateProperty(propProperty.MemberName, componentType, argType, propProperty)) {
                    var baseType = propProperty.ComponentBaseType.ToString();
                    var baseTypeName = baseType.ToString();
                    var hash = GetHashForComponentAndMember(baseTypeName, propProperty.MemberName);
                    if (ProcessTypeAndMember(baseTypeName, propProperty.MemberName, hash)) {
                        GenerateFunctions(klass, baseTypeName, argType.ToString(), propProperty.MemberName, hash);
                    }

                    const int nFuncs = 2;
                    var funcs = m_TargetHashToFuncMap[hash];
                    for (int i = 0; i < nFuncs; ++i) {
                        var func = funcs[i];
                        func.AddResponsibleGameObject(propProperty, m_OpenScene);
                    }
                }
            }

            didWork |= UpdateSerializedProperty(propProperty);
            return didWork;
        }

        private bool ProcessTrigger(RavenTriggerPropertyComponentBase trigProperty, CodeTypeDeclaration klass) {
            bool didWork = false;
            if (ShouldReserializeProperties()) {
                didWork |= UpdateSerializedProperty(trigProperty, true);
            }

            const int nFuncs = 1;
            if (trigProperty.TargetComponent != null && !string.IsNullOrEmpty(trigProperty.FunctionName)) {
                var componentType = trigProperty.TargetComponent.GetType();
                if (ValidateFunction(trigProperty.FunctionName, componentType, trigProperty)) {
                    var baseType = trigProperty.ComponentBaseType.ToString();
                    var baseTypeName = baseType.ToString();
                    var hash = GetHashForComponentAndMember(baseTypeName, trigProperty.FunctionName);
                    if (ProcessTypeAndMember(baseTypeName, trigProperty.FunctionName, hash)) {
                        GenerateCaller(klass, baseTypeName, RavenUtility.GetFunctionParametersFromPackedFunctionName(trigProperty.FunctionName), trigProperty.FunctionName, hash);
                    }

                    var funcs = m_TargetHashToFuncMap[hash];
                    for (int i = 0; i < nFuncs; ++i) {
                        var func = funcs[i];
                        func.AddResponsibleGameObject(trigProperty, m_OpenScene);
                    }
                }
            }

            didWork |= UpdateSerializedProperty(trigProperty);
            return didWork;
        }

        /// <summary>
        /// Returns true if it's a new type:member pair and function generation required.
        /// </summary>
        private bool ProcessTypeAndMember(string type, string member, ulong hash) {
            var stitchedName = RavenUtility.StitchTypeAndMember(type, member);
            if (!m_TargetHashToNameMap.ContainsKey(hash)) {
                m_TargetHashToNameMap[hash] = stitchedName;
                m_NameToTargetHashMap[stitchedName] = hash;

                return true;
            }

            RavenAssert.IsTrue(m_TargetHashToNameMap[hash] == stitchedName, "{0} HASH COLLISION WITH {1}", m_TargetHashToNameMap[hash], stitchedName);

            return false;
        }

        private bool UpdateSerializedProperty(RavenAnimationPropertyComponentBase propProperty, bool forceUpdate = false) {
            if (propProperty.Target == null) {
                return false;
            }

            // it is important to change both managed and native data and then call applymodifiedproperties
            // else it doesn't work

            bool didWork = false;
            var serializedObj = new UnityEditor.SerializedObject(propProperty as UnityEngine.Object);
            ulong hash = RavenUtility.s_InvalidHash;
            if (forceUpdate) {
                // this can happen when rebuilding old projects
                // so just force update it here
                var type = RavenUtility.GetTypeFromLoadedAssemblies(propProperty.ComponentType);
                UnityEngine.Object comp = null;
                if (type != null) {
                    if (type == typeof(GameObject)) {
                        comp = propProperty.Target;
                    } else {
                        comp = propProperty.Target.GetComponent(type);
                    }
                } else {
                    m_Warnings.Add(string.Format("Component type {0} does not exist. It's used on object {1} ({2}) in scene {3}.", propProperty.ComponentType, propProperty, propProperty.GetInstanceID(), m_OpenScene));
                }
                var sp1 = serializedObj.FindProperty("m_TargetComponent");
                if (sp1.objectReferenceValue != comp) {
                    propProperty.TargetComponent = comp;
                    sp1.objectReferenceValue = comp;
                    didWork = true;
                }
            } else if (propProperty.TargetComponent != null) {
                var baseTypeName = propProperty.ComponentBaseType.ToString();
                try {
                    hash = m_NameToTargetHashMap[RavenUtility.StitchTypeAndMember(baseTypeName, propProperty.MemberName)];
                } catch (Exception) {
                    hash = RavenUtility.s_InvalidHash;
                }

                // only update object when we're not reserializing because we'd always do work otherwise and that's not what we want
                var sp2 = serializedObj.FindProperty("m_TargetHash");

                if ((ulong)sp2.longValue != hash) {
                    propProperty.TargetHash = hash;
                    sp2.longValue = (long)hash;
                    didWork = true;
                }
            }

            if (didWork) {
                serializedObj.ApplyModifiedProperties();
            }

            return didWork;
        }

        private bool UpdateSerializedProperty(RavenTriggerPropertyComponentBase trigProperty, bool forceUpdate = false) {
            if (trigProperty.Target == null) {
                return false;
            }

            bool didWork = false;
            var serializedObj = new UnityEditor.SerializedObject(trigProperty as UnityEngine.Object);

            string typeName;
            ulong hash = RavenUtility.s_InvalidHash;
            if (!string.IsNullOrEmpty(trigProperty.FunctionName) && (trigProperty.TargetComponent != null || forceUpdate)) {
                if (forceUpdate) {
                    var type = RavenUtility.GetTypeFromLoadedAssemblies(trigProperty.ComponentType);
                    UnityEngine.Object comp = null;
                    if (type != null) {
                        if (type == typeof(GameObject)) {
                            comp = trigProperty.Target;
                        } else {
                            comp = trigProperty.Target.GetComponent(type);
                        }
                    } else {
                        m_Warnings.Add(string.Format("Component type {0} does not exist. It's used on object {1} ({2}) in scene {3}.", trigProperty.ComponentType, trigProperty, trigProperty.GetInstanceID(), m_OpenScene));
                    }
                    var sp1 = serializedObj.FindProperty("m_TargetComponent");
                    if (sp1.objectReferenceValue != comp) {
                        trigProperty.TargetComponent = comp;
                        sp1.objectReferenceValue = comp;
                        didWork = true;
                    }
                } else {
                    typeName = trigProperty.ComponentBaseType.ToString();
                    try {
                        hash = m_NameToTargetHashMap[RavenUtility.StitchTypeAndMember(typeName, trigProperty.FunctionName)];
                    } catch (Exception) {
                        hash = RavenUtility.s_InvalidHash;
                    }

                    var sp2 = serializedObj.FindProperty("m_TargetHash");

                    if ((ulong)sp2.intValue != hash) {
                        trigProperty.TargetHash = hash;
                        sp2.longValue = (long)hash;
                        didWork = true;
                    }
                }
            }

            if (didWork) {
                serializedObj.ApplyModifiedProperties();
            }
            return didWork;
        }

        private void RecordObject(RavenPropertyComponent property) {
            Undo.RecordObject(property, "Property Change");
        }

        /// <summary>
        /// Calls editor's function which provides a list of functions for a component type. If the function isn't in that list, return false;
        /// </summary>
        private bool ValidateFunction(string function, Type componentType, object property) {
            if (!m_ValidateProperties) {
                return true;
            }

            if (componentType == null) {
                return false;
            }

            var funcs = RavenEditorUtility.GetComponentFunctions(componentType, null);
            var func = funcs.Find((x) => {
                return function == RavenUtility.GetPackedFunctionNameFromFullFunctionName(x);
            });

            if (!string.IsNullOrEmpty(func)) {
                return true;
            }

            if (property != null) {
                var uProperty = (UnityEngine.Object)property;
                m_Warnings.Add(string.Format("{0} type does not contain function {1} on object {2} ({3}) in scene {4}. SKIPPING.", componentType.ToString(), function, uProperty, uProperty.GetInstanceID(), m_OpenScene));
            }
            return false;
        }

        /// <summary>
        /// Calls editor's function which provides a list of properties for a component type and argument type. If the property isn't in that list, return false;
        /// </summary>
        private bool ValidateProperty(string propertyName, Type componentType, Type argType, object property) {
            if (!m_ValidateProperties) {
                return true;
            }

            if (componentType == null) {
                return false;
            }

            var properties = RavenEditorUtility.GetComponentMembers(argType, componentType);
            var propertyFound = properties.Find((x) => x.Split('|')[1] == propertyName);
            if (!string.IsNullOrEmpty(propertyFound)) {
                return true;
            }

            if (property != null) {
                var uProperty = (UnityEngine.Object)property;
                m_Warnings.Add(string.Format("{0} type does not contain property {1} on object {2} ({3}) in scene {4}. SKIPPING.", componentType.ToString(), propertyName, uProperty, uProperty.GetInstanceID(), m_OpenScene));
            }
            return false;
        }

        /// <summary>
        /// Validates generated function.
        /// </summary>
        private bool ValidateGeneratedFunction(FunctionDeclaration func) {
            if (func.GetValidated()) {
                return true;
            }

            var valid = false;
            switch (func.m_ID) {
                case 0:
                case 1:
                    valid = ValidateGeneratedPropertyFunction(func);
                    break;

                case 2:
                    valid = ValidateGeneratedCallFunction(func);
                    break;
            }

            func.m_IsValid = valid;
            func.SetValidated(true);

            if (!valid) {
                m_Info.Add(string.Format("Function {0} is invalid.", func));
            }

            return valid;
        }

        private bool ValidateGeneratedCallFunction(FunctionDeclaration func) {
            return ValidateFunction(func.m_MemberName, RavenUtility.GetTypeFromLoadedAssemblies(func.m_ComponentType), null);
        }

        private bool ValidateGeneratedPropertyFunction(FunctionDeclaration func) {
            return ValidateProperty(func.m_MemberName, RavenUtility.GetTypeFromLoadedAssemblies(func.m_ComponentType), RavenUtility.GetTypeFromLoadedAssemblies(func.m_ArgumentType), null);
        }

        private void GenerateFunctionJumpTable(CodeTypeDeclaration klass) {
            klass.DeclareMember("private static readonly Dictionary<ulong, Delegate[]> {0} = new Dictionary<ulong, Delegate[]>(){3}{2}{{{3}{1}{2}}};", c_FunctionJumpTableName, GetFunctionTableMembers(), MultiplyString(c_Indent, 2), c_NewLine);
        }

        /// <summary>
        /// private static readonly Dictionary<ulong, Delegate[]> s_FunctionJumpTable = new Dictionary<ulong, Delegate[]>()
        /// {
        ///     {
        ///         543534,
        ///         new Delegate[]
        ///         {
        ///             (RavenPropertyGetter<UnityEngine.Vector3>)GetValue_0_0,
        ///             (RavenPropertySetter<UnityEngine.Vector3>)SetValue_0_0,
        ///         }
        ///     },
        /// };
        /// </summary>
        /// <returns></returns>
        private string GetFunctionTableMembers() {
            string stuff = "";
            int currentIndent = 2;
            foreach (var kvp in m_TargetHashToNameMap) {
                var hash = kvp.Key;
                FunctionDeclaration[] funcs = null;
                // I don't know how this can happen but it happened once...
                if (!m_TargetHashToFuncMap.TryGetValue(hash, out funcs)) {
                    m_Warnings.Add(string.Format("Function entry in m_TargetHashToNameMap {0}/{1} doesn't exist in function jump table!", hash, kvp.Value));
                    continue;
                }
                currentIndent++;
                stuff += string.Format("{0}{{{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                int funcLen = funcs.Length;
                currentIndent++;
                stuff += string.Format("{0}{1}ul,{2}", MultiplyString(c_Indent, currentIndent), hash, c_NewLine);
                stuff += string.Format("{0}new Delegate[]{1}{0}{{{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                currentIndent++;
                for (int j = 0; j < funcLen; ++j) {
                    stuff += string.Format("{0}({1}){2},{3}", MultiplyString(c_Indent, currentIndent), funcs[j].GetCastStatementForDelegate(), funcs[j].m_FunctionName, c_NewLine);
                }
                currentIndent--;
                stuff += string.Format("{0}}}{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                currentIndent--;
                stuff += string.Format("{0}}},{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                currentIndent--;
            }

            return stuff;
        }

        private string DumpNameToTargetHashMap() {
            string stuff = "";
            int currentIndent = 3;
            foreach (var kvp in m_NameToTargetHashMap) {
                stuff += string.Format("{0}{{ \"{1}\", {2}ul }},{3}", MultiplyString(c_Indent, currentIndent), kvp.Key, kvp.Value, c_NewLine);
            }
            return stuff;
        }

        private string DumpTargetHashToFuncMap() {
            string stuff = "";
            int currentIndent = 2;
            foreach (var kvp in m_TargetHashToFuncMap) {
                currentIndent++;
                stuff += string.Format("{0}{{{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);

                currentIndent++;
                stuff += string.Format("{0}{1}ul,{2}", MultiplyString(c_Indent, currentIndent), kvp.Key, c_NewLine);
                stuff += string.Format("{0}new FunctionDeclaration[{1}]{2}", MultiplyString(c_Indent, currentIndent), kvp.Value.Length, c_NewLine);
                stuff += string.Format("{0}{{{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);

                currentIndent++;
                foreach (var func in kvp.Value) {
                    stuff += string.Format("{0}new FunctionDeclaration(){1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                    stuff += string.Format("{0}{{{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                    currentIndent++;
                    foreach (var fi in func.GetType().GetFields()) {
                        if (fi.Name != "m_Objects" && fi.Name != "m_IsValid") {
                            stuff += string.Format("{0}{1} = {3}{2}{3}{4},{5}", MultiplyString(c_Indent, currentIndent), fi.Name, fi.GetValue(func), fi.FieldType == typeof(string) ? "\"" : "", fi.FieldType == typeof(ulong) ? "ul" : "", c_NewLine);
                        }
                    }
                    currentIndent--;
                    stuff += string.Format("{0}}},{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                }
                currentIndent--;
                stuff += string.Format("{0}}}{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);

                currentIndent--;
                stuff += string.Format("{0}}},{1}", MultiplyString(c_Indent, currentIndent), c_NewLine);
                currentIndent--;
            }
            return stuff;
        }

        private string DumpTargetClassSpecializationToIntMap() {
            string stuff = "";
            int currentIndent = 3;
            foreach (var kvp in m_TargetClassSpecializationToNameMap) {
                stuff += string.Format("{0}{{ \"{1}\", \"{2}\" }},{3}", MultiplyString(c_Indent, currentIndent), kvp.Key, kvp.Value, c_NewLine);
            }
            return stuff;
        }

        private void GenerateConfigurePropertyPropertyFunction(CodeTypeDeclaration klass) {
            klass.DeclareMethod(typeof(void).ToString(), "ConfigureRavenAnimationProperty<T>",
                (body) => {
                    body.Attributes |= MemberAttributes.Static;
                    body.DeclareParameter("RavenAnimationDataPropertyBase<T>", "animationProperty");
                    body.Statements.If("animationProperty == null".Expr(),
                        (stmts) => {
                            stmts.Expr("return");
                        });

                    body.Statements.Expr("var targetHash = animationProperty.TargetHash");
                    body.Statements.Expr("animationProperty.SetOnSyncCallback(((({0}<T>)s_FunctionJumpTable[targetHash][0])))", c_GetDelegateName);
                    body.Statements.Expr("animationProperty.SetOnExecuteCallback(((({0}<T>)s_FunctionJumpTable[targetHash][1])))", c_SetDelegateName);
                });
        }

        private void GenerateConfigureTriggerPropertyFunctions(CodeTypeDeclaration klass) {
            for (int i = 0; i < m_GeneratedClassTemplates.Count; ++i) {
                var templateTypes = RavenUtility.GetTemplateTypesForTemplateArguments(m_GeneratedClassTemplates[i]);
                klass.DeclareMethod(typeof(void).ToString(), "ConfigureRavenTriggerProperty" + templateTypes,
                (body) => {
                    body.Attributes |= MemberAttributes.Static;
                    body.DeclareParameter("RavenTriggerPropertyBase" + i + templateTypes, "triggerProperty");
                    body.Statements.If("triggerProperty == null".Expr(),
                        (stmts) => {
                            stmts.Expr("return");
                        });

                    body.Statements.Expr("var targetHash = triggerProperty.TargetHash");
                    body.Statements.Expr("triggerProperty.SetOnExecuteCallback(((({0})s_FunctionJumpTable[targetHash][0])))", c_CallDelegateName + templateTypes);
                });
            }
        }

        private void RegenerateFunctions(CodeTypeDeclaration klass) {
            foreach (var kvp in m_TargetHashToFuncMap) {
                foreach (var func in kvp.Value) {
                    ValidateGeneratedFunction(func);
                    switch (func.m_ID) {
                        case 0:
                            GenerateGetter(klass, func);
                            break;

                        case 1:
                            GenerateSetter(klass, func);
                            break;

                        case 2:
                            GenerateCaller(klass, func);
                            break;

                        default:
                            throw new Exception(string.Format("Function with id {0} not defined!", func.m_ID));
                    }
                }
            }
        }

        private void GenerateFunctions(CodeTypeDeclaration klass, string typeName, string argType, string memberName, ulong hash) {
            GenerateGetter(klass, typeName, argType, memberName, hash);
            GenerateSetter(klass, typeName, argType, memberName, hash);
        }

        private FunctionDeclaration GenerateGetter(CodeTypeDeclaration klass, string compType, string argType, string memberName, ulong hash) {
            var func = new FunctionDeclaration(0, argType, compType, memberName, hash, GetDelegateName(0));

            if (!m_TargetHashToFuncMap.ContainsKey(hash)) {
                m_TargetHashToFuncMap[hash] = new FunctionDeclaration[2];
            }
            m_TargetHashToFuncMap[hash][0] = func;

            return GenerateGetter(klass, func);
        }

        private FunctionDeclaration GenerateGetter(CodeTypeDeclaration klass, FunctionDeclaration func) {
            Debug.AssertFormat(func.GetValidated(), "Function {0} not validated!", func);
            var legitType = RavenUtility.GetTypeFromLoadedAssemblies(func.m_ArgumentType) != null;
            klass.DeclareMethod(string.Format("{0}", legitType ? func.m_ArgumentType : RavenUtility.s_DefaultFallbackType.ToString()), func.m_FunctionName, (x) => {
                x.Attributes |= MemberAttributes.Static | MemberAttributes.Private;
                x.DeclareParameter("UnityEngine.Object", "c");
                if (!legitType) {
                    m_Info.Add(string.Format("Function {0} is invalid.", func));
                }
                if (func.m_IsValid && legitType) {
                    x.Statements.If("c != null".Expr(), (stmts) => {
                        stmts.Expr("return (c as {0}).{1}", func.GetCastStatementForComponentType(), func.m_MemberName);
                    });
                }
                x.Statements.Expr("return default({0})", legitType ? func.m_ArgumentType : RavenUtility.s_DefaultFallbackType.ToString());
            });
            return func;
        }

        private FunctionDeclaration GenerateSetter(CodeTypeDeclaration klass, string compType, string argType, string memberName, ulong hash) {
            var func = new FunctionDeclaration(1, argType, compType, memberName, hash, GetDelegateName(1));

            if (!m_TargetHashToFuncMap.ContainsKey(hash)) {
                Debug.LogErrorFormat("Compilation error; no getter generated for {0} - {1} before setters.", func.m_ComponentType, func.m_ArgumentType);
                return null;
            }
            m_TargetHashToFuncMap[hash][1] = func;

            return GenerateSetter(klass, func);
        }

        private FunctionDeclaration GenerateSetter(CodeTypeDeclaration klass, FunctionDeclaration func) {
            Debug.AssertFormat(func.GetValidated(), "Function {0} not validated!", func);
            klass.DeclareMethod(typeof(void).ToString(), func.m_FunctionName, (x) => {
                x.Attributes |= MemberAttributes.Static | MemberAttributes.Private;
                x.DeclareParameter("UnityEngine.Object", "c");
                var legitType = RavenUtility.GetTypeFromLoadedAssemblies(func.m_ArgumentType) != null;
                x.DeclareParameter(legitType ? func.m_ArgumentType : RavenUtility.s_DefaultFallbackType.ToString(), "v");
                if (!legitType) {
                    m_Info.Add(string.Format("Function {0} is invalid.", func));
                }
                if (func.m_IsValid && legitType) {
                    x.Statements.If("c != null".Expr(), (stmts) => {
                        stmts.Expr("(c as {0}).{1} = v", func.GetCastStatementForComponentType(), func.m_MemberName);
                    });
                }
            });
            return func;
        }

        private FunctionDeclaration GenerateCaller(CodeTypeDeclaration klass, string compType, string argType, string memberName, ulong hash) {
            var func = new FunctionDeclaration(2, argType, compType, memberName, hash, GetDelegateName(2));

            if (!m_TargetHashToFuncMap.ContainsKey(hash)) {
                m_TargetHashToFuncMap[hash] = new FunctionDeclaration[1];
            }
            m_TargetHashToFuncMap[hash][0] = func;

            var typesPacked = RavenUtility.GetFunctionParametersFromPackedFunctionName(memberName);
            GenerateCallerDataClass(RavenUtility.GetFunctionParameterTypesUnpacked(typesPacked), typesPacked);
            return GenerateCaller(klass, func);
        }

        private FunctionDeclaration GenerateCaller(CodeTypeDeclaration klass, FunctionDeclaration func) {
            Debug.AssertFormat(func.GetValidated(), "Function {0} not validated!", func);
            var parameterTypes = RavenUtility.GetFunctionParameterTypesUnpacked(func.m_ArgumentType);
            var values = new List<string>();
            var allTypesValid = true;
            klass.DeclareMethod(typeof(void).ToString(), func.m_FunctionName, (x) => {
                x.Attributes |= MemberAttributes.Static | MemberAttributes.Private;
                x.DeclareParameter("UnityEngine.Object", "c");
                for (int i = 0; i < parameterTypes.Length; ++i) {
                    var parameterType = parameterTypes[i];
                    if (!string.IsNullOrEmpty(parameterType)) {
                        var legitType = RavenUtility.GetTypeFromLoadedAssemblies(parameterType) != null;
                        allTypesValid &= legitType;
                        values.Add("v" + (i + 1));
                        x.DeclareParameter(legitType ? parameterType : RavenUtility.s_DefaultFallbackType.ToString(), values[i]);
                    }
                }
                if (!allTypesValid) {
                    m_Info.Add(string.Format("Function {0} is invalid.", func));
                }
                if (func.m_IsValid && allTypesValid) {
                    x.Statements.If("c != null".Expr(), (stmts) => {
                        stmts.Expr("(c as {0}).{1}({2})", func.GetCastStatementForComponentType(), RavenUtility.GetFunctionNameFromPackedFunctionName(func.m_MemberName), string.Join(", ", values.ToArray()));
                    });
                }
            });

            return func;
        }

        private void RegenerateClasses() {
            foreach (var kvp in PropertyReflectionCompiledOutput.s_TargetClassSpecializationToNameMap) {
                GenerateCallerDataClass(RavenUtility.GetFunctionParameterTypesUnpacked(kvp.Key), kvp.Key);
                m_TargetClassSpecializationToNameMap[kvp.Key] = kvp.Value;
            }
        }

        private void CleanupOldClasses() {
            var fileList = Directory.GetFiles(m_TargetFolder + "/" + c_SpecializedFunctionClassesFolder, "*.cs", SearchOption.TopDirectoryOnly);
            var currentClasses = m_TargetClassSpecializationToNameMap.Values.ToList();
            foreach (var file in fileList) {
                if (currentClasses.Contains(RavenEditorUtility.c_RavenCompilerNamespace + "." + Path.GetFileNameWithoutExtension(file))) {
                    continue;
                }
                File.Delete(file);
            }
        }

        private void GenerateCallerDataClass(string[] types, string packedTypes) {
            if (types[0] == string.Empty) {
                types = new string[0];
            }

            string name;
            if (!m_TargetClassSpecializationToNameMap.TryGetValue(packedTypes, out name)) {
                name = c_FunctionCallClassName + "_" + RavenUtility.HashString(packedTypes);
                m_TargetClassSpecializationToNameMap[packedTypes] = RavenEditorUtility.c_RavenCompilerNamespace + "." + name;

                // create a new compiler for this class (each class should have its own file)
#if STARLITE_EDITOR
                GenerateCallerDataClassForStarlite(name, types);
#else
                var codeNamespaceCompiler = GenerateCodeCompileUnitForSoloClass(m_CodeCompileUnitsForSpecializedFunctionClasses);

                var klass = codeNamespaceCompiler.DeclareClass(name);
                klass.IsPartial = true;
                var inheritedType = new CodeTypeReference(RavenUtility.GetTypeWithoutNamespace(c_FunctionCallPropertyType) + types.Length, types.Select(x => {
                    var type = RavenUtility.GetTypeFromLoadedAssemblies(x);
                    if (type == null) {
                        m_Warnings.Add(string.Format("Type {0} does not exist anymore!", x));
                        return new CodeTypeReference(RavenUtility.s_DefaultFallbackType);
                    }
                    return new CodeTypeReference(type);
                }).ToArray());
                klass.BaseTypes.Add(inheritedType);
                klass.TypeAttributes |= TypeAttributes.Sealed;
#endif
            }
        }

#if STARLITE_EDITOR
        private void GenerateCallerDataClassForStarlite(string name, string[] types) {
            var codeCompileUnit = new StarliteCodeCompileUnit() {
                m_TypeName = name
            };
            m_CodeCompileUnitsForSpecializedFunctionClasses.Add(codeCompileUnit);

            var templateTypes = types.Select(x => {
                var type = RavenUtility.GetTypeFromLoadedAssemblies(x);
                if (type == null) {
                    m_Warnings.Add(string.Format("Type {0} does not exist anymore!", x));
                    return RavenUtility.s_DefaultFallbackType;
                }
                return type;
            }).ToArray();

            string templateTypesString = "";
            for (int i = 0; i < templateTypes.Length; i++) {
                var type = templateTypes[i];
                if (!string.IsNullOrEmpty(templateTypesString)) {
                    templateTypesString += ",";
                }
                templateTypesString += Starlite.CStoCC.GetReflectedType(type, type.ToString(), false, true);
            }
            var template = RavenUtility.GetTemplateTypesForTypeArguments(templateTypesString);

            codeCompileUnit.m_CCContents.AppendFormat("#ifdef STARLITE\n");
            codeCompileUnit.m_CCContents.AppendFormat("#include \"{0}.h\"\n", name);
            codeCompileUnit.m_CCContents.AppendFormat("#include \"{0}.cs\"\n", name);
            codeCompileUnit.m_CCContents.AppendFormat("#endif");

            codeCompileUnit.m_HeaderContents.Append("#ifdef STARLITE\n");
            codeCompileUnit.m_HeaderContents.Append("#pragma once\n");
            codeCompileUnit.m_HeaderContents.Append("#include <Scripts/Compiled/RavenTriggerPropertyBase.h>\n\n");

            int indentCount = 0;
            foreach (var ns in RavenEditorUtility.c_RavenCompilerNamespace.Split('.')) {
                codeCompileUnit.m_HeaderContents.AppendFormat("{0}namespace {1} {{\n\n", MultiplyString(c_Indent, indentCount), ns);
                ++indentCount;
            }

            codeCompileUnit.m_HeaderContents.AppendFormat("{0}class {1} : public {2}{3} {{\n", MultiplyString(c_Indent, indentCount), name, RavenUtility.GetTypeWithoutNamespace(c_FunctionCallPropertyType) + types.Length, template);

            ++indentCount;
            codeCompileUnit.m_HeaderContents.AppendFormat("{0}SCLASS_SEALED({1});\n\n", MultiplyString(c_Indent, indentCount), name);

            --indentCount;
            codeCompileUnit.m_HeaderContents.AppendFormat("{0}public:\n", MultiplyString(c_Indent, indentCount));

            ++indentCount;
            codeCompileUnit.m_HeaderContents.AppendFormat("{0}{1}() = default;\n\n", MultiplyString(c_Indent, indentCount), name);

            --indentCount;
            codeCompileUnit.m_HeaderContents.AppendFormat("{0}}};\n", MultiplyString(c_Indent, indentCount));

            while (indentCount > 0) {
                --indentCount;
                codeCompileUnit.m_HeaderContents.AppendFormat("{0}}}\n", MultiplyString(c_Indent, indentCount));
            }
            codeCompileUnit.m_HeaderContents.AppendFormat("#endif");
        }
#endif

        private void GenerateBaseGenericCallerDataClass(int nArguments, CodeNamespace codeNamespace) {
            if (m_GeneratedClassTemplates.IndexOf(nArguments) != -1) {
                return;
            }

            var typeName = RavenUtility.GetTypeWithoutNamespace(c_FunctionCallPropertyType) + nArguments;
            var klass = codeNamespace.DeclareClass(typeName);
            klass.TypeAttributes |= TypeAttributes.Abstract;
            klass.IsPartial = true;
            klass.BaseTypes.Add(new CodeTypeReference(RavenUtility.GetTypeFromLoadedAssemblies(c_FunctionCallPropertyTypeBaseType)));

            var templateSuffix = RavenUtility.GetTemplateTypesForTemplateArguments(nArguments);
            var callDelegate = new CodeTypeDelegate(c_CallDelegateName + templateSuffix);
            callDelegate.Parameters.Add(new CodeParameterDeclarationExpression("UnityEngine.Object", "component"));

            var paramCollection = new CodeTypeParameterCollection();
            var memberNames = new List<string>() {
                // base member
                "m_TargetComponent"
            };
            for (int i = 0; i < nArguments; ++i) {
                var genericType = new CodeTypeParameter("T" + i);
                var genericTypeReference = new CodeTypeReference(genericType);
                klass.TypeParameters.Add(genericType);

                var field = new CodeMemberField(genericTypeReference, "m_Value" + i) {
                    Attributes = MemberAttributes.Family
                };
                field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
#if !STARLITE_EDITOR
                klass.Members.Add(field);
#endif
                memberNames.Add(field.Name);

                // line below doesn't work
                // callDelegate.TypeParameters.Add(new CodeTypeParameter("T" + i));
                callDelegate.Parameters.Add(new CodeParameterDeclarationExpression(genericTypeReference, "value" + i));

                if (i == 0) {
                    var headerAttribute = new CodeAttributeDeclaration(new CodeTypeReference(typeof(HeaderAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression("Values")));
                    field.CustomAttributes.Add(headerAttribute);
                }
            }

            var callbackField = klass.DeclareField(c_CallDelegateName + templateSuffix, "e_ExecuteCallback");
            callbackField.Attributes = MemberAttributes.Private;
            klass.TypeParameters.AddRange(paramCollection);

            // methods
            klass.DeclareMethod(typeof(void).ToString(), "Initialize", (body) => {
                var parameter = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(RavenSequence)), "sequence");
                body.Parameters.Add(parameter);
                body.Statements.Call("base".Expr(), "Initialize", "sequence".Expr());
                body.Statements.Call("PropertyReflectionCompiledOutput".Expr(), "ConfigureRavenTriggerProperty" + templateSuffix, "this".Expr());
                body.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            });

            klass.DeclareMethod(typeof(void).ToString(), "OnEnter", (body) => {
                body.Statements.IfElse("HasOverridenTargetComponents()".Expr(),
                    trueStmts => {
                        var oldName = memberNames[0];
                        memberNames[0] = "m_OverridenTargetComponents[i]";
                        trueStmts.For("i", "i < m_OverridenTargetComponents.Count".Expr(), forStmts => {
                            forStmts.Call(null, "e_ExecuteCallback", memberNames.Select(x => x.Expr()).ToArray());
                        });
                        memberNames[0] = oldName;
                    },
                    elseStmts => {
                        elseStmts.Call(null, "e_ExecuteCallback", memberNames.Select(x => x.Expr()).ToArray());
                    });
                body.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            });

            klass.DeclareMethod(typeof(void).ToString(), "ManualExecute", (body) => {
                var paramNames = new string[nArguments + 1];
                paramNames[0] = memberNames[0];
                for (int i = 0; i < nArguments; ++i) {
                    paramNames[i + 1] = "value" + i;
                    body.DeclareParameter("T" + i, paramNames[i + 1]);
                }
                body.Statements.IfElse("HasOverridenTargetComponents()".Expr(),
                    trueStmts => {
                        var oldName = paramNames[0];
                        paramNames[0] = "m_OverridenTargetComponents[i]";
                        trueStmts.For("i", "i < m_OverridenTargetComponents.Count".Expr(), forStmts => {
                            forStmts.Call(null, "e_ExecuteCallback", paramNames.Select(x => x.Expr()).ToArray());
                        });
                        paramNames[0] = oldName;
                    },
                    elseStmts => {
                        elseStmts.Call(null, "e_ExecuteCallback", paramNames.Select(x => x.Expr()).ToArray());
                    });
                body.Attributes = MemberAttributes.Public;
            });

            klass.DeclareMethod(typeof(void).ToString(), "SetOnExecuteCallback", (body) => {
                body.DeclareParameter(c_CallDelegateName + templateSuffix, "callback");
                body.Statements.Assign("e_ExecuteCallback".Expr(), "callback".Expr());
            });

            // add delegate
            codeNamespace.Types.Add(callDelegate);
            m_GeneratedClassTemplates.Add(nArguments);
        }

        private void DumpFunctionObjects(CodeTypeDeclaration klass) {
            if (!ShouldDumpFunctionObjects()) {
                return;
            }

            klass.DeclareMember(c_NewLine);
            klass.DeclareMember("#region FunctionInfo");
            foreach (var kvp in m_TargetHashToFuncMap) {
                foreach (var func in kvp.Value) {
                    klass.DeclareMember(c_NewLine);
                    klass.DeclareMember(string.Format("/// Responsible gameobjects for [{0}]:", func.m_FunctionName));
                    foreach (var obj in func.m_Objects) {
                        klass.DeclareMember(string.Format("/// {0}", obj));
                    }
                }
            }

            klass.DeclareMember(c_NewLine);
            klass.DeclareMember("#endregion FunctionInfo");
            klass.DeclareMember(c_NewLine);
        }

        private bool ShouldDumpFunctionObjects() {
            return m_DumpFunctionInfo && ShouldProcessAssets();
        }

        private bool ShouldProcessAssets() {
            return !m_SceneUpdate && m_PropertyToUpdate == null;
        }

        private bool ShouldReserializeProperties() {
            return m_ReserializeProperties;
        }

        private bool ShouldProcessCurrentScene() {
            return m_SceneUpdate;
        }

        private void GenerateIfRavenCompiled(bool start, StringBuilder sb) {
            if (start) {
                sb.AppendLine("#if RAVEN_COMPILED");
            } else {
                sb.AppendLine("#endif\t // RAVEN_COMPILED");
            }
        }

        private void GenerateUsingStatements(StringBuilder sb) {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine();
        }

        private void GenerateSourceCode(string file, CodeCompileUnit codeCompileUnit) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            GenerateIfRavenCompiled(true, sb);
            GenerateUsingStatements(sb);

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();

            options.BlankLinesBetweenMembers = true;
            options.IndentString = c_Indent;

            provider.GenerateCodeFromCompileUnit(codeCompileUnit, sw, options);

            sw.Flush();
            sw.Dispose();

            GenerateIfRavenCompiled(false, sb);
            File.WriteAllText(file, sb.ToString());
        }

        private void CallSceneBeingProcessedDelegate(int sceneIndex, int totalScenes) {
            if (e_OnSceneBeingProcessed != null) {
                e_OnSceneBeingProcessed(m_OpenScene, sceneIndex, totalScenes);
            }
        }

        private static ulong GetHashForComponentAndMember(string component, string member) {
            return RavenUtility.HashString(component + "." + member);
        }

        private static ulong GetHashForAnimationProperty(RavenAnimationPropertyComponentBase property) {
            return RavenUtility.HashString(property.ComponentBaseType.ToString() + "." + property.MemberName);
        }

        private static ulong GetHashForAnimationProperty(RavenTriggerPropertyComponentBase property) {
            return RavenUtility.HashString(property.ComponentBaseType.ToString() + "." + property.FunctionName);
        }

        private static string GetDelegateName(int funcID) {
            switch (funcID) {
                case 0:
                    return c_GetDelegateName;

                case 1:
                    return c_SetDelegateName;

                case 2:
                    return c_CallDelegateName;

                default:
                    return null;
            }
        }

        private static string MultiplyString(string source, int multiplier) {
            StringBuilder sb = new StringBuilder(multiplier * source.Length);
            for (int i = 0; i < multiplier; i++) {
                sb.Append(source);
            }

            return sb.ToString();
        }

        private static void FindAllScenesInProject(string path, List<string> outScenes, string[] exclusionFilters) {
            try {
                var newFiles = Directory.GetFiles(path);
                var len = newFiles.Length;
                for (int i = 0; i < len; ++i) {
                    var file = newFiles[i];

                    if (exclusionFilters != null) {
                        for (int j = 0; j < exclusionFilters.Length; ++j) {
                            if (file.ToLowerInvariant().Contains(exclusionFilters[j])) {
                                return;
                            }
                        }
                    }

                    if (Path.GetExtension(file).CompareTo(".unity") == 0) {
                        outScenes.Add("Assets" + file.Replace(Application.dataPath, "").Replace("\\\\", "/"));
                    }
                }
            } catch (System.Exception ex) {
                Debug.LogException(ex);
                return;
            }

            try {
                foreach (string subDir in Directory.GetDirectories(path)) {
                    FindAllScenesInProject(subDir, outScenes, exclusionFilters);
                }
            } catch (System.Exception ex) {
                Debug.LogException(ex);
            }
        }
    }
}