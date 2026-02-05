var INTERVALTYPE_MANUALLY = "1";
 var intervalId = -1;
var sysTerms = null;
var validations = 0;
var nrOfDecimals = 4;

var _selectedCurrencyId = {
    getValue: function(){
        return this._selectedCurrencyId;
    },
    setValue: function(id){
        this._selectedCurrencyId = id;
    }
}

function init()
{
    initTerms();
}

function initTerms()
{
    //create
    sysTerms = new Array();
    sysTerms.push(TermManager.createSysTerm(1, 4075, 'Basvaluta/valuta'));
    sysTerms.push(TermManager.createSysTerm(1, 4074, 'Valuta/basvaluta'));

    //validate
    intervalId = setInterval(validateTerms, TermManager.delay);

    this._selectedCurrencyId = 0;
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

function toggleCurrencyRate() {
    var intervalType = $$("IntervalType");
    var rateToBase = $$("RateToBase");
    var rateDate = $$("RateDate");
    if (intervalType == null || rateToBase == null || rateDate == null)
        return;

    if (intervalType.value == INTERVALTYPE_MANUALLY) {
        rateToBase.disabled = '';
        rateDate.disabled = '';
    }
    else {
        rateToBase.disabled = 'disabled';
        rateDate.disabled = 'disabled';
    }
}

function getCurrency() {
    var selectList = $$("Code");
    var rateToBase = $$("RateToBase");
    var rateToBaseInfo = $$("RateToBase-infotext");
    var rateFromBase = $$("RateFromBase");
    var rateFromBaseInfo = $$("RateFromBase-infotext");
    var title = $$("formtitle");

    for (var i = 0; i < selectList.length; i++) {
        if (selectList[i].selected && selectList[i].selected == true) {
            _selectedCurrencyId = selectList[i].value;
            break;
        }
    }

    console.log(_selectedCurrencyId);

    var url = '/ajax/getCurrency.aspx?currencyId=' + _selectedCurrencyId + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
    DOMAssistant.AJAX.get(url, function (data, status) {
        var obj = JSON.parse(data);
        if (obj && obj.length > 0 && obj[0].Found) {
            if (title)
                title.value = obj[0].Name;
            selectList.infoText = obj[0].Name;

            if (rateToBase != null && rateFromBase != null) {

                //RateToBase
                rateToBase.parentNode.parentNode.getElementsByTagName("TH")[0].innerText = TermManager.getText(sysTerms, 1, 4075);
                rateToBase.innerText = obj[0].RateToBase;
                if (rateToBaseInfo != null)
                    rateToBaseInfo.innerText = obj[0].RateToBaseInfo;

                //RateFromBase
                rateFromBase.parentNode.parentNode.getElementsByTagName("TH")[0].innerText = TermManager.getText(sysTerms, 1, 4074);
                rateFromBase.innerText = obj[0].RateFromBase;
                if (rateFromBaseInfo != null)
                    rateFromBaseInfo.innerText = obj[0].RateFromBaseInfo;

                if (obj[0].IntervalType == 1) {
                    rateToBase.disabled = '';
                }
                else {
                    rateToBase.disabled = 'disabled';
                }
            }

            return;
        }
    });
}

function SetRateFromBase() {
    var rateToBase = $$("RateToBase");
    var rateFromBase = $$("RateFromBase");
    if (rateToBase == null || rateFromBase == null)
        return;

    var rateToBaseValue = 0;
    var rateFromBaseValue = 0;

    if (rateToBase.value.length > 0) {
        rateToBaseValue = parseFloat(rateToBase.value.replace(',', '.'));
        if (rateToBaseValue > 0) {
            rateFromBaseValue = 1 / parseFloat(rateToBaseValue);
        }
    }

    if (rateToBaseValue > 0)
        rateToBaseValue = rateToBaseValue.toFixed(nrOfDecimals)
    if (rateFromBaseValue > 0)
        rateFromBaseValue = rateFromBaseValue.toFixed(nrOfDecimals)

    rateToBase.value = rateToBaseValue;
    rateFromBase.value = rateFromBaseValue;
}

$(window).bind('load', init);