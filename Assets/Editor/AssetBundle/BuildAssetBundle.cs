using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Utility;

namespace AssetBundle {
    public static class BuildAssetBundle {

        /// <summary>
        /// 打包AeestBundle和配置信息等到与Asset同级的AssetBundle目录下
        /// </summary>
        /// <returns></returns>
        public static IEnumerator Build() {
            yield return "开始打包...";

            // 1. 读取打包资源列表配置
            yield return "读取所有需要打包的资源";
            List<BaseAssetBundleManager> managerList = BuildAssetBundleConfig.GetAssetManagerList();
            if (managerList == null || managerList.Count < 1) {
                string str = "无任何要打包的资源，请检查配置是否正确。";
                yield return str;
                EditorUtility.DisplayDialog("错误", str, "确定");
                yield break;
            }

            StringBuilder sb = new StringBuilder("资源列表如下：\n");
            foreach (var manager in managerList) {
                sb.AppendLine(manager.ToString());
            }
            yield return sb.ToString();

            // 2.打包
            yield return "打包中..." + BuildAssetBundleConfig.buildingRootFolder;

            BuildAssetBundles(BuildAssetBundleConfig.buildingAssetBundlesFolder, managerList);

            // 打包完成事件
            foreach(var manager in managerList) {
                manager.OnBuildFinished();
            }
            yield return "打包完成！";

            // 3. 计算现在的Hash...
            yield return "计算资源的Hash...";
            foreach(var manager in managerList) {
                manager.ComputeHash();
            }

            // 4. 对比上次打包的结果，寻找差异包
            yield return "读取上次包的Hash...";
            List<string> lastAssetBundleConfigList = null;
            List<string> newAssetBundleList = new List<string>();
            if(File.Exists(BuildAssetBundleConfig.buildingListPath)) {
                string[] lastBuildAssetBundleConfig = File.ReadAllLines(BuildAssetBundleConfig.buildingListPath);
                if(lastBuildAssetBundleConfig != null && lastBuildAssetBundleConfig.Length > 0) {
                    lastAssetBundleConfigList = new List<string>(lastBuildAssetBundleConfig);
                    int findIndex;
                    foreach(var manager in managerList) {
                        foreach(var item in manager.items) {
                            findIndex = item.ReadConfig(lastAssetBundleConfigList);
                            if(findIndex >= 0) {
                                //找到对应文件下标则删除该列数据
                                lastAssetBundleConfigList.RemoveAt(findIndex);
                            } else {
                                //没找到说明是新增的
                                newAssetBundleList.Add(item.assetBundleName);
                            }
                        }
                    }
                }
            } else {
                yield return "这是第一次打包。";

                //全为新增
                foreach(var manager in managerList) {
                    foreach(var item in manager.items) {
                        newAssetBundleList.Add(item.assetBundleName);
                    }
                }
            }

            bool isVersionChanged = false;

            // 5.列举新增的包
            sb.Length = 0;
            if(newAssetBundleList.Count > 0) {
                sb.AppendLine("新增的资源如下：");
                isVersionChanged = true;
                foreach(string path in newAssetBundleList) {
                    sb.AppendLine(path);
                }
            } else {
                sb.AppendLine("没有新增的资源");
            }


            // 5.寻找差异包
            List<string> differentAssetBundleList = new List<string>();

            foreach(var manager in managerList) {
                differentAssetBundleList.AddRange(manager.GetDifferentAssetBundleList());
            }
            if(differentAssetBundleList.Count > 0) {
                isVersionChanged = true;
                sb.AppendLine("有变化的资源如下：");
                foreach(string path in differentAssetBundleList) {
                    sb.AppendLine(path);
                }
            } else {
                sb.AppendLine("没有有变化的资源");
            }

            //CopyDifferentFileToDifferentFolder(BuildAssetBundleConfig.buildingAssetBundlesFolder, BuildAssetBundleConfig.differentBuildingAssetBundlesFolder, newAssetBundleList, differentAssetBundleList);

            // 6. 删除多余的包
            //lastBuildAssetBundleConfigList还未删除的数据说明已经没有这些资源需要打包，即要删掉对应的AB包
            List<string> removedList = GetFileNeedRemoveArray(lastAssetBundleConfigList);
            if(removedList.Count > 0) {
                isVersionChanged = true;
                sb.AppendLine("删除的资源如下：");
                foreach(string path in removedList) {
                    sb.AppendLine(path);
                }
            } else {
                sb.AppendLine("没有要删除的资源");
            }
            DeleteRemovedAssetBundles(removedList);

            string buildResult = sb.ToString();
            yield return buildResult;

            // 6. 生成列表文件，版本文件，日志文件
            yield return "生成增量文件...";
            if(isVersionChanged) {
                SaveBuildingListFile(managerList, BuildAssetBundleConfig.buildingListPath, ref sb);        // 供打包用的列表文件
                int version = UpdateVersion();
                WriteVersionFile(BuildAssetBundleConfig.buildingVersionPath, version);                     // 供打包用的版本文件
            }

            // 供下载用的版本文件（存本地版本号，用于和服务器上最新版本号对比）
            string strVersion = SaveLoadingVersionFile(BuildAssetBundleConfig.tempLoadingVersionPath);
            yield return "最新版本为： " + strVersion;

            // 日志文件
            if(isVersionChanged) {
                SaveLog(buildResult, strVersion, BuildAssetBundleConfig.buildingLogPath);
            }

            // 供下载用的列表文件
            SaveLoadingListFile(managerList, BuildAssetBundleConfig.tempLoadingListPath);

            // dispose
            foreach(var manager in managerList) {
                manager.Dispose();
            }

            yield return "导入主工程...";

        }

        static void BuildAssetBundles(string folder, List<BaseAssetBundleManager> managerList) {
            
            if(Directory.Exists(folder)) {
                if(BuildAssetBundleSetting.instance.isForceRebuildAll) {
                    Directory.Delete(folder, true);
                    Directory.CreateDirectory(folder);
                }
            } else {
                Directory.CreateDirectory(folder);
            }

            // 准备打包
            List<AssetBundleBuild> list = new List<AssetBundleBuild>();
            foreach(var manager in managerList) {
                    //var noPackAssets = manager as NotBuildAssetBundleManager;
                    //if(noPackAssets != null) {
                    //    // 无需打包的资源
                    //    noPackAssets.Build(BuildAssetBundleConfig.differentBuildingAssetBundlesFolder);
                    //} else {
                        manager.PrepareBuild();
                        list.AddRange(manager.assetBundleBuilds);
                    //}
            }
            AssetDatabase.Refresh();

            BuildPipeline.BuildAssetBundles(folder, list.ToArray(), BuildAssetBundleConfig.buildingOptions, BuildAssetBundleSetting.instance.selectedBuildTarget);

            // 删除所有的.manifest
            //string[] manifestFiles = Directory.GetFiles(folder, "*.manifest", SearchOption.AllDirectories);
            //foreach(var mf in manifestFiles) {
            //    File.Delete(mf);
            //}

            FileUtility.CopyFiles(BuildAssetBundleConfig.buildingAssetBundlesFolder, string.Format("{0}/StreamingAssets", Application.dataPath), ".manifest");
        }

        /// <summary>
        /// 将新增差异的AB放入单独的文件夹
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="newList">新增文件列表</param>
        /// <param name="differentList">差异文件列表</param>
        static void CopyDifferentFileToDifferentFolder(string fromFolderPath, string toFolderPath, List<string> newList, List<string> differentList) {
            if(Directory.Exists(toFolderPath)) {
                Directory.Delete(toFolderPath, true);
            }
            Directory.CreateDirectory(toFolderPath);

            int i = 0, j = 0;
            fromFolderPath += "/";
            toFolderPath += "/";
            for(j = newList.Count; i < j; i++) {
                FileUtility.CopyFile(fromFolderPath + newList[i], toFolderPath + newList[i]);
            }
            for(i = 0, j = differentList.Count; i < j; i++) {
                FileUtility.CopyFile(fromFolderPath + differentList[i], toFolderPath + differentList[i]);
            }
        }

        /// <summary>
        /// 获取需要删除的AB列表
        /// </summary>
        /// <param name="lastBuildAssetBundleConfig">剩下的表数据</param>
        /// <returns>要删除的AB文件路径数组</returns>
        static List<string> GetFileNeedRemoveArray(List<string> lastBuildAssetBundleConfig) {
            List<string> removedList = new List<string>();
            string relativePath, hash;
            if(lastBuildAssetBundleConfig != null) {
                foreach(var line in lastBuildAssetBundleConfig) {
                    BaseAssetBundle.ParseConfigLine(line, out relativePath, out hash);
                    removedList.Add(relativePath);
                }
            }
            return removedList;
        }

        /// <summary>
        /// 删除不需要的AB
        /// </summary>
        static void DeleteRemovedAssetBundles(List<string> removedList) {
            for (int i = 0; i < removedList.Count; i++) {
                string fullPath = string.Format("{0}/{1}", BuildAssetBundleConfig.buildingAssetBundlesFolder, removedList[i]);
                if(Directory.Exists(Path.GetDirectoryName(fullPath))) {
                    File.Delete(fullPath);
                }
            }
        }

        static bool TryReadBuildingVersion(out int version) {
            string path = BuildAssetBundleConfig.buildingVersionPath;
            if (File.Exists(path)) {
                string str = File.ReadAllText(path);
                if (int.TryParse(str, out version)) {
                    return true;
                }
            }

            version = 0;
            return false;
        }

        static int UpdateVersion() {
            int version;
            TryReadBuildingVersion(out version);
            version++;
            return version;
        }

        static void WriteVersionFile(string path, int version) {
            File.WriteAllText(path, version.ToString());
        }

        static string SaveLoadingVersionFile(string path) {
            int version;
            TryReadBuildingVersion(out version);
            FileUtility.CreateDirectory(path);
            File.WriteAllText(path, version.ToString());
            return version.ToString();
        }

        static void SaveLoadingListFile(List<BaseAssetBundleManager> managerList, string path) {
            StringBuilder sb = new StringBuilder();

            string itemPath, name, Hash;
            int startIndex;
            byte[] bytes;

            for (int i = 0, j = 0; i < managerList.Count; i++) {
                for (j = 0; j < managerList[i].items.Length; j++) {
                    var item = managerList[i].items[j];
                    
                    if (item.flag == AssetFlag.NoChange) {
                        itemPath = string.Format("{0}/{1}", BuildAssetBundleConfig.buildingAssetBundlesFolder, item.assetBundleName);
                    } else {
                        itemPath = string.Format("{0}/{1}", BuildAssetBundleConfig.differentBuildingAssetBundlesFolder, item.assetBundleName);
                    }

                    startIndex = BuildAssetBundleConfig.buildingAssetBundlesFolder.Length + 1;
                    name = itemPath.Substring(startIndex);
                    bytes = null;
                    bytes = File.ReadAllBytes(itemPath);
                    Hash = TypeConvertUtility.ByteToHash(bytes);

                    sb.AppendFormat("{0},{1},{2};", name, Hash, bytes.Length);
                }
            }

            File.WriteAllText(path, sb.ToString());
        }

        static void SaveBuildingListFile(List<BaseAssetBundleManager> managerList, string path, ref StringBuilder sb) {
            sb.Length = 0;
            foreach (var manager in managerList) {
                foreach (var item in manager.items) {
                    string config = item.GenerateConfigLine();
                    sb.AppendLine(config);
                }
            }
            FileUtility.CreateDirectory(path);
            File.WriteAllText(path, sb.ToString());
        }

        static void SaveLog(string log, string version, string path) {
            if(string.IsNullOrEmpty(log)) {
                return;
            }

            string oldLog = "";
            if(File.Exists(path)) {
                oldLog = File.ReadAllText(path);
            }
            string currentLog = string.Format("打包时间：{0}\n版本：{1}\n{2}\n--------------------------------\n\n{3}", DateTime.Now, version, log, oldLog);
            File.WriteAllText(path, currentLog);
        }

    }
}
