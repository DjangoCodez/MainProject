function init() {
    ReminderGenerateProductRowChecked();
    SeqNbrPerTypeChanged();
}

function ReminderGenerateProductRowChecked() {
    var productRow = $$('ReminderGenerateProductRow');
    var newInvoice = $$('ReminderHandlingTypeNew');
    var nextInvoice = $$('ReminderHandlingTypeNext');
    if (productRow == null || nextInvoice == null)
        return;

    var productRowChecked = productRow.checked == 'checked' || productRow.checked == true;
    var newInvoiceChecked = newInvoice.checked == 'checked' || newInvoice.checked == true;
    var nextInvoiceChecked = nextInvoice.checked == 'checked' || nextInvoice.checked == true;

    /* newInvoice and nextInvoice only enabled when productRow is checked. NewInvoice is default. */
    if (productRowChecked) {
        newInvoice.disabled = '';
        nextInvoice.disabled = '';

        //Set default
        if (newInvoiceChecked == false && nextInvoiceChecked == false) {
            newInvoice.checked = 'checked';
            newInvoice.checked = true;
        }
    }
    else {
        newInvoice.checked = '';
        newInvoice.checked = false;
        newInvoice.disabled = 'disabled';

        nextInvoice.checked = '';
        nextInvoice.checked = false;
        nextInvoice.disabled = 'disabled';
        
    }
}

function ReminderHandlingTypeNewChecked() {
    var newInvoice = $$('ReminderHandlingTypeNew');
    var nextInvoice = $$('ReminderHandlingTypeNext');
    if (newInvoice == null || nextInvoice == null)
        return;

    if (newInvoice.checked == 'checked' || newInvoice.checked == true) {
        nextInvoice.checked = '';
        nextInvoice.checked = false;
    }
    else {
        nextInvoice.checked = 'checked';
        nextInvoice.checked = true;
    }
}

function ReminderHandlingTypeNextChecked() {
    var newInvoice = $$('ReminderHandlingTypeNew');
    var nextInvoice = $$('ReminderHandlingTypeNext');
    if (newInvoice == null || nextInvoice == null)
        return;

    if (nextInvoice.checked == 'checked' || nextInvoice.checked == true) {
        newInvoice.checked = '';
        newInvoice.checked = false;
    }
    else {
        newInvoice.checked = 'checked';
        newInvoice.checked = true;
    }
}

function InterestHandlingTypeNewChecked() {
    var newInvoice = $$('InterestHandlingTypeNew');
    var nextInvoice = $$('InterestHandlingTypeNext');
    if (newInvoice == null || nextInvoice == null)
        return;

    if (newInvoice.checked == 'checked' || newInvoice.checked == true) {
        nextInvoice.checked = '';
        nextInvoice.checked = false;
    }
}

function InterestHandlingTypeNextChecked() {
    var newInvoice = $$('InterestHandlingTypeNew');
    var nextInvoice = $$('InterestHandlingTypeNext');
    if (newInvoice == null || nextInvoice == null)
        return;

    if (nextInvoice.checked == 'checked' || nextInvoice.checked == true) {
        newInvoice.checked = '';
        newInvoice.checked = false;
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

function AgeDistNbrOfIntervalsChanged() {
    var nbrOfIntervals = $$('AgeDistNbrOfIntervals');
    var interval3 = $$('AgeDistInterval3');
    var interval4 = $$('AgeDistInterval4');
    var interval5 = $$('AgeDistInterval5');

    if (nbrOfIntervals.value < 6) {
        hideField(interval5);
    }
    else {
        showField(interval5);
    }

    if (nbrOfIntervals.value < 5) {
        hideField(interval4);
    }
    else {
        showField(interval4);
    }

    if (nbrOfIntervals.value < 4) {
        hideField(interval3);
    }
    else {
        showField(interval3);
    }

    SetAgeDistributionExample();
}

function SetAgeDistributionExample() {
    var nbrOfIntervals = $$('AgeDistNbrOfIntervals');
    var interval1 = $$('AgeDistInterval1');
    var interval2 = $$('AgeDistInterval2');
    var interval3 = $$('AgeDistInterval3');
    var interval4 = $$('AgeDistInterval4');
    var interval5 = $$('AgeDistInterval5');
    var example = $$('AgeDistExample');

    var ex = '< ';
    ex += interval1.value;
    ex += '    |    ';
    ex += interval1.value + '-' + (Number(interval2.value) - 1);
    ex += '    |    ';

    if (nbrOfIntervals.value > 3) {
        ex += interval2.value + '-' + (Number(interval3.value) - 1);
        ex += '    |    ';

        if (nbrOfIntervals.value > 4) {
            ex += interval3.value + '-' + (Number(interval4.value) - 1);
            ex += '    |    ';

            if (nbrOfIntervals.value > 5) {
                ex += interval4.value + '-' + (Number(interval5.value) - 1);
                ex += '    |    ';
                ex += '> ' + (Number(interval5.value) - 1);
            }
            else {
                ex += '> ' + (Number(interval4.value) - 1);
            }
        }
        else {
            ex += '> ' + (Number(interval3.value) - 1);
        }
    }
    else {
        ex += '> ' + (Number(interval2.value) - 1);
    }

    example.innerHTML = ex;
}

/*function LiqPlanNbrOfIntervalsChanged() {
    var nbrOfIntervals = $$('LiqPlanNbrOfIntervals');
    var interval3 = $$('LiqPlanInterval3');
    var interval4 = $$('LiqPlanInterval4');
    var interval5 = $$('LiqPlanInterval5');

    if (nbrOfIntervals.value < 6) {
        hideField(interval5);
    }
    else {
        showField(interval5);
    }

    if (nbrOfIntervals.value < 5) {
        hideField(interval4);
    }
    else {
        showField(interval4);
    }

    if (nbrOfIntervals.value < 4) {
        hideField(interval3);
    }
    else {
        showField(interval3);
    }

    SetLiquidityPlanningExample();
}

function SetLiquidityPlanningExample() {
    var nbrOfIntervals = $$('LiqPlanNbrOfIntervals');
    var interval1 = $$('LiqPlanInterval1');
    var interval2 = $$('LiqPlanInterval2');
    var interval3 = $$('LiqPlanInterval3');
    var interval4 = $$('LiqPlanInterval4');
    var interval5 = $$('LiqPlanInterval5');
    var example = $$('LiqPlanExample');

    var ex = '< ';
    ex += interval1.value;
    ex += '    |    ';
    ex += interval1.value + '-' + (Number(interval2.value) - 1);
    ex += '    |    ';

    if (nbrOfIntervals.value > 3) {
        ex += interval2.value + '-' + (Number(interval3.value) - 1);
        ex += '    |    ';

        if (nbrOfIntervals.value > 4) {
            ex += interval3.value + '-' + (Number(interval4.value) - 1);
            ex += '    |    ';

            if (nbrOfIntervals.value > 5) {
                ex += interval4.value + '-' + (Number(interval5.value) - 1);
                ex += '    |    ';
                ex += '> ' + (Number(interval5.value) - 1);
            }
            else {
                ex += '> ' + (Number(interval4.value) - 1);
            }
        }
        else {
            ex += '> ' + (Number(interval3.value) - 1);
        }
    }
    else {
        ex += '> ' + (Number(interval2.value) - 1);
    }

    example.innerHTML = ex;
}*/

function showField(elem) {
    if (elem != null) {
        setDisplayStyle(elem, 'block');
    }
}

function hideField(elem) {
    if (elem != null) {
        setDisplayStyle(elem, 'none');
    }
}

function setDisplayStyle(el, style) {
    el.style.display = style;
    var label = findLabelForControl(el);
    if (label != null)
        label.style.display = style;
}

function findLabelForControl(el) {
    var idVal = el.id;
    var labels = document.getElementsByTagName('label');
    for (var i = 0; i < labels.length; i++) {
        if (labels[i].htmlFor == idVal)
            return labels[i];
    }
}

$(window).bind('load', function () {
    SeqNbrPerTypeChanged();
    AgeDistNbrOfIntervalsChanged();
    //LiqPlanNbrOfIntervalsChanged();
});
