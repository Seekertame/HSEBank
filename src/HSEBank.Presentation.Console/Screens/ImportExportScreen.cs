using HSEBank.Domain.Repositories;
using HSEBank.Infrastructure.Export;
using HSEBank.Infrastructure.Import;
using static HSEBank.Presentation.Console.UI.ConsoleUi;

namespace HSEBank.Presentation.Console.Screens;

public sealed class ImportExportScreen
{
    private readonly IBankAccountRepository _accs;
    private readonly ICategoryRepository _cats;
    private readonly IOperationRepository _ops;
    private readonly CsvAccountImporter _accCsvIn;
    private readonly CsvCategoryImporter _catCsvIn;
    private readonly CsvOperationImporter _opCsvIn;
    private readonly JsonAllImporter _jsonAllIn;
    private readonly YamlAllImporter _yamlAllIn;


    public ImportExportScreen(IBankAccountRepository accs, ICategoryRepository cats, IOperationRepository ops,
                              CsvAccountImporter accCsvIn, CsvCategoryImporter catCsvIn, CsvOperationImporter opCsvIn,
                              JsonAllImporter jsonAllIn, YamlAllImporter yamlAllIn)
    {
        _accs = accs; _cats = cats; _ops = ops;
        _accCsvIn = accCsvIn; _catCsvIn = catCsvIn; _opCsvIn = opCsvIn;
        _jsonAllIn = jsonAllIn; _yamlAllIn = yamlAllIn;
    }


    public void Run()
    {
        while (true)
        {
            Title("Экспорт/Импорт: 1-Экспорт JSON  2-Экспорт YAML  3-Экспорт CSV (3 файла)  4-Импорт JSON  5-Импорт CSV (Accounts)  6-Импорт CSV (Categories)  7-Импорт CSV (Operations)  8-Импорт YAML  0-Назад");
            var key = System.Console.ReadKey(true).Key;
            if (key == ConsoleKey.D0) return;

            switch (key)
            {
                case ConsoleKey.D1: ExportJsonSnapshot(); break;
                case ConsoleKey.D2: ExportYamlSnapshot(); break;
                case ConsoleKey.D3: ExportCsvTriplet(); break;
                case ConsoleKey.D4: ImportJsonSnapshot(); break;
                case ConsoleKey.D5: ImportCsvAccounts(); break;
                case ConsoleKey.D6: ImportCsvCategories(); break;
                case ConsoleKey.D7: ImportCsvOperations(); break;
                case ConsoleKey.D8: ImportYamlSnapshot(); break;

            }
        }
    }

    private static string DefaultExportDir()
    {
        var dir = Path.Combine(Environment.CurrentDirectory, "exports");
        Directory.CreateDirectory(dir);
        return dir;
    }
    private static string Ts() => DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

    private void ExportJsonSnapshot()
    {
        var v = new JsonAllExportVisitor();
        foreach (var a in _accs.GetAll()) v.Visit(a);
        foreach (var c in _cats.GetAll()) v.Visit(c);
        foreach (var o in _ops.GetAll()) v.Visit(o);

        var path = Path.Combine(DefaultExportDir(), $"snapshot_{Ts()}.json");
        File.WriteAllText(path, v.GetResult());
        System.Console.WriteLine($"Экспорт JSON завершён → {path}");
    }

    private void ExportYamlSnapshot()
    {
        var v = new YamlAllExportVisitor();
        foreach (var a in _accs.GetAll()) v.Visit(a);
        foreach (var c in _cats.GetAll()) v.Visit(c);
        foreach (var o in _ops.GetAll()) v.Visit(o);

        var path = Path.Combine(DefaultExportDir(), $"snapshot_{Ts()}.yaml");
        File.WriteAllText(path, v.GetResult());
        System.Console.WriteLine($"Экспорт YAML завершён → {path}");
    }

    private void ExportCsvTriplet()
    {
        var folder = DefaultExportDir();

        var va = new CsvAccountsExportVisitor();
        foreach (var a in _accs.GetAll()) va.Visit(a);
        File.WriteAllText(Path.Combine(folder, "accounts.csv"), va.GetResult());

        var vc = new CsvCategoriesExportVisitor();
        foreach (var c in _cats.GetAll()) vc.Visit(c);
        File.WriteAllText(Path.Combine(folder, "categories.csv"), vc.GetResult());

        var vo = new CsvOperationExportVisitor(); // или CsvOperationSExportVisitor — по твоему имени класса
        foreach (var o in _ops.GetAll()) vo.Visit(o);
        File.WriteAllText(Path.Combine(folder, "operations.csv"), vo.GetResult());

        System.Console.WriteLine($"Экспорт CSV завершён → {folder}");
    }


    private void ImportJsonSnapshot()
    {
        var path = Prompt("Путь к .json: ");
        var raw = File.ReadAllText(path);
        var (a, c, o) = _jsonAllIn.ImportJsonString(raw);
        System.Console.WriteLine($"Импорт JSON выполнен: счета={a}, категории={c}, операций={o}.");
    }

    private void ImportCsvAccounts()
    {
        var path = Prompt("Путь к accounts.csv: ");
        using var fs = File.OpenRead(path);
        _accCsvIn.Import(fs);
        System.Console.WriteLine("Импорт Accounts CSV завершён.");
    }

    private void ImportCsvCategories()
    {
        var path = Prompt("Путь к categories.csv: ");
        using var fs = File.OpenRead(path);
        _catCsvIn.Import(fs);
        System.Console.WriteLine("Импорт Categories CSV завершён.");
    }

    private void ImportCsvOperations()
    {
        var path = Prompt("Путь к operations.csv: ");
        using var fs = File.OpenRead(path);
        _opCsvIn.Import(fs);
        System.Console.WriteLine("Импорт Operations CSV завершён.");
    }
    private void ImportYamlSnapshot()
    {
        var path = Prompt("Путь к .yaml: ");
        using var fs = File.OpenRead(path);
        _yamlAllIn.Import(fs);
        System.Console.WriteLine("Импорт YAML завершён.");
    }
}
