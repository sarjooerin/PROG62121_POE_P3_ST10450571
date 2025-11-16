using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PROG6212_POE_P3.Services
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("Your32CharLongEncryptionKey123!"); // 32 chars
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("Your16CharIV1234"); // 16 chars

        public static string EncryptFile(byte[] fileContent)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(fileContent, 0, fileContent.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static byte[] DecryptFile(string encryptedBase64)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedBase64);
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
            return ms.ToArray();
        }
    }
}
