using BL.Interfaces.Repositories;
using BL.Models.MenuItem;
using DAL.Infrastructure;
using Dapper;
using Npgsql;

namespace DAL.Repositories;

public sealed class MenuItemRepository : IMenuItemRepository
{
    private readonly IDbSession _session;

    public MenuItemRepository(IDbSession session)
    {
        _session = session;
    }

    public async Task<IReadOnlyList<MenuItemListDto>> ListarAsync()
    {
        const string sql = @"
            SELECT
                id_menu_item   AS IdMenuItem,
                etiqueta       AS Etiqueta,
                url_destino    AS UrlDestino,
                orden          AS Orden,
                COALESCE(activo, 1) AS Activo
            FROM public.menu_item
            WHERE COALESCE(activo, 1) = 1
            ORDER BY orden, id_menu_item";

        var conn = _session.Connection;
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        var result = await conn.QueryAsync<MenuItemListDto>(sql);
        return result.AsList().AsReadOnly();
    }

    public async Task<MenuItemDetailDto?> ObtenerAsync(int idMenuItem)
    {
        const string sql = @"
            SELECT
                id_menu_item   AS IdMenuItem,
                etiqueta       AS Etiqueta,
                url_destino    AS UrlDestino,
                orden          AS Orden,
                COALESCE(activo, 1) AS Activo
            FROM public.menu_item
            WHERE id_menu_item = @IdMenuItem
              AND COALESCE(activo, 1) = 1";

        var conn = _session.Connection;
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        return await conn.QueryFirstOrDefaultAsync<MenuItemDetailDto>(sql, new { IdMenuItem = idMenuItem });
    }

    public async Task<bool> ExisteEtiquetaAsync(string etiqueta, int? idMenuItem)
    {
        const string sql = @"
            SELECT 1
            FROM public.menu_item
            WHERE COALESCE(activo, 1) = 1
              AND LOWER(TRIM(etiqueta)) = LOWER(TRIM(@Etiqueta))
              AND (@IdMenuItem IS NULL OR id_menu_item <> @IdMenuItem)
            LIMIT 1";

        var conn = _session.Connection;
        if (conn.State == System.Data.ConnectionState.Closed)
            conn.Open();

        var valor = await conn.ExecuteScalarAsync<int?>(sql, new { Etiqueta = etiqueta, IdMenuItem = idMenuItem });
        return valor.HasValue;
    }

    public async Task<int> GuardarAsync(MenuItemSaveRequest request)
    {
        var startedLocalTransaction = false;

        try
        {
            if (_session.Transaction is null)
            {
                _session.BeginTransaction();
                startedLocalTransaction = true;
            }

            var tr = _session.Transaction;

            int id;

            if (request.IdMenuItem == 0)
            {
                const string sql = @"
                    INSERT INTO public.menu_item
                        (etiqueta, url_destino, orden, activo, creado_por)
                    VALUES
                        (@Etiqueta, @UrlDestino, @Orden, 1, @UsuarioAccion)
                    RETURNING id_menu_item";

                id = await _session.Connection.ExecuteScalarAsync<int>(sql, request, tr);
            }
            else
            {
                const string sql = @"
                    UPDATE public.menu_item
                    SET etiqueta            = @Etiqueta,
                        url_destino         = @UrlDestino,
                        orden               = @Orden,
                        fecha_modificacion  = NOW(),
                        modificado_por      = @UsuarioAccion
                    WHERE id_menu_item = @IdMenuItem
                      AND COALESCE(activo, 1) = 1";

                var rows = await _session.Connection.ExecuteAsync(sql, request, tr);
                id = rows > 0 ? request.IdMenuItem : 0;
            }

            if (startedLocalTransaction) _session.Commit();
            return id;
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            if (startedLocalTransaction) _session.Rollback();
            throw new InvalidOperationException("Ya existe un item de menu con esa etiqueta.");
        }
        catch
        {
            if (startedLocalTransaction) _session.Rollback();
            throw;
        }
    }

    public async Task<MenuItemDeleteResult> EliminarAsync(int idMenuItem, int usuarioAccion)
    {
        var startedLocalTransaction = false;

        try
        {
            if (_session.Transaction is null)
            {
                _session.BeginTransaction();
                startedLocalTransaction = true;
            }

            var tr = _session.Transaction;

            const string sql = @"
                UPDATE public.menu_item
                SET activo              = 0,
                    fecha_modificacion  = NOW(),
                    modificado_por      = @UsuarioAccion
                WHERE id_menu_item = @IdMenuItem
                  AND COALESCE(activo, 1) = 1";

            var rows = await _session.Connection.ExecuteAsync(sql,
                new { IdMenuItem = idMenuItem, UsuarioAccion = usuarioAccion }, tr);

            if (startedLocalTransaction) _session.Commit();
            return new MenuItemDeleteResult { RowsAffected = rows };
        }
        catch
        {
            if (startedLocalTransaction) _session.Rollback();
            throw;
        }
    }
}
