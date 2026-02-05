import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { RetroactivePayrollDTO, RetroactivePayrollEmployeeDTO, RetroactivePayrollAccountDTO } from "../../../Common/Models/RetroactivePayroll";
import { TimePeriodHeadGridDTO } from "../../../Common/Models/TimePeriodHeadDTO";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { IPayrollService } from "../PayrollService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup_TimePeriodType, SoeCategoryType, TermGroup, TermGroup_SoeRetroactivePayrollEmployeeStatus, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/controllerflowhandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { RetroactiveFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../Util/Enumerations";
import { IRetroactivePayrollDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Guid } from "../../../Util/StringUtility";
import { Constants } from "../../../Util/Constants";
import { RetroactivePayrollReviewController } from "../../Dialogs/RetroactivePayrollReview/RetroactivePayrollReviewController";
import { SettingsUtility } from "../../../Util/SettingsUtility";

enum ValidationError {
    Unknown = 0,
}

enum RetroAccordionState {
    NotLoaded = 0,
    Loading = 1,
    Loaded = 2,
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private modal;
    isModal = false;

    //Data
    private retroactivePayrollId: number;
    private presetEmployeeId: number = 0;
    private presetTimePeriodHeadId: number = 0;
    private presetTimePeriodId: number = 0;
    private retroactivePayroll: RetroactivePayrollDTO;

    // Properties
    private get allFilteredEmployees(): boolean {
        var selected = true;
        _.forEach(this.retroEmployeesFiltered, retroEmployee => {
            if (!retroEmployee.selected) {
                selected = false;
                return false;
            }
        });
        return selected;
    }
    // Lookups             
    retroEmployees: RetroactivePayrollEmployeeDTO[];
    retroEmployeesDestination: RetroactivePayrollEmployeeDTO[] = [];
    retroAccounts: RetroactivePayrollAccountDTO[];
    retroPayrollAccountTypes: any[];
    timePeriodHeads: TimePeriodHeadGridDTO[];
    timePeriods: TimePeriodDTO[];
    payrollGroups: any[];
    categories: any[];
    accounts: any[];
    statusName: ISmallGenericType[] = [];


    // Data
    payrollGroupsFiltered: any;
    retroEmployeesFiltered: RetroactivePayrollEmployeeDTO[];
    paymentDate: Date;
    employeesLoadedForTimePeriodId: number = 0;
    selectedPayrollGroupId: number;
    selectedAccountOrCategoryId: number;    
    ignoreEmploymentStopDate: boolean = false;
    accordionAccountingState: RetroAccordionState = RetroAccordionState.NotLoaded;
    accordionEmployeeState: RetroAccordionState = RetroAccordionState.NotLoaded;
    companyUseAccountHierarchy: boolean = false;

    //Function
    functions: any = [];
    functionTerms: any = [];
    selectedOption: {};
    private edit: ng.IFormController;

    // Filters
    amountFilter: any;

    //@ngInject
    constructor(
        protected $timeout: ng.ITimeoutService,
        protected $uibModal,
        private $q: ng.IQService,
        private payrollService: IPayrollService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        protected notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $scope: ng.IScope,
        private $filter: ng.IFilterService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        //private edit: ng.IFormController,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        
        this.amountFilter = $filter("amount");        
        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.retroactivePayrollId = parameters.id;
        if (!this.retroactivePayrollId)
            this.retroactivePayrollId = 0;
        this.presetEmployeeId = parameters.employeeId;
        this.presetTimePeriodHeadId = parameters.timePeriodHeadId;
        this.presetTimePeriodId = parameters.timePeriodId;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Payroll_Retroactive, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Payroll_Retroactive].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_Retroactive].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.setup(),
            this.loadCompanySettings(),
            
        ]).then(() => {
            this.$q.all([
                this.loadTimePeriodHeads(),
                this.loadCategoriesOrAccounts(),
                this.loadRetroactivePayrollAccountTypes(),
            ]).then(() => {                
            });
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.retroactivePayrollId > 0) {
            return this.payrollService.getRetroactivePayroll(this.retroactivePayrollId).then((result: RetroactivePayrollDTO) => {
                this.retroactivePayroll = result;                

                if (this.retroactivePayroll) {                     
                    if (this.retroactivePayroll.dateFrom)
                        this.retroactivePayroll.dateFrom = new Date(<any>this.retroactivePayroll.dateFrom);
                    if (this.retroactivePayroll.dateTo)
                        this.retroactivePayroll.dateTo = new Date(<any>this.retroactivePayroll.dateTo);
                    this.populateAfterTimePeriodHead(this.retroactivePayroll.timePeriodHeadId)
                }
                this.loadRetroAccounts();
                this.isNew = false;
                this.loadStatusName();
            });
        }
        else {
            this.new();
            this.loadRetroAccounts();
        }
    }

    public closeModal() {
        if (this.isModal) {
            if (this.retroactivePayrollId) {
                this.modal.close(this.retroactivePayrollId);
            } else {
                this.modal.dismiss();
            }
        }
    }

    private setup(): ng.IPromise<any> {
        return this.setupFunctions();
    }

    private setupFunctions(): ng.IPromise<any> {
        this.functions = [];

        var keys: string[] = [
            "core.save",
            "time.payroll.retroactive.saveandcalculate",
            "time.payroll.retroactive.calculate",
            "time.payroll.retroactive.createtransactions",
            "time.payroll.retroactive.deletetransactions",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.functionTerms = terms;
            this.functions.push(this.getFunction(RetroactiveFunctions.Save));
            this.functions.push(this.getFunction(RetroactiveFunctions.SaveAndCalculate));
            if (this.retroactivePayrollId > 0) {
                this.functions.push(this.getFunction(RetroactiveFunctions.Calculate));
                if (this.showCreateTransactionsFunction()) {
                    this.functions.push(this.getFunction(RetroactiveFunctions.CreateTransactions));
                }
                if (this.showDeleteTransactionsFunction()) {
                    this.functions.push(this.getFunction(RetroactiveFunctions.DeleteTransactions));
                }
            }
            this.setSelectedOption();
        });
    }

    private new() {
        this.isNew = true;
        this.retroactivePayrollId = 0;
        if (!this.retroactivePayroll)
            this.retroactivePayroll = <RetroactivePayrollDTO>{}; //we just fake it
        if (this.presetTimePeriodHeadId > 0 && this.presetTimePeriodId > 0) {
            this.retroactivePayroll.timePeriodHeadId = this.presetTimePeriodHeadId;
            this.loadTimePeriods(this.presetTimePeriodHeadId, this.presetTimePeriodId);
        }
    }

    //LOOKUPS

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.companyUseAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadTimePeriodHeads(): ng.IPromise<any> {
        this.clearTimePeriodHeads();

        return this.payrollService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.Payroll, false, false).then((result) => {
            this.timePeriodHeads = result;

            // Add empty
            var empty = new TimePeriodHeadGridDTO();
            empty.timePeriodHeadId = 0;
            empty.name = '';
            this.timePeriodHeads.splice(0, 0, empty);
        });
    }

    private loadTimePeriods(timePeriodHeadId: number, selectedTimePeriodId: number = 0) {
        
        return this.payrollService.getTimePeriods(timePeriodHeadId).then((result: TimePeriodDTO[]) => {
            this.timePeriods = _.filter(result, function (o: TimePeriodDTO) {
                return o.paymentDate
            });

            this.timePeriods = _.orderBy(this.timePeriods, ['paymentDate'], ['desc']);

            // Add empty
            var emptyTimePeriod = new TimePeriodDTO();
            emptyTimePeriod.timePeriodId = 0;
            emptyTimePeriod.name = '';
            emptyTimePeriod.paymentDateString = '';
            this.timePeriods.splice(0, 0, emptyTimePeriod);

            this.retroactivePayroll.timePeriodId = selectedTimePeriodId;
            if (selectedTimePeriodId > 0)
                this.timePeriodChanged(selectedTimePeriodId);
        });
    }

    private loadStatusName(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.RetroactivePayrollEmployeeStatus, false, true, true).then(terms => {
            this.statusName = terms;
        });
    }

    private loadCategoriesOrAccounts(): ng.IPromise<any> {
        if (this.companyUseAccountHierarchy) {
            return this.loadAccounts();
        } else {
            return this.loadCategories();
        }
    }
    private loadCategories(): ng.IPromise<any> {
        this.clearCategories();

        return this.coreService.getCategories(SoeCategoryType.Employee, false, false, false, false).then((result) => {
            this.categories = result;

            // Add empty
            var empty: any = {};
            empty.categoryId = 0;
            empty.name = '';
            this.categories.splice(0, 0, empty);

            this.selectedAccountOrCategoryId = 0;
        });
    }

    private loadAccounts(): ng.IPromise<any>  {      
        this.clearAccounts();

        return this.coreService.getAccountsFromHierarchyByUserSetting(null, null, false, false, true).then(result => {

            this.accounts = result;
            // Add empty
            var empty: any = {};
            empty.accountId = 0;
            empty.name = '';
            this.accounts.splice(0, 0, empty);

            this.selectedAccountOrCategoryId = 0;
        });
    }

    private loadRetroactivePayrollAccountTypes(): ng.IPromise<any> {
        this.retroPayrollAccountTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.RetroactivePayrollAccountType, false, true).then((result) => {
            this.retroPayrollAccountTypes = result;
        });
    }

    private loadPayrollGroups() {
        this.clearPayrollGroups();

        if (!this.payrollGroups) {
            this.payrollService.getPayrollGroups().then((result) => {
                this.payrollGroups = result;

                // Add empty
                var empty: any = {};
                empty.payrollGroupId = 0;
                empty.name = '';
                this.payrollGroups.splice(0, 0, empty);

                this.filterPayrollGroups();
            });
        }
        else {
            this.filterPayrollGroups();
        }
    }

    private loadRetroEmployees(timePeriodId: number, force: boolean = false) {
        if (timePeriodId && timePeriodId == this.employeesLoadedForTimePeriodId && !force)
            return;

        this.clearEmployees();
        if (!timePeriodId || timePeriodId === 0)
            return;

        this.accordionEmployeeState = RetroAccordionState.Loading;

        var presetEmployeeIds: number[] = [];
        if (this.presetEmployeeId)
            presetEmployeeIds.push(this.presetEmployeeId);

        return this.payrollService.getRetroactivePayrollEmployees(this.retroactivePayrollId, timePeriodId, this.ignoreEmploymentStopDate, presetEmployeeIds).then((x) => {

            this.retroEmployees = x;
            _.forEach(this.retroEmployees, retroEmployee => {
                retroEmployee.guid = Guid.newGuid();
                retroEmployee.selected = false;
                retroEmployee.moved = false;
            });

            this.employeesLoadedForTimePeriodId = timePeriodId;
            this.filterEmployees(this.selectedPayrollGroupId, this.selectedAccountOrCategoryId);

            var hasSelectedEmployees: boolean = false;
            _.forEach(this.retroEmployees, retroEmployee => {
                if (retroEmployee.retroactivePayrollEmployeeId > 0 || (this.presetEmployeeId === retroEmployee.employeeId)) {
                    retroEmployee.selected = true;
                    hasSelectedEmployees = true;
                }
            });
            if (hasSelectedEmployees)
                this.addSelectedEmployees();

            this.accordionEmployeeState = RetroAccordionState.Loaded;
            this.setupFunctions();
        });
    }

    private reloadDestinationEmployees() {
        if (this.retroactivePayroll && this.retroEmployeesDestination && this.retroEmployeesDestination.length > 0) {
            this.accordionEmployeeState = RetroAccordionState.Loading;

            var presetEmployeeIds: number[] = [];
            _.forEach(this.retroEmployeesDestination, retroEmployee => {
                presetEmployeeIds.push(retroEmployee.employeeId);
            });
            return this.payrollService.getRetroactivePayrollEmployees(this.retroactivePayrollId, this.retroactivePayroll.timePeriodId, this.ignoreEmploymentStopDate, presetEmployeeIds).then((x) => {
                var reloadedDestinationEmployees = x;
                _.forEach(reloadedDestinationEmployees, retroEmployee => {                    
                    var sourceRetroEmployee = (_.filter(this.retroEmployees, { employeeId: retroEmployee.employeeId }))[0];
                    var deletedSourceRetroEmployee = this.retroEmployees.splice(this.retroEmployees.indexOf(sourceRetroEmployee), 1, retroEmployee);
                    var destinationRetroEmployee = (_.filter(this.retroEmployeesDestination, { employeeId: retroEmployee.employeeId }))[0];
                    var deletedDestinationRetroEmployee = this.retroEmployeesDestination.splice(this.retroEmployeesDestination.indexOf(destinationRetroEmployee), 1, retroEmployee);
                    this.accordionEmployeeState = RetroAccordionState.Loaded;
                });
                this.reloadStatus();
                this.setupFunctions();
            });
        }
    }

    private reloadStatus() {
        return this.payrollService.getRetroactivePayroll(this.retroactivePayrollId).then((result: RetroactivePayrollDTO) => {
            if (result) {
                this.retroactivePayroll.status = result.status;
                this.retroactivePayroll.statusName = result.statusName;
            }
        });
    }

    private loadRetroAccounts() {
        this.clearRetroAccounts();
        this.accordionAccountingState = RetroAccordionState.Loading;
        return this.payrollService.getRetroactivePayrollAccounts(this.retroactivePayrollId).then((x) => {
            this.retroAccounts = x;
            _.forEach(this.retroAccounts, retroAccount => {
                retroAccount.guid = Guid.newGuid();
                if (retroAccount.accountId)
                    retroAccount.selectedAccount = (_.filter(retroAccount.accountDim.accounts, { accountId: retroAccount.accountId }))[0];
            });
            this.accordionAccountingState = RetroAccordionState.Loaded;
        });
    }

    private getFilteredEmployeesByAccount(accountId: number) {
       
        return this.payrollService.filterRetroactivePayrollEmployeesOnAccount(this.retroEmployeesFiltered, accountId).then((x) => {
            this.retroEmployeesFiltered = x;
            _.forEach(this.retroEmployeesFiltered, retroEmployee => {
                retroEmployee.guid = Guid.newGuid();
                retroEmployee.selected = false;
                retroEmployee.moved = false;    
            });
        });
    }

    //ACTIONS

    private save(newAfterSave: boolean, closeAfterSave: boolean, calculateAfterSave: boolean) {
        if (!this.retroactivePayroll)
            return;
        this.progress.startSaveProgress((completion) => {
            this.retroactivePayroll.retroactivePayrollEmployees = this.retroEmployeesDestination;
            _.forEach(this.retroactivePayroll.retroactivePayrollEmployees, retroEmployee => {
                retroEmployee.retroactivePayrollId = this.retroactivePayroll.retroactivePayrollId;
            });
            this.retroactivePayroll.retroactivePayrollAccounts = this.retroAccounts;
            _.forEach(this.retroactivePayroll.retroactivePayrollAccounts, retroAccount => {
                retroAccount.retroactivePayrollId = this.retroactivePayroll.retroactivePayrollId;
                if (retroAccount.selectedAccount)
                    retroAccount.accountId = retroAccount.selectedAccount.accountId;
            });

            this.payrollService.saveRetroactivePayroll(this.retroactivePayroll).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.retroactivePayrollId = result.integerValue;
                        this.employeesLoadedForTimePeriodId = 0;
                        if (calculateAfterSave) {
                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.retroactivePayroll, true);
                            this.retroactivePayroll.retroactivePayrollId = this.retroactivePayrollId;
                            this.calculate(false);
                        }
                        else {
                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.retroactivePayroll);
                        }
                    }
                    else {
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.retroactivePayroll);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                if (!calculateAfterSave) {
                    this.onLoadData();
                }
            });
    }

    private delete() {
        if (!this.validateDelete())
            return;

        this.progress.startWorkProgress((completion) => {
            this.payrollService.deleteRetroactivePayroll(this.retroactivePayrollId).then((result) => {
                if (result.success) {
                    completion.completed(this.retroactivePayroll);
                    if (this.modal)
                        this.closeModal()
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    private calculate(includeAlreadyCalculated: boolean) {
        if (!this.validateCalculate())
            return;

        this.progress.startWorkProgress((completion) => {
            this.payrollService.calculateRetroactivePayroll(this.retroactivePayroll, includeAlreadyCalculated).then((result) => {
                if (result.success) {
                    this.reloadDestinationEmployees();
                    completion.completed(this.retroactivePayroll);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private deleteOutcomes() {
        if (!this.validateDeleteOutcomes())
            return;

        this.progress.startWorkProgress((completion) => {
            this.payrollService.deleteRetroactivePayrollOutcomes(this.retroactivePayroll).then((result) => {
                if (result.success) {
                    this.translationService.translate("time.payroll.retroactive.deleteoutcomesdone").then((message) => {
                        this.reloadDestinationEmployees();
                        completion.completed(this.retroactivePayroll, false, message);
                    });
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }


    private createTransactions() {
        if (!this.validateCreateTransactions())
            return;

        this.progress.startWorkProgress((completion) => {
            this.payrollService.createRetroactivePayrollTransactions(this.retroactivePayroll).then((result) => {
                if (result.success) {
                    this.reloadDestinationEmployees();
                    completion.completed(this.retroactivePayroll);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private deleteTransactions() {
        if (!this.validateDeleteTransactions())
            return;

        this.progress.startWorkProgress((completion) => {
            this.payrollService.deleteRetroactivePayrollTransactions(this.retroactivePayroll).then((result) => {
                if (result.success) {
                    this.reloadDestinationEmployees();
                    completion.completed(this.retroactivePayroll);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private review() {
        if (!this.validateReview())
            return;

        this.openReview();
    }

    //DIALOGS

    protected initDelete() {
        if (!this.validateDelete())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.deletequestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.deletequestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.delete();
                }
            });
        });
    }

    protected initCalculate() {
        if (!this.validateCalculate())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.calculatequestion",
            "time.payroll.retroactive.includecalculatedemployees"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.calculatequestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, terms["time.payroll.retroactive.includecalculatedemployees"]);            
            modal.result.then(val => {                              
                if (val) {                    
                    this.calculate(val.isChecked);
                }
            });
        });
    }

    protected initDeleteOutcomes() {
        if (!this.validateDeleteOutcomes())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.deleteoutcomesquestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.deleteoutcomesquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.deleteOutcomes();
                }
            });
        });
    }

    protected initCreateTransactons() {
        if (!this.validateCreateTransactions())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.createtransactionsquestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.createtransactionsquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.createTransactions();
                }
            });
        });
    }

    protected initDeleteTransactions() {
        if (!this.validateDeleteTransactions())
            return;

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.retroactive.deletetransactionsquestion",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.deletetransactionsquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.deleteTransactions();
                }
            });
        });
    }

    protected openReview() {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/RetroactivePayrollReview/retroactivePayrollReview.html"),
            controller: RetroactivePayrollReviewController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                isModal: true,
                retroactivePayrollId: this.retroactivePayrollId,
            }
        });

        modal.result.then(result => {
            if (result.hasChanges)
                this.reloadDestinationEmployees();
        });
    }

    // EVENTS

    private ignoreEmploymentStopDateChanged(value) {
        this.$timeout(() => {
            if (this.retroactivePayroll && this.retroactivePayroll.timePeriodId && this.retroactivePayroll.timePeriodId > 0 && this.accordionEmployeeState != RetroAccordionState.Loading) {
                this.loadRetroEmployees(this.retroactivePayroll.timePeriodId, true);
            }
        });
    }

    private timePeriodHeadChanged(oldValue, newValue) {
        
        if (oldValue != newValue && this.retroEmployeesDestination && this.retroEmployeesDestination.length > 0) {
            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "time.payroll.retroactive.changetimepperiodheadquestion",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.retroactive.changetimepperiodheadquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.populateAfterTimePeriodHead(newValue);
                }, (reason) => {
                    // User cancelled, revoke to previous timePeriodHead
                    this.retroactivePayroll.timePeriodHeadId = oldValue;
                });
            });
        }
        else {
            this.populateAfterTimePeriodHead(newValue);
        }
    }

    private populateAfterTimePeriodHead(timePeriodHeadId) {
        
        if (!timePeriodHeadId || timePeriodHeadId == 0)
            return;

        //clear        
        this.clearPayrollGroups();
        this.clearEmployees();

        //load
        this.loadTimePeriods(timePeriodHeadId, this.retroactivePayroll && this.retroactivePayroll.timePeriodHeadId == timePeriodHeadId ? this.retroactivePayroll.timePeriodId : null);
        this.loadPayrollGroups();
    }

    private timePeriodChanged(timePeriodId) {
        this.loadRetroEmployees(timePeriodId);
    }

    private payrollGroupFilterChanged(newValue) {
        this.filterEmployees(newValue, this.selectedAccountOrCategoryId);
    }

    private accountOrCategoryFilterChanged(newValue) {
        this.filterEmployees(this.selectedPayrollGroupId, newValue);
    }

    private retroAccountChanged(retroAccount: RetroactivePayrollAccountDTO) {
        if (retroAccount && retroAccount.selectedAccount) {
            retroAccount.accountId = retroAccount.selectedAccount.accountId;
        }
    }

    // HELP-METHODS

    private getSelectedOptionLabelKey(option: RetroactiveFunctions): string {
        var labelKey: string = "";
        switch (option) {
            case RetroactiveFunctions.Save:
                labelKey = "core.save";
                break;
            case RetroactiveFunctions.SaveAndCalculate:
                labelKey = "time.payroll.retroactive.saveandcalculate";
                break;
            case RetroactiveFunctions.Calculate:
                labelKey = "time.payroll.retroactive.calculate";
                break;
            case RetroactiveFunctions.CreateTransactions:
                labelKey = "time.payroll.retroactive.createtransactions";
                break;
            case RetroactiveFunctions.DeleteTransactions:
                labelKey = "time.payroll.retroactive.deletetransactions";
                break;
        }
        return labelKey;
    }

    private setSelectedOption() {
        if (this.retroactivePayrollId > 0)
            this.selectedOption = this.getFunction(RetroactiveFunctions.Calculate);
        else
            this.selectedOption = this.getFunction(RetroactiveFunctions.Save);
    }

    private getFunction(option: RetroactiveFunctions) {
        return {
            id: option,
            name: this.functionTerms[this.getSelectedOptionLabelKey(option)],
        }
    }

    private executeFunction(option) {
        switch (option.id) {
            case RetroactiveFunctions.Save:
                this.save(false, false, false);
                break;
            case RetroactiveFunctions.SaveAndCalculate:
                this.save(false, false, true);
                break;
            case RetroactiveFunctions.Calculate:
                this.initCalculate();
                break;
            case RetroactiveFunctions.CreateTransactions:
                this.initCreateTransactons();
                break;
            case RetroactiveFunctions.DeleteTransactions:
                this.initDeleteTransactions();
                break;
        }
    }

    private clearTimePeriodHeads() {
        this.timePeriodHeads = [];
    }
    
    private clearPayrollGroups() {
        this.payrollGroupsFiltered = [];
    }

    private clearCategories() {
        this.categories = [];
    }

    private clearAccounts() {
        this.accounts = [];
    }

    private clearEmployees() {
        this.deSelectAllEmployees();
        this.retroEmployees = [];
        this.retroEmployeesFiltered = [];
        this.retroEmployeesDestination = [];
        this.employeesLoadedForTimePeriodId = null;
        this.accordionEmployeeState = RetroAccordionState.NotLoaded;
    }

    private clearRetroAccounts() {
        this.retroAccounts = [];
    }

    private filterPayrollGroups() {
        if (this.retroactivePayroll && this.retroactivePayroll.timePeriodHeadId) {
            _.forEach(this.payrollGroups, (payrollGroup: any) => {
                if (payrollGroup.payrollGroupId === 0 || payrollGroup.timePeriodHeadId == this.retroactivePayroll.timePeriodHeadId)
                    this.payrollGroupsFiltered.push(payrollGroup);
            });

            this.selectedPayrollGroupId = 0;
        }
    }

    private filterEmployees(payrollGroupId, accountOrCategoryId) {
        this.retroEmployeesFiltered = this.retroEmployees;
        this.retroEmployeesFiltered = (_.filter(this.retroEmployeesFiltered, { moved: false }));
        if (payrollGroupId > 0)
            this.retroEmployeesFiltered = this.getFilteredPayrollGroups(this.retroEmployeesFiltered, payrollGroupId);

        if (this.companyUseAccountHierarchy) {
            if (accountOrCategoryId > 0)
                this.getFilteredEmployeesByAccount(accountOrCategoryId);
        } else {
            if (accountOrCategoryId > 0)
                this.retroEmployeesFiltered = this.getFilteredCategories(this.retroEmployeesFiltered, accountOrCategoryId);
        }        
    }

    private getFilteredPayrollGroups(filteredEmployees: RetroactivePayrollEmployeeDTO[], payrollGroupId: number): RetroactivePayrollEmployeeDTO[] {
        return (_.filter(filteredEmployees, { payrollGroupId: payrollGroupId }))
    }

    private getFilteredCategories(filteredEmployees: RetroactivePayrollEmployeeDTO[], categoryId: number): RetroactivePayrollEmployeeDTO[] {
        var filteredEmployees2: RetroactivePayrollEmployeeDTO[] = [];
        _.forEach(filteredEmployees, (filteredEmployee: RetroactivePayrollEmployeeDTO) => {
            if (filteredEmployee.categoryIds.indexOf(categoryId) >= 0)
                filteredEmployees2.push(filteredEmployee);
        });
        return filteredEmployees2;
    }

    private hasPaymentDate(): boolean {
        if (this.retroactivePayroll && this.retroactivePayroll.timePeriodId && this.retroactivePayroll.timePeriodId > 0)
            return true;
        return false;
    }

    private getPaymentDate(): string {
        var paymentDateStr: string;
        if (this.hasPaymentDate()) {
            var timePeriod = (_.filter(this.timePeriods, { timePeriodId: this.retroactivePayroll.timePeriodId }))[0];
            if (timePeriod)
                paymentDateStr = timePeriod.paymentDateString;
        }
        return paymentDateStr;
    }

    private getPayrollGroupName(employee: RetroactivePayrollEmployeeDTO): string {
        var name: string;
        var payrollGroup = (_.filter(this.payrollGroups, { payrollGroupId: employee.payrollGroupId }))[0];
        if (payrollGroup)
            name = payrollGroup.name;
        return name;
    }

    private getCategoryNames(employee: RetroactivePayrollEmployeeDTO): string {
        var name: string = '';
        _.forEach(employee.categoryIds, categoryId => {
            var category = (_.filter(this.categories, { categoryId: categoryId }))[0];
            if (category) {
                if (name != '')
                    name += ',';
                name += category.name;
            }
        });
        return name;
    }

    private getSelectedEmployees(): RetroactivePayrollEmployeeDTO[] {
        return _.filter(this.retroEmployeesFiltered, i => i.selected);
    }

    private getDestinationEmployeesWithOutcome() {
        return _.filter(this.retroEmployeesDestination, r => r.retroactivePayrollOutcomes && r.retroactivePayrollOutcomes.length > 0);
    }

    private getDestinationEmployeeIdsWithOutcome() {
        var employeeIds: number[] = [];
        _.forEach(this.getDestinationEmployeesWithOutcome(), retroEmployee => {
            employeeIds.push(retroEmployee.employeeId);
        });
        return employeeIds;
    }

    private doShowRetroAccounting(): boolean {
        return this.accordionAccountingState == RetroAccordionState.Loaded;
    }

    private doShowRetroAccountingLoading(): boolean {
        return (
            this.accordionAccountingState == RetroAccordionState.Loading ||
            this.accordionAccountingState == RetroAccordionState.NotLoaded
        );
    }

    private doShowRetroEmployees(): boolean {
        return this.accordionEmployeeState == RetroAccordionState.Loaded;
    }

    private doShowRetroEmployeesLoading(): boolean {
        return this.accordionEmployeeState == RetroAccordionState.Loading;
    }

    private doShowRetroEmployeesNotLoaded(): boolean {
        return this.accordionEmployeeState == RetroAccordionState.NotLoaded;
    }

    private hasDestinationEmployeesWithOutcome(): boolean {
        return this.getDestinationEmployeesWithOutcome().length > 0;
    }

    private getDestinationEmployeesWithTransactions() {
        return _.filter(this.retroEmployeesDestination, r => r.hasTransactions);
    }
    
    private getDestinationEmployeesThatCanCreateTransactions() {
        return _.filter(this.retroEmployeesDestination, r => r.hasOutcomes && !r.hasTransactions);
    }

    private showDeleteTransactionsFunction(): boolean {
        return this.getDestinationEmployeesWithTransactions().length > 0;
    }

    private showCreateTransactionsFunction(): boolean {
        return this.getDestinationEmployeesThatCanCreateTransactions().length > 0;
    }

    private getDestinationEmployeesWithErrors() {
        return _.filter(this.retroEmployeesDestination, r => r.status >= TermGroup_SoeRetroactivePayrollEmployeeStatus.Error);
    }

    private getRetroEmployeesTotalAmount() {
        var totalAmount: number = 0;
        _.each(this.retroEmployeesDestination, retroEmployee => {
            _.each(retroEmployee.retroactivePayrollOutcomes, retroOutcome => {
                if (totalAmount === 0)
                    totalAmount = retroOutcome.amount;
                else
                    totalAmount = totalAmount + retroOutcome.amount;
            });
        });
        return this.amountFilter(totalAmount, 4);
    }

    private isGuiReadOnly(importantField: boolean = false): boolean {
        if (!this.retroactivePayroll && this.retroactivePayrollId > 0) {
            return true;
        }
        if (importantField && this.hasDestinationEmployeesWithOutcome()) {
            return true;
        }
        return false;
    }

    private isAddSelectedEmployeeDisabled(): boolean {
        return this.getSelectedEmployees().length === 0 || this.isGuiReadOnly(true);
    }

    private isRemoveEmployeeFromDestinationValid(): boolean {
        return this.isGuiReadOnly(true);
    }

    private clearSourceEmployes() {
        _.forEach(this.retroEmployeesFiltered, employee => {
            employee.selected = false;
        });
    }

    private selectAllEmployees() {
        var selected: boolean = this.allFilteredEmployees;
        _.forEach(this.retroEmployeesFiltered, employee => {
            employee.selected = !selected;
        });
    }

    private deSelectAllEmployees() {
        _.forEach(this.retroEmployeesFiltered, employee => {
            employee.selected = false;
        });
    }

    private addSelectedEmployees() {
        this.addEmployees(this.getSelectedEmployees());
    }

    private addEmployees(employees: RetroactivePayrollEmployeeDTO[]) {
        if (!employees || employees.length === 0)
            return;

        _.forEach(employees, employee => {
            // Don't add the same recipient twice
            if (!_.find(this.retroEmployeesDestination, r => r.employeeId === employee.employeeId)) {
                var cloneEmployee: RetroactivePayrollEmployeeDTO = _.cloneDeep(employee);
                if (cloneEmployee) {
                    this.retroEmployeesDestination.push(cloneEmployee);
                }
                this.setRetroEmployeeToMoved(employee.employeeId, true);
            }
        });

        this.filterEmployees(this.selectedPayrollGroupId, this.selectedAccountOrCategoryId);
    }

    private removeAllEmployeesFromDestination() {
        this.retroEmployeesDestination = [];
        _.forEach(this.retroEmployees, retroEmployee => {
            this.setRetroEmployeeToMoved(retroEmployee.employeeId, false);
        });
        this.filterEmployees(this.selectedPayrollGroupId, this.selectedAccountOrCategoryId);
    }

    private removeEmployeeFromDestination(retroEmployee: RetroactivePayrollEmployeeDTO) {
        var index: number = this.retroEmployeesDestination.indexOf(retroEmployee);
        this.retroEmployeesDestination.splice(index, 1);
        this.setRetroEmployeeToMoved(retroEmployee.employeeId, false);
        this.filterEmployees(this.selectedPayrollGroupId, this.selectedAccountOrCategoryId);
    }

    private setRetroEmployeeToMoved(employeeId: number, moved: boolean) {
        var retroEmployee = (_.filter(this.retroEmployees, { employeeId: employeeId }))[0];
        if (retroEmployee)
            retroEmployee.moved = moved;
    }

    private getStatusName(id: number) {
        return this.statusName.find(f => f.id == id)?.name ?? '';
    }

    // VALIDATION

    public isDisabled(): boolean {
        return this.edit.$invalid;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.retroactivePayroll) {
                if (!this.retroactivePayroll.name) {
                    mandatoryFieldKeys.push("time.payroll.retroactive.name");
                }
                if (!this.retroactivePayroll.timePeriodHeadId) {
                    mandatoryFieldKeys.push("time.time.timeperiod.timeperiodhead");
                }
                if (!this.retroactivePayroll.timePeriodId) {
                    mandatoryFieldKeys.push("time.time.timeperiod.timeperiod");
                }
                if (!this.retroactivePayroll.dateFrom) {
                    mandatoryFieldKeys.push("time.payroll.retroactive.dates");
                }
                if (!this.modifyPermission) {
                    validationErrorKeys.push("time.payroll.retroactive.missingpermission");
                }
                if (this.accordionAccountingState !== RetroAccordionState.Loaded) {
                    validationErrorKeys.push("time.payroll.retroactive.accountingnotloaded");
                }
                if (this.accordionEmployeeState !== RetroAccordionState.Loaded) {
                    validationErrorKeys.push("time.payroll.retroactive.employeesnotloaded");
                }
            }
        });
    }

    private validateEmployeeFilter(): boolean {
        return (
            this.retroactivePayroll &&
            this.retroactivePayroll.timePeriodHeadId &&
            this.retroactivePayroll.timePeriodHeadId > 0 &&
            this.retroactivePayroll.timePeriodId &&
            this.retroactivePayroll.timePeriodId > 0
        );
    }

    private validateFunctions(): boolean {
        return (
            !this.isDisabled() &&
            this.modifyPermission !== null &&
            this.retroactivePayroll !== null &&
            this.retroEmployeesDestination !== null &&
            this.accordionAccountingState == RetroAccordionState.Loaded &&
            this.accordionEmployeeState == RetroAccordionState.Loaded
        );
    }

    private validateReview(): boolean {
        return (
            this.retroactivePayroll && this.retroactivePayroll.retroactivePayrollId > 0 && this.retroEmployeesDestination && this.retroEmployeesDestination.length > 0
        );
    }

    private validateDelete(): boolean {
        return (
            this.modifyPermission &&
            this.retroactivePayrollId > 0
        );
    }

    private validateCalculate(): boolean {
        return (
            !this.isDisabled() &&
            this.retroactivePayrollId > 0
        );
    }

    private validateDeleteOutcomes(): boolean {
        return (
            this.retroactivePayroll &&
            this.retroEmployees &&
            (this.getDestinationEmployeesWithOutcome().length > 0 || this.getDestinationEmployeesWithErrors().length > 0) &&
            this.getDestinationEmployeesWithTransactions().length === 0
        );
    }

    private validateCreateTransactions(): boolean {
        return (
            !this.isDisabled() &&
            this.retroactivePayrollId > 0
        );
    }

    private validateDeleteTransactions(): boolean {
        return (
            !this.isDisabled() &&
            this.retroactivePayrollId > 0
        );
    }
}