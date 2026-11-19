
using PetCareAnalytics.Core.Models;
using System.Collections.Generic;
using System;

namespace PetCareAnalytics.Core.Services;

public class MeasurementAnalysisService
{
    public string AnalyzeLatest(
        IReadOnlyList<PetMeasurement> measurements,
        string measurementType,
        double thresholdPercent = 30)
    {
        if (thresholdPercent < 0)
            throw new ArgumentOutOfRangeException(
                nameof(thresholdPercent));

        var values = measurements
            .Where(m => string.Equals(
                m.MeasurementType,
                measurementType,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.RecordedAt)
            .ToList();

        if (values.Count < 2)
        {
            return $"Недостатньо даних для аналізу показника «{measurementType}». " +
                   "Потрібно щонайменше два вимірювання.";
        }

        var previousValues = values
            .Take(values.Count - 1)
            .Select(m => m.Value)
            .ToList();

        var latestValue = values[^1].Value;
        var average = previousValues.Average();

        if (average == 0)
        {
            return latestValue == 0
                ? $"Показник «{measurementType}» не змінився."
                : $"Показник «{measurementType}» змінився з нульового базового значення. " +
                  "Потрібна додаткова перевірка.";
        }

        var deviationPercent =
            Math.Abs(latestValue - average) / average * 100;

        if (deviationPercent > thresholdPercent)
        {
            return $"Увага: показник «{measurementType}» відхиляється " +
                   $"від попереднього середнього на {deviationPercent:F1}%. " +
                   "Рекомендується перевірити дані.";
        }

        return $"Показник «{measurementType}» у межах заданого порога. " +
               $"Відхилення: {deviationPercent:F1}%.";
    }
}
