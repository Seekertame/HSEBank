using HSEBank.Application.Facades;
using HSEBank.Domain.ValueObjects;
using static HSEBank.Presentation.Console.UI.ConsoleUi;

namespace HSEBank.Presentation.Console.Screens;

public sealed class CategoriesScreen
{
    private readonly CategoriesFacade _facade;
    public CategoriesScreen(CategoriesFacade facade) => _facade = facade;

    public void Run()
    {
        while (true)
        {
            Title("Категории: 1-Список  2-Создать  3-Переименовать  4-Удалить  0-Назад");
            var key = System.Console.ReadKey(true).Key;
            if (key == ConsoleKey.D0) return;

            var list = _facade.GetAll().ToList();

            switch (key)
            {
                case ConsoleKey.D1:
                    if (!list.Any()) { System.Console.WriteLine("Категории отсутствуют."); break; }
                    PrintIndexed(list, c => $"{(c.Type == CategoryType.Income ? "Доход" : "Расход")} · {c.Name}");
                    break;

                case ConsoleKey.D2:
                    var type = PromptCategoryType();
                    var name = Prompt("Название категории: ");
                    _facade.Create(type, name);
                    System.Console.WriteLine("Категория создана.");
                    break;

                case ConsoleKey.D3:
                    if (!list.Any()) { System.Console.WriteLine("Нет категорий для переименования."); break; }
                    var ir = PromptIndex(list, "Переименовать", c => c.Name);
                    var nn = Prompt("Новое имя: ");
                    _facade.Rename(list[ir].Id, nn);
                    System.Console.WriteLine("Имя изменено.");
                    break;

                case ConsoleKey.D4:
                    if (!list.Any()) { System.Console.WriteLine("Нет категорий для удаления."); break; }
                    var idel = PromptIndex(list, "Удалить", c => c.Name);
                    _facade.Remove(list[idel].Id);
                    System.Console.WriteLine("Категория удалена.");
                    break;
            }
        }
    }
}
