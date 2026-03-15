// Generic.js - Utilidades generales para LandingAdmin
// Patron: fetchGetJsonSinLoading, fetchPostJson, generarTabla, mostrarExito/Error, confirmar

// ─── Helpers de DOM ─────────────────────────────────────────────────────────

function get(id) {
    return document.getElementById(id);
}

function set(id, valor) {
    var el = get(id);
    if (el) el.value = valor;
}

function setHTML(id, html) {
    var el = get(id);
    if (el) el.innerHTML = html;
}

// ─── CSRF ────────────────────────────────────────────────────────────────────

function obtenerTokenCsrf() {
    var match = document.cookie.match(/(^|;)\s*XSRF-TOKEN=([^;]+)/);
    return match ? decodeURIComponent(match[2]) : '';
}

function obtenerHeadersConCsrf() {
    return {
        'Content-Type': 'application/json',
        'X-CSRF-TOKEN': obtenerTokenCsrf()
    };
}

// ─── Fetch GET ───────────────────────────────────────────────────────────────

function fetchGetJsonSinLoading(url, callback) {
    fetch(url, {
        method: 'GET',
        headers: { 'Accept': 'application/json' }
    })
    .then(function (resp) {
        if (resp.status === 401) {
            window.location.href = '/Auth/Login';
            return null;
        }
        return resp.json();
    })
    .then(function (data) {
        if (data !== null) callback(data);
    })
    .catch(function (err) {
        console.error('fetchGetJsonSinLoading error:', err);
        mostrarError('Error al comunicarse con el servidor.');
    });
}

// ─── Fetch POST ──────────────────────────────────────────────────────────────

function fetchPostJson(url, payload, callback) {
    fetch(url, {
        method: 'POST',
        headers: obtenerHeadersConCsrf(),
        body: JSON.stringify(payload)
    })
    .then(function (resp) {
        if (resp.status === 401) {
            window.location.href = '/Auth/Login';
            return null;
        }
        return resp.json();
    })
    .then(function (data) {
        if (data !== null) callback(data);
    })
    .catch(function (err) {
        console.error('fetchPostJson error:', err);
        mostrarError('Error al comunicarse con el servidor.');
    });
}

// ─── DataTables ──────────────────────────────────────────────────────────────

var _tablaInstancias = {};

function generarTabla(opciones) {
    /*
     opciones = {
       idContenedor: 'divTablaXxx',
       datos: [...],
       columnas: [{ titulo, campo, render }],
       acciones: function(fila) { return '<button>...</button>'; }
     }
    */
    var idTabla = 'tbl_' + opciones.idContenedor;

    // Destruir instancia previa si existe
    if (_tablaInstancias[idTabla]) {
        _tablaInstancias[idTabla].destroy();
        delete _tablaInstancias[idTabla];
    }

    // Construir encabezados
    var ths = opciones.columnas.map(function (c) {
        return '<th>' + c.titulo + '</th>';
    });
    if (opciones.acciones) ths.push('<th>Acciones</th>');

    var html = '<table id="' + idTabla + '" class="table table-striped table-hover w-100">'
             + '<thead><tr>' + ths.join('') + '</tr></thead>'
             + '<tbody></tbody></table>';

    get(opciones.idContenedor).innerHTML = html;

    // Construir columnas para DataTables
    var dtColumnas = opciones.columnas.map(function (c) {
        return {
            title: c.titulo,
            data: c.campo,
            render: c.render || null
        };
    });

    if (opciones.acciones) {
        dtColumnas.push({
            title: 'Acciones',
            data: null,
            orderable: false,
            render: function (data, type, row) {
                return opciones.acciones(row);
            }
        });
    }

    var instancia = $('#' + idTabla).DataTable({
        data: opciones.datos,
        columns: dtColumnas,
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        },
        pageLength: 10,
        responsive: true
    });

    _tablaInstancias[idTabla] = instancia;
}

// ─── Alertas ─────────────────────────────────────────────────────────────────

function mostrarExito(msg) {
    Swal.fire({
        icon: 'success',
        title: 'Exito',
        text: msg,
        timer: 2000,
        showConfirmButton: false
    });
}

function mostrarError(msg) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: msg || 'Ocurrio un error inesperado.'
    });
}

function confirmar(msg, titulo, callback) {
    Swal.fire({
        title: titulo || 'Confirmar',
        text: msg,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Si, continuar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33'
    }).then(function (result) {
        if (result.isConfirmed) callback();
    });
}

// ─── Validacion ──────────────────────────────────────────────────────────────

function validarRequerido(valor) {
    return valor !== null && valor !== undefined && String(valor).trim() !== '';
}

// ─── Formulario ──────────────────────────────────────────────────────────────

function limpiarFormulario(idForm) {
    var form = get(idForm);
    if (!form) return;
    var inputs = form.querySelectorAll('input, textarea, select');
    inputs.forEach(function (el) {
        if (el.type === 'hidden') return;
        if (el.type === 'checkbox' || el.type === 'radio') {
            el.checked = false;
        } else {
            el.value = '';
        }
    });
}

function registrarResetModal(idModal, idForm, callback) {
    var modal = get(idModal);
    if (!modal) return;
    modal.addEventListener('hidden.bs.modal', function () {
        limpiarFormulario(idForm);
        if (callback) callback();
    });
}
