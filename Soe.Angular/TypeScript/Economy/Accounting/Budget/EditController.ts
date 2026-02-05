import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { BudgetHeadFlattenedDTO, BudgetRowFlattenedDTO } from "../../../Common/Models/BudgetDTOs"
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Guid } from "../../../Util/StringUtility";
import { PreviousResultController } from "./Directives/PreviousResultController";
import { Feature, BudgetHeadStatus, DistributionCodeBudgetType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private budgetHeadId: number;
    budgetHead: BudgetHeadFlattenedDTO;

    // Lookups 
    public accountStdsDict: any;
    accountYears: any = [];
    accountYearsDict: any = [];
    distributionCodes: any = [];
    distributionCodesDict: any = [];
    dim2s: any = [];
    dim3s: any = [];
    public accountDims: AccountDimSmallDTO[] = [];
    terms: any;

    // Properties
    public setDefinitePersmission: boolean;
    public showDim2 = false;
    public showDim3 = false;
    public useDim2Label: string;
    public dim2Label: string; se;
    public showPassedPeriodsDialog = false;
    useDim3Label: string;
    dim3Label: string
    protected dim2name: string;
    protected dim3name: string;
    currentGuid: Guid;
    timerToken: any;
    modal: angular.ui.bootstrap.IModalService;
    currentRowNr: number;
    rowsModified: boolean;

    public ignorePassedPeriod: boolean;

    public _numberOfPeriods: any;
    get numberOfPeriods() {
        return this._numberOfPeriods;
    }
    set numberOfPeriods(item: any) {
        this._numberOfPeriods = item;
    }
    //@ngInject
    constructor(
        $uibModal,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private translationService: ITranslationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $q: ng.IQService, private $scope: ng.IScope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadBudget())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.$scope.$on("getPreviousPeriodResult", (event, params) => {
            var dims = [];
            if (params.dim2Id && params.dim2Id != 0)
                dims.push(2);
            if (params.dim3Id && params.dim3Id != 0)
                dims.push(3);
            this.currentRowNr = params.budgetRowNr;
            this.getResult(false, params.dim1Id, dims);
        });
        this.$scope.$on("rowUpdated", (event, guid) => {
            if (guid === this.guid)
                this.dirtyHandler.setDirty();
        });
        this.modal = $uibModal;
    }

    public onInit(parameters: any) {
        this.progress.setProgressBusy(true);
        this.budgetHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Accounting_Budget_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_Budget_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_Budget_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => {
            this.copy();
        }, () => this.isNew || !this.modifyPermission);
    }

    // LOOKUPS
    private loadDefiniteModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Accounting_Budget_Edit_Definite);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (x[Feature.Economy_Accounting_Budget_Edit_Definite])
                this.setDefinitePersmission = true;
        });
    }
    private onDoLookups() {
        return this.$q.all([this.loadDefiniteModifyPermissions(), this.getGridTerms(),
        this.loadAccountStds(), this.loadAccountYears(), this.loadDistributionCodes(),
        this.loadAccountDims()]);
    }

    private loadBudget(): ng.IPromise<any> {
        this.budgetHead = undefined;
        if (this.budgetHeadId > 0) {
            return this.accountingService.getBudget(this.budgetHeadId, true).then((x) => {
                this.numberOfPeriods = x.noOfPeriods;
                this.budgetHead = x;
                this.isNew = false;
                this.setDefaultValues();
                this.progress.setProgressBusy(false);
            });
        }
        else {
            this.new();
            this.setDefaultValues();
            this.progress.setProgressBusy(false);
        }
    }

    private setDefaultValues() {
        this.useDim2Label = this.terms["economy.accounting.budget.use"] + " " + this.dim2name;
        this.useDim3Label = this.terms["economy.accounting.budget.use"] + " " + this.dim3name;
        this.dim2Label = this.terms["economy.accounting.budget.default"] + " " + this.dim2name;
        this.dim3Label = this.terms["economy.accounting.budget.default"] + " " + this.dim3name;
    }

    private loadAccountStds(): ng.IPromise<any> {
        return this.accountingService.getAccountStdsNumberName(true).then((x) => {
            this.accountStdsDict = x;
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        this.accountYears = [];
        this.accountYearsDict = [];
        return this.accountingService.getAccountYears().then((x) => {
            this.accountYears = x;
            this.accountYearsDict.push({ id: 0, name: " " });
            _.forEach(this.accountYears, (year) => {
                this.accountYearsDict.push({ id: year.accountYearId, name: year.yearFromTo });
            });
        });
    }

    private loadDistributionCodes(): ng.IPromise<any> {
        return this.accountingService.getDistributionCodesByType(DistributionCodeBudgetType.AccountingBudget, true).then((x) => {
            this.distributionCodes = x;
            this.distributionCodesDict.push({ id: 0, name: '' })
            _.forEach(x, (y: any) => {
                this.distributionCodesDict.push({ id: y.distributionCodeHeadId, name: y.name })
            });
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, false, true, false).then(x => {
            this.accountDims = x;
            _.forEach(x, (y: any) => {
                if (y.accountDimNr == 2) {
                    this.showDim2 = true;
                    this.dim2name = y.name;
                    this.dim2s = y.accounts;
                }
                else if (y.accountDimNr == 3) {
                    this.showDim3 = true;
                    this.dim3name = y.name;
                    this.dim3s = y.accounts;
                }
                if (y.accounts) {
                    y.accounts.splice(0, 0, { accountId: 0, accountDimId: y.accountDimId, accountNr: " ", name: " ", numberName: " " });
                }
                else {
                    y.accounts = [];
                    y.accounts.push({ accountId: 0, accountDimId: y.accountDimId, accountNr: " ", name: " ", numberName: " " });
                }
            });
        });
    }

    private getGridTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "economy.accounting.account",
            "economy.accounting.budget.sum",
            "economy.accounting.budget.use",
            "economy.accounting.budget.default",
            "economy.accounting.budget.getresult",
            "economy.accounting.budget.getresultinfotext",
            "economy.accounting.budget.includecountdim",
            "economy.accounting.budget.getting"
        ];

        return this.translationService.translateMany(keys).then((x) => {
            this.terms = x;
        });
    }

    // ACTIONS

    public accountyearChanged(model) {
        var accountYear = this.accountYears.find(x => x.accountYearId === model);
        if (accountYear) 
            this.numberOfPeriods = accountYear['noOfPeriods'];
        this.openPreviousResultDialog();
    }

    public dim2Changed(model) {
        this.budgetHead.dim2Id = 0;
    }

    public dim3Changed(model) {
        this.budgetHead.dim3Id = 0;
    }

    public lock() {
        if (this.budgetHead.status !== BudgetHeadStatus.Active) {
            this.budgetHead.status = BudgetHeadStatus.Active;
            this.save();
        }
    }

    public unlock() {
        if (this.budgetHead.status !== BudgetHeadStatus.Preliminary) {
            this.budgetHead.status = BudgetHeadStatus.Preliminary;
            this.save();
        }
    }

    public clear() {
        const tempRows = [];
        _.forEach(this.budgetHead.rows, (r) => {
            r.isDeleted = true;
            if (r.budgetRowId)
                tempRows.push(r);
        });
        this.budgetHead.rows = tempRows;
        this.$scope.$broadcast('resetRows', { guid: this.guid });
        this.dirtyHandler.setDirty();
    }

    public getResult(previous: boolean, accountId: number, dims: any) {
        this.progress.showProgressDialog(this.terms["economy.accounting.budget.getting"] + "...");
        this.currentGuid = Guid.newGuid();

        if (accountId === 0)
            this.budgetHead.rows = [];

        //this.startLoad();
        this.accountingService.getBalanceChangePerPeriod(this.currentGuid.toString(), this.budgetHead.noOfPeriods, this.budgetHead.accountYearId, accountId, previous, dims).then((x) => {
            this.timerToken = setInterval(() => this.getProgress(accountId > 0), 500);
        });
    }

    private getProgress(keepExistingRows: boolean = false) {
        this.coreService.getProgressInfo(this.currentGuid.toString()).then((x) => {
            this.progress.updateProgressDialogMessage(this.terms["economy.accounting.budget.getting"] + " " + x.message);
            if (x.abort == true)
                this.getProcessedResult(keepExistingRows);
        });
    }

    private getProcessedResult(keepExistingRows: boolean = false) {
        clearInterval(this.timerToken);
        
        this.accountingService.getBalanceChangeResult(this.currentGuid).then((x) => {
            var noOfRows = this.budgetHead.rows.length;
            if (noOfRows == 1 && this.currentRowNr) {
                if (x.length > 0) {
                    var fromRow = x[0];
                    var toRow = (_.filter(this.budgetHead.rows, { budgetRowNr: this.currentRowNr }))[0];
                    for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                        toRow['amount' + i] = fromRow['amount' + i];
                    }
                }
                this.currentRowNr = 0;
            }
            else {
                if (!keepExistingRows)
                    this.budgetHead.rows = [];

                _.forEach(x, (row: BudgetRowFlattenedDTO) => {
                    row.budgetRowNr = noOfRows;
                    row.dim1Id = row.accountId;
                    var dim1 = (<any>_).find(this.accountStdsDict, { accountId: row.accountId });
                    if (dim1) {
                        row.dim1Name = dim1.name;
                        row.dim1Nr = dim1.number;
                    }
                    if (row.dim2Id != 0 && this.dim2s) {
                        var dim2 = (<any>_).find(this.dim2s, { accountId: row.dim2Id });
                        if (dim2) {
                            row.dim2Name = dim2.name;
                            row.dim2Nr = dim2.accountNr;
                        }
                        else {
                            row.dim2Name = "";
                            row.dim2Nr = "";
                        }
                    }
                    else {
                        row.dim2Name = "";
                        row.dim2Nr = "";
                    }
                    if (row.dim3Id != 0 && this.dim3s) {
                        var dim3 = (<any>_).find(this.dim3s, { accountId: row.dim3Id });
                        if (dim3) {
                            row.dim3Name = dim3.name;
                            row.dim3Nr = dim3.accountNr;
                        }
                        else {
                            row.dim3Name = "";
                            row.dim3Nr = "";
                        }
                    }
                    else {
                        row.dim3Name = "";
                        row.dim3Nr = "";
                    }

                    this.budgetHead.rows.push(row);
                    noOfRows++;
                });
            }

            this.$scope.$broadcast('resultLoaded', { guid: this.guid });
            this.progress.hideProgressDialog();
        });
    }

    public save() {
        if (!this.budgetHead.budgetHeadId || this.budgetHead.budgetHeadId == 0) {
            this.budgetHead.type = DistributionCodeBudgetType.AccountingBudget;
            this.budgetHead.status = BudgetHeadStatus.Preliminary;
        }

        // Remove new rows without data
        this.budgetHead.rows = _.filter(this.budgetHead.rows, (r) => r.budgetRowId || r.accountId || r.totalAmount || r.dim1Id || r.dim2Id || r.dim3Id);

        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveBudget(this.budgetHead).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.budgetHeadId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.budgetHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadBudget();
            }, error => {

            });
    }
    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteBudget(this.budgetHead.budgetHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.budgetHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.budgetHeadId = 0;
        this.budgetHead = new BudgetHeadFlattenedDTO();
        this.budgetHead.status = 1;
        this.budgetHead.noOfPeriods = 12;
        this.numberOfPeriods = this.budgetHead.noOfPeriods;
        this.budgetHead.rows = [];
    }

    protected copy() {
        this.isNew = true;
        this.budgetHeadId = 0;
        this.budgetHead.budgetHeadId = 0;
        this.budgetHead.status = 1;

        _.forEach(this.budgetHead.rows, (r) => {
            r.budgetHeadId = 0;
            r.budgetRowId = 0;
        });

        this.dirtyHandler.setDirty();
    }

    private openPreviousResultDialog() {
        var result: any = [];
        result.getPreviousResult = false;
        result.getDim2 = false;
        result.getDim3 = false;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("PreviousResult.html"),
            controller: PreviousResultController,
            controllerAs: "ctrl",
            resolve: {
                result: () => result,
                terms: () => this.terms,
                dim2Name: () => this.dim2name,
                dim3Name: () => this.dim3name
            }
        }
        this.modal.open(options).result.then((result: any) => {
            if (result.getPreviousResult) {
                var dims = [];
                if (result.getDim2 == true)
                    dims.push(2);
                if (result.getDim3)
                    dims.push(3);
                this.getResult(true, 0, dims);
            }
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.budgetHead) {
                if (!this.budgetHead.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this._numberOfPeriods || !this.budgetHead.noOfPeriods || this.budgetHead.noOfPeriods < 1) {
                    mandatoryFieldKeys.push("economy.accounting.budget.noofperiods");
                }
                if (!this.budgetHead.accountYearId) {
                    mandatoryFieldKeys.push("economy.accounting.accountyear.accountyear");
                }

                if (_.find(this.budgetHead.rows, (x) => {
                    return !x.dim1Id;
                })) {
                    mandatoryFieldKeys.push("economy.accounting.account");
                }
            }
        });
    }
}