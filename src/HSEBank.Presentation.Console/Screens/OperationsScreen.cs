using HSEBank.Application.Commands;
using HSEBank.Application.Facades;
using HSEBank.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using static HSEBank.Presentation.Console.UI.ConsoleUi;

namespace HSEBank.Presentation.Console.Screens;

public sealed class OperationsScreen
{
    private readonly OperationsFacade _ops;
    private readonly CategoriesFacade _cats;
    private readonly ILogger<TimedCommand> _log;

    public OperationsScreen(OperationsFacade ops, CategoriesFacade cats, ILogger<TimedCommand> log)
    { _ops = ops; _cats = cats; _log = log; }

    public void Run(AccountId? currentAccount)
    {
        if (currentAccount is null) { System.Console.WriteLine("Сначала выберите счёт."); return; }

        while (true)
        {
            Title("Операции: 1-Список  2-Добавить  3-Удалить  4-Редактировать  0-Назад");
            var key = System.Console.ReadKey(true).Key;
            if (key == ConsoleKey.D0) return;

            var list = _ops.GetByAccount(currentAccount.Value).OrderByDescending(o => o.Date).ToList();

            switch (key)
            {
                case ConsoleKey.D1:
                    {
                        if (list.Count == 0) { System.Console.WriteLine("Операций нет."); break; }
                        PrintIndexed(list, o =>
                        {
                            var sign = o.Type == OperationType.Income ? "+" : "-";
                            var name = _cats.GetAll().FirstOrDefault(c => c.Id.Equals(o.CategoryId))?.Name ?? o.CategoryId.ToString();
                            var desc = o.Description.IsEmpty ? "" : $" · {o.Description.Value}";
                            return $"{o.Date:dd.MM.yyyy} · {o.Type} · {sign}{o.Amount} · {name}{desc}";
                        });
                        break;
                    }

                case ConsoleKey.D2:
                    {
                        var type = PromptOperationType();
                        var addCats = _cats.GetAll()
                            .Where(c => (c.Type == CategoryType.Income) == (type == OperationType.Income))
                            .ToList();
                        if (addCats.Count == 0) { System.Console.WriteLine("Нет подходящих категорий."); break; }

                        var ix = PromptIndex(addCats, "Категория", c => c.Name);
                        var amount = PromptMoney("Сумма (> 0): ", mustBePositive: true);
                        var date = PromptDate("Дата (Enter = сейчас): ") ?? DateTime.UtcNow;
                        var desc = new Description(Prompt("Описание (необязательно): ", allowEmpty: true));

                        var cmd = new AddOperationCommand(_ops, currentAccount.Value, addCats[ix].Id, type, new Money(amount), date, desc);
                        new TimedCommand(cmd, _log).Execute();
                        System.Console.WriteLine("Операция добавлена.");
                        break;
                    }

                case ConsoleKey.D3:
                    {
                        if (list.Count == 0) { System.Console.WriteLine("Нет операций для удаления."); break; }
                        var iop = PromptIndex(list, "Удалить операцию", o => $"{o.Date:dd.MM} · {o.Type} · {o.Amount}");
                        _ops.Remove(list[iop].Id);
                        System.Console.WriteLine("Операция удалена.");
                        break;
                    }

                case ConsoleKey.D4:
                    {
                        if (list.Count == 0) { System.Console.WriteLine("Нет операций для редактирования."); break; }
                        var idx = PromptIndex(list, "Выберите операцию", o => $"{o.Date:dd.MM.yyyy} · {o.Type} · {o.Amount}");
                        var old = list[idx];

                        var newType = PromptOperationTypeOrDefault(old.Type);
                        var editCats = _cats.GetAll()
                            .Where(c => (c.Type == CategoryType.Income) == (newType == OperationType.Income))
                            .ToList();
                        if (editCats.Count == 0) { System.Console.WriteLine("Нет подходящих категорий."); break; }

                        var icat = PromptIndexWithDefault(editCats, "Категория (Enter — оставить текущую)",
                                                               view: c => c.Name,
                                                               @default: editCats.FindIndex(c => c.Id.Equals(old.CategoryId)));
                        var newAmount = PromptMoneyOptional($"Сумма (текущая {old.Amount}): ");
                        var newDate = PromptDateOptional($"Дата (текущая {old.Date:dd.MM.yyyy HH:mm}, Enter — оставить): ");
                        var newDesc = PromptOptional("Описание (Enter — оставить): ");

                        _ops.Edit(
                            old.Id,
                            newType,
                            editCats[icat].Id,
                            new Money(newAmount ?? old.Amount.Value),
                            newDate ?? old.Date,
                            new Description(string.IsNullOrWhiteSpace(newDesc) ? old.Description.Value : newDesc));

                        System.Console.WriteLine("Операция обновлена.");
                        break;
                    }
            }
        }
    }
}
