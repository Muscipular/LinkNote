using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Awesomium.Core.Data;

namespace LinkNote2.Data
{
    public class DirectoryDataSource : DataSourceBase
    {
        public string DirectoryPath { get; private set; }

        public DirectoryDataSource(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(directoryPath);
            }
            DirectoryPath = directoryPath;
        }

        protected override string ParsePath(string path)
        {
            path = DirectoryPath + "\\" + path.ToLower().Replace('/', '\\').Trim();
            path = path.Remove(path.IndexOf('?'));
            path = path.Remove(path.IndexOf('#'));
            return path;
        }

        protected override byte[] GetFileData(string path)
        {
            return File.ReadAllBytes(path);
        }

        protected override bool IsExistPath(string path)
        {
            return File.Exists(path);
        }
    }
}
