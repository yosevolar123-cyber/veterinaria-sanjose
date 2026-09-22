// Envoltorio sobre la Web Speech API para dictado de notas médicas.
// Mantiene una única sesión de reconocimiento activa a la vez y
// siempre libera sus listeners al detenerse, para evitar fugas de
// memoria si el doctor abre y cierra el dictado repetidamente.
let sesionActiva = null;

export function esSoportado() {
    return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
}

export function iniciar(dotNetRef, idioma) {
    const ReconocedorCtor = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!ReconocedorCtor) {
        return false;
    }

    detener();

    // El navegador solo admite un reconocimiento activo: avisamos al asistente de voz global
    // (asistenteVoz.js) para que suelte el micrófono mientras dure el dictado.
    window.dispatchEvent(new CustomEvent('vsj:dictado-inicia'));

    const reconocedor = new ReconocedorCtor();
    reconocedor.lang = idioma || 'es-BO';
    reconocedor.continuous = true;
    reconocedor.interimResults = true;

    reconocedor.onresult = (evento) => {
        let textoFinal = '';
        for (let i = evento.resultIndex; i < evento.results.length; i++) {
            const resultado = evento.results[i];
            if (resultado.isFinal) {
                textoFinal += resultado[0].transcript;
            }
        }
        if (textoFinal.trim()) {
            dotNetRef.invokeMethodAsync('OnTextoReconocidoJs', textoFinal.trim());
        }
    };

    reconocedor.onerror = (evento) => {
        dotNetRef.invokeMethodAsync('OnErrorJs', evento.error || 'desconocido');
    };

    reconocedor.onend = () => {
        if (sesionActiva && sesionActiva.reconocedor === reconocedor) {
            sesionActiva = null;
        }
        dotNetRef.invokeMethodAsync('OnFinalizadoJs');
    };

    sesionActiva = { reconocedor };

    try {
        reconocedor.start();
    } catch {
        sesionActiva = null;
        return false;
    }

    return true;
}

export function detener() {
    if (!sesionActiva) {
        return;
    }

    const { reconocedor } = sesionActiva;
    reconocedor.onresult = null;
    reconocedor.onerror = null;
    reconocedor.onend = null;
    try {
        reconocedor.stop();
    } catch {
        // ya estaba detenido
    }
    sesionActiva = null;
}
