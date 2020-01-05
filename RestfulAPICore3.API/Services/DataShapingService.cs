using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using API.Exceptions;

namespace API.Services
{
    public interface IDataShapingService
    {
        IEnumerable<ExpandoObject> ShapeData<T>(IEnumerable<T> data, IEnumerable<string> properties);
        IEnumerable<ExpandoObject> ShapeData<T>(IEnumerable<T> data);
        ExpandoObject ShapeData<T>(T data, IEnumerable<string> properties);
        ExpandoObject ShapeData<T>(T data);
    }

    public class DataShapingService : IDataShapingService
    {
        public IEnumerable<ExpandoObject> ShapeData<T>(IEnumerable<T> data, IEnumerable<string> properties)
        {
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            if (!properties.Any())
            {
                propertyInfos.AddRange(typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public));
            }
            else
            {
                foreach (var propertyName in properties)
                {
                    var propertyInfo =
                        typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                        ?? throw new InvalidPropertyMappingException(
                            $"Property name '{propertyName}' not found on type {typeof(T)}.");

                    propertyInfos.Add(propertyInfo);
                }
            }
            var shapedData = data.Select(
                target =>
                {
                    ExpandoObject shapedTarget = new ExpandoObject();
                    propertyInfos.ForEach(
                        propertyInfo =>
                        {
                            ((IDictionary<string, object>)shapedTarget).Add(propertyInfo.Name, propertyInfo.GetValue(target));
                        }
                    );
                    return shapedTarget;
                }
            );
            return shapedData;
        }

        public IEnumerable<ExpandoObject> ShapeData<T>(IEnumerable<T> data)
            => ShapeData(data, new List<string>());

        public ExpandoObject ShapeData<T>(T data, IEnumerable<string> properties)
        {

            if (properties.Count() == 0)
            {
                return new ExpandoObject();
            }
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            foreach (var propertyName in properties)
            {
                var propertyInfo =
                    typeof(T).GetProperty(propertyName)
                    ?? throw new InvalidPropertyMappingException(
                        $"Property name '{propertyName}' not found on type {typeof(T)}.");

                propertyInfos.Add(propertyInfo);
            }
            ExpandoObject shapedTarget = new ExpandoObject();
            propertyInfos.ForEach(
                propertyInfo =>
                {
                    ((IDictionary<string, object>)shapedTarget).Add(propertyInfo.Name, propertyInfo.GetValue(data));
                }
            );
            return shapedTarget;
        }

        public ExpandoObject ShapeData<T>(T data)
            => ShapeData(data, new List<string>());
    }
}