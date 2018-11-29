using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace LogViewer.Model
{
    public class ObservableCounterDictionary<T> : PropertyChangesNotifier, IEnumerable
    {
        private readonly Dictionary<T, int> _itemMap = new Dictionary<T, int>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public int this[T index]
        {
            get => _itemMap[index];
            set => _itemMap[index] = value;
        }

        public void IncrementCounter(T key, bool fireChangedEvent = true)
        {
            object oldValue = _itemMap[key];
            _itemMap[key]++;

            if (fireChangedEvent)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, _itemMap[key], oldValue));
            }
        }

        public void ResetCounter(T key, bool fireChangedEvent = true)
        {
            _itemMap[key] = 0;

            if (fireChangedEvent)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, _itemMap[key]));
            }
        }

        public void ResetAllCounters(bool fireChangedEvent = true)
        {
            foreach (var key in _itemMap.Keys.ToList())
            {
                _itemMap[key] = 0;

                if (fireChangedEvent)
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, _itemMap[key]));
                }
            }
        }

        public void Add(T key, int item)
        {
            _itemMap.Add(key, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void AddRange(T key, IEnumerable<int> items)
        {
            foreach (var item in items)
            {
                _itemMap.Add(key, item);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public void Remove(T key)
        {
            _itemMap.Remove(key);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, key));
        }

        public void RemoveRange(IEnumerable<T> keys)
        {
            foreach (var key in keys)
            {
                _itemMap.Remove(key);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, keys));
        }

        public void Clear()
        {
            _itemMap.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        public Dictionary<T, int> GetItemSet()
        {
            return _itemMap;
        }

        public IEnumerator GetEnumerator()
        {
            return _itemMap.GetEnumerator();
        }
    }
}
