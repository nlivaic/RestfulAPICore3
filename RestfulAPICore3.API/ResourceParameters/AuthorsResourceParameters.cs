using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using API.Exceptions;
using API.Extensions;
using API.Helpers;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        private const int _maximumPageSize = 10;
        private int _pageSize = 5;

        public string MainCategory { get; set; }
        public string SearchQuery { get; set; }
        [BindProperty(BinderType = typeof(ArrayModelBinder))]
        public IEnumerable<OrderingCriteriaDto> OrderBy { get; set; }
            = new List<OrderingCriteriaDto> { new OrderingCriteriaDto { OrderByCriteria = "Name", OrderingDirection = OrderingDirection.Asc } };
        [BindProperty(BinderType = typeof(ArrayModelBinder))]
        public IEnumerable<string> Fields { get; set; } = new List<string>();
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = (value > _maximumPageSize) ? _maximumPageSize : value;
            }
        }

        [TypeConverter(typeof(OrderingDirectionDtoConverter))]
        public class OrderingCriteriaDto
        {
            public string OrderByCriteria { get; set; }
            public OrderingDirection OrderingDirection { get; set; }

            public override string ToString() => $"{OrderByCriteria} {OrderingDirection.ToString().ToLower()}";
        }
    }
}
