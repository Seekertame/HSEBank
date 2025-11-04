using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;
using HSEBank.Presentation.Console.Screens;

namespace HSEBank.Presentation.Console.Shell;

public sealed class AppShell
{
    private readonly AccountsScreen _accounts;
    private readonly CategoriesScreen _categories;
    private readonly OperationsScreen _operations;
    private readonly AnalyticsScreen _analytics;
    private readonly ImportExportScreen _io;
    private readonly IBankAccountRepository _accRepo;

    private AccountId? _current;

    public AppShell(
        AccountsScreen accounts,
        CategoriesScreen categories,
        OperationsScreen operations,
        AnalyticsScreen analytics,
        ImportExportScreen io,
        IBankAccountRepository accRepo)
    {
        _accounts = accounts; _categories = categories; _operations = operations;
        _analytics = analytics; _io = io; _accRepo = accRepo;
    }

    public void Run()
    {
        while (true)
        {
            var title = _current is null ? "— не выбран —" :
                        _accRepo.Get(_current.Value) is { } a ? $"{a.Name} · {a.Balance}" : "— удалён —";
            System.Console.WriteLine();
            System.Console.WriteLine($"Текущий счёт: {title}");
            System.Console.WriteLine("Меню: 1-Счета  2-Категории  3-Операции  4-Аналитика  5-Экспорт/Импорт  0-Выход");

            switch (System.Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1: _current = _accounts.Run(_current); break;
                case ConsoleKey.D2: _categories.Run(); break;
                case ConsoleKey.D3: _operations.Run(_current); break;
                case ConsoleKey.D4: _analytics.Run(_current); break;
                case ConsoleKey.D5: _io.Run(_current); break;
                case ConsoleKey.D0: return;
            }
        }
    }
}
