using Microsoft.EntityFrameworkCore;

// Tabla Categorias
public class Categoria
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Tabla Proveedores
public class Proveedor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Tabla Items (asociada a Categorias y Proveedores)
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CategoriaId { get; set; }   // clave foránea -> Categorias
    public int ProveedorId { get; set; }   // clave foránea -> Proveedores
}

// Contexto de Base de Datos
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
}
