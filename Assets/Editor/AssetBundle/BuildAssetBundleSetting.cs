using UnityEngine;
using Utility;
using UnityEditor;

namespace AssetBundle {
	public class BuildAssetBundleSetting : SingleClass<BuildAssetBundleSetting> {

        public BuildAssetBundleSetting() {
            Read();
        }

        BuildTarget m_selectedBuildTarget;
        string m_keyPrefix = "Project_BuildAssetBundle_";
        bool m_isBuild = true;
        bool m_isForceRebuildAll = false;

        public void Read() {
            m_selectedBuildTarget = EditorUserBuildSettings.activeBuildTarget;

            string str = PlayerPrefs.GetString(m_keyPrefix + "Build", m_isBuild.ToString());
            bool.TryParse(str, out m_isBuild);

            str = PlayerPrefs.GetString(m_keyPrefix + "ForceRebuildAll", m_isForceRebuildAll.ToString());
            bool.TryParse(str, out m_isForceRebuildAll);
        }

        public void Save() {
            PlayerPrefs.SetString(m_keyPrefix + "Build", isBuild.ToString());
            PlayerPrefs.SetString(m_keyPrefix + "ForceRebuildAll", isForceRebuildAll.ToString());
        }

        public bool isBuild {
            get { return m_isBuild; }
            set { m_isBuild = value; }
        }

        public bool isForceRebuildAll {
            get { return m_isForceRebuildAll; }
            set { m_isForceRebuildAll = value; }
        }

        public BuildTarget selectedBuildTarget {
            get { return m_selectedBuildTarget; }
            set { m_selectedBuildTarget = value; }
        }
    }
}
