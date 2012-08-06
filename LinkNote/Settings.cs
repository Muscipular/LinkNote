using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Muscipular.LinkNote
{
    sealed class Settings
    {
        private static Dictionary<string, string> _dic = new Dictionary<string, string>();
        private static Dictionary<Version, string> _sql = new Dictionary<Version, string>();
        private static Version _version;
        private static Version _dbVersion;
        static Settings()
        {
            _sql.Add(new Version(1, 3, 0, 0), "CREATE TABLE Settings ([Key] TEXT PRIMARY KEY,Value TEXT NOT NULL);ALTER TABLE Node ADD [Index] INT;");
            _sql.Add(new Version(1, 4, 0, 0), "ALTER TABLE Content ADD Type INT NOT NULL DEFAULT (0);");
            Current = new Settings();
            Current.Load();
            _dbVersion = new Version(Current["Version"] ?? "1.0.0.0");
            _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            bool HaveNoChanged = true;
            foreach (Version v in _sql.Keys.OrderBy(x => x))
            {
                if (v > _dbVersion)
                {
                    DataBase.Current.Excute(_sql[v]);
                    HaveNoChanged = false;
                }
            }
            if (HaveNoChanged)
                return;
            _dbVersion = _sql.Keys.Max();
            Current["Version"] = _dbVersion.ToString();
            Current.Save();
        }
        public override string ToString()
        {
            return string.Format("Program: {0}; DataBase: {1}", Version, DBVersion);
        }
        private Settings() { }
        public static Settings Current { get; private set; }
        public Version Version { get { return _version; } }
        public Version DBVersion { get { return _dbVersion; } }
        public string this[string key]
        {
            get
            {
                if (_dic.ContainsKey(key))
                    return _dic[key];
                return null;
            }
            set
            {
                if (_dic.ContainsKey(key))
                    _dic[key] = value;
                else
                    _dic.Add(key, value);
            }
        }
        public void Save()
        {
            int i = 0;
            StringBuilder str = new StringBuilder("delete from settings;");
            Dictionary<string, object> d = new Dictionary<string, object>();
            foreach (var kv in _dic)
            {
                d.Add("key" + i, kv.Key);
                d.Add("value" + i, kv.Value);
                str.AppendFormat("insert into settings ([key],value) values(@key{0},@value{0});", i);
                i++;
            }
            DataBase.Current.Excute(str.ToString(), d);
        }
        public void Load()
        {
            _dic.Clear();
            var dt = DataBase.Current.ExcuteRead("select * from settings");
            if (dt == null)
                return;
            foreach (DataRow dr in dt.Rows)
            {
                _dic.Add(dr["Key"].ToString(), dr["Value"].ToString());
            }
        }
    }
}
