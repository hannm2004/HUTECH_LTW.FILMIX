using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace untitled1.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieDescriptionTrailer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "IsTrending",
                table: "Movies");

            migrationBuilder.AddColumn<string>(
                name: "Cast",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrailerUrl",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "int", nullable: false),
                    SeasonNumber = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Episodes_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieCategories",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieCategories", x => new { x.MovieId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_MovieCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovieCategories_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovieImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovieImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovieImages_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[] { 5, "Kịch Tính" });

            migrationBuilder.InsertData(
                table: "Episodes",
                columns: new[] { "Id", "EpisodeNumber", "MovieId", "SeasonNumber", "Title", "VideoUrl" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, "Pilot (Tập Mở Đầu)", "/videos/sample.mp4" },
                    { 2, 2, 1, 1, "Cat's in the Bag... (Chiếc túi bí mật)", "/videos/sample.mp4" },
                    { 3, 3, 1, 1, "...And the Bag's in the River (Bí ẩn trôi sông)", "/videos/sample.mp4" },
                    { 4, 1, 1, 2, "Seven Thirty-Seven (Chuyến bay 737)", "/videos/sample.mp4" },
                    { 5, 1, 2, 1, "Winter Is Coming (Mùa đông đang đến)", "/videos/sample.mp4" },
                    { 6, 2, 2, 1, "The Kingsroad (Con đường hoàng gia)", "/videos/sample.mp4" },
                    { 7, 1, 8, 1, "Wednesday's Child Is Full of Woe (Đứa trẻ ngày Thứ Tư)", "/videos/sample.mp4" },
                    { 8, 2, 8, 1, "Woe Is the Loneliest Number (Cô độc nhất)", "/videos/sample.mp4" },
                    { 9, 3, 8, 1, "Friend or Woe (Bạn hay thù)", "/videos/sample.mp4" },
                    { 10, 1, 9, 1, "Red Light, Green Light (Đèn đỏ, đèn xanh)", "/videos/sample.mp4" },
                    { 11, 2, 9, 1, "Hell (Địa ngục)", "/videos/sample.mp4" }
                });

            migrationBuilder.InsertData(
                table: "MovieCategories",
                columns: new[] { "CategoryId", "MovieId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 3, 2 },
                    { 1, 4 },
                    { 3, 4 },
                    { 1, 6 },
                    { 3, 7 },
                    { 2, 8 },
                    { 3, 8 },
                    { 1, 9 },
                    { 1, 10 },
                    { 3, 10 }
                });

            migrationBuilder.InsertData(
                table: "MovieImages",
                columns: new[] { "Id", "ImageUrl", "MovieId" },
                values: new object[,]
                {
                    { 1, "/images/movies/still_action.png", 1 },
                    { 2, "/images/movies/1.jpg", 1 },
                    { 3, "/images/movies/2.jpg", 1 },
                    { 4, "/images/movies/still_scifi.png", 2 },
                    { 5, "/images/movies/2.jpg", 2 },
                    { 6, "/images/movies/3.jpg", 2 },
                    { 7, "/images/movies/3.jpg", 3 },
                    { 8, "/images/movies/4.jpg", 3 },
                    { 9, "/images/movies/5.jpg", 3 },
                    { 10, "/images/movies/still_scifi.png", 4 },
                    { 11, "/images/movies/still_action.png", 4 },
                    { 12, "/images/movies/4.jpg", 4 },
                    { 13, "/images/movies/5.jpg", 5 },
                    { 14, "/images/movies/6.jpg", 5 },
                    { 15, "/images/movies/7.jpg", 5 },
                    { 16, "/images/movies/still_action.png", 6 },
                    { 17, "/images/movies/6.jpg", 6 },
                    { 18, "/images/movies/7.jpg", 6 },
                    { 19, "/images/movies/still_scifi.png", 7 },
                    { 20, "/images/movies/7.jpg", 7 },
                    { 21, "/images/movies/8.jpg", 7 },
                    { 22, "/images/movies/8.jpg", 8 },
                    { 23, "/images/movies/9.jpg", 8 },
                    { 24, "/images/movies/10.jpg", 8 },
                    { 25, "/images/movies/9.jpg", 9 },
                    { 26, "/images/movies/10.jpg", 9 },
                    { 27, "/images/movies/1.jpg", 9 },
                    { 28, "/images/movies/still_scifi.png", 10 },
                    { 29, "/images/movies/10.jpg", 10 },
                    { 30, "/images/movies/1.jpg", 10 }
                });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cast", "Description", "Director", "Genre", "IsTVSeries", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Crime/Drama", true, "Breaking Bad", "", 2008 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cast", "Description", "Director", "Genre", "IsTVSeries", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Action/Fantasy", true, "Game of Thrones", "", 2011 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Cast", "Description", "Director", "Genre", "ImageUrl", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Drama/History", "/images/movies/3.jpg", "Oppenheimer", "", 2023 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Cast", "Description", "Director", "Genre", "ImageUrl", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Action/Sci-Fi", "/images/movies/4.jpg", "Avengers: Infinity War", "", 2018 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Cast", "Description", "Director", "Genre", "ImageUrl", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Drama/Thriller", "/images/movies/5.jpg", "Fight Club", "", 1999 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Cast", "Description", "Director", "Genre", "ImageUrl", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Action/Drama", "/images/movies/6.jpg", "The Dark Knight", "", 2008 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Cast", "Description", "Director", "Genre", "ImageUrl", "IsTVSeries", "Title", "TrailerUrl", "Year" },
                values: new object[] { "", "", "", "Sci-Fi/Drama", "/images/movies/7.jpg", false, "Interstellar", "", 2014 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Cast", "Description", "Director", "TrailerUrl" },
                values: new object[] { "", "", "", "" });

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
                columns: new[] { "Cast", "Description", "Director", "Genre", "Title", "TrailerUrl" },
                values: new object[] { "", "", "", "Action/Sci-Fi", "Spider-Man: No Way Home", "" });

            migrationBuilder.InsertData(
                table: "MovieCategories",
                columns: new[] { "CategoryId", "MovieId" },
                values: new object[,]
                {
                    { 5, 1 },
                    { 5, 3 },
                    { 5, 5 },
                    { 5, 6 },
                    { 5, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_MovieId",
                table: "Episodes",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieCategories_CategoryId",
                table: "MovieCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieImages_MovieId",
                table: "MovieImages",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "MovieCategories");

            migrationBuilder.DropTable(
                name: "MovieImages");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "Cast",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "Director",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "TrailerUrl",
                table: "Movies");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Movies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrending",
                table: "Movies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryId", "Description", "Genre", "IsTVSeries", "IsTrending", "Title", "Year" },
                values: new object[] { 1, "The Avengers assemble once more in order to restore order to the universe.", "Action", false, true, "Avengers: Endgame", 2019 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "Description", "Genre", "IsTVSeries", "IsTrending", "Title", "Year" },
                values: new object[] { 1, "John Wick uncovers a path to defeating The High Table.", "Action", false, true, "John Wick 4", 2023 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "Description", "Genre", "ImageUrl", "IsTrending", "Title", "Year" },
                values: new object[] { 2, "Paranormal investigators Ed and Lorraine Warren work to help a family terrorized by a dark presence.", "Horror", "/images/movies/8.jpg", false, "The Conjuring", 2013 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CategoryId", "Description", "Genre", "ImageUrl", "IsTrending", "Title", "Year" },
                values: new object[] { 3, "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.", "Sci-Fi", "/images/movies/7.jpg", true, "Interstellar", 2014 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CategoryId", "Description", "Genre", "ImageUrl", "IsTrending", "Title", "Year" },
                values: new object[] { 3, "A thief who steals corporate secrets through the use of dream-sharing technology.", "Sci-Fi", "/images/movies/6.jpg", false, "Inception", 2010 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CategoryId", "Description", "Genre", "ImageUrl", "IsTrending", "Title", "Year" },
                values: new object[] { 4, "While navigating their careers in Los Angeles, a pianist and an actress fall in love.", "Romance", "/images/movies/5.jpg", false, "La La Land", 2016 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CategoryId", "Description", "Genre", "ImageUrl", "IsTVSeries", "IsTrending", "Title", "Year" },
                values: new object[] { 1, "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine.", "Drama/Action", "/images/movies/4.jpg", true, true, "Breaking Bad", 2008 });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CategoryId", "Description", "IsTrending" },
                values: new object[] { 2, "Follows Wednesday Addams' years as a student, when she attempts to master her emerging psychic ability.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CategoryId", "Description", "Genre", "IsTrending" },
                values: new object[] { 1, "Hundreds of cash-strapped players accept a strange invitation to compete in children's games.", "Thriller", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CategoryId", "Description", "Genre", "IsTrending", "Title" },
                values: new object[] { 1, "With Spider-Man's identity now revealed, Peter asks Doctor Strange for help.", "Action", true, "Spider-Man: NWH" });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "CategoryId", "Description", "Genre", "ImageUrl", "IsTVSeries", "IsTrending", "Title", "Year" },
                values: new object[,]
                {
                    { 11, 3, "When a young boy disappears, his mother, a police chief and his friends must confront terrifying supernatural forces.", "Sci-Fi", "/images/movies/1.jpg", true, true, "Stranger Things", 2016 },
                    { 12, 2, "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan.", "Thriller", "/images/movies/2.jpg", false, false, "Parasite", 2019 },
                    { 13, 3, "Paul Atreides unites with Chani and the Fremen while on a warpath of revenge against the conspirators who destroyed his family.", "Sci-Fi", "/images/movies/3.jpg", false, true, "Dune: Part Two", 2024 },
                    { 14, 1, "After a global pandemic destroys civilization, a hardened survivor takes charge of a 14-year-old girl.", "Drama", "/images/movies/4.jpg", true, true, "The Last of Us", 2023 },
                    { 15, 1, "A gangster family epic set in 1900s England, centering on a gang who sew razor blades in the peaks of their caps.", "Crime", "/images/movies/5.jpg", true, false, "Peaky Blinders", 2013 },
                    { 16, 3, "A thief who steals corporate secrets through the use of dream-sharing technology.", "Sci-Fi", "/images/movies/6.jpg", false, false, "Inception", 2010 },
                    { 17, 3, "Geralt of Rivia, a solitary monster hunter, struggles to find his place in a world where people often prove more wicked than beasts.", "Fantasy", "/images/movies/7.jpg", true, true, "The Witcher", 2019 },
                    { 18, 1, "An unusual group of robbers attempt to carry out the most perfect heist in Spanish history.", "Crime", "/images/movies/8.jpg", true, true, "Money Heist", 2017 },
                    { 19, 2, "A mentally troubled comedian is disregarded and mistreated by society.", "Drama", "/images/movies/9.jpg", false, true, "Joker", 2019 },
                    { 20, 1, "A financial advisor drags his family from Chicago to the Missouri Ozarks.", "Crime", "/images/movies/10.jpg", true, false, "Ozark", 2017 }
                });
        }
    }
}
