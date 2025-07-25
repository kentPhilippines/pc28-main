using System;
using System.Security.Cryptography;
using System.Text;

namespace CommonLibrary
{
    public class AESUtil
    {
        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密文 注意：密文长度有 16，24，32，不能用其它长度的密文</param>
        /// <returns></returns>
        public static string AesEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            try
            {
                Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);
                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes(Get16BitLowerCaseMd5(key)),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = rm.CreateEncryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密文 注意：密文长度有 16，24，32，不能用其它长度的密文</param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            try
            {
                Byte[] toEncryptArray = Convert.FromBase64String(str);

                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes(Get16BitLowerCaseMd5(key)),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = rm.CreateDecryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }

        }

        /**
         * 生成16位的小写MD5值
         *
         * @return
         */
        public static string Get16BitLowerCaseMd5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(bytes);

                string hexString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                return hexString.Substring(8, 16).ToLowerInvariant();
            }
        }
    }
}
