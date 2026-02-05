import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, CompanySettingType, TermGroup_AttestEntity, SoeModule, TermGroup_TemplateScheduleActivateFunctions, TermGroup } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { SoeGridOptionsEvent, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, DayOfWeek } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { ActivateScheduleControlDTO, ActivateScheduleGridDTO } from "../../../Common/Models/EmployeeScheduleDTOs";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TimeScheduleTemplateHeadSmallDTO, TimeScheduleTemplatePeriodSmallDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";
import { RecalculateTimeStatusDialogController } from "../../Dialogs/RecalculateTimeStatus/RecalculateTimeStatusDialogController";
import { ActiveScheduleControlDialogController } from "../../Dialogs/ActiveScheduleControl/ActiveScheduleControlDialogController";
import { WorkProgressCompletion } from "../../../Core/Handlers/ProgressHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Modal
    private modal: any;
    private modalInstance: any;

    // Terms:
    private terms: any;
    private title: string;

    // Permissions
    private templateScheduleEditHiddenPermission: boolean = false;

    // Company settings
    private useStaffing: boolean = false;
    private showPreliminary: boolean = false;
    private useAccountsHierarchy: boolean = false;
    private defaultPreliminary: boolean = false;
    private defaultEmployeeAccountDimId: number = 0;
    private defaultEmployeeAccountDimName: string = '';

    // Data
    private hiddenEmployeeId: number = 0;
    private employeeIds: number[];
    private affectedEmployeeIds: number[] = [];
    private dateFrom?: Date;
    private dateTo?: Date;
    private rows: ActivateScheduleGridDTO[];
    private employeeGroups: SmallGenericType[];
    private functions: any[];
    private templateHeads: TimeScheduleTemplateHeadSmallDTO[] = [];
    private templatePeriods: TimeScheduleTemplatePeriodSmallDTO[] = [];

    // Toolbar
    private toolbarInclude: any;

    // Footer
    private gridFooterComponentUrl: any;

    // Flags
    private isModal: boolean = false;
    private onlyLatest: boolean = true;
    private hasInitialAttestState: boolean = false;
    private selectedCount: number = 0;
    private executing: boolean = false;

    // Properties
    private dummyModel: any;
    private selectedFunction: TermGroup_TemplateScheduleActivateFunctions = TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate;
    private _selectedHeadId: number = 0;
    private get selectedHeadId(): number {
        return this._selectedHeadId;
    }
    private set selectedHeadId(id: number) {
        this._selectedHeadId = id;
        this.loadTemplatePeriods();
    }
    private selectedPeriodId: number = 0;
    private startDate: Date;
    private stopDate: Date;
    private stopDateOptions = {
        maxDate: CalendarUtility.getDateToday().addYears(2),
        customClass: this.getStopDateDayClass
    };
    private preliminary: boolean = false;

    private validationHandler: IValidationSummaryHandler;
    private gridform: ng.IFormController;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Time.Schedule.Activate", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;
        this.toolbarInclude = urlHelperService.getGlobalUrl("Time/Schedule/Activate/Views/gridHeader.html");
        this.gridFooterComponentUrl = urlHelperService.getGlobalUrl("Time/Schedule/Activate/Views/gridFooter.html");

        this.validationHandler = validationSummaryHandlerFactory.create();

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadModifyPermissions())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadFunctions())
            .onBeforeSetUpGrid(() => this.getHiddenEmployeeId())
            .onBeforeSetUpGrid(() => this.loadTemplateHeads())
            .onBeforeSetUpGrid(() => this.getInitialAttestState())
            .onBeforeSetUpGrid(() => this.loadEmployeeGroups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
        });
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        if (this.parameters.employeeIds)
            this.employeeIds = this.parameters.employeeIds;
        if (this.parameters.dateFrom)
            this.dateFrom = new Date(parameters.dateFrom);
        if (this.parameters.dateTo)
            this.dateTo = new Date(parameters.dateTo);
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {
                this.reloadData();
            });
        }

        this.flowHandler.start([
            { feature: Feature.Time_Schedule_Placement, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_Placement].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_Placement].modifyPermission;
    }

    public setupGrid() {
        this.doubleClickToEdit = false;

        var colDefNr = this.gridAg.addColumnText("employeeNr", this.terms["time.employee.employee.employeenrshort"], 80);
        colDefNr.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
            return valueA.padLeft(50, '0').toLowerCase().localeCompare(valueB.padLeft(50, '0').toLowerCase());
        };
        this.gridAg.addColumnText("employeeName", this.terms["common.name"], null);
        this.gridAg.addColumnSelect("employeeGroupName", this.terms["time.employee.employeegroup.employeegroup"], 100, { displayField: "employeeGroupName", selectOptions: this.employeeGroups, dropdownValueLabel: "name", enableHiding: true });
        if (this.useAccountsHierarchy)
            this.gridAg.addColumnText("accountNamesString", this.terms["time.employee.employee.accountswithdefault"], 100, true);
        else
            this.gridAg.addColumnText("categoryNamesString", this.terms["time.employee.employee.categories"], 100, true);
        this.gridAg.addColumnDate("employmentEndDate", this.terms["time.employee.employee.employmentenddate"], null, true);
        this.gridAg.addColumnBool("isPlaced", this.terms["time.schedule.activate.isplaced"], 25);
        this.gridAg.addColumnText("timeScheduleTemplateHeadName", this.terms["time.schedule.activate.templatename"], null, true);
        this.gridAg.addColumnNumber("employeeScheduleStartDayNumber", this.terms["time.schedule.activate.startday"], 50, { enableHiding: true })
        this.gridAg.addColumnDate("employeeScheduleStartDate", this.terms["common.startdate"], 90);
        this.gridAg.addColumnDate("employeeScheduleStopDate", this.terms["common.stopdate"], 90);
        if (this.modifyPermission)
            this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-times iconDelete", onClick: this.initDelete.bind(this), showIcon: this.showDeleteIcon.bind(this), toolTip: this.terms["time.schedule.activate.delete"] });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (rowNode) => {
            // Permission for selecting hidden employee
            return this.templateScheduleEditHiddenPermission || (rowNode.data && rowNode.data.employeeId !== this.hiddenEmployeeId);
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => {
            this.$timeout(() => {
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("time.schedule.activate", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.recalculatetimestatus", "time.recalculatetimestatus", IconLibrary.FontAwesome, "fa-calendar-check",
            () => { this.openRecalculateTimeStatusDialog(false); }
        )));

        this.toolbar.addInclude(this.toolbarInclude);
    }

    // SERVICE CALLS

    private getHiddenEmployeeId() {
        return this.sharedScheduleService.getHiddenEmployeeId().then((id) => {
            this.hiddenEmployeeId = id;
        });
    }

    private getInitialAttestState(): ng.IPromise<any> {
        return this.coreService.hasInitialAttestState(TermGroup_AttestEntity.Unknown, SoeModule.Time).then(x => {
            this.hasInitialAttestState = x;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.name",
            "common.startdate",
            "common.stopdate",
            "time.employee.employee.employeenrshort",
            "time.employee.employeegroup.employeegroup",
            "time.employee.employee.categories",
            "time.employee.employee.employmentenddate",
            "time.employee.employee.accountswithdefault",
            "time.schedule.activate",
            "time.schedule.activate.isplaced",
            "time.schedule.activate.ispreliminary",
            "time.schedule.activate.templatename",
            "time.schedule.activate.startday",
            "time.schedule.activate.delete",
            "time.schedule.activate.delete.error",
            "time.schedule.activate.progressmessage",
            "time.schedule.activate.searchtemplate"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.title = this.terms["time.schedule.activate"];
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.templateScheduleEditHiddenPermission = x[Feature.Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeUseStaffing);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.TimePlacementHidePreliminary);
        settingTypes.push(CompanySettingType.TimePlacementDefaultPreliminary);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useStaffing = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseStaffing);
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.showPreliminary = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimePlacementHidePreliminary);
            this.defaultPreliminary = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimePlacementDefaultPreliminary);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            if (this.defaultPreliminary)
                this.preliminary = true;
            if (this.useAccountsHierarchy && this.defaultEmployeeAccountDimId != 0)
                this.loadDefaultEmployeeAccount();
        });
    }

    private loadFunctions(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TemplateScheduleActivateFunctions, false, true).then(x => {
            this.functions = x;

            // Set default action to 'Change stop date'
            this.selectedFunction = TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate;
        });
    }

    private loadTemplateHeads(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.parameters.employeeIds) {
            deferral.resolve();
            return;
        }

        this.scheduleService.getTimeScheduleTemplateHeadsForActivate().then(x => {
            this.templateHeads = x;

            let search = new TimeScheduleTemplateHeadSmallDTO();
            search.timeScheduleTemplateHeadId = 0;
            search.name = this.terms["time.schedule.activate.searchtemplate"];

            this.templateHeads.splice(0, 0, search);

            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadTemplatePeriods(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (!this.selectedHeadId) {
            deferral.resolve();
            return;
        }

        this.scheduleService.getTimeScheduleTemplatePeriodsForActivate(this.selectedHeadId).then(x => {
            this.templatePeriods = x;
            this.setPeriod();
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadDefaultEmployeeAccount(): ng.IPromise<any> {
        return this.coreService.getAccountDim(this.defaultEmployeeAccountDimId, true, false, false, true).then(x => {
            this.defaultEmployeeAccountDimName = x.name;
        })
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.coreService.getEmployeeGroupsDict(false).then(x => {
            this.employeeGroups = x;
        })
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getPlacementsForGrid(this.onlyLatest, true, this.employeeIds, this.dateFrom, this.dateTo).then(x => {
                this.rows = x;
                this.setData(this.rows);
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private delete(control: ActivateScheduleControlDTO, item: ActivateScheduleGridDTO, completion: WorkProgressCompletion) {
        return this.scheduleService.deleteEmployeeSchedule(control, item).then(result => {
            if (result.success) {
                completion.completed(result, true);
                this.affectedEmployeeIds.push(item.employeeId);
                this.reloadData();
            } else {
                completion.failed(result.errorMessage);
            }
        });
    }

    // EVENTS

    private onlyLatestChanged() {
        this.$timeout(() => {
            this.reloadData();
        });
    }

    private initActivate() {
        this.executing = true;
        this.openRecalculateTimeStatusDialog(true);
    }

    private functionChanged(func) {
        if (func == TermGroup_TemplateScheduleActivateFunctions.ChangeStopDate)
            this.startDate = null;
    }

    private initDelete(item: ActivateScheduleGridDTO) {
        var keys: string[] = [
            "time.schedule.activate.delete.message",
            "time.schedule.activate.delete.message.nocheck",
            "time.schedule.activate.delete.message.nocheck.info",
            "time.recalculatetimestatus.activateschedulecontrol",
            'time.schedule.activate.delete.message.hidden.info.category',
            'time.schedule.activate.delete.message.hidden.info.accountshierarchy'
        ];

        this.translationService.translateMany(keys).then(deleteTerms => {
            var msg: string = deleteTerms["time.schedule.activate.delete.message"]
            if (item.employeeHidden) {
                if (this.useAccountsHierarchy)
                    msg += "\n<b>" + deleteTerms["time.schedule.activate.delete.message.hidden.info.accountshierarchy"] + ' ' + this.defaultEmployeeAccountDimName + '</b>'
                else
                    msg += "\n<b>" + deleteTerms["time.schedule.activate.delete.message.hidden.info.category"] + '</b>'
            }
            if (CoreUtility.isSupportAdmin)
                msg += "\n\n" + deleteTerms["time.schedule.activate.delete.message.nocheck.info"]

            this.progress.startWorkProgress((completion) => {
                var modal = this.notificationService.showDialogEx(this.terms["time.schedule.activate.delete"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { showCheckBox: CoreUtility.isSupportAdmin, checkBoxLabel: deleteTerms["time.schedule.activate.delete.message.nocheck"] });
                modal.result.then(result => {
                    if (result) {
                        var items: ActivateScheduleGridDTO[] = [];
                        items.push(item);

                        this.scheduleService.controlActivations(items, null, null, true).then(control => {
                            control.discardCheckesAll = !!result.isChecked;
                            if (!control.hasWarnings) {
                                this.delete(control, item, completion);
                            }
                            else {
                                var modalControl = this.modalInstance.open({
                                    templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/ActiveScheduleControl/Views/ActiveScheduleControlDialog.html"),
                                    controller: ActiveScheduleControlDialogController,
                                    controllerAs: 'ctrl',
                                    bindToController: true,
                                    backdrop: 'static',
                                    keyboard: true,
                                    size: 'xl',
                                    windowClass: 'fullsize-modal',
                                    scope: this.$scope,
                                    resolve: {
                                        control: () => { return control; },
                                        activateDate: () => { return item.employeeScheduleStartDate.addDays(-1); }
                                    }
                                });

                                modalControl.result.then(val => {
                                    control.createResult();
                                    this.delete(control, item, completion);
                                }, (reason) => {
                                    // Cancelled
                                    completion.completed(null, true)
                                });
                            }
                        });

                    }
                }, (reason) => {
                    // Cancelled
                    completion.completed(null, true);
                });
            }, null, deleteTerms["time.recalculatetimestatus.activateschedulecontrol"]);
        });
    }

    public closeModal(reload: boolean) {
        if (this.isModal)
            this.modal.close({ reload: reload || this.affectedEmployeeIds.length > 0, employeeIds: this.affectedEmployeeIds });
    }

    // HELP-METHDS

    private showDeleteIcon(row: ActivateScheduleGridDTO) {
        return row.isPlaced && (this.templateScheduleEditHiddenPermission || row.employeeId !== this.hiddenEmployeeId);
    }

    private setPeriod() {
        if (!this.selectedHeadId || this.templatePeriods.length === 0)
            return;

        var idx = 0;
        this.selectedPeriodId = this.templatePeriods[idx].timeScheduleTemplatePeriodId;
        let period = _.find(this.templatePeriods, p => p.timeScheduleTemplatePeriodId === this.selectedPeriodId);

        var head = _.find(this.templateHeads, h => h.timeScheduleTemplateHeadId === this.selectedHeadId);
        if (!head || !this.startDate)
            return;

        while (period && this.startDate.addDays(-period.dayNumber + 1).dayOfWeek() != DayOfWeek.Monday) {
            if (idx + 1 < this.templatePeriods.length) {
                idx++;
                this.selectedPeriodId = this.templatePeriods[idx].timeScheduleTemplatePeriodId;
                period = _.find(this.templatePeriods, p => p.timeScheduleTemplatePeriodId === this.selectedPeriodId);
            } else {
                let keys: string[] = [
                    "time.schedule.activate.cantsetperiod",
                    "time.schedule.activate.cantsetperiod.toofewdays"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.activate.cantsetperiod"], terms["time.schedule.activate.cantsetperiod.toofewdays"], SOEMessageBoxImage.Error);
                });
                break;
            }
        }
    }

    private getStopDateDayClass(data) {
        return (data.mode === 'day' && CalendarUtility.convertToDate(data.date) > CalendarUtility.getDateToday().addYears(2)) ? 'disabledDate' : '';
    }

    private openRecalculateTimeStatusDialog(activateMode: boolean) {
        let rows: ActivateScheduleGridDTO[] = this.gridAg.options.getSelectedRows();

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/RecalculateTimeStatus/Views/RecalculateTimeStatusDialog.html"),
            controller: RecalculateTimeStatusDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            keyboard: false,
            size: 'xl',
            scope: this.$scope,
            resolve: {
                activateMode: () => { return activateMode; },
                items: () => { return rows; },
                selectedFunction: () => { return this.selectedFunction; },
                selectedHeadId: () => { return this.selectedHeadId; },
                selectedPeriodId: () => { return this.selectedPeriodId; },
                startDate: () => { return this.startDate; },
                stopDate: () => { return this.stopDate; },
                preliminary: () => { return this.preliminary; },
            }
        });

        modal.result.then(result => {
            this.executing = false;
            if (result.reload) {
                this.affectedEmployeeIds.push(...rows.map(r => r.employeeId));
                this.reloadData();
            }
        }, function () {
            // Cancelled
        });
    }

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['gridform'].$error;

            if (errors['startDate'])
                mandatoryFieldKeys.push("common.startdate");

            if (errors['stopDate'])
                mandatoryFieldKeys.push("common.stopdate");

            //if (errors['startDateOnMonday'])
            //    validationErrorKeys.push("time.schedule.activate.muststartonmonday");

            if (errors['stopDateMaxTwoYears'])
                validationErrorKeys.push("time.schedule.activate.stopdatemaxtwoyears");

            if (errors['stopDateAfterStartDate'])
                validationErrorKeys.push("time.schedule.activate.stopdatebeforestartdate");

            if (errors['initialAttestState'])
                validationErrorKeys.push("time.schedule.activate.missinginitialatteststate");

            if (errors['selectedEmployees'])
                validationErrorKeys.push("time.schedule.activate.noemployeeselected");
        });
    }
}