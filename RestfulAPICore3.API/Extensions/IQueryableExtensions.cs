using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using API.Services;

namespace API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, IEnumerable<PropertyMappingValue> propertyMappings)
        {
            if (!propertyMappings.Any())
            {
                return query;
            }
            var allTargetProperties = propertyMappings
                .SelectMany(mapping => mapping.TargetPropertyNames
                    .Select(targetPropertyName => new { TargetPropertyName = targetPropertyName, mapping.Revert }));
            var orderedQuery = query
                .OrderBy(allTargetProperties
                    .First()
                    .TargetPropertyName +
                    (allTargetProperties
                        .First().Revert ? " descending" : string.Empty));
            foreach (var targetProperty in allTargetProperties.Skip(1))
            {
                orderedQuery = orderedQuery
                    .ThenBy(targetProperty.TargetPropertyName + (targetProperty.Revert ? " descending" : string.Empty));
            }
            return orderedQuery;
        }
    }
}