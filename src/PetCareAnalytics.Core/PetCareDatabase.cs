using System;
using System.IO;

namespace PetCareAnalytics.Core.Storage;

public static class PetCareDatabase
{
    private const string DatabaseFileName = "petcare.db";

    public static string GetDatabasePath()
    {
        // Каталог для зберігання даних користувача.
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "PetCareAnalytics");

        // Створюємо каталог, якщо його ще немає.
        Directory.CreateDirectory(appDataDirectory);

        var databasePath = Path.Combine(
            appDataDirectory,
            DatabaseFileName);

        // Якщо нової бази ще немає, шукаємо стару.
        // Це потрібно, щоб зберегти вже наявні записи.
        if (!File.Exists(databasePath))
        {
            var legacyDatabasePaths = new[]
            {
                // Старе розташування консольного застосунку.
                Path.Combine(
                    AppContext.BaseDirectory,
                    DatabaseFileName),

                // Старе розташування, яке використовував Desktop.
                Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..", "..",
                    "src",
                    "PetCareAnalytics.App",
                    "bin",
                    "Debug",
                    "net10.0",
                    DatabaseFileName))
            };

            foreach (var legacyDatabasePath in legacyDatabasePaths)
            {
                if (File.Exists(legacyDatabasePath))
                {
                    File.Copy(
                        legacyDatabasePath,
                        databasePath);

                    break;
                }
            }
        }

        return databasePath;
    }
}
