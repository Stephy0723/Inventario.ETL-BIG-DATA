using Inventario.ETL.Models;
using ScottPlot;

public sealed class ResumenInventario
{
    public int TotalProductos { get; init; }
    public int TotalCategorias { get; init; }
    public decimal ValorTotal { get; init; }
    public decimal PrecioPromedio { get; init; }
    public string CategoriaMayorValor { get; init; } = string.Empty;
}

public static class GeneradorGraficos
{
    public const string GraficoProductosPorCategoria = "grafico_productos_por_categoria.png";
    public const string GraficoValorPorCategoria = "grafico_valor_por_categoria.png";
    public const string GraficoProductosMasCaros = "grafico_productos_mas_caros.png";
    public const string GraficoPrecioPromedioCategoria = "grafico_precio_promedio_categoria.png";

    public static ResumenInventario CrearGraficosInventario(IReadOnlyCollection<DimProducto> productos)
    {
        var productosPorCategoria = productos
            .GroupBy(producto => producto.Categoria)
            .Select(grupo => (Etiqueta: grupo.Key, Valor: (double)grupo.Count()))
            .OrderByDescending(item => item.Valor)
            .ThenBy(item => item.Etiqueta)
            .ToList();

        var valorPorCategoria = productos
            .GroupBy(producto => producto.Categoria)
            .Select(grupo => (Etiqueta: grupo.Key, Valor: (double)grupo.Sum(producto => producto.Precio)))
            .OrderByDescending(item => item.Valor)
            .ThenBy(item => item.Etiqueta)
            .ToList();

        var productosMasCaros = productos
            .OrderByDescending(producto => producto.Precio)
            .ThenBy(producto => producto.Nombre)
            .Take(5)
            .Select(producto => (Etiqueta: RecortarEtiqueta(producto.Nombre, 24), Valor: (double)producto.Precio))
            .ToList();

        var precioPromedioPorCategoria = productos
            .GroupBy(producto => producto.Categoria)
            .Select(grupo => (Etiqueta: grupo.Key, Valor: (double)grupo.Average(producto => producto.Precio)))
            .OrderByDescending(item => item.Valor)
            .ThenBy(item => item.Etiqueta)
            .ToList();

        CrearGraficoBarras(
            GraficoProductosPorCategoria,
            "Productos por categoria",
            productosPorCategoria,
            "Cantidad de productos");

        CrearGraficoBarras(
            GraficoValorPorCategoria,
            "Valor total por categoria",
            valorPorCategoria,
            "Suma de precios");

        CrearGraficoBarras(
            GraficoProductosMasCaros,
            "Productos mas caros",
            productosMasCaros,
            "Precio",
            horizontal: true);

        CrearGraficoBarras(
            GraficoPrecioPromedioCategoria,
            "Precio promedio por categoria",
            precioPromedioPorCategoria,
            "Precio promedio");

        return new ResumenInventario
        {
            TotalProductos = productos.Count,
            TotalCategorias = productosPorCategoria.Count,
            ValorTotal = productos.Sum(producto => producto.Precio),
            PrecioPromedio = productos.Count == 0 ? 0 : productos.Average(producto => producto.Precio),
            CategoriaMayorValor = valorPorCategoria.FirstOrDefault().Etiqueta ?? "Sin categoria"
        };
    }

    private static void CrearGraficoBarras(
        string rutaSalida,
        string titulo,
        IReadOnlyList<(string Etiqueta, double Valor)> datos,
        string etiquetaEjeValores,
        bool horizontal = false)
    {
        var serie = datos.Count > 0
            ? datos
            : new (string Etiqueta, double Valor)[] { ("Sin datos", 0d) };

        Plot plot = new();
        ScottPlot.Palettes.Category10 palette = new();
        Bar[] barras = serie
            .Select((dato, indice) => new Bar
            {
                Position = indice + 1d,
                Value = dato.Valor,
                FillColor = palette.GetColor(indice)
            })
            .ToArray();

        var barPlot = plot.Add.Bars(barras);

        if (horizontal)
        {
            barPlot.Horizontal = true;
            plot.Axes.Left.SetTicks(
                barras.Select(barra => barra.Position).ToArray(),
                serie.Select(item => item.Etiqueta).ToArray());
            plot.Axes.Left.MajorTickStyle.Length = 0;
            plot.Axes.Left.MinimumSize = 140;
            plot.Axes.Margins(left: 0, right: 0.05);
            plot.XLabel(etiquetaEjeValores);
        }
        else
        {
            plot.Axes.Bottom.SetTicks(
                barras.Select(barra => barra.Position).ToArray(),
                serie.Select(item => item.Etiqueta).ToArray());
            plot.Axes.Bottom.MajorTickStyle.Length = 0;
            plot.Axes.Bottom.TickLabelStyle.Rotation = 20;
            plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            plot.Axes.Bottom.MinimumSize = 70;
            plot.Axes.Margins(bottom: 0, top: 0.1);
            plot.YLabel(etiquetaEjeValores);
        }

        plot.HideGrid();
        plot.Title(titulo);
        plot.SavePng(rutaSalida, 900, 450);
    }

    private static string RecortarEtiqueta(string texto, int longitudMaxima)
    {
        if (texto.Length <= longitudMaxima)
            return texto;

        return $"{texto[..(longitudMaxima - 3)]}...";
    }
}
