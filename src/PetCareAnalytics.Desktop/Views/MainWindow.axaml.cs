using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PetCareAnalytics.Desktop.ViewModels;
using System;
namespace PetCareAnalytics.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ExportCsv_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "«берегти дан≥ у формат≥ CSV",
                SuggestedFileName = "PetCareAnalytics.csv",
                DefaultExtension = "csv",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CSV файл")
                    {
                        Patterns = new[] { "*.csv" }
                    }
                }
            });

        if (file is null)
            return;

        var path = file.Path.LocalPath;

        if (!path.EndsWith(".csv",
                StringComparison.OrdinalIgnoreCase))
        {
            path += ".csv";
        }

        viewModel.ExportCsv(path);
    }

    private async void ExportExcel_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "«берегти дан≥ у формат≥ Excel",
                SuggestedFileName = "PetCareAnalytics.xlsx",
                DefaultExtension = "xlsx",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Excel Workbook")
                    {
                        Patterns = new[] { "*.xlsx" }
                    }
                }
            });

        if (file is null)
            return;

        var path = file.Path.LocalPath;

        if (!path.EndsWith(".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            path += ".xlsx";
        }

        viewModel.ExportExcel(path);
    }
}