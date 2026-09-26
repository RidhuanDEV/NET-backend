using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularBackend.Infrastructure.Persistence.Migrations;

[Migration("20260926113000_AddRefreshTokens")]
public partial class AddRefreshTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                FamilyId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                FamilyExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ReplacedByTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                table.ForeignKey("FK_refresh_tokens_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_refresh_tokens_TokenHash", "refresh_tokens", "TokenHash", unique: true);
        migrationBuilder.CreateIndex("IX_refresh_tokens_UserId_ExpiresAt", "refresh_tokens", new[] { "UserId", "ExpiresAt" });
        migrationBuilder.CreateIndex("IX_refresh_tokens_FamilyId_RevokedAt", "refresh_tokens", new[] { "FamilyId", "RevokedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("refresh_tokens");
}
