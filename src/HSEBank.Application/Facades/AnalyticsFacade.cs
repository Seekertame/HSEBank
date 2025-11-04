using HSEBank.Domain.Services;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Application.Facades
{
    public class AnalyticsFacade
    {
        private readonly AnalyticsService _service;
        public AnalyticsFacade(AnalyticsService service) => _service = service;

        public Money IncomeMinusExpense(AccountId accountId, DateTime from, DateTime to)
            => _service.IncomeMinusExpense(accountId, from, to);

        public IEnumerable<(CategoryId categoryId, Money total)> GroupByCategory(AccountId accountId, DateTime from, DateTime to)
            => _service.GroupByCategory(accountId, from, to);
    }
}