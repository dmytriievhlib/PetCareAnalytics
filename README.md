# PetCareAnalytics

**PetCareAnalytics** — настільний застосунок для обліку домашніх тварин, збереження показників їхнього здоров'я та проведення аналітики на основі вимірювань.

Проєкт розроблено мовою C# із використанням платформи .NET 10, Avalonia UI та SQLite. Застосунок має графічний інтерфейс, підтримує збереження даних між запусками, аналіз показників, порівняння тварин і експорт даних.

## Основні можливості

- Додавання, редагування та видалення записів про домашніх тварин.
- Пошук тварин за іменем, видом і породою.
- Перегляд профілю тварини.
- Додавання, перегляд і видалення вимірювань.
- Аналіз показників здоров'я тварин.
- Фільтрація вимірювань за датами та заданим періодом.
- Порівняння показників декількох тварин.
- Перегляд динаміки вимірювань за допомогою графіків.
- Експорт даних у формати CSV та Excel (XLSX).
- Збереження даних у локальній базі SQLite.
- Перевірка коректності введених даних.

## Технології

| Технологія | Призначення |
|---|---|
| C# | Основна мова програмування |
| .NET 10 | Платформа розробки та виконання |
| Avalonia UI 12.1.5 | Створення графічного інтерфейсу |
| SQLite | Локальне зберігання даних |
| Entity Framework Core 10.0.13 | Взаємодія з базою даних |
| CommunityToolkit.Mvvm 8.4.2 | Реалізація архітектури MVVM |
| ClosedXML 0.105.1 | Формування Excel-файлів |
| OxyPlot.SkiaSharp 2.2.0 | Побудова графіків |
| xUnit | Автоматизоване тестування |

## Архітектура проєкту

Проєкт складається з чотирьох основних частин:

- **PetCareAnalytics.Core** — основна логіка застосунку, моделі даних, контекст бази даних, репозиторій і сервіси.
- **PetCareAnalytics.App** — консольний застосунок для роботи з основною логікою.
- **PetCareAnalytics.Desktop** — графічний настільний застосунок на основі Avalonia UI, що містить інтерфейс, ViewModel та сервіс експорту.
- **PetCareAnalytics.Tests** — автоматизовані тести для перевірки роботи сервісів.

### Структура каталогів

```text
PetCareAnalytics/
│
├── src/
│   ├── PetCareAnalytics.Core/
│   │   ├── Models/
│   │   │   ├── Pet.cs
│   │   │   └── PetMeasurement.cs
│   │   ├── Repositories/
│   │   │   └── PetCareRepository.cs
│   │   ├── Services/
│   │   │   ├── MeasurementAnalysisService.cs
│   │   │   ├── PetCareApplicationService.cs
│   │   │   └── PetCareService.cs
│   │   ├── PetCareDatabase.cs
│   │   ├── PetCareDbContext.cs
│   │   └── PetCareAnalytics.Core.csproj
│   │
│   ├── PetCareAnalytics.App/
│   │   ├── Program.cs
│   │   └── PetCareAnalytics.App.csproj
│   │
│   └── PetCareAnalytics.Desktop/
│       ├── Assets/
│       ├── Services/
│       │   └── DataExportService.cs
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── MeasurementBarViewModel.cs
│       │   ├── PetComparisonSelectionViewModel.cs
│       │   ├── PetComparisonViewModel.cs
│       │   └── ViewModelBase.cs
│       ├── Views/
│       │   ├── MainWindow.axaml
│       │   └── MainWindow.axaml.cs
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── Program.cs
│       ├── ViewLocator.cs
│       ├── app.manifest
│       └── PetCareAnalytics.Desktop.csproj
│
├── tests/
│   └── PetCareAnalytics.Tests/
│       ├── MeasurementAnalysisServiceTests.cs
│       ├── PetCareApplicationServiceTests.cs
│       └── PetCareAnalytics.Tests.csproj
│
├── PetCareAnalytics.slnx
├── .gitignore
└── README.md
```

## Вимоги для запуску

Для роботи з вихідним кодом необхідні:

- Windows, Linux або macOS;
- .NET 10 SDK;
- Git для отримання вихідного коду.

## Отримання вихідного коду

Клонуйте репозиторій:

```bash
git clone https://github.com/dmytriievhlib/PetCareAnalytics.git
cd PetCareAnalytics
```

## Збірка та запуск

### 1. Відновлення залежностей

```bash
dotnet restore
```

### 2. Збірка проєкту

```bash
dotnet build
```

### 3. Запуск графічного застосунку

```bash
dotnet run --project src/PetCareAnalytics.Desktop/PetCareAnalytics.Desktop.csproj
```

### 4. Release-збірка

Для створення збірки в конфігурації Release виконайте:

```bash
dotnet build -c Release
```

## Публікація для Windows

Для створення автономної версії застосунку для Windows x64 виконайте:

```powershell
dotnet publish .\src\PetCareAnalytics.Desktop\PetCareAnalytics.Desktop.csproj -c Release -r win-x64 --self-contained true
```

Після завершення публікації виконуваний файл буде розміщений у каталозі:

```text
src\PetCareAnalytics.Desktop\bin\Release\net10.0\win-x64\publish\
```

Основний файл для запуску:

```text
PetCareAnalytics.Desktop.exe
```

Параметр `--self-contained true` дає змогу запускати опублікований застосунок без окремого встановлення .NET Runtime на комп'ютері.

## База даних

Для зберігання інформації використовується SQLite.

Файл бази даних створюється в каталозі:

```text
%LOCALAPPDATA%\PetCareAnalytics\petcare.db
```

У Windows типовий шлях має такий вигляд:

```text
C:\Users\<USER>\AppData\Local\PetCareAnalytics\petcare.db
```

Шлях до бази даних визначається спільним класом `PetCareDatabase`. Завдяки цьому різні частини застосунку використовують єдине розташування локальної бази даних.

## Тестування

Для запуску автоматизованих тестів виконайте:

```bash
dotnet test
```

Тести перевіряють основні сценарії роботи сервісів, зокрема:

- додавання та редагування інформації про тварин;
- видалення тварин і пов'язаних вимірювань;
- роботу з вимірюваннями;
- аналіз показників;
- обробку некоректних даних;
- перевірку ситуацій, коли потрібний запис відсутній.

### Результати перевірки

За результатами виконання автоматизованих тестів:

| Показник | Результат |
|---|---:|
| Загальна кількість тестів | 23 |
| Успішно пройдено | 23 |
| Помилок | 0 |
| Пропущено | 0 |

Додатково перевірено роботу графічного інтерфейсу, основні операції з тваринами та вимірюваннями, аналітику, експорт у CSV і Excel, збереження даних після перезапуску та каскадне видалення пов'язаних записів.

Release-збірка всіх проєктів завершилася успішно. Опубліковану автономну версію для Windows x64 перевірено запуском виконуваного файлу та повторним відкриттям застосунку зі збереженням даних.

## Експорт даних

Застосунок підтримує експорт даних у такі формати:

- **CSV** — для збереження табличних даних і подальшої роботи з ними в інших програмах.
- **Excel (XLSX)** — для формування електронних таблиць, які можна переглядати та редагувати в Microsoft Excel та сумісних програмах.

## Подальший розвиток

Можливі напрями вдосконалення проєкту:

- розширення аналітичних можливостей;
- додавання нових типів вимірювань;
- удосконалення системи звітності;
- розширення автоматизованого тестування;
- додавання резервного копіювання бази даних;
- розширення можливостей порівняння показників тварин.

## Репозиторій

Вихідний код проєкту доступний на GitHub:

https://github.com/dmytriievhlib/PetCareAnalytics

## Ліцензія

Навчальний проєкт. Умови використання та поширення визначаються автором.