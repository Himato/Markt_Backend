using System;

namespace Markt.Helpers.Attributes.Filterable
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FilterableAttribute : Attribute
    {
        public string EntityProperty { get; set; }

        public IFilterExpressionProvider ExpressionProvider { get; set; }
            = new DefaultFilterExpressionProvider();
    }
}
