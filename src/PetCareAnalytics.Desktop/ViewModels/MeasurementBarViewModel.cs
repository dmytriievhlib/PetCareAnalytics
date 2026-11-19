namespace PetCareAnalytics.Desktop.ViewModels;

/// <summary>
/// Дані одного стовпчика аналітичного графіка.
/// </summary>
public class MeasurementBarViewModel
{
    public string PetName { get; set; } = string.Empty;

    public string DateLabel { get; set; } = string.Empty;

    public string ValueLabel { get; set; } = string.Empty;

    public double Percentage { get; set; }
}