using System;
using System.Collections.Generic;
using System.Linq;
using API.Entities;
using API.Models;

namespace API.Services
{
    public interface IPropertyMappingService
    {
        PropertyMappingValue GetMapping<TSource, TTarget>(string sourcePropertyName);
        IEnumerable<PropertyMappingValue> GetMappings<TSource, TTarget>(params string[] sourcePropertyNames);
    }

    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly IEnumerable<IPropertyMapping> _propertyMappings =
            new List<IPropertyMapping>
            {
                new PropertyMapping<AuthorDto, Author>()
                    .Add(nameof(AuthorDto.Id), nameof(Author.Id))
                    .Add(nameof(AuthorDto.MainCategory), nameof(Author.MainCategory))
                    .Add(nameof(AuthorDto.Age), true, nameof(Author.DateOfBirth))
                    .Add(nameof(AuthorDto.Name), nameof(Author.LastName), nameof(Author.FirstName))
            };

        public PropertyMappingValue GetMapping<TSource, TTarget>(string sourcePropertyName)
        {
            var propertyMapping = _propertyMappings
                .OfType<PropertyMapping<TSource, TTarget>>()
                .FirstOrDefault()
                ?? throw new ArgumentException($"Unknown property mapping types: {typeof(TSource)}, {typeof(TTarget)}.");
            return propertyMapping.GetMapping(sourcePropertyName);
        }

        public IEnumerable<PropertyMappingValue> GetMappings<TSource, TTarget>(params string[] sourcePropertyNames)
        {
            var propertyMapping = _propertyMappings
                .OfType<PropertyMapping<TSource, TTarget>>()
                .FirstOrDefault()
                ?? throw new ArgumentException($"Unknown property mapping types: {typeof(TSource)}, {typeof(TTarget)}.");
            return propertyMapping.GetMappings(sourcePropertyNames);
        }
    }
}