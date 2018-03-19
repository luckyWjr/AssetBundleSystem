
namespace Utility {
    public abstract class SingleClass<T> where T : new() {
        private static T m_instance;
        public static T instance {
            get {
                if (m_instance == null) {
                    m_instance = new T();
                }
                return m_instance;
            }
        }
    }
}
