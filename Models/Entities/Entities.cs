using untitled1.Models.Entities;

namespace untitled1.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation property for many-to-many
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Genre { get; set; } = string.Empty; // Keeps textual genre overview for reference
        public bool IsTVSeries { get; set; }
        public string Director { get; set; } = string.Empty;
        public string Cast { get; set; } = string.Empty;

        /// <summary>Short description / synopsis shown on Detail page and Hover Preview popup.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>YouTube embed URL for trailer (e.g. https://www.youtube.com/embed/XXXX).
        /// Optional – hover preview falls back to a placeholder if empty.</summary>
        public string TrailerUrl { get; set; } = string.Empty;

        // Many-to-many relationship with Categories via join table
        public ICollection<MovieCategory> MovieCategories { get; set; } = new List<MovieCategory>();

        // One-to-many relationship with Episodes
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();

        // One-to-many relationship with MovieImages
        public ICollection<MovieImage> MovieImages { get; set; } = new List<MovieImage>();
    }

    // Join Entity for Many-to-Many Movie <-> Category
    public class MovieCategory
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }

    // Episode Entity for One-to-Many Movie <-> Episode
    public class Episode
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string VideoUrl { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }

    // MovieImage Entity for One-to-Many Movie <-> MovieImage
    public class MovieImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;
    }

    // ──────────────────────────────────────────
    // SUBSCRIPTION / PAYMENT ENTITIES
    // ──────────────────────────────────────────

    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;          // e.g. "Cơ Bản"
        public string Tagline { get; set; } = string.Empty;       // short subtitle
        public decimal Price { get; set; }                        // VND / month
        public string Resolution { get; set; } = string.Empty;   // "480p", "1080p", "4K"
        public int MaxScreens { get; set; }                       // simultaneous screens
        public bool IsPopular { get; set; }                       // highlight badge
        public bool HasDownload { get; set; }                     // download feature
        public bool HasSpatialAudio { get; set; }                 // spatial audio
        public string AccentColor { get; set; } = "#e50914";     // card accent color

        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    }

    public class UserSubscription
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public int PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // "credit_card","momo","banking","zalopay"
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
