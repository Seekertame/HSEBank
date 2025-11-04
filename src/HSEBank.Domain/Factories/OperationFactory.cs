using HSEBank.Domain.Entities;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Domain.Factories
{
    public class OperationFactory
    {
        public Operation CreateOperation(
            OperationType type,
            AccountId accountId,
            CategoryId categoryId,
            Money amount,
            DateTime date,
            Description description)
        {
            return new Operation(
                OperationId.New(),
                type,
                accountId,
                categoryId,
                amount,
                date,
                description);
        }
    }
}