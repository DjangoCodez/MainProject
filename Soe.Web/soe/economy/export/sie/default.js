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
    sysTerms.push(TermManager.createSysTerm(1, 5541, 'Exporterar...'));

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

    var submit = $$('submit');
    if (submit != null) {
        submit.addEvent('click', doInformReportProgress);
    }
}

function doInformReportProgress() {
    document.elmsByClass('messageBar').each(function () {
        var messageBar = $$(this);
        var span = messageBar.elmsByTag('span')[0];
        if (span && span.className == "message") {
            span.innerText = TermManager.getText(sysTerms, 1, 5541);
        }
        return;
    });
}

$(window).bind('load', init);