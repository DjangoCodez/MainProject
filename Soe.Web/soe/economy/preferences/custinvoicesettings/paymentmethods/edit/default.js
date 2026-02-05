var intervalId = -1;
var sysTerms = null;
var validations = 0;

function init()
{
    initTerms();
}

function initTerms()
{
    //create
    sysTerms = new Array();
    sysTerms.push(TermManager.createSysTerm(1, 5540, 'Konto saknas i kontoplan'));

    //validate
    intervalId = setInterval(validateTerms, TermManager.delay);
}

function validateTerms()
{
    validations++;
    var valid = TermManager.validateSysTerms(sysTerms);
    if (valid || TermManager.reachedMaxValidations(validations))
    {
        clearInterval(intervalId);
        setup();
    }
}

function setup()
{
    //Add initialization here!
}

function getAccountName(field, dimId) {
    var accField = $$(field);
    var infoText = $$(field + "-infotext");
    if (accField == null || infoText == null)
        return;

    if (accField.value == "") {
        infoText.innerText = '';
    } else {
        var url = '/ajax/getAccountsByNr.aspx?acc=' + accField.value + '&dim=' + dimId;
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj && obj.length > 0)
                infoText.innerText = obj[0].Name;
            else
                infoText.innerText = TermManager.getText(sysTerms, 1, 5540);
        });
    }
}

$(window).bind('load', init);