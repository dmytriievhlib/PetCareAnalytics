using PetCareAnalytics.Desktop.Services;
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PetCareAnalytics.Core.Models;
using PetCareAnalytics.Core.Repositories;
using PetCareAnalytics.Core.Services;
using PetCareAnalytics.Core.Storage;

namespace PetCareAnalytics.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PetCareDbContext _dbContext;
    private readonly PetCareApplicationService _service;

    public ObservableCollection<Pet> Pets { get; } = new();
    public ObservableCollection<Pet> FilteredPets { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    // Форма редагування
    [ObservableProperty]
    private bool isEditPetFormVisible;

    [ObservableProperty]
    private string editPetName = string.Empty;

    [ObservableProperty]
    private string editPetSpecies = string.Empty;

    [ObservableProperty]
    private string editPetBreed = string.Empty;

    [ObservableProperty]
    private string editPetBirthDate = string.Empty;

    [ObservableProperty]
    private string editPetWeight = string.Empty;

    // Підтвердження видалення
    [ObservableProperty]
    private bool isDeleteConfirmationVisible;
    public ObservableCollection<PetMeasurement> Measurements { get; } = new();
    // Підтвердження видалення вимірювання
    [ObservableProperty]
    private PetMeasurement? measurementToDelete;

    [ObservableProperty]
    private bool isMeasurementDeleteConfirmationVisible;
    [ObservableProperty]
    private Pet? selectedPet;

    [ObservableProperty]
    private string statusMessage = "Дані завантажено.";
    [ObservableProperty]
    private string currentSection = "Home";

    [ObservableProperty]
    private string pageTitle = "Головна";

    [ObservableProperty]
    private string pageSubtitle =
        "Огляд інформації про домашніх тварин";

    public bool IsHomeSection => CurrentSection == "Home";
    public bool IsPetsSection => CurrentSection == "Pets";
    public bool IsMeasurementsSection => CurrentSection == "Measurements";
    public bool IsAnalyticsSection => CurrentSection == "Analytics";

    partial void OnCurrentSectionChanged(string value)
    {
        OnPropertyChanged(nameof(IsHomeSection));
        OnPropertyChanged(nameof(IsPetsSection));
        OnPropertyChanged(nameof(IsMeasurementsSection));
        OnPropertyChanged(nameof(IsAnalyticsSection));

        switch (value)
        {
            case "Home":
                PageTitle = "Головна";
                PageSubtitle = "Огляд інформації про домашніх тварин";
                break;

            case "Pets":
                PageTitle = "Мої тварини";
                PageSubtitle = "Керування профілями домашніх тварин";
                break;

            case "Measurements":
                PageTitle = "Вимірювання";
                PageSubtitle = "Історія та реєстрація показників";
                break;

            case "Analytics":
                PageTitle = "Аналітика";
                PageSubtitle = "Аналіз і порівняння показників тварин";
                break;
        }
    }

    private void NavigateTo(string section)
    {
        IsAddPetFormVisible = false;
        IsAddMeasurementFormVisible = false;
        IsEditPetFormVisible = false;
        IsDeleteConfirmationVisible = false;

        CurrentSection = section;
    }

    [RelayCommand]
    private void ShowHome()
    {
        NavigateTo("Home");
    }

    [RelayCommand]
    private void ShowPets()
    {
        NavigateTo("Pets");
    }

    [RelayCommand]
    private void ShowMeasurements()
    {
        NavigateTo("Measurements");
    }

    [RelayCommand]
    private void ShowAnalytics()
    {
        NavigateTo("Analytics");
    }
    [ObservableProperty]
    private bool isAddPetFormVisible;

    [ObservableProperty]
    private string newPetName = string.Empty;

    [ObservableProperty]
    private string newPetSpecies = string.Empty;

    [ObservableProperty]
    private string newPetBreed = string.Empty;

    [ObservableProperty]
    private string newPetBirthDate = string.Empty;

    [ObservableProperty]
    private string newPetWeight = string.Empty;

    [ObservableProperty]
    private bool isAddMeasurementFormVisible;

    [ObservableProperty]
    private string newMeasurementType = "Активність";

    [ObservableProperty]
    private string newMeasurementValue = string.Empty;

    [ObservableProperty]
    private string newMeasurementUnit = "хв";

    [ObservableProperty]
    private string newMeasurementDate =
        DateTime.Now.ToString("dd.MM.yyyy");

    [ObservableProperty]
    private string newMeasurementNotes = string.Empty;

    [ObservableProperty]
    private string analyticsStartDate = DateTime.Today
    .AddDays(-30).ToString("dd.MM.yyyy");

    [ObservableProperty]
    private string analyticsEndDate = DateTime.Today
        .ToString("dd.MM.yyyy");

    public bool IsCustomAnalyticsPeriod =>
        SelectedAnalyticsPeriod == "Власний період";

    // Аналітичні дані
    public ObservableCollection<string> MeasurementTypes { get; } = new();
    public ObservableCollection<string> AnalyticsPeriods { get; } = new()
    {
    "Увесь час",
    "Останні 7 днів",
    "Останні 30 днів",
    "Останні 90 днів",
    "Власний період"
    };
    public ObservableCollection<MeasurementBarViewModel> ChartBars { get; } = new();
    /// <summary>
    /// Результати порівняння показників між тваринами.
    /// </summary>
    public ObservableCollection<PetComparisonViewModel> PetComparisons { get; } = new();
    /// <summary>
    /// Список тварин для вибору в порівнянні.
    /// </summary>
    public ObservableCollection<PetComparisonSelectionViewModel>
        PetComparisonSelections
    { get; } = new();
    [ObservableProperty]
    private bool isComparisonMode;
    [ObservableProperty]
    private string selectedAnalyticsPeriod = "Увесь час";
    [ObservableProperty]
    private string selectedAnalyticsType = string.Empty;
    // Режим аналітики для всіх тварин або лише вибраної.
    [ObservableProperty]
    private bool isAllPetsAnalytics;
    [ObservableProperty]
    private string averageValue = "—";

    [ObservableProperty]
    private string minimumValue = "—";

    [ObservableProperty]
    private string maximumValue = "—";

    [ObservableProperty]
    private string analysisMessage = "Оберіть тварину для аналізу.";

    [ObservableProperty]
    private string analyticsUnit = string.Empty;
    [RelayCommand]
    private void ShowAddMeasurementForm()
    {
        if (SelectedPet is null)
        {
            StatusMessage = "Спочатку виберіть тварину.";
            return;
        }
        CurrentSection = "Measurements";
        ClearMeasurementForm();
        IsAddMeasurementFormVisible = true;
        StatusMessage = $"Додавання вимірювання для {SelectedPet.Name}.";
    }

    [RelayCommand]
    private void CancelAddMeasurement()
    {
        RefreshAnalytics();
        IsAddMeasurementFormVisible = false;
        ClearMeasurementForm();
        StatusMessage = "Додавання вимірювання скасовано.";
    }

    [RelayCommand]
    private void AddMeasurement()
    {
        if (SelectedPet is null)
        {
            StatusMessage = "Спочатку виберіть тварину.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewMeasurementType))
        {
            StatusMessage = "Вкажіть тип показника.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewMeasurementUnit))
        {
            StatusMessage = "Вкажіть одиницю вимірювання.";
            return;
        }

        var validValue =
            double.TryParse(
                NewMeasurementValue,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var value)
            ||
            double.TryParse(
                NewMeasurementValue,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out value);

        if (!validValue || !double.IsFinite(value) || value <= 0)
        {
            StatusMessage =
                "Значення має бути числом, більшим за нуль.";
            return;
        }

        if (!DateTime.TryParseExact(
                NewMeasurementDate,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var recordedAt))
        {
            StatusMessage =
                "Введіть дату у форматі ДД.ММ.РРРР.";
            return;
        }
        if (recordedAt.Date > DateTime.Today)
        {
            StatusMessage =
                "Дата вимірювання не може бути в майбутньому.";
            return;
        }
        try
        {
            var measurement = new PetMeasurement
            {
                PetId = SelectedPet.Id,
                RecordedAt = recordedAt,
                MeasurementType = NewMeasurementType.Trim(),
                Value = value,
                Unit = NewMeasurementUnit.Trim(),
                Notes = string.IsNullOrWhiteSpace(NewMeasurementNotes)
                    ? null
                    : NewMeasurementNotes.Trim()
            };

            _service.AddMeasurement(measurement);

            var petName = SelectedPet.Name;

            // Оновлюємо історію вимірювань вибраної тварини.
            Measurements.Clear();

            foreach (var item in _service.GetMeasurements(SelectedPet.Id))
            {
                Measurements.Add(item);
            }

            IsAddMeasurementFormVisible = false;
            ClearMeasurementForm();

            StatusMessage =
                $"Вимірювання для «{petName}» успішно збережено.";
        }
        catch (ArgumentException ex)
        {
            StatusMessage = $"Помилка: {ex.Message}";
        }
        catch (DbUpdateException)
        {
            StatusMessage =
                "Не вдалося зберегти вимірювання в базі даних.";
        }
    }
    private bool TryGetCustomDateRange(
    out DateTime startDate,
    out DateTime endDate)
    {
        startDate = default;
        endDate = default;

        bool validStart = DateTime.TryParseExact(
            AnalyticsStartDate,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out startDate);

        bool validEnd = DateTime.TryParseExact(
            AnalyticsEndDate,
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out endDate);

        if (!validStart || !validEnd)
        {
            AnalysisMessage =
                "Введіть обидві дати у форматі ДД.ММ.РРРР.";

            return false;
        }

        if (startDate.Date > endDate.Date)
        {
            AnalysisMessage =
                "Початкова дата не може бути пізнішою за кінцеву.";

            return false;
        }

        return true;
    }
    private List<PetMeasurement> GetFilteredAnalyticsMeasurements(
    bool allPets = false)
    {
        var measurements = (allPets
            ? _service.GetAllMeasurements()
            : GetAnalyticsSourceMeasurements()).AsEnumerable();
        var today = DateTime.Today;

        DateTime customStartDate = default;
        DateTime customEndDate = default;

        if (SelectedAnalyticsPeriod == "Власний період" &&
            !TryGetCustomDateRange(
                out customStartDate,
                out customEndDate))
        {
            return new List<PetMeasurement>();
        }

        measurements = SelectedAnalyticsPeriod switch
        {
            "Останні 7 днів" =>
                measurements.Where(m =>
                    m.RecordedAt.Date >= today.AddDays(-6) &&
                    m.RecordedAt.Date <= today),

            "Останні 30 днів" =>
                measurements.Where(m =>
                    m.RecordedAt.Date >= today.AddDays(-29) &&
                    m.RecordedAt.Date <= today),

            "Останні 90 днів" =>
                measurements.Where(m =>
                    m.RecordedAt.Date >= today.AddDays(-89) &&
                    m.RecordedAt.Date <= today),

            "Власний період" =>
                measurements.Where(m =>
                    m.RecordedAt.Date >= customStartDate.Date &&
                    m.RecordedAt.Date <= customEndDate.Date),

            _ => measurements
        };

        return measurements
            .OrderBy(m => m.RecordedAt)
            .ToList();
    }
    private void RefreshPetComparisonSelections()
    {
        var existingIds = PetComparisonSelections
            .Select(p => p.PetId)
            .ToHashSet();

        foreach (var pet in Pets)
        {
            if (!existingIds.Contains(pet.Id))
            {
                var selection = new PetComparisonSelectionViewModel
                {
                    PetId = pet.Id,
                    PetName = pet.Name,
                    IsSelected = true
                };

                selection.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName ==
                        nameof(PetComparisonSelectionViewModel.IsSelected))
                    {
                        RefreshPetComparisons();
                    }
                };

                PetComparisonSelections.Add(selection);
            }
        }

        var currentPetIds = Pets
            .Select(p => p.Id)
            .ToHashSet();

        for (int i = PetComparisonSelections.Count - 1; i >= 0; i--)
        {
            if (!currentPetIds.Contains(PetComparisonSelections[i].PetId))
            {
                PetComparisonSelections.RemoveAt(i);
            }
        }
    }
    private void RefreshPetComparisons()
    {
        PetComparisons.Clear();
        RefreshPetComparisonSelections();

        var selectedPetIds = PetComparisonSelections
            .Where(p => p.IsSelected)
            .Select(p => p.PetId)
            .ToHashSet();
        var measurements = GetFilteredAnalyticsMeasurements(allPets: true);

        if (measurements.Count == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedAnalyticsType))
        {
            return;
        }

        var groupedMeasurements = measurements
            .Where(m =>
                string.Equals(
                    m.MeasurementType,
                    SelectedAnalyticsType,
                    StringComparison.OrdinalIgnoreCase)
                && selectedPetIds.Contains(m.PetId))
            .GroupBy(m => m.PetId)
            .ToList();
        if (groupedMeasurements.Count == 0)
        {
            return;
        }

        var comparisonResults = groupedMeasurements
            .Select(group =>
            {
                var pet = Pets.FirstOrDefault(p => p.Id == group.Key);
                var average = group.Average(m => m.Value);

                return new
                {
                    PetName = pet?.Name ?? "Невідома тварина",
                    Average = average,
                    Count = group.Count()
                };
            })
            .OrderByDescending(item => item.Average)
            .ToList();

        var maxAverage = comparisonResults.Max(item => item.Average);

        foreach (var item in comparisonResults)
        {
            var percentage = maxAverage > 0
                ? item.Average / maxAverage * 100
                : 0;

            PetComparisons.Add(new PetComparisonViewModel
            {
                PetName = item.PetName,
                AverageValue = item.Average,
                AverageLabel = item.Average.ToString(
                    "0.##", CultureInfo.CurrentCulture),
                MeasurementCount = item.Count,
                Percentage = percentage
            });
        }
    }

    private static string GetMeasurementWord(int count)
    {
        int lastTwo = count % 100;
        int last = count % 10;

        if (lastTwo >= 11 && lastTwo <= 14)
            return "вимірювань";

        return last switch
        {
            1 => "вимірювання",
            2 or 3 or 4 => "вимірювання",
            _ => "вимірювань"
        };
    }
    private List<PetMeasurement> GetAnalyticsSourceMeasurements()
    {
        if (IsAllPetsAnalytics)
        {
            return _service.GetAllMeasurements();
        }

        return Measurements.ToList();
    }
    private void RefreshAnalytics()
    {
        MeasurementTypes.Clear();
        ChartBars.Clear();

        AverageValue = "—";
        MinimumValue = "—";
        MaximumValue = "—";
        AnalyticsUnit = string.Empty;

        if (SelectedPet is null && !IsAllPetsAnalytics)
        {
            AnalysisMessage = "Оберіть тварину для аналізу.";
            SelectedAnalyticsType = string.Empty;
            return;
        }

        if (SelectedAnalyticsPeriod == "Власний період" &&
            !TryGetCustomDateRange(out _, out _))
        {
            SelectedAnalyticsType = string.Empty;
            AnalysisMessage = "Вкажіть коректний власний період.";
            return;
        }

        var filteredMeasurements = GetFilteredAnalyticsMeasurements();

        if (filteredMeasurements.Count == 0)
        {
            SelectedAnalyticsType = string.Empty;

            AnalysisMessage =
                $"Немає показників за вибраний період «{SelectedAnalyticsPeriod}». " +
                "Спробуйте вибрати довший період.";

            return;
        }

        var types = filteredMeasurements
            .Select(m => m.MeasurementType)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        foreach (var type in types)
            MeasurementTypes.Add(type);

        if (types.Count == 0)
        {
            SelectedAnalyticsType = string.Empty;
            AnalysisMessage = "Немає доступних типів вимірювань.";
            return;
        }

        if (!types.Contains(
            SelectedAnalyticsType,
            StringComparer.OrdinalIgnoreCase))
        {
            SelectedAnalyticsType = types[0];
        }
        else
        {
            UpdateAnalyticsForSelectedType();
        }
    }


    partial void OnSelectedAnalyticsTypeChanged(string value)
    {
        UpdateAnalyticsForSelectedType();
    }
    partial void OnIsAllPetsAnalyticsChanged(bool value)
    {
        RefreshAnalytics();
    }
    partial void OnSelectedAnalyticsPeriodChanged(string value)
    {
        OnPropertyChanged(nameof(IsCustomAnalyticsPeriod));
        RefreshAnalytics();
    }
    partial void OnAnalyticsStartDateChanged(string value)
    {
        RefreshAnalytics();
    }

    partial void OnAnalyticsEndDateChanged(string value)
    {
        RefreshAnalytics();
    }
    private void UpdateAnalyticsForSelectedType()
    {
        ChartBars.Clear();
        RefreshPetComparisons();
        AverageValue = "—";
        MinimumValue = "—";
        MaximumValue = "—";
        AnalyticsUnit = string.Empty;

        if ((SelectedPet is null && !IsAllPetsAnalytics) ||
            string.IsNullOrWhiteSpace(SelectedAnalyticsType))
        {
            return;
        }

        var values = GetFilteredAnalyticsMeasurements()
            .Where(m => string.Equals(
                m.MeasurementType,
                SelectedAnalyticsType,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.RecordedAt)
            .ToList();

        if (values.Count == 0)
        {
            AnalysisMessage =
                "Для цього показника немає вимірювань за вибраний період.";
            return;
        }

        var numbers = values.Select(m => m.Value).ToList();

        AverageValue = numbers.Average().ToString(
            "0.##", CultureInfo.CurrentCulture);

        MinimumValue = numbers.Min().ToString(
            "0.##", CultureInfo.CurrentCulture);

        MaximumValue = numbers.Max().ToString(
            "0.##", CultureInfo.CurrentCulture);

        AnalyticsUnit = values[^1].Unit;

        var max = numbers.Max();
        
        foreach (var item in values.TakeLast(10))
        {
            var percentage = max > 0
                ? item.Value / max * 100
                : 0;
            var petName = IsAllPetsAnalytics
                ? Pets.FirstOrDefault(p => p.Id == item.PetId)?.Name ?? "Невідома тварина"
                : SelectedPet?.Name ?? "Невідома тварина";
            ChartBars.Add(new MeasurementBarViewModel
            {
                PetName = petName,

                DateLabel = item.RecordedAt.ToString(
                    "dd.MM", CultureInfo.CurrentCulture),

                ValueLabel = item.Value.ToString(
                    "0.##", CultureInfo.CurrentCulture),

                Percentage = percentage
            });
        }

        if (IsAllPetsAnalytics)
        {
            AnalysisMessage =
                $"Загальна статистика: показник «{SelectedAnalyticsType}» — " +
                $"{values.Count} {GetMeasurementWord(values.Count)} " +
                $"усіх тварин за період «{SelectedAnalyticsPeriod}».";
        }
        else
        {
            AnalysisMessage =
                $"Показник «{SelectedAnalyticsType}»: {values.Count} " +
                $"{GetMeasurementWord(values.Count)} за період «{SelectedAnalyticsPeriod}».";
        }
    }

    private void ClearMeasurementForm()
    {
        NewMeasurementType = "Активність";
        NewMeasurementValue = string.Empty;
        NewMeasurementUnit = "хв";
        NewMeasurementDate = DateTime.Now.ToString("dd.MM.yyyy");
        NewMeasurementNotes = string.Empty;
    }
    public int PetCount => Pets.Count;
    public void ExportCsv(string filePath)
    {
        try
        {
            var pets = _service.GetAllPets();
            var measurements = _service.GetAllMeasurements();

            var exporter = new DataExportService();

            exporter.ExportCsv(filePath, pets, measurements);

            StatusMessage =
                $"CSV-файл успішно збережено: {filePath}";
        }
        catch (IOException ex)
        {
            StatusMessage = $"Помилка запису CSV-файлу: {ex.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage =
                "Немає дозволу на запис у вибрану папку.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка експорту CSV: {ex.Message}";
        }
    }

    public void ExportExcel(string filePath)
    {
        try
        {
            var pets = _service.GetAllPets();
            var measurements = _service.GetAllMeasurements();

            var exporter = new DataExportService();

            exporter.ExportExcel(filePath, pets, measurements);

            StatusMessage =
                $"Excel-файл успішно збережено: {filePath}";
        }
        catch (IOException ex)
        {
            StatusMessage = $"Помилка запису Excel-файлу: {ex.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage =
                "Немає дозволу на запис у вибрану папку.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Помилка експорту Excel: {ex.Message}";
        }
    }
    public MainViewModel()
    {
        var projectRoot = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", ".."));


        var databasePath = PetCareDatabase.GetDatabasePath();

        var options = new DbContextOptionsBuilder<PetCareDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        _dbContext = new PetCareDbContext(options);

        // Автоматично створюємо базу та таблиці,
        // якщо вони ще не існують.
        _dbContext.Database.EnsureCreated();


        var repository = new PetCareRepository(_dbContext);
        var analysisService = new MeasurementAnalysisService();

        _service = new PetCareApplicationService(
            repository,
            analysisService);

        LoadPets();
    }
    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredPets();
    }

    private void UpdateFilteredPets()
    {
        FilteredPets.Clear();

        var query = SearchText.Trim();

        var result = Pets.Where(p =>
            string.IsNullOrWhiteSpace(query) ||
            p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Species.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Breed.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var pet in result)
        {
            FilteredPets.Add(pet);
        }
    }
    private void LoadPets()
    {
        Pets.Clear();

        foreach (var pet in _service.GetAllPets())
        {
            Pets.Add(pet);
        }

        UpdateFilteredPets();

        StatusMessage = $"Завантажено тварин: {Pets.Count}";

        SelectedPet = Pets.FirstOrDefault();

        OnPropertyChanged(nameof(PetCount));
    }
    // Відкриття підтвердження видалення вимірювання
    [RelayCommand]
    private void RequestDeleteMeasurement(PetMeasurement? measurement)
    {
        if (measurement is null)
            return;

        MeasurementToDelete = measurement;
        IsMeasurementDeleteConfirmationVisible = true;
    }

    // Скасування видалення вимірювання
    [RelayCommand]
    private void CancelDeleteMeasurement()
    {
        MeasurementToDelete = null;
        IsMeasurementDeleteConfirmationVisible = false;

        StatusMessage = "Видалення вимірювання скасовано.";
    }

    // Підтвердження видалення вимірювання
    [RelayCommand]
    private void ConfirmDeleteMeasurement()
    {
        if (MeasurementToDelete is null)
        {
            IsMeasurementDeleteConfirmationVisible = false;
            return;
        }

        var measurement = MeasurementToDelete;

        try
        {
            // Видаляємо запис із бази даних
            _service.DeleteMeasurement(measurement.Id);

            // Видаляємо запис із колекції
            var item = Measurements.FirstOrDefault(
                m => m.Id == measurement.Id);

            if (item is not null)
                Measurements.Remove(item);

            MeasurementToDelete = null;
            IsMeasurementDeleteConfirmationVisible = false;

            // Оновлюємо аналітику
            RefreshAnalytics();

            StatusMessage =
                $"Вимірювання «{measurement.MeasurementType}» успішно видалено.";
        }
        catch (ArgumentException ex)
        {
            MeasurementToDelete = null;
            IsMeasurementDeleteConfirmationVisible = false;

            StatusMessage = $"Помилка: {ex.Message}";
        }
        catch (DbUpdateException)
        {
            StatusMessage =
                "Не вдалося видалити вимірювання з бази даних.";
        }
    }
    partial void OnSelectedPetChanged(Pet? value)
    {
        Measurements.Clear();

        if (value is null)
        {
            RefreshAnalytics();
            return;
        }

        foreach (var measurement in _service.GetMeasurements(value.Id))
        {
            Measurements.Add(measurement);
        }

        StatusMessage =
            $"Профіль: {value.Name}. Вимірювань: {Measurements.Count}";
        RefreshAnalytics();
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadPets();
    }

    [RelayCommand]
    private void ShowAddPetForm()
    {
        CurrentSection = "Pets";
        ClearPetForm();
        IsAddPetFormVisible = true;
        StatusMessage = "Заповніть дані нової тварини.";
    }

    [RelayCommand]
    private void CancelAddPet()
    {
        IsAddPetFormVisible = false;
        ClearPetForm();
        StatusMessage = "Додавання тварини скасовано.";
    }

    [RelayCommand]
    private void AddPet()
    {
        if (string.IsNullOrWhiteSpace(NewPetName))
        {
            StatusMessage = "Помилка: введіть ім'я тварини.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPetSpecies))
        {
            StatusMessage = "Помилка: вкажіть вид тварини.";
            return;
        }

        DateOnly? birthDate = null;

        if (!string.IsNullOrWhiteSpace(NewPetBirthDate))
        {
            if (!DateOnly.TryParse(
                    NewPetBirthDate,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                StatusMessage =
                    "Помилка: дата народження має бути у форматі ДД.ММ.РРРР.";
                return;
            }
            if (parsedDate > DateOnly.FromDateTime(DateTime.Today))
            {
                StatusMessage =
                    "Дата народження не може бути в майбутньому.";
                return;
            }
            birthDate = parsedDate;
        }

        double? weight = null;

        if (!string.IsNullOrWhiteSpace(NewPetWeight))
        {
            var validWeight =
                double.TryParse(
                    NewPetWeight,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out var parsedWeight)
                ||
                double.TryParse(
                    NewPetWeight,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out parsedWeight);

            if (!validWeight ||
                !double.IsFinite(parsedWeight) ||
                parsedWeight <= 0)
            {
                StatusMessage =
                    "Помилка: вага має бути числом, більшим за нуль.";
                return;
            }

            weight = parsedWeight;
        }

        try
        {
            var pet = new Pet
            {
                Name = NewPetName.Trim(),
                Species = NewPetSpecies.Trim(),
                Breed = NewPetBreed.Trim(),
                BirthDate = birthDate,
                CurrentWeightKg = weight
            };

            var addedPet = _service.AddPet(pet);

            LoadPets();

            SelectedPet = Pets.FirstOrDefault(
                p => p.Id == addedPet.Id);

            IsAddPetFormVisible = false;
            ClearPetForm();

            StatusMessage =
                $"Тварину «{addedPet.Name}» успішно додано. ID: {addedPet.Id}";
        }
        catch (ArgumentException ex)
        {
            StatusMessage = $"Помилка: {ex.Message}";
        }
        catch (DbUpdateException)
        {
            StatusMessage =
                "Не вдалося зберегти тварину в базі даних.";
        }
    }
    [RelayCommand]
    private void ShowEditPetForm()
    {
        if (SelectedPet is null)
        {
            StatusMessage = "Спочатку виберіть тварину.";
            return;
        }

        EditPetName = SelectedPet.Name;
        EditPetSpecies = SelectedPet.Species;
        EditPetBreed = SelectedPet.Breed;

        EditPetBirthDate = SelectedPet.BirthDate?.ToString(
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture) ?? string.Empty;

        EditPetWeight = SelectedPet.CurrentWeightKg?.ToString(
            CultureInfo.InvariantCulture) ?? string.Empty;

        IsEditPetFormVisible = true;
        IsAddPetFormVisible = false;

        StatusMessage = $"Редагування профілю: {SelectedPet.Name}.";
    }

    [RelayCommand]
    private void CancelEditPet()
    {
        IsEditPetFormVisible = false;
        StatusMessage = "Редагування скасовано.";
    }

    [RelayCommand]
    private void UpdatePet()
    {
        if (SelectedPet is null)
        {
            StatusMessage = "Спочатку виберіть тварину.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditPetName) ||
            string.IsNullOrWhiteSpace(EditPetSpecies))
        {
            StatusMessage = "Ім'я та вид тварини є обов'язковими.";
            return;
        }

        DateOnly? birthDate = null;

        if (!string.IsNullOrWhiteSpace(EditPetBirthDate))
        {
            if (!DateOnly.TryParseExact(
                    EditPetBirthDate,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                StatusMessage =
                    "Дата народження має бути у форматі ДД.ММ.РРРР.";
                return;
            }
            if (parsedDate > DateOnly.FromDateTime(DateTime.Today))
            {
                StatusMessage =
                    "Дата народження не може бути в майбутньому.";
                return;
            }
            birthDate = parsedDate;
        }

        double? weight = null;

        if (!string.IsNullOrWhiteSpace(EditPetWeight))
        {
            var validWeight =
                double.TryParse(
                    EditPetWeight,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out var parsedWeight)
                ||
                double.TryParse(
                    EditPetWeight,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out parsedWeight);

            if (!validWeight ||
                !double.IsFinite(parsedWeight) ||
                parsedWeight <= 0)
            {
                StatusMessage = "Вага має бути числом, більшим за нуль.";
                return;
            }

            weight = parsedWeight;
        }

        try
        {
            var updatedPet = new Pet
            {
                Id = SelectedPet.Id,
                Name = EditPetName.Trim(),
                Species = EditPetSpecies.Trim(),
                Breed = EditPetBreed.Trim(),
                BirthDate = birthDate,
                CurrentWeightKg = weight
            };

            var savedPet = _service.UpdatePet(updatedPet);

            var index = Pets.ToList().FindIndex(p => p.Id == savedPet.Id);

            if (index >= 0)
            {
                Pets[index] = savedPet;
            }

            SelectedPet = savedPet;

            UpdateFilteredPets();

            IsEditPetFormVisible = false;

            StatusMessage =
                $"Профіль тварини «{savedPet.Name}» успішно оновлено.";
        }
        catch (ArgumentException ex)
        {
            StatusMessage = $"Помилка: {ex.Message}";
        }
        catch (DbUpdateException)
        {
            StatusMessage = "Не вдалося оновити дані в базі.";
        }
    }
    [RelayCommand]
    private void ShowDeleteConfirmation()
    {
        if (SelectedPet is null)
        {
            StatusMessage = "Спочатку виберіть тварину.";
            return;
        }

        IsDeleteConfirmationVisible = true;
    }

    [RelayCommand]
    private void CancelDeletePet()
    {
        IsDeleteConfirmationVisible = false;
        StatusMessage = "Видалення скасовано.";
    }

    [RelayCommand]
    private void ConfirmDeletePet()
    {
        if (SelectedPet is null)
        {
            IsDeleteConfirmationVisible = false;
            return;
        }

        var pet = SelectedPet;

        try
        {
            _service.DeletePet(pet.Id);

            Pets.Remove(pet);
            IsDeleteConfirmationVisible = false;

            UpdateFilteredPets();

            SelectedPet = Pets.FirstOrDefault();

            OnPropertyChanged(nameof(PetCount));

            StatusMessage =
                $"Тварину «{pet.Name}» видалено.";
        }
        catch (ArgumentException ex)
        {
            IsDeleteConfirmationVisible = false;
            StatusMessage = $"Помилка: {ex.Message}";
        }
        catch (DbUpdateException)
        {
            IsDeleteConfirmationVisible = false;
            StatusMessage = "Не вдалося видалити тварину з бази даних.";
        }
    }
    private void ClearPetForm()
    {
        NewPetName = string.Empty;
        NewPetSpecies = string.Empty;
        NewPetBreed = string.Empty;
        NewPetBirthDate = string.Empty;
        NewPetWeight = string.Empty;
    }
}
