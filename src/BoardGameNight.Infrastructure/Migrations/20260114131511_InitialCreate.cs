using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardGameNight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoardGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Genre = table.Column<int>(type: "int", nullable: false),
                    GameType = table.Column<int>(type: "int", nullable: false),
                    IsAdultOnly = table.Column<bool>(type: "bit", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DietaryPreferences = table.Column<int>(type: "int", nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameNights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HouseNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    IsAdultOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsPotluck = table.Column<bool>(type: "bit", nullable: false),
                    AvailableDietaryOptions = table.Column<int>(type: "int", nullable: false),
                    OrganizerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameNights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameNights_Persons_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DietaryOptions = table.Column<int>(type: "int", nullable: false),
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    BroughtByPersonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoodItems_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodItems_Persons_BroughtByPersonId",
                        column: x => x.BroughtByPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GameNightBoardGames",
                columns: table => new
                {
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    BoardGameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameNightBoardGames", x => new { x.GameNightId, x.BoardGameId });
                    table.ForeignKey(
                        name: "FK_GameNightBoardGames_BoardGames_BoardGameId",
                        column: x => x.BoardGameId,
                        principalTable: "BoardGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameNightBoardGames_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameNightParticipations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DidAttend = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameNightParticipations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameNightParticipations_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameNightParticipations_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    ReviewText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameNightId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_GameNights_GameNightId",
                        column: x => x.GameNightId,
                        principalTable: "GameNights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Persons_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_BroughtByPersonId",
                table: "FoodItems",
                column: "BroughtByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_GameNightId",
                table: "FoodItems",
                column: "GameNightId");

            migrationBuilder.CreateIndex(
                name: "IX_GameNightBoardGames_BoardGameId",
                table: "GameNightBoardGames",
                column: "BoardGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameNightParticipations_GameNightId_PersonId",
                table: "GameNightParticipations",
                columns: new[] { "GameNightId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameNightParticipations_PersonId",
                table: "GameNightParticipations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GameNights_OrganizerId",
                table: "GameNights",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Email",
                table: "Persons",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_IdentityUserId",
                table: "Persons",
                column: "IdentityUserId",
                unique: true,
                filter: "[IdentityUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_GameNightId_ReviewerId",
                table: "Reviews",
                columns: new[] { "GameNightId", "ReviewerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "GameNightBoardGames");

            migrationBuilder.DropTable(
                name: "GameNightParticipations");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "BoardGames");

            migrationBuilder.DropTable(
                name: "GameNights");

            migrationBuilder.DropTable(
                name: "Persons");
        }
    }
}
