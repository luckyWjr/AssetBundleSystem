using System;
using System.IO;

namespace AssetBundle {
    public class GroupAssetBundle<T> : BaseAssetBundle where T : UnityEngine.Object {
        string m_parentPath;//资源根目录
        string m_groupFolderPath;//文件夹目录
        string[] m_assetPathArray;//文件夹内的所有资源路径

        string m_groupName;//ab 名称(文件夹名称)

        /// <param name="parentPath">资源根目录</param>
        /// <param name="folderPath">文件夹目录</param>
        /// <param name="assetPathArray">文件夹内的所有资源路径</param>
        /// <param name="outputFolderName">输出目录</param>
        public GroupAssetBundle(string parentPath, string folderPath, string[] assetPathArray, string outputFolderName)
            : base(outputFolderName) {
            if(string.IsNullOrEmpty(folderPath) || !folderPath.StartsWith("Assets")) {
                throw new ArgumentException("folderPath");
            }
            if(assetPathArray == null) {
                throw new ArgumentNullException("assetPathArray");
            }
            if(assetPathArray.Length < 1) {
                throw new ArgumentException("assetPathArray.Length < 1");
            }

            m_parentPath = parentPath;
            m_assetPathArray = assetPathArray;
            m_groupName = Path.GetFileNameWithoutExtension(folderPath);
        }

        public override string[] assetNames {
            get {
                if(m_assetPathArray.Length > 0) {
                    return m_assetPathArray;
                }
                return null;
            }
        }

        public override string name {
            get {
                return m_groupName;
            }
        }
    }
}
