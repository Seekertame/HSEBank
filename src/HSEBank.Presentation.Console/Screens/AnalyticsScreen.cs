using HSEBank.Application.Facades;
using HSEBank.Domain.ValueObjects;
using static HSEBank.Presentation.Console.UI.ConsoleUi;

namespace HSEBank.Presentation.Console.Screens;

public sealed class AnalyticsScreen(AnalyticsFacade facade, CategoriesFacade cats)
{
    private readonly AnalyticsFacade _facade = facade;
    private readonly CategoriesFacade _cats = cats;

    public void Run(AccountId? currentAccount)
    {
        if (currentAccount is null) { System.Console.WriteLine("Сначала выберите счёт."); return; }

        var days = 30;
        Title($"Аналитика (последние {days} дней)");
        var to = DateTime.UtcNow;
        var from = to.AddDays(-days);

        var delta = _facade.IncomeMinusExpense(currentAccount.Value, from, to);
        System.Console.WriteLine($"Сальдо (доходы - расходы): {delta}");

        var groups = _facade.GroupByCategory(currentAccount.Value, from, to)
            .OrderByDescending(x => Math.Abs(x.total.Value));

        System.Console.WriteLine("По категориям:");
        foreach (var (catId, total) in groups)
        {
            var name = _cats.GetAll().FirstOrDefault(c => c.Id.Equals(catId))?.Name ?? catId.ToString();
            System.Console.WriteLine($"  {name}: {total}");
        }
    }
}
