using Geolocation.Constants;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Geolocation.Utilities.Encryption
{
    public static class DesEncryptor
    {
        private static readonly string _defaultIV;
        private static readonly string _defaultKey;
        private static readonly DESCryptoServiceProvider _cryptoServiceProvider;

        static DesEncryptor()
        {
            _defaultIV = Environment.GetEnvironmentVariable(EnvironmentVariablesNames.DES_DEFAULT_IV);
            _defaultKey = Environment.GetEnvironmentVariable(EnvironmentVariablesNames.DES_DEFAULT_KEY);

            _cryptoServiceProvider = new DESCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
            };
        }

        private static byte[] ToByteArrayOfNumLength(string str, int num)
        {
            string strOfNumLength = str.Length >= 8 ? str.Substring(0, num) : str.PadLeft(num, 'a');
            return Encoding.ASCII.GetBytes(strOfNumLength);
        }

        private static (byte[] bytesKey, byte[] bytesIv) ComputeKeyIvByteArrays(string key, string iv)
        {
            var bytesKey = ToByteArrayOfNumLength(key ?? _defaultKey, 8);
            var bytesIv = ToByteArrayOfNumLength(iv ?? _defaultIV, 8);
            return (bytesKey, bytesIv);
        }

        public static string EncryptData(string strData, string key = null, string iv = null)
        {
            try
            {
                var inputByteArray = Encoding.UTF8.GetBytes(strData);

                using (var memoryStream = new MemoryStream())
                {
                    (byte[] bytesKey, byte[] bytesIv) = ComputeKeyIvByteArrays(key, iv);
                    var encryptor = _cryptoServiceProvider.CreateEncryptor(bytesKey, bytesIv);
                    var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                    cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cryptoStream.FlushFinalBlock();
                    return Convert.ToBase64String(memoryStream.ToArray());//encrypted string
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static string DecryptData(string strData, string key = null, string iv = null)
        {
            try
            {
                var inputByteArray = Convert.FromBase64String(strData);

                using (var memoryStream = new MemoryStream())
                {
                    (byte[] bytesKey, byte[] bytesIv) = ComputeKeyIvByteArrays(key, iv);
                    var decryptor = _cryptoServiceProvider.CreateDecryptor(bytesKey, bytesIv);
                    var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);
                    cryptoStream.Write(inputByteArray, 0, inputByteArray.Length);
                    cryptoStream.FlushFinalBlock();
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
