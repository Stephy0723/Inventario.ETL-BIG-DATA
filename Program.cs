using System.Globalization;
using CsvHelper;
using Inventario.ETL.Data;
using Inventario.ETL.Models;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("=== SISTEMA DE CARGA DATA WAREHOUSE (ETL) ===");

try
{
    using (var db = new DwhContext())
    {
        // Crear la base de datos y la tabla automáticamente si no existen
        Console.WriteLine("Verificando base de datos...");
        db.Database.EnsureCreated();

        // 1. EXTRACT: Leer el CSV
        string path = "Source/Inventario_Fuente.csv";

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
                Sku = r.Sku,
                Nombre = ((string)r.Nombre).ToUpper().Trim(), // Limpieza y normalización
                Categoria = r.Categoria ?? "Sin Categoría",
                Precio = decimal.Parse(r.Precio, CultureInfo.InvariantCulture),
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
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nOcurrió un error durante el proceso ETL:");
    Console.WriteLine(ex.Message);
}

Console.WriteLine("\nPresione cualquier tecla para salir...");
Console.ReadKey();