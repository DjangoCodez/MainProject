
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

var CustomerByNumberSearch =
{
    items: [],
    timeout: 300,
    companyId: 0,
    timers: {},
    numberField: {},
    displayDefault: false,
    init: function (displayDefault) {
        CustomerByNumberSearch.displayDefault = displayDefault;
        CustomerByNumberSearch.companyId = getQueryVariable("c");
        CustomerByNumberSearch.setFieldValues(null);
    },
    keydown: function (field, e) {
        var evtobj = window.event ? event : e;
        var key = window.event ? window.event.keyCode : e.which;
        switch (key) {
            case 8:
            case 46:
                CustomerByNumberSearch.searchOnKey(field);
                break;
        }
        if ((key > 47 && key < 91) || (key > 95 && key < 106))
            CustomerByNumberSearch.searchOnKey(field);
    },
    searchField: function (field) {

        if (CustomerByNumberSearch && $$(field) != null) {
            CustomerByNumberSearch.numberField = $$(field);
            CustomerByNumberSearch.nameField = $$(field + '-infotext');

            CustomerByNumberSearch.search(CustomerByNumberSearch.numberField.value);
        }
    },
    setFieldValues: function (value) {

        if (CustomerByNumberSearch.nameField == null) return;

        if (value != null)
            CustomerByNumberSearch.removeWarning();

        if (value != null && value.CustomerNr && value.CustomerNr != null && value.CustomerNr != undefined)
            CustomerByNumberSearch.numberField.value = value.CustomerNr;

        var prefixText = '';
             
        if (CustomerByNumberSearch.numberField.id == 'CustomerNr-from-1') {
            prefixText = value.CustomerNr;
        }
        
        //else if (CustomerByNumberSearch.numberField.id == 'InvoiceFee')
        //    prefixText = defaultInvoiceFee;
        //else if (CustomerByNumberSearch.numberField.id == 'CentRounding')
        //    prefixText = defaultCentRounding;
        //else if (CustomerByNumberSearch.numberField.id == 'ReminderFee')
        //    prefixText = defaultReminderFee;
        //else if (CustomerByNumberSearch.numberField.id == 'InterestInvoicing')
        //    prefixText = defaultInterestInvoicing;

        if (prefixText == undefined)
            prefixText = "";

        //if (value == null)
        //    fixInfoLabel(CustomerByNumberSearch.nameField.id, CustomerByNumberSearch.displayDefault, prefixText);
        //else if (value.CustomerNr && value.CustomerNr != null && value.CustomerNr != undefined)
        //{
        //    CustomerByNumberSearch.nameField.innerText = "";
        //    if (CustomerByNumberSearch.displayDefault)
        //    {
        //        CustomerByNumberSearch.nameField.innerText += "(";
        //        CustomerByNumberSearch.nameField.innerText += prefixText;
        //        CustomerByNumberSearch.nameField.innerText += ") ";
        //    }
        //    CustomerByNumberSearch.nameField.innerText += value.CustomerNr;
        //}
        //else if (value.length > 0 && value[0].CustomerNr && value[0].CustomerNr != null && value[0].CustomerNr != undefined)
        //{
        //    CustomerByNumberSearch.nameField.innerText = "";
        //    if (CustomerByNumberSearch.displayDefault)
        //    {
        //        CustomerByNumberSearch.nameField.innerText += "(";
        //        CustomerByNumberSearch.nameField.innerText += prefixText;
        //        CustomerByNumberSearch.nameField.innerText += ") ";
        //    }
        //    CustomerByNumberSearch.nameField.innerText += value[0].NameCustomerNr
        //}

        if (value.CustomerNr != null)
            copyValue('CustomerNr-from-1', 'CustomerNr-to-1', 'true');

    },
    searchOnKey: function (field) {
        if (CustomerByNumberSearch.timers[field])
            clearTimeout(CustomerByNumberSearch.timers[field]);
        var f = "CustomerByNumberSearch.searchField('" + field + "')";
        CustomerByNumberSearch.timers[field] = setTimeout(f, 300);
    },
    search: function (text) {
        if (searchComponent && searchComponent.open) {
            searchComponent.open = false;
            searchComponent.dispose();
        }
        
        var url = '/ajax/getCustomerByNumber.aspx?c=' + CustomerByNumberSearch.companyId + '&cnr=' + encodeURIComponent(text);
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj) {
                if (obj.length == 0) {
                    CustomerByNumberSearch.addWarning();
                    CustomerByNumberSearch.setFieldValues(null);
                    return;
                }
                else if (obj.length == 1) {
                    CustomerByNumberSearch.setFieldValues(obj);
                    return;
                }
                else {
                    CustomerByNumberSearch.render(obj);
                }
            }
        });
    },
    render: function (products) {
        var indexPrefix = "customer_";
        var containerId = "searchContainer";
        var templateId = "CustomerByNumberSearchItem_$customerNr$";
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
            guiObj.Number = products[counter].CustomerNr;
            guiObj.id = counter;
            guiObj.Name = products[counter].CustomerName;

            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            if (maxlengthNum <= guiObj.Number.length)
                maxlengthNum = guiObj.Number.length;
            if (maxlengthName <= guiObj.Name.length)
                maxlengthName = guiObj.Name.length;
            item.innerHTML = item.innerHTML.replace('$customerNr$', guiObj.Number);
            item.innerHTML = item.innerHTML.replace('$customerName$', guiObj.Name);
            container.innerHTML += item.innerHTML;
        }

        searchComponent.init(products, container, CustomerByNumberSearch.timeout, CustomerByNumberSearch.numberField.id, indexPrefix, 'CustomerByNumberSearch.searchOnKey', 'CustomerByNumberSearch.setFieldValues', maxlengthNum, maxlengthName);
    },
    addWarning: function () {
        if (!CustomerByNumberSearch.hasWarning()) {
            var parent = CustomerByNumberSearch.numberField.parentElement.parentElement;
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
        var parent = CustomerByNumberSearch.numberField.parentElement.parentElement;
        if (parent.children.length > 1) {
            var tmp = parent.children[parent.children.length - 1];
            if (tmp.children && tmp.children.length > 0)
                if ($(tmp.children[0]).hasClass("warningDiv"))
                    return true;
        }
        return false;
    },
    removeWarning: function () {
        if (CustomerByNumberSearch.hasWarning()) {
            var parent = CustomerByNumberSearch.numberField.parentElement.parentElement;
            parent.removeChild(parent.children[parent.children.length - 1]);
        }
    }
}
