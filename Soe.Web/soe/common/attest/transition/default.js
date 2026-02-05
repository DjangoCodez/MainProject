function entityChanged(module) {
    var entity = document.getElementById("Entity");
    var name = document.getElementById("Name");
    var stateFrom = document.getElementById("StateFrom");
    var stateTo = document.getElementById("StateTo");
    if (entity == null || entity == null || stateFrom == null || stateTo == null)
        return;

    name.value = '';

    var url = '/ajax/getAttestStates.aspx?entity=' + entity.value + "&module=" + module + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
    DOMAssistant.AJAX.get(url, function (data, status) {
        stateFrom.options.length = 0;
        stateTo.options.length = 0;
        var obj = JSON.parse(data);
        if (obj) {
            obj.each(function () {
                stateFrom.options[parseInt(this.Position)] = new Option(this.Name, this.AttestStateId);
                stateTo.options[parseInt(this.Position)] = new Option(this.Name, this.AttestStateId);
            });
        }
    });
}

function SuggestTransitionName()
{
    var name = document.getElementById("Name");
    var stateFrom = document.getElementById("StateFrom");
    var stateTo = document.getElementById("StateTo");
    if (name == null || stateFrom == null || stateTo == null)
        return;

    name.value = stateFrom.options[stateFrom.selectedIndex].text + ' - ' + stateTo.options[stateTo.selectedIndex].text;
    FormValidation.validateField(name);
}
