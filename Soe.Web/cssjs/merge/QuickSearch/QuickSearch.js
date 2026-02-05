var QuickSearch = {
    //lastValue: null,
    alertTimerId: null,
    ipt: null,
    resultDiv: null,
    mouseIsOverDiv: false,

    init: function () {
        if (!QuickSearch.ipt)
            QuickSearch.ipt = $$('QuickSearch');

        if (QuickSearch.ipt) {
            QuickSearch.ipt.value = '';

            QuickSearch.ipt.onkeyup = function () {
                if (this.value) {
                    //if (this.value != QuickSearch.lastValue) {
                    if (QuickSearch.alertTimerId)
                        clearTimeout(QuickSearch.alertTimerId);
                    QuickSearch.alertTimerId = setTimeout('QuickSearch.doSearch()', 500);
                    //}
                } else {
                    QuickSearch.hide();
                }
            };

            Kbd.setKeydownListener(this, Kbd.escape, function () {
                QuickSearch.hide();
                QuickSearch.ipt.value = '';
            });

            QuickSearch.ipt.onblur = function () {
                if (!QuickSearch.mouseIsOverDiv)
                    QuickSearch.hide();
                QuickSearch.ipt.value = '';
            };

            QuickSearch.ipt.form.onsubmit = function () { return false; };
        }
    },

    doSearch: function () {
        var val = QuickSearch.ipt.value;

        // Perform this check again since value might have changed since timer started!
        if (val /*&& (val != QuickSearch.lastValue)*/) {
            QuickSearch.showResult(val);
            //QuickSearch.lastValue = val;
        }
    },

    getHeight: function (el) {
        var c = 0;
        if (el.offsetHeight) {
            while (el.offsetHeight) {
                c += el.offsetHeight;
                el = el.offsetHeight;
            }
        }
        else if (el.height) {
            c += el.height;
        }
        return c;
    },    

    getTopPos: function (el) {
        var c = 0;
        if (el.offsetParent) {
            while (el.offsetParent) {
                c += el.offsetTop
                el = el.offsetParent;
            }
        } else if (el.y) {
            c += el.y;
        }
        return c;
    },    

    showResult: function (search) {
        if (!QuickSearch.resultDiv) {
            var div = $$(document.body).create('div', { id: 'QuickSearchResultDiv' }, true);
            div.style.top = QuickSearch.getTopPos(QuickSearch.ipt) + QuickSearch.getHeight(QuickSearch.ipt) + 'px';
            div.onmouseover = function () { QuickSearch.mouseIsOverDiv = true; }
            div.onmouseout = function () { QuickSearch.mouseIsOverDiv = false; }
            QuickSearch.resultDiv = div;
        }
        QuickSearch.resultDiv.get('/ajax/qSearch.aspx?search=' + encodeURIComponent(search), function (responseText) {
            this.innerHTML = responseText;
            this.style.display = 'block';
        });
    },

    hide: function () {
        if (QuickSearch.resultDiv)
            QuickSearch.resultDiv.style.display = 'none';
    }
}

$(window).bind('load', QuickSearch.init);
