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
};
