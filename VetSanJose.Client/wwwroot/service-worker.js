// Service worker de Veterinaria San José: cachea el app shell y usa
// stale-while-revalidate para el resto de las peticiones same-origin,
// dejando pasar sin tocar cualquier petición cross-origin (p. ej. la API).
// El navegador y el código de la app (index.html + _framework) van siempre
// por red primero, para que un despliegue nuevo no quede atrapado en caché.
const CACHE_VERSION = '__BUILD_VERSION__';
const CACHE_NAME = `vetsanjose-cache-${CACHE_VERSION}`;
const PRECACHE_URLS = [
  './',
  'index.html',
  'manifest.json',
  'css/app.css',
  'css/tailwind.css',
  'favicon.png',
  'icon-192.png',
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => cache.addAll(PRECACHE_URLS))
      .then(() => self.skipWaiting())
      .catch(() => {})
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key))))
      .then(() => self.clients.claim())
  );
});

function guardar(request, response) {
  if (response && response.status === 200) {
    const clone = response.clone();
    caches.open(CACHE_NAME).then((cache) => cache.put(request, clone));
  }
  return response;
}

// Red primero: si hay conexión siempre gana la versión recién desplegada.
function redPrimero(request) {
  return fetch(request)
    .then((response) => guardar(request, response))
    .catch(() => caches.match(request).then((cached) => cached || caches.match('index.html')));
}

// Stale-while-revalidate: responde al instante y refresca en segundo plano.
function cacheConRefresco(request) {
  return caches.match(request).then((cached) => {
    const network = fetch(request)
      .then((response) => guardar(request, response))
      .catch(() => cached);

    return cached || network;
  });
}

self.addEventListener('fetch', (event) => {
  const request = event.request;

  if (request.method !== 'GET') {
    return;
  }

  const url = new URL(request.url);
  if (url.origin !== self.location.origin) {
    return;
  }

  const esNavegacion = request.mode === 'navigate';
  const esCodigoDeApp = url.pathname.includes('/_framework/') || url.pathname.endsWith('service-worker.js');

  event.respondWith(esNavegacion || esCodigoDeApp ? redPrimero(request) : cacheConRefresco(request));
});
