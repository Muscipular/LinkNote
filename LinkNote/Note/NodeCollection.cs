using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Muscipular.LinkNote
{
    public interface INodeCollection<out T> : IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged
          where T : class,INode
    {
        int IndexOf(INode node);
        void Add(INode node);
        void Remove(INode node);
        void Clear();
    }
    public class NodeCollection<T> : ObservableCollection<T>, INodeCollection<T>
        where T : class,INode
    {
        public NodeCollection()
            : base()
        {
        }
        public NodeCollection(IEnumerable<T> nodeCollection)
            : base(nodeCollection)
        {
        }
        public NodeCollection(List<T> nodeList)
            : base(nodeList)
        {
        }

        int INodeCollection<T>.IndexOf(INode node)
        {
            return this.IndexOf(node as T);
        }


        void INodeCollection<T>.Add(INode node)
        {
            ((ObservableCollection<T>)this).Add(node as T);
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((ObservableCollection<T>)this).GetEnumerator();
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { this.CollectionChanged += value; }
            remove { this.CollectionChanged -= value; }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { this.PropertyChanged += value; }
            remove { this.PropertyChanged -= value; }
        }


        void INodeCollection<T>.Remove(INode node)
        {
            ((ObservableCollection<T>)this).Remove(node as T);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
