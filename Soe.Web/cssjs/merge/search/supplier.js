
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

var SupplierByNumberSearch =
{
    items: [],
    timeout: 300,
    companyId: 0,
    timers: {},
    numberField: {},
    displayDefault: false,
    init: function (displayDefault) {
        SupplierByNumberSearch.displayDefault = displayDefault;
        SupplierByNumberSearch.companyId = getQueryVariable("c");
        SupplierByNumberSearch.setFieldValues(null);
    },
    keydown: function (field, e) {
        var evtobj = window.event ? event : e;
        var key = window.event ? window.event.keyCode : e.which;
        switch (key) {
            case 8:
            case 46:
                SupplierByNumberSearch.searchOnKey(field);
                break;
        }
        if ((key > 47 && key < 91) || (key > 95 && key < 106))
            SupplierByNumberSearch.searchOnKey(field);
    },
    searchField: function (field) {

        if (SupplierByNumberSearch && $$(field) != null) {
            SupplierByNumberSearch.numberField = $$(field);
            SupplierByNumberSearch.nameField = $$(field + '-infotext');

            SupplierByNumberSearch.search(SupplierByNumberSearch.numberField.value);
        }
    },
    setFieldValues: function (value) {

        if (SupplierByNumberSearch.nameField == null) return;

        if (value != null)
            SupplierByNumberSearch.removeWarning();

        if (value != null && value.SupplierNr && value.SupplierNr != null && value.SupplierNr != undefined)
            SupplierByNumberSearch.numberField.value = value.SupplierNr;

        var prefixText = '';

        if (SupplierByNumberSearch.numberField.id == 'SupplierNr-from-1') {
            prefixText = value.SupplierNr;
        }


        if (prefixText == undefined)
            prefixText = "";


        if (value.SupplierNr != null)
            copyValue('SupplierNr-from-1', 'SupplierNr-to-1', 'true');
    },
    searchOnKey: function (field) {
        if (SupplierByNumberSearch.timers[field])
            clearTimeout(SupplierByNumberSearch.timers[field]);
        var f = "SupplierByNumberSearch.searchField('" + field + "')";
        SupplierByNumberSearch.timers[field] = setTimeout(f, 300);
    },
    search: function (text) {
        if (searchComponent && searchComponent.open) {
            searchComponent.open = false;
            searchComponent.dispose();
        }

        var url = '/ajax/getSupplierByNumber.aspx?c=' + SupplierByNumberSearch.companyId + '&snr=' + encodeURIComponent(text);
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj) {
                if (obj.length == 0) {
                    SupplierByNumberSearch.addWarning();
                    SupplierByNumberSearch.setFieldValues(null);
                    return;
                }
                else if (obj.length == 1) {
                    SupplierByNumberSearch.setFieldValues(obj);
                    return;
                }
                else {
                    SupplierByNumberSearch.render(obj);
                }
            }
        });
    },
    render: function (products) {
        var indexPrefix = "supplier_";
        var containerId = "searchContainer";
        var templateId = "SupplierByNumberSearchItem_$supplierNr$";
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
            guiObj.Number = products[counter].SupplierNr;
            guiObj.id = counter;
            guiObj.Name = products[counter].SupplierName;

            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            if (maxlengthNum <= guiObj.Number.length)
                maxlengthNum = guiObj.Number.length;
            if (maxlengthName <= guiObj.Name.length)
                maxlengthName = guiObj.Name.length;
            item.innerHTML = item.innerHTML.replace('$supplierNr$', guiObj.Number);
            item.innerHTML = item.innerHTML.replace('$supplierName$', guiObj.Name);
            container.innerHTML += item.innerHTML;
        }

        searchComponent.init(products, container, SupplierByNumberSearch.timeout, SupplierByNumberSearch.numberField.id, indexPrefix, 'SupplierByNumberSearch.searchOnKey', 'SupplierByNumberSearch.setFieldValues', maxlengthNum, maxlengthName);
    },
    addWarning: function () {
        if (!SupplierByNumberSearch.hasWarning()) {
            var parent = SupplierByNumberSearch.numberField.parentElement.parentElement;
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
        var parent = SupplierByNumberSearch.numberField.parentElement.parentElement;
        if (parent.children.length > 1) {
            var tmp = parent.children[parent.children.length - 1];
            if (tmp.children && tmp.children.length > 0)
                if ($(tmp.children[0]).hasClass("warningDiv"))
                    return true;
        }
        return false;
    },
    removeWarning: function () {
        if (SupplierByNumberSearch.hasWarning()) {
            var parent = SupplierByNumberSearch.numberField.parentElement.parentElement;
            parent.removeChild(parent.children[parent.children.length - 1]);
        }
    }
}
