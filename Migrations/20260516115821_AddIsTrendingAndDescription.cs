using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace untitled1.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTrendingAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Movies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "The Avengers assemble once more in order to restore order to the universe.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "John Wick uncovers a path to defeating The High Table.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "Paranormal investigators Ed and Lorraine Warren work to help a family terrorized by a dark presence.", false });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A thief who steals corporate secrets through the use of dream-sharing technology.", false });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "While navigating their careers in Los Angeles, a pianist and an actress fall in love.", false });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "Follows Wednesday Addams' years as a student, when she attempts to master her emerging psychic ability.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "Hundreds of cash-strapped players accept a strange invitation to compete in children's games.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "With Spider-Man's identity now revealed, Peter asks Doctor Strange for help.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "When a young boy disappears, his mother, a police chief and his friends must confront terrifying supernatural forces.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan.", false });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "Paul Atreides unites with Chani and the Fremen while on a warpath of revenge against the conspirators who destroyed his family.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "After a global pandemic destroys civilization, a hardened survivor takes charge of a 14-year-old girl.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A gangster family epic set in 1900s England, centering on a gang who sew razor blades in the peaks of their caps.", false });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A thief who steals corporate secrets through the use of dream-sharing technology.", false });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "Geralt of Rivia, a solitary monster hunter, struggles to find his place in a world where people often prove more wicked than beasts.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "An unusual group of robbers attempt to carry out the most perfect heist in Spanish history.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A mentally troubled comedian is disregarded and mistreated by society.", true });

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Description", "IsTrending" },
                values: new object[] { "A financial advisor drags his family from Chicago to the Missouri Ozarks.", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "IsTrending",
                table: "Movies");
        }
    }
}
