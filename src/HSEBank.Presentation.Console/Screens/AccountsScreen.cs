using HSEBank.Application.Facades;
using HSEBank.Domain.ValueObjects;
using static HSEBank.Presentation.Console.UI.ConsoleUi;

namespace HSEBank.Presentation.Console.Screens;

public sealed class AccountsScreen
{
    private readonly AccountsFacade _facade;
    public AccountsScreen(AccountsFacade facade) => _facade = facade;

    public AccountId? Run(AccountId? current)
    {
        while (true)
        {
            Title("Счета: 1-Список  2-Создать  3-Выбрать текущий  4-Переименовать  5-Удалить  0-Назад");
            var key = System.Console.ReadKey(true).Key;
            if (key == ConsoleKey.D0) return current;

            var list = _facade.GetAll().ToList();

            switch (key)
            {
                case ConsoleKey.D1:
                    if (list.Count == 0) { System.Console.WriteLine("Счета отсутствуют."); break; }
                    PrintIndexed(list, a => $"{a.Name} · {a.Balance}");
                    break;

                case ConsoleKey.D2:
                    var name = Prompt("Название счёта: ");
                    var init = PromptMoney("Начальный баланс: ");
                    var newId = _facade.Create(name, new Money(init)); 
                    if (current is null) current = newId;
                    System.Console.WriteLine("Счёт создан.");
                    break;


                case ConsoleKey.D3:
                    if (list.Count == 0) { System.Console.WriteLine("Нет счётов для выбора."); break; }
                    var ix = PromptIndex(list, "Выберите счёт", a => $"{a.Name} · {a.Balance}");
                    current = list[ix].Id;
                    System.Console.WriteLine("Счёт выбран.");
                    break;

                case ConsoleKey.D4:
                    if (list.Count == 0) { System.Console.WriteLine("Нет счётов для переименования."); break; }
                    var ir = PromptIndex(list, "Переименовать", a => a.Name);
                    var nn = Prompt("Новое имя: ");
                    _facade.Rename(list[ir].Id, nn);
                    System.Console.WriteLine("Имя изменено.");
                    break;

                case ConsoleKey.D5:
                    if (list.Count == 0) { System.Console.WriteLine("Нет счётов для удаления."); break; }
                    var idel = PromptIndex(list, "Удалить", a => a.Name);
                    var delId = list[idel].Id;
                    _facade.Remove(delId);
                    if (current == delId) current = null;
                    System.Console.WriteLine("Счёт удалён.");
                    break;
            }
        }
    }
}
