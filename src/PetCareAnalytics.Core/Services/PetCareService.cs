
using PetCareAnalytics.Core.Models;
using System.Collections.Generic;
using System;

namespace PetCareAnalytics.Core.Services;

public class PetCareService
{
    private readonly List<Pet> _pets = [];
    private readonly List<PetMeasurement> _measurements = [];

    private int _nextPetId = 1;
    private int _nextMeasurementId = 1;

    public IReadOnlyList<Pet> GetAllPets()
    {
        return _pets.AsReadOnly();
    }

    public Pet AddPet(
        string name,
        string species,
        string breed,
        DateOnly? birthDate,
        double? currentWeightKg)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("¬каж≥ть ≥м'€ тварини.");

        if (string.IsNullOrWhiteSpace(species))
            throw new ArgumentException("¬каж≥ть вид тварини.");

        if (currentWeightKg is <= 0)
            throw new ArgumentException("¬ага повинна бути б≥льшою за нуль.");

        var pet = new Pet
        {
            Id = _nextPetId++,
            Name = name.Trim(),
            Species = species.Trim(),
            Breed = breed?.Trim() ?? string.Empty,
            BirthDate = birthDate,
            CurrentWeightKg = currentWeightKg
        };

        _pets.Add(pet);
        return pet;
    }

    public Pet? GetPetById(int id)
    {
        return _pets.FirstOrDefault(pet => pet.Id == id);
    }

    public PetMeasurement AddMeasurement(
        int petId,
        string measurementType,
        double value,
        string unit,
        string? notes = null)
    {
        if (GetPetById(petId) is null)
            throw new ArgumentException("“варину з таким ID не знайдено.");

        if (string.IsNullOrWhiteSpace(measurementType))
            throw new ArgumentException("¬каж≥ть тип показника.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("¬каж≥ть одиницю вим≥рюванн€.");

        if (!double.IsFinite(value) || value < 0)
            throw new ArgumentException("«наченн€ маЇ бути нев≥д'Їмним ≥ ск≥нченним.");

        var measurement = new PetMeasurement
        {
            Id = _nextMeasurementId++,
            PetId = petId,
            RecordedAt = DateTime.Now,
            MeasurementType = measurementType.Trim(),
            Value = value,
            Unit = unit.Trim(),
            Notes = notes
        };

        _measurements.Add(measurement);
        return measurement;
    }

    public IReadOnlyList<PetMeasurement> GetMeasurements(int petId)
    {
        return _measurements
            .Where(measurement => measurement.PetId == petId)
            .OrderByDescending(measurement => measurement.RecordedAt)
            .ToList()
            .AsReadOnly();
    }
}
