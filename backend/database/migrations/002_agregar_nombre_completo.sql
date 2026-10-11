-- Agrega el nombre legal mostrado en la credencial sin alterar el nickname de acceso.
-- Ejecutar una sola vez en PostgreSQL antes de publicar el backend que incluye este campo.

BEGIN;

ALTER TABLE usuarios
    ADD COLUMN IF NOT EXISTS nombre_completo varchar(161);

-- Conserva los registros ya existentes usando el nickname como valor temporal.
UPDATE usuarios
SET nombre_completo = nickname
WHERE nombre_completo IS NULL OR btrim(nombre_completo) = '';

ALTER TABLE usuarios
    ALTER COLUMN nombre_completo SET NOT NULL;

ALTER TABLE usuarios
    DROP CONSTRAINT IF EXISTS ck_usuarios_nombre_completo;

ALTER TABLE usuarios
    ADD CONSTRAINT ck_usuarios_nombre_completo
    CHECK (char_length(btrim(nombre_completo)) BETWEEN 3 AND 161);

COMMIT;
