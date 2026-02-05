import { TimeScheduleTemplateGroupEmployeeDTO, TimeScheduleTemplateGroupRowDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";

export class TemplateGroupsValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                rows: '=',
                employees: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => scope['rows'], (newValues, oldValues) => {
                    let rows: TimeScheduleTemplateGroupRowDTO[] = scope['rows'];

                    let rowValidTemplate: boolean = true;
                    if (_.filter(rows, r => !r.timeScheduleTemplateHeadId).length > 0)
                        rowValidTemplate = false;
                    ngModelController.$setValidity("rowTemplateMandatory", rowValidTemplate);

                    let rowValidStartDate: boolean = true;
                    if (_.filter(rows, r => !r.startDate).length > 0)
                        rowValidStartDate = false;
                    ngModelController.$setValidity("rowStartDateMandatory", rowValidStartDate);

                    let rowDatesValidOrder: boolean = true;
                    if (_.filter(rows, r => r.startDate && r.stopDate && r.startDate.isAfterOnDay(r.stopDate)).length > 0)
                        rowDatesValidOrder = false;
                    ngModelController.$setValidity("rowDatesValidOrder", rowDatesValidOrder);
                }, true);

                scope.$watch(() => scope['employees'], (newValues, oldValues) => {
                    let employees: TimeScheduleTemplateGroupEmployeeDTO[] = scope['employees'];
                    if (employees) {
                        // Clear validation messages on each employee
                        _.forEach(employees, emp => {
                            emp['fromDateOverlapping'] = false;
                            emp['dateRangeOverlapping'] = false;
                        });

                        let employeeValidEmployee: boolean = true;
                        if (_.filter(employees, e => !e.employeeId).length > 0)
                            employeeValidEmployee = false;
                        ngModelController.$setValidity("employeeEmployeeMandatory", employeeValidEmployee);

                        let employeeValidFromDate: boolean = true;
                        if (_.filter(employees, e => !e.fromDate).length > 0)
                            employeeValidFromDate = false;
                        ngModelController.$setValidity("employeeFromDateMandatory", employeeValidFromDate);

                        let employeeDatesValidOrder: boolean = true;
                        if (_.filter(employees, e => e.fromDate && e.toDate && e.fromDate.isAfterOnDay(e.toDate)).length > 0)
                            employeeDatesValidOrder = false;
                        ngModelController.$setValidity("employeeDatesValidOrder", employeeDatesValidOrder);

                        let employeeValidFromDateOverlapping: boolean = true;
                        let employeeValidDateRangeOverlapping: boolean = true;

                        // Multiple rows with same employee
                        let empGroups = _.groupBy(employees, e => e.employeeId);
                        let multiEmpGroups = _.filter(empGroups, g => Object.keys(g).length > 1);
                        if (multiEmpGroups.length > 0) {
                            let keys = Object.keys(multiEmpGroups);
                            for (const key of keys) {
                                let empRows: TimeScheduleTemplateGroupEmployeeDTO[] = multiEmpGroups[key];
                                // Check same start date
                                let fromDateGroups = _.groupBy(empRows, e => e.fromDate);
                                if (_.filter(fromDateGroups, g => Object.keys(g).length > 1).length > 0) {
                                    employeeValidFromDateOverlapping = false;
                                    empRows.forEach(empRow => {
                                        empRow['fromDateOverlapping'] = true;
                                    });
                                }

                                // Check overlapping date ranges
                                _.filter(empRows, e => e.fromDate && e.toDate).forEach(empRow => {
                                    if (_.find(empRows, f => f.fromDate && f.fromDate.isAfterOnDay(empRow.fromDate) && f.fromDate.isSameOrBeforeOnDay(empRow.toDate))) {
                                        employeeValidDateRangeOverlapping = false;
                                        empRow['dateRangeOverlapping'] = true;
                                    }
                                });
                            }
                        }
                        ngModelController.$setValidity("employeeFromDateOverlapping", employeeValidFromDateOverlapping);
                        ngModelController.$setValidity("employeeDateRangeOverlapping", employeeValidDateRangeOverlapping);
                    }
                }, true);
            }
        }
    }
}


