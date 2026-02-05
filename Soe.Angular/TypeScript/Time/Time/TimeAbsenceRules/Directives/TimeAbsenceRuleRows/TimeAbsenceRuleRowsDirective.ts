import { PayrollProductDTO } from "../../../../../Common/Models/ProductDTOs";
import { TimeAbsenceRuleRowDTO, TimeAbsenceRuleRowPayrollProductsDTO } from "../../../../../Common/Models/TimeAbsenceRuleHeadDTO";
import { TimeAbsenceRuleRowDialogController } from "./TimeAbsenceRuleRowDialogController";
import { TimeAbsenceRulePayrollProductRowDialogController } from "./TimeAbsenceRulePayrollProductRowDialogController";
import { SoeEntityState, TermGroup, TermGroup_TimeAbsenceRuleRowScope, TermGroup_TimeAbsenceRuleRowType, TermGroup_TimeAbsenceRuleType } from "../../../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITimeService } from "../../../../Time/TimeService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";

export class TimeAbsenceRuleRowsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeAbsenceRules/Directives/TimeAbsenceRuleRows/Views/TimeAbsenceRuleRows.html'),
            scope: {
                timeAbsenceRuleHeadId: '=',
                timeAbsenceRuleRows: '=',
                type: '<',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TimeAbsenceRuleRowsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class TimeAbsenceRuleRowsController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeAbsenceRuleHeadId: number;
    private type: number;
    private timeAbsenceRuleRows: TimeAbsenceRuleRowDTO[];
    private selectedTimeAbsenceRuleRow: TimeAbsenceRuleRowDTO;
    private types: ISmallGenericType[] = [];
    private scopes: ISmallGenericType[] = [];
    private payrollProducts: PayrollProductDTO[] = [];

    // Flags
    private readOnly: boolean;
    private allPayrollProductRowsExpanded: boolean = false;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private timeService: ITimeService) {
    }

    public $onInit() {  
        this.timeAbsenceRuleRows = _.orderBy(this.timeAbsenceRuleRows, ['start', 'stop']);

        this.$q.all([
            this.loadTerms(),
            this.loadTypes(),
            this.loadScopes(),
            this.loadPayrollProducts()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watchCollection(() => this.timeAbsenceRuleRows, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                this.selectedTimeAbsenceRuleRow = this.timeAbsenceRuleRows.length ? this.timeAbsenceRuleRows[0] : null;
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeAbsenceRuleRowType, false, true).then(x => {
            this.types = x;
        });
    }

    private loadScopes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeAbsenceRuleRowScope, false, false).then(x => {
            this.scopes = x;
        });
    }

    private loadPayrollProducts() {
        this.timeService.getPayrollProducts(false).then((x) => {
            this.payrollProducts = x;
        });
    }

    // EVENTS

    private showPayrollProduct(): boolean {
        if (this.type === TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_UNPAID)
            return false;
        if (this.type === TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_PAID)
            return false;
        if (this.type === TermGroup_TimeAbsenceRuleType.SickDuringStandby_PAID)
            return false;
        if (this.type === TermGroup_TimeAbsenceRuleType.SickDuringStandby_UNPAID)
            return false;
        if (this.type === TermGroup_TimeAbsenceRuleType.Vacation)
            return false;
        return true;
    }

    private showScope(): boolean {
        if (this.type === TermGroup_TimeAbsenceRuleType.TemporaryParentalLeave_PAID)
            return true;
        if (this.type === TermGroup_TimeAbsenceRuleType.TemporaryParentalLeave_UNPAID)
            return true;
        return false;
    }
    
    private expandAllPayrollProductRows() {
        _.forEach(this.timeAbsenceRuleRows, timeAbsenceRuleRow => {
            timeAbsenceRuleRow['expanded'] = true;
        });
        this.allPayrollProductRowsExpanded = true;
    }

    private collapseAllPayrollProductRows() {
        _.forEach(this.timeAbsenceRuleRows, timeAbsenceRuleRow => {
            timeAbsenceRuleRow['expanded'] = false;
        });
        this.allPayrollProductRowsExpanded = false;
    }

    private editTimeAbsenceRuleRow(timeAbsenceRuleRow: TimeAbsenceRuleRowDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAbsenceRules/Directives/TimeAbsenceRuleRows/Views/TimeAbsenceRuleRowDialog.html"),
            controller: TimeAbsenceRuleRowDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {
                timeAbsenceRuleRow: () => { return timeAbsenceRuleRow },
                type: () => { return this.type },
                showPayrollProduct: () => { return this.showPayrollProduct() },
                showScope: () => { return this.showScope() },
                types: () => { return this.getValidRowTypes() },
                scopes: () => { return this.scopes },
                defaultScope: () => { return this.getDefaultScope() },
                payrollProducts: () => { return this.payrollProducts },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.timeAbsenceRuleRow) {
                if (!timeAbsenceRuleRow) {
                    timeAbsenceRuleRow = new TimeAbsenceRuleRowDTO();
                    timeAbsenceRuleRow.payrollProductRows = [];
                    this.timeAbsenceRuleRows.push(timeAbsenceRuleRow);
                    this.$timeout(() => {
                        this.selectedTimeAbsenceRuleRow = timeAbsenceRuleRow;
                    });
                }

                timeAbsenceRuleRow.timeAbsenceRuleRowId = result.timeAbsenceRuleRow.timeAbsenceRuleRowId;
                timeAbsenceRuleRow.start = result.timeAbsenceRuleRow.start;
                timeAbsenceRuleRow.stop = result.timeAbsenceRuleRow.stop;
                timeAbsenceRuleRow.type = result.timeAbsenceRuleRow.type;
                timeAbsenceRuleRow.scope = result.timeAbsenceRuleRow.scope;
                timeAbsenceRuleRow.payrollProductId = result.timeAbsenceRuleRow.payrollProductId;

                // Set typeName
                let type = _.find(this.types, p => p.id === timeAbsenceRuleRow.type);
                if (type) {
                    timeAbsenceRuleRow.typeName = type.name;
                }

                // Set scopeName
                let scope = _.find(this.scopes, p => p.id === timeAbsenceRuleRow.scope);
                if (type) {
                    timeAbsenceRuleRow.scopeName = scope.name;
                }

                // Set PayrollProduct number and name
                let payrollProduct = _.find(this.payrollProducts, p => p.productId === timeAbsenceRuleRow.payrollProductId);
                if (payrollProduct) {
                    timeAbsenceRuleRow.payrollProductNr = payrollProduct.number;
                    timeAbsenceRuleRow.payrollProductName = payrollProduct.name;
                }

                this.setAsDirty();
            }
        });
    }

    private getValidRowTypes(): ISmallGenericType[] {
        let validTypes = [];

        switch (this.type) {
            case TermGroup_TimeAbsenceRuleType.Vacation:
                return _.filter(this.types, (t) =>
                    t.id == TermGroup_TimeAbsenceRuleRowType.CalendarDay
                );
            case TermGroup_TimeAbsenceRuleType.Sick_PAID:
            case TermGroup_TimeAbsenceRuleType.Sick_UNPAID:
            case TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_PAID:
            case TermGroup_TimeAbsenceRuleType.SickDuringInconvenientWorkingHours_UNPAID:
            case TermGroup_TimeAbsenceRuleType.SickDuringStandby_PAID:
            case TermGroup_TimeAbsenceRuleType.SickDuringStandby_UNPAID:
            case TermGroup_TimeAbsenceRuleType.WorkInjury_PAID:
                return this.types;
            default:
                return _.filter(this.types, (t) =>
                    t.id == TermGroup_TimeAbsenceRuleRowType.CalendarDay ||
                    t.id == TermGroup_TimeAbsenceRuleRowType.PartOfDay
                );
        }
    }

    private deleteTimeAbsenceRuleRow(timeAbsenceRuleRow: TimeAbsenceRuleRowDTO) {
        if (this.timeAbsenceRuleRows)
            _.pull(this.timeAbsenceRuleRows, timeAbsenceRuleRow);
        this.setAsDirty();
    }

    private editPayrollProductRow(payrollProductRow: TimeAbsenceRuleRowPayrollProductsDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAbsenceRules/Directives/TimeAbsenceRuleRows/Views/TimeAbsenceRulePayrollProductRowDialog.html"),
            controller: TimeAbsenceRulePayrollProductRowDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                payrollProductRow: () => { return payrollProductRow },
                payrollProducts: () => { return this.payrollProducts },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.payrollProductRow && this.selectedTimeAbsenceRuleRow) {
                if (!payrollProductRow) {
                    payrollProductRow = new TimeAbsenceRuleRowPayrollProductsDTO();
                    if (!this.selectedTimeAbsenceRuleRow.payrollProductRows)
                        this.selectedTimeAbsenceRuleRow.payrollProductRows = [];
                    this.selectedTimeAbsenceRuleRow.payrollProductRows.push(payrollProductRow);
                    this.selectedTimeAbsenceRuleRow['expanded'] = true;
                }

                payrollProductRow.sourcePayrollProductId = result.payrollProductRow.sourcePayrollProductId;
                payrollProductRow.targetPayrollProductId = result.payrollProductRow.targetPayrollProductId;

                // Set source PayrollProduct number and name
                let sourcePayrollProduct = _.find(this.payrollProducts, p => p.productId === payrollProductRow.sourcePayrollProductId);
                if (sourcePayrollProduct) {
                    payrollProductRow.sourcePayrollProductNr = sourcePayrollProduct.number;
                    payrollProductRow.sourcePayrollProductName = sourcePayrollProduct.name;
                }

                // Set target PayrollProduct number and name
                let targetPayrollProduct = _.find(this.payrollProducts, p => p.productId === payrollProductRow.targetPayrollProductId);
                if (targetPayrollProduct) {
                    payrollProductRow.targetPayrollProductNr = targetPayrollProduct.number;
                    payrollProductRow.targetPayrollProductName = targetPayrollProduct.name;
                }

                this.setAsDirty();
            }
        });
    }

    private deletePayrollProductRow(payrollProductRow: TimeAbsenceRuleRowPayrollProductsDTO) {
        if (this.selectedTimeAbsenceRuleRow && this.selectedTimeAbsenceRuleRow.payrollProductRows)
            _.pull(this.selectedTimeAbsenceRuleRow.payrollProductRows, payrollProductRow);
        this.setAsDirty();
    }

    // HELP-METHIDS

    private getDefaultScope(): TermGroup_TimeAbsenceRuleRowScope {
        if (this.timeAbsenceRuleRows && _.filter(this.timeAbsenceRuleRows, r => r.scope == TermGroup_TimeAbsenceRuleRowScope.Calendaryear && r.state == SoeEntityState.Active).length > 0)
            return TermGroup_TimeAbsenceRuleRowScope.Calendaryear;
        return TermGroup_TimeAbsenceRuleRowScope.Coherent;
    }

    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}