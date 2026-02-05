import { AccountPeriodDTO, AccountYearDTO, AccountYearLightDTO } from "../../../../../Common/Models/AccountYear";
import { AccountIntervalSelectionDTO, YearAndPeriodSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IYearAndPeriodSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup_AccountStatus } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Services/CoreService";
import { ITranslationService } from "../../../../Services/TranslationService";
import { IReportDataService } from "../../ReportDataService";

interface MultiSelectViewModel {
    id: string;
    label: string;
}

export class AccountingYearAndPeriodRangeSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: AccountingYearAndPeriodRangeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/AccountingYearAndPeriodRangeSelection/AccountingYearAndPeriodRangeSelectionView.html",
            bindings: {
                onIntervalFromSelected: "&",
                onIntervalToSelected: "&",
                selectedRangeTo: "<",
                selectedRangeFrom: "="
            }
        };

        return options;
    }

    public static componentKey = "accountingYearAndPeriodRangeSelection";

    private accountYears: AccountYearLightDTO[];

    private selectedFromYear: AccountYearLightDTO;
    private selectableFromIntervals: SmallGenericType[];
    private selectedFromInterval: SmallGenericType;


    private selectedToYear: AccountYearLightDTO;
    private selectableToIntervals: SmallGenericType[];
    private selectedToInterval: SmallGenericType;

    private onSelected: (_: { selection: IYearAndPeriodSelectionDTO }) => void = angular.noop;

    private selectedRangeFrom: AccountIntervalSelectionDTO;
    private selectedRangeTo: AccountIntervalSelectionDTO;
    private userSelectionInput: YearAndPeriodSelectionDTO;
    private onAccountYearSelected: (_: { selection: SmallGenericType }) => void = angular.noop;
    private onIntervalFromSelected: (_: { selection: AccountIntervalSelectionDTO }) => void = angular.noop;
    private onIntervalToSelected: (_: { selection: AccountIntervalSelectionDTO }) => void = angular.noop;

    private initiallyLoaded: boolean = false;


    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService, private translationService: ITranslationService, private coreService: ICoreService, private reportDataService: IReportDataService) {
        this.loadAccountYears();
        this.$scope.$watch(() => this.selectedRangeFrom, () => {
            if (this.selectedRangeFrom) {
                this.initiallyLoaded = false;
                this.handlePeriodFrom(this.selectedRangeFrom);
            }
        }, true);

        this.$scope.$watch(() => this.selectedRangeTo, () => {
            if (this.selectedRangeTo) {
                this.initiallyLoaded = false;
                this.handlePeriodTo(this.selectedRangeTo);
            }
        }, true);
    }

    public $onInit() {
    }


    public intervalFromChanged(selection: SmallGenericType) {
        this.selectedFromInterval = selection;
        let id = selection ? selection.id : 0;
        if (!this.selectedToInterval || !this.selectedToInterval.id) {

            let toSelection = _.find(this.selectableToIntervals, a => a.id == id); //this.selectableToIntervals.find(x => x.id == id);
            if (toSelection) {
                this.intervalToChanged(toSelection);
            }
        }
        let interval = new AccountIntervalSelectionDTO(id, this.selectedFromYear.id);
        this.onIntervalFromSelected({ selection: interval });
    }

    public intervalToChanged(selection: SmallGenericType) {
        this.selectedToInterval = selection;
        let id = selection ? selection.id : 0;
        let interval = new AccountIntervalSelectionDTO(id, this.selectedToYear.id);
        this.onIntervalToSelected({ selection: interval });
    }

    private handlePeriodFrom(reportSelection: AccountIntervalSelectionDTO) {
        if (!reportSelection) return;
        this.reportDataService.getAccountYearIntervals(reportSelection.yearId).then(accountYear => {
            var selectedYear = _.find(this.accountYears, a => a.id == reportSelection.yearId);//this.accountYears.find(x => x.id == reportSelection.yearId);
            this.selectedFromYear = selectedYear;
            this.selectableFromIntervals = this.transformIntervals(accountYear.periods);
            var fromPeriod = _.find(this.selectableFromIntervals, a => a.id == reportSelection.value);//this.selectableFromIntervals.find(x => x.id == reportSelection.value);
            this.selectedFromInterval = fromPeriod;
            this.onIntervalFromSelected({ selection: reportSelection });
        });

    }

    private handlePeriodTo(reportSelection: AccountIntervalSelectionDTO) {
        if (!reportSelection) return;
        this.reportDataService.getAccountYearIntervals(reportSelection.yearId).then(accountYear => {
            var selectedYear = _.find(this.accountYears, a => a.id == reportSelection.yearId);//this.accountYears.find(x => x.id == reportSelection.yearId);
            this.selectedToYear = selectedYear;
            this.selectableToIntervals = this.transformIntervals(accountYear.periods);
            var toPeriod = _.find(this.selectableToIntervals, a => a.id == reportSelection.value); //this.selectableToIntervals.find(x => x.id == reportSelection.value);
            this.selectedToInterval = toPeriod;
            this.onIntervalToSelected({ selection: reportSelection });
        });
    }

    private accountYearChanged(item: SmallGenericType) {
        let selection = new SmallGenericType(item.id, item.name);
        this.onAccountYearSelected({ selection: selection });
    }


    public accountYearFromChanged(selection: SmallGenericType) {
        this.selectableFromIntervals = [];
        this.selectedFromYear = _.find(this.accountYears, a => a.id == selection.id);// this.accountYears.find(x => x.id == selection.id);

        if (this.selectedFromYear.id == 0) {
            this.intervalFromChanged(new SmallGenericType(0, ""));
        }
        else {
            let isSelectedToYearNull = false;
            if (this.selectedToYear.id == 0) {

                this.selectedToYear = _.find(this.accountYears, a => a.id == selection.id);
                isSelectedToYearNull = true;
                this.selectableToIntervals = [];
            }

            this.reportDataService.getAccountYearIntervals(selection.id).then(accountYear => {
                if (accountYear) {
                    this.selectableFromIntervals = this.transformIntervals(accountYear.periods);
                    this.intervalFromChanged(new SmallGenericType(0, ""));
                    if (isSelectedToYearNull) {
                        this.selectableToIntervals = this.transformIntervals(accountYear.periods);
                        this.intervalToChanged(new SmallGenericType(0, ""));
                    }
                }
            });
        }

    }

    public accountYearToChanged(selection: SmallGenericType) {
        var year = this.accountYears.find(x => x.id == selection.id);
        this.selectedToYear = year;
        this.selectableToIntervals = [];
        if (this.selectedToYear.id == 0) {
            this.intervalToChanged(new SmallGenericType(0, ""));
        }
        else {
            this.reportDataService.getAccountYearIntervals(selection.id).then(accountYear => {
                if (accountYear) {
                    this.selectableToIntervals = this.transformIntervals(accountYear.periods);
                }
            });
        }
    }

    private transformIntervals(intervals: AccountPeriodDTO[]): SmallGenericType[] {
        var transformed = new Array<SmallGenericType>();
        transformed.push(new SmallGenericType(0, ''));
        intervals.forEach(interval => {
            var name = interval.from.toString().substring(0, 7).replace("-", "");
            var genericType = new SmallGenericType(interval.accountPeriodId, name);
            transformed.push(genericType);
        });

        return transformed;
    }
    private getAccountYearIntervals(id: number): ng.IPromise<AccountYearDTO> {
        return this.reportDataService.getAccountYearIntervals(id).then(x => {
            return x;
        });
    }

    private loadAccountYears() {
        this.coreService.getCurrentAccountYear().then((x) => {
            const accountYearId =
                x?.status === TermGroup_AccountStatus.Open
                    ? x.accountYearId
                    : soeConfig.accountYearId;

            if (accountYearId && accountYearId !== 0) {
                this.getAccountYears(accountYearId);
            }
        });
    }

    private getAccountYears(currentAccountYearId: number) {
        this.initiallyLoaded = true;
        this.reportDataService.getAccountYears().then(accountYears => {
            this.accountYears = accountYears;
            let emptyrow = new AccountYearLightDTO();
            emptyrow.id = 0;
            emptyrow.name = "";
            this.accountYears.unshift(emptyrow);

            if (this.accountYears.length > 1 && (this.initiallyLoaded)) {
                const selectedStartInterval = this.accountYears.find(x => x.id === currentAccountYearId) ?? this.accountYears[1];
                this.selectedFromYear = selectedStartInterval;
                this.selectedToYear = selectedStartInterval;
                this.getAccountYearIntervals(selectedStartInterval.id).then(accountYearInformation => {
                    const transformedIntervals = this.transformIntervals(accountYearInformation.periods);
                    this.selectableFromIntervals = transformedIntervals;
                    this.selectableToIntervals = transformedIntervals;

                    let fromSelection = _.find(this.selectableFromIntervals, a => a.id == 0); //this.selectableToIntervals.find(x => x.id == id);
                    if (fromSelection) {
                        this.intervalFromChanged(fromSelection);
                    }

                });
            }
        });
    }
}
