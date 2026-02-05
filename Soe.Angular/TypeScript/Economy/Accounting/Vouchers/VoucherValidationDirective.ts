import { TermGroup_AccountStatus } from "../../../Util/CommonEnumerations";

export class VoucherValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                accountYearIsOpen: '=',
                accountPeriod: '=',
                defaultVoucherSeriesId: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['accountYearIsOpen', 'accountPeriod', 'defaultVoucherSeriesId'], (newValues, oldValues, scope) => {
                    // Account year
                    ngModelController.$setValidity("accountYearStatus", scope['accountYearIsOpen'] === true);

                    // Account period
                    var accountPeriod = scope['accountPeriod'];
                    ngModelController.$setValidity("accountPeriod", accountPeriod);
                    ngModelController.$setValidity("accountPeriodStatus", accountPeriod && accountPeriod.status === TermGroup_AccountStatus.Open);

                    // Voucher series
                    ngModelController.$setValidity("defaultVoucherSeries", scope['defaultVoucherSeriesId'] !== 0);

                    // Voucher
                    //var voucher: VoucherHeadDTO = ngModelController.$modelValue;
                });
            }
        }
    }
}


