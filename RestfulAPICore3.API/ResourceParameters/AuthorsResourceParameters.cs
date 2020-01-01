using System;
using System.Collections.Generic;
using System.Linq;
using API.Extensions;
using API.Models;

namespace API.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        private const int _maximumPageSize = 10;
        private int _pageSize = 5;

        public string MainCategory { get; set; }
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "Name";
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = (value > _maximumPageSize) ? _maximumPageSize : value;
            }
        }

        public IEnumerable<ValueTuple<string, OrderingDirection>> OrderByWithDirection() => OrderBy
                .Split(',')
                .Select(s =>
                {
                    var orderByCriteriaWithDirection = s.Split(' ');
                    OrderingDirection orderByDirection = OrderingDirection.Asc;     // Default value.
                    if (orderByCriteriaWithDirection.Length > 1
                        && !Enum.TryParse(orderByCriteriaWithDirection[1].CapitalizeFirstLetter(),
                        out orderByDirection))
                    {
                        throw new ArgumentException($"Unknown ordering direction: {orderByCriteriaWithDirection[1]}");
                    }
                    return (
                        OrderByCriteria: orderByCriteriaWithDirection[0],
                        OrderByDirection: orderByDirection);
                });
    }
}