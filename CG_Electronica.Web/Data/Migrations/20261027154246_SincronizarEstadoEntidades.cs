using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CG_Electronica.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarEstadoEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "ROL",
                type: "varchar(30)",
                                               maxLength: 30,
                                               nullable: false,
                                               defaultValue: "Activo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                table: "ROL");
        }
    }
}
