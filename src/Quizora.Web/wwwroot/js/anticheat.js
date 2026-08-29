
window.QuizoraAntiCheat = (function () {
    let dotNetRef = null;
    let active = false;

    function onVisibility() {
        if (!active || !dotNetRef) return;
        if (document.hidden) {
            try { dotNetRef.invokeMethodAsync('OnVisibilityHidden'); } catch (e) { }
        }
    }

    function onBlur() {
        if (!active || !dotNetRef) return;
        try { dotNetRef.invokeMethodAsync('OnWindowBlur'); } catch (e) { }
    }

    function onCopy(e) {
        if (!active || !dotNetRef) return;
        e.preventDefault();
        try { dotNetRef.invokeMethodAsync('OnCopyAttempt'); } catch (ex) { }
    }

    function onPaste(e) {
        if (!active || !dotNetRef) return;
        e.preventDefault();
        try { dotNetRef.invokeMethodAsync('OnPasteAttempt'); } catch (ex) { }
    }

    function onContext(e) {
        if (!active) return;
        e.preventDefault();
    }

    return {
        register: function (ref) {
            dotNetRef = ref;
            active = true;
            document.addEventListener('visibilitychange', onVisibility);
            window.addEventListener('blur', onBlur);
            document.addEventListener('copy', onCopy);
            document.addEventListener('paste', onPaste);
            document.addEventListener('contextmenu', onContext);
        },
        unregister: function () {
            active = false;
            document.removeEventListener('visibilitychange', onVisibility);
            window.removeEventListener('blur', onBlur);
            document.removeEventListener('copy', onCopy);
            document.removeEventListener('paste', onPaste);
            document.removeEventListener('contextmenu', onContext);
            dotNetRef = null;
        },
        requestFullscreen: async function () {
            try {
                const el = document.documentElement;
                if (el.requestFullscreen) await el.requestFullscreen();
            } catch (e) { /* user may deny */ }
        }
    };
})();
