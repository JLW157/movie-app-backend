using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieReactAPI.Migrations
{
    /// <inheritdoc />
    public partial class fixedReleaseDateName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RealeaseDate",
                table: "Movies",
                newName: "ReleaseDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleaseDate",
                table: "Movies",
                newName: "RealeaseDate");
        }
    }
}
