import { EmployeeUserDTO, UserRolesDTO } from "../../../../Common/Models/EmployeeUserDTO";

export class UserValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                isLoginNameValid: '=',
                isContactAddressesValid: '=',
                userRoles: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['isLoginNameValid', 'isContactAddressesValid'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("loginName", scope['isLoginNameValid']);
                    ngModelController.$setValidity("contactAddress", scope['isContactAddressesValid']);
                });

                scope.$watch(() => scope['userRoles'], (newValues, oldValues, scope) => {
                    var roleValid: boolean = false;
                    var roleDefaultValid: boolean = false;
                    var user: EmployeeUserDTO = ngModelController.$modelValue;
                    if (user && user.userId) {
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
            }
        }
    }
}