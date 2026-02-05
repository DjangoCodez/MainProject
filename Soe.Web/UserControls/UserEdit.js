function getRoles() {
    var company = $$('DefaultCompany');
    var role = $$('DefaultRole');
    if (company == null || role == null) {
        return;
    }

    if (company.options.length == 0) {
        role.disabled = 'disabled';
        return;
    }

    role.options.length = 0;

    var url = '/ajax/getRoles.aspx?company=' + company.value;
    DOMAssistant.AJAX.get(url, function (data, status) {
        var obj = JSON.parse(data);
        if (obj) {
            role.disabled = '';
            obj.each(function () {
                role.options[parseInt(this.Position)] = new Option(this.Name, this.RoleId);
            });
        }
    });
}
