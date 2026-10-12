-- Separa los datos personales sin perder los usuarios existentes.
-- Es independiente: puede ejecutarse aunque nombre_completo no exista.

BEGIN;

ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS nombre varchar(80);
ALTER TABLE usuarios ADD COLUMN IF NOT EXISTS apellido varchar(80);

-- Los registros antiguos solo tenían nickname; se conserva como nombre
-- temporal y se marca un apellido neutro para no perder ningún usuario.
UPDATE usuarios
SET
    nombre = COALESCE(
        NULLIF(btrim(nombre), ''),
        NULLIF(split_part(nickname, '.', 1), ''),
        'Usuario'
    ),
    apellido = COALESCE(
        NULLIF(btrim(apellido), ''),
        NULLIF(btrim(substring(nickname from position('.' in nickname) + 1)), ''),
        'Registrado'
    );

ALTER TABLE usuarios ALTER COLUMN nombre SET NOT NULL;
ALTER TABLE usuarios ALTER COLUMN apellido SET NOT NULL;

ALTER TABLE usuarios DROP CONSTRAINT IF EXISTS ck_usuarios_nombre;
ALTER TABLE usuarios DROP CONSTRAINT IF EXISTS ck_usuarios_apellido;
ALTER TABLE usuarios ADD CONSTRAINT ck_usuarios_nombre CHECK (char_length(btrim(nombre)) BETWEEN 2 AND 80);
ALTER TABLE usuarios ADD CONSTRAINT ck_usuarios_apellido CHECK (char_length(btrim(apellido)) BETWEEN 2 AND 80);

COMMIT;
