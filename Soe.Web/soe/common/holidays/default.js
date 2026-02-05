function init() {
    $$('HolidayYear').addEvent('change', loadHolidays);
}

function loadHolidays() {
    var el = $$("loadHolidays");
    el.click();
}

$(window).bind('load', init);
