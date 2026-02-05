import { EmployeeUserDTO, UserRolesDTO } from "../../../../Common/Models/EmployeeUserDTO";
import { TimeScheduleTemplateGroupEmployeeDTO } from "../../../../Common/Models/TimeScheduleTemplateDTOs";
import { TermGroup_EmployeeDisbursementMethod } from "../../../../Util/CommonEnumerations";

export class EmployeeValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                isLoginNameValid: '=',
                isBankAccountValid: '=',
                dontValidateBankAccount: '=',
                isSocialSecurityNumberValid: '=',
                isAccountingSettingsValid: '=',
                isEmployeeVacationValid: '=',
                isContactAddressesValid: '=',
                isEmploymentPriceTypesValid: '=',
                userRoles: '=',
                usePayroll: '=',
                payrollGroupMandatory: '=',
                useAccountsHierarchy: '=',
                employeeTemplateGroups: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    var employee: EmployeeUserDTO = ngModelController.$modelValue;
                    if (employee) {
                        var bankAccountNotSet: boolean = (scope['usePayroll'] && employee.disbursementMethod && (!employee.disbursementMethod));
                        var bankAccountNotSetWhenMethod: boolean = (employee.disbursementMethod == TermGroup_EmployeeDisbursementMethod.SE_AccountDeposit && ((!employee.disbursementAccountNr || employee.disbursementAccountNr.length == 0) || (!employee.disbursementClearingNr || employee.disbursementClearingNr.length == 0)));
                        ngModelController.$setValidity("bankAccountMandatory", scope['dontValidateBankAccount'] || !bankAccountNotSet);
                        ngModelController.$setValidity("bankAccountMandatoryIfMethodSelected", scope['dontValidateBankAccount'] || !bankAccountNotSetWhenMethod);

                        var useAccountsHierarchy: boolean = scope['useAccountsHierarchy'];
                        var accountSet: boolean = !!(!useAccountsHierarchy || (employee.accounts && _.filter(employee.accounts, a => a.default).length > 0));
                        var categorySet: boolean = !!(useAccountsHierarchy || (employee.categoryRecords && _.filter(employee.categoryRecords, c => c.default).length > 0));
                        ngModelController.$setValidity("accountMandatory", accountSet);
                        ngModelController.$setValidity("categoryMandatory", categorySet);

                        var payrollGroupMissing: boolean = scope['payrollGroupMandatory'] && !employee.vacant && _.filter(employee.employments, e => e.state === 0 && !e.payrollGroupId).length > 0;
                        ngModelController.$setValidity("payrollGroupMandatory", !payrollGroupMissing);

                        var noActiveEmployments: boolean = !employee.vacant && (!employee.employments || _.filter(employee.employments, e => e.state === 0).length === 0);
                        ngModelController.$setValidity("employmentMandatory", !noActiveEmployments);
                    }
                }, true);

                scope.$watchGroup(['isLoginNameValid', 'isBankAccountValid', 'isSocialSecurityNumberValid', 'isAccountingSettingsValid', 'isEmploymentPriceTypesValid', 'isEmployeeVacationValid', 'isContactAddressesValid'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("loginName", scope['isLoginNameValid']);
                    ngModelController.$setValidity("bankAccount", scope['isBankAccountValid']);
                    ngModelController.$setValidity("socialSecurityNumber", scope['isSocialSecurityNumberValid']);
                    ngModelController.$setValidity("accountingSettings", scope['isAccountingSettingsValid']);
                    ngModelController.$setValidity("empVacation", scope['isEmployeeVacationValid']);
                    ngModelController.$setValidity("contactAddress", scope['isContactAddressesValid']);
                    ngModelController.$setValidity("employmentPriceTypes", scope['isEmploymentPriceTypesValid']);                    
                }); 

                scope.$watch(() => scope['userRoles'], (newValues, oldValues) => {
                    var roleValid: boolean = false;
                    var roleDefaultValid: boolean = false;
                    var employee: EmployeeUserDTO = ngModelController.$modelValue;
                    if (employee && employee.employeeId) {
                        // Must have at least one role and one marked as default
                        var userRoles: UserRolesDTO[] = scope['userRoles'];
                        _.forEach(userRoles, userRole => {
                            if (userRole.roles && userRole.roles.length > 0) {
                                roleValid = true;
                                if (_.filter(userRole.roles, r => r.default).length > 0)
                                    roleDefaultValid = true;
                            }
                        });
                    } else {
                        // Do not validate for new employee
                        roleValid = true;
                        roleDefaultValid = true;
                    }
                    ngModelController.$setValidity("role", roleValid);
                    ngModelController.$setValidity("defaultRole", roleDefaultValid);
                });

                scope.$watch(() => scope['employeeTemplateGroups'], (newValues, oldValues) => {
                    let validTemplateGroupDates: boolean = true;
                    let groups: TimeScheduleTemplateGroupEmployeeDTO[] = scope['employeeTemplateGroups'];

                    if (_.filter(groups, g => !g.toDate).length > 1 || _.filter(groups, g => g.toDate && g.toDate.isBeforeOnDay(g.fromDate)).length > 0)
                        validTemplateGroupDates = false;
                    else {
                        let prevGroup: TimeScheduleTemplateGroupEmployeeDTO;
                        _.forEach(_.orderBy(groups, g => g.fromDate), group => {
                            if (prevGroup) {
                                if (!prevGroup.toDate || prevGroup.toDate.isSameOrAfterOnDay(group.fromDate))
                                    validTemplateGroupDates = false;
                            }
                            prevGroup = group;
                        });
                    }

                    ngModelController.$setValidity("templateGroupDates", validTemplateGroupDates);
                }, true);
            }
        }
    }
}