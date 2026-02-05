function init() {
    var sysSieDimNr = document.getElementById('SysSieDimNr');
    if (sysSieDimNr == null)
        return;

    sysSieDimNr.addEvent('change', getSieName);
}

function enableDisableFields() {
    var sysPaymentMethod = document.getElementById('SysPaymentMethod');
    var paymentInformation = document.getElementById('PaymentInformation');
    var customerNr = document.getElementById('CustomerNr');
    var selectedIndex = sysPaymentMethod.selectedIndex;
    var option = sysPaymentMethod.options[selectedIndex];
    var sysPaymentMethodId = option.value;
    if (isExportablePaymentMethod(sysPaymentMethodId)) {
        paymentInformation.disabled = false;
        customerNr.disabled = false;
    }
    else {
        paymentInformation.disabled = true;
        customerNr.disabled = true;
        paymentInformation.selectedIndex = null;
        customerNr.value = '';
    }
}

function isExportablePaymentMethod(sysPaymentMethodId) {
    return sysPaymentMethodId != '5' && sysPaymentMethodId != '6';
}

$(window).bind('load', function () {
    enableDisableFields() });