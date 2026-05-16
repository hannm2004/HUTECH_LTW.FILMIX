namespace untitled1.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsTVSeries { get; set; } = false;
        public bool IsTrending { get; set; } = false;
    }
}
