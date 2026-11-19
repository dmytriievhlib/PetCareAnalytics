
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PetCareAnalytics.Core.Models;
using PetCareAnalytics.Core.Repositories;
using PetCareAnalytics.Core.Services;
using PetCareAnalytics.Core.Storage;

Console.OutputEncoding = Encoding.UTF8;


var databasePath = PetCareDatabase.GetDatabasePath();

var options = new DbContextOptionsBuilder<PetCareDbContext>()
    .UseSqlite($"Data Source={databasePath}")
    .Options;

using var dbContext = new PetCareDbContext(options);

dbContext.Database.EnsureCreated();


var repository = new PetCareRepository(dbContext);
var analysisService = new MeasurementAnalysisService();

var petCareService = new PetCareApplicationService(
    repository,
    analysisService);

Console.WriteLine($"База даних: {databasePath}");

// Демонстраційні дані додаються лише до порожньої бази.
if (petCareService.GetAllPets().Count == 0)
{
    var demoPet = petCareService.AddPet(new Pet
    {
        Name = "Бобік",
        Species = "Собака",
        Breed = "Лабрадор",
        BirthDate = new DateOnly(2022, 5, 12),
        CurrentWeightKg = 12.5
    });

    var demoMeasurements = new[]
    {
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Активність",
            Value = 45,
            Unit = "хв",
            RecordedAt = DateTime.Now.AddDays(-2)
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Активність",
            Value = 50,
            Unit = "хв",
            RecordedAt = DateTime.Now.AddDays(-1)
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Активність",
            Value = 20,
            Unit = "хв"
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Вода",
            Value = 800,
            Unit = "мл",
            RecordedAt = DateTime.Now.AddDays(-2)
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Вода",
            Value = 850,
            Unit = "мл",
            RecordedAt = DateTime.Now.AddDays(-1)
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Вода",
            Value = 1200,
            Unit = "мл"
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Сон",
            Value = 10,
            Unit = "год",
            RecordedAt = DateTime.Now.AddDays(-2)
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Сон",
            Value = 9,
            Unit = "год",
            RecordedAt = DateTime.Now.AddDays(-1)
        },
        new PetMeasurement
        {
            PetId = demoPet.Id,
            MeasurementType = "Сон",
            Value = 8,
            Unit = "год"
        }
    };

    foreach (var measurement in demoMeasurements)
    {
        petCareService.AddMeasurement(measurement);
    }
}

// Головне меню.
while (true)
{
    Console.WriteLine();
    Console.WriteLine("==================================");
    Console.WriteLine("       PET CARE ANALYTICS");
    Console.WriteLine("==================================");
    Console.WriteLine("1. Переглянути всіх тварин");
    Console.WriteLine("2. Додати тварину");
    Console.WriteLine("3. Переглянути профіль тварини");
    Console.WriteLine("4. Додати вимірювання");
    Console.WriteLine("5. Проаналізувати показники");
    Console.WriteLine("0. Вийти");
    Console.WriteLine("==================================");
    Console.Write("Виберіть дію: ");

    var choice = Console.ReadLine()?.Trim();

    try
    {
        switch (choice)
        {
            case "1":
                ShowAllPets();
                break;

            case "2":
                AddPet();
                break;

            case "3":
                ShowPetProfile();
                break;

            case "4":
                AddMeasurement();
                break;

            case "5":
                AnalyzeMeasurements();
                break;

            case "0":
                Console.WriteLine("Роботу програми завершено.");
                return;

            default:
                Console.WriteLine("Невідома команда. Спробуйте ще раз.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }

    Console.WriteLine();
    Console.Write("Натисніть Enter, щоб повернутися до меню...");
    Console.ReadLine();
}

// 1. Перегляд усіх тварин.
void ShowAllPets()
{
    var pets = petCareService.GetAllPets();

    Console.WriteLine("\n=== Список тварин ===");

    if (pets.Count == 0)
    {
        Console.WriteLine("У базі даних поки немає тварин.");
        return;
    }

    foreach (var pet in pets)
    {
        Console.WriteLine(
            $"ID: {pet.Id} | Ім'я: {pet.Name} | " +
            $"Вид: {pet.Species} | Порода: {pet.Breed}");
    }

    Console.WriteLine($"Усього тварин: {pets.Count}");
}

// 2. Додавання тварини.
void AddPet()
{
    Console.WriteLine("\n=== Додавання тварини ===");

    Console.Write("Ім'я: ");
    var name = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Ім'я не може бути порожнім.");
        return;
    }

    Console.Write("Вид тварини (наприклад, Собака, Кіт): ");
    var species = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(species))
    {
        Console.WriteLine("Вид тварини обов'язковий.");
        return;
    }

    Console.Write("Порода (можна залишити порожньою): ");
    var breed = Console.ReadLine()?.Trim() ?? "";

    Console.Write("Дата народження (ДД.ММ.РРРР, або Enter): ");
    var birthDateText = Console.ReadLine()?.Trim() ?? "";

    DateOnly? birthDate = null;

    if (birthDateText.Length > 0)
    {
        if (!DateOnly.TryParseExact(
                birthDateText,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate))
        {
            Console.WriteLine("Неправильний формат дати.");
            return;
        }

        if (parsedDate > DateOnly.FromDateTime(DateTime.Today))
        {
            Console.WriteLine("Дата народження не може бути в майбутньому.");
            return;
        }

        birthDate = parsedDate;
    }

    Console.Write("Поточна вага у кг (або Enter): ");
    var weightText = Console.ReadLine()?.Trim() ?? "";

    double? weight = null;

    if (weightText.Length > 0)
    {
        if (!TryReadNumber(weightText, out var parsedWeight)
            || parsedWeight <= 0)
        {
            Console.WriteLine("Вага має бути числом, більшим за нуль.");
            return;
        }

        weight = parsedWeight;
    }

    var pet = petCareService.AddPet(new Pet
    {
        Name = name,
        Species = species,
        Breed = breed,
        BirthDate = birthDate,
        CurrentWeightKg = weight
    });

    Console.WriteLine(
        $"Тварину «{pet.Name}» успішно додано. ID: {pet.Id}");
}

// 3. Перегляд профілю та історії вимірювань.
void ShowPetProfile()
{
    Console.WriteLine("\n=== Профіль тварини ===");

    var pet = FindPetById();

    if (pet is null)
        return;

    Console.WriteLine($"ID: {pet.Id}");
    Console.WriteLine($"Ім'я: {pet.Name}");
    Console.WriteLine($"Вид: {pet.Species}");
    Console.WriteLine($"Порода: {pet.Breed}");
    Console.WriteLine(
        $"Дата народження: " +
        $"{(pet.BirthDate.HasValue ? pet.BirthDate.Value.ToString("dd.MM.yyyy") : "не вказана")}");
    Console.WriteLine(
        $"Вага: {(pet.CurrentWeightKg.HasValue ? pet.CurrentWeightKg.Value + " кг" : "не вказана")}");

    var measurements = petCareService.GetMeasurements(pet.Id);

    Console.WriteLine("\nІсторія вимірювань:");

    if (measurements.Count == 0)
    {
        Console.WriteLine("Вимірювань ще немає.");
        return;
    }

    foreach (var measurement in measurements)
    {
        Console.WriteLine(
            $"ID: {measurement.Id} | " +
            $"{measurement.MeasurementType}: " +
            $"{measurement.Value} {measurement.Unit} | " +
            $"{measurement.RecordedAt:dd.MM.yyyy HH:mm}" +
            (string.IsNullOrWhiteSpace(measurement.Notes)
                ? ""
                : $" | Примітка: {measurement.Notes}"));
    }

    Console.WriteLine($"Усього вимірювань: {measurements.Count}");
}

// 4. Додавання вимірювання.
void AddMeasurement()
{
    Console.WriteLine("\n=== Додавання вимірювання ===");

    var pet = FindPetById();

    if (pet is null)
        return;

    Console.WriteLine($"Тварина: {pet.Name} (ID: {pet.Id})");
    Console.WriteLine("Приклади типів: Активність, Вода, Сон, Вага");

    Console.Write("Тип показника: ");
    var type = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(type))
    {
        Console.WriteLine("Тип показника обов'язковий.");
        return;
    }

    Console.Write("Значення: ");
    var valueText = Console.ReadLine()?.Trim() ?? "";

    if (!TryReadNumber(valueText, out var value) || value < 0)
    {
        Console.WriteLine(
            "Введіть коректне невід'ємне числове значення.");
        return;
    }

    Console.Write("Одиниця вимірювання (хв, мл, год, кг тощо): ");
    var unit = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(unit))
    {
        Console.WriteLine("Одиниця вимірювання обов'язкова.");
        return;
    }

    Console.Write("Примітка (необов'язково): ");
    var notes = Console.ReadLine()?.Trim();

    petCareService.AddMeasurement(new PetMeasurement
    {
        PetId = pet.Id,
        RecordedAt = DateTime.Now,
        MeasurementType = type,
        Value = value,
        Unit = unit,
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
    });

    Console.WriteLine("Вимірювання успішно збережено в SQLite.");
}

// 5. Аналіз показників.
void AnalyzeMeasurements()
{
    Console.WriteLine("\n=== Аналіз показників ===");

    var pet = FindPetById();

    if (pet is null)
        return;

    var types = petCareService.GetMeasurementTypes(pet.Id);

    if (types.Count == 0)
    {
        Console.WriteLine("Для цієї тварини немає вимірювань.");
        return;
    }

    Console.WriteLine($"Тварина: {pet.Name}");
    Console.WriteLine("Доступні типи показників:");

    foreach (var type in types)
    {
        Console.WriteLine("- " + type);
    }

    Console.Write("\nВведіть тип показника для аналізу: ");
    var selectedType = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(selectedType))
    {
        Console.WriteLine("Тип показника не вказано.");
        return;
    }

    Console.Write("Поріг відхилення у відсотках (Enter = 30): ");
    var thresholdText = Console.ReadLine()?.Trim() ?? "";

    var threshold = 30.0;

    if (thresholdText.Length > 0)
    {
        if (!TryReadNumber(thresholdText, out threshold)
            || threshold < 0)
        {
            Console.WriteLine("Поріг має бути невід'ємним числом.");
            return;
        }
    }

    Console.WriteLine("\nРезультат:");

    Console.WriteLine(
        petCareService.AnalyzeMeasurements(
            pet.Id,
            selectedType,
            threshold));
}

// Пошук тварини за ID.
Pet? FindPetById()
{
    Console.Write("Введіть ID тварини: ");
    var idText = Console.ReadLine()?.Trim() ?? "";

    if (!int.TryParse(idText, out var id) || id <= 0)
    {
        Console.WriteLine("ID має бути додатним цілим числом.");
        return null;
    }

    var pet = petCareService.GetPetById(id);

    if (pet is null)
    {
        Console.WriteLine($"Тварину з ID {id} не знайдено.");
    }

    return pet;
}

// Читання числа з крапкою або комою як десятковим роздільником.
bool TryReadNumber(string input, out double value)
{
    var normalized = input.Trim().Replace(',', '.');

    return double.TryParse(
        normalized,
        NumberStyles.Float,
        CultureInfo.InvariantCulture,
        out value)
        && double.IsFinite(value);
}
