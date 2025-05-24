using Geometryclass;
using laba12._2;
using System;
using System.Collections;
using System.Collections.Generic;

namespace lab12._4
{
    public class MyHashTable<T> : IEnumerable<T>, ICollection<T>, IDictionary<string, T> where T : Geometryfigure1
    {
        static Random random = new Random();
        // Массив записей Point<T>, представляющий собой хэш-таблицу
        public Point<T>[] table;

        // Счётчик текущего количества элементов в таблице
        private int count = 0;

        // Свойство, возвращающее количество элементов
        public int Count => count;
        public ICollection<string> Keys => GetKeys();
        public ICollection<T> Values => GetValues();

        // Коэффициент заполненности таблицы
        public double LoadFactor => (double)count / table.Length;

        public bool IsReadOnly => throw new NotImplementedException();
        public T this[string key]
        {
            get => Find(key);
            set => Add(key, value);
        }
        public bool IsDeleted;
        // Конструкторы
        public MyHashTable() : this(10) { }

        public MyHashTable(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            table = new Point<T>[length];
            for (int i = 0; i < length; i++)
            {
                Add(GenerateRandomKey(), GenerateRandomFigure());
            }
        }

        public MyHashTable(MyHashTable<T> c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));

            table = new Point<T>[c.table.Length];
            count = c.count;

            for (int i = 0; i < c.table.Length; i++)
            {
                if (c.table[i] != null && !c.table[i].IsDeleted)
                {
                    table[i] = new Point<T>(c.table[i].Key, c.table[i].Value);
                }
            }
        }

        // Методы для генерации случайных данных
        private static string GenerateRandomKey()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int length = random.Next(5, 10);
            char[] key = new char[length];

            for (int i = 0; i < length; i++)
            {
                key[i] = chars[random.Next(chars.Length)];
            }

            return new string(key);
        }

        private static T GenerateRandomFigure()
        {
            int figureType = random.Next(1, 4);

            switch (figureType)
            {
                case 1: // Круг
                    return (T)(object)new Circle1(random.Next(1, 11));

                case 2: // Прямоугольник
                    return (T)(object)new Rectangle1(random.Next(1, 11), random.Next(1, 11));

                case 3: // Параллелепипед
                    return (T)(object)new Parallelepiped1(random.Next(1, 11), random.Next(1, 11), random.Next(1, 11));

                default:
                    throw new InvalidOperationException("Неизвестный тип фигуры");
            }
        }


        // Конструктор таблицы
        public MyHashTable(int capacity = 10, double loadFactor = 0.72)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (loadFactor < 0 || loadFactor > 1)
                throw new ArgumentOutOfRangeException(nameof(loadFactor));

            // Создание массива заданной ёмкости
            table = new Point<T>[capacity];
        }

        // Проверка наличия ключа в таблице
        public bool ContainsKey(string key)
        {
            if (key == null) return false;

            int index = Math.Abs(key.GetHashCode()) % table.Length;

            //  проход по таблице 
            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    return false; // Если встретили null, значит такого ключа нет

                if (!entry.IsDeleted && entry.Key == key)
                    return true; // Нашли существующий  ключ
            }

            return false;
        }

        // Добавление пары ключ-значение в таблицу
        public void Add(string key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // Если таблица переполнена — увеличиваем её размер
            if (LoadFactor >= 0.72)
                Resize();

            int index = Math.Abs(key.GetHashCode()) % table.Length;

            // Поиск подходящей позиции для вставки
            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                {
                    // Вставка новой записи
                    table[currentIndex] = new Point<T>(key, value);
                    count++;
                    return;
                }
                else if (entry.IsDeleted || !entry.Key.Equals(key)) continue;

                // Если такой ключ уже есть —  исключение
                throw new ArgumentException("Элемент с таким ключом уже существует.");
            }

            // Если цикл завершился, а место не найдено — таблица переполнена
            throw new InvalidOperationException("Таблица переполнена.");
        }

        // Увеличение размера таблицы при превышении коэффициента заполнения
        public void Resize()
        {
            Point<T>[] oldTable = table;
            table = new Point<T>[oldTable.Length * 2]; // Удваиваем размер
            count = 0; // Обнуляем счётчик перед повторным добавлением

            foreach (var item in oldTable)
            {
                if (item != null && !item.IsDeleted)
                {
                    Add(item.Key, item.Value); // Перезаписываем все элементы в новую таблицу
                }
            }
        }

        // Получение значения по ключу
        public T Find(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            int index = Math.Abs(key.GetHashCode()) % table.Length;

            // Ищем значение по ключу 
            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    throw new KeyNotFoundException("Элемент не найден.");

                if (!entry.IsDeleted && entry.Key == key)
                    return entry.Value;
            }

            throw new KeyNotFoundException("Элемент не найден.");
        }

        // Удаление элемента по ключу
        public bool Remove(string key)
        {
            if (key == null)
                return false;

            int index = Math.Abs(key.GetHashCode()) % table.Length;

            // Поиск нужного элемента
            for (int i = 0; i < table.Length; i++)
            {
                int currentIndex = (index + i) % table.Length;
                var entry = table[currentIndex];

                if (entry == null)
                    return false;

                if (!entry.IsDeleted && entry.Key == key)
                {
                    entry.IsDeleted = true; // Просто помечаем как удалённый
                    count--;
                    return true;
                }
            }

            return false;
        }

        // Метод вывода содержимого таблицы 
        public void PrintTable()
        {
            Console.WriteLine($"Хеш-таблица ( Элементов: {Count}):");
            for (int i = 0; i < table.Length; i++)
            {
                var entry = table[i];
                if (entry != null)
                {
                    if (entry.IsDeleted)
                        Console.WriteLine($"[{i}]: Удалено");
                    else
                        Console.WriteLine($"[{i}]: {entry.Key} - {entry.Value}");
                }
                else
                {
                    Console.WriteLine($"[{i}]: Пусто");
                }
            }
        }

        // Вспомогательный метод добавления с выводом информации о состоянии таблицы
        public void AddWithLoadFactorCheck(string key, T value)
        {
            try
            {
                Add(key, value);
                Console.WriteLine($"Добавлен {key}, LoadFactor = {LoadFactor:P2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось добавить {key}: {ex.Message}");
            }
        }
        private ICollection<string> GetKeys()
        {
            var keys = new List<string>();
            foreach (var item in table)
            {
                if (item != null && !item.IsDeleted)
                {
                    keys.Add(item.Key);
                }
            }
            return keys;
        }

        private ICollection<T> GetValues()
        {
            var values = new List<T>();
            foreach (var item in table)
            {
                if (item != null && !item.IsDeleted)
                {
                    values.Add(item.Value);
                }
            }
            return values;
        }
        // Реализация IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in table)
            {
                if (item != null && !item.IsDeleted)
                {
                    yield return item.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();// не используется
        // Реализация ICollection<T>
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            Add(Guid.NewGuid().ToString(), item);
        }

        public void Clear()
        {
            table = new Point<T>[table.Length];
            count = 0;
        }

        public bool Contains(T item)
        {
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted && entry.Value.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < count)
                throw new ArgumentException("Недостаточно места в целевом массиве.");

            int i = arrayIndex;
            foreach (var item in this)
            {
                array[i++] = item;
            }
        }

        public bool Remove(T item)
        {
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted && entry.Value.Equals(item))
                {
                    entry.IsDeleted = true;
                    count--;
                    return true;
                }
            }
            return false;
        }
        // Реализация IDictionary<string, T>
        public bool TryGetValue(string key, out T value)
        {
            try
            {
                value = Find(key);
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = default;
                return false;
            }
        }

        void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
        {
            return TryGetValue(item.Key, out var value) && value.Equals(item.Value);
        }

        void ICollection<KeyValuePair<string, T>>.CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < count)
                throw new ArgumentException("Недостаточно места в целевом массиве.");

            int i = arrayIndex;
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted)
                {
                    array[i++] = new KeyValuePair<string, T>(entry.Key, entry.Value);
                }
            }
        }

        bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
        {
            if (((ICollection<KeyValuePair<string, T>>)this).Contains(item))
            {
                return Remove(item.Key);
            }
            return false;
        }

        IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        {
            foreach (var entry in table)
            {
                if (entry != null && !entry.IsDeleted)
                {
                    yield return new KeyValuePair<string, T>(entry.Key, entry.Value);
                }
            }
        }

    }
}