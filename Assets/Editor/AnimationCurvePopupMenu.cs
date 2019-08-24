using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class AnimationCurvePopupMenu
{
    private static AnimationCurve s_ClipBoardAnimationCurve;

    public static void Show(Rect popupRect, AnimationCurve animationCurve, SerializedProperty property)
    {
        if (GUI.Button(popupRect, GUIContent.none, "ShurikenDropdown"))
        {
            GUIContent content = new GUIContent("Copy");
            GUIContent content2 = new GUIContent("Paste");
            GUIContent content3 = new GUIContent("Clear");
            GenericMenu genericMenu = new GenericMenu();

            if (property != null)
            {
                genericMenu.AddItem(content, false, AnimationCurveCallbackCopy, property);
                genericMenu.AddItem(content2, false, AnimationCurveCallbackPaste, property);
                genericMenu.AddItem(content3, false, AnimationCurveCallbackClear, property);
            }
            else
            {
                genericMenu.AddItem(content, false, AnimationCurveCallback2Copy, animationCurve);
                genericMenu.AddItem(content2, false, AnimationCurveCallback2Paste, animationCurve);
                genericMenu.AddItem(content3, false, AnimationCurveCallback2Clear, animationCurve);
            }

            if (!HasClipBoardAnimationCurve())
            {
                genericMenu.AddDisabledItem(content2);
            }
            genericMenu.DropDown(popupRect);
            Event.current.Use();
        }
    }

    /// <summary>
    /// 清除检视器界面的曲线缓存，才能刷新新的值
    /// </summary>
    public static void AnimationCurvePreviewCacheClearCache()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(ReorderableList));
        Type type = assembly.GetType("UnityEditorInternal.AnimationCurvePreviewCache");
        MethodInfo clearCache = type.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
        if (clearCache != null)
        {
            clearCache.Invoke(null, null);
        }
    }

    private static bool HasClipBoardAnimationCurve()
    {
        return s_ClipBoardAnimationCurve != null;
    }

    private static void AnimationCurveCallbackCopy(object obj)
    {
        SerializedProperty property = (SerializedProperty)obj;
        s_ClipBoardAnimationCurve = property.animationCurveValue;
    }

    private static void AnimationCurveCallbackPaste(object obj)
    {
        if (s_ClipBoardAnimationCurve == null)
        {
            return;
        }
        SerializedProperty property = (SerializedProperty)obj;
        property.serializedObject.Update();
        property.animationCurveValue = s_ClipBoardAnimationCurve;
        property.serializedObject.ApplyModifiedProperties();
    }

    private static void AnimationCurveCallbackClear(object obj)
    {
        SerializedProperty property = (SerializedProperty)obj;
        property.serializedObject.Update();
        property.animationCurveValue = new AnimationCurve();
        property.serializedObject.ApplyModifiedProperties();
    }

    private static void AnimationCurveCallback2Copy(object obj)
    {
        AnimationCurve animationCurve = (AnimationCurve)obj;
        s_ClipBoardAnimationCurve = animationCurve;
    }

    private static void AnimationCurveCallback2Paste(object obj)
    {
        if (s_ClipBoardAnimationCurve == null)
        {
            return;
        }
        AnimationCurve animationCurve = (AnimationCurve)obj;
        animationCurve.keys = s_ClipBoardAnimationCurve.keys;
        animationCurve.postWrapMode = s_ClipBoardAnimationCurve.postWrapMode;
        animationCurve.preWrapMode = s_ClipBoardAnimationCurve.preWrapMode;
        AnimationCurvePreviewCacheClearCache();
    }

    private static void AnimationCurveCallback2Clear(object obj)
    {
        AnimationCurve animationCurve = (AnimationCurve)obj;
        if (animationCurve != null)
        {
            for (int i = animationCurve.length - 1; i >= 0; i--)
            {
                animationCurve.RemoveKey(i);
            }
            AnimationCurvePreviewCacheClearCache();
        }
    }
}
