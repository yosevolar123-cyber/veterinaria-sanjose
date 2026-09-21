// Envoltorio delgado sobre Chart.js (cargado via CDN en index.html).
// Guarda una instancia por canvas para poder destruirla antes de
// volver a dibujar y así no acumular listeners/memoria al refrescar
// los datos del reporte.
const graficos = new Map();

export function renderizarBarras(idCanvas, etiquetas, series) {
    const canvas = document.getElementById(idCanvas);
    if (!canvas || !window.Chart) {
        return;
    }

    destruir(idCanvas);

    const grafico = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: etiquetas,
            datasets: series.map((serie) => ({
                label: serie.etiqueta,
                data: serie.datos,
                backgroundColor: serie.color,
                borderRadius: 6,
            })),
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } },
            scales: { y: { beginAtZero: true } },
        },
    });

    graficos.set(idCanvas, grafico);
}

export function destruir(idCanvas) {
    const existente = graficos.get(idCanvas);
    if (existente) {
        existente.destroy();
        graficos.delete(idCanvas);
    }
}
