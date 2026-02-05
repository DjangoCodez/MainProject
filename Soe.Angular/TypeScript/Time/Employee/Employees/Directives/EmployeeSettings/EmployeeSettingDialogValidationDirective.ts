import { EmployeeSettingDTO } from "../../../../../Common/Models/EmployeeUserDTO";

export class EmployeeSettingDialogValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                settings: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    const employeeSetting: EmployeeSettingDTO = ngModelController.$modelValue;
                    if (employeeSetting) {
                        const groupMissing = !employeeSetting.employeeSettingGroupType;
                        const typeMissing = !employeeSetting.employeeSettingType;
                        const invalidDates = (employeeSetting.validFromDate && employeeSetting.validToDate && employeeSetting.validFromDate > employeeSetting.validToDate);

                        ngModelController.$setValidity("groupMandatory", !groupMissing);
                        ngModelController.$setValidity("typeMandatory", !typeMissing);
                        ngModelController.$setValidity("validDates", !invalidDates);

                        const settings: EmployeeSettingDTO[] = scope['settings'];

                        let invalidFromDate = false;
                        if (employeeSetting.employeeSettingType && employeeSetting.validFromDate) {

                            let sameTypeAndDateSettings = settings.filter(s => s.employeeSettingType === employeeSetting.employeeSettingType && s.validFromDate && s.validFromDate.isSameDayAs(employeeSetting.validFromDate));
                            if (employeeSetting.employeeSettingId) {
                                if (sameTypeAndDateSettings.filter(s => s.employeeSettingId !== employeeSetting.employeeSettingId).length > 0)
                                    invalidFromDate = true;
                            } else {
                                if (sameTypeAndDateSettings.filter(s => s.tmpEmployeeSettingId !== employeeSetting.tmpEmployeeSettingId).length > 0)
                                    invalidFromDate = true;
                            }
                        }

                        ngModelController.$setValidity("validFromDate", !invalidFromDate);
                    }
                }, true);
            }
        }
    }
}