using System.Text;
using HSEBank.Application.Facades;
using HSEBank.Domain.Factories;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.Services;
using HSEBank.Infrastructure.Export;
using HSEBank.Infrastructure.Import;
using HSEBank.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HSEBank.Presentation.Console;

public class Program
{
    public static void Main(string[] args)
    {
        System.Console.InputEncoding = Encoding.UTF8;
        System.Console.OutputEncoding = Encoding.UTF8;

        var services = new ServiceCollection();

        // Repositories
        services.AddSingleton<InMemoryBankAccountRepository>();
        services.AddSingleton<IBankAccountRepository>(sp =>
            new CachedBankAccountRepository(sp.GetRequiredService<InMemoryBankAccountRepository>()));

        services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
        services.AddSingleton<IOperationRepository, InMemoryOperationRepository>();

        // Domain & Application
        services.AddSingleton<OperationFactory>();
        services.AddSingleton<AnalyticsService>();
        services.AddSingleton<AccountsFacade>();
        services.AddSingleton<CategoriesFacade>();
        services.AddSingleton<OperationsFacade>();
        services.AddSingleton<AnalyticsFacade>();

        // Import / Export
        // Exporters
        services.AddSingleton<IExportVisitor, JsonAllExportVisitor>();
        services.AddSingleton<YamlAllExportVisitor>();
        services.AddSingleton<CsvAccountsExportVisitor>();
        services.AddSingleton<CsvCategoriesExportVisitor>();
        services.AddSingleton<CsvOperationExportVisitor>();

        // Importers
        services.AddSingleton<CsvAccountImporter>();
        services.AddSingleton<CsvCategoryImporter>();
        services.AddSingleton<CsvOperationImporter>();
        services.AddSingleton<JsonAllImporter>();
        services.AddSingleton<YamlAllImporter>();

        // UI
        services.AddSingleton<Shell.AppShell>();
        services.AddSingleton<Screens.AccountsScreen>();
        services.AddSingleton<Screens.CategoriesScreen>();
        services.AddSingleton<Screens.OperationsScreen>();
        services.AddSingleton<Screens.AnalyticsScreen>();
        services.AddSingleton<Screens.ImportExportScreen>();

        // Logging
        services.AddLogging(b =>
        {
            b.AddConsole();
            b.SetMinimumLevel(LogLevel.Information);
        });

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<Shell.AppShell>().Run();
    }
}
