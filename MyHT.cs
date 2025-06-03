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
    public class MyHashTable<TKey, TValue> : IDictionary<TKey, TValue>
    {
        // Основное хранилище данных - массив элементов (Point)
        private Point<TKey, TValue>[] table;

        // Количество элементов в таблице
        private int count = 0;

        // Пороговое значение коэффициента заполнения для расширения таблицы
        private readonly double loadFactorThreshold;

        // Компаратор для сравнения ключей
        private readonly IEqualityComparer<TKey> comparer;

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
        /// Основной конструктор хеш-таблицы
        /// </summary>
        /// <param name="capacity">Начальная емкость таблицы</param>
        /// <param name="loadFactorThreshold">Порог заполнения для расширения (0.0 - 1.0)</param>
        /// <param name="comparer">Компаратор для сравнения ключей</param>
        public MyHashTable(int capacity = 10, double loadFactorThreshold = 0.72,
                         IEqualityComparer<TKey> comparer = null)
        {
            // Проверка входных параметров
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive");
            if (loadFactorThreshold <= 0 || loadFactorThreshold > 1)
                throw new ArgumentException("Invalid load factor threshold");

            // Инициализация полей
            table = new Point<TKey, TValue>[capacity];
            this.loadFactorThreshold = loadFactorThreshold;
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        /// <summary>
        /// Конструктор для создания таблицы со случайными элементами
        /// </summary>
        /// <param name="length">Количество случайных элементов</param>
        public MyHashTable(int length)
        {
            // Рассчитываем начальную емкость
            int capacity = (int)(length / 0.7) + 1;

            // Инициализируем основные поля
            if (capacity <= 0) throw new ArgumentException("Capacity must be positive");
            table = new Point<TKey, TValue>[capacity];
            this.loadFactorThreshold = 0.72; // стандартное значение
            this.comparer = EqualityComparer<TKey>.Default;
            this.random = new Random();

            // Заполняем таблицу случайными элементами
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
                    // Пропускаем неподдерживаемые типы
                    continue;
                }
            }
        }

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="source">Исходная таблица для копирования</param>
        public MyHashTable(MyHashTable<TKey, TValue> source) : this(source.table.Length)
        {
            // Копирование всех элементов из исходной таблицы
            foreach (var item in source)
            {
                this.Add(item.Key, item.Value);
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

                // Поиск существующего элемента
                int index = GetIndex(key);
                for (int i = 0; i < table.Length; i++)
                {
                    int currentIndex = (index + i) % table.Length;
                    var entry = table[currentIndex];

                    if (entry == null)
                    {
                        // Ключ не найден - добавляем новый элемент
                        Add(key, value);
                        return;
                    }

                    if (!entry.IsDeleted && comparer.Equals(entry.Key, key))
                    {
                        // Ключ найден - обновляем значение
                        entry.Value = value;
                        return;
                    }
                }

                // Если не нашли - добавляем новый
                Add(key, value);
            }
        }

        /// <summary>
        /// Проверка наличия ключа в таблице
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (count == 0) return false;

            int index = GetIndex(key);

            // Линейное пробирование для поиска ключа
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

        /// <summary>
        /// Добавление нового элемента в таблицу
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            // Проверка необходимости расширения таблицы
            if (LoadFactor >= loadFactorThreshold)
                Resize();

            int index = GetIndex(key);

            // Поиск места для вставки (свободной ячейки или ячейки с удаленным элементом)
            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null || entry.IsDeleted)
                {
                    // Нашли свободное место - вставляем элемент
                    table[currentIndex] = new Point<TKey, TValue>(key, value);
                    count++;
                    return;
                }

                // Проверка на дубликат ключа
                if (comparer.Equals(entry.Key, key))
                    throw new ArgumentException($"An item with the same key '{key}' already exists");
            }

            // Если не нашли свободного места (маловероятно после Resize)
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

            // Линейное пробирование для поиска ключа
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

        /// <summary>
        /// Удаление элемента по ключу (логическое удаление)
        /// </summary>
        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (count == 0) return false;

            int index = GetIndex(key);

            // Поиск элемента для удаления
            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    break;

                if (!entry.IsDeleted && comparer.Equals(entry.Key, key))
                {
                    // Логическое удаление (помечаем флагом)
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

            // Копирование элементов в массив
            int i = arrayIndex;
            foreach (var item in this)
            {
                array[i++] = item;
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
        private int GetIndex(TKey key) => Math.Abs(comparer.GetHashCode(key)) % table.Length;

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