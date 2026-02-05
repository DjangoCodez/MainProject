//Has DistributionSelectionStd_ suffix to avoid name collisions when this UserControl is placed inside a page
var DistributionSelectionStd_intervalId = -1;
var DistributionSelectionStd_sysTerms = null;
var DistributionSelectionStd_validations = 0;

function DistributionSelectionStd_init()
{
    DistributionSelectionStd_initTerms();    
}

function DistributionSelectionStd_initTerms()
{
    //create
    DistributionSelectionStd_sysTerms = new Array();
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5530, 'Det saknas inställningar för att kunna skapa momsavräkningsverifikat'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5531, 'Standardserie för momsavräkningsverifikat saknas'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5532, 'Konto för momsredovisning saknas'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5533, 'Kontakta SoftOne support om du vill få hjälp att få det korrekt uppsatt'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5535, 'Det finns ett momsavräkningsverifikat för vald period. Det måste tas bort eller motbokas innan nytt kan skapas'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5539, 'Varning! Det finns ett momsavräkningsverifikat för en senare period än den som är vald'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5536, 'Ver.nr'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5537, 'Ver.datum'));
    DistributionSelectionStd_sysTerms.push(TermManager.createSysTerm(1, 5538, 'Ver.text'));   

    //validate
    DistributionSelectionStd_intervalId = setInterval(DistributionSelectionStd_validateTerms, TermManager.delay); 
}

function DistributionSelectionStd_validateTerms()
{
    DistributionSelectionStd_validations++;
    var valid = TermManager.validateSysTerms(DistributionSelectionStd_sysTerms, 'DistributionSelectionStd');
    if (valid || TermManager.reachedMaxValidations(DistributionSelectionStd_validations))
    {
        clearInterval(DistributionSelectionStd_intervalId);
        DistributionSelectionStd_setup();
    }
}

function DistributionSelectionStd_setup()
{
    
    //Add initialization here!
    var accPerFrom = $$('AccountPeriod-from-1');
    if (accPerFrom != null) {
        if (accPerFrom.value == 0) {
            getAccPerFrom();
            disableAccPerFrom();
        }
    }

    var accPerTo = $$('AccountPeriod-to-1');
    if (accPerTo) {
        if (accPerTo.value == 0) {
            getAccPerToEvent();
            disableAccPerTo();
        }
    }

    const accYearFrom = $$('AccountYear-from-1');
    const accYearTo = $$('AccountYear-to-1');

    if (accYearFrom != null) {
        if (accYearTo == null) {
            accYearFrom.addEvent('change', getAccPer);
        }
        else {
            accYearFrom.addEvent('change', getAccPerFrom);
        }
        accYearFrom.addEvent('change', getVoucherSeries);
    }
        
    if (accYearTo != null) {
        accYearTo.addEvent('change', getAccPerToEvent);
        accYearTo.addEvent('change', getVoucherSeries);
    }

    if (isVatVoucherReport() == true)
        checkVatVoucherSettings();
    showHideDateSelection();
}

function disableAccPerFrom()
{
    var accPerFrom = document.getElementById('AccountPeriod-from-1');
    if(accPerFrom != null)
    {
        if(accPerFrom.options.length == 0)
            accPerFrom.disabled = 'disabled';
    }
}

function disableAccPerTo()
{
    var accPerTo = document.getElementById('AccountPeriod-to-1');        
    if(accPerTo)
    {
        if(accPerTo.options.length == 0)
            accPerTo.disabled = 'disabled';
    }
}

function getAccPer() {
    
    const accYearFrom = document.getElementById('AccountYear-from-1');

    getAccPerFrom();
    getAccPerTo(accYearFrom.value);
}
function getAccPerFrom()
{
    var accYearFrom = document.getElementById('AccountYear-from-1');
    var accPerFrom = document.getElementById('AccountPeriod-from-1');
    var accPerTo = document.getElementById('AccountPeriod-to-1');
    if(accYearFrom == null || accPerFrom == null)
    {
        return;
    }
    
    if(accYearFrom.value == 0)
    {         
        accPerFrom.options.length = 0;
        accPerFrom.disabled = "disabled";
        return;
    }
   
    var url = '/ajax/getAccountPeriods.aspx?year=' + accYearFrom.value;
    DOMAssistant.AJAX.get(url, function (data, status)
    {
        var obj = JSON.parse(data);
        if (obj)
	    {
            var fillAccPerTo = false;
            if(accPerTo != null)
            {
                fillAccPerTo = accPerTo.disabled;
            }
                
            accPerFrom.disabled = '';
            accPerFrom.options.length = 0;
            if(fillAccPerTo)
            {
                accPerTo.disabled = '';
                accPerTo.options.length = 0;
            }
            
            obj.each(function ()
            {
	            accPerFrom.options[parseInt(this.Position)] = new Option(this.Interval, this.AccountPeriodId);
	            if(fillAccPerTo)
                    accPerTo.options[parseInt(this.Position)] = new Option(this.Interval, this.AccountPeriodId);
            });
	    }
    });
}

function getAccPerToEvent() {
    getAccPerTo(null);
}
function getAccPerTo(accYearToValue)
{
    if (!accYearToValue) {
        var accYearTo = document.getElementById('AccountYear-to-1');
        if (accYearTo == null) {
            return;
        }

        accYearToValue = accYearTo.value;
    }
    
    const accPerTo = document.getElementById('AccountPeriod-to-1');

    if(accPerTo == null)
    {
        return;
    }
    
    if (accYearToValue == "0")
    {    
        accPerTo.options.length = 0;
        accPerTo.disabled = "disabled";
        return;
    }

    var url = '/ajax/getAccountPeriods.aspx?year=' + accYearToValue;
    DOMAssistant.AJAX.get(url, function (data, status)
    {
        var obj = JSON.parse(data);
        if (obj.length)
	    {            
            accPerTo.disabled = '';
            accPerTo.options.length = 0;
            obj.each(function ()
            {
                accPerTo.options[parseInt(this.Position)] = new Option(this.Interval, this.AccountPeriodId);
            });
	    }
    });
}

function getVoucherSeries()
{
    var accYearFrom = $$('AccountYear-from-1');
    var accYearTo = $$('AccountYear-to-1');
    var voucherSeriesFrom = $$('VoucherSeries-from-1');
    var voucherSeriesTo = $$('VoucherSeries-to-1');
    if (accYearFrom == null || accYearTo == null || voucherSeriesFrom == null || voucherSeriesTo == null)
    {
        return;
    }
    
    //empty selection
    voucherSeriesFrom.options.length = 0;
    voucherSeriesTo.options.length = 0;

    if (accYearFrom.value == 0 || accYearTo == null)
    {
        return;
    }

    var url = '/ajax/getVoucherSeries.aspx?ay=' + accYearFrom.value + '&ayto=' + accYearTo.value + '&inctemp=0';
    DOMAssistant.AJAX.get(url, function (data, status)
    {
        var obj = JSON.parse(data);
        if (obj)
        {
            var index = 1;

            //empty selection from
            var optFromEmpty = document.createElement('OPTION');
            optFromEmpty.value = 0;
            optFromEmpty.text = '';
            voucherSeriesFrom.options.add(optFromEmpty, index);

            //empty selection to
            var optToEmpty = document.createElement('OPTION');
            optToEmpty.value = 0;
            optToEmpty.text = '';
            voucherSeriesTo.options.add(optToEmpty, index);
            
            obj.each(function ()
            {
                var optFrom = document.createElement('OPTION');
                optFrom.value = this.Nr;
                optFrom.text = this.Nr + ". " + this.Name;
                voucherSeriesFrom.options.add(optFrom, index);

                var optTo = document.createElement('OPTION');
                optTo.value = this.Nr;
                optTo.text = this.Nr + ". " + this.Name;
                voucherSeriesTo.options.add(optTo, index);
                index++;
            });
        }
    });
}

function showHideDateSelection()
{
    var divDate = document.getElementById('DivDate');
    var divYearAndPeriod = document.getElementById('DivYearAndPeriod');
    var dateSelection = document.getElementById('DateSelection');
    if (divDate == null || divYearAndPeriod == null || dateSelection == null)
    {
        return;
    }

    var dateSelectionChecked = dateSelection.checked;
    divDate.style.display = (dateSelectionChecked ? '' : 'none');
    divYearAndPeriod.style.display = (dateSelectionChecked ? 'none' : '');

    /*
    if (dateSelectionChecked)
    {
        getVoucherSeries();
    }
    else
    {
        var voucherSeriesFrom = $$('VoucherSeries-from-1');
        var voucherSeriesTo = $$('VoucherSeries-to-1');
        if (voucherSeriesFrom == null || voucherSeriesTo == null)
        {
            return;
        }

        voucherSeriesFrom.options.length = 0;
        voucherSeriesTo.options.length = 0;
    }*/
}

function isVatVoucherReport() {
    var res = false;
    var div = $$('DivTaxAudit');
    if (div != null) {
        var table = div.getElementsByTagName('table')[0];
        if (table != null) {
            var hidden = table.getElementsByTagName('input')[0];
            if (hidden != null && hidden.value == "1")
                res = true;
        }
    }
    return res;
}

function disableCreateVatVoucher() {
    var createVatVoucher = $$('CreateVatVoucher');
    if (createVatVoucher != null)
        createVatVoucher.disabled = 'disabled';
}

function enableCreateVatVoucher() {
    var createVatVoucher = $$('CreateVatVoucher');
    if (createVatVoucher != null)
        createVatVoucher.disabled = '';
}

function checkVatVoucherSettings() {
    var url = '/ajax/checkVatVoucherSettings.aspx?timestamp=' + new Date().getTime(); //make each request unique to prevent cache
    DOMAssistant.AJAX.get(url, function (data, status) {
        var obj = JSON.parse(data);
        if (obj && obj.Found) {
            if (obj.AllValid) {
                var accPerTo = $$('AccountPeriod-to-1');
                accPerTo.addEvent('change', getVatVoucher);

                getVatVoucher();
            }
            else {
                disableCreateVatVoucher();

                //Show message
                var message = '';
                message += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5530) + '\r\n\n';
                if (!obj.VoucherSeriesValid)
                    message += '- ' + TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5531) + '\r\n';
                if (!obj.AccountValid)
                    message += '- ' + TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5532) + '\r\n';
                message += '\r\n' + TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5533);
                alert(message);
            }
        }
    });
}

function getVatVoucher() {
    enableCreateVatVoucher();

    if (isVatVoucherReport()) {
        var accPerTo = $$('AccountPeriod-to-1');
        if (accPerTo) {
            var url = '/ajax/getVatVoucher.aspx?period=' + accPerTo.value + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
            DOMAssistant.AJAX.get(url, function (data, status) {
                var obj = JSON.parse(data);
                if (obj.Found) {
                    if (obj.VatVoucherExists) {
                        disableCreateVatVoucher();
                        var message1 = TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5535) + '\r\n\n';
                        message1 += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5536) + ':\t\t' + obj.VoucherNr + '\r\n';
                        message1 += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5537) + ':\t' + obj.Date + '\r\n';
                        message1 += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5538) + ':\t\t' + obj.Text;
                        alert(message1);
                    }
                    else if (obj.VatVoucherExistsLaterThanPeriod){
                        var message2 = TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5539) + '\r\n\n';
                        message2 += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5536) + ':\t\t' + obj.VoucherNr + '\r\n';
                        message2 += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5537) + ':\t' + obj.Date + '\r\n';
                        message2 += TermManager.getText(DistributionSelectionStd_sysTerms, 1, 5538) + ':\t\t' + obj.Text;
                        alert(message2);
                    }
                }
            });
        }
    }
    return;
}

$(window).bind('load', DistributionSelectionStd_init);
