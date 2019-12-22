namespace API.Models
{
    public class PagingDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotaItems { get; }
        public string NextPageUrl { get; set; }
        public string PreviousPageUrl { get; set; }

        public PagingDto(int currentPage, int totalPages, int totaItems, string previousPageUrl, string nextPageUrl)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
            TotaItems = totaItems;
            PreviousPageUrl = previousPageUrl;
            NextPageUrl = nextPageUrl;
        }
    }
}