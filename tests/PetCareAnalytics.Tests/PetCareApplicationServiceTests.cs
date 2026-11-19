
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PetCareAnalytics.Core.Models;
using PetCareAnalytics.Core.Repositories;
using PetCareAnalytics.Core.Services;
using PetCareAnalytics.Core.Storage;
using System;
using Xunit;

namespace PetCareAnalytics.Tests;

public class PetCareApplicationServiceTests
{
    private sealed class TestContext : IDisposable
    {
        private readonly SqliteConnection _connection;

        public PetCareDbContext DbContext { get; }
        public PetCareApplicationService Service { get; }

        public TestContext()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<PetCareDbContext>()
                .UseSqlite(_connection)
                .Options;

            DbContext = new PetCareDbContext(options);
            DbContext.Database.EnsureCreated();

            var repository = new PetCareRepository(DbContext);
            var analysisService = new MeasurementAnalysisService();

            Service = new PetCareApplicationService(
                repository,
                analysisService);
        }

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Dispose();
        }
    }

    [Fact]
    public void GetAllPets_ReturnsAllPets()
    {
        using var test = new TestContext();

        test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        test.Service.AddPet(new Pet
        {
            Name = "Мурчик",
            Species = "Кіт",
            Breed = "Сіамський"
        });

        var result = test.Service.GetAllPets();

        Assert.Equal(2, result.Count);
        Assert.Equal("Бобік", result[0].Name);
        Assert.Equal("Мурчик", result[1].Name);
    }

    [Fact]
    public void GetPetById_WhenPetExists_ReturnsPet()
    {
        using var test = new TestContext();

        var addedPet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        var result = test.Service.GetPetById(addedPet.Id);

        Assert.NotNull(result);
        Assert.Equal("Бобік", result.Name);
    }

    [Fact]
    public void GetPetById_WhenPetDoesNotExist_ReturnsNull()
    {
        using var test = new TestContext();

        var result = test.Service.GetPetById(999);

        Assert.Null(result);
    }

    [Fact]
    public void GetPetById_WithInvalidId_ReturnsNull()
    {
        using var test = new TestContext();

        var result = test.Service.GetPetById(0);

        Assert.Null(result);
    }

    [Fact]
    public void AddPet_SavesPetToDatabase()
    {
        using var test = new TestContext();

        var result = test.Service.AddPet(new Pet
        {
            Name = "  Рекс  ",
            Species = "  Собака  ",
            Breed = "  Вівчарка  "
        });

        Assert.True(result.Id > 0);
        Assert.Equal("Рекс", result.Name);
        Assert.Equal("Собака", result.Species);
        Assert.Equal("Вівчарка", result.Breed);

        var savedPet = test.Service.GetPetById(result.Id);

        Assert.NotNull(savedPet);
        Assert.Equal("Рекс", savedPet.Name);
    }

    [Fact]
    public void GetMeasurements_ReturnsMeasurementsInDescendingOrder()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 25,
            Unit = "хв",
            RecordedAt = new DateTime(2026, 10, 26)
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 40,
            Unit = "хв",
            RecordedAt = new DateTime(2026, 10, 27)
        });

        var result = test.Service.GetMeasurements(pet.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(40, result[0].Value);
        Assert.Equal(25, result[1].Value);
    }

    [Fact]
    public void GetMeasurements_WhenPetDoesNotExist_ThrowsException()
    {
        using var test = new TestContext();

        Assert.Throws<ArgumentException>(() =>
            test.Service.GetMeasurements(999));
    }

    [Fact]
    public void GetMeasurementTypes_ReturnsDistinctTypes()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Мурчик",
            Species = "Кіт",
            Breed = "Сіамський"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 25,
            Unit = "хв"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 40,
            Unit = "хв"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Вода",
            Value = 500,
            Unit = "мл"
        });

        var result = test.Service.GetMeasurementTypes(pet.Id);

        Assert.Equal(2, result.Count);
        Assert.Contains("Активність", result);
        Assert.Contains("Вода", result);
    }

    [Fact]
    public void AnalyzeMeasurements_DetectsSignificantDeviation()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 25,
            Unit = "хв",
            RecordedAt = new DateTime(2026, 10, 26)
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 40,
            Unit = "хв",
            RecordedAt = new DateTime(2026, 10, 27)
        });

        var result = test.Service.AnalyzeMeasurements(
            pet.Id,
            "Активність",
            30);

        Assert.Contains("Увага", result);
        Assert.Contains("60", result);
    }

    [Fact]
    public void AnalyzeMeasurements_WithNegativeThreshold_ThrowsException()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            test.Service.AnalyzeMeasurements(
                pet.Id,
                "Активність",
                -1));
    }

    [Fact]
    public void UpdatePet_UpdatesExistingPet()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        var updatedPet = test.Service.UpdatePet(new Pet
        {
            Id = pet.Id,
            Name = "Рекс",
            Species = "Собака",
            Breed = "Вівчарка",
            BirthDate = new DateOnly(2020, 5, 10),
            CurrentWeightKg = 25.5
        });

        Assert.Equal("Рекс", updatedPet.Name);
        Assert.Equal("Собака", updatedPet.Species);
        Assert.Equal("Вівчарка", updatedPet.Breed);
        Assert.Equal(25.5, updatedPet.CurrentWeightKg);

        var savedPet = test.Service.GetPetById(pet.Id);

        Assert.NotNull(savedPet);
        Assert.Equal("Рекс", savedPet.Name);
    }

    [Fact]
    public void UpdatePet_WhenPetDoesNotExist_ThrowsException()
    {
        using var test = new TestContext();

        var pet = new Pet
        {
            Id = 999,
            Name = "Рекс",
            Species = "Собака",
            Breed = "Вівчарка"
        };

        Assert.Throws<ArgumentException>(() =>
            test.Service.UpdatePet(pet));
    }

    [Fact]
    public void DeleteMeasurement_RemovesMeasurement()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        var measurement = test.Service.AddMeasurement(
            new PetMeasurement
            {
                PetId = pet.Id,
                MeasurementType = "Активність",
                Value = 30,
                Unit = "хв"
            });

        test.Service.DeleteMeasurement(measurement.Id);

        var result = test.Service.GetAllMeasurements();

        Assert.Empty(result);
    }

    [Fact]
    public void DeleteMeasurement_WhenMeasurementDoesNotExist_ThrowsException()
    {
        using var test = new TestContext();

        Assert.Throws<ArgumentException>(() =>
            test.Service.DeleteMeasurement(999));
    }

    [Fact]
    public void DeletePet_RemovesPetAndRelatedMeasurements()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Мурчик",
            Species = "Кіт",
            Breed = "Сіамський"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Вага",
            Value = 5,
            Unit = "кг"
        });

        test.Service.AddMeasurement(new PetMeasurement
        {
            PetId = pet.Id,
            MeasurementType = "Активність",
            Value = 40,
            Unit = "хв"
        });

        test.Service.DeletePet(pet.Id);

        Assert.Null(test.Service.GetPetById(pet.Id));
        Assert.Empty(test.Service.GetAllMeasurements());
    }

    [Fact]
    public void AddMeasurement_WithNegativeValue_ThrowsException()
    {
        using var test = new TestContext();

        var pet = test.Service.AddPet(new Pet
        {
            Name = "Бобік",
            Species = "Собака",
            Breed = "Лабрадор"
        });

        Assert.Throws<ArgumentException>(() =>
            test.Service.AddMeasurement(new PetMeasurement
            {
                PetId = pet.Id,
                MeasurementType = "Вага",
                Value = -5,
                Unit = "кг"
            }));
    }

}
