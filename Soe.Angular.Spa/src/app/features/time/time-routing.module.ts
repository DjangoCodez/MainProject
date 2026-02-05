import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'employee/annualleavebalance',
    loadChildren: () =>
      import('./annual-leave-balance/annual-leave-balance.module').then(
        m => m.AnnualLeaveBalanceModule
      ),
  },
  {
    path: 'employee/annualleavegroups',
    loadChildren: () =>
      import('./annual-leave-groups/annual-leave-groups.module').then(
        m => m.AnnualLeaveGroupsModule
      ),
  },
  {
    path: 'employee/cardnumbers',
    loadChildren: () =>
      import('./employee-card-numbers/employee-card-numbers.module').then(
        m => m.EmployeeCardNumbersModule
      ),
  },
  {
    path: 'employee/categories',
    loadChildren: () =>
      import('../../shared/features/category/category.module').then(
        m => m.CategoryModule
      ),
  },
  {
    path: 'employee/collectiveagreements',
    loadChildren: () =>
      import('./collective-agreements/collective-agreements.module').then(
        m => m.CollectiveAgreementsModule
      ),
  },
  {
    path: 'employee/csr/export_angular',
    loadChildren: () =>
      import('./employee-csr-export/employee-csr-export.module').then(
        m => m.EmployeeCsrExportModule
      ),
  },
  {
    path: 'employee/employmenttypes',
    loadChildren: () =>
      import('./employment-types/employment-types.module').then(
        m => m.EmploymentTypesModule
      ),
  },
  {
    path: 'employee/endreasons',
    loadChildren: () =>
      import('./end-reasons/end-reasons.module').then(m => m.EndReasonsModule),
  },
  {
    path: 'employee/extrafields',
    loadChildren: () =>
      import('../../shared/features/extra-fields/extra-fields.module').then(
        m => m.ExtraFieldsModule
      ),
  },
  {
    path: 'employee/followuptypes',
    loadChildren: () =>
      import('./employee-followup-types/employee-followup-types.module').then(
        m => m.EmployeeFollowupTypesModule
      ),
  },
  {
    path: 'employee/groups',
    loadChildren: () =>
      import('./employee-groups/employee-groups.module').then(
        m => m.EmployeeGroupsModule
      ),
  },
  {
    path: 'employee/payrolllevels',
    loadChildren: () =>
      import('./payroll-levels/payroll-levels.module').then(
        m => m.PayrollLevelsModule
      ),
  },
  {
    path: 'employee/positions',
    loadChildren: () =>
      import('./employee-position/employee-position.module').then(
        m => m.EmployeePositionModule
      ),
  },
  {
    path: 'export',
    loadChildren: () =>
      import('./export/export.module').then(m => m.ExportModule),
  },
  {
    path: 'export/xeconnect',
    loadChildren: () =>
      import('../../shared/features/export/export.module').then(
        m => m.ExportModule
      ),
  },
  {
    path: 'import/excelimport',
    loadChildren: () =>
      import(
        '../../shared/features/import/excel-import/excel-import.module'
      ).then(m => m.ExcelImportModule),
  },
  {
    path: 'payroll/accountprovision/accountprovisionbase',
    loadChildren: () =>
      import('./account-provision-base/account-provision-base.module').then(
        m => m.AccountProvisionBaseModule
      ),
  },
  {
    path: 'payroll/accountprovision/accountprovisiontransaction',
    loadChildren: () =>
      import(
        './account-provision-transactions/account-provision-transactions.module'
      ).then(m => m.AccountProvisionTransactionsModule),
  },
  {
    path: 'payroll/unionfee',
    loadChildren: () =>
      import('./union-fees/union-fees.module').then(m => m.UnionFeesModule),
  },
  {
    path: 'payroll/worktimeaccount',
    loadChildren: () =>
      import('./time-work-account/time-work-account.module').then(
        m => m.TimeWorkAccountModule
      ),
  },
  {
    path: 'preferences/extrafields',
    loadChildren: () =>
      import('../../shared/features/extra-fields/extra-fields.module').then(
        m => m.ExtraFieldsModule
      ),
  },
  {
    path: 'preferences/needssettings',
    loadChildren: () =>
      import('./staffing-needs/staffing-needs.module').then(
        m => m.StaffingNeedsModule
      ),
  },
  {
    path: 'preferences/salarysettings/pricetype',
    loadChildren: () =>
      import('./payroll-price-types/payroll-price-types.module').then(
        m => m.PayrollPriceTypesModule
      ),
  },
  {
    path: 'preferences/schedulesettings/daytypes',
    loadChildren: () =>
      import('./day-types/day-types.module').then(m => m.DayTypesModule),
  },
  {
    path: 'preferences/schedulesettings/halfdays',
    loadChildren: () =>
      import('./halfdays/halfdays.module').then(m => m.HalfdaysModule),
  },
  {
    path: 'preferences/schedulesettings/holidays',
    loadChildren: () =>
      import('./holidays/holidays.module').then(m => m.HolidaysModule),
  },
  {
    path: 'preferences/schedulesettings/incomingdeliverytype',
    loadChildren: () =>
      import('./incoming-delivery-types/incoming-delivery-types.module').then(
        m => m.IncomingDeliveryTypesModule
      ),
  },
  {
    path: 'preferences/schedulesettings/leisurecode',
    loadChildren: () =>
      import('./leisure-codes/leisure-codes.module').then(
        m => m.LeisureCodesModule
      ),
  },
  {
    path: 'preferences/schedulesettings/leisurecodetype',
    loadChildren: () =>
      import('./leisure-code-types/leisure-code-types.module').then(
        m => m.LeisureCodeTypesModule
      ),
  },
  {
    path: 'preferences/schedulesettings/skill',
    loadChildren: () =>
      import('./skills/skills.module').then(m => m.SkillsModule),
  },
  {
    path: 'preferences/schedulesettings/skilltype',
    loadChildren: () =>
      import('./skill-types/skill-types.module').then(m => m.SkillTypesModule),
  },
  {
    path: 'preferences/schedulesettings/shifttype',
    loadChildren: () =>
      import('../../shared/features/shift-type/shift-type.module').then(
        m => m.ShiftTypeModule
      ),
  },
  {
    path: 'preferences/timesettings/timedeviationcause',
    loadChildren: () =>
      import('./time-deviation-causes/time-deviation-causes.module').then(
        m => m.TimeDeviationCausesModule
      ),
  },
  {
    path: 'preferences/schedulesettings/timescheduletasktype',
    loadChildren: () =>
      import('./time-schedule-task-types/time-schedule-task-types.module').then(
        m => m.TimeScheduleTaskTypesModule
      ),
  },
  {
    path: 'preferences/timesettings/planningperiod',
    loadChildren: () =>
      import('./planning-periods/planning-periods.module').then(
        m => m.PlanningPeriodsModule
      ),
  },
  {
    path: 'preferences/timesettings/timecodeadditiondeduction',
    loadChildren: () =>
      import(
        './time-code-addition-deduction/time-code-addition-deduction.module'
      ).then(m => m.TimeCodeAdditionDeductionModule),
  },
  {
    path: 'preferences/timesettings/timecodebreakgroup',
    loadChildren: () =>
      import('./time-code-break-group/time-code-break-group.module').then(
        m => m.TimeCodeBreakGroupModule
      ),
  },
  {
    path: 'preferences/timesettings/timecoderanking',
    loadChildren: () =>
      import('./time-code-ranking/time-code-ranking.module').then(
        m => m.TimeCodeRankingModule
      ),
  },
  {
    path: 'preferences/timesettings/timescheduletype',
    loadChildren: () =>
      import('./time-schedule-type/time-schedule-type.module').then(
        m => m.TimeScheduleTypeModule
      ),
  },
  {
    path: 'schedule/absencerequests',
    loadChildren: () =>
      import('./absence-requests/absence-requests.module').then(
        m => m.AbsenceRequestsModule
      ),
  },
  {
    path: 'schedule/loggedwarnings',
    loadChildren: () =>
      import('./logged-warnings/logged-warnings.module').then(
        m => m.LoggedWarningsModule
      ),
  },
  {
    path: 'schedule/availability',
    loadChildren: () =>
      import('./availability/availability.module').then(
        m => m.AvailabilityModule
      ),
  },
  {
    path: 'schedule/placement',
    loadChildren: () =>
      import('./placements/placements.module').then(m => m.PlacementsModule),
  },
  {
    path: 'schedule/planning/spaschedule',
    loadChildren: () =>
      import('./schedule-planning/schedule-planning.module').then(
        m => m.SchedulePlanningModule
      ),
  },
  {
    path: 'schedule/schedulecycle',
    loadChildren: () =>
      import('./schedule-cycles/schedule-cycles.module').then(
        m => m.ScheduleCyclesModule
      ),
  },
  {
    path: 'schedule/staffingneedsdelivery',
    loadChildren: () =>
      import('./incoming-deliveries/incoming-deliveries.module').then(
        m => m.IncomingDeliveriesModule
      ),
  },
  {
    path: 'schedule/staffingneedstask',
    loadChildren: () =>
      import('./time-schedule-tasks/time-schedule-tasks.module').then(
        m => m.TimeScheduleTasksModule
      ),
  },
  {
    path: 'schedule/timebreaktemplate',
    loadChildren: () =>
      import('./time-break-templates/time-break-templates.module').then(
        m => m.TimeBreakTemplatesModule
      ),
  },
  {
    path: 'time/attest/adjusttimestamps',
    loadChildren: () =>
      import('./adjust-time-stamps/adjust-time-stamps.module').then(
        m => m.AdjustTimeStampsModule
      ),
  },
  {
    path: 'time/earnedholiday',
    loadChildren: () =>
      import('./earned-holiday/earned-holiday.module').then(
        m => m.EarnedHolidayModule
      ),
  },
  {
    path: 'time/timeworkreduction',
    loadChildren: () =>
      import('./time-work-reduction/time-work-reduction.module').then(
        m => m.TimeWorkReductionModule
      ),
  },
  {
    path: 'schedule/timescheduleevents',
    loadChildren: () =>
      import('./time-schedule-events/time-schedule-events.module').then(
        m => m.TimeScheduleEventsModule
      ),
  },
  {
    path: 'schedule/schedulecycleruletype',
    loadChildren: () =>
      import(
        './schedule-cycle-rule-types/schedule-cycle-rule-types.module'
      ).then(m => m.ScheduleCycleRuleTypesModule),
  },
];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeRoutingModule {}
