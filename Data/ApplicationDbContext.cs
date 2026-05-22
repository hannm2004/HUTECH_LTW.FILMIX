using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using untitled1.Models.Entities;

namespace untitled1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieCategory> MovieCategories { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<MovieImage> MovieImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Many-to-Many Movie <-> Category via MovieCategory
            modelBuilder.Entity<MovieCategory>()
                .HasKey(mc => new { mc.MovieId, mc.CategoryId });

            modelBuilder.Entity<MovieCategory>()
                .HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCategories)
                .HasForeignKey(mc => mc.MovieId);

            modelBuilder.Entity<MovieCategory>()
                .HasOne(mc => mc.Category)
                .WithMany(c => c.MovieCategories)
                .HasForeignKey(mc => mc.CategoryId);

            // Configure One-to-Many Movie <-> Episode
            modelBuilder.Entity<Episode>()
                .HasOne(e => e.Movie)
                .WithMany(m => m.Episodes)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure One-to-Many Movie <-> MovieImage
            modelBuilder.Entity<MovieImage>()
                .HasOne(mi => mi.Movie)
                .WithMany(m => m.MovieImages)
                .HasForeignKey(mi => mi.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seeding Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Hành Động" },
                new Category { Id = 2, Name = "Kinh Dị" },
                new Category { Id = 3, Name = "Viễn Tưởng" },
                new Category { Id = 4, Name = "Tình Cảm" },
                new Category { Id = 5, Name = "Kịch Tính" }
            );

            // Seeding Movies (removed direct CategoryId column)
            modelBuilder.Entity<Movie>().HasData(
                new Movie { Id = 1, Title = "Breaking Bad", ImageUrl = "/images/movies/1.jpg", Year = 2008, Genre = "Crime/Drama", IsTVSeries = true },
                new Movie { Id = 2, Title = "Game of Thrones", ImageUrl = "/images/movies/2.jpg", Year = 2011, Genre = "Action/Fantasy", IsTVSeries = true },
                new Movie { Id = 3, Title = "Oppenheimer", ImageUrl = "/images/movies/3.jpg", Year = 2023, Genre = "Drama/History", IsTVSeries = false },
                new Movie { Id = 4, Title = "Avengers: Infinity War", ImageUrl = "/images/movies/4.jpg", Year = 2018, Genre = "Action/Sci-Fi", IsTVSeries = false },
                new Movie { Id = 5, Title = "Fight Club", ImageUrl = "/images/movies/5.jpg", Year = 1999, Genre = "Drama/Thriller", IsTVSeries = false },
                new Movie { Id = 6, Title = "The Dark Knight", ImageUrl = "/images/movies/6.jpg", Year = 2008, Genre = "Action/Drama", IsTVSeries = false },
                new Movie { Id = 7, Title = "Interstellar", ImageUrl = "/images/movies/7.jpg", Year = 2014, Genre = "Sci-Fi/Drama", IsTVSeries = false },
                new Movie { Id = 8, Title = "Wednesday", ImageUrl = "/images/movies/8.jpg", Year = 2022, Genre = "Horror/Fantasy", IsTVSeries = true },
                new Movie { Id = 9, Title = "Squid Game", ImageUrl = "/images/movies/9.jpg", Year = 2021, Genre = "Action/Thriller", IsTVSeries = true },
                new Movie { Id = 10, Title = "Spider-Man: No Way Home", ImageUrl = "/images/movies/10.jpg", Year = 2021, Genre = "Action/Sci-Fi", IsTVSeries = false }
            );

            // Seeding Many-to-Many connections (MovieCategory)
            modelBuilder.Entity<MovieCategory>().HasData(
                // Breaking Bad (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 1, CategoryId = 1 },
                new MovieCategory { MovieId = 1, CategoryId = 5 },
                // Game of Thrones (Hành Động + Viễn Tưởng)
                new MovieCategory { MovieId = 2, CategoryId = 1 },
                new MovieCategory { MovieId = 2, CategoryId = 3 },
                // Oppenheimer (Kịch Tính)
                new MovieCategory { MovieId = 3, CategoryId = 5 },
                // Avengers: Infinity War (Hành Động + Viễn Tưởng)
                new MovieCategory { MovieId = 4, CategoryId = 1 },
                new MovieCategory { MovieId = 4, CategoryId = 3 },
                // Fight Club (Kịch Tính)
                new MovieCategory { MovieId = 5, CategoryId = 5 },
                // The Dark Knight (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 6, CategoryId = 1 },
                new MovieCategory { MovieId = 6, CategoryId = 5 },
                // Interstellar (Viễn Tưởng + Kịch Tính)
                new MovieCategory { MovieId = 7, CategoryId = 3 },
                new MovieCategory { MovieId = 7, CategoryId = 5 },
                // Wednesday (Kinh Dị + Viễn Tưởng)
                new MovieCategory { MovieId = 8, CategoryId = 2 },
                new MovieCategory { MovieId = 8, CategoryId = 3 },
                // Squid Game (Hành Động)
                new MovieCategory { MovieId = 9, CategoryId = 1 },
                // Spider-Man: NWH (Hành Động + Viễn Tưởng)
                new MovieCategory { MovieId = 10, CategoryId = 1 },
                new MovieCategory { MovieId = 10, CategoryId = 3 }
            );

            // Seeding Episodes (TV Series)
            modelBuilder.Entity<Episode>().HasData(
                // Breaking Bad - Season 1
                new Episode { Id = 1, MovieId = 1, SeasonNumber = 1, EpisodeNumber = 1, Title = "Pilot (Tập Mở Đầu)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 2, MovieId = 1, SeasonNumber = 1, EpisodeNumber = 2, Title = "Cat's in the Bag... (Chiếc túi bí mật)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 3, MovieId = 1, SeasonNumber = 1, EpisodeNumber = 3, Title = "...And the Bag's in the River (Bí ẩn trôi sông)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 4, MovieId = 1, SeasonNumber = 2, EpisodeNumber = 1, Title = "Seven Thirty-Seven (Chuyến bay 737)", VideoUrl = "/videos/sample.mp4" },

                // Game of Thrones - Season 1
                new Episode { Id = 5, MovieId = 2, SeasonNumber = 1, EpisodeNumber = 1, Title = "Winter Is Coming (Mùa đông đang đến)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 6, MovieId = 2, SeasonNumber = 1, EpisodeNumber = 2, Title = "The Kingsroad (Con đường hoàng gia)", VideoUrl = "/videos/sample.mp4" },

                // Wednesday - Season 1
                new Episode { Id = 7, MovieId = 8, SeasonNumber = 1, EpisodeNumber = 1, Title = "Wednesday's Child Is Full of Woe (Đứa trẻ ngày Thứ Tư)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 8, MovieId = 8, SeasonNumber = 1, EpisodeNumber = 2, Title = "Woe Is the Loneliest Number (Cô độc nhất)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 9, MovieId = 8, SeasonNumber = 1, EpisodeNumber = 3, Title = "Friend or Woe (Bạn hay thù)", VideoUrl = "/videos/sample.mp4" },

                // Squid Game - Season 1
                new Episode { Id = 10, MovieId = 9, SeasonNumber = 1, EpisodeNumber = 1, Title = "Red Light, Green Light (Đèn đỏ, đèn xanh)", VideoUrl = "/videos/sample.mp4" },
                new Episode { Id = 11, MovieId = 9, SeasonNumber = 1, EpisodeNumber = 2, Title = "Hell (Địa ngục)", VideoUrl = "/videos/sample.mp4" }
            );

            // Seeding MovieImages (stills gallery)
            modelBuilder.Entity<MovieImage>().HasData(
                // Breaking Bad (Id = 1)
                new MovieImage { Id = 1, MovieId = 1, ImageUrl = "/images/movies/still_action.png" },
                new MovieImage { Id = 2, MovieId = 1, ImageUrl = "/images/movies/1.jpg" },
                new MovieImage { Id = 3, MovieId = 1, ImageUrl = "/images/movies/2.jpg" },

                // Game of Thrones (Id = 2)
                new MovieImage { Id = 4, MovieId = 2, ImageUrl = "/images/movies/still_scifi.png" },
                new MovieImage { Id = 5, MovieId = 2, ImageUrl = "/images/movies/2.jpg" },
                new MovieImage { Id = 6, MovieId = 2, ImageUrl = "/images/movies/3.jpg" },

                // Oppenheimer (Id = 3)
                new MovieImage { Id = 7, MovieId = 3, ImageUrl = "/images/movies/3.jpg" },
                new MovieImage { Id = 8, MovieId = 3, ImageUrl = "/images/movies/4.jpg" },
                new MovieImage { Id = 9, MovieId = 3, ImageUrl = "/images/movies/5.jpg" },

                // Avengers: Infinity War (Id = 4)
                new MovieImage { Id = 10, MovieId = 4, ImageUrl = "/images/movies/still_scifi.png" },
                new MovieImage { Id = 11, MovieId = 4, ImageUrl = "/images/movies/still_action.png" },
                new MovieImage { Id = 12, MovieId = 4, ImageUrl = "/images/movies/4.jpg" },

                // Fight Club (Id = 5)
                new MovieImage { Id = 13, MovieId = 5, ImageUrl = "/images/movies/5.jpg" },
                new MovieImage { Id = 14, MovieId = 5, ImageUrl = "/images/movies/6.jpg" },
                new MovieImage { Id = 15, MovieId = 5, ImageUrl = "/images/movies/7.jpg" },

                // The Dark Knight (Id = 6)
                new MovieImage { Id = 16, MovieId = 6, ImageUrl = "/images/movies/still_action.png" },
                new MovieImage { Id = 17, MovieId = 6, ImageUrl = "/images/movies/6.jpg" },
                new MovieImage { Id = 18, MovieId = 6, ImageUrl = "/images/movies/7.jpg" },

                // Interstellar (Id = 7)
                new MovieImage { Id = 19, MovieId = 7, ImageUrl = "/images/movies/still_scifi.png" },
                new MovieImage { Id = 20, MovieId = 7, ImageUrl = "/images/movies/7.jpg" },
                new MovieImage { Id = 21, MovieId = 7, ImageUrl = "/images/movies/8.jpg" },

                // Wednesday (Id = 8)
                new MovieImage { Id = 22, MovieId = 8, ImageUrl = "/images/movies/8.jpg" },
                new MovieImage { Id = 23, MovieId = 8, ImageUrl = "/images/movies/9.jpg" },
                new MovieImage { Id = 24, MovieId = 8, ImageUrl = "/images/movies/10.jpg" },

                // Squid Game (Id = 9)
                new MovieImage { Id = 25, MovieId = 9, ImageUrl = "/images/movies/9.jpg" },
                new MovieImage { Id = 26, MovieId = 9, ImageUrl = "/images/movies/10.jpg" },
                new MovieImage { Id = 27, MovieId = 9, ImageUrl = "/images/movies/1.jpg" },

                // Spider-Man: No Way Home (Id = 10)
                new MovieImage { Id = 28, MovieId = 10, ImageUrl = "/images/movies/still_scifi.png" },
                new MovieImage { Id = 29, MovieId = 10, ImageUrl = "/images/movies/10.jpg" },
                new MovieImage { Id = 30, MovieId = 10, ImageUrl = "/images/movies/1.jpg" }
            );
        }
    }
}
