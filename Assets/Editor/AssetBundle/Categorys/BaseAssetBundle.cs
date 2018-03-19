using System;
using System.Collections.Generic;
using System.IO;
using Utility;
using UnityEditor;

namespace AssetBundle {
    public enum AssetFlag {
        NoChange,
        NewAdded,
        Modified,
    }

    /// <summary>
    /// 单个的ab包
    /// </summary>
    public abstract class BaseAssetBundle {

        /// <summary>
        /// AB输出文件夹名称 如a/b
        /// </summary>
        string m_outputFolderName;
        string m_assetBundleFileFullPath;
        string m_currentHash;

        //存放临时变量
        int m_i, m_len;
        string m_tempString;

        AssetFlag m_flag = AssetFlag.NoChange;

        public BaseAssetBundle(string outputFolderName) {
            if(string.IsNullOrEmpty(outputFolderName)) {
                throw new ArgumentException("outputFolder");
            }

            m_outputFolderName = outputFolderName.TrimStart('/').TrimEnd('/');
            m_outputFolderName = m_outputFolderName.ToLower();
        }

        #region 属性

        /// <summary>
        /// 资源文件名 不带扩展名
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// 资源文件名，带扩展名
        /// </summary>
        public string fullName {
            get { return name + ext; }
        }

        /// <summary>
        /// AB 的包名 如：a/b/c.d
        /// </summary>
        public string assetBundleName {
            get {
                string path = m_outputFolderName.Contains("{0}") ? string.Format(m_outputFolderName, fullName) : string.Format("{0}/{1}", m_outputFolderName, fullName);
                return path.ToLower();
            }
        }

        /// <summary>
        /// AB 的全路径 c:/a/b.c
        /// </summary>
        public string assetBundleFileFullPath {
            get {
                if(string.IsNullOrEmpty(m_assetBundleFileFullPath)) {
                    m_assetBundleFileFullPath = string.Format("{0}/{1}", BuildAssetBundleConfig.buildingAssetBundlesFolder, assetBundleName);
                }
                return m_assetBundleFileFullPath;
            }
        }

        public string outputFolderName {
            get {
                return m_outputFolderName;
            }
        }

        public string lastHash;

        public string currentHash {
            get {
                if(string.IsNullOrEmpty(m_currentHash)) {
                    m_currentHash = ComputeHash();
                }
                return m_currentHash;
            }
        }

        public virtual string ext {
            get { return ".u"; }
        }

        public AssetFlag flag {
            get {
                return m_flag;
            }
            set {
                m_flag = value;
            }
        }

        /// <summary>
        /// AB里面资源的名称（即路径）
        /// </summary>
        public abstract string[] assetNames { get; }
        #endregion

        public static void ParseConfigLine(string configLine, out string relativePath, out string hash) {
            string[] words = configLine.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            relativePath = words[0];
            hash = words[1];
        }

        /// <summary>
        /// 获取AB文件的btye[]
        /// </summary>
        /// <param name="assetPath">AB全路径</param>
        /// <returns>Asset和.meta文件的btye[]</returns>
        public byte[] ReadAssetBundleBytes() {
            if(!File.Exists(assetBundleFileFullPath)) {
                return null;
            }

            byte[] list = File.ReadAllBytes(assetBundleFileFullPath);

            return list;
        }

        /// <summary>
        /// 计算AB资源的Hash码
        /// </summary>
        public string ComputeHash() {
            byte[] buffer = ReadAssetBundleBytes();
            if(buffer != null) {
                m_currentHash = TypeConvertUtility.ByteToHash(buffer);
            } else {
                m_currentHash = "";
            }
            return m_currentHash;
        }

        /// <summary>
        /// 生成一条配置
        /// </summary>
        public string GenerateConfigLine() {
            return string.Format("{0},{1},", assetBundleName, currentHash);
        }

        /// <summary>
        /// 读取配置，从中找出属于自己的那条
        /// </summary>
        /// <param name="configLines">配置数组</param>
        /// <returns>对应下标，若没有返回-1</returns>
        public int ReadConfig(List<string> configLines) {
            for(m_i = 0, m_len = configLines.Count; m_i < m_len; m_i++) {
                if(RightConfig(configLines[m_i])) {
                    ParseConfigLine(configLines[m_i], out m_tempString, out lastHash);
                    return m_i;
                }
            }
            return -1;
        }

        public bool RightConfig(string configLine) {
            return configLine.StartsWith(assetBundleName + ",");
        }

        public virtual void Dispose() {

        }
    }
}
