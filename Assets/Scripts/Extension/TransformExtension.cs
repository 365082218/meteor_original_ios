using ShortcutExtension;
using System.Collections.Generic;
using UnityEngine;

namespace ShortcutExtension
{
    public static class TransformExtension
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            if (comp == null)
            {
                comp = obj.AddComponent<T>();
            }
            return comp;
        }

        public static T GetOrAddComponent<T>(this Transform self) where T : Component
        {
            T comp = self.GetComponent<T>();
            if(comp == null)
            {
                comp = self.gameObject.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// 获取模型包围盒的中心点.
        /// </summary>
        public static Vector3 GetCenter(this Transform model)
        {
            Vector3 result = Vector3.zero;
            int counter = 0;
            CalculateCenter(model, ref result, ref counter);
            return result / counter;
        }

        /// <summary>
        /// 获取模型包围盒.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Bounds GetBounds(this Transform model)
        {
            Bounds resultBounds = new Bounds(model.GetCenter(), Vector3.zero);
            CalculateBounds(model, ref resultBounds);
            return resultBounds;
        }

        /// <summary>
        /// 附加碰撞体.
        /// </summary>
        public static BoxCollider AttachCollider(this Transform model)
        {
            // 先去掉所有碰撞器（包括子物体）
            Collider[] colliders = model.GetComponentsInChildren<Collider>();
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(colliders[i]);
            }

            Bounds bounds = model.GetBounds();
            Vector3 scale = model.localScale;
            BoxCollider boxCollider = model.GetOrAddComponent<BoxCollider>();
            boxCollider.center = bounds.center - model.position;
            boxCollider.size = new Vector3(bounds.size.x / scale.x, bounds.size.y / scale.y, bounds.size.z / scale.z);
            return boxCollider;
        }

        private static void CalculateCenter(Transform model, ref Vector3 result, ref int counter)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                result += renderers[i].transform.position;
                counter++;
            }
        }

        private static void CalculateBounds(Transform model, ref Bounds bounds)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        #region 未使用

        ///// <summary>
        ///// 获取该物体的子物体和孙物体中包含该组件的物体.
        ///// </summary>
        //public static List<T> GetComponentsInChildrenNoSelf<T>(this Transform model) where T : Component
        //{
        //    T[] childrenTrans = model.GetComponentsInChildren<T>();
        //    List<T> children = new List<T>();

        //    for (int i = 1; i < childrenTrans.Length; i++)
        //    {
        //        children.Add(childrenTrans[i]);
        //    }
        //    return children;
        //}

        ///// <summary>
        ///// 获取该物体的子物体(不含孙物体)中包含该组件的物体.
        ///// </summary>
        //public static List<T> GetComponentsInSonNoSelf<T>(this Transform model) where T : Component
        //{
        //    List<T> children = new List<T>();
        //    for (int i = 0; i < model.childCount; i++)
        //    {
        //        children.Add(model.GetChild(i).GetComponent<T>());
        //    }

        //    return children;
        //}

        #endregion
    }
}
