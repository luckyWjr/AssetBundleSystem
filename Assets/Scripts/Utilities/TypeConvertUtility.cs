using System.Security.Cryptography;
using System.Text;

namespace Utility {

	public class TypeConvertUtility {

        static MD5 m_md5;
        static MD5 md5 {
            get {
                return m_md5 ?? (m_md5 = MD5.Create());
            }
        }

        /// <summary>
        /// btye数组转Hash
        /// </summary>
        /// <param name="buffer">数据流</param>
        /// <returns>Hash</returns>
        public static string ByteToHash(byte[] buffer) {
            if(buffer == null || buffer.Length < 1) {
                return "";
            }
            byte[] hash = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();

            foreach(var b in hash) {
                //x2 大写16进制
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}