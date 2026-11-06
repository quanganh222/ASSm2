using System;
using System.Collections.Generic;
using System.Linq;
using Metroidvania.Types;

namespace Metroidvania.Repositories
{
    public class InMemoryRepository<T> where T : IBaseEntity
    {
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();


        public List<T> GetAll()
        {
            return _items.Values.ToList();
        }


        public T GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return default;

            _items.TryGetValue(id, out T item);
            return item;
        }

        public T Create(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Cannot create null entity.");

            if (string.IsNullOrEmpty(item.Id))
                throw new ArgumentException("Entity must have a non-empty Id.");

            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException(
                    $"Entity with Id '{item.Id}' already exists. Use Update instead.");

            _items[item.Id] = item;
            return item;
        }

        public T Update(string id, Action<T> updateAction)
        {
            if (string.IsNullOrEmpty(id))
                return default;
            if (updateAction == null)                          
                throw new ArgumentNullException(nameof(updateAction));  
            if (!_items.ContainsKey(id))
                return default;

            T item = _items[id];
            updateAction(item);
            item.UpdatedAt = DateTime.Now;
            return item;
        }

        public bool Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return _items.Remove(id);
        }

        public bool Exists(string id)
        {
            return !string.IsNullOrEmpty(id) && _items.ContainsKey(id);
        }

        public int Count()
        {
            return _items.Count;
        }

        public List<T> Find(Predicate<T> predicate)
        {
            if (predicate == null)
                return GetAll();

            return _items.Values.ToList().FindAll(predicate);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}