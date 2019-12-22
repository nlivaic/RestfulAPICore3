namespace API.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        private int _maximumPageSize = 10;
        private int _pageSize = 5;

        public string MainCategory { get; set; }
        public string SearchQuery { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = (value > _maximumPageSize) ? _pageSize : value;
            }
        }
    }
}