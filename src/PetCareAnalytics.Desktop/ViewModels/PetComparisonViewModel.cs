
namespace PetCareAnalytics.Desktop.ViewModels;

/// <summary>
/// Дані для порівняння показників окремої тварини.
/// </summary>
public class PetComparisonViewModel
{
    public string PetName { get; set; } = string.Empty;

    public double AverageValue { get; set; }

    public string AverageLabel { get; set; } = string.Empty;

    public int MeasurementCount { get; set; }

    public double Percentage { get; set; }
}
