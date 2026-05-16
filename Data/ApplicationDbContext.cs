using Microsoft.EntityFrameworkCore;
using untitled1.Models.Entities;

namespace untitled1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Hành Động" },
                new Category { Id = 2, Name = "Kinh Dị" },
                new Category { Id = 3, Name = "Viễn Tưởng" },
                new Category { Id = 4, Name = "Tình Cảm" }
            );

            modelBuilder.Entity<Movie>().HasData(
                new Movie { Id = 1, Title = "Avengers: Endgame", CategoryId = 1, ImageUrl = "/images/movies/1.jpg", Year = 2019, Genre = "Action", Description = "The Avengers assemble once more in order to restore order to the universe.", IsTVSeries = false, IsTrending = true },
                new Movie { Id = 2, Title = "John Wick 4", CategoryId = 1, ImageUrl = "/images/movies/2.jpg", Year = 2023, Genre = "Action", Description = "John Wick uncovers a path to defeating The High Table.", IsTVSeries = false, IsTrending = true },
                new Movie { Id = 3, Title = "The Conjuring", CategoryId = 2, ImageUrl = "/images/movies/8.jpg", Year = 2013, Genre = "Horror", Description = "Paranormal investigators Ed and Lorraine Warren work to help a family terrorized by a dark presence.", IsTVSeries = false, IsTrending = false },
                new Movie { Id = 4, Title = "Interstellar", CategoryId = 3, ImageUrl = "/images/movies/7.jpg", Year = 2014, Genre = "Sci-Fi", Description = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.", IsTVSeries = false, IsTrending = true },
                new Movie { Id = 5, Title = "Inception", CategoryId = 3, ImageUrl = "/images/movies/6.jpg", Year = 2010, Genre = "Sci-Fi", Description = "A thief who steals corporate secrets through the use of dream-sharing technology.", IsTVSeries = false, IsTrending = false },
                new Movie { Id = 6, Title = "La La Land", CategoryId = 4, ImageUrl = "/images/movies/5.jpg", Year = 2016, Genre = "Romance", Description = "While navigating their careers in Los Angeles, a pianist and an actress fall in love.", IsTVSeries = false, IsTrending = false },
                new Movie { Id = 7, Title = "Breaking Bad", CategoryId = 1, ImageUrl = "/images/movies/4.jpg", Year = 2008, Genre = "Drama/Action", Description = "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 8, Title = "Wednesday", CategoryId = 2, ImageUrl = "/images/movies/8.jpg", Year = 2022, Genre = "Horror/Fantasy", Description = "Follows Wednesday Addams' years as a student, when she attempts to master her emerging psychic ability.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 9, Title = "Squid Game", CategoryId = 1, ImageUrl = "/images/movies/9.jpg", Year = 2021, Genre = "Thriller", Description = "Hundreds of cash-strapped players accept a strange invitation to compete in children's games.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 10, Title = "Spider-Man: NWH", CategoryId = 1, ImageUrl = "/images/movies/10.jpg", Year = 2021, Genre = "Action", Description = "With Spider-Man's identity now revealed, Peter asks Doctor Strange for help.", IsTVSeries = false, IsTrending = true },
                new Movie { Id = 11, Title = "Stranger Things", CategoryId = 3, ImageUrl = "/images/movies/1.jpg", Year = 2016, Genre = "Sci-Fi", Description = "When a young boy disappears, his mother, a police chief and his friends must confront terrifying supernatural forces.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 12, Title = "Parasite", CategoryId = 2, ImageUrl = "/images/movies/2.jpg", Year = 2019, Genre = "Thriller", Description = "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan.", IsTVSeries = false, IsTrending = false },
                new Movie { Id = 13, Title = "Dune: Part Two", CategoryId = 3, ImageUrl = "/images/movies/3.jpg", Year = 2024, Genre = "Sci-Fi", Description = "Paul Atreides unites with Chani and the Fremen while on a warpath of revenge against the conspirators who destroyed his family.", IsTVSeries = false, IsTrending = true },
                new Movie { Id = 14, Title = "The Last of Us", CategoryId = 1, ImageUrl = "/images/movies/4.jpg", Year = 2023, Genre = "Drama", Description = "After a global pandemic destroys civilization, a hardened survivor takes charge of a 14-year-old girl.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 15, Title = "Peaky Blinders", CategoryId = 1, ImageUrl = "/images/movies/5.jpg", Year = 2013, Genre = "Crime", Description = "A gangster family epic set in 1900s England, centering on a gang who sew razor blades in the peaks of their caps.", IsTVSeries = true, IsTrending = false },
                new Movie { Id = 16, Title = "Inception", CategoryId = 3, ImageUrl = "/images/movies/6.jpg", Year = 2010, Genre = "Sci-Fi", Description = "A thief who steals corporate secrets through the use of dream-sharing technology.", IsTVSeries = false, IsTrending = false },
                new Movie { Id = 17, Title = "The Witcher", CategoryId = 3, ImageUrl = "/images/movies/7.jpg", Year = 2019, Genre = "Fantasy", Description = "Geralt of Rivia, a solitary monster hunter, struggles to find his place in a world where people often prove more wicked than beasts.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 18, Title = "Money Heist", CategoryId = 1, ImageUrl = "/images/movies/8.jpg", Year = 2017, Genre = "Crime", Description = "An unusual group of robbers attempt to carry out the most perfect heist in Spanish history.", IsTVSeries = true, IsTrending = true },
                new Movie { Id = 19, Title = "Joker", CategoryId = 2, ImageUrl = "/images/movies/9.jpg", Year = 2019, Genre = "Drama", Description = "A mentally troubled comedian is disregarded and mistreated by society.", IsTVSeries = false, IsTrending = true },
                new Movie { Id = 20, Title = "Ozark", CategoryId = 1, ImageUrl = "/images/movies/10.jpg", Year = 2017, Genre = "Crime", Description = "A financial advisor drags his family from Chicago to the Missouri Ozarks.", IsTVSeries = true, IsTrending = false }
            );
        }
    }
}
