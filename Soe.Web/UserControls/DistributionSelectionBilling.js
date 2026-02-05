function disableEnableInvoiceCopyAsOriginal() {

    var showCopy = document.getElementById('ShowCopy');
    var invoiceCopyAsOriginal = document.getElementById('InvoiceCopyAsOriginal');

    if (showCopy != null && invoiceCopyAsOriginal != null) 
    {
        if (showCopy.checked == 'checked' || showCopy.checked == true)
        {
            invoiceCopyAsOriginal.disabled = '';
        }
        else
        {
            invoiceCopyAsOriginal.disabled = 'disabled';
        }
    }

}