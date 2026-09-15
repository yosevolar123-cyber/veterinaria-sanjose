using VetSanJose.Shared.Productos;

namespace VetSanJose.Client.Services;

public record CartItem(long ProductoId, string Nombre, decimal Precio, string? ImagenUrl, int Cantidad);

public class CartService
{
    private readonly List<CartItem> _items = [];

    public IReadOnlyList<CartItem> Items => _items;

    public int CantidadTotal => _items.Sum(i => i.Cantidad);

    public decimal Total => _items.Sum(i => i.Precio * i.Cantidad);

    public event Action? OnChange;

    public void Agregar(ProductoTiendaDto producto, int cantidad = 1)
    {
        var existente = _items.FirstOrDefault(i => i.ProductoId == producto.Id);
        if (existente is not null)
        {
            _items[_items.IndexOf(existente)] = existente with { Cantidad = existente.Cantidad + cantidad };
        }
        else
        {
            _items.Add(new CartItem(producto.Id, producto.Nombre, producto.Precio, producto.ImagenUrl, cantidad));
        }

        OnChange?.Invoke();
    }

    public void Quitar(long productoId)
    {
        _items.RemoveAll(i => i.ProductoId == productoId);
        OnChange?.Invoke();
    }

    public void Vaciar()
    {
        _items.Clear();
        OnChange?.Invoke();
    }
}
