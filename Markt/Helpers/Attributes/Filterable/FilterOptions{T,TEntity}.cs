using System.Linq;

namespace Markt.Helpers.Attributes.Filterable
{
    public class FilterOptions<T, TEntity>
    {
        private readonly string[] _values;

        public FilterOptions(string[] values)
        {
            _values = values;
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            var processor = new FilterOptionsProcessor<T, TEntity>(_values);
            return processor.Apply(query);
        }
    }
}
