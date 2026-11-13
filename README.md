# TP Final — UDI 2: Infraestructura para Aplicaciones Backend

API REST en .NET 10 (Minimal APIs) + PostgreSQL 17, contenerizada con Docker Compose.
Basada en la plantilla de la cátedra.

## Base de datos (3 tablas asociadas)

- **Categorias** (Id, Name)
- **Proveedores** (Id, Name)
- **Items** (Id, Name, CreatedAt, CategoriaId → Categorias, ProveedorId → Proveedores)

Las tablas y los datos de prueba (seeding) se crean con `init.sql` al iniciar el contenedor de Postgres por primera vez.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/items` | Lista todos los ítems |
| GET | `/api/items/{id}` | Busca un ítem por Id |
| POST | `/api/items` | Agrega un ítem |
| PUT | `/api/items/{id}` | Modifica un ítem |
| DELETE | `/api/items/{id}` | Elimina un ítem |
| GET | `/api/categorias` | Lista las categorías |
| GET | `/api/categorias/{id}/items` | Ítems de una categoría (consulta filtrada) |
| GET | `/api/proveedores` | Lista los proveedores |
| GET | `/api/status` | Entorno y configuración |
| GET | `/api/kestrel-info` | Diagnóstico (solo puerto 8080) |

## Despliegue

1. Clonar el repositorio y entrar a la carpeta.
2. Crear el archivo `.env` (no se sube a Git):

```
PUERTO_API=5000
PUERTO_ADMIN=8080
POSTGRES_USER=admin
POSTGRES_PASSWORD=una_clave_segura
```

3. Levantar los contenedores:

```bash
docker compose up -d --build
docker ps
curl -i http://localhost:5000/api/items
```
