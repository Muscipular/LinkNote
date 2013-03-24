using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LinkNote2.Data.Model;

namespace LinkNote2.Service
{
    sealed class IndexService
    {
        public string DirPath { get; private set; }
        private static readonly Lazy<IndexService> _Instance = new Lazy<IndexService>(() => new IndexService(""));
        private DataPath _Root;

        public static IndexService Instance
        {
            get
            {
                return _Instance.Value;
            }
        }

        private IndexService(string dirPath)
        {
            DirPath = dirPath;
        }

        public DataPath GetRoot()
        {
            if (_Root == null)
            {
                var hash = CommonService.Instance.HashMD5(DirPath);
                _Root = new DataPath() { Id = hash, Parent = hash, Path = DirPath };
            }

            return _Root;
        }

        public DataPath[] GetChildren(DataPath dataPath)
        {
            var dirs = Directory.GetDirectories(dataPath.Path);
            var dataPaths = new DataPath[dirs.Length];
            for (int i = 0, mx = dirs.Length; i < mx; i++)
            {
                dataPaths[i] = new DataPath()
                {
                    Id = CommonService.Instance.HashMD5(dirs[i]),
                    Parent = dataPath.Id,
                    Path = dirs[i],
                };
            }
            return dataPaths;
        }

        public Content[] GetContents(DataPath dataPath)
        {
            var path = dataPath + "\\index.data.json";
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<DataPath>(json).Contents;
                }
                catch (Exception)
                {
                }
            }
            return new Content[0];
        }
    }
}
