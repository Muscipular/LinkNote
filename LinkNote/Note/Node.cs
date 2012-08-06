using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections;
namespace Muscipular.LinkNote
{
    public interface INode : System.ComponentModel.INotifyPropertyChanged
    {
        int Id { get; set; }
        INodeCollection<INode> Children { get; set; }
        Node Parent { get; set; }
        string Name { get; set; }
        bool Contains(INode node, bool isSearchAll = false, bool isFromChildToParent = false);
    }
    public class Node : INode
    {
        private static Node _BaseNode;
        private static Node _VisualNode;
        public static Node BaseNode { get { return _BaseNode; } }
        public static Node VisualNode { get { return _VisualNode; } }
        static Node()
        {
            _BaseNode = new Node("BaseNode");
            _VisualNode = new Node("VisualNode");
            _BaseNode.Children = new NodeCollection<INode>(DataBase.Current.ListNode(_BaseNode));
        }
        private int _Index;
        public int Index { get { return _Index; } set { _Index = value; } }
        private string _Name;
        public string Name { get { return _Name; } set { _Name = value; OnPropertyChanged("Name"); } }
        private ContentCollection _Content;
        public ContentCollection Content
        {
            get
            {
                if (_Content == null)
                {
                    _Content = new ContentCollection(this);
                }
                return _Content;
            }
            protected set
            {
                _Content = value;
            }
        }
        private INodeCollection<INode> _Children;
        public INodeCollection<INode> Children
        {
            get
            {
                if (_Children == null)
                    _Children = new NodeCollection<INode>();
                return _Children;
            }
            set
            {
                _Children = value;
                foreach (var child in value)
                {
                    child.Parent = this;
                }
                OnChildrenChanged();
            }
        }
        public bool IsRoot { get { return Parent == BaseNode; } }
        private int _Id = -1;
        public int Id
        {
            get
            {
                return _Id;
            }
            set
            {
                if (_Id > -1 && value != -1)
                    return;
                _Id = value;
                OnPropertyChanged("Id");
            }
        }
        public Node TopNode
        {
            get
            {
                if (IsRoot)
                    return BaseNode;
                Node n = this;
                while (n != null && n.Parent != null && n.Parent != BaseNode)
                {
                    n = n.Parent;
                }
                return n;
            }
        }
        private Node _Parent = BaseNode;
        public Node Parent
        {
            get
            {
                return _Parent;
            }
            set
            {
                if (value == VisualNode)
                    return;
                _Parent = value;
                OnPropertyChanged("Parent");
            }
        }
        public int ParentId
        {
            get
            {
                if (this == BaseNode)
                    return -1;
                return Parent.Id;
            }
        }
        private Node(string name) { Name = name; }
        private Node()
        {
            Name = string.Empty;
            Children.Add(VisualNode);
        }
        public Node CreateChildNode(int id, string name, List<byte[]> content = null)
        {
            Node n = new Node();
            n.Id = id;
            n.Name = name;
            n.Parent = this;
            if (content != null && content.Count > 0)
            {
                foreach (var c in content)
                {
                    n.Content.Add(c);
                }
            }
            return n;
        }
        public Node AddChildNode(Node node)
        {
            if (this == VisualNode)
                throw new InvalidOperationException();
            DataBase.Current.BeginTransaction();
            node.Parent.Children.Remove(node);
            if (!DataBase.Current.UpdateList((node.Parent.Children as NodeCollection<INode>).ToList()))
            {
                DataBase.Current.Rollback();
                return null;
            }
            node.Parent = this;
            if (this.Children.Contains(VisualNode))
            {
                this.Children.Clear();
                DataBase.Current.ListNode(this).ForEach(x => this.Children.Add(x));
            }
            this.Children.Add(node);
            DataBase.Current.UpdateList((this.Children as NodeCollection<INode>).ToList());
            //DataBase.Current.Update(node);
            DataBase.Current.Commit();
            return node;
        }
        public Node AddChildNode(string name = null, List<byte[]> content = null)
        {
            if (this == VisualNode)
                throw new InvalidOperationException();
            Node n = new Node();
            n.Parent = this;
            if (!string.IsNullOrWhiteSpace(name))
                n.Name = name;
            if (content != null && content.Count > 0)
            {
                foreach (var c in content)
                {
                    n.Content.Add(c);
                }
            }
            if (this.Children.Contains(VisualNode))
            {
                this.Children.Clear();
                DataBase.Current.ListNode(this).ForEach(x => this.Children.Add(x));
            }
            this.Children.Add(n);
            DataBase.Current.InsertNode(n);
            return n;
        }
        public void CleanChildrenNode()
        {
            foreach (Node n in Children)
            {
                DataBase.Current.DeleteNode(this);
            }
            Children.Clear();
        }
        public bool Delete()
        {
            return Parent.Remove(this);
        }
        public bool Remove(Node node)
        {
            DataBase.Current.BeginTransaction();
            Children.Remove(node);
            if (node != VisualNode && !DataBase.Current.DeleteNode(node))
            {
                DataBase.Current.Rollback();
                return false;
            }
            foreach (Node n in node.Children.ToArray())
            {
                if (!node.Remove(n))
                {
                    DataBase.Current.Rollback();
                    return false;
                }
            }
            foreach (Node n in Children)
            {
                if (!DataBase.Current.UpdateNode(n))
                {
                    DataBase.Current.Rollback();
                    return false;
                }
            }
            DataBase.Current.Commit();
            return true;
        }
        public override string ToString()
        {
            return Name;
        }
        public bool Contains(INode node, bool isSearchAll = false, bool isFromChildToParent = false)
        {
            if (node == null)
                return false;
            if (Children.Contains(node))
                return true;
            if (!isSearchAll)
                return false;
            if (isFromChildToParent)
            {
                while (node.Parent != null && node.Parent != Node.BaseNode)
                {
                    node = node.Parent;
                    if (node == this)
                        return true;
                }
            }
            else
            {
                foreach (var n in this.Children)
                {
                    if (n.Contains(node, isSearchAll))
                        return true;
                }
            }
            return false;
        }
        public bool SaveChange()
        {
            return DataBase.Current.UpdateNode(this);
        }
        #region INotifyPropertyChanged 成员

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnChildrenChanged()
        {
            OnPropertyChanged("Children");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var h = PropertyChanged;
            if (h != null)
            {
                h(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
