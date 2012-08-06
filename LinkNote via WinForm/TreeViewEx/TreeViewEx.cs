using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Muscipular.LinkNote
{
    public class TreeViewEx<T> : TreeView where T : class, INode
    {
        protected System.Linq.Expressions.Expression<Func<T, string>> _NameExpression;
        protected INodeCollection<T> _DataSource;
        public event EventHandler DataSourceChanged;
        public TreeNodeEx<T> PreSelectedNode { get; set; }
        public TreeNodeEx<T> ClickedNode { get; set; }
        public new TreeNodeEx<T> SelectedNode
        {
            get
            {
                return base.SelectedNode as TreeNodeEx<T>;
            }
            set
            {
                base.SelectedNode = value;
            }
        }

        public TreeViewEx(System.Linq.Expressions.Expression<Func<T, string>> nameExpression)
            : this()
        {
            _NameExpression = nameExpression;
        }

        public TreeViewEx()
        {
            //this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            //var c = new TextBox() { Dock = DockStyle.Top };
            //this.Controls.Add(c);
            _NameExpression = x => string.Empty;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //SetBounds(this.Left, this.Top, this.Width, this.Height - 30);
            //SetClientSizeCore(this.ClientSize.Width, this.ClientSize.Height - 30);
            base.OnPaint(e);
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

        }

        void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            TreeNodeEx<T> n = e.Node as TreeNodeEx<T>;
            T node = n.Node;
            if (node.Children.Contains(Node.VisualNode))
            {
                node.Children.Clear();
                DataBase.Current.ListNode(node as Node).ForEach(x => node.Children.Add(x));
            }
            n.DataSource = (INodeCollection<T>)node.Children;
            //n.Nodes.Clear();
            //foreach (var child in node.Children)
            //{
            //    TreeNodeEx<T> nx = new TreeNodeEx<T>(child as T, _NameExpression);
            //    nx.DataSource = node.Children;
            //    n.Nodes.Add(nx);
            //}
            base.OnBeforeExpand(e);
        }

        protected virtual void OnDataSourceChanged()
        {
            base.Nodes.Clear();
            foreach (T note in _DataSource)
            {
                TreeNodeEx<T> n = new TreeNodeEx<T>(note, _NameExpression);
                n.DataSource = note.Children as INodeCollection<T>;
                base.Nodes.Add(n);
            }
            EventHandler handler = this.DataSourceChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            TreeNodeEx<T> n = null;
            var p = MousePosition;
            if ((n = (TreeNodeEx<T>)this.GetNodeAt(this.PointToClient(p))) != null)
            {
                this.SelectedNode = PreSelectedNode = n;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                PreSelectedNode = this.SelectedNode as TreeNodeEx<T>;
                this.SelectedNode = null;
            }
            base.OnMouseDown(e);
        }

    }
}
