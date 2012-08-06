using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Muscipular.LinkNote
{
    public class TreeNodeEx<T> : TreeNode where T : class,INode
    {
        protected System.Linq.Expressions.Expression<Func<T, string>> _NameExpression;
        protected INodeCollection<T> _DataSource;
        public event EventHandler DataSourceChanged;
        protected T _Node;
        public T Node { get { return _Node; } }

        public TreeNodeEx(T node, System.Linq.Expressions.Expression<Func<T, string>> nameExpression)
            : base(nameExpression.Compile()(node))
        {
            _NameExpression = nameExpression;
            this._Node = node;
            _Node.PropertyChanged += PropertyChanged;
            //this.Nodes.Add(new TreeNodeEx<T>());
        }

        public TreeNodeEx()
            : base()
        {
            _NameExpression = x => string.Empty;
        }

        public new TreeNodeCollection Nodes
        {
            get { return base.Nodes; }
            set
            {
                if (_DataSource != null)
                    throw new NotSupportedException("DataSource is not null.");
                Nodes = value;
            }
        }

        public INodeCollection<T> DataSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                if (_DataSource == value)
                    return;
                RemoveCollectionEventHandler(_DataSource);
                _DataSource = value;
                AddCollectionEventHandler(_DataSource);
                OnDataSourceChanged();
            }
        }

        void AddCollectionEventHandler(INodeCollection<T> list)
        {
            if (list != null)
            {
                list.CollectionChanged += CollectionChanged;
                list.PropertyChanged += PropertyChanged;
            }
        }

        void RemoveCollectionEventHandler(INodeCollection<T> list)
        {
            if (list != null)
            {
                list.CollectionChanged -= CollectionChanged;
                list.PropertyChanged -= PropertyChanged;
            }
        }

        void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                this.Text = _NameExpression.Compile()(this.Node);
            }
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (T node in e.NewItems)
                    {
                        base.Nodes.Add(new TreeNodeEx<T>(node, _NameExpression) { DataSource = (INodeCollection<T>)node.Children });
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                //break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (T node in e.OldItems)
                    {
                        var n = node;
                        var f = (from TreeNodeEx<T> b in base.Nodes where b.Node == n select b).FirstOrDefault();
                        base.Nodes.Remove(f);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();
                //break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    OnDataSourceChanged();
                    break;
            }
        }

        protected virtual void OnDataSourceChanged()
        {
            base.Nodes.Clear();
            foreach (T note in _DataSource)
            {
                TreeNodeEx<T> n = new TreeNodeEx<T>(note, _NameExpression);
                n.DataSource = (INodeCollection<T>)note.Children;
                base.Nodes.Add(n);
            }
            EventHandler handler = this.DataSourceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

    }
}
