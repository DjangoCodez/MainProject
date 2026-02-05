
var invoiceProductSearch =
{
    items: [],
    timeout: 300,
    companyId: 0,
    timers: {},
    numberField: {},
    displayDefault: false,
    init: function (displayDefault) {
        invoiceProductSearch.displayDefault = displayDefault;
        invoiceProductSearch.companyId = getQueryVariable("c");
        invoiceProductSearch.setFieldValues(null);
    },
    keydown: function (field, e) {
        var evtobj = window.event ? event : e;
        var key = window.event ? window.event.keyCode : e.which;
        switch (key) {
            case 8:
            case 46:
                invoiceProductSearch.searchOnKey(field);
                break;
        }
        if ((key > 47 && key < 91) || (key > 95 && key < 106))
            invoiceProductSearch.searchOnKey(field);
    },
    searchField: function (field) {
        if (invoiceProductSearch && $$(field) != null) {
            invoiceProductSearch.numberField = $$(field);
            invoiceProductSearch.nameField = $$(field + '-infotext');
            invoiceProductSearch.search(invoiceProductSearch.numberField.value, (field == 'FlatPrice' || field == 'FlatPriceKeepPrices'));
        }
    },
    setFieldValues: function (value) {
        if (invoiceProductSearch.nameField == null) return;

        if (value != null)
            invoiceProductSearch.removeWarning();

        if (value != null && value.Number && value.Number != null && value.Number != undefined)
            invoiceProductSearch.numberField.value = value.Number;
        else if (value != null && Array.isArray(value) && value.length == 1 && value[0].Number && value[0].Number != null && value[0].Number != undefined)
            invoiceProductSearch.numberField.value = value[0].Number;

        var prefixText = '';
        if (invoiceProductSearch.numberField.id == 'Freight')
            prefixText = defaultFreight;
        else if (invoiceProductSearch.numberField.id == 'InvoiceFee')
            prefixText = defaultInvoiceFee;
        else if (invoiceProductSearch.numberField.id == 'CentRounding')
            prefixText = defaultCentRounding;
        else if (invoiceProductSearch.numberField.id == 'ReminderFee')
            prefixText = defaultReminderFee;
        else if (invoiceProductSearch.numberField.id == 'InterestInvoicing')
            prefixText = defaultInterestInvoicing;

        if (prefixText == undefined)
            prefixText = "";

        if (value == null)
            fixInfoLabel(invoiceProductSearch.nameField.id, invoiceProductSearch.displayDefault, prefixText);
        else if (value.Name && value.Name != null && value.Name != undefined) {
            invoiceProductSearch.nameField.innerText = "";
            if (invoiceProductSearch.displayDefault) {
                invoiceProductSearch.nameField.innerText += "(";
                invoiceProductSearch.nameField.innerText += prefixText;
                invoiceProductSearch.nameField.innerText += ") ";
            }
            invoiceProductSearch.nameField.innerText += value.Name;
        }
        else if (value.length > 0 && value[0].Name && value[0].Name != null && value[0].Name != undefined) {
            invoiceProductSearch.nameField.innerText = "";
            if (invoiceProductSearch.displayDefault) {
                invoiceProductSearch.nameField.innerText += "(";
                invoiceProductSearch.nameField.innerText += prefixText;
                invoiceProductSearch.nameField.innerText += ") ";
            }
            invoiceProductSearch.nameField.innerText += value[0].Name;
        }
    },
    searchOnKey: function (field) {
        if (invoiceProductSearch.timers[field])
            clearTimeout(invoiceProductSearch.timers[field]);
        var f = "invoiceProductSearch.searchField('" + field + "')";
        invoiceProductSearch.timers[field] = setTimeout(f, 300);
    },
    search: function (text, onlyFixed) {
        if (searchComponent && searchComponent.open) {
            searchComponent.open = false;
            searchComponent.dispose();
        }

        var url = '/ajax/getInvoiceProducts.aspx?c=' + invoiceProductSearch.companyId + '&prod=' + encodeURIComponent(text) + '&of=' + onlyFixed;
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj) {
                if (obj.length == 0) {
                    invoiceProductSearch.addWarning();
                    invoiceProductSearch.setFieldValues(null);
                    return;
                }
                else if (obj.length == 1) {
                    invoiceProductSearch.setFieldValues(obj);
                    return;
                }
                else {
                    invoiceProductSearch.render(obj);
                }
            }
        });
    },
    render: function (products) {
        var indexPrefix = "product_";
        var containerId = "searchContainer";
        var templateId = "invoiceProductSearchItem_$number$";
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
            guiObj.Number = products[counter].Number;
            guiObj.id = counter;
            guiObj.Name = products[counter].Name;
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            if (maxlengthNum <= guiObj.Number.length)
                maxlengthNum = guiObj.Number.length;
            if (maxlengthName <= guiObj.Name.length)
                maxlengthName = guiObj.Name.length;
            item.innerHTML = item.innerHTML.replace('$number$', guiObj.Number);
            item.innerHTML = item.innerHTML.replace('$name$', guiObj.Name);
            container.innerHTML += item.innerHTML;
        }
        searchComponent.init(products, container, invoiceProductSearch.timeout, invoiceProductSearch.numberField.id, indexPrefix, 'invoiceProductSearch.searchOnKey', 'invoiceProductSearch.setFieldValues', maxlengthNum, maxlengthName);
    },
    addWarning: function () {
        if (!invoiceProductSearch.hasWarning()) {
            var parent = invoiceProductSearch.numberField.parentElement.parentElement;
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
        var parent = invoiceProductSearch.numberField.parentElement.parentElement;
        if (parent.children.length > 1) {
            var tmp = parent.children[parent.children.length - 1];
            if (tmp.children && tmp.children.length > 0)
                if ($(tmp.children[0]).hasClass("warningDiv"))
                    return true;
        }
        return false;
    },
    removeWarning: function () {
        if (invoiceProductSearch.hasWarning()) {
            var parent = invoiceProductSearch.numberField.parentElement.parentElement;
            parent.removeChild(parent.children[parent.children.length - 1]);
        }
    }
}
