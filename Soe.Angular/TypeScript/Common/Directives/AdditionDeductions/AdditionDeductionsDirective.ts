import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { AttestEmployeeAdditionDeductionDTO } from "../../Models/TimeEmployeeTreeDTO";
import { AddExpenseDialogController } from "../AddExpense/AddExpenseDialogController";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { AccountingSettingsRowDTO } from "../../Models/AccountingSettingsRowDTO";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { ICoreService } from "../../../Core/Services/CoreService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { ITimeCodeAdditionDeductionDTO } from "../../../Scripts/TypeLite.Net4";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";

export class AdditionDeductionsDirectiveFactory {

    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Directives/AdditionDeductions/AdditionDeductions.html'),
            scope: {
                rows: '=',
                readOnly: '=',
                employeeId: '=',
                timePeriodId: '=',
                standsOnDate: '=',
                isErp: '=',
                isMySelf: '=',
            },
            restrict: 'E',
            replace: true,
            controller: AdditionDeductionsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AdditionDeductionsController {

    // Init parameters
    private rows: AttestEmployeeAdditionDeductionDTO[];
    private readOnly: boolean;
    private employeeId: number;
    private timePeriodId: number;
    private standsOnDate: Date;
    private allRowsSelected: boolean;
    private isErp: boolean;
    private isMySelf: boolean = false;

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private settingTypes: SmallGenericType[] = [];
    private settings: AccountingSettingsRowDTO[];
    private timeCodes: ITimeCodeAdditionDeductionDTO[];

    // Properties
    private sortBy: string[] = ['start', 'stop', 'timeCodeName'];
    private sortByReverse: boolean = false;

    private progress: IProgressHandler;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private coreService: ICoreService) {
        this.progress = progressHandlerFactory.create();
    }

    public $onInit() {
        this.loadTerms().then(() => {
            this.settingTypes.push(new SmallGenericType(0, this.terms["common.accountingsettings.account"]));
        });
        this.loadTimeCodes();
    }

    // SERVICE CALLS

    private loadTimeCodes(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
            return this.coreService.getAdditionDeductionTimeCodes(true, this.isMySelf).then(x => {
                this.timeCodes = x
            });
        }]);
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.accountingsettings.account",
            "core.warning",
            "core.deletewarning"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    private getAccounting(accountingString: string): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (accountingString) {
            this.coreService.getAccountingFromString(accountingString).then(x => {
                this.settings = [x];
                deferral.resolve();
            });
        } else {
            this.settings = [];
            deferral.resolve();
        }

        return deferral.promise;
    }

    private deleteTransaction(row: any) {

        this.coreService.deleteExpenseRow(row.expenseRowId).then((result) => {
            this.messagingService.publish(Constants.EVENT_ADDITIONDEDUCTION_ROWS_RELOAD, null);
        }, error => {
            this.messagingService.publish(Constants.EVENT_ADDITIONDEDUCTION_ROWS_RELOAD, null);
        });
    }

    // EVENTS

    private showComment(row: AttestEmployeeAdditionDeductionDTO) {
        var keys: string[] = [
            "common.comment",
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["common.comment"], row.comment, SOEMessageBoxImage.Information);
        });
    }

    private selectAllRows() {
        this.allRowsSelected = !this.allRowsSelected;

        _.forEach(this.rows, (row: AttestEmployeeAdditionDeductionDTO) => {
            row['selected'] = this.allRowsSelected;
        });

        this.messagingService.publish(Constants.EVENT_ADDITIONDEDUCTION_ROWS_SELECTED, _.filter(this.rows, r => r['selected']));
    }

    private rowSelected(row: AttestEmployeeAdditionDeductionDTO) {
        if (row['readOnly'])
            return;

        row['selected'] = !row['selected'];

        this.messagingService.publish(Constants.EVENT_ADDITIONDEDUCTION_ROWS_SELECTED, _.filter(this.rows, r => r['selected']));
    }

    private rowExpanded(row: AttestEmployeeAdditionDeductionDTO) {
        row['expanded'] = !row['expanded'];
    }

    private sort(column: string) {
        this.sortByReverse = !this.sortByReverse && this.sortBy[0] == column;
        this.sortBy = [column];
    }

    private editRow(row: AttestEmployeeAdditionDeductionDTO) {
        this.getAccounting(row ? row.accounting : '').then(() => {
            var result: any;
            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/AddExpense/AddExpenseDialog.html"),
                controller: AddExpenseDialogController,
                controllerAs: "ctrl",
                size: 'lg',
                resolve: {
                    readOnly: () => { return this.readOnly },
                    isMySelf: () => { return true },
                    settings: () => { return this.settings },
                    settingTypes: () => { return this.settingTypes },
                    employeeId: () => { return this.employeeId },
                    standsOnDate: () => { return this.standsOnDate },
                    isProjectMode: () => { return this.isErp },
                    timePeriodId: () => { return this.timePeriodId },
                    projectId: () => { return undefined },
                    customerInvoiceId: () => { return undefined },
                    expenseRowId: () => { return row ? row.expenseRowId : undefined },
                    timeCodes: () => { return this.timeCodes },
                    employees: () => { return [] },
                    currencyCode: () => { return "" },
                    priceListTypeInclusiveVat: () => { return row ? row.priceListInclVat : false },
                    hasFiles: () => { return false }
                }
            }
            this.$uibModal.open(options).result.then((result: any) => {
                if (result) {
                    this.messagingService.publish(Constants.EVENT_ADDITIONDEDUCTION_ROWS_RELOAD, null);
                    //if (this.onChange)
                    //    this.onChange();
                }
            });

        });
    }

    private deleteRow(row: AttestEmployeeAdditionDeductionDTO) {

        const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val === true) {
                _.pull(this.rows, row);
                this.deleteTransaction(row);
            }
        });
    }

    // HELP-METHODS  

    private showTimeStart(row: AttestEmployeeAdditionDeductionDTO) {

        if (row.start)
            row.start = CalendarUtility.defaultDateToNull(row.start);

        return row.start && !(row.start.isBeginningOfDay());
    }

    private showTimeStop(row: AttestEmployeeAdditionDeductionDTO) {

        if (row.stop)
            row.stop = CalendarUtility.defaultDateToNull(row.stop);

        return row.stop && !(row.stop.isBeginningOfDay());
    }

    private isReadOnly(row: AttestEmployeeAdditionDeductionDTO) {
        return row.isReadOnly || this.readOnly;

    }
}