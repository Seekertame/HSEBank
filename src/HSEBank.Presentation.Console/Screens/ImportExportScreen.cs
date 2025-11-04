using System.Text;
using HSEBank.Application.Facades;
using HSEBank.Domain.ValueObjects;
using HSEBank.Infrastructure.Export;
using HSEBank.Infrastructure.Import;
using static HSEBank.Presentation.Console.UI.ConsoleUi;

namespace HSEBank.Presentation.Console.Screens;

public sealed class ImportExportScreen
{
    private readonly OperationsFacade _ops;
    private readonly IExportVisitor _exporter;
    private readonly CsvOperationImporter _importer;

    public ImportExportScreen(OperationsFacade ops, IExportVisitor exporter, CsvOperationImporter importer)
    { _ops = ops; _exporter = exporter; _importer = importer; }

    public void Run(AccountId? currentAccount)
    {
        Title("Экспорт/Импорт: 1-Экспорт JSON  2-Импорт CSV  0-Назад");
        var key = System.Console.ReadKey(true).Key;
        if (key == ConsoleKey.D0) return;

        if (key == ConsoleKey.D1)
        {
            if (currentAccount is null) { System.Console.WriteLine("Сначала выберите счёт."); return; }
            foreach (var op in _ops.GetByAccount(currentAccount.Value)) _exporter.Visit(op);

            var path = "ops.json";
            if (_exporter is JsonOperationExportVisitor jov)
            {
                using var fs = File.Create(path);
                jov.WriteTo(fs);
                jov.Reset();
            }
            else File.WriteAllText(path, _exporter.GetResult(), Encoding.UTF8);

            System.Console.WriteLine($"Экспортировано в {path}");
        }
        else if (key == ConsoleKey.D2)
        {
            var path = Prompt("Путь к CSV: ");
            if (!File.Exists(path)) { System.Console.WriteLine("Файл не найден."); return; }
            using var fs = File.OpenRead(path);
            _importer.Import(fs);
            System.Console.WriteLine("Импорт завершён.");
        }
    }
}
