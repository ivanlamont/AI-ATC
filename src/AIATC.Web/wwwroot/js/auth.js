// Auth helper utilities.
// OAuth is handled server-side by AIATC.BFF; there is no client-side popup flow.
window.aiatcAuth = {
    storageGet: function (key) {
        return window.localStorage.getItem(key);
    },

    storageSet: function (key, value) {
        window.localStorage.setItem(key, value);
    },

    storageRemove: function (key) {
        window.localStorage.removeItem(key);
    }
};
