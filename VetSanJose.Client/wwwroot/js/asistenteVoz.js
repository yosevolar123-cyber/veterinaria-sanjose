// Asistente de voz global: escucha una frase por vez y, mientras el usuario no lo apague, vuelve a
// escuchar solo después de cada comando. Chrome corta el reconocimiento tras cada frase o tras unos
// segundos de silencio (evento onend), así que el "modo continuo" se logra reiniciando en onend.
let estado = null;

const ERRORES_FATALES = new Set(['not-allowed', 'service-not-allowed', 'audio-capture', 'network', 'language-not-supported']);

export function esSoportado() {
    return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
}

export function iniciar(dotNetRef, idioma) {
    const Ctor = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!Ctor) return false;

    detener();

    const reconocedor = new Ctor();
    reconocedor.lang = idioma || 'es-BO';
    reconocedor.continuous = false;
    reconocedor.interimResults = true;
    reconocedor.maxAlternatives = 1;

    const actual = { reconocedor, dotNetRef, activo: true, fallosSeguidos: 0, timer: null };
    estado = actual;

    reconocedor.onresult = (evento) => {
        let parcial = '';
        for (let i = evento.resultIndex; i < evento.results.length; i++) {
            const r = evento.results[i];
            if (r.isFinal) {
                const texto = r[0].transcript.trim();
                if (texto) {
                    actual.fallosSeguidos = 0;
                    dotNetRef.invokeMethodAsync('OnComandoJs', texto);
                }
            } else {
                parcial += r[0].transcript;
            }
        }
        if (parcial.trim()) dotNetRef.invokeMethodAsync('OnParcialJs', parcial.trim());
    };

    reconocedor.onerror = (evento) => {
        const codigo = evento.error || 'desconocido';
        if (codigo === 'no-speech' || codigo === 'aborted') return; // silencio: se reinicia en onend
        if (ERRORES_FATALES.has(codigo)) actual.activo = false;
        dotNetRef.invokeMethodAsync('OnErrorJs', codigo);
    };

    reconocedor.onend = () => {
        if (estado !== actual) return;
        if (!actual.activo) {
            estado = null;
            dotNetRef.invokeMethodAsync('OnFinalizadoJs');
            return;
        }
        // Pequeña pausa para no martillar el servicio si algo falla en bucle.
        actual.fallosSeguidos++;
        const espera = Math.min(250 * actual.fallosSeguidos, 2000);
        actual.timer = setTimeout(() => {
            if (estado !== actual || !actual.activo) return;
            try {
                reconocedor.start();
            } catch {
                estado = null;
                dotNetRef.invokeMethodAsync('OnFinalizadoJs');
            }
        }, espera);
    };

    try {
        reconocedor.start();
    } catch {
        estado = null;
        return false;
    }
    return true;
}

window.addEventListener('vsj:dictado-inicia', () => {
    if (!estado) return;
    const { dotNetRef } = estado;
    detener();
    dotNetRef.invokeMethodAsync('OnFinalizadoJs');
});

export function detener() {
    if (!estado) return;
    const { reconocedor, timer } = estado;
    estado.activo = false;
    clearTimeout(timer);
    reconocedor.onresult = null;
    reconocedor.onerror = null;
    reconocedor.onend = null;
    try { reconocedor.abort(); } catch { /* ya estaba detenido */ }
    estado = null;
}
