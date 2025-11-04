using HSEBank.Application.Facades;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Application.Commands
{
    public sealed class AddOperationCommand : ICommand
    {
        private readonly OperationsFacade _facade;
        private readonly AccountId _accId;
        private readonly CategoryId _catId;
        private readonly OperationType _type;
        private readonly Money _amount;
        private readonly DateTime _date;
        private readonly Description _desc;

        public AddOperationCommand(OperationsFacade facade, AccountId accId, CategoryId catId,
            OperationType type, Money amount, DateTime date, Description desc)
        {
            _facade = facade; _accId = accId; _catId = catId; _type = type; _amount = amount; _date = date; _desc = desc;
        }

        public void Execute() => _facade.Add(_accId, _catId, _type, _amount, _date, _desc);
    }
}