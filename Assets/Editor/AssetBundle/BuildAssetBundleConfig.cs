using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetBundle {
    /// <summary>
    /// 编AssetBundle的一些配置信息
    /// </summary>
    public static class BuildAssetBundleConfig {

        static BuildAssetBundleConfig() {
            
        }

        public static List<BaseAssetBundleManager> GetAssetManagerList() {
            IEnumerable<BaseAssetBundleManager> config = GetAssetBundleManagerList();
            return config != null ? config.ToList() : null;
        }

        static IEnumerable<BaseAssetBundleManager> GetAssetBundleManagerList() {
            yield return new NormalAssetBundleManager<Texture>("Assets/Res/Images", "t:Texture2D", "Images");
            yield return new GroupAssetBundleManager<Sprite>("Assets/Res/Sprites", "t:Sprite", "Sprites");
            yield return new NormalAssetBundleManager<UnityEngine.Object>("Assets/Res/UIPrefabs", "t:Prefab", "UIPrefabs");
        }

        public static string finalLoadingVersionName {
            get { return "version.dat"; }
        }

        public static string GetLoadingListName() {
            return "mainlist.dat";
        }

        public static string GetLoadingVersionName() {
            return "mainversion.dat";
        }


        #region 存放完整的AB资源
        public static string buildingRootFolder {
            get {
                string folder = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
                string path = string.Format("{0}/AssetBundles/{1}", folder, platformFolderName);
                return path.Replace("\\", "/");
            }
        }

        public static string buildingAssetBundlesFolder {
            get { return buildingRootFolder + "/StreamingAssets"; }
        }

        public static string buildingVersionPath {
            get { return buildingRootFolder + "/version.txt"; }
        }

        public static string buildingListPath {
            get { return buildingRootFolder + "/list.csv"; }
        }

        public static string buildingLogPath {
            get { return buildingRootFolder + "/log.txt"; }
        }

        public static string loadingVersionPath {
            get {
                string name = GetLoadingVersionName();
                return string.Format("{0}/{1}", buildingAssetBundlesFolder, name);
            }
        }

        public static string loadingListPath {
            get { return string.Format("{0}/{1}", buildingAssetBundlesFolder, GetLoadingListName()); }
        }
        #endregion

        #region different 用于存放差异包
        public static string differentBuildingRootFolder {
            get {
                string folder = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
                string path = string.Format("{0}/AssetBundles/{1}_different", folder, platformFolderName);
                return path.Replace("\\", "/");
            }
        }

        public static string differentBuildingAssetBundlesFolder {
            get { return differentBuildingRootFolder + "/StreamingAssets"; }
        }

        public static string differentBuildingVersionPath {
            get { return differentBuildingRootFolder + "/version.txt"; }
        }

        public static string differentBuildingListPath {
            get { return differentBuildingRootFolder + "/list.csv"; }
        }

        public static string differentBuildingLogPath {
            get { return differentBuildingRootFolder + "/log.txt"; }
        }

        public static string tempLoadingVersionPath {
            get {
                string name = GetLoadingVersionName();
                return string.Format("{0}/{1}", differentBuildingAssetBundlesFolder, name);
            }
        }

        public static string tempLoadingListPath {
            get { return string.Format("{0}/{1}", differentBuildingAssetBundlesFolder, GetLoadingListName()); }
        }

#endregion

        public static string platformFolderName {
            get {
                string folder;
                switch (BuildAssetBundleSetting.instance.selectedBuildTarget) {
                    case BuildTarget.Android:
                    folder = "Android";
                    break;
                    case BuildTarget.iOS:
                    folder = "IOS";
                    break;
                    default:
                    folder = "PC";
                    break;
                }

                return folder;
            }
        }

        public static BuildAssetBundleOptions buildingOptions {
            get {
                BuildAssetBundleOptions options;
                if(BuildAssetBundleSetting.instance.isForceRebuildAll) {
                    options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle;
                } else {
                    options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle;
                }
                return options;
            }
        }
    }
}
