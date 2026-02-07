using System;
using System.Security.Cryptography;
using System.Text;

namespace MoreAutomation.Application.Services
{
    /// <summary>
    /// 简单的数据加密服务，基于 AES 对称加密。
    /// 用于加密敏感的账号密码信息。
    /// </summary>
    public class DataEncryptionService
    {
        // 静态加密密钥（基于产品名称，可替换为更安全的机制）
        private static readonly byte[] StaticKey = Encoding.UTF8.GetBytes("MoreAutomation12345678"); // 24 字节用于 TripleDES 或 32 字节用于 AES-256
        private static readonly byte[] StaticIv = Encoding.UTF8.GetBytes("AutomationIV123"); // 16 字节

        /// <summary>
        /// 加密字符串。
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = StaticKey;
                    aes.IV = StaticIv;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        using (var ms = new System.IO.MemoryStream())
                        {
                            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            {
                                using (var sw = new System.IO.StreamWriter(cs))
                                {
                                    sw.Write(plainText);
                                }
                                return Convert.ToBase64String(ms.ToArray());
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果加密失败，返回原文本（降级处理）
                return plainText;
            }
        }

        /// <summary>
        /// 解密字符串。
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = StaticKey;
                    aes.IV = StaticIv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        var buffer = Convert.FromBase64String(cipherText);
                        using (var ms = new System.IO.MemoryStream(buffer))
                        {
                            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                            {
                                using (var sr = new System.IO.StreamReader(cs))
                                {
                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果解密失败，返回密文（可能已是明文或格式有误）
                return cipherText;
            }
        }
    }
}
