using System;
using System.IO;
using UnityEditor;
using Utility;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AssetBundle {
	public abstract class BaseAssetBundleManager {

        string m_outputFolderName;
        BaseAssetBundle[] m_items;

        bool m_isChange = false;//记录该目录下的资源包是否发生变化
        bool m_isCompare = false;//是否对比过上个版本的资源

        protected abstract BaseAssetBundle[] GetAssetArray();

        /// <summary>
        /// 资源类别，如lua是一个类别，ui prefab是一个类别
        /// </summary>
        /// <param name="assetFolderPath">资源所在项目目录 如：Assets/Res/Textures</param>
        /// <param name="filter">过滤器，其中如f:*.*是自定义的过滤器，做为SearchPattern搜索匹配文件</param>
        /// <param name="outputFolderName">此资源输出的文件夹名 如Textures</param>
        public BaseAssetBundleManager(string assetFolderPath, string filter, string outputFolderName) {
            if(string.IsNullOrEmpty(assetFolderPath) || !assetFolderPath.StartsWith("Assets/")) {
                throw new ArgumentException("assetFolderPath");
            }
            if(string.IsNullOrEmpty(filter)) {
                throw new ArgumentException("filter");
            }
            if(string.IsNullOrEmpty(outputFolderName)) {
                throw new ArgumentException("outputFolder");
            }

            this.assetFolderPath = assetFolderPath;
            this.filter = filter;
            this.outputFolderName = outputFolderName;
        }

        /// <summary>
        /// 获取指定目录的资源
        /// </summary>
        /// <param name="filter">过滤器，若以t:开头，表示用unity的方式过滤；若以f:开头，表示用windows的SearchPattern方式过滤；若以r:开头，表示用正则表达式的方式过滤。</param>
        public string[] GetAssets(string folder, string filter) {
            if (string.IsNullOrEmpty(folder)) {
                throw new ArgumentException("folder");
            }
            if (string.IsNullOrEmpty(filter)) {
                throw new ArgumentException("filter");
            }

            if (filter.StartsWith("t:")) {
                string[] guids = AssetDatabase.FindAssets(filter, new string[] { folder });
                string[] paths = new string[guids.Length];
                for (int i = 0; i < guids.Length; i++) {
                    paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                }
                return paths;
            } else if (filter.StartsWith("f:")) {
                string folderFullPath = PathUtility.GetFullPath(folder);
                string searchPattern = filter.Substring(2);
                string[] files = Directory.GetFiles(folderFullPath, searchPattern, SearchOption.AllDirectories);
                string[] paths = new string[files.Length];
                for (int i = 0; i < files.Length; i++) {
                    paths[i] = PathUtility.GetAssetPath(files[i]);
                }
                return paths;
            } else if (filter.StartsWith("r:")) {
                string folderFullPath = PathUtility.GetFullPath(folder);
                string pattern = filter.Substring(2);
                string[] files = Directory.GetFiles(folderFullPath, "*.*", SearchOption.AllDirectories);
                List<string> list = new List<string>();
                for (int i = 0; i < files.Length; i++) {
                    string name = Path.GetFileName(files[i]);
                    if (Regex.IsMatch(name, pattern)) {
                        string p = PathUtility.GetAssetPath(files[i]);
                        list.Add(p);
                    }
                }
                return list.ToArray();
            } else {
                throw new InvalidOperationException("Unexpected filter: " + filter);
            }
        }

        /// <summary>
        /// 计算所有资源各自的Hash
        /// </summary>
        public virtual void ComputeHash() {
            foreach (var item in items) {
                item.ComputeHash();
            }
        }

        /// <summary>
        /// 获取该种类所有需要打包资源
        /// </summary>
        /// <returns>资源路径数组</returns>
        protected string[] GetAssets() {
            return GetAssets(assetFolderPath, filter);
        }

        public override string ToString() {
            return string.Format("{0}\t{1}\t{2}", assetFolderPath, filter, m_outputFolderName);
        }

        public virtual void Dispose() {
            foreach (var item in items) {
                item.Dispose();
            }
        }

        public virtual void PrepareBuild() {

        }

        public virtual void OnBuildFinished() {

        }

        #region 属性
        public string assetFolderPath {
            private set;
            get;
        }

        public string filter {
            private set;
            get;
        }

        public string outputFolderName {
            get { return m_outputFolderName; }
            set { m_outputFolderName = value.TrimStart('/').TrimEnd('/').ToLower(); }
        }

        public BaseAssetBundle[] items {
            get {
                if (m_items == null) {
                    m_items = GetAssetArray();
                }
                return m_items;
            }
        }

        /// <summary>
        /// 获取和上次不一样的AB包，不包含新增的
        /// </summary>
        public List<string> GetDifferentAssetBundleList() {
            List<string> list = new List<string>();
            foreach(var item in items) {
                if(!string.IsNullOrEmpty(item.lastHash) && item.lastHash != item.currentHash) {
                    list.Add(item.assetBundleName);
                }
            }
            return list;
        }

        public IEnumerable<AssetBundleBuild> assetBundleBuilds {
            get {
                foreach(var item in items) {
                    yield return new AssetBundleBuild() {
                        assetBundleName = item.assetBundleName,
                        assetNames = item.assetNames,
                    };
                }
            }
        }
        #endregion
    }
}
