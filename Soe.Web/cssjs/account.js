var accountMissingText = '';
var account_intervalId = -1;
var account_sysTerms = null;
var validations = 0;

function account_init()
{
    account_initTerms();
}

function account_initTerms()
{
    //create
    account_sysTerms = new Array();
    account_sysTerms.push(TermManager.createSysTerm(1, 5540, 'Konto saknas i kontoplan'));

    //validate
    account_intervalId = setInterval(account_validateTerms, TermManager.delay);
}

function account_validateTerms()
{
    validations++;
    var valid = TermManager.validateSysTerms(account_sysTerms, 'account.js');
    if (valid || TermManager.reachedMaxValidations(validations))
    {
        clearInterval(account_intervalId);
        account_setup();
    }
}

function account_setup()
{
    //Add initialization here!

    accountMissingText = TermManager.getText(account_sysTerms, 1, 5540);
}

function getAccountName(field, dimId) {
    var accField = $$(field);
    var infoText = $$(field + "-infotext");
    if (accField == null || infoText == null)
        return;

    if (accField.value == "") {
        infoText.innerText = '';
    } 
    else {
        var url = '/ajax/getAccountsByNr.aspx?acc=' + accField.value + '&dim=' + dimId;
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj && obj.length > 0)
                infoText.innerText = obj[0].Name;
            else
                infoText.innerText = TermManager.getText(account_sysTerms, 1, 5540);
        });
    }
}

function getQueryVariable(variable) {
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if (pair[0] == variable) {
            return pair[1];
        }
    }
}

//IE fix
function fixInfoLabel(id, displayDefault, value) { 
    if (id == null)
        return false;

    id = id.replace('-infotext', '');

    var element = document.getElementById(id + '-infotext');
    if(element==null) {
        var repeat='fixInfoLabel("'+id+'",'+displayDefault+')';
        setTimeout(repeat,100);
    }
    else {
        var tmp = element.innerHTML;
        if (tmp == accountMissingText)
            return;

        element.innerHTML="";
        if (value != null && value != '') {
            tmp = value;
        }
        else if (tmp.length > 4) {
            var accountNumber = tmp.substring(0, 4);
            if (isNaN(accountNumber))
                element.innerHTML = "() " + tmp;
            else
                element.innerHTML = "(" + accountNumber + ") " + tmp.substring(4);

            return false;
        }
        
        if(tmp=="()"||tmp=="() ")
            tmp = "";

        if (displayDefault)
            element.innerHTML = "(";
        element.innerHTML+=tmp;
        if(displayDefault)
            element.innerHTML+=")";
    }
}

var accountSearch =
{
    items: [],
    timeout: 300,
    dim: 0,
    accountYear: 0,
    companyId: 0,
    timers: {},
    accountNrField: {},
    displayDefault: false,

    init: function (displayDefault) {
        accountSearch.displayDefault = displayDefault;
        accountSearch.companyId = getQueryVariable("company");
    },

    keydown: function (field, e) {
       
        var evtobj = $$(window).event ? event : e;
        var key = window.event ? window.event.keyCode : e.which;
        switch (key) {
            case 8:
            case 46:
                accountSearch.searchOnKey(field);
                break;
        }
        if ((key > 47 && key < 91) || (key > 95 && key < 106))
            accountSearch.searchOnKey(field);
    },
    searchField: function (field) {
        if (accountSearch && $$(field) != null) {
            accountSearch.accountNrField = $$(field);
            accountSearch.accountNameField = $$(field + '-infotext');
            accountSearch.search(accountSearch.accountNrField.value);
        }
    },
    setFieldValues: function (value) {
        if (value != null && value.AccountNr && value.AccountNr != null && value.AccountNr != undefined)
            accountSearch.accountNrField.value = value.AccountNr;

        if (value != null)
            accountSearch.removeWarning();

        var prefixText = '';

        if (accountSearch.accountNrField.id == 'CreditAccount')
            prefixText = defaultCreditNr;
        else if (accountSearch.accountNrField.id == 'DebitAccount')
            prefixText = defaultDebitNr;
        else if (accountSearch.accountNrField.id == 'VatAccount')
            prefixText = defaultVatNr;
        else if (accountSearch.accountNrField.id == 'InterimAccount')
            prefixText = defaultInterimNr;
        else if (accountSearch.accountNrField.id == 'DebitVatFreeAccount')
            prefixText = defaultDebitVatFreeNr;

        if (prefixText == undefined)
            prefixText = "";

        if (accountSearch.accountNameField != null && accountSearch.accountNameField != undefined) {
            if (value == null) {
                fixInfoLabel(accountSearch.accountNameField.id, accountSearch.displayDefault, prefixText);
            }
            else if (value.Name && value.Name != null && value.Name != undefined) {
                accountSearch.accountNameField.innerText = "";
                if (accountSearch.displayDefault) {
                    accountSearch.accountNameField.innerText += "(";
                    accountSearch.accountNameField.innerText += prefixText;
                    accountSearch.accountNameField.innerText += ") ";
                }
                accountSearch.accountNameField.innerText += value.Name;
            }
        }
    },
    searchOnKey: function (field) {
        if (accountSearch.timers[field])
            clearTimeout(accountSearch.timers[field]);
        var f = "accountSearch.searchField('" + field + "')";
        accountSearch.timers[field] = setTimeout(f, 300);
    },
    search: function (text) {
        if (searchComponent && searchComponent.open) {
            searchComponent.open = false;
            searchComponent.dispose();
        }
        if (accountSearch.dim == 0)
            accountSearch.dim = stdDimID;

        if (SOE.isNumeric(text) && accountSearch.accountYear && accountSearch.accountYear > 0)
            accountSearch.searchSingle(text);
        else
            accountSearch.searchMultiple(text);
    },
    searchSingle: function (text) {    
        var folder = '/ajax/';
        var paramBase = '.aspx?c=' + accountSearch.companyId + '&dim=' + accountSearch.dim + '&acc=';

        var url = folder + 'getAccount' + paramBase + text + '&ay=' + accountSearch.accountYear;

        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj && obj.Found) {
                accountSearch.removeWarning();
                accountSearch.setFieldValues(obj);
                return;
            }
            else
                accountSearch.searchMultiple(text)
        });
    },
    searchMultiple: function (text) {
        var folder = '/ajax/';
        var paramBase = '.aspx?c=' + accountSearch.companyId + '&dim=' + accountSearch.dim + '&acc=';

        var url;
        if (SOE.isNumeric(text))
            url = folder + 'getAccountsByNr' + paramBase + text;
        else
            url = folder + 'getAccountsByName' + paramBase + encodeURIComponent(text);

        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj) {
                accountSearch.render(obj);
                return;
            }
            accountSearch.addWarning();
            accountSearch.setFieldValues(null); //reset suffix
        });
    },
    render: function (accounts) {
        var indexPrefix = "account_";
        var templateId = "accountSearchItem_$accountNr$";
        var template = $$(templateId);
        if (template == null)
            return;
        var container = document.createElement("div");
        container.style.display = "none";
        var maxlengthNum = 0;
        var maxlengthName = 0;
        for (var counter = 0; counter < accounts.length; counter++) {
            var item = document.createElement("div");
            item.innerHTML = template.innerHTML;
            var guiObj = new Object();
            guiObj.AccountNr = accounts[counter].AccountNr;
            guiObj.id = counter;
            guiObj.AccountName = accounts[counter].Name;
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$accountNr$', guiObj.AccountNr);
            item.innerHTML = item.innerHTML.replace('$accountName$', guiObj.AccountName);
            if (maxlengthNum <= guiObj.AccountNr.length)
                maxlengthNum = guiObj.AccountNr.length;
            if (maxlengthName <= guiObj.AccountName.length)
                maxlengthName = guiObj.AccountName.length;
            container.innerHTML += item.innerHTML;
        }
        searchComponent.init(accounts, container, accountSearch.timeout, accountSearch.accountNrField.id, indexPrefix, 'accountSearch.searchOnKey', 'accountSearch.setFieldValues', maxlengthNum, maxlengthName);
    },
    addWarning: function () {
        if (!accountSearch.hasWarning()) {
            var parent = accountSearch.accountNameField.parentElement.parentElement;
            var td = document.createElement('td');
            var warning = document.createElement('div');
            var img = document.createElement('img');
            img.src = "/img/exclamation.png";
            img.title = accountMissingText;
            $(warning).addClass('warningDiv');
            $(img).addClass('warningImg');
            warning.appendChild(img);
            td.appendChild(warning);
            parent.appendChild(td);
        }
    },
    hasWarning: function () {
        var parent = accountSearch.accountNameField.parentElement.parentElement;
        if (parent.children.length > 1) {
            var tmp = parent.children[parent.children.length - 1];
            if (tmp.children && tmp.children.length > 0)
                if ($(tmp.children[0]).hasClass("warningDiv"))
                    return true;
        }
        return false;
    },
    removeWarning: function () {
        if (accountSearch.hasWarning()) {
            var parent = accountSearch.accountNameField.parentElement.parentElement;
            parent.removeChild(parent.children[parent.children.length - 1]);
        }
    }
}

$(window).bind('load', account_init);