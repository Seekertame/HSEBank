
namespace HSEBank.Infrastructure.Import
{
    public abstract class ImporterBase<T>
    {
        public void Import(Stream input)
        {
            var raw = ReadAll(input);
            var items = Parse(raw).ToList();
            Persist(items);
        }

        protected virtual string ReadAll(Stream s)
        {
            using var sr = new StreamReader(s, System.Text.Encoding.UTF8, leaveOpen: true);
            return sr.ReadToEnd();
        }

        protected abstract IEnumerable<T> Parse(string raw);
        protected abstract void Persist(IEnumerable<T> items);
    }
}