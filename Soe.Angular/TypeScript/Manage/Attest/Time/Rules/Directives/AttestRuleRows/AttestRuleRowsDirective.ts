import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { AttestRuleRowDTO } from "../../../../../../Common/Models/AttestRuleHeadDTO";
import { IAttestService } from "../../../../AttestService";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { WildCard, TermGroup, SoeTimeCodeType, TermGroup_InvoiceProductVatType, TermGroup_AttestRuleRowLeftValueType, TermGroup_AttestRuleRowRightValueType } from "../../../../../../Util/CommonEnumerations";
import { AttestRuleRowsDialogController } from "./AttestRuleRowsDialogController";
import { TimeCalendarPeriodDTO } from "../../../../../../Common/Models/TimeCalendarDTOs";



export class AttestRuleRowsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Manage/Attest/Time/Rules/Directives/AttestRuleRows/Views/AttestRuleRows.html'),
            scope: {
                attestRuleHeadId: '=',
                attestRuleRows: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AttestRuleRowsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AttestRuleRowsController {

    // Init parameters
    private attestRuleHeadId: number;
    private attestRuleRows: AttestRuleRowDTO[] = [];

    // Data
    private comparisonOperators: SmallGenericType[];
    private leftOperators: SmallGenericType[];
    private rightOperators: SmallGenericType[];
    private timeCodes: SmallGenericType[];
    private payrollProducts: SmallGenericType[];
    private invoiceProducts: SmallGenericType[];

    // Flags
    private readOnly: boolean;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private attestService: IAttestService) {

        this.setupComparisonOperators();
        this.$q.all([
            this.loadLeftOperators(),
            this.loadRightOperators(),
            this.loadTimeCodes(),
            this.loadPayrollProducts(),
            this.loadInvoiceProducts(),
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupComparisonOperators() {
        this.comparisonOperators = [];
        this.comparisonOperators.push(new SmallGenericType(WildCard.LessThan, "<"));
        this.comparisonOperators.push(new SmallGenericType(WildCard.LessThanOrEquals, "<="));
        this.comparisonOperators.push(new SmallGenericType(WildCard.Equals, "="));
        this.comparisonOperators.push(new SmallGenericType(WildCard.GreaterThanOrEquals, ">="));
        this.comparisonOperators.push(new SmallGenericType(WildCard.GreaterThan, ">"));
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.attestRuleRows, () => {
            this.setAllRowNames();
        });
    }

    // SERVICE CALLS

    private loadLeftOperators(): ng.IPromise<any> {
        this.leftOperators = [];
        return this.coreService.getTermGroupContent(TermGroup.AttestRuleRowLeftValueType, true, true).then(x => {
            this.leftOperators = x;
        });
    }

    private loadRightOperators(): ng.IPromise<any> {
        this.rightOperators = [];
        return this.coreService.getTermGroupContent(TermGroup.AttestRuleRowRightValueType, true, true).then(x => {
            this.rightOperators = x;
        });
    }

    private loadTimeCodes() {
        this.timeCodes = [];
        return this.attestService.getTimeCodesDict(SoeTimeCodeType.None, true, true).then(x => {
            this.timeCodes = x;
        });
    }

    private loadPayrollProducts() {
        this.payrollProducts = [];
        return this.attestService.getPayrollProductsDict(true, true, true).then(x => {
            this.payrollProducts = x;
        });
    }

    private loadInvoiceProducts() {
        this.invoiceProducts = [];
        return this.attestService.getInvoiceProductsDict(TermGroup_InvoiceProductVatType.Service, true).then(x => {
            this.invoiceProducts = x;
        });
    }

    // EVENTS

    private editRow(row: AttestRuleRowDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/Attest/Time/Rules/Directives/AttestRuleRows/Views/AttestRuleRowsDialog.html"),
            controller: AttestRuleRowsDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                row: () => { return row },
                comparisonOperators: () => { return this.comparisonOperators },
                leftOperators: () => { return this.leftOperators },
                rightOperators: () => { return this.rightOperators },
                timeCodes: () => { return this.timeCodes },
                payrollProducts: () => { return this.payrollProducts },
                invoiceProducts: () => { return this.invoiceProducts }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.row) {
                if (!row) {
                    // Add new row to the original collection
                    row = new AttestRuleRowDTO();
                    row.attestRuleHeadId = this.attestRuleHeadId;
                    this.attestRuleRows.push(row);
                }

                row.leftValueId = result.row.leftValueId;
                row.leftValueType = result.row.leftValueType;
                row.comparisonOperator = result.row.comparisonOperator;
                row.rightValueId = result.row.rightValueId;
                row.rightValueType = result.row.rightValueType;
                row.minutes = result.row.minutes;
                this.setRowNames(row);

                this.setAsDirty();
            }
        });
    }

    private deleteRow(row: AttestRuleRowDTO) {
        _.pull(this.attestRuleRows, row);

        this.setAsDirty();
    }

    // HELP-METHODS

    private setRowNames(row: AttestRuleRowDTO) {
        row.setLeftValueTypeName(this.leftOperators);
        row.setLeftValueIdName(this.timeCodes, this.payrollProducts, this.invoiceProducts);
        row.setComparisonOperatorString(this.comparisonOperators);
        row.setRightValueTypeName(this.rightOperators);
        row.setRightValueIdName(this.timeCodes, this.payrollProducts, this.invoiceProducts);
    }

    private setAllRowNames() {
        _.forEach(this.attestRuleRows, row => {
            this.setRowNames(row);
        });
    }

    private getTimeCodeName(id: number): string {
        let timeCode = _.find(this.timeCodes, t => t.id === id);
        return timeCode ? timeCode.name : '';
    }

    private getPayrollProductName(id: number): string {
        let prod = _.find(this.payrollProducts, p => p.id === id);
        return prod ? prod.name : '';
    }

    private getInvoiceProductName(id: number): string {
        let prod = _.find(this.invoiceProducts, p => p.id === id);
        return prod ? prod.name : '';
    }

    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}