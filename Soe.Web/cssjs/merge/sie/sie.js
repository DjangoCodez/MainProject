function sieExportAccountChanged()
{
    var expAcc = document.getElementById('ExportAccount');
 	var expAccType = document.getElementById('ExportAccountType');
 	var expSru = document.getElementById('ExportSruCodes');
    if(expAcc == null || expAccType == null || expSru == null)
    {
        return;
    }
       
    if(expAcc.checked == 'checked' || expAcc.checked == true)
    {
        expAccType.checked = 'checked';
        expAccType.value = true;
        expAccType.disabled = '';
        
        expSru.checked = 'checked';
        expSru.value = true;
        expSru.disabled = '';
        return;
    }
    
    if(expAcc.checked == '' || expAcc.checked == false)
    {
        expAccType.checked = '';
        expAccType.value = false;
        expAccType.disabled = 'disabled';
        
        expSru.checked = '';
        expSru.value = false;
        expSru.disabled = 'disabled';
        return;
    }
}

function checkVoucherSeries()
{
    var voucherSeries = $$('VoucherSeries');
    var overrideVoucherSeries = $$('OverrideVoucherSeries');
    if(voucherSeries == null && overrideVoucherSeries == null)
    {
        return;
    }
    
    if(voucherSeries.value == 0)
    {
        overrideVoucherSeries.checked = '';
        overrideVoucherSeries.disabled = 'disabled';
    }
    else
    {
        overrideVoucherSeries.disabled = '';
    }

    checkVoucherSeriesMapping();
    checkVoucherDelete();
}

function checkVoucherSeriesMapping()
{
    var overrideVoucherSeries = $$('OverrideVoucherSeries');
    var divVoucherSeriesMapping = $$('DivVoucherSeriesMapping');
    if (overrideVoucherSeries == null && divVoucherSeriesMapping == null)
    {
        return;
    }

    if (overrideVoucherSeries.checked == 'checked' || overrideVoucherSeries.checked == true)
    {
        divVoucherSeriesMapping.style.display = 'none';
    }
    else
    {
        divVoucherSeriesMapping.style.display = '';
    }
}

function checkVoucherDelete() {
    var overrideVoucherSeriesDelete = $$('OverrideVoucherSeriesDelete');
    var divVoucherSeriesDelete = $$('DivVoucherSeriesDelete');
    if (overrideVoucherSeriesDelete == null && divVoucherSeriesDelete == null) {
        return;
    }

    if (overrideVoucherSeriesDelete.checked == 'checked' || overrideVoucherSeriesDelete.checked == true) {
        divVoucherSeriesDelete.style.display = 'none';
    }
    else {
        divVoucherSeriesDelete.style.display = '';
    }
}
function getVoucherSeries()
{
    var accYear = document.getElementById('AccountYear');
    var voucherSeries = document.getElementById('VoucherSeries');
    var overrideVoucherSeries = $$('OverrideVoucherSeries');
    if(accYear == null || voucherSeries == null || overrideVoucherSeries == null)
    {
        return;
    }
    
    voucherSeries.options.length = 0;
    checkVoucherSeries();
    var url = '/ajax/getVoucherSeries.aspx?ay=' + accYear.value + '&inctemp=0&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
    DOMAssistant.AJAX.get(url, function (data, status) {
        var obj = JSON.parse(data);	    
        if (obj)
	    {
	        var index = 1;        
	        
            //empty selection
            var opt = document.createElement('OPTION');
            opt.value = 0;
            opt.text = '';
            voucherSeries.options.add(opt, index);
            
            obj.each(function ()
            {
                var opt = document.createElement('OPTION');
                opt.value = this.VoucherSeriesId;
                opt.text = this.Nr + ". " + this.Name;
                voucherSeries.options.add(opt, index);
                index++;
            });
	    }
	    else
	    {
            overrideVoucherSeries.disabled = 'disabled';
            overrideVoucherSeries.checked = '';
	    }
	});
}