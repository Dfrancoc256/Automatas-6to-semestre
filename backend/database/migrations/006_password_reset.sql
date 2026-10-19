CREATE TABLE IF NOT EXISTS tokens_restablecimiento_password (
    id          uuid PRIMARY KEY,
    usuario_id  integer NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    token_hash  char(64) NOT NULL UNIQUE,
    expira_en   timestamptz NOT NULL,
    usado_en    timestamptz,
    creado_en   timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_tokens_restablecimiento_usuario_expira
    ON tokens_restablecimiento_password (usuario_id, expira_en);
