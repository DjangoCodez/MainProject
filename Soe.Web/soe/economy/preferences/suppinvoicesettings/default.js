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

        //seqNbrStart.parentNode.parentNode.style.visibility = 'visible';
        //seqNbrStartDebit.parentNode.parentNode.style.visibility = 'hidden';
        //seqNbrStartCredit.parentNode.parentNode.style.visibility = 'hidden';
        //seqNbrStartInterest.parentNode.parentNode.style.visibility = 'hidden';
    } else {
        seqNbrStart.disabled = false;
        seqNbrStartDebit.disabled = true;
        seqNbrStartCredit.disabled = true;
        seqNbrStartInterest.disabled = true;

        //seqNbrStart.parentNode.parentNode.style.visibility = 'hidden';
        //seqNbrStartDebit.parentNode.parentNode.style.visibility = 'visible';
        //seqNbrStartCredit.parentNode.parentNode.style.visibility = 'visible';
        //seqNbrStartInterest.parentNode.parentNode.style.visibility = 'visible';
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
