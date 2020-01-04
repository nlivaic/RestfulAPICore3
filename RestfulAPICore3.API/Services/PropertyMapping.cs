using System;
using System.Collections.Generic;
using API.Exceptions;

namespace API.Services
{
    // Marker interface.
    public interface IPropertyMapping
    {
    }

    public class PropertyMapping<TSource, TTarget> : IPropertyMapping
    {
        private Dictionary<string, PropertyMappingValue> _propertyMappingValues
            = new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase);

        public PropertyMapping<TSource, TTarget> Add(string sourcePropertyName, bool revert, params string[] targetPropertyNames)
        {
            _propertyMappingValues.Add(
                sourcePropertyName,
                new PropertyMappingValue(sourcePropertyName, targetPropertyNames, revert)
                );
            return this;
        }
        public PropertyMapping<TSource, TTarget> Add(string sourcePropertyName, params string[] targetPropertyNames)
        {
            return Add(sourcePropertyName, false, targetPropertyNames);
        }

        public PropertyMappingValue GetMapping(string sourcePropertyName)
        {
            PropertyMappingValue propertyMappingValue = null;
            _propertyMappingValues.TryGetValue(sourcePropertyName, out propertyMappingValue);
            return
                propertyMappingValue
                ?? throw new InvalidPropertyMappingException($"Source property name '{sourcePropertyName}' for source type {typeof(TSource)} not mapped to target type {typeof(TTarget)}.");
        }

        public IEnumerable<PropertyMappingValue> GetMappings(params string[] sourcePropertyNames)
        {
            PropertyMappingValue propertyMappingValue = null;
            List<PropertyMappingValue> propertyMappingValues = new List<PropertyMappingValue>();
            Array.ForEach(sourcePropertyNames, sourcePropertyName =>
            {
                _propertyMappingValues.TryGetValue(sourcePropertyName, out propertyMappingValue);
                if (propertyMappingValue == null)
                {
                    throw new InvalidPropertyMappingException($"Source property name '{sourcePropertyName}' for source type {typeof(TSource)} not mapped to target type {typeof(TTarget)}.");
                }
                propertyMappingValues.Add(propertyMappingValue);

            });
            return propertyMappingValues;
        }
    }
}