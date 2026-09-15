namespace VetSanJose.Domain.Common;

public static class Roles
{
    public const string Cliente = "cliente";
    public const string Doctor = "doctor";
    public const string Secretaria = "secretaria";
    public const string Administrador = "administrador";

    public static readonly string[] Todos = [Cliente, Doctor, Secretaria, Administrador];
}

public static class EstadosCita
{
    public const string Pendiente = "pendiente";
    public const string Confirmada = "confirmada";
    public const string Completada = "completada";
    public const string Cancelada = "cancelada";
}

public static class EstadosVenta
{
    public const string Pendiente = "pendiente";
    public const string Completada = "completada";
    public const string Cancelada = "cancelada";
}

public static class InventarioConfig
{
    public const int UmbralStockBajoPorDefecto = 5;
}

public static class TiposProducto
{
    public const string VentaPublico = "venta_publico";
    public const string InsumoInterno = "insumo_interno";

    public static readonly string[] Todos = [VentaPublico, InsumoInterno];
}
