// Registro del service worker + UX de PWA: aviso de actualización disponible,
// banner de "sin conexión" y botón propio de instalación (beforeinstallprompt).
// Vive fuera de Blazor a propósito: tiene que funcionar aunque el WASM no
// haya terminado de cargar (p. ej. sin conexión en el primer arranque).
(function () {
    'use strict';

    function crearBanner(id, texto, colorFondo, colorTexto, boton) {
        var existente = document.getElementById(id);
        if (existente) return existente;

        var banner = document.createElement('div');
        banner.id = id;
        banner.setAttribute('role', 'status');
        banner.style.cssText =
            'position:fixed;left:0;right:0;z-index:9999;display:flex;align-items:center;' +
            'justify-content:center;gap:12px;padding:10px 16px;font:600 14px/1.3 system-ui,sans-serif;' +
            'border:3px solid #0f172a;box-sizing:border-box;text-align:center;' +
            'background:' + colorFondo + ';color:' + colorTexto + ';';

        var span = document.createElement('span');
        span.textContent = texto;
        banner.appendChild(span);

        if (boton) {
            var btn = document.createElement('button');
            btn.textContent = boton.texto;
            btn.style.cssText =
                'border:2px solid #0f172a;border-radius:8px;padding:4px 12px;font-weight:700;' +
                'background:#ffffff;color:#0f172a;cursor:pointer;box-shadow:2px 2px 0 0 #0f172a;';
            btn.addEventListener('click', boton.onClick);
            banner.appendChild(btn);
        }

        document.body.appendChild(banner);
        return banner;
    }

    function quitarBanner(id) {
        var el = document.getElementById(id);
        if (el) el.remove();
    }

    // --- Aviso de actualización disponible ---
    function mostrarAvisoActualizacion(registration) {
        crearBanner(
            'pwa-update-banner',
            'Hay una actualización disponible.',
            '#38bdf8',
            '#0f172a',
            {
                texto: 'Recargar',
                onClick: function () {
                    var waiting = registration.waiting;
                    if (waiting) {
                        waiting.postMessage({ type: 'SKIP_WAITING' });
                    } else {
                        window.location.reload();
                    }
                },
            }
        );
        var banner = document.getElementById('pwa-update-banner');
        if (banner) banner.style.top = '0';
    }

    if ('serviceWorker' in navigator) {
        var recargando = false;
        navigator.serviceWorker.addEventListener('controllerchange', function () {
            if (recargando) return;
            recargando = true;
            window.location.reload();
        });

        window.addEventListener('load', function () {
            navigator.serviceWorker
                .register('service-worker.js')
                .then(function (registration) {
                    if (registration.waiting && navigator.serviceWorker.controller) {
                        mostrarAvisoActualizacion(registration);
                    }

                    registration.addEventListener('updatefound', function () {
                        var nuevoWorker = registration.installing;
                        if (!nuevoWorker) return;
                        nuevoWorker.addEventListener('statechange', function () {
                            if (nuevoWorker.state === 'installed' && navigator.serviceWorker.controller) {
                                mostrarAvisoActualizacion(registration);
                            }
                        });
                    });
                })
                .catch(function () {
                    // Degradación silenciosa: la app sigue funcionando sin cache offline.
                });
        });
    }

    // --- Banner de sin conexión ---
    function actualizarEstadoConexion() {
        if (navigator.onLine) {
            quitarBanner('pwa-offline-banner');
        } else {
            var banner = crearBanner(
                'pwa-offline-banner',
                'Sin conexión. Algunas funciones (inicio de sesión, datos nuevos) no van a estar disponibles.',
                '#0f172a',
                '#ffffff'
            );
            banner.style.top = '0';
        }
    }
    window.addEventListener('online', actualizarEstadoConexion);
    window.addEventListener('offline', actualizarEstadoConexion);
    document.addEventListener('DOMContentLoaded', actualizarEstadoConexion);

    // --- Botón propio de instalación (Android/Chrome/Edge) ---
    var deferredPrompt = null;

    function estaInstalada() {
        return window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true;
    }

    function mostrarBotonInstalar() {
        if (estaInstalada() || document.getElementById('pwa-install-btn')) return;

        var btn = document.createElement('button');
        btn.id = 'pwa-install-btn';
        btn.textContent = '⬇ Instalar app';
        btn.style.cssText =
            'position:fixed;right:16px;bottom:16px;z-index:9998;border:3px solid #0f172a;' +
            'border-radius:10px;padding:10px 16px;font:700 14px system-ui,sans-serif;' +
            'background:#38bdf8;color:#0f172a;cursor:pointer;box-shadow:4px 4px 0 0 #0f172a;';
        btn.addEventListener('click', function () {
            btn.remove();
            if (!deferredPrompt) return;
            deferredPrompt.prompt();
            deferredPrompt.finally && deferredPrompt.finally(function () {
                deferredPrompt = null;
            });
            deferredPrompt.userChoice.then(function () {
                deferredPrompt = null;
            });
        });
        document.body.appendChild(btn);
    }

    window.addEventListener('beforeinstallprompt', function (event) {
        event.preventDefault();
        deferredPrompt = event;
        mostrarBotonInstalar();
    });

    window.addEventListener('appinstalled', function () {
        deferredPrompt = null;
        var btn = document.getElementById('pwa-install-btn');
        if (btn) btn.remove();
    });
})();
