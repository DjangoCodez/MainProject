function init() {
    UpdateFileTemplateChanged();
}

function UpdateFileTemplateChanged() {
    var fileTemplate = document.getElementById('FileTemplate');
    var update = document.getElementById('UpdateFileTemplate');
    if (fileTemplate == null || update == null)
        return;

    if (update.checked == 'checked' || update.checked == true)
        fileTemplate.disabled = '';
    else
        fileTemplate.disabled = 'disabled';
}

$(window).bind('load', init);