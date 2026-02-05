function clearInternalAccounts(field) {
    // Only CostAccount and IncomeAccount have internal account fields
    if (field != 'CostAccount' && field != 'IncomeAccount')
        return;

    // Loop through rows in table to find internal accounts and clear them
    var tbl = (field == 'CostAccount' ? $$('CostAccountTable') : $$('IncomeAccountTable'));
    var rows = tbl.rows;
    var cells;
    var cell;
    for (var i = 2; i < rows.length; i++) {
        cells = rows[i].cells;
        if (cells != null) {
            cell = cells[field == 'CostAccount' ? 1 : 0];
            if (cell != null && cell.childNodes != null)
                cell.childNodes[0].value = 0;
        }
    }
}

function changedTimeReportType() {
    var timeReportTypeValue = $('#TimeReportType').val();
    
    var autoGenTimeAndBreakForProject = $('#AutoGenTimeAndBreakForProject');
    if (autoGenTimeAndBreakForProject) {
        if (timeReportTypeValue === "2") {
            autoGenTimeAndBreakForProject.show();
            $('label[for="AutoGenTimeAndBreakForProject"]').show();
        }
        else {
            autoGenTimeAndBreakForProject.hide();
            $('label[for="AutoGenTimeAndBreakForProject"]').hide();
        }
    }
}

window.onload = changedTimeReportType;