using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace PLINQ
{
    public class Resume
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Experience { get; set; }
        public string City { get; set; }
        public decimal SalaryDemand { get; set; }
    }

    internal class Program
    {
        private const string NumbersFileName = "numbers.txt";
        private const string ResumeFileName = "resume.txt";

        static void Main(string[] args)
        {

            ProcessUniqueNumbers();

            ProcessMaxIncreasingSequence();

            ProcessResumes();

            WriteLine("Все операции завершены. Нажмите любую клавишу для выхода.");
            ReadKey();
        }

        static void ProcessUniqueNumbers()
        {
            WriteLine("=== Задание 1: Подсчет уникальных значений ===");

            try
            {
                CreateNumbersFile();
                var numbers = ReadNumbersFromFile(NumbersFileName);

                if (!numbers.Any())
                {
                    WriteLine("Файл не содержит числовых данных.");
                    return;
                }

                int uniqueCount = numbers.AsParallel()
                                       .Distinct()
                                       .Count();

                WriteLine($"Количество уникальных значений: {uniqueCount}");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при обработке уникальных значений: {ex.Message}");
            }
        }

        static void ProcessMaxIncreasingSequence()
        {
            WriteLine("\n=== Задание 2: Поиск максимальной возрастающей последовательности ===");

            try
            {
                var numbers = ReadNumbersFromFile(NumbersFileName);

                if (!numbers.Any())
                {
                    WriteLine("Список пуст или отсутствуют данные.");
                    return;
                }

                int maxLength = FindMaxIncreasingSequenceLength(numbers);
                WriteLine($"Максимальная длина возрастающей последовательности: {maxLength}");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при поиске последовательности: {ex.Message}");
            }
        }

        static void ProcessResumes()
        {
            WriteLine("\n=== Задание 3: Анализ резюме ===");

            try
            {
                CreateResumesFile();
                var allResumes = LoadAllResumes();

                if (!allResumes.Any())
                {
                    WriteLine("Нет данных о резюме для анализа.");
                    return;
                }

                GenerateReports(allResumes);
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при анализе резюме: {ex.Message}");
            }
        }

        static List<int> ReadNumbersFromFile(string filename)
        {
            return File.ReadLines(filename)
                      .Where(line => int.TryParse(line, out _))
                      .Select(int.Parse)
                      .ToList();
        }

        static int FindMaxIncreasingSequenceLength(List<int> numbers)
        {
            return numbers
                .AsParallel()
                .Select((value, index) =>
                {
                    int length = 1;
                    for (int i = index + 1; i < numbers.Count; i++)
                    {
                        if (numbers[i] > numbers[i - 1])
                            length++;
                        else
                            break;
                    }
                    return length;
                })
                .Max();
        }

        static List<Resume> LoadAllResumes()
        {
            var files = new List<string> { ResumeFileName };
            var allResumes = new List<Resume>();
            var lockObj = new object();

            Parallel.ForEach(files, file =>
            {
                if (File.Exists(file))
                {
                    var resumes = ParseResumesFromFile(file);
                    lock (lockObj)
                    {
                        allResumes.AddRange(resumes);
                    }
                }
            });

            return allResumes;
        }

        static List<Resume> ParseResumesFromFile(string filename)
        {
            var resumes = new List<Resume>();

            foreach (var line in File.ReadAllLines(filename))
            {
                var parts = line.Split(';');
                if (parts.Length == 5)
                {
                    var resume = new Resume
                    {
                        Name = parts[0].Trim(),
                        Age = int.TryParse(parts[1], out var age) ? age : 0,
                        Experience = int.TryParse(parts[2], out var exp) ? exp : 0,
                        City = parts[3].Trim(),
                        SalaryDemand = decimal.TryParse(parts[4], out var sal) ? sal : 0
                    };
                    resumes.Add(resume);
                }
            }
            return resumes;
        }

        static void GenerateReports(List<Resume> resumes)
        {
            var mostExperienced = resumes.OrderByDescending(r => r.Experience).FirstOrDefault();
            var leastExperienced = resumes.OrderBy(r => r.Experience).FirstOrDefault();

            var candidatesByCity = resumes
                .GroupBy(r => r.City)
                .ToDictionary(g => g.Key, g => g.ToList());

            var minSalaryCandidate = resumes.OrderBy(r => r.SalaryDemand).FirstOrDefault();
            var maxSalaryCandidate = resumes.OrderByDescending(r => r.SalaryDemand).FirstOrDefault();

            var averageSalary = resumes.Average(r => r.SalaryDemand);
            var averageExperience = resumes.Average(r => r.Experience);

            WriteLine($"Самый опытный кандидат: {mostExperienced?.Name} с {mostExperienced?.Experience} лет опыта");
            WriteLine($"Самый неопытный кандидат: {leastExperienced?.Name} с {leastExperienced?.Experience} лет опыта");
            WriteLine();

            WriteLine("Кандидаты по городам:");
            foreach (var cityGroup in candidatesByCity)
            {
                WriteLine($"  {cityGroup.Key} ({cityGroup.Value.Count} кандидатов):");
                foreach (var candidate in cityGroup.Value)
                {
                    WriteLine($"    - {candidate.Name}, {candidate.Age} лет, {candidate.Experience} лет опыта, зарплата: {candidate.SalaryDemand:C}");
                }
            }
            WriteLine();

            WriteLine($"Кандидат с минимальным зарплатным требованием: {minSalaryCandidate?.Name} - {minSalaryCandidate?.SalaryDemand:C}");
            WriteLine($"Кандидат с максимальным зарплатным требованием: {maxSalaryCandidate?.Name} - {maxSalaryCandidate?.SalaryDemand:C}");
            WriteLine();

            WriteLine($"Средняя зарплата: {averageSalary:C}");
            WriteLine($"Средний опыт работы: {averageExperience:F1} лет");
            WriteLine($"Всего кандидатов: {resumes.Count}");
        }

        static void CreateNumbersFile()
        {
            int[] numbers = { 5, 3, 7, 3, 9, 5, 2, 8, 7, 10, 1, 12, 15, 11, 6 };

            try
            {
                File.WriteAllLines(NumbersFileName, numbers.Select(n => n.ToString()));
                WriteLine($"Файл '{NumbersFileName}' успешно создан и заполнен данными.");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при создании файла: {ex.Message}");
                throw;
            }
        }

        static void CreateResumesFile()
        {
            var resumes = new List<string>
            {
        "Сергей Новиков;32;6;Новосибирск;65000",
        "Ольга Васнецова;27;3;Екатеринбург;48000",
        "Артем Белов;38;12;Краснодар;82000",
        "Юлия Ковалева;26;2;Ростов-на-Дону;42000",
        "Павел Григорьев;45;18;Воронеж;95000",
        "Алина Морозова;23;1;Нижний Новгород;38000",
        "Максим Орлов;31;5;Уфа;58000",
        "Дарья Лебедева;34;9;Самара;72000",
        "Кирилл Семенов;29;4;Красноярск;52000",
        "Наталья Петрова;41;16;Челябинск;88000",
        "Игорь Федоров;36;11;Волгоград;78000",
        "Екатерина Захарова;24;2;Пермь;46000",
        "Владимир Козлов;39;14;Тюмень;83000",
        "Светлана Павлова;28;3;Омск;44000",
        "Андрей Медведев;33;7;Томск;61000"
            };

            try
            {
                File.WriteAllLines(ResumeFileName, resumes);
                WriteLine($"Файл '{ResumeFileName}' успешно создан и заполнен тестовыми резюме.");
            }
            catch (Exception ex)
            {
                WriteLine($"Ошибка при создании файла: {ex.Message}");
                throw;
            }
        }
    }
}