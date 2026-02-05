function init() {
    var initialControl = document.getElementById('Initial');
    var closedControl = document.getElementById('Closed');
    closedControl.disabled = initialControl.checked;
    initialControl.disabled = closedControl.checked;
}

$(window).bind('load', init);
