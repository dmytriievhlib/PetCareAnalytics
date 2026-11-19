
namespace PetCareAnalytics.Core.Models;

/// <summary>
/// Модель домашньої тварини.
/// </summary>
public class Pet
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Species { get; set; } = string.Empty;

    public string Breed { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public double? CurrentWeightKg { get; set; }
}
