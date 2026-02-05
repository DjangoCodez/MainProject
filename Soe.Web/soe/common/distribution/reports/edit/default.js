function Page_Start()
{
    exportTypeSelected();
}


function reportSelected(controlValueId, controlClearId)
{
    var controlValue = document.getElementById(controlValueId);
    var controlClear = document.getElementById(controlClearId);
    if (controlValue == null || controlClear == null)
    {
        return;
    }

    if (controlValue.value > 0)
    {
        controlClear.value = 0;

        var name = document.getElementById('Name');
        var reportNr = document.getElementById('ReportNr');
        if (name == null || reportNr == null)
        {
            return;
        }

        var selectedIndex = controlValue.selectedIndex;
        var text = controlValue.options[selectedIndex].text;
        if (name.value == '')
        {
            name.value = text;
            reportNr.focus();
        }
    }

    var divImportReportContent = document.getElementById('DivImportReportContent');
    if (divImportReportContent == null)
    {
        return;
    }        
}

function exportTypeSelected(parameter)
{
    var exportType = document.getElementById('ExportType');
    var exportFileType = document.getElementById('ReportExportFileType');
    if (exportType == null)
    {
        return;
    }

    if (exportFileType != null && exportFileType.parentNode != null && exportFileType.parentNode.parentNode != null)
    {
        // Value 10 = TermGroup_ReportExportType.File
        if (exportType.value == 10)
            exportFileType.parentNode.parentNode.style.visibility = 'visible';
        else
            exportFileType.parentNode.parentNode.style.visibility = 'hidden';
    }    
 }


function companySelected(reportId, sysReportTemplateId)
{
    var importCompany = document.getElementById('ImportCompany');
    var importReport = document.getElementById('ImportReport');
    if (importCompany == null || importReport == null)
    {
        return;
    }

    importReport.options.length = 0;

    if (importCompany.value > 0)
    {
        var url = '/ajax/getReports.aspx?company=' + importCompany.value + '&templatetype=' + sysReportTemplateId;
        DOMAssistant.AJAX.get(url, function (data, status)
        {
            var obj = JSON.parse(data);
            if (obj)
            {
                obj.each(function ()
                {
                    //Do not add self
                    if(this.ReportId != reportId)
                        importReport.options[parseInt(this.Position)] = new Option(this.Name, this.ReportId);
                });
            }
        });
    }
}