import { TermGroup_EmployeeDisbursementMethod, TermGroup_EmploymentType } from "../../../../../Util/CommonEnumerations";

export class CreateFromTemplateValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                employmentStopDateExistsInTemplate: '=',
                disbursementAccountExistsInTemplate: '=',
                employmentTypes: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    const employee = ngModelController.$modelValue;

                    // Employment stop date
                    // Required if type is not permanent
                    let validEmploymentStopDate = true;
                    if (scope['employmentStopDateExistsInTemplate'] && employee && employee.employmentType && !employee.employmentStopDate && scope['employmentTypes'] && scope['employmentTypes'].length > 0) {
                        const employmentType = scope['employmentTypes'].find(e => e.id === employee.employmentType);
                        const isPermanent = employmentType.type === TermGroup_EmploymentType.SE_Permanent;
                        validEmploymentStopDate = isPermanent;
                    }
                    ngModelController.$setValidity("mandatoryEmploymentStopDate", validEmploymentStopDate);

                    // Disbursement method
                    let mandatoryDisbursementMethod = true;
                    let validDisbursementMethod = true;
                    if (scope['disbursementAccountExistsInTemplate'] && employee) {
                        if (employee.disbursementAccount) {
                            const account = JSON.parse(employee.disbursementAccount);
                            if (account) {
                                if (account.method === undefined)
                                    mandatoryDisbursementMethod = false;
                                else if (account.method == TermGroup_EmployeeDisbursementMethod.Unknown)
                                    validDisbursementMethod = false;
                            } else {
                                mandatoryDisbursementMethod = false;
                            }
                        } else {
                            mandatoryDisbursementMethod = false;
                        }
                    }

                    ngModelController.$setValidity("mandatoryDisbursementMethod", mandatoryDisbursementMethod);
                    ngModelController.$setValidity("validDisbursementMethod", validDisbursementMethod);
                }, true);
            }
        }
    }
}