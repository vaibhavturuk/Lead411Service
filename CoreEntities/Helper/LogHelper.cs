using System.Security.Cryptography;
using System.Text;

namespace CoreEntities.Helper
{
    public class LogHelper
    {
        /// <summary>
        /// Return hash for string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetSha1HashData(string data)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] hashData = sha1.ComputeHash(Encoding.Default.GetBytes(data));
            StringBuilder returnValue = new StringBuilder();
            foreach (byte t in hashData)
            {
                returnValue.Append(t.ToString());
            }
            return returnValue.ToString();
        }
    }
}
