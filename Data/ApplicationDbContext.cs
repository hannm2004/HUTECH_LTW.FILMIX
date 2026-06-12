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
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ViewingHistory> ViewingHistories { get; set; }
        public DbSet<WatchlistItem> WatchlistItems { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

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

            // Configure UserSubscription relationships
            modelBuilder.Entity<UserSubscription>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSubscription>()
                .HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Plan)
                .WithMany()
                .HasForeignKey(oi => oi.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ViewingHistory relationships
            modelBuilder.Entity<ViewingHistory>()
                .HasOne(vh => vh.User)
                .WithMany()
                .HasForeignKey(vh => vh.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewingHistory>()
                .HasOne(vh => vh.Movie)
                .WithMany()
                .HasForeignKey(vh => vh.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure WatchlistItem relationships + unique (UserId, MovieId)
            modelBuilder.Entity<WatchlistItem>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WatchlistItem>()
                .HasOne(w => w.Movie)
                .WithMany()
                .HasForeignKey(w => w.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WatchlistItem>()
                .HasIndex(w => new { w.UserId, w.MovieId })
                .IsUnique();

            // Seed SubscriptionPlans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan
                {
                    Id = 1, Name = "Cơ Bản",
                    Tagline = "Xem phim chất lượng tốt",
                    Price = 79000, Resolution = "480p SD",
                    MaxScreens = 1, IsPopular = false,
                    HasDownload = false, HasSpatialAudio = false,
                    AccentColor = "#6b7280"
                },
                new SubscriptionPlan
                {
                    Id = 2, Name = "Tiêu Chuẩn",
                    Tagline = "Full HD & Âm thanh không gian",
                    Price = 149000, Resolution = "1080p Full HD",
                    MaxScreens = 2, IsPopular = true,
                    HasDownload = true, HasSpatialAudio = true,
                    AccentColor = "#e50914"
                },
                new SubscriptionPlan
                {
                    Id = 3, Name = "Cao Cấp",
                    Tagline = "4K Ultra HD + HDR + Dolby Atmos",
                    Price = 219000, Resolution = "4K Ultra HD + HDR",
                    MaxScreens = 4, IsPopular = false,
                    HasDownload = true, HasSpatialAudio = true,
                    AccentColor = "#f59e0b"
                }
            );

            // Seeding Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Hành Động" },
                new Category { Id = 2, Name = "Kinh Dị" },
                new Category { Id = 3, Name = "Viễn Tưởng" },
                new Category { Id = 4, Name = "Tình Cảm" },
                new Category { Id = 5, Name = "Kịch Tính" },
                new Category { Id = 6, Name = "Hoạt Hình" },
                new Category { Id = 7, Name = "Phiêu Lưu" }
            );

            // Seeding Movies with complete fields (Title, Year, Genre, Director, Cast, Description, TrailerUrl, TrailerVideoUrl, Rating)
            modelBuilder.Entity<Movie>().HasData(
                new Movie 
                { 
                    Id = 1, 
                    Title = "Breaking Bad", 
                    ImageUrl = "/images/posters/breakingbad.jpg", 
                    Year = 2008, 
                    Genre = "Kịch tính / Hình sự", 
                    IsTVSeries = true,
                    Director = "Vince Gilligan",
                    Cast = "Bryan Cranston, Aaron Paul, Anna Gunn",
                    Description = "Một giáo viên hóa học cấp ba bị chẩn đoán mắc bệnh ung thư phổi giai đoạn cuối quyết định bắt tay với một cựu học sinh để sản xuất và bán ma túy đá chất lượng cao nhằm bảo đảm tài chính cho gia đình.",
                    TrailerUrl = "https://www.youtube.com/embed/HhesaQXLuRY",
                    TrailerVideoUrl = "/videos/trailers/breaking_bad.mp4",
                    Rating = 9.5
                },
                new Movie 
                { 
                    Id = 2, 
                    Title = "Game of Thrones", 
                    ImageUrl = "/images/posters/gameofthrones.jpg", 
                    Year = 2011, 
                    Genre = "Hành động / Kỳ ảo / Phiêu lưu", 
                    IsTVSeries = true,
                    Director = "David Benioff",
                    Cast = "Emilia Clarke, Kit Harington, Peter Dinklage",
                    Description = "Chín gia tộc quý tộc chiến đấu để giành quyền kiểm soát vùng đất giả tưởng Westeros, trong khi một kẻ thù cổ xưa đang thức tỉnh sau hàng thiên niên kỷ ngủ yên dưới bức tường băng tuyết phía Bắc.",
                    TrailerUrl = "https://www.youtube.com/embed/KPLYYLDtMJ0",
                    TrailerVideoUrl = null,
                    Rating = 9.2
                },
                new Movie 
                { 
                    Id = 3, 
                    Title = "Oppenheimer", 
                    ImageUrl = "/images/posters/oppenheimer.jpg", 
                    Year = 2023, 
                    Genre = "Kịch tính / Tiểu sử / Lịch sử", 
                    IsTVSeries = false,
                    Director = "Christopher Nolan",
                    Cast = "Cillian Murphy, Emily Blunt, Matt Damon",
                    Description = "Câu chuyện về nhà vật lý lý thuyết J. Robert Oppenheimer, người lãnh đạo Dự án Manhattan chế tạo ra quả bom nguyên tử đầu tiên cho nhân loại, mở ra thời đại hạt nhân và những dằn vặt đạo đức khôn nguôi.",
                    TrailerUrl = "https://www.youtube.com/embed/uYPbbksJxIg",
                    TrailerVideoUrl = null,
                    Rating = 8.4
                },
                new Movie 
                { 
                    Id = 4, 
                    Title = "Avengers: Infinity War", 
                    ImageUrl = "/images/posters/avengers.jpg", 
                    Year = 2018, 
                    Genre = "Hành động / Viễn tưởng / Phiêu lưu", 
                    IsTVSeries = false,
                    Director = "Anthony Russo, Joe Russo",
                    Cast = "Robert Downey Jr., Chris Hemsworth, Mark Ruffalo",
                    Description = "Biệt đội siêu anh hùng Avengers và các đồng minh phải sẵn sàng hy sinh tất cả để cố gắng đánh bại Thanos quyền năng trước khi hắn hủy diệt một nửa vũ trụ bằng sáu Viên đá Vô cực.",
                    TrailerUrl = "https://www.youtube.com/embed/6ZfuNTqbHE8",
                    TrailerVideoUrl = null,
                    Rating = 8.4
                },
                new Movie 
                { 
                    Id = 5, 
                    Title = "Fight Club", 
                    ImageUrl = "/images/posters/fightclub.jpg", 
                    Year = 1999, 
                    Genre = "Kịch tính / Giật gân", 
                    IsTVSeries = false,
                    Director = "David Fincher",
                    Cast = "Brad Pitt, Edward Norton, Helena Bonham Carter",
                    Description = "Một nhân viên văn phòng mất ngủ triền miên và một người bán xà phòng lập dị thành lập một câu lạc bộ đánh lộn ngầm đầy bạo lực, dần biến tướng thành một tổ chức vô chính phủ chống lại chủ nghĩa tiêu dùng.",
                    TrailerUrl = "https://www.youtube.com/embed/qtR39562a1g",
                    TrailerVideoUrl = null,
                    Rating = 8.8
                },
                new Movie 
                { 
                    Id = 6, 
                    Title = "The Dark Knight", 
                    ImageUrl = "/images/posters/thedarkknight.jpg", 
                    Year = 2008, 
                    Genre = "Hành động / Kịch tính / Tội phạm", 
                    IsTVSeries = false,
                    Director = "Christopher Nolan",
                    Cast = "Christian Bale, Heath Ledger, Aaron Eckhart",
                    Description = "Khi mối đe dọa mang tên Joker xuất hiện tàn phá thành phố Gotham, Người Dơi phải chấp nhận những thử thách tâm lý và thể xác tột cùng để duy trì ranh giới mong manh giữa công lý và thù hận.",
                    TrailerUrl = "https://www.youtube.com/embed/LDG9bisJEaI",
                    TrailerVideoUrl = null,
                    Rating = 9.0
                },
                new Movie 
                { 
                    Id = 7, 
                    Title = "Interstellar", 
                    ImageUrl = "/images/posters/interstellar.jpg", 
                    Year = 2014, 
                    Genre = "Viễn tưởng / Phiêu lưu / Kịch tính", 
                    IsTVSeries = false,
                    Director = "Christopher Nolan",
                    Cast = "Matthew McConaughey, Anne Hathaway, Jessica Chastain",
                    Description = "Trong tương lai khi Trái Đất sắp bị hủy diệt bởi nạn đói, một nhóm các nhà du hành vũ trụ dấn thân vào chuyến đi xuyên hố đen vũ trụ để tìm kiếm một mái nhà mới cho nhân loại.",
                    TrailerUrl = "https://www.youtube.com/embed/zSWdZATo3cA",
                    TrailerVideoUrl = "/videos/trailers/interstellar.mp4",
                    Rating = 8.7
                },
                new Movie 
                { 
                    Id = 8, 
                    Title = "Wednesday", 
                    ImageUrl = "/images/posters/wednesday.jpg", 
                    Year = 2022, 
                    Genre = "Kinh dị / Kỳ ảo / Kịch tính", 
                    IsTVSeries = true,
                    Director = "Tim Burton",
                    Cast = "Jenna Ortega, Gwendoline Christie, Riki Lindhome",
                    Description = "Wednesday Addams theo học tại Học viện Nevermore, nơi cô cố gắng làm chủ năng lượng ngoại cảm của mình, phá giải một vụ án giết người hàng loạt kinh hoàng và khám phá bí ẩn gia đình 25 năm trước.",
                    TrailerUrl = "https://www.youtube.com/embed/Di310WS8zLk",
                    TrailerVideoUrl = null,
                    Rating = 8.1
                },
                new Movie 
                { 
                    Id = 9, 
                    Title = "Squid Game", 
                    ImageUrl = "/images/posters/squidgame.jpg", 
                    Year = 2021, 
                    Genre = "Hành động / Giật gân / Kịch tính", 
                    IsTVSeries = true,
                    Director = "Hwang Dong-hyuk",
                    Cast = "Lee Jung-jae, Park Hae-soo, Wi Ha-jun",
                    Description = "Hàng trăm người chơi nợ nần chồng chất chấp nhận lời mời kỳ lạ tham gia các trò chơi dân gian dành cho trẻ em với giải thưởng hấp dẫn, nhưng thất bại đồng nghĩa với cái chết ngay lập tức.",
                    TrailerUrl = "https://www.youtube.com/embed/oqxAJKy0R4A",
                    TrailerVideoUrl = null,
                    Rating = 8.0
                },
                new Movie 
                { 
                    Id = 10, 
                    Title = "Spider-Man: No Way Home", 
                    ImageUrl = "/images/posters/spiderman-no-way-home.jpg", 
                    Year = 2021, 
                    Genre = "Hành động / Viễn tưởng / Phiêu lưu", 
                    IsTVSeries = false,
                    Director = "Jon Watts",
                    Cast = "Tom Holland, Zendaya, Benedict Cumberbatch",
                    Description = "Danh tính siêu anh hùng của Peter Parker bị tiết lộ. Anh tìm đến Doctor Strange để làm phép xóa ký ức mọi người, vô tình xé rách đa vũ trụ và kéo theo những phản diện huyền thoại từ các thế giới khác.",
                    TrailerUrl = "https://www.youtube.com/embed/JfVOs4VSpmA",
                    TrailerVideoUrl = null,
                    Rating = 8.2
                },
                new Movie 
                { 
                    Id = 11, 
                    Title = "Dune: Part Two", 
                    ImageUrl = "/images/posters/dune-part-two.jpg", 
                    Year = 2024, 
                    Genre = "Viễn tưởng / Phiêu lưu / Hành động", 
                    IsTVSeries = false,
                    Director = "Denis Villeneuve",
                    Cast = "Timothée Chalamet, Zendaya, Rebecca Ferguson",
                    Description = "Paul Atreides hợp lực cùng Chani và người Fremen trên con đường phục thù những kẻ đã hủy hoại gia tộc mình, đối mặt với sự giằng xé giữa tình yêu của đời mình và số phận của vũ trụ.",
                    TrailerUrl = "https://www.youtube.com/embed/U2Qp5pL3ovA",
                    TrailerVideoUrl = "/videos/trailers/dune2.mp4",
                    Rating = 8.6
                },
                new Movie 
                { 
                    Id = 12, 
                    Title = "John Wick: Chapter 4", 
                    ImageUrl = "/images/posters/johnwick-chapter4.jpg", 
                    Year = 2023, 
                    Genre = "Hành động / Giật gân", 
                    IsTVSeries = false,
                    Director = "Chad Stahelski",
                    Cast = "Keanu Reeves, Donnie Yen, Bill Skarsgård",
                    Description = "John Wick tìm ra con đường đánh bại Hội Tối Cao để giành lại tự do cho bản thân, đối đầu với một liên minh sát thủ toàn cầu mới và những người bạn cũ nay ở hai đầu chiến tuyến.",
                    TrailerUrl = "https://www.youtube.com/embed/qEVUardwtLH",
                    TrailerVideoUrl = "/videos/trailers/john_wick4.mp4",
                    Rating = 7.7
                },
                new Movie 
                { 
                    Id = 13, 
                    Title = "The Batman", 
                    ImageUrl = "/images/posters/thebatman.jpg", 
                    Year = 2022, 
                    Genre = "Hành động / Hình sự / Kịch tính", 
                    IsTVSeries = false,
                    Director = "Matt Reeves",
                    Cast = "Robert Pattinson, Zoë Kravitz, Jeffrey Wright",
                    Description = "Trong năm thứ hai chống tội phạm ở Gotham, Batman dấn thân vào cuộc điều tra các vụ giết người hàng loạt của gã điên Riddler, phơi bày những bê bối tham nhũng chôn giấu từ lâu của thành phố.",
                    TrailerUrl = "https://www.youtube.com/embed/mqq_HDEH518",
                    TrailerVideoUrl = "/videos/trailers/the_batman.mp4",
                    Rating = 7.8
                },
                new Movie 
                { 
                    Id = 14, 
                    Title = "Spider-Man: Into the Spider-Verse", 
                    ImageUrl = "/images/posters/spiderverse.jpg", 
                    Year = 2018, 
                    Genre = "Hoạt hình / Hành động / Phiêu lưu", 
                    IsTVSeries = false,
                    Director = "Bob Persichetti, Peter Ramsey, Rodney Rothman",
                    Cast = "Shameik Moore, Jake Johnson, Hailee Steinfeld",
                    Description = "Miles Morales, một thiếu niên da màu ở Brooklyn, vô tình có siêu năng lực nhện và phải hợp tác với những Người Nhện khác từ các chiều không gian song song để giải cứu New York.",
                    TrailerUrl = "https://www.youtube.com/embed/g4Hbz2jLxLk",
                    TrailerVideoUrl = null,
                    Rating = 8.4
                },
                new Movie 
                { 
                    Id = 15, 
                    Title = "Spirited Away", 
                    ImageUrl = "/images/posters/spiritedaway.jpg", 
                    Year = 2001, 
                    Genre = "Hoạt hình / Phiêu lưu / Kỳ ảo", 
                    IsTVSeries = false,
                    Director = "Hayao Miyazaki",
                    Cast = "Rumi Hiiragi, Miyu Irino, Mari Natsuki",
                    Description = "Cô bé Chihiro đi lạc vào thế giới linh hồn kỳ lạ, nơi cha mẹ cô bị biến thành heo. Cô phải làm việc tại nhà tắm của mụ phù thủy Yubaba và tìm cách giải cứu gia đình mình.",
                    TrailerUrl = "https://www.youtube.com/embed/ByXuk9QqQkk",
                    TrailerVideoUrl = null,
                    Rating = 8.6
                },
                new Movie 
                { 
                    Id = 16, 
                    Title = "Avatar: The Way of Water", 
                    ImageUrl = "/images/posters/avatar-way-of-water.jpg", 
                    Year = 2022, 
                    Genre = "Viễn tưởng / Hành động / Phiêu lưu", 
                    IsTVSeries = false,
                    Director = "James Cameron",
                    Cast = "Sam Worthington, Zoe Saldana, Sigourney Weaver",
                    Description = "Nhiều năm sau cuộc chiến đầu tiên, Jake Sully và Neytiri phải rời bỏ quê hương rừng xanh để tìm kiếm nơi trú ẩn tại bộ tộc vùng biển đảo Pandora trước mối hiểm họa xâm lăng mới.",
                    TrailerUrl = "https://www.youtube.com/embed/d9MyW72ELq0",
                    TrailerVideoUrl = null,
                    Rating = 7.6
                },
                new Movie 
                { 
                    Id = 17, 
                    Title = "Inception", 
                    ImageUrl = "/images/posters/inception.jpg", 
                    Year = 2010, 
                    Genre = "Viễn tưởng / Hành động / Giật gân", 
                    IsTVSeries = false,
                    Director = "Christopher Nolan",
                    Cast = "Leonardo DiCaprio, Joseph Gordon-Levitt, Elliot Page",
                    Description = "Một kẻ trộm chuyên nghiệp chuyên xâm nhập vào tiềm thức của mục tiêu thông qua giấc mơ để đánh cắp bí mật kinh doanh, nay nhận nhiệm vụ đảo ngược: cấy một ý tưởng vào tâm trí đối thủ.",
                    TrailerUrl = "https://www.youtube.com/embed/YoHD9XEInc0",
                    TrailerVideoUrl = null,
                    Rating = 8.8
                },
                new Movie 
                { 
                    Id = 18, 
                    Title = "How to Train Your Dragon", 
                    ImageUrl = "/images/posters/howtotrainyourdragon.jpg", 
                    Year = 2010, 
                    Genre = "Hoạt hình / Phiêu lưu / Hài hước", 
                    IsTVSeries = false,
                    Director = "Dean DeBlois, Chris Sanders",
                    Cast = "Jay Baruchel, Gerard Butler, America Ferrera",
                    Description = "Hiccup, một cậu bé Viking yếu ớt sống trên đảo Berk vốn có truyền thống diệt rồng, vô tình bắt được một chú rồng loài Night Fury quý hiếm và nhận ra rồng không đáng sợ như họ vẫn tưởng.",
                    TrailerUrl = "https://www.youtube.com/embed/fTlrTevG1-I",
                    TrailerVideoUrl = null,
                    Rating = 8.1
                }
            );

            // Seeding Many-to-Many connections (MovieCategory)
            modelBuilder.Entity<MovieCategory>().HasData(
                // Breaking Bad (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 1, CategoryId = 1 },
                new MovieCategory { MovieId = 1, CategoryId = 5 },
                // Game of Thrones (Hành Động + Kịch Tính + Phiêu Lưu)
                new MovieCategory { MovieId = 2, CategoryId = 1 },
                new MovieCategory { MovieId = 2, CategoryId = 5 },
                new MovieCategory { MovieId = 2, CategoryId = 7 },
                // Oppenheimer (Kịch Tính)
                new MovieCategory { MovieId = 3, CategoryId = 5 },
                // Avengers: Infinity War (Hành Động + Viễn Tưởng + Phiêu Lưu)
                new MovieCategory { MovieId = 4, CategoryId = 1 },
                new MovieCategory { MovieId = 4, CategoryId = 3 },
                new MovieCategory { MovieId = 4, CategoryId = 7 },
                // Fight Club (Kịch Tính)
                new MovieCategory { MovieId = 5, CategoryId = 5 },
                // The Dark Knight (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 6, CategoryId = 1 },
                new MovieCategory { MovieId = 6, CategoryId = 5 },
                // Interstellar (Viễn Tưởng + Kịch Tính + Phiêu Lưu)
                new MovieCategory { MovieId = 7, CategoryId = 3 },
                new MovieCategory { MovieId = 7, CategoryId = 5 },
                new MovieCategory { MovieId = 7, CategoryId = 7 },
                // Wednesday (Kinh Dị + Viễn Tưởng + Kịch Tính)
                new MovieCategory { MovieId = 8, CategoryId = 2 },
                new MovieCategory { MovieId = 8, CategoryId = 3 },
                new MovieCategory { MovieId = 8, CategoryId = 5 },
                // Squid Game (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 9, CategoryId = 1 },
                new MovieCategory { MovieId = 9, CategoryId = 5 },
                // Spider-Man: NWH (Hành Động + Viễn Tưởng + Phiêu Lưu)
                new MovieCategory { MovieId = 10, CategoryId = 1 },
                new MovieCategory { MovieId = 10, CategoryId = 3 },
                new MovieCategory { MovieId = 10, CategoryId = 7 },
                // Dune: Part Two (Hành Động + Viễn Tưởng + Phiêu Lưu)
                new MovieCategory { MovieId = 11, CategoryId = 1 },
                new MovieCategory { MovieId = 11, CategoryId = 3 },
                new MovieCategory { MovieId = 11, CategoryId = 7 },
                // John Wick 4 (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 12, CategoryId = 1 },
                new MovieCategory { MovieId = 12, CategoryId = 5 },
                // The Batman (Hành Động + Kịch Tính)
                new MovieCategory { MovieId = 13, CategoryId = 1 },
                new MovieCategory { MovieId = 13, CategoryId = 5 },
                // Spider-Man: Into the Spider-Verse (Hành Động + Hoạt Hình + Phiêu Lưu)
                new MovieCategory { MovieId = 14, CategoryId = 1 },
                new MovieCategory { MovieId = 14, CategoryId = 6 },
                new MovieCategory { MovieId = 14, CategoryId = 7 },
                // Spirited Away (Hoạt Hình + Phiêu Lưu)
                new MovieCategory { MovieId = 15, CategoryId = 6 },
                new MovieCategory { MovieId = 15, CategoryId = 7 },
                // Avatar: The Way of Water (Hành Động + Viễn Tưởng + Phiêu Lưu)
                new MovieCategory { MovieId = 16, CategoryId = 1 },
                new MovieCategory { MovieId = 16, CategoryId = 3 },
                new MovieCategory { MovieId = 16, CategoryId = 7 },
                // Inception (Hành Động + Viễn Tưởng)
                new MovieCategory { MovieId = 17, CategoryId = 1 },
                new MovieCategory { MovieId = 17, CategoryId = 3 },
                // How to Train Your Dragon (Hoạt Hình + Phiêu Lưu)
                new MovieCategory { MovieId = 18, CategoryId = 6 },
                new MovieCategory { MovieId = 18, CategoryId = 7 }
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

            // Seeding MovieImages — mỗi phim 1 ảnh poster đại diện (tránh trùng lặp trong thư viện)
            modelBuilder.Entity<MovieImage>().HasData(
                new MovieImage { Id = 1, MovieId = 1, ImageUrl = "/images/posters/breakingbad.jpg" },
                new MovieImage { Id = 2, MovieId = 2, ImageUrl = "/images/posters/gameofthrones.jpg" },
                new MovieImage { Id = 3, MovieId = 3, ImageUrl = "/images/posters/oppenheimer.jpg" },
                new MovieImage { Id = 4, MovieId = 4, ImageUrl = "/images/posters/avengers.jpg" },
                new MovieImage { Id = 5, MovieId = 5, ImageUrl = "/images/posters/fightclub.jpg" },
                new MovieImage { Id = 6, MovieId = 6, ImageUrl = "/images/posters/thedarkknight.jpg" },
                new MovieImage { Id = 7, MovieId = 7, ImageUrl = "/images/posters/interstellar.jpg" },
                new MovieImage { Id = 8, MovieId = 8, ImageUrl = "/images/posters/wednesday.jpg" },
                new MovieImage { Id = 9, MovieId = 9, ImageUrl = "/images/posters/squidgame.jpg" },
                new MovieImage { Id = 10, MovieId = 10, ImageUrl = "/images/posters/spiderman-no-way-home.jpg" },
                new MovieImage { Id = 11, MovieId = 11, ImageUrl = "/images/posters/dune-part-two.jpg" },
                new MovieImage { Id = 12, MovieId = 12, ImageUrl = "/images/posters/johnwick-chapter4.jpg" },
                new MovieImage { Id = 13, MovieId = 13, ImageUrl = "/images/posters/thebatman.jpg" },
                new MovieImage { Id = 14, MovieId = 14, ImageUrl = "/images/posters/spiderverse.jpg" },
                new MovieImage { Id = 15, MovieId = 15, ImageUrl = "/images/posters/spiritedaway.jpg" },
                new MovieImage { Id = 16, MovieId = 16, ImageUrl = "/images/posters/avatar-way-of-water.jpg" },
                new MovieImage { Id = 17, MovieId = 17, ImageUrl = "/images/posters/inception.jpg" },
                new MovieImage { Id = 18, MovieId = 18, ImageUrl = "/images/posters/howtotrainyourdragon.jpg" }
            );
        }
    }
}
