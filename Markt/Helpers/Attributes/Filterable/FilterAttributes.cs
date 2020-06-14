using System;

namespace Markt.Helpers.Attributes.Filterable
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FilterableDecimalAttribute : FilterableAttribute
    {
        public FilterableDecimalAttribute()
        {
            ExpressionProvider = new DecimalToIntFilterExpressionProvider();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FilterableDateTimeAttribute : FilterableAttribute
    {
        public FilterableDateTimeAttribute()
        {
            ExpressionProvider = new DateTimeFilterExpressionProvider();
        }
    }
}
