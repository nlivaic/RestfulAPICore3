using System;
using System.Collections.Generic;

namespace API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalItems { get; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public PagedList(List<T> elements, int currentPage, int totalItems, int pageSize)
        {
            CurrentPage = currentPage;
            TotalItems = totalItems;
            TotalPages = Convert.ToInt32(Math.Ceiling((decimal)totalItems / pageSize));
            PageSize = pageSize;
            HasNextPage = CurrentPage < TotalPages;
            HasPreviousPage = CurrentPage > 1;
            AddRange(elements);
        }
    }
}