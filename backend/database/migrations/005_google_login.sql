ALTER TABLE usuarios
    ADD COLUMN IF NOT EXISTS google_subject varchar(100);

CREATE UNIQUE INDEX IF NOT EXISTS ux_usuarios_google_subject
    ON usuarios (google_subject)
    WHERE google_subject IS NOT NULL;

-- PostgreSQL convierte a minúsculas los nombres que no se crean entre comillas.
-- Se cubren ambas variantes para instalaciones anteriores.
ALTER TABLE bitacora_login DROP CONSTRAINT IF EXISTS ck_bitacora_metodo;
ALTER TABLE bitacora_login DROP CONSTRAINT IF EXISTS "CK_bitacora_metodo";
ALTER TABLE bitacora_login
    ADD CONSTRAINT ck_bitacora_metodo
    CHECK (metodo IN ('password', 'facial', 'qr', 'google'));
