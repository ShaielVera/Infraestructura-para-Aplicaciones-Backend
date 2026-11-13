-- init.sql
CREATE TABLE IF NOT EXISTS "Categorias" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL
);

CREATE TABLE IF NOT EXISTS "Proveedores" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL
);

CREATE TABLE IF NOT EXISTS "Items" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    "CategoriaId" INT NOT NULL REFERENCES "Categorias"("Id"),
    "ProveedorId" INT NOT NULL REFERENCES "Proveedores"("Id")
);

-- Inserción de datos iniciales de prueba (Seeding)
INSERT INTO "Categorias" ("Name") VALUES
('Procesadores'),
('Memorias'),
('Almacenamiento');

INSERT INTO "Proveedores" ("Name") VALUES
('Compu Rafaela'),
('Tecno Litoral');

INSERT INTO "Items" ("Name", "CreatedAt", "CategoriaId", "ProveedorId") VALUES
('Procesador', NOW(), 1, 1),
('Memoria RAM', NOW(), 2, 2),
('Disco rígido HDD', NOW(), 3, 1),
('Volumen Nombrado pgdata_vol', NOW(), 3, 2);
