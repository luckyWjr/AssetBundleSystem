using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Utility;

namespace AssetBundle {
    public class BuildAssetBundleWindow : EditorWindow {
        Vector2 scrollPoint;
        List<string> m_log = new List<string>();

        [MenuItem("Custom/AssetBundle/BuildAll %A")]
        static void Create() {
            GetWindow<BuildAssetBundleWindow>("打包工具");
        }

        void OnGUI() {
            scrollPoint = GUILayout.BeginScrollView(scrollPoint);

            GUILayout.Label("设置：");
            BuildAssetBundleSetting.instance.selectedBuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("平台：", BuildAssetBundleSetting.instance.selectedBuildTarget, GUILayout.Width(320));

            BuildAssetBundleSetting.instance.isBuild = GUILayout.Toggle(BuildAssetBundleSetting.instance.isBuild, "打包");
            if (BuildAssetBundleSetting.instance.isBuild) {
                BuildAssetBundleSetting.instance.isForceRebuildAll = GUILayout.Toggle(BuildAssetBundleSetting.instance.isForceRebuildAll, "强制全部重新打包（否则为增量打包）");
            }

            GUILayout.Space(10);
            if (GUILayout.Button("执行", GUILayout.Width(200))) {
                Build();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("查看历史打包记录", GUILayout.Width(200))) {
                //m_mainThread = WatchLog();
            }

            if (m_log.Count > 0) {
                GUILayout.Space(10);
                if (GUILayout.Button("清除日志", GUILayout.Width(200))) {
                    m_log.Clear();
                }

                for (int i = 0; i < m_log.Count; i++) {
                    GUILayout.Label((i + 1) + ": " + m_log[i]);
                }
            }

            GUILayout.EndScrollView();
        }

        void Build() {
            IEnumerator etor = Execute(true);
            while (etor.MoveNext()) {
                // building...
                Debug.Log(etor.Current);
                AddLog((string)etor.Current);
            }
        }

        static IEnumerator Execute(bool showDialog) {
            if (showDialog) {
                //启用对话框，代码会停顿在此，等点击了才往下执行
                if (!EditorUtility.DisplayDialog("一键打包工具", "确定要执行吗？", "确定", "取消")) {
                    yield break;
                }
            }

            int count = 0;

            // 计时
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            #region 流程

            // 打包
            if (BuildAssetBundleSetting.instance.isBuild) {
                count++;

                //if (BuildAssetBundleSetting.instance.isForceRebuildAll && Directory.Exists(BuildAssetBundleConfig.buildingRootFolder)) {
                //    Directory.Delete(BuildAssetBundleConfig.buildingRootFolder, true);
                //}

                IEnumerator buildEtor = BuildAssetBundle.Build();
                while (buildEtor.MoveNext()) {
                    yield return buildEtor.Current;
                }
            }

            #endregion

            BuildAssetBundleSetting.instance.Save();

            stopwatch.Stop();

            int totalSeconds = (int)(stopwatch.ElapsedMilliseconds / 1000f);
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = totalSeconds % 60;
            string dialog = count > 0 ? string.Format("执行结束！耗时： {0} 分 {1} 秒。", minutes, seconds) : "什么也没发生...";
            yield return dialog;

            // 弹出结果框
            if (showDialog) {
                EditorUtility.DisplayDialog("完成", dialog, "确定");
            }
        }

        void AddLog(string log) {
            if (!string.IsNullOrEmpty(log)) {
                int maxLen = 1000;
                log = log.Length > maxLen ? log.Substring(0, maxLen) + "..." : log;
                m_log.Add(log);
                base.Repaint();
            }
        }
    }
}
