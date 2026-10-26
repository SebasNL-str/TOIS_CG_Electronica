namespace CG_Electronica.Web.Models.ViewModels;

public class ViewModelFacturaCreate
{
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdCliente { get; set; }
}

public class ItemFacturaDto
{
    public int IdItem { get; set; }
    public int Cantidad { get; set; }
}