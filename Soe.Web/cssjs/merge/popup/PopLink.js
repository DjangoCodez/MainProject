var PopLink = {
    container: null,
    currentLink: null,
    onShowFns: null,
    onSubmitFns: null,
    init: function () {


        $('a.PopLink').each(function () {
            PopLink.initElement(this);
        });


    },
    /* Om funktionen onShowFn tillhandahålls anropas denna efter att popup:en visats med popup-elementet som parameter.
    * Om funktionen onSubmitFn tillhandahålls anropas denna vid klick på submit-knapp med popup-elementet som parameter. */
    initElement: function (el, onShowFn, onSubmitFn) {
        if (onShowFn) {
            if (!PopLink.onShowFns)
                PopLink.onShowFns = new Object();
            PopLink.onShowFns[getID(el)] = onShowFn;
        }
        if (onSubmitFn) {
            if (!PopLink.onSubmitFns)
                PopLink.onSubmitFns = new Object();
            PopLink.onSubmitFns[getID(el)] = onSubmitFn;
        }
        el.onclick = PopLink.pop;
    },
    close: function () {
        Popup.hide();
        document.onkeydown = null;
    },
    pop: function () {
        PopLink.currentLink = this;
        DOMAssistant.AJAX.get(PopLink.currentLink.href, function (response) {

            PopLink.container = $(Popup.showInnerHtml(response));

            Kbd.setKeydownListener(doc, Kbd.escape, PopLink.close);

            var frms = $(PopLink.container).find('form');

            if (frms.length) {

                if (frms[0].elements) {
                    Forms.initForm(frms[0]);
                    frms[0].elements[0].focus();

                    $(frms[0]).find('button').each(function () {
                        var btn = $$(this);
                        if (btn.hasClass('cancel')) {
                            btn.onclick = PopLink.close;
                        } else if (this.type == 'submit' && PopLink.onSubmitFns) {
                            btn.onclick = function () {
                                PopLink.onSubmitFns[PopLink.currentLink.id](PopLink.container);
                                return false;
                            };
                        }
                    });
                }
            }
            if (PopLink.onShowFns && PopLink.currentLink.id) {
                var onShowFn = PopLink.onShowFns[PopLink.currentLink.id];
                if (onShowFn)
                    onShowFn(PopLink.container);
            }
        });
        return false;
    },
    modalWindowShow: function (href) { //todo refactor with pop

        DOMAssistant.AJAX.get(href, function (response) {

            PopLink.container = $$(Popup.showInnerHtml(response));
            Kbd.setKeydownListener(doc, Kbd.escape, PopLink.close);

            var frms = $(PopLink.container).find('form');

            //var frms = $target.prop('form');//PopLink.container.elmsByTag('form');

            if (frms.length) {
                if (frms[0].elements) {
                    Forms.initForm(frms[0]);

                    $(frms[0]).find('button').each(function () {
                        var btn = $$(this);
                        if (btn.hasClass('cancel')) {
                            btn.onclick = PopLink.close;
                        } else if (this.type == 'submit' && PopLink.onSubmitFns) {
                            btn.onclick = function () {
                                PopLink.onSubmitFns[PopLink.currentLink.id](PopLink.container);
                                return false;
                            };
                        }
                    });
                }
            }
            if (PopLink.onShowFns && PopLink.currentLink.id) {
                var onShowFn = PopLink.onShowFns[PopLink.currentLink.id];
                if (onShowFn)
                    onShowFn(PopLink.container);
            }
        });
        return false;
    }
}


$(window).bind('load', PopLink.init);