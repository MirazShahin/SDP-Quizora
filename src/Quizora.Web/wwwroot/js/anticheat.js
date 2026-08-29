window.QuizoraAntiCheat = (function () {
    let dotNetRef = null;
    let active = false;

    function onVisibility() {
        if (!active || !dotNetRef) return;
        if (document.hidden) {
            try { dotNetRef.invokeMethodAsync('OnVisibilityHidden'); } catch (e) { }
        }
    }

    return {
        register: function (ref) {
            dotNetRef = ref;
            active = true;
            document.addEventListener('visibilitychange', onVisibility);
        },
        unregister: function () {
            active = false;
            document.removeEventListener('visibilitychange', onVisibility);
            dotNetRef = null;
        },
        requestFullscreen: async function () { }
    };
})();