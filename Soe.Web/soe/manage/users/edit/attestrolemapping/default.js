function AttestRoleChecked(suffix) {
    var check = document.getElementById('Check_' + suffix);
    var maxAmount = document.getElementById('MaxAmount_' + suffix);
    if (check == null || maxAmount == null)
        return;

    if (check.checked == 'checked' || check.checked == true)
        maxAmount.disabled = '';
    else
        maxAmount.disabled = 'disabled';
}