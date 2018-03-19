using System;
using UI;

namespace Utility {
    public abstract class GroupAssetItem {
        /// <summary>
        /// 同类资源根文件夹,如 Image/ ,Effect/ 等
        /// </summary>
        public string assetCategoryPath = string.Empty;
        /// <summary>
        /// 子文件夹 如 Image/ 下 Bg/ ,Icon/ 等
        /// </summary>
        public string folder = string.Empty;

        protected Action m_callback;
        protected string m_fullPath;
        protected AssetBundleItem assetBundleItem;

        public virtual void Load() {
            assetBundleItem = AssetBundleUtility.Load(m_fullPath, "", false);
        }

        public virtual void LoadAsync(Action callback = null) {
            m_callback = callback;
            UICoroutine.instance.StartCoroutine(AssetBundleUtility.LoadAsync(m_fullPath, "", LoadAsyncCallback));
        }

        void LoadAsyncCallback(AssetBundleItem ab) {
            assetBundleItem = ab;
            if(m_callback != null) {
                m_callback();
            }
        }

        public void Destroy() {
            if(assetBundleItem != null) {
                AssetBundleUtility.Delete(m_fullPath);
                assetBundleItem = null;
            }
        }
    }
}