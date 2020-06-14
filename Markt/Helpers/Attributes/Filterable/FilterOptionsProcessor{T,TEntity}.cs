using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Markt.Helpers.Attributes.Filterable
{
    public class FilterOptionsProcessor<T, TEntity>
    {
        private readonly string[] _values;

        public FilterOptionsProcessor(string[] values)
        {
            _values = values;
        }

        public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
        {
            if (_values == null || _values.Length == 0)
            {
                return null;
            }

            var term = GetTermsFromModel().First();
            if (term == null) return query;

            var modifiedQuery = query;

            var propertyInfo = ExpressionHelper
                .GetPropertyInfo<TEntity>(term.EntityName ?? term.Name);
            var obj = ExpressionHelper.Parameter<TEntity>();
            
            var left = ExpressionHelper.GetPropertyExpression(obj, propertyInfo);

            foreach (var value in _values)
            {
                // "Value"
                var right = term.ExpressionProvider.GetValue(value);

                // x.Property == "Value"
                var comparisonExpression = term.ExpressionProvider
                    .GetComparison(left, term.Operator, right);

                // x => x.Property == "Value"
                var lambdaExpression = ExpressionHelper
                    .GetLambda<TEntity, bool>(obj, comparisonExpression);

                // query = query.Where...
                modifiedQuery = ExpressionHelper.CallWhere(modifiedQuery, lambdaExpression);
            }

            return modifiedQuery;
        }

        private static IEnumerable<FilterTerm> GetTermsFromModel()
            => typeof(T).GetTypeInfo()
            .DeclaredProperties
            .Where(p => p.GetCustomAttributes<FilterableAttribute>().Any())
            .Select(p =>
            {
                var attribute = p.GetCustomAttribute<FilterableAttribute>();
                return new FilterTerm
                {
                    Name = p.Name,
                    EntityName = attribute.EntityProperty,
                    ExpressionProvider = attribute.ExpressionProvider,
                    Operator = DefaultFilterExpressionProvider.EqualsOperator
                };
            });
    }
}
