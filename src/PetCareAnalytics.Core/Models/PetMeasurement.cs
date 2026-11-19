
using System;

namespace PetCareAnalytics.Core.Models;

/// <summary>
/// Історичний показник стану або активності тварини.
/// </summary>
public class PetMeasurement
{
    public int Id { get; set; }

    public int PetId { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.Now;

    public string MeasurementType { get; set; } = string.Empty;

    public double Value { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
