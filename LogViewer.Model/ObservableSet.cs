using LogViewer.Utilities;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

namespace LogViewer.Model
{
    public class ObservableSet<T> : PropertyChangesNotifier
    {
        private readonly List<T> _itemList = new List<T>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        public T this[int index]
        {
            get => _itemList[index];
            set => _itemList[index] = value;
        }
        
        public bool Any()
        {
            return _itemList.Any();
        }
        
        public int Count()
        {
            return _itemList.Count;
        }
        
        public void Add(T item)
        {
            if (_itemList.Contains(item)) {
                throw new DuplicateNameException("Item already on the collection");
            }

            _itemList.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (_itemList.Any(items.Contains)) {
                throw new DuplicateNameException("One or more items already on the collection");
            }
            
            _itemList.AddRange(items);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public void Remove(T item)
        {
            _itemList.Remove(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _itemList.Remove(item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items));
        }

        public void Clear()
        {
            _itemList.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        public List<T> GetItemList()
        {
            return _itemList;
        }
    }
}
