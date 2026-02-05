import { IUrlHelperService, UrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmploymentDTO, EmploymentChangeDTO, EmploymentVacationGroupDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { IEmployeeGroupSmallDTO, IPayrollGroupReportDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SOEMessageBoxImage, SOEMessageBoxButtons, CreateFromEmployeeTemplateMode, EditUserEmploymentFunctions } from "../../../../../Util/Enumerations";
import { CoreService } from "../../../../../Core/Services/CoreService";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { NotificationService } from "../../../../../Core/Services/NotificationService";
import { EmployeeService } from "../../../EmployeeService";
import { Feature, SoeEmploymentFinalSalaryStatus, SoeEntityState, CompanySettingType, TermGroup_EmploymentType, SoeTimeSalaryExportTarget, SoeReportTemplateType, ActionResultSave } from "../../../../../Util/CommonEnumerations";
import { AddEmploymentController } from "./AddEmploymentController";
import { EditEmploymentController } from "./EditEmploymentController";
import { DeleteEmploymentController } from "./DeleteEmploymentController";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { PayrollGroupSmallDTO, PayrollGroupPriceTypeDTO, PayrollGroupVacationGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { PayrollService } from "../../../../Payroll/PayrollService";
import { VacationGroupDTO } from "../../../../../Common/Models/VacationGroupDTO";
import { MessagingService } from "../../../../../Core/Services/MessagingService";
import { Constants } from "../../../../../Util/Constants";
import { EmploymentDialogController } from "./EmploymentDialogController";
import { SelectReportController } from "../../../../../Common/Dialogs/SelectReport/SelectReportController";
import { ReportService } from "../../../../../Core/Services/ReportService";
import { Guid, StringUtility } from "../../../../../Util/StringUtility";
import { IReportDataService } from "../../../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ReportJobDefinitionFactory } from "../../../../../Core/Handlers/ReportJobDefinitionFactory";
import { CreateFromTemplateDialogController } from "../../Dialogs/CreateFromTemplate/CreateFromTemplateDialogController";
import { AddDocumentToAttestFlowController } from "../../../../../Common/Dialogs/AddDocumentToAttestFlow/AddDocumentToAttestFlowController";
import { TimeHibernatingAbsenceController } from "../../../../Dialogs/TimeHibernatingAbsence/TimeHibernatingAbsenceController";
import { ExtraFieldRecordDTO } from "../../../../../Common/Models/ExtraFieldDTO";

export class EmploymentsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/Employments/Views/Employments.html'),
            scope: {
                employeeId: '=',
                numberAndName: '=',
                employeeTemplateId: '=',
                employments: '=',
                selectedEmployment: '=',
                selectedDate: '=',
                payrollGroupPriceTypes: '=',
                payrollGroupVacationGroups: '=',
                readOnly: '=',
                onReload: '&',
                isNew: '=',
                isManuallyNew: '=',
                parentGuid: '=',
            },
            restrict: 'E',
            replace: true,
            controller: EmploymentsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmploymentsController {

    // Data
    private employeeId: number;
    private numberAndName: string;
    private employeeTemplateId: number;
    private employments: EmploymentDTO[];
    private payrollGroups: PayrollGroupSmallDTO[] = [];
    private payrollGroupPriceTypes: PayrollGroupPriceTypeDTO[];
    private payrollGroupVacationGroups: PayrollGroupVacationGroupDTO[];
    private vacationGroups: VacationGroupDTO[];
    private employeeGroups: IEmployeeGroupSmallDTO[] = [];
    private annualLeaveGroups: ISmallGenericType[] = [];
    private companyPayrollGroupReports: IPayrollGroupReportDTO[] = [];
    private isNew: boolean;
    private isManuallyNew: boolean;
    private parentGuid: Guid;

    // Functions
    private onReload: (date: any) => void;

    // Terms
    private terms: { [index: string]: string; };

    // Settings
    private defaultEmployeeGroupId: number;
    private defaultPayrollGroupId: number;
    private showExternalCode = false;
    private setEmploymentPercentManually: boolean;
    private useEmploymentExperienceAsStartValue: boolean;
    private usePayroll: boolean;
    private payrollGroupMandatory = false;
    private useSimplifiedEmployeeRegistration = false;
    private useHibernatingEmployment = false;
    private hasEmployeeTemplates = false;
    private useAnnualLeave = false;

    // Permissions
    private reportPermission: boolean = false;
    private createDeleteShortenExtendHibernatingPermission: boolean = false;

    // Properties
    private _selectedDate: Date;
    private get selectedDate(): Date {
        return this._selectedDate;
    }
    private set selectedDate(date: Date) {
        if (!date)
            date = new Date().date();

        this._selectedDate = date;
    }
    private _selectedEmployment: EmploymentDTO;
    private get selectedEmployment(): EmploymentDTO {
        return this._selectedEmployment;
    }
    private set selectedEmployment(employment: EmploymentDTO) {
        if (!employment) {
            this.vacationGroupId = 0;
            this.payrollGroupPriceTypes = [];
            this.payrollGroupVacationGroups = [];
            return;
        }

        if (employment.employmentId)
            this._selectedEmployment = _.find(this.employments, e => e.employmentId === employment.employmentId);
        else
            this._selectedEmployment = employment;

        if (employment.payrollGroupId) {
            var payrollGroup = _.find(this.payrollGroups, p => p.payrollGroupId === this.selectedEmployment.payrollGroupId);
            this.payrollGroupPriceTypes = payrollGroup ? payrollGroup.priceTypes : [];
            this.loadPayrollGroupVacationGroups(employment.payrollGroupId, false, null).then(() => {
                this.setEmploymentVacationGroup();
            });
        }
    }

    private selectedEmploymentVacationGroup: EmploymentVacationGroupDTO;
    private vacationGroupId: number;
    private setEmploymentVacationGroup() {
        this.selectedEmploymentVacationGroup = undefined;
        this.vacationGroupId = 0;

        if (this.selectedEmployment.employmentVacationGroup && this.selectedEmployment.employmentVacationGroup.length > 0) {
            // Technically there can be multiple vacation groups connected to one employment with different date periods, but it's not used that way.
            // But to be certain we select one based on date.
            this.selectedEmploymentVacationGroup = _.find(this.selectedEmployment.employmentVacationGroup, v => (v.fromDate && v.fromDate.isSameOrBeforeOnDay(this.selectedDate)) || !v.fromDate);
            this.vacationGroupId = this.selectedEmploymentVacationGroup ? this.selectedEmploymentVacationGroup.vacationGroupId : 0;
        }
    }

    private nbrOfDeleted: number = 0;

    private get hasNewEmployments(): boolean {
        return this.employments && _.filter(this.employments, e => !e.employmentId).length > 0;
    }

    // Flags
    private allRowsExpanded: boolean = false;
    private showDeleted: boolean = false;

    // Current values
    private deletedEmploymentIds: number[] = [];

    //@ngInject
    constructor(
        private $uibModal,
        private coreService: CoreService,
        private reportService: ReportService,
        private reportDataService: IReportDataService,
        private urlHelperService: UrlHelperService,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        private messagingService: MessagingService,
        private employeeService: EmployeeService,
        private payrollService: PayrollService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $window: ng.IWindowService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadReadOnlyPermissions(),
            this.loadModifyPermissions(),
            this.addEmploymentAfterNewEmployee(),
            this.loadPayrollGroups(),
            this.loadEmployeeGroups(),
            this.loadPayrollGroupReports(),
        ]).then(() => {
            if (this.useAnnualLeave) {
                this.loadAnnualLeaveGroups();
            }
            this.selectedDate = new Date().date();
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employments, (newVal, oldVal) => {
            var filteredEmployments = this.filteredEmployments();
            this.collapseAllRows();

            this.selectedEmployment = null;
            if (filteredEmployments && filteredEmployments.length > 0) {
                this.selectedEmployment = _.find(filteredEmployments, e => e.dateFrom <= this.selectedDate && (!e.dateTo || e.dateTo >= this.selectedDate));
                // If no employment at selected date, select first in list (last employment, since ordered descending).
                if (!this.selectedEmployment)
                    this.selectedEmployment = filteredEmployments[0];
            }

            this.setPayrollGroupHasReports();
            this.nbrOfDeleted = _.filter(this.employments, e => e.state === SoeEntityState.Deleted).length;
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.info",
            "core.time.months",
            "core.time.month",
            "common.comment",
            "common.year",
            "time.employee.employment.history.message",
            "time.employee.employment.history.message.novalue",
            "time.employee.employment.experience.agreed",
            "time.employee.employment.experience.established",
            "time.employee.employment.cannotdelete.title",
            "time.employee.employment.cannotdelete.message",
            "time.employee.employment.cannotchangeemployment.title",
            "time.employee.employment.cannotchangeemployment.message"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeDefaultEmployeeGroup);
        settingTypes.push(CompanySettingType.TimeDefaultPayrollGroup);
        settingTypes.push(CompanySettingType.SalaryExportTarget);
        settingTypes.push(CompanySettingType.TimeSetEmploymentPercentManually);
        settingTypes.push(CompanySettingType.UsePayroll);
        settingTypes.push(CompanySettingType.UseEmploymentExperienceAsStartValue);
        settingTypes.push(CompanySettingType.PayrollGroupMandatory);
        settingTypes.push(CompanySettingType.UseHibernatingEmployment);
        settingTypes.push(CompanySettingType.UseSimplifiedEmployeeRegistration);
        settingTypes.push(CompanySettingType.UseAnnualLeave);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultEmployeeGroupId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeeGroup);
            this.defaultPayrollGroupId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultPayrollGroup);
            let salaryExportTarget: number = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportTarget);
            this.showExternalCode = (salaryExportTarget == SoeTimeSalaryExportTarget.BlueGarden || salaryExportTarget == SoeTimeSalaryExportTarget.Orkla);
            this.setEmploymentPercentManually = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSetEmploymentPercentManually);
            this.usePayroll = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UsePayroll);
            this.useEmploymentExperienceAsStartValue = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseEmploymentExperienceAsStartValue);
            this.payrollGroupMandatory = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollGroupMandatory);
            this.useHibernatingEmployment = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseHibernatingEmployment);
            this.useSimplifiedEmployeeRegistration = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseSimplifiedEmployeeRegistration);
            if (this.useSimplifiedEmployeeRegistration)
                this.loadHasEmployeeTemplates();
            this.useAnnualLeave = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAnnualLeave);
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Distribution_Reports_Selection);
        features.push(Feature.Time_Distribution_Reports_Selection_Download);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.reportPermission = x[Feature.Time_Distribution_Reports_Selection] && x[Feature.Time_Distribution_Reports_Selection_Download];
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment_CreateDeleteShortenExtendHibernating);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.createDeleteShortenExtendHibernatingPermission = x[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment_CreateDeleteShortenExtendHibernating];
        });
    }

    private loadPayrollGroups(): ng.IPromise<any> {
        return this.payrollService.getPayrollGroupsSmall(true, false).then(x => {
            this.payrollGroups = x;
        });
    }

    private loadPayrollGroupVacationGroups(payrollGroupId: number, selectStandard: boolean, changeDate: Date): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.payrollGroupVacationGroups && _.filter(this.payrollGroupVacationGroups, p => p.payrollGroupId === payrollGroupId).length > 0) {
            // Already loaded
            deferral.resolve();
        } else {
            return this.payrollService.getPayrollGroupVacationGroups(payrollGroupId, true).then(x => {
                this.payrollGroupVacationGroups = x;

                // Remove unsaved data (the user may change payrollgroup more then once )
                if (this.selectedEmployment && this.selectedEmployment.employmentVacationGroup && this.selectedEmployment.employmentVacationGroup.length > 0)
                    _.pullAll(this.selectedEmployment.employmentVacationGroup, _.filter(this.selectedEmployment.employmentVacationGroup, v => !v.employmentVacationGroupId));

                // Select standard vacation group
                if (selectStandard) {
                    var countVacationGroup = _.filter(this.selectedEmployment.employmentVacationGroup, v => v.fromDate?.isSameDayAs(changeDate)).length;
                    var standardVacationGroup: PayrollGroupVacationGroupDTO = _.find(this.payrollGroupVacationGroups, v => v.isDefault);
                    if (standardVacationGroup && countVacationGroup == 0) {
                        var avalibleGroups: PayrollGroupVacationGroupDTO[] = this.getAvailableEmploymentVacationGroups(0);
                        if (_.filter(avalibleGroups, g => g.vacationGroupId === standardVacationGroup.vacationGroupId).length > 0) {
                            // Standard vacation group doesn't exist, add it, but close current vacation group
                            this.closeCurrentVacationGroup(changeDate);
                            this.selectedEmploymentVacationGroup = this.addEmploymentVacationGroup(standardVacationGroup, changeDate);
                        } else {
                            var standardVacationGroups: EmploymentVacationGroupDTO[] = _.filter(this.selectedEmployment.employmentVacationGroup, v => v.vacationGroupId === standardVacationGroup.vacationGroupId);
                            if (_.filter(standardVacationGroups, v => !v.fromDate).length === 0)// if standard vacationgroup with fromdate null exist then do nothing.
                            {
                                // Otherwise close current vacationgroup if it has fromdate = null
                                this.closeCurrentVacationGroup(changeDate);
                                var lastGroup;
                                if (this.selectedEmployment.employmentVacationGroup.length > 0) {
                                    lastGroup = _.orderBy(this.selectedEmployment.employmentVacationGroup, 'sortableDate', 'asc')[0];
                                }
                                if (!lastGroup || lastGroup.vacationGroupId !== standardVacationGroup.vacationGroupId)
                                    this.addEmploymentVacationGroup(standardVacationGroup, changeDate);
                            }
                        }
                    }
                }
            });
        }

        return deferral.promise;
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.employeeService.getEmployeeGroupsSmall().then(x => {
            this.employeeGroups = x;
        });
    }

    private loadAnnualLeaveGroups(): ng.IPromise<any> {
        return this.employeeService.getAnnualLeaveGroupsDict(true).then(x => {
            this.annualLeaveGroups = x;
        });
    }

    private loadPayrollGroupReports(): ng.IPromise<any> {
        return this.employeeService.getCompanyPayrollGroupReports(true).then(x => {
            this.companyPayrollGroupReports = x;
        });
    }

    private loadHasEmployeeTemplates(): ng.IPromise<any> {
        return this.employeeService.hasEmployeeTemplates().then(x => {
            this.hasEmployeeTemplates = x;
        });
    }

    // EVENTS

    private expandAllRows() {
        _.forEach(this.employments, employment => {
            employment['expanded'] = true;
        });
        this.allRowsExpanded = true;
    }

    private collapseAllRows() {
        _.forEach(this.employments, employment => {
            employment['expanded'] = false;
        });
        this.allRowsExpanded = false;
    }

    private addEmploymentAfterNewEmployee() {
        if (this.isNew && this.isManuallyNew) {
            this.initAddEmployment(false);
        }
    }

    private initAddEmployment(showSecondary: boolean = true) {
        if (this.useSimplifiedEmployeeRegistration && this.hasEmployeeTemplates)
            this.addEmploymentFromTemplate(showSecondary);
        else
            this.addEmployment(showSecondary);
    }

    private addEmploymentFromTemplate(showSecondary: boolean = true) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/CreateFromTemplate/CreateFromTemplateDialog.html"),
            controller: CreateFromTemplateDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                mode: () => { return CreateFromEmployeeTemplateMode.NewEmployment },
                createdWithEmployeeTemplateId: () => { return this.employeeTemplateId },
                employeeId: () => { return this.employeeId },
                numberAndName: () => { return this.numberAndName }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result) {
                if (result.openEditPage)
                    this.addEmployment(showSecondary);
                else {
                    this.reloadEmployments();

                    this.messagingService.publish('employmentDateChanged', { newDateFrom: result.dto.date, newDateTo: null });
                    this.messagingService.publish('reloadFieldsChangedByNewEmploymentFromTemplate', { employeeId: this.employeeId });

                    if (result.initSigning) {
                        if (result.errorNumber == ActionResultSave.PrintEmploymentContractFromTemplateFailed) {
                            this.translationService.translate('time.employee.printemploymentcontractfromtemplatefailed').then(term => {
                                this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                            });
                        } else if (result.userId && result.dataStorageId) {
                            this.initSigningDocument(result.userId, result.dataStorageId);
                        }
                    }
                }
            }
        }, (reason) => {
            // Cancelled
        });
    }

    private addEmployment(showSecondary: boolean = true) {

        // Show add employment dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Employments/Views/AddEmployment.html"),
            controller: AddEmploymentController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                isNew: () => { return this.isNew; },
                showTemporaryPrimary: this.useHibernatingEmployment && this.createDeleteShortenExtendHibernatingPermission,
                showSecondary: showSecondary,
                employments: () => { return this.employments; },
                latestEmployment: () => { return this.getLatestEmployment(); }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                var newDateFrom: Date = result.dateFrom;
                var newDateTo: Date = result.dateTo;
                var latestEmployment = this.getLatestEmployment();
                var employmentCopy = result.copyLatest ? latestEmployment : null;
                var newEmployment: EmploymentDTO;
                if (result.closePrevious && newDateFrom) {
                    if (latestEmployment && newDateFrom > latestEmployment.dateFrom) {
                        var oldDateFrom: Date = newDateFrom; //trigger not evaluation on datefrom
                        var oldDateTo: Date = latestEmployment.dateTo;
                        this.validateShortenEmployment(oldDateFrom, oldDateTo, newDateFrom, newDateTo, result.finalSalary).then(passed => {
                            if (passed) {
                                this.updatePreviousEmployment(newDateFrom, result.finalSalary);
                                newEmployment = this.newEmployment(newDateFrom, newDateTo, result.comment, employmentCopy, result.isSecondaryEmployment, result.isTemporaryPrimaryEmployment, result.hibernatingTimeDeviationCauseId);
                                this.openEmploymentDialog(newEmployment, true, latestEmployment != null);
                            }
                        });
                    }
                    else {
                        this.updatePreviousEmployment(newDateFrom, result.finalSalary);
                        newEmployment = this.newEmployment(newDateFrom, newDateTo, result.comment, employmentCopy, result.isSecondaryEmployment, result.isTemporaryPrimaryEmployment, result.hibernatingTimeDeviationCauseId);
                        this.openEmploymentDialog(newEmployment, true, latestEmployment != null);
                    }
                } else {
                    newEmployment = this.newEmployment(newDateFrom, newDateTo, result.comment, employmentCopy, result.isSecondaryEmployment, result.isTemporaryPrimaryEmployment, result.hibernatingTimeDeviationCauseId);
                    this.openEmploymentDialog(newEmployment, true, latestEmployment != null);
                }

                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid });
            }
        }, (reason) => {
            // Escape or cancel was pressed
            if (this.isNew) {
                this.messagingService.publish(Constants.EVENT_CLOSE_TAB, this.parentGuid);
                this.messagingService.publish(Constants.EVENT_CLOSE_DIALOG, this.parentGuid);
            }
        });
    }

    private editEmployment(employment: EmploymentDTO) {
        if (employment.isEditing && employment.isChangingEmployment) {
            this.openEmploymentDialog(employment, false, this.getPreviousEmployment(employment.dateFrom) != null);
            return;
        }

        // Show edit employment dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Employments/Views/EditEmployment.html"),
            controller: EditEmploymentController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                employment: () => { return employment; },
                employments: () => { return this.employments; },
                usePayroll: () => { return this.usePayroll; },
                forceModifyEmploymentDates: () => { return employment.isChangingEmploymentDates; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                employment.isEdited = true;
                let oldDateFrom: Date = employment.dateFrom;
                let oldDateTo: Date = employment.dateTo;
                let newDateFrom: Date = result.dateFrom;
                let newDateTo: Date = result.dateTo;
                const dateHasChanged = !oldDateFrom.isSameDayAs(newDateFrom) || (oldDateTo && newDateTo && !oldDateTo.isSameDayAs(newDateTo)) || (oldDateTo && !newDateTo) || (!oldDateTo && newDateTo);

                let changeEmploymentFunction: EditUserEmploymentFunctions = result.changeEmploymentFunction;
                if (changeEmploymentFunction === EditUserEmploymentFunctions.ChangeEmployment && newDateFrom < oldDateFrom) {
                    this.notificationService.showDialogEx(this.terms["time.employee.employment.cannotchangeemployment.title"], this.terms["time.employee.employment.cannotchangeemployment.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    return;
                }

                if (changeEmploymentFunction === EditUserEmploymentFunctions.ChangeEmployment) {
                    this.selectedDate = newDateFrom ?? CalendarUtility.getDateToday();
                    this.employeeService.getEmployments(this.employeeId, this.selectedDate).then(employments => {
                        this.messagingService.publish(Constants.EVENT_EMPLOYMENTS_RELOADED, { employments: employments, date: this.selectedDate });
                        employment = _.find(employments, e => e.employmentId == employment.employmentId);
                        if (employment) {
                            employment.comment = result.comment;
                            employment.isChangingEmployment = true;
                            employment.currentChangeDateFrom = newDateFrom;
                            employment.currentChangeDateTo = newDateTo;
                            this.openEmploymentDialog(employment, false, this.getPreviousEmployment(employment.currentChangeDateFrom) != null);

                            if (dateHasChanged)
                                this.messagingService.publish('employmentDateChanged', { newDateFrom: newDateFrom, newDateTo: newDateTo });
                        }
                    });
                } else if (changeEmploymentFunction === EditUserEmploymentFunctions.ChangeEmploymentDates || changeEmploymentFunction === EditUserEmploymentFunctions.ChangeToNotTemporary) {
                    employment.comment = result.comment;
                    employment.currentChangeDateFrom = newDateFrom;
                    employment.currentChangeDateTo = newDateTo;
                    this.validateShortenEmployment(oldDateFrom, oldDateTo, newDateFrom, newDateTo, result.finalSalary, employment).then(passed => {
                        if (passed) {
                            employment.dateFrom = newDateFrom;
                            employment.dateTo = newDateTo;
                            employment.employmentEndReason = result.employmentEndReason ? result.employmentEndReason.id : 0;
                            employment.employmentEndReasonName = result.employmentEndReason ? result.employmentEndReason.name : '';

                            if (changeEmploymentFunction == EditUserEmploymentFunctions.ChangeEmploymentDates) {
                                employment.isChangingEmploymentDates = true;
                                if (result.finalSalary)
                                    employment.finalSalaryStatus = SoeEmploymentFinalSalaryStatus.ApplyFinalSalary;
                                else if (result.appliedFinalSalaryManually)
                                    employment.finalSalaryStatus = SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually;
                                else
                                    employment.finalSalaryStatus = SoeEmploymentFinalSalaryStatus.None;
                            }
                            else {
                                employment.isChangingToNotTemporary = true;
                            }

                            if (dateHasChanged)
                                this.messagingService.publish('employmentDateChanged', { newDateFrom: newDateFrom, newDateTo: newDateTo });
                        }
                    });
                }

                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid });
            }
        });
    }

    private deleteEmployment(employment: EmploymentDTO) {
        if (employment && employment.finalSalaryStatus > 0) {
            this.notificationService.showDialogEx(this.terms["time.employee.employment.cannotdelete.title"], this.terms["time.employee.employment.cannotdelete.message"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        // Show delete employment dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Employments/Views/DeleteEmployment.html"),
            controller: DeleteEmploymentController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                employment: () => { return employment; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                employment.isEdited = true;

                var oldDateFrom: Date = employment.dateFrom;
                var oldDateTo: Date = employment.dateTo;
                var newDateFrom: Date = employment.dateFrom;
                var newDateTo: Date = employment.dateFrom;
                this.validateShortenEmployment(oldDateFrom, oldDateTo, newDateFrom, newDateTo, false, employment, this.employments).then(passed => {
                    if (passed) {
                        employment.comment = result.comment;
                        employment.state = SoeEntityState.Deleted;
                        employment.isDeletingEmployment = true;
                        if (employment.employmentId > 0) {
                            this.deletedEmploymentIds.push(employment.employmentId);
                        }
                    }
                });

                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid });
            }
        });
    }

    private restoreEmployment(employment: EmploymentDTO) {
        if (employment && employment.isDeleted && this.isEmploymentDeletedInThisSession(employment)) {
            employment.comment = '';
            employment.state = SoeEntityState.Active;
            var index = this.deletedEmploymentIds.indexOf(employment.employmentId);
            if (index > -1) {
                this.deletedEmploymentIds.splice(index, 1);
            }
            this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid });
        }
    }

    private isEmploymentDeletedInThisSession(employment: EmploymentDTO): boolean {
        if (!employment)
            return false;
        if (this.deletedEmploymentIds && this.deletedEmploymentIds.indexOf(employment.employmentId) >= 0)
            return true;
        return false;
    }

    private openEmploymentDialog(employment: EmploymentDTO, isNew: boolean, hasPreviousEmployment) {

        const oldDateFrom = employment.dateFrom;
        const oldDateTo = employment.dateTo;

        // Show employment dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/Employments/Views/EmploymentDialog.html"),
            controller: EmploymentDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                payrollGroups: () => { return this.payrollGroups; },
                employeeGroups: () => { return this.employeeGroups; },
                vacationGroups: () => { return this.payrollGroupVacationGroups; },
                vacationGroupId: () => { return this.getCurrentVacationGroupId(employment); },
                annualLeaveGroups: () => { return this.annualLeaveGroups; },
                employment: () => { return employment; },
                isNew: () => { return isNew; },
                hasPreviousEmployment: () => { return hasPreviousEmployment },
                showExternalCode: () => { return this.showExternalCode; },
                setEmploymentPercentManually: () => { return this.setEmploymentPercentManually; },
                useEmploymentExperienceAsStartValue: () => { return this.useEmploymentExperienceAsStartValue; },
                payrollGroupMandatory: () => { return this.payrollGroupMandatory; },
                useAnnualLeave: () => { return this.useAnnualLeave; },
                onChangePayrollGroup: () => { return this.changePayrollGroup.bind(this); }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.employment) {
                if (result.employment.employeeGroupId)
                    this.messagingService.publish("employeeGroupChanged", { employeeId: this.employeeId, employeeGroupId: result.employment.employeeGroupId });
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid });

                const dateHasChanged = isNew || !oldDateFrom.isSameDayAs(employment.dateFrom) || (oldDateTo && employment.dateTo && !oldDateTo.isSameDayAs(employment.dateTo)) || (oldDateTo && !employment.dateTo) || (!oldDateTo && employment.dateTo);
                if (dateHasChanged)
                    this.messagingService.publish('employmentDateChanged', { newDateFrom: employment.dateFrom, newDateTo: employment.dateTo });
            }
        });
    }

    private printEmploymentContract(employment: EmploymentDTO) {
        if (!employment)
            return;

        var payrollGroupReports: IPayrollGroupReportDTO[] = _.filter(this.companyPayrollGroupReports, (e) => (e.payrollGroupId == employment.payrollGroupId || e.payrollGroupId == 0));

        //Show dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
            controller: SelectReportController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => { return null },
                reportTypes: () => { return null },
                showCopy: () => { return false },
                showEmail: () => { return false },
                copyValue: () => { return false },
                reports: () => { return payrollGroupReports },
                defaultReportId: () => { return null },
                langId: () => { return null },
                showReminder: () => { return false },
                showLangSelection: () => { return false },
                showSavePrintout: () => { return true },
                savePrintout: () => { return false }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.reportId && result.reportType) {
                this.createEmploymentContractReportJob(result.reportType, result.reportId, employment, this.selectedDate, result.savePrintout, result.employeeTemplateId);
            }
        });
    }

    private timeHibernatingAbsenceDialog(employment: EmploymentDTO) {
        if (!employment)
            return;
        //Show dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/TimeHibernatingAbsence/timeHibernatingAbsence.html"),
            controller: TimeHibernatingAbsenceController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => { return null },
                employeeId: () => { return this.employeeId },
                employmentId: () => { return employment.employmentId }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                this.messagingService.publish(Constants.EVENT_CLOSE_DIALOG, this.parentGuid);
            }
        });
    }

    private createEmploymentContractReportJob(sysReportTemplateTypeId: SoeReportTemplateType, reportId: number, employment: EmploymentDTO, date: Date, savePrintout: boolean, employeeTemplateId: number) {
        this.reportDataService.createReportJob(ReportJobDefinitionFactory.createEmploymentContractFromEmployeeReportDefinition(reportId, sysReportTemplateTypeId, employment.employeeId, employment.employmentId, date, savePrintout, employeeTemplateId), true);
    }

    private reloadEmployments() {
        if (this.onReload) {
            this.$timeout(() => {
                this.onReload({ date: this.selectedDate });
            });
        }
    }

    private initSigningDocument(userId: number, recordId: number) {
        this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AddDocumentToAttestFlow/Views/addDocumentToAttestFlow.html"),
            controller: AddDocumentToAttestFlowController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                recordId: () => { return recordId },
                endUserId: () => { return userId }
            }
        });
    }

    // ACTIONS

    private newEmployment(dateFrom?: Date, dateTo?: Date, comment?: string, copy?: EmploymentDTO, isSecondaryEmployment: boolean = false, isTemporaryPrimaryEmployment: boolean = false, hibernatingTimeDeviationCauseId?: number): EmploymentDTO {
        var employment = new EmploymentDTO();

        //Do not copy
        employment.dateFrom = dateFrom;
        employment.dateTo = dateTo;
        employment.comment = comment;
        employment.calculatedExperienceMonths = 0;
        employment.employmentEndReason = 0;
        employment.isSecondaryEmployment = isSecondaryEmployment;
        employment.isTemporaryPrimary = isTemporaryPrimaryEmployment;
        employment.isAddingEmployment = true;
        employment.isChangingEmployment = false;
        employment.isChangingEmploymentDates = false;
        employment.isDeletingEmployment = false;
        employment.isEdited = true;
        employment.isAddingEmployment = true;
        employment.isNewFromCopy = copy != null ? true : false;

        //Copy
        employment.name = copy != null ? copy.name : '';
        employment.employmentType = copy != null ? copy.employmentType : TermGroup_EmploymentType.Unknown;
        employment.workTimeWeek = copy != null ? copy.workTimeWeek : 0;
        employment.employeeGroupWorkTimeWeek = copy != null ? copy.employeeGroupWorkTimeWeek : 0;
        employment.percent = copy != null ? copy.percent : 0;
        if (copy != null) {
            if (dateFrom && dateFrom > CalendarUtility.getDateNow()) {
                employment.updateExperienceMonthsReminder = true;
            }
            this.getTotalExperienceMonthsForEmployment(copy).then(months => {
                if (copy?.dateTo && copy.dateTo.getMonth() == dateFrom.getMonth() && copy.dateTo.getFullYear() == dateFrom.getFullYear()) {
                    employment.experienceMonths = months - 1;
                } else {
                    employment.experienceMonths = months;
                }
            });
        } else {
            employment.experienceMonths = 0;
        }
        employment.experienceAgreedOrEstablished = copy != null ? copy.experienceAgreedOrEstablished : true;
        employment.workTasks = copy != null ? copy.workTasks : null;
        employment.workPlace = copy != null ? copy.workPlace : null;
        employment.specialConditions = copy != null ? copy.specialConditions : null;
        employment.state = copy != null ? copy.state : SoeEntityState.Active;
        employment.baseWorkTimeWeek = copy != null ? copy.baseWorkTimeWeek : 0;
        employment.substituteFor = copy != null ? copy.substituteFor : null;
        employment.substituteForDueTo = copy != null ? copy.substituteForDueTo : null;
        employment.externalCode = copy != null ? copy.externalCode : null;

        //Set keys
        employment.employmentId = 0;
        employment.actorCompanyId = CoreUtility.actorCompanyId;
        employment.employeeId = this.employeeId;
        employment.employeeGroupId = copy != null ? copy.employeeGroupId : this.defaultEmployeeGroupId;
        employment.payrollGroupId = copy != null ? copy.payrollGroupId : this.defaultPayrollGroupId;
        employment.hibernatingTimeDeviationCauseId = hibernatingTimeDeviationCauseId;

        //Set references
        if (copy && this.selectedEmploymentVacationGroup) {
            let vacationGroup = this.copyEmploymentVacationGroup(this.selectedEmploymentVacationGroup);
            vacationGroup.fromDate = dateFrom;
            employment.employmentVacationGroup = [];
            employment.employmentVacationGroup.push(vacationGroup);
            this.selectedEmploymentVacationGroup = vacationGroup;
        }
        employment.priceTypes = copy ? copy.priceTypes : null;
        employment.accountingSettings = copy ? copy.accountingSettings : null;

        this.selectedEmployment = employment;

        if (copy == null && employment.payrollGroupId != 0 && employment.payrollGroupId == this.defaultPayrollGroupId) {
            this.payrollGroupVacationGroups = null;
            this.changePayrollGroup(employment.payrollGroupId);
        }
        this.employments.push(employment);

        return employment;
    }

    private changePayrollGroup(payrollGroupId: number) {
        var payrollGroup: PayrollGroupSmallDTO = _.find(this.payrollGroups, g => g.payrollGroupId === payrollGroupId);
        if (payrollGroup) {
            this.selectedEmployment.payrollGroupName = payrollGroup.name;
            this.selectedEmployment.payrollGroupId = payrollGroupId;
        } else {
            this.selectedEmployment.payrollGroupName = '';
            this.selectedEmployment.payrollGroupId = 0;
        }

        // Employment vacation groups                        
        this.loadPayrollGroupVacationGroups(payrollGroupId, true, this.selectedEmployment.currentChangeDateFrom);

        // Payroll price types
        if (payrollGroup && payrollGroup.priceTypes && payrollGroup.priceTypes.length > 0)
            this.payrollGroupPriceTypes = _.filter(payrollGroup.priceTypes, p => p.showOnEmployee);
    }

    private addEmploymentVacationGroup(dto: PayrollGroupVacationGroupDTO, fromDate: Date): EmploymentVacationGroupDTO {
        var newGroup: EmploymentVacationGroupDTO = new EmploymentVacationGroupDTO();
        newGroup.employmentId = this.selectedEmployment.employmentId;
        newGroup.vacationGroupId = dto.vacationGroupId;
        newGroup.fromDate = fromDate;
        newGroup.name = dto.name;
        newGroup.calculationType = dto.calculationType;
        newGroup.vacationHandleRule = dto.vacationHandleRule;
        newGroup.vacationDaysHandleRule = dto.vacationDaysHandleRule;

        if (!this.selectedEmployment.employmentVacationGroup)
            this.selectedEmployment.employmentVacationGroup = [];
        this.selectedEmployment.employmentVacationGroup.push(newGroup);

        return newGroup;
    }

    private copyEmploymentVacationGroup(dto: EmploymentVacationGroupDTO): EmploymentVacationGroupDTO {
        var newGroup: EmploymentVacationGroupDTO = new EmploymentVacationGroupDTO();
        newGroup.vacationGroupId = dto.vacationGroupId;
        newGroup.fromDate = dto.fromDate;
        newGroup.name = dto.name;
        newGroup.calculationType = dto.calculationType;
        newGroup.vacationHandleRule = dto.vacationHandleRule;
        newGroup.vacationDaysHandleRule = dto.vacationDaysHandleRule;

        return newGroup;
    }

    private closeCurrentVacationGroup(changeDate?: Date) {
        if (!changeDate)
            return;

        _.forEach(this.selectedEmployment.employmentVacationGroup, group => {
            if (!group.fromDate)
                group.fromDate = changeDate.addDays(-1);
        });
    }

    // HELP-METHODS

    private setPayrollGroupHasReports() {
        _.forEach(this.employments, employment => {
            employment['payrollGroupHasReports'] = _.filter(this.companyPayrollGroupReports, p => employment.payrollGroupId && p.payrollGroupId == employment.payrollGroupId || p.payrollGroupId == 0).length > 0;
        });
    }

    private getEmploymentVacationGroups(employmentId: number) {
        var employmentVacationGroups = [];
        _.forEach(this.employments, employment => {
            if (employment.employmentVacationGroup && employment.employmentId == employmentId) {
                _.forEach(employment.employmentVacationGroup, employmentVacationGroup => {
                    employmentVacationGroups.push(employmentVacationGroup);
                });
            }
        });
        return _.orderBy(employmentVacationGroups, ['sortableDate'], ['desc']);
    }

    private getCurrentVacationGroup(employment: EmploymentDTO): EmploymentVacationGroupDTO {
        var res: EmploymentVacationGroupDTO = null;
        var date = this.selectedDate;
        if (this.selectedDate) {
            if (date < employment.dateFrom)
                date = employment.dateFrom;
            else if (employment.dateTo && date > employment.dateTo)
                date = employment.dateTo;
            var employmentVacationGroups = this.getEmploymentVacationGroups(employment.employmentId);
            _.forEach(employmentVacationGroups, employmentVacationGroup => {
                if (!res && employmentVacationGroup.fromDate <= date)
                    res = employmentVacationGroup;
            });
        }
        return res;
    }

    private getCurrentVacationGroupId(employment: EmploymentDTO): number {
        var group = this.getCurrentVacationGroup(employment);
        return group ? group.vacationGroupId : 0;
    }

    private getCurrentVacationGroupName(employment: EmploymentDTO): string {
        var group = this.getCurrentVacationGroup(employment);
        return group ? group.name : '';
    }

    private getExperienceAgreedOrEstablishedName(employment: EmploymentDTO): string {
        if (employment.experienceAgreedOrEstablished)
            return this.terms["time.employee.employment.experience.agreed"];
        else
            return this.terms["time.employee.employment.experience.established"];
    }

    private setExperienceMonthsText(employment: EmploymentDTO) {
        if (employment.experienceMonthsText)
            return;

        let yearTerms = { one: this.terms["common.year"].toLowerCase(), plural: this.terms["common.year"].toLowerCase() }
        let monthsTerms = { one: this.terms["core.time.month"].toLowerCase(), plural: this.terms["core.time.months"].toLowerCase() }

        if (this.useEmploymentExperienceAsStartValue) {
            this.getTotalExperienceMonthsForEmployment(employment).then(months => {
                if (months > 0)
                    employment.experienceMonthsText = StringUtility.getYearMonthString(months, yearTerms, monthsTerms) + " / " + this.getExperienceAgreedOrEstablishedName(employment);
            });
        } else if (employment.experienceMonths > 0) {
            employment.experienceMonthsText = StringUtility.getYearMonthString(employment.experienceMonths, yearTerms, monthsTerms) + " / " + this.getExperienceAgreedOrEstablishedName(employment);
        }
    }

    private getTotalExperienceMonthsForEmployment(employment: EmploymentDTO): ng.IPromise<number> {
        let deferral = this.$q.defer<number>();

        if (this.useEmploymentExperienceAsStartValue) {
            this.employeeService.getTotalExperienceMonthsForEmployment(employment.employmentId, null).then(result => {
                deferral.resolve(result);
            });
        } else {
            deferral.resolve(employment.experienceMonths);
        }

        return deferral.promise;
    }

    private showHistoryInformation(employmentChange: EmploymentChangeDTO) {
        var noValue: string = this.terms["time.employee.employment.history.message.novalue"];

        var message: string = this.terms["time.employee.employment.history.message"].format(employmentChange.createdBy,
            employmentChange.fieldTypeName,
            (employmentChange.fromValueName ? employmentChange.fromValueName : noValue),
            (employmentChange.toValueName ? employmentChange.toValueName : noValue),
            (employmentChange.created ? employmentChange.created.toFormattedDateTime() : ''));

        if (employmentChange.comment)
            message += "\r\n\r\n{0}:\r\n{1}".format(this.terms["common.comment"], employmentChange.comment);

        this.notificationService.showDialogEx(this.terms["core.info"], message, SOEMessageBoxImage.Information);
    }

    private filteredEmployments() {
        return _.orderBy(_.filter(this.employments, e => !e.isDeleted || this.showDeleted), ['dateFrom', 'dateTo', 'isDeleted'], ['desc', 'desc', 'asc']);
    }

    private updatePreviousEmployment(dateTo: Date, finalSalary: boolean) {
        var previousEmployment = this.getPreviousEmployment(dateTo);
        if (previousEmployment) {
            previousEmployment.dateTo = (dateTo.addDays(-1));
            previousEmployment.currentChangeDateTo = previousEmployment.dateTo;
            if (finalSalary)
                previousEmployment.finalSalaryStatus = SoeEmploymentFinalSalaryStatus.ApplyFinalSalary;
        }
    }

    private getLatestEmployment(): EmploymentDTO {
        var employment = _.head(_.filter(this.employments, (e) => (e.state == SoeEntityState.Active && !e.dateTo)));
        if (!employment)
            employment = _.head(_.orderBy(_.filter(this.employments, (e) => (e.state == SoeEntityState.Active && e.dateTo)), 'dateTo', 'desc'));
        return employment;
    }

    private getPreviousEmployment(date: Date): EmploymentDTO {
        var employment = _.head(_.filter(this.employments, (e) => (e.state == SoeEntityState.Active && !e.dateTo && (!e.dateFrom || e.dateFrom < date))));
        if (!employment) {
            employment = _.head(_.orderBy(_.filter(this.employments, (e) => (e.state == SoeEntityState.Active && e.dateTo && e.dateFrom && e.dateFrom <= date)), 'dateFrom', 'desc'));
        }
        return employment;
    }

    private validateShortenEmployment(oldDateFrom: Date, oldDateTo: Date, newDateFrom: Date, newDateTo: Date, applyFinalSalary: boolean, changedEmployment?: EmploymentDTO, employments?: EmploymentDTO[]): ng.IPromise<boolean> {
        return this.employeeService.validateShortenEmployment(this.employeeId, oldDateFrom, oldDateTo, newDateFrom, newDateTo, applyFinalSalary, changedEmployment, employments).then(result => {
            return this.notificationService.showValidateShortenEmploymentResult(result);
        });
    }

    private getAvailableEmploymentVacationGroups(excludeVacationGroupId: number): PayrollGroupVacationGroupDTO[] {
        var availableGroups: PayrollGroupVacationGroupDTO[] = [];

        _.forEach(this.payrollGroupVacationGroups, group => {
            if (_.filter(this.selectedEmployment.employmentVacationGroup, v => v.vacationGroupId !== excludeVacationGroupId && v.vacationGroupId === group.vacationGroupId).length === 0)
                availableGroups.push(group);
        });

        return availableGroups;
    }
}