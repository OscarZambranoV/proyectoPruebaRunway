// menuItem.js - CRUD de items del menu principal
// Patron basado en Generic.js

var modalMenuItem = null;
var listaMenuCache = [];

window.onload = function () {
    var elModal = document.getElementById('modalMenuItem');
    if (elModal) {
        modalMenuItem = new bootstrap.Modal(elModal);
    }

    registrarResetModal('modalMenuItem', 'formMenuItem', function () {
        set('hdfIdMenuItem', '0');
        document.getElementById('modalMenuItemLabel').textContent = 'Nuevo Item';
    });

    listarMenuItems();
};

// ─── Listar ──────────────────────────────────────────────────────────────────

function listarMenuItems() {
    var base = get('hdfOculto') ? get('hdfOculto').value : '/';
    var url  = base.replace(/\/$/, '') + '/MenuItem/Listar';

    fetchGetJsonSinLoading(url, function (datos) {
        listaMenuCache = datos;
        pintarTablaMenuItems(datos);
    });
}

// ─── Pintar tabla ─────────────────────────────────────────────────────────────

function pintarTablaMenuItems(datos) {
    generarTabla({
        idContenedor: 'divTablaMenuItems',
        datos: datos,
        columnas: [
            { titulo: 'Etiqueta',   campo: 'etiqueta' },
            { titulo: 'URL',        campo: 'urlDestino' },
            { titulo: 'Orden',      campo: 'orden' }
        ],
        acciones: function (fila) {
            return '<button class="btn btn-sm btn-warning me-1" onclick="Editar(' + fila.idMenuItem + ')">'
                 + '<i class="bi bi-pencil-fill"></i></button>'
                 + '<button class="btn btn-sm btn-danger" onclick="Eliminar(' + fila.idMenuItem + ')">'
                 + '<i class="bi bi-trash-fill"></i></button>';
        }
    });
}

// ─── Abrir modal nuevo ───────────────────────────────────────────────────────

function abrirModalNuevo() {
    limpiarFormulario('formMenuItem');
    set('hdfIdMenuItem', '0');
    set('txtOrden', '0');
    document.getElementById('modalMenuItemLabel').textContent = 'Nuevo Item';
    if (modalMenuItem) modalMenuItem.show();
}

// ─── Editar ──────────────────────────────────────────────────────────────────

function Editar(id) {
    var fila = listaMenuCache.find(function (x) { return x.idMenuItem === id; });
    if (!fila) {
        mostrarError('No se encontro el item en cache. Refresque la pagina.');
        return;
    }

    set('hdfIdMenuItem', fila.idMenuItem);
    set('txtEtiqueta',   fila.etiqueta);
    set('txtUrlDestino', fila.urlDestino);
    set('txtOrden',      fila.orden);
    document.getElementById('modalMenuItemLabel').textContent = 'Editar Item';
    if (modalMenuItem) modalMenuItem.show();
}

// ─── Guardar ─────────────────────────────────────────────────────────────────

function guardarMenuItem() {
    var etiqueta   = get('txtEtiqueta') ? get('txtEtiqueta').value.trim() : '';
    var urlDestino = get('txtUrlDestino') ? get('txtUrlDestino').value.trim() : '';
    var orden      = get('txtOrden') ? parseInt(get('txtOrden').value) || 0 : 0;
    var idMenuItem = get('hdfIdMenuItem') ? parseInt(get('hdfIdMenuItem').value) || 0 : 0;

    if (!validarRequerido(etiqueta)) {
        mostrarError('La etiqueta es obligatoria.');
        return;
    }
    if (!validarRequerido(urlDestino)) {
        mostrarError('La URL destino es obligatoria.');
        return;
    }

    var base = get('hdfOculto') ? get('hdfOculto').value : '/';
    var url  = base.replace(/\/$/, '') + '/MenuItem/Guardar';

    var payload = {
        idMenuItem: idMenuItem,
        etiqueta:   etiqueta,
        urlDestino: urlDestino,
        orden:      orden
    };

    fetchPostJson(url, payload, function (resp) {
        if (resp.success) {
            if (modalMenuItem) modalMenuItem.hide();
            mostrarExito('Item guardado correctamente.');
            listarMenuItems();
        } else {
            mostrarError(resp.message || 'No se pudo guardar el item.');
        }
    });
}

// ─── Eliminar ────────────────────────────────────────────────────────────────

function Eliminar(id) {
    confirmar('Esta accion no se puede deshacer.', 'Eliminar item de menu?', function () {
        var base = get('hdfOculto') ? get('hdfOculto').value : '/';
        var url  = base.replace(/\/$/, '') + '/MenuItem/Eliminar';

        fetchPostJson(url, id, function (resp) {
            if (resp.success) {
                mostrarExito('Item eliminado correctamente.');
                listarMenuItems();
            } else {
                mostrarError(resp.message || 'No se pudo eliminar el item.');
            }
        });
    });
}
