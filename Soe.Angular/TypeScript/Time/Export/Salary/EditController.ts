import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../../Time/TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Feature, SoeTimeCodeType, TermGroup, SoeEntityState, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { TimeSalaryExportSelectionGroupDTO, TimeSalaryExportSelectionEmployeeDTO, TimeSalaryExportSelectionDTO } from "../../../Common/Models/TimeSalaryExportDTOs";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { DateSelectionDTO, DateRangeSelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { AttestReminderDialogController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/AttestReminder/AttestReminderDialogController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {    

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private accountDims: AccountDimSmallDTO[] = [];
    private head: TimeSalaryExportSelectionDTO;
    private groups: TimeSalaryExportSelectionGroupDTO[];
    modalInstance: any;

    //Permissions
    sendAttestReminderPermission: boolean = false;

    // Company settings
    private exportTarget: number;
    private externalExportId: string;
    private lockPeriod: boolean;    
    private showLockPeriod: boolean;
    private showCreateAsPreliminary: boolean;

    // Properties
    private timePeriodItem: string;
    private dateFrom: Date;
    private dateTo: Date;
    private accountDimId: number;

    private tmpFilterText: string;
    private filterText: string;

    // Flags
    private searchSelectionAccordionOpen = false;
    private exportSelectionAccordionOpen = false;
    private allGroupsExpanded: boolean = false;
    private allGroupsSelected: boolean = true;
    private searching: boolean = false;
    private exporting: boolean = false;

    private _createAsPreliminary: boolean;
    public get createAsPreliminary(): boolean {
        return this._createAsPreliminary;
    }
    public set createAsPreliminary(value: boolean) {        
        this._createAsPreliminary = value;

        this.createAsPreliminaryChanged();
    }


    //@ngInject
    constructor(
        private $q: ng.IQService,
        $uibModal,
        private $scope: ng.IScope,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;

        this.flowHandler.start([{ feature: Feature.Time_Export_Salary, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Export_Salary].readPermission;
        this.modifyPermission = response[Feature.Time_Export_Salary].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            this.$q.all([
                this.loadAccountDims()
            ]).then(() => {
                this.searchSelectionAccordionOpen = true;
            })
        });
    }

    // SERVICE CALLS

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_SendAttestReminder);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.sendAttestReminderPermission = x[Feature.Time_Time_Attest_SendAttestReminder];
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.exporting",
            "core.nogrouping",
            "core.warning",
            "core.error",
            "time.export.salary.exportwarning",
            "time.export.salary.accounthierachypayrollexportexternalcode",
            "common.categories.category",
            "common.employeegroups",
            "common.payrollgroups"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.SalaryExportTarget);
        settingTypes.push(CompanySettingType.SalaryExportExternalExportID);
        settingTypes.push(CompanySettingType.SalaryExportLockPeriod);
        settingTypes.push(CompanySettingType.SalaryExportAllowPreliminary);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.exportTarget = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportTarget);
            this.externalExportId = SettingsUtility.getStringCompanySetting(x, CompanySettingType.SalaryExportExternalExportID);
            this.showLockPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SalaryExportLockPeriod);
            this.showCreateAsPreliminary = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SalaryExportAllowPreliminary);
            this.lockPeriod = this.showLockPeriod;
            
            if (this.externalExportId && this.externalExportId.contains("#"))
                this.externalExportId = this.externalExportId.split('#')[0];
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        this.accountDims = [];
        return this.coreService.getAccountDimsSmall(false, true, false, false, false, false, true).then(x => {
            this.accountDims = x;

            let empty = new AccountDimSmallDTO();
            empty.accountDimId = 0;
            empty.name = this.terms["core.nogrouping"];
            this.accountDims.unshift(empty);

            let category = new AccountDimSmallDTO();
            category.accountDimId = -1;
            category.name = this.terms["common.categories.category"];
            this.accountDims.push(category);

            let employeeGroup = new AccountDimSmallDTO();
            employeeGroup.accountDimId = -2;
            employeeGroup.name = this.terms["common.employeegroups"];
            this.accountDims.push(employeeGroup);

            let payrollGroup = new AccountDimSmallDTO();
            payrollGroup.accountDimId = -3;
            payrollGroup.name = this.terms["common.payrollgroups"];
            this.accountDims.push(payrollGroup);

            let externalCode = new AccountDimSmallDTO();
            externalCode.accountDimId = -4;
            externalCode.name = this.terms["time.export.salary.accounthierachypayrollexportexternalcode"];
            this.accountDims.push(externalCode);            

            // Select first dim as default
            this.accountDimId = this.accountDims[0].accountDimId;
        });
    }

    private loadSelection() {
        this.searching = true;
        this.timeService.getTimeSalaryExportSelection(this.dateFrom, this.dateTo, this.accountDimId).then(x => {
            this.head = x;
            this.groups = x.timeSalaryExportSelectionGroups;

            this.searching = false;
            this.exportSelectionAccordionOpen = true;

            // Close selection accordion if result found
            if (this.groups.length > 0) {
                this.searchSelectionAccordionOpen = false;

                // If only one group, expand it
                if (this.groups.length === 1) {
                    this.groups[0].expanded = true;
                    this.allGroupsExpanded = true;
                }
            }
        });
    }

    // ACTIONS

    private initExport() {
        this.checkWarnings().then(warningsPassed => {
            if (warningsPassed) {
                this.validateExport().then(passed => {
                    if (passed)
                        this.export();
                });
            }
        });
    }

    private createAsPreliminaryChanged() {
        if (this.showLockPeriod) {
            if (this.createAsPreliminary)
                this.lockPeriod = false;
            else
                this.lockPeriod = true;
        }
    }

    private export() {
        this.exporting = true;
       
        this.progress.startWorkProgress((completion) => {
            this.timeService.exportSalary(this.getSelectedEmployeeIds(), this.dateFrom, this.dateTo, this.exportTarget, this.lockPeriod, this.createAsPreliminary).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                    this.exporting = false;
                }
            }, error => {
                completion.failed(error.message);
                this.exporting = false;
            });
        }, null, this.terms["core.exporting"] + '...').then(data => {
            this.messagingHandler.publishReloadGrid(this.guid);
            super.closeMe(true);
        }, error => {
            this.exporting = false;
        });
    }

    private openAttestReminderDialog(group: TimeSalaryExportSelectionGroupDTO) {
        let employeeIds: number[] = [];
        if (group)
            employeeIds.push(..._.map(group.timeSalaryExportSelectionEmployees, e => e.employeeId));
        else {
            _.forEach(this.groups, grp => {
                employeeIds.push(..._.map(grp.timeSalaryExportSelectionEmployees, e => e.employeeId));
            });
        }

        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Time/TimeAttest/Dialogs/AttestReminder/AttestReminderDialog.html"),
            controller: AttestReminderDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            scope: this.$scope,
            resolve: {
                employeeIds: () => { return employeeIds },
                dateFrom: () => { return this.dateFrom },
                dateTo: () => { return this.dateTo },
            }
        });
    }

    // EVENTS

    private onTimePeriodChanged(item) {
        this.timePeriodItem = item;
        this.dateFrom = null;
        this.dateTo = null;
        this.groups = [];
    }

    private onTimeIntervalSelected(selection: DateRangeSelectionDTO) {
        this.dateFrom = selection.from;
        this.dateTo = selection.to;
        this.groups = [];
    }

    private clearFilter() {
        this.filterText = this.tmpFilterText = '';
        this.setEmployeeVisibility();
    }

    private filterEmployees = _.debounce(() => {
        this.filterText = this.tmpFilterText;
        this.setEmployeeVisibility();
        this.$scope.$apply();
    }, 500, { leading: false, trailing: true });

    private expandAllGroups() {
        let expand = !this.allGroupsExpanded;

        _.forEach(this.groups, group => {
            group.expanded = !expand;
            this.groupExpanded(group);
        });
    }

    private selectAllGroups() {
        let select = !this.allGroupsSelected;

        _.forEach(this.groups, group => {
            group.selected = !select;
            this.groupSelected(group);
        });
    }

    private groupSelected(group: TimeSalaryExportSelectionGroupDTO) {
        group.selected = !group.selected;

        _.forEach(group.timeSalaryExportSelectionEmployees, employee => {
            employee.selected = group.selected;
        });

        this.allGroupsSelected = this.isAllGroupsSelected;
    }

    private groupExpanded(group: TimeSalaryExportSelectionGroupDTO) {
        group.expanded = !group.expanded;

        this.allGroupsExpanded = this.isAllGroupsExpanded;
    }

    private employeeSelected(employee: TimeSalaryExportSelectionEmployeeDTO) {
        employee.selected = !employee.selected;

        let group = this.getGroupFromEmployee(employee);
        if (group)
            group.selected = group.isAllEmployeesSelected;

        this.allGroupsSelected = this.isAllGroupsSelected;
    }

    private get isAllGroupsExpanded() {
        return _.filter(this.groups, g => !g.expanded).length === 0;
    }

    private get isAllGroupsSelected() {
        let allSelected: boolean = true;

        _.forEach(this.groups, group => {
            if (!group.isAllEmployeesSelected) {
                allSelected = false;
                return false;
            }
        });

        return allSelected;
    }

    // HELP-METHODS

    private getGroupFromEmployee(employee: TimeSalaryExportSelectionEmployeeDTO): TimeSalaryExportSelectionGroupDTO {
        let group: TimeSalaryExportSelectionGroupDTO;

        _.forEach(this.groups, g => {
            if (_.find(g.timeSalaryExportSelectionEmployees, e => e.employeeId === employee.employeeId)) {
                group = g;
                return false;
            }
        });

        return group;
    }

    private setEmployeeVisibility() {
        _.forEach(this.groups, group => {
            _.forEach(group.timeSalaryExportSelectionEmployees, employee => {
                employee.setVisible(this.filterText);
            });
        });
    }

    private getTotalSelected(): number {
        let total: number = 0;

        _.forEach(this.groups, group => {
            total += _.filter(group.timeSalaryExportSelectionEmployees, e => e.selected).length;
        });

        return total;
    }

    private getSelectedEmployeeIds(): number[] {
        let employeeIds: number[] = [];

        _.forEach(this.groups, group => {
            employeeIds.push(..._.map(_.filter(group.timeSalaryExportSelectionEmployees, e => e.selected), e => e.employeeId));
        });

        return employeeIds;
    }

    private hasSelectedAnyWarnings(): boolean {        
        let warnings: any[] = [];
        _.forEach(this.groups, group => {
            if (group.selected && !group.entirePeriodValidForExport)
                warnings.push(group);
        });
        return warnings.length > 0;
    }

    // VALIDATION

    private checkWarnings(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (!this.hasSelectedAnyWarnings()) {
            deferral.resolve(true);
        } else {
            // Warning
            var modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["time.export.salary.exportwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                deferral.resolve(true);
            }, (reason) => {
                deferral.resolve(false);
            });
        }

        return deferral.promise;
    }

    private validateExport(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.timeService.validateExportSalary(this.getSelectedEmployeeIds(), this.dateFrom, this.dateTo).then(result => {
            if (result) {
                if (result.success) {
                    // No warnings or errors
                    deferral.resolve(true);
                } else {
                    if (result.canUserOverride) {
                        // Warning
                        var modal = this.notificationService.showDialogEx(this.terms["core.warning"], result.infoMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            deferral.resolve(true);
                        }, (reason) => {
                            deferral.resolve(false);
                        });
                    } else {
                        // Error
                        this.notificationService.showDialogEx(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error);
                        deferral.resolve(false);
                    }
                }
            } else {
                // Error in call
                deferral.resolve(false);
            }
        });

        return deferral.promise;
    }
}