namespace API.Models
{
    public class PagingDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotaItems { get; }

        public PagingDto(int currentPage, int totalPages, int totaItems)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
            TotaItems = totaItems;
        }
    }
}