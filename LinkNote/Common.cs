using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Muscipular.LinkNote
{
    //static class EncryptCode
    //{
    //    static uint[] _CRCTable = new uint[256];
    //    static byte[][] _DataTable = new byte[256][];
    //    static EncryptCode()
    //    {
    //        InitDataTable();
    //        InitCRCTable();
    //    }

    //    private static void InitDataTable()
    //    {
    //        for (int i = 0; i < 256; i++)
    //        {
    //            _DataTable[i] = new byte[256];
    //            for (int j = 0; j < 256; j++)
    //            {
    //                _DataTable[i][j] = (byte)(i ^ j);
    //            }
    //        }
    //    }
    //    public static bool Encrypt(byte[] rawdata, out byte[] encrypted, string password)
    //    {
    //        encrypted = null;
    //        if (rawdata == null || rawdata.Length <= 0)
    //            return false;
    //        if (string.IsNullOrEmpty(password))
    //            return false;
    //        var pwd = CommonHelper.GetData(password);
    //        var pwdLength = pwd.Length;
    //        var data_count = rawdata.Length;
    //        encrypted = new byte[data_count + 12];
    //        DateTime time = DateTime.Now;
    //        uint crc_base = (uint)(time.Millisecond | time.Second << 16);
    //        for (int i = 0; i < pwdLength; ++i)
    //            pwd[i] = _DataTable[pwd[i]][0x37];//(byte)(pwd[i] ^ 0x37);
    //        uint crc = crc_base ^ pwd[0];
    //        encrypted[0] = (byte)(((crc) >> 24));
    //        encrypted[1] = (byte)(((crc) >> 16));
    //        encrypted[2] = (byte)(((crc) >> 8));
    //        encrypted[3] = (byte)(((crc)));
    //        for (int i = 0; i < data_count; i++)
    //        {
    //            int index = i % (pwdLength);
    //            encrypted[i + 4] = _DataTable[rawdata[i]][pwd[index]];//(byte)(rawdata[i] ^ pwd[index]);
    //        }
    //        crc = HashCRC32(crc_base, rawdata, data_count);
    //        encrypted[data_count + 4] = (byte)((crc >> 24));
    //        encrypted[data_count + 5] = (byte)((crc >> 16));
    //        encrypted[data_count + 6] = (byte)((crc >> 8));
    //        encrypted[data_count + 7] = (byte)((crc));
    //        crc = HashCRC32(crc_base, encrypted, data_count + 8);
    //        encrypted[data_count + 8] = (byte)((crc >> 24));
    //        encrypted[data_count + 9] = (byte)((crc >> 16));
    //        encrypted[data_count + 10] = (byte)((crc >> 8));
    //        encrypted[data_count + 11] = (byte)((crc));
    //        return true;
    //    }

    //    public static bool Decrypt(byte[] encrypted, out byte[] rawdata, string password)
    //    {
    //        rawdata = null;
    //        if (encrypted == null || encrypted.Length <= 12)
    //            return false;
    //        rawdata = new byte[encrypted.Length - 12];
    //        if (string.IsNullOrEmpty(password))
    //            return false;
    //        var dataLength = encrypted.Length;
    //        var pwd = CommonHelper.GetData(password);
    //        var pwdLength = pwd.Length;
    //        for (int i = 0; i < pwdLength; ++i)
    //            pwd[i] = _DataTable[pwd[i]][0x37];// (byte)(pwd[i] ^ 0x37);
    //        uint crc = (uint)(encrypted[0] << 24 | encrypted[1] << 16 | encrypted[2] << 8 | encrypted[3]);
    //        uint crc_base = crc ^ pwd[0];
    //        crc = HashCRC32(crc_base, encrypted, dataLength - 4);
    //        rawdata[0] = (byte)(((crc) >> 24));
    //        rawdata[1] = (byte)(((crc) >> 16));
    //        rawdata[2] = (byte)(((crc) >> 8));
    //        rawdata[3] = (byte)(((crc)));
    //        if (crc != (uint)(encrypted[dataLength - 4] << 24 | encrypted[dataLength - 3] << 16 | encrypted[dataLength - 2] << 8 | encrypted[dataLength - 1]))
    //        {
    //            return false;
    //        }
    //        for (int i = 4; i < dataLength - 8; i++)
    //        {
    //            int index = (i - 4) % (pwdLength);
    //            rawdata[i - 4] = _DataTable[encrypted[i]][pwd[index]];//(byte)(encrypted[i] ^ pwd[index]);
    //        }
    //        crc = HashCRC32(crc_base, rawdata, rawdata.Length);
    //        if (crc != (uint)(encrypted[dataLength - 8] << 24 | encrypted[dataLength - 7] << 16 | encrypted[dataLength - 6] << 8 | encrypted[dataLength - 5]))
    //            return false;
    //        return true;
    //    }

    //    static void InitCRCTable()
    //    {
    //        for (uint i = 0; i < 256; i++)
    //        {
    //            uint c = i;
    //            for (uint j = 0; j < 8; j++)
    //            {
    //                if ((c & 1) == 1)
    //                    c = 0xedbfc320 ^ (c >> 1);
    //                else
    //                    c = c >> 1;
    //            }
    //            _CRCTable[i] = c;
    //        }
    //    }

    //    /*计算buffer的crc校验码*/
    //    private static uint HashCRC32(uint crc, byte[] buffer, int size)
    //    {
    //        for (int i = 0; i < size; i++)
    //        {
    //            crc = _CRCTable[(crc ^ buffer[i]) & 0xff] ^ (crc >> 8);
    //        }
    //        return crc;
    //    }
    //}

    static class CommonHelper
    {
        static System.Security.Cryptography.SHA256Managed sha1 = new System.Security.Cryptography.SHA256Managed();
        static System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

        //[DllImport("PwdHelperCode.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Encode", CharSet = CharSet.Auto)]
        //private extern static int _Encode(byte[] data, int data_count, byte[] out_data, int out_data_count, byte[] pwd, int pwd_count);
        //[DllImport("PwdHelperCode.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Decode", CharSet = CharSet.Auto)]
        //private extern static int _Decode(byte[] data, int data_count, byte[] out_data, int out_data_count, byte[] pwd, int pwd_count);

        static System.Security.Cryptography.AesCryptoServiceProvider aes = new System.Security.Cryptography.AesCryptoServiceProvider();
        public static bool Encode(byte[] rawdata, out byte[] encrypteddata, string password)
        {
            encrypteddata = null;
            try
            {
                var buffer = GetData(password);
                aes.Key = sha1.ComputeHash(buffer, 0, buffer.Length);
                aes.IV = md5.ComputeHash(buffer, 0, buffer.Length);
                encrypteddata = aes.CreateEncryptor().TransformFinalBlock(rawdata, 0, rawdata.Length);
            }
            catch
            {
                return false;
            }
            return true;
            //return EncryptCode.Encrypt(rawdata, out encrypteddata, password);
            //encrypteddata = new byte[rawdata.Length + 12];
            //var pwd = Encoding.UTF8.GetBytes(password);
            //return 0 == _Encode(rawdata, rawdata.Length, encrypteddata, encrypteddata.Length, pwd, pwd.Length);
        }

        public static bool Decode(byte[] encrypteddata, out byte[] rawdata, string password)
        {
            rawdata = null;
            try
            {
                var buffer = GetData(password);
                aes.Key = sha1.ComputeHash(buffer, 0, buffer.Length);
                aes.IV = md5.ComputeHash(buffer, 0, buffer.Length);
                rawdata = aes.CreateDecryptor().TransformFinalBlock(encrypteddata, 0, encrypteddata.Length);
            }
            catch
            {
                return false;
            }
            return true;

            //return EncryptCode.Decrypt(encrypteddata, out rawdata, password);
            //rawdata = new byte[encrypteddata.Length - 12];
            //var pwd = Encoding.UTF8.GetBytes(password);
            //return 0 == _Decode(encrypteddata, encrypteddata.Length, rawdata, rawdata.Length, pwd, pwd.Length);
        }

        public static byte[] Compress(byte[] data)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                {
                    gZipStream.Write(data, 0, data.Length);
                }
                buffer = stream.ToArray();
            }
            return buffer;
        }
        public static byte[] Decompress(byte[] data)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                {
                    buffer = new byte[4096];
                    int n;
                    while ((n = gZipStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        stream.Write(buffer, 0, n);
                    }
                }
                buffer = stream.ToArray();
            }
            return buffer;
        }

        public static string GetString(byte[] data)
        {
            if (data == null)
                return null;
            return Encoding.UTF8.GetString(data);
        }

        public static byte[] GetData(string str)
        {
            if (str == null)
                return null;
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
