function formSubmitted() {
    var impDiv = document.getElementById("importingDiv");
    impDiv.removeAttribute("style");
    impDiv.style.visibility = "visible";
    return true;
}

function providerChanged() {
    var file2 = $$('FileInput2');
    
    var provider = document.getElementById("Provider");
    if (provider.value.startsWith("Lunda") ||
        provider.value == "RexelFINetto" || 
        provider.value == "AhlsellFINetto" ||
        provider.value == "AhlsellFIPLNetto" ||
        provider.value == "SoneparFINetto" ||
        provider.value == "OnninenFINettoS" ||
        provider.value == "DahlFINetto" ||
        provider.value == "OnninenFINettoLVI") {
        showField(file2);
    }
    else {
        hideField(file2);
    }

    return true;
}

function showField(elem) {
    if (elem != null) {
        setDisplayStyle(elem, 'block');
    }
}

function hideField(elem) {
    if (elem != null) {
        setDisplayStyle(elem, 'none'); //hides element
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
    providerChanged();
});