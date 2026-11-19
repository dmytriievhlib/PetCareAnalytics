
using CommunityToolkit.Mvvm.ComponentModel;

namespace PetCareAnalytics.Desktop.ViewModels;

/// <summary>
/// Модель вибору тварини для порівняння.
/// </summary>
public partial class PetComparisonSelectionViewModel : ObservableObject
{
    public int PetId { get; set; }

    public string PetName { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isSelected = true;
}
