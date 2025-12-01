using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ПРАКТИЧЕСКОЕ ЗАДАНИЕ ===");

        Console.WriteLine("\n--- Задание 1: Вычисление факториала ---");
        Task1_Factorial();

        Console.WriteLine("\n--- Задание 2: Операции с числами ---");
        Task2_NumberOperations();

        Console.WriteLine("\n--- Задание 3: Таблица умножения ---");
        Task3_MultiplicationTable();

        Console.WriteLine("\n--- Задание 4: Факториал чисел из файла ---");
        Task4_FactorialFromFile();

        Console.WriteLine("\n--- Задание 5: Статистика чисел из файла ---");
        Task5_StatisticsFromFile();

        Console.WriteLine("\nВсе задания выполнены!");
    }

    static void Task1_Factorial()
    {
        Console.Write("Введите число для вычисления факториала: ");
        if (int.TryParse(Console.ReadLine(), out int number) && number >= 0)
        {
            object lockObject = new object();
            BigInteger result = 1;

            Parallel.For(1, number + 1, i =>
            {
                lock (lockObject)
                {
                    result *= i;
                }
            });

            Console.WriteLine($"Факториал {number} = {result}");
        }
        else
        {
            Console.WriteLine("Некорректный ввод!");
        }
    }

    static void Task2_NumberOperations()
    {
        Console.Write("Введите число для анализа: ");
        if (long.TryParse(Console.ReadLine(), out long number))
        {
            BigInteger factorial = 1;
            int digitCount = 0;
            long digitSum = 0;


            Parallel.Invoke(
                () => {
                    factorial = CalculateFactorial(Math.Abs((int)Math.Min(number, 50))); 
                    Console.WriteLine($"Факториал: {factorial}");
                },
                () => {
                    digitCount = Math.Abs(number).ToString().Length;
                    Console.WriteLine($"Количество цифр: {digitCount}");
                },
                () => {
                    digitSum = CalculateDigitSum(Math.Abs(number));
                    Console.WriteLine($"Сумма цифр: {digitSum}");
                }
            );
        }
        else
        {
            Console.WriteLine("Некорректный ввод!");
        }
    }

    static void Task3_MultiplicationTable()
    {
        try
        {
            Console.Write("Введите начальную границу диапазона: ");
            if (!int.TryParse(Console.ReadLine(), out int start)) return;

            Console.Write("Введите конечную границу диапазона: ");
            if (!int.TryParse(Console.ReadLine(), out int end)) return;

            if (start > end)
            {
                (start, end) = (end, start);
            }

            string fileName = $"multiplication_table_{start}_to_{end}.txt";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                Parallel.For(start, end + 1, i =>
                {
                    string tableSection = GenerateMultiplicationTable(i);

                    lock (writer)
                    {
                        writer.WriteLine(tableSection);
                        writer.WriteLine(new string('-', 30));
                    }
                });
            }

            Console.WriteLine($"Таблица умножения сохранена в файл: {fileName}");

            Console.WriteLine("\nСодержимое файла:");
            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                Console.WriteLine(line);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void Task4_FactorialFromFile()
    {
        try
        {
            string fileName = "numbers.txt";

            if (!File.Exists(fileName))
            {
                CreateTestNumbersFile(fileName);
                Console.WriteLine($"Создан тестовый файл {fileName} с числами");
            }

            List<int> numbers = ReadNumbersFromFile(fileName);
            Console.WriteLine($"Прочитано чисел из файла: {numbers.Count}");

            var results = new (int number, BigInteger factorial)[numbers.Count];

            Parallel.ForEach(numbers, (number, state, index) =>
            {
                results[(int)index] = (number, CalculateFactorial(Math.Abs(number)));
            });

            Console.WriteLine("\nРезультаты вычисления факториалов:");
            foreach (var result in results)
            {
                Console.WriteLine($"{result.number}! = {result.factorial}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void Task5_StatisticsFromFile()
    {
        try
        {
            string fileName = "numbers.txt";

            if (!File.Exists(fileName))
            {
                CreateTestNumbersFile(fileName);
                Console.WriteLine($"Создан тестовый файл {fileName} с числами");
            }

            List<int> numbers = ReadNumbersFromFile(fileName);
            Console.WriteLine($"Прочитано чисел из файла: {numbers.Count}");

            var statistics = numbers
                .AsParallel()
                .Aggregate(
                    new { Sum = 0L, Min = int.MaxValue, Max = int.MinValue, Count = 0 },
                    (acc, num) => new {
                        Sum = acc.Sum + num,
                        Min = Math.Min(acc.Min, num),
                        Max = Math.Max(acc.Max, num),
                        Count = acc.Count + 1
                    },
                    (acc1, acc2) => new {
                        Sum = acc1.Sum + acc2.Sum,
                        Min = Math.Min(acc1.Min, acc2.Min),
                        Max = Math.Max(acc1.Max, acc2.Max),
                        Count = acc1.Count + acc2.Count
                    },
                    acc => new {
                        Sum = acc.Sum,
                        Min = acc.Min,
                        Max = acc.Max,
                        Count = acc.Count,
                        Average = (double)acc.Sum / acc.Count
                    }
                );

            Console.WriteLine("\nСтатистика чисел (используя PLINQ):");
            Console.WriteLine($"Сумма: {statistics.Sum}");
            Console.WriteLine($"Минимум: {statistics.Min}");
            Console.WriteLine($"Максимум: {statistics.Max}");
            Console.WriteLine($"Количество: {statistics.Count}");
            Console.WriteLine($"Среднее: {statistics.Average:F2}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }


    static BigInteger CalculateFactorial(int n)
    {
        if (n == 0 || n == 1) return 1;

        BigInteger result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    static long CalculateDigitSum(long number)
    {
        long sum = 0;
        long n = Math.Abs(number);

        while (n > 0)
        {
            sum += n % 10;
            n /= 10;
        }

        return sum;
    }

    static string GenerateMultiplicationTable(int number)
    {
        var lines = new List<string>();
        lines.Add($"Таблица умножения для {number}:");

        for (int i = 1; i <= 10; i++)
        {
            lines.Add($"{number} * {i} = {number * i}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    static void CreateTestNumbersFile(string fileName)
    {
        var random = new Random();
        var numbers = new List<int>();

        for (int i = 0; i < 20; i++)
        {
            numbers.Add(random.Next(1, 16));
        }

        File.WriteAllLines(fileName, numbers.Select(n => n.ToString()));
    }

    static List<int> ReadNumbersFromFile(string fileName)
    {
        var numbers = new List<int>();

        foreach (string line in File.ReadAllLines(fileName))
        {
            if (int.TryParse(line.Trim(), out int number))
            {
                numbers.Add(number);
            }
        }

        return numbers;
    }
}