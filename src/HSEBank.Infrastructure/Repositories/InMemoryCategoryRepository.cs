using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Repositories
{
    public class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly Dictionary<CategoryId, Category> _store = [];

        public void Add(Category category) => _store[category.Id] = category;
        public Category? Get(CategoryId id) => _store.TryGetValue(id, out var v) ? v : null;
        public IEnumerable<Category> GetAll() => _store.Values;
        public void Remove(CategoryId id) => _store.Remove(id);
        public void Update(Category category) => _store[category.Id] = category;
    }
}