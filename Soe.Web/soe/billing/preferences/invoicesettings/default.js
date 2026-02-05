function init() {
    UseOrderSeqNbrInternalChanged();
}

function checkTextAreaMaxLength(field, maxLen) {
    return (field.value.length <= maxLen);
}

function FakturaOCRChecked() {
    var CopyInvoiceNrToOcr = document.getElementById('CopyInvoiceNrToOcr');
    var FormReferenceToOcr = document.getElementById('FormReferenceToOcr');
    var FormFIReferenceToOcr = document.getElementById('FormFIReferenceToOcr');

    if (CopyInvoiceNrToOcr.checked == true) {
        FormReferenceToOcr.checked = false     // RF Ref can't be used if invoice no is copied to OCR
        FormFIReferenceToOcr.checked = false;  // FI Ref can't be used if invoice no is copied to OCR
    };
}

function RFOCRChecked() {
    var CopyInvoiceNrToOcr = document.getElementById('CopyInvoiceNrToOcr');
    var FormReferenceToOcr = document.getElementById('FormReferenceToOcr');
    var FormFIReferenceToOcr = document.getElementById('FormFIReferenceToOcr');

    if (FormReferenceToOcr.checked == true) 
    {
        CopyInvoiceNrToOcr.checked = false;  // Invoiceno can't be used if reference is used
    };

    if (FormReferenceToOcr.checked == false) {
        FormFIReferenceToOcr.checked = false;
    };
}

function FIOCRChecked() {
    
    const CopyInvoiceNrToOcr = document.getElementById('CopyInvoiceNrToOcr');
    const FormReferenceToOcr = document.getElementById('FormReferenceToOcr');
    const FormFIReferenceToOcr = document.getElementById('FormFIReferenceToOcr');

    if (FormFIReferenceToOcr.checked == true) {
        CopyInvoiceNrToOcr.checked = false; // Invoiceno can't be used if reference is used
        FormReferenceToOcr.checked = true;  // Requires this too. 
    };
}

function UseOrderSeqNbrInternalChanged() {
    const useOrderSeqNbrInternal = document.getElementById('UseOrderSeqNbrInternal');
    const orderSeqNbrStartInternal = document.getElementById('OrderSeqNbrStartInternal');
    if (useOrderSeqNbrInternal && orderSeqNbrStartInternal) {
        orderSeqNbrStartInternal.disabled = useOrderSeqNbrInternal.checked !== true;
    }
}

function SeqNbrPerTypeChanged() {
    var seqNbrPerType = document.getElementById('SeqNbrPerType');
    var seqNbrStart = document.getElementById('SeqNbrStart');
    var seqNbrStartDebit = document.getElementById('SeqNbrStartDebit');
    var seqNbrStartCredit = document.getElementById('SeqNbrStartCredit');
    var seqNbrStartInterest = document.getElementById('SeqNbrStartInterest');

    if (seqNbrPerType.checked == true) {
        seqNbrStart.disabled = true;
        seqNbrStartDebit.disabled = false;
        seqNbrStartCredit.disabled = false;
        seqNbrStartInterest.disabled = false;
    } else {
        seqNbrStart.disabled = false;
        seqNbrStartDebit.disabled = true;
        seqNbrStartCredit.disabled = true;
        seqNbrStartInterest.disabled = true;
    }
}


$(window).bind('load', init);