using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Helpers
{
    public class ArrayModelBinder : IModelBinder
    {
        public ArrayModelBinder()
        {

        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }
            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
            var modelValue = valueProviderResult.FirstValue;
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            var elementType = bindingContext.ModelMetadata.ModelType.GetGenericArguments()[0];
            if (string.IsNullOrEmpty(modelValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }
            var converter = TypeDescriptor.GetConverter(elementType);
            var convertedValues = modelValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(mv => converter.ConvertFrom(mv.Trim()))
                .ToArray();
            var valuesArray = System.Array.CreateInstance(elementType, convertedValues.Length);
            convertedValues.CopyTo(valuesArray, 0);
            bindingContext.Result = ModelBindingResult.Success(valuesArray);
            return Task.CompletedTask;
        }
    }
}