using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LinkNote2.Service
{
    class CommonService
    {
        private static readonly Lazy<CommonService> _Instance = new Lazy<CommonService>(() => new CommonService());

        public static CommonService Instance
        {
            get
            {
                return _Instance.Value;
            }
        }

        public string HashMD5(Stream stream)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var hash = md5.ComputeHash(stream);
            return Bin2Hex(hash);
        }

        public string HashMD5(string data)
        {
            return HashMD5(Encoding.UTF8.GetBytes(data));
        }

        public string HashMD5(byte[] data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var result = md5.ComputeHash(data);
            return Bin2Hex(result);
        }

        public string Bin2Hex(byte[] data)
        {
            var sb = new StringBuilder(data.Length << 1);
            foreach (var b in data)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public byte[] Hex2Bin(string hex)
        {
            if ((hex.Length & 1) == 1)
            {
                throw new FormatException("Error hex data.");
            }
            byte[] buffer = new byte[hex.Length >> 1];
            for (int i = 0, mx = hex.Length >> 1; i < mx; i++)
            {
                buffer[i] = byte.Parse(hex.Substring(i << 1, 2), NumberStyles.HexNumber);
            }
            return buffer;
        }
    }
}
