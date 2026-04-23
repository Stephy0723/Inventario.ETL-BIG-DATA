using System.ComponentModel.DataAnnotations;

namespace Inventario.ETL.Models
{
    public class DimProducto
    {
        [Key]
        public int ProductoKey { get; set; }
        public int IdOriginal { get; set; }
        public string Sku { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaCarga { get; set; }
    }
}