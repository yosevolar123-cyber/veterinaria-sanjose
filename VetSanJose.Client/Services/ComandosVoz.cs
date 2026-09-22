namespace VetSanJose.Client.Services;

public enum TipoComandoVoz { Navegar, Buscar, Detener, Ayuda, NoPermitido, NoEntendido }

public record ComandoVoz(TipoComandoVoz Tipo, string? Ruta = null, string? Busqueda = null, string? Mensaje = null);

// Traduce una frase reconocida por el micrófono a una acción. Es puro (sin JS ni navegación) para
// que el asistente del layout solo tenga que ejecutar lo que se decide acá.
public static class ComandosVoz
{
    private record Destino(string Ruta, string Nombre, string[] Frases, string[]? Roles = null);

    // El orden importa: las frases más específicas ("mis mascotas") van antes que las genéricas.
    private static readonly Destino[] Destinos =
    [
        new("mi-cuenta/mascotas", "mis mascotas", ["mis mascotas", "mascotas"], [Domain.Common.Roles.Cliente]),
        new("mi-cuenta/citas", "mis citas", ["mis citas", "citas", "turnos"], [Domain.Common.Roles.Cliente]),
        new("mi-cuenta/compras", "mis compras", ["mis compras", "compras", "pedidos"], [Domain.Common.Roles.Cliente]),
        new("checkout", "el pago", ["pagar", "checkout", "caja"], [Domain.Common.Roles.Cliente]),
        new("mi-cuenta", "mi cuenta", ["mi cuenta", "mi perfil", "cuenta"], [Domain.Common.Roles.Cliente]),
        new("panel-doctor", "el panel del doctor", ["panel doctor", "panel del doctor", "panel de doctor", "panel medico"], [Domain.Common.Roles.Doctor]),
        new("panel-secretaria", "el panel de secretaría", ["panel secretaria", "panel de secretaria", "panel de la secretaria"], [Domain.Common.Roles.Secretaria]),
        new("panel-admin", "el panel de administración", ["panel admin", "panel de admin", "panel administrador", "panel de administracion", "panel del administrador", "administracion"], [Domain.Common.Roles.Administrador]),
        new("contacto", "contacto", ["contacto", "contactos", "contactarlos"]),
        new("tienda", "la tienda", ["tienda", "productos", "catalogo"]),
        new("", "inicio", ["inicio", "home", "pagina principal", "principal", "portada"]),
    ];

    private static readonly string[] VerbosNavegar =
    [
        "llevame a", "llevame al", "lleva me a", "llevarme a", "quiero ir a", "quiero ir al", "ir a", "ir al",
        "ve a", "ve al", "vamos a", "vamos al", "navega a", "navegar a", "abre", "abrir", "abreme",
        "ensename", "muestrame", "mostrar", "quiero ver", "ver",
    ];

    private static readonly string[] VerbosBuscar =
    [
        "muestrame", "ensename", "mostrar", "quiero ver", "quiero comprar", "quiero", "necesito", "busca", "buscar",
        "buscame", "encuentra", "tienes", "tienen", "hay", "venden", "ver",
    ];

    private static readonly string[] Relleno =
    [
        "por favor", "porfa", "oye", "hola", "asistente", "puedes", "podes", "podrias", "me", "la pagina de",
        "la seccion de", "pagina de", "seccion de", "la pagina", "el", "la", "los", "las", "a", "al", "de", "en",
        "para", "con", "algo de", "algun", "alguna",
    ];

    public static ComandoVoz Interpretar(string? frase, string? rol)
    {
        var texto = BusquedaProductos.Normalizar(frase);
        if (texto.Length == 0) return NoEntendido();

        if (Contiene(texto, "deja de escuchar", "para de escuchar", "detente", "apagate", "cancelar", "silencio"))
            return new(TipoComandoVoz.Detener, Mensaje: "Listo, dejo de escuchar.");

        if (Contiene(texto, "ayuda", "que puedo decir", "comandos"))
            return new(TipoComandoVoz.Ayuda, Mensaje: "Probá: \"llévame a la tienda\", \"muéstrame comida para perro\" o \"llévame a mis citas\".");

        foreach (var destino in Destinos)
        {
            var frase_ = destino.Frases.FirstOrDefault(f => ContienePalabras(texto, f));
            if (frase_ is null) continue;

            var resto = Limpiar(Quitar(texto, frase_));

            // "quiero ver productos para gato" → no es solo ir a la tienda, es buscar "gato" en ella.
            if (destino.Ruta == "tienda" && resto.Length > 0)
                return Buscar(resto);

            // Si sobra algo más, la palabra de destino es parte de otra cosa: "muéstrame shampoo
            // para mascotas" es una búsqueda, no un pedido de ir a Mis mascotas.
            if (resto.Length > 0) continue;

            if (destino.Roles is not null && (rol is null || !destino.Roles.Contains(rol)))
            {
                return new(TipoComandoVoz.NoPermitido, Mensaje: rol is null
                    ? $"Para ir {A(destino.Nombre)} primero iniciá sesión."
                    : $"Tu cuenta no tiene acceso {A(destino.Nombre)}.");
            }

            return new(TipoComandoVoz.Navegar, Ruta: destino.Ruta, Mensaje: $"Te llevo {A(destino.Nombre)}.");
        }

        var verbo = VerbosBuscar.FirstOrDefault(v => EmpiezaCon(texto, v));
        if (verbo is not null)
        {
            var terminos = Limpiar(texto[verbo.Length..]);
            if (BusquedaProductos.ExtraerPalabrasClave(terminos).Count > 0)
                return Buscar(terminos);
        }

        return NoEntendido();
    }

    private static ComandoVoz Buscar(string terminos) =>
        new(TipoComandoVoz.Buscar, Ruta: "tienda", Busqueda: terminos, Mensaje: $"Buscando \"{terminos}\" en la tienda.");

    // "a" + "el panel" se contrae a "al panel".
    private static string A(string nombre) => nombre.StartsWith("el ") ? $"al {nombre[3..]}" : $"a {nombre}";

    private static ComandoVoz NoEntendido() =>
        new(TipoComandoVoz.NoEntendido, Mensaje: "No entendí el comando, ¿podés repetirlo?");

    // Quita relleno y verbos del principio para quedarnos con lo que el usuario realmente pidió.
    private static string Limpiar(string texto)
    {
        var palabras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        bool cambio;
        do
        {
            cambio = false;
            var actual = string.Join(' ', palabras);
            foreach (var prefijo in VerbosNavegar.Concat(VerbosBuscar).Concat(Relleno).OrderByDescending(p => p.Length))
            {
                if (EmpiezaCon(actual, prefijo))
                {
                    palabras.RemoveRange(0, prefijo.Split(' ').Length);
                    cambio = true;
                    break;
                }
            }
        } while (cambio && palabras.Count > 0);

        return string.Join(' ', palabras);
    }

    private static string Quitar(string texto, string frase) =>
        string.Join(' ', $" {texto} ".Replace($" {frase} ", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static bool EmpiezaCon(string texto, string prefijo) =>
        texto == prefijo || texto.StartsWith(prefijo + " ", StringComparison.Ordinal);

    private static bool ContienePalabras(string texto, string frase) =>
        $" {texto} ".Contains($" {frase} ", StringComparison.Ordinal);

    private static bool Contiene(string texto, params string[] frases) =>
        frases.Any(f => ContienePalabras(texto, f));
}
