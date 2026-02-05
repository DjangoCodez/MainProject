
function copyValue(fromId, toId, focus) {
    var from = document.getElementById(fromId);

    if (from == null)
        return;

    var to = document.getElementById(toId);
    if (to == null)
        return;

    if (to.value == "" || to.value == "0") {
        to.value = from.value;
        if (focus && (focus == 'true' || focus == "1")) {
            if (to.style.visibility == "hidden" || to.style.display == "none" || to.disabled == true) {
                //do something because it can't accept focus()
            }
            else
                to.focus();
        }
    }
}

var EmployeeByNumberSearch =
{
    items: [],
    timeout: 300,
    companyId: 0,
    timers: {},
    numberField: {},
    displayDefault: false,
    init: function (displayDefault) {
        EmployeeByNumberSearch.displayDefault = displayDefault;
        EmployeeByNumberSearch.companyId = getQueryVariable("c");
        EmployeeByNumberSearch.setFieldValues(null);
    },
    keydown: function (field, e) {
        var evtobj = window.event ? event : e;
        var key = window.event ? window.event.keyCode : e.which;
        switch (key) {
            case 8:
            case 46:
                EmployeeByNumberSearch.searchOnKey(field);
                break;
        }
        if ((key > 47 && key < 91) || (key > 95 && key < 106))
            EmployeeByNumberSearch.searchOnKey(field);
    },
    searchField: function (field) {

        if (EmployeeByNumberSearch && $$(field) != null) {
            EmployeeByNumberSearch.numberField = $$(field);
            EmployeeByNumberSearch.nameField = $$(field + '-infotext');

            EmployeeByNumberSearch.search(EmployeeByNumberSearch.numberField.value);
        }
    },
    setFieldValues: function (value) {

        if (EmployeeByNumberSearch.nameField == null) return;

        if (value != null)
            EmployeeByNumberSearch.removeWarning();

        if (value != null && value.EmployeeNr && value.EmployeeNr != null && value.EmployeeNr != undefined)
            EmployeeByNumberSearch.numberField.value = value.EmployeeNr;

        var prefixText = '';

        if (EmployeeByNumberSearch.numberField.id == 'EmployeeNr-from-1') {
            prefixText = value.EmployeeNr;
        }

        if (prefixText == undefined)
            prefixText = "";

        if (value.EmployeeNr != null)
            copyValue('EmployeeNr-from-1', 'EmployeeNr-to-1', 'true');
    },
    searchOnKey: function (field) {
        if (EmployeeByNumberSearch.timers[field])
            clearTimeout(EmployeeByNumberSearch.timers[field]);
        var f = "EmployeeByNumberSearch.searchField('" + field + "')";
        EmployeeByNumberSearch.timers[field] = setTimeout(f, 300);
    },
    search: function (text) {
        if (searchComponent && searchComponent.open) {
            searchComponent.open = false;
            searchComponent.dispose();
        }

        var url = '/ajax/getEmployeesByNumber.aspx?c=' + EmployeeByNumberSearch.companyId + '&enr=' + encodeURIComponent(text);
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj) {
                if (obj.length == 0) {
                    EmployeeByNumberSearch.addWarning();
                    EmployeeByNumberSearch.setFieldValues(null);
                    return;
                }
                else if (obj.length == 1) {
                    EmployeeByNumberSearch.setFieldValues(obj);
                    return;
                }
                else {
                    EmployeeByNumberSearch.render(obj);
                }
            }
        });
    },
    render: function (products) {
        var indexPrefix = "employee_";
        var containerId = "searchContainer";
        var templateId = "EmployeeByNumberSearchItem_$employeeNr$";
        var template = $$(templateId);
        if (template == null)
            return;

        var container = document.createElement("div");
        container.style.display = "none";
        var maxlengthNum = 0;
        var maxlengthName = 0;
        for (var counter = 0; counter < products.length; counter++) {

            var item = document.createElement("div");
            item.innerHTML = template.innerHTML;

            var guiObj = new Object();
            guiObj.Number = products[counter].EmployeeNr;
            guiObj.id = counter;
            guiObj.Name = products[counter].EmployeeName;

            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            if (maxlengthNum <= guiObj.Number.length)
                maxlengthNum = guiObj.Number.length;
            if (maxlengthName <= guiObj.Name.length)
                maxlengthName = guiObj.Name.length;
            item.innerHTML = item.innerHTML.replace('$employeeNr$', guiObj.Number);
            item.innerHTML = item.innerHTML.replace('$employeeName$', guiObj.Name);
            container.innerHTML += item.innerHTML;
        }

        searchComponent.init(products, container, EmployeeByNumberSearch.timeout, EmployeeByNumberSearch.numberField.id, indexPrefix, 'EmployeeByNumberSearch.searchOnKey', 'EmployeeByNumberSearch.setFieldValues', maxlengthNum, maxlengthName);
    },
    addWarning: function () {
        if (!EmployeeByNumberSearch.hasWarning()) {
            var parent = EmployeeByNumberSearch.numberField.parentElement.parentElement;
            var td = document.createElement('td');
            var warning = document.createElement('div');
            var img = document.createElement('img');
            img.src = "/img/exclamation.png";
            $(warning).addClass('warningDiv');
            $(img).addClass('warningImg');
            img.title = "Artikel finns inte i register!";
            warning.appendChild(img);
            td.appendChild(warning);
            parent.appendChild(td);
        }
    },
    hasWarning: function () {
        var parent = EmployeeByNumberSearch.numberField.parentElement.parentElement;
        if (parent.children.length > 1) {
            var tmp = parent.children[parent.children.length - 1];
            if (tmp.children && tmp.children.length > 0)
                if ($(tmp.children[0]).hasClass("warningDiv"))
                    return true;
        }
        return false;
    },
    removeWarning: function () {
        if (EmployeeByNumberSearch.hasWarning()) {
            var parent = EmployeeByNumberSearch.numberField.parentElement.parentElement;
            parent.removeChild(parent.children[parent.children.length - 1]);
        }
    }
}
