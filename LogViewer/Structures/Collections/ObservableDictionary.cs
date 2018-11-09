using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using LogViewer.ViewModel.Helpers;

namespace LogViewer.Structures.Collections
{
    public class ObservableCounterDictionary<T> : PropertyChangesNotifier, IEnumerable
    {
        private readonly Dictionary<T, int> ItemMap = new Dictionary<T, int>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int this[T index]
        {
            get { return ItemMap[index]; }
            set { ItemMap[index] = value; }
        }

        public void IncrementCounter(T key, bool fireChangedEvent = true)
        {
            object oldValue = ItemMap[key];
            ItemMap[key]++;

            if (fireChangedEvent)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ItemMap[key], oldValue));
            }
        }

        public void ResetCounter(T key, bool fireChangedEvent = true)
        {
            ItemMap[key] = 0;

            if (fireChangedEvent)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, ItemMap[key]));
            }
        }

        public void ResetAllCounters(bool fireChangedEvent = true)
        {
            foreach (var key in ItemMap.Keys.ToList())
            {
                ItemMap[key] = 0;

                if (fireChangedEvent)
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, ItemMap[key]));
                }
            }
        }

        public void Add(T key, int item)
        {
            ItemMap.Add(key, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void AddRange(T key, IEnumerable<int> items)
        {
            foreach (var item in items)
            {
                ItemMap.Add(key, item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public void Remove(T key)
        {
            ItemMap.Remove(key);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, key));
        }

        public void RemoveRange(IEnumerable<T> keys)
        {
            foreach (var key in keys)
            {
                ItemMap.Remove(key);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, keys));
        }

        public void Clear()
        {
            ItemMap.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        public Dictionary<T, int> GetItemSet()
        {
            return ItemMap;
        }

        public IEnumerator GetEnumerator()
        {
            return ItemMap.GetEnumerator();
        }
    }
}
