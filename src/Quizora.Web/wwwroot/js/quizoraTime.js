window.quizoraTime = {
    getOffsetMinutes: function () { 
        return -new Date().getTimezoneOffset();
    }
};