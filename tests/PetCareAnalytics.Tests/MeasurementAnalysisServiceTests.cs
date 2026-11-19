
using PetCareAnalytics.Core.Models;
using PetCareAnalytics.Core.Services;
using Xunit;

namespace PetCareAnalytics.Tests;

public class MeasurementAnalysisServiceTests
{
    private readonly MeasurementAnalysisService _service = new();

    [Fact]
    public void AnalyzeLatest_WithTwoMeasurements_DetectsDeviation()
    {
        // Arrange
        var measurements = new List<PetMeasurement>
        {
            new()
            {
                MeasurementType = "Активність",
                Value = 25,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 27, 10, 0, 0)
            },
            new()
            {
                MeasurementType = "Активність",
                Value = 40,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 27, 11, 0, 0)
            }
        };

        // Act
        var result = _service.AnalyzeLatest(
            measurements,
            "Активність",
            30);

        // Assert
        Assert.Contains("60,0%", result);
        Assert.Contains("Увага", result);
    }

    [Fact]
    public void AnalyzeLatest_WithOneMeasurement_ReportsInsufficientData()
    {
        var measurements = new List<PetMeasurement>
        {
            new()
            {
                MeasurementType = "Сон",
                Value = 8,
                Unit = "год",
                RecordedAt = new DateTime(2026, 10, 27)
            }
        };

        var result = _service.AnalyzeLatest(
            measurements,
            "Сон");

        Assert.Contains("Недостатньо даних", result);
    }

    [Fact]
    public void AnalyzeLatest_WhenValueIsWithinThreshold_ReportsNormalResult()
    {
        var measurements = new List<PetMeasurement>
        {
            new()
            {
                MeasurementType = "Активність",
                Value = 100,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 26)
            },
            new()
            {
                MeasurementType = "Активність",
                Value = 110,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 27)
            }
        };

        var result = _service.AnalyzeLatest(
            measurements,
            "Активність",
            30);

        Assert.Contains("у межах заданого порога", result);
        Assert.Contains("10,0%", result);
    }

    [Fact]
    public void AnalyzeLatest_WhenPreviousAverageIsZeroAndLatestIsZero_ReportsNoChange()
    {
        var measurements = new List<PetMeasurement>
        {
            new()
            {
                MeasurementType = "Активність",
                Value = 0,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 26)
            },
            new()
            {
                MeasurementType = "Активність",
                Value = 0,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 27)
            }
        };

        var result = _service.AnalyzeLatest(
            measurements,
            "Активність");

        Assert.Contains("не змінився", result);
    }

    [Fact]
    public void AnalyzeLatest_WhenPreviousAverageIsZeroAndLatestIsNonZero_RequestsReview()
    {
        var measurements = new List<PetMeasurement>
        {
            new()
            {
                MeasurementType = "Активність",
                Value = 0,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 26)
            },
            new()
            {
                MeasurementType = "Активність",
                Value = 10,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 27)
            }
        };

        var result = _service.AnalyzeLatest(
            measurements,
            "Активність");

        Assert.Contains("нульового базового значення", result);
    }

    [Fact]
    public void AnalyzeLatest_WithDifferentMeasurementTypes_AnalyzesSelectedType()
    {
        var measurements = new List<PetMeasurement>
        {
            new()
            {
                MeasurementType = "Вода",
                Value = 800,
                Unit = "мл",
                RecordedAt = new DateTime(2026, 10, 26)
            },
            new()
            {
                MeasurementType = "Активність",
                Value = 25,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 26)
            },
            new()
            {
                MeasurementType = "Активність",
                Value = 40,
                Unit = "хв",
                RecordedAt = new DateTime(2026, 10, 27)
            }
        };

        var result = _service.AnalyzeLatest(
            measurements,
            "Активність",
            30);

        Assert.Contains("60,0%", result);
        Assert.Contains("Увага", result);
    }

    [Fact]
    public void AnalyzeLatest_WithNegativeThreshold_ThrowsException()
    {
        var measurements = new List<PetMeasurement>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _service.AnalyzeLatest(
                measurements,
                "Активність",
                -1));
    }
}

