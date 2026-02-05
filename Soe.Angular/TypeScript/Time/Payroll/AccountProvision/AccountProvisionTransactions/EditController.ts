import { EditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { ISoeGridOptions, SoeGridOptions } from "../../../../Util/SoeGridOptions";
import { ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { IPayrollService } from "../../PayrollService";
import { Feature, TermGroup_TimePeriodType, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase {

    // Data
    timePeriodHeadId: number = 0;
    timePeriodId: number = 0;
    transactions: any = [];
    attestStates: any = [];
    attestStateOptions: any = [{}];
    attestStateInitial: any;

    // Lookups
    timePeriodHeads: ISmallGenericType[];
    timePeriods: any;

    protected gridOptions: ISoeGridOptions;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    constructor(
        private id: number,
        private $timeout: ng.ITimeoutService,
        private $window: ng.IWindowService,
        $uibModal,
        coreService: ICoreService,
        private payrollService: IPayrollService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Payroll.AccountProvision.AccountProvisionTransactions.Edit", Feature.Time_Payroll_Provision_AccountProvisionTransaction, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
        this.initGrid();
    }

    // SETUP

    protected setupLookups() {
        this.setupGrid(); //must be called after permissions in base class is done            
        this.lookups = 3;
        this.startLoad();
        this.loadTimePeriodHeads();
        this.loadUserValidAttestStates();
        this.loadAttestStateInitial();
    }

    private setupToolBar() {
        this.setupDefaultToolBar();
    }

    private initGrid() {
        this.gridOptions = new SoeGridOptions("Time.Payroll.AccountProvision.AccountProvisionTransactions", this.$timeout, this.uiGridConstants);
        this.gridOptions.enableGridMenu = false;
        this.gridOptions.showGridFooter = false;
    }

    private setupGrid() {
        var keys: string[] = [
            "time.employee.employee.employeenr",
            "common.firstname",
            "common.lastname",
            "time.payroll.accountprovision.accountnr",
            "time.payroll.accountprovision.accountname",
            "time.payroll.accountprovision.accountdesc",
            "time.payroll.accountprovision.worktime",
            "time.payroll.accountprovision.comment",
            "common.amount",
            "time.atteststate.state",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridOptions.addColumnIsModified();
            this.gridOptions.addColumnText("employeeNr", terms["time.employee.employee.employeenr"], "6%");
            this.gridOptions.addColumnText("employeeFirstName", terms["common.firstname"], "9%");
            this.gridOptions.addColumnText("employeeLastName", terms["common.lastname"], "9%");
            this.gridOptions.addColumnText("accountNr", terms["time.payroll.accountprovision.accountnr"], "6%");
            this.gridOptions.addColumnText("accountName", terms["time.payroll.accountprovision.accountname"], "10%");
            this.gridOptions.addColumnText("accountDescription", terms["time.payroll.accountprovision.accountdesc"], "10%");
            this.gridOptions.addColumnText("workTime", terms["time.payroll.accountprovision.worktime"], "6%");

            var colDefComment = this.gridOptions.addColumnText("comment", terms["time.payroll.accountprovision.comment"], null);
            colDefComment.cellEditableCondition = (scope) => { return this.isRowEditable(scope); }
            colDefComment.enableCellEdit = true;

            var colDefAmount = this.gridOptions.addColumnText("amount", terms["common.amount"], "6%");
            colDefAmount.cellEditableCondition = (scope) => { return this.isRowEditable(scope); }
            colDefAmount.enableCellEdit = true;

            this.gridOptions.addColumnShape("attestStateColor", null, null, "", Constants.SHAPE_CIRCLE, "attestStateName");
            this.gridOptions.addColumnText("attestStateName", terms["time.atteststate.state"], "8%");

            _.forEach(this.gridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableFiltering = false;
                colDef.enableSorting = false;
                colDef.enableColumnMenu = false;
            });
        });
    }

    // LOOKUPS   
    private loadTimePeriodHeads() {
        this.payrollService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.Payroll, false).then((x) => {
            this.timePeriodHeads = x;
            this.lookupLoaded();
            // Selecting a period will set the dirty flag, reset it
            this.isDirty = false;
        });
    }

    private loadUserValidAttestStates() {
        this.payrollService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, null, null, true, null).then((x) => {
            this.attestStates = x;
            this.setupAttestStateCombo();
            this.lookupLoaded();
        });
    }

    private loadAttestStateInitial() {
        this.coreService.getAttestStateInitial(TermGroup_AttestEntity.PayrollTime).then((x) => {
            this.attestStateInitial = x;
            this.lookupLoaded();
        });
    }

    // EVENTS

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups <= 0) {
            this.setupToolBar();
        }
    }

    private timePeriodHeadSelected(id) {
        this.timePeriodHeadId = id;
        this.loadTimePeriods();
    }

    private timePeriodSelected(id) {
        this.timePeriodId = id;
        this.load();
    }

    // ACTIONS

    private loadTimePeriods() {
        this.payrollService.getTimePeriodsDict(this.timePeriodHeadId, false).then((x) => {
            this.timePeriods = x;
            // Selecting a period will set the dirty flag, reset it
            this.isDirty = false;
        });
    }

    private load() {
        this.lookups = 1;
        this.startLoad();
        if (this.timePeriodId > 0) {
            this.payrollService.getAccountProvisionTransactions(this.timePeriodId).then((x) => {
                this.transactions = x;
                this.gridOptions.setData(this.transactions);
                this.lookupLoaded();
                // Selecting a period will set the dirty flag, reset it
                this.isDirty = false;
            }, error => {
                this.failedWork(error.message);
            });
        }
        else {
            this.lookupLoaded();
        }
    }

    private save() {
        this.startLoad();
        var modifiedTransactions: any[] = [];
        _.forEach(this.transactions, (x: any) => {
            if (x.isModified && x.isModified === true)
                modifiedTransactions.push({ timePayrollTransactionId: x.timePayrollTransactionId, amount: x.amount, quantity: x.quantity, comment: x.comment })
        });

        this.payrollService.saveAccountProvisionTransactions(modifiedTransactions).then((result) => {
            if (result.success) {
                this.completedSave(null); //TODO:new completedsave is needed      
                this.load();
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    private applyAttest(option: any) {
        this.startLoad();
        var selectedRows = this.gridOptions.getSelectedRows();
        _.forEach(selectedRows, (x: any) => {
            x.attestStateId = option.id;
        });

        this.payrollService.changeAttestStateAccountProvisionTransactions(selectedRows).then((result) => {
            if (result.success) {
                this.completedSave(null); //TODO:new completedsave is needed      
                this.load();
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    // HELP-METHODS    

    private setupAttestStateCombo() {
        _.forEach(this.attestStates, (x: any) => {
            this.attestStateOptions.push({ id: x.attestStateId, name: x.name });
        });
    }

    // VALIDATION

    private isAttestDisabled(): boolean {
        return this.gridOptions.getSelectedCount() === 0;
    }

    protected validate() {
    }

    public isRowEditable(scope: any) {
        if (this.attestStateInitial && scope) {
            return this.modifyPermission === true && scope.row.entity.attestStateId === this.attestStateInitial.attestStateId;
        }
        return false;
    }
}
