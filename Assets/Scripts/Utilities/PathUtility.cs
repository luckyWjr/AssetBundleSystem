using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility {

	public static class PathUtility {

        /// <summary>
        /// 获取资源的完整路径
        /// </summary>
        /// <param name="assetPath">资源的Unity路径 如Assets/a/b.c</param>
        /// <returns>完整路径 如：c:/a/b.c</returns>
        public static string GetFullPath(string assetPath) {
            if(string.IsNullOrEmpty(assetPath)) {
                return "";
            }

            string p = Application.dataPath + assetPath.Substring(6);
            return p.Replace("\\", "/");
        }

        /// <summary>
        /// 获取资源的Unity路径
        /// </summary>
        /// <param name="fullPath">资源的完整路径 如：c:/a/b.c</param>
        /// <returns>Unity路径 如Assets/a/b.c</returns>
        public static string GetAssetPath(string fullPath) {
            if(string.IsNullOrEmpty(fullPath)) {
                return "";
            }

            fullPath = fullPath.Replace("\\", "/");
            return fullPath.StartsWith("Assets/") ? fullPath : "Assets" + fullPath.Substring(Application.dataPath.Length);
        }

        public static string FixPath(this string old) {
            return !string.IsNullOrEmpty(old) ? old.Replace("\\", "/") : old;
        }
    }
}