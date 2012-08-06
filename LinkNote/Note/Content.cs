using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Muscipular.LinkNote
{
    [Flags]
    public enum ContentType
    {
        Unknow = 0,
        Compressable = 1,
        Encryptable = 2,
        Compressed = 4,
        Encrypted = 8,
        Defualt = 16
    }

    public interface IDataContent
    {
        byte[] Data { get; set; }
        int Index { get; set; }
        INode Parent { get; set; }
        void Save();
        bool IsCompressable { get; }
        bool IsEncryptable { get; }
    }

    public interface ICompressable
    {
        byte[] RawData { get; set; }
        bool IsCompressed { get; }
    }

    public static class ContentHelper
    {
        public static ContentType GetContentType(this IDataContent content)
        {
            ContentType type = ContentType.Defualt;
            type |= content.IsCompressable ? ((content as ICompressable).IsCompressed ? ContentType.Compressed : 0) | ContentType.Compressable : 0;
            type |= content.IsEncryptable ? ((content as IEncryptable).IsEnctypted ? ContentType.Encrypted : 0) | ContentType.Encryptable : 0;
            return type;
        }
    }

    //public interface IBinaryContent : IDataContent, ICompressable
    //{
    //}

    public interface IEncryptable
    {
        bool Encrypt(string password);
        bool Decrypt(string password);
        bool IsEnctypted { get; }
    }

    public abstract class ContentBase : IDataContent
    {
        private bool _IsCompressable;
        private bool _IsEncryptable;
        protected ContentBase()
        {
            _IsCompressable = this is ICompressable;
            _IsEncryptable = this is IEncryptable;
        }
        public bool IsCompressable
        {
            get { return _IsCompressable; }
        }
        public bool IsEncryptable
        {
            get { return _IsEncryptable; }
        }
        protected byte[] _RawData;
        public virtual int Index { get; set; }
        public virtual INode Parent { get; set; }
        public abstract byte[] Data { get; set; }
        public virtual void Save()
        {
            DataBase.Current.SaveContent(this);
        }
    }

    public class Content : ContentBase, IDataContent, ICompressable
    {
        public Content(INode parent, byte[] rawData, int index)
        {
            _RawData = rawData;
            Index = index;
            Parent = parent;
        }
        public Content(INode parent, byte[] data, int index, bool isCompressed)
        {
            if (isCompressed)
            {
                _RawData = data;
            }
            else
            {
                _RawData = CommonHelper.Compress(data);
            }
        }

        public override byte[] Data
        {
            get
            {
                return CommonHelper.Decompress(_RawData);
            }
            set
            {
                if (value == null)
                    throw new InvalidOperationException();
                RawData = CommonHelper.Compress(value);
            }
        }
        public virtual byte[] RawData
        {
            get
            {
                return _RawData;
            }
            set
            {
                if (value == null)
                    throw new InvalidOperationException();
                _RawData = value;
                //Save();
            }
        }

        bool ICompressable.IsCompressed
        {
            get { return (_RawData.Length > 3 && _RawData[0] == 0x1f && _RawData[1] == 0x8b && _RawData[2] == 0x08); }
        }
    }

    public class EncryptContent : ContentBase, IDataContent, IEncryptable, ICompressable
    {
        public EncryptContent(INode parent, byte[] rawData, int index)
        {
            Parent = parent;
            _RawData = rawData;
            Index = index;
            _IsEnctypted = true;
        }

        public EncryptContent(INode parent, byte[] compresseData, int index, bool isEncypted)
        {
            Parent = parent;
            _RawData = compresseData;
            Index = index;
            _IsEnctypted = isEncypted;
        }

        public EncryptContent(INode parent, byte[] data, int index, bool isCompressed, bool isEncypted)
        {
            Parent = parent;
            if (isCompressed)
            {
                _RawData = data;
            }
            else
            {
                _RawData = CommonHelper.Compress(data);
            }
            Index = index;
            _IsEnctypted = isEncypted;
        }

        public override byte[] Data
        {
            get
            {
                if (_IsEnctypted)
                    return null;
                return CommonHelper.Decompress(_RawData);
            }
            set
            {
                if (value == null)
                    throw new InvalidOperationException();
                RawData = CommonHelper.Compress(value);
                _IsEnctypted = false;
            }
        }

        public virtual byte[] RawData
        {
            get
            {
                return _RawData;
            }
            set
            {
                if (value == null)
                    throw new InvalidOperationException();
                _RawData = value;
            }
        }

        public bool Encrypt(string password)
        {
            byte[] buffer;
            if (!CommonHelper.Encode(_RawData, out buffer, password))
                return false;
            _RawData = buffer;
            _IsEnctypted = true;
            return true;
        }

        public bool Decrypt(string password)
        {
            byte[] buffer;
            if (!CommonHelper.Decode(_RawData, out buffer, password))
                return false;
            _RawData = buffer;
            _IsEnctypted = false;
            return true;
        }

        protected bool _IsEnctypted;
        public bool IsEnctypted
        {
            get { return _IsEnctypted; }
        }

        bool ICompressable.IsCompressed
        {
            get { return IsEnctypted || (_RawData.Length > 3 && _RawData[0] == 0x1f && _RawData[1] == 0x8b && _RawData[2] == 0x08); }
        }
    }
}
