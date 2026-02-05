var SoeTabView = {
    tabs: {},

    init: function () {
        $$(this.document).elmsByClass('SoeTabView', 'div').each(function () {
            SoeTabView.initTabs(this);
        });
    },
    initTabs: function (el) {
        var tabList = $$(el).elmsByClass('tabList')[0];
        SoeTabView.tabs[el.id] = $$(tabList).elmsByTag('a');
        SoeTabView.tabs[el.id].each(function ()
        {
            this.onclick = SoeTabView.toggle;
        });
    },
    toggle: function () {
        var a = this;
                              
        if (a.innerText == '' && a.children.length > 0 && a.children[0].tagName.toLowerCase() == 'img') {
            return;
        }
        var id = a.href.split('#')[1];
        var div = a.parentNode.parentNode.parentNode.parentNode;
         
       var counter = 0;
       $$(div).elmsByClass('tabContent', 'div').each(function () {
           var tc = this;
           if (tc.id == id) {
               $$(tc).addClass('active');
               if (setFocus != undefined)
                   if ($$(tc).disabled == false)
                       setFocus(counter);
           }
           else {
               $$(tc).removeClass('active');
               counter++;
           }
        });
        SoeTabView.tabs[div.id].each(function () {
            var tab = this;
            if (tab == a) {
                $$(tab).addClass('active');
            }
            else {
                $$(tab).removeClass('active');
            }
        });
        a.blur();
        return false;
    }
}

$(window).bind('load', SoeTabView.init);
