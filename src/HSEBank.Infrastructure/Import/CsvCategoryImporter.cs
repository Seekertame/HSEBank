using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HSEBank.Domain.Entities;
using HSEBank.Domain.Repositories;
using HSEBank.Domain.ValueObjects;

namespace HSEBank.Infrastructure.Import
{
    public sealed class CsvCategoryImporter : ImporterBase<Category>
    {
        private readonly ICategoryRepository _cats;
        public CsvCategoryImporter(ICategoryRepository cats) => _cats = cats;

        private sealed class Row { public Guid Id { get; set; } public string Type { get; set; } = ""; public string Name { get; set; } = ""; }

        protected override IEnumerable<Category> Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) yield break;
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = ",", TrimOptions = TrimOptions.Trim };
            using var csv = new CsvReader(new StringReader(raw), cfg);
            foreach (var r in csv.GetRecords<Row>())
                yield return new Category(new CategoryId(r.Id), Enum.Parse<CategoryType>(r.Type, true), r.Name);
        }

        protected override void Persist(IEnumerable<Category> items)
        {
            foreach (var cat in items)
            {
                var existing = _cats.Get(cat.Id);
                if (existing is null) _cats.Add(cat); else _cats.Update(cat);
            }
        }
    }
}
