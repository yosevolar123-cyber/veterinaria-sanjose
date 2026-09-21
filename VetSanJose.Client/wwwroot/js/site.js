window.vetSanJose = {
    aosInit: function () {
        if (window.AOS) {
            AOS.init({ once: true, duration: 600, easing: 'ease-out' });
        }
    },
    aosRefresh: function () {
        if (window.AOS) {
            AOS.refreshHard();
        }
    },
    descargarArchivo: function (nombreArchivo, contenido, tipoContenido) {
        const blob = new Blob([contenido], { type: tipoContenido });
        const url = URL.createObjectURL(blob);
        const enlace = document.createElement('a');
        enlace.href = url;
        enlace.download = nombreArchivo;
        document.body.appendChild(enlace);
        enlace.click();
        document.body.removeChild(enlace);
        URL.revokeObjectURL(url);
    },
};
