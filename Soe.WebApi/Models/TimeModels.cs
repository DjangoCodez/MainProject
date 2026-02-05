using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Soe.WebApi.Models
{
    #region Core

    #region UserCompanySetting
    [TSInclude]
    public class SaveUserCompanySettingModel
    {
        [Required]
        public int SettingMainType { get; set; }

        [Required]
        public int SettingTypeId { get; set; }

        public bool BoolValue { get; set; }
        public int IntValue { get; set; }
        public string StringValue { get; set; }

    }

    #endregion

    #endregion

    #region Accounting

    public class RecalculateAccountingModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        public int? TimePeriodId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    #endregion

    #region Employee

    public class GetDefaultRoleModel
    {
        public int UserId { get; set; }
        public DateTime? Date { get; set; }
        public List<UserCompanyRoleDTO> UserCompanyRoles { get; set; }
    }

    public class ValidateEmployeeAccountsModel
    {
        public List<EmployeeAccountDTO> Accounts { get; set; }
        public bool MustHaveMainAllocation { get; set; }
        public bool MustHaveDefault { get; set; }
    }

    public class ValidateSaveEmployeeModel
    {
        public EmployeeUserDTO EmployeeUser { get; set; }
        public List<ContactAddressItem> ContactAdresses { get; set; }
    }

    public class SaveEmployeeModel
    {
        public TermGroup_TrackChangesActionMethod ActionMethod { get; set; }
        public EmployeeUserDTO EmployeeUser { get; set; }
        public List<ContactAddressItem> ContactAdresses { get; set; }
        public List<EmployeePositionDTO> EmployeePositions { get; set; }
        public List<EmployeeSkillDTO> EmployeeSkills { get; set; }
        public UserReplacementDTO UserReplacement { get; set; }
        public EmployeeTaxSEDTO EmployeeTax { get; set; }
        public bool SaveRoles { get; set; }
        public bool SaveAttestRoles { get; set; }
        public List<UserRolesDTO> UserRoles { get; set; }
        public List<FileUploadDTO> Files { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFields { get; set; }
    }

    public class SaveEmployeeNoteModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public string Note { get; set; }
    }

    public class SaveEmployeeRequestModel
    {
        [Required]
        public int EmployeeId { get; set; }
        public List<EmployeeRequestDTO> DeletedEmployeeRequests { get; set; }

        public List<EmployeeRequestDTO> EditedOrNewRequests { get; set; }
    }

    public class CreateVacantEmployeesModel
    {
        public List<CreateVacantEmployeeDTO> Employees { get; set; }
    }

    public class GetEmployeeAccountsModel
    {
        public List<int> EmployeeIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    public class GetEmployeeAccumulatorsModel
    {
        public List<int> EmployeeIds { get; set; }
        public List<int> AccumulatorIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int RangeType { get; set; }
        public TermGroup_TimeAccumulatorCompareModel CompareModel { get; set; }
        public int? OwnLimitMin { get; set; }
        public int? OwnLimitMax { get; set; }
    }

    #endregion

    #region Schedule

    #region Absence

    public class PerformAbsenceRequestPlanningActionModel
    {
        public int EmployeeRequestId { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class PerformAbsencePlanningActionModel
    {
        public EmployeeRequestDTO EmployeeRequest { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public bool ScheduledAbsence { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class PerformAbsencePlanningActionModelV2
    {
        public EmployeeRequestDTO EmployeeRequest { get; set; }
        public List<ShiftDTO> Shifts { get; set; }
        public bool ScheduledAbsence { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class CheckShiftsIncludedInAbsenceRequestModel
    {
        public int EmployeeId { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
    }

    public class EvaluateAbsenceRequestPlanningAgainstWorkRules
    {
        public int EmployeeId { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class EvaluateAbsenceRequestPlanningAgainstWorkRulesV2
    {
        public int EmployeeId { get; set; }
        public List<ShiftDTO> Shifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class GetAbsenceRequestAffectedShiftsModel
    {
        public int? TimeScheduleScenarioHeadId { get; set; }
        public EmployeeRequestDTO Request { get; set; }
        public ExtendedAbsenceSettingDTO ExtendedSettings { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus ShiftUserStatus { get; set; }
    }

    public class GetAbsenceAffectedShiftsModel
    {
        public int EmployeeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<DateTime> SelectedDays { get; set; }
        public int ShiftId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public bool IncludeAlreadyAbsence { get; set; }
        public bool IncludeLinkedShifts { get; set; }
        public bool GetAllshifts { get; set; }
        public ExtendedAbsenceSettingDTO ExtendedSettings { get; set; }
    }

    public class GetShiftsForQuickAbsenceModel
    {
        public int EmployeeId { get; set; }
        //public DateTime DateFrom { get; set; }
        //public DateTime DateTo { get; set; }
        //public List<DateTime> SelectedDays { get; set; }
        public List<int> ShiftIds { get; set; }
        //public int TimeDeviationCauseId { get; set; }
        public bool IncludeLinkedShifts { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        //public bool IncludeAlreadyAbsence { get; set; }
        //public bool GetAllshifts { get; set; }
        //public ExtendedAbsenceSettingDTO ExtendedSettings { get; set; }
    }
    public class SaveAbsenceRequestModel
    {
        public EmployeeRequestDTO Request { get; set; }
        public int EmployeeId { get; set; }
        public TermGroup_EmployeeRequestType RequestType { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public bool IsForcedDefinitive { get; set; }
    }

    public class RemoveAbsenceInScenarioModel
    {
        #region Variables
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }

        public int TimeScheduleScenarioHeadId { get; set; }

        #endregion
    }

    #endregion

    #region Annual leave 

    public class GetAnnualLeaveBalanceModel
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        public bool PreviousYear { get; set; }
    }

    [TSInclude]
    public class AnnualLeaveCalculationModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    #endregion

    #region Annual scheduled time

    public class GetAnnualScheduledTimeSummaryModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int? TimePeriodHeadId { get; set; }
    }

    #endregion

    #region EmployeePost

    #region EmployeePostModel

    public class EmployeePostChangeStatusModel
    {
        public int EmployeePostId { get; set; }
        public SoeEmployeePostStatus Status { get; set; }
    }

    #endregion

    #endregion

    #region EmployeeSchedule

    public class GetEmployeeScheduleForActivateGridModel
    {
        public bool OnlyLatest { get; set; }
        public bool AddEmptyPlacement { get; set; }
        public List<int> EmployeeIds { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class HasEmployeeOverlappingPlacementModel
    {
        public int EmployeeId { get; set; }
        public DateTime? OldDateFrom { get; set; }
        public DateTime? OldDateTo { get; set; }
        public DateTime? NewDateFrom { get; set; }
        public DateTime? NewDateTo { get; set; }
        public bool ApplyFinalSalary { get; set; }
        public EmploymentDTO ChangedEmployment { get; set; }
        public List<EmployeeSchedulePlacementGridViewDTO> EmployeePlacements { get; set; }
        public List<RecalculateTimeRecordDTO> ScheduledEmployeePlacements { get; set; }
        public List<EmploymentDTO> Employments { get; set; }
    }

    public class IsPlacementsUnchangedModel
    {
        public List<ActivateScheduleGridDTO> Items { get; set; }
        public DateTime PlacementStopDate { get; set; }
    }

    [TSInclude]
    public class SaveEmployeeScheduleModel
    {
        public List<ActivateScheduleGridDTO> Items { get; set; }
        public ActivateScheduleControlDTO Control { get; set; }
        public TermGroup_TemplateScheduleActivateFunctions Function { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public bool Preliminary { get; set; }
    }

    public class DeletePlacementModel
    {
        public ActivateScheduleGridDTO Item { get; set; }
        public ActivateScheduleControlDTO Control { get; set; }
    }

    public class ControlActivationsModel
    {
        public List<ActivateScheduleGridDTO> Items { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool IsDelete { get; set; }
    }

    public class ControlActivationModel
    {
        public int EmployeeId { get; set; }
        public DateTime? EmployeeScheduleStartDate { get; set; }
        public DateTime? EmployeeScheduleStopDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool IsDelete { get; set; }
    }

    #endregion

    #region Order planning

    public class GetUnscheduledOrdersModel
    {
        public DateTime? DateTo { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> OrderIds { get; set; }
    }

    #endregion

    #region Schedule

    public class CopyScheduleModel
    {
        [Required]
        public int SourceEmployeeId { get; set; }
        public DateTime? SourceDateEnd { get; set; }
        [Required]
        public int TargetEmployeeId { get; set; }
        [Required]
        public DateTime TargetDateStart { get; set; }
        public DateTime? TargetDateEnd { get; set; }
        public bool UseAccountingFromSourceSchedule { get; set; }
    }

    #endregion

    #region ScheduleSwap

    public class InitiateScheduleSwapModel
    {
        [Required]
        public int InitiatorEmployeeId { get; set; }
        [Required]
        public DateTime InitiatorShiftDate { get; set; }
        [Required]
        public List<int> InitiatorShiftIds { get; set; }
        [Required]
        public int SwapWithEmployeeId { get; set; }
        [Required]
        public DateTime SwapShiftDate { get; set; }
        [Required]
        public List<int> SwapWithShiftIds { get; set; }

        public string Comment { get; set; }
    }

    #endregion

    #region StaffingNeedsHead

    public class CreateStaffingNeedsHeadModel
    {
        public int Interval { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int DayTypeId { get; set; }
        public int DayOfWeek { get; set; }
        public bool WholeWeek { get; set; }
        public List<TimeScheduleTaskDTO> TimeScheduleTaskDTOs { get; set; }
        public List<IncomingDeliveryHeadDTO> IncomingDeliveryHeadDTOs { get; set; }
        public int StaffingNeedsFrequencyTimeScheduleTaskId { get; set; }
        public bool IncludeStaffingNeedsChartData { get; set; }
        public DateTime IntervalDateFrom { get; set; }
        public DateTime IntervalDateTo { get; set; }
        public List<int> DayOfWeeks { get; set; }
        public decimal AdjustPercent { get; set; }

        public DateTime CurrentDate { get; set; }
    }

    public class GenerateStaffingNeedsModel
    {
        public TermGroup_StaffingNeedHeadsFilterType NeedFilterType { get; set; }
        public int? DayTypeId { get; set; }
        public int AccountDimId { get; set; }
        public int AccountId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        // Extended properties for follow up (charts and tables in schedule planning)
        public TermGroup_TimeSchedulePlanningFollowUpCalculationType CalculationType { get; set; }
        public bool CalculateNeed { get; set; }
        public bool CalculateNeedFrequency { get; set; }
        public bool CalculateNeedRowFrequency { get; set; }
        public bool CalculateBudget { get; set; }
        public bool CalculateForecast { get; set; }
        public bool CalculateTemplateSchedule { get; set; }
        public bool CalculateTemplateScheduleForEmployeePost { get; set; }
        public bool CalculateSchedule { get; set; }
        public bool CalculateTime { get; set; }
        public bool IncludeEmpTaxAndSuppCharge { get; set; }
        public List<int> EmployeeIds { get; set; }
        public List<int> EmployeePostIds { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public bool ForceWeekView { get; set; }
    }

    public class RecalculateStaffingNeedsSummaryModel
    {
        public StaffingStatisticsIntervalRowDTO Row { get; set; }
    }

    #endregion

    #region StaffingNeedsLocationGroup

    public class SaveStaffingNeedsLocationGroupModel
    {
        [Required]
        public StaffingNeedsLocationGroupDTO Dto { get; set; }

        public List<int> ShiftTypeIds { get; set; }
    }

    #endregion

    #region Shift

    [TSInclude]
    public class GetShiftsModel
    {
        public int EmployeeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public bool LoadYesterdayAlso { get; set; }
        public List<int> EmployeeIds { get; set; }
        public TimeSchedulePlanningMode PlanningMode { get; set; }
        public TimeSchedulePlanningDisplayMode DisplayMode { get; set; }
        public bool IncludeSecondaryCategories { get; set; }
        public bool IncludeBreaks { get; set; }
        public bool IncludeGrossNetAndCost { get; set; }
        public bool IncludePreliminary { get; set; }
        public bool LoadTasks { get; set; }
        public bool IncludeEmploymentTaxAndSupplementChargeCost { get; set; }
        public bool IncludeShiftRequest { get; set; }
        public bool IncludeAbsenceRequest { get; set; }
        public bool CheckToIncludeDeliveryAdress { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public bool IncludeHolidaySalary { get; set; }
        public bool IncludeLeisureCodes { get; set; }
    }

    public class SaveShiftsModel
    {
        [Required]
        public string Source { get; set; }
        [Required]
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }

        public bool UpdateBreaks { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public bool AdjustTasks { get; set; }
        public int MinutesMoved { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    [TSInclude]
    public class SaveShiftsModelV2
    {
        [Required]
        public string Source { get; set; }
        [Required]
        public List<ShiftDTO> Shifts { get; set; }

        public bool UpdateBreaks { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public bool AdjustTasks { get; set; }
        public int MinutesMoved { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class CreateAnnualLeaveShiftModel
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public int EmployeeId { get; set; }
    }

    public class SaveOrderAssignmentsModel
    {
        public int EmployeeId { get; set; }
        public int OrderId { get; set; }
        public int? ShiftTypeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public TermGroup_AssignmentTimeAdjustmentType AssignmentTimeAdjustmentType { get; set; }
        public bool SkipXEMailOnChanges { get; set; }

    }

    [TSInclude]
    public class DragShiftModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public int SourceShiftId { get; set; }
        [Required]
        public int TargetShiftId { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public Guid? TargetLink { get; set; }
        [Required]
        public bool UpdateLinkOnTarget { get; set; }
        [Required]
        public int TimeDeviationCauseId { get; set; }
        public int? EmployeeChildId { get; set; }
        [Required]
        public bool WholeDayAbsence { get; set; }
        [Required]
        public bool SkipXEMailOnChanges { get; set; }
        public bool CopyTaskWithShift { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public bool IncludeOnDutyShifts { get; set; }
        public List<int> IncludedOnDutyShiftIds { get; set; }
    }

    public class DragTemplateShiftModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public int SourceShiftId { get; set; }
        [Required]
        public int TargetShiftId { get; set; }
        [Required]
        public int SourceTemplateHeadId { get; set; }
        [Required]
        public int TargetTemplateHeadId { get; set; }
        [Required]
        public DateTime SourceDate { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        public int? EmployeeId { get; set; }
        public int? EmployeePostId { get; set; }
        public Guid? TargetLink { get; set; }
        [Required]
        public bool UpdateLinkOnTarget { get; set; }
        [Required]
        public int TimeDeviationCauseId { get; set; }
        public bool CopyTaskWithShift { get; set; }
    }

    [TSInclude]
    public class DragShiftsModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public List<int> SourceShiftIds { get; set; }
        [Required]
        public int OffsetDays { get; set; }
        [Required]
        public int TargetEmployeeId { get; set; }
        [Required]
        public bool SkipXEMailOnChanges { get; set; }
        public bool CopyTaskWithShift { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public bool IncludeOnDutyShifts { get; set; }
        public List<int> IncludedOnDutyShiftIds { get; set; }
    }

    public class DragTemplateShiftsModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public DateTime FirstTargetDate { get; set; }
        [Required]
        public List<int> SourceShiftIds { get; set; }
        [Required]
        public DateTime FirstSourceDate { get; set; }
        [Required]
        public int OffsetDays { get; set; }
        [Required]
        public int SourceTemplateHeadId { get; set; }
        [Required]
        public int TargetTemplateHeadId { get; set; }
        public int TargetEmployeeId { get; set; }
        public int TargetEmployeePostId { get; set; }
        public bool CopyTaskWithShift { get; set; }
    }

    [TSInclude]
    
    public class DeleteShiftsModel
    {
        public List<int> ShiftIds { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public List<int> IncludedOnDutyShiftIds { get; set; }
    }

    public class HandleShiftModel
    {
        [Required]
        public HandleShiftAction Action { get; set; }
        [Required]
        public int TimeScheduleTemplateBlockId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int EmployeeId { get; set; }
        public int SwapTimeScheduleTemplateBlockId { get; set; }
        public bool PreventAutoPermissions { get; set; }
    }

    public class GetShiftPeriodsModel
    {
        public int EmployeeId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<int> EmployeeIds { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public List<int> DeviationCauseIds { get; set; }
        public List<TermGroup_TimeScheduleTemplateBlockType> BlockTypes { get; set; }
        public TimeSchedulePlanningDisplayMode DisplayMode { get; set; }
        public bool IncludeGrossNetAndCost { get; set; }
        public bool IncludePreliminary { get; set; }
        public bool IncludeEmploymentTaxAndSupplementChargeCost { get; set; }
        public bool IncludeHolidaySalary { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class GetShiftPeriodDetailsModel
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public List<int> EmployeeIds { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public List<int> DeviationCauseIds { get; set; }
        public List<TermGroup_TimeScheduleTemplateBlockType> BlockTypes { get; set; }
        public bool IncludePreliminary { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class SplitShiftModel
    {
        [Required]
        public TimeSchedulePlanningDayDTO Shift { get; set; }
        [Required]
        public int EmployeeId1 { get; set; }
        [Required]
        public int EmployeeId2 { get; set; }
        [Required]
        public DateTime SplitTime { get; set; }
        [Required]
        public bool KeepShiftsTogether { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        [Required]
        public bool SkipXEMailOnChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    [TSInclude]
    public class SplitShiftModelV2
    {
        [Required]
        public ShiftDTO Shift { get; set; }
        [Required]
        public int EmployeeId1 { get; set; }
        [Required]
        public int EmployeeId2 { get; set; }
        [Required]
        public DateTime SplitTime { get; set; }
        [Required]
        public bool KeepShiftsTogether { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        [Required]
        public bool SkipXEMailOnChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class SplitTemplateShiftModel
    {
        [Required]
        public TimeSchedulePlanningDayDTO SourceShift { get; set; }
        [Required]
        public int SourceTemplateHeadId { get; set; }
        [Required]
        public int? EmployeeId1 { get; set; }
        [Required]
        public int? EmployeeId2 { get; set; }
        public int? EmployeePostId1 { get; set; }
        [Required]
        public int? EmployeePostId2 { get; set; }
        public int TemplateHeadId1 { get; set; }
        [Required]
        public int TemplateHeadId2 { get; set; }
        [Required]
        public DateTime SplitTime { get; set; }
        [Required]
        public bool KeepShiftsTogether { get; set; }

    }

    public class GetCycleWorkTimeMinutesModel
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
    }

    public class GetGrossNetCostModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public bool IncludeSecondaryCategories { get; set; }
        [Required]
        public bool IncludeBreaks { get; set; }
        [Required]
        public bool IncludePreliminary { get; set; }
        [Required]
        public bool IncludeEmploymentTaxAndSupplementChargeCost { get; set; }
        public bool IncludeHolidaySalary { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class DefToFromPrelShiftModel
    {
        [Required]
        public bool PrelToDef { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public List<int> EmployeeIds { get; set; }
        public bool IncludeScheduleShifts { get; set; }
        public bool includeStandbyShifts { get; set; }
    }

    [TSInclude]
    public class GetAvailableEmployeesModel
    {
        [Required]
        public List<int> TimeScheduleTemplateBlockIds { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public bool FilterOnShiftType { get; set; }
        [Required]
        public bool FilterOnAvailability { get; set; }
        [Required]
        public bool FilterOnSkills { get; set; }
        [Required]
        public bool FilterOnWorkRules { get; set; }
        public int? FilterOnMessageGroupId { get; set; }
    }

    public class PrintTimeEmploymentContractShortSubstituteModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public List<DateTime> Dates { get; set; }
        [Required]
        public bool PrintedFromScheduleplanning { get; set; }
        public bool SavePrintout { get; set; }
    }

    public class ExportShiftsToExcelModel
    {
        [Required]
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        [Required]
        public List<EmployeeListDTO> Employees { get; set; }
        [Required]
        public List<DateTime> Dates { get; set; }
        public ICollection<ReportDataSelectionDTO> Selections { get; set; }

    }

    public class CreateTimeSchedulePlanningDayDTOsFromEmployeePostModel
    {
        public int EmployeePostId { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class CreateTimeSchedulePlanningDayDTOsFromEmployeePostsModel
    {
        public List<int> EmployeePostIds { get; set; }
        public DateTime FromDate { get; set; }
    }

    public class GetEmployeePostShiftsModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<int> EmployeePostIds { get; set; }
        public bool LoadTasks { get; set; }
    }

    public class UnscheduledTasksModel
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public List<int> ShiftTypeIds { get; set; }
        public SoeStaffingNeedType Type { get; set; }
    }

    #endregion

    #region ShiftTask

    public class AssignTaskToEmployeeModel
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public List<StaffingNeedsTaskDTO> TaskDTOs { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
    }

    public class EvaluateAssignTaskToEmployeeAgainstWorkRulesModel
    {
        public int DestinationEmployeeId { get; set; }
        public DateTime DestinationDate { get; set; }
        public List<StaffingNeedsTaskDTO> TaskDTOs { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
    }

    public class AssignTaskToEmployeePostModel
    {
        public List<StaffingNeedsTaskDTO> Tasks { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public DateTime Date { get; set; }
    }

    #endregion

    #region TimeBreakTemplate

    public class TimeBreakTemplatesModel
    {
        public List<TimeBreakTemplateGridDTO> BreakTemplates { get; set; }
    }

    public class CreateBreaksFromTemplatesForEmployeeModel
    {
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public int EmployeeId { get; set; }
    }

    public class CreateBreaksFromTemplatesForEmployeesModel
    {
        public DateTime Date { get; set; }
        public List<int> EmployeeIds { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    #endregion

    #region TimeLeisureCode

    public class AllocateLeisureDaysModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime StopDate { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
    }

    #endregion

    #region TimeScheduleScenarioHead

    public class GetTimeScheduleScenarioHeadsModel
    {
        public List<int> ValidAccountIds { get; set; }
        public bool AddEmptyRow { get; set; }
    }

    public class SaveTimeScheduleScenarioHeadModel
    {
        [Required]
        public TimeScheduleScenarioHeadDTO ScenarioHead { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public bool IncludeAbsence { get; set; }
        public int DateFunction { get; set; }
    }

    #endregion

    #region TimeScheduleTemplate

    public class SaveTimeScheduleTemplateModel
    {
        public TimeScheduleTemplateHeadDTO Head { get; set; }
        public List<TimeScheduleTemplateBlockDTO> Blocks { get; set; }
    }

    public class SaveTimeScheduleTemplateAndPlacementModel
    {
        public bool SaveTemplate { get; set; }
        public bool SavePlacement { get; set; }
        public ActivateScheduleControlDTO Control { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int TemplateNoOfDays { get; set; }
        public DateTime TemplateStartDate { get; set; }
        public DateTime? TemplateStopDate { get; set; }
        public DateTime? FirstMondayOfCycle { get; set; }
        public DateTime? PlacementDateFrom { get; set; }
        public DateTime? PlacementDateTo { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<DateTime> ActivateDates { get; set; }
        public int? ActivateDayNumber { get; set; }
        public bool SimpleSchedule { get; set; }
        public bool StartOnFirstDayOfWeek { get; set; }
        public bool Preliminary { get; set; }
        public bool Locked { get; set; }
        public int EmployeeId { get; set; }
        public int? CopyFromTimeScheduleTemplateHeadId { get; set; }
        public bool UseAccountingFromSourceSchedule { get; set; }

        public int DayNumberFrom { get; set; }
        public int DayNumberTo { get; set; }
        public bool SkipXEMailOnChanges { get; set; }

        public string Key
        {
            get
            {

                return $"{TimeScheduleTemplateHeadId}#{EmployeeId}";
            }
        }
    }

    public class GetTimeScheduleTemplateChanges
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int TimeScheduleTemplateHeadId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        [Required]
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
    }

    public class CreateStringFromShiftsModel
    {
        [Required]
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
    }

    #endregion

    #region Work rules

    public class SaveEvaluateAllWorkRulesByPassModel
    {
        public EvaluateWorkRulesActionResult Result { get; set; }
        public int EmployeeId { get; set; }
    }

    public class EvaluateAllWorkRulesModel
    {
        [Required]
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime StopDate { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    public class EvaluateWorkRulesModel
    {
        [Required]
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    [TSInclude]
    public class EvaluateWorkRulesModelV2
    {
        [Required]
        public List<ShiftDTO> Shifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    [TSInclude]
    public class EvaluateWorkRulesDragModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public int SourceShiftId { get; set; }
        [Required]
        public int TargetShiftId { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        [Required]
        public bool WholeDayAbsence { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public bool? FromQueue { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    public class EvaluateWorkRulesDragTemplateModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public int SourceShiftId { get; set; }
        [Required]
        public int TargetShiftId { get; set; }
        [Required]
        public int SourceTemplateHeadId { get; set; }
        [Required]
        public int TargetTemplateHeadId { get; set; }
        [Required]
        public DateTime SourceDate { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        public int? EmployeeId { get; set; }
        public int? EmployeePostId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
    }

    [TSInclude]
    public class EvaluateWorkRulesDragMultipleModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public List<int> SourceShiftIds { get; set; }
        [Required]
        public int OffsetDays { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    public class EvaluateWorkRulesDragTemplateMultipleModel
    {
        [Required]
        public DragShiftAction Action { get; set; }
        [Required]
        public List<int> SourceShiftIds { get; set; }
        [Required]
        public int SourceTemplateHeadId { get; set; }
        [Required]
        public DateTime FirstSourceDate { get; set; }
        [Required]
        public int TargetTemplateHeadId { get; set; }
        [Required]
        public DateTime FirstTargetDate { get; set; }
        [Required]
        public int OffsetDays { get; set; }
        public int? EmployeeId { get; set; }
        public int? EmployeePostId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
    }

    public class EvaluateWorkRulesSplitModel
    {
        [Required]
        public TimeSchedulePlanningDayDTO Shift { get; set; }
        [Required]
        public int EmployeeId1 { get; set; }
        [Required]
        public int EmployeeId2 { get; set; }
        [Required]
        public DateTime SplitTime { get; set; }
        [Required]
        public bool KeepShiftsTogether { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    [TSInclude]
    public class EvaluateWorkRulesSplitModelV2
    {
        [Required]
        public ShiftDTO Shift { get; set; }
        [Required]
        public int EmployeeId1 { get; set; }
        [Required]
        public int EmployeeId2 { get; set; }
        [Required]
        public DateTime SplitTime { get; set; }
        [Required]
        public bool KeepShiftsTogether { get; set; }
        [Required]
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
    }

    #endregion

    #endregion

    #region Time

    #region Deviations

    public class ApplyCalculationFunctionForEmployeeModel
    {
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }
        public int Option { get; set; }
    }

    public class ApplyCalculationFunctionForEmployeesModel
    {
        public List<AttestEmployeesDaySmallDTO> Items { get; set; }
        public int Option { get; set; }

        public int? TimeScheduleScenarioHeadId { get; set; }
    }

    public class ApplyCalculationFunctionValidationModel
    {
        [Required]
        public List<AttestEmployeeDayDTO> Items { get; set; }
        [Required]
        public SoeTimeAttestFunctionOption Option { get; set; }
        [Required]
        public int EmployeeId { get; set; }
    }

    public class DeviationsAfterEmploymentModel
    {
        [Required]
        public EmployeeDeviationAfterEmploymentDTO Deviations { get; set; }
    }

    public class CreateTransactionsForPlannedPeriodCalculationModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
    }

    public class PeriodCalculationForEmployeesModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
    }

    #endregion

    #region EarnedHoliday
    [TSInclude]
    public class EarnedHolidayModel
    {
        public int Year { get; set; }
        public int HolidayId { get; set; }
        public bool LoadSuggestions { get; set; }
        public List<EmployeeEarnedHolidayDTO> EmployeeEarnedHolidays { get; set; }
    }
    [TSInclude]
    public class ManageTransactionsForEarnedHolidayModel
    {
        public int Year { get; set; }
        public int HolidayId { get; set; }
        public List<int> EmployeeIds { get; set; }
    }

    #endregion

    #region ExportSalary

    public class ExportSalaryModel
    {
        public List<int> EmployeeIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int ExportTarget { get; set; }
        public bool LockPeriod { get; set; }
        public bool IsPreliminary { get; set; }
    }

    #endregion

    #region ExportSalaryPayment

    public class ExportSalaryPaymentModel
    {
        public int TimePeriodHeadId { get; set; }
        public int TimePeriodId { get; set; }
        public List<int> EmployeeIds { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? DebitDate { get; set; }
    }

    public class ExportSalaryPaymentExtendedModel
    {
        public int BasedOnTimeSalarPaymentExportId { get; set; }
        public DateTime CurrencyDate { get; set; }
        public decimal CurrencyRate { get; set; }
        public TermGroup_Currency Currency { get; set; }
    }

    #endregion

    #region HibernationAbscense
    public class SaveTimeHibernatingAbsenceHeadModel
    {
        [Required]
        public TimeHibernatingAbsenceHeadDTO TimeHibernatingAbsenceHead { get; set; }
    }
    #endregion

    #region Attest

    public class AttestTreeModel
    {
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int TimePeriodId { get; set; }
        public TermGroup_AttestTreeGrouping Grouping { get; set; }
        public TermGroup_AttestTreeSorting Sorting { get; set; }
        public TimeEmployeeTreeSettings Settings { get; set; }

        public void Beautify()
        {
            if (this.Settings?.SearchPattern == Constants.SOE_WEBAPI_STRING_EMPTY)
                this.Settings.SearchPattern = string.Empty;
        }
    }

    public class RefreshAttestTreeModel
    {
        [Required]
        public TimeEmployeeTreeDTO Tree { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime StopDate { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public TimeEmployeeTreeSettings Settings { get; set; }
    }

    public class RefreshAttestTreeGroupNodeModel
    {
        [Required]
        public TimeEmployeeTreeDTO Tree { get; set; }
        [Required]
        public TimeEmployeeTreeGroupNodeDTO GroupNode { get; set; }
    }

    public class GetAttestTreeWarningsModel
    {
        [Required]
        public TimeEmployeeTreeDTO Tree { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime StopDate { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public bool DoShowOnlyWithWarnings { get; set; }
        [Required]
        public bool FlushCache { get; set; }
    }

    public class GetTimeAttestEmployeePeriodsModel
    {
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        [Required]
        public int? TimePeriodId { get; set; }
        [Required]
        public TermGroup_AttestTreeGrouping Grouping { get; set; }
        [Required]
        public int GroupId { get; set; }
        [Required]
        public List<int> VisibleEmployeeIds { get; set; }
        [Required]
        public bool IsAdditional { get; set; }
        [Required]
        public bool IncludeAdditionalEmployees { get; set; }
        [Required]
        public bool DoNotShowDaysOutsideEmployeeAccount { get; set; }
        [Required]
        public string CacheKeyToUse { get; set; }
        [Required]
        public bool FlushCache { get; set; }
    }

    public class GetTimeAttestEmployeePeriodsPreviewModel
    {
        [Required]
        public DateTime DateFrom { get; set; }
        [Required]
        public DateTime DateTo { get; set; }
        [Required]
        public TimeEmployeeTreeDTO Tree { get; set; }
        [Required]
        public TimeEmployeeTreeGroupNodeDTO GroupNode { get; set; }
    }

    public class SaveGeneratedDeviationsModel
    {
        [Required]
        public List<AttestEmployeeDayTimeBlockDTO> TimeBlocks { get; set; }
        [Required]
        public List<AttestEmployeeDayTimeCodeTransactionDTO> TimeCodeTransactions { get; set; }
        [Required]
        public List<AttestPayrollTransactionDTO> TimePayrollTransactions { get; set; }
        [Required]
        public List<ApplyAbsenceDTO> ApplyAbsences { get; set; }
        [Required]
        public int TimeBlockDateId { get; set; }
        [Required]
        public int TimeScheduleTemplatePeriodId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public List<int> PayrollImportEmployeeTransactionIds { get; set; }
    }

    public class SaveAttestEmployeeAdditionDeductionTransactionModel
    {
        [Required]
        public List<AttestEmployeeAdditionDeductionDTO> AttestEmployeeAdditionDeductions { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public int? TimePeriodId { get; set; }
        public DateTime? StandsOnDate { get; set; }
    }

    public class RunAutoAttestModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime StopDate { get; set; }
    }

    public class ReverseTransactionsModel
    {
        [Required]
        public List<DateTime> Dates { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? TimePeriodId { get; set; }
    }

    public class SaveAttestForEmployeesModel
    {
        [Required]
        public int CurrentEmployeeId { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public int? TimePeriodId { get; set; }
        [Required]
        public DateTime? StartDate { get; set; }
        [Required]
        public DateTime? StopDate { get; set; }
        [Required]
        public bool IsPayrollAttest { get; set; }
    }

    public class SaveAttestForEmployeeModel
    {
        [Required]
        public List<SaveAttestEmployeeDayDTO> Items { get; set; } //TODO: Days
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool IsMySelf { get; set; }
        [Required]
        public bool IsPayrollAttest { get; set; }
    }

    public class SaveAttestForEmployeeValidationModel
    {
        [Required]
        public List<AttestEmployeeDayDTO> Items { get; set; } //TODO: Days
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool IsMySelf { get; set; }
    }

    [TSInclude]
    public class SaveAttestForTransactionsModel
    {
        [Required]
        public List<SaveAttestTransactionDTO> Items { get; set; }
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public bool IsMySelf { get; set; }
    }

    [TSInclude]
    public class SaveAttestForTransactionsValidationModel
    {
        [Required]
        public List<AttestPayrollTransactionDTO> Items { get; set; }
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public bool IsMySelf { get; set; }
    }

    public class SaveAttestForAdditionDeductionsModel
    {
        [Required]
        public List<SaveAttestTransactionDTO> SaveItems { get; set; }
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool IsMySelf { get; set; }
    }

    public class SaveAttestForAdditionDeductionsValidationModel
    {
        [Required]
        public List<AttestEmployeeAdditionDeductionTransactionDTO> TransactionItems { get; set; }
        [Required]
        public int AttestStateToId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public bool IsMySelf { get; set; }
    }

    public class UnlockDayModel
    {
        [Required]
        public List<SaveAttestEmployeeDayDTO> Items { get; set; } //TODO: Days
        [Required]
        public int EmployeeId { get; set; }
    }

    public class SendAttestReminderModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime StopDate { get; set; }
        [Required]
        public bool DoSendToExecutive { get; set; }
        [Required]
        public bool DoSendToEmployee { get; set; }
    }

    public class SaveTimeStampsModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public bool DiscardBreakEvaluation { get; set; }
        public List<AttestEmployeeDayTimeStampDTO> Entries { get; set; }
    }

    public class ValidateDeviationChangeModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int TimeBlockId { get; set; }
        [Required]
        public string TimeBlockGuidId { get; set; }
        [Required]
        public List<AttestEmployeeDayTimeBlockDTO> TimeBlocks { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime StopTime { get; set; }
        public SoeTimeBlockClientChange ClientChange { get; set; }
        public bool OnlyUseInTimeTerminal { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? EmployeeChildId { get; set; }
        public string Comment { get; set; }
        public AccountingSettingsRowDTO AccountSetting { get; set; }
    }

    public class GetUnhandledShiftChangesModel
    {
        public List<int> EmployeeIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
    public class RecalculateUnhandledShiftChangesModel
    {
        public List<TimeUnhandledShiftChangesEmployeeDTO> UnhandledEmployees { get; set; }
        public bool DoRecalculateShifts { get; set; }
        public bool DoRecalculateExtraShifts { get; set; }
    }

    #endregion

    #region SaveTimeAbsenceDetailsModel

    public class SaveTimeAbsenceDetailsModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public List<TimeAbsenceDetailDTO> TimeAbsenceDetails { get; set; }
    }

    #endregion

    #region TimePeriod
    [TSInclude]
    public class SaveTimePeriodHeadModel
    {
        [Required]
        public TimePeriodHeadDTO TimePeriodHead { get; set; }
        public bool RemovePeriodLinks { get; set; }
    }

    #endregion

    #region TimeStamp   
    [TSInclude]
    public class SearchTimeStampModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public DateTime dateFrom { get; set; }
        [Required]
        public DateTime dateTo { get; set; }
    }

    #endregion

    #region AnnualLeaveTransactions

    [TSInclude]
    public class SearchAnnualLeaveTransactionModel
    {
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public DateTime dateFrom { get; set; }
        [Required]
        public DateTime dateTo { get; set; }
    }

    #endregion

    #endregion

    #region Payroll

    #region AccountProvision
    [TSInclude]
    public class AccountProvisionTransactionsModel
    {
        [Required]
        public List<AccountProvisionTransactionGridDTO> Transactions { get; set; }
    }

    #endregion

    #region EmployeeCalculateVacationResult

    public class UpdateEmployeeCalculateVacationResultModel
    {
        [Required]
        public int EmployeeCalculateVacationResultHeadId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public List<EmployeeCalculateVacationResultDTO> Results { get; set; }
    }

    #endregion

    #region FixedPayrollRow

    public class SaveFixedPayrollRowsModel
    {
        [Required]
        public List<FixedPayrollRowDTO> rows { get; set; }

        [Required]
        public int employeeId { get; set; }
    }

    #endregion

    #region PayrollCalculation

    public class EmployeesForTimePeriodModel
    {
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
    }

    public class PayrollCalculationTreeModel
    {
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public TermGroup_AttestTreeGrouping Grouping { get; set; }
        [Required]
        public TermGroup_AttestTreeSorting Sorting { get; set; }
        [Required]
        public TimeEmployeeTreeSettings Settings { get; set; }

        public void Beautify()
        {
            if (this.Settings?.SearchPattern == Constants.SOE_WEBAPI_STRING_EMPTY)
                this.Settings.SearchPattern = string.Empty;
        }
    }

    public class RefreshPayrollCalculationTreeModel
    {
        [Required]
        public TimeEmployeeTreeDTO Tree { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public TimeEmployeeTreeSettings Settings { get; set; }
    }

    public class PayrollCalculationTreeWarningsModel
    {
        [Required]
        public TimeEmployeeTreeDTO Tree { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public bool DoShowOnlyWithWarnings { get; set; }
        [Required]
        public TermGroup_TimeTreeWarningFilter WarningFilter { get; set; }
        [Required]
        public bool FlushCache { get; set; }
    }

    public class GetPayrollCalculationPeriodSumModel
    {
        public List<PayrollCalculationProductDTO> PayrollCalculationProducts { get; set; }
    }

    public class GetPayrollCalculationEmployeePeriodsModel
    {
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public TermGroup_AttestTreeGrouping Grouping { get; set; }
        [Required]
        public int GroupId { get; set; }
        [Required]
        public List<int> VisibleEmployeeIds { get; set; }
        [Required]
        public string CacheKeyToUse { get; set; }
        [Required]
        public bool FlushCache { get; set; }
        [Required]
        public bool IgnoreEmploymentStopDate { get; set; }
    }

    public class RecalculatePayrollPeriodModel
    {
        [Required]
        public string Key { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public bool IncludeScheduleTransactions { get; set; }
        [Required]
        public bool IgnoreEmploymentStopDate { get; set; }
    }

    public class CreateFinalSalaryModel
    {
        public int EmployeeId { get; set; }
        public int? TimePeriodId { get; set; }
        public bool CreateReport { get; set; }
    }

    public class CreateFinalSalariesModel
    {
        public List<int> EmployeeIds { get; set; }
        public int? TimePeriodId { get; set; }
        public bool CreateReport { get; set; }
    }

    public class AssignPayrollTransactionsToTimePeriodModel
    {
        [Required]
        public List<AttestPayrollTransactionDTO> Transactions { get; set; }
        [Required]
        public List<AttestPayrollTransactionDTO> ScheduleTransactions { get; set; }
        [Required]
        public TimePeriodDTO TimePeriod { get; set; }
        [Required]
        public TermGroup_TimePeriodType PeriodType { get; set; }
        [Required]
        public int EmployeeId { get; set; }
    }
    public class SaveAddedTransactionModel
    {
        [Required]
        public AttestPayrollTransactionDTO Transaction { get; set; }
        [Required]
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public bool IgnoreEmploymentHasEnded { get; set; }
    }

    public class SaveEmployeeTimePeriodProductSettingModel
    {
        [Required]
        public EmployeeTimePeriodProductSettingDTO setting { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int TimePeriodId { get; set; }
    }

    public class PayrollWarningsEmployeeModel
    {
        [Required]
        public int EmployeeId { get; set; }
        [Required]
        public int EmployeeTimePeriodId { get; set; }
        [Required]
        public List<PayrollCalculationProductDTO> PayrollCalculationProducts { get; set; }
        public bool ShowDeleted { get; set; }
    }
    public class PayrollWarningsGroupModel
    {
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
        public List<int> EmployeeTimePeriodsIds { get; set; }
        public bool ShowDeleted { get; set; }
    }
    public class PayrollWarningsCalculateModel
    {
        [Required]
        public int TimePeriodId { get; set; }
        [Required]
        public List<int> EmployeeIds { get; set; }
    }

    #endregion

    #region PayrollGroupReport

    public class PayrollGroupContractReportPrintModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int EmploymentId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public int ReportTemplateTypeId { get; set; }

        [Required]
        public List<DateTime> DateChanges { get; set; }

        [Required]
        public bool PrintedFromScheduleplanning { get; set; }
    }

    #endregion

    #region PayrollImport

    public class PayrollImportExecuteModel
    {
        [Required]
        public int PayrollImportHeadId { get; set; }
        public List<int> PayrollImportEmployeeIds { get; set; }
    }

    public class RollbackPayrollImportExecuteModel
    {
        [Required]
        public int PayrollImportHeadId { get; set; }
        public List<int> PayrollImportEmployeeIds { get; set; }
        public bool RollbackImport { get; set; }
        public bool RollbackOutcomeForAllEmployees { get; set; }
        public bool RollbackFileContentForAllEmployees { get; set; }

    }

    #endregion

    #region PayrollPriceFormula

    public class EvaluateFormulaModel
    {
        [Required]
        public string Formula { get; set; }

        [Required]
        public List<string> Identifiers { get; set; }
    }

    #endregion

    #region PayrollProductDistributionRule
    [TSInclude]
    public class SavePayrollProductDistributionRuleHeadModel
    {
        [Required]
        public PayrollProductDistributionRuleHeadDTO PayrollProductDistributionRuleHead { get; set; }
    }

    #endregion

    #region PayrollReview

    public class PayrollReviewEmployeeModel
    {
        [Required]
        public DateTime FromDate { get; set; }
        [Required]
        public List<int> PayrollGroupIds { get; set; }
        [Required]
        public List<int> PayrollPriceTypeIds { get; set; }
        public List<int?> PayrollLevelIds { get; set; }
        public List<int> EmployeeIds { get; set; }
    }

    #endregion

    #region PayrollStartValue

    public class PayrollStartValueRowsModel
    {
        [Required]
        public List<PayrollStartValueRowDTO> StartValueRows { get; set; }
        public int PayrollStartValueHeadId { get; set; }
    }

    #endregion

    #region Retroactive payroll

    public class RetroactivePayrollModel
    {
        [Required]
        public RetroactivePayrollDTO RetroactivePayroll { get; set; }
        public bool IncludeAlreadyCalculated { get; set; }

        //Optional
        public List<int> FilterEmployeeIds { get; set; }
    }

    public class GetRetroactiveEmployeesModel
    {
        public int RetroactivePayrollId { get; set; }
        public int TimePeriodId { get; set; }
        public bool IgnoreEmploymentStopDate { get; set; }
        public List<int> FilterEmployeeIds { get; set; }
    }

    public class FilterRetroactiveEmployeesModel
    {
        public int AccountOrCategoryId { get; set; }
        public List<RetroactivePayrollEmployeeDTO> Employees { get; set; }
    }

    public class SaveRetroactivePayrollOutcomeModel
    {
        [Required]
        public List<RetroactivePayrollOutcomeDTO> RetroactivePayrollOutcomeDTOs { get; set; }
        [Required]
        public int RetroactivePayrollId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
    }

    #endregion

    #region VacationYearEnd

    public class CreateVacationYearEndModel
    {
        [Required]
        public TermGroup_VacationYearEndHeadContentType ContentType { get; set; }
        [Required]
        public List<int> ContentTypeIds { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }

    public class ValidateVacationYearEndModel
    {
        public DateTime Date { get; set; }
        public List<int> VacationGroupIds { get; set; }
        public List<int> EmployeeIds { get; set; }
    }

    #endregion

    #endregion
}