using LogViewer.ViewModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LogViewer.Model
{
    public class ObservableSet<T> : PropertyChangesNotifier
    {
        private readonly HashSet<T> ItemSet = new HashSet<T>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            ItemSet.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ItemSet.Add(item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public void Remove(T item)
        {
            ItemSet.Remove(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                ItemSet.Remove(item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items));
        }

        public void Clear()
        {
            ItemSet.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        public HashSet<T> GetItemSet()
        {
            return ItemSet;
        }
    }
}
