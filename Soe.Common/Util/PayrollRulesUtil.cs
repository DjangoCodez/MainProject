using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.Util
{
    public static class PayrollRulesUtil
    {
        #region SysPayrollTypeLevel

        public static bool Isnull(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return !sysPayrollTypeLevel1.HasValue && !sysPayrollTypeLevel2.HasValue && !sysPayrollTypeLevel3.HasValue && !sysPayrollTypeLevel4.HasValue;
        }

        public static bool IsAbsence(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence;
        }

        public static bool IsAbsenceSick(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick;
        }

        public static bool IsAbsenceSickDayQualifyingDay(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_QualifyingDay;
        }

        public static bool IsAbsenceSickDay2_14(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_Day2_14;
        }

        public static bool IsAbsenceSickDay15(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_Day15;
        }

        public static bool IsAbsence_SicknessSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary;
        }

        public static bool IsAbsence_SicknessSalary_Day2_14(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Day2_14;
        }

        public static bool IsAbsence_SicknessSalary_Deduction(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Deduction;
        }

        public static bool IsAbsence_SicknessSalary_Day15(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Day15;
        }

        public static bool IsAbsenceWorkInjury(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury;
        }

        public static bool IsAbsencePermission(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Permission;
        }

        public static bool IsAbsencePayedAbsence(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_PayedAbsence;
        }

        public static bool IsAbsenceSickOrWorkInjury(int sysPayrollTypeLevel3)
        {
            return IsAbsenceSickOrWorkInjury((int)TermGroup_SysPayrollType.SE_GrossSalary, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence, sysPayrollTypeLevel3);
        }

        public static bool IsAbsenceSickOrWorkInjury(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                (sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick || sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury);
        }

        public static bool IsAbsencePayrollExport(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence)
                    ||
                    (sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator &&
                    (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_MinusTime || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_Withdrawal));
        }

        public static bool IsAbsenceVacation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation;
        }

        public static bool IsAbsenceVacationNoVacationDaysDeducted(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_NoVacationDaysDeducted;
        }

        public static bool IsQualifyingDeduction(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SicknessSalary_Deduction;
        }

        public static bool IsTimeAccumulatorMinusTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_MinusTime;
        }

        public static bool IsVacationAddition(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAddition;
        }

        public static bool IsVacationAdditionVariable(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable;
        }

        public static bool IsVacationAdditionVariablePaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable_Paid;
        }

        public static bool IsVacationAdditionVariableAdvance(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionVariable_Advance;
        }

        public static bool IsVacationSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary;
        }

        public static bool IsVacationSalaryPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary_Paid;
        }

        public static bool IsAbsenceVacationAll(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return IsAbsenceVacation(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationPaid(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationUnPaid(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationSavedOverdue(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationSavedYear1(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationSavedYear2(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationSavedYear3(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationSavedYear4(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                     IsVacationSavedYear5(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsVacationCost(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                IsVacationAddition(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                IsVacationCompensation(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                IsVacationSalary(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsVacationPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                   (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Paid || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Paid_Secondary);
        }

        public static bool IsVacationUnPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                   (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Unpaid || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Unpaid_Secondary);
        }

        public static bool IsVacationAdvance(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                   (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Advance || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_Advance_Secondary);
        }

        public static bool IsVacationSavedYear1(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                   (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear1 || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear1_Secondary);
        }

        public static bool IsVacationSavedYear2(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                          sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                          (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear2 || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear2_Secondary);
        }

        public static bool IsVacationSavedYear3(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
               sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
               (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear3 || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear3_Secondary);
        }

        public static bool IsVacationSavedYear4(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                    (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear4 || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear4_Secondary);
        }

        public static bool IsVacationSavedYear5(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                    (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear5 || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedYear5_Secondary);
        }

        public static bool IsVacationSavedOverdue(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                 sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation &&
                 (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedOverdue || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation_SavedOverdue_Secondary);

        }

        public static bool IsVacationAdditionOrSalaryPrepayment(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment;
        }

        public static bool IsVacationAdditionOrSalaryVariablePrepayment(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment;
        }
        public static bool IsVacationAdditionOrSalaryPrepaymentPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment &&
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment_Paid;
        }

        public static bool IsVacationAdditionOrSalaryPrepaymentInvert(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment &&
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment_Invert;
        }

        public static bool IsVacationAdditionOrSalaryVariablePrepaymentPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment &&
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment_Paid;
        }

        public static bool IsVacationAdditionOrSalaryVariablePrepaymentInvert(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment &&
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment_Invert;
        }

        public static bool IsWorkTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime) ||
                    (sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                    sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary);
        }

        public static bool IsValidAbsenceAsWorkTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence;
        }

        public static bool IsInvalidAbsenceAsWorkTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence;
        }

        public static bool IsGrossSalaryTimeHourMonthly(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   (sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary || sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_TimeSalary || sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary || sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_MonthlySalary));
        }

        public static bool IsDutySalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Duty;
        }

        public static bool IsWeekendSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_WeekendSalary;
        }

        public static bool IsOverTimeAddition(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition;
        }

        public static bool IsAddedOrOverTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return IsOvertimeCompensation(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                 IsAddedTime(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsAddedTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime;
        }
        public static bool IsAddedTimeCompensation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime &&
                   (sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation || 
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation35 ||
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation70 ||
                    sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation100
                   );
        }
        public static bool IsAddedTimeCompensation35(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation35;
        }
        public static bool IsAddedTimeCompensation70(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation70;
        }
        public static bool IsAddedTimeCompensation100(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation100;
        }
        public static bool IsAddedTimeAddition(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Addition;
        }

        public static bool IsAddition(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation;
        }

        public static bool IsDeduction(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction;
        }

        public static bool IsDeductionCarBenefit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_CarBenefit;
        }

        public static bool IsDeductionHouseKeeping(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_HouseKeeping;
        }

        public static bool IsDeductionOther(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_Other;
        }

        public static bool IsDeductionSalaryDistress(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistress;
        }

        public static bool IsCompensation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation;
        }

        public static bool IsCompensation_Rental(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_RentalCompensation;
        }

        public static bool IsCompensation_CarCompensation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_CarCompensation;
        }

        public static bool IsCompensation_CarCompensation_BenefitCar(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_CarCompensation && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_CarCompensation_BenefitCar;
        }

        public static bool IsCompensation_CarCompensation_PrivateCar(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_CarCompensation && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_CarCompensation_PrivateCar;
        }

        public static bool IsCompensation_Other(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_Other;
        }

        public static bool IsCompensation_Other_Taxfree(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation
                && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_Other
                && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_Other_TaxFree;
        }
        public static bool IsCompensation_Other_Taxable(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation
                && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_Other
                && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_Other_Taxable;
        }

        public static bool IsCompensation_Representation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_Representation;
        }

        public static bool IsCompensation_SportsActivity(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_SportsActivity;
        }

        public static bool IsCompensation_TravelAllowance(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance;
        }

        public static bool IsCompensation_TravelAllowance_DomesticShortTerm(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_DomesticShortTerm;
        }

        public static bool IsCompensation_TravelAllowance_ForeignShortTerm(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_ForeignShortTerm;
        }

        public static bool IsCompensation_TravelAllowance_DomesticLongTerm(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_DomesticLongTerm;
        }

        public static bool IsCompensation_TravelAllowance_DomesticLongTermOrOverTwoYears(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_DomesticLongTerm ||
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_DomesticOverTwoYears);
        }

        public static bool IsCompensation_TravelAllowance_ForeignLongTermOrOverTwoYears(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_ForeignLongTerm ||
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelAllowance_ForeignOverTwoYears);
        }

        public static bool IsCompensation_TravelCost(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_TravelCost;
        }

        public static bool IsCompensation_Accomodation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_Accomodation;
        }

        public static bool IsCompensation_Vat(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Compensation &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Compensation_Vat;
        }

        public static bool IsPersonellAcquisitionOptions(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return false;
        }

        public static bool IsOBAddition(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition;
        }

        public static bool IsDuty(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Duty;
        }

        public static bool IsOBAddition40(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition40;
        }

        public static bool IsOBAddition50(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition50;
        }

        public static bool IsOBAddition57(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition57;
        }

        public static bool IsOBAddition70(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition70;
        }

        public static bool IsOBAddition79(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition79;
        }

        public static bool IsOBAddition100(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition100;
        }

        public static bool IsOBAddition113(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OBAddition_OBAddition113;
        }

        public static bool IsGrossSalaryDuty(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Duty;
        }

        public static bool IsContract(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Contract;
        }

        public static bool IsDutyAndBenefitNotInvert(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                ||
                IsGrossSalaryDuty(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsTimeScheduledTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_WorkedScheduledTime;
        }

        public static bool IsTimeAccumulatorTimeOrAddedTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_Time || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_AddedTime;
        }

        public static bool IsTimeAccumulatorAddedTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_AddedTime;
        }

        public static bool IsTax(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax;
        }

        public static bool IsOptionalTax(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_Optional;
        }

        public static bool IsSINKTax(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_SINK;
        }

        public static bool IsASINKTax(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Tax &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Tax_ASINK;
        }

        public static bool IsTaxAndNotOptional(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            //Optional tax is added as an "Added transaction"
            return IsTax(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) && !IsOptionalTax(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsEmploymentTax(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit || sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
        }

        public static bool IsEmploymentTaxDebit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit;
        }

        public static bool IsEmploymentTaxCredit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
        }

        public static bool IsEmploymentTaxCreditTo37(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year <= 1937)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else
                return false;
        }

        public static bool IsEmploymentTaxCreditEarlyPension(int year, int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (year == 2016 && birthDate.HasValue && birthDate.Value.Year >= 1938 && birthDate.Value.Year <= 1950)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else if (year == 2017 && birthDate.HasValue && birthDate.Value.Year >= 1939 && birthDate.Value.Year <= 1951)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else if (year == 2018 && birthDate.HasValue && birthDate.Value.Year >= 1940 && birthDate.Value.Year <= 1952)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else if (year == 2019 && birthDate.HasValue && birthDate.Value.Year >= 1941 && birthDate.Value.Year <= 1953)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else
                return false;
        }

        public static bool IsEmploymentTaxCredit51To90(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1951 && birthDate.Value.Year <= 1990)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else
                return false;
        }

        public static bool IsEmploymentTaxCreditFrom89(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1989)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else
                return false;
        }

        public static bool IsEmploymentTaxCredit91ToNow(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1991)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit;
            else
                return false;
        }

        public static bool IsSupplementChargeDebit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit;
        }

        public static bool IsSupplementChargeCredit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit;
        }

        public static bool IsSupplementCharge(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit || sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit;
        }

        public static bool IsParentalLeave(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave;
        }

        public static bool IsAbsenceTemporaryParentalLeave(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave;
        }

        public static bool IsAbsenceParentalLeaveOrTemporaryParentalLeave(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                (sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave || sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave);
        }

        public static bool IsAbsenceMilitaryService(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService;
        }

        public static bool IsAbsenceMilitaryServiceTotal(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService_Total;
        }

        public static bool IsLeaveOfAbsence(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence;
        }

        public static bool IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave(int sysPayrollTypeLevel3)
        {
            return IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave((int)TermGroup_SysPayrollType.SE_GrossSalary, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence, sysPayrollTypeLevel3);
        }

        public static bool IsLeaveOfAbsenceOrParentalLeaveOrTemporaryParentalLeave(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                IsLeaveOfAbsence(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                IsAbsenceParentalLeaveOrTemporaryParentalLeave(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsNetSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_NetSalary;
        }

        public static bool IsNetSalaryPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_NetSalary &&
                 sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_NetSalary_Paid;
        }

        public static bool IsNetSalaryRounded(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_NetSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_NetSalary_Rounded;
        }

        public static bool IsGrossSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
        }

        public static bool IsWork(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary;
        }

        public static bool IsWorkForHourlyPay(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return IsWorkTime(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                   IsOverTimeAddition(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                   IsOBAddition(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                   IsOvertimeCompensation(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) ||
                   IsAddedTime(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsWorkHourlySalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary;
        }

        public static bool IsGrossSalaryStandby(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Standby;
        }

        public static bool IsGrossSalaryCarAllowanceFlat(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_CarAllowanceFlat;
        }

        public static bool IsGrossSalaryAllowanceStandard(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AllowanceStandard;
        }

        public static bool IsGrossSalaryLayOffSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_LayOffSalary;
        }

        public static bool IsGrossSalaryRetroactive(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_SalaryRetroactive;
        }

        public static bool IsGrossSalaryTravelTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_TravelTime;
        }

        public static bool IsGrossSalaryCommision(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Commission;
        }

        public static bool IsGrossSalaryWeekendSalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_WeekendSalary;
        }

        public static bool IsGrossSalaryEarnedHolidayPayment(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_EarnedHolidayPayment;
        }

        public static bool IsGrossSalaryEarlyPension(int year, int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (year == 2016 && birthDate.HasValue && birthDate.Value.Year >= 1938 && birthDate.Value.Year <= 1950)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
            if (year == 2017 && birthDate.HasValue && birthDate.Value.Year >= 1939 && birthDate.Value.Year <= 1951)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
            if (year == 2018 && birthDate.HasValue && birthDate.Value.Year >= 1940 && birthDate.Value.Year <= 1952)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
            else
                return false;
        }

        public static bool IsGrossSalaryTimeWorkReduction(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_TimeWorkReduction;
        }

        public static bool IsBoardRemuneration(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary_BoardRemuneration;
        }
        public static bool IsRoleSupplement(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary_RoleSupplement;
        }
        public static bool IsActivitySupplement(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary_ActivitySupplement;
        }
        public static bool IsCompetenceSupplement(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary_CompetenceSupplement;
        }
        public static bool IsResponsibilitySupplement(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Salary_ResponsibilitySupplement;
        }

        public static bool IsGrossSalaryFrom89(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1989)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
            else
                return false;
        }

        public static bool IsGrossSalaryYouth(int year, int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (year == 2016 && birthDate.HasValue && birthDate.Value.Year >= 1991)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
            else
                return false;
        }

        public static bool IsGrossSalaryTo37(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year <= 1937)
                return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary;
            else
                return false;
        }

        public static bool IsBenefitInvert(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert;
        }

        public static bool IsBenefitInvertWithLevel3NotNull(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return PayrollRulesUtil.IsBenefitInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4) && sysPayrollTypeLevel3.HasValue;
        }

        public static bool IsOvertimeCompensation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation;
        }
        public static bool IsOvertimeCompensation35(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation_OBAddition35;
        }
        public static bool IsOvertimeCompensation50(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation_OBAddition50;
        }

        public static bool IsOvertimeCompensation70(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation_OBAddition70;
        }

        public static bool IsOvertimeCompensation100(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation_OBAddition100;
        }

        public static bool IsOvertimeAddition(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition;
        }
        public static bool IsOvertimeAddition35(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition_OBAddition35;
        }
        public static bool IsOvertimeAddition50(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition_OBAddition50;
        }

        public static bool IsOvertimeAddition70(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition_OBAddition70;
        }

        public static bool IsOvertimeAddition100(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition_OBAddition100;
        }

        public static bool IsBenefitAndNotInvert38To50(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1938 && birthDate.Value.Year <= 1950)
                return IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            else
                return false;
        }

        public static bool IsBenefitAndNotInvert38To52(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1938 && birthDate.Value.Year <= 1952)
                return IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            else
                return false;
        }

        public static bool IsBenefitAndNotInvertFrom89(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1989)
                return IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            else
                return false;
        }

        public static bool IsBenefitAndNotInvertFrom91(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year >= 1991)
                return IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            else
                return false;
        }

        public static bool IsBenefitAndNotInvertTo37(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null, DateTime? birthDate = null)
        {
            if (birthDate.HasValue && birthDate.Value.Year <= 1937)
                return IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
            else
                return false;
        }

        public static bool IsOccupationalPension(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_OccupationalPension;
        }

        public static bool IsBenefit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit;
        }

        public static bool IsBenefitOther(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Other;
        }

        public static bool IsBenefitParking(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Parking;
        }

        public static bool IsBenefitPropertyHouse(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_PropertyHouse;
        }

        public static bool IsBenefitPropertyNotHouse(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_PropertyNotHouse;
        }

        public static bool IsBenefitROT(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_ROT;
        }

        public static bool IsBenefitRUT(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_RUT;
        }

        public static bool IsBenefitBorrowedComputer(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_BorrowedComputer;
        }

        public static bool IsBenefitCompanyCar(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_CompanyCar;
        }

        public static bool IsBenefitFood(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Food;
        }

        public static bool IsBenefitFuel(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Fuel;
        }

        public static bool IsBenefitInterest(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Interest;
        }

        public static bool IsBenefitInvertOther(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Other;
        }

        public static bool IsBenefitInvertPropertyNotHouse(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyNotHouse;
        }

        public static bool IsBenefitInvertPropertyHouse(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyHouse;
        }

        public static bool IsBenefitInvertFuel(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Fuel;
        }

        public static bool IsBenefitInvertROT(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_ROT;
        }

        public static bool IsBenefitInvertRUT(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_RUT;
        }

        public static bool IsBenefitInvertFood(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Food;
        }

        public static bool IsBenefitInvertBorrowedComputer(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_BorrowedComputer;
        }

        public static bool IsBenefitInvertParking(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Parking;
        }

        public static bool IsBenefitInvertInterest(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Interest;
        }

        public static bool IsBenefitInvertCompanyCar(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_CompanyCar;
        }

        public static bool IsBenefitInvertStandard(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert &&
                sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Standard;
        }

        public static bool IsBenefitAndNotInvert(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit &&
                sysPayrollTypeLevel2 != (int)TermGroup_SysPayrollType.SE_Benefit_Invert;
        }

        public static bool IsBenefit_Other(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Other;
        }

        public static bool IsBenefit_Not_CompanyCar_And_FuelBenefit(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit
                && !IsBenefitCompanyCar(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                && !IsBenefit_Fuel(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                && IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4);
        }

        public static bool IsBenefit_Fuel_PartNotAnnualized(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Fuel && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Fuel_PartNotAnnualized;
        }

        public static bool IsBenefit_Fuel_PartAnnualized(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Fuel && sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Fuel_PartAnnualized;
        }

        public static bool IsBenefit_CompanyCar(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_CompanyCar;
        }

        public static bool IsBenefit_PropertyNotHouse(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_PropertyNotHouse;
        }

        public static bool IsBenefit_PropertyHouse(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_PropertyHouse;
        }

        public static bool IsBenefit_Fuel(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Fuel;
        }

        public static bool IsBenefit_Parking(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Parking;
        }

        public static bool IsBenefit_Food(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Food;
        }

        public static bool IsBenefit_BorrowedComputer(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_BorrowedComputer;
        }

        public static bool IsBenefit_ROT(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_ROT;
        }

        public static bool IsBenefit_RUT(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_RUT;
        }

        public static bool IsBenefit_Interest(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit && sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Benefit_Interest;
        }

        public static bool IsCostDeduction(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_CostDeduction;
        }

        public static bool IsSalaryDistress(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistress;
        }

        public static bool IsUnionFee(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Deduction &&
                sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Deduction_UnionFee;
        }

        public static bool IsEmployeeVehicleTransaction(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return
                (PayrollRulesUtil.IsBenefitCompanyCar(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                ||
                PayrollRulesUtil.IsDeductionCarBenefit(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4));
        }

        public static bool IsCollectumArslon(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Benefit_CompanyCar;

        }

        public static bool IsCollectumLonevaxling(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return false;
        }

        public static bool IsAbsenceNoVacation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_UnionWork_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_UnionEduction_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_ParentalLeave_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_PregnancyCompensation_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_RelativeCare_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_NoVacation_QualifyingDay ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_NoVacation_Day2_14 ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick_NoVacation_Day15 ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_SwedishForImmigrants_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TemporaryParentalLeave_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_MilitaryService_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_LeaveOfAbsence_NoVacation ||
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_TransmissionOfInfection_NoVacation;
        }

        public static bool IsVacationCompensation(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation;
        }

        public static bool IsVacationCompensationEarned(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Earned;
        }

        public static bool IsVacationCompensationDirectPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_DirectPaid;
        }

        public static bool IsVacationCompensationSavedOverdueVariable(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdueVariable;
        }

        public static bool IsVacationCompensationPaid(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Paid;
        }

        public static bool IsVacationCompensationAdvance(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Advance;
        }

        public static bool IsVacationCompensationSavedYear1(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1;
        }

        public static bool IsVacationCompensationSavedYear2(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear2;
        }

        public static bool IsVacationCompensationSavedYear3(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear3;
        }

        public static bool IsVacationCompensationSavedYear4(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear4;
        }

        public static bool IsVacationCompensationSavedYear5(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear5;
        }

        public static bool IsVacationCompensationSavedOverdue(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdue;
        }

        public static bool IsTimeAccumulator(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator;
        }

        public static bool IsTimeAccumulatorNegate(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator &&
                   (sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_MinusTime || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_Payment || sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_Withdrawal);
        }

        public static bool IsTimeAccumulatorOverTime(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Time &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator &&
                   sysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_AccumulatorPlaceholder &&
                   sysPayrollTypeLevel4 == (int)TermGroup_SysPayrollType.SE_Time_Accumulator_OverTime;
        }

        public static bool IsTaxBasis(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (IsGrossSalary(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                    || IsCostDeduction(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                    || IsOccupationalPension(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                    || IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4));
        }

        public static bool IsEmploymentTaxBasis(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return ((IsGrossSalary(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                    || IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)) &&
                    !IsBenefit_Fuel_PartAnnualized(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4));
        }

        public static bool IsSupplementChargeBasis(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return (IsGrossSalary(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4)
                    || IsBenefitAndNotInvert(sysPayrollTypeLevel1, sysPayrollTypeLevel2, sysPayrollTypeLevel3, sysPayrollTypeLevel4));
        }

        public static bool IsHourlySalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_HourlySalary;
        }

        public static bool IsMonthlySalary(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_MonthlySalary;
        }

        public static bool isITP1(TermGroup_PensionCompany pensionCompany)
        {
            return (pensionCompany == TermGroup_PensionCompany.SE_ITP1 || pensionCompany == TermGroup_PensionCompany.SE_ITP1_ITP2);
        }

        public static bool isITP2(TermGroup_PensionCompany pensionCompany)
        {
            return (pensionCompany == TermGroup_PensionCompany.SE_ITP2 || pensionCompany == TermGroup_PensionCompany.SE_ITP1_ITP2);
        }

        public static bool isFora(TermGroup_PensionCompany pensionCompany)
        {
            return (pensionCompany == TermGroup_PensionCompany.SE_FORA);
        }

        public static bool isKPA(TermGroup_PensionCompany pensionCompany)
        {
            return (pensionCompany == TermGroup_PensionCompany.SE_KPA);
        }

        public static bool isGTP(TermGroup_PensionCompany pensionCompany)
        {
            return (pensionCompany == TermGroup_PensionCompany.SE_GTP);
        }

        public static bool IsSkandia(TermGroup_PensionCompany pensionCompany)
        {
            return (pensionCompany == TermGroup_PensionCompany.SE_SKANDIA);
        }

        public static bool IsBygglosenPaidoutExcess(int? sysPayrollTypeLevel1 = null, int? sysPayrollTypeLevel2 = null, int? sysPayrollTypeLevel3 = null, int? sysPayrollTypeLevel4 = null)
        {
            return sysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Statistic &&
                   sysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_BygglosenPaidoutExcess;
        }

        #endregion

        #region EmployeeTax

        public static decimal CalculateSchoolYouthLimitRemaining(decimal schoolYouthLimit, decimal? schoolYouthLimitInitial, decimal? schoolYouthLimitUsed)
        {
            decimal initial = schoolYouthLimitInitial ?? 0;
            decimal limitUsed = schoolYouthLimitUsed ?? 0;
            decimal remaining = schoolYouthLimit - initial - limitUsed;
            if (remaining < 0)
                remaining = 0;
            if (remaining > schoolYouthLimit)
                remaining = schoolYouthLimit;
            return remaining;
        }

        #endregion

        #region Experience

        public static decimal CalculateExperienceMonths(int employmentDays)
        {
            return (employmentDays / Convert.ToDecimal(30.42));
        }

        #endregion

        #region TimeCode

        public static bool IsWork(SoeTimeCodeType timeCodeType)
        {
            return timeCodeType == SoeTimeCodeType.Work;
        }

        public static bool IsAbsence(SoeTimeCodeType timeCodeType)
        {
            return timeCodeType == SoeTimeCodeType.Absense;
        }

        public static bool IsBreak(SoeTimeCodeType timeCodeType)
        {
            return timeCodeType == SoeTimeCodeType.Break;
        }

        public static bool IsAdditionAndDeduction(SoeTimeCodeType timeCodeType)
        {
            return timeCodeType == SoeTimeCodeType.AdditionDeduction;
        }

        public static bool IsMaterial(SoeTimeCodeType timeCodeType)
        {
            return timeCodeType == SoeTimeCodeType.Material;
        }

        #endregion

        #region PayrollCalculationProductDTO

        public static List<PayrollCalculationProductDTO> ConvertToPayrollItems(List<AttestEmployeeDayDTO> attestEmployeeDays, bool showDetailedInfo)
        {
            var dtos = new List<PayrollCalculationProductDTO>();

            if (attestEmployeeDays.IsNullOrEmpty())
                return dtos;

            var transactionItemsGrouped = new List<AttestPayrollTransactionDTO>();
            foreach (var attestEmployeeDay in attestEmployeeDays.OrderBy(i => i.Date))
            {
                transactionItemsGrouped.AddRange(PayrollRulesUtil.GroupTransactionsByDay(attestEmployeeDay.AttestPayrollTransactions, showDetailedInfo));
            }

            dtos = CreatePayrollCalculationProducts(transactionItemsGrouped, attestEmployeeDays.First().EmployeeId, false);

            return dtos;
        }

        #endregion

        #region PayrollCalculationPeriodSumDTO

        public static PayrollCalculationPeriodSumDTO CalculateSum(List<PayrollCalculationProductDTO> dtos)
        {
            PayrollCalculationPeriodSumDTO sum = new PayrollCalculationPeriodSumDTO();
            if (dtos.IsNullOrEmpty())
                return sum;

            sum.TransactionNet = dtos.Where(x => x.Amount.HasValue && x.IsNetSalaryPaid()).Sum(x => x.Amount.Value);

            foreach (var dto in dtos)
            {
                PayrollRulesUtil.CalculateSum(sum, dto.AttestPayrollTransactions.ToPayrollCalculationPeriodSumItemDTOs());
            }

            return sum;
        }

        public static void CalculateSum(PayrollCalculationPeriodSumDTO sum, List<PayrollCalculationPeriodSumItemDTO> sums)
        {
            foreach (var sumGrouping in sums.Where(i => i.Amount.HasValue && i.SysPayrollTypeLevel1.HasValue).GroupBy(i => i.SysPayrollTypeLevel1.Value))
            {
                int sysPayrollTypeLevel1 = sumGrouping.Key;

                switch (sysPayrollTypeLevel1)
                {
                    #region GrossSalarySum

                    case (int)TermGroup_SysPayrollType.SE_GrossSalary:
                    case (int)TermGroup_SysPayrollType.SE_OccupationalPension:
                        sum.Gross += sumGrouping.Sum(i => i.Amount.Value);
                        break;
                    case (int)TermGroup_SysPayrollType.SE_CostDeduction:
                        sum.Gross -= sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region Benefit

                    case (int)TermGroup_SysPayrollType.SE_Benefit:
                        sum.BenefitInvertExcluded += sumGrouping.Where(x => x.SysPayrollTypeLevel2 != (int)TermGroup_SysPayrollType.SE_Benefit_Invert).Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region Tax

                    case (int)TermGroup_SysPayrollType.SE_Tax:
                        sum.Tax += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region Compensation

                    case (int)TermGroup_SysPayrollType.SE_Compensation:
                        sum.Compensation += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region Deduction

                    case (int)TermGroup_SysPayrollType.SE_Deduction:
                        sum.Deduction += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region EmploymentTax Debit

                    case (int)TermGroup_SysPayrollType.SE_EmploymentTaxDebit:
                        sum.EmploymentTaxDebit += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region EmploymentTax Credit

                    case (int)TermGroup_SysPayrollType.SE_EmploymentTaxCredit:
                        sum.EmploymentTaxCredit += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region SupplementCharge Debit

                    case (int)TermGroup_SysPayrollType.SE_SupplementChargeDebit:
                        sum.SupplementChargeDebit += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region SupplementCharge Credit

                    case (int)TermGroup_SysPayrollType.SE_SupplementChargeCredit:
                        sum.SupplementChargeCredit += sumGrouping.Sum(i => i.Amount.Value);
                        break;

                    #endregion

                    #region Netsalary rounded

                    case (int)TermGroup_SysPayrollType.SE_NetSalary:
                        var netSalaryRoundedTransactions = sumGrouping.Where(x => x.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_NetSalary_Rounded).ToList();
                        decimal roundedSum = netSalaryRoundedTransactions.Sum(i => i.Amount.Value);
                        if (roundedSum < 0)
                            sum.Deduction += roundedSum;
                        else
                            sum.Compensation += roundedSum;
                        break;

                        #endregion
                }
            }

            sum.Net = sum.Gross + sum.Tax + sum.Compensation + sum.Deduction;
            if (sum.Net > 0 && sum.Net < 1)
                sum.Net = 0;
        }

        #endregion

        #region AttestPayrollTransactionDTO

        public static List<AttestPayrollTransactionDTO> GroupTransactionsByDay(List<AttestPayrollTransactionDTO> transactionItems, bool showAllTransactions)
        {
            List<AttestPayrollTransactionDTO> groupedTransactionsItems = new List<AttestPayrollTransactionDTO>();

            if (transactionItems.IsNullOrEmpty())
                return groupedTransactionsItems;

            #region Not IsAddedOrFixed or IncludedInPayrollProductChain

            //First, group by PayrollProduct
            foreach (var transactionItemsByNumber in transactionItems.Where(i => !i.IsAddedOrFixed && !i.IncludedInPayrollProductChain).GroupBy(i => i.PayrollProductNumber))
            {
                if (showAllTransactions)
                {
                    #region Show one row per transaction

                    foreach (var transactionItem in transactionItemsByNumber.OrderBy(i => i.StartTime))
                    {
                        groupedTransactionsItems.Add(transactionItem);
                    }

                    #endregion
                }
                else
                {
                    #region Show one row per grouping

                    //Second, group by TimeUnit
                    foreach (var transactionItemsByTimeUnit in transactionItemsByNumber.GroupBy(i => i.TimeUnit))
                    {
                        //Third, group by QuantityDays
                        foreach (var transactionItemsByQuantityDays in transactionItemsByTimeUnit.GroupBy(i => i.QuantityDays)) //its needed because of calendarfactor
                        {
                            //Fourth, group by IsQuantityOrFixed
                            foreach (var transactionItemsByType in transactionItemsByQuantityDays.GroupBy(i => i.IsQuantityOrFixed))
                            {
                                //Fifth, group by UnitPrice (smallest first)
                                foreach (var transactionItemsByPrice in transactionItemsByType.GroupBy(i => i.UnitPrice))
                                {
                                    //Six, group by Account
                                    foreach (var transactionItemsByAccount in transactionItemsByPrice.GroupBy(i => i.AccountingDescription))
                                    {
                                        //Seventh, group by IsScheduleTransaction
                                        foreach (var transactionItemsGrouped in transactionItemsByAccount.GroupBy(i => i.IsScheduleTransaction))
                                        {
                                            var transactionItem = transactionItemsGrouped.First().CloneDTO();

                                            //Properties included in GroupBy and therefore correct from clone
                                            //UnitPrice
                                            //UnitPriceCurrency
                                            //UnitPriceEntCurrency
                                            //AccountDims
                                            //AccountStd
                                            //AccountInternals
                                            //AccountingShortString
                                            //AccountingLongString

                                            transactionItem.AllTimePayrollTransactionIds = transactionItemsGrouped.Select(i => i.TimePayrollTransactionId).Distinct().ToList();
                                            transactionItem.StartTime = transactionItemsGrouped.Min(i => i.StartTime);
                                            transactionItem.StopTime = transactionItemsGrouped.Max(i => i.StopTime);
                                            transactionItem.Quantity = transactionItemsGrouped.Sum(i => i.Quantity);
                                            transactionItem.Amount = transactionItemsGrouped.Sum(i => i.Amount);
                                            transactionItem.AmountCurrency = transactionItemsGrouped.Sum(i => i.AmountCurrency);
                                            transactionItem.AmountEntCurrency = transactionItemsGrouped.Sum(i => i.AmountEntCurrency);
                                            transactionItem.AttestStateName = transactionItemsGrouped.Any(i => i.IsScheduleTransaction) ? transactionItemsGrouped.First().AttestStateName : transactionItemsGrouped.GetAttestStates().GetAttestStateString(transactionItemsByNumber.GetAttestStateIds());
                                            transactionItem.HasSameAttestState = transactionItemsGrouped.GetAttestStateIds().Count() <= 1;
                                            transactionItem.Comment = transactionItemsGrouped.GetComments();
                                            transactionItem.IsScheduleTransaction = transactionItemsGrouped.Any(i => i.IsScheduleTransaction);
                                            transactionItem.PayrollPriceFormulaId = null; //Multiple values
                                            transactionItem.PayrollPriceTypeId = null; //Multiple values
                                            transactionItem.Formula = "*"; //NA
                                            transactionItem.FormulaPlain = "*"; //Multiple values
                                            transactionItem.FormulaExtracted = "*"; //Multiple values
                                            transactionItem.FormulaNames = "*"; //Multiple values
                                            transactionItem.FormulaOrigin = "*"; //Multiple values
                                            transactionItem.PayrollCalculationPerformed = false; //Multiple values

                                            groupedTransactionsItems.Add(transactionItem);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region IsAddedOrFixed or IncludedInPayrollProductChain

            foreach (var transactionItemsByNumber in transactionItems.Where(i => i.IsAddedOrFixed || i.IncludedInPayrollProductChain).GroupBy(i => i.PayrollProductNumber))
            {
                foreach (var transactionItem in transactionItemsByNumber.OrderBy(i => i.StartTime))
                {
                    groupedTransactionsItems.Add(transactionItem);
                }
            }

            #endregion

            return groupedTransactionsItems;
        }

        public static List<PayrollCalculationProductDTO> CreatePayrollCalculationProducts(List<AttestPayrollTransactionDTO> transactionItems, int employeeId, bool isPayrollSlip)
        {
            List<PayrollCalculationProductDTO> payrollCalculationProducts = new List<PayrollCalculationProductDTO>();

            //group by PayrollProduct
            foreach (var transactionItemsByNumber in transactionItems.GroupBy(i => i.PayrollProductNumber))
            {
                foreach (var transactionItemsByRetro in transactionItemsByNumber.GroupBy(i => i.IsRetroactive))
                {
                    bool isRetroactive = transactionItemsByRetro.Key;
                    if (isRetroactive)
                    {
                        payrollCalculationProducts.AddRange(CreatePayrollCalculationProducts(transactionItemsByRetro.ToList(), employeeId, isRetroactive, false, false, false));
                    }
                    else
                    {
                        PayrollRulesUtil.SetAbsenceIntervalNr(transactionItemsByRetro, isPayrollSlip);

                        //group by AbsenceIntervalNr
                        foreach (var transactionItemsByAbsenceIntervalNr in transactionItemsByRetro.GroupBy(i => i.AbsenceIntervalNr))
                        {
                            //group by IsQuantityOrFixed
                            foreach (var transactionItemsByType in transactionItemsByAbsenceIntervalNr.GroupBy(i => i.IsQuantityOrFixed))
                            {
                                //group by IsRounding
                                foreach (var transactionItemsByRounding in transactionItemsByType.GroupBy(i => i.IsRounding))
                                {
                                    foreach (var transactionItemsByAdditionDeduction in transactionItemsByRounding.GroupBy(x => x.IsAdditionOrDeduction))
                                    {
                                        //group by IsAverageCalculated
                                        foreach (var transactionItemsByAverageCalculated in transactionItemsByAdditionDeduction.GroupBy(i => i.IsAverageCalculated))
                                        {
                                            // group by UnitPrice (smallest first)
                                            foreach (var transactionItemsByUnitPrice in transactionItemsByAverageCalculated.GroupBy(i => (!isPayrollSlip ? i.UnitPriceGrouping : i.UnitPricePayrollSlipGrouping)))
                                            {
                                                // group by VacationFiveDaysPerWeek
                                                foreach (var transactionItemsByVacationFiveDaysPerWeek in transactionItemsByUnitPrice.GroupBy(i => !isPayrollSlip && i.IsVacationFiveDaysPerWeek))
                                                {
                                                    foreach (var transactionItemsByCommentGrouping in transactionItemsByVacationFiveDaysPerWeek.GroupBy(i => i.CommentGrouping))
                                                    {
                                                        //group by IncludedInPayrollProductChain
                                                        foreach (var transactionItemsByPayrollProductChain in transactionItemsByCommentGrouping.GroupBy(i => i.IncludedInPayrollProductChain))
                                                        {
                                                            //group by TimeUnit
                                                            foreach (var transactionItemsByTimeUnit in transactionItemsByPayrollProductChain.GroupBy(i => i.TimeUnit))
                                                            {
                                                                //group by QuantityDays
                                                                foreach (var transactionItemsByQuantityDays in transactionItemsByTimeUnit.GroupBy(i => i.QuantityDays)) //its needed because of calendarfactor
                                                                {
                                                                    foreach (var transactionItemsByVacationYearEnd in transactionItemsByQuantityDays.GroupBy(i => i.IsVacationYearEnd))
                                                                    {
                                                                        //group by Accounting
                                                                        foreach (var transactionItemsByAccount in transactionItemsByVacationYearEnd.GroupBy(i => i.AccountingDescription))
                                                                        {
                                                                            payrollCalculationProducts.AddRange(CreatePayrollCalculationProducts(transactionItemsByAccount.ToList(), employeeId, isRetroactive, transactionItemsByVacationFiveDaysPerWeek.Key, isPayrollSlip, transactionItemsByVacationYearEnd.Key));
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return payrollCalculationProducts;
        }

        #region Help-methods

        private static List<PayrollCalculationProductDTO> CreatePayrollCalculationProducts(List<AttestPayrollTransactionDTO> transactionItems, int employeeId, bool isRetroactive, bool isVacationFiveDaysPerWeek, bool isPayrollSlip, bool isVacationYearEnd)
        {
            List<PayrollCalculationProductDTO> payrollCalculationProducts = new List<PayrollCalculationProductDTO>();
            if (transactionItems.IsNullOrEmpty())
                return payrollCalculationProducts;

            #region Grouping

            var groups = new List<Tuple<DateTime, DateTime, List<AttestPayrollTransactionDTO>>>();

            DateTime dateFrom = transactionItems.Any(i => i.IsAdditionOrDeduction && i.StartTime.HasValue) ? transactionItems.Where(i => i.StartTime.HasValue).OrderBy(i => i.StartTime).First().StartTime.Value : transactionItems.OrderBy(i => i.Date).First().Date;
            DateTime dateTo = transactionItems.Any(i => i.IsAdditionOrDeduction && i.StopTime.HasValue) ? transactionItems.Where(i => i.StopTime.HasValue).OrderByDescending(i => i.StopTime).First().StopTime.Value : transactionItems.OrderByDescending(i => i.Date).First().Date;

            if (isRetroactive)
            {
                #region Retroactive transactions

                groups.Add(Tuple.Create(dateFrom, dateTo, transactionItems));

                #endregion
            }
            else
            {
                #region Regular transaction

                bool isAdded = transactionItems.Any(i => i.AddedDateFrom.HasValue || i.AddedDateTo.HasValue);
                if (isAdded)
                {
                    #region Added transaction

                    AttestPayrollTransactionDTO firstAddedTransactionItem = transactionItems.Where(i => i.AddedDateFrom.HasValue).OrderBy(i => i.AddedDateFrom.Value).FirstOrDefault();
                    if (firstAddedTransactionItem != null && firstAddedTransactionItem.AddedDateFrom.HasValue)
                        dateFrom = firstAddedTransactionItem.AddedDateFrom.Value;

                    AttestPayrollTransactionDTO lastAddedTransactionItem = transactionItems.Where(i => i.AddedDateTo.HasValue).OrderByDescending(i => i.AddedDateTo.Value).FirstOrDefault();
                    if (lastAddedTransactionItem != null && firstAddedTransactionItem != null && firstAddedTransactionItem.AddedDateTo.HasValue)
                        dateTo = lastAddedTransactionItem.AddedDateTo.Value;

                    groups.Add(Tuple.Create(dateFrom, dateTo, transactionItems));

                    #endregion
                }
                else
                {
                    #region Intervals

                    if (isPayrollSlip)
                    {
                        transactionItems.Where(w => w.IsAbsenceVacation() && w.Amount == 0 && w.QuantityDays != 0 && w.IsVacationFiveDaysPerWeek).ToList().ForEach(f => f.QuantityCalendarDays = 0);
                        transactionItems.Where(w => w.IsAbsenceVacation() && w.Amount == 0 && w.QuantityDays != 0 && w.IsVacationFiveDaysPerWeek).ToList().ForEach(f => f.QuantityWorkDays = 0);
                        transactionItems.Where(w => w.IsAbsenceVacation() && w.Amount == 0 && w.Quantity != 0 && w.IsVacationFiveDaysPerWeek).ToList().ForEach(f => f.Quantity = 0);
                    }

                    bool onlyCoherentIntervals = transactionItems.Any(i => i.IsAbsence() || i.IsVacationSalary());
                    if (onlyCoherentIntervals && isPayrollSlip && transactionItems.Any(x => x.IsAbsenceVacationAll() || x.IsVacationSalary()))
                        onlyCoherentIntervals = false;

                    if (onlyCoherentIntervals)
                    {
                        List<Tuple<DateTime, DateTime>> ranges = transactionItems.Select(i => i.Date).GetCoherentDateRanges();
                        foreach (Tuple<DateTime, DateTime> range in ranges)
                        {
                            groups.Add(Tuple.Create(range.Item1, range.Item2, transactionItems.Where(i => i.Date >= range.Item1 && i.Date <= range.Item2).ToList()));
                        }
                    }
                    else
                    {
                        groups.Add(Tuple.Create(dateFrom, dateTo, transactionItems));
                    }

                    #endregion
                }

                #endregion
            }

            #endregion

            #region PayrollCalculationProduct

            foreach (Tuple<DateTime, DateTime, List<AttestPayrollTransactionDTO>> group in groups)
            {
                DateTime groupDateFrom = group.Item1;
                DateTime groupDateTo = group.Item2;
                List<AttestPayrollTransactionDTO> groupTransationItems = group.Item3;
                AttestPayrollTransactionDTO groupFirstTransactionItem = groupTransationItems.Where(i => !i.IsDistributed).OrderBy(i => i.Date).FirstOrDefault();
                if (groupFirstTransactionItem == null)
                    groupFirstTransactionItem = groupTransationItems.OrderBy(i => i.Date).First();

                var payrollCalculationProduct = new PayrollCalculationProductDTO();

                //General
                payrollCalculationProduct.EmployeeId = employeeId;
                payrollCalculationProduct.DateFrom = groupDateFrom;
                payrollCalculationProduct.DateTo = groupDateTo;

                //PayrollProduct
                payrollCalculationProduct.PayrollProductId = groupFirstTransactionItem.PayrollProductId;
                payrollCalculationProduct.SysPayrollTypeLevel1 = groupFirstTransactionItem.TransactionSysPayrollTypeLevel1;
                payrollCalculationProduct.SysPayrollTypeLevel2 = groupFirstTransactionItem.TransactionSysPayrollTypeLevel2;
                payrollCalculationProduct.SysPayrollTypeLevel3 = groupFirstTransactionItem.TransactionSysPayrollTypeLevel3;
                payrollCalculationProduct.SysPayrollTypeLevel4 = groupFirstTransactionItem.TransactionSysPayrollTypeLevel4;
                payrollCalculationProduct.PayrollProductNumber = groupFirstTransactionItem.PayrollProductNumber;
                payrollCalculationProduct.PayrollProductName = groupFirstTransactionItem.PayrollProductName;
                payrollCalculationProduct.PayrollProductShortName = groupFirstTransactionItem.PayrollProductShortName;
                payrollCalculationProduct.PayrollProductFactor = groupFirstTransactionItem.PayrollProductFactor;
                payrollCalculationProduct.PayrollProductPayed = groupFirstTransactionItem.PayrollProductPayed;
                payrollCalculationProduct.PayrollProductExport = groupFirstTransactionItem.PayrollProductExport;

                //Fields (depending on grouping)
                bool hasMultiple = false;
                payrollCalculationProduct.UnitPrice = isRetroactive ? groupTransationItems.GetUnitPrice(out hasMultiple) : groupFirstTransactionItem.UnitPrice;
                payrollCalculationProduct.HasMultipleUnitPrice = isRetroactive && hasMultiple;
                payrollCalculationProduct.UnitPriceCurrency = isRetroactive ? groupTransationItems.GetUnitPriceCurrency(out hasMultiple) : groupFirstTransactionItem.UnitPriceCurrency;
                payrollCalculationProduct.HasMultipleUnitPriceCurrency = isRetroactive && hasMultiple;
                payrollCalculationProduct.UnitPriceEntCurrency = isRetroactive ? groupTransationItems.GetUnitPriceEntCurrency(out hasMultiple) : groupFirstTransactionItem.UnitPriceEntCurrency;
                payrollCalculationProduct.HasMultipleUnitPriceEntCurrency = isRetroactive && hasMultiple;
                payrollCalculationProduct.IsCentRounding = isRetroactive ? groupTransationItems.GetIsRounding(out hasMultiple) : groupFirstTransactionItem.IsCentRounding;
                payrollCalculationProduct.HasMultipleIsCentRounding = isRetroactive && hasMultiple;
                payrollCalculationProduct.IsQuantityRounding = isRetroactive ? groupTransationItems.GetIsCentRounding(out hasMultiple) : groupFirstTransactionItem.IsQuantityRounding;
                payrollCalculationProduct.HasMultipleIsQuantityRounding = isRetroactive && hasMultiple;
                payrollCalculationProduct.IncludedInPayrollProductChain = isRetroactive ? groupTransationItems.GetIncludedInPayrollProductChain(out hasMultiple) : groupFirstTransactionItem.IncludedInPayrollProductChain;
                payrollCalculationProduct.HasMultipleIncludedInPayrollProductChain = isRetroactive && hasMultiple;
                payrollCalculationProduct.IsAverageCalculated = isRetroactive ? groupTransationItems.GetIsAverageCalculated(out hasMultiple) : groupFirstTransactionItem.IsAverageCalculated;
                payrollCalculationProduct.HasMultipleIsAverageCalculated = isRetroactive && hasMultiple;
                payrollCalculationProduct.TimeUnit = isRetroactive ? groupTransationItems.GetTimeUnit(out hasMultiple) : groupFirstTransactionItem.TimeUnit;
                payrollCalculationProduct.HasMultipleTimeUnit = isRetroactive && hasMultiple;
                payrollCalculationProduct.IsVacationFiveDaysPerWeek = isVacationFiveDaysPerWeek;

                //Flags
                payrollCalculationProduct.HasInfo = groupTransationItems.Any(i => i.HasInfo);
                payrollCalculationProduct.IsRetroactive = isRetroactive;

                //Accumulated below
                payrollCalculationProduct.Amount = 0;
                payrollCalculationProduct.AmountCurrency = 0;
                payrollCalculationProduct.AmountEntCurrency = 0;
                payrollCalculationProduct.Quantity = 0;

                //Accounting (depending on grouping)
                payrollCalculationProduct.AccountDims = groupFirstTransactionItem.AccountDims;
                bool hasMultipleAccountings =/*isRetroactive && */groupTransationItems.Select(i => i.AccountingShortString).Distinct().Count() > 1;
                if (!hasMultipleAccountings)
                {
                    payrollCalculationProduct.AccountStd = groupFirstTransactionItem.AccountStd;
                    payrollCalculationProduct.AccountInternals.AddRange(groupFirstTransactionItem.AccountInternals);
                    //if (!groupFirstTransactionItem.IsDistributed)
                    //{
                    payrollCalculationProduct.AccountingShortString = groupFirstTransactionItem.AccountingShortString;
                    payrollCalculationProduct.AccountingLongString = groupFirstTransactionItem.AccountingLongString;
                    //}
                }

                if (payrollCalculationProduct.TimeUnit != (int)TermGroup_PayrollProductTimeUnit.Hours && !isRetroactive && !isVacationYearEnd)
                    payrollCalculationProduct.SetQuantityDaysValues(groupTransationItems, groupFirstTransactionItem.QuantityDays);

                foreach (var transactionItem in groupTransationItems)
                {
                    #region Accumulate PayrollProduct

                    payrollCalculationProduct.Amount += transactionItem.Amount;
                    payrollCalculationProduct.AmountCurrency += transactionItem.AmountCurrency;
                    payrollCalculationProduct.AmountEntCurrency += transactionItem.AmountEntCurrency;

                    if (payrollCalculationProduct.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.Hours || isVacationYearEnd)
                        payrollCalculationProduct.Quantity += transactionItem.Quantity;

                    if (!payrollCalculationProduct.HasComment)
                        payrollCalculationProduct.HasComment = !String.IsNullOrEmpty(transactionItem.Comment);

                    if (!payrollCalculationProduct.AttestStates.Any(a => a.AttestStateId == transactionItem.AttestStateId))
                        payrollCalculationProduct.AttestStates.Add(transactionItem.GetAttestState());
                    payrollCalculationProduct.SetAttestStates(transactionItem.HasSameAttestState);

                    if (transactionItem.IsScheduleTransaction)
                        payrollCalculationProduct.HasPayrollScheduleTransactions = true;
                    else
                        payrollCalculationProduct.HasPayrollTransactions = true;

                    #endregion

                    #region AttestTransitionLog

                    foreach (AttestTransitionLogDTO attestTransitionLog in transactionItem.AttestTransitionLogs)
                    {
                        payrollCalculationProduct.AttestTransitionLogs.Add(new AttestTransitionLogDTO
                        {
                            TimePayrollTransactionId = transactionItem.TimePayrollTransactionId,
                            AttestTransitionLogId = attestTransitionLog.AttestTransitionLogId,
                            AttestStateFromName = attestTransitionLog.AttestStateFromName,
                            AttestStateToName = attestTransitionLog.AttestStateToName,
                            AttestTransitionDate = attestTransitionLog.AttestTransitionDate,
                            AttestTransitionUserId = attestTransitionLog.AttestTransitionUserId,
                            AttestTransitionUserName = attestTransitionLog.AttestTransitionUserName,
                            AttestTransitionCreatedBySupport = attestTransitionLog.AttestTransitionCreatedBySupport,
                        });
                    }

                    #endregion

                    #region Update transaction

                    transactionItem.PayrollCalculationProductUniqueId = payrollCalculationProduct.UniqueId;

                    #endregion

                    payrollCalculationProduct.AttestPayrollTransactions.Add(transactionItem);
                }

                payrollCalculationProducts.Add(payrollCalculationProduct);
            }

            #endregion

            return payrollCalculationProducts;
        }

        private static void SetAbsenceIntervalNr(IEnumerable<AttestPayrollTransactionDTO> transactionItems, bool isPayrollSlip)
        {
            if (transactionItems.IsNullOrEmpty() || !PayrollRulesUtil.IsAbsence(transactionItems.First().TransactionSysPayrollTypeLevel1, transactionItems.First().TransactionSysPayrollTypeLevel2))
                return;
            if (isPayrollSlip && PayrollRulesUtil.IsAbsenceVacationAll(transactionItems.First().TransactionSysPayrollTypeLevel1, transactionItems.First().TransactionSysPayrollTypeLevel2, transactionItems.First().TransactionSysPayrollTypeLevel3, transactionItems.First().TransactionSysPayrollTypeLevel4))
                return;

            int absenceIntervalNr = 0;
            DateTime? previousDate = null;

            //Give each transaction AbsenceIntervalNr based on cohesive dateintervals
            foreach (var transactionItemsByDate in transactionItems.GroupBy(i => i.Date).OrderBy(i => i.Key))
            {
                var currentDate = transactionItemsByDate.Key;
                if (!previousDate.HasValue)
                    absenceIntervalNr = 1;
                else if (previousDate.Value.AddDays(1) < currentDate)
                    absenceIntervalNr++;

                previousDate = currentDate;

                foreach (var transaction in transactionItemsByDate)
                {
                    transaction.AbsenceIntervalNr = absenceIntervalNr;
                }
            }
        }

        #endregion

        #endregion

    }
}
