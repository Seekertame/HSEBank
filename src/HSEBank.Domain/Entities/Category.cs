using HSEBank.Domain.ValueObjects;

namespace HSEBank.Domain.Entities
{
    public class Category
    {
        public CategoryId Id { get; }
        public CategoryType Type { get; }
        public string Name { get; private set; }

        public Category(CategoryId id, CategoryType type, string name)
        {
            Id = id;
            Type = type;
            Name = string.IsNullOrWhiteSpace(name) ?
                throw new ArgumentException("Name required") : name;
        }
        public void Rename(string newName)
        {
            Name = string.IsNullOrWhiteSpace(newName) ?
                throw new ArgumentException("Name required") : newName;
        }
    }
}