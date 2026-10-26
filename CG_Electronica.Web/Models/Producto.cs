namespace CG_Electronica.Web.Models;

public class Producto : ItemFacturable
{
    public string Descripcion { get; set; } = string.Empty;
    public string? Modelo { get; set; }
    public string? CodigoFabricante { get; set; }
    public decimal PrecioCompra { get; set; }
    public int Existencias { get; set; }
    public int ExistenciasMinimas { get; set; }

    public int IdMarca { get; set; }
    public virtual Marca Marca { get; set; } = null!;

    public int IdCategoria { get; set; }
    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}