using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Muscipular.LinkNote
{
    public class ContentCollection : IList<IDataContent>
    {
        public INode Parent { get; protected set; }
        public ContentCollection(INode parent)
        {
            Parent = parent;
        }

        #region IList<Content> 成员

        public int IndexOf(IDataContent item)
        {
            return item.Index;
        }

        public void Insert(int index, IDataContent item)
        {
            if (item.Parent.Id != this.Parent.Id)
                throw new ArgumentException();
            item.Index = index;
            DataBase.Current.InsertContent(item);
            //item.Parent = Parent;
            //byte[] data = item.Data;
            //if (item.IsCompressable)
            //{
            //    ICompressable dataItem = item as ICompressable;
            //    data = dataItem.RawData;
            //}
            //Dictionary<string, object> p = new Dictionary<string, object>();
            //p.Add("@index", index);
            //p.Add("@parent", Parent.Id);
            //p.Add("@content", data);
            //string sql = "update Content set [index]=[index]+1 where [index]>=@index and parent=@parent;";
            //sql += "Insert into Content (Content, [Index],Parent) VALUES (@content,@index,@parent);";
            //DataBase.Current.Excute(sql, p);
        }

        public void RemoveAt(int index)
        {
            DataBase.Current.DeleteContent(Parent, index);
            //Dictionary<string, object> p = new Dictionary<string, object>();
            //p.Add("@index", index);
            //p.Add("@parent", Parent.Id);
            //string sql = "delete from Content where [index]=@index and parent=@parent;";
            //sql += "update Content set [index]=[index]-1 where [index]>=@index and parent=@parent;";
            //DataBase.Current.Excute(sql, p);
        }

        public IDataContent this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();
                return DataBase.Current.GetContent(this.Parent, index);
                //Dictionary<string, object> p = new Dictionary<string, object>();
                //p.Add("@index", index);
                //p.Add("@parent", Parent.Id);
                //string sql = "select Content from Content where [index]=@index and parent=@parent;";
                //var r = DataBase.Current.ExcuteRead(sql, p);
                //if (r == null)
                //    return null;
                //if (r.Rows.Count == 0)
                //    return null;
                //var rr = r.Rows[0]["Content"] as byte[];
                //IDataContent c = new Content(this.Parent, rr, index);
                //return c;
            }
            set
            {
                if (index >= Count)
                    throw new IndexOutOfRangeException();
                if (value == null)
                    throw new InvalidOperationException();
                value.Index = index;
                value.Parent = Parent;
                DataBase.Current.SaveContent(value);
                //Dictionary<string, object> p = new Dictionary<string, object>();
                //p.Add("@index", index);
                //p.Add("@parent", Parent.Id);
                //p.Add("@content", value.Data);
                //string sql = "update Content set Content=@content where [index]=@index and parent=@parent;";
                //DataBase.Current.Excute(sql, p);
            }
        }

        #endregion

        #region ICollection<Content> 成员

        public void Add(byte[] data)
        {
            IDataContent c = new Content(this.Parent, data, 0);
            Add(c);
        }
        public void Add(IDataContent item)
        {
            Insert(this.Count, item);
        }

        public void Clear()
        {
            DataBase.Current.DeleteAllContent(Parent);
            //Dictionary<string, object> p = new Dictionary<string, object>();
            //p.Add("@parent", Parent.Id);
            //string sql = "delete from Content where parent=@parent;";
            //DataBase.Current.Excute(sql, p);
        }

        public bool Contains(IDataContent item)
        {
            return item.Parent.Id == Parent.Id && item.Index < Count;
        }

        public void CopyTo(IDataContent[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get
            {
                Dictionary<string, object> p = new Dictionary<string, object>();
                p.Add("@parent", Parent.Id);
                string sql = "select count(0) from Content where parent=@parent;";
                var r = DataBase.Current.ExcuteRead(sql, p);
                return int.Parse(r.Rows[0][0].ToString());
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IDataContent item)
        {
            if (item.Parent.Id != Parent.Id || item.Index >= Count)
                return false;
            RemoveAt(item.Index);
            return true;
        }

        #endregion

        #region IEnumerable<Content> 成员

        public IEnumerator<IDataContent> GetEnumerator()
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
            //return new Enumerator(this);
        }

        #endregion

        #region IEnumerable 成员

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
