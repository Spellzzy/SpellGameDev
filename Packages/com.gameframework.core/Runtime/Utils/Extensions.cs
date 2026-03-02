using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Utils
{
    /// <summary>
    /// 常用扩展方法集合。
    /// </summary>
    public static class Extensions
    {
        #region Transform

        /// <summary>
        /// 重置Transform的本地位置、旋转、缩放
        /// </summary>
        public static void Reset(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>
        /// 销毁所有子物体
        /// </summary>
        public static void DestroyAllChildren(this Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 设置X坐标
        /// </summary>
        public static void SetPositionX(this Transform t, float x)
        {
            var pos = t.position;
            pos.x = x;
            t.position = pos;
        }

        /// <summary>
        /// 设置Y坐标
        /// </summary>
        public static void SetPositionY(this Transform t, float y)
        {
            var pos = t.position;
            pos.y = y;
            t.position = pos;
        }

        /// <summary>
        /// 设置Z坐标
        /// </summary>
        public static void SetPositionZ(this Transform t, float z)
        {
            var pos = t.position;
            pos.z = z;
            t.position = pos;
        }

        #endregion

        #region Vector

        /// <summary>
        /// 返回XZ平面上的距离
        /// </summary>
        public static float DistanceXZ(this Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>
        /// Vector2转Vector3（Y=0）
        /// </summary>
        public static Vector3 ToVector3XZ(this Vector2 v)
        {
            return new Vector3(v.x, 0f, v.y);
        }

        /// <summary>
        /// 随机偏移
        /// </summary>
        public static Vector3 RandomOffset(this Vector3 v, float range)
        {
            return v + new Vector3(
                Random.Range(-range, range),
                Random.Range(-range, range),
                Random.Range(-range, range));
        }

        #endregion

        #region Collection

        /// <summary>
        /// 从列表随机取一个元素
        /// </summary>
        public static T RandomElement<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// 洗牌（Fisher-Yates）
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion

        #region Color

        /// <summary>
        /// 修改Alpha值
        /// </summary>
        public static Color WithAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        #endregion

        #region GameObject

        /// <summary>
        /// 安全获取组件，不存在则自动添加
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// 设置层级（包含所有子物体）
        /// </summary>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        #endregion
    }
}
