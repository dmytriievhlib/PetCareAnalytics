
using PetCareAnalytics.Core.Models;
using PetCareAnalytics.Core.Repositories;
using System.Collections.Generic;
using System;

namespace PetCareAnalytics.Core.Services;

/// <summary>
/// Координує бізнес-операції із тваринами та їхніми вимірюваннями.
/// </summary>
public class PetCareApplicationService
{
    private readonly PetCareRepository _repository;
    private readonly MeasurementAnalysisService _analysisService;

    public PetCareApplicationService(
        PetCareRepository repository,
        MeasurementAnalysisService analysisService)
    {
        _repository = repository
            ?? throw new ArgumentNullException(nameof(repository));

        _analysisService = analysisService
            ?? throw new ArgumentNullException(nameof(analysisService));
    }

    /// <summary>
    /// Повертає список усіх тварин.
    /// </summary>
    public List<Pet> GetAllPets()
    {
        return _repository.GetAllPets();
    }

    /// <summary>
    /// Знаходить тварину за ідентифікатором.
    /// </summary>
    public Pet? GetPetById(int id)
    {
        if (id <= 0)
            return null;

        return _repository.GetPetById(id);
    }

    /// <summary>
    /// Додає нову тварину.
    /// </summary>
    public Pet AddPet(Pet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);

        return _repository.AddPet(pet);
    }
    /// <summary>
    /// Отримує всі вимірювання з бази даних.
    /// </summary>
    public List<PetMeasurement> GetAllMeasurements()
    {
        return _repository.GetAllMeasurements();
    }
    /// <summary>
    /// Повертає історію вимірювань конкретної тварини.
    /// </summary>
    public List<PetMeasurement> GetMeasurements(int petId)
    {
        EnsurePetExists(petId);

        return _repository.GetMeasurements(petId);
    }

    /// <summary>
    /// Додає вимірювання для наявної тварини.
    /// </summary>
    public PetMeasurement AddMeasurement(PetMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);

        return _repository.AddMeasurement(measurement);
    }
    /// <summary>
    /// Видаляє вимірювання за ідентифікатором.
    /// </summary>
    public void DeleteMeasurement(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Некоректний ID вимірювання.");

        _repository.DeleteMeasurement(id);
    }
    /// <summary>
    /// Повертає типи показників, які вже записані для тварини.
    /// </summary>
    public List<string> GetMeasurementTypes(int petId)
    {
        EnsurePetExists(petId);

        return _repository.GetMeasurements(petId)
            .Select(m => m.MeasurementType)
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(type => type)
            .ToList();
    }

    /// <summary>
    /// Аналізує останнє вимірювання вибраного типу.
    /// </summary>
    public string AnalyzeMeasurements(
        int petId,
        string measurementType,
        double thresholdPercent = 30)
    {
        EnsurePetExists(petId);

        if (string.IsNullOrWhiteSpace(measurementType))
        {
            throw new ArgumentException(
                "Тип показника обов'язковий.",
                nameof(measurementType));
        }

        if (!double.IsFinite(thresholdPercent) || thresholdPercent < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(thresholdPercent),
                "Поріг відхилення має бути скінченним невід'ємним числом.");
        }

        var measurements = _repository.GetMeasurements(petId);

        return _analysisService.AnalyzeLatest(
            measurements,
            measurementType.Trim(),
            thresholdPercent);
    }

    /// <summary>
    /// Оновлює інформацію про тварину.
    /// </summary>
    public Pet UpdatePet(Pet pet)
    {
        ArgumentNullException.ThrowIfNull(pet);

        return _repository.UpdatePet(pet);
    }

    /// <summary>
    /// Видаляє тварину за ідентифікатором.
    /// </summary>
    public void DeletePet(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Некоректний ID тварини.");

        _repository.DeletePet(id);
    }

    private void EnsurePetExists(int petId)
    {
        if (petId <= 0 || _repository.GetPetById(petId) is null)
        {
            throw new ArgumentException(
                $"Тварину з ID {petId} не знайдено.",
                nameof(petId));
        }
    }
}