// Envoltorio delgado sobre Chart.js (cargado via CDN en index.html).
// Guarda una instancia por canvas para poder destruirla antes de
// volver a dibujar y así no acumular listeners/memoria al refrescar
// los datos del reporte.
const graficos = new Map();

// Chart.js llega por CDN con `defer`, asi que puede no estar listo cuando Blazor
// dibuja el primer grafico. Antes se devolvia en silencio y el canvas quedaba
// vacio para siempre; ahora se espera a que la libreria aparezca.
function esperarChartJs(intentosRestantes = 50) {
    if (window.Chart) {
        return Promise.resolve(true);
    }

    if (intentosRestantes <= 0) {
        return Promise.resolve(false);
    }

    return new Promise((resolve) => setTimeout(resolve, 100))
        .then(() => esperarChartJs(intentosRestantes - 1));
}

export async function renderizarBarras(idCanvas, etiquetas, series) {
    if (!await esperarChartJs()) {
        return;
    }

    const canvas = document.getElementById(idCanvas);
    if (!canvas) {
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
