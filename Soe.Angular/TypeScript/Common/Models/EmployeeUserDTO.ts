import { IEmployeeUserDTO, IEmployeeChildDTO, IEmployeeSkillDTO, IEmployeeVacationSEDTO, IEmploymentDTO, IEmployeeFactorDTO, IEmployeeChildCareDTO, IEmployeeMeetingDTO, IEmployeeUnionFeeDTO, IEmploymentVacationGroupDTO, IEmploymentPriceTypeDTO, IEmploymentChangeDTO, IEmployeeTaxSEDTO, IEmploymentPriceTypePeriodDTO, IAccountingSettingsRowDTO, IUserRolesDTO, IUserAttestRoleDTO, IDeleteEmployeeDTO, ICompanyRolesDTO, ICompanyAttestRoleDTO, IDeleteUserDTO, IEmployeeAccountDTO, IUserCompanyRoleDTO, IEmployeeCalculatedCostDTO, ICreateVacantEmployeeDTO, IEmployeeGridDTO, IInactivateEmployeeDTO, IEmployeeCSRExportDTO, IExtraFieldRecordDTO, ITimeWorkAccountDTO, ITimeWorkAccountYearDTO, ITimeWorkAccountYearEmployeeDTO, IEmployeeTimeWorkAccountDTO, IDateRangeDTO, ITimeWorkAccountWorkTimeWeekDTO, IEmployeeSettingDTO, IEmployeeSettingTypeDTO, ISmallGenericType, IValidatePossibleDeleteOfEmployeeAccountDTO, IValidatePossibleDeleteOfEmployeeAccountRowDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TermGroup_EmployeeDisbursementMethod, TermGroup_Sex, SoeEntityState, TermGroup_EmploymentType, SoeEmploymentFinalSalaryStatus, TermGroup_EmploymentChangeFieldType, TermGroup_EmploymentChangeType, TermGroup_EmployeeTaxAdjustmentType, TermGroup_EmployeeTaxEmploymentAbroadCode, TermGroup_EmployeeTaxEmploymentTaxType, TermGroup_EmployeeTaxSalaryDistressAmountType, TermGroup_EmployeeTaxSinkType, TermGroup_EmployeeTaxType, TermGroup_VacationGroupCalculationType, TermGroup_VacationGroupVacationDaysHandleRule, TermGroup_VacationGroupVacationHandleRule, TermGroup_SoePayrollPriceType, TermGroup_EmployeeFactorType, DeleteEmployeeAction, DeleteUserAction, TermGroup_AttestRoleUserAccountPermissionType, TermGroup_TimeWorkAccountWithdrawalMethod, TermGroup_TimeWorkAccountYearEmployeeStatus, SettingDataType, TermGroup_EmployeeSettingType } from "../../Util/CommonEnumerations";
import { PayrollPriceTypeDTO } from "./PayrollPriceTypeDTOs";
import { CategoryDTO, CompanyCategoryRecordDTO } from "./Category";
import { TimeScheduleTemplateGroupEmployeeDTO } from "./TimeScheduleTemplateDTOs";
import { EmployeeChildCareDTO, EmployeeChildDTO } from "./EmployeeChildDTOs";
import { EmployeeVacationSEDTO } from "./EmployeeVacationDTOs";
import { NumberUtility } from "../../Util/NumberUtility";

export class EmployeeUserDTO implements IEmployeeUserDTO {
    extraFieldRecords: IExtraFieldRecordDTO[];
    tempTaxRate: number;
    absence105DaysExcluded: boolean;
    absence105DaysExcludedDays: number;
    accounts: IEmployeeAccountDTO[];
    actorCompanyId: number;
    actorContactPersonId: number;
    aFACategory: number;
    aFAParttimePensionCode: boolean;
    aFASpecialAgreement: number;
    aFAWorkplaceNr: string;
    aGIPlaceOfEmploymentAddress: string;
    aGIPlaceOfEmploymentCity: string;
    aGIPlaceOfEmploymentIgnore: boolean;
    attestRoleIds: number[];
    attestRoles: IUserAttestRoleDTO[];
    benefitAsPension: boolean;
    blockedFromDate: Date;
    bygglosenAgreementArea: string;
    bygglosenAllocationNumber: string;
    bygglosenMunicipalCode: string;
    bygglosenSalaryFormula: number;
    bygglosenSalaryFormulaName: string;
    bygglosenWorkPlace: string;
    bygglosenProfessionCategory: string;
    bygglosenLendedToOrgNr: string;
    bygglosenAgreedHourlyPayLevel: number;
    bygglosenSalaryType: number;
    calculatedCosts: IEmployeeCalculatedCostDTO[];
    cardNumber: string;
    categoryId: number;
    categoryRecords: CompanyCategoryRecordDTO[];
    changePassword: boolean;
    childCares: IEmployeeChildCareDTO[];
    collectumAgreedOnProduct: string;
    collectumCancellationDate: Date;
    collectumCancellationDateIsLeaveOfAbsence: boolean;
    collectumCostPlace: string;
    collectumITPPlan: number;
    clearScheduleFrom: Date;
    created: Date;
    createdBy: string;
    currentEmployeeGroupId: number;
    defaultActorCompanyId: number;
    disbursementAccountNr: string;
    disbursementClearingNr: string;
    disbursementMethod: TermGroup_EmployeeDisbursementMethod;
    disbursementCountryCode: string;
    disbursementBIC: string;
    disbursementIBAN: string;
    disconnectExistingEmployee: boolean;
    disconnectExistingUser: boolean;
    dontNotifyChangeOfAttestState: boolean;
    dontNotifyChangeOfDeviations: boolean;
    dontValidateDisbursementAccountNr: boolean;
    email: string;
    emailCopy: boolean;
    employeeChilds: IEmployeeChildDTO[];
    employeeId: number;
    employeeMeetings: EmployeeMeetingDTO[];
    employeeNr: string;
    employeeNrAndName: string;
    employeeSettings: EmployeeSettingDTO[];
    employeeSkills: EmployeeSkillDTO[];
    employeeTemplateId: number;
    employeeTemplateName: string;
    employeeVacationSE: IEmployeeVacationSEDTO;
    employmentDate: Date;
    employments: EmploymentDTO[];
    endDate: Date;
    estatusLoginId: string;
    externalAuthId: string;
    externalAuthIdModified: boolean;
    externalCode: string;
    factors: EmployeeFactorDTO[];
    firstName: string;
    found: boolean;
    gtpAgreementNumber: number;
    gtpExcluded: boolean;
    hasNotAttestRoleToSeeEmployee: boolean;
    highRiskProtection: boolean;
    highRiskProtectionTo: Date;
    iFAssociationNumber: number;
    iFPaymentCode: number;
    iFWorkPlace: string;
    isEmployeeMeetingsChanged: boolean;
    isEmploymentsChanged: boolean;
    isMobileUser: boolean;
    isPayrollUpdated: boolean;
    isTemplateGroupsChanged: boolean;
    kpaAgreementType: number;
    kpaBelonging: number;
    kpaEndCode: number;
    kpaRetirementAge: number;
    langId: number;
    lastName: string;
    licenseId: number;
    lifetimeSeconds: number;
    lifetimeSecondsModified: boolean;
    loginName: string;
    medicalCertificateDays: number;
    medicalCertificateReminder: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    newPassword: string;
    note: string;
    parentalLeaves: IEmployeeChildDTO[];
    partnerInCloseCompany: boolean;
    password: number[];
    passwordHomePage: string;
    payrollReportsCFARNumber: number;
    payrollReportsPersonalCategory: number;
    payrollReportsSalaryType: number;
    payrollReportsWorkPlaceNumber: number;
    payrollReportsWorkTimeCategory: number;
    portraitConsent: boolean;
    portraitConsentDate: Date;
    saveEmployee: boolean;
    saveUser: boolean;
    sex: TermGroup_Sex;
    showNote: boolean;
    socialSec: string;
    state: SoeEntityState;
    templateGroups: TimeScheduleTemplateGroupEmployeeDTO[];
    timeCodeId: number;
    timeDeviationCauseId: number;
    unionFees: IEmployeeUnionFeeDTO[];
    useFlexForce: boolean;
    userId: number;
    userLinkConnectionKey: string;
    userRoles: IUserRolesDTO[];
    vacant: boolean;
    wantsExtraShifts: boolean;
    workPlaceSCB: string;
    excludeFromPayroll: boolean;
    employeeTimeWorkAccounts: EmployeeTimeWorkAccountDTO[];

    public get active(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set active(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public get numberAndName(): string {
        return "({0}) {1} {2}".format(this.employeeNr, this.firstName, this.lastName);
    }

    public fixDates() {
        this.portraitConsentDate = CalendarUtility.convertToDate(this.portraitConsentDate);
        this.collectumCancellationDate = CalendarUtility.convertToDate(this.collectumCancellationDate);
        this.employmentDate = CalendarUtility.convertToDate(this.employmentDate);
        this.endDate = CalendarUtility.convertToDate(this.endDate);
        this.highRiskProtectionTo = CalendarUtility.convertToDate(this.highRiskProtectionTo);
        this.blockedFromDate = CalendarUtility.convertToDate(this.blockedFromDate);
        if (this.employments) {
            this.employments = this.employments.map(e => {
                var eObj = new EmploymentDTO();
                angular.extend(eObj, e);
                eObj.fixDates();
                eObj.setTypes();
                return eObj;
            });
        }
    }

    public setTypes() {
        if (this.employments) {
            this.employments = this.employments.map(e => {
                var eObj = new EmploymentDTO();
                angular.extend(eObj, e);
                eObj.fixDates();
                eObj.setTypes();
                return eObj;
            });
        }

        if (this.accounts) {
            this.accounts = this.accounts.map(a => {
                let aObj = new EmployeeAccountDTO();
                angular.extend(aObj, a);
                aObj.fixDates();

                if (a.children) {
                    aObj.children = aObj.children.map(c => {
                        let cObj = new EmployeeAccountDTO();
                        angular.extend(cObj, c);
                        cObj.fixDates();
                        return cObj;
                    });
                }

                return aObj;
            });
        }

        if (this.employeeChilds) {
            this.employeeChilds = this.employeeChilds.map(c => {
                var cObj = new EmployeeChildDTO();
                angular.extend(cObj, c);
                cObj.fixDates();
                return cObj;
            });
        }

        if (this.childCares) {
            this.childCares = this.childCares.map(c => {
                var cObj = new EmployeeChildCareDTO();
                angular.extend(cObj, c);
                return cObj;
            });
        }

        if (this.factors) {
            this.factors = this.factors.map(f => {
                var fObj = new EmployeeFactorDTO();
                angular.extend(fObj, f);
                fObj.fixDates();
                return fObj;
            });
        }

        if (this.employeeVacationSE) {
            var vacation = new EmployeeVacationSEDTO();
            angular.extend(vacation, this.employeeVacationSE);
            vacation.fixDates();
            this.employeeVacationSE = vacation;
        }

        if (this.categoryRecords) {
            this.categoryRecords = this.categoryRecords.map(c => {
                var cObj = new CompanyCategoryRecordDTO();
                angular.extend(cObj, c);
                cObj.fixDates();

                let cat: CategoryDTO = new CategoryDTO();
                angular.extend(cat, cObj.category);
                cObj.category = cat;

                return cObj;
            });
        }

        if (this.templateGroups) {
            this.templateGroups = this.templateGroups.map(t => {
                var tObj = new TimeScheduleTemplateGroupEmployeeDTO();
                angular.extend(tObj, t);
                tObj.fixDates();
                tObj.setTypes();
                return tObj;
            });
        }

        if (this.employeeMeetings) {
            this.employeeMeetings = this.employeeMeetings.map(m => {
                var mObj = new EmployeeMeetingDTO();
                angular.extend(mObj, m);
                mObj.fixDates();
                return mObj;
            });
        }

        if (this.employeeSkills) {
            this.employeeSkills = this.employeeSkills.map(s => {
                var sObj = new EmployeeSkillDTO();
                angular.extend(sObj, s);
                sObj.fixDates();
                return sObj;
            });
        }

        if (this.employeeSettings) {
            this.employeeSettings = this.employeeSettings.map(s => {
                var sObj = new EmployeeSettingDTO();
                angular.extend(sObj, s);
                sObj.fixDates();
                return sObj;
            });
        }

    }
}

export class EmployeeGridDTO implements IEmployeeGridDTO {
    accountNamesString: string;
    age: number;
    categoryNamesString: string;
    currentVacationGroupName: string;
    employeeGroupNamesString: string;
    employeeId: number;
    employeeNr: string;
    employmentEndDate: Date;
    employmentStart: Date;
    employmentStop: Date;
    employmentTypeString: string;
    name: string;
    payrollGroupNamesString: string;
    percent: number;
    roleNamesString: string;
    sex: TermGroup_Sex;
    sexString: string;
    socialSec: string;
    state: SoeEntityState;
    userBlockedFromDate: Date;
    vacant: boolean;
    workTimeWeek: number;
    fullTimeWorkTimeWeek: number;

    public fixDates() {
        this.employmentEndDate = CalendarUtility.convertToDate(this.employmentEndDate);
        this.employmentStart = CalendarUtility.convertToDate(this.employmentStart);
        this.employmentStop = CalendarUtility.convertToDate(this.employmentStop);
        this.userBlockedFromDate = CalendarUtility.convertToDate(this.userBlockedFromDate);
    }

    // Extensions
    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }

    get workTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.workTimeWeek);
    }
    set workTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.workTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }

    get fullTimeWorkTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.fullTimeWorkTimeWeek);
    }
    set fullTimeWorkTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.fullTimeWorkTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }
}

export class InactivateEmployeeDTO implements IInactivateEmployeeDTO {
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    message: string;
    success: boolean;

    // Extensions
    selected: boolean;
}

export class CreateVacantEmployeeDTO implements ICreateVacantEmployeeDTO {
    accounts: EmployeeAccountDTO[];
    categories: CompanyCategoryRecordDTO[];
    employeeGroupId: number;
    employeeNr: string;
    employmentDateFrom: Date;
    firstName: string;
    lastName: string;
    percent: number;
    workTimeWeek: number;

    // Extensions
    employeeGroupName: string;
    accountNames: string;
    categoryNames: string;

    get name(): string {
        return this.firstName + ' ' + this.lastName;
    }

    get workTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.workTimeWeek);
    }
    set workTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.workTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }
}

export class EmployeeAccountDTO implements IEmployeeAccountDTO {
    accountId: number;
    addedOtherEmployeeAccount: boolean;
    children: EmployeeAccountDTO[];
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    default: boolean;
    employeeAccountId: number;
    employeeId: number;
    mainAllocation: boolean;
    modified: Date;
    modifiedBy: string;
    parentEmployeeAccountId: number;
    state: SoeEntityState;

    // Extensions
    accountDimId: number;
    accountDimName: string;
    accountName: string;
    accountNumberName: string;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class ValidatePossibleDeleteOfEmployeeAccountDTO implements IValidatePossibleDeleteOfEmployeeAccountDTO {
    employeeId: number;
    rows: ValidatePossibleDeleteOfEmployeeAccountRowDTO[];
}

export class ValidatePossibleDeleteOfEmployeeAccountRowDTO implements IValidatePossibleDeleteOfEmployeeAccountRowDTO {
    dateFrom: Date;
    dateTo: Date;
    employeeAccountId: number;
}

export class DeleteEmployeeDTO implements IDeleteEmployeeDTO {
    action: DeleteEmployeeAction;
    employeeId: number;
    removeInfoAbsenceParentalLeave: boolean;
    removeInfoAbsenceSick: boolean;
    removeInfoAddress: boolean;
    removeInfoBankAccount: boolean;
    removeInfoClosestRelative: boolean;
    removeInfoEmail: boolean;
    removeInfoImage: boolean;
    removeInfoMeeting: boolean;
    removeInfoNote: boolean;
    removeInfoOtherContactInfo: boolean;
    removeInfoPhone: boolean;
    removeInfoSalaryDistress: boolean;
    removeInfoSkill: boolean;
    removeInfoUnionFee: boolean;

    constructor(employeeId: number, action: DeleteEmployeeAction) {
        this.employeeId = employeeId;
        this.action = action;
    }
}

export class DeleteUserDTO implements IDeleteUserDTO {
    action: DeleteUserAction;
    disconnectEmployee: boolean;
    removeInfoAddress: boolean;
    removeInfoClosestRelative: boolean;
    removeInfoEmail: boolean;
    removeInfoOtherContactInfo: boolean;
    removeInfoPhone: boolean;
    userId: number;

    constructor(userId: number, action: DeleteUserAction) {
        this.userId = userId;
        this.action = action;
    }
}

export class EmploymentDTO implements IEmploymentDTO {
    accountingSettings: IAccountingSettingsRowDTO[];
    actorCompanyId: number;
    baseWorkTimeWeek: number;
    changes: EmploymentChangeDTO[];
    comment: string;
    currentApplyChangeDate: Date;
    currentChangeDateFrom: Date;
    currentChangeDateTo: Date;
    currentChanges: EmploymentChangeDTO[];
    dateFrom: Date;
    dateTo: Date;
    employeeGroupId: number;
    employeeGroupName: string;
    employeeGroupTimeCodes: number[];
    employeeGroupWorkTimeWeek: number;
    employeeId: number;
    employeeName: string;
    employmentEndReason: number;
    employmentEndReasonName: string;
    employmentId: number;
    employmentType: TermGroup_EmploymentType;
    employmentTypeName: string;
    employmentVacationGroup: EmploymentVacationGroupDTO[];
    experienceAgreedOrEstablished: boolean;
    experienceMonths: number;
    updateExperienceMonthsReminder: boolean;
    externalCode: string;
    finalSalaryStatus: SoeEmploymentFinalSalaryStatus;
    fixedAccounting: boolean;
    hibernatingPeriods: IDateRangeDTO[];
    hibernatingTimeDeviationCauseId: number;
    isAddingEmployment: boolean;
    isChangingEmployment: boolean;
    isChangingEmploymentDates: boolean;
    isChangingToNotTemporary: boolean;
    isDeletingEmployment: boolean;
    isNewFromCopy: boolean;
    isReadOnly: boolean;
    isSecondaryEmployment: boolean;
    isTemporaryPrimary: boolean;
    name: string;
    payrollGroupId: number;
    payrollGroupName: string;
    percent: number;
    priceTypes: IEmploymentPriceTypeDTO[];
    specialConditions: string;
    state: SoeEntityState;
    substituteFor: string;
    substituteForDueTo: string;
    uniqueId: string;
    workPlace: string;
    workTasks: string;
    workTimeWeek: number;
    fullTimeWorkTimeWeek: number;
    excludeFromWorkTimeWeekCalculationOnSecondaryEmployment?: boolean;

    // Extensions
    calculatedExperienceMonths: number;
    experienceMonthsText: string;
    hibernatingTimeDeviationCauseName: string;
    isEdited: boolean;
    currentEmployeeGroup: any;

    //Properties
    public get isDeleted(): boolean {
        return this.state !== SoeEntityState.Active && this.state !== SoeEntityState.Hidden;
    }
    public get isEditing(): boolean {
        return this.isChangingEmployment || this.isChangingEmploymentDates || this.isChangingToNotTemporary || this.isAddingEmployment;
    }
    public set isEditing(value: boolean) { /* Needed in extend */ }
    public get isNew(): boolean {
        return this.employmentId == 0;
    }
    public get allowEdit(): boolean {
        return !this.isDeleted && !this.isNew && !this.isReadOnly && this.finalSalaryStatus !== SoeEmploymentFinalSalaryStatus.AppliedFinalSalary;
    }
    public get allowDelete(): boolean {
        return !this.isDeleted && !this.isReadOnly && this.finalSalaryStatus !== SoeEmploymentFinalSalaryStatus.AppliedFinalSalary;
    }
    public get allowPrintEmploymentContract(): boolean {
        return !this.isDeleted && this.employmentId > 0;
    }
    public get workTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.workTimeWeek);
    }
    public set workTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.workTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }
    public get baseWorkTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.baseWorkTimeWeek);
    }
    public set baseWorkTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.baseWorkTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }

    public get fullTimeWorkTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.fullTimeWorkTimeWeek);
    }

    public set fullTimeWorkTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.fullTimeWorkTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }

    public getTotalExperienceMonths() {
        return this.calculatedExperienceMonths + this.experienceMonths;
    }

    //Methods
    public fixDates() {
        this.currentApplyChangeDate = CalendarUtility.convertToDate(this.currentApplyChangeDate);
        this.currentChangeDateFrom = CalendarUtility.convertToDate(this.currentChangeDateFrom);
        this.currentChangeDateTo = CalendarUtility.convertToDate(this.currentChangeDateTo);
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
        if (this.hibernatingPeriods) {
            _.forEach(this.hibernatingPeriods, hibernatingPeriod => {
                hibernatingPeriod.start = CalendarUtility.convertToDate(hibernatingPeriod.start);
                hibernatingPeriod.stop = CalendarUtility.convertToDate(hibernatingPeriod.stop);
            });
        }
    }
    public setTypes() {
        if (this.changes) {
            this.changes = this.changes.map(ec => {
                var ecObj = new EmploymentChangeDTO();
                angular.extend(ecObj, ec);
                ecObj.fixDates();
                return ecObj;
            });
        }

        if (this.currentChanges) {
            this.currentChanges = this.currentChanges.map(ec => {
                var ecObj = new EmploymentChangeDTO();
                angular.extend(ecObj, ec);
                ecObj.fixDates();
                return ecObj;
            });
        }

        if (this.employmentVacationGroup) {
            this.employmentVacationGroup = this.employmentVacationGroup.map(vg => {
                var vgObj = new EmploymentVacationGroupDTO();
                angular.extend(vgObj, vg);
                vgObj.fixDates();
                return vgObj;
            });
        }

        if (this.priceTypes) {
            this.priceTypes = this.priceTypes.map(pt => {
                var ptObj = new EmploymentPriceTypeDTO();
                angular.extend(ptObj, pt);
                ptObj.fixDates();

                ptObj.periods = ptObj.periods.map(p => {
                    var pObj = new EmploymentPriceTypePeriodDTO();
                    angular.extend(pObj, p);
                    pObj.fixDates();
                    return pObj;
                });

                var tObj: PayrollPriceTypeDTO = new PayrollPriceTypeDTO();
                angular.extend(tObj, ptObj.type);
                tObj.fixDates();
                ptObj.type = tObj;

                return ptObj;
            });
        }
    }
}

export class EmploymentChangeDTO implements IEmploymentChangeDTO {
    actorCompanyId: number;
    comment: string;
    created: Date;
    createdBy: string;
    employeeId: number;
    employmentChangeId: number;
    employmentId: number;
    fieldType: TermGroup_EmploymentChangeFieldType;
    fieldTypeName: string;
    fromDate: Date;
    fromValue: string;
    fromValueName: string;
    isDeleted: boolean;
    state: SoeEntityState;
    toDate: Date;
    toValue: string;
    toValueName: string;
    type: TermGroup_EmploymentChangeType;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}

export class EmploymentPriceTypeDTO implements IEmploymentPriceTypeDTO {
    code: string;
    employeeId: number;
    employmentId: number;
    employmentPriceTypeId: number;
    isPayrollGroupPriceType: boolean;
    name: string;
    payrollGroupAmount: number;
    payrollGroupAmountDate: Date;
    payrollPriceType: TermGroup_SoePayrollPriceType;
    payrollPriceTypeId: number;
    periods: EmploymentPriceTypePeriodDTO[];
    readOnly: boolean;
    sort: number;
    type: PayrollPriceTypeDTO;
    levelIsMandatory: boolean;

    // Extensions
    currentAmount: number;

    public fixDates() {
        this.payrollGroupAmountDate = CalendarUtility.convertToDate(this.payrollGroupAmountDate);
    }
}

export class EmploymentPriceTypePeriodDTO implements IEmploymentPriceTypePeriodDTO {
    payrollLevelId: number;
    payrollLevelName: string;
    amount: number;
    employmentPriceTypeId: number;
    employmentPriceTypePeriodId: number;
    fromDate: Date;
    hidden: boolean;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class EmploymentVacationGroupDTO implements IEmploymentVacationGroupDTO {
    calculationType: TermGroup_VacationGroupCalculationType;
    created: Date;
    createdBy: string;
    employmentId: number;
    employmentVacationGroupId: number;
    fromDate: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    type: number;
    vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule;
    vacationGroupId: number;
    vacationHandleRule: TermGroup_VacationGroupVacationHandleRule;

    // Extensions
    public get sortableDate(): Date {
        return CalendarUtility.nullToDefaultDate(this.fromDate);
    }

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.modified = CalendarUtility.convertToDate(this.modified);
    }
}

export class EmployeeTaxSEDTO implements IEmployeeTaxSEDTO {
    adjustmentPeriodFrom: Date;
    adjustmentPeriodTo: Date;
    adjustmentType: TermGroup_EmployeeTaxAdjustmentType;
    adjustmentValue: number;
    birthPlace: string;
    countryCode: string;
    countryCodeBirthPlace: string;
    countryCodeCitizen: string;
    created: Date;
    createdBy: string;
    csrExportDate: Date;
    csrImportDate: Date;
    employeeId: number;
    employeeTaxId: number;
    employmentAbroadCode: TermGroup_EmployeeTaxEmploymentAbroadCode;
    employmentTaxType: TermGroup_EmployeeTaxEmploymentTaxType;
    estimatedAnnualSalary: number;
    firstEmployee: boolean;
    mainEmployer: boolean;
    modified: Date;
    modifiedBy: string;
    oneTimeTaxPercent: number;
    regionalSupport: boolean;
    salaryDistressAmount: number;
    salaryDistressAmountType: TermGroup_EmployeeTaxSalaryDistressAmountType;
    salaryDistressReservedAmount: number;
    salaryDistressCase: string;
    schoolYouthLimitInitial: number;
    sinkType: TermGroup_EmployeeTaxSinkType;
    state: SoeEntityState;
    taxRate: number;
    taxRateColumn: number;
    tinNumber: string;
    type: TermGroup_EmployeeTaxType;
    typeName: string;
    year: number;
    applyEmploymentTaxMinimumRule: boolean;

    public fixDates() {
        this.adjustmentPeriodFrom = CalendarUtility.convertToDate(this.adjustmentPeriodFrom);
        this.adjustmentPeriodTo = CalendarUtility.convertToDate(this.adjustmentPeriodTo);
        this.csrExportDate = CalendarUtility.convertToDate(this.csrExportDate);
        this.csrImportDate = CalendarUtility.convertToDate(this.csrImportDate);
    }
}

export class EmployeeCSRExportDTO implements IEmployeeCSRExportDTO {
    csrExportDate: Date;
    csrImportDate: Date;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeeSocialSec: string;
    employeeTaxId: number;
    year: number;

    public fixDates() {
        this.csrExportDate = CalendarUtility.convertToDate(this.csrExportDate);
        this.csrImportDate = CalendarUtility.convertToDate(this.csrImportDate);
    }
}

export class EmployeeUnionFeeDTO implements IEmployeeUnionFeeDTO {
    employeeId: number;
    employeeUnionFeeId: number;
    fromDate: Date;
    state: SoeEntityState;
    toDate: Date;
    unionFeeId: number;
    unionFeeName: string;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}

export class EmployeeFactorDTO implements IEmployeeFactorDTO {
    employeeFactorId: number;
    factor: number;
    fromDate: Date;
    isCurrent: boolean;
    isReadOnly: boolean;
    type: TermGroup_EmployeeFactorType;
    typeName: string;
    vacationGroupId: number;
    vacationGroupName: string;

    // Extensions
    public get sortableDate(): Date {
        return CalendarUtility.nullToDefaultDate(this.fromDate);
    }

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class EmployeeSettingDTO implements IEmployeeSettingDTO {
    actorCompanyId: number;
    boolData: boolean;
    created: Date;
    createdBy: string;
    dataType: SettingDataType;
    dateData: Date;
    decimalData: number;
    employeeId: number;
    employeeSettingId: number;
    employeeSettingAreaType: TermGroup_EmployeeSettingType;
    employeeSettingGroupType: TermGroup_EmployeeSettingType;
    employeeSettingType: TermGroup_EmployeeSettingType;
    intData: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    strData: string;
    timeData: Date;
    validFromDate: Date;
    validToDate: Date;

    // Extensions
    isCurrent: boolean;
    isModified: boolean;
    tmpEmployeeSettingId: number;
    groupTypeName: string;
    typeName: string;
    optionName: string;

    public fixDates() {
        this.validFromDate = CalendarUtility.convertToDate(this.validFromDate);
        this.validToDate = CalendarUtility.convertToDate(this.validToDate);
    }

    get sortableDate(): Date {
        return CalendarUtility.nullToDefaultDate(this.validFromDate);
    }

    get value(): string {
        switch (this.dataType) {
            case SettingDataType.String:
                return this.strData;
            case SettingDataType.Integer:
                return this.optionName ?? NumberUtility.printDecimal(this.intData, 0, 0);
            case SettingDataType.Decimal:
                return NumberUtility.printDecimal(this.decimalData, 2, 5);
            case SettingDataType.Boolean:
                return this.boolData.toString();
            case SettingDataType.Date:
                return CalendarUtility.toFormattedDate(this.dateData);
            case SettingDataType.Time:
                return CalendarUtility.toFormattedTime(this.timeData);
            default:
                return this.strData;
        }
    }

    get isDataTypeString(): boolean {
        return this.dataType === SettingDataType.String;
    }

    get isDataTypeInteger(): boolean {
        return this.dataType === SettingDataType.Integer;
    }

    get isDataTypeDecimal(): boolean {
        return this.dataType === SettingDataType.Decimal;
    }

    get iDataTypesBoolean(): boolean {
        return this.dataType === SettingDataType.Boolean;
    }

    get isDataTypeDate(): boolean {
        return this.dataType === SettingDataType.Date;
    }

    get isDataTypeTime(): boolean {
        return this.dataType === SettingDataType.Time;
    }

    clearData() {
        this.strData = undefined;
        this.intData = undefined;
        this.decimalData = undefined;
        this.boolData = undefined;
        this.dateData = undefined;
        this.timeData = undefined;
    }
}

export class EmployeeSettingTypeDTO implements IEmployeeSettingTypeDTO {
    dataType: SettingDataType;
    employeeSettingAreaType: TermGroup_EmployeeSettingType;
    employeeSettingGroupType: TermGroup_EmployeeSettingType;
    employeeSettingType: TermGroup_EmployeeSettingType;
    maxLength: number;
    name: string;
    options: ISmallGenericType[];

    get hasOptions(): boolean {
        return this.options && this.options.length > 0;
    }
}

export class UserRolesDTO implements IUserRolesDTO {
    actorCompanyId: number;
    attestRoles: UserAttestRoleDTO[];
    companyName: string;
    defaultCompany: boolean;
    roles: UserCompanyRoleDTO[];

    // Extensions
    visible: boolean;

    public get defaultRoles(): UserCompanyRoleDTO[] {
        return _.sortBy(_.filter(this.roles, r => r.default && (!r.dateTo || r.dateTo.isSameOrAfterOnDay(CalendarUtility.getDateToday()))), r => r.name);
    }

    public get defaultRoleNamesDistinct(): string {
        let reduced = this.defaultRoles.map(r => r.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
        let result = Object.keys(reduced).sort().map(k => {
            return { name: k, count: reduced[k] };
        });

        let names: string = '';
        _.forEach(result, key => {
            if (names)
                names += ', ';

            names += key.name;
            if (key.count > 1)
                names += ' ({0})'.format(key.count.toString());
        });

        return names;
    }

    public get otherRoles(): UserCompanyRoleDTO[] {
        return _.sortBy(_.filter(this.roles, r => !r.default && (!r.dateTo || r.dateTo.isSameOrAfterOnDay(CalendarUtility.getDateToday()))), r => r.name);
    }

    public get otherRoleNamesDistinct(): string {
        let reduced = this.otherRoles.map(r => r.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
        let result = Object.keys(reduced).sort().map(k => {
            return { name: k, count: reduced[k] };
        });

        let names: string = '';
        _.forEach(result, key => {
            if (names)
                names += ', ';

            names += key.name;
            if (key.count > 1)
                names += ' ({0})'.format(key.count.toString());
        });

        return names;
    }

    public get attestRoleNames(): string {
        return _.sortBy(_.map(this.attestRoles, r => r.name)).join(', ');
    }

    public get attestRoleNamesDistinct(): string {
        let reduced = this.attestRoles.map(r => r.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
        let result = Object.keys(reduced).sort().map(k => {
            return { name: k, count: reduced[k] };
        });

        let names: string = '';
        _.forEach(result, key => {
            if (names)
                names += ', ';

            names += key.name;
            if (key.count > 1)
                names += ' ({0})'.format(key.count.toString());
        });

        return names;
    }

    public isRoleActive(role: UserCompanyRoleDTO, date?: Date): boolean {
        if (!role)
            return false;

        return role.isRoleActive(date);
    }
}

export class UserCompanyRoleDTO implements IUserCompanyRoleDTO {
    actorCompanyId: number;
    dateFrom: Date;
    dateTo: Date;
    default: boolean;
    isDelegated: boolean;
    isModified: boolean;
    name: string;
    roleId: number;
    stateId: number;
    state: SoeEntityState;
    userCompanyRoleId: number;
    userId: number;

    // Extensions
    readOnly: boolean;
    initiallySelected: boolean;
    selected: boolean;
    tmpUserCompanyRoleId: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }

    public isRoleActive(date?: Date): boolean {
        if (!date)
            date = CalendarUtility.getDateToday();

        return ((!this.dateFrom || this.dateFrom.isSameOrBeforeOnDay(date)) && (!this.dateTo || this.dateTo.isSameOrAfterOnDay(date)));
    }
}

export class UserAttestRoleDTO implements IUserAttestRoleDTO {
    accountId: number;
    accountName: string;
    accountDimId: number
    accountDimName: string;
    accountPermissionType: TermGroup_AttestRoleUserAccountPermissionType;
    accountPermissionTypeName: string;
    attestRoleId: number;
    attestRoleUserId: number;
    children: UserAttestRoleDTO[];
    dateFrom: Date;
    dateTo: Date;
    isDelegated: boolean;
    isExecutive: boolean;
    isModified: boolean;
    isNearestManager: boolean;
    maxAmount: number;
    moduleName: string;
    name: string;
    parentAttestRoleUserId: number;
    prevAccountId: number;
    userId: number;
    roleId: number;
    state: SoeEntityState;

    // Extensions
    selected: boolean;
    tmpAttestRoleUserId: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);

        _.forEach(this.children, child => {
            child.dateFrom = CalendarUtility.convertToDate(child.dateFrom);
            child.dateTo = CalendarUtility.convertToDate(child.dateTo);
        });
    }
}

export class CompanyRolesDTO implements ICompanyRolesDTO {
    actorCompanyId: number;
    attestRoles: CompanyAttestRoleDTO[];
    companyName: string;
    roles: UserCompanyRoleDTO[];

    // Extensions
    readOnly: boolean;
    defaultCompany: boolean;
    visible: boolean;
    expanded: boolean;
    isModified: boolean;
    hasUserRoles: boolean;
    hasUserAttestRoles: boolean;
    userCompanyRoles: UserCompanyRoleDTO[];
    userCompanyAttestRoles: UserCompanyAttestRoleDTO[];
}

export class CompanyAttestRoleDTO implements ICompanyAttestRoleDTO {
    alsoAttestAdditionsFromTime: boolean;
    attestByEmployeeAccount: boolean;
    attestRoleId: number;
    defaultMaxAmount: number;
    humanResourcesPrivacy: boolean;
    isExecutive: boolean;
    isNearestManager: boolean;
    moduleName: string;
    name: string;
    showAllCategories: boolean;
    showTemplateSchedule: boolean;
    showUncategorized: boolean;
    staffingByEmployeeAccount: boolean;
    state: SoeEntityState;

    // Extensions
    readOnly: boolean;

    public get hasAttestRoleSettings(): boolean {
        return this.showUncategorized ||
            this.showAllCategories ||
            this.showTemplateSchedule ||
            this.alsoAttestAdditionsFromTime ||
            this.humanResourcesPrivacy ||
            this.isExecutive ||
            this.isNearestManager ||
            this.attestByEmployeeAccount ||
            this.staffingByEmployeeAccount;
    }
}

export class UserCompanyAttestRoleDTO {
    attestRoleId: number;
    attestRoleUserId: number;
    accountId: number;
    accountName: string;
    accountDimId: number
    accountDimName: string;
    accountPermissionType: TermGroup_AttestRoleUserAccountPermissionType;
    dateFrom: Date;
    dateTo: Date;
    defaultMaxAmount: number;
    isDelegated: boolean;
    isExecutive: boolean;
    isNearestManager: boolean;
    showAllCategories: boolean;
    showUncategorized: boolean;
    maxAmount: number;
    name: string;
    children: UserAttestRoleDTO[];
    state: SoeEntityState;
    roleId: number;

    // Extensions
    accountPermissionTypeName: string;
    moduleName: string;
    readOnly: boolean;
    initiallySelected: boolean;
    selected: boolean;
    isModified: boolean;
    tmpAttestRoleUserId: number;

    constructor() {
        this.children = [];
    }
}
export class EmployeeMeetingDTO implements IEmployeeMeetingDTO {
    attestRoleIds: number[];
    completed: boolean;
    created: Date;
    createdBy: string;
    employeeCanEdit: boolean;
    employeeId: number;
    employeeMeetingId: number;
    followUpTypeId: number;
    followUpTypeName: string;
    modified: Date;
    modifiedBy: string;
    note: string;
    otherParticipants: string;
    participantIds: number[];
    participantNames: string;
    reminder: boolean;
    startTime: Date;
    state: SoeEntityState;

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
    }
}

export class EmployeeSkillDTO implements IEmployeeSkillDTO {
    dateTo: Date;
    employeeId: number;
    employeeSkillId: number;
    skillId: number;
    skillLevel: number;
    skillLevelStars: number;
    skillLevelUnreached: boolean;
    skillName: string;
    skillTypeName: string;

    public fixDates() {
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class TimeWorkAccountDTO implements ITimeWorkAccountDTO {
    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    defaultPaidLeaveNotUsed: TermGroup_TimeWorkAccountWithdrawalMethod;
    defaultWithdrawalMethod: TermGroup_TimeWorkAccountWithdrawalMethod;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    timeWorkAccountId: number;
    timeWorkAccountYears: ITimeWorkAccountYearDTO[];
    useDirectPayment: boolean;
    usePaidLeave: boolean;
    usePensionDeposit: boolean;
}

export class TimeWorkAccountYearDTO implements ITimeWorkAccountYearDTO {
    created: Date;
    createdBy: string;
    directPaymentLastDate: Date;
    earningStart: Date;
    earningStop: Date;
    employeeLastDecidedDate: Date;
    modified: Date;
    modifiedBy: string;
    paidAbsenceStopDate: Date;
    state: SoeEntityState;
    timeWorkAccountId: number;
    timeWorkAccountWorkTimeWeeks: ITimeWorkAccountWorkTimeWeekDTO[];
    timeWorkAccountYearEmployees: ITimeWorkAccountYearEmployeeDTO[];
    timeWorkAccountYearId: number;
    directPaymentPercent: number;
    paidLeavePercent: number;
    pensionDepositPercent: number;
    withdrawalStart: Date;
    withdrawalStop: Date;
}

export class TimeWorkAccountYearEmployeeDTO implements ITimeWorkAccountYearEmployeeDTO {
    calculatedDirectPaymentAmount: number;
    calculatedPaidLeaveAmount: number;
    calculatedPaidLeaveMinutes: number;
    calculatedPensionDepositAmount: number;
    calculatedWorkingTimePromoted: number;
    created: Date;
    createdBy: string;
    earningStart: Date;
    earningStop: Date;
    employeeId: number;
    employeeName: string;
    employeeNumber: string;
    modified: Date;
    modifiedBy: string;
    selectedDate: Date;
    selectedWithdrawalMethod: TermGroup_TimeWorkAccountWithdrawalMethod;
    selectedWithdrawalMethodName: string;
    state: SoeEntityState;
    status: TermGroup_TimeWorkAccountYearEmployeeStatus;
    statusName: string;
    timeWorkAccountId: number;
    timeWorkAccountYearEmployeeId: number;
}

export class EmployeeTimeWorkAccountDTO implements IEmployeeTimeWorkAccountDTO {
    actorCompanyId: number;
    dateFrom: Date;
    dateTo: Date;
    employeeId: number;
    employeeTimeWorkAccountId: number;
    state: SoeEntityState;
    timeWorkAccountId: number;
    timeWorkAccountName: string;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}