var intervalId = -1;
var sysTerms = null;
var validations = 0;

function init() {
    initTerms();
    CopyReports();
    CopyAccounts();
    CopyVoucherSeries();
}

function initTerms() {
    //create
    sysTerms = new Array();
    sysTerms.push(TermManager.createSysTerm(1, 5553, 'Demoföretag'));
    sysTerms.push(TermManager.createSysTerm(1, 5558, 'Demo'));
    sysTerms.push(TermManager.createSysTerm(1, 5918, 'Välj mallföretag'));
    sysTerms.push(TermManager.createSysTerm(1, 2225, 'OBS: Du har bockat i kopiera kunder från mallbolag. Detta innebär att samtliga kunder kopieras från mallbolaget till målbolaget, detta kan inte ångras.'));

    //validate
    intervalId = setInterval(validateTerms, TermManager.delay);
}

function validateTerms() {
    validations++;
    var valid = TermManager.validateSysTerms(sysTerms);
    if (valid || TermManager.reachedMaxValidations(validations)) {
        clearInterval(intervalId);
        setup();
    }
}

function setup() {
    //Add initialization here!
}

function CheckAllClick() {
    var className = 'checkbox copyTemplate';
    var checkAllcbx = document.getElementById('CheckAll');

    var all = document.all ? document.all : document.getElementsByTagName('*');
    //To check or uncheck all checkboxes
    if (checkAllcbx.checked == 'checked' || checkAllcbx.checked == true) {
        for (var i = 0; i < all.length; i++) {
            var element = all[i];
            if (element.className == className) {
                element.checked = 'checked';
                element.value = true;
            }
        }

    }
    else {
        for (var i = 0; i < all.length; i++) {
            var element = all[i];
            if (element.className == className) {
                element.checked = '';
                element.value = false;
            }
        }
    }

    CheckTemplateCompany();
}

function CopyReports(sender)
{
    var copyReports = document.getElementById('CopyReportsAndReportTemplates');
    //var copyPackages = document.getElementById('CopyReportPackages');
    var copyGroupsAndHeaders = document.getElementById('CopyReportGroupsAndReportHeaders');
    var copyReportSettings = document.getElementById('CopyReportSettings');
    var copyReportSelections = document.getElementById('CopyReportSelections');
    if (copyReports == null /*|| copyPackages == null*/ || copyGroupsAndHeaders == null || copyReportSettings == null || copyReportSelections == null)
        return;

    if (copyGroupsAndHeaders.checked == 'checked' || copyGroupsAndHeaders.checked || 
        copyReportSettings.checked == 'checked' ||  copyReportSettings.checked  || 
        copyReportSelections.checked == 'checked' || copyReportSelections.checked )
    {
        copyReports.checked = 'checked';
        copyReports.value = true;
    }

    CheckTemplateCompany(sender);
}

function CopyAccounts(sender)
{
    var copyAccountSTDs = document.getElementById('CopyAccountStds');
    var copyPaymentMethods = document.getElementById('CopyPaymentMethods');
    if (copyAccountSTDs == null || copyPaymentMethods == null)
        return;

    if (copyPaymentMethods.checked == 'checked' || copyPaymentMethods.checked == true) {
        copyAccountSTDs.checked = 'checked';
        copyAccountSTDs.value = true;
    }

    CheckTemplateCompany(sender);
}

function CopyVoucherSeries(sender)
{
    var copyVoucherSeries = document.getElementById('CopyVoucherSeriesTypes');
    var copyYearsAndPeriods = document.getElementById('CopyAccountYearsAndPeriods');
    if (copyVoucherSeries == null || copyYearsAndPeriods == null)
        return;

    if (copyYearsAndPeriods.checked == 'checked' || copyYearsAndPeriods.checked == true) {
        copyVoucherSeries.checked = 'checked';
        copyVoucherSeries.value = true;
    }

    CheckTemplateCompany(sender);
}

function CopyInventoryClicked(sender) {
    var copyAccountSTDs = document.getElementById('CopyAccountStds');
    var copyAccountInternals = document.getElementById('CopyAccountInternals');
    var copyVoucherSeries = document.getElementById('CopyVoucherSeriesTypes');
    var copyInventory = document.getElementById('CopyInventory'); 
    if (copyAccountSTDs == null || copyAccountInternals == null || copyVoucherSeries == null || copyInventory == null)
        return;

    if (copyInventory.checked == 'checked' || copyInventory.checked == true) {
        copyAccountSTDs.checked = 'checked';
        copyAccountSTDs.value = true;
        copyAccountInternals.checked = 'checked';
        copyAccountInternals.value = true;
        copyVoucherSeries.checked = 'checked';
        copyVoucherSeries.value = true;
    }

    CheckTemplateCompany(sender);
}

function CheckTemplateCompany(sender)
{
    var className = 'checkbox copyTemplate';
    var templateCompany = document.getElementById('TemplateCompany');
    var checkedall = document.getElementById('CheckAll');
    // We need to uncheck CheckAll if any of the indvidual checkboxes is disabled! / Jukka 2015.01.23
    if (sender != null && sender.name != 'CheckAll') {
        if (!sender.checked)
            checkedall.checked = false;
    }

    if (templateCompany == null)
        return;
    
    var showWarning = false;
    
    if (templateCompany.value == 0) {
        if (sender != null && sender.checked == true) {
            showWarning = true;
        }
        else {
            var all = document.all ? document.all : document.getElementsByTagName('*');
            for (var i = 0; i < all.length; i++) {
                var element = all[i];
                if (element.className == className) {
                    var ch = element.checked == 'checked' || element.checked == true;
                    if (ch == true) {
                        showWarning = true;
                        break;
                    }
                }
            }
        }
    }

    var copyCustomers = document.getElementById('CopyCustomers');
    if (copyCustomers != null && copyCustomers.checked == true)
        alert(TermManager.getText(sysTerms, 1, 2225));
       
    if (showWarning)
        alert(TermManager.getText(sysTerms, 1, 5918));
}

function DemoChanged() {
    var demo = document.getElementById('Demo');
    var name = document.getElementById('Name');
    var shortName = document.getElementById('ShortName');
    var orgNr = document.getElementById('OrgNr');
    if (name == null || shortName == null || demo == null || orgNr == null)
        return;

    if (demo.checked == 'checked' || demo.checked == true) {
        name.value = TermManager.getText(sysTerms, 1, 5553);
        name.disabled = 'disabled';
        shortName.value = TermManager.getText(sysTerms, 1, 5558);
        shortName.disabled = 'disabled';
        orgNr.value = '0000000000';
        orgNr.disabled = 'disabled';
    }
    else {
        name.value = '';
        name.disabled = '';
        shortName.value = '';
        shortName.disabled = '';
        orgNr.value = '';
        orgNr.disabled = '';
    }
    FormValidation.validateField(name);
    FormValidation.validateField(shortName);
    FormValidation.validateField(orgNr);
}

$(window).bind('load', init);