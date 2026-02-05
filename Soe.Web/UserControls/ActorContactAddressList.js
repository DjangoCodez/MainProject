//Has DistributionSelectionStd_ suffix to avoid name collisions when this UserControl is placed inside a page
var ActorContactAddressList_intervalId = -1;
var ActorContactAddressList_sysTerms = null;
var ActorContactAddressList_validations = 0;

function ActorContactAddressList_init() {
    ActorContactAddressList_initTerms();
}

function ActorContactAddressList_initTerms() {
    //create
    ActorContactAddressList_sysTerms = new Array();
    ActorContactAddressList_sysTerms.push(TermManager.createSysTerm(1, 5543, 'Ta bort adress'));

    //validate
    ActorContactAddressList_intervalId = setInterval(ActorContactAddressList_validateTerms, TermManager.delay);
}

function ActorContactAddressList_validateTerms() {
    ActorContactAddressList_validations++;
    var valid = TermManager.validateSysTerms(ActorContactAddressList_sysTerms, 'ActorContactAddressList');
    if (valid || TermManager.reachedMaxValidations(ActorContactAddressList_validations)) {
        clearInterval(ActorContactAddressList_intervalId);
        ActorContactAddressList_setup();
    }
}

function ActorContactAddressList_setup() {
    //Add initialization here!
}

function selectAdress(fullidentifier, id, type) {
    if (id < 0) return;
    deselectAdresses();
    var adress = $$(fullidentifier);
    if (adress != null) {
        if (!adress.hasClass('currentAdress')) {
            adress.addClass('currentAdress');
        }
    }
    selectAdressRows(id, type);
}

function deselectAdresses() {
    var adressContainer = $$('adresses');
    for (var i = 0; i < adressContainer.children.length; i++) {
        if (adressContainer.children[i].className == 'adress') {
            for (var j = 0; j < adressContainer.children[i].children.length; j++) {
                if (adressContainer.children[i].children[j].className == 'adressInput currentAdress')
                    deselectAdress(adressContainer.children[i].children[j].id);
            }
        }
    }
}

function deselectAdress(fullidentifier) {
    var adress = $$(fullidentifier);
    if (adress != null) {
        if (adress.hasClass('currentAdress')) {
            adress.removeClass('currentAdress');
        }
    }
}

function renderAdressRows(id, type, adress) {
    var labelContainer = $$('labelcontainer2');
    var inputContainer = $$('inputcontainer2');
    for (var i = 0; i < adress.Rows.length; i++) {
        var rowId = 'row_' + adress.Rows[i].SysContactAddressRowTypeId + "_" + id;
        var labeldiv = document.createElement('div');
        labeldiv.setAttribute('id', rowId + '_label');
        labeldiv.innerHTML = adress.Rows[i].Label;
        labeldiv.className = 'label';
        var inputdiv = document.createElement('input');
        inputdiv.setAttribute('id', rowId + '_input');
        inputdiv.setAttribute('type', 'text');
        inputdiv.setAttribute('name', rowId + '_input');
        inputdiv.setAttribute('value', adress.Rows[i].value);
        inputdiv.className = 'rowinput';
        labelContainer.appendChild(labeldiv);
        inputContainer.appendChild(inputdiv);
    }
}

function createAdressRows(id, type) {
    var labelContainer = $$('labelcontainer2');
    var inputContainer = $$('inputcontainer2');
    for (var i = 0; i < actorAdresses[type].Rows.length; i++) {
        var rowId = 'row_' + actorAdresses[type].Rows[i].SysContactAddressRowTypeId + "_" + id;

        var labeldiv = document.createElement('div');
        labeldiv.setAttribute('id', rowId + '_label');
        labeldiv.innerHTML = actorAdresses[type].Rows[i].Label;
        labeldiv.className = 'label';
        var inputdiv = document.createElement('input');
        inputdiv.setAttribute('id', rowId + '_input');
        inputdiv.setAttribute('type', 'text');
        inputdiv.setAttribute('name', rowId + '_input');
        inputdiv.className = 'rowinput';
        labelContainer.appendChild(labeldiv);
        inputContainer.appendChild(inputdiv);
    }
}

function deleteAddress(fullidentifier, id, type) {
    var adressContainer = $$('adresses');
    var adress = $$('row_' + id);
    var input = $$(fullidentifier);
    var iscurrent = input.hasClass('currentAdress');
    adressContainer.removeChild(adress);
    deleteAddressRows(id, type, iscurrent);
}

function deleteAddressRows(id, type, iscurrent) {
    deletePartialAddressRows(id, type, iscurrent, 'label');
    deletePartialAddressRows(id, type, iscurrent, 'input');
}

function deletePartialAddressRows(id, type, iscurrent, suffix) {
    var to = suffix + 'container';
    if (!iscurrent)
        to = to + "2";
    var target = $$(to);
    for (var i = 0; i < actorAdresses[type].Rows.length; i++) {
        var rowId = 'row_' + actorAdresses[type].Rows[i].SysContactAddressRowTypeId + "_" + id + "_" + suffix;
        var row = $$(rowId);
        if (row != null) {
            target.removeChild(row);
        }
    }
}

function addAdress() {
    var dropdown = $$('AddressType');
    var selected = null;
    for (var n = 0; n < dropdown.length; n++) {
        if (dropdown[n].selected == true) {
            selected = dropdown[n];
        }
    }
    if (selected == null)
        return;

    var type = 0;
    var object = null;
    try {
        if (actorAdresses != undefined) {
            for (var j = 0; j < actorAdresses.length; j++) {
                if (actorAdresses[j].SysContactAddressTypeId == selected.value) {
                    object = actorAdresses[j];
                    type = j;
                    break;
                }
            }
        }
    }
    catch (err) {
        return;
    }
    if (object == null)
        return;

    var adressContainerDiv = $$('adresses');
    adressContainerDiv.className = 'adressesContainer';
    if (adressContainerDiv == undefined || adressContainerDiv == null)
        return;

    var id = 0;
    if (adressContainerDiv.children.length > 0) {
        for (var i = adressContainerDiv.children.length - 1; i >= 0; i--) {
            if (adressContainerDiv.children[i].className = 'adress') {
                id = adressContainerDiv.children[i].id.replace('row_', '');
                id = (id * 1) + 1;
                break;
            }
        }

    }

    var fullidentifier = 'adress_ContactAddressId_0_SysContactAddressId_' + object.SysContactAddressId + '_SysContactAddressTypeId_' + object.SysContactAddressTypeId + '_id_' + id;
    var adressDiv = document.createElement('div');
    adressDiv.setAttribute('id', 'row_' + id);
    adressDiv.className = 'adress';

    var emptyDiv = document.createElement('div');
    emptyDiv.className = 'clear';

    var adressInput = document.createElement('input');
    adressInput.setAttribute('id', fullidentifier);
    adressInput.setAttribute('name', fullidentifier);
    adressInput.className = 'adressInput';
    adressInput.onclick = function () { selectAdress(fullidentifier, id, type); };
    adressInput.value = object.Label;
    adressDiv.appendChild(adressInput);

    var deleteimg = document.createElement('span');
    deleteimg.setAttribute('id', 'delete_' + id);
    deleteimg.className = 'deleteimg fal fa-times';
    //deleteimg.setAttribute('alt', TermManager.getText(ActorContactAddressList_sysTerms, 1, 5543));
    deleteimg.onclick = function () { deleteAddress(fullidentifier, id, type); };
    adressDiv.appendChild(deleteimg);

    adressDiv.appendChild(emptyDiv);
    adressContainerDiv.appendChild(adressDiv);
    adressDiv.appendChild(emptyDiv);

    createAdressRows(id, type);
    selectAdress(fullidentifier, id, type);
}

function renderAdress(adress) {

    var type = 0;
    for (var i = 0; i < actorAdresses.length; i++) {
        if (actorAdresses[i].SysContactAddressTypeId == adress.SysContactAddressTypeId) {
            type = i;
        }
    }

    var adressContainerDiv = $$('adresses');
    adressContainerDiv.className = 'adressesContainer';
    if (adressContainerDiv == undefined || adressContainerDiv == null)
        return;

    var id = 0;
    if (adressContainerDiv.children.length > 0) {
        for (var i = adressContainerDiv.children.length - 1; i >= 0; i--) {
            if (adressContainerDiv.children[i].className = 'adress') {
                id = adressContainerDiv.children[i].id.replace('row_', '');
                id = (id * 1) + 1;
                break;
            }
        }
    }

    var sysContactAddressId = 0;
    for (var i = 0; i < actorAdresses.length; i++) {
        if (actorAdresses[i].SysContactAddressTypeId == adress.SysContactAddressTypeId) {
            sysContactAddressId = actorAdresses[i].SysContactAddressId;
        }
    }

    var fullidentifier = 'adress_ContactAddressId_' + adress.ContactAddressId + '_SysContactAddressId_' + sysContactAddressId + '_SysContactAddressTypeId_' + adress.SysContactAddressTypeId + '_id_' + id;

    var adressDiv = document.createElement('div');
    adressDiv.setAttribute('id', 'row_' + id);
    adressDiv.className = 'adress';

    var emptyDiv = document.createElement('div');
    emptyDiv.className = 'clear';

    var adressInput = document.createElement('input');
    adressInput.setAttribute('id', fullidentifier);
    adressInput.setAttribute('name', fullidentifier);
    adressInput.className = 'adressInput';
    adressInput.onclick = function () { selectAdress(fullidentifier, id, type); };
    adressInput.value = adress.Label;
    adressDiv.appendChild(adressInput);

    var deleteimg = document.createElement('span');
    deleteimg.setAttribute('id', 'delete_' + id);
    deleteimg.className = 'deleteimg fal fa-times';
    //deleteimg.setAttribute('alt', TermManager.getText(ActorContactAddressList_sysTerms, 1, 5543));
    deleteimg.onclick = function () { deleteAddress(fullidentifier, id, type); };
    adressDiv.appendChild(deleteimg);

    adressDiv.appendChild(emptyDiv);
    adressContainerDiv.appendChild(adressDiv);
    adressDiv.appendChild(emptyDiv);

    renderAdressRows(id, type, adress);
    selectAdress(fullidentifier, id, type);
}

function selectAdressRows(id, type) {
    $$('inputcontainer2').className = 'inputcontainerhidden';
    $$('labelcontainer2').className = 'labelcontainerhidden';

    moveCurrentVisibleRows('inputcontainer', 'inputcontainer2');
    moveCurrentVisibleRows('labelcontainer', 'labelcontainer2');
    moveHiddenToVisible('inputcontainer', 'inputcontainer2', id, type, 'input');
    moveHiddenToVisible('labelcontainer', 'labelcontainer2', id, type, 'label');
}

function moveHiddenToVisible(to, from, id, type, suffix) {
    var target = $$(to);
    var source = $$(from);
    while (target.children.length > 0) {
        target.children.removeChild(target.children[0]);
    }
    for (var i = 0; i < actorAdresses[type].Rows.length; i++) {
        var rowId = 'row_' + actorAdresses[type].Rows[i].SysContactAddressRowTypeId + "_" + id + "_" + suffix;
        var row = $$(rowId);
        if (row != null) {
            source.removeChild(row);
            target.appendChild(row);
            var emptyDiv = document.createElement('div');
            emptyDiv.className = 'clear';
            target.appendChild(emptyDiv);
        }
    }
}

function moveCurrentVisibleRows(from, to) {
    var target = $$(to);
    var source = $$(from);
    while (source.children.length > 0) {
        var item = source.children[0];
        source.removeChild(item);
        if (item.className == 'clear') {
        }
        else {
            target.appendChild(item);
        }
    }
}

function init() {
    for (var i = 0; i < initialAdresses.length; i++) {
        renderAdress(initialAdresses[i]);
    }
}


var initialized = false;
var maxCycles = 10;
while (!initialized && maxCycles > 0) {
    try {
        if (initialAdresses != undefined) {
            if (initialAdresses.length > 0)
                init();
            initialized = true;
        }
    }
    catch (err) {
        //sleep
        setTimeout('', 1000);
        maxCycles = maxCycles - 1;
    }

}

$(window).bind('load', ActorContactAddressList_init);