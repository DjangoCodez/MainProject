import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeService } from "../EmployeeService";
import { IPayrollService } from "../../Payroll/PayrollService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { Feature, TermGroup_PayrollReviewStatus, UserSettingType, SettingMainType } from "../../../Util/CommonEnumerations";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize, SoeGridOptionsEvent, IconLibrary } from "../../../Util/Enumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { Constants } from "../../../Util/Constants";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { MassAdjustmentDialogController } from "./Dialogs/MassAdjustment/MassAdjustmentDialogController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { PayrollLevelDTO } from "../../../Common/Models/PayrollLevelDTO";
import { PayrollGroupSmallDTO, PayrollGroupPriceTypeDTO } from "../../../Common/Models/PayrollGroupDTOs";
import { PayrollReviewHeadDTO, PayrollReviewRowDTO } from "../../../Common/Models/PayrollReviewDTOs";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private panelLabel: string;

    // Data    
    private payrollReviewHead: PayrollReviewHeadDTO;
    private payrollReviewHeadId: number = 0;
    private payrollGroups: PayrollGroupSmallDTO[] = [];
    private payrollPriceTypes: PayrollGroupPriceTypeDTO[] = [];
    private allPayrollLevels: PayrollLevelDTO[] = [];
    private payrollLevels: PayrollLevelDTO[] = [];

    // Settings
    private usePayrollLevels: boolean = false;
    private disableShowUpdateInfo: boolean = false;

    // Grid
    private gridHandler: EmbeddedGridController;
    public gridToolbar: IToolbar;

    // Flags
    private loading: boolean = false;
    private selectedCount: number = 0;
    private keepFuture = false;

    // Properties
    private selectedPayrollGroups: any[] = [];
    private selectedPayrollPriceTypes: any[] = [];
    private selectedPayrollLevels: any[] = [];

    private get updateEnabled() {
        return this.payrollReviewHead && this.payrollReviewHead.rows && this.payrollReviewHead.rows.length > 0 && !this.dirtyHandler.isDirty && this.payrollReviewHead.status !== TermGroup_PayrollReviewStatus.Executed;
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $timeout: ng.ITimeoutService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "PayrollReviewRows");
    }

    public onInit(parameters: any) {
        this.payrollReviewHeadId = parameters.id || 0;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Employee_PayrollReview, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_PayrollReview].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_PayrollReview].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        this.gridToolbar = toolbarFactory.createEmpty();
        let group: ToolBarButtonGroup = ToolBarUtility.createGroup();
        group.buttons.push(new ToolBarButton("time.employee.payrollreview.exportexcel", "time.employee.payrollreview.exportexcel", IconLibrary.FontAwesome, "fa-file-excel", () => this.exportExcel(), () => { return !this.updateEnabled || this.selectedCount === 0 }));
        group.buttons.push(new ToolBarButton("time.employee.payrollreview.importexcel", "time.employee.payrollreview.importexcel", IconLibrary.FontAwesome, "fa-file-excel", () => this.importExcel(), () => { return !this.updateEnabled }));
        this.gridToolbar.addButtonGroup(group);

        this.gridToolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.employee.payrollreview.massadjustment", "time.employee.payrollreview.massadjustment", IconLibrary.FontAwesome, "fa-ballot", () => this.openMassAdjustmentDialog(), () => { return this.selectedCount === 0 })));

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.payrollReviewHeadId, recordId => {
            if (recordId !== this.payrollReviewHeadId) {
                this.payrollReviewHeadId = recordId;
                this.load();
            }
        });
    }

    public doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadUserSettings(),
            this.loadPayrollGroups(),
            this.loadPayrollLevels(),
        ]).then(() => {
            this.setupGrid();
        });
    }

    private onLoadData() {
        if (this.payrollReviewHeadId && this.payrollReviewHeadId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.info",
            "core.warning",
            "core.question",
            "common.name",
            "common.new",
            "common.obs",
            "common.status",
            "time.employee.employee.employeenr",
            "time.employee.employmentpricetype.payrollgroupamount",
            "time.employee.payrollgroup.payrollgroup",
            "time.employee.payrollreview.payrollreview",
            "time.employee.payrollreview.payrollreviewlabel",
            "time.employee.payrollreview.newamount",
            "time.employee.payrollreview.removerow",
            "time.employee.payrollreview.updatereminder",
            "time.employee.payrollreview.loademployeeswarning",
            "time.employee.payrollreview.amountwarning",
            "time.employee.payrollreview.updateinformation",
            "time.employee.payrollreview.deletequestion",
            "time.employee.payrollreview.currentamount",
            "time.employee.payrollreview.adjustment",
            "time.employee.payrollreview.exportexcel.progress",
            "time.employee.payrollreview.exportexcel.error",
            "time.employee.payrollreview.importexcel",
            "time.employee.payrollreview.importexcel.error",
            "time.employee.payrollreview.importexcel.error.title",
            "time.employee.payrollreview.importexcel.warning.title",
            "time.payroll.payrollpricetype.payrollpricetype",
            "time.employee.payrolllevel.payrolllevel",
            "time.schedule.planning.templateschedulewarning.donotshowagain",
            "time.employee.payrolllevel.missing",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public setupGrid() {
        this.gridHandler.gridAg.addColumnIsModified("isModified", "", 20);
        this.gridHandler.gridAg.addColumnText("employeeNr", this.terms["time.employee.employee.employeenr"], null);
        this.gridHandler.gridAg.addColumnText("employeeName", this.terms["common.name"], null, false);
        this.gridHandler.gridAg.addColumnText("payrollGroupName", this.terms["time.employee.payrollgroup.payrollgroup"], null);
        this.gridHandler.gridAg.addColumnText("payrollPriceTypeName", this.terms["time.payroll.payrollpricetype.payrollpricetype"], null);
        this.gridHandler.gridAg.addColumnSelect("payrollLevelId", this.terms["time.employee.payrolllevel.payrolllevel"], null, {
            selectOptions: [],
            dynamicSelectOptions: {
                idField: "payrollLevelId",
                displayField: "name",
                options: "selectableLevels"
            },
            enableHiding: false,
            displayField: "payrollLevelName",
            dropdownIdLabel: "payrollLevelId",
            dropdownValueLabel: "name",
            editable: (row) => {
                return !this.isLocked(row)
            },
            onChanged: (row) => {
                this.setAsModified.bind(row)
            }
        });
        this.gridHandler.gridAg.addColumnNumber("payrollGroupAmount", this.terms["time.employee.employmentpricetype.payrollgroupamount"], null, {
            enableHiding: false,
            decimals: 2
        });
        this.gridHandler.gridAg.addColumnIcon(null, "", 22, {
            icon: "fal fa-lock-alt",
            showIcon: this.isLocked.bind(this),
            minWidth: 22,
            maxWidth: 22
        });
        this.gridHandler.gridAg.addColumnNumber("employmentAmount", this.terms["time.employee.payrollreview.currentamount"], null, {
            enableHiding: false,
            decimals: 2
        });
        this.gridHandler.gridAg.addColumnNumber("adjustment", this.terms["time.employee.payrollreview.adjustment"], null, {
            enableHiding: false,
            decimals: 2,
            editable: (row) => !this.isLocked(row),
            onChanged: this.setAsModified.bind(this)
        });
        this.gridHandler.gridAg.addColumnNumber("amount", this.terms["time.employee.payrollreview.newamount"], null, {
            enableHiding: false,
            decimals: 2,
            editable: (row) => !this.isLocked(row),
            onChanged: this.setAsModified.bind(this)
        });
        this.gridHandler.gridAg.addColumnIcon("warningMessage", "", null, {
            icon: "fal fa-info-circle infoColor",
            showIcon: (row: PayrollReviewRowDTO) => row.hasWarning,
            onClick: this.showWarning.bind(this), toolTip: this.terms["time.employee.payrollreview.importexcel.warning.title"]
        });
        this.gridHandler.gridAg.addColumnIcon("errorMessage", "", null, {
            icon: "fal fa-exclamation-triangle errorColor",
            showIcon: (row: PayrollReviewRowDTO) => row.hasError,
            onClick: this.showError.bind(this),
            toolTip: this.terms["time.employee.payrollreview.importexcel.error.title"]
        });
        this.gridHandler.gridAg.addColumnIcon(null, "", null, {
            icon: "fal fa-times iconDelete",
            onClick: this.deleteRow.bind(this),
            toolTip: this.terms["time.employee.payrollreview.removerow"]
        });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => { this.selectionChanged() }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (rowNode) => { this.selectionChanged() }));
        this.gridHandler.gridAg.options.subscribe(events);
        this.gridHandler.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);
        this.gridHandler.gridAg.options.enableRowSelection = true;
        this.gridHandler.gridAg.options.setMinRowsToShow(15);
        this.gridHandler.gridAg.finalizeInitGrid(this.terms["time.employee.payrollreview.payrollreview"], true);
    }

    public isLocked(row: PayrollReviewRowDTO) {
        console.log("isLocked", row.readOnly, this.modifyPermission, this.payrollReviewHead.status);
        return row.readOnly || !this.modifyPermission || this.payrollReviewHead.status === TermGroup_PayrollReviewStatus.Executed;
    }

    private showWarning(row: PayrollReviewRowDTO) {
        this.notificationService.showDialogEx(this.terms["time.employee.payrollreview.importexcel.warning.title"], row.warningMessage, SOEMessageBoxImage.Error);
    }

    private showError(row: PayrollReviewRowDTO) {
        this.notificationService.showDialogEx(this.terms["time.employee.payrollreview.importexcel.error.title"], row.errorMessage, SOEMessageBoxImage.Error);
    }

    public deleteRow(row: PayrollReviewRowDTO) {
        this.payrollReviewHead.rows.splice(this.payrollReviewHead.rows.indexOf(row), 1);
        this.refreshGrid();
        this.setDirty();
    }

    private setAsModified(row: PayrollReviewRowDTO) {
        this.$timeout(() => {
            row.isModified = true;
            this.setDirty();
        });
    }

    // SERVICE CALLS
    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.PayrollReviewDisableShowUpdateInfo];
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.disableShowUpdateInfo = SettingsUtility.getBoolUserSetting(x, UserSettingType.PayrollReviewDisableShowUpdateInfo, false);
        });
    }

    private loadPayrollGroups(): ng.IPromise<any> {
        return this.payrollService.getPayrollGroupsSmall(false, true).then(x => {
            this.payrollGroups = x;
            this.payrollGroups.forEach(p => p['id'] = p.payrollGroupId);
        });
    }

    private loadPayrollLevels(): ng.IPromise<any> {
        return this.payrollService.getPayrollLevels().then(x => {
            this.allPayrollLevels = x;
            this.allPayrollLevels.forEach(p => p['id'] = p.payrollLevelId);
            this.usePayrollLevels = this.allPayrollLevels.length > 0;
        });
    }

    private load(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        if (this.payrollReviewHeadId > 0) {
            this.loading = true;
            this.employeeService.getPayrollReviewHead(this.payrollReviewHeadId, true, true, true, true, true).then(x => {
                this.isNew = false;
                this.payrollReviewHead = x;
                this.setPanelLabel();
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.employee.payrollreview.payrollreview"] + ' ' + this.payrollReviewHead.name);

                this.setSelectedPayrollGroups();
                this.payrollGroupsChanged().then(() => {
                    this.setSelectedPayrollTypes();
                    this.payrollPriceTypesChanged().then(() => {
                        this.setSelectedPayrollLevels();
                    });
                });
                this.loadEmployees(false).then(() => {
                    this.loading = false;
                    deferral.resolve();
                });
            });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private initLoadEmployees() {
        if (_.filter(this.payrollReviewHead.rows, r => r.isModified).length === 0) {
            this.loadEmployees(true);
        } else {
            const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.employee.payrollreview.loademployeeswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium);
            modal.result.then(val => {
                this.loadEmployees(true);
            });
        }
    }

    private loadEmployees(createRows: boolean): ng.IPromise<any> {
        const deferral = this.$q.defer();
        if (!this.payrollReviewHead.dateFrom || this.payrollReviewHead.payrollGroupIds.length === 0 || this.payrollReviewHead.payrollPriceTypeIds.length === 0) {
            deferral.resolve();
        } else {
            // Clear previous
            if (createRows)
                this.payrollReviewHead.rows = [];

            this.progress.startLoadingProgress([() => {
                return this.employeeService.getEmployeesForPayrollReview(this.payrollReviewHead.dateFrom, this.payrollReviewHead.payrollGroupIds, this.payrollReviewHead.payrollPriceTypeIds, this.payrollReviewHead.payrollLevelIds, this.payrollReviewHead.rows.map(r => r.employeeId)).then(employees => {
                    _.forEach(employees, employee => {
                        let row: PayrollReviewRowDTO;
                        if (createRows) {
                            row = new PayrollReviewRowDTO();
                            row.adjustment = 0;
                            row.payrollLevelId = employee.payrollLevelId;
                            row.amount = employee.employmentAmount;
                            this.payrollReviewHead.rows.push(row);
                        } else {
                            row = _.find(this.payrollReviewHead.rows, r =>
                                r.employeeId === employee.employeeId &&
                                r.payrollGroupId === employee.payrollGroupId &&
                                r.payrollPriceTypeId === employee.payrollPriceTypeId);
                        }

                        if (row) {
                            row.employeeId = employee.employeeId;
                            row.employeeNr = employee.employeeNr;
                            row.employeeName = employee.name;
                            row.payrollGroupId = employee.payrollGroupId;
                            row.payrollGroupName = this.getPayrollGroupName(employee.payrollGroupId);
                            row.payrollPriceTypeId = employee.payrollPriceTypeId;
                            row.payrollPriceTypeName = this.getPayrollPriceTypeName(employee.payrollPriceTypeId);
                            row.selectableLevels = employee.selectableLevels;
                            row.payrollLevelName = this.getPayrollLevelName(createRows ? employee.payrollLevelId : row.payrollLevelId);
                            row.payrollGroupAmount = createRows ? employee.payrollGroupAmount : this.getPayrollGroupAmount(row);
                            row.employmentAmount = employee.employmentAmount;
                            row.readOnly = employee.readOnly;
                            row.isModified = false;
                        }
                    });
                    this.refreshGrid();
                    if (createRows)
                        this.setDirty();
                });
            }]).then(() => {
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.payrollReviewHead.status = this.payrollReviewHead.status === TermGroup_PayrollReviewStatus.New ? TermGroup_PayrollReviewStatus.Preliminary : this.payrollReviewHead.status;
            this.employeeService.savePayrollReviewHead(this.payrollReviewHead).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.payrollReviewHeadId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.payrollReviewHead.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.payrollReviewHeadId = result.integerValue;
                        this.payrollReviewHead.payrollReviewHeadId = result.integerValue;
                    }

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.payrollReviewHead, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.showUpdateReminderDialog();
            this.dirtyHandler.clean();
            this.load();
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.employeeService.getPayrollReviewHeads(false, true, true, true, true).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.payrollReviewHeadId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.payrollReviewHeadId) {
                    this.payrollReviewHeadId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deletePayrollReviewHead(this.payrollReviewHeadId).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                    this.new();
                } else {
                    completion.failed(result.errorMessage);
                };
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private initUpdate() {
        var rowsWithZeroAmount = _.filter(this.payrollReviewHead.rows, r => r.amount === 0).length;
        if (rowsWithZeroAmount > 0) {
            var modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.employee.payrollreview.amountwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium);
            modal.result.then(val => {
                this.update();
            });
        } else {
            this.update();
        }
    }

    protected update() {
        var modal = this.notificationService.showDialog(this.terms["core.info"], this.terms["time.employee.payrollreview.updateinformation"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium);
        modal.result.then(val => {
            this.progress.startSaveProgress((completion) => {
                this.employeeService.updatePayrollReview(this.payrollReviewHead.payrollReviewHeadId, this.keepFuture).then((result) => {
                    if (result.success) {
                        this.load();
                        completion.completed(Constants.EVENT_EDIT_SAVED, this.payrollReviewHead);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, this.guid);
        });
    }

    private openMassAdjustmentDialog() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollReview/Dialogs/MassAdjustment/MassAdjustmentDialog.html"),
            controller: MassAdjustmentDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                nbrOfEmployeesSelected: () => { return this.selectedCount },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.setDirty();

                let rows: PayrollReviewRowDTO[] = this.gridHandler.gridAg.options.getSelectedRows();
                let value: number = result.value;

                _.forEach(rows, row => {
                    row.adjustment = (result.adjustmentType === 2) ? (row.employmentAmount || 0) * (value / 100) : value;
                    row.amount = (row.employmentAmount || 0) + (row.adjustment || 0);
                    this.gridHandler.gridAg.options.refreshRows(row);
                });
            }
        });
    }

    private exportExcel() {
        let clone: PayrollReviewHeadDTO = CoreUtility.cloneDTO(this.payrollReviewHead);
        clone.rows = this.gridHandler.gridAg.options.getSelectedRows();

        this.progress.startWorkProgress((completion) => {
            this.employeeService.exportPayrollReview(clone).then((result) => {
                if (result) {
                    completion.completed(null, true);
                    window.location.assign(result);
                } else {
                    completion.failed(this.terms["time.employee.payrollreview.exportexcel.error"]);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, this.terms["time.employee.payrollreview.exportexcel.progress"]);
    }

    private importExcel() {
        var url = CoreUtility.apiPrefix + Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW_IMPORT + this.payrollReviewHead.dateFrom.toDateTimeString();
        var modal = this.notificationService.showFileUpload(url, this.terms["time.employee.payrollreview.importexcel"], true, true, false);
        modal.result.then(res => {
            let rows: PayrollReviewRowDTO[] = res.result
            if (!rows)
                rows = [];

            rows = rows.map(r => {
                let obj = new PayrollReviewRowDTO();
                angular.extend(obj, r);
                return obj;
            });

            if (rows.length > 0) {
                this.payrollReviewHead.rows = rows;
                this.loadEmployees(false).then(() => {
                    this.refreshGrid();
                    this.setDirty();
                });
            } else {
                this.notificationService.showDialogEx(this.terms["time.employee.payrollreview.importexcel"], this.terms["time.employee.payrollreview.importexcel.error"], SOEMessageBoxImage.Error);
            }
        });
    }

    // EVENTS

    private selectionChanged() {
        this.$timeout(() => {
            this.selectedCount = this.gridHandler.gridAg.options.getSelectedCount();
        });
    }

    private payrollGroupsChanged(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.mapSelectedPayrollGroups();
            this.populatePayrollPriceTypes();
        });
    }

    private payrollPriceTypesChanged(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.mapSelectedPayrollPriceTypes();
            this.populatePayrollLevels();
        });
    }

    private payrollLevelsChanged(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.mapSelectedPayrolLevels();
        });
    }

    private isEditableColumn(colDef) {
        return colDef && (colDef.field === 'adjustment' || colDef.field === 'amount' || colDef.field === 'payrollLevelId');
    }

    private afterCellEdit(row: PayrollReviewRowDTO, colDef, newValue, oldValue) {
        if (newValue !== oldValue) {
            this.setDirty();

            if (this.isEditableColumn(colDef)) {
                if (row) {
                    if (colDef.field === 'adjustment')
                        row.amount = (row.employmentAmount || 0) + (row.adjustment || 0);
                    else if (colDef.field === 'amount')
                        row.adjustment = (row.amount || 0) - (row.employmentAmount || 0);
                    else if (colDef.field === 'payrollLevelId')
                        row.payrollGroupAmount = this.getPayrollGroupAmount(row);
                    this.gridHandler.gridAg.options.refreshRows(row);
                }
            }
        }
    }

    private handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { previousCellPosition, nextCellPosition } = params;
        let { rowIndex, colDef } = previousCellPosition;

        if (this.isEditableColumn(colDef)) {
            let row: PayrollReviewRowDTO = this.gridHandler.gridAg.options.getVisibleRowByIndex(rowIndex).data;
            if (row) {
                const nextRowResult = this.findNextRow(row);
                if (nextRowResult) {
                    this.gridHandler.gridAg.options.startEditingCell(nextRowResult.rowNode.data, this.gridHandler.gridAg.options.getColumnByField(colDef.colId));
                    return null;
                }
            }
        }

        return { rowIndex: nextCellPosition ? nextCellPosition.rowIndex : rowIndex, column: nextCellPosition ? nextCellPosition.column : colDef };
    }

    private findNextRow(row): { rowIndex: number, rowNode: any } {
        const result = this.gridHandler.gridAg.options.getNextRow(row);

        return !!result.rowNode ? result : null;
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.selectedPayrollGroups = [];
        this.selectedPayrollLevels = [];
        this.selectedPayrollPriceTypes = [];

        this.payrollReviewHeadId = 0;
        this.payrollReviewHead = new PayrollReviewHeadDTO();
        this.payrollReviewHead.payrollGroupIds = [];
        this.payrollReviewHead.payrollPriceTypeIds = [];
        this.payrollReviewHead.payrollLevelIds = [];
        this.payrollReviewHead.status = TermGroup_PayrollReviewStatus.New;
        this.payrollReviewHead.statusName = this.terms["common.new"];
        this.payrollReviewHead.rows = [];
        this.refreshGrid();

        this.setPanelLabel();
    }

    protected copy() {
        if (!this.payrollReviewHead || !this.payrollReviewHead.payrollReviewHeadId || this.payrollReviewHead.payrollReviewHeadId === 0)
            return;

        super.copy();

        this.isNew = true;
        this.payrollReviewHeadId = 0;
        this.payrollReviewHead.payrollReviewHeadId = 0;
        this.payrollReviewHead.status = TermGroup_PayrollReviewStatus.New;
        this.payrollReviewHead.statusName = this.terms["common.new"];
        _.forEach(this.payrollReviewHead.rows, row => {
            row.payrollReviewHeadId = 0;
            row.payrollReviewRowId = 0;
            row.readOnly = false;
        });
        this.refreshGrid();
        this.setPanelLabel();
        this.setDirty();
    }

    private populatePayrollPriceTypes() {
        this.payrollPriceTypes = [];
        _.forEach(this.payrollReviewHead.payrollGroupIds, payrollGroupId => {
            let payrollGroup = _.find(this.payrollGroups, p => p.payrollGroupId === payrollGroupId);
            if (payrollGroup) {
                _.forEach(payrollGroup.priceTypes, priceType => {
                    if (!_.includes(this.payrollPriceTypes.map(p => p.payrollPriceTypeId), priceType.payrollPriceTypeId))
                        this.payrollPriceTypes.push(priceType);
                });
            }
        });
        this.payrollPriceTypes.forEach(p => p['id'] = p.payrollPriceTypeId);
        this.payrollPriceTypes = _.orderBy(this.payrollPriceTypes, p => p.priceTypeName);
    }

    private populatePayrollLevels() {
        this.payrollLevels = [];
        if (!this.payrollReviewHead.payrollPriceTypeIds)
            return;

        let allPriceTypes = this.getAllPayrollPriceTypes();
        _.forEach(this.payrollReviewHead.payrollPriceTypeIds, payrollPriceTypeId => {
            let payrollPriceTypes = _.filter(allPriceTypes, p => p.payrollPriceTypeId === payrollPriceTypeId && p.priceTypeLevel && p.priceTypeLevel.selectableLevelIds);
            _.forEach(payrollPriceTypes, payrollPriceType => {
                _.forEach(payrollPriceType.priceTypeLevel.selectableLevelIds, selectableLevelId => {
                    let payrollLevel = _.find(this.allPayrollLevels, p => p.payrollLevelId === selectableLevelId);
                    if (payrollLevel && !_.includes(this.payrollLevels.map(p => p.payrollLevelId), selectableLevelId)) {
                        this.payrollLevels.push(payrollLevel);
                    }
                });
            });

        });
        this.payrollLevels = _.orderBy(this.payrollLevels, p => p.name);
        if (this.usePayrollLevels) {
            let emptyLevel = new PayrollLevelDTO(); 
            emptyLevel.payrollLevelId = null;
            emptyLevel.name = this.terms["time.employee.payrolllevel.missing"];
            this.payrollLevels.unshift(emptyLevel);
        }
        this.payrollLevels.forEach(p => p['id'] = p.payrollLevelId);
    }

    private getAllPayrollPriceTypes(): PayrollGroupPriceTypeDTO[] {
        var all = [];
        _.forEach(this.payrollGroups, payrollGroup => {
            _.forEach(payrollGroup.priceTypes, priceType => {
                all.push(priceType);
            });
        });
        return all;
    }

    private mapSelectedPayrollGroups() {
        this.payrollReviewHead.payrollGroupIds = this.selectedPayrollGroups.map(p => p.id);
    }

    private mapSelectedPayrollPriceTypes() {
        this.payrollReviewHead.payrollPriceTypeIds = this.selectedPayrollPriceTypes.map(p => p.id);
    }

    private mapSelectedPayrolLevels() {
        this.payrollReviewHead.payrollLevelIds = this.selectedPayrollLevels.map(p => p.id);
    }

    private setSelectedPayrollGroups() {
        this.selectedPayrollGroups = _.filter(this.payrollGroups, p => _.includes(this.payrollReviewHead.payrollGroupIds, p.payrollGroupId));
    }

    private setSelectedPayrollTypes() {
        this.selectedPayrollPriceTypes = _.filter(this.payrollPriceTypes, p => _.includes(this.payrollReviewHead.payrollPriceTypeIds, p.payrollPriceTypeId));
    }

    private setSelectedPayrollLevels() {
        this.selectedPayrollLevels = _.filter(this.payrollLevels, p => _.includes(this.payrollReviewHead.payrollLevelIds, p.payrollLevelId));
    }

    private getPayrollGroupName(payrollGroupId: number): string {
        let payrollGroup = _.find(this.payrollGroups, p => p.payrollGroupId === payrollGroupId);
        return payrollGroup ? payrollGroup.name : '';
    }

    private getPayrollLevelName(payrollLevelId: number): string {
        let payrollLevel = this.usePayrollLevels ? _.find(this.allPayrollLevels, p => p.payrollLevelId === payrollLevelId) : null;
        return payrollLevel ? payrollLevel.name : '';
    }

    private getPayrollPriceTypeName(payrollPriceTypeId: number): string {
        let payrollPriceType = _.find(this.payrollPriceTypes, p => p.payrollPriceTypeId === payrollPriceTypeId);
        return payrollPriceType ? payrollPriceType.priceTypeName : '';
    }

    private getPayrollGroupAmount(row: PayrollReviewRowDTO) {
        return row && row.selectableLevels ? _.find(row.selectableLevels, p => p.payrollLevelId === row.payrollLevelId)?.amount : 0;
    }

    private setPanelLabel() {
        this.panelLabel = this.terms["time.employee.payrollreview.payrollreviewlabel"] + " | " + this.terms["common.status"] + ": " + this.payrollReviewHead.statusName;
    }

    private refreshGrid() {
        this.gridHandler.gridAg.setData(this.payrollReviewHead.rows);
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private showUpdateReminderDialog() {
        if (this.disableShowUpdateInfo)
            return;

        var modal = this.notificationService.showDialog(this.terms["common.obs"], this.terms["time.employee.payrollreview.updatereminder"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium, false, true, this.terms["time.schedule.planning.templateschedulewarning.donotshowagain"]);
        modal.result.then(val => {
            if (val.isChecked)
                this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollReviewDisableShowUpdateInfo, true);
        });
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.payrollReviewHead) {
                var errors = this['edit'].$error;

                if (!this.payrollReviewHead.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.payrollReviewHead.dateFrom)
                    mandatoryFieldKeys.push("time.employee.payrollreview.validfrom");
                if (this.payrollReviewHead.payrollGroupIds.length === 0)
                    mandatoryFieldKeys.push("time.employee.payrollgroup.payrollgroups");
                if (this.payrollReviewHead.payrollPriceTypeIds.length === 0)
                    mandatoryFieldKeys.push("time.payroll.payrollpricetype.payrollpricetypes");

                if (errors['rowValid'])
                    validationErrorKeys.push("time.employee.payrollreview.rowinvalid");
            }
        });
    }
}
