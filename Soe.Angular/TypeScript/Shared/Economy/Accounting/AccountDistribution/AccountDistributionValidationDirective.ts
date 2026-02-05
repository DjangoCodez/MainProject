import { IAccountDistributionHeadDTO } from "../../../../Scripts/TypeLite.Net4";
import { TermGroup_AccountDistributionTriggerType } from "../../../../Util/CommonEnumerations";

export class AccountDistributionValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                isPeriodAccountDistribution: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var accountDistribution: IAccountDistributionHeadDTO = ngModelController.$modelValue;

                        ngModelController.$setValidity("name", !!(accountDistribution && accountDistribution.name));

                        ngModelController.$setValidity("sort", !!(accountDistribution && accountDistribution.sort !== undefined && accountDistribution.sort !== null));

                        ngModelController.$setValidity("calculationtype", !!(accountDistribution && accountDistribution.calculationType));

                        var isPeriodAccounting: boolean = scope["isPeriodAccountDistribution"];
                        if (isPeriodAccounting) {
                            ngModelController.$setValidity("triggertype", !!(accountDistribution && accountDistribution.triggerType));

                            ngModelController.$setValidity("voucherseriestypeid", !!(accountDistribution && accountDistribution.voucherSeriesTypeId));

                            ngModelController.$setValidity("periodtype", !!(accountDistribution && accountDistribution.periodType));

                            ngModelController.$setValidity("periodvalue", accountDistribution.triggerType.valueOf() == TermGroup_AccountDistributionTriggerType.Registration ? !!(accountDistribution && accountDistribution.periodValue && accountDistribution.periodValue > 0) : true);

                            ngModelController.$setValidity("daynumber", !!(accountDistribution && accountDistribution.dayNumber));

                            ngModelController.$setValidity("startdate", !!(accountDistribution && accountDistribution.startDate));

                            if (accountDistribution) {
                                var same: number = 0;
                                var opposite: number = 0;
                                _.forEach(accountDistribution.rows, r => {
                                    same += parseFloat(<any>r.sameBalance) || 0;
                                    opposite += parseFloat(<any>r.oppositeBalance) || 0;
                                });

                                ngModelController.$setValidity("diff", !!((same - opposite) === 0));
                            }
                        }
                    }
                }, true);
            }
        }
    }
}


