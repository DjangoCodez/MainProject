var checkboxSelectAccount = document.getElementById('CheckBoxSelectAccount');
var checkboxSelectVoucher = document.getElementById('CheckBoxSelectVoucher');
var checkboxSelectAccountBalance = document.getElementById('CheckBoxSelectAccountBalance');

function init() {            

    if (document.getElementById('DivImportTypeSelection') == null) return;

    document.getElementById('DivAccountYearSelection').style.display = 'none';

    if (checkboxSelectAccount.checked == 'checked' || checkboxSelectAccount.checked) 
        document.getElementById('DivAccountSelection').style.display = 'inline';
    else 
        document.getElementById('DivAccountSelection').style.display = 'none';    

    if (checkboxSelectVoucher.checked == 'checked' || checkboxSelectVoucher.checked) {
        document.getElementById('DivVoucherSelection').style.display = 'inline';
        document.getElementById('DivAccountYearSelection').style.display = 'inline';
    }
    else {
        document.getElementById('DivVoucherSelection').style.display = 'none';
    }

    if (checkboxSelectAccountBalance.checked == 'checked' || checkboxSelectAccountBalance.checked) {
        document.getElementById('DivAccountBalanceSelection').style.display = 'inline';
        document.getElementById('DivAccountYearSelection').style.display = 'inline';
    }
    else {
        document.getElementById('DivAccountBalanceSelection').style.display = 'none';
    }

    document.getElementById('SkipAlreadyExistingVouchers').checked = true;
}

if (checkboxSelectAccount != null) {
    checkboxSelectAccount.onclick = function () {
        if (!toggle_visibility('DivAccountSelection')) {
            if (!check_visibility('DivVoucherSelection') && !check_visibility('DivAccountBalanceSelection')) {
                document.getElementById('DivAccountYearSelection').style.display = 'none';
            }
        }
    };
}

if (checkboxSelectVoucher != null) {
    checkboxSelectVoucher.onclick = function () {
        if (toggle_visibility('DivVoucherSelection')) {
            document.getElementById('DivAccountYearSelection').style.display = 'inline';
        }
        else {
            if (!check_visibility('DivAccountBalanceSelection')) {
                document.getElementById('DivAccountYearSelection').style.display = 'none';
            }
        }
    };
}

if (checkboxSelectAccountBalance != null) {
    checkboxSelectAccountBalance.onclick = function () {
        if (toggle_visibility('DivAccountBalanceSelection')) {
            document.getElementById('DivAccountYearSelection').style.display = 'inline';
        }
        else {
            if (!check_visibility('DivVoucherSelection')) {
                document.getElementById('DivAccountYearSelection').style.display = 'none';
            }
        }
    };
}

function toggle_visibility(id) 
{
    var e = document.getElementById(id);
    if ( e.style.display == 'inline' || e.style.display == ''){
        e.style.display = 'none';
        return false;
    }
    else{
        e.style.display = 'inline';
        return true;
    }
}

function check_visibility(id) {
    var e = document.getElementById(id);
    return e && (e.style.display == 'inline' || e.style.display == '');
}

$(window).bind('load', init);