using System.Globalization;
using CsvHelper;
using Inventario.ETL.Data;
using Inventario.ETL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using QuestPDF.Infrastructure;

// Cargar configuración
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = config.GetConnectionString("DwhConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DwhConnection' en appsettings.json.");

// Configurar licencia Community de QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

Console.WriteLine("=== SISTEMA DE CARGA DATA WAREHOUSE (ETL) ===");

try
{
    using (var db = new DwhContext(connectionString))
    {
        // Crear la base de datos y la tabla automáticamente si no existen
        Console.WriteLine("Verificando base de datos...");
        db.Database.EnsureCreated();

        // 1. EXTRACT: Leer el CSV
        string path = Path.Combine(AppContext.BaseDirectory, "Source", "Inventario_Fuente.csv");

        if (!File.Exists(path))
        {
            Console.WriteLine($"Error: No se encontró el archivo en {path}");
            return;
        }

        using (var reader = new StreamReader(path))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // dynamic permite leer las columnas del CSV por nombre
            var registros = csv.GetRecords<dynamic>().ToList();

            // 2. TRANSFORM: Preparar los datos
            var listaDimensiones = registros.Select(r => new DimProducto
            {
                IdOriginal = int.Parse(r.Id),
                Sku = ((string)r.Sku).Trim().ToUpperInvariant(),
                Nombre = ((string)r.Nombre).Trim().ToUpperInvariant(),
                Categoria = string.IsNullOrWhiteSpace((string?)r.Categoria)
                    ? "Sin categoria"
                    : ((string)r.Categoria).Trim(),
                Precio = decimal.Parse(((string)r.Precio).Trim(), CultureInfo.InvariantCulture),
                FechaCarga = DateTime.Now
            }).ToList();

            // 3. LOAD: Cargar al DWH
            Console.WriteLine("Limpiando datos antiguos (Carga Full)...");
            db.DimProductos.RemoveRange(db.DimProductos);
            db.SaveChanges(); // Limpiamos antes de insertar

            Console.WriteLine("Insertando nuevas dimensiones...");
            db.DimProductos.AddRange(listaDimensiones);
            db.SaveChanges();

            Console.WriteLine($"\n¡Éxito!");
            Console.WriteLine($"Se procesaron y cargaron {listaDimensiones.Count} productos correctamente.");
        }

        // Generar graficos y tablero a partir de los datos reales cargados
        var resumenInventario = GeneradorGraficos.CrearGraficosInventario(db.DimProductos.AsNoTracking().ToList());
        TableroVentasPDF.GenerarPDF("TableroVentas.pdf", resumenInventario);
        Console.WriteLine("Tablero de ventas PDF generado: TableroVentas.pdf");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nOcurrió un error durante el proceso ETL:");
    Console.WriteLine(ex.Message);
}

Console.WriteLine("\nPresione cualquier tecla para salir...");
Console.ReadKey();
