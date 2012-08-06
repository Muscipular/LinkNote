using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.IO.Compression;

namespace Muscipular.LinkNote
{
    public sealed class DataBase
    {
        #region 静态属性
        #region 更新SQL
        private const string ___CreateSql =
   @"
CREATE TABLE Content ( 
    Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    Content BLOB,
    [Index] INTEGER NOT NULL DEFAULT (0),
    Parent INTEGER NOT NULL,
    Type INT NOT NULL DEFAULT (0),
    UNIQUE ( [Index], Parent ) 
);
CREATE TABLE Node (
    Id INTEGER PRIMARY KEY NOT NULL,
    Name TEXT,
    Parent INTEGER,
    [Index] INT NOT NULL DEFAULT (0) 
);
CREATE TABLE Settings ( 
    [Key] TEXT PRIMARY KEY,
    Value TEXT NOT NULL 
);
";
        #endregion

        #region 属性字段
        private static DataBase _DataBase;
        private SQLiteConnection _Connection = null;
        private SQLiteTransaction _Transaction = null;
        private int _TransactionCount = 0;

        bool HasTransaction { get { return _Transaction != null; } }

        public void BeginTransaction()
        {
            if (!HasTransaction)
                _Transaction = _Connection.BeginTransaction();
            _TransactionCount++;
        }

        public void Commit()
        {
            if (_TransactionCount < 1 || !HasTransaction)
                throw new InvalidOperationException();
            if (_TransactionCount == 1)
            {
                _Transaction.Commit();
                DisposeTransaction();
                return;
            }
            _TransactionCount--;
        }

        private void DisposeTransaction()
        {
            _TransactionCount = 0;
            if (HasTransaction)
                _Transaction.Dispose();
            _Transaction = null;
        }

        public void Rollback()
        {
            if (_TransactionCount < 1 || !HasTransaction)
                return;
            _Transaction.Rollback();
            DisposeTransaction();
        }

        public static DataBase Current
        {
            get
            {
                if (_DataBase == null)
                    _DataBase = new DataBase();
                return _DataBase;
            }
        }
        #endregion
        #endregion

        #region 构造函数
        private DataBase()
        {
            string dbPath = Environment.CurrentDirectory + "\\LinkNote.db";
            bool IsDbExist = File.Exists(dbPath);
            _Connection = new SQLiteConnection("Data Source=" + dbPath + ";Pooling=true;FailIfMissing=false");
            _Connection.Open();
            if (!IsDbExist)
                Excute(___CreateSql);
        }
        #endregion

        #region 属性字段
        private int GetNodeNextIndex()
        {
            int i = 0;
            using (var cmd = _Connection.CreateCommand())
            {
                cmd.CommandText = "select max(id) from node";
                var reader = cmd.ExecuteScalar();
                if (reader != null && reader != DBNull.Value)
                    i = int.Parse(reader.ToString());
            }
            return i + 1;
        }
        #endregion

        #region Node操作

        #region CURD操作
        public bool InsertNode(Node node)
        {
            BeginTransaction();
            try
            {
                node.Id = GetNodeNextIndex();
                using (var cmd = _Connection.CreateCommand())
                {
                    string sql = "INSERT INTO Node (Id, Name, Parent,[Index]) VALUES (@Id,@Name,@Parent,@NIndex);";
                    cmd.Parameters.AddWithValue("@Id", node.Id);
                    cmd.Parameters.AddWithValue("@Name", node.Name);
                    cmd.Parameters.AddWithValue("@NIndex", node.Parent.Children.IndexOf(node));
                    cmd.Parameters.AddWithValue("@Parent", node.Parent == Node.BaseNode ? DBNull.Value : (object)node.ParentId);
                    int index = 0;
                    foreach (var buffer in node.Content)
                    {
                        sql += "INSERT INTO Content (Content, [Index],Parent) VALUES (@Content_" + index + ",@CIndex_" + index + ",@Id);";
                        cmd.Parameters.AddWithValue("@Content_" + index, buffer);
                        cmd.Parameters.AddWithValue("@CIndex_" + index, index);
                        index++;
                    }
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                Commit();
                return true;
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return false;
        }

        public bool UpdateNode(Node node)
        {
            if (node.Id < 0)
                return false;
            BeginTransaction();
            try
            {
                using (var cmd = _Connection.CreateCommand())
                {
                    string sql = "UPDATE Node SET Name=@Name,Parent=@Parent,[Index]=@NIndex where Id=@Id";
                    cmd.Parameters.AddWithValue("@Id", node.Id);
                    cmd.Parameters.AddWithValue("@Name", node.Name);
                    cmd.Parameters.AddWithValue("@NIndex", node.Parent.Children.IndexOf(node));
                    cmd.Parameters.AddWithValue("@Parent", node.ParentId == -1 ? DBNull.Value : (object)node.ParentId);
                    cmd.CommandText = sql;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        Rollback();
                        return false;
                    }
                }
                Commit();
                return true;
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return false;
        }

        public bool DeleteNode(INode node)
        {
            if (node.Id < 0)
                return false;
            BeginTransaction();
            try
            {
                using (var cmd = _Connection.CreateCommand())
                {
                    string sql = "delete from Node where Id=@Id";
                    cmd.Parameters.AddWithValue("@Id", node.Id);
                    cmd.CommandText = sql;
                    if (cmd.ExecuteNonQuery() != 1)
                    {
                        Rollback();
                        return false;
                    }
                }
                using (var cmd = _Connection.CreateCommand())
                {
                    string sql = "delete from Content where Parent=@Id;";
                    cmd.Parameters.AddWithValue("@Id", node.Id);
                    cmd.CommandText = sql;
                    if (cmd.ExecuteNonQuery() <= -1)
                    {
                        Rollback();
                        return false;
                    }
                }
                Commit();
                return true;
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return false;
        }

        public List<Node> ListNode(Node node = null)
        {
            if (node == null)
                node = Node.BaseNode;
            BeginTransaction();
            try
            {
                List<Node> list = new List<Node>();
                using (var cmd = _Connection.CreateCommand())
                {
                    string sql = "select * from Node where Parent" + (node == Node.BaseNode ? " is Null" : (" = " + node.Id)) + " order by [index] asc;";
                    cmd.CommandText = sql;
                    DataTable dt = new DataTable();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        Node n = node.CreateChildNode(int.Parse(dr["Id"].ToString()), dr["Name"].ToString());
                        list.Add(n);
                    }
                }
                Commit();
                return list;
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return null;
        }

        #endregion

        #region 其他数据库操作
        public bool UpdateList(List<INode> nodeList)
        {
            if (nodeList == null)
                throw new ArgumentNullException();
            BeginTransaction();
            try
            {
                for (int i = 0, max = nodeList.Count; i < max; i++)
                {
                    if (!UpdateNode(nodeList[i] as Node))
                    {
                        Rollback();
                        return false;
                    }
                }
                Commit();
                return true;
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return false;
        }
        #endregion

        #endregion

        #region Content操作
        public bool InsertContent(IDataContent content)
        {
            byte[] data = content.Data;
            if (content.IsCompressable)
            {
                ICompressable dataItem = content as ICompressable;
                data = dataItem.RawData;
            }
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@index", content.Index);
            p.Add("@parent", content.Parent.Id);
            p.Add("@content", data);
            p.Add("@Type", (int)content.GetContentType());
            string sql = "update Content set [index]=[index]+1 where [index]>=@index and parent=@parent;";
            sql += "Insert into Content (Content, [Index],Parent) VALUES (@content,@index,@parent);";
            return DataBase.Current.Excute(sql, p) > -1;
        }

        public bool SaveContent(IDataContent content)
        {
            var data = content.Data;
            if (content.IsCompressable)
            {
                data = (content as ICompressable).RawData;
            }
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@index", content.Index);
            p.Add("@parent", content.Parent.Id);
            p.Add("@content", data);
            p.Add("@type", (int)content.GetContentType());
            string sql = "update Content set Content=@content, Type=@type where [index]=@index and parent=@parent;";
            return DataBase.Current.Excute(sql, p) > -1;
        }

        public bool DeleteContent(IDataContent content)
        {
            return DeleteContent(content.Parent, content.Index);
        }

        public bool DeleteContent(INode parent, int index)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@index", index);
            p.Add("@parent", parent.Id);
            string sql = "delete from Content where [index]=@index and parent=@parent;";
            sql += "update Content set [index]=[index]-1 where [index]>=@index and parent=@parent;";
            return DataBase.Current.Excute(sql, p) > -1;
        }

        public bool DeleteAllContent(INode parent)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@parent", parent.Id);
            string sql = "delete from Content where parent=@parent;";
            return DataBase.Current.Excute(sql, p) > -1;
        }

        public IDataContent GetContent(INode node, int index)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("@index", index);
            p.Add("@parent", node.Id);
            string sql = "select Content,Type from Content where [index]=@index and parent=@parent;";
            var result = DataBase.Current.ExcuteRead(sql, p);
            if (result == null)
                return null;
            if (result.Rows.Count == 0)
                return null;
            var data = result.Rows[0]["Content"] as byte[];
            var type = (ContentType)(int)result.Rows[0]["Type"];
            if (type == ContentType.Unknow)
            {
                type = ContentType.Encryptable | ContentType.Compressable | ContentType.Compressed;
                if (!(data.Length > 3 && data[0] == 0x1f && data[1] == 0x8b && data[2] == 0x08))
                    type |= ContentType.Encrypted;
            }
            if (type.HasFlag(ContentType.Encrypted))
                return new EncryptContent(node, data, index);
            if (type.HasFlag(ContentType.Encryptable))
                return new EncryptContent(node, data, index, false);
            if (type.HasFlag(ContentType.Compressed))
                return new Content(node, data, index);
            if (type.HasFlag(ContentType.Compressable))
                return new Content(node, data, index, false);
            return null;
        }
        #endregion

        #region 执行SQL
        public int Excute(string sql, Dictionary<string, object> parameters = null)
        {
            BeginTransaction();
            try
            {
                int result;
                using (var cmd = _Connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                        {
                            cmd.Parameters.AddWithValue(p.Key, p.Value);
                        }
                    }
                    result = cmd.ExecuteNonQuery();
                    if (result < 0)
                    {
                        Rollback();
                        return -1;
                    }
                }
                Commit();
                return result;
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return -1;
        }
        public DataTable ExcuteRead(string sql, Dictionary<string, object> parameters = null)
        {
            BeginTransaction();
            try
            {
                using (var cmd = _Connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters != null)
                    {
                        foreach (var p in parameters)
                        {
                            cmd.Parameters.AddWithValue(p.Key, p.Value);
                        }
                    }
                    DataTable dt = new DataTable();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    Commit();
                    return dt;
                }
            }
            catch (Exception e)
            {
                Log.LogError(e);
                Rollback();
            }
            return null;
        }
        #endregion

    }

}
