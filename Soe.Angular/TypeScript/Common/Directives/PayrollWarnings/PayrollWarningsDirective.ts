import { IQService } from 'angular';
import '../../../Common/Dialogs/AddDocumentToAttestFlow/Module';
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from '../../../Core/Services/CoreService';
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, UrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPayrollService } from '../../../Time/Payroll/PayrollService';
import { Feature, SoeEntityState, TermGroup, TermGroup_PayrollControlFunctionType, } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarButtonGroup } from '../../../Util/ToolBarUtility';
import { PayrollWarningsChangesDialogController } from './PayrollWarningsChangesDialog';
import { PayrollCalculationEmployeePeriodDTO } from '../../Models/TimeEmployeeTreeDTO';


export class PayrollWarningsFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('PayrollWarnings', 'PayrollWarnings.html'),
            scope: {
                employeeTimePeriodId: '=',
                timePeriodId: '=',
                employeeId: '=',
                filteredPayrollEmployeePeriods: '=',
                readOnly: '=',
                allWarnings: '=',
                showHistory: '='
            },
            restrict: 'E',
            replace: true,
            controller:  PayrollWarningsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
          
        };
    }
}

class PayrollWarningsController extends GridControllerBase2Ag implements ICompositionGridController {

    // Init parameters

    private readOnly: boolean;
    private warnings: any = [];
    private _allWarnings: any = [];
    private employeeTimePeriodId: number;
    private employeeId: number;
    private filteredPayrollEmployeePeriods: PayrollCalculationEmployeePeriodDTO[] = [];
    private options: any = [];
    private payrollControlFunctionStatus: any = [];
    private showHistory: boolean = false;
    private timePeriodId: number = 0;

    set allWarnings(value: any) {
        this._allWarnings = value;
        this.loadWarnings();
    }
    get allWarnings(): any {
       return this._allWarnings;
    }
 
    // Terms
    private terms: { [index: string]: string; };

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();
    modalInstance: any;

    // Permissions


    //@ngInject
    constructor(
        private $window,
        private $uibModal,
        private $q: IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: UrlHelperService,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private payrollService: IPayrollService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "time.payroll.payrollcalculation.warnings", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onSetUpGrid(() => this.setupGrid())

        this.modalInstance = $uibModal;

        this.onInit({});
    }

    // SETUP

    uploadItem() {
        throw new Error('Method not implemented.');
    }

    $onInit() {

        let queue = [];
        queue.push(this.loadTerms());
        queue.push(this.loadPayrollControlFunctionTypes());
      
        this.$q.all(queue).then(() => {
            this.flowHandler.start([
                { feature: Feature.Time_Employee_Employees_Edit_OtherEmployees_Files_InitSigning, loadModifyPermissions: true, loadReadPermissions: true }
            ]);

        });
        this.$scope.$on(Constants.EVENT_RELOAD_GRID, (e, parameters) => {
            if (this.terms) {
                this.loadOptions();
                this.loadWarnings();
            }
        });
        this.$scope.$on(Constants.EVENT_RELOAD_GROUP_GRID, (e, parameters) => {
            if (this.terms) {
                this.loadOptions();
                this.loadWarningsGroup();
            }
        });
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.progress.setProgressBusy(true);

    }
    private loadOptions() {
        this.options = [];
        this.options.push({ id: 0, name: this.terms["core.save"] });
        this.options.push({ id: 1, name: this.terms["time.payroll.payrollcalculation.warnings.runcontroll"] });
    }

    
    private setGridData() {
        this.gridAg.setData(this.warnings);
    }

    private loadPayrollControlFunctionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollControlFunctionStatus, false, false).then(x => {
            this.payrollControlFunctionStatus = x;
        });
        
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {


    }

    private setupGrid(): void {
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setTooltipDelay = 200;
        this.gridAg.options.setMinRowsToShow(5);

        this.gridAg.addColumnText("typeName", this.terms["common.type"], null, true, { cellClassRules: { "errorColor": (row) => { return this.checkError(row.data) } }, enableHiding: false, editable: false });
        this.gridAg.addColumnSelect("status", this.terms["common.status"], null, { displayField: "statusName", selectOptions: this.payrollControlFunctionStatus, dropdownValueLabel: "name", editable: this.checkIsEditable.bind(this)});
        this.gridAg.addColumnText("comment", this.terms["common.comment"], null, true, { editable: (row) => !this.readOnly, onChanged: () => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid }) });
        this.gridAg.addColumnIcon(null, "   ", null, { icon: "fal fa-info-circle infoColor", suppressFilter: true, showIcon: (row) => row.hasChanges, onClick: this.showLog.bind(this), toolTip: this.terms["common.entitylogviewer.changelog"] });

        let events: GridEvent[] = [];

        this.gridAg.options.subscribe(events);
        
        this.gridAg.finalizeInitGrid("time.payroll.payrollcalculation.warnings", true);

    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.comment",
            "common.type",
            "common.status",
            "common.entitylogviewer.changelog",
            "core.yes",
            "core.no",
            "common.active",
            "manage.attest.state.hidden",
            "core.save",
            "time.payroll.payrollcalculation.warnings.runcontroll"
          
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }


    private loadWarnings(reload: boolean = false) {
        if (reload) {
            if (!this.employeeTimePeriodId || !this.employeeId) {
                return;
            }

            if (this.filteredPayrollEmployeePeriods != undefined) {
                this.loadWarningsGroup(reload);
            } else {
                this.payrollService.getPayrollWarnings(this.employeeId, this.employeeTimePeriodId, this.showHistory).then(x => {
                    this.allWarnings = x;
                    this.populateGrid();
                });
            }
        } else
            this.populateGrid();
    }
    private loadWarningsGroup(reload: boolean = false) {
        if (reload) {

            let employeeIds = _.map(this.filteredPayrollEmployeePeriods, e => e.employeeId);
            let timePeriodId = this.filteredPayrollEmployeePeriods[0].timePeriodId;

            this.payrollService.getPayrollWarningsGroup(employeeIds, timePeriodId, this.showHistory).then(x => {
                this.allWarnings = x;
                this.populateGrid();
            });
        } else
            this.populateGrid();
    }
    private changeChecker() {
        this.showHistory = !this.showHistory;
        this.loadWarnings(true);
    }

    private populateGrid() {
        this.warnings = this.allWarnings;
        this.setGridData();
    }
 
    // HELP-METHODS
    private checkError(row: any) {
        return row.isStoppingPayrollWarning;
    }

    private checkIsEditable(row: any) {
        return !row.isStoppingPayrollWarning && row.state != SoeEntityState.Deleted && row.type != TermGroup_PayrollControlFunctionType.PeriodHasNotBeenCalculated;
    }

    private run(option: any) {
        if (option.id == 0)
            this.save();
        else if(option.id == 1)
            this.runControl();
    }

    private save() {
        const data = this.gridAg.options.getData();
        if (data) this.gridAg.options.clearFocusedCell();
        this.progress.startSaveProgress((completion) => {

            this.payrollService.savePayrollWarnings(data).then((result) => {
                if (result.success) {
                    completion.completed();
                    this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_WARNINGS_SAVEDORCALCULATED, null);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(() => {
                this.loadWarnings(true);
            });
    }

    private runControl() {
        this.progress.startSaveProgress((completion) => {
            this.payrollService.runPayrollControll([this.employeeId], this.timePeriodId).then(result => {
                if (result.success) {
                    completion.completed();
                    this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_WARNINGS_SAVEDORCALCULATED, null);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(() => {
                this.loadWarnings(true);
            });
      
    }

    private showLog(row: any) {
        this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/PayrollWarnings/PayrollWarningsChangesDialog.html"),
            controller: PayrollWarningsChangesDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            keyboard: true,
            size: 'lg',

            scope: this.$scope,
            resolve: {
                changes: () => { return row; },
                terms: () => { return this.terms; }
            }
        });

    }

}