namespace UserSyncAPI_Tomcat.Security
{
    public static class SecurityExtensions
    {
        public static string Encrypt(string plainText)
        {
            byte[] input = Security.utf8.GetBytes(plainText);
            byte[] output = Security.TransformCryptography(input, Security.des.CreateEncryptor(Security.rgbKey, Security.rgbIV));
            return Convert.ToBase64String(output);
        }

        public static string Decrypt(this string encryptedText)
        {
            byte[] input = Convert.FromBase64String(encryptedText);
            byte[] output = Security.TransformCryptography(input, Security.des.CreateDecryptor(Security.rgbKey, Security.rgbIV));
            return Security.utf8.GetString(output);
        }
    }
}