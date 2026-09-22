using System.Globalization;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VetSanJose.Shared.Reportes;

namespace VetSanJose.Application.Reportes;

// Maqueta del PDF de reportes con la identidad del sitio: franja celeste, huella, Inter/Baloo 2.
internal static class ReporteFinancieroPdf
{
    public record Datos(
        ReporteFinancieroDto Reporte,
        ProductoVendidoDto? MasVendido,
        ProductoVendidoDto? MenosVendido,
        decimal ValorInventario,
        int ProductosStockBajo,
        int UmbralStockBajo);

    // Paleta de wwwroot/css/tailwind.css (@theme).
    private const string Celeste = "#38BDF8";
    private const string CelesteIntenso = "#0284C7";
    private const string CelesteSuave = "#E0F2FE";
    private const string CelesteFila = "#F0F9FF";
    private const string CelesteBorde = "#BAE6FD";
    private const string Tinta = "#0F172A";
    private const string GrisTexto = "#475569";
    private const string GrisSuave = "#94A3B8";

    private const string FuenteCuerpo = "Inter";
    private const string FuenteTitulos = "Baloo 2";

    private const string Direccion = "Av. Principal 123, Zona Central, Santa Cruz, Bolivia";
    private const string Horario = "Lun a vie 8:00–19:00 · Sáb 9:00–14:00 · Dom cerrado";

    private static readonly CultureInfo Bolivia = CultureInfo.GetCultureInfo("es-BO");
    private static readonly TimeSpan OffsetBolivia = TimeSpan.FromHours(-4);

    private static string Huella(string color, double opacidad = 1) => $"""
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 48 48">
          <g fill="{color}" fill-opacity="{opacidad.ToString(CultureInfo.InvariantCulture)}">
            <ellipse cx="24" cy="30" rx="12" ry="10"/>
            <ellipse cx="9" cy="16" rx="5" ry="6.5"/>
            <ellipse cx="20" cy="9" rx="5" ry="6.5"/>
            <ellipse cx="28" cy="9" rx="5" ry="6.5"/>
            <ellipse cx="39" cy="16" rx="5" ry="6.5"/>
          </g>
        </svg>
        """;

    private static readonly Lazy<bool> FuentesRegistradas = new(() =>
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var ensamblado = typeof(ReporteFinancieroPdf).Assembly;
        foreach (var recurso in ensamblado.GetManifestResourceNames().Where(n => n.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)))
        {
            using var stream = ensamblado.GetManifestResourceStream(recurso)!;
            FontManager.RegisterFont(stream);
        }
        return true;
    });

    public static byte[] Generar(Datos datos)
    {
        _ = FuentesRegistradas.Value;
        var reporte = datos.Reporte;

        return Document.Create(container =>
        {
            container.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(0);
                pagina.PageColor(Colors.White);
                pagina.DefaultTextStyle(e => e.FontFamily(FuenteCuerpo).FontSize(9.5f).FontColor(GrisTexto));

                pagina.Header().Element(c => Encabezado(c, reporte));
                pagina.Content().PaddingHorizontal(36).PaddingTop(20).PaddingBottom(10).Column(columna =>
                {
                    columna.Spacing(18);
                    columna.Item().Element(c => Seccion(c, "Resumen del período", s => ResumenFinanciero(s, reporte)));
                    columna.Item().Element(c => Seccion(c, "Productos e inventario", s => ResumenProductos(s, datos)));
                    columna.Item().Element(c => Seccion(c, "Detalle día por día", s => TablaDiaria(s, reporte)));
                });
                pagina.Footer().Element(Pie);
            });
        })
        .WithMetadata(new DocumentMetadata
        {
            Title = $"Reporte financiero {reporte.Desde:dd/MM/yyyy} – {reporte.Hasta:dd/MM/yyyy}",
            Author = "Veterinaria San José",
        })
        .GeneratePdf();
    }

    private static void Encabezado(IContainer container, ReporteFinancieroDto reporte)
    {
        container.Column(columna =>
        {
            columna.Item().Background(CelesteIntenso).PaddingHorizontal(36).PaddingVertical(20).Row(fila =>
            {
                fila.ConstantItem(46).AlignMiddle().Height(46).Background(Colors.White).CornerRadius(23)
                    .Padding(9).Svg(Huella(CelesteIntenso));

                fila.RelativeItem().PaddingLeft(14).AlignMiddle().Column(c =>
                {
                    c.Item().Text("Veterinaria San José").FontFamily(FuenteTitulos).FontSize(20).Bold().FontColor(Colors.White).LineHeight(1);
                    c.Item().Text("Reporte financiero e inventario").FontSize(11).SemiBold().FontColor(CelesteSuave);
                });

                fila.AutoItem().AlignMiddle().Column(c =>
                {
                    c.Item().AlignRight().Text("PERÍODO").FontSize(7.5f).SemiBold().LetterSpacing(0.08f).FontColor(CelesteBorde);
                    c.Item().AlignRight().Background(Colors.White).CornerRadius(10).PaddingHorizontal(10).PaddingVertical(4)
                        .Text($"{reporte.Desde:dd/MM/yyyy} – {reporte.Hasta:dd/MM/yyyy}").FontSize(10).SemiBold().FontColor(CelesteIntenso);
                });
            });

            // Franja clara bajo el encabezado, como el borde del navbar del sitio.
            columna.Item().Height(4).Background(Celeste);
        });
    }

    private static void Seccion(IContainer container, string titulo, Action<IContainer> contenido)
    {
        container.Column(columna =>
        {
            columna.Item().PaddingBottom(8).Row(fila =>
            {
                fila.ConstantItem(12).Height(12).AlignMiddle().Svg(Huella(Celeste));
                fila.RelativeItem().PaddingLeft(6).Text(titulo).FontFamily(FuenteTitulos).FontSize(13).Bold().FontColor(Tinta);
            });
            columna.Item().Element(contenido);
        });
    }

    private static void ResumenFinanciero(IContainer container, ReporteFinancieroDto r)
    {
        container.Row(fila =>
        {
            fila.Spacing(10);
            fila.RelativeItem().Element(c => Tarjeta(c, "Ingresos del período", Bs(r.TotalIngresos), null));
            fila.RelativeItem().Element(c => Tarjeta(c, "Costos de insumos", Bs(r.TotalCostos), null));
            fila.RelativeItem().Element(c => Tarjeta(c, "Clientes atendidos", r.TotalClientesAtendidos.ToString(Bolivia), null));
            fila.RelativeItem().Element(c => Tarjeta(c, "Consultas", r.TotalConsultas.ToString(Bolivia), null));
        });
    }

    private static void ResumenProductos(IContainer container, Datos d)
    {
        container.Row(fila =>
        {
            fila.Spacing(10);
            fila.RelativeItem(1.2f).Element(c => Tarjeta(c, "Más vendido",
                d.MasVendido?.Nombre ?? "Sin ventas",
                d.MasVendido is null ? null : $"{d.MasVendido.CantidadVendida} u. · {Bs(d.MasVendido.MontoVendido)}",
                valorPequeno: true));
            fila.RelativeItem(1.2f).Element(c => Tarjeta(c, "Menos vendido",
                d.MenosVendido?.Nombre ?? "—",
                d.MenosVendido is null ? null : d.MenosVendido.CantidadVendida == 0
                    ? "Sin ventas en el período"
                    : $"{d.MenosVendido.CantidadVendida} u. · {Bs(d.MenosVendido.MontoVendido)}",
                valorPequeno: true));
            fila.RelativeItem(1.2f).Element(c => Tarjeta(c, "Valor del inventario", Bs(d.ValorInventario), "Precio × stock actual"));
            fila.RelativeItem(0.8f).Element(c => Tarjeta(c, "Stock bajo", d.ProductosStockBajo.ToString(Bolivia),
                $"Con ≤ {d.UmbralStockBajo} unidades"));
        });
    }

    private static void Tarjeta(IContainer container, string etiqueta, string valor, string? detalle, bool valorPequeno = false)
    {
        container.Background(CelesteFila).Border(1).BorderColor(CelesteBorde).CornerRadius(10)
            .PaddingVertical(10).PaddingHorizontal(12).Column(c =>
            {
                c.Item().Text(etiqueta.ToUpper(Bolivia)).FontSize(7).SemiBold().LetterSpacing(0.06f).FontColor(GrisSuave);
                c.Item().PaddingTop(3).Text(valor).FontFamily(valorPequeno ? FuenteCuerpo : FuenteTitulos)
                    .FontSize(valorPequeno ? 10.5f : 15).Bold().FontColor(CelesteIntenso).LineHeight(valorPequeno ? 1.15f : 1);
                if (detalle is not null)
                {
                    c.Item().PaddingTop(2).Text(detalle).FontSize(7.5f).FontColor(GrisTexto);
                }
            });
    }

    private static void TablaDiaria(IContainer container, ReporteFinancieroDto r)
    {
        container.Border(1).BorderColor(CelesteBorde).CornerRadius(8).Table(tabla =>
        {
            tabla.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.4f);
                c.RelativeColumn(1.3f);
                c.RelativeColumn(1.3f);
                c.RelativeColumn(1);
                c.RelativeColumn(1);
            });

            tabla.Header(h =>
            {
                CeldaTitulo(h.Cell(), "Fecha", derecha: false);
                CeldaTitulo(h.Cell(), "Ingresos (Bs)");
                CeldaTitulo(h.Cell(), "Costos (Bs)");
                CeldaTitulo(h.Cell(), "Clientes");
                CeldaTitulo(h.Cell(), "Consultas");

                static void CeldaTitulo(IContainer celda, string texto, bool derecha = true)
                {
                    var c = celda.Background(CelesteIntenso).PaddingVertical(7).PaddingHorizontal(10);
                    (derecha ? c.AlignRight() : c.AlignLeft()).Text(texto).FontSize(8.5f).SemiBold().FontColor(Colors.White);
                }
            });

            var indice = 0;
            foreach (var item in r.Items)
            {
                var fondo = indice++ % 2 == 0 ? "#FFFFFF" : CelesteFila;
                var sinMovimiento = item.Ingresos == 0 && item.Costos == 0 && item.Consultas == 0;
                var color = sinMovimiento ? GrisSuave : GrisTexto;

                Celda(item.Fecha.ToString("ddd dd/MM/yyyy", Bolivia), derecha: false);
                Celda(item.Ingresos.ToString("N2", Bolivia));
                Celda(item.Costos.ToString("N2", Bolivia));
                Celda(item.ClientesAtendidos.ToString(Bolivia));
                Celda(item.Consultas.ToString(Bolivia));

                void Celda(string texto, bool derecha = true)
                {
                    var c = tabla.Cell().Background(fondo).BorderBottom(0.5f).BorderColor(CelesteSuave)
                        .PaddingVertical(5).PaddingHorizontal(10);
                    (derecha ? c.AlignRight() : c.AlignLeft()).Text(texto).FontColor(color);
                }
            }

            Total("Total", derecha: false);
            Total(r.TotalIngresos.ToString("N2", Bolivia));
            Total(r.TotalCostos.ToString("N2", Bolivia));
            Total(r.TotalClientesAtendidos.ToString(Bolivia));
            Total(r.TotalConsultas.ToString(Bolivia));

            void Total(string texto, bool derecha = true)
            {
                var c = tabla.Cell().Background(CelesteSuave).PaddingVertical(7).PaddingHorizontal(10);
                (derecha ? c.AlignRight() : c.AlignLeft()).Text(texto).Bold().FontColor(CelesteIntenso);
            }
        });
    }

    private static void Pie(IContainer container)
    {
        var generado = DateTimeOffset.UtcNow.ToOffset(OffsetBolivia);

        container.PaddingHorizontal(36).PaddingBottom(18).Column(columna =>
        {
            // Hilera de huellitas como separador: decorativa, muy clara para no competir con los datos.
            columna.Item().PaddingBottom(6).Row(fila =>
            {
                fila.RelativeItem().AlignMiddle().LineHorizontal(0.75f).LineColor(CelesteBorde);
                for (var i = 0; i < 5; i++)
                {
                    fila.ConstantItem(12).PaddingHorizontal(2).TranslateY(i % 2 == 0 ? -2 : 2)
                        .Height(8).Svg(Huella(Celeste, 0.35));
                }
                fila.RelativeItem().AlignMiddle().LineHorizontal(0.75f).LineColor(CelesteBorde);
            });

            columna.Item().Row(fila =>
            {
                fila.RelativeItem().Column(c =>
                {
                    c.Item().Text(Direccion).FontSize(7.5f).FontColor(GrisSuave);
                    c.Item().Text(Horario).FontSize(7.5f).FontColor(GrisSuave);
                });
                fila.AutoItem().AlignBottom().Column(c =>
                {
                    c.Item().AlignRight().Text(t =>
                    {
                        t.DefaultTextStyle(e => e.FontSize(8).SemiBold().FontColor(CelesteIntenso));
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                    c.Item().AlignRight().Text($"Generado el {generado.ToString("dd/MM/yyyy HH:mm", Bolivia)}")
                        .FontSize(7).FontColor(GrisSuave);
                });
            });
        });
    }

    private static string Bs(decimal valor) => $"Bs {valor.ToString("N2", Bolivia)}";
}
