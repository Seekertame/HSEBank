using HSEBank.Domain.ValueObjects;
using HSEBank.Domain.Repositories;

namespace HSEBank.Domain.Services
{
    public class AnalyticsService
    {
        private readonly IOperationRepository _ops;
        public AnalyticsService(IOperationRepository ops) => _ops = ops;
        public Money IncomeMinusExpense(AccountId accountId, DateTime from, DateTime to)
        {
            var list = _ops.GetByAccount(accountId)
                            .Where(o => o.Date >= from && o.Date <= to)
                            .ToList();
            var income = list.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount.Value);
            var expense = list.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount.Value);

            return new Money(income - expense);
        }
        public IEnumerable<(CategoryId categoryId, Money total)> GroupByCategory(AccountId accountId, DateTime from, DateTime to)
            => _ops.GetByAccount(accountId)
           .Where(o => o.Date >= from && o.Date <= to)
           .GroupBy(o => o.CategoryId)
           .Select(g =>
           {
               var sum = g.Sum(o => o.Type == OperationType.Income ? +o.Amount.Value : -o.Amount.Value);
               return (g.Key, new Money(sum));
           });
    }
}