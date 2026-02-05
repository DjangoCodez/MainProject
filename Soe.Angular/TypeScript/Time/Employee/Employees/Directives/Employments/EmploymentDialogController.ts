import { EmploymentDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { ISmallGenericType, IEmployeeGroupSmallDTO, IEmploymentTypeSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup_Languages, TermGroup_EmploymentType, TermGroup, TermGroup_ExcludeFromWorkTimeWeekCalculationItems } from "../../../../../Util/CommonEnumerations";
import { EmployeeService } from "../../../EmployeeService";
import { PayrollGroupSmallDTO, PayrollGroupVacationGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { PayrollService } from "../../../../Payroll/PayrollService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { CoreService } from "../../../../../Core/Services/CoreService";

export class EmploymentDialogController {

    // Terms
    private terms: { [index: string]: string; };
    private employeeGroupLabel: string;
    private selectedEmployeeGroupLabel: string;
    private percentLabel: string = "%";
    private monthsLabel: string;

    // Data
    private employmentTypes: IEmploymentTypeSmallDTO[] = [];
    private excludeFromWorkTimeWeekCalculationItems: ISmallGenericType[] = [];
    private allEmploymentTypes: IEmploymentTypeSmallDTO[] = [];
    private employmentEndReasons: ISmallGenericType[] = [];
    private experiences: any[] = [];
    private totalExperienceMonths: number;
    private calculatedExperienceMonths: number;
    private fetchingTotalExperienceMonths: boolean = false;
    private deviantFullTimeWorkTime: boolean = false;
    private selectedRuleWorkTimeWeek: number;
    private temporaryStateFullTimeWorkTime: number = 0;

    // Properties
    private _selectedEmploymentType: ISmallGenericType;
    private get selectedEmploymentType(): ISmallGenericType {
        return this._selectedEmploymentType;
    }
    private set selectedEmploymentType(type: ISmallGenericType) {
        this._selectedEmploymentType = type;

        this.employment.employmentType = type ? <TermGroup_EmploymentType>type.id : TermGroup_EmploymentType.Unknown;
        this.employment.employmentTypeName = type ? type.name : '';
    }

    private _excludeFromWorkTimeWeekCalculationOnSecondaryEmployment?: TermGroup_ExcludeFromWorkTimeWeekCalculationItems;
    private get excludeFromWorkTimeWeekCalculationOnSecondaryEmployment(): TermGroup_ExcludeFromWorkTimeWeekCalculationItems {
        return this._excludeFromWorkTimeWeekCalculationOnSecondaryEmployment;
    }
    private set excludeFromWorkTimeWeekCalculationOnSecondaryEmployment(exclude: TermGroup_ExcludeFromWorkTimeWeekCalculationItems) {
        this._excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = exclude;

        switch (exclude) {
            case TermGroup_ExcludeFromWorkTimeWeekCalculationItems.UseSettingOnEmploymentType:
                this.employment.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = null;
                break;
            case TermGroup_ExcludeFromWorkTimeWeekCalculationItems.No:
                this.employment.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = false;
                break;
            case TermGroup_ExcludeFromWorkTimeWeekCalculationItems.Yes:
                this.employment.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = true;
                break;
        }
    }

    private _selectedEndReason: SmallGenericType;
    private get selectedEndReason(): SmallGenericType {
        return this._selectedEndReason;
    }
    private set selectedEndReason(reason: SmallGenericType) {
        this._selectedEndReason = reason;

        this.employment.employmentEndReason = reason ? reason.id : 0;
        this.employment.employmentEndReasonName = reason ? reason.name : '';
    }

    private _selectedPayrollGroup: PayrollGroupSmallDTO;
    private get selectedPayrollGroup(): PayrollGroupSmallDTO {
        return this._selectedPayrollGroup;
    }
    private set selectedPayrollGroup(group: PayrollGroupSmallDTO) {
        if (this.employment.employmentId && this.employment.payrollGroupId && this.employment.payrollGroupId !== group.payrollGroupId) {
            this.initPayrollGroupChanged(group.payrollGroupId).then((showPriceTypeWarning: boolean) => {
                if (showPriceTypeWarning) {
                    var modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["time.employee.employment.changepayrollgroup.warning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        this._selectedPayrollGroup = group;
                        this.onChangePayrollGroup(group ? group.payrollGroupId : 0);
                    }, (cancel) => { });
                } else {
                    this._selectedPayrollGroup = group;
                    this.onChangePayrollGroup(group ? group.payrollGroupId : 0);
                }
            });
        } else {
            this._selectedPayrollGroup = group;
            this.onChangePayrollGroup(group ? group.payrollGroupId : 0);
        }
    }

    private _selectedEmployeeGroup: IEmployeeGroupSmallDTO;
    private get selectedEmployeeGroup(): IEmployeeGroupSmallDTO {
        return this._selectedEmployeeGroup;
    }
    private set selectedEmployeeGroup(group: IEmployeeGroupSmallDTO) {
        this._selectedEmployeeGroup = group;
        this.employment.employeeGroupId = group ? group.employeeGroupId : 0;
        this.employment.employeeGroupName = group ? group.name : '';
        this.employment.currentEmployeeGroup = group;
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: TranslationService,
        private notificationService: INotificationService,
        private coreService: CoreService,
        private employeeService: EmployeeService,
        private payrollService: PayrollService,
        private payrollGroups: PayrollGroupSmallDTO[],
        private employeeGroups: IEmployeeGroupSmallDTO[],
        private vacationGroups: PayrollGroupVacationGroupDTO[],
        private vacationGroupId: number,
        private annualLeaveGroups: ISmallGenericType[],
        private employment: EmploymentDTO,
        private isNew: boolean,
        private hasPreviousEmployment: boolean,
        private showExternalCode: boolean,
        private setEmploymentPercentManually: boolean,
        private useEmploymentExperienceAsStartValue: boolean,
        private payrollGroupMandatory: boolean,
        private useAnnualLeave: boolean,
        private onChangePayrollGroup: Function) {
    }

    // SETUP

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadEmploymentTypes(),
            this.loadExcludeFromWorkTimeWeekCalculationItems(),
            this.loadEmploymentEndReasons()]).then(() => {
                this.setupExperiences();
                this.setup();
                this.setupDeviantFullTimeWorkTime();
                this.setEmployeeWorkTimeWeekLabels();
            });
    }

    private setupDeviantFullTimeWorkTime() {
        if (this.employment.fullTimeWorkTimeWeek > 0)
            this.deviantFullTimeWorkTime = true;
    }

    private setupExperiences() {
        this.experiences = [];
        this.experiences.push({ id: true, name: this.terms["time.employee.employment.experience.agreed"] });
        this.experiences.push({ id: false, name: this.terms["time.employee.employment.experience.established"] });
    }

    private setup() {
        if (this.employment) {
            this.selectedEmploymentType = _.find(this.allEmploymentTypes, t => t.id === this.employment.employmentType);
            if (!this.employmentTypes.find(f => f.id == this.selectedEmploymentType.id)) {
                if (this.isNew) 
                    this.selectedEmploymentType = _.find(this.allEmploymentTypes, t => t.id === TermGroup_EmploymentType.Unknown);
                else
                    this.employmentTypes.push(this.allEmploymentTypes.find(f => f.id == this.selectedEmploymentType.id));
            }

            if (this.employment.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment === true)
                this.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = TermGroup_ExcludeFromWorkTimeWeekCalculationItems.Yes;
            else if (this.employment.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment === false)
                this.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = TermGroup_ExcludeFromWorkTimeWeekCalculationItems.No;
            else
                this.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment = TermGroup_ExcludeFromWorkTimeWeekCalculationItems.UseSettingOnEmploymentType;

            this.selectedEndReason = _.find(this.employmentEndReasons, r => r.id === this.employment.employmentEndReason);
            this.selectedEmployeeGroup = _.find(this.employeeGroups, g => g.employeeGroupId === this.employment.employeeGroupId);
            if (!this.selectedEmployeeGroup && this.employeeGroups && this.employeeGroups.length > 0)
                this.selectedEmployeeGroup = this.employeeGroups[0];
                
            this.selectedPayrollGroup = _.find(this.payrollGroups, g => g.payrollGroupId === this.employment.payrollGroupId);
        }

        if (this.isNew) {
            if (this.selectedEmployeeGroup) {
                if (!this.employment.workTimeWeek || this.employment.workTimeWeek === 0) {
                    this.employment.workTimeWeek = this.selectedEmployeeGroup.ruleWorkTimeWeek;
                    this.workTimeWeekChanged();
                }
            }
            this.getExperienceMonthsForEmployee();
        }
        else {
            this.getTotalExperienceMonthsForEmployment();
        }

    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.time.months",
            "core.warning",
            "time.employee.employeesfulltimeweek",
            "time.employee.employeegroup.employeegroup",
            "time.employee.employment.changepayrollgroup.warning",
            "time.employee.employment.experience.agreed",
            "time.employee.employment.experience.established",
            "time.employee.employment.updateexperiencemonths.question",

        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.employeeGroupLabel = this.terms["time.employee.employeegroup.employeegroup"];
        });
    }

    private loadEmploymentTypes(): ng.IPromise<any> {
        return this.employeeService.getEmploymentEmploymentTypes(CoreUtility.languageId).then(x => {
            this.allEmploymentTypes = x;
            this.employmentTypes = this.allEmploymentTypes.filter(f => f.active);
        });
    }

    private loadExcludeFromWorkTimeWeekCalculationItems(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ExcludeFromWorkTimeWeekCalculationItems,false,true,true).then(x => {
            this.excludeFromWorkTimeWeekCalculationItems = x;
        });
    }

    private loadEmploymentEndReasons(): ng.IPromise<any> {
        return this.employeeService.getEmploymentEndReasons(CoreUtility.languageId).then(x => {
            this.employmentEndReasons = x;
        });
    }

    //Gets called at setup (existing employment)
    private getTotalExperienceMonthsForEmployment(): number {

        if (!this.fetchingTotalExperienceMonths && !this.totalExperienceMonths) {
            this.fetchingTotalExperienceMonths = true;
            if (this.useEmploymentExperienceAsStartValue) {
                this.employeeService.getTotalExperienceMonthsForEmployment(this.employment.employmentId, null).then(result => {
                    this.totalExperienceMonths = result;
                    this.calculatedExperienceMonths = result > this.employment.experienceMonths ? result - this.employment.experienceMonths : 0;                    
                });
            }
            else
                this.totalExperienceMonths = this.employment.experienceMonths;
            this.fetchingTotalExperienceMonths = false;
        }

        return this.totalExperienceMonths
    }
    //Gets called at setup (new employment)
    private getExperienceMonthsForEmployee(): number {

        if (!this.fetchingTotalExperienceMonths && !this.totalExperienceMonths) {
            this.fetchingTotalExperienceMonths = true;
            if (this.useEmploymentExperienceAsStartValue) {
                this.employeeService.getExperienceMonthsForEmployee(this.employment.employeeId, this.employment.dateFrom).then(result => {
                    this.totalExperienceMonths = result;                    
                    this.employment.experienceMonths = result;
                    this.calculatedExperienceMonths = 0;                    
                });
            }
            else
                this.totalExperienceMonths = this.employment.experienceMonths;
            this.fetchingTotalExperienceMonths = false;
        }

        return this.totalExperienceMonths
    }

    private getTotalExperienceMonthFromPreviousEmployment() {
        if (this.useEmploymentExperienceAsStartValue) {         
            this.employeeService.getTotalExperienceMonthFromPreviousEmployment(this.employment.employmentId).then(result => {
                var modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["time.employee.employment.updateexperiencemonths.question"].format(result.toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.employment.experienceMonths = result;
                    this.totalExperienceMonths = this.employment.experienceMonths + this.calculatedExperienceMonths;
                    
                }, (cancel) => { });
            });
        }
    }

    // EVENTS

    private initPayrollGroupChanged(payrollGroupId: number): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        // Check if selected payroll group includes all existing price types,
        // otherwise show a warning message where the change can be cancelled.
        // Only do the check if there are any existing price types
        var currentPriceTypeIds: number[] = _.map(this.employment.priceTypes, p => p.payrollPriceTypeId);
        if (currentPriceTypeIds.length === 0) {
            deferral.resolve(false);
        } else {
            if (!payrollGroupId) {
                // User cleared the payroll group, always show warning in this case
                deferral.resolve(true);
            } else {
                this.payrollService.priceTypesExistsInPayrollGroup(payrollGroupId, currentPriceTypeIds).then((allExists: boolean) => {
                    deferral.resolve(!allExists);
                });
            }
        }

        return deferral.promise;
    }

    private employeeGroupChanged(item) {
        this.$timeout(() => {
            this.resetWorkTimeWeek();
        });
    }

    private workTimeWeekChanged() {
        this.$timeout(() => {
            this.calculateEmploymentPercent();
            this.setEmployeeWorkTimeWeekLabels();
        });
    }

    private handleFullTimeWorkTime() {
        this.$timeout(() => {
            if (this.deviantFullTimeWorkTime) {
                this.employment.fullTimeWorkTimeWeek = this.temporaryStateFullTimeWorkTime;
            } else {
                this.temporaryStateFullTimeWorkTime = this.employment.fullTimeWorkTimeWeek;
                this.employment.fullTimeWorkTimeWeek = 0;
            }
            this.setEmployeeWorkTimeWeekLabels();
            if (this.employment.fullTimeWorkTimeWeek > 0)
                this.calculateEmploymentPercent();
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ employment: this.employment });
    }

    // HELP-METHODS

    private resetWorkTimeWeek() {
        if (this.employment && this.employment.currentEmployeeGroup && !this.setEmploymentPercentManually && this.employment.percent == 100) {
            this.employment.workTimeWeek = this.employment.currentEmployeeGroup.ruleWorkTimeWeek;
        }
    }

    private calculateEmploymentPercent() {
        if (this.employment && this.employment.currentEmployeeGroup && !this.setEmploymentPercentManually) {
            if (this.employment.fullTimeWorkTimeWeek > 0)
                this.employment.percent = this.employment.workTimeWeek > 0 ? (100 * (this.employment.workTimeWeek / this.employment.fullTimeWorkTimeWeek)).round(2) : 0;
            else
                this.employment.percent = this.employment.workTimeWeek > 0 ? (100 * (this.employment.workTimeWeek / this.employment.currentEmployeeGroup.ruleWorkTimeWeek)).round(2) : 0;
        }
    }

    private showStartvaluemonthsinstruction() {        
        return (this.hasPreviousEmployment && this.employment && this.employment.dateFrom > CalendarUtility.getFirstDayOfMonth(this.employment.dateFrom));
    }

    private setEmployeeWorkTimeWeekLabels() {
        this.selectedEmployeeGroupLabel = this.employment.fullTimeWorkTimeWeek > 0 ? this.terms["time.employee.employeesfulltimeweek"] : this.employeeGroupLabel;
        this.selectedRuleWorkTimeWeek = this.employment.fullTimeWorkTimeWeek > 0 ? this.employment.fullTimeWorkTimeWeek : this.selectedEmployeeGroup.ruleWorkTimeWeek;
    }
}
