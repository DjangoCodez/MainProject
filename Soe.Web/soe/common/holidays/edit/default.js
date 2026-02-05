function init() {
    $$('Name').addEvent('change', clearNames);

    if ($$('Names') != null)
        $$('Names').addEvent('change', setName);

    $$('Date').focus();
}

function clearNames() {
    var names = $$("Names");
    names.selectedIndex = 0;
}

function setName() {
    var name = $$("Name");
    var names = $$("Names");

    name.value = names[names.selectedIndex].text;
    name.focus();
}

$(window).bind('load', init);
