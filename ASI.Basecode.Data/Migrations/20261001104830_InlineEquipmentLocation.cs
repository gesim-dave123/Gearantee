using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class InlineEquipmentLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "EquipmentItem",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM [EquipmentItem] AS item
    INNER JOIN [Location] AS place ON item.[LocationId] = place.[LocationId]
    WHERE CONVERT(varbinary(max), place.[LocationName]) <>
          CONVERT(varbinary(max), CONVERT(nvarchar(200), CONVERT(varchar(200), place.[LocationName])))
)
    THROW 50001, 'A location name cannot be represented as varchar(200) without data loss.', 1;

-- Dynamic SQL binds the new column after ALTER TABLE has executed, including
-- when this migration is run from a generated SQL script in SSMS.
EXEC(N'
UPDATE item
SET [Location] = CONVERT(varchar(200), place.[LocationName])
FROM [EquipmentItem] AS item
INNER JOIN [Location] AS place ON item.[LocationId] = place.[LocationId];

IF EXISTS (SELECT 1 FROM [EquipmentItem] WHERE [Location] IS NULL)
    THROW 50002, ''An equipment item has no location to migrate.'', 1;
');");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "EquipmentItem",
                type: "varchar(200)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentItem_Location_LocationId",
                table: "EquipmentItem");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentItem_LocationId",
                table: "EquipmentItem");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "EquipmentItem");

            migrationBuilder.DropTable(
                name: "Location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    LocationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LocationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.LocationId);
                });

            migrationBuilder.AddColumn<long>(
                name: "LocationId",
                table: "EquipmentItem",
                type: "bigint",
                nullable: true);

            // Only locations still used by equipment can be reconstructed.
            migrationBuilder.Sql(@"
EXEC(N'
INSERT INTO [Location] ([LocationName])
SELECT DISTINCT CONVERT(nvarchar(200), [Location])
FROM [EquipmentItem];

UPDATE item
SET [LocationId] = place.[LocationId]
FROM [EquipmentItem] AS item
INNER JOIN [Location] AS place
    ON CONVERT(nvarchar(200), item.[Location]) = place.[LocationName];

IF EXISTS (SELECT 1 FROM [EquipmentItem] WHERE [LocationId] IS NULL)
    THROW 50003, ''An equipment location could not be reconstructed.'', 1;
');");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "EquipmentItem",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentItem_LocationId",
                table: "EquipmentItem",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocationName",
                table: "Location",
                column: "LocationName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentItem_Location_LocationId",
                table: "EquipmentItem",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Location",
                table: "EquipmentItem");
        }
    }
}
