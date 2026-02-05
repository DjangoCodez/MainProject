
function useLimitedEmployeeAccountDimLevelsClicked() {
    var useLimited = $$('UseLimitedEmployeeAccountDimLevels');
    var useExtended = $$('UseExtendedEmployeeAccountDimLevels');
    if (!useLimited && !useExtended)
        return;

    if (useLimited.checked === 'checked' || useLimited.checked === true) {
        useExtended.checked = '';
        useExtended.value = false;
    }
}

function useExtendedEmployeeAccountDimLevelsClicked() {
    var useLimited = $$('UseLimitedEmployeeAccountDimLevels');
    var useExtended = $$('UseExtendedEmployeeAccountDimLevels');
    if (!useLimited && !useExtended)
        return;

    if (useExtended.checked === 'checked' || useExtended.checked === true) {
        useLimited.checked = '';
        useLimited.value = false;
    } 
}

function syncAllowSupportLoginChanged() {
    var allowSupportLogin = $$('AllowSupportLogin');
    var supportLoginTo = $$('SupportLoginTo');
    var supportLoginTimeTo = $$('SupportLoginTimeTo');
    if (allowSupportLogin && supportLoginTo && supportLoginTimeTo) {
        supportLoginTo.disabled = !(allowSupportLogin.checked == 'checked' || allowSupportLogin.checked == true);
        supportLoginTimeTo.disabled = !(allowSupportLogin.checked == 'checked' || allowSupportLogin.checked == true);
    }
}

function useAccountsHierarchyClicked() {
    var useAccountsHierarchy = $$('UseAccountsHierarchy');
    var defaultEmployeeAccountDim = $$('DefaultEmployeeAccountDim');
    if (!useAccountsHierarchy || !defaultEmployeeAccountDim)
        return;
    
    if (useAccountsHierarchy.checked === 'checked' || useAccountsHierarchy.checked === true) {
        defaultEmployeeAccountDim.disabled = false;
    } else {
        defaultEmployeeAccountDim.disabled = 'disabled';
    }
}

function inexchangeChanged() {
    $('#RegisterAPI').prop("disabled", false);
    $("#RegisterAPI").removeClass("aspNetDisabled");
}
