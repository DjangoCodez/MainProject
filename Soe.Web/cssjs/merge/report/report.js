var billingInvoiceReport = {
    sent: 0,
    generated: 0,
    url: '',
    isReportSent: function () {
        if (billingInvoiceReport.sent == billingInvoiceReport.generated)
            return "True";
        return "False";
    },
    generateReportAndSendEmail: function (reportUrl) {
        billingInvoiceReport.url = reportUrl;
        billingInvoiceReport.sent++;
        DOMAssistant.AJAX.get(billingInvoiceReport.url, function (response) {
            if (response == "EmailSent" || response == "UploadComplete") //no obj don't eval
                billingInvoiceReport.generated++;
        });
    },
}

var symbrioEdiReport = {
    sent: 0,
    generated: 0,
    url: '',
    reportsGenerated: function () {
        if (symbrioEdiReport.sent == symbrioEdiReport.generated)
            return "True";
        return "False";
    },
    generate: function (ediEntryId) {
        symbrioEdiReport.url = symbrioEdiReport.getRelativePath(location.pathname);
        symbrioEdiReport.sent++;
        var url = symbrioEdiReport.url + 'ajax/generateReport.aspx?ediEntry=' + ediEntryId + '&templateType=23';
        DOMAssistant.AJAX.get(url, function (data, status) { //NOSONAR
            var obj = JSON.parse(data);
            if (obj && obj.Found) {
                symbrioEdiReport.generated++;
            }
        });
    },
    generateMulti: function (ediEntryIds) {
        symbrioEdiReport.url = symbrioEdiReport.getRelativePath(location.pathname);
        symbrioEdiReport.sent++;
        var url = symbrioEdiReport.url + 'ajax/generateReport.aspx?ediEntrys=' + ediEntryIds + '&templateType=23';
        DOMAssistant.AJAX.get(url, function (data, status) { //NOSONAR
            var obj = JSON.parse(data);
            if (obj && obj.Found) {
                symbrioEdiReport.generated++;
            }
        });
    },
    getRelativePath: function (path) {
        var count = 0;
        var relative = "";
        for (var i = 0; i < path.length; i++) {
            if (path.substr(i, 1) == "/")
                count++;
        }
        for (var j = 1; j < count; j++) {
            relative += "../";
        }
        return relative;
    }
}
