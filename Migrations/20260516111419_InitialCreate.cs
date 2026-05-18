using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace untitled1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Hành Động" },
                    { 2, "Kinh Dị" },
                    { 3, "Viễn Tưởng" },
                    { 4, "Tình Cảm" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "CategoryId", "Genre", "ImageUrl", "Title", "Year" },
                values: new object[,]
                {
                    { 1, 1, "Action", "/images/movies/1.jpg", "Avengers: Endgame", 2019 },
                    { 2, 1, "Action", "/images/movies/2.jpg", "John Wick 4", 2023 },
                    { 3, 2, "Horror", "/images/movies/8.jpg", "The Conjuring", 2013 },
                    { 4, 3, "Sci-Fi", "/images/movies/7.jpg", "Interstellar", 2014 },
                    { 5, 3, "Sci-Fi", "/images/movies/6.jpg", "Inception", 2010 },
                    { 6, 4, "Romance", "/images/movies/5.jpg", "La La Land", 2016 },
                    { 7, 1, "Drama/Action", "/images/movies/4.jpg", "Breaking Bad", 2008 },
                    { 8, 2, "Horror/Fantasy", "/images/movies/8.jpg", "Wednesday", 2022 },
                    { 9, 1, "Thriller", "/images/movies/9.jpg", "Squid Game", 2021 },
                    { 10, 1, "Action", "/images/movies/10.jpg", "Spider-Man: NWH", 2021 },
                    { 11, 3, "Sci-Fi", "/images/movies/1.jpg", "Stranger Things", 2016 },
                    { 12, 2, "Thriller", "/images/movies/2.jpg", "Parasite", 2019 },
                    { 13, 3, "Sci-Fi", "/images/movies/3.jpg", "Dune: Part Two", 2024 },
                    { 14, 1, "Drama", "/images/movies/4.jpg", "The Last of Us", 2023 },
                    { 15, 1, "Crime", "/images/movies/5.jpg", "Peaky Blinders", 2013 },
                    { 16, 3, "Sci-Fi", "/images/movies/6.jpg", "Inception", 2010 },
                    { 17, 3, "Fantasy", "/images/movies/7.jpg", "The Witcher", 2019 },
                    { 18, 1, "Crime", "/images/movies/8.jpg", "Money Heist", 2017 },
                    { 19, 2, "Drama", "/images/movies/9.jpg", "Joker", 2019 },
                    { 20, 1, "Crime", "/images/movies/10.jpg", "Ozark", 2017 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
