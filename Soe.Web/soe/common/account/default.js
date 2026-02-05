function init() {
    var vatAccount = $$('VatAccount');
    if (vatAccount) {
        vatAccount.addEvent('change', getSysVatRate);
        getSysVatRate();
    }
}

function getSysVatRate() {
    var va = $$('VatAccount');
    if (va.selectedIndex >= 0) {
        var vatAccId = va[va.selectedIndex].value;
        va.value = vatAccId;
        var url = '/ajax/getSysVatRate.aspx?vatAccId=' + vatAccId;
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj.Found) {
                $$('VatAccountRate').value = obj.Value + '%';
            }
        });
    } else {
        $$('VatAccountRate').value = '0%';
    }
}

function checkSysAccountStdParent() {
    var accountNr = $$('AccountNr');
    var accountName = $$('Name');
    var accountType = $$('AccountType');
    var amountStop = $$('AmountStop');
    var unit = $$('Unit');
    var unitStop = document.getElementById('UnitStop');
    var sru1 = $$('AccountSru1');
    var sru2 = $$('AccountSru2');
    if (accountNr == null || accountName == null || accountType == null ||
        amountStop == null || unit == null || unitStop == null || sru1 == null || sru2 == null) {
        return;
    }

    accountName.value = '';
    if (accountNr.value) {
        var url = '/ajax/getSysAccountStdParent.aspx?acc=' + accountNr.value;
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj.Found) {
                accountName.value = obj.Name;
                accountType.value = obj.AccountType;
                amountStop.value = obj.AmountStop;
                unit.value = obj.Unit;
                unitStop.value = obj.UnitStop;
                if (unitStop.value == 'True')
                    unitStop.checked = 'checked';
                else
                    unitStop.checked = '';
                sru1.value = obj.AccountSru1;
                sru2.value = obj.AccountSru2;
            }
        });
    }
}

function checkMandatoryLevel(checkBoxId) {
    var dimId;
    var elem1;
    var elem2;

    if (document.getElementById(checkBoxId).checked == true) {
        if (Right(checkBoxId, 5) == "_warn") {
            dimId = Left(checkBoxId, checkBoxId.length - 5);
            elem1 = document.getElementById(dimId + "_mandatory");
            elem2 = document.getElementById(dimId + "_stop");
            
            // If disabled, clear internal account
            var acc = document.getElementById(dimId + "_default");
            acc.value = 0;
        }
        else if (Right(checkBoxId, 10) == "_mandatory") {
            dimId = Left(checkBoxId, checkBoxId.length - 10);
            elem1 = document.getElementById(dimId + "_warn");
            elem2 = document.getElementById(dimId + "_stop");
        }
        else if (Right(checkBoxId, 5) == "_stop") {
            dimId = Left(checkBoxId, checkBoxId.length - 5);
            elem1 = document.getElementById(dimId + "_warn");
            elem2 = document.getElementById(dimId + "_mandatory");
        }
        elem1.checked = false;
        elem2.checked = false;
    }
}

function internalAccountChanged(comboBoxId) {
    var dimId = Left(comboBoxId, comboBoxId.length - 8);
    var checkBox = document.getElementById(dimId + "_warn");
    checkBox.checked = false;
}

$(window).bind('load', init);
