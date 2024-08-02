namespace UserServer.Models
{
    public class PagedResult<T>
    {
        public int First { get; set; }
        public int? Prev { get; set; }
        public int? Next { get; set; }
        public int Last { get; set; }
        public int Pages { get; set; }
        public int Items { get; set; }
        public List<T> Data { get; set; } = [];
    }
}
