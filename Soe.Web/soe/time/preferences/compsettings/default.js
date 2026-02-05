function setup() {
    var submit = $$('submit');
    if (submit !== null) {
        submit.addEvent('click', onSubmit);
    }
}

function usePayrollClicked() {
    var usePayroll = $$('UsePayroll');
    if (!usePayroll)
        return;

    var forceSocSec = $$('ForceSocialSecurityNbr');
    if (forceSocSec) {
        if (usePayroll.checked === 'checked' || usePayroll.checked === true) {
            forceSocSec.checked = 'checked';
            forceSocSec.value = true;
            forceSocSec.disabled = 'disabled';
        } else {
            forceSocSec.disabled = false;
        }
    }
    var useHibernatingEmployment = $$('UseHibernatingEmployment');
    if (useHibernatingEmployment) {
        if (usePayroll.checked === 'checked' || usePayroll.checked === true) {
            useHibernatingEmployment.checked = false;
            useHibernatingEmployment.value = false;
            useHibernatingEmployment.disabled = 'disabled';
        } else {
            useHibernatingEmployment.disabled = false;
        }
    }
}

function useHibernatingEmploymentClicked() {

    var useHibernatingEmployment = $$('UseHibernatingEmployment');
    if (!useHibernatingEmployment)
        return;

    var usePayroll = $$('UsePayroll');
    if (usePayroll) {
        if (useHibernatingEmployment.checked === 'checked' || useHibernatingEmployment.checked === true) {
            usePayroll.checked = false;
            usePayroll.value = false;
            usePayroll.disabled = 'disabled';
        } else {
            usePayroll.disabled = false;
        }
    }
}

function onSubmit() {
    var forceSocSec = $$('ForceSocialSecurityNbr');
    forceSocSec.disabled = false;
}

function validatePercent(elem) {
    var value = parseFloat(elem.value);
    if (value) {
        if (value < 0)
            value = 0;
        if (value > 100)
            value = 100;
    } else {
        value = 0;
    }
    elem.value = value;
}

$(window).bind('load', function () {
    setup();
    usePayrollClicked();
});
