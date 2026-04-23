using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Globalization;

public class TableroVentasPDF
{
    public class Indicador
    {
        public string? Titulo { get; set; }
        public string? Valor { get; set; }
        public string? Descripcion { get; set; }
    }

    public static void GenerarPDF(string ruta, ResumenInventario resumen)
    {
        var indicadores = new List<Indicador>
        {
            new Indicador
            {
                Titulo = resumen.TotalProductos.ToString("N0", CultureInfo.InvariantCulture),
                Valor = "Productos cargados",
                Descripcion = $"Categorias detectadas: {resumen.TotalCategorias}"
            },
            new Indicador
            {
                Titulo = resumen.ValorTotal.ToString("N2", CultureInfo.InvariantCulture),
                Valor = "Valor total del inventario",
                Descripcion = "Suma de los precios de todos los productos"
            },
            new Indicador
            {
                Titulo = resumen.PrecioPromedio.ToString("N2", CultureInfo.InvariantCulture),
                Valor = "Precio promedio",
                Descripcion = $"Categoria con mayor valor: {resumen.CategoriaMayorValor}"
            }
        };

        string[] rutasGraficos =
        {
            Path.GetFullPath(GeneradorGraficos.GraficoProductosPorCategoria),
            Path.GetFullPath(GeneradorGraficos.GraficoValorPorCategoria),
            Path.GetFullPath(GeneradorGraficos.GraficoProductosMasCaros),
            Path.GetFullPath(GeneradorGraficos.GraficoPrecioPromedioCategoria)
        };

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Tablero de Inventario").Bold().FontSize(22).AlignCenter();
                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        foreach (var indicador in indicadores)
                        {
                            row.RelativeItem().Column(card =>
                            {
                                card.Item().Text(indicador.Titulo).Bold().FontSize(28);
                                card.Item().Text(indicador.Valor).FontSize(12);
                                card.Item().Text(indicador.Descripcion).FontSize(10).Italic();
                            });
                        }
                    });

                    col.Item().PaddingVertical(10);
                    col.Item().Text("Graficos generados con datos reales del ETL").FontSize(12).Bold();

                    foreach (var rutaGrafico in rutasGraficos)
                    {
                        col.Item().Element(elemento => elemento.Image(Image.FromFile(rutaGrafico)).FitWidth());
                    }
                });
            });
        })
        .GeneratePdf(ruta);
    }
}
