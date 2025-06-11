using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lab12._4
{
    /// <summary>
    /// Реализация хеш-таблицы с методом открытой адресации (линейное пробирование)
    /// </summary>
    /// <typeparam name="TKey">Тип ключа</typeparam>
    /// <typeparam name="TValue">Тип значения</typeparam>
    public class MyHashTable<TKey, TValue> : IDictionary<TKey, TValue>//уже включает в себя другие базовые интерфейсы
    {
        // Основное хранилище данных - массив элементов (Point)
        private Point<TKey, TValue>[] table;

        // Количество элементов в таблице
        private int count = 0;

        // Пороговое значение коэффициента заполнения для расширения таблицы
        private readonly double loadFactorThreshold;

        // Генератор случайных чисел для создания тестовых данных
        private readonly Random random = new Random();

        // Свойства интерфейса IDictionary
        public int Count => count;
        public bool IsReadOnly => false;
        public ICollection<TKey> Keys => GetKeys();
        public ICollection<TValue> Values => GetValues();

        // Текущий коэффициент заполнения таблицы
        public double LoadFactor => (double)count / table.Length;
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public MyHashTable() : this(10) { }

        /// <summary>
        /// Основной конструктор хеш-таблицы
        /// </summary>
        /// <param name="capacity">Начальная емкость таблицы</param>
        /// <param name="loadFactorThreshold">Порог заполнения для расширения (0.0 - 1.0)</param>
        public MyHashTable(int capacity = 10, double loadFactorThreshold = 0.72)
        {
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive");
            if (loadFactorThreshold <= 0 || loadFactorThreshold > 1)
                throw new ArgumentException("Invalid load factor threshold");

            table = new Point<TKey, TValue>[capacity];
            this.loadFactorThreshold = loadFactorThreshold;
        }

        /// <summary>
        /// Конструктор для создания таблицы со случайными элементами
        /// </summary>
        /// <param name="length">Количество случайных элементов</param>
        public MyHashTable(int length)
        {
            int capacity = (int)(length / 0.7) + 1;
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive");

            table = new Point<TKey, TValue>[capacity];
            this.loadFactorThreshold = 0.72;
            this.random = new Random();

            for (int i = 0; i < length; i++)
            {
                try
                {
                    TKey key = GenerateRandomKey();
                    TValue value = GenerateRandomValue();
                    Add(key, value);
                }
                catch (NotSupportedException)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Конструктор копирования с глубокой копией значений
        /// </summary>
        public MyHashTable(MyHashTable<TKey, TValue> source) : this(source.table.Length)
        {
            foreach (var item in source)
            {
                TValue valueCopy;
                if (typeof(TValue).IsValueType || item.Value == null)
                {
                    valueCopy = item.Value; // значение или null
                }
                else if (item.Value is ICloneable cloneable)
                {
                    valueCopy = (TValue)cloneable.Clone();
                }
                else
                {
                    throw new InvalidOperationException("Cannot deep copy non-ICloneable reference type");
                }

                Add(item.Key, valueCopy);
            }
        }

        /// <summary>
        /// Генерация случайного ключа 
        /// </summary>
        private TKey GenerateRandomKey()
        {
            if (typeof(TKey) == typeof(int))
            {
                return (TKey)(object)random.Next();
            }
            else if (typeof(TKey) == typeof(string))
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                int length = random.Next(5, 10);
                return (TKey)(object)new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            throw new NotSupportedException($"Type {typeof(TKey)} not supported for random key generation");
        }

        /// <summary>
        /// Генерация случайного значения (поддерживаются int и string)
        /// </summary>
        private TValue GenerateRandomValue()
        {
            if (typeof(TValue) == typeof(int))
            {
                return (TValue)(object)random.Next();
            }
            else if (typeof(TValue) == typeof(string))
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                int length = random.Next(5, 15);
                return (TValue)(object)new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            throw new NotSupportedException($"Type {typeof(TValue)} not supported for random value generation");
        }

        /// <summary>
        /// Индексатор для доступа к элементам по ключу
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                    return value;
                throw new KeyNotFoundException($"Key '{key}' not found");
            }
            set
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                if (TryFindIndex(key, out int index, out _))
                {
                    // Обновляем значение по найденному индексу
                    table[index].Value = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }
        private bool TryFindIndex(TKey key, out int index, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (GetIndex(key) + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                {
                    index = -1;
                    value = default;
                    return false;
                }

                if (!entry.IsDeleted && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                {
                    index = currentIndex;
                    value = entry.Value;
                    return true;
                }
            }

            index = -1;
            value = default;
            return false;
        }
        /// <summary>
        /// Проверка наличия ключа в таблице
        /// </summary>
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
                if (!entry.IsDeleted && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Добавление нового элемента в таблицу
        /// </summary>
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
                if (!entry.IsDeleted && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                    throw new ArgumentException($"An item with the same key '{key}' already exists");
            }
            throw new InvalidOperationException("Hash table is full");
        }

        /// <summary>
        /// Попытка получить значение по ключу
        /// </summary>
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
                if (!entry.IsDeleted && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                {
                    value = entry.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Удаление элемента по ключу (логическое удаление)
        /// </summary>
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
                if (!entry.IsDeleted && EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                {
                    entry.IsDeleted = true;
                    count--;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Очистка таблицы
        /// </summary>
        public void Clear()
        {
            table = new Point<TKey, TValue>[table.Length];
            count = 0;
        }

        /// <summary>
        /// Добавление элемента через KeyValuePair
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Проверка наличия пары ключ-значение
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (TryGetValue(item.Key, out TValue value))
            {
                return EqualityComparer<TValue>.Default.Equals(value, item.Value);
            }
            return false;
        }

        /// <summary>
        /// Копирование элементов в массив
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < count)
                throw new ArgumentException("Not enough space in destination array");

            int i = arrayIndex;
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted)
                {
                    TValue valueCopy;
                    if (typeof(TValue).IsValueType || entry.Value == null)
                    {
                        valueCopy = entry.Value;
                    }
                    else if (entry.Value is ICloneable cloneable)
                    {
                        valueCopy = (TValue)cloneable.Clone();
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot deep copy non-ICloneable reference type");
                    }

                    array[i++] = new KeyValuePair<TKey, TValue>(entry.Key, valueCopy);
                }
            }
        }

        /// <summary>
        /// Удаление элемента через KeyValuePair
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }
            return false;
        }

        /// <summary>
        /// Получение коллекции ключей
        /// </summary>
        private ICollection<TKey> GetKeys()
        {
            List<TKey> keys = new List<TKey>();
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted)
                {
                    keys.Add(entry.Key);
                }
            }
            return keys;
        }

        /// <summary>
        /// Получение коллекции значений
        /// </summary>
        private ICollection<TValue> GetValues()
        {
            List<TValue> values = new List<TValue>();
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted)
                {
                    values.Add(entry.Value);
                }
            }
            return values;
        }

        /// <summary>
        /// Получение информации о состоянии таблицы (для отладки)
        /// </summary>
        public string GetTableInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Hash table (Elements: {Count}, Size: {table.Length}, Load factor: {LoadFactor:P})");
            for (int i = 0; i < table.Length; i++)
            {
                var entry = table[i];
                sb.AppendLine(entry == null
                    ? $"[{i}]: Empty"
                    : entry.IsDeleted
                        ? $"[{i}]: Deleted"
                        : $"[{i}]: {entry.Key} -> {entry.Value}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Увеличение размера таблицы при достижении порога заполнения
        /// </summary>
        private void Resize()
        {
            var oldTable = table;
            table = new Point<TKey, TValue>[oldTable.Length * 2]; // Удваиваем размер
            count = 0;

            // Перехеширование всех элементов
            foreach (var item in oldTable)
            {
                if (item != null && !item.IsDeleted)
                {
                    Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Вычисление индекса в таблице по ключу
        /// </summary>
        private int GetIndex(TKey key)
        {
            int hashCode = key == null ? 0 : EqualityComparer<TKey>.Default.GetHashCode(key);
            return Math.Abs(hashCode) % table.Length;
        }

        /// <summary>
        /// Получение перечислителя для foreach
        /// </summary>
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

        /// <summary>
        /// Явная реализация интерфейса IEnumerable
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}