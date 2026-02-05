function init() {
    var noOfIntervals = $$('AttestTransitions-noofintervals').value;
    for (var i = 1; i <= noOfIntervals; i++) {
        var attestTransitionSelect = $$('AttestTransitions-label-' + i);
        if (attestTransitionSelect != null) {
            addChangeEvent(attestTransitionSelect, i);
            if (attestTransitionSelect.options.length > 0 && attestTransitionSelect.selectedIndex == 0) {
                attestTransitionSelect.selectedIndex = 0;
                entityChanged(i);
            }
        }
    }
}

function addChangeEvent(entity, nbr) {
    entity.addEvent('change', function () { entityChanged(nbr) });
    entityChanged(nbr);
}

function entityChanged(nbr) {
    var entity = $$('AttestTransitions-label-' + nbr);
    var transition = $$('AttestTransitions-from-' + nbr);
    transition.options.length = 0;
    if (entity.count == 0 || entity.value == '0' || entity.value == '')
        return;

    var url = '/ajax/getAttestTransitions.aspx?entity=' + entity.value + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
    DOMAssistant.AJAX.get(url, function (data, status) {       
        if (data && data.length > 0) {
            var obj = JSON.parse(data);
            obj.each(function () {
                transition.options[parseInt(this.Position)] = new Option(this.Name, this.AttestTransitionId);
                if (arrayTransitions != undefined) {
                    for (var i = 0; i < attestTransitionCount; i++) {
                        var sel = $$('AttestTransitions-from-' + (i + 1));
                        if (sel != null) {
                            for (var j = 0; j < sel.length; j++) {
                                if (sel[j].value == arrayTransitions[i])
                                    sel[j].selected = true;
                            }
                        }
                    }
                }
            });
        }
    });
}

$(window).bind('load', init);