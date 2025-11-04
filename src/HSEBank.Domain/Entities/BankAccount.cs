using HSEBank.Domain.ValueObjects;

namespace HSEBank.Domain.Entities
{
    public class BankAccount
    {
        public AccountId Id { get; }
        public string Name { get; private set; }
        public Money Balance { get; private set; }

        public BankAccount(AccountId id, string name, Money initialBalance)
        {
            Id = id;
            Name = string.IsNullOrWhiteSpace(name) ? 
                throw new ArgumentException("Name required") : name;
            Balance = initialBalance.Value < 0 ? 
                throw new ArgumentException("Initial balance cannot be negative") : initialBalance;
        }

        public void Rename(string newName)
        {
            Name = string.IsNullOrWhiteSpace(newName) ? 
                throw new ArgumentException("Name required") : newName;
        }

        public void Apply(Operation op)
        {
            if (op.AccountId != Id)
            {
                throw new ArgumentException("Operation account ID does not match bank account ID.");
            }

            if (op.Type == OperationType.Income)
            {
                Balance += op.Amount;
            }
            else if (op.Type == OperationType.Expense)
            {
                var newBalance = Balance - op.Amount;
                if (newBalance.Value < 0)
                {
                    throw new InvalidOperationException("Insufficient funds for this expense operation.");
                }
                Balance = newBalance;
            }
            else
            {
                throw new ArgumentException("Invalid operation type.");
            }
        }
        public void RecomputeBalance(IEnumerable<Operation> operations)
        {
            var bal = Money.Zero;
            foreach (var op in operations)
                bal = op.Type == OperationType.Income ? bal + op.Amount : bal - op.Amount;

            Balance = bal;
        }
    }
}