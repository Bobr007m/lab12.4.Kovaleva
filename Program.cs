using System;
using System.Collections.Generic;
using Geometryclass;

namespace lab12._4
{
    class Program
    {
        static void Main(string[] args)
        {
            MyHashTable<Geometryfigure1> table = new MyHashTable<Geometryfigure1>();
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== Демонстрация работы MyHashTable ===");
                Console.WriteLine("1. Создать новую таблицу");
                Console.WriteLine("2. Добавить элемент");
                Console.WriteLine("3. Найти элемент по ключу");
                Console.WriteLine("4. Удалить элемент по ключу");
                Console.WriteLine("5. Проверить наличие ключа");
                Console.WriteLine("6. Вывести таблицу");
                Console.WriteLine("7. Перебор foreach");
                Console.WriteLine("8. Очистить таблицу");
                Console.WriteLine("9. Скопировать в массив");
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
                            table = new MyHashTable<Geometryfigure1>(capacity);
                        else
                            table = new MyHashTable<Geometryfigure1>();
                        Console.WriteLine("Таблица создана.");
                        break;

                    case 2:
                        Console.Write("Введите ключ: ");
                        string key = Console.ReadLine();
                        Console.WriteLine("Выберите тип фигуры (1 - Круг, 2 - Прямоугольник): ");
                        if (int.TryParse(Console.ReadLine(), out int figureType))
                        {
                            Geometryfigure1 figure = null;
                            if (figureType == 1)
                            {
                                Console.Write("Введите радиус круга: ");
                                if (double.TryParse(Console.ReadLine(), out double radius))
                                    figure = new Circle1(radius);
                            }
                            else if (figureType == 2)
                            {
                                Console.Write("Введите ширину прямоугольника: ");
                                if (double.TryParse(Console.ReadLine(), out double width))
                                {
                                    Console.Write("Введите высоту прямоугольника: ");
                                    if (double.TryParse(Console.ReadLine(), out double height))
                                        figure = new Rectangle1(width, height);
                                }
                            }

                            if (figure != null)
                            {
                                try
                                {
                                    table.Add(key, figure);
                                    Console.WriteLine($"Элемент добавлен. LoadFactor = {table.LoadFactor:P2}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Некорректные параметры фигуры!");
                            }
                        }
                        break;

                    case 3:
                        Console.Write("Введите ключ для поиска: ");
                        string searchKey = Console.ReadLine();
                        try
                        {
                            var found = table.Find(searchKey);
                            Console.WriteLine($"Найден элемент: {found}");
                        }
                        catch (KeyNotFoundException)
                        {
                            Console.WriteLine("Элемент не найден!");
                        }
                        break;

                    case 4:
                        Console.Write("Введите ключ для удаления: ");
                        string removeKey = Console.ReadLine();
                        if (table.Remove(removeKey))
                            Console.WriteLine("Элемент удален.");
                        else
                            Console.WriteLine("Элемент не найден.");
                        break;

                    case 5:
                        Console.Write("Введите ключ для проверки: ");
                        string checkKey = Console.ReadLine();
                        Console.WriteLine($"Ключ {(table.ContainsKey(checkKey) ? "найден" : "не найден")}");
                        break;

                    case 6:
                        table.PrintTable();
                        break;

                    case 7:
                        Console.WriteLine("Перебор элементов через foreach:");
                        foreach (var item in table)
                        {
                            Console.WriteLine(item);
                        }
                        Console.WriteLine("\nПеребор пар ключ-значение:");
                        foreach (var pair in (table as IEnumerable<KeyValuePair<string, Geometryfigure1>>))
                        {
                            Console.WriteLine($"{pair.Key}: {pair.Value}");
                        }
                        break;

                    case 8:
                        table.Clear();
                        Console.WriteLine("Таблица очищена.");
                        break;

                    case 9:
                        var array = new Geometryfigure1[table.Count];
                        table.CopyTo(array, 0);
                        Console.WriteLine("Элементы скопированы в массив:");
                        foreach (var item in array)
                        {
                            Console.WriteLine(item);
                        }
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