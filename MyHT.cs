using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace lab12._4
{
    public class MyHashTable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Point<TKey, TValue>[] table;
        private int count = 0;
        private readonly double loadFactorThreshold;
        private readonly IEqualityComparer<TKey> comparer;

        public int Count => count;
        public double LoadFactor => (double)count / table.Length;

        public MyHashTable(int capacity = 10, double loadFactorThreshold = 0.72,
                         IEqualityComparer<TKey> comparer = null)
        {
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive");
            if (loadFactorThreshold <= 0 || loadFactorThreshold > 1)
                throw new ArgumentException("Invalid load factor threshold");

            table = new Point<TKey, TValue>[capacity];
            this.loadFactorThreshold = loadFactorThreshold;
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (count == 0) return false;

            int index = GetIndex(key);

            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    return false;

                if (!entry.IsDeleted && comparer.Equals(entry.Key, key))
                    return true;
            }

            return false;
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (LoadFactor >= loadFactorThreshold)
                Resize();

            int index = GetIndex(key);

            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null || entry.IsDeleted)
                {
                    table[currentIndex] = new Point<TKey, TValue>(key, value);
                    count++;
                    return;
                }

                if (comparer.Equals(entry.Key, key))
                    throw new ArgumentException("Duplicate key");
            }

            throw new InvalidOperationException("Failed to insert - table full");
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (count == 0)
            {
                value = default;
                return false;
            }

            int index = GetIndex(key);

            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    break;

                if (!entry.IsDeleted && comparer.Equals(entry.Key, key))
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (count == 0) return false;

            int index = GetIndex(key);

            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    break;

                if (!entry.IsDeleted && comparer.Equals(entry.Key, key))
                {
                    entry.IsDeleted = true;
                    count--;
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            table = new Point<TKey, TValue>[table.Length];
            count = 0;
        }

        public string GetTableInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Хеш-таблица (Элементов: {Count}, Размер: {table.Length}):");
            for (int i = 0; i < table.Length; i++)
            {
                var entry = table[i];
                sb.AppendLine(entry == null
                    ? $"[{i}]: Пусто"
                    : entry.IsDeleted
                        ? $"[{i}]: Удалено"
                        : $"[{i}]: {entry.Key} - {entry.Value}");
            }
            return sb.ToString();
        }

        private void Resize()
        {
            var oldTable = table;
            table = new Point<TKey, TValue>[oldTable.Length * 2];
            count = 0;

            foreach (var item in oldTable)
            {
                if (item != null && !item.IsDeleted)
                {
                    Add(item.Key, item.Value);
                }
            }
        }

        private int GetIndex(TKey key) => Math.Abs(comparer.GetHashCode(key)) % table.Length;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted)
                {
                    yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}