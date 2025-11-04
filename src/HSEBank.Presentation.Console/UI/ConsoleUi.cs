using System.Globalization;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Presentation.Console.UI;

public static class ConsoleUi
{
    public static void Title(string text)
    {
        System.Console.WriteLine();
        System.Console.WriteLine(text);
    }

    public static void PrintIndexed<T>(IReadOnlyList<T> list, Func<T, string> view)
    {
        for (int i = 0; i < list.Count; i++)
            System.Console.WriteLine($"[{i}] {view(list[i])}");
    }

    public static int PromptIndex<T>(IReadOnlyList<T> list, string label, Func<T, string>? view = null)
    {
        view ??= (x => x?.ToString() ?? "");
        PrintIndexed(list, view);
        while (true)
        {
            System.Console.Write($"{label} → номер: ");
            var s = System.Console.ReadLine();
            if (int.TryParse(s, out var ix) && ix >= 0 && ix < list.Count) return ix;
            System.Console.WriteLine("Неверный ввод. Повторите.");
        }
    }

    public static string Prompt(string label, bool allowEmpty = false)
    {
        while (true)
        {
            System.Console.Write(label);
            var s = (System.Console.ReadLine() ?? "").Trim();
            if (allowEmpty || s.Length > 0) return s;
            System.Console.WriteLine("Поле не может быть пустым.");
        }
    }

    public static decimal PromptMoney(string label, bool mustBePositive = false)
    {
        while (true)
        {
            System.Console.Write(label);
            var s = (System.Console.ReadLine() ?? "").Trim();
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out var v) ||
                decimal.TryParse(s.Replace(',', '.').Replace(" ", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out v))
            {
                if (!mustBePositive || v > 0) return v;
            }
            System.Console.WriteLine("Введите корректную сумму" + (mustBePositive ? " (> 0)." : "."));
        }
    }

    public static DateTime? PromptDate(string label)
    {
        System.Console.Write(label);
        var s = (System.Console.ReadLine() ?? "").Trim();
        if (s == "") return null;
        if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var local)) return local;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utc)) return utc;
        System.Console.WriteLine("Не удалось распознать дату. Использую текущее время.");
        return null;
    }

    public static CategoryType PromptCategoryType()
    {
        while (true)
        {
            System.Console.Write("Тип категории (1 — Доход, 2 — Расход): ");
            var k = System.Console.ReadKey(true).Key;
            if (k == ConsoleKey.D1) { System.Console.WriteLine("Доход"); return CategoryType.Income; }
            if (k == ConsoleKey.D2) { System.Console.WriteLine("Расход"); return CategoryType.Expense; }
            System.Console.WriteLine("Нажмите 1 или 2.");
        }
    }

    public static OperationType PromptOperationType()
    {
        while (true)
        {
            System.Console.Write("Тип операции (1 — Доход, 2 — Расход): ");
            var k = System.Console.ReadKey(true).Key;
            if (k == ConsoleKey.D1) { System.Console.WriteLine("Доход"); return OperationType.Income; }
            if (k == ConsoleKey.D2) { System.Console.WriteLine("Расход"); return OperationType.Expense; }
            System.Console.WriteLine("Нажмите 1 или 2.");
        }
    }

    public static string? PromptOptional(string label)
    {
        System.Console.Write(label);
        var s = (System.Console.ReadLine() ?? "").Trim();
        return s == "" ? null : s;
    }

    public static decimal? PromptMoneyOptional(string label)
    {
        System.Console.Write(label);
        var s = (System.Console.ReadLine() ?? "").Trim();
        if (s == "") return null;
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out var v) ||
            decimal.TryParse(s.Replace(',', '.').Replace(" ", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out v))
            return v;
        System.Console.WriteLine("Некорректная сумма — оставляю текущее значение.");
        return null;
    }

    public static DateTime? PromptDateOptional(string label)
    {
        System.Console.Write(label);
        var s = (System.Console.ReadLine() ?? "").Trim();
        if (s == "") return null;
        if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var local)) return local;
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utc)) return utc;
        System.Console.WriteLine("Некорректная дата — оставляю текущее значение.");
        return null;
    }

    public static OperationType PromptOperationTypeOrDefault(OperationType current)
    {
        System.Console.Write($"Тип операции (1 — Доход, 2 — Расход, Enter — {current}): ");
        var s = (System.Console.ReadLine() ?? "").Trim();
        if (s == "") return current;
        return s == "1" ? OperationType.Income : OperationType.Expense;
    }

    public static int PromptIndexWithDefault<T>(IReadOnlyList<T> list, string label, Func<T, string> view, int @default)
    {
        PrintIndexed(list, view);
        System.Console.Write($"{label} → номер (Enter — {@default}): ");
        var s = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return @default;
        if (int.TryParse(s, out var ix) && ix >= 0 && ix < list.Count) return ix;
        System.Console.WriteLine("Неверный ввод — использую значение по умолчанию.");
        return @default;
    }

}
