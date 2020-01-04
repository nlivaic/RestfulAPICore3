using System.Linq;
using API.Helpers;

namespace API.Services
{
    public interface IPagingService
    {
        PagedList<T> PageList<T>(IQueryable<T> query, int pageNumber, int pageSize);
    }

    public class PagingService : IPagingService
    {
        public PagedList<T> PageList<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {
            var totalItems = query.Count();
            var elements = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return new PagedList<T>(elements, pageNumber, totalItems, pageSize);
        }
    }
}