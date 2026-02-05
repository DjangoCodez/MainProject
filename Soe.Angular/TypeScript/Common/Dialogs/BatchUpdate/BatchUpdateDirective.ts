import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { BatchUpdateFieldType, SoeEntityType } from "../../../Util/CommonEnumerations";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { BatchUpdateDTO } from "../../Models/BatchUpdateDTO";
import { SmallGenericType } from "../../Models/SmallGenericType";

export class BatchUpdateController {
    private batchUpdates: BatchUpdateDTO[] = [];
    private selectedField: number;
    private count: number;
    private terms: any;
    private isLoaded: boolean = false;
    private latestSelectedFromDate: Date;
    private latestSelectedToDate: Date;
    private filterTranslactionKey: string;
    private filterOptions: SmallGenericType[] = [];
    private filterSelected: SmallGenericType[] = [];
    private get addedBatchUpdates(): BatchUpdateDTO[] {
        return this.batchUpdates.filter(i => i.added == true);
    }
    private notAddedBatchUpdates: BatchUpdateDTO[] = [];

    private get entityTypeIsEmployee(): boolean {
        return this.entityType === SoeEntityType.Employee;
    }
    private get entityTypeIsPayrollProduct(): boolean {
        return this.entityType === SoeEntityType.PayrollProduct;
    }
    private doUseFilter(): boolean {
        return this.entityTypeIsPayrollProduct;
    }
    private doShowFilter(): boolean {
        if (this.doUseFilter && _.filter(this.addedBatchUpdates, b => b.doShowFilter).length > 0)
            return true;
        return false;
    }
    private showStartDateOnPayrollProduct(): boolean {
        if (this.entityTypeIsPayrollProduct &&  _.filter(this.addedBatchUpdates, b => b.doShowFromDate).length > 0)
            return true;
        return false;
    }
    private showToDateOnPayrollProduct(): boolean {
        if (this.entityTypeIsPayrollProduct && _.filter(this.addedBatchUpdates, b => b.doShowToDate).length > 0)
            return true;
        return false;
    }
    private doShowStartDate(): boolean {
        return this.entityTypeIsEmployee || this.showStartDateOnPayrollProduct(); 
    }
    private doShowToDate(): boolean {
        return this.entityTypeIsEmployee || this.showToDateOnPayrollProduct();
    }
    private doShowDates(): boolean {
        return this.doShowStartDate() || this.doShowToDate();
    }
    private validationHandler: IValidationSummaryHandler;
    private dialogform: ng.IFormController;
    private progress: IProgressHandler;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private progressHandlerFactory: IProgressHandlerFactory,
        private notificationService: INotificationService,
        private validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private entityType: SoeEntityType,
        private selectedIds: number[]
    ) {
        this.validationHandler = validationSummaryHandlerFactory.create();
        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();
        this.progress.startLoadingProgress([() => {
            return this.$q.all([
                this.getBatchUpdateForEntity(),
                this.getBatchUpdateFilterOptions(),
                this.loadTerms()
            ]).then(() => {
                this.isLoaded = true;
            }).catch(() => {
                this.cancel()
            })
        }]);
        this.count = selectedIds.length;
    }
    private getBatchUpdateForEntity(): ng.IPromise<any> {
        return this.coreService.getBatchUpdate(this.entityType).then(data => {
            this.batchUpdates = data;
            this.setNotAddedBatchUpdates();
        })
    }
    private getBatchUpdateOptions(batchUpdate: BatchUpdateDTO) {
        return this.coreService.refreshBatchUpdateOptions(this.entityType, batchUpdate).then((refreshedBatchUpdate) => {
            batchUpdate.options = refreshedBatchUpdate.options;
            if (batchUpdate.children && batchUpdate.children.length > 0 && refreshedBatchUpdate.children && refreshedBatchUpdate.children.length == batchUpdate.children.length) {
                for (let i = 0; i < batchUpdate.children.length; i++) {
                    batchUpdate.children[i].options = refreshedBatchUpdate.children[i].options;
                }
            }
        });
    }
    private getBatchUpdateFilterOptions(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.doUseFilter) {
            this.coreService.getBatchUpdateFilterOptions(this.entityType).then(data => {
                this.filterOptions = data;
                this.setFilterTranslactionKey();
            });
        }
        deferral.resolve();
        return deferral.promise;
    }
    private loadTerms(): ng.IPromise<any> {
        const keys = [
            "common.batchupdate.info",
            "common.batchupdate.selectedrows",
            "common.batchupdate.confirm",
            "common.batchupdate.howto",
            "common.batchupdate.datefromnotvalid",
            "core.info",
            "core.warning"
        ]
        return this.translationService.translateMany(keys).then(terms => {
            terms["common.batchupdate.info"] = terms["common.batchupdate.info"].replace("{0}", String(this.count));
            terms["common.batchupdate.selectedrows"] = terms["common.batchupdate.selectedrows"].replace("{0}", String(this.count));
            this.terms = terms;
        })
    }
    private addRow() {
        const selectedBatchUpdate = this.batchUpdates.find(i => i.field == this.selectedField);
        if (selectedBatchUpdate) {
            selectedBatchUpdate.added = true;            
            if (selectedBatchUpdate.doShowFromDate && this.latestSelectedFromDate)
                selectedBatchUpdate.fromDate = this.latestSelectedFromDate;
            if (selectedBatchUpdate.doShowToDate && this.latestSelectedToDate)
                selectedBatchUpdate.toDate = this.latestSelectedToDate;
            if (selectedBatchUpdate.dataType == BatchUpdateFieldType.Id && (!selectedBatchUpdate.options || selectedBatchUpdate.options.length == 0))
                this.getBatchUpdateOptions(selectedBatchUpdate);
            this.setNotAddedBatchUpdates();
        }
    }
    private removeRow(batchUpdate: any) {
        batchUpdate.added = false;
        this.setNotAddedBatchUpdates();
    }
    private ok() {
        this.validate().then(passed => {
            if (passed) {
                this.notificationService
                    .showDialog(this.terms["core.warning"], this.terms["common.batchupdate.confirm"].replace("{0}", String(this.count)), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo)
                    .result
                    .then(val => {
                        if (val === true) {
                            this.startSave();
                        }
                    });
            }
        });
    }
    private startSave() {
        this.progress.startSaveProgress(
            (completion) => {
                return this.coreService.performBatchUpdate({
                    ids: this.selectedIds,
                    filterIds: this.getSelectedFilterIds(),
                    batchUpdates: this.addedBatchUpdates,
                    entityType: this.entityType
                }).then((result) => {                    
                    if (result.success) {
                        if (result.infoMessage && result.infoMessage.length > 0) {
                            this.notificationService.showDialogEx(this.terms["core.info"], result.infoMessage, SOEMessageBoxImage.Information).result.then(val => {
                                if (val) {
                                    this.$uibModalInstance.close();
                                    completion.completed(null, null, true);
                                }
                            }, (reason) => {
                                // User cancelled
                                    this.$uibModalInstance.close();
                                    completion.completed(null, null, true);
                            });                            
                        }
                        else {
                            this.$uibModalInstance.close();
                            completion.completed(null, null, false);                         
                        }
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                })
            },
            "",
        )
    }
    private validate(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();
        
        let validationErrors: string = '';
        let isValid: boolean = true;

        if(this.entityTypeIsEmployee){
            _.forEach(this.addedBatchUpdates, (item: BatchUpdateDTO) => {
                if (item.doShowFromDate && !item.fromDate) {
                    validationErrors = this.terms["common.batchupdate.datefromnotvalid"];
                    isValid = false;
                    return false;
                }
            });
        }
        if (!isValid)
            this.notificationService.showDialog(this.terms["core.unabletosave"], validationErrors, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);

        deferral.resolve(isValid);

        return deferral.promise;

    }
    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
    private selectedFromDateChanged(batchUpdate: BatchUpdateDTO) {
        this.$timeout(() => {
            this.latestSelectedFromDate = batchUpdate.fromDate;
        });
    }
    private setFilterTranslactionKey() {
        if (this.entityType == SoeEntityType.PayrollProduct)
            this.filterTranslactionKey = "time.employee.payrollgroup.payrollgroups";
    }
    private getSelectedFilterIds(): number[] {
        let ids: number[] = [];
        if (this.filterSelected && this.filterSelected.length > 0)
            ids = this.filterSelected.map(f => f.id);
        return ids;
    }
    private selectedToDateChanged(batchUpdate: BatchUpdateDTO) {
        this.$timeout(() => {
            this.latestSelectedToDate = batchUpdate.toDate;
        });
    }
    private setNotAddedBatchUpdates() {
        this.notAddedBatchUpdates = this.batchUpdates.filter(i => !i.added);
    }
    private disableSave():boolean {
        return !this.addedBatchUpdates || this.addedBatchUpdates.length == 0
    }
    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.doShowFilter() && (!this.filterSelected || this.filterSelected.length === 0))
                mandatoryFieldKeys.push(this.filterTranslactionKey);
        });
    }
    private checkSelected() {
        if (this.entityType == SoeEntityType.PayrollProduct) {
            this.$timeout(() => { }, 10).then(() => {
                if (this.filterSelected && this.filterSelected.length > 0) {
                    if (this.filterSelected && this.filterSelected.length > 0) {
                        if (this.filterSelected[this.filterSelected.length - 1].id === -1)
                            this.filterSelected = this.filterSelected.filter(i => i.id == -1);
                        else
                            this.filterSelected = this.filterSelected.filter(i => i.id !== -1);
                    }
                }
            });
        }
    }
}
