using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Encabezados reenviados para trabajar con Nginx (Práctica 8)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configuración de Kestrel: puerto público y puerto administrativo (Práctica 2)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(8080);
});

// CORS: permite que un frontend consuma la API (Clase 9)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Configuración de Entity Framework para PostgreSQL / Supabase
// La cadena de conexión se lee de la variable de entorno ConnectionStrings__DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors();
app.UseDefaultFiles();  // Frontend: wwwroot/index.html
app.UseStaticFiles();

// Creación automática del esquema en la base de datos si no existe (Práctica 7)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Middleware: /api/kestrel-info solo por el puerto administrativo 8080 (Práctica 2)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;

    if (!string.IsNullOrEmpty(path) &&
        path.Equals("/api/kestrel-info", StringComparison.OrdinalIgnoreCase))
    {
        if (context.Connection.LocalPort != 8080)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync(
                "Acceso denegado. Endpoint accesible por puerto administrativo 8080.");
            return;
        }
    }

    await next();
});

// Endpoint de Ciclo de Vida y Entornos (Práctica 1)
app.MapGet("/api/status", (IConfiguration config, IHostEnvironment environment) =>
{
    var response = new
    {
        AppName = config["AppSettings:AppName"],
        Environment = environment.EnvironmentName,
        EnableDetailedLogs = config.GetValue<bool>("AppSettings:FeatureFlag_EnableDetailedLogs"),
        DatabaseTimeout = config["DATABASE_TIMEOUT"] ?? "No definida"
    };
    return Results.Ok(response);
});

// Endpoint de diagnóstico de Kestrel (Práctica 2)
app.MapGet("/api/kestrel-info", () =>
{
    ThreadPool.GetAvailableThreads(out int workerAvailable, out int completionAvailable);
    ThreadPool.GetMaxThreads(out int workerMax, out int completionMax);

    using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
    double workinSetMb = Math.Round(currentProcess.WorkingSet64 / (1024.0 * 1024.0), 2);

    var response = new
    {
        WorkerThreads = new { Available = workerAvailable, Max = workerMax },
        CompletionPortThreads = new { Available = completionAvailable, Max = completionMax },
        Memory = new { WorkingSetMB = workinSetMb },
        System = new { LogicalProcessors = Environment.ProcessorCount }
    };
    return Results.Ok(response);
});

// ==========================================
// ENDPOINTS QUE INTERACTÚAN CON LA BASE DE DATOS
// ==========================================

// 1. GET: listar todos los ítems
app.MapGet("/api/items", async (AppDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});

// 2. GET: buscar un ítem por Id
app.MapGet("/api/items/{id}", async (int id, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound("El item no existe.");
    }
    return Results.Ok(item);
});

// 3. POST: agregar un nuevo ítem
app.MapPost("/api/items", async ([FromBody] Item item, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(item.Name))
    {
        return Results.BadRequest("El nombre del item no puede estar vacío.");
    }
    item.CreatedAt = DateTime.UtcNow;
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{item.Id}", item);
});

// 4. PUT: modificar un ítem existente
app.MapPut("/api/items/{id}", async (int id, [FromBody] Item datos, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound("El item no existe.");
    }
    item.Name = datos.Name;
    item.CategoriaId = datos.CategoriaId;
    item.ProveedorId = datos.ProveedorId;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// 5. DELETE: eliminar un ítem
app.MapDelete("/api/items/{id}", async (int id, AppDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound("El item no existe.");
    }
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok("Item eliminado.");
});

// 6. GET: listar todas las categorías
app.MapGet("/api/categorias", async (AppDbContext db) =>
{
    var categorias = await db.Categorias.ToListAsync();
    return Results.Ok(categorias);
});

// 7. GET: consulta filtrada, ítems de una categoría
app.MapGet("/api/categorias/{id}/items", async (int id, AppDbContext db) =>
{
    var items = await db.Items.Where(i => i.CategoriaId == id).ToListAsync();
    return Results.Ok(items);
});

// 8. GET: listar todos los proveedores
app.MapGet("/api/proveedores", async (AppDbContext db) =>
{
    var proveedores = await db.Proveedores.ToListAsync();
    return Results.Ok(proveedores);
});

app.Run();
