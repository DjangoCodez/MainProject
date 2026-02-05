function init() {

    var sysSieDimNr = document.getElementById('SysSieDimNr');
    if(sysSieDimNr == null)
        return;

    sysSieDimNr.addEvent('change', getSieName);
}

function getSieName() {
    var sysSieDimNr = document.getElementById('SysSieDimNr');
    var sysSieDimName = document.getElementById('SysSieDimName');
    if(sysSieDimNr == null || sysSieDimName == null)
    {
        return;
    }
    
    if(sysSieDimNr.value == '')
    {
        return;
    }

    var url = '/ajax/getSysTerm.aspx?sysTermId=' + sysSieDimNr.value + '&sysTermGroupId=14';
    DOMAssistant.AJAX.get(url, function (data, status) {
        try {
            var obj = JSON.parse(data);
            if (obj && obj.Found) {
                sysSieDimName.value = obj.Term;
            }
            else {
                sysSieDimName.value='';
            }
        }
        catch(err) {
            return; 
		}
    });
}

$(window).bind('load', init);
