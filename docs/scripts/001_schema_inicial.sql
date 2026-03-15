-- =============================================================================
-- 001_schema_inicial.sql
-- Esquema inicial para LandingAdmin
-- Ejecutar en Railway via consola psql o cliente SQL
-- =============================================================================

CREATE TABLE IF NOT EXISTS public.menu_item (
    id_menu_item        SERIAL PRIMARY KEY,
    etiqueta            VARCHAR(100) NOT NULL,
    url_destino         VARCHAR(300) NOT NULL,
    orden               INTEGER NOT NULL DEFAULT 0,
    activo              SMALLINT NOT NULL DEFAULT 1,
    fecha_creacion      TIMESTAMP NOT NULL DEFAULT NOW(),
    fecha_modificacion  TIMESTAMP,
    creado_por          INTEGER,
    modificado_por      INTEGER,
    CONSTRAINT chk_menu_item_activo CHECK (activo IN (0, 1))
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_menu_item_etiqueta
    ON public.menu_item (LOWER(TRIM(etiqueta)))
    WHERE activo = 1;

INSERT INTO public.menu_item (etiqueta, url_destino, orden, activo, creado_por)
VALUES
    ('Inicio',    '#inicio',    1, 1, 0),
    ('Nosotros',  '#nosotros',  2, 1, 0),
    ('Servicios', '#servicios', 3, 1, 0),
    ('Contacto',  '#contacto',  4, 1, 0)
ON CONFLICT DO NOTHING;
