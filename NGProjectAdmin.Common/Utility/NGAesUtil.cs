﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NGProjectAdmin.Common.Utility
{
    /// <summary>
    /// AES加解密工具
    /// </summary>
    public static class NGAesUtil
    {
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="value">加密串</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量</param>
        /// <returns>加密后的字符串</returns>
        public static String Encrypt(String value, String key, String iv = "")
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            if (key == null)
            {
                throw new Exception("未将对象引用设置到对象的实例。");
            }

            if (key.Length < 16)
            {
                throw new Exception("指定的密钥长度不能少于16位。");
            }

            if (key.Length > 32)
            {
                throw new Exception("指定的密钥长度不能多于32位。");
            }

            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            {
                throw new Exception("指定的密钥长度不明确。");
            }

            if (!String.IsNullOrEmpty(iv))
            {
                if (iv.Length < 16)
                {
                    throw new Exception("指定的向量长度不能少于16位。");
                }
            }

            var _keyByte = Encoding.UTF8.GetBytes(key);
            var _valueByte = Encoding.UTF8.GetBytes(value);

            using (var aes = Aes.Create())
            {
                aes.IV = !String.IsNullOrEmpty(iv) ?
                    Encoding.UTF8.GetBytes(iv) :
                    Encoding.UTF8.GetBytes(key.Reverse().ToString().ToUpper().Substring(0, 16));
                aes.Key = _keyByte;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var cryptoTransform = aes.CreateEncryptor();
                var resultArray = cryptoTransform.TransformFinalBlock(_valueByte, 0, _valueByte.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="value">解密串</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量</param>
        /// <returns></returns>
        public static String Decrypt(String value, String key, String iv = "")
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            if (key == null)
            {
                throw new Exception("未将对象引用设置到对象的实例。");
            }

            if (key.Length < 16)
            {
                throw new Exception("指定的密钥长度不能少于16位。");
            }

            if (key.Length > 32)
            {
                throw new Exception("指定的密钥长度不能多于32位。");
            }

            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            {
                throw new Exception("指定的密钥长度不明确。");
            }

            if (!String.IsNullOrEmpty(iv))
            {
                if (iv.Length < 16)
                {
                    throw new Exception("指定的向量长度不能少于16位。");
                }
            }

            var _keyByte = Encoding.UTF8.GetBytes(key);
            var _valueByte = Convert.FromBase64String(value);

            using (var aes = Aes.Create())
            {
                aes.IV = !String.IsNullOrEmpty(iv) ?
                    Encoding.UTF8.GetBytes(iv) :
                    Encoding.UTF8.GetBytes(key.Reverse().ToString().ToUpper().Substring(0, 16));
                aes.Key = _keyByte;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                var cryptoTransform = aes.CreateDecryptor();
                var resultArray = cryptoTransform.TransformFinalBlock(_valueByte, 0, _valueByte.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
        }
    }
}
