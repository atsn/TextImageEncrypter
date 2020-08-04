using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TextImageEncryptor
{
    public static class Encryptor
    {

        public static string Decrypt(string password, byte[] encryptedText)
        {
            byte[] decrypted;
            int decryptedLenth;

            using (var decryptor = GetEncryptorOrDecryptor(password, EncryptionDirection.Decrypt))
            {
                using (MemoryStream from = new MemoryStream(encryptedText))
                {
                    using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                    {
                        decrypted = new byte[encryptedText.Length];

                        decryptedLenth = reader.Read(decrypted, 0, decrypted.Length);

                    }
                }
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedLenth);
        }

        public static byte[] Encrypt(string password, string text)
        {
            var textAsBytes = Encoding.UTF8.GetBytes(text);
            byte[] output;

            using (var encryptor = GetEncryptorOrDecryptor(password, EncryptionDirection.Encrypt))
            {
                using (MemoryStream to = new MemoryStream())
                {
                    using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                    {
                        writer.Write(textAsBytes, 0, textAsBytes.Length);
                        writer.FlushFinalBlock();
                        output = to.ToArray();
                    }
                }
            }

            return output;
        }

        private static ICryptoTransform GetEncryptorOrDecryptor(string password, EncryptionDirection direction)
        {
            Rfc2898DeriveBytes _passwordBytes =
                new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("I Dont Care About Salt In This Case"), 1000,
                    HashAlgorithmName.SHA512);
            byte[] keyBytes = _passwordBytes.GetBytes(32);
            AesManaged managed = new AesManaged
            {
                KeySize = 256,
                Padding = PaddingMode.Zeros,
                Mode = CipherMode.CBC
            };
            switch (direction)
            {
                case EncryptionDirection.Encrypt:
                    return managed.CreateEncryptor(keyBytes, Encoding.UTF8.GetBytes("I Dont Care About Vector Bytes In This Case", 0, managed.BlockSize / 8));
                case EncryptionDirection.Decrypt:
                    return managed.CreateDecryptor(keyBytes, Encoding.UTF8.GetBytes("I Dont Care About Vector Bytes In This Case", 0, managed.BlockSize / 8));
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

        }

    }
}