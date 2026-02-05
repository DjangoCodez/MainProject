function init()
{
    var employeeCategory = $$('employeeCategory');
    if (employeeCategory != null)
        employeeCategory.addEvent('change', getEmployees);
}

function setSelection()
{
    var periodSelection = document.getElementById('TimePeriodSelection');
    var weekSelection = document.getElementById('WeekSelection');
    var dateSelection = document.getElementById('DateSelection');
    var tblPeriod = document.getElementById('TableTimePeriodSelection');
    var tblWeek = document.getElementById('TableWeekSelection');
    var tblDate = document.getElementById('TableDateSelection');
    if (periodSelection == null || weekSelection == null || dateSelection == null || tblPeriod == null || tblWeek == null || tblDate == null)
        return;

    if (periodSelection.checked == 'checked' || periodSelection.checked == true)
    {
        weekSelection.checked = '';
        weekSelection.value = false;
        dateSelection.checked = '';
        dateSelection.value = false;

        periodSelection.style.display = '';
        weekSelection.style.display = 'none';
        dateSelection.style.display = 'none';
    }
    else if (weekSelection.checked == 'checked' || weekSelection.checked == true)
    {
        periodSelection.checked = '';
        periodSelection.value = false;
        dateSelection.checked = '';
        dateSelection.value = false;

        periodSelection.style.display = 'none';
        weekSelection.style.display = '';
        dateSelection.style.display = 'none';
    }
    else if (dateSelection.checked == 'checked' || dateSelection.checked == true)
    {
        periodSelection.checked = '';
        periodSelection.value = false;
        weekSelection.checked = '';
        weekSelection.value = false;

        periodSelection.style.display = 'none';
        weekSelection.style.display = 'none';
        dateSelection.style.display = '';
    }
}

function getEmployees()
{
    var employeeCategory = $$('EmployeeCategory');
    var employee = $$('Employee');
    if (employeeCategory == null || employee == null)
        return;

    //empty selection
    employeeCategory.options.length = 0;
    employee.options.length = 0;

    if (employeeCategory == null || employeeCategory.value == 0)
        return;

    var url = '/ajax/getEmployees.aspx';
    DOMAssistant.AJAX.get(url, function (data, status) {
        var obj = JSON.parse(data);
        if (obj) {
            var index = 1;

            var optEmpty = doc.createElement('OPTION');
            optEmpty.value = 0;
            optEmpty.text = '';
            employee.options.add(optEmpty, index);

            obj.each(function () {
                var opt = doc.createElement('OPTION');
                opt.value = this.EmployeeId;
                opt.text = this.EmployeeName;
                employee.options.add(opt, index);
                index++;
            });
        }
    });
}

$(window).bind('load', init);