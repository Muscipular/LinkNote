using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Awesomium.Core.Data;

namespace LinkNote2.Data
{
    public abstract class DataSourceBase : DataSource
    {
        protected sealed override void OnRequest(DataSourceRequest request)
        {
            var path = ParsePath(request.Path);
            var data = GetData(path, true);
            if (data == null || data.Length == 0)
            {
                SendNullResponse(request);
                return;
            }
            var mime = request.MimeType;
            if (mime == "unknown/unknown")
            {
                var ext = Path.GetExtension(path);
                mime = MIMEHelper.GetMIME(ext);
            }
            unsafe
            {
                fixed (byte* pdata = data)
                {
                    IntPtr ptr = new IntPtr(pdata);
                    DataSourceResponse response = new DataSourceResponse
                    {
                        Buffer = ptr,
                        Size = (uint)data.Length,
                        MimeType = mime,
                    };
                    SendResponse(request, response);
                }
            }
        }

        protected virtual string ParsePath(string s)
        {
            var path = s.ToLower().Trim();
            var p = path.IndexOf('?');
            if (p >= 0)
            {
                path = path.Remove(p);
            }
            p = path.IndexOf('#');
            if (p >= 0)
            {
                path = path.Remove(p);
            }
            return path;
        }

        public byte[] GetData(string path)
        {
            path = ParsePath(path);
            if (!IsExistPath(path))
            {
                return new byte[0];
            }
            return GetFileData(path);
        }

        protected byte[] GetData(string path, bool isParse)
        {
            if (!isParse)
            {
                path = ParsePath(path);
            }
            if (!IsExistPath(path))
            {
                return new byte[0];
            }
            return GetFileData(path);
        }

        private void SendNullResponse(DataSourceRequest request)
        {
            SendResponse(request, new DataSourceResponse { Buffer = IntPtr.Zero, MimeType = "text/plain", Size = 0 });
        }

        protected abstract byte[] GetFileData(string path);
        protected abstract bool IsExistPath(string path);
    }
}
