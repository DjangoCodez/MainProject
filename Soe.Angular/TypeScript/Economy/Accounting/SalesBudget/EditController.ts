import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { BudgetHeadSalesDTO, BudgetRowSalesDTO } from "../../../Common/Models/BudgetDTOs";
import { Guid } from "../../../Util/StringUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SalesBudgetPreviousResultController } from "./Directives/PreviousResultController";
import { Feature, DistributionCodeBudgetType, TermGroup, TermGroup_AccountingBudgetType, BudgetHeadStatus } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { OpeningHoursDTO } from "../../../Common/Models/OpeningHoursDTO";
import { DistributionCodeHeadDTO } from "../../../Common/Models/DistributionCodeHeadDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";


export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private budgetHeadId: number;
    private budgetHead: BudgetHeadSalesDTO;

    // Lookups 
    public accountStdsDict: any;
    private distributionCodes: DistributionCodeHeadDTO[] = [];
    private distributionCodesDict: SmallGenericType[] = [];
    dim2s: any = [];
    dim3s: any = [];
    public accountDims: AccountDimSmallDTO[] = [];
    terms: any;
    intervalOptions: any[];
    selectedInterval: number;
    types: any = [];
    private openingHours: OpeningHoursDTO[];
    private earliestOpeningHour: number = 0;
    private latestClosingHour: number = 24;

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
    timerToken: number;
    modal: angular.ui.bootstrap.IModalService;
    currentRowNr: number;
    definitive: boolean = false;

    public ignorePassedPeriod: boolean;
    startDate: Date = new Date(Date.now());

    public _numberOfPeriods: any;
    get numberOfPeriods() {
        return this._numberOfPeriods;
    }
    set numberOfPeriods(item: any) {
        this._numberOfPeriods = item;
    }

    get isDisabled() {
        return this.budgetHead && this.budgetHead.status === BudgetHeadStatus.Active;
    }

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private translationService: ITranslationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService, private $scope: ng.IScope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadBudget())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.$scope.$on("rowUpdated", (event, guid) => {
            if (guid === this.guid)
                this.dirtyHandler.setDirty();
        });

        this.$scope.$on("getPreviousPeriodResult", (event, params) => {
            var dims = [];
            if (params.dim2Id && params.dim2Id != 0)
                dims.push(2);
            if (params.dim3Id && params.dim3Id != 0)
                dims.push(3);
            this.currentRowNr = params.budgetRowNr;
            this.getResult(false, params.dim1Id, dims);
        });
        this.modal = $uibModal;
    }

    public onInit(parameters: any) {
        this.budgetHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Accounting_SalesBudget, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_SalesBudget].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_SalesBudget].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
    }

    // LOOKUPS
    private loadDefiniteModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Accounting_SalesBudget);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (x[Feature.Economy_Accounting_SalesBudget])
                this.setDefinitePersmission = true;
        });
    }
    private onDoLookups() {
        return this.$q.all([this.loadDefiniteModifyPermissions(), this.getGridTerms(),
        this.loadAccountStds(),
        this.loadAccountDims(),
        this.loadBudgetIntervals(),
        this.loadTypes()]);
    }

    private loadBudget(resetRows: boolean = false): ng.IPromise<any> {
        if (this.budgetHeadId > 0) {
            return this.progress.startLoadingProgress([() => {
                return this.accountingService.getSalesBudgetV2(this.budgetHeadId).then(x => {
                    this.budgetHead = x;
                    if (!this.numberOfPeriods)
                        this.numberOfPeriods = this.budgetHead.noOfPeriods;

                    //Handle rows and periods
                    _.forEach(this.budgetHead.rows, (row: BudgetRowSalesDTO) => {
                        var periodCounter = 1;
                        _.forEach(row.periods, (period) => {
                            row["amount" + periodCounter] = period.amount;
                            row["quantity" + periodCounter] = period.quantity;
                            row.firstLevel = true;
                            periodCounter++;
                        });
                    });

                    // Get status
                    this.definitive = this.budgetHead.status === BudgetHeadStatus.Active;
                    this.isNew = false;

                    this.loadDistributionCodes();
                    this.loadOpeningHours();

                    if (resetRows)
                        this.$scope.$broadcast('resetRows', null);
                });
            }]);
        } else {
            this.new();
        }
    }

    private loadAccountStds(): ng.IPromise<any> {
        return this.accountingService.getAccountStdsNumberName(false).then(x => {
            this.accountStdsDict = x;
        });
    }

    private loadDistributionCodes(): ng.IPromise<any> {
        return this.accountingService.getDistributionCodes(true, false, this.budgetHead ? this.budgetHead.type : null, this.budgetHead ? this.budgetHead.fromDate : null, this.budgetHead ? this.budgetHead.toDate : null).then(x => {
            this.distributionCodes = x;

            this.distributionCodesDict = [];
            this.distributionCodesDict.push({ id: 0, name: '' });

            _.forEach(this.distributionCodes, code => {
                let name = code.name;
                if (code.fromDate)
                    name = "{0} ({1})".format(name, code.fromDate.toFormattedDate())
                this.distributionCodesDict.push({ id: code.distributionCodeHeadId, name: name });
            });
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false).then(x => {
            this.accountDims = x;
            _.forEach(this.accountDims, (y: any) => {
                if (y.accounts)
                    y.accounts.splice(0, 0, { accountId: 0, accountDimId: y.accountDimId, accountNr: " ", name: " ", numberName: " " });
            });
        });
    }

    private loadTypes(): ng.IPromise<any> {
        this.types = [];
        return this.coreService.getTermGroupContent(TermGroup.AccountingBudgetType, false, false).then((x) => {
            _.forEach(x, (row) => {
                if (row.id === TermGroup_AccountingBudgetType.SalesBudget || row.id === TermGroup_AccountingBudgetType.SalesBudgetTime || row.id === TermGroup_AccountingBudgetType.SalesBudgetSalaryCost)
                    this.types.push({ id: row.id, name: row.name });
            });
        });
    }

    private loadOpeningHours(): ng.IPromise<any> {
        return this.accountingService.getOpeningHours(false, this.budgetHead ? this.budgetHead.fromDate : null, this.budgetHead ? this.budgetHead.toDate : null).then((x) => {
            this.openingHours = x;
            var first: boolean = true;
            _.forEach(this.openingHours, (o) => {
                if (first) {
                    this.earliestOpeningHour = o.openingTime.getHours();
                    this.latestClosingHour = o.closingTime.getHours();
                    first = false;
                } else {
                    var opening = o.openingTime.getHours();
                    var closing = o.closingTime.getHours();
                    if (opening < this.earliestOpeningHour)
                        this.earliestOpeningHour = opening;
                    if (closing > this.latestClosingHour)
                        this.latestClosingHour = closing;
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
            "economy.accounting.salesbudget.department",
            "economy.accounting.salesbudget.usedepartment"
        ];

        return this.translationService.translateMany(keys).then((x) => {
            this.terms = x;
            this.useDim2Label = this.terms["economy.accounting.salesbudget.usedepartment"];
            this.useDim3Label = this.terms["economy.accounting.salesbudget.usedepartment"];
            this.dim2Label = this.terms["economy.accounting.salesbudget.department"]
            this.dim3Label = this.dim3name;
        });
    }

    private loadBudgetIntervals(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SalesBudgetInterval, false, true).then((x) => {
            this.intervalOptions = x;
            this.selectedInterval = 1;
        });
    }

    // ACTIONS

    public intervalChanged(item: any) {
        this.$timeout(() => {
            if (item == 1) //Year
                this.numberOfPeriods = 12;
            if (item == 2) //Month show weeks
                this.numberOfPeriods = 5;
            if (item == 3) { //Month Show days
                if (this.startDate)
                    this.numberOfPeriods = moment(this.startDate).daysInMonth();
                else
                    this.numberOfPeriods = 31;
            }
            if (item == 4) //Week
                this.numberOfPeriods = 7;
            if (item == 5) //Day
                this.numberOfPeriods = 24;
            if (this.budgetHeadId >= 0) {
                this.loadBudget();
            }
        });
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
        this.budgetHead.rows.splice(0);
    }

    public getResult(previous: boolean, accountId: number, dims: any) {
        this.currentGuid = Guid.newGuid();
        //this.startLoad();
        /*this.accountingService.getBalanceChangePerPeriod(this.currentGuid.toString(), this.budgetHead.noOfPeriods, this.budgetHead.accountYearId, accountId, previous, dims).then((x) => {
            this.timerToken = setInterval(() => this.getProgress(), 500);
        });*/
    }

    private getProgress() {
        this.coreService.getProgressInfo(this.currentGuid.toString()).then((x) => {
            //this.progressMessage = x.message;
            if (x.abort == true)
                this.getProcessedResult();
        });
    }

    private getProcessedResult() {
        clearInterval(this.timerToken);
        //this.stopProgress();
        this.accountingService.getBalanceChangeResult(this.currentGuid).then((x) => {
            var noOfRows = this.budgetHead.rows.length;
            if (noOfRows == 1 && this.currentRowNr) {
                var fromRow = x[0];
                var toRow = (_.filter(this.budgetHead.rows, { budgetRowNr: this.currentRowNr }))[0];
                for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                    toRow['amount' + i] = fromRow['amount' + i];
                }
                this.currentRowNr = 0;
            }
            else {
                this.budgetHead.rows = [];
                _.forEach(x, (row: BudgetRowSalesDTO) => {
                    row.budgetRowNr = noOfRows;
                    row.dim1Id = row.accountId;
                    var dim1 = (<any>_).find(this.accountStdsDict, { accountId: row.accountId });
                    if (dim1) {
                        row.dim1Name = dim1.name;
                        row.dim1Nr = dim1.number;
                    }
                    if (row.dim2Id != 0) {
                        var dim2 = (<any>_).find(this.accountStdsDict, { accountId: row.dim2Id });
                        if (dim2) {
                            row.dim2Name = dim2.name;
                            row.dim2Nr = dim2.accountNr;
                        }
                    }
                    if (row.dim3Id != 0) {
                        var dim3 = (<any>_).find(this.accountStdsDict, { accountId: row.dim3Id });
                        if (dim3) {
                            row.dim3Name = dim3.name;
                            row.dim3Nr = dim3.accountNr;
                        }
                    }

                    this.budgetHead.rows.push(row);
                    noOfRows++;
                });
            }
        });
    }

    public save() {
        //Set status
        this.budgetHead.status = this.definitive ? BudgetHeadStatus.Active : BudgetHeadStatus.Preliminary;

        //item to save
        //var head = this.budgetHead;
        //head.rows = _.filter()
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveSalesBudgetV2(this.budgetHead).then((result) => {
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
                // Clean
                this.dirtyHandler.clean();

                // Reload 
                this.loadBudget(true);
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

    // EVENTS

    public typeChanged() {
        this.$timeout(() => {
            this.loadDistributionCodes();
        });
    }

    private dateChanged() {
        this.$timeout(() => {
            this.loadDistributionCodes();
            this.loadOpeningHours();
        });
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.budgetHeadId = 0;
        this.budgetHead = new BudgetHeadSalesDTO();
        this.budgetHead.type = DistributionCodeBudgetType.SalesBudget;
        this.budgetHead.status = BudgetHeadStatus.Preliminary;
        this.budgetHead.noOfPeriods = 12;
        this.numberOfPeriods = this.budgetHead.noOfPeriods;
        this.budgetHead.rows = [];

        this.loadDistributionCodes();
        this.loadOpeningHours();

        this.selectedInterval = 1;
    }

    private openPreviousResultDialog() {
        var result: any = [];
        result.getPreviousResult = false;
        result.getDim2 = false;
        result.getDim3 = false;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("PreviousResult.html"),
            controller: SalesBudgetPreviousResultController,
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

    private addRow() {
        this.$scope.$broadcast('addRow', null);
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
                if (!this.budgetHead.distributionCodeHeadId || this.budgetHead.distributionCodeHeadId === 0) {
                    mandatoryFieldKeys.push("economy.accounting.distributioncode.distributioncode");
                }
            }
        });
    }
}
