using Microsoft.EntityFrameworkCore;
using Inventario.ETL.Models; // <--- Esta línea es la que da el error CS0234 si no coinciden

namespace Inventario.ETL.Data
{
    public class DwhContext : DbContext
    {
        public DbSet<DimProducto> DimProductos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=localhost;Database=DWH_Inventario;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}