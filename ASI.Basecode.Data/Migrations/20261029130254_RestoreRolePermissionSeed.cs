using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestoreRolePermissionSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePermissionSeed",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionSeed", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_RolePermissionSeed_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Roles that already have grants were initialized by an earlier seeder run or
            // by the AddUserAdministration migration. Mark them so a later --seed run does
            // not restore defaults an administrator intentionally removed.
            migrationBuilder.Sql(
                "INSERT INTO RolePermissionSeed (RoleId) " +
                "SELECT role.Id FROM AspNetRoles AS role " +
                "WHERE EXISTS (SELECT 1 FROM RolePermission AS grantRow " +
                "WHERE grantRow.RoleId = role.Id);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissionSeed");
        }
    }
}
