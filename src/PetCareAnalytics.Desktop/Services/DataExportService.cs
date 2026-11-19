using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using ClosedXML.Excel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

using PetCareAnalytics.Core.Models;
namespace PetCareAnalytics.Desktop.Services;

public class DataExportService
{
    public void ExportCsv(
        string filePath,
        IReadOnlyList<Pet> pets,
        IReadOnlyList<PetMeasurement> measurements)
    {
        using var writer = new StreamWriter(
            filePath,
            false,
            new UTF8Encoding(true));

        writer.WriteLine(
            "ID;Ім'я;Вид;Порода;Дата народження;Вага (кг)");

        foreach (var pet in pets)
        {
            writer.WriteLine(string.Join(";",
                Csv(pet.Id.ToString(CultureInfo.InvariantCulture)),
                Csv(pet.Name),
                Csv(pet.Species),
                Csv(pet.Breed),
                Csv(pet.BirthDate?.ToString(
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture) ?? ""),
                Csv(pet.CurrentWeightKg?.ToString(
                    CultureInfo.InvariantCulture) ?? "")));
        }

        writer.WriteLine();
        writer.WriteLine(
            "ID вимірювання;ID тварини;Дата;Показник;Значення;Одиниця;Примітки");

        foreach (var item in measurements)
        {
            writer.WriteLine(string.Join(";",
                Csv(item.Id.ToString(CultureInfo.InvariantCulture)),
                Csv(item.PetId.ToString(CultureInfo.InvariantCulture)),
                Csv(item.RecordedAt.ToString(
                    "dd.MM.yyyy HH:mm",
                    CultureInfo.InvariantCulture)),
                Csv(item.MeasurementType),
                Csv(item.Value.ToString(CultureInfo.InvariantCulture)),
                Csv(item.Unit),
                Csv(item.Notes ?? "")));
        }
    }

    public void ExportExcel(
        string filePath,
        IReadOnlyList<Pet> pets,
        IReadOnlyList<PetMeasurement> measurements)
    {
        using var workbook = new XLWorkbook();

        var petsSheet = workbook.Worksheets.Add("Тварини");

        petsSheet.Cell(1, 1).Value = "ID";
        petsSheet.Cell(1, 2).Value = "Ім'я";
        petsSheet.Cell(1, 3).Value = "Вид";
        petsSheet.Cell(1, 4).Value = "Порода";
        petsSheet.Cell(1, 5).Value = "Дата народження";
        petsSheet.Cell(1, 6).Value = "Вага (кг)";

        for (int i = 0; i < pets.Count; i++)
        {
            var pet = pets[i];
            int row = i + 2;

            petsSheet.Cell(row, 1).Value = pet.Id;
            petsSheet.Cell(row, 2).Value = pet.Name;
            petsSheet.Cell(row, 3).Value = pet.Species;
            petsSheet.Cell(row, 4).Value = pet.Breed;

            if (pet.BirthDate.HasValue)
            {
                petsSheet.Cell(row, 5).Value =
                    pet.BirthDate.Value.ToDateTime(TimeOnly.MinValue);

                petsSheet.Cell(row, 5).Style.DateFormat.Format =
                    "dd.mm.yyyy";
            }

            if (pet.CurrentWeightKg.HasValue)
            {
                petsSheet.Cell(row, 6).Value =
                    pet.CurrentWeightKg.Value;
            }
        }

        var measurementsSheet =
            workbook.Worksheets.Add("Вимірювання");

        measurementsSheet.Cell(1, 1).Value = "ID вимірювання";
        measurementsSheet.Cell(1, 2).Value = "ID тварини";
        measurementsSheet.Cell(1, 3).Value = "Дата";
        measurementsSheet.Cell(1, 4).Value = "Показник";
        measurementsSheet.Cell(1, 5).Value = "Значення";
        measurementsSheet.Cell(1, 6).Value = "Одиниця";
        measurementsSheet.Cell(1, 7).Value = "Примітки";

        for (int i = 0; i < measurements.Count; i++)
        {
            var item = measurements[i];
            int row = i + 2;

            measurementsSheet.Cell(row, 1).Value = item.Id;
            measurementsSheet.Cell(row, 2).Value = item.PetId;
            measurementsSheet.Cell(row, 3).Value = item.RecordedAt;

            measurementsSheet.Cell(row, 3).Style.DateFormat.Format =
                "dd.mm.yyyy hh:mm";

            measurementsSheet.Cell(row, 4).Value =
                item.MeasurementType;

            measurementsSheet.Cell(row, 5).Value = item.Value;
            measurementsSheet.Cell(row, 6).Value = item.Unit;
            measurementsSheet.Cell(row, 7).Value = item.Notes ?? "";
        }
        // Аркуш зі статистикою
        var statisticsSheet = workbook.Worksheets.Add("Статистика");

        statisticsSheet.Cell(1, 1).Value = "ID тварини";
        statisticsSheet.Cell(1, 2).Value = "Ім'я тварини";
        statisticsSheet.Cell(1, 3).Value = "Показник";
        statisticsSheet.Cell(1, 4).Value = "Одиниця";
        statisticsSheet.Cell(1, 5).Value = "Кількість вимірювань";
        statisticsSheet.Cell(1, 6).Value = "Середнє значення";
        statisticsSheet.Cell(1, 7).Value = "Мінімум";
        statisticsSheet.Cell(1, 8).Value = "Максимум";

        var groups = measurements
            .GroupBy(m => new
            {
                m.PetId,
                m.MeasurementType,
                m.Unit
            })
            .OrderBy(g => g.Key.PetId)
            .ThenBy(g => g.Key.MeasurementType);

        int statisticsRow = 2;

        foreach (var group in groups)
        {
            var pet = pets.FirstOrDefault(
                p => p.Id == group.Key.PetId);

            statisticsSheet.Cell(statisticsRow, 1).Value =
                group.Key.PetId;

            statisticsSheet.Cell(statisticsRow, 2).Value =
                pet?.Name ?? "Невідома тварина";

            statisticsSheet.Cell(statisticsRow, 3).Value =
                group.Key.MeasurementType;

            statisticsSheet.Cell(statisticsRow, 4).Value =
                group.Key.Unit;

            statisticsSheet.Cell(statisticsRow, 5).Value =
                group.Count();

            statisticsSheet.Cell(statisticsRow, 6).Value =
                group.Average(m => m.Value);

            statisticsSheet.Cell(statisticsRow, 7).Value =
                group.Min(m => m.Value);

            statisticsSheet.Cell(statisticsRow, 8).Value =
                group.Max(m => m.Value);

            statisticsRow++;
        }

        // Форматування всіх аркушів
        FormatSheet(petsSheet);
        FormatSheet(measurementsSheet);
        FormatSheet(statisticsSheet);

        // Додавання діаграм на аркуш статистики
        AddStatisticsCharts(
            statisticsSheet,
            pets,
            measurements,
            statisticsRow + 2);

        workbook.SaveAs(filePath);
    }

    private static void AddStatisticsCharts(
    IXLWorksheet worksheet,
    IReadOnlyList<Pet> pets,
    IReadOnlyList<PetMeasurement> measurements,
    int startRow)
    {
        int currentRow = startRow;

        // Окрема діаграма для кожного типу вимірювання
        var measurementTypes = measurements
            .Select(m => m.MeasurementType)
            .Distinct()
            .OrderBy(type => type);

        foreach (var measurementType in measurementTypes)
        {
            var groups = measurements
                .Where(m => m.MeasurementType == measurementType)
                .GroupBy(m => m.PetId)
                .OrderBy(g => g.Key)
                .ToList();

            if (groups.Count == 0)
                continue;

            var unit = groups.First().First().Unit;

            var model = new PlotModel
            {
                Title = $"{measurementType} ({unit})",
                Background = OxyColors.White
            };

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Title = "Тварина"
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = $"Середнє значення ({unit})",
                MinimumPadding = 0,
                AbsoluteMinimum = 0
            };

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var barSeries = new BarSeries
            {
                Title = "Середнє значення",
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0:0.##}"
            };

            foreach (var group in groups)
            {
                var pet = pets.FirstOrDefault(
                    p => p.Id == group.Key);

                string petName = pet?.Name ?? $"ID {group.Key}";

                categoryAxis.Labels.Add(petName);

                barSeries.Items.Add(new BarItem
                {
                    Value = group.Average(m => m.Value)
                });
            }

            model.Series.Add(barSeries);

            // Створення тимчасового PNG-файлу
            string imagePath = Path.Combine(
                Path.GetTempPath(),
                $"PetCareChart_{Guid.NewGuid():N}.png");

            try
            {
                var exporter = new PngExporter
                {
                    Width = 1000,
                    Height = 450,
                    Dpi = 96
                };

                using (var stream = File.Create(imagePath))
                {
                    exporter.Export(model, stream);
                }

                // Додавання зображення на аркуш Excel
                worksheet.AddPicture(imagePath)
                    .MoveTo(worksheet.Cell(currentRow, 1));

                currentRow += 25;
            }
            finally
            {
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
            }
        }
    }

    private static void FormatSheet(IXLWorksheet sheet)
    {
        var header = sheet.Row(1);

        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#2563EB");

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }

    private static string Csv(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}