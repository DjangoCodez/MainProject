import { ICsrResponseDTO, IEmployeeSkillDTO, IActionResult, IPositionGridDTO, ISysPositionGridDTO, IEmployeeCalculatedCostDTO, IEmployeeGroupSmallDTO, IEmployeeCalculateVacationResultDTO, IEmploymentTypeSmallDTO, IEmployeeAccountDTO } from "../../Scripts/TypeLite.Net4";
import { IHttpService } from "../../Core/Services/HttpService";
import { ISysVehicleDTO, ISmallGenericType, IPayrollGroupReportDTO, IEmployeeTaxSEDTO } from "../../Scripts/TypeLite.Net4";
import { EmployeeUserDTO, EmploymentDTO, EmployeeTaxSEDTO, DeleteEmployeeDTO, UserRolesDTO, EmployeeMeetingDTO, CreateVacantEmployeeDTO, InactivateEmployeeDTO, EmployeeCSRExportDTO, EmployeeSettingTypeDTO } from "../../Common/Models/EmployeeUserDTO";
import { TermGroup_VehicleType, TermGroup_Languages, SoeTimeCodeType, TermGroup_EmployeeStatisticsType, TermGroup_TrackChangesActionMethod, TermGroup_TimeAccumulatorCompareModel, TermGroup_ApiMessageType, TermGroup_ApiMessageSourceType, TermGroup_EmployeeSettingType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { VacationGroupDTO } from "../../Common/Models/VacationGroupDTO";
import { ContactAddressItemDTO } from "../../Common/Models/ContactAddressDTOs";
import { EmployeePositionDTO } from "../../Common/Models/EmployeePositionDTO";
import { SmallGenericType } from "../../Common/Models/SmallGenericType";
import { UserReplacementDTO } from "../../Common/Models/UserDTO";
import { EmployeeVehicleDTO, EmployeeVehicleGridDTO, EmployeeVehicleDeductionDTO, EmployeeVehicleEquipmentDTO, EmployeeVehicleTaxDTO } from "../../Common/Models/EmployeeVehicleDTO";
import { EmployeeStatisticsChartData } from "../../Common/Models/EmployeeStatistics";
import { SearchEmployeeSkillDTO } from "../../Common/Models/SkillDTOs";
import { FileUploadDTO } from "../../Common/Models/FileUploadDTO";
import { EmployeeCalculatedCostDTO } from "../../Common/Models/EmployeeCalculatedCostDTO";
import { PayrollReviewHeadDTO, PayrollReviewEmployeeDTO } from "../../Common/Models/PayrollReviewDTOs";
import { EmployeeCalculateVacationResultFlattenedDTO } from "../../Common/Models/EmployeeCalculateVacationResultFlattenedDTO";
import { EmployeeAccumulatorDTO } from "../../Common/Models/EmployeeAccumulatorDTO";
import { TimeScheduleTemplateGroupEmployeeDTO } from "../../Common/Models/TimeScheduleTemplateDTOs";
import { ApiMessageGridDTO } from "../../Common/Models/ApiMessageDTO";
import { EmployeeTemplateDTO, EmployeeTemplateGridDTO, SaveEmployeeFromTemplateHeadDTO } from "../../Common/Models/EmployeeTemplateDTOs";
import { EmployeeCollectiveAgreementDTO, EmployeeCollectiveAgreementGridDTO } from "../../Common/Models/EmployeeCollectiveAgreementDTOs";
import { CalendarUtility } from "../../Util/CalendarUtility";

export interface IEmployeeService {

    // GET

    cardNumberExists(cardNumber: string, excludeEmployeeId: number): ng.IPromise<IActionResult>
    validateEmployeeSocialSecNumberNotExists(socialSecNr: string, excludeEmployeeId: number): ng.IPromise<IActionResult>
    csrInquiry(employeeId: number, year: number): ng.IPromise<ICsrResponseDTO>
    employeeNumberExists(employeeNumber: string, getHidden: boolean, excludeEmployeeId: number): ng.IPromise<boolean>
    getApiMessages(type: TermGroup_ApiMessageType, source: TermGroup_ApiMessageSourceType, filterFromDate: Date, filterToDate: Date, filterShowVerified: boolean, filterShowOnlyErrors: boolean): ng.IPromise<ApiMessageGridDTO[]>;
    getHasAttestRoles(dateFrom: Date, dateTo: Date): ng.IPromise<boolean>;
    getAnnualLeaveGroupsDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getAttestRolesForMeetings(): ng.IPromise<ISmallGenericType[]>;
    getCardNumbers(): ng.IPromise<any>
    getContactAddressItems(actorId: number): ng.IPromise<ContactAddressItemDTO[]>
    getContactAddressItemsByUser(userId: number): ng.IPromise<ContactAddressItemDTO[]>
    getCompanyPayrollGroupReports(checkRolePermission: boolean): ng.IPromise<IPayrollGroupReportDTO[]>
    getDayTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>
    getDefaultEmployeeAccountDimName(): ng.IPromise<string>
    getDefaultEmployeeAccountId(employeeId: number, date?: Date): ng.IPromise<number>
    getEmployeeAccountIds(employeeId: number, date?: Date): ng.IPromise<number[]>
    getEmployeeAccumulators(dateFrom: Date, dateTo: Date, employeeIds: number[], accumulatorIds: number[], rangeType: number, compareModel: TermGroup_TimeAccumulatorCompareModel, ownLimitMin: number, ownLimitMax: number): ng.IPromise<EmployeeAccumulatorDTO[]>
    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean, useCache?: boolean): ng.IPromise<any>
    getEmployeesCount(): ng.IPromise<any>
    getEmployeeForEdit(employeeId: number, loadMeetings: boolean, loadTemplateGroups: boolean): ng.IPromise<EmployeeUserDTO>
    getEmployeeForExport(employeeId: number): ng.IPromise<any>;
    getEmployeeChilds(employeeId: number): ng.IPromise<any>
    getEmployeeChildsDict(employeeId: number, addEmptyRow: boolean): ng.IPromise<any>
    getEmployeeCollectiveAgreementsDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getEmployeeCollectiveAgreementsGrid(): ng.IPromise<EmployeeCollectiveAgreementGridDTO[]>
    getEmployeeCollectiveAgreement(employeeCollectiveAgreementId: number): ng.IPromise<EmployeeCollectiveAgreementDTO>
    getEmployeeGroups(): ng.IPromise<any>
    getEmployeeGroupsSmall(): ng.IPromise<IEmployeeGroupSmallDTO[]>
    getEmployeeGroup(employeeGroupId: number): ng.IPromise<any>;
    getEmployeeGroupSmall(employeeGroupId: number): ng.IPromise<any>;
    getEmployeeLicenseInfo(): ng.IPromise<any>
    getEmployeeTax(employeeTaxId: number): ng.IPromise<IEmployeeTaxSEDTO>
    getEmployeeTaxByYear(employeeId: number, year: number): ng.IPromise<IEmployeeTaxSEDTO>
    getEmployeeTaxYears(employeeId: number): ng.IPromise<number[]>
    calculateSchoolYouthLimitRemaining(schoolYouthLimitInitial: number, schoolYouthLimitUsed: number, date: Date): ng.IPromise<any>
    calculateSchoolYouthLimitUsed(employeeId: number, date: Date): ng.IPromise<any>
    getEmployeesForCsrExport(year: number): ng.IPromise<EmployeeCSRExportDTO[]>
    getEmployeeMeetings(employeeId: number, userId: number): ng.IPromise<any>
    getEmployeePosition(employeePositionId: number, loadSkills: boolean): ng.IPromise<any>
    getEmployeePositions(employeeId: number, loadSysPosition: boolean): ng.IPromise<any>
    getEmployeeStatisticsEmployeeData(employeeId: number, dateFrom: Date, dateTo: Date, type: TermGroup_EmployeeStatisticsType): ng.IPromise<EmployeeStatisticsChartData[]>
    getEmployeeTemplatesDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getEmployeeTemplatesGrid(): ng.IPromise<EmployeeTemplateGridDTO[]>
    getEmployeeTemplate(employeeTemplateId: number): ng.IPromise<EmployeeTemplateDTO>
    getEmployeeVehicles(loadEmployee: boolean, loadDeduction: boolean, loadEquipment: boolean, loadTax: boolean): ng.IPromise<EmployeeVehicleGridDTO[]>
    getEmployeeVehicle(employeeVehicleId: number, loadEmployee: boolean, loadDeduction: boolean, loadEquipment: boolean, loadTax: boolean): ng.IPromise<EmployeeVehicleDTO>
    getEmployments(employeeId: number, date: Date): ng.IPromise<EmploymentDTO[]>
    getEmploymentTypes(): ng.IPromise<any>
    getEmploymentType(employmentTypeId: number): ng.IPromise<any>
    getStandardEmploymentTypes(language: TermGroup_Languages): ng.IPromise<IEmploymentTypeSmallDTO[]>
    getEmploymentEmploymentTypes(language: TermGroup_Languages): ng.IPromise<IEmploymentTypeSmallDTO[]>
    getTotalExperienceMonthsForEmployment(employmentId: number, stopDate: Date): ng.IPromise<number>
    getTotalExperienceMonthFromPreviousEmployment(currentEmploymentId: number): ng.IPromise<number>
    getExperienceMonthsForEmployee(employeeId: number, stopDate: Date): ng.IPromise<number>
    getEndReasons(): ng.IPromise<any>
    getEndReason(endReasonId: number): ng.IPromise<any>
    getEmploymentEndReasons(language: TermGroup_Languages): ng.IPromise<ISmallGenericType[]>
    getLastUsedEmployeeSequenceNumber(): ng.IPromise<number>
    getPayrollReviewHeads(loadRows: boolean, loadPayrollGroups: boolean, loadPayrollPriceTypes: boolean, loadPayrollLevels: boolean, setStatusName: boolean): ng.IPromise<PayrollReviewHeadDTO[]>
    getPayrollReviewHead(payrollReviewHeadId: number, loadRows: boolean, loadPayrollGroups: boolean, loadPayrollPriceTypes: boolean, loadPayrollLevels: boolean, setStatusName: boolean): ng.IPromise<PayrollReviewHeadDTO>
    getPayrollPriceFormulasDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getPayrollPriceTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>
    getPrelPaidDaysYear1(employeeId: number): ng.IPromise<number>
    getPositionsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<SmallGenericType[]>;
    getPositionsGrid(loadSkills: boolean, useCache: boolean): ng.IPromise<IPositionGridDTO[]>
    getSkillsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<SmallGenericType[]>;
    getTimeCodesDict(timeCodeType: SoeTimeCodeType, addEmptyRow: boolean, concatCodeAndName: boolean): ng.IPromise<any>
    getTimeDeviationCauses(): ng.IPromise<any>
    getTimeDeviationCausesGrid(employeeGroupId?: number): ng.IPromise<any>
    getHibernatingTimeDeviationCauses(): ng.IPromise<any>
    getTimeScheduleTemplateGroupsForEmployee(employeeId: number, loadGroup: boolean, loadRows: boolean): ng.IPromise<TimeScheduleTemplateGroupEmployeeDTO[]>
    getSysPayrollPriceAmount(sysTermId: number, date: Date): ng.IPromise<any>
    getUnionFeesDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getVacationGroups(setTypeName: boolean, loadOnlyActive: boolean): ng.IPromise<any>
    getVacationGroup(vacationGroupId: number): ng.IPromise<VacationGroupDTO>
    getVacationDebtCalculations(): ng.IPromise<EmployeeCalculateVacationResultFlattenedDTO[]>
    getEmployeeVacationDebtCalculationResults(employeeCalculateVacationResultHeadId: number, employeeId: number, onlyActive: boolean): ng.IPromise<IEmployeeCalculateVacationResultDTO[]>
    getSysVehicleManufacturingYears(): ng.IPromise<any>
    getSysVehicleMakes(type: TermGroup_VehicleType, manufacturingYear: number): ng.IPromise<any>
    getSysVehicleModels(type: TermGroup_VehicleType, manufacturingYear: number, make: string): ng.IPromise<any>
    getSysVehicleByCode(modelCode: string): ng.IPromise<ISysVehicleDTO>
    getFollowUpTypes(): ng.IPromise<any>
    getFollowUpType(followUpTypeId: number): ng.IPromise<any>
    getCsrResultFromDataStorage(dataStorageId: number): ng.IPromise<any[]>
    getEmployeeImage(employeeId: number): ng.IPromise<any>
    getImageByFileId(fileId: number): ng.IPromise<any>
    getEmployeesWithoutUsers(onlyActive: boolean, addEmptyRow: boolean, concatNumberAndName: boolean): ng.IPromise<ISmallGenericType[]>
    getCalculatedCosts(employeeId: number): ng.IPromise<IEmployeeCalculatedCostDTO[]>
    hasEmployeeTemplates(): ng.IPromise<boolean>
    getHasAllowToAddOtherEmployeeAccounts(date: Date): ng.IPromise<boolean>
    getAvailableEmployeeSettingsByArea(area: TermGroup_EmployeeSettingType): ng.IPromise<EmployeeSettingTypeDTO[]>;
    searchEmployeeSkills(employeeNrFrom: string, employeeNrTo: string, categoryId: number, positionId: number, skillId: number, endDate: Date, getMissingSkill: boolean, getMissingPosition: boolean, accountId?: number): ng.IPromise<SearchEmployeeSkillDTO[]>;
    validateInactivateEmployee(employeeId: number): ng.IPromise<IActionResult>
    validateDeleteEmployee(employeeId: number): ng.IPromise<IActionResult>
    validateImmediateDeleteEmployee(employeeId: number): ng.IPromise<IActionResult>

    // POST
    validateShortenEmployment(employeeId: number, oldDateFrom: Date, oldDateTo: Date, newDateFrom: Date, newDateTo: Date, applyFinalSalary: boolean, changedEmployment: EmploymentDTO, employments?: EmploymentDTO[]): ng.IPromise<any>
    validateSaveEmployee(employeeUser: EmployeeUserDTO, contactAddresses: any): ng.IPromise<any>
    saveEmployeeVacationCalculationResultValues(employeeCalculateVacationResultHeadId: number, employeeId: number, results: IEmployeeCalculateVacationResultDTO[]): ng.IPromise<any>
    validateEmployeeAccounts(accounts: IEmployeeAccountDTO[], mustHaveMainAllocation: boolean, mustHaveDefault: boolean): ng.IPromise<IActionResult>
    saveEmployeeUser(employeeUser: EmployeeUserDTO, contactAddresses: ContactAddressItemDTO[], employeePositions: EmployeePositionDTO[], employeeSkills: IEmployeeSkillDTO[], userReplacement: UserReplacementDTO, employeeTax: EmployeeTaxSEDTO, saveRoles: boolean, saveAttestRoles: boolean, userRoles: UserRolesDTO[], files: FileUploadDTO[], extraFields: any[]): ng.IPromise<any>
    saveEmployeeFromTemplate(model: SaveEmployeeFromTemplateHeadDTO): ng.IPromise<IActionResult>
    createVacantEmployees(employees: CreateVacantEmployeeDTO[]): ng.IPromise<IActionResult>
    saveEmployeeCollectiveAgreement(employeeCollectiveAgreement: EmployeeCollectiveAgreementDTO): ng.IPromise<IActionResult>
    saveEmployeeTemplate(employeeTemplate: EmployeeTemplateDTO): ng.IPromise<IActionResult>
    saveEmployeeVehicle(employeeVehicle: any): ng.IPromise<any>
    saveEmployeeTaxSe(employeeTaxSeId: number, employeeId: number): ng.IPromise<any>
    saveEmploymentType(employmentType: any): ng.IPromise<any>
    updateEmploymentTypesState(dict: any): ng.IPromise<any>
    saveEndReason(endReason: any): ng.IPromise<any>
    updateEndReasonsState(dict: any): ng.IPromise<any>
    markEmployeesAsVacant(employeeIds: number[]): ng.IPromise<IActionResult>
    exportPayrollReview(payrollReviewHead: PayrollReviewHeadDTO): ng.IPromise<any>;
    savePayrollReviewHead(payrollReviewHead: PayrollReviewHeadDTO): ng.IPromise<IActionResult>
    updatePayrollReview(payrollReviewHeadId: number, keepFuture: boolean): ng.IPromise<IActionResult>;
    saveFollowUpType(followUpType: any): ng.IPromise<any>
    updateFollowUpTypesState(dict: any): ng.IPromise<any>
    saveVacationGroup(vacationGroup: any): ng.IPromise<any>
    getTaxSeReportURL(ids: number[], year: number): ng.IPromise<any>
    getCsrResponses(ids: number[], year: number): ng.IPromise<ICsrResponseDTO[]>
    savePosition(position: any): ng.IPromise<any>
    updateLinkedPositions(): ng.IPromise<any>
    updateSysPosionsGrid(items: ISysPositionGridDTO[], link: boolean): ng.IPromise<IActionResult>
    getTimeEmploymentContractUrl(employeeId: number, employmentId: number, dateFrom: Date, dateTo: Date, dates: Date[], reportId: number, reportTemplateTypeId: number, printedFromSchedulePlanning: boolean): ng.IPromise<string>
    deleteEmployee(input: DeleteEmployeeDTO): ng.IPromise<IActionResult>
    inactivateEmployees(employeeIds: number[]): ng.IPromise<InactivateEmployeeDTO[]>
    getEmployeesForPayrollReview(fromDate: Date, payrollGroupIds: number[], payrollPriceTypeIds: number[], payrollLevelIds: number[], employeeIds?: number[]): ng.IPromise<PayrollReviewEmployeeDTO[]>;
    validateInactivateEmployees(employeeIds: number[]): ng.IPromise<InactivateEmployeeDTO[]>

    // DELETE
    deleteEmployeeCalculateVacationResultHead(employeeCalculateVacationResultHeadId: number): ng.IPromise<any>
    deleteEmployeeCalculateVacationResultsForEmployee(employeeCalculateVacationResultHeadId: number, employeeId: number): ng.IPromise<any>
    deleteCardNumber(employeeId: number): ng.IPromise<any>
    deleteEmployeeCollectiveAgreement(employeeCollectiveAgreementId: number): ng.IPromise<IActionResult>
    deleteEmployeeTemplate(employeeTemplateId: number): ng.IPromise<IActionResult>
    deleteEmploymentType(employmentTypeId: number): ng.IPromise<any>
    deleteEndReason(endReasonId: number): ng.IPromise<any>
    deleteEmployeeVehicle(employeeVehicleId: number): ng.IPromise<any>
    deletePayrollReviewHead(payrollReviewHeadId: number): ng.IPromise<any>
    deleteFollowUpType(followUpTypeId: number): ng.IPromise<any>
    deleteVacationGroup(vacationGroupId: number): ng.IPromise<any>
    deletePosition(positionId: number): ng.IPromise<any>
}

export class EmployeeService implements IEmployeeService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    cardNumberExists(cardNumber: string, excludeEmployeeId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CARD_NUMBER_EXISTS + cardNumber + "/" + excludeEmployeeId, false);
    }

    validateEmployeeSocialSecNumberNotExists(socialSecNr: string, excludeEmployeeId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_SOCIAL_SEC_NUMBER_EXISTS + socialSecNr + "/" + excludeEmployeeId, false);
    }

    csrInquiry(employeeId: number, year: number): ng.IPromise<ICsrResponseDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CSR_INQUIRY + employeeId + "/" + year, false);
    }

    employeeNumberExists(employeeNumber: string, getHidden: boolean, excludeEmployeeId: number): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_NUMBER_EXISTS + employeeNumber + "/" + getHidden + "/" + excludeEmployeeId, false);
    }

    getApiMessages(type: TermGroup_ApiMessageType, source: TermGroup_ApiMessageSourceType, filterFromDate: Date, filterToDate: Date, filterShowVerified: boolean, filterShowOnlyErrors: boolean): ng.IPromise<ApiMessageGridDTO[]> {
        var fromDate: string = null;
        if (filterFromDate)
            fromDate = filterFromDate.toDateTimeString();
        var toDate: string = null;
        if (filterToDate)
            toDate = filterToDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_CORE_API_MESSAGES + type + "/" + source + "/" + fromDate + "/" + toDate + "/" + filterShowVerified + "/" + filterShowOnlyErrors, false).then(x => {
            return x.map(y => {
                let obj = new ApiMessageGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getHasAttestRoles(dateFrom: Date, dateTo: Date): ng.IPromise<boolean> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();

        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE_USER_HAS_ATTEST_ROLES + dateFromString + "/" + dateToString, false);
    }

    getAnnualLeaveGroupsDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_ANNUAL_LEAVE_GROUP + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getAttestRolesForMeetings() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_ATTEST_ROLE_MEETING, false);
    }

    getCardNumbers() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CARD_NUMBER, false);
    }

    getContactAddressItems(actorId: number): ng.IPromise<ContactAddressItemDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_ADDRESSITEM + actorId, false).then(x => {
            return x.map(y => {
                let obj = new ContactAddressItemDTO();
                angular.extend(obj, y);
                return obj;
            })
        });
    }

    getContactAddressItemsByUser(userId: number): ng.IPromise<ContactAddressItemDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_ADDRESSITEM_BY_USER + userId, false).then(x => {
            return x.map(y => {
                let obj = new ContactAddressItemDTO();
                angular.extend(obj, y);
                return obj;
            })
        });
    }

    getCompanyPayrollGroupReports(checkRolePermission: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP_REPORTS + checkRolePermission, false);
    }

    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean, useCache: boolean = true) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName + "&getHidden=" + getHidden + "&orderByName=" + orderByName, useCache);
    }
    getDayTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }
    getDefaultEmployeeAccountDimName() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_ACCOUNTDIM_DEFAULT, false);
    }

    getDefaultEmployeeAccountId(employeeId: number, date?: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_ACCOUNT_DEFAULT + employeeId + "/" + dateString, false);
    }

    getEmployeeAccountIds(employeeId: number, date?: Date): ng.IPromise<number[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_ACCOUNT + employeeId + "/" + dateString, false);
    }

    getEmployeeAccumulators(dateFrom: Date, dateTo: Date, employeeIds: number[], accumulatorIds: number[], rangeType: number, compareModel: TermGroup_TimeAccumulatorCompareModel, ownLimitMin: number, ownLimitMax: number): ng.IPromise<EmployeeAccumulatorDTO[]> {
        var model = {
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds,
            accumulatorIds: accumulatorIds,
            rangeType: rangeType,
            compareModel: compareModel,
            ownLimitMin: ownLimitMin,
            ownLimitMax: ownLimitMax,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_ACCUMULATORS, model);
    }


    getEmployeesCount() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_COUNT, false);
    }

    getEmployeeForEdit(employeeId: number, loadMeetings: boolean, loadTemplateGroups: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_EDIT + employeeId + "/" + loadMeetings + "/" + loadTemplateGroups, false).then((x: EmployeeUserDTO) => {
            if (x) {
                var obj: EmployeeUserDTO = new EmployeeUserDTO();
                angular.extend(obj, x);
                obj.fixDates();
                obj.setTypes();
                return obj;
            } else {
                return x;
            }
        });
    }

    getEmployeeForExport(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_EXPORT + employeeId, false);
    }

    getEmployeeChilds(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_CHILD + "?employeeId=" + employeeId, true, Constants.WEBAPI_ACCEPT_DTO);
    }

    getEmployeeChildsDict(employeeId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_CHILD + "?employeeId=" + employeeId + "&addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getEmployeeCollectiveAgreementsDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_COLLECTIVE_AGREEMENT + "?addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getEmployeeCollectiveAgreementsGrid(): ng.IPromise<EmployeeCollectiveAgreementGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_COLLECTIVE_AGREEMENT, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj: EmployeeCollectiveAgreementGridDTO = new EmployeeCollectiveAgreementGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getEmployeeCollectiveAgreement(employeeCollectiveAgreementId: number): ng.IPromise<EmployeeCollectiveAgreementDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_COLLECTIVE_AGREEMENT + employeeCollectiveAgreementId, false).then(x => {
            let obj: EmployeeCollectiveAgreementDTO = new EmployeeCollectiveAgreementDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getEmployeeGroups() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP, false);
    }

    getEmployeeGroupsSmall() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getEmployeeGroup(employeeGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP + employeeGroupId, false);
    }

    getEmployeeGroupSmall(employeeGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP + employeeGroupId, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getEmployeeLicenseInfo(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_LICENSE_INFO, false);
    }

    getEmployeeTax(employeeTaxId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TAX + employeeTaxId, false);
    }

    getEmployeeTaxByYear(employeeId: number, year: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TAX + employeeId + "/" + year, false).then(x => {
            if (x) {
                var employeeTax: EmployeeTaxSEDTO = new EmployeeTaxSEDTO();
                angular.extend(employeeTax, x);
                employeeTax.fixDates();

                return employeeTax;
            } else {
                return x;
            }
        });
    }

    getEmployeeTaxYears(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TAX_YEARS + employeeId, false);
    }

    getTotalExperienceMonthsForEmployment(employmentId: number, stopDate: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (stopDate)
            dateString = stopDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_EXPERIENCEMONTHS + employmentId + "/" + dateString, false);
    }

    getTotalExperienceMonthFromPreviousEmployment(currentEmploymentId: number): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_EXPERIENCEMONTHS_PREVIOUS_EMPLOYMENT + currentEmploymentId, false);
    }

    getExperienceMonthsForEmployee(employeeId: number, stopDate: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (stopDate)
            dateString = stopDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EXPERIENCEMONTHS + employeeId + "/" + dateString, false);
    }

    calculateSchoolYouthLimitRemaining(schoolYouthLimitInitial: number, schoolYouthLimitUsed: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_SCHOOLYOUTH_LIMIT_CALCULATE_REMAINING + schoolYouthLimitInitial + "/" + schoolYouthLimitUsed + "/" + dateString, false);
    }

    calculateSchoolYouthLimitUsed(employeeId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_SCHOOLYOUTH_LIMIT_CALCULATE_USED + employeeId + "/" + dateString, false);
    }


    getEmployeesForCsrExport(year: number): ng.IPromise<EmployeeCSRExportDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CSR_EXPORT + year, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeCSRExportDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getPayrollReviewHeads(loadRows: boolean, loadPayrollGroups: boolean, loadPayrollPriceTypes: boolean, loadPayrollLevels: boolean, setStatusName: boolean): ng.IPromise<PayrollReviewHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW + loadRows + "/" + loadPayrollGroups + "/" + loadPayrollPriceTypes + "/" + loadPayrollLevels + "/" + setStatusName, false).then(x => {
            return x.map(y => {
                let obj = new PayrollReviewHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollReviewHead(payrollReviewHeadId: number, loadRows: boolean, loadPayrollGroups: boolean, loadPayrollPriceTypes: boolean, loadPayrollLevels: boolean, setStatusName: boolean): ng.IPromise<PayrollReviewHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW + payrollReviewHeadId + "/" + loadRows + "/" + loadPayrollGroups + "/" + loadPayrollPriceTypes + "/" + loadPayrollLevels + "/" + setStatusName, false).then(x => {
            let obj = new PayrollReviewHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getPositionsDict(addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE_POSITION + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getPositionsGrid(loadSkills: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE_POSITION + "?loadSkills=" + loadSkills, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getSkillsDict(addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_SCHEDULE_SKILL + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getTimeCodesDict(timeCodeType: SoeTimeCodeType, addEmptyRow: boolean, concatCodeAndName: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + "?timeCodeType=" + timeCodeType + "&addEmptyRow=" + addEmptyRow + "&concatCodeAndName=" + concatCodeAndName, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeDeviationCauses() {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getTimeDeviationCausesGrid(employeeGroupId?: number) {
        if (employeeGroupId)
            return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?employeeGroupId=" + employeeGroupId, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
        else
            return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getHibernatingTimeDeviationCauses() {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_HIBERNATING, false);
    }

    getTimeScheduleTemplateGroupsForEmployee(employeeId: number, loadGroup: boolean, loadRows: boolean): ng.IPromise<TimeScheduleTemplateGroupEmployeeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP_FOR_EMPLOYEE + employeeId + "/" + loadGroup + "/" + loadRows, false).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTemplateGroupEmployeeDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollPriceFormulasDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getPayrollPriceTypesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getPrelPaidDaysYear1(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACATION_GET_PREL_PAID_DAYS_YEAR_1 + employeeId, false);
    }

    getEmployeeTemplatesDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE + "?addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getEmployeeTemplatesGrid(): ng.IPromise<EmployeeTemplateGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj: EmployeeTemplateGridDTO = new EmployeeTemplateGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getEmployeeTemplate(employeeTemplateId: number): ng.IPromise<EmployeeTemplateDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE + employeeTemplateId, false).then(x => {
            let obj: EmployeeTemplateDTO = new EmployeeTemplateDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getEmployeeVehicles(loadEmployee: boolean, loadDeduction: boolean, loadEquipment: boolean, loadTax: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_VEHICLE + loadEmployee + "/" + loadDeduction + "/" + loadEquipment + "/" + loadTax, false).then(x => {
            return x.map(y => {
                let obj: EmployeeVehicleGridDTO = new EmployeeVehicleGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEmployeeVehicle(employeeVehicleId: number, loadEmployee: boolean, loadDeduction: boolean, loadEquipment: boolean, loadTax: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_VEHICLE + employeeVehicleId + "/" + loadEmployee + "/" + loadDeduction + "/" + loadEquipment + "/" + loadTax, false).then(x => {
            let obj = new EmployeeVehicleDTO();
            angular.extend(obj, x);
            obj.fixDates();

            if (!obj.deduction)
                obj.deduction = [];
            obj.deduction = obj.deduction.map(d => {
                var dObj = new EmployeeVehicleDeductionDTO();
                angular.extend(dObj, d);
                dObj.fixDates();
                return dObj;
            });

            if (!obj.equipment)
                obj.equipment = [];
            obj.equipment = obj.equipment.map(d => {
                var eObj = new EmployeeVehicleEquipmentDTO();
                angular.extend(eObj, d);
                eObj.fixDates();
                return eObj;
            });

            if (!obj.tax)
                obj.tax = [];
            obj.tax = obj.tax.map(d => {
                var tObj = new EmployeeVehicleTaxDTO();
                angular.extend(tObj, d);
                tObj.fixDates();
                return tObj;
            });

            return obj;
        });
    }

    getEmployments(employeeId: number, date: Date): ng.IPromise<EmploymentDTO[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT + employeeId + "/" + dateString, false).then(x => {
            return x.map(e => {
                var eObj = new EmploymentDTO();
                angular.extend(eObj, e);
                eObj.fixDates();
                eObj.setTypes();
                return eObj;
            });
        });
    }

    getEmploymentTypes(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE, false);
    }

    getEmploymentType(employmentTypeId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE + employmentTypeId, false);
    }

    getStandardEmploymentTypes(language: TermGroup_Languages) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE_STANDARD + language, true);
    }
    getEmploymentEmploymentTypes(language: TermGroup_Languages) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE_EMPLOYMENT + language, true);
    }

    getEndReasons() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_END_REASON, false);
    }

    getEndReason(endReasonId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_END_REASON + endReasonId, false);
    }

    getEmploymentEndReasons(language: TermGroup_Languages) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_END_REASON_EMPLOYMENT + language, true);
    }

    getTimeEmploymentContractUrl(employeeId: number, employmentId: number, dateFrom: Date, dateTo: Date, dates: Date[], reportId: number, reportTemplateTypeId: number, printedFromSchedulePlanning: boolean) {
        var model = {
            employeeId: employeeId,
            employmentId: employmentId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            reportId: reportId,
            reportTemplateTypeId: reportTemplateTypeId,
            dateChanges: dates,
            printedFromScheduleplanning: printedFromSchedulePlanning,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_CONTRACT_URL, model);
    }

    getLastUsedEmployeeSequenceNumber(): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_LAST_USED_EMPLOYEE_NUMBER, false);
    }

    getSysPayrollPriceAmount(sysTermId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_PAYROLL_PRICE_AMOUNT + sysTermId + "/" + dateString, true);
    }

    getUnionFeesDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_UNION_FEE_DICT + addEmptyRow, false);
    }

    getVacationGroups(setTypeName: boolean, loadOnlyActive: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACATION_GROUP + setTypeName + "/" + loadOnlyActive, false);
    }

    getVacationGroup(vacationGroupId: number): ng.IPromise<VacationGroupDTO> {
        if (!vacationGroupId)
            return null;

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACATION_GROUP + vacationGroupId, false).then(x => {
            let obj = new VacationGroupDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getVacationDebtCalculations(): ng.IPromise<EmployeeCalculateVacationResultFlattenedDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CALCULATE_VACATION_HEAD_GRID, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeCalculateVacationResultFlattenedDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEmployeeVacationDebtCalculationResults(employeeCalculateVacationResultHeadId: number, employeeId: number, onlyActive: boolean): ng.IPromise<IEmployeeCalculateVacationResultDTO[]> {

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CALCULATE_VACATION_RESULT_EMPLOYEE + employeeCalculateVacationResultHeadId + "/" + employeeId + "/" + onlyActive, false);
    }

    getSysVehicleByCode(modelCode: string) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE_BY_CODE + modelCode, true);
    }

    getSysVehicleManufacturingYears() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE_MANUFACTURING_YEAR, true);
    }

    getSysVehicleMakes(type: TermGroup_VehicleType, manufacturingYear: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE_VEHICLE_MAKES + type + "/" + manufacturingYear, true);
    }

    getSysVehicleModels(type: TermGroup_VehicleType, manufacturingYear: number, make: string) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_VEHICLE_TYPE_VEHICLE_MODEL + type + "/" + manufacturingYear + "/" + make, true);
    }

    getFollowUpTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_FOLLOW_UP_TYPE, false);
    }

    getFollowUpType(followUpTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_FOLLOW_UP_TYPE + followUpTypeId, false);
    }

    getCsrResultFromDataStorage(dataStorageId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CSR_IMPORT + dataStorageId, false);
    }

    getEmployeePosition(employeePositionId: number, loadSkills: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_POSITION + employeePositionId + "/" + loadSkills, false);
    }

    getEmployeePositions(employeeId: number, loadSysPosition: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_POSITION + employeeId + "/" + loadSysPosition, false);
    }

    getEmployeeStatisticsEmployeeData(employeeId: number, dateFrom: Date, dateTo: Date, type: TermGroup_EmployeeStatisticsType): ng.IPromise<EmployeeStatisticsChartData[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_STATISTICS_EMPLOYEE_DATA + employeeId + "/" + dateFromString + "/" + dateToString + "/" + type, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeStatisticsChartData();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEmployeeImage(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_SUPPLIERINVOICEIMAGE + employeeId, true);
    }

    getImageByFileId(fileId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_FILE_SUPPLIERINVOICEIMAGE + fileId, true);
    }

    getEmployeesWithoutUsers(onlyActive: boolean, addEmptyRow: boolean, concatNumberAndName: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_WITHOUT_USERS + onlyActive + "/" + addEmptyRow + "/" + concatNumberAndName, false);
    }

    getEmployeeMeetings(employeeId: number, userId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_MEETING + employeeId + "/" + userId, false).then(x => {
            return x.map(y => {
                let obj: EmployeeMeetingDTO = new EmployeeMeetingDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getCalculatedCosts(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CALCULATED_COSTS + employeeId, false).then(x => {
            return x.map(y => {
                let obj: EmployeeCalculatedCostDTO = new EmployeeCalculatedCostDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    hasEmployeeTemplates(): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE_HAS_EMPLOYEE_TEMPLATES, true);
    }

    getHasAllowToAddOtherEmployeeAccounts(date: Date): ng.IPromise<boolean> {
        var dateString: string = null;
        if (!date)
            date = CalendarUtility.getDateToday();

        dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE_USER_HAS_ALLOW_TO_ADD_OTHER_EMPLOYEE_ACCOUNTS + dateString, false);
    }

    getAvailableEmployeeSettingsByArea(area: TermGroup_EmployeeSettingType): ng.IPromise<EmployeeSettingTypeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_SETTING_AREA + area, false).then(x => {
            return x.map(y => {
                let obj: EmployeeSettingTypeDTO = new EmployeeSettingTypeDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    searchEmployeeSkills(employeeNrFrom: string, employeeNrTo: string, categoryId: number, positionId: number, skillId: number, endDate: Date, getMissingSkill: boolean, getMissingPosition: boolean, accountId: number = 0) {
        var endDateString: string = null;
        if (endDate)
            endDateString = endDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_SKILL_SEARCH + "?employeeNrFrom=" + employeeNrFrom + "&employeeNrTo=" + employeeNrTo + "&categoryId=" + categoryId + "&positionId=" + positionId + "&skillId=" + skillId + "&endDateString=" + endDateString + "&getMissingSkill=" + getMissingSkill + "&getMissingPosition=" + getMissingPosition + "&accountId=" + accountId, false).then(x => {
            return x.map(y => {
                let obj: SearchEmployeeSkillDTO = new SearchEmployeeSkillDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    validateInactivateEmployee(employeeId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VALIDATE_INACTIVATE + employeeId, false);
    }

    validateDeleteEmployee(employeeId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VALIDATE_DELETE + employeeId, false);
    }

    validateImmediateDeleteEmployee(employeeId: number): ng.IPromise<IActionResult> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VALIDATE_IMMEDIATE_DELETE + employeeId, false);
    }


    // POST
    validateShortenEmployment(employeeId: number, oldDateFrom: Date, oldDateTo: Date, newDateFrom: Date, newDateTo: Date, applyFinalSalary: boolean, changedEmployment: EmploymentDTO, employments?: EmploymentDTO[]) {
        const model = {
            employeeId: employeeId,
            oldDateFrom: oldDateFrom,
            oldDateTo: oldDateTo,
            newDateFrom: newDateFrom,
            newDateTo: newDateTo,
            applyFinalSalary: applyFinalSalary,
            changedEmployment: changedEmployment,
            employments: employments,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_VALIDATE_SHORTEN_EMPLOYMENT, model);
    }

    validateSaveEmployee(employeeUser: EmployeeUserDTO, contactAddresses: any) {
        const model = {
            employeeUser: employeeUser,
            contactAdresses: contactAddresses,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_VALIDATE_SAVE_EMPLOYEE, model);
    }

    saveEmployeeVacationCalculationResultValues(employeeCalculateVacationResultHeadId: number, employeeId: number, results: IEmployeeCalculateVacationResultDTO[]) {
        const model = {
            employeeCalculateVacationResultHeadId: employeeCalculateVacationResultHeadId,
            employeeId: employeeId,
            results: results,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_CALCULATE_VACATION_RESULT_VALUES, model);
    }

    validateEmployeeAccounts(accounts: IEmployeeAccountDTO[], mustHaveMainAllocation: boolean, mustHaveDefault: boolean): ng.IPromise<IActionResult> {
        const model = {
            accounts: accounts,
            mustHaveMainAllocation: mustHaveMainAllocation,
            mustHaveDefault: mustHaveDefault
        }
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_VALIDATE_EMPLOYEE_ACCOUNTS, model);
    }

    saveEmployeeUser(employeeUser: EmployeeUserDTO, contactAddresses: ContactAddressItemDTO[], employeePositions: EmployeePositionDTO[], employeeSkills: IEmployeeSkillDTO[], userReplacement: UserReplacementDTO, employeeTax: EmployeeTaxSEDTO, saveRoles: boolean, saveAttestRoles: boolean, userRoles: UserRolesDTO[], files: FileUploadDTO[], extraFields: any[]) {
        const model = {
            actionMethod: TermGroup_TrackChangesActionMethod.Employee_Save,
            employeeUser: employeeUser,
            contactAdresses: contactAddresses,
            employeePositions: employeePositions,
            employeeSkills: employeeSkills,
            userReplacement: userReplacement,
            employeeTax: employeeTax,
            saveRoles: saveRoles,
            saveAttestRoles: saveAttestRoles,
            userRoles: userRoles,
            files: files,
            extraFields: extraFields
        };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE, model);
    }

    saveEmployeeFromTemplate(model: SaveEmployeeFromTemplateHeadDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_SAVE_EMPLOYEE_FROM_TEMPLATE, model);
    }

    createVacantEmployees(employees: CreateVacantEmployeeDTO[]): ng.IPromise<IActionResult> {
        const model = {
            employees: employees
        };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_VACANT, model);
    }

    saveEmployeeCollectiveAgreement(employeeCollectiveAgreement: EmployeeCollectiveAgreementDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_COLLECTIVE_AGREEMENT, employeeCollectiveAgreement);
    }

    saveEmployeeTemplate(employeeTemplate: EmployeeTemplateDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE, employeeTemplate);
    }

    saveEmployeeVehicle(employeeVehicle: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_VEHICLE, employeeVehicle);
    }

    saveEmployeeTaxSe(employeeTaxSeId: number, employeeId: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_TAXSE + employeeTaxSeId + "/" + employeeId, false);
    }

    getTaxSeReportURL(ids: number[], year: number) {
        const model = {
            IdsToTransfer: ids,
            year: year
        };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_CSR_EXPORT_PRINTURL, model);
    }

    getCsrResponses(ids: number[], year: number) {
        const model = {
            IdsToTransfer: ids,
            year: year
        };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_CSR_RESPONSES, model);
    }


    saveEmploymentType(employmentType: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE, employmentType);
    }

    updateEmploymentTypesState(dict: any): ng.IPromise<any> {
        const model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE_UPDATE_STATE, model);
    }

    saveEndReason(endReason: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_END_REASON, endReason);
    }

    updateEndReasonsState(dict: any) {
        const model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_END_REASON_UPDATE_STATE, model);
    }

    markEmployeesAsVacant(employeeIds: number[]): ng.IPromise<IActionResult> {
        const model = { numbers: employeeIds };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_MARK_AS_VACANT, model);
    }

    saveFollowUpType(followUpType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_FOLLOW_UP_TYPE, followUpType);
    }

    exportPayrollReview(payrollReviewHead: PayrollReviewHeadDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW_EXPORT, payrollReviewHead);
    }

    savePayrollReviewHead(payrollReviewHead: PayrollReviewHeadDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW, payrollReviewHead);
    }

    updatePayrollReview(payrollReviewHeadId: number, keepFuture: boolean) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW_UPDATE + payrollReviewHeadId + "/" + keepFuture, false);
    }

    updateFollowUpTypesState(dict: any) {
        const model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_FOLLOW_UP_TYPE_UPDATE_STATE, model);
    }

    saveVacationGroup(vacationGroup: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_VACATION_GROUP, vacationGroup);
    }

    savePosition(position: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_POSITION, position);
    }

    updateLinkedPositions() {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_UPDATE_POSITION, null);
    }

    updateSysPosionsGrid(items: ISysPositionGridDTO[], link: boolean): ng.IPromise<IActionResult> {
        return this.httpService.post(link ? Constants.WEBAPI_TIME_EMPLOYEE_UPDATE_LINK_SYS_POSITION : Constants.WEBAPI_TIME_EMPLOYEE_UPDATE_SYS_POSITION, items);
    }

    deleteEmployee(input: DeleteEmployeeDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_DELETE, input);
    }

    inactivateEmployees(employeeIds: number[]): ng.IPromise<InactivateEmployeeDTO[]> {
        const model = {
            numbers: employeeIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_INACIVATE_MULTIPLE, model);
    }

    getEmployeesForPayrollReview(fromDate: Date, payrollGroupIds: number[], payrollPriceTypeIds: number[], payrollLevelIds: number[], employeeIds?: number[]): ng.IPromise<PayrollReviewEmployeeDTO[]> {
        const model = {
            fromDate: fromDate,
            payrollGroupIds: payrollGroupIds,
            payrollPriceTypeIds: payrollPriceTypeIds,
            payrollLevelIds: payrollLevelIds,
            employeeIds: employeeIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW_EMPLOYEES, model).then(x => {
            return x.map(y => {
                let obj = new PayrollReviewEmployeeDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    validateInactivateEmployees(employeeIds: number[]): ng.IPromise<InactivateEmployeeDTO[]> {
        const model = {
            numbers: employeeIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_VALIDATE_INACTIVATE_MULTIPLE, model).then(x => {
            return x.map(y => {
                let obj = new InactivateEmployeeDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    // DELETE

    deleteCardNumber(employeeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_CARD_NUMBER + employeeId);
    }

    deleteEmployeeVehicle(employeeVehicleId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_VEHICLE + employeeVehicleId);
    }


    deleteEmployeeCalculateVacationResultHead(employeeCalculateVacationResultHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_CALCULATE_VACATION_HEAD + employeeCalculateVacationResultHeadId);
    }

    deleteEmployeeCalculateVacationResultsForEmployee(employeeCalculateVacationResultHeadId: number, employeeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_CALCULATE_VACATION_RESULT_EMPLOYEE + employeeCalculateVacationResultHeadId + "/" + employeeId);
    }

    deletePayrollReviewHead(payrollReviewHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_REVIEW + payrollReviewHeadId);
    }

    deleteEmployeeCollectiveAgreement(employeeCollectiveAgreementId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_COLLECTIVE_AGREEMENT + employeeCollectiveAgreementId);
    }

    deleteEmployeeTemplate(employeeTemplateId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE + employeeTemplateId);
    }

    deleteEmploymentType(employmentTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYMENT_TYPE + employmentTypeId);
    }

    deleteEndReason(endReasonId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_END_REASON + endReasonId);
    }

    deleteFollowUpType(followUpTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_FOLLOW_UP_TYPE + followUpTypeId);
    }

    deleteVacationGroup(vacationGroupId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_DELETE_VACATIONGROUP + vacationGroupId);
    }

    deletePosition(positionId: number): ng.IPromise<any> {
        return this.httpService.delete(Constants.WEBAPI_TIME_EMPLOYEE_POSITION + positionId);
    }
}

