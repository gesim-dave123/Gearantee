using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "IF EXISTS (" +
                "    SELECT NormalizedEmail FROM AspNetUsers " +
                "    WHERE NormalizedEmail IS NOT NULL " +
                "    GROUP BY NormalizedEmail HAVING COUNT_BIG(*) > 1" +
                ") " +
                "    THROW 51001, 'Cannot enforce unique account emails: duplicate normalized email addresses already exist. Resolve them before retrying this migration.', 1;");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AdministrationAuditEvent",
                columns: table => new
                {
                    AdministrationAuditEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TargetRoleId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrationAuditEvent", x => x.AdministrationAuditEventId);
                    table.ForeignKey(
                        name: "FK_AdministrationAuditEvent_AspNetRoles_TargetRoleId",
                        column: x => x.TargetRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdministrationAuditEvent_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrationAuditEvent_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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

            // Existing deployments may already have role grants. Mark those roles
            // initialized so a later --seed run preserves their assignments.
            migrationBuilder.Sql(
                "INSERT INTO RolePermissionSeed (RoleId) " +
                "SELECT role.Id FROM AspNetRoles AS role " +
                "WHERE EXISTS (SELECT 1 FROM RolePermission AS grantRow " +
                "WHERE grantRow.RoleId = role.Id);");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationAuditEvent_ActorUserId",
                table: "AdministrationAuditEvent",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationAuditEvent_OccurredAt",
                table: "AdministrationAuditEvent",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationAuditEvent_TargetRoleId",
                table: "AdministrationAuditEvent",
                column: "TargetRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrationAuditEvent_TargetUserId",
                table: "AdministrationAuditEvent",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministrationAuditEvent");

            migrationBuilder.DropTable(
                name: "RolePermissionSeed");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
        }
    }
}
