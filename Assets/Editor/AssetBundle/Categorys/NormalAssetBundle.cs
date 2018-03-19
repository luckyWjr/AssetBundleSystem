using System;

namespace AssetBundle {
    public class NormalAssetBundle<T> : BaseAssetBundle where T : UnityEngine.Object {
        protected string m_assetPath;//资源路径
        string m_assetFolderPath;//资源根目录
        string m_name;//ab 名称(m_assetPath - m_assetFolderPath)

        /// <param name="assetPath">资源路径 如：Assets/a/b.c</param>
        /// <param name="assetFolderPath">资源根目录 如：Assets/a/</param>
        /// <param name="outputFolderName">输出目录 如：a</param>
        public NormalAssetBundle(string assetPath, string assetFolderPath, string outputFolderName)
            : base(outputFolderName) {
            if(string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets")) {
                throw new ArgumentException("assetPath");
            }

            m_assetPath = assetPath.Replace("\\", "/");
            m_assetFolderPath = assetFolderPath.Replace("\\", "/").TrimEnd('/');
        }

        public override string[] assetNames {
            get { return new string[] { m_assetPath }; }
        }

        public override string name {
            get {
                if(string.IsNullOrEmpty(m_name)) {
                    m_name = GetName(m_assetPath, m_assetFolderPath);
                }
                return m_name;
            }
        }

        public string assetPath {
            get { return m_assetPath; }
        }

        protected string assetFolderPath {
            get { return m_assetFolderPath; }
        }

        protected virtual string GetName(string assetPath, string assetFolderPath) {
            int startIndex = assetFolderPath.Length + 1;
            int lastIndex = assetPath.LastIndexOf(".");
            int length = lastIndex - startIndex;
            return assetPath.Substring(startIndex, length);
        }
    }
}
