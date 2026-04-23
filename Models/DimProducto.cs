using System.ComponentModel.DataAnnotations;

namespace Inventario.ETL.Models
{
    public class DimProducto
    {
        [Key]
        public int ProductoKey { get; set; }
        public int IdOriginal { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime FechaCarga { get; set; }
    }
}
