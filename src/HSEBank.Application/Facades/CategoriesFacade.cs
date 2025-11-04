using HSEBank.Domain.ValueObjects;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.Entities;

namespace HSEBank.Application.Facades
{
    public class CategoriesFacade
    {
        private readonly ICategoryRepository _categories;
        public CategoriesFacade(ICategoryRepository categories)
            => _categories = categories;
        public IEnumerable<Category> GetAll()
            => _categories.GetAll();
        public CategoryId Create(CategoryType type, string name)
        {
            var category = new Category(CategoryId.New(), type, name);
            _categories.Add(category);
            return category.Id;
        }
        public void Rename(CategoryId id, string newName)
        {
            var cat = _categories.Get(id) ?? throw new InvalidOperationException("Category not found");
            cat.Rename(newName);
            _categories.Update(cat);
        }

        public void Remove(CategoryId id) => _categories.Remove(id);
    }
}
