var exportTypeId = 'ExportType'
var reportClassName = 'ReportExport';
//var xmlClassName = 'XmlExport';
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
    sysTerms.push(TermManager.createSysTerm(1, 5542, 'Skriver ut rapport...'));
    sysTerms.push(TermManager.createSysTerm(1, 5969, 'Utskrift klar'));

    //validate
    intervalId = setInterval(validateTerms, TermManager.delay);
}

function validateTerms()
{
    validations++;
    var valid = TermManager.validateSysTerms(sysTerms, 'default.js');
    if (valid || TermManager.reachedMaxValidations(validations))
    {
        clearInterval(intervalId);
        setup();
    }
}

function setup()
{
    //Add initialization here!

    showHideExportType();

    var runReport = $$('runreport');
    if (runReport != null) {
        runReport.addEvent('click', doInformReportProgress);
    }
}

function doInformReportProgress() 
{
    document.elmsByClass('messageBar').each(function () {
        var messageBar = $$(this);
        var span = messageBar.elmsByTag('span')[0];
        if (span && span.className == "message") {
            span.innerText = TermManager.getText(sysTerms, 1, 5542);
            var readyStateCheckInterval = setInterval(function () {
                if (document.readyState == "complete") {
                    clearInterval(readyStateCheckInterval);
                    span.innerText = TermManager.getText(sysTerms, 1, 5969);
                }
            }, 10);
        }
        return;
    });
}

function showHideExportType() 
{
    var exportType = document.getElementById(exportTypeId); 
    if (exportType == null)  
    {  
        return; 
    }
    
    //ReportOnly
    if(exportType.value == 1)
    {
        showExportType(reportClassName);
        //hideExportType(xmlClassName);
    }
    //XML
    else if(exportType.value == 2)
    {
        hideExportType(reportClassName);
        //showExportType(xmlClassName);
    }
    else
    {
        hideExportType(reportClassName);
        //hideExportType(xmlClassName);
    }
}

function showExportType (className) 
{
    var all = document.all ? document.all : document.getElementsByTagName('*');    
    for (var i = 0; i < all.length; i++)
    {
        var element = all[i];
        if (element.className == className)
        {
            element.style.display = '';
        }
    }
}

function hideExportType (className) 
{
    var all = document.all ? document.all : document.getElementsByTagName('*');    
    for (var i = 0; i < all.length; i++)
    {
        var element = all[i];
        if (element.className == className)
        {
            element.style.display = 'none';
        }
    }
}

function masterKeyDown(e, formElementId) {
    e = e || event;
    //13 = Enter, 9 = Tab
    if (e.keyCode == 13) { 
        event.keyCode = 9; //Tab

        return false;
    }
}

$(window).bind('load', init);
