using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace untitled1.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieRatingAndLocalTrailer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Movies",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TrailerVideoUrl",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PremiumEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PremiumStartDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxScreens = table.Column<int>(type: "int", nullable: false),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false),
                    HasDownload = table.Column<bool>(type: "bit", nullable: false),
                    HasSpatialAudio = table.Column<bool>(type: "bit", nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViewingHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    WatchTime = table.Column<int>(type: "int", nullable: false),
                    WatchedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViewingHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ViewingHistories_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 6, "Hoạt Hình" },
                    { 7, "Phiêu Lưu" }
                });

            migrationBuilder.InsertData(
                table: "MovieCategories",
                columns: new[] { "CategoryId", "MovieId" },
                values: new object[,]
                {
                    { 5, 2 },
                    { 5, 8 },
                    { 5, 9 }
                });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Bryan Cranston, Aaron Paul, Anna Gunn", "Một giáo viên hóa học cấp ba bị chẩn đoán mắc bệnh ung thư phổi giai đoạn cuối quyết định bắt tay với một cựu học sinh để sản xuất và bán ma túy đá chất lượng cao nhằm bảo đảm tài chính cho gia đình.", "Vince Gilligan", "Kịch tính / Hình sự", 9.5, "https://www.youtube.com/embed/HhesaQXLuRY", "/videos/trailers/breaking_bad.mp4" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Emilia Clarke, Kit Harington, Peter Dinklage", "Chín gia tộc quý tộc chiến đấu để giành quyền kiểm soát vùng đất giả tưởng Westeros, trong khi một kẻ thù cổ xưa đang thức tỉnh sau hàng thiên niên kỷ ngủ yên dưới bức tường băng tuyết phía Bắc.", "David Benioff", "Hành động / Kỳ ảo / Phiêu lưu", 9.1999999999999993, "https://www.youtube.com/embed/KPLYYLDtMJ0", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Cillian Murphy, Emily Blunt, Matt Damon", "Câu chuyện về nhà vật lý lý thuyết J. Robert Oppenheimer, người lãnh đạo Dự án Manhattan chế tạo ra quả bom nguyên tử đầu tiên cho nhân loại, mở ra thời đại hạt nhân và những dằn vặt đạo đức khôn nguôi.", "Christopher Nolan", "Kịch tính / Tiểu sử / Lịch sử", 8.4000000000000004, "https://www.youtube.com/embed/uYPbbksJxIg", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Robert Downey Jr., Chris Hemsworth, Mark Ruffalo", "Biệt đội siêu anh hùng Avengers và các đồng minh phải sẵn sàng hy sinh tất cả để cố gắng đánh bại Thanos quyền năng trước khi hắn hủy diệt một nửa vũ trụ bằng sáu Viên đá Vô cực.", "Anthony Russo, Joe Russo", "Hành động / Viễn tưởng / Phiêu lưu", 8.4000000000000004, "https://www.youtube.com/embed/6ZfuNTqbHE8", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Brad Pitt, Edward Norton, Helena Bonham Carter", "Một nhân viên văn phòng mất ngủ triền miên và một người bán xà phòng lập dị thành lập một câu lạc bộ đánh lộn ngầm đầy bạo lực, dần biến tướng thành một tổ chức vô chính phủ chống lại chủ nghĩa tiêu dùng.", "David Fincher", "Kịch tính / Giật gân", 8.8000000000000007, "https://www.youtube.com/embed/qtR39562a1g", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Christian Bale, Heath Ledger, Aaron Eckhart", "Khi mối đe dọa mang tên Joker xuất hiện tàn phá thành phố Gotham, Người Dơi phải chấp nhận những thử thách tâm lý và thể xác tột cùng để duy trì ranh giới mong manh giữa công lý và thù hận.", "Christopher Nolan", "Hành động / Kịch tính / Tội phạm", 9.0, "https://www.youtube.com/embed/LDG9bisJEaI", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Matthew McConaughey, Anne Hathaway, Jessica Chastain", "Trong tương lai khi Trái Đất sắp bị hủy diệt bởi nạn đói, một nhóm các nhà du hành vũ trụ dấn thân vào chuyến đi xuyên hố đen vũ trụ để tìm kiếm một mái nhà mới cho nhân loại.", "Christopher Nolan", "Viễn tưởng / Phiêu lưu / Kịch tính", 8.6999999999999993, "https://www.youtube.com/embed/zSWdZATo3cA", "/videos/trailers/interstellar.mp4" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Jenna Ortega, Gwendoline Christie, Riki Lindhome", "Wednesday Addams theo học tại Học viện Nevermore, nơi cô cố gắng làm chủ năng lượng ngoại cảm của mình, phá giải một vụ án giết người hàng loạt kinh hoàng và khám phá bí ẩn gia đình 25 năm trước.", "Tim Burton", "Kinh dị / Kỳ ảo / Kịch tính", 8.0999999999999996, "https://www.youtube.com/embed/Di310WS8zLk", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Lee Jung-jae, Park Hae-soo, Wi Ha-jun", "Hàng trăm người chơi nợ nần chồng chất chấp nhận lời mời kỳ lạ tham gia các trò chơi dân gian dành cho trẻ em với giải thưởng hấp dẫn, nhưng thất bại đồng nghĩa với cái chết ngay lập tức.", "Hwang Dong-hyuk", "Hành động / Giật gân / Kịch tính", 8.0, "https://www.youtube.com/embed/oqxAJKy0R4A", null });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Cast", "Description", "Director", "Genre", "Rating", "TrailerUrl", "TrailerVideoUrl" },
                values: new object[] { "Tom Holland, Zendaya, Benedict Cumberbatch", "Danh tính siêu anh hùng của Peter Parker bị tiết lộ. Anh tìm đến Doctor Strange để làm phép xóa ký ức mọi người, vô tình xé rách đa vũ trụ và kéo theo những phản diện huyền thoại từ các thế giới khác.", "Jon Watts", "Hành động / Viễn tưởng / Phiêu lưu", 8.1999999999999993, "https://www.youtube.com/embed/JfVOs4VSpmA", null });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "Cast", "Description", "Director", "Genre", "ImageUrl", "IsTVSeries", "Rating", "Title", "TrailerUrl", "TrailerVideoUrl", "Year" },
                values: new object[,]
                {
                    { 11, "Timothée Chalamet, Zendaya, Rebecca Ferguson", "Paul Atreides hợp lực cùng Chani và người Fremen trên con đường phục thù những kẻ đã hủy hoại gia tộc mình, đối mặt với sự giằng xé giữa tình yêu của đời mình và số phận của vũ trụ.", "Denis Villeneuve", "Viễn tưởng / Phiêu lưu / Hành động", "/images/hero/hero2.jpg", false, 8.5999999999999996, "Dune: Part Two", "https://www.youtube.com/embed/U2Qp5pL3ovA", "/videos/trailers/dune2.mp4", 2024 },
                    { 12, "Keanu Reeves, Donnie Yen, Bill Skarsgård", "John Wick tìm ra con đường đánh bại Hội Tối Cao để giành lại tự do cho bản thân, đối đầu với một liên minh sát thủ toàn cầu mới và những người bạn cũ nay ở hai đầu chiến tuyến.", "Chad Stahelski", "Hành động / Giật gân", "/images/hero/hero3.jpg", false, 7.7000000000000002, "John Wick: Chapter 4", "https://www.youtube.com/embed/qEVUardwtLH", "/videos/trailers/john_wick4.mp4", 2023 },
                    { 13, "Robert Pattinson, Zoë Kravitz, Jeffrey Wright", "Trong năm thứ hai chống tội phạm ở Gotham, Batman dấn thân vào cuộc điều tra các vụ giết người hàng loạt của gã điên Riddler, phơi bày những bê bối tham nhũng chôn giấu từ lâu của thành phố.", "Matt Reeves", "Hành động / Hình sự / Kịch tính", "/images/hero/hero4.jpg", false, 7.7999999999999998, "The Batman", "https://www.youtube.com/embed/mqq_HDEH518", "/videos/trailers/the_batman.mp4", 2022 },
                    { 14, "Shameik Moore, Jake Johnson, Hailee Steinfeld", "Miles Morales, một thiếu niên da màu ở Brooklyn, vô tình có siêu năng lực nhện và phải hợp tác với những Người Nhện khác từ các chiều không gian song song để giải cứu New York.", "Bob Persichetti, Peter Ramsey, Rodney Rothman", "Hoạt hình / Hành động / Phiêu lưu", "/images/hero/hero5.jpg", false, 8.4000000000000004, "Spider-Man: Into the Spider-Verse", "https://www.youtube.com/embed/g4Hbz2jLxLk", null, 2018 },
                    { 15, "Rumi Hiiragi, Miyu Irino, Mari Natsuki", "Cô bé Chihiro đi lạc vào thế giới linh hồn kỳ lạ, nơi cha mẹ cô bị biến thành heo. Cô phải làm việc tại nhà tắm của mụ phù thủy Yubaba và tìm cách giải cứu gia đình mình.", "Hayao Miyazaki", "Hoạt hình / Phiêu lưu / Kỳ ảo", "/images/hero/hero6.jpg", false, 8.5999999999999996, "Spirited Away", "https://www.youtube.com/embed/ByXuk9QqQkk", null, 2001 },
                    { 16, "Sam Worthington, Zoe Saldana, Sigourney Weaver", "Nhiều năm sau cuộc chiến đầu tiên, Jake Sully và Neytiri phải rời bỏ quê hương rừng xanh để tìm kiếm nơi trú ẩn tại bộ tộc vùng biển đảo Pandora trước mối hiểm họa xâm lăng mới.", "James Cameron", "Viễn tưởng / Hành động / Phiêu lưu", "/images/hero/hero7.jpg", false, 7.5999999999999996, "Avatar: The Way of Water", "https://www.youtube.com/embed/d9MyW72ELq0", null, 2022 },
                    { 17, "Leonardo DiCaprio, Joseph Gordon-Levitt, Elliot Page", "Một kẻ trộm chuyên nghiệp chuyên xâm nhập vào tiềm thức của mục tiêu thông qua giấc mơ để đánh cắp bí mật kinh doanh, nay nhận nhiệm vụ đảo ngược: cấy một ý tưởng vào tâm trí đối thủ.", "Christopher Nolan", "Viễn tưởng / Hành động / Giật gân", "/images/hero/hero8.jpg", false, 8.8000000000000007, "Inception", "https://www.youtube.com/embed/YoHD9XEInc0", null, 2010 },
                    { 18, "Jay Baruchel, Gerard Butler, America Ferrera", "Hiccup, một cậu bé Viking yếu ớt sống trên đảo Berk vốn có truyền thống diệt rồng, vô tình bắt được một chú rồng loài Night Fury quý hiếm và nhận ra rồng không đáng sợ như họ vẫn tưởng.", "Dean DeBlois, Chris Sanders", "Hoạt hình / Phiêu lưu / Hài hước", "/images/movies/1.jpg", false, 8.0999999999999996, "How to Train Your Dragon", "https://www.youtube.com/embed/fTlrTevG1-I", null, 2010 }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "AccentColor", "HasDownload", "HasSpatialAudio", "IsPopular", "MaxScreens", "Name", "Price", "Resolution", "Tagline" },
                values: new object[,]
                {
                    { 1, "#6b7280", false, false, false, 1, "Cơ Bản", 79000m, "480p SD", "Xem phim chất lượng tốt" },
                    { 2, "#e50914", true, true, true, 2, "Tiêu Chuẩn", 149000m, "1080p Full HD", "Full HD & Âm thanh không gian" },
                    { 3, "#f59e0b", true, true, false, 4, "Cao Cấp", 219000m, "4K Ultra HD + HDR", "4K Ultra HD + HDR + Dolby Atmos" }
                });

            migrationBuilder.InsertData(
                table: "MovieCategories",
                columns: new[] { "CategoryId", "MovieId" },
                values: new object[,]
                {
                    { 7, 2 },
                    { 7, 4 },
                    { 7, 7 },
                    { 7, 10 },
                    { 1, 11 },
                    { 3, 11 },
                    { 7, 11 },
                    { 1, 12 },
                    { 5, 12 },
                    { 1, 13 },
                    { 5, 13 },
                    { 1, 14 },
                    { 6, 14 },
                    { 7, 14 },
                    { 6, 15 },
                    { 7, 15 },
                    { 1, 16 },
                    { 3, 16 },
                    { 7, 16 },
                    { 1, 17 },
                    { 3, 17 },
                    { 6, 18 },
                    { 7, 18 }
                });

            migrationBuilder.InsertData(
                table: "MovieImages",
                columns: new[] { "Id", "ImageUrl", "MovieId" },
                values: new object[,]
                {
                    { 31, "/images/hero/hero2.jpg", 11 },
                    { 32, "/images/hero/hero3.jpg", 12 },
                    { 33, "/images/hero/hero4.jpg", 13 },
                    { 34, "/images/hero/hero5.jpg", 14 },
                    { 35, "/images/hero/hero6.jpg", 15 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PlanId",
                table: "OrderItems",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PlanId",
                table: "UserSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewingHistories_MovieId",
                table: "ViewingHistories",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_ViewingHistories_UserId",
                table: "ViewingHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "ViewingHistories");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 2 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 4 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 7 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 5, 8 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 5, 9 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 10 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 1, 11 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 3, 11 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 11 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 1, 12 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 5, 12 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 1, 13 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 5, 13 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 1, 14 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 6, 14 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 14 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 6, 15 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 15 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 1, 16 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 3, 16 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 16 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 1, 17 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 3, 17 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 6, 18 });

            migrationBuilder.DeleteData(
                table: "MovieCategories",
                keyColumns: new[] { "CategoryId", "MovieId" },
                keyValues: new object[] { 7, 18 });

            migrationBuilder.DeleteData(
                table: "MovieImages",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "MovieImages",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "MovieImages",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "MovieImages",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "MovieImages",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TrailerVideoUrl",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "PremiumEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PremiumStartDate",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "MovieCategories",
                columns: new[] { "CategoryId", "MovieId" },
                values: new object[] { 3, 2 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Crime/Drama", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Action/Fantasy", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Drama/History", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Action/Sci-Fi", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Drama/Thriller", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Action/Drama", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Sci-Fi/Drama", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Horror/Fantasy", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Action/Thriller", "" });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Cast", "Description", "Director", "Genre", "TrailerUrl" },
                values: new object[] { "", "", "", "Action/Sci-Fi", "" });
        }
    }
}
