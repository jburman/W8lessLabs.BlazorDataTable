﻿if (!window.blazorDataTable) {
    window.blazorDataTable = {};
}
window.blazorDataTable = {
    scrollHandlers: {},
    attachEvents: function (rootId) {
        setTimeout(function () {
            var theader = document.querySelector('#' + rootId + ' .w8less-header');
            var tbody = document.querySelector('#' + rootId + ' .w8less-theight');
            var handler = null;
            if (theader && tbody) {
                if (typeof blazorDataTable.scrollHandlers[rootId] === 'function') {
                    tbody.removeEventListener('scroll', blazorDataTable.scrollHandlers[rootId]);
                }
                handler = function (e) {
                    theader.scrollLeft = tbody.scrollLeft;
                };
                tbody.addEventListener('scroll', handler);
                blazorDataTable.scrollHandlers[rootId] = handler;
            }
        }, 100);
    }
}