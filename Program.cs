using System;
using System.Collections.Generic;
using Geometryclass;

namespace lab12._4
{
    class Program
    {
        static Random random = new Random();

        static Geometryfigure1 GenerateRandomFigure()
        {
            // Случайный выбор типа фигуры (1 - круг, 2 - прямоугольник, 3 - параллелепипед)
            int figureType = random.Next(1, 4);

            switch (figureType)
            {
                case 1: // Круг
                    double radius = random.Next(1, 11);
                    return new Circle1(radius);

                case 2: // Прямоугольник
                    double width = random.Next(1, 11);
                    double height = random.Next(1, 11);
                    return new Rectangle1(width, height);

                case 3: // Параллелепипед
                    double a = random.Next(1, 11);
                    double b = random.Next(1, 11);
                    double c = random.Next(1, 11);
                    return new Parallelepiped1(a, b, c);

                default:
                    throw new InvalidOperationException("Неизвестный тип фигуры");
            }
        }

        static string GenerateRandomKey()
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

        static void Main(string[] args)
        {
            MyHashTable<string, Geometryfigure1> table = new MyHashTable<string, Geometryfigure1>();
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Демонстрация работы MyHashTable");
                Console.WriteLine("1. Создать новую таблицу");
                Console.WriteLine("2. Добавить случайный элемент");
                Console.WriteLine("3. Добавить 10 случайных элементов");
                Console.WriteLine("4. Найти элемент по ключу");
                Console.WriteLine("5. Удалить элемент по ключу");
                Console.WriteLine("6. Проверить наличие ключа");
                Console.WriteLine("7. Вывести таблицу");
                Console.WriteLine("8. Перебор foreach");
                Console.WriteLine("9. Очистить таблицу");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите действие: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Некорректный ввод!");
                    Console.ReadKey();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Введите емкость таблицы (по умолчанию 10): ");
                        if (int.TryParse(Console.ReadLine(), out int capacity) && capacity > 0)
                            table = new MyHashTable<string, Geometryfigure1>(capacity);
                        else
                            table = new MyHashTable<string, Geometryfigure1>();
                        Console.WriteLine("Таблица создана.");
                        break;

                    case 2:
                        string key = GenerateRandomKey();
                        Geometryfigure1 figure = GenerateRandomFigure();

                        try
                        {
                            table.Add(key, figure);
                            Console.WriteLine($"Добавлен элемент с ключом '{key}': {figure}");
                            Console.WriteLine($"Текущий LoadFactor: {table.LoadFactor:P2}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка: {ex.Message}");
                        }
                        break;

                    case 3:
                        for (int i = 0; i < 10; i++)
                        {
                            string newKey = GenerateRandomKey();
                            Geometryfigure1 newFigure = GenerateRandomFigure();

                            try
                            {
                                table.Add(newKey, newFigure);
                                Console.WriteLine($"Добавлен элемент с ключом '{newKey}': {newFigure}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка при добавлении: {ex.Message}");
                            }
                        }
                        Console.WriteLine($"Текущий LoadFactor: {table.LoadFactor:P2}");
                        break;

                    case 4:
                        Console.Write("Введите ключ для поиска: ");
                        string searchKey = Console.ReadLine();
                        if (table.TryGetValue(searchKey, out Geometryfigure1 found))
                            Console.WriteLine($"Найден элемент: {found}");
                        else
                            Console.WriteLine("Элемент не найден!");
                        break;

                    case 5:
                        Console.Write("Введите ключ для удаления: ");
                        string removeKey = Console.ReadLine();
                        if (table.Remove(removeKey))
                            Console.WriteLine("Элемент удален.");
                        else
                            Console.WriteLine("Элемент не найден.");
                        break;

                    case 6:
                        Console.Write("Введите ключ для проверки: ");
                        string checkKey = Console.ReadLine();
                        Console.WriteLine($"Ключ {(table.ContainsKey(checkKey) ? "найден" : "не найден")}");
                        break;

                    case 7:
                        Console.WriteLine(table.GetTableInfo());
                        break;

                    case 8:
                        Console.WriteLine("Перебор элементов:");
                        foreach (var pair in table)
                        {
                            Console.WriteLine($"{pair.Key}: {pair.Value}");
                        }
                        break;

                    case 9:
                        table.Clear();
                        Console.WriteLine("Таблица очищена.");
                        break;

                    case 0:
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }
    }
}