using System.Globalization;
using System.Text;
using VetSanJose.Shared.Productos;

namespace VetSanJose.Client.Services;

// Búsqueda flexible por palabras clave para la tienda: "comida para perro" no se busca como frase
// exacta sino como las palabras "comida" y "perro" contra nombre, descripción y categoría.
public static class BusquedaProductos
{
    private static readonly HashSet<string> PalabrasVacias =
    [
        "el", "la", "los", "las", "un", "una", "unos", "unas", "de", "del", "al", "a", "en", "y", "o",
        "para", "por", "con", "sin", "mi", "mis", "que", "algo", "algun", "alguna", "tipo",
    ];

    // Términos que el cliente dice de una forma y el catálogo escribe de otra.
    private static readonly Dictionary<string, string[]> Sinonimos = new()
    {
        ["comida"] = ["alimento", "croqueta", "balanceado"],
        ["alimento"] = ["comida", "croqueta", "balanceado"],
        ["perro"] = ["canino", "cachorro", "dog"],
        ["gato"] = ["felino", "gatito", "cat"],
        ["shampoo"] = ["champu", "champoo"],
        ["champu"] = ["shampoo"],
        ["remedio"] = ["medicamento", "antiparasitario"],
        ["medicina"] = ["medicamento"],
        ["juguete"] = ["pelota", "mordedor"],
        ["collar"] = ["correa", "arnes"],
    };

    public static List<string> ExtraerPalabrasClave(string? texto) =>
        Normalizar(texto)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length > 1 && !PalabrasVacias.Contains(p))
            .Distinct()
            .ToList();

    public static List<ProductoTiendaDto> Filtrar(IEnumerable<ProductoTiendaDto> productos, string? consulta)
    {
        var claves = ExtraerPalabrasClave(consulta);
        if (claves.Count == 0)
        {
            return productos.ToList();
        }

        var puntuados = productos
            .Select(p => (Producto: p, Aciertos: claves.Count(c => Coincide(TextoIndexable(p), c))))
            .Where(x => x.Aciertos > 0)
            .ToList();

        // Si algún producto cumple todas las palabras nos quedamos solo con esos; si no, mostramos
        // los que más se acercan en vez de una pantalla vacía.
        var maximo = puntuados.Count == 0 ? 0 : puntuados.Max(x => x.Aciertos);
        return puntuados
            .Where(x => x.Aciertos == maximo)
            .Select(x => x.Producto)
            .ToList();
    }

    private static string TextoIndexable(ProductoTiendaDto p) =>
        Normalizar($"{p.Nombre} {p.Descripcion} {p.CategoriaNombre}");

    private static bool Coincide(string texto, string clave)
    {
        foreach (var variante in Variantes(clave))
        {
            if (texto.Contains(variante, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    private static IEnumerable<string> Variantes(string clave)
    {
        var raiz = Singular(clave);
        yield return raiz;
        if (Sinonimos.TryGetValue(raiz, out var sinonimos))
        {
            foreach (var s in sinonimos) yield return s;
        }
    }

    // "perros" → "perro", "gatos" → "gato"; basta para que el Contains encuentre ambas formas.
    private static string Singular(string palabra) =>
        palabra.Length > 4 && palabra.EndsWith("es") && !palabra.EndsWith("ches") ? palabra[..^2]
        : palabra.Length > 3 && palabra.EndsWith('s') ? palabra[..^1]
        : palabra;

    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";

        var descompuesto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
