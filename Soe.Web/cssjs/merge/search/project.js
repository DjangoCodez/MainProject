
function copyValue(fromId, toId, focus) {
    var from = document.getElementById(fromId);

    if (from == null)
        return;

    var to = document.getElementById(toId);
    if (to == null)
        return;

    if (to.value == "" || to.value == "0") {
        to.value = from.value;
        if (focus && (focus == 'true' || focus == "1")) {
            if (to.style.visibility == "hidden" || to.style.display == "none" || to.disabled == true) {
                //do something because it can't accept focus()
            }
            else
                to.focus();
        }
    }
}

var ProjectByNumberSearch =
{
    items: [],
    timeout: 300,
    companyId: 0,
    timers: {},
    numberField: {},
    displayDefault: false,
    init: function (displayDefault) {
        ProjectByNumberSearch.displayDefault = displayDefault;
        ProjectByNumberSearch.companyId = getQueryVariable("c");
        ProjectByNumberSearch.setFieldValues(null);
    },
    keydown: function (field, e) {
        var evtobj = window.event ? event : e;
        var key = window.event ? window.event.keyCode : e.which;
        switch (key) {
            case 8:
            case 46:
                ProjectByNumberSearch.searchOnKey(field);
                break;
        }
        if ((key > 47 && key < 91) || (key > 95 && key < 106))
            ProjectByNumberSearch.searchOnKey(field);
    },
    searchField: function (field) {

        if (ProjectByNumberSearch && $$(field) != null) {
            ProjectByNumberSearch.numberField = $$(field);
            ProjectByNumberSearch.nameField = $$(field + '-infotext');

            ProjectByNumberSearch.search(ProjectByNumberSearch.numberField.value);
        }
    },
    setFieldValues: function (value) {

        if (ProjectByNumberSearch.nameField == null) return;

        if (value != null)
            ProjectByNumberSearch.removeWarning();

        if (value != null && value.ProjectNr != null && value.ProjectNr != undefined) {
            ProjectByNumberSearch.numberField.value = value.ProjectNr;
        }

        var prefixText = '';

        if (ProjectByNumberSearch.numberField.id == 'ProjectNr-from-1') {
            prefixText = value.ProjectNr;
        }


        if (prefixText == undefined)
            prefixText = "";

        if (value.ProjectNr != null)
            copyValue('ProjectNr-from-1', 'ProjectNr-to-1', 'true');
    },
    searchOnKey: function (field) {
        if (ProjectByNumberSearch.timers[field])
            clearTimeout(ProjectByNumberSearch.timers[field]);
        var f = "ProjectByNumberSearch.searchField('" + field + "')";
        ProjectByNumberSearch.timers[field] = setTimeout(f, 300);
    },
    search: function (text) {
        if (searchComponent && searchComponent.open) {
            searchComponent.open = false;
            searchComponent.dispose();
        }

        var url = '/ajax/getProjectByNumber.aspx?c=' + ProjectByNumberSearch.companyId + '&pnr=' + encodeURIComponent(text);
        DOMAssistant.AJAX.get(url, function (data, status) {
            var obj = JSON.parse(data);
            if (obj) {
                if (obj.length == 0) {
                    ProjectByNumberSearch.addWarning();
                    ProjectByNumberSearch.setFieldValues(null);
                    return;
                }
                else if (obj.length == 1) {
                    ProjectByNumberSearch.setFieldValues(obj);
                    return;
                }
                else {
                    ProjectByNumberSearch.render(obj);
                }
            }
        });
    },
    render: function (products) {
        var indexPrefix = "project_";
        var containerId = "searchContainer";
        var templateId = "ProjectByNumberSearchItem_$projectNr$";
        var template = $$(templateId);
        if (template == null)
            return;

        var container = document.createElement("div");
        container.style.display = "none";
        var maxlengthNum = 0;
        var maxlengthName = 0;
        for (var counter = 0; counter < products.length; counter++) {

            var item = document.createElement("div");
            item.innerHTML = template.innerHTML;

            var guiObj = new Object();
            guiObj.Number = products[counter].ProjectNr;
            guiObj.id = counter;
            guiObj.Name = products[counter].ProjectName;

            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            item.innerHTML = item.innerHTML.replace('$id$', guiObj.id);
            if (maxlengthNum <= guiObj.Number.length)
                maxlengthNum = guiObj.Number.length;
            if (maxlengthName <= guiObj.Name.length)
                maxlengthName = guiObj.Name.length;
            item.innerHTML = item.innerHTML.replace('$projectNr$', guiObj.Number);
            item.innerHTML = item.innerHTML.replace('$projectName$', guiObj.Name);
            container.innerHTML += item.innerHTML;
        }

        searchComponent.init(products, container, ProjectByNumberSearch.timeout, ProjectByNumberSearch.numberField.id, indexPrefix, 'ProjectByNumberSearch.searchOnKey', 'ProjectByNumberSearch.setFieldValues', maxlengthNum, maxlengthName);
    },
    addWarning: function () {
        if (!ProjectByNumberSearch.hasWarning()) {
            var parent = ProjectByNumberSearch.numberField.parentElement.parentElement;
            var td = document.createElement('td');
            var warning = document.createElement('div');
            var img = document.createElement('img');
            img.src = "/img/exclamation.png";
            $(warning).addClass('warningDiv');
            $(img).addClass('warningImg');
            img.title = "Artikel finns inte i register!";
            warning.appendChild(img);
            td.appendChild(warning);
            parent.appendChild(td);
        }
    },
    hasWarning: function () {
        var parent = ProjectByNumberSearch.numberField.parentElement.parentElement;
        if (parent.children.length > 1) {
            var tmp = parent.children[parent.children.length - 1];
            if (tmp.children && tmp.children.length > 0)
                if ($(tmp.children[0]).hasClass("warningDiv"))
                    return true;
        }
        return false;
    },
    removeWarning: function () {
        if (ProjectByNumberSearch.hasWarning()) {
            var parent = ProjectByNumberSearch.numberField.parentElement.parentElement;
            parent.removeChild(parent.children[parent.children.length - 1]);
        }
    }
}
