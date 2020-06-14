using Markt.Helpers.Attributes.Searchable;

namespace Markt.Helpers.Attributes.Filterable
{
    public class FilterTerm
    {
        public string Name { get; set; }

        public string EntityName { get; set; }

        public string Operator { get; set; }

        public IFilterExpressionProvider ExpressionProvider { get; set; }
    }
}
