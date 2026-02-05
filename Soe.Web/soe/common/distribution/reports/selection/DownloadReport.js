function init()
{
    var downloadReportFlag = $$('DownloadReportFlag');
    if(downloadReportFlag != null)
    {
        if(downloadReportFlag.value == "1")
        {
            //Reset
            downloadReportFlag.value = "0";

            //Open new window
            window.open('preview/default.aspx', 'Preview', 'width=1024, height=768, resizable=1, scrollbars=1', false);
        }
    }
}

$(window).bind('load', init);