var SoeForm = {

    ClientData: {},

    init: function () {
        $('form').each(function () {
            var frm = $(this);
            if (frm.hasClass(SoeForm))
                Forms.initForm(frm);
        });
    },

    initForm: function (frm) {
        SoeForm.ClientData[SOE.getID(frm)] = eval('(' + frm['ClientData'].value + ')');
        frm.onsubmit = SoeForm.onSubmit;
    },

    onSubmit: function () {
        this['ClientData'].value = JSON.stringify(SoeForm.ClientData[this.id]);
    }
};

$(window).bind('load', SoeForm.init);
