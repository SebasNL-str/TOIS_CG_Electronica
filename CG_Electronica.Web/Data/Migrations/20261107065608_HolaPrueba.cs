using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CG_Electronica.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class HolaPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DETALLEFACTURA_FACTURA_idFactura",
                table: "DETALLEFACTURA");

            migrationBuilder.DropForeignKey(
                name: "FK_DETALLEFACTURA_ITEMFACTURABLE_idItem",
                table: "DETALLEFACTURA");

            migrationBuilder.RenameColumn(
                name: "total",
                table: "FACTURA",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "telefonoClienteHistorico",
                table: "FACTURA",
                newName: "TelefonoClienteHistorico");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                table: "FACTURA",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "numeroFactura",
                table: "FACTURA",
                newName: "NumeroFactura");

            migrationBuilder.RenameColumn(
                name: "nombreClienteHistorico",
                table: "FACTURA",
                newName: "NombreClienteHistorico");

            migrationBuilder.RenameColumn(
                name: "iva",
                table: "FACTURA",
                newName: "Iva");

            migrationBuilder.RenameColumn(
                name: "fechaAnulacion",
                table: "FACTURA",
                newName: "FechaAnulacion");

            migrationBuilder.RenameColumn(
                name: "fecha",
                table: "FACTURA",
                newName: "Fecha");

            migrationBuilder.RenameColumn(
                name: "estado",
                table: "FACTURA",
                newName: "Estado");

            migrationBuilder.RenameColumn(
                name: "cedulaClienteHistorica",
                table: "FACTURA",
                newName: "CedulaClienteHistorica");

            migrationBuilder.RenameIndex(
                name: "IX_FACTURA_numeroFactura",
                table: "FACTURA",
                newName: "IX_FACTURA_NumeroFactura");

            migrationBuilder.RenameColumn(
                name: "precioUnitarioHistorico",
                table: "DETALLEFACTURA",
                newName: "PrecioUnitarioHistorico");

            migrationBuilder.RenameColumn(
                name: "idItem",
                table: "DETALLEFACTURA",
                newName: "IdItem");

            migrationBuilder.RenameColumn(
                name: "idFactura",
                table: "DETALLEFACTURA",
                newName: "IdFactura");

            migrationBuilder.RenameColumn(
                name: "cantidad",
                table: "DETALLEFACTURA",
                newName: "Cantidad");

            migrationBuilder.RenameIndex(
                name: "IX_DETALLEFACTURA_idItem",
                table: "DETALLEFACTURA",
                newName: "IX_DETALLEFACTURA_IdItem");

            migrationBuilder.RenameIndex(
                name: "IX_DETALLEFACTURA_idFactura_idItem",
                table: "DETALLEFACTURA",
                newName: "IX_DETALLEFACTURA_IdFactura_IdItem");

            migrationBuilder.AlterColumn<string>(
                name: "TelefonoClienteHistorico",
                table: "FACTURA",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroFactura",
                table: "FACTURA",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "NombreClienteHistorico",
                table: "FACTURA",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "FACTURA",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "CedulaClienteHistorica",
                table: "FACTURA",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DETALLEFACTURA_FACTURA_IdFactura",
                table: "DETALLEFACTURA",
                column: "IdFactura",
                principalTable: "FACTURA",
                principalColumn: "idFactura",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DETALLEFACTURA_ITEMFACTURABLE_IdItem",
                table: "DETALLEFACTURA",
                column: "IdItem",
                principalTable: "ITEMFACTURABLE",
                principalColumn: "idItem",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
