
using Microsoft.EntityFrameworkCore;
using PetCareAnalytics.Core.Models;
using PetCareAnalytics.Core.Storage;
using System.Collections.Generic;
using System;

namespace PetCareAnalytics.Core.Repositories;

/// <summary>
/// Репозиторій для роботи з тваринами та їхніми вимірюваннями.
/// </summary>
public class PetCareRepository
{
    private readonly PetCareDbContext _dbContext;

    public PetCareRepository(PetCareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Отримує список усіх тварин.
    /// </summary>
    public List<Pet> GetAllPets()
    {
        return _dbContext.Pets
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToList();
    }

    /// <summary>
    /// Знаходить тварину за ідентифікатором.
    /// </summary>
    public Pet? GetPetById(int id)
    {
        return _dbContext.Pets
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Додає нову тварину до бази даних.
    /// </summary>
    public Pet AddPet(Pet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);

        if (string.IsNullOrWhiteSpace(pet.Name))
            throw new ArgumentException("Ім'я тварини обов'язкове.");

        if (string.IsNullOrWhiteSpace(pet.Species))
            throw new ArgumentException("Вид тварини обов'язковий.");

        if (pet.CurrentWeightKg is double weight
            && (!double.IsFinite(weight) || weight <= 0))
        {
            throw new ArgumentException(
                "Вага повинна бути скінченним числом, більшим за нуль.");
        }

        pet.Name = pet.Name.Trim();
        pet.Species = pet.Species.Trim();
        pet.Breed = pet.Breed?.Trim() ?? string.Empty;

        _dbContext.Pets.Add(pet);
        _dbContext.SaveChanges();

        return pet;
    }

    /// <summary>
    /// Отримує вимірювання конкретної тварини.
    /// </summary>
    public List<PetMeasurement> GetMeasurements(int petId)
    {
        return _dbContext.Measurements
            .AsNoTracking()
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.RecordedAt)
            .ToList();
    }
    /// <summary>
    /// Отримує всі вимірювання всіх тварин.
    /// </summary>
    public List<PetMeasurement> GetAllMeasurements()
    {
        return _dbContext.Measurements
            .AsNoTracking()
            .OrderBy(m => m.PetId)
            .ThenBy(m => m.RecordedAt)
            .ToList();
    }
    /// <summary>
    /// Додає вимірювання до бази даних.
    /// </summary>
    public PetMeasurement AddMeasurement(PetMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);

        if (!_dbContext.Pets.Any(p => p.Id == measurement.PetId))
            throw new ArgumentException("Тварину з таким ID не знайдено.");

        if (string.IsNullOrWhiteSpace(measurement.MeasurementType))
            throw new ArgumentException("Вкажіть тип показника.");

        if (string.IsNullOrWhiteSpace(measurement.Unit))
            throw new ArgumentException("Вкажіть одиницю вимірювання.");

        if (!double.IsFinite(measurement.Value) || measurement.Value < 0)
            throw new ArgumentException(
                "Значення має бути невід'ємним і скінченним числом.");

        measurement.MeasurementType =
            measurement.MeasurementType.Trim();

        measurement.Unit = measurement.Unit.Trim();

        _dbContext.Measurements.Add(measurement);
        _dbContext.SaveChanges();

        return measurement;
    }
    /// <summary>
    /// Видаляє вимірювання за ідентифікатором.
    /// </summary>
    public void DeleteMeasurement(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Некоректний ID вимірювання.");

        var measurement = _dbContext.Measurements
            .FirstOrDefault(m => m.Id == id);

        if (measurement is null)
            throw new ArgumentException("Вимірювання не знайдено.");

        _dbContext.Measurements.Remove(measurement);
        _dbContext.SaveChanges();
    }
    /// <summary>
    /// Оновлює інформацію про тварину.
    /// </summary>
    public Pet UpdatePet(Pet updatedPet)
    {
        ArgumentNullException.ThrowIfNull(updatedPet);

        if (updatedPet.Id <= 0)
            throw new ArgumentException("Некоректний ID тварини.");

        if (string.IsNullOrWhiteSpace(updatedPet.Name))
            throw new ArgumentException("Ім'я тварини обов'язкове.");

        if (string.IsNullOrWhiteSpace(updatedPet.Species))
            throw new ArgumentException("Вид тварини обов'язковий.");

        if (updatedPet.CurrentWeightKg is double weight
            && (!double.IsFinite(weight) || weight <= 0))
        {
            throw new ArgumentException(
                "Вага повинна бути скінченним числом, більшим за нуль.");
        }

        var existingPet = _dbContext.Pets
            .FirstOrDefault(p => p.Id == updatedPet.Id);

        if (existingPet is null)
            throw new ArgumentException("Тварину не знайдено.");

        existingPet.Name = updatedPet.Name.Trim();
        existingPet.Species = updatedPet.Species.Trim();
        existingPet.Breed = updatedPet.Breed?.Trim() ?? string.Empty;
        existingPet.BirthDate = updatedPet.BirthDate;
        existingPet.CurrentWeightKg = updatedPet.CurrentWeightKg;

        _dbContext.SaveChanges();

        return existingPet;
    }

    /// <summary>
    /// Видаляє тварину та пов'язані з нею вимірювання.
    /// </summary>
    public void DeletePet(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Некоректний ID тварини.");

        var pet = _dbContext.Pets
            .FirstOrDefault(p => p.Id == id);

        if (pet is null)
            throw new ArgumentException("Тварину не знайдено.");

        _dbContext.Pets.Remove(pet);
        _dbContext.SaveChanges();
    }
}
