using System.Security.Cryptography;
using System.Text;

namespace UserSyncAPI_Tomcat.Security
{
    public class Security
    {
        protected internal static UTF8Encoding utf8 = new UTF8Encoding();
        protected internal static TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
        protected internal static byte[] rgbKey = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        protected internal static byte[] rgbIV = { 65, 110, 68, 26, 69, 178, 200, 219 };

        protected internal static byte[] TransformCryptography(byte[] input, ICryptoTransform cryptoTransform)
        {
            // create the necessary streams
            using (MemoryStream memStream = new MemoryStream())
            {
                using (CryptoStream cryptStream = new CryptoStream(memStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    // transform the bytes as requested
                    cryptStream.Write(input, 0, input.Length);
                    cryptStream.FlushFinalBlock();

                    // Read the memory stream and convert it back into byte array
                    return memStream.ToArray();
                }
            }
        }
    }
}