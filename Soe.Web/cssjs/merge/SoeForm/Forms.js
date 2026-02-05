/*
 * Standard behaviour for forms.
 */

var Forms={
    focusedElementClass: 'focus',
    init: function() {
        $('form').each(function () {
            Forms.initForm(this);
            FormValidation.initForm(this);
        });
    },
    initForm: function(frm) {
        $(frm).find('input').each(Forms.initIpt);
    },
    initIpt: function() {
        var e=$(this);
        if(e.hasClass('numeric')) {
            //e.onkeypress=Forms.supressNonNumericKeys; // Funkar inte med e.addEvent
            e.onkeydown=Forms.supressNonNumericKeys; //Capture CTRL in IE
        }
        if(e.hasClass('date'))
            e.bind('focus',DatePicker.focus);
        if(e.hasClass('combo'))
            Forms.initCombo(e);
        e.bind('focus',Forms.iptFocus);
        e.bind('blur',Forms.iptBlur);
    },
    initCombo: function(el) {
        el.onclick=function() {
            $$(el.id+'-options').style.display='block';
        };
        $$(el.id+'-options').elmsByTag('a').each(function() {
            this.onclick=Forms.comboClick;
        });
    },
    comboClick: function() {
        $(this.parentNode.parentNode).style.display='none';
        var p=this.id.split('-');
        var name=p[0];
        var id=p[1];
        $$(name).value=this.innerHTML;
        $$(name+'-value').value=id;
        return false;
    },
    supressNonNumericKeys: function(e) {
        var key=window.event?window.event.keyCode:e.which;
        var allowFirst=null;
        var el=$(this);
        if(el.hasClass('negative'))
            allowFirst=['-'];
        return Forms.supressNonNumericKeysSub(el,key,allowFirst);
    },
    supressNonNumericKeysSub: function(element, key, allowFirst) {
        if (!key || (key == Kbd.backspace) || (key == Kbd.tab) || (key == Kbd.enter) || (key == Kbd.escape))
            return true;
        if (key >= 8 && key <= 57 && key != 32)
            return true;
        var keychar = String.fromCharCode(key);
        if (key >= 96 && key <= 105) {
            return true;
        }
        if (allowFirst == '-' && (key == 109 || key == 189) && element.value.length == 0)
            return true;
        if (keychar == SOE.decimalSeparator && element.hasClass('decimals') && element.value.indexOf(SOE.decimalSeparator) < 0)
            return true;
        if ((key == 188 || key == 110) && element.hasClass('decimals') && element.value.indexOf(SOE.decimalSeparator) < 0)
            return true;
        if (key < 14)
            return false;
        window.event.returnValue = false;
        window.event.cancelBubble = true;
        return false;
    },
    iptFocus: function() {
        if(!this.readOnly)
            $(this).addClass(Forms.focusedElementClass);
    },
    iptBlur: function() {
        $(this).removeClass(Forms.focusedElementClass);
    }
}
$(window).bind('load', Forms.init);
