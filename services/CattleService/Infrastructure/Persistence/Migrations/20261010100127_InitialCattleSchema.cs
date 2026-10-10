using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFarm.Cattle.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCattleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cattle");

            migrationBuilder.CreateTable(
                name: "ranches",
                schema: "cattle",
                columns: table => new
                {
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranches", x => x.ranch_id);
                });

            migrationBuilder.CreateTable(
                name: "lots",
                schema: "cattle",
                columns: table => new
                {
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lots", x => x.lot_id);
                    table.UniqueConstraint("AK_lots_lot_id_ranch_id", x => new { x.lot_id, x.ranch_id });
                    table.ForeignKey(
                        name: "FK_lots_ranches_ranch_id",
                        column: x => x.ranch_id,
                        principalSchema: "cattle",
                        principalTable: "ranches",
                        principalColumn: "ranch_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cattle",
                schema: "cattle",
                columns: table => new
                {
                    cattle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ranch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ear_tag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cattle", x => x.cattle_id);
                    table.ForeignKey(
                        name: "FK_cattle_lots_lot_id_ranch_id",
                        columns: x => new { x.lot_id, x.ranch_id },
                        principalSchema: "cattle",
                        principalTable: "lots",
                        principalColumns: new[] { "lot_id", "ranch_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cattle_ranches_ranch_id",
                        column: x => x.ranch_id,
                        principalSchema: "cattle",
                        principalTable: "ranches",
                        principalColumn: "ranch_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cattle_lot_id_ranch_id",
                schema: "cattle",
                table: "cattle",
                columns: new[] { "lot_id", "ranch_id" });

            migrationBuilder.CreateIndex(
                name: "IX_cattle_ranch_id_ear_tag",
                schema: "cattle",
                table: "cattle",
                columns: new[] { "ranch_id", "ear_tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lots_ranch_id",
                schema: "cattle",
                table: "lots",
                column: "ranch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cattle",
                schema: "cattle");

            migrationBuilder.DropTable(
                name: "lots",
                schema: "cattle");

            migrationBuilder.DropTable(
                name: "ranches",
                schema: "cattle");
        }
    }
}
