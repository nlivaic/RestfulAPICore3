using System;
using System.ComponentModel;
using System.Linq;
using API.Exceptions;
using API.Extensions;
using API.Models;
using API.ResourceParameters;

namespace API.Helpers
{
    public class OrderingDirectionDtoConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string source = value as string;
            if (source != null)
            {
                var orderByCriteriaWithDirection = source.Split(' ');
                OrderingDirection orderByDirection = OrderingDirection.Asc;     // Default value.
                if (orderByCriteriaWithDirection.Length > 1
                    && !Enum.TryParse(orderByCriteriaWithDirection[1].CapitalizeFirstLetter(),
                    out orderByDirection))
                {
                    throw new InvalidPropertyMappingException($"Unknown ordering direction: {orderByCriteriaWithDirection[1]}");
                }
                return new AuthorsResourceParameters.OrderingCriteriaDto
                {
                    OrderByCriteria = orderByCriteriaWithDirection[0],
                    OrderingDirection = orderByDirection
                };
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}