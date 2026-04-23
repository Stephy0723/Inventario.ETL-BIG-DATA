using Microsoft.EntityFrameworkCore;
using Inventario.ETL.Models;

namespace Inventario.ETL.Data
{
    public class DwhContext : DbContext
    {
        private readonly string _connectionString;

        public DwhContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<DimProducto> DimProductos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
        }
    }
}