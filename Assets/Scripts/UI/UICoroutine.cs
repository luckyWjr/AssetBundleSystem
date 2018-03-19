using UnityEngine;

namespace UI {

	public class UICoroutine : MonoBehaviour {

        static UICoroutine m_instance;

        public static UICoroutine instance {
            get {
                if(m_instance == null) {
                    GameObject go = new GameObject();
                    if(go != null) {
                        go.name = "UICoroutine";
                        go.AddComponent<UICoroutine>();
                    } else {
                        Debug.LogError("Init UICoroutine faild. GameObjet can not be null.");
                    }
                }
                return m_instance;
            }
        }

        void Awake() {
            DontDestroyOnLoad(gameObject);
            m_instance = this;
        }

        void OnDestroy() {
            m_instance = null;
        }
    }
}