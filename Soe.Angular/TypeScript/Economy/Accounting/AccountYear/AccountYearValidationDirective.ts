import { AccountYearDTO } from "../../../Common/Models/AccountYear";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TermGroup_AccountStatus } from "../../../Util/CommonEnumerations";
export class YearValidationDirective {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                accountYears: '=',
                previouseAccountYearTo: '=',
                previouseAccountYearFrom: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        let validity = true;
                        const accountYear: AccountYearDTO = ngModelController.$modelValue;
                        const accountYears: AccountYearDTO[] = scope["accountYears"];
                        const previouseAccountYearTo: any = scope["previouseAccountYearTo"];
                        const previouseAccountYearFrom: any = scope["previouseAccountYearFrom"];
                      
                        if (accountYear && accountYears && accountYears.length > 0) {

                            const overlappingYear = accountYears.find(x => x.accountYearId != accountYear.accountYearId && ((x.from <= accountYear.to && accountYear.to <= x.to) || (x.from <= accountYear.from && accountYear.from <= x.to)));
                            if (!overlappingYear) {
                                if (previouseAccountYearFrom != null && previouseAccountYearTo != null && (previouseAccountYearFrom.toDateTimeString() != accountYear.from.toDateTimeString() || previouseAccountYearTo.toDateTimeString() != accountYear.to.toDateTimeString())) {
                                    const canClearPeriods = accountYear.periods.every(py => py.status === TermGroup_AccountStatus.New);
                                    if (!canClearPeriods)
                                        validity = false;
                                }
                            } else {
                                validity = false;
                            }
                            ngModelController.$setValidity("to", validity);
                        }
                    }
                }, true);
            }
        }
    }
}