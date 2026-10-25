using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CG_Electronica.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "CLIENTE");
        }
    }
}
