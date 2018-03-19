using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Utility {
    /// <summary>
    /// 读取AB的工具类
    /// </summary>
	public class AssetBundleUtility {

        //public static readonly string suffix = ".assetbundle";
        public static readonly string streamingAssetsFileName = "StreamingAssets";

        static AssetBundleManifest m_mainfest;
        static AssetBundleManifest mainfest {
            get {
                if(m_mainfest == null) {
                    AssetBundle ab = AssetBundle.LoadFromFile(streamingAssetsPath + "/" + streamingAssetsFileName);
                    if(ab != null) {
                        m_mainfest = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");

                        ab.Unload(false);
                        ab = null;
                    } else {
                        Debug.LogError("Get Mainfest Error");
                        return null;
                    }
                }
                return m_mainfest;
            }
        }
        /// <summary>
        /// 缓存使用到的AssetBundle
        /// </summary>
        static Dictionary<string, AssetBundleItem> cacheAssetBundleItemDic = new Dictionary<string, AssetBundleItem>();

        /// <summary>
        /// 同步加载AB资源
        /// </summary>
        /// <param name="path">AB资源路径</param>
        /// <param name="fileName">AB资源文件名</param>
        /// <param name="isHasDependence">是否存在依赖关系</param>
        /// <returns>AB包信息</returns>
        public static AssetBundleItem Load(string path, string fileName, bool isHasDependence = true) {
            if(mainfest != null) {
                path = path.ToLower();
                fileName = fileName.ToLower();

                if(isHasDependence) {
                    //读取依赖
                    string[] dps = mainfest.GetAllDependencies(path);
                    int len = dps.Length;
                    for(int i = 0; i < len; i++) {
                        AssetBundleItem dItem;
                        if(cacheAssetBundleItemDic.ContainsKey(dps[i])) {
                            dItem = cacheAssetBundleItemDic[dps[i]];
                        } else {
                            dItem = new AssetBundleItem(dps[i], fileName, false);
                            dItem.assetBundle = AssetBundle.LoadFromFile(dItem.pathName);
                            cacheAssetBundleItemDic.Add(dps[i], dItem);
                        }
                        Debug.Log("wjr---ABLoad---Dependence---" + dps[i]);
                        dItem.refCount++;
                    }
                }
                
                AssetBundleItem ab;
                if(cacheAssetBundleItemDic.ContainsKey(path)) {
                    ab = cacheAssetBundleItemDic[path];
                } else {
                    ab = new AssetBundleItem(path, fileName, isHasDependence);
                    ab.assetBundle = AssetBundle.LoadFromFile(ab.pathName);
                    cacheAssetBundleItemDic.Add(path, ab);
                }
                Debug.Log("wjr---ABLoad---" + path);
                ab.refCount++;
                return ab;
            }
            return null;
        }

        /// <summary>
        /// 异步加载AB资源
        /// </summary>
        /// <param name="path">AB资源路径</param>
        /// <param name="fileName">AB资源文件名</param>
        /// <param name="isHasDependence">是否存在依赖关系</param>
        /// <returns>AB包信息</returns>
        public static IEnumerator LoadAsync(string path, string fileName, System.Action<AssetBundleItem> callback, bool isHasDependence = true) {
            if(mainfest != null) {
                path = path.ToLower();
                fileName = fileName.ToLower();

                if(isHasDependence) {
                    //读取依赖
                    string[] dps = mainfest.GetAllDependencies(path);
                    int len = dps.Length;
                    for(int i = 0; i < len; i++) {
                        AssetBundleItem dItem;
                        if(cacheAssetBundleItemDic.ContainsKey(dps[i])) {
                            dItem = cacheAssetBundleItemDic[dps[i]];
                        } else {
                            dItem = new AssetBundleItem(dps[i], fileName, false);
                            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(dItem.pathName);
                            yield return request;
                            dItem.assetBundle = request.assetBundle;
                            cacheAssetBundleItemDic[dps[i]] = dItem;
                        }
                        Debug.Log("wjr---ABLoadAsync---Dependence---" + dps[i]);
                        dItem.refCount++;
                    }
                }

                AssetBundleItem ab;
                if(cacheAssetBundleItemDic.ContainsKey(path)) {
                    ab = cacheAssetBundleItemDic[path];
                } else {
                    ab = new AssetBundleItem(path, fileName, isHasDependence);
                    AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(ab.pathName);
                    yield return request;
                    ab.assetBundle = request.assetBundle;
                    cacheAssetBundleItemDic[path] = ab;
                }
                Debug.Log("wjr---ABLoadAsync---" + path);
                ab.refCount++;
                if(callback != null) {
                    callback(ab);
                }
            }
        }

        /// <summary>
        /// AB的引用-1，若为0则删除该AB
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path) {
            path = path.ToLower();
            if(cacheAssetBundleItemDic.ContainsKey(path)) {
                AssetBundleItem ab = cacheAssetBundleItemDic[path];

                if(ab.isHasDependence) {
                    //删除依赖
                    string[] dps = mainfest.GetAllDependencies(path);
                    for(int i = 0, len = dps.Length; i < len; i++) {
                        Delete(dps[i]);
                    }
                }

                ab.refCount--;
                if(ab.refCount <= 0) {
                    Debug.Log("wjr---ABDelete---" + ab.pathName);
                    ab.assetBundle.Unload(true);
                    ab = null;
                    cacheAssetBundleItemDic.Remove(path);
                }
            }
        }

        private static StringBuilder getPathResult = new StringBuilder();
        private static string tmpPath = string.Empty;
        /// <summary>
        /// 资源同步加载路径（无 file:///）
        /// </summary>
        public static string GetAssetPath(string path) {
            // 先尝试从 persist 目录加载
            if(true) {
                getPathResult.Length = 0;
                getPathResult.Append(sandboxPath);
                getPathResult.Append("/");
                getPathResult.Append(path);
                tmpPath = getPathResult.ToString();
                if(File.Exists(tmpPath)) {
                    getPathResult.Length = 0;
                    return tmpPath;
                }
            }
            getPathResult.Length = 0;
            getPathResult.Append(streamingAssetsPath);
            getPathResult.Append("/");
            getPathResult.Append(path);
            tmpPath = getPathResult.ToString();
            return tmpPath;
        }

        /// <summary>
        /// 沙盒路径
        /// 可读可写，一般存放网上下载的资源
        /// </summary>
        public static string sandboxPath {
            get { return Application.persistentDataPath; }
        }

        /// <summary>
        /// StreamingAssets 路径
        /// </summary>
        public static string streamingAssetsPath {
            get {
#if UNITY_ANDROID
                return Application.dataPath + "!assets";   // 安卓平台
#else
                return Application.streamingAssetsPath;  // 其他平台
#endif
            }
            //get { return Application.streamingAssetsPath; }
        }
    }

    /// <summary>
    /// 存储单个AB资源信息
    /// </summary>
    public class AssetBundleItem {
        public string pathName;
        public string fileName;
        public AssetBundle assetBundle;
        public int refCount;
        public bool isHasDependence;

        public AssetBundleItem(string path, string file, bool isHasDependence) {
            pathName = AssetBundleUtility.GetAssetPath(path);
            fileName = file;
            assetBundle = null;
            refCount = 0;
            this.isHasDependence = isHasDependence;
        }

        public Object LoadAsset(System.Type type) {
            return LoadAsset(fileName, type);
        }

        public Object LoadAsset(string name, System.Type type) {
            if(assetBundle != null) {
                return assetBundle.LoadAsset(name, type);
            }
            return null;
        }
    }
}