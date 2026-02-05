using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Locks a Payrollperiod
        /// </summary>
        /// <returns>Output DTO</returns>
        private LockUnlockPayrollPeriodOutputDTO TaskLockPayrollPeriod()
        {
            var (iDTO, oDTO) = InitTask<LockUnlockPayrollPeriodInputDTO, LockUnlockPayrollPeriodOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeIds.IsNullOrEmpty() || iDTO.TimePeriodIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #region Init

            int noOfValidTransactions = 0;
            int noOfInvalidTransactions = 0;
            bool continueIfNotSucces = iDTO.EmployeeIds.Count > 1;
            List<int> sendPayrollSlipPublishedUserIds = new List<int>();

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    StartWatch("Method", append: true);

                    StartWatch("Prereq", append: true);
                    #region Prereq

                    DateTime logDate = DateTime.Now;

                    //VacactionGroups
                    List<VacationGroup> vacationGroups = GetVacationGroupsWithSEFromCache();

                    //TimeAccumulators 
                    List<TimeAccumulator> timeAccumulators = TimeAccumulatorManager.GetTimeAccumulators(entities, actorCompanyId);

                    //Settings
                    int companyPayrollResultingAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus);
                    int companySalaryPaymentLockedAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentLockedAttestStateId);

                    //AttestState
                    AttestStateDTO attestStateFrom = GetAttestState(companyPayrollResultingAttestStateId);
                    if (attestStateFrom == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateTo = GetAttestStateFromCache(companySalaryPaymentLockedAttestStateId);
                    if (attestStateTo == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));
                        return oDTO;
                    }

                    //Get AttestTransition from given AttestStates
                    AttestTransitionDTO attestTransition = GetAttestTransition(attestStateFrom.AttestStateId, attestStateTo.AttestStateId);
                    if (attestTransition == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.SaveAttestNoAttestTransitions, GetText(10086, "Inga attestövergångar hittades"));
                        return oDTO;
                    }

                    List<AttestTransitionDTO> attestTransitionsToState = new List<AttestTransitionDTO> { attestTransition };

                    AttestStateDTO attestStateInitial = null;
                    if (iDTO.IgnoreResultingAttestStateId)
                        attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);

                    if (attestStateInitial != null)
                    {
                        AttestTransitionDTO attestTransitionInitial = GetAttestTransition(attestStateInitial.AttestStateId, attestStateTo.AttestStateId);
                        if (attestTransitionInitial != null)
                            attestTransitionsToState.Add(attestTransitionInitial);
                    }

                    AddEmployeesWithEmploymentToCache(iDTO.EmployeeIds);
                    List<EmployeeGroup> employeeGroups = GetEmployeeGroupsWithDeviationCausesDayTypesAndTransitionsFromCache();
                    List<PayrollGroup> payrollGroups = GetPayrollGroupsWithPriceTypesSettingsReportsAccountsVacationGroupAndProductsFromCache();
                    List<PayrollPriceType> payrollPriceTypes = GetPayrollPriceTypesWithPeriodsFromCache();

                    Dictionary<int, string> disbursementMethodsDict = EmployeeManager.GetEmployeeDisbursementMethods((int)TermGroup_Languages.Swedish);
                    List<PayrollProduct> companyProducts = ProductManager.GetPayrollProductsWithSettings(entities, ActorCompanyId, null);
                    List<AccountDim> accountDimInternals = AccountManager.GetAccountDimInternalsByCompany(entities, ActorCompanyId);
                    PaymentInformation paymentInformation = PaymentManager.GetPaymentInformationFromActor(entities, ActorCompanyId, true, false);
                    List<PayrollStartValueHead> payrollStartValueHeads = PayrollManager.GetPayrollStartValueHeads(entities, ActorCompanyId);

                    #endregion

                    StopWatch("Prereq");

                    foreach (int timePeriodId in iDTO.TimePeriodIds)
                    {
                        StartWatch("Period", append: true);
                        #region TimePeriod

                        StartWatch("Period-Prereq", append: true);
                        #region Prereq

                        TimePeriod timePeriod = GetTimePeriod(timePeriodId);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                            return oDTO;
                        }

                        //Valid AttestTransitions                        
                        List<AttestUserRoleView> userValidTransitions = GetAttestUserRoleViews(timePeriod.StartDate, timePeriod.StopDate);
                        if (userValidTransitions.IsNullOrEmpty())
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsAttestRole, GetText(10086, "Inga attestövergångar hittades"));
                            return oDTO;
                        }

                        if (!HasValidAttestTransition(false, attestTransition, null, userValidTransitions))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsAttestRole, GetText(11830, "Giltig attestövergång saknas"));
                            return oDTO;
                        }

                        //FinalSalary
                        List<VacationYearEndRow> finalSalaryVacationYearEndRows = GetVacationYearEndRowsForCompanyAndPeriod(actorCompanyId, SoeVacationYearEndType.FinalSalary, timePeriodId);

                        //EmployeeTimePeriod
                        List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriodsWithValuesAndWarnings(timePeriod.TimePeriodId, iDTO.EmployeeIds);
                        Dictionary<int, List<EmployeeTimePeriod>> employeeTimePeriodsGrouped = employeeTimePeriods.GroupBy(g => g.EmployeeId).ToDictionary(x => x.Key, x => x.ToList());

                        List<PayrollPeriodChangeHead> payrollPeriodChangeHeads = GetPayrollPeriodChangeHeads(employeeTimePeriods.Select(x => x.EmployeeTimePeriodId).ToList(), PayrollPeriodChangeHeadType.Lock);
                        Dictionary<int, List<PayrollPeriodChangeHead>> payrollPeriodChangeHeadsGrouped = payrollPeriodChangeHeads.GroupBy(g => g.EmployeeTimePeriodId).ToDictionary(x => x.Key, x => x.ToList());

                        #endregion
                        StopWatch("Period-Prereq");

                        foreach (int employeeId in iDTO.EmployeeIds)
                        {
                            StartWatch("Employee", append: true);
                            
                            #region Employee

                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, false);
                            if (employee == null)
                                continue;

                            List<DateTime> employmentDatesInPeriod = employee.GetEmploymentDates(timePeriod.StartDate, timePeriod.StopDate);
                            bool isFinalSalaryPeriod = finalSalaryVacationYearEndRows.Any(row => row.EmployeeId == employeeId);

                            #endregion

                            #region Check warnings

                            EmployeeTimePeriod employeeTimePeriod = employeeTimePeriodsGrouped.ContainsKey(employee.EmployeeId) ? employeeTimePeriodsGrouped[employee.EmployeeId].FirstOrDefault() : null;
                            if (employeeTimePeriod == null)
                            {
                                if (continueIfNotSucces)
                                {
                                    continue;
                                }
                                else
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10089, "Period hittades inte.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                    return oDTO;
                                }
                            }
                            
                            if(employeeTimePeriod.HasStoppingWarnings())
                            {
                                if (continueIfNotSucces)
                                {
                                    continue;
                                }
                                else
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8955, "En eller flera stoppande varningar måste hanteras.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                    return oDTO;
                                }
                            }
                            
                            #endregion

                            StopWatch("Employee");

                            StartWatch("GetPayrollCalculationProducts", append: true);

                            #region GetPayrollCalculationProducts

                            List<PayrollCalculationProductDTO> payrollCalculationProductItems = TimeTreePayrollManager.GetPayrollCalculationProducts(
                                entities,
                                actorCompanyId,
                                timePeriod,
                                employee,
                                getEmployeeTimePeriodSettings: false,
                                showAllTransactions: true,
                                companyAccountDims: new List<AccountDimDTO>(),
                                employeeGroups: employeeGroups,
                                timePayrollTransactionItems: null,
                                timePayrollTransactionAccountStds: new List<AccountDTO>(),
                                timePayrollTransactionAccountInternalItems: new List<GetTimePayrollTransactionAccountsForEmployee_Result>(),
                                timePayrollScheduleTransactionItems: null,
                                timePayrollScheduleTransactionAccountStds: new List<AccountDTO>(),
                                timePayrollScheduleTransactionAccountInternalItems: new List<GetTimePayrollScheduleTransactionAccountsForEmployee_Result>(),
                                employeeTimePeriods: null, isPayrollSlip: true).ToList();

                            #endregion

                            StopWatch("GetPayrollCalculationProducts");

                            StartWatch("CalculateSums", append: true);
                            
                            #region Calculate sums

                            PayrollCalculationPeriodSumDTO periodSum = PayrollRulesUtil.CalculateSum(payrollCalculationProductItems);
                            //Leave this for the new control framework aswell since it is not going to be migrated
                            if (periodSum.HasNetSalaryDiff)
                            {
                                if (continueIfNotSucces)
                                {
                                    continue;
                                }
                                else
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8721, "Perioden behöver räknas om innan den kan låsas.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                    return oDTO;
                                }
                            }

                            #endregion

                            StopWatch("CalculateSums");

                            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {
                                InitTransaction(transaction);

                                StartWatch("Employee-Prereq", append: true);
                                #region Prereq

                                List<TimePayrollTransaction> allTimePayrollTransactions = GetTimePayrollTransactionsWithExtendedAndTimeBlockDate(payrollCalculationProductItems);
                                if (!allTimePayrollTransactions.Any())
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10090, "Inga transaktioner med rätt status hittades för perioden.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }
                                if (!allTimePayrollTransactions.Any(x => x.TimeBlockDate != null))// Ensure that timeBlockDate is loaded
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10091, "Perioden innehåller transaktioner utan komplett information.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }
                                if (!allTimePayrollTransactions.Any(x => x.TimePayrollTransactionExtended != null))// Ensure that extended is loaded - its is needed for calculating vacation
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10091, "Perioden innehåller transaktioner utan komplett information.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }

                                //this must be done before we check for companyPayrollResultingAttestStateId  
                                allTimePayrollTransactions = allTimePayrollTransactions.Where(i => employmentDatesInPeriod.Contains(i.TimeBlockDate.Date) || i.TimePeriodId.HasValue).ToList();   //transactions with a periodid are also valid even if they are not employmentDatesInPeriod
                                if (!iDTO.IgnoreResultingAttestStateId && allTimePayrollTransactions.Any(x => x.AttestStateId != companyPayrollResultingAttestStateId))
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8720, "Perioden innehåller transaktioner med ogiltig status för att kunna låsas.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }

                                Employment employment = employee.GetEmployment(timePeriod.PayrollStopDate) ?? employee.GetPrevEmployment(timePeriod.PayrollStopDate ?? timePeriod.StopDate);

                                bool calculateHours = false;
                                bool sammanfallande = false;

                                if (vacationGroups.Any() && employment != null && timePeriod.PayrollStopDate.HasValue)
                                {
                                    VacationGroup vacationGroup = employment.GetVacationGroup(timePeriod.PayrollStopDate, vacationGroups) ?? employment.GetVacationGroup(employment.GetEndDate(), vacationGroups);
                                    VacationGroupSE vacationGroupSE = vacationGroup?.VacationGroupSE.FirstOrDefault();
                                    if (vacationGroupSE != null)
                                    {
                                        calculateHours = vacationGroupSE.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours;
                                        TermGroup_VacationGroupCalculationType calculationType = (TermGroup_VacationGroupCalculationType)vacationGroupSE.CalculationType;
                                        sammanfallande = calculationType == TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement || calculationType == TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition;
                                    }
                                }

                                List<PayrollPeriodChangeRowDTO> changeDTOs = new List<PayrollPeriodChangeRowDTO>();

                                #endregion
                                StopWatch("Employee-Prereq");

                                StartWatch("SetEmployeeTimePeriodId", append: true);

                                #region Set EmployeeTimePeriodId on Transactions

                                allTimePayrollTransactions.ForEach(x => x.EmployeeTimePeriodId = employeeTimePeriod.EmployeeTimePeriodId);
                                var timePayrollScheduleTransactions = GetTimePayrollScheduleTransactions(payrollCalculationProductItems);
                                timePayrollScheduleTransactions.ForEach(x => x.EmployeeTimePeriodId = employeeTimePeriod.EmployeeTimePeriodId);

                                #endregion

                                StopWatch("SetEmployeeTimePeriodId");

                                StartWatch("Vacation", append: true);
                                #region Vacation

                                StartWatch("Vacation-PayrollPeriodChangeHead", append: true);
                                #region PayrollPeriodChangeHead

                                var existingChangeHeads = payrollPeriodChangeHeadsGrouped.ContainsKey(employeeTimePeriod.EmployeeTimePeriodId) ? payrollPeriodChangeHeadsGrouped[employeeTimePeriod.EmployeeTimePeriodId] : new List<PayrollPeriodChangeHead>();
                                foreach (var existingChangeHead in existingChangeHeads)//There should always be only one active, but to be sure.....
                                {
                                    SetModifiedProperties(existingChangeHead);
                                    existingChangeHead.State = (int)SoeEntityState.Deleted;
                                }

                                PayrollPeriodChangeHead newPayrollPeriodChangeHead = new PayrollPeriodChangeHead()
                                {
                                    EmployeeTimePeriodId = employeeTimePeriod.EmployeeTimePeriodId,
                                    ActorCompanyId = this.actorCompanyId,
                                    Type = (int)PayrollPeriodChangeHeadType.Lock,
                                    State = (int)SoeEntityState.Active
                                };
                                SetCreatedProperties(newPayrollPeriodChangeHead);
                                entities.PayrollPeriodChangeHead.AddObject(newPayrollPeriodChangeHead);

                                #endregion
                                StopWatch("Vacation-PayrollPeriodChangeHead");

                                StartWatch("Vacation-VacationInPeriod", append: true);
                                #region Vacation in period

                                decimal periodUsedDaysPaidCount = 0;                            //Betalda dagar
                                decimal periodUsedDaysUnpaidCount = 0;                          //Obetalda dagar
                                decimal periodUsedDaysAdvanceCount = 0;                         //Förskott
                                decimal periodUsedDaysYear1Count = 0;                           //Sparade år 1
                                decimal periodUsedDaysYear2Count = 0;                           //Sparade år 2
                                decimal periodUsedDaysYear3Count = 0;                           //Sparade år 3
                                decimal periodUsedDaysYear4Count = 0;                           //Sparade år 4
                                decimal periodUsedDaysYear5Count = 0;                           //Sparade år 5   
                                decimal periodUsedDaysOverdueCount = 0;                         //Förfallna dagar
                                decimal periodVacationCompensationPaidCount = 0;                //Betalda dagar
                                decimal periodVacationCompensationSavedYear1Count = 0;          //Slutlön - Sparade år 1
                                decimal periodVacationCompensationSavedYear2Count = 0;          //Slutlön - Sparade år 2
                                decimal periodVacationCompensationSavedYear3Count = 0;          //Slutlön - Sparade år 3
                                decimal periodVacationCompensationSavedYear4Count = 0;          //Slutlön - Sparade år 4
                                decimal periodVacationCompensationSavedYear5Count = 0;          //Slutlön - Sparade år 5
                                decimal periodVacationCompensationSavedOverdueCount = 0;        //Slutlön - Förfallna dagar
                                decimal periodVariableVacationAddition = 0;                     //Rörligt Semestertillägg
                                decimal periodPaidVacationAllowance = 0;                        //Förutbetald - Utbetald                                
                                decimal periodVacationPrepaymentInvert = 0;                     //Förutbetald - Motbokning 
                                decimal periodPaidVacationVariableAllowance = 0;                //Förutbetald rörligt semestertillägg- Utbetald
                                decimal periodPaidVacationVariableInvert = 0;                   //Förutbetald rörligt semestertillägg- Motbokning
                                decimal periodDebtAdvanceAmount = 0;                            //Summa förskott

                                decimal periodUsedDaysAllowanceYear1 = 0;
                                decimal periodUsedDaysAllowanceYear2 = 0;
                                decimal periodUsedDaysAllowanceYear3 = 0;
                                decimal periodUsedDaysAllowanceYear4 = 0;
                                decimal periodUsedDaysAllowanceYear5 = 0;
                                decimal periodUsedDaysAllowanceYearOverdue = 0;

                                decimal periodUsedDaysVariableAllowanceYear1 = 0;
                                decimal periodUsedDaysVariableAllowanceYear2 = 0;
                                decimal periodUsedDaysVariableAllowanceYear3 = 0;
                                decimal periodUsedDaysVariableAllowanceYear4 = 0;
                                decimal periodUsedDaysVariableAllowanceYear5 = 0;
                                decimal periodUsedDaysVariableAllowanceYearOverdue = 0;


                                VacationDaysCalculationDTO vacationDaysCalculationDTO = allTimePayrollTransactions.ToVacationDaysCalculationDTO(calculateHours);
                                periodUsedDaysPaidCount = vacationDaysCalculationDTO.PeriodUsedDaysPaidCount;
                                periodUsedDaysUnpaidCount = vacationDaysCalculationDTO.PeriodUsedDaysUnpaidCount;
                                periodUsedDaysAdvanceCount = vacationDaysCalculationDTO.PeriodUsedDaysAdvanceCount;
                                periodUsedDaysYear1Count = vacationDaysCalculationDTO.PeriodUsedDaysYear1Count;
                                periodUsedDaysYear2Count = vacationDaysCalculationDTO.PeriodUsedDaysYear2Count;
                                periodUsedDaysYear3Count = vacationDaysCalculationDTO.PeriodUsedDaysYear3Count;
                                periodUsedDaysYear4Count = vacationDaysCalculationDTO.PeriodUsedDaysYear4Count;
                                periodUsedDaysYear5Count = vacationDaysCalculationDTO.PeriodUsedDaysYear5Count;
                                periodUsedDaysOverdueCount = vacationDaysCalculationDTO.PeriodUsedDaysOverdueCount;
                                periodVacationCompensationPaidCount = vacationDaysCalculationDTO.PeriodVacationCompensationPaidCount;
                                periodVacationCompensationSavedYear1Count = vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear1Count;
                                periodVacationCompensationSavedYear2Count = vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear2Count;
                                periodVacationCompensationSavedYear3Count = vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear3Count;
                                periodVacationCompensationSavedYear4Count = vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear4Count;
                                periodVacationCompensationSavedYear5Count = vacationDaysCalculationDTO.PeriodVacationCompensationSavedYear5Count;
                                periodVacationCompensationSavedOverdueCount = vacationDaysCalculationDTO.PeriodVacationCompensationSavedOverdueCount;
                                periodVariableVacationAddition = vacationDaysCalculationDTO.PeriodVariableVacationAddition;
                                periodPaidVacationAllowance = vacationDaysCalculationDTO.PeriodVacationPrepaymentPaid;
                                periodVacationPrepaymentInvert = vacationDaysCalculationDTO.PeriodVacationPrepaymentInvert;
                                periodPaidVacationVariableAllowance = vacationDaysCalculationDTO.PeriodVariablePrepaymentPaid;
                                periodPaidVacationVariableInvert = vacationDaysCalculationDTO.PeriodVariablePrepaymentInvert;
                                periodDebtAdvanceAmount = vacationDaysCalculationDTO.PeriodDebtAdvanceAmount;

                                #endregion
                                StopWatch("Vacation-VacationInPeriod");

                                StartWatch("Vacation-EmployeeVacationSE", append: true);
                                #region EmployeeVacationSE

                                List<EmployeeVacationSE> vacations = EmployeeManager.GetEmployeeVacationSEs(entities, employeeId);
                                EmployeeVacationSE currentEmployeeVacationSe = vacations.Any() ? vacations.OrderBy(x => x.Created).ThenBy(x => x.Modified).Last() : null; //There should always be only one active EmployeeVacationSE
                                foreach (EmployeeVacationSE vacation in vacations)
                                {
                                    SetModifiedProperties(vacation);
                                    vacation.State = (int)SoeEntityState.Deleted;
                                }

                                EmployeeVacationSE newVacation = new EmployeeVacationSE()
                                {
                                    EmployeeId = employeeId,
                                    PrevEmployeeVacationSEId = currentEmployeeVacationSe?.EmployeeVacationSEId,
                                    EarnedDaysPaid = currentEmployeeVacationSe?.EarnedDaysPaid,
                                    EarnedDaysUnpaid = currentEmployeeVacationSe?.EarnedDaysUnpaid,
                                    EarnedDaysAdvance = currentEmployeeVacationSe?.EarnedDaysAdvance,
                                    SavedDaysYear1 = currentEmployeeVacationSe?.SavedDaysYear1,
                                    SavedDaysYear2 = currentEmployeeVacationSe?.SavedDaysYear2,
                                    SavedDaysYear3 = currentEmployeeVacationSe?.SavedDaysYear3,
                                    SavedDaysYear4 = currentEmployeeVacationSe?.SavedDaysYear4,
                                    SavedDaysYear5 = currentEmployeeVacationSe?.SavedDaysYear5,
                                    SavedDaysOverdue = currentEmployeeVacationSe?.SavedDaysOverdue,

                                    UsedDaysPaid = currentEmployeeVacationSe?.UsedDaysPaid,
                                    PaidVacationAllowance = currentEmployeeVacationSe?.PaidVacationAllowance,
                                    PaidVacationVariableAllowance = currentEmployeeVacationSe?.PaidVacationVariableAllowance,
                                    UsedDaysUnpaid = currentEmployeeVacationSe?.UsedDaysUnpaid,
                                    UsedDaysAdvance = currentEmployeeVacationSe?.UsedDaysAdvance,
                                    UsedDaysYear1 = currentEmployeeVacationSe?.UsedDaysYear1,
                                    UsedDaysYear2 = currentEmployeeVacationSe?.UsedDaysYear2,
                                    UsedDaysYear3 = currentEmployeeVacationSe?.UsedDaysYear3,
                                    UsedDaysYear4 = currentEmployeeVacationSe?.UsedDaysYear4,
                                    UsedDaysYear5 = currentEmployeeVacationSe?.UsedDaysYear5,
                                    UsedDaysOverdue = currentEmployeeVacationSe?.UsedDaysOverdue,

                                    RemainingDaysPaid = currentEmployeeVacationSe?.RemainingDaysPaid,
                                    RemainingDaysUnpaid = currentEmployeeVacationSe?.RemainingDaysUnpaid,
                                    RemainingDaysAdvance = currentEmployeeVacationSe?.RemainingDaysAdvance,
                                    RemainingDaysYear1 = currentEmployeeVacationSe?.RemainingDaysYear1,
                                    RemainingDaysYear2 = currentEmployeeVacationSe?.RemainingDaysYear2,
                                    RemainingDaysYear3 = currentEmployeeVacationSe?.RemainingDaysYear3,
                                    RemainingDaysYear4 = currentEmployeeVacationSe?.RemainingDaysYear4,
                                    RemainingDaysYear5 = currentEmployeeVacationSe?.RemainingDaysYear5,
                                    RemainingDaysOverdue = currentEmployeeVacationSe?.RemainingDaysOverdue,

                                    EarnedDaysRemainingHoursPaid = currentEmployeeVacationSe?.EarnedDaysRemainingHoursPaid,
                                    EarnedDaysRemainingHoursUnpaid = currentEmployeeVacationSe?.EarnedDaysRemainingHoursUnpaid,
                                    EarnedDaysRemainingHoursAdvance = currentEmployeeVacationSe?.EarnedDaysRemainingHoursAdvance,
                                    EarnedDaysRemainingHoursYear1 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear1,
                                    EarnedDaysRemainingHoursYear2 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear2,
                                    EarnedDaysRemainingHoursYear3 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear3,
                                    EarnedDaysRemainingHoursYear4 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear4,
                                    EarnedDaysRemainingHoursYear5 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear5,
                                    EarnedDaysRemainingHoursOverdue = currentEmployeeVacationSe?.EarnedDaysRemainingHoursOverdue,

                                    EmploymentRatePaid = currentEmployeeVacationSe?.EmploymentRatePaid,
                                    EmploymentRateYear1 = currentEmployeeVacationSe?.EmploymentRateYear1,
                                    EmploymentRateYear2 = currentEmployeeVacationSe?.EmploymentRateYear2,
                                    EmploymentRateYear3 = currentEmployeeVacationSe?.EmploymentRateYear3,
                                    EmploymentRateYear4 = currentEmployeeVacationSe?.EmploymentRateYear4,
                                    EmploymentRateYear5 = currentEmployeeVacationSe?.EmploymentRateYear5,
                                    EmploymentRateOverdue = currentEmployeeVacationSe?.EmploymentRateOverdue,

                                    DebtInAdvanceAmount = currentEmployeeVacationSe?.DebtInAdvanceAmount,
                                    DebtInAdvanceDueDate = currentEmployeeVacationSe?.DebtInAdvanceDueDate,
                                    DebtInAdvanceDelete = currentEmployeeVacationSe?.DebtInAdvanceDelete ?? false,

                                    RemainingDaysAllowanceYear1 = currentEmployeeVacationSe?.RemainingDaysAllowanceYear1,
                                    RemainingDaysAllowanceYear2 = currentEmployeeVacationSe?.RemainingDaysAllowanceYear2,
                                    RemainingDaysAllowanceYear3 = currentEmployeeVacationSe?.RemainingDaysAllowanceYear3,
                                    RemainingDaysAllowanceYear4 = currentEmployeeVacationSe?.RemainingDaysAllowanceYear4,
                                    RemainingDaysAllowanceYear5 = currentEmployeeVacationSe?.RemainingDaysAllowanceYear5,
                                    RemainingDaysAllowanceYearOverdue = currentEmployeeVacationSe?.RemainingDaysAllowanceYearOverdue,
                                    RemainingDaysVariableAllowanceYear1 = currentEmployeeVacationSe?.RemainingDaysVariableAllowanceYear1,
                                    RemainingDaysVariableAllowanceYear2 = currentEmployeeVacationSe?.RemainingDaysVariableAllowanceYear2,
                                    RemainingDaysVariableAllowanceYear3 = currentEmployeeVacationSe?.RemainingDaysVariableAllowanceYear3,
                                    RemainingDaysVariableAllowanceYear4 = currentEmployeeVacationSe?.RemainingDaysVariableAllowanceYear4,
                                    RemainingDaysVariableAllowanceYear5 = currentEmployeeVacationSe?.RemainingDaysVariableAllowanceYear5,
                                    RemainingDaysVariableAllowanceYearOverdue = currentEmployeeVacationSe?.RemainingDaysVariableAllowanceYearOverdue,
                                };
                                entities.EmployeeVacationSE.AddObject(newVacation);

                                if (vacations.Count == 0)
                                    SetCreatedProperties(newVacation);
                                else
                                {
                                    newVacation.Created = vacations.First().Created;
                                    newVacation.CreatedBy = vacations.First().CreatedBy;
                                    SetModifiedProperties(newVacation);
                                }

                                #endregion
                                StopWatch("Vacation-EmployeeVacationSE");

                                StartWatch("Vacation-Update", append: true);
                                #region Update values

                                #region Update used columns

                                if (isFinalSalaryPeriod && sammanfallande)
                                {
                                    periodUsedDaysPaidCount = newVacation.RemainingDaysPaid ?? 0; //we need to clear paid days if this is final salary period and sammanfallande
                                }

                                decimal currentUsedDaysPaid = (newVacation.UsedDaysPaid ?? 0);
                                decimal newUsedDaysPaid = currentUsedDaysPaid + periodUsedDaysPaidCount + periodVacationCompensationPaidCount;
                                newVacation.UsedDaysPaid = newUsedDaysPaid;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysPaid, currentUsedDaysPaid.ToString(), newUsedDaysPaid.ToString(), periodUsedDaysPaidCount + periodVacationCompensationPaidCount));

                                decimal currentPaidVacationAllowance = (newVacation.PaidVacationAllowance ?? 0);
                                //if (currentPaidVacationAllowance == 0) //we need to do this because this column did not get updated before.
                                //    currentPaidVacationAllowance = currentUsedDaysPaid;

                                decimal newPaidVacationAllowance = currentPaidVacationAllowance + periodPaidVacationAllowance + periodUsedDaysPaidCount + periodVacationCompensationPaidCount + periodVacationPrepaymentInvert;
                                //if (newUsedDaysPaid > newPaidVacationAllowance)//PaidVacationAllowance shoul never be LESS then UsedDaysPaid - NOT TRUE ANYMORE
                                //    newPaidVacationAllowance = newUsedDaysPaid;

                                newVacation.PaidVacationAllowance = newPaidVacationAllowance;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.PaidVacationAllowance, currentPaidVacationAllowance.ToString(), newPaidVacationAllowance.ToString(), newPaidVacationAllowance - currentPaidVacationAllowance));

                                decimal currentPaidVacationVariableAllowance = (newVacation.PaidVacationVariableAllowance ?? 0);
                                decimal newPaidVacationVariableAllowance = currentPaidVacationVariableAllowance + periodPaidVacationVariableAllowance + periodVariableVacationAddition + periodPaidVacationVariableInvert;

                                newVacation.PaidVacationVariableAllowance = newPaidVacationVariableAllowance;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.PaidVacationVariableAllowance, currentPaidVacationVariableAllowance.ToString(), newPaidVacationVariableAllowance.ToString(), newPaidVacationVariableAllowance - currentPaidVacationVariableAllowance));

                                if (isFinalSalaryPeriod)
                                    periodUsedDaysUnpaidCount = newVacation.RemainingDaysUnpaid ?? 0; //we need to clear unpaid days if this is final salary period

                                decimal currentUsedDaysUnpaid = (newVacation.UsedDaysUnpaid ?? 0);
                                decimal newUsedDaysUnpaid = currentUsedDaysUnpaid + periodUsedDaysUnpaidCount;
                                newVacation.UsedDaysUnpaid = newUsedDaysUnpaid;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysUnpaid, currentUsedDaysUnpaid.ToString(), newUsedDaysUnpaid.ToString(), periodUsedDaysUnpaidCount));

                                decimal currentUsedDaysAdvance = (newVacation.UsedDaysAdvance ?? 0);
                                decimal newUsedDaysAdvance = currentUsedDaysAdvance + periodUsedDaysAdvanceCount;
                                newVacation.UsedDaysAdvance = newUsedDaysAdvance;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysAdvance, currentUsedDaysAdvance.ToString(), newUsedDaysAdvance.ToString(), periodUsedDaysAdvanceCount));

                                decimal currentUsedDaysYear5 = (newVacation.UsedDaysYear5 ?? 0);
                                decimal newUsedDaysYear5 = currentUsedDaysYear5 + periodUsedDaysYear5Count + periodVacationCompensationSavedYear5Count;
                                newVacation.UsedDaysYear5 = newUsedDaysYear5;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear5, currentUsedDaysYear5.ToString(), newUsedDaysYear5.ToString(), periodUsedDaysYear5Count + periodVacationCompensationSavedYear5Count));

                                decimal currentUsedDaysYear4 = (newVacation.UsedDaysYear4 ?? 0);
                                decimal newUsedDaysYear4 = currentUsedDaysYear4 + periodUsedDaysYear4Count + periodVacationCompensationSavedYear4Count;
                                newVacation.UsedDaysYear4 = newUsedDaysYear4;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear4, currentUsedDaysYear4.ToString(), newUsedDaysYear4.ToString(), periodUsedDaysYear4Count + periodVacationCompensationSavedYear4Count));

                                decimal currentUsedDaysYear3 = (newVacation.UsedDaysYear3 ?? 0);
                                decimal newUsedDaysYear3 = currentUsedDaysYear3 + periodUsedDaysYear3Count + periodVacationCompensationSavedYear3Count;
                                newVacation.UsedDaysYear3 = newUsedDaysYear3;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear3, currentUsedDaysYear3.ToString(), newUsedDaysYear3.ToString(), periodUsedDaysYear3Count + periodVacationCompensationSavedYear3Count));

                                decimal currentUsedDaysYear2 = (newVacation.UsedDaysYear2 ?? 0);
                                decimal newUsedDaysYear2 = currentUsedDaysYear2 + periodUsedDaysYear2Count + periodVacationCompensationSavedYear2Count;
                                newVacation.UsedDaysYear2 = newUsedDaysYear2;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear2, currentUsedDaysYear2.ToString(), newUsedDaysYear2.ToString(), periodUsedDaysYear2Count + periodVacationCompensationSavedYear2Count));

                                decimal currentUsedDaysYear1 = (newVacation.UsedDaysYear1 ?? 0);
                                decimal newUsedDaysYear1 = currentUsedDaysYear1 + periodUsedDaysYear1Count + periodVacationCompensationSavedYear1Count;
                                newVacation.UsedDaysYear1 = newUsedDaysYear1;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear1, currentUsedDaysYear1.ToString(), newUsedDaysYear1.ToString(), periodUsedDaysYear1Count + periodVacationCompensationSavedYear1Count));

                                decimal currentUsedDaysOverdue = (newVacation.UsedDaysOverdue ?? 0);
                                decimal newUsedDaysOverdue = currentUsedDaysOverdue + periodUsedDaysOverdueCount + periodVacationCompensationSavedOverdueCount;
                                newVacation.UsedDaysOverdue = newUsedDaysOverdue;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysOverdue, currentUsedDaysOverdue.ToString(), newUsedDaysOverdue.ToString(), periodUsedDaysOverdueCount + periodVacationCompensationSavedOverdueCount));

                                decimal currentDebtAdvanceAmount = (newVacation.DebtInAdvanceAmount ?? 0);
                                decimal newDebtAdvanceAmount = currentDebtAdvanceAmount + periodDebtAdvanceAmount;
                                newVacation.DebtInAdvanceAmount = newDebtAdvanceAmount;
                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.DebtInAdvanceAmount, currentDebtAdvanceAmount.ToString(), newDebtAdvanceAmount.ToString(), periodDebtAdvanceAmount));

                                decimal currentRemainingDaysAllowanceYear1 = (newVacation.RemainingDaysAllowanceYear1 ?? 0);
                                decimal newRemainingDaysAllowanceYear1 = currentRemainingDaysAllowanceYear1 + periodUsedDaysAllowanceYear1;
                                newVacation.RemainingDaysAllowanceYear1 = newRemainingDaysAllowanceYear1;
                                if (currentRemainingDaysAllowanceYear1 != newRemainingDaysAllowanceYear1)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear1, currentRemainingDaysAllowanceYear1.ToString(), newRemainingDaysAllowanceYear1.ToString(), periodUsedDaysAllowanceYear1));

                                decimal currentRemainingDaysAllowanceYear2 = (newVacation.RemainingDaysAllowanceYear2 ?? 0);
                                decimal newRemainingDaysAllowanceYear2 = currentRemainingDaysAllowanceYear2 + periodUsedDaysAllowanceYear2;
                                newVacation.RemainingDaysAllowanceYear2 = newRemainingDaysAllowanceYear2;
                                if (currentRemainingDaysAllowanceYear2 != newRemainingDaysAllowanceYear2)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear2, currentRemainingDaysAllowanceYear2.ToString(), newRemainingDaysAllowanceYear2.ToString(), periodUsedDaysAllowanceYear2));

                                decimal currentRemainingDaysAllowanceYear3 = (newVacation.RemainingDaysAllowanceYear3 ?? 0);
                                decimal newRemainingDaysAllowanceYear3 = currentRemainingDaysAllowanceYear3 + periodUsedDaysAllowanceYear3;
                                newVacation.RemainingDaysAllowanceYear3 = newRemainingDaysAllowanceYear3;
                                if (currentRemainingDaysAllowanceYear3 != newRemainingDaysAllowanceYear3)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear3, currentRemainingDaysAllowanceYear3.ToString(), newRemainingDaysAllowanceYear3.ToString(), periodUsedDaysAllowanceYear3));

                                decimal currentRemainingDaysAllowanceYear4 = (newVacation.RemainingDaysAllowanceYear4 ?? 0);
                                decimal newRemainingDaysAllowanceYear4 = currentRemainingDaysAllowanceYear4 + periodUsedDaysAllowanceYear4;
                                newVacation.RemainingDaysAllowanceYear4 = newRemainingDaysAllowanceYear4;
                                if (currentRemainingDaysAllowanceYear4 != newRemainingDaysAllowanceYear4)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear4, currentRemainingDaysAllowanceYear4.ToString(), newRemainingDaysAllowanceYear4.ToString(), periodUsedDaysAllowanceYear4));

                                decimal currentRemainingDaysAllowanceYear5 = (newVacation.RemainingDaysAllowanceYear5 ?? 0);
                                decimal newRemainingDaysAllowanceYear5 = currentRemainingDaysAllowanceYear5 + periodUsedDaysAllowanceYear5;
                                newVacation.RemainingDaysAllowanceYear5 = newRemainingDaysAllowanceYear5;
                                if (currentRemainingDaysAllowanceYear5 != newRemainingDaysAllowanceYear5)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear5, currentRemainingDaysAllowanceYear5.ToString(), newRemainingDaysAllowanceYear5.ToString(), periodUsedDaysAllowanceYear5));

                                decimal currentRemainingDaysAllowanceYearOverdue = (newVacation.RemainingDaysAllowanceYearOverdue ?? 0);
                                decimal newRemainingDaysAllowanceYearOverdue = currentRemainingDaysAllowanceYearOverdue + periodUsedDaysAllowanceYearOverdue;
                                newVacation.RemainingDaysAllowanceYearOverdue = newRemainingDaysAllowanceYearOverdue;
                                if (currentRemainingDaysAllowanceYearOverdue != newRemainingDaysAllowanceYearOverdue)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYearOverdue, currentRemainingDaysAllowanceYearOverdue.ToString(), newRemainingDaysAllowanceYearOverdue.ToString(), periodUsedDaysAllowanceYearOverdue));


                                decimal currentRemainingDaysVariableAllowanceYear1 = (newVacation.RemainingDaysVariableAllowanceYear1 ?? 0);
                                decimal newRemainingDaysVariableAllowanceYear1 = currentRemainingDaysVariableAllowanceYear1 + periodUsedDaysVariableAllowanceYear1;
                                newVacation.RemainingDaysVariableAllowanceYear1 = newRemainingDaysVariableAllowanceYear1;
                                if (currentRemainingDaysVariableAllowanceYear1 != newRemainingDaysVariableAllowanceYear1)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear1, currentRemainingDaysVariableAllowanceYear1.ToString(), newRemainingDaysVariableAllowanceYear1.ToString(), periodUsedDaysVariableAllowanceYear1));

                                decimal currentRemainingDaysVariableAllowanceYear2 = (newVacation.RemainingDaysVariableAllowanceYear2 ?? 0);
                                decimal newRemainingDaysVariableAllowanceYear2 = currentRemainingDaysVariableAllowanceYear2 + periodUsedDaysVariableAllowanceYear2;
                                newVacation.RemainingDaysVariableAllowanceYear2 = newRemainingDaysVariableAllowanceYear2;
                                if (currentRemainingDaysVariableAllowanceYear2 != newRemainingDaysVariableAllowanceYear2)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear2, currentRemainingDaysVariableAllowanceYear2.ToString(), newRemainingDaysVariableAllowanceYear2.ToString(), periodUsedDaysVariableAllowanceYear2));

                                decimal currentRemainingDaysVariableAllowanceYear3 = (newVacation.RemainingDaysVariableAllowanceYear3 ?? 0);
                                decimal newRemainingDaysVariableAllowanceYear3 = currentRemainingDaysVariableAllowanceYear3 + periodUsedDaysVariableAllowanceYear3;
                                newVacation.RemainingDaysVariableAllowanceYear3 = newRemainingDaysVariableAllowanceYear3;
                                if (currentRemainingDaysVariableAllowanceYear3 != newRemainingDaysVariableAllowanceYear3)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear3, currentRemainingDaysVariableAllowanceYear3.ToString(), newRemainingDaysVariableAllowanceYear3.ToString(), periodUsedDaysVariableAllowanceYear3));

                                decimal currentRemainingDaysVariableAllowanceYear4 = (newVacation.RemainingDaysVariableAllowanceYear4 ?? 0);
                                decimal newRemainingDaysVariableAllowanceYear4 = currentRemainingDaysVariableAllowanceYear4 + periodUsedDaysVariableAllowanceYear4;
                                newVacation.RemainingDaysVariableAllowanceYear4 = newRemainingDaysVariableAllowanceYear4;
                                if (currentRemainingDaysVariableAllowanceYear4 != newRemainingDaysVariableAllowanceYear4)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear4, currentRemainingDaysVariableAllowanceYear4.ToString(), newRemainingDaysVariableAllowanceYear4.ToString(), periodUsedDaysVariableAllowanceYear4));

                                decimal currentRemainingDaysVariableAllowanceYear5 = (newVacation.RemainingDaysVariableAllowanceYear5 ?? 0);
                                decimal newRemainingDaysVariableAllowanceYear5 = currentRemainingDaysVariableAllowanceYear5 + periodUsedDaysVariableAllowanceYear5;
                                newVacation.RemainingDaysVariableAllowanceYear5 = newRemainingDaysVariableAllowanceYear5;
                                if (currentRemainingDaysVariableAllowanceYear5 != newRemainingDaysVariableAllowanceYear5)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear5, currentRemainingDaysVariableAllowanceYear5.ToString(), newRemainingDaysVariableAllowanceYear5.ToString(), periodUsedDaysVariableAllowanceYear5));

                                decimal currentRemainingDaysVariableAllowanceYearOverdue = (newVacation.RemainingDaysVariableAllowanceYearOverdue ?? 0);
                                decimal newRemainingDaysVariableAllowanceYearOverdue = currentRemainingDaysVariableAllowanceYearOverdue + periodUsedDaysVariableAllowanceYearOverdue;
                                newVacation.RemainingDaysVariableAllowanceYearOverdue = newRemainingDaysVariableAllowanceYearOverdue;
                                if (currentRemainingDaysVariableAllowanceYearOverdue != newRemainingDaysVariableAllowanceYearOverdue)
                                    changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYearOverdue, currentRemainingDaysVariableAllowanceYearOverdue.ToString(), newRemainingDaysVariableAllowanceYearOverdue.ToString(), periodUsedDaysVariableAllowanceYearOverdue));

                                if (isFinalSalaryPeriod && sammanfallande)
                                {
                                    decimal currentEarnedDaysPaid = (newVacation.EarnedDaysPaid ?? 0);
                                    decimal newEarnedDaysPaid = 0;
                                    if (newVacation.EarnedDaysPaid != newEarnedDaysPaid)
                                    {
                                        var change = newEarnedDaysPaid - currentEarnedDaysPaid;
                                        newVacation.EarnedDaysPaid = newEarnedDaysPaid;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.EarnedDaysPaid, currentEarnedDaysPaid.ToString(), newEarnedDaysPaid.ToString(), change));
                                    }

                                    decimal currentRemainingDaysPaid = (newVacation.RemainingDaysPaid ?? 0);
                                    decimal newRemainingDaysPaid = 0;
                                    if (newVacation.RemainingDaysPaid != newRemainingDaysPaid)
                                    {
                                        var change = newRemainingDaysPaid - currentRemainingDaysPaid;
                                        newVacation.RemainingDaysPaid = newRemainingDaysPaid;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysPaid, currentRemainingDaysPaid.ToString(), newRemainingDaysPaid.ToString(), change));
                                    }
                                }

                                #endregion

                                #region Update remaining columns

                                newVacation.RemainingDaysPaid = (newVacation.EarnedDaysPaid ?? 0) - newVacation.UsedDaysPaid;
                                if (isFinalSalaryPeriod && sammanfallande && newVacation.RemainingDaysPaid < 0)
                                    newVacation.RemainingDaysPaid = 0;

                                newVacation.RemainingDaysUnpaid = (newVacation.EarnedDaysUnpaid ?? 0) - newVacation.UsedDaysUnpaid;
                                if (isFinalSalaryPeriod && sammanfallande && newVacation.RemainingDaysUnpaid < 0)
                                    newVacation.RemainingDaysUnpaid = 0;

                                newVacation.RemainingDaysAdvance = (newVacation.EarnedDaysAdvance ?? 0) - newVacation.UsedDaysAdvance;
                                newVacation.RemainingDaysYear1 = (newVacation.SavedDaysYear1 ?? 0) - newVacation.UsedDaysYear1;
                                newVacation.RemainingDaysYear2 = (newVacation.SavedDaysYear2 ?? 0) - newVacation.UsedDaysYear2;
                                newVacation.RemainingDaysYear3 = (newVacation.SavedDaysYear3 ?? 0) - newVacation.UsedDaysYear3;
                                newVacation.RemainingDaysYear4 = (newVacation.SavedDaysYear4 ?? 0) - newVacation.UsedDaysYear4;
                                newVacation.RemainingDaysYear5 = (newVacation.SavedDaysYear5 ?? 0) - newVacation.UsedDaysYear5;
                                newVacation.RemainingDaysOverdue = (newVacation.SavedDaysOverdue ?? 0) - newVacation.UsedDaysOverdue;

                                #region Hours

                                if (vacationGroups.Any() && employment != null && timePeriod.PayrollStopDate.HasValue)
                                {
                                    var group = employment.GetVacationGroup(timePeriod.PayrollStopDate, vacationGroups);
                                    if (group != null)
                                    {
                                        var vacationGroupSE = group.VacationGroupSE.FirstOrDefault();
                                        if (vacationGroupSE != null && vacationGroupSE.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours)
                                        {
                                            //VacationGroupVacationHandleRule
                                            EmployeeFactor vacationNet = GetEmployeeFactorFromCache(employment.EmployeeId, TermGroup_EmployeeFactorType.Net, timePeriod.PayrollStopDate.Value);
                                            decimal netFactor = vacationNet == null ? 5 : vacationNet.Factor;
                                            var workTimeWeek = employment.GetWorkTimeWeek();
                                            if (workTimeWeek > 0)
                                            {
                                                decimal dayFactor = decimal.Divide(decimal.Divide(workTimeWeek, new decimal(60)), netFactor);
                                                newVacation.EarnedDaysRemainingHoursPaid = this.Round((newVacation.RemainingDaysPaid * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursUnpaid = this.Round((newVacation.EarnedDaysUnpaid * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursAdvance = this.Round((newVacation.RemainingDaysAdvance * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursYear1 = this.Round((newVacation.RemainingDaysYear1 * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursYear2 = this.Round((newVacation.RemainingDaysYear2 * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursYear3 = this.Round((newVacation.RemainingDaysYear3 * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursYear4 = this.Round((newVacation.RemainingDaysYear4 * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursYear5 = this.Round((newVacation.RemainingDaysYear5 * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                                newVacation.EarnedDaysRemainingHoursOverdue = this.Round((newVacation.RemainingDaysOverdue * dayFactor) ?? 0, TermGroup_PayrollProductCentRoundingType.Up, TermGroup_PayrollProductCentRoundingLevel.One);
                                            }
                                        }
                                    }
                                }

                                #endregion

                                #endregion

                                foreach (var item in changeDTOs)
                                {
                                    var newChangeRow = new PayrollPeriodChangeRow()
                                    {
                                        PayrollPeriodChangeHead = newPayrollPeriodChangeHead,
                                        Field = (int)item.Field,
                                        FromValue = item.FromValue,
                                        ToValue = item.ToValue,
                                        ChangeDecimalValue = item.ChangeDecimalValue,
                                    };

                                    SetCreatedProperties(newChangeRow);
                                    entities.PayrollPeriodChangeRow.AddObject(newChangeRow);
                                }

                                #endregion
                                StopWatch("Vacation-Update");

                                #endregion
                                StopWatch("Vacation");

                                StartWatch("Atteststate", append: true);
                                #region Atteststate

                                oDTO.Result = TrySetTimePayrollTransactionsAttestState(allTimePayrollTransactions, attestStateTo, attestTransitionsToState, userValidTransitions, null, logDate, false, ref noOfValidTransactions, ref noOfInvalidTransactions, validatePayrollLockedAttestState: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                StartWatch("Atteststate-Save", append: true);
                                oDTO.Result = Save(); //Save before printing the report
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                StopWatch("Atteststate-Save");

                                if (noOfInvalidTransactions > 0)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.CannotSetAttestState, GetText(8691, "En eller flera transaktioner kunde inte låsas, attestnivå kunde inte sättas"));
                                    if (continueIfNotSucces)
                                        continue;
                                    else
                                        return oDTO;
                                }

                                #endregion
                                StopWatch("Atteststate");

                                StartWatch("Payrollslip", append: true);
                                #region PayrollSlip

                                StartWatch("Payrollslip-Delete", append: true);
                                oDTO.Result = SetPayrollSlipsToDeleted(entities, timePeriod, employeeId, saveChanges: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;
                                StopWatch("Payrollslip-Delete");

                                StartWatch("PayrollSlip-Save", append: true);
                                oDTO.Result = Save(); //Save before printing the report
                                if (!oDTO.Result.Success)
                                    return oDTO;
                                StopWatch("PayrollSlip-Save");

                                StartWatch("Payrollslip Generate", append: true);
                                int defaultReportId = 0; // ReportManager.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip, this.actorCompanyId, this.userId);
                                var jobdefinition = TimeReportDataManager.CreateReportJobDefinitionDTO(defaultReportId, SoeReportTemplateType.PayrollSlip, TermGroup_ReportExportType.Pdf, new List<int>() { timePeriodId }, new List<int>() { employeeId });
                                var reportResult = TimeReportDataManager.InitReportResult(entities, jobdefinition, this.actorCompanyId, this.userId, iDTO.RoleId);
                                reportResult.Input.IsPreliminary = false;

                                XDocument xdoc = TimeReportDataManager.CreateReportXML(entities, reportResult,
                                    timeAccumulators: timeAccumulators,
                                    employee: employee,
                                    currentTimePeriod: timePeriod,
                                    employeeGroups: employeeGroups,
                                    payrollGroups: payrollGroups,
                                    priceTypes: payrollPriceTypes,
                                    disbursementMethodsDict: disbursementMethodsDict,
                                    companyProducts: companyProducts,
                                    accountDimInternals: accountDimInternals,
                                    paymentInformation: paymentInformation,
                                    payrollStartValueHeads: payrollStartValueHeads,
                                    vacationGroups: vacationGroups,
                                    payrollCalculationProductItems: payrollCalculationProductItems,
                                    currentEmployeeTimePeriod: employeeTimePeriod);

                                GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.PayrollSlipXML, xdoc.ToString(), null, timePeriod.TimePeriodId, employeeId, this.actorCompanyId);
                                oDTO.Result = Save();
                                StopWatch("Payrollslip Generate");

                                #endregion
                                StopWatch("Payrollslip");

                                StartWatch("EmployeeTimePeriod", append: true);
                                #region EmployeeTimePeriod

                                employeeTimePeriod.Status = (int)SoeEmployeeTimePeriodStatus.Locked;
                                SetModifiedProperties(employeeTimePeriod);

                                #endregion
                                StartWatch("EmployeeTimePeriod", append: true);

                                StartWatch("RetroactiveEmployee", append: true);
                                #region RetroactiveEmployee

                                oDTO.Result = SetRetroactivePayrollEmployeeLocked(employee.EmployeeId, timePeriod.TimePeriodId);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion
                                StopWatch("RetroactiveEmployee");

                                StartWatch("Save", append: true);
                                #region Save

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion
                                StopWatch("Save");

                                if (TryCommit(oDTO))
                                {
                                    if (employee.UserId.HasValue)//NOSONAR
                                        sendPayrollSlipPublishedUserIds.Add(employee.UserId.Value);
                                }
                            }
                        }

                        #endregion
                        StopWatch("Period");
                    }

                    StopWatch("Method");
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result.IntegerValue = noOfValidTransactions;
                        oDTO.Result.IntegerValue2 = noOfInvalidTransactions;
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            if (oDTO.Result.Success && !sendPayrollSlipPublishedUserIds.IsNullOrEmpty())
                CommunicationManager.SendXEMailPayrollSlipPublishedWhenLockingPeriod(actorCompanyId, sendPayrollSlipPublishedUserIds, userId, RoleId);

            oDTO.Result.Strings = base.GetWatchLogs();
            return oDTO;
        }

        /// <summary>
        /// UnLocks a Payrollperiod
        /// </summary>
        /// <returns>Output DTO</returns>
        private LockUnlockPayrollPeriodOutputDTO TaskUnLockPayrollPeriod()
        {
            var (iDTO, oDTO) = InitTask<LockUnlockPayrollPeriodInputDTO, LockUnlockPayrollPeriodOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeIds.IsNullOrEmpty() || iDTO.TimePeriodIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            #region Init

            int noOfValidTransactions = 0;
            int noOfInvalidTransactions = 0;
            bool continueIfNotSucces = iDTO.EmployeeIds.Count > 1;

            #endregion

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    DateTime logDate = DateTime.Now;

                    //VactionGroups
                    List<VacationGroup> vacationGroups = GetVacationGroupsWithSEFromCache();

                    //Settings
                    int companyPayrollResultingAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus);
                    int companySalaryPaymentLockedAttestStateId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentLockedAttestStateId);

                    AttestStateDTO attestStateFrom = GetAttestState(companySalaryPaymentLockedAttestStateId);
                    if (attestStateFrom == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));
                        return oDTO;
                    }

                    AttestStateDTO attestStateTo = GetAttestState(companyPayrollResultingAttestStateId);
                    if (attestStateTo == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.EntityIsNull, GetText(10085, "Attestnivå hittades inte"));
                        return oDTO;
                    }

                    //Get AttestTransition from given AttestStates
                    AttestTransitionDTO attestTransition = GetAttestTransition(attestStateFrom.AttestStateId, attestStateTo.AttestStateId);
                    if (attestTransition == null)
                    {
                        oDTO.Result = new ActionResult(false, (int)ActionResultSave.SaveAttestNoAttestTransitions, GetText(10086, "Inga attestövergångar hittades"));
                        return oDTO;
                    }

                    List<AttestTransitionDTO> attestTransitionsToState = new List<AttestTransitionDTO> { attestTransition };

                    AttestStateDTO attestStateInitial = null;
                    if (iDTO.IgnoreResultingAttestStateId)
                        attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);

                    if (attestStateInitial != null)
                    {
                        AttestTransitionDTO attestTransitionInitial = GetAttestTransition(attestStateInitial.AttestStateId, attestStateTo.AttestStateId);
                        if (attestTransitionInitial != null)
                            attestTransitionsToState.Add(attestTransitionInitial);
                    }

                    AddEmployeesWithEmploymentToCache(iDTO.EmployeeIds);

                    #endregion

                    #region Perform

                    foreach (int timePeriodId in iDTO.TimePeriodIds)
                    {
                        #region TimePeriod

                        #region Prereq

                        TimePeriod timePeriod = GetTimePeriod(timePeriodId);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                            return oDTO;
                        }

                        //Valid AttestTransitions                        
                        List<AttestUserRoleView> userValidTransitions = GetAttestUserRoleViews(timePeriod.StartDate, timePeriod.StopDate);
                        if (userValidTransitions.IsNullOrEmpty())
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsAttestRole, GetText(10086, "Inga attestövergångar hittades"));
                            return oDTO;
                        }

                        if (!HasValidAttestTransition(false, attestTransition, null, userValidTransitions))
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.SaveAttestNoValidAttestTransitionsAttestRole, GetText(11830, "Giltig attestövergång saknas"));
                            return oDTO;
                        }

                        #endregion

                        foreach (int employeeId in iDTO.EmployeeIds)
                        {
                            #region Prereq

                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, false);
                            if (employee == null)
                                continue;

                            
                            #endregion

                            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {
                                InitTransaction(transaction);

                                #region Employee

                                #region Init

                                EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriod(timePeriod.TimePeriodId, employeeId);
                                if (employeeTimePeriod == null)
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10089, "Period hittades inte.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }

                                List<TimePayrollTransaction> allTimePayrollTransactions = GetTimePayrollTransactionsWithExtendedAndTimeBlockDate(employeeTimePeriod);
                                //Get only transactions that is locked 
                                allTimePayrollTransactions = allTimePayrollTransactions.Where(i => i.AttestStateId == companySalaryPaymentLockedAttestStateId).ToList();

                                if (!allTimePayrollTransactions.Any())
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10090, "Inga transaktioner med rätt status hittades för perioden.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }

                                if (!allTimePayrollTransactions.Any(x => x.TimeBlockDate != null))// Ensure that timeBlockDate is loaded
                                {
                                    if (continueIfNotSucces)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10091, "Perioden innehåller transaktioner utan komplett information.") + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                                        return oDTO;
                                    }
                                }

                                List<PayrollPeriodChangeRowDTO> changeDTOs = new List<PayrollPeriodChangeRowDTO>();

                                Employment employment = employee.GetEmployment(timePeriod.PayrollStopDate);


                                #endregion

                                var allChangeHeads = GetPayrollPeriodChangeHeads(employeeTimePeriod.EmployeeTimePeriodId);
                                var lastChangeHead = allChangeHeads.OrderBy(e => e.Created).ThenBy(x => x.Modified).LastOrDefault();

                                if (lastChangeHead != null && lastChangeHead.Type == (int)PayrollPeriodChangeHeadType.Lock)
                                {

                                    #region PayrollPeriodChangeHead

                                    var existingChangeHeads = allChangeHeads.Where(x => x.Type == (int)PayrollPeriodChangeHeadType.UnLock);
                                    foreach (var existingChangeHead in existingChangeHeads)//There should always be only one active, but to be sure.....
                                    {
                                        SetModifiedProperties(existingChangeHead);
                                        existingChangeHead.State = (int)SoeEntityState.Deleted;
                                    }

                                    PayrollPeriodChangeHead newPayrollPeriodChangeHead = new PayrollPeriodChangeHead()
                                    {
                                        EmployeeTimePeriodId = employeeTimePeriod.EmployeeTimePeriodId,
                                        ActorCompanyId = this.actorCompanyId,
                                        Type = (int)PayrollPeriodChangeHeadType.UnLock,
                                        State = (int)SoeEntityState.Active
                                    };
                                    SetCreatedProperties(newPayrollPeriodChangeHead);
                                    entities.PayrollPeriodChangeHead.AddObject(newPayrollPeriodChangeHead);

                                    #endregion

                                    if (lastChangeHead != null)
                                    {
                                        #region Create new EmployeeVacationSE

                                        List<EmployeeVacationSE> vacations = EmployeeManager.GetEmployeeVacationSEs(entities, employeeId);
                                        EmployeeVacationSE currentEmployeeVacationSe = vacations.OrderBy(x => x.Created).Last(); //There should always be only one active EmployeeVacationSE
                                        foreach (EmployeeVacationSE vacation in vacations)
                                        {
                                            SetModifiedProperties(vacation);
                                            vacation.State = (int)SoeEntityState.Deleted;
                                        }

                                        #region Create new copy from current

                                        EmployeeVacationSE newVacation = new EmployeeVacationSE()
                                        {
                                            EmployeeId = employeeId,
                                            PrevEmployeeVacationSEId = currentEmployeeVacationSe.EmployeeVacationSEId,
                                            EarnedDaysPaid = currentEmployeeVacationSe?.EarnedDaysPaid,
                                            EarnedDaysUnpaid = currentEmployeeVacationSe?.EarnedDaysUnpaid,
                                            EarnedDaysAdvance = currentEmployeeVacationSe?.EarnedDaysAdvance,
                                            SavedDaysYear1 = currentEmployeeVacationSe?.SavedDaysYear1,
                                            SavedDaysYear2 = currentEmployeeVacationSe?.SavedDaysYear2,
                                            SavedDaysYear3 = currentEmployeeVacationSe?.SavedDaysYear3,
                                            SavedDaysYear4 = currentEmployeeVacationSe?.SavedDaysYear4,
                                            SavedDaysYear5 = currentEmployeeVacationSe?.SavedDaysYear5,
                                            SavedDaysOverdue = currentEmployeeVacationSe?.SavedDaysOverdue,

                                            UsedDaysPaid = currentEmployeeVacationSe?.UsedDaysPaid,
                                            PaidVacationAllowance = currentEmployeeVacationSe?.PaidVacationAllowance,
                                            PaidVacationVariableAllowance = currentEmployeeVacationSe?.PaidVacationVariableAllowance,
                                            UsedDaysUnpaid = currentEmployeeVacationSe?.UsedDaysUnpaid,
                                            UsedDaysAdvance = currentEmployeeVacationSe?.UsedDaysAdvance,
                                            UsedDaysYear1 = currentEmployeeVacationSe?.UsedDaysYear1,
                                            UsedDaysYear2 = currentEmployeeVacationSe?.UsedDaysYear2,
                                            UsedDaysYear3 = currentEmployeeVacationSe?.UsedDaysYear3,
                                            UsedDaysYear4 = currentEmployeeVacationSe?.UsedDaysYear4,
                                            UsedDaysYear5 = currentEmployeeVacationSe?.UsedDaysYear5,
                                            UsedDaysOverdue = currentEmployeeVacationSe?.UsedDaysOverdue,

                                            RemainingDaysPaid = currentEmployeeVacationSe?.RemainingDaysPaid,
                                            RemainingDaysUnpaid = currentEmployeeVacationSe?.RemainingDaysUnpaid,
                                            RemainingDaysAdvance = currentEmployeeVacationSe?.RemainingDaysAdvance,
                                            RemainingDaysYear1 = currentEmployeeVacationSe?.RemainingDaysYear1,
                                            RemainingDaysYear2 = currentEmployeeVacationSe?.RemainingDaysYear2,
                                            RemainingDaysYear3 = currentEmployeeVacationSe?.RemainingDaysYear3,
                                            RemainingDaysYear4 = currentEmployeeVacationSe?.RemainingDaysYear4,
                                            RemainingDaysYear5 = currentEmployeeVacationSe?.RemainingDaysYear5,
                                            RemainingDaysOverdue = currentEmployeeVacationSe?.RemainingDaysOverdue,

                                            EarnedDaysRemainingHoursPaid = currentEmployeeVacationSe?.EarnedDaysRemainingHoursPaid,
                                            EarnedDaysRemainingHoursUnpaid = currentEmployeeVacationSe?.EarnedDaysRemainingHoursUnpaid,
                                            EarnedDaysRemainingHoursAdvance = currentEmployeeVacationSe?.EarnedDaysRemainingHoursAdvance,
                                            EarnedDaysRemainingHoursYear1 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear1,
                                            EarnedDaysRemainingHoursYear2 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear2,
                                            EarnedDaysRemainingHoursYear3 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear3,
                                            EarnedDaysRemainingHoursYear4 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear4,
                                            EarnedDaysRemainingHoursYear5 = currentEmployeeVacationSe?.EarnedDaysRemainingHoursYear5,
                                            EarnedDaysRemainingHoursOverdue = currentEmployeeVacationSe?.EarnedDaysRemainingHoursOverdue,

                                            EmploymentRatePaid = currentEmployeeVacationSe?.EmploymentRatePaid,
                                            EmploymentRateYear1 = currentEmployeeVacationSe?.EmploymentRateYear1,
                                            EmploymentRateYear2 = currentEmployeeVacationSe?.EmploymentRateYear2,
                                            EmploymentRateYear3 = currentEmployeeVacationSe?.EmploymentRateYear3,
                                            EmploymentRateYear4 = currentEmployeeVacationSe?.EmploymentRateYear4,
                                            EmploymentRateYear5 = currentEmployeeVacationSe?.EmploymentRateYear5,
                                            EmploymentRateOverdue = currentEmployeeVacationSe?.EmploymentRateOverdue,

                                            DebtInAdvanceAmount = currentEmployeeVacationSe?.DebtInAdvanceAmount,
                                            DebtInAdvanceDueDate = currentEmployeeVacationSe?.DebtInAdvanceDueDate,
                                            DebtInAdvanceDelete = currentEmployeeVacationSe?.DebtInAdvanceDelete ?? false,
                                        };

                                        entities.EmployeeVacationSE.AddObject(newVacation);

                                        if (vacations.Count == 0)
                                            SetCreatedProperties(newVacation);
                                        else
                                        {
                                            newVacation.Created = vacations.First().Created;
                                            newVacation.CreatedBy = vacations.First().CreatedBy;
                                            SetModifiedProperties(newVacation);
                                        }

                                        #endregion

                                        #region Update values

                                        #region Update used columns

                                        decimal currentUsedDaysPaid = (newVacation.UsedDaysPaid ?? 0);
                                        var usedDaysPaidChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysPaid);
                                        decimal periodUsedDaysPaidCount = (usedDaysPaidChangeRow != null ? usedDaysPaidChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysPaid = currentUsedDaysPaid - periodUsedDaysPaidCount;
                                        newVacation.UsedDaysPaid = newUsedDaysPaid;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysPaid, currentUsedDaysPaid.ToString(), newUsedDaysPaid.ToString(), (decimal.Negate(periodUsedDaysPaidCount))));

                                        decimal currentPaidVacationAllowance = (newVacation.PaidVacationAllowance ?? 0);
                                        var paidVacationAllowanceChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.PaidVacationAllowance);
                                        decimal periodPaidVacationAllowance = (paidVacationAllowanceChangeRow != null ? paidVacationAllowanceChangeRow.ChangeDecimalValue : 0);
                                        decimal newPaidVacationAllowance = currentPaidVacationAllowance - periodPaidVacationAllowance;
                                        newVacation.PaidVacationAllowance = newPaidVacationAllowance;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.PaidVacationAllowance, currentPaidVacationAllowance.ToString(), newPaidVacationAllowance.ToString(), (decimal.Negate(periodPaidVacationAllowance))));

                                        decimal currentPaidVacationVariableAllowance = (newVacation.PaidVacationVariableAllowance ?? 0);
                                        var paidVacationVariableAllowanceChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.PaidVacationVariableAllowance);
                                        decimal periodPaidVacationVariableAllowance = (paidVacationVariableAllowanceChangeRow != null ? paidVacationVariableAllowanceChangeRow.ChangeDecimalValue : 0);
                                        decimal newPaidVacationVariableAllowance = currentPaidVacationVariableAllowance - periodPaidVacationVariableAllowance;
                                        newVacation.PaidVacationVariableAllowance = newPaidVacationVariableAllowance;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.PaidVacationVariableAllowance, currentPaidVacationVariableAllowance.ToString(), newPaidVacationVariableAllowance.ToString(), (decimal.Negate(periodPaidVacationVariableAllowance))));

                                        decimal currentUsedDaysUnpaid = (newVacation.UsedDaysUnpaid ?? 0);
                                        var usedDaysUnpaidChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysUnpaid);
                                        decimal periodUsedDaysUnpaidCount = (usedDaysUnpaidChangeRow != null ? usedDaysUnpaidChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysUnpaid = currentUsedDaysUnpaid - periodUsedDaysUnpaidCount;
                                        newVacation.UsedDaysUnpaid = newUsedDaysUnpaid;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysUnpaid, currentUsedDaysUnpaid.ToString(), newUsedDaysUnpaid.ToString(), decimal.Negate(periodUsedDaysUnpaidCount)));

                                        decimal currentUsedDaysAdvance = (newVacation.UsedDaysAdvance ?? 0);
                                        var usedDaysAdvanceChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysAdvance);
                                        decimal periodUsedDaysAdvanceCount = (usedDaysAdvanceChangeRow != null ? usedDaysAdvanceChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysAdvance = currentUsedDaysAdvance - periodUsedDaysAdvanceCount;
                                        newVacation.UsedDaysAdvance = newUsedDaysAdvance;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysAdvance, currentUsedDaysAdvance.ToString(), newUsedDaysAdvance.ToString(), decimal.Negate(periodUsedDaysAdvanceCount)));

                                        decimal currentUsedDaysYear1 = (newVacation.UsedDaysYear1 ?? 0);
                                        var usedDaysYear1CountChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysYear1);
                                        decimal periodUsedDaysYear1Count = (usedDaysYear1CountChangeRow != null ? usedDaysYear1CountChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysYear1 = currentUsedDaysYear1 - periodUsedDaysYear1Count;
                                        newVacation.UsedDaysYear1 = newUsedDaysYear1;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear1, currentUsedDaysYear1.ToString(), newUsedDaysYear1.ToString(), decimal.Negate(periodUsedDaysYear1Count)));

                                        decimal currentUsedDaysYear2 = (newVacation.UsedDaysYear2 ?? 0);
                                        var usedDaysYear2CountChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysYear2);
                                        decimal periodUsedDaysYear2Count = (usedDaysYear2CountChangeRow != null ? usedDaysYear2CountChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysYear2 = currentUsedDaysYear2 - periodUsedDaysYear2Count;
                                        newVacation.UsedDaysYear2 = newUsedDaysYear2;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear2, currentUsedDaysYear2.ToString(), newUsedDaysYear2.ToString(), decimal.Negate(periodUsedDaysYear2Count)));

                                        decimal currentUsedDaysYear3 = (newVacation.UsedDaysYear3 ?? 0);
                                        var usedDaysYear3CountChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysYear3);
                                        decimal periodUsedDaysYear3Count = (usedDaysYear3CountChangeRow != null ? usedDaysYear3CountChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysYear3 = currentUsedDaysYear3 - periodUsedDaysYear3Count;
                                        newVacation.UsedDaysYear3 = newUsedDaysYear3;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear3, currentUsedDaysYear3.ToString(), newUsedDaysYear3.ToString(), decimal.Negate(periodUsedDaysYear3Count)));

                                        decimal currentUsedDaysYear4 = (newVacation.UsedDaysYear4 ?? 0);
                                        var usedDaysYear4CountChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysYear4);
                                        decimal periodUsedDaysYear4Count = (usedDaysYear4CountChangeRow != null ? usedDaysYear4CountChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysYear4 = currentUsedDaysYear4 - periodUsedDaysYear4Count;
                                        newVacation.UsedDaysYear4 = newUsedDaysYear4;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear4, currentUsedDaysYear4.ToString(), newUsedDaysYear4.ToString(), decimal.Negate(periodUsedDaysYear4Count)));

                                        decimal currentUsedDaysYear5 = (newVacation.UsedDaysYear5 ?? 0);
                                        var usedDaysYear5CountChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysYear5);
                                        decimal periodUsedDaysYear5Count = (usedDaysYear5CountChangeRow != null ? usedDaysYear5CountChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysYear5 = currentUsedDaysYear5 - periodUsedDaysYear5Count;
                                        newVacation.UsedDaysYear5 = newUsedDaysYear5;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysYear5, currentUsedDaysYear5.ToString(), newUsedDaysYear5.ToString(), decimal.Negate(periodUsedDaysYear5Count)));

                                        decimal currentUsedDaysOverdue = (newVacation.UsedDaysOverdue ?? 0);
                                        var usedDaysOverdueChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.UsedDaysOverdue);
                                        decimal periodUsedDaysOverdueCount = (lastChangeHead != null ? usedDaysOverdueChangeRow.ChangeDecimalValue : 0);
                                        decimal newUsedDaysOverdue = currentUsedDaysOverdue - periodUsedDaysOverdueCount;
                                        newVacation.UsedDaysOverdue = newUsedDaysOverdue;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.UsedDaysOverdue, currentUsedDaysOverdue.ToString(), newUsedDaysOverdue.ToString(), decimal.Negate(periodUsedDaysOverdueCount)));

                                        decimal currentAdvanceAmount = (newVacation.DebtInAdvanceAmount ?? 0);
                                        var advanceAmountChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.DebtInAdvanceAmount);
                                        decimal periodAdvanceAmount = (advanceAmountChangeRow != null ? advanceAmountChangeRow.ChangeDecimalValue : 0);
                                        decimal newAdvanceAmount = currentAdvanceAmount - periodAdvanceAmount;
                                        newVacation.DebtInAdvanceAmount = newAdvanceAmount;
                                        changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.DebtInAdvanceAmount, currentAdvanceAmount.ToString(), newAdvanceAmount.ToString(), decimal.Negate(periodAdvanceAmount)));

                                        decimal currentRemainingDaysAllowanceYear1 = (newVacation.RemainingDaysAllowanceYear1 ?? 0);
                                        var remainingDaysAllowanceYear1ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysAllowanceYear1);
                                        decimal periodRemainingDaysAllowanceYear1 = (remainingDaysAllowanceYear1ChangeRow != null ? remainingDaysAllowanceYear1ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysAllowanceYear1 = currentRemainingDaysAllowanceYear1 - periodRemainingDaysAllowanceYear1;
                                        newVacation.RemainingDaysAllowanceYear1 = newRemainingDaysAllowanceYear1;
                                        if (currentRemainingDaysAllowanceYear1 != newRemainingDaysAllowanceYear1)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear1, currentRemainingDaysAllowanceYear1.ToString(), newRemainingDaysAllowanceYear1.ToString(), decimal.Negate(periodRemainingDaysAllowanceYear1)));

                                        decimal currentRemainingDaysAllowanceYear2 = (newVacation.RemainingDaysAllowanceYear2 ?? 0);
                                        var remainingDaysAllowanceYear2ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysAllowanceYear2);
                                        decimal periodRemainingDaysAllowanceYear2 = (remainingDaysAllowanceYear2ChangeRow != null ? remainingDaysAllowanceYear2ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysAllowanceYear2 = currentRemainingDaysAllowanceYear2 - periodRemainingDaysAllowanceYear2;
                                        newVacation.RemainingDaysAllowanceYear2 = newRemainingDaysAllowanceYear2;
                                        if (currentRemainingDaysAllowanceYear2 != newRemainingDaysAllowanceYear2)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear2, currentRemainingDaysAllowanceYear2.ToString(), newRemainingDaysAllowanceYear2.ToString(), decimal.Negate(periodRemainingDaysAllowanceYear2)));

                                        decimal currentRemainingDaysAllowanceYear3 = (newVacation.RemainingDaysAllowanceYear3 ?? 0);
                                        var remainingDaysAllowanceYear3ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysAllowanceYear3);
                                        decimal periodRemainingDaysAllowanceYear3 = (remainingDaysAllowanceYear3ChangeRow != null ? remainingDaysAllowanceYear3ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysAllowanceYear3 = currentRemainingDaysAllowanceYear3 - periodRemainingDaysAllowanceYear3;
                                        newVacation.RemainingDaysAllowanceYear3 = newRemainingDaysAllowanceYear3;
                                        if (currentRemainingDaysAllowanceYear3 != newRemainingDaysAllowanceYear3)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear3, currentRemainingDaysAllowanceYear3.ToString(), newRemainingDaysAllowanceYear3.ToString(), decimal.Negate(periodRemainingDaysAllowanceYear3)));

                                        decimal currentRemainingDaysAllowanceYear4 = (newVacation.RemainingDaysAllowanceYear4 ?? 0);
                                        var remainingDaysAllowanceYear4ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysAllowanceYear4);
                                        decimal periodRemainingDaysAllowanceYear4 = (remainingDaysAllowanceYear4ChangeRow != null ? remainingDaysAllowanceYear4ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysAllowanceYear4 = currentRemainingDaysAllowanceYear4 - periodRemainingDaysAllowanceYear4;
                                        newVacation.RemainingDaysAllowanceYear4 = newRemainingDaysAllowanceYear4;
                                        if (currentRemainingDaysAllowanceYear4 != newRemainingDaysAllowanceYear4)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear4, currentRemainingDaysAllowanceYear4.ToString(), newRemainingDaysAllowanceYear4.ToString(), decimal.Negate(periodRemainingDaysAllowanceYear4)));

                                        decimal currentRemainingDaysAllowanceYear5 = (newVacation.RemainingDaysAllowanceYear5 ?? 0);
                                        var remainingDaysAllowanceYear5ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysAllowanceYear5);
                                        decimal periodRemainingDaysAllowanceYear5 = (remainingDaysAllowanceYear5ChangeRow != null ? remainingDaysAllowanceYear5ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysAllowanceYear5 = currentRemainingDaysAllowanceYear5 - periodRemainingDaysAllowanceYear5;
                                        newVacation.RemainingDaysAllowanceYear5 = newRemainingDaysAllowanceYear5;
                                        if (currentRemainingDaysAllowanceYear5 != newRemainingDaysAllowanceYear5)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYear5, currentRemainingDaysAllowanceYear5.ToString(), newRemainingDaysAllowanceYear5.ToString(), decimal.Negate(periodRemainingDaysAllowanceYear5)));

                                        decimal currentRemainingDaysAllowanceYearOverdue = (newVacation.RemainingDaysAllowanceYearOverdue ?? 0);
                                        var remainingDaysAllowanceYearOverdueChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysAllowanceYearOverdue);
                                        decimal periodRemainingDaysAllowanceYearOverdue = (remainingDaysAllowanceYearOverdueChangeRow != null ? remainingDaysAllowanceYearOverdueChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysAllowanceYearOverdue = currentRemainingDaysAllowanceYearOverdue - periodRemainingDaysAllowanceYearOverdue;
                                        newVacation.RemainingDaysAllowanceYearOverdue = newRemainingDaysAllowanceYearOverdue;
                                        if (currentRemainingDaysAllowanceYearOverdue != newRemainingDaysAllowanceYearOverdue)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysAllowanceYearOverdue, currentRemainingDaysAllowanceYearOverdue.ToString(), newRemainingDaysAllowanceYearOverdue.ToString(), decimal.Negate(periodRemainingDaysAllowanceYearOverdue)));

                                        decimal currentRemainingDaysVariableAllowanceYear1 = (newVacation.RemainingDaysVariableAllowanceYear1 ?? 0);
                                        var remainingDaysVariableAllowanceYear1ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear1);
                                        decimal periodRemainingDaysVariableAllowanceYear1 = (remainingDaysVariableAllowanceYear1ChangeRow != null ? remainingDaysVariableAllowanceYear1ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysVariableAllowanceYear1 = currentRemainingDaysVariableAllowanceYear1 - periodRemainingDaysVariableAllowanceYear1;
                                        newVacation.RemainingDaysVariableAllowanceYear1 = newRemainingDaysVariableAllowanceYear1;
                                        if (currentRemainingDaysVariableAllowanceYear1 != newRemainingDaysVariableAllowanceYear1)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear1, currentRemainingDaysVariableAllowanceYear1.ToString(), newRemainingDaysVariableAllowanceYear1.ToString(), decimal.Negate(periodRemainingDaysVariableAllowanceYear1)));

                                        decimal currentRemainingDaysVariableAllowanceYear2 = (newVacation.RemainingDaysVariableAllowanceYear2 ?? 0);
                                        var remainingDaysVariableAllowanceYear2ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear2);
                                        decimal periodRemainingDaysVariableAllowanceYear2 = (remainingDaysVariableAllowanceYear2ChangeRow != null ? remainingDaysVariableAllowanceYear2ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysVariableAllowanceYear2 = currentRemainingDaysVariableAllowanceYear2 - periodRemainingDaysVariableAllowanceYear2;
                                        newVacation.RemainingDaysVariableAllowanceYear2 = newRemainingDaysVariableAllowanceYear2;
                                        if (currentRemainingDaysVariableAllowanceYear2 != newRemainingDaysVariableAllowanceYear2)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear2, currentRemainingDaysVariableAllowanceYear2.ToString(), newRemainingDaysVariableAllowanceYear2.ToString(), decimal.Negate(periodRemainingDaysVariableAllowanceYear2)));

                                        decimal currentRemainingDaysVariableAllowanceYear3 = (newVacation.RemainingDaysVariableAllowanceYear3 ?? 0);
                                        var remainingDaysVariableAllowanceYear3ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear3);
                                        decimal periodRemainingDaysVariableAllowanceYear3 = (remainingDaysVariableAllowanceYear3ChangeRow != null ? remainingDaysVariableAllowanceYear3ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysVariableAllowanceYear3 = currentRemainingDaysVariableAllowanceYear3 - periodRemainingDaysVariableAllowanceYear3;
                                        newVacation.RemainingDaysVariableAllowanceYear3 = newRemainingDaysVariableAllowanceYear3;
                                        if (currentRemainingDaysVariableAllowanceYear3 != newRemainingDaysVariableAllowanceYear3)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear3, currentRemainingDaysVariableAllowanceYear3.ToString(), newRemainingDaysVariableAllowanceYear3.ToString(), decimal.Negate(periodRemainingDaysVariableAllowanceYear3)));

                                        decimal currentRemainingDaysVariableAllowanceYear4 = (newVacation.RemainingDaysVariableAllowanceYear4 ?? 0);
                                        var remainingDaysVariableAllowanceYear4ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear4);
                                        decimal periodRemainingDaysVariableAllowanceYear4 = (remainingDaysVariableAllowanceYear4ChangeRow != null ? remainingDaysVariableAllowanceYear4ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysVariableAllowanceYear4 = currentRemainingDaysVariableAllowanceYear4 - periodRemainingDaysVariableAllowanceYear4;
                                        newVacation.RemainingDaysVariableAllowanceYear4 = newRemainingDaysVariableAllowanceYear4;
                                        if (currentRemainingDaysVariableAllowanceYear4 != newRemainingDaysVariableAllowanceYear4)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear4, currentRemainingDaysVariableAllowanceYear4.ToString(), newRemainingDaysVariableAllowanceYear4.ToString(), decimal.Negate(periodRemainingDaysVariableAllowanceYear4)));

                                        decimal currentRemainingDaysVariableAllowanceYear5 = (newVacation.RemainingDaysVariableAllowanceYear5 ?? 0);
                                        var remainingDaysVariableAllowanceYear5ChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear5);
                                        decimal periodRemainingDaysVariableAllowanceYear5 = (remainingDaysVariableAllowanceYear5ChangeRow != null ? remainingDaysVariableAllowanceYear5ChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysVariableAllowanceYear5 = currentRemainingDaysVariableAllowanceYear5 - periodRemainingDaysVariableAllowanceYear5;
                                        newVacation.RemainingDaysVariableAllowanceYear5 = newRemainingDaysVariableAllowanceYear5;
                                        if (currentRemainingDaysVariableAllowanceYear5 != newRemainingDaysVariableAllowanceYear5)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYear5, currentRemainingDaysVariableAllowanceYear5.ToString(), newRemainingDaysVariableAllowanceYear5.ToString(), decimal.Negate(periodRemainingDaysVariableAllowanceYear5)));

                                        decimal currentRemainingDaysVariableAllowanceYearOverdue = (newVacation.RemainingDaysVariableAllowanceYearOverdue ?? 0);
                                        var remainingDaysVariableAllowanceYearOverdueChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYearOverdue);
                                        decimal periodRemainingDaysVariableAllowanceYearOverdue = (remainingDaysVariableAllowanceYearOverdueChangeRow != null ? remainingDaysVariableAllowanceYearOverdueChangeRow.ChangeDecimalValue : 0);
                                        decimal newRemainingDaysVariableAllowanceYearOverdue = currentRemainingDaysVariableAllowanceYearOverdue - periodRemainingDaysVariableAllowanceYearOverdue;
                                        newVacation.RemainingDaysVariableAllowanceYearOverdue = newRemainingDaysVariableAllowanceYearOverdue;
                                        if (currentRemainingDaysVariableAllowanceYearOverdue != newRemainingDaysVariableAllowanceYearOverdue)
                                            changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysVariableAllowanceYearOverdue, currentRemainingDaysVariableAllowanceYearOverdue.ToString(), newRemainingDaysVariableAllowanceYearOverdue.ToString(), decimal.Negate(periodRemainingDaysVariableAllowanceYearOverdue)));

                                        decimal currentEarnedDaysPaid = (newVacation.EarnedDaysPaid ?? 0);
                                        var earnedDaysPaidChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.EarnedDaysPaid);
                                        if (earnedDaysPaidChangeRow != null)
                                        {
                                            decimal periodEarnedDaysPaidCount = earnedDaysPaidChangeRow.ChangeDecimalValue;
                                            decimal newEarnedDaysPaid = currentEarnedDaysPaid - periodEarnedDaysPaidCount;
                                            if (newVacation.EarnedDaysPaid != newEarnedDaysPaid)
                                            {
                                                newVacation.EarnedDaysPaid = newEarnedDaysPaid;
                                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.EarnedDaysPaid, currentEarnedDaysPaid.ToString(), newEarnedDaysPaid.ToString(), (decimal.Negate(periodEarnedDaysPaidCount))));
                                            }
                                        }

                                        decimal currentRemainingDaysPaid = (newVacation.RemainingDaysPaid ?? 0);
                                        var remainingDaysPaidChangeRow = lastChangeHead.PayrollPeriodChangeRow.FirstOrDefault(x => x.Field == (int)PayrollPeriodChangeRowField.RemainingDaysPaid);
                                        if (remainingDaysPaidChangeRow != null)
                                        {
                                            decimal periodRemainingDaysPaidCount = remainingDaysPaidChangeRow.ChangeDecimalValue;
                                            decimal newRemainingDaysPaid = currentRemainingDaysPaid - periodRemainingDaysPaidCount;
                                            if (newVacation.RemainingDaysPaid != newRemainingDaysPaid)
                                            {
                                                newVacation.RemainingDaysPaid = newRemainingDaysPaid;
                                                changeDTOs.Add(new PayrollPeriodChangeRowDTO(PayrollPeriodChangeRowField.RemainingDaysPaid, currentRemainingDaysPaid.ToString(), newRemainingDaysPaid.ToString(), (decimal.Negate(periodRemainingDaysPaidCount))));
                                            }
                                        }

                                        #endregion

                                        #region Update remaining columns

                                        newVacation.RemainingDaysPaid = (newVacation.EarnedDaysPaid ?? 0) - newVacation.UsedDaysPaid;
                                        newVacation.RemainingDaysUnpaid = (newVacation.EarnedDaysUnpaid ?? 0) - newVacation.UsedDaysUnpaid;
                                        newVacation.RemainingDaysAdvance = (newVacation.EarnedDaysAdvance ?? 0) - newVacation.UsedDaysAdvance;
                                        newVacation.RemainingDaysYear1 = (newVacation.SavedDaysYear1 ?? 0) - newVacation.UsedDaysYear1;
                                        newVacation.RemainingDaysYear2 = (newVacation.SavedDaysYear2 ?? 0) - newVacation.UsedDaysYear2;
                                        newVacation.RemainingDaysYear3 = (newVacation.SavedDaysYear3 ?? 0) - newVacation.UsedDaysYear3;
                                        newVacation.RemainingDaysYear4 = (newVacation.SavedDaysYear4 ?? 0) - newVacation.UsedDaysYear4;
                                        newVacation.RemainingDaysYear5 = (newVacation.SavedDaysYear5 ?? 0) - newVacation.UsedDaysYear5;
                                        newVacation.RemainingDaysOverdue = (newVacation.SavedDaysOverdue ?? 0) - newVacation.UsedDaysOverdue;

                                        #region Hours

                                        if (vacationGroups.Any() && employment != null && timePeriod.PayrollStopDate.HasValue)
                                        {
                                            var group = employment.GetVacationGroup(timePeriod.PayrollStopDate, vacationGroups);
                                            if (group != null)
                                            {
                                                var vacationGroupSE = group.VacationGroupSE.FirstOrDefault();
                                                if (vacationGroupSE != null && vacationGroupSE.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours)
                                                {
                                                    //VacationGroupVacationHandleRule
                                                    EmployeeFactor vacationNet = GetEmployeeFactorFromCache(employment.EmployeeId, TermGroup_EmployeeFactorType.Net, timePeriod.PayrollStopDate.Value);
                                                    decimal netFactor = vacationNet == null ? 5 : vacationNet.Factor;
                                                    var workTimeWeek = employment.GetWorkTimeWeek();
                                                    if (workTimeWeek > 0)
                                                    {
                                                        decimal dayFactor = decimal.Divide(decimal.Divide(workTimeWeek, new decimal(60)), netFactor);
                                                        newVacation.EarnedDaysRemainingHoursPaid = newVacation.RemainingDaysPaid * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursUnpaid = newVacation.EarnedDaysUnpaid * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursAdvance = newVacation.RemainingDaysAdvance * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursYear1 = newVacation.RemainingDaysYear1 * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursYear2 = newVacation.RemainingDaysYear2 * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursYear3 = newVacation.RemainingDaysYear3 * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursYear4 = newVacation.RemainingDaysYear4 * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursYear5 = newVacation.RemainingDaysYear5 * dayFactor;
                                                        newVacation.EarnedDaysRemainingHoursOverdue = newVacation.RemainingDaysOverdue * dayFactor;
                                                    }
                                                }
                                            }
                                        }

                                        #endregion

                                        #endregion

                                        foreach (var item in changeDTOs)
                                        {
                                            var newChangeRow = new PayrollPeriodChangeRow()
                                            {
                                                PayrollPeriodChangeHead = newPayrollPeriodChangeHead,
                                                Field = (int)item.Field,
                                                FromValue = item.FromValue,
                                                ToValue = item.ToValue,
                                                ChangeDecimalValue = item.ChangeDecimalValue,
                                            };

                                            SetCreatedProperties(newChangeRow);
                                            entities.PayrollPeriodChangeRow.AddObject(newChangeRow);
                                        }

                                        #endregion

                                        #endregion
                                    }
                                }

                                #region Clear EmployeeTimePeriodId on Transactions

                                allTimePayrollTransactions.ForEach(x => x.EmployeeTimePeriodId = null);
                                var timePayrollScheduleTransactions = GetTimePayrollScheduleTransactions(employeeTimePeriod);
                                timePayrollScheduleTransactions.ForEach(x => x.EmployeeTimePeriodId = null);

                                #endregion

                                #region Change Atteststate

                                oDTO.Result = TrySetTimePayrollTransactionsAttestState(allTimePayrollTransactions, attestStateTo, attestTransitionsToState, userValidTransitions, null, logDate, false, ref noOfValidTransactions, ref noOfInvalidTransactions, validatePayrollLockedAttestState: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                #region PayrollSlip

                                oDTO.Result = SetPayrollSlipsToDeleted(entities, timePeriod, employeeId, saveChanges: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                #region Update EmployeeTimePeriod Status

                                employeeTimePeriod.Status = (int)SoeEmployeeTimePeriodStatus.Open;
                                SetModifiedProperties(employeeTimePeriod);

                                #endregion

                                #region Update RetroactiveEmployee Status

                                oDTO.Result = SetRetroactivePayrollEmployeeUnLocked(employee.EmployeeId, timePeriod.TimePeriodId);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                #endregion

                                oDTO.Result = Save();

                                TryCommit(oDTO);
                            }
                        }

                        #endregion
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                    {
                        oDTO.Result.IntegerValue = noOfValidTransactions;
                        oDTO.Result.IntegerValue2 = noOfInvalidTransactions;
                    }
                    else
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Recalculate TimePayrollTransactions for period
        /// </summary>
        /// <returns>Output DTO</returns>
        private ClearPayrollCalculationOutputDTO TaskClearPayrollCalculation()
        {
            var (iDTO, oDTO) = InitTask<ClearPayrollCalculationInputDTO, ClearPayrollCalculationOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeId == 0)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }


            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq
                    EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriodWithValues(iDTO.TimePeriodId, iDTO.EmployeeId);
                    List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsWithAccountInternals(iDTO.EmployeeId, iDTO.TimePeriodId).GetPayrollCalculationTransactions(true);

                    if (timePayrollTransactions.IsNullOrEmpty() && employeeTimePeriod == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(10073, "Inga transaktioner hittades"));
                        return oDTO;
                    }
                    AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                    if (attestStateInitial == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));
                        return oDTO;
                    }

                    #endregion

                    #region Perform

                    oDTO.Result = ValidatePayrollLockedAttestStates(timePayrollTransactions);
                    if (!oDTO.Result.Success)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8868, "Perioden innehåller transaktioner med ogiltig status "));
                        return oDTO;
                    }

                    oDTO.Result = SetEmployeeTimePeriodToDeleted(employeeTimePeriod);
                    if (!oDTO.Result.Success)
                        return oDTO;

                    if (!timePayrollTransactions.IsNullOrEmpty())
                        oDTO.Result = SetTimePayrollTransactionsForRecalculateToDeleted(timePayrollTransactions);

                    #endregion
                }

                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private RecalculatePayrollControllOutputDTO RecalculatePayrollControll()
        {
            var (iDTO, oDTO) = InitTask<RecalculatePayrollControllInputDTO, RecalculatePayrollControllOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || !iDTO.EmployeeIds.Any())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }


            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        #region Prereq
                        Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
                        if (employee == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
                            return oDTO;
                        }

                        TimePeriod timePeriod = GetTimePeriodFromCache(iDTO.TimePeriodId);
                        if (timePeriod == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                            return oDTO;
                        }

                        EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriod(iDTO.TimePeriodId, employeeId);

                        #endregion

                        #region Perform

                        oDTO.Result = RecalculatePayrollControlFunctions(employee, timePeriod, employeeTimePeriod);

                        if (!oDTO.Result.Success)
                            break;

                        #endregion
                    }
                }

                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Output DTO</returns>
        private RecalculatePayrollPeriodOutputDTO TaskRecalculatePayrollPeriod()
        {
            var (iDTO, oDTO) = InitTask<RecalculatePayrollPeriodInputDTO, RecalculatePayrollPeriodOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeIds.IsNullOrEmpty() || iDTO.TimePeriodIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            StartWatch("Method", append: true);

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);
                entities.CommandTimeout = 300;
                StringBuilder errorMessageAllPeriods = new StringBuilder();

                try
                {
                    #region Prereq

                    StartWatch("Prereq", append: true);

                    var companyDTO = new ReCalculatePayrollPeriodCompanyDTO();
                    var result = SetUpReCalculatePayrollPeriodCompanyDTO(companyDTO, iDTO.EmployeeIds);

                    if (!result.Success)
                    {
                        oDTO.Result = result;
                        return oDTO;
                    }

                    StopWatch("Prereq");

                    #endregion

                    #region Perform

                    if (iDTO.DoLogProgressInfo)
                    {
                        //Set info in base class, from now on each watchlog will also log task to info class
                        base.info = iDTO.Info;
                    }

                    foreach (int timePeriodId in iDTO.TimePeriodIds)
                    {
                        result = ReCalculatePayrollPeriod(companyDTO, iDTO.EmployeeIds, timePeriodId, iDTO.IgnoreEmploymentHasEnded, iDTO.IncludeScheduleTransactions, iDTO.DoLogProgressInfo, out _, out string errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                            errorMessageAllPeriods.Append(errorMessage + Environment.NewLine);

                        if (!result.Success)
                        {
                            oDTO.Result = result;
                            return oDTO;
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (errorMessageAllPeriods.Length > 0)
                    {
                        oDTO.Result.Success = false;
                        StringBuilder infoMessage = new StringBuilder();
                        infoMessage.Append(GetText(8820, "Alla anställda gick inte att beräkna."));
                        infoMessage.Append("\n");
                        infoMessage.Append(errorMessageAllPeriods.ToString());
                        infoMessage.Append("\n");
                        infoMessage.Append("\n");
                        infoMessage.Append(GetText(8821, "Korrigera eventuella fel och försök igen, sortera på kolumnen 'Senast beräknad' för att se vilka anställda som inte har beräknats."));
                        infoMessage.Append("\n");
                        oDTO.Result.InfoMessage = infoMessage.ToString();
                    }

                    entities.Connection.Close();

                    if (iDTO.DoLogProgressInfo)
                    {
                        iDTO.Monitor.AddResult(info.PollingKey, oDTO.Result);
                        iDTO.Info.Abort = true;
                    }
                }
            }

            StopWatch("Method");

            oDTO.Result.Strings = base.GetWatchLogs();
            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Output DTO</returns>
        private RecalculateExportedEmploymentTaxJOBOutputDTO TaskRecalculateExportedEmploymentTaxJOB()
        {
            var (iDTO, oDTO) = InitTask<RecalculateExportedEmploymentTaxJOBInputDTO, RecalculateExportedEmploymentTaxJOBOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeIds.IsNullOrEmpty() || iDTO.TimePeriodIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Perform

                    PayrollProduct payrollProductEmploymentTaxCredit = GetPayrollProductEmploymentTaxCredit();
                    if (payrollProductEmploymentTaxCredit == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductEmploymentTax);
                        return oDTO;
                    }

                    foreach (int timePeriodId in iDTO.TimePeriodIds)
                    {
                        #region TimePeriod

                        TimePeriod timePeriod = GetTimePeriod(timePeriodId);
                        if (timePeriod == null)
                            continue;

                        foreach (int employeeId in iDTO.EmployeeIds)
                        {
                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, false);
                            if (employee == null)
                                continue;

                            EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriodWithValues(timePeriod.TimePeriodId, employeeId);
                            if (employeeTimePeriod == null)
                                continue;

                            using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                            {
                                InitTransaction(transaction);

                                #region Employee                          

                                List<AttestPayrollTransactionDTO> periodAttestPayrollTransactionDtos = new List<AttestPayrollTransactionDTO>();
                                List<PayrollCalculationProductDTO> periodDtos = TimeTreePayrollManager.GetPayrollCalculationProducts(entities, base.ActorCompanyId, timePeriodId, employeeId, showAllTransactions: true);
                                periodDtos.ForEach(x => periodAttestPayrollTransactionDtos.AddRange(x.AttestPayrollTransactions.Where(y => !y.IsScheduleTransaction).ToList()));

                                List<int> timepayrollTransactionIds = periodAttestPayrollTransactionDtos.Select(x => x.TimePayrollTransactionId).ToList();
                                List<TimePayrollTransaction> periodTimePayrollTransactions = GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(employee.EmployeeId, timepayrollTransactionIds);

                                //For now this function is only suppose to be used if the period is exported. We dont want to use this function unless there is something wrong with the employmenttax and the bankfile is created.
                                if (!periodTimePayrollTransactions.Any(x => x.TimeSalaryPaymentExportEmployeeId.HasValue))
                                    continue;

                                TimePayrollTransaction periodTimePayrollTransaction = periodTimePayrollTransactions.FirstOrDefault();
                                if (periodTimePayrollTransaction == null)
                                    continue;

                                int attestStateId = periodTimePayrollTransaction.AttestStateId;
                                int timeSalaryPaymentExportEmployeeId = periodTimePayrollTransaction.TimeSalaryPaymentExportEmployeeId.Value;

                                TimeBlockDate employmentTaxTimeBlockDate = null;
                                TimePayrollTransaction periodEmploymentTaxCreditTransaction = periodTimePayrollTransactions.FirstOrDefault(x => x.IsEmploymentTaxCredit());
                                if (periodEmploymentTaxCreditTransaction != null)
                                    employmentTaxTimeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, periodEmploymentTaxCreditTransaction.TimeBlockDateId);
                                if (employmentTaxTimeBlockDate == null)
                                    employmentTaxTimeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, timePeriod.StopDate, true);
                                if (employmentTaxTimeBlockDate == null)
                                    continue;

                                List<TimePayrollTransaction> deletedEmploymentTaxTransactions = periodTimePayrollTransactions.Where(x => x.IsEmploymentTax()).ToList();
                                foreach (var timepayrolltransaction in deletedEmploymentTaxTransactions)
                                {
                                    timepayrolltransaction.State = (int)SoeEntityState.Deleted;
                                    timepayrolltransaction.Modified = DateTime.Now;
                                    timepayrolltransaction.ModifiedBy = "REET_JOB";
                                }

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                List<TimeBlockDate> timeBlockDatesInPeriod = GetTimeBlockDatesForPeriod(employee.EmployeeId, timePeriod.TimePeriodId);
                                List<TimeBlockDate> timeBlockDatesInEmployment = employee.GetTimeBlockDatesInEmployment(timeBlockDatesInPeriod);
                                List<TimeBlockDate> transactionDates = GetTimeBlockDates(employee.EmployeeId, periodTimePayrollTransactions.Select(x => x.TimeBlockDateId).Distinct().ToList());
                                List<TimePayrollScheduleTransaction> existingAbsenceScheduleTransactions = GetTimePayrollScheduleTransactionsWithAccountingAndDim(employee.EmployeeId, timeBlockDatesInEmployment.OrderBy(i => i.Date).First().Date, timeBlockDatesInEmployment.OrderByDescending(i => i.Date).First().Date, timePeriod.TimePeriodId, SoeTimePayrollScheduleTransactionType.Absence);
                                List<TimePayrollTransaction> newEmploymentTaxTransactions = new List<TimePayrollTransaction>();

                                //Delete employment tax - they will be recalculated
                                oDTO.Result = SetTimePayrollScheduleTransactionsToDeleted(existingAbsenceScheduleTransactions.Where(x => x.IsEmploymentTaxDebit()).ToList(), saveChanges: true, excludeEmploymentTaxAndSupplementCharge: false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                var timeBlockDatesForPeriodScheduleTransactions = GetTimeBlockDates(employee.EmployeeId, existingAbsenceScheduleTransactions.Where(x => x.TimePeriodId.HasValue && !x.IsEmploymentTaxDebit() && !x.IsSupplementChargeDebit()).Select(x => x.TimeBlockDateId).ToList());
                                foreach (TimeBlockDate timeBlockDate in timeBlockDatesForPeriodScheduleTransactions)
                                {
                                    if (!transactionDates.Any(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId))
                                        transactionDates.Add(timeBlockDate);
                                }

                                oDTO.Result = ReCalculateEmploymentTaxDebet(employee, transactionDates, timePeriod, deletedEmploymentTaxTransactions, periodTimePayrollTransactions, existingAbsenceScheduleTransactions.Where(i => !i.IsEmploymentTaxDebit() && !i.IsSupplementChargeDebit() && i.Type == (int)SoeTimePayrollScheduleTransactionType.Absence).ToList(), false, false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                if (oDTO.Result.Value is List<TimePayrollTransaction> timePayrollTransactions)
                                    newEmploymentTaxTransactions.AddRange(timePayrollTransactions);

                                //Restore values (we know that we loose attesttransitionlogs)
                                foreach (var newEmploymentTaxTransaction in newEmploymentTaxTransactions)
                                {
                                    newEmploymentTaxTransaction.AttestStateId = attestStateId;
                                    newEmploymentTaxTransaction.TimeSalaryPaymentExportEmployeeId = timeSalaryPaymentExportEmployeeId;
                                }

                                //Employmenttax debet has just been calculated...sum it and set it as credit
                                List<PayrollCalculationProductDTO> periodDtosAfterSave = TimeTreePayrollManager.GetPayrollCalculationProducts(entities, base.ActorCompanyId, timePeriodId, employeeId, showAllTransactions: true);
                                decimal employmentTaxAmount = -1 * (periodDtosAfterSave.Where(x => x.IsEmploymentTaxDebit() && x.Amount.HasValue).Sum(x => x.Amount.Value));

                                oDTO.Result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.EmploymentTaxCredit, employmentTaxAmount, false);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                //EmploymentTax Debit
                                TimePayrollTransaction timePayrollTransactionEmploymentTaxCredit = CreateOrUpdateTimePayrollTransaction(payrollProductEmploymentTaxCredit, employmentTaxTimeBlockDate, employee, employeeTimePeriod.TimePeriodId, attestStateId, 1, employmentTaxAmount, employmentTaxAmount, 0, String.Empty);
                                if (timePayrollTransactionEmploymentTaxCredit == null)
                                {
                                    oDTO.Result = new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);
                                    return oDTO;
                                }

                                //Restore values
                                timePayrollTransactionEmploymentTaxCredit.AttestStateId = attestStateId;
                                timePayrollTransactionEmploymentTaxCredit.TimeSalaryPaymentExportEmployeeId = timeSalaryPaymentExportEmployeeId;

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                TryCommit(oDTO);
                            }
                        }

                        #endregion
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();

                }
            }

            return oDTO;
        }

        /// <summary>
        /// Sets amount/price on each TimePayrollTransaction for a given Employees day
        /// - Takes each PayrollProduct and decides if a priceformula or pricetype should be used to calculate amount
        /// - Takes the amount and transforms to corrent dimension (hour/day/month)
        /// - Sets the amount the transaction from the amount and quantity
        /// </summary>
        /// <returns>Output DTO</returns>
        private SavePayrollTransactionAmountsOutputDTO TaskSavePayrollTransactionAmounts()
        {
            var (iDTO, oDTO) = InitTask<SavePayrollTransactionAmountsInputDTO, SavePayrollTransactionAmountsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                if (iDTO.TimeBlockDateId > 0)
                    oDTO.Result = SaveTimePayrollTransactionAmounts(iDTO.EmployeeId, iDTO.TimeBlockDateId);
                else if (iDTO.Date.HasValue)
                    oDTO.Result = SaveTimePayrollTransactionAmounts(iDTO.EmployeeId, iDTO.Date.Value);
            }

            return oDTO;
        }
        /// <summary>   
        /// This method retrieves payroll/schedule transactions for a given employee and date range that
        /// can be moved between salary periods.
        /// </summary>
        private GetUnhandledPayrollTransactionsOutputDTO TaskGetUnhandledPayrollTransactions()
        {
            var (iDTO, oDTO) = InitTask<GetUnhandledPayrollTransactionsInputDTO, GetUnhandledPayrollTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    #region Prereq

                    #region Load company data

                    AttestStateDTO attestStateMinPayroll = GetAttestStateFromCache(GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollMinimumAttestStatus));
                    if (attestStateMinPayroll == null)
                        return oDTO;

                    List<AccountDimDTO> accountDims = GetAccountDimsFromCache().ToDTOs();

                    #endregion

                    #region Load employee data

                    //TimePayrollTransactions
                    var timePayrollTransactionItemsEmployee = TimeTransactionManager.GetTimePayrollTransactionItemsForEmployee(entities, iDTO.EmployeeId, iDTO.StartDate, iDTO.StopDate);
                    timePayrollTransactionItemsEmployee = timePayrollTransactionItemsEmployee.Where(i => i.PayrollProductUseInPayroll && !i.IsFixed).ToList();
                    if (timePayrollTransactionItemsEmployee.Any(i => i.VacationYearEndRowId.HasValue))
                    {
                        List<VacationYearEndRow> vacationYearEndRows = GetVacationYearEndRows(iDTO.EmployeeId, SoeVacationYearEndType.FinalSalary);
                        if (!vacationYearEndRows.IsNullOrEmpty())
                        {
                            List<int> vacationYearEndRowIds = vacationYearEndRows.Select(i => i.VacationYearEndRowId).ToList();
                            timePayrollTransactionItemsEmployee = timePayrollTransactionItemsEmployee.Where(i => !i.VacationYearEndRowId.HasValue || !vacationYearEndRowIds.Contains(i.VacationYearEndRowId.Value)).ToList();
                        }

                    }

                    var accountIds = timePayrollTransactionItemsEmployee.Select(i => i.AccountId).Distinct().ToList();
                    var timePayrollTransactionAccountItemsEmployee = TimeTransactionManager.GetTimePayrollTransactionAccountsForEmployee(entities, iDTO.StartDate, iDTO.StopDate, null, iDTO.EmployeeId);


                    //TimePayrollSceduleTransactions
                    var timePayrollScheduleTransactions = TimeTransactionManager.GetTimePayrollScheduleTransactionsForEmployeeNotLockedWithPayrollProduct(entities, iDTO.EmployeeId, iDTO.StartDate, iDTO.StopDate, SoeTimePayrollScheduleTransactionType.Absence);
                    timePayrollScheduleTransactions = timePayrollScheduleTransactions.Where(i => i.PayrollProduct.UseInPayroll).ToList();
                    accountIds.AddRange(timePayrollScheduleTransactions.Select(i => i.AccountStdId).Distinct().ToList());

                    var timePayrollScheduleTransactionAccountItemsEmployee = TimeTransactionManager.GetTimePayrollScheduleTransactionAccountsForEmployee(entities, (int)SoeTimePayrollScheduleTransactionType.Absence, iDTO.StartDate, iDTO.StopDate, null, iDTO.EmployeeId);

                    var accountStdsEmployee = AccountManager.GetAccountStds(entities, actorCompanyId, accountIds, false);

                    #endregion

                    #endregion

                    #region Perform

                    foreach (var timePayrollTransactionItem in timePayrollTransactionItemsEmployee)
                    {
                        #region Validate

                        //Only transactions with AttestStateId before Payroll should be included
                        if (attestStateMinPayroll.AttestStateId != timePayrollTransactionItem.AttestStateId)
                            continue;

                        #endregion

                        #region Accounts

                        var accountInternals = timePayrollTransactionAccountItemsEmployee.GetAccountInternals(timePayrollTransactionItem.TimePayrollTransactionId);
                        var accountStd = accountStdsEmployee.FirstOrDefault(i => i.AccountId == timePayrollTransactionItem.AccountId);

                        #endregion

                        var transactionItem = timePayrollTransactionItem.CreateTransactionItem(accountInternals, accountStd, accountDims);
                        if (transactionItem != null)
                            oDTO.TimePayrollTransactionItems.Add(transactionItem);
                    }

                    string scheduleTransactionAttestStateName = GetText(8570, "Preliminär");

                    foreach (var timePayrollScheduleTransaction in timePayrollScheduleTransactions)
                    {
                        #region Validate

                        //Extra safety even though timePayrollScheduleTransactions should already only include UNLOCKED transactions
                        if (timePayrollScheduleTransaction.EmployeeTimePeriodId.HasValue)
                            continue;

                        if (iDTO.IsBackwards)
                        {
                            //Only scheduleTransactions within a locked period should be included
                            if (timePayrollScheduleTransaction.TimeBlockDate == null || !IsEmployeeTimePeriodLockedForChanges(timePayrollScheduleTransaction.EmployeeId, date: timePayrollScheduleTransaction.TimeBlockDate.Date))
                                continue;
                        }
                        else
                        {
                            //The period is not locked yet: Only scheduleTransactions on days with attested TimePayrollTransactions should be included
                            if (!oDTO.TimePayrollTransactionItems.Any(x => !x.IsScheduleTransaction && x.Date == timePayrollScheduleTransaction.TimeBlockDate.Date))
                                continue;
                        }

                        #endregion

                        #region Accounts

                        var accountInternals = timePayrollScheduleTransactionAccountItemsEmployee.GetAccountInternals(timePayrollScheduleTransaction.TimePayrollScheduleTransactionId);
                        var accountStd = accountStdsEmployee.FirstOrDefault(i => i.AccountId == timePayrollScheduleTransaction.AccountStdId);

                        #endregion

                        var transactionItem = timePayrollScheduleTransaction.CreateTransactionItem(accountInternals, accountStd, accountDims, scheduleTransactionAttestStateName);
                        if (transactionItem != null)
                            oDTO.TimePayrollTransactionItems.Add(transactionItem);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Assign TimePayrollTransactions to TimePeriod
        /// </summary>
        /// <returns>Output DTO</returns>
        private AssignPayrollTransactionsToTimePeriodOutputDTO TaskAssignPayrollTransactionsToTimePeriod()
        {
            var (iDTO, oDTO) = InitTask<AssignPayrollTransactionsToTimePeriodInputDTO, AssignPayrollTransactionsToTimePeriodOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.TimePeriodItem == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
                return oDTO;
            }

            //Can save TimePeriod (extra) without adding transactions
            if (iDTO.TransactionItems == null)
                iDTO.TransactionItems = new List<AttestPayrollTransactionDTO>();
            if (iDTO.ScheduleTransactionItems == null)
                iDTO.ScheduleTransactionItems = new List<AttestPayrollTransactionDTO>();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        Employee employee = EmployeeManager.GetEmployeeIgnoreState(entities, base.ActorCompanyId, iDTO.EmployeeId);
                        if (employee == null)
                        {
                            oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
                            return oDTO;
                        }

                        oDTO.Result = AssignPayrollTransactionsToTimePeriod(iDTO.TransactionItems.Select(x => x.TimePayrollTransactionId).ToList(), iDTO.ScheduleTransactionItems.Select(x => x.TimePayrollTransactionId).ToList(), iDTO.TimePeriodItem, employee);
                        if (!oDTO.Result.Success)
                            return oDTO;

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ReverseTransactionsValidationOutputDTO TaskReverseTransactionsValidation()
        {
            var (iDTO, oDTO) = InitTask<ReverseTransactionsValidationInputDTO, ReverseTransactionsValidationOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Dates.IsNullOrEmpty())
            {
                oDTO = new ReverseTransactionsValidationOutputDTO(GetText(9305, "Inga datum har valts"));
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(iDTO.EmployeeId);
                    if (employee == null)
                    {
                        oDTO = new ReverseTransactionsValidationOutputDTO(GetText(5027, "Anställd hittades inte"));
                        return oDTO;
                    }

                    List<TimeBlockDate> timeBlockDates = GetTimeBlockDates(iDTO.EmployeeId, iDTO.Dates);
                    List<TimeBlock> allTimeBlocks = GetTimeBlocksWithTransactions(iDTO.EmployeeId, timeBlockDates.Select(x => x.TimeBlockDateId).ToList());
                    SetIsTransferedToSalary(allTimeBlocks);
                    List<DateTime> validDates = new List<DateTime>();
                    List<DateTime> invalidDates = new List<DateTime>();
                    foreach (var date in iDTO.Dates)
                    {
                        TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(x => x.Date == date);
                        if (timeBlockDate != null)
                        {
                            List<TimeBlock> timeBlocks = allTimeBlocks.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                            var validTimeBlocks = timeBlocks.Where(x => !x.IsBreak && (x.TimePayrollTransaction.Any() || x.TimeInvoiceTransaction.Any()));
                            if (validTimeBlocks.Any() && validTimeBlocks.All(x => x.IsTransferedToSalary))
                                validDates.Add(date);
                            else
                                invalidDates.Add(date);
                        }
                        else
                        {
                            invalidDates.Add(date);
                        }
                    }

                    ReverseTransactionsValidationDTO validationDTO = new ReverseTransactionsValidationDTO()
                    {
                        UsePayroll = UsePayroll(),
                        ValidDates = validDates,
                        Success = true,
                    };

                    if (!invalidDates.Any())
                    {
                        //All dates are valid, no dialog needs to be shown                        
                        validationDTO.ApplySilent = true;
                        validationDTO.CanContinue = true;
                    }
                    else
                    {
                        validationDTO.ApplySilent = false;
                        if (validDates.Any())
                        {
                            validationDTO.CanContinue = true;
                            validationDTO.Title = GetText(10074, "Kontrollfråga");
                            validationDTO.Message = GetText(9308, "En eller flera dagar har ogiltig status för att vändas.") + "\r\n";
                            validationDTO.Message += GetText(9309, "Följande dagar kommer att vändas: ") + "\r\n";
                            validationDTO.Message += StringUtility.GetCommaSeparatedString<string>(validDates.Select(i => i.ToShortDateString()).Distinct().ToList()) + "\r\n";
                            validationDTO.Message += GetText(8494, "Vill du fortsätta?");
                        }
                        else
                        {
                            validationDTO.CanContinue = false;
                            validationDTO.Title = GetText(9306, "Inga dagar kunde vändas");
                            validationDTO.Message = GetText(9307, "Inga dagar har giltig status för att vändas.");
                        }
                    }

                    if (validationDTO.Success && validationDTO.CanContinue)
                    {
                        EmployeeGroup employeeGroup = employee.GetEmployeeGroup(validDates.OrderByDescending(x => x).First(), GetEmployeeGroupsFromCache());
                        if (employeeGroup == null)
                        {
                            oDTO = new ReverseTransactionsValidationOutputDTO(GetText(8539, "Tidavtal hittades inte"));
                            return oDTO;
                        }

                        validationDTO.ValidPeriods = GetUpcomingOpenperiods(employee, validDates.OrderByDescending(x => x).First()).ToDTOs().ToList();
                        validationDTO.ValidCauses = GetTimeDeviationCausesByEmployeeGroup(employeeGroup.EmployeeGroupId, false).Where(x => x.Type == (int)TermGroup_TimeDeviationCauseType.Absence).ToDTOs().ToList();
                    }

                    oDTO.ValidationOutput = validationDTO;
                }
                catch (Exception ex)
                {
                    oDTO = new ReverseTransactionsValidationOutputDTO(ex.Message);
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ReverseTransactionsOutputDTO TaskReverseTransactions()
        {
            var (iDTO, oDTO) = InitTask<ReverseTransactionsAngularInputDTO, ReverseTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO.Dates.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(9305, "Inga datum har valts"));
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(iDTO.EmployeeId, false, false);
                    if (employee == null)
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
                        return oDTO;
                    }

                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        #region Perform

                        List<TimeBlockDate> timeBlockDates = GetTimeBlockDates(iDTO.EmployeeId, iDTO.Dates);
                        List<TimeBlock> allTimeBlocks = GetTimeBlocksWithTransactions(iDTO.EmployeeId, timeBlockDates.Select(x => x.TimeBlockDateId).ToList());
                        List<TimePayrollScheduleTransaction> allAbsenceScheduleTransactions = GetTimePayrollScheduleTransactions(iDTO.EmployeeId, timeBlockDates.Select(x => x.TimeBlockDateId).ToList(), SoeTimePayrollScheduleTransactionType.Absence);
                        allAbsenceScheduleTransactions = allAbsenceScheduleTransactions.Where(x => x.IsGrossSalary()).ToList();
                        List<TimePayrollTransaction> timePayrollTransactionsToMove = new List<TimePayrollTransaction>();
                        List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsToMove = new List<TimePayrollScheduleTransaction>();

                        if (iDTO.TimeDeviationCauseId.HasValue)
                            InitAbsenceDays(iDTO.EmployeeId, iDTO.Dates);

                        foreach (var timeBlockDate in timeBlockDates)
                        {
                            List<TimePayrollScheduleTransaction> absenceScheduleTransactions = allAbsenceScheduleTransactions.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                            List<TimeBlock> timeBlocks = allTimeBlocks.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                            if (timeBlocks.Any())
                            {
                                #region Reverse the transactions (those who are connected to timeblocks)

                                oDTO.Result = ReverseTransactions(timeBlocks, absenceScheduleTransactions);
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #endregion

                                #region Collect the newly created reversed transactions

                                if (oDTO.Result.Value is List<TimePayrollTransaction> timePayrollTransactions)
                                    timePayrollTransactionsToMove.AddRange(timePayrollTransactions);

                                if (oDTO.Result.Value2 is List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions)
                                    timePayrollScheduleTransactionsToMove.AddRange(timePayrollScheduleTransactions);

                                #endregion

                                oDTO.Result = Save();
                                if (!oDTO.Result.Success)
                                    return oDTO;

                                #region Apply absence

                                if (iDTO.TimeDeviationCauseId.HasValue)
                                {
                                    TimeBlockDTO dto = new TimeBlockDTO()
                                    {
                                        StartTime = timeBlockDate.Date,
                                        StopTime = CalendarUtility.GetEndOfDay(timeBlockDate.Date),
                                    };

                                    oDTO.Result = SaveWholedayDeviations(new List<TimeBlockDTO>() { dto }, iDTO.TimeDeviationCauseId.Value, iDTO.TimeDeviationCauseId.Value, "", TermGroup_TimeDeviationCauseType.Absence, iDTO.EmployeeId, iDTO.EmployeeChildId, false, false);
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                            }

                            #endregion
                        }

                        #region Move the reversed transactions och new transactions from the applied absence

                        if (this.UsePayroll() && iDTO.TimePeriodId.HasValue && iDTO.TimePeriodId.Value != 0)
                        {
                            TimePeriod timePeriod = GetTimePeriodWithHead(iDTO.TimePeriodId.Value);
                            if (timePeriod == null)
                            {
                                oDTO.Result = new ActionResult((int)ActionResultSave.NothingSaved, GetText(10088, "Period hittades inte"));
                                return oDTO;
                            }
                            allTimeBlocks = GetTimeBlocksWithTransactions(iDTO.EmployeeId, timeBlockDates.Select(x => x.TimeBlockDateId).ToList());
                            allTimeBlocks.ForEach(x => timePayrollTransactionsToMove.AddRange(x.TimePayrollTransaction.Where(y => y.State == (int)SoeEntityState.Active).ToList()));

                            allAbsenceScheduleTransactions = GetTimePayrollScheduleTransactions(iDTO.EmployeeId, timeBlockDates.Select(x => x.TimeBlockDateId).ToList(), SoeTimePayrollScheduleTransactionType.Absence);
                            allAbsenceScheduleTransactions = allAbsenceScheduleTransactions.Where(x => x.IsGrossSalary() && !x.ReversedDate.HasValue && !x.TimePeriodId.HasValue).ToList();
                            timePayrollScheduleTransactionsToMove.AddRange(allAbsenceScheduleTransactions);

                            oDTO.Result = AssignPayrollTransactionsToTimePeriod(timePayrollTransactionsToMove.Select(x => x.TimePayrollTransactionId).ToList(), timePayrollScheduleTransactionsToMove.Select(x => x.TimePayrollScheduleTransactionId).ToList(), timePeriod.ToDTO(), employee);
                            if (!oDTO.Result.Success)
                                return oDTO;
                        }

                        #endregion

                        #endregion

                        #region Notify

                        if (oDTO.Result.Success)
                            DoNotifyChangeOfDeviations();

                        #endregion

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Saves fixed payroll rows
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveFixedPayrollRowsOutputDTO TaskSaveFixedPayrollRows()
        {
            var (iDTO, oDTO) = InitTask<SaveFixedPayrollRowsInputDTO, SaveFixedPayrollRowsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeId <= 0 || iDTO.InputItems == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveFixedPayrollRows(iDTO.InputItems, iDTO.EmployeeId);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Saves Addedtransaction
        /// </summary>
        /// <param name="iDTO">Input DTO</param>
        /// <returns>Output DTO</returns>
        private SaveAddedTransactionOutputDTO TaskSaveAddedTransaction()
        {
            var (iDTO, oDTO) = InitTask<SaveAddedTransactionInputDTO, SaveAddedTransactionOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeId <= 0 || !iDTO.TimePeriodId.HasValue || iDTO.InputItem == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveAddedTransaction(iDTO.InputItem, iDTO.AccountingSettings, iDTO.AccountingSettingsAngular, iDTO.EmployeeId, iDTO.TimePeriodId.Value, ignoreEmploymentHasEnded: iDTO.IgnoreEmploymentHasEnded);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Creates added transactions from massregistration template
        /// </summary>
        /// <returns>Output DTO</returns>
        private CreateAddedTransactionsFromTemplateOutputDTO TaskCreateAddedTransactionsFromTemplate()
        {
            var (iDTO, oDTO) = InitTask<CreateAddedTransactionsFromTemplateInputDTO, CreateAddedTransactionsFromTemplateOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO?.Template == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = CreateAddedTransactionsFromTemplate(iDTO.Template, iDTO.Template.Rows, true, true, 0);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        private SavePayrollScheduleTransactionsOutputDTO TaskSavePayrollScheduleTransactions()
        {
            var (iDTO, oDTO) = InitTask<SavePayrollScheduleTransactionsInputDTO, SavePayrollScheduleTransactionsOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                try
                {
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = SaveTimePayrollScheduleTransactions(iDTO.EmployeeDates);

                        TryCommit(oDTO);
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (!oDTO.Result.Success)
                        LogTransactionFailed(this.ToString());

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        #endregion

        #region AddedTransaction

        private ActionResult SaveAddedTransaction(AttestPayrollTransactionDTO inputItem, List<AccountingSettingDTO> accountSettings, List<AccountingSettingsRowDTO> accountSettingsAngular, int employeeId, int timePeriodId, int? massregistrationTemplateRowId = null, bool ignoreEmploymentHasEnded = false)
        {
            ActionResult result;

            #region Prereq

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));
            if (!inputItem.AddedDateFrom.HasValue || !inputItem.AddedDateTo.HasValue)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8537, "Du måste ange ett datuminterval.") + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
            if (inputItem.PayrollProductId == 0)
                return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8538, "Löneart ej angiven.") + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));

            TimePeriod timePeriod = GetTimePeriodFromCache(timePeriodId);
            if (timePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8693, "Period hittades inte.") + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));

            bool applyEmploymentHasEnded = ignoreEmploymentHasEnded && (employee.GetEmployment(timePeriod.StartDate, timePeriod.StopDate) == null);

            // Validate from date
            Employment employment = employee.GetEmployment(inputItem.AddedDateFrom.Value);
            if (employment == null)
            {
                Employment nerestEmployment = employee.GetNearestEmployment(inputItem.AddedDateFrom.Value);
                if (nerestEmployment != null)
                {
                    return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8692, "Anställning på datum {0} saknas."), inputItem.AddedDateFrom.Value.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName) +
                        " (" + (nerestEmployment.DateFrom.HasValue ? nerestEmployment.DateFrom.Value.ToShortDateString() : " ") + " - " + (nerestEmployment.DateTo.HasValue ? nerestEmployment.DateTo.Value.ToShortDateString() : "-") + ")");
                }
                else
                {
                    return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8692, "Anställning på datum {0} saknas."), inputItem.AddedDateFrom.Value.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                }
            }

            employment = employee.GetEmployment(inputItem.AddedDateTo.Value);
            if (employment == null && applyEmploymentHasEnded)
                employment = employee.GetLastEmployment();
            if (employment == null)
            {
                Employment nerestEmployment = employee.GetNearestEmployment(inputItem.AddedDateTo.Value);
                if (nerestEmployment != null)
                {
                    return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8692, "Anställning på datum {0} saknas."), inputItem.AddedDateTo.Value.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName) +
                    " (" + (nerestEmployment.DateFrom.HasValue ? nerestEmployment.DateFrom.Value.ToShortDateString() : " ") + " - " + (nerestEmployment.DateTo.HasValue ? nerestEmployment.DateTo.Value.ToShortDateString() : "-") + ")");
                }
                else
                {
                    return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8692, "Anställning på datum {0} saknas."), inputItem.AddedDateTo.Value.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
                }
            }
            TimeBlockDate timeBlockDateCalculate = GetTimeBlockDateFromCache(employeeId, inputItem.AddedDateTo.Value.Date, true);
            if (timeBlockDateCalculate == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8695, "Datum {0} saknas."), inputItem.AddedDateTo.Value.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));

            DateTime? latestEmploymentDateInPeriod = employee.GetLatestEmploymentDate(timePeriod.StartDate, timePeriod.StopDate);
            if (!latestEmploymentDateInPeriod.HasValue)
            {
                if (applyEmploymentHasEnded)
                    latestEmploymentDateInPeriod = timePeriod.StopDate;
                else
                    latestEmploymentDateInPeriod = timeBlockDateCalculate.Date;
            }

            TimeBlockDate timeBlockDateTransaction = GetTimeBlockDateFromCache(employeeId, latestEmploymentDateInPeriod.Value, true);
            if (timeBlockDateTransaction == null)
                return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8695, "Datum {0} saknas."), latestEmploymentDateInPeriod.Value.ToShortDateString()) + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));

            EmployeeGroup employeeGroup = employment.GetEmployeeGroup(timeBlockDateCalculate.Date, timeBlockDateCalculate.Date, GetEmployeeGroupsFromCache(), forward: false, useLastIfCurrentNotExists: applyEmploymentHasEnded);
            if (employeeGroup == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8539, "Tidavtal hittades inte") + ". " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));

            PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(inputItem.PayrollProductId);
            if (payrollProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8694, "Löneart kunde inte hittas.") + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8334, "Lägsta attestnivå saknas"));

            // Get AccountInternal's (Dim 2-6)
            List<AccountInternal> accountInternals = GetAccountInternalsWithAccountFromCache();

            #endregion

            #region Check locked period

            var netTransaction = GetTimePayrollTransactions(employeeId, timePeriod.TimePeriodId).FirstOrDefault(x => x.IsNetSalaryPaid());
            if (netTransaction != null)
            {
                int[] payrollLockedAttestStateIds = GetPayrollLockedAttestStateIdsFromCache();
                if (netTransaction.AttestStateId.IsEqualToAny(payrollLockedAttestStateIds))
                    return new ActionResult((int)ActionResultSave.TimePeriodIsLocked, GetText(8689, "Du måste öpnna upp perioden för att kunna lägga till en transaktion.") + " " + string.Format(GetText(8688, "Anställd: {0}"), employee.EmployeeNrAndName));
            }

            #endregion

            bool isNew = false;
            bool quantityHasChanged = false;

            TimePayrollTransaction timePayrollTransaction = GetTimePayrollTransactionWithAccountInternals(inputItem.TimePayrollTransactionId);
            if (timePayrollTransaction == null)
            {
                #region Add

                isNew = true;

                timePayrollTransaction = new TimePayrollTransaction
                {
                    UnitPrice = inputItem.UnitPrice,
                    VatAmount = inputItem.VatAmount,
                    Quantity = inputItem.Quantity,
                    IsPreliminary = false,
                    Exported = false,
                    AutoAttestFailed = false,
                    ManuallyAdded = true,
                    Comment = !string.IsNullOrEmpty(inputItem.Comment) ? inputItem.Comment : "",
                    AddedDateFrom = inputItem.AddedDateFrom,
                    AddedDateTo = inputItem.AddedDateTo,
                    IsAdded = true,
                    IsSpecifiedUnitPrice = inputItem.IsSpecifiedUnitPrice,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    EmployeeId = employee.EmployeeId,
                    ProductId = inputItem.PayrollProductId,
                    AttestStateId = attestStateInitialPayroll.AttestStateId,
                    TimePeriodId = timePeriodId,
                    MassRegistrationTemplateRowId = massregistrationTemplateRowId,

                    //Set reference
                    TimeBlockDate = timeBlockDateTransaction,
                };
                SetCreatedProperties(timePayrollTransaction);

                #endregion
            }
            else
            {
                #region Update

                if (timePayrollTransaction.Quantity != inputItem.Quantity)
                    quantityHasChanged = true;
                timePayrollTransaction.ProductId = inputItem.PayrollProductId;
                timePayrollTransaction.Quantity = inputItem.Quantity;
                timePayrollTransaction.IsSpecifiedUnitPrice = inputItem.IsSpecifiedUnitPrice;
                timePayrollTransaction.UnitPrice = inputItem.UnitPrice;
                timePayrollTransaction.AddedDateFrom = inputItem.AddedDateFrom;
                timePayrollTransaction.AddedDateTo = inputItem.AddedDateTo;
                timePayrollTransaction.VatAmount = inputItem.VatAmount;
                timePayrollTransaction.Comment = !string.IsNullOrEmpty(inputItem.Comment) ? inputItem.Comment : "";
                timePayrollTransaction.TimeBlockDate = timeBlockDateTransaction;
                SetModifiedProperties(timePayrollTransaction);

                if (inputItem.UpdateChildren && inputItem.IsPayrollProductChainMainParent && quantityHasChanged)
                {
                    List<TimePayrollTransaction> addedTransactionsInPeriod = GetTimePayrollTransactionsAdded(employee.EmployeeId, timePeriod.TimePeriodId);
                    List<TimePayrollTransaction> chainedTransactions = new List<TimePayrollTransaction>();
                    addedTransactionsInPeriod.GetChain(timePayrollTransaction, chainedTransactions);

                    foreach (TimePayrollTransaction chainedTransaction in chainedTransactions)
                    {
                        chainedTransaction.Quantity = inputItem.Quantity;
                        SetModifiedProperties(chainedTransaction);
                    }
                }

                #endregion
            }

            #region Common

            SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);

            #endregion

            #region Calculate formula

            SetAddedTimeTransactionAmounts(employee, employment, timePayrollTransaction, inputItem.AddedDateTo.Value);

            #endregion

            #region Accounting

            ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, timeBlockDateCalculate.Date, payrollProduct, setAccountInternal: true);

            bool accountingIsOverridenByUser = false;
            if (accountSettings != null)
            {
                //Only silverlight
                AccountingSettingDTO accountSettingStd = accountSettings.FirstOrDefault(x => x.DimNr == Constants.ACCOUNTDIM_STANDARD);
                if (accountSettingStd != null && accountSettingStd.Account1Id != 0)
                    timePayrollTransaction.AccountStdId = accountSettingStd.Account1Id;

                timePayrollTransaction.AccountInternal?.Clear();

                foreach (AccountingSettingDTO accountSetting in accountSettings)
                {
                    AccountInternal accountInt = accountInternals.FirstOrDefault(x => x.AccountId == accountSetting.Account1Id);
                    if (accountInt != null)
                        timePayrollTransaction.AccountInternal.Add(accountInt);
                }
            }
            else if (accountSettingsAngular != null)
            {
                var accountSettingAngular = accountSettingsAngular.FirstOrDefault();
                //Only angular
                if (accountSettingAngular != null)
                {
                    var accountInternalIds = accountSettingAngular.GetAccountInternalIds();
                    if (accountSettingAngular.Account1Id != 0 || accountInternalIds.Any())
                    {
                        accountingIsOverridenByUser = true;

                        //AccountStd
                        if (accountSettingAngular.Account1Id != 0)
                            timePayrollTransaction.AccountStdId = accountSettingAngular.Account1Id;

                        timePayrollTransaction.AccountInternal?.Clear();

                        foreach (var accountInternalId in accountInternalIds)
                        {
                            AccountInternal accountInt = accountInternals.FirstOrDefault(x => x.AccountId == accountInternalId);
                            if (accountInt != null)
                                timePayrollTransaction.AccountInternal.Add(accountInt);
                        }
                    }
                }
            }

            #endregion

            #region PayrollProductChain/Fixed accouting

            if (isNew)
            {
                List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction> { timePayrollTransaction };

                //Must be done after accounting is set on parent transaction
                result = CreateTransactionsFromPayrollProductChain(timePayrollTransaction, employee, timeBlockDateCalculate, out List<TimePayrollTransaction> childTransactions);
                if (!result.Success)
                    return result;

                newTransactions.AddRange(childTransactions);

                if (!accountingIsOverridenByUser)
                {
                    result = CreateFixedAccountingTransactions(newTransactions, employee, timeBlockDateCalculate, out List<TimePayrollTransaction> fixedAccountingTransactions);
                    if (!result.Success)
                        return result;

                    newTransactions.AddRange(fixedAccountingTransactions);

                    foreach (var newTransaction in newTransactions)
                    {
                        SetAddedTimeTransactionAmounts(employee, employment, newTransaction, inputItem.AddedDateTo.Value);
                    }
                }

                result = Save();
                if (!result.Success)
                    return result;
            }

            #endregion

            result = Save();
            if (!result.Success)
                return result;

            LogEmployeeIdOnTransactionAndTimeBlockDateMissMatch(timePayrollTransaction);
            ActivateWarningPayrollPeriodHasChanged(employeeId, timePeriodId);

            return result;
        }
        private void SetAddedTimeTransactionAmounts(Employee employee, Employment employment, TimePayrollTransaction timePayrollTransaction, DateTime calculationDate)
        {
            CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);

            if (!timePayrollTransaction.IsSpecifiedUnitPrice)
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransaction.ProductId);
                if (payrollProduct == null)
                    return;

                PayrollPriceFormulaResultDTO formulaResult = EvaluatePayrollPriceFormula(calculationDate, employee, employment, payrollProduct);
                if (formulaResult != null)
                {
                    SetTimePayrollTransactionFormulas(timePayrollTransaction, formulaResult);
                    timePayrollTransaction.UnitPrice = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                    timePayrollTransaction.Amount = Decimal.Round((timePayrollTransaction.Quantity * timePayrollTransaction.UnitPrice.Value), 2, MidpointRounding.AwayFromZero);//quantity from addedtransactions is never time

                    timePayrollTransaction.State = (int)SoeEntityState.Active;
                    SetCreatedProperties(timePayrollTransaction.TimePayrollTransactionExtended);
                }
                else
                {
                    timePayrollTransaction.UnitPrice = 0;
                    timePayrollTransaction.Amount = 0;

                    RevertTimePayrollTransactionExtended(timePayrollTransaction);
                }
            }
            else
            {
                timePayrollTransaction.Amount = timePayrollTransaction.Quantity * timePayrollTransaction.UnitPrice;
            }

            SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
            SetModifiedProperties(timePayrollTransaction);
        }

        #endregion

        #region BenefitInvert

        private ActionResult CreateBenefitInvertTransactions(List<TimePayrollTransaction> timePayrollTransactionsForPeriod, TimePeriod timePeriod, TimeBlockDate lastTimeBlockDateInPeriod, Employee employee, Dictionary<TermGroup_SysPayrollType, decimal> amounts)
        {
            ActionResult result = null;

            AttestStateDTO attestState = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestState == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            decimal benefitInvertStandardAmount = 0;
            List<TimePayrollTransaction> invertedTransactions = new List<TimePayrollTransaction>();
            List<PayrollProduct> benefitInvertProducts = GetPayrollProductsBenefitInvert();

            foreach (var item in amounts)
            {
                decimal quantity = -1;
                decimal unitPrice = item.Value;
                decimal amount = quantity * unitPrice;

                if (item.Value != 0)
                {
                    PayrollProduct product = benefitInvertProducts.FirstOrDefault(x => x.SysPayrollTypeLevel3 == (int)item.Key);
                    if (product != null)
                    {
                        result = CreateTimePayrollTransaction(timePayrollTransactionsForPeriod, product, lastTimeBlockDateInPeriod, employee, quantity, amount, 0, unitPrice, String.Empty, attestState.AttestStateId, timePeriod.TimePeriodId, true, true, out List<TimePayrollTransaction> newTransactions, true, true);
                        if (!result.Success)
                            return result;

                        invertedTransactions.AddRange(newTransactions);
                    }
                    else
                        benefitInvertStandardAmount += item.Value;

                }
            }

            if (benefitInvertStandardAmount != 0)
            {
                decimal quantity = -1;
                decimal unitPrice = benefitInvertStandardAmount;
                decimal amount = quantity * unitPrice;

                PayrollProduct benefitInvertStandardProduct = benefitInvertProducts.FirstOrDefault(x => x.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Standard);
                if (benefitInvertStandardProduct != null)
                {
                    result = CreateTimePayrollTransaction(timePayrollTransactionsForPeriod, benefitInvertStandardProduct, lastTimeBlockDateInPeriod, employee, quantity, amount, 0, unitPrice, String.Empty, attestState.AttestStateId, timePeriod.TimePeriodId, true, true, out List<TimePayrollTransaction> newTransactions, true, true);
                    if (!result.Success)
                        return result;

                    invertedTransactions.AddRange(newTransactions);
                }
                else
                {
                    string msg = GetPayrollProductIsMissingMessage((int)TermGroup_SysPayrollType.SE_Benefit, (int)TermGroup_SysPayrollType.SE_Benefit_Invert, (int)TermGroup_SysPayrollType.SE_Benefit_Invert_Standard, null);
                    return new ActionResult((int)ActionResultSave.ProductNotFound, msg);
                }
            }

            result = Save();
            result.Value = invertedTransactions;

            return result;
        }

        #endregion

        #region EarnedHoliday

        private ActionResult CreateTransactionsForEarnedHoliday(List<Employee> employees, int holidayId, int year)
        {
            ActionResult result = new ActionResult(true);

            #region Prereq

            HolidayDTO holiday = GetHoliday(holidayId, year);
            if (holiday == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8740, "Helgdag kunde inte hittas"));

            int earnedHolidayTimeCodeId = GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimeCodeEarnedHoliday);
            if (earnedHolidayTimeCodeId == 0)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8741, "Företagsinställning saknas, standard tillägg för intjänade röda dagar"));

            TimeCode timeCode = GetTimeCodeWithProductsFromCache(earnedHolidayTimeCodeId);
            if (timeCode == null || timeCode.TimeCodePayrollProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8742, "Standard tillägg för intjänade röda dagar hittades inte"));

            TimeCodePayrollProduct timeCodePayrollProduct = timeCode.TimeCodePayrollProduct?.FirstOrDefault();
            PayrollProduct payrollProduct = timeCodePayrollProduct != null ? GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timeCodePayrollProduct.ProductId) : null;
            if (payrollProduct == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8744, "Löneart mappad mot tidkod hittades inte"));

            int attestStateMinPayrollId = GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollMinimumAttestStatus);
            if (attestStateMinPayrollId == 0)
                return new ActionResult(false, (int)ActionResultSelect.EntityIsNull, GetText(8159, "Lägsta status för export av löneartstransaktioner kunde inte hittas"));

            List<int> employeeIds = employees.Select(x => x.EmployeeId).ToList();
            Dictionary<int, List<TimePayrollTransaction>> existingEarnedHolidayTimePayrollTransactionsByEmployee = GetTimePayrollTransactionsForDayAndEarnedHoliday(employeeIds, holiday.Date).GroupBy(t => t.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

            #endregion

            #region Perform

            int nrOfEmployeesWithTransactionsCreated = 0;
            int nrOfTransactionsCreated = 0;
            foreach (Employee employee in employees)
            {
                int nrOfTransactionsCreatedForEmployee = 0;

                TimeBlockDate timeBlockDate = GetTimeBlockDateFromCache(employee.EmployeeId, holiday.Date, createIfNotExists: true);
                if (timeBlockDate == null)
                    continue;

                List<TimePayrollTransaction> existingEarnedHolidayTimePayrollTransactionsForEmployee = existingEarnedHolidayTimePayrollTransactionsByEmployee.GetList(employee.EmployeeId);
                if (existingEarnedHolidayTimePayrollTransactionsForEmployee.Any())
                    continue; //employee has allready transactions for given holiday               

                #region TimeCodeTransaction

                TimeCodeTransaction timeCodeTransaction = CreateTimeCodeTransaction(timeCode.TimeCodeId, TimeCodeTransactionType.Time, 1, timeBlockDate.Date, timeBlockDate.Date, 0, timeBlockDate: timeBlockDate);
                if (timeCodeTransaction == null)
                    continue;

                timeCodeTransaction.IsEarnedHoliday = true;
                timeCodeTransaction.IsAdditionOrDeduction = true;

                #endregion

                #region TimePayrollTransaction

                TimePayrollTransaction timePayrollTransaction = CreateTimePayrollTransaction(payrollProduct, timeBlockDate, 1, 0, 0, 0, "", attestStateMinPayrollId, null, employee.EmployeeId);
                if (timePayrollTransaction == null)
                    continue;

                timePayrollTransaction.TimeCodeTransaction = timeCodeTransaction;

                //Accounting
                ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, holiday.Date, payrollProduct, setAccountStd: true, setAccountInternal: false);

                #endregion

                nrOfTransactionsCreatedForEmployee++;
                nrOfEmployeesWithTransactionsCreated++;
                nrOfTransactionsCreated += nrOfTransactionsCreatedForEmployee;
            }

            if (nrOfTransactionsCreated > 0)
            {
                result = Save();
                if (!result.Success)
                    return result;
            }

            #endregion

            if (result.Success)
            {
                result.IntegerValue = nrOfEmployeesWithTransactionsCreated;
                result.IntegerValue2 = nrOfTransactionsCreated;
            }

            return result;
        }

        #endregion

        #region EmployeeVehicle

        private ActionResult CreateEmployeeVehicleTransactions(TimePeriod timePeriod, TimeBlockDate lastTimeBlockDateInPeriod, Employee employee)
        {
            ActionResult result;

            if (timePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
            if (timePeriod.ExtraPeriod)
                return new ActionResult(true);
            if (lastTimeBlockDateInPeriod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "TimeBlockDate");
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            PayrollProduct payrollProductBenefitCompanyCar = GetPayrollProductBenefitCompanyCar();
            if (payrollProductBenefitCompanyCar == null)
                return new ActionResult(true);

            PayrollProduct payrollProductDeductionCarBenefit = GetPayrollProductDeductionCarBenefit();
            if (payrollProductDeductionCarBenefit == null)
                return new ActionResult(true);

            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();
            List<EmployeeVehiclePayrollCalculationDTO> calculationDtos = EmployeeManager.CalculateEmployeeVehiclesAmounts(entities, employee.EmployeeId, actorCompanyId, timePeriod.PaymentDate.Value);
            if (calculationDtos.IsNullOrEmpty())
                return new ActionResult(true);

            foreach (var dto in calculationDtos)
            {
                #region Benefit transaction

                TimePayrollTransaction benefitTransaction = CreateTimePayrollTransaction(payrollProductBenefitCompanyCar, lastTimeBlockDateInPeriod, 1, dto.TaxableValue, 0, dto.TaxableValue, "", attestStateInitialPayroll.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId);
                if (benefitTransaction == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePayrollTransaction");

                CreateTimePayrollTransactionExtended(benefitTransaction, employee.EmployeeId, actorCompanyId);
                benefitTransaction.EmployeeVehicleId = dto.EmployeeVehicleId;

                // Accounting
                ApplyAccountingOnTimePayrollTransaction(benefitTransaction, employee, lastTimeBlockDateInPeriod.Date, payrollProductBenefitCompanyCar);

                if (benefitTransaction.Amount == 0)
                {
                    result = SetTimePayrollTransactionToDeleted(benefitTransaction, saveChanges: false);
                    if (!result.Success)
                        return result;
                }
                else
                {
                    timePayrollTransactions.Add(benefitTransaction);
                }

                #endregion

                #region Deduction

                TimePayrollTransaction deductionTransaction = CreateTimePayrollTransaction(payrollProductDeductionCarBenefit, lastTimeBlockDateInPeriod, 1, Decimal.Negate(dto.NetSalaryDeduction), 0, Decimal.Negate(dto.NetSalaryDeduction), "", attestStateInitialPayroll.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId);
                if (deductionTransaction == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePayrollTransaction");

                CreateTimePayrollTransactionExtended(deductionTransaction, employee.EmployeeId, actorCompanyId);
                deductionTransaction.EmployeeVehicleId = dto.EmployeeVehicleId;

                // Accounting
                ApplyAccountingOnTimePayrollTransaction(deductionTransaction, employee, lastTimeBlockDateInPeriod.Date, payrollProductDeductionCarBenefit);

                if (deductionTransaction.Amount == 0)
                {
                    result = SetTimePayrollTransactionToDeleted(deductionTransaction, saveChanges: false);
                    if (!result.Success)
                        return result;
                }
                else
                {
                    timePayrollTransactions.Add(deductionTransaction);
                }

                #endregion
            }

            result = Save();
            if (!result.Success)
                return result;

            result.Value = timePayrollTransactions;

            return result;
        }

        #endregion

        #region Evaluate/Calculate PayrollPriceFormulaResultDTO

        private EvaluatePayrollPriceFormulaInputDTO CreateEvaluatePriceFormulaInputDTO(List<int> employeeIds = null)
        {
            //Note: Do not use memorycache here
            return new EvaluatePayrollPriceFormulaInputDTO()
            {
                SysCountryId = GetSysCountryFromCache(),
                EmployeeGroups = GetEmployeeGroupsFromCache(),
                PayrollGroups = GetPayrollGroupsWithSettingsFromCache(),
                PayrollPriceTypes = GetPayrollPriceTypesWithPeriodsFromCache(),
                EmploymentPriceTypesDict = employeeIds.IsNullOrEmpty() ? null : EmployeeManager.GetEmploymentPriceTypesForCompany(entities, actorCompanyId, employeeIds).GroupBy(g => g.EmploymentId).ToDictionary(k => k.Key, v => v.ToList()),
                PayrollGroupPriceTypes = PayrollManager.GetPayrollGroupPriceTypesForCompany(entities, actorCompanyId),
                PayrollProductPriceTypes = ProductManager.GetPayrollProductPriceTypes(entities, actorCompanyId),
                PayrollProductPriceFormulas = ProductManager.GetPayrollProductPriceFormulas(entities, actorCompanyId),
                PayrollPriceFormulas = PayrollManager.GetPayrollPriceFormulas(entities, actorCompanyId, false),
                SysPayrollPriceViews = SysDbCache.Instance.SysPayrollPriceViewDTOs,
                TimePeriods = TimePeriodManager.GetTimePeriods(entities, TermGroup_TimePeriodType.Payroll, actorCompanyId, addTimePeriodHeadName: false),
                EmployeeFactorsDict = EmployeeManager.GetEmployeesFactorsForCompany(entities, actorCompanyId).GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList()),
                PayrollProducts = ProductManager.GetPayrollProductsWithSettings(entities, actorCompanyId, null),
            };
        }
        private PayrollPriceFormulaResultDTO EvaluatePayrollPriceFormula(DateTime date, Employee employee, Employment employment, PayrollProduct payrollProduct, decimal? inputValue = null)
        {
            return PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, payrollProduct, date, inputValue: inputValue, iDTO: GetEvaluatePriceFormulaInputDTOFromCache());
        }
        private PayrollPriceFormulaResultDTO CalculatePayrollPriceFormula(DateTime? payrollStartDateInput, DateTime? payrollStopDateInput, DateTime transactionDate, Employee employee, Employment employment, PayrollProduct payrollProduct)
        {
            if (payrollProduct == null)
                return null;

            PayrollPriceFormulaResultDTO formulaResult = new PayrollPriceFormulaResultDTO();

            if (payrollProduct.AverageCalculated)
            {
                if (payrollStartDateInput.HasValue && payrollStopDateInput.HasValue)
                {
                    DateTime payrollStartDate = payrollStartDateInput.Value;
                    DateTime payrollStopDate = payrollStopDateInput.Value;
                    decimal amount = 0;
                    while (payrollStartDate <= payrollStopDate)
                    {
                        if (!employee.HasEmployment(payrollStartDate))
                        {
                            payrollStartDate = payrollStartDate.AddDays(1);
                            continue;
                        }

                        Employment currentEmployment = employee.GetEmployment(payrollStartDate) ?? employment; //use given employmnet or? -- cant happen, see above continue?
                        if (currentEmployment != null)
                        {
                            formulaResult = EvaluatePayrollPriceFormula(payrollStartDate, employee, currentEmployment, payrollProduct);
                            amount += formulaResult.Amount;
                        }

                        payrollStartDate = payrollStartDate.AddDays(1);
                    }

                    formulaResult.Amount = amount;
                }
            }
            else
            {
                if (employment != null)
                    return EvaluatePayrollPriceFormula(transactionDate, employee, employment, payrollProduct);
            }

            return formulaResult;
        }

        #endregion

        #region FixedAccounting

        private ActionResult CreateFixedAccountingTransactions(TimeEngineTemplate template)
        {
            if (!UsePayroll())
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(template.EmployeeId, getHidden: false);
            if (employee == null)
                return new ActionResult(false);

            Employment employment = employee.GetEmployment(template.Date);
            if (employment == null || !employment.FixedAccounting)
                return new ActionResult(true);

            List<TimePayrollTransaction> newTimePayrollTransactions = new List<TimePayrollTransaction>();

            foreach (TimePayrollTransaction transaction in template.Outcome.TimePayrollTransactions)
            {
                if (transaction.IsTax())
                    continue;

                newTimePayrollTransactions.AddRange(CreateFixedAccountingTransactions(transaction, employment, employee, template.Date));
            }

            var parentTransactionsGroupedByProduct = template.Outcome.TimePayrollTransactions.Where(x => x.IncludedInPayrollProductChain && !x.ParentId.HasValue && x.TimePayrollTransactionId != 0).GroupBy(x => x.ProductId);
            foreach (var parentGroup in parentTransactionsGroupedByProduct)
            {
                TimePayrollTransaction mainParent = parentGroup.FirstOrDefault();
                List<TimePayrollTransaction> templateChain = new List<TimePayrollTransaction>();
                template.Outcome.TimePayrollTransactions.GetChain(mainParent, templateChain);
                SetPayrollProductChainParent(newTimePayrollTransactions, templateChain);
            }

            template.Outcome.TimePayrollTransactions.AddRange(newTimePayrollTransactions);

            return newTimePayrollTransactions.Any() ? Save() : new ActionResult(true);
        }
        private ActionResult CreateFixedAccountingTransactions(List<TimePayrollTransaction> timePayrollTransactions, Employee employee, TimeBlockDate timeBlockDate, out List<TimePayrollTransaction> newTransactions, Employment employment = null)
        {
            newTransactions = new List<TimePayrollTransaction>();

            if (!UsePayroll())
                return new ActionResult(true);

            if (employment == null)
                employment = employee.GetEmployment(timeBlockDate.Date);
            if (employment == null || !employment.FixedAccounting)
                return new ActionResult(true);

            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {
                if (timePayrollTransaction.IsTax())
                    continue;

                newTransactions.AddRange(CreateFixedAccountingTransactions(timePayrollTransaction, employment, employee, timeBlockDate.Date));
            }

            var parentTransactionsGroupedByProduct = timePayrollTransactions.Where(x => x.IncludedInPayrollProductChain && !x.ParentId.HasValue).GroupBy(x => x.ProductId);
            foreach (var parentGroup in parentTransactionsGroupedByProduct)
            {
                TimePayrollTransaction mainParent = parentGroup.FirstOrDefault();
                List<TimePayrollTransaction> templateChain = new List<TimePayrollTransaction>();
                timePayrollTransactions.GetChain(mainParent, templateChain);
                SetPayrollProductChainParent(newTransactions, templateChain);
            }

            return Save();
        }
        private ActionResult CreateFixedAccountingTransactions(List<TimeTransactionItem> transactionItems, int employeeId, TimeBlockDate timeBlockDate)
        {
            ActionResult result = new ActionResult();

            if (!UsePayroll())
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
            if (employee == null)
                return new ActionResult(false);

            Employment employment = employee.GetEmployment(timeBlockDate.Date);
            if (employment == null || !employment.FixedAccounting)
                return new ActionResult(true);

            var newTransactions = new List<TimeTransactionItem>();
            foreach (var transaction in transactionItems)
            {
                newTransactions.AddRange(CreateFixedAccountingTransactions(transaction, employment, employee, timeBlockDate.Date));
            }

            var parentTransactionsGroupedByProduct = transactionItems.Where(x => x.IncludedInPayrollProductChain && !x.ParentGuidId.HasValue).GroupBy(x => x.ProductId);
            foreach (var parentGroup in parentTransactionsGroupedByProduct)
            {
                var mainParent = parentGroup.FirstOrDefault();
                List<TimeTransactionItem> templateChain = new List<TimeTransactionItem>();
                transactionItems.GetChain(mainParent, templateChain);
                SetPayrollProductChainParent(newTransactions, templateChain);
            }

            transactionItems.AddRange(newTransactions);

            return result;
        }
        private List<TimePayrollTransaction> CreateFixedAccountingTransactions(TimePayrollTransaction transaction, Employment employment, Employee employee, DateTime date)
        {
            if (employment == null || !employment.FixedAccounting)
                return new List<TimePayrollTransaction>();

            PayrollProduct product = GetPayrollProductFromCache(transaction.ProductId);
            if (product != null && product.DontUseFixedAccounting)
                return new List<TimePayrollTransaction>();

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            decimal originalQuantity = transaction.Quantity;

            List<EmploymentAccountStd> accountingSettings = GetEmploymentAccountingFromCache(employment);
            foreach (EmploymentAccountType type in EnumUtility.GetEmploymentAccountTypes())
            {
                if (type == EmploymentAccountType.Cost || type == EmploymentAccountType.Income)
                    continue;

                var employmentAccountStd = accountingSettings.FirstOrDefault(x => x.Type == (int)type);
                if (employmentAccountStd == null || employmentAccountStd.Percent == 0)
                    continue;

                if (type == EmploymentAccountType.Fixed1)
                {
                    decimal percent = decimal.Divide(employmentAccountStd.Percent, 100);

                    //ReUse orginal transaction
                    transaction.Quantity = decimal.Round(originalQuantity * percent, 2, MidpointRounding.AwayFromZero);
                    if (product != null && product.IsQuantity((TermGroup_PayrollResultType)product.ResultType))
                        transaction.Amount = decimal.Round(transaction.Quantity * transaction.UnitPrice ?? 0, 2, MidpointRounding.AwayFromZero);

                    //Clear accountStd
                    transaction.AccountStdId = 0;
                    transaction.AccountStd = null;

                    if (employmentAccountStd.AccountId.HasValue)
                        transaction.AccountStdId = employmentAccountStd.AccountId.Value;
                    else
                        ApplyAccountingOnTimePayrollTransaction(transaction, employee, date, product, setAccountStd: true, setAccountInternal: false);

                    transaction.AccountInternal.Clear();
                    AddAccountInternalsToTimePayrollTransaction(transaction, employmentAccountStd.AccountInternal);
                }
                else
                {
                    var newTransaction = CreateFixedAccountingTimePayrolltransactionFromOriginal(employee, transaction, product, originalQuantity, employmentAccountStd, employment);
                    if (newTransaction != null)
                        newTransactions.Add(newTransaction);
                }
            }

            #region Fix decimals (Item 50904)

            if (product != null && !transaction.IsFixed && !transaction.IsAdditionOrDeduction && !transaction.IsAdded && !transaction.VacationYearEndRowId.HasValue && !product.IsQuantity((TermGroup_PayrollResultType)product.ResultType))
            {
                transaction.Quantity = (int)transaction.Quantity;

                foreach (var item in newTransactions)
                {
                    item.Quantity = (int)item.Quantity;
                }

                decimal newTotalQuantity = transaction.Quantity + newTransactions.Sum(x => x.Quantity);
                if (newTotalQuantity != originalQuantity)
                {
                    decimal diff = originalQuantity - newTotalQuantity;
                    //add the diff to the  orginal transaction
                    transaction.Quantity += diff;
                }
            }

            #endregion

            return newTransactions;
        }
        private ActionResult CreateDistributedTransactions(List<TimePayrollTransaction> timePayrollTransactions, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions, TimePayrollTransaction originalTransaction, Employee employee, TimeBlockDate timeBlockDate, out List<TimePayrollTransaction> newTransactions)
        {
            newTransactions = new List<TimePayrollTransaction>();

            if (!UsePayroll())
                return new ActionResult(true);

            Employment employment = employee.GetEmployment(timeBlockDate.Date);
            if (employment == null)
                return new ActionResult(true);

            List<AccountingDistributionDTO> accountingDistributionDTOs = new List<AccountingDistributionDTO>();
            foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
            {

                accountingDistributionDTOs.Add(new AccountingDistributionDTO()
                {
                    Quantity = timePayrollTransaction.Quantity,
                    AccountStdId = timePayrollTransaction.AccountStdId,
                    AccountInternal = timePayrollTransaction.AccountInternal.ToList(),
                });
            }

            foreach (TimePayrollScheduleTransaction timePayrollScheduleTransaction in timePayrollScheduleTransactions)
            {
                accountingDistributionDTOs.Add(new AccountingDistributionDTO()
                {
                    Quantity = timePayrollScheduleTransaction.Quantity,
                    AccountStdId = timePayrollScheduleTransaction.AccountStdId,
                    AccountInternal = timePayrollScheduleTransaction.AccountInternal.ToList(),
                });
            }

            decimal totalQuantity = accountingDistributionDTOs.Sum(x => x.Quantity);
            decimal originalTransactionQuantity = originalTransaction.Quantity;
            int counter = 0;
            var groupedByAccounting = accountingDistributionDTOs.GroupBy(x => x.AccountingString);
            int groupCount = groupedByAccounting.Count();
            foreach (var distributionDTOsGroupedByAccounting in groupedByAccounting)
            {
                counter++;
                var dto = distributionDTOsGroupedByAccounting.FirstOrDefault();
                if (dto == null)
                    continue;

                decimal currentGroupQuantitySum = distributionDTOsGroupedByAccounting.Sum(x => x.Quantity);
                decimal currentGroupQuantityPercent = 0;
                if (groupCount == 1)
                    currentGroupQuantityPercent = 1;
                else if (counter == groupCount)
                    currentGroupQuantityPercent = decimal.Round(1 - newTransactions.Sum(x => x.Quantity) - originalTransaction.Quantity, 2, MidpointRounding.AwayFromZero); //prevent rounding errors
                else
                    currentGroupQuantityPercent = decimal.Round(currentGroupQuantitySum / totalQuantity, 2, MidpointRounding.AwayFromZero);

                decimal currentGroupDistributedQuantity = decimal.Round(originalTransactionQuantity * currentGroupQuantityPercent, 2, MidpointRounding.AwayFromZero);

                if (counter == 1)
                {
                    //Reuse original transaction
                    originalTransaction.Quantity = currentGroupDistributedQuantity;
                    originalTransaction.AccountInternal.Clear();
                    originalTransaction.AccountStdId = dto.AccountStdId;
                    originalTransaction.TimePayrollTransactionExtended.IsDistributed = true;
                    AddAccountInternalsToTimePayrollTransaction(originalTransaction, dto.AccountInternal);
                }
                else
                {
                    var newTransaction = CreateDistributedTimePayrolltransactionFromOriginal(originalTransaction, employment, currentGroupDistributedQuantity, dto.AccountStdId, dto.AccountInternal);
                    if (newTransaction != null)
                        newTransactions.Add(newTransaction);
                }
            }

            return Save();
        }
        private List<TimeTransactionItem> CreateFixedAccountingTransactions(TimeTransactionItem transaction, Employment employment, Employee employee, DateTime date)
        {
            if (employment == null || !employment.FixedAccounting)
                return new List<TimeTransactionItem>();

            PayrollProduct product = GetPayrollProductFromCache(transaction.ProductId);
            if (product != null && product.DontUseFixedAccounting)
                return new List<TimeTransactionItem>();

            List<TimeTransactionItem> newTransactions = new List<TimeTransactionItem>();
            decimal originalQuantity = transaction.Quantity;

            List<EmploymentAccountStd> accountingSettings = GetEmploymentAccountingFromCache(employment);

            foreach (EmploymentAccountType type in EnumUtility.GetEmploymentAccountTypes())
            {
                if (type == EmploymentAccountType.Cost || type == EmploymentAccountType.Income)
                    continue;

                var employmentAccountStd = accountingSettings.FirstOrDefault(x => x.Type == (int)type);
                if (employmentAccountStd == null || employmentAccountStd.Percent == 0)
                    continue;

                if (type == EmploymentAccountType.Fixed1)
                {
                    decimal percent = decimal.Divide(employmentAccountStd.Percent, 100);

                    //ReUse orginal transaction
                    transaction.Quantity = decimal.Round(originalQuantity * percent, 2, MidpointRounding.AwayFromZero);
                    transaction.Dim1Id = 0;
                    if (employmentAccountStd.AccountStd != null)
                        AddAccountStdToTimeTransactionItem(transaction, employmentAccountStd.AccountStd);
                    else
                        ApplyAccountingOnTimeTransactionItem(transaction, employee, product, date, setAccountStd: true, setAccountInternal: false);

                    transaction.ClearAccountInternals();
                    AddAccountInternalsToTimeTransactionItem(transaction, employmentAccountStd.AccountInternal.ToList());
                }
                else
                {
                    var newTransaction = CreateFixedAccountingTimePayrollTransactionFromOriginal(employee, transaction, product, originalQuantity, employmentAccountStd, date);
                    newTransactions.Add(newTransaction);
                }
            }

            #region Fix decimals (Item 50904)

            if (!transaction.IsFixed && !transaction.IsAdded)
            {
                transaction.Quantity = (int)transaction.Quantity;

                foreach (var item in newTransactions)
                {
                    item.Quantity = (int)item.Quantity;
                }

                decimal newTotalQuantity = transaction.Quantity + newTransactions.Sum(x => x.Quantity);
                if (newTotalQuantity != originalQuantity)
                {
                    decimal diff = originalQuantity - newTotalQuantity;
                    //add the diff to the  orginal transaction
                    transaction.Quantity += diff;
                }
            }
            #endregion

            return newTransactions;
        }
        private void SetPayrollProductChainParent(List<TimePayrollTransaction> newTransactions, List<TimePayrollTransaction> templateChain)
        {
            List<TimePayrollTransaction> copyNewTransactions = newTransactions.ToList(); // create new copy            

            TimePayrollTransaction mainParent = templateChain.FirstOrDefault(x => x.IncludedInPayrollProductChain && !x.ParentId.HasValue);
            if (mainParent != null)
            {
                var newParents = copyNewTransactions.Where(x => x.ProductId == mainParent.ProductId).ToList();
                foreach (var newParent in newParents)
                {
                    var childTransactions = copyNewTransactions.Where(x => x.ProductId != newParent.ProductId && x.TimeCodeTransactionId == newParent.TimeCodeTransactionId).ToList();
                    //only get childs whose parents has not already been set
                    childTransactions = childTransactions.Where(x => x.Parent == null).ToList();
                    foreach (var childTemplate in templateChain.Where(x => x.ParentId.HasValue))
                    {
                        var parentTemplate = templateChain.FirstOrDefault(x => x.TimePayrollTransactionId == childTemplate.ParentId.Value);
                        if (parentTemplate != null)
                        {
                            var childProductTransactions = childTransactions.Where(x => x.ProductId == childTemplate.ProductId).ToList();
                            var parentProductTransactions = copyNewTransactions.Where(x => x.ProductId == parentTemplate.ProductId && x.TimeCodeTransactionId == newParent.TimeCodeTransactionId).ToList();

                            foreach (var childProductTransaction in childProductTransactions)
                            {
                                var parent = parentProductTransactions.FirstOrDefault();
                                if (parent != null)
                                {
                                    parentProductTransactions.Remove(parent);
                                    copyNewTransactions.Remove(parent); // a transaction kan only be parent once
                                    childProductTransaction.Parent = parent;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void SetPayrollProductChainParent(List<TimeTransactionItem> newTransactions, List<TimeTransactionItem> templateChain)
        {
            List<TimeTransactionItem> copyNewTransactions = newTransactions.ToList();

            TimeTransactionItem mainParent = templateChain.FirstOrDefault(x => x.IncludedInPayrollProductChain && !x.ParentGuidId.HasValue);
            if (mainParent != null)
            {
                var newParents = copyNewTransactions.Where(x => x.ProductId == mainParent.ProductId).ToList();
                foreach (var newParent in newParents)
                {
                    var childTransactions = copyNewTransactions.Where(x => x.ProductId != newParent.ProductId && x.GuidInternalFK == newParent.GuidInternalFK).ToList();
                    //only get childs whose parents has not already been set
                    childTransactions = childTransactions.Where(x => x.ParentGuidId == null).ToList();
                    foreach (var childTemplate in templateChain.Where(x => x.ParentGuidId.HasValue))
                    {
                        var parentTemplate = templateChain.FirstOrDefault(x => x.GuidId == childTemplate.ParentGuidId.Value);
                        if (parentTemplate != null)
                        {
                            var childProductTransactions = childTransactions.Where(x => x.ProductId == childTemplate.ProductId).ToList();
                            var parentProductTransactions = copyNewTransactions.Where(x => x.ProductId == parentTemplate.ProductId && x.GuidInternalFK == newParent.GuidInternalFK).ToList();

                            foreach (var childProductTransaction in childProductTransactions)
                            {
                                var parent = parentProductTransactions.FirstOrDefault();
                                if (parent != null)
                                {
                                    parentProductTransactions.Remove(parent);
                                    copyNewTransactions.Remove(parent); // a transaction kan only be parent once
                                    childProductTransaction.ParentGuidId = parent.GuidId;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region FixedPayrollRow

        private ActionResult SaveFixedPayrollRows(List<FixedPayrollRowDTO> inputItems, int employeeId)
        {
            ActionResult result;

            #region Prereq

            var changesRepository = TrackChangesManager.CreateEmployeeUserChangesRepository(actorCompanyId, Guid.NewGuid(), TermGroup_TrackChangesActionMethod.Employee_Save, SoeEntityType.Employee);
            changesRepository.SetBeforeValue(GetFixedPayrollRows(employeeId), GetPayrollProductsWithSettingsAndAccountInternalsAndStdsFromCache());

            List<FixedPayrollRow> fixedPayrollRows = new List<FixedPayrollRow>();

            foreach (var item in inputItems)
            {
                if (item.FromDate.HasValue && item.ToDate.HasValue && item.FromDate.Value > item.ToDate.Value)
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(8657, "En eller flera lönerader har ett startdatum som är större än slutdatum"));
            }

            Employee employee = GetEmployeeFromCache(employeeId);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            #endregion

            foreach (var inputRow in inputItems)
            {
                if (inputRow.IsReadOnly)
                    continue;
                if (inputRow.ProductId == 0)
                    continue;
                if (inputRow.FixedPayrollRowId == 0 && inputRow.State == SoeEntityState.Deleted)
                    continue;

                if (!inputRow.IsSpecifiedUnitPrice)
                {
                    //Amount are set to dislplay them when the grid is opened, they should not be saved
                    inputRow.UnitPrice = 0;
                    inputRow.Amount = 0;
                }

                FixedPayrollRow fixedPayrollRow = GetFixedPayrollRow(inputRow.FixedPayrollRowId);
                if (fixedPayrollRow == null)
                {
                    #region Add

                    fixedPayrollRow = new FixedPayrollRow
                    {
                        FromDate = inputRow.FromDate,
                        ToDate = inputRow.ToDate,
                        UnitPrice = inputRow.UnitPrice,
                        Quantity = inputRow.Quantity,
                        Amount = inputRow.Quantity * inputRow.UnitPrice,
                        VatAmount = inputRow.VatAmount,
                        State = (int)SoeEntityState.Active,
                        IsSpecifiedUnitPrice = inputRow.IsSpecifiedUnitPrice,
                        Distribute = inputRow.Distribute,

                        //FK
                        ActorCompanyId = this.actorCompanyId,
                        ProductId = inputRow.ProductId,
                        EmployeeId = employeeId,
                    };

                    SetCreatedProperties(fixedPayrollRow);
                    entities.FixedPayrollRow.AddObject(fixedPayrollRow);
                    #endregion
                }
                else
                {
                    #region Update

                    if (inputRow.State == SoeEntityState.Deleted)
                    {
                        //Delete
                        fixedPayrollRow.State = (int)inputRow.State;
                    }
                    else
                    {
                        fixedPayrollRow.FromDate = inputRow.FromDate;
                        fixedPayrollRow.ToDate = inputRow.ToDate;
                        fixedPayrollRow.UnitPrice = inputRow.UnitPrice;
                        fixedPayrollRow.Quantity = inputRow.Quantity;
                        fixedPayrollRow.Amount = inputRow.Quantity * inputRow.UnitPrice;
                        fixedPayrollRow.VatAmount = inputRow.VatAmount;
                        fixedPayrollRow.IsSpecifiedUnitPrice = inputRow.IsSpecifiedUnitPrice;
                        fixedPayrollRow.Distribute = inputRow.Distribute;
                        fixedPayrollRow.ProductId = inputRow.ProductId;
                    }

                    SetModifiedProperties(fixedPayrollRow);

                    #endregion
                }

                fixedPayrollRows.Add(fixedPayrollRow);

                result = Save();
                if (!result.Success)
                    return result;

            }

            result = Save();
            if (!result.Success)
                return result;

            #region Track changes (Insert)

            changesRepository.SetAfterValue(fixedPayrollRows);
            TrackChangesManager.SaveEmployeeUserChanges(entities, currentTransaction, changesRepository);

            #endregion

            return result;
        }
        private ActionResult CreateTimePayrollTransactionsFromFixedPayrollRows(List<TimePayrollTransaction> timePayrollTransactions, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions, int employeeId, int timePeriodId, TimeBlockDate transactionTimeBlockDate, ReCalculatePayrollPeriodCompanyDTO companyDTO, bool deleteTransactions = true)
        {
            ActionResult result = null;

            #region Prereq

            TimePeriod timePeriod = GetTimePeriodFromCache(timePeriodId);
            if (timePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));

            if (IsEmployeeTimePeriodLockedForChanges(employeeId, timePeriodId: timePeriodId))
                return new ActionResult(true);

            if (timePeriod.ExtraPeriod)
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8540, "Anställd kunde inte hittas"));

            Employment employment = ((timePeriod.HasPayrollDates() ? employee.GetEmployments(timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value).GetLastEmployment() : null) 
                ?? employee.GetEmployment(transactionTimeBlockDate.Date)) 
                ?? employee.GetEmployments(timePeriod.StartDate, timePeriod.StopDate).GetLastEmployment();
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            List<TimePayrollTransaction> timePayrollTransactionsForPeriod = new List<TimePayrollTransaction>();

            //VacationGroups
            List<VacationGroup> vacationGroups = GetVacationGroupsWithSEFromCache();

            #endregion

            #region Delete existing fixed TimePayrollTransaction's in period

            if (deleteTransactions)
            {
                List<TimePayrollTransaction> fixedTransactionsInPeriod = GetTimePayrollTransactionsFixedWithExtended(employeeId, timePeriodId);

                foreach (var fixedTransactionInPeriod in fixedTransactionsInPeriod)
                {
                    if (!fixedTransactionInPeriod.IsFixed) //just to be sure
                        continue;

                    ChangeEntityState(fixedTransactionInPeriod, SoeEntityState.Deleted);
                    if (fixedTransactionInPeriod.TimePayrollTransactionExtended != null)
                        ChangeEntityState(fixedTransactionInPeriod.TimePayrollTransactionExtended, SoeEntityState.Deleted);
                }
            }

            #endregion

            #region Find FixedPayrollRows to use

            List<FixedPayrollRowDTO> fixedPayrollRowsToUse = new List<FixedPayrollRowDTO>();
            List<FixedPayrollRowDTO> employeeFixedPayrollRows = GetFixedPayrollRowsFromCache(employeeId);

            foreach (var employeeFixedPayrollRow in employeeFixedPayrollRows)
            {
                if (employeeFixedPayrollRow.FromDate.HasValue && employeeFixedPayrollRow.ToDate.HasValue)
                {
                    if (employeeFixedPayrollRow.FromDate.Value <= transactionTimeBlockDate.Date && transactionTimeBlockDate.Date <= employeeFixedPayrollRow.ToDate.Value) //if date is in the range
                        fixedPayrollRowsToUse.Add(employeeFixedPayrollRow);
                }
                else if (!employeeFixedPayrollRow.FromDate.HasValue && !employeeFixedPayrollRow.ToDate.HasValue)
                {
                    fixedPayrollRowsToUse.Add(employeeFixedPayrollRow);
                }
                else if (!employeeFixedPayrollRow.FromDate.HasValue && employeeFixedPayrollRow.ToDate.HasValue)
                {
                    if (employeeFixedPayrollRow.ToDate.Value >= transactionTimeBlockDate.Date) //if toDate is after date
                        fixedPayrollRowsToUse.Add(employeeFixedPayrollRow);
                }
                else if (employeeFixedPayrollRow.FromDate.HasValue && !employeeFixedPayrollRow.ToDate.HasValue && employeeFixedPayrollRow.FromDate.Value <= transactionTimeBlockDate.Date)
                {
                    fixedPayrollRowsToUse.Add(employeeFixedPayrollRow);
                }
            }

            #endregion

            #region Get PayrollGroupPayrollProducts - Convert to FixedPayrollRowDTO

            int? payrollGroupId = employment.GetPayrollGroupId(transactionTimeBlockDate.Date);
            if (payrollGroupId.HasValue)
            {
                List<PayrollGroupPayrollProduct> payrollGroupProducts = GetPayrollGroupPayrollProductsFromCache(payrollGroupId.Value);
                fixedPayrollRowsToUse.AddRange(payrollGroupProducts.ToDTOFixedPayrollRowDTOs(false).ToList());
            }

            #endregion

            #region Get Monthly salary from previous payrollgroup

            List<int> monthySalaryProductIds = companyDTO.PayrollProductMonthlySalaries.Select(x => x.ProductId).ToList();

            if (timePeriod.HasPayrollDates() && transactionTimeBlockDate.Date > timePeriod.StartDate && !fixedPayrollRowsToUse.Any(x => monthySalaryProductIds.Contains(x.ProductId)))
            {
                (int? previousPayrollGroupId, DateTime date) = employee.GetPreviousPayrollGroupId(timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value, currentPayrollGroupId: payrollGroupId);
                if (previousPayrollGroupId.HasValue && previousPayrollGroupId != payrollGroupId)
                {
                    List<PayrollGroupPayrollProduct> payrollGroupProducts = GetPayrollGroupPayrollProductsFromCache(previousPayrollGroupId.Value);
                    if (payrollGroupProducts.Any(x => monthySalaryProductIds.Contains(x.ProductId)))
                    {
                        FixedPayrollRowDTO monthlySalaryFromPreviousPayrollGroup = payrollGroupProducts.FirstOrDefault(x => monthySalaryProductIds.Contains(x.ProductId)).ToDTOFixedPayrollRowDTO(false);
                        monthlySalaryFromPreviousPayrollGroup.IsFromPreviousPayrollGroup = true;
                        monthlySalaryFromPreviousPayrollGroup.IsFromPreviousPayrollGroupDate = date;
                        fixedPayrollRowsToUse.Add(monthlySalaryFromPreviousPayrollGroup);
                    }
                }
            }

            #endregion

            #region Create TimePayrollTransactions

            foreach (var fixedPayrollRowToUse in fixedPayrollRowsToUse)
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(fixedPayrollRowToUse.ProductId);
                if (payrollProduct == null)
                    continue;

                TimeBlockDate timeBlockDate = transactionTimeBlockDate;
                Employment employmentToUse = employment;
                DateTime? monthlySalaryDateFrom = timePeriod.PayrollStartDate;
                DateTime? monthlySalaryDateTo = timePeriod.PayrollStopDate;
                if (fixedPayrollRowToUse.IsFromPreviousPayrollGroup)
                {
                    employmentToUse = employee.GetEmployment(fixedPayrollRowToUse.IsFromPreviousPayrollGroupDate);
                    if (employmentToUse == null)
                        continue;
                    //timeBlockDate = GetTimeBlockDateFromCache(employeeId, fixedPayrollRowToUse.IsFromPreviousPayrollGroupDate);
                    //if (timeBlockDate == null)
                    //    continue;

                    monthlySalaryDateFrom = timePeriod.PayrollStartDate;
                    monthlySalaryDateTo = fixedPayrollRowToUse.IsFromPreviousPayrollGroupDate;
                }

                List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();

                TimePayrollTransaction timePayrollTransaction = new TimePayrollTransaction
                {
                    VatAmount = fixedPayrollRowToUse.VatAmount,
                    Quantity = fixedPayrollRowToUse.Quantity,

                    IsPreliminary = false,
                    Exported = false,
                    AutoAttestFailed = false,
                    ManuallyAdded = true,
                    Comment = "",
                    IsFixed = true,
                    IsSpecifiedUnitPrice = fixedPayrollRowToUse.IsSpecifiedUnitPrice,

                    //Set FK
                    ActorCompanyId = actorCompanyId,
                    EmployeeId = employee.EmployeeId,
                    ProductId = fixedPayrollRowToUse.ProductId,
                    AttestStateId = attestStateInitialPayroll.AttestStateId,
                    TimePeriodId = timePeriodId,
                    AccountStdId = 0,

                    //Set reference
                    TimeBlockDate = timeBlockDate,
                };
                SetCreatedProperties(timePayrollTransaction);
                entities.TimePayrollTransaction.AddObject(timePayrollTransaction);

                SetTimePayrollTransactionType(timePayrollTransaction, payrollProduct);
                CreateTimePayrollTransactionExtended(timePayrollTransaction, employeeId, actorCompanyId);
                SetTimePayrollTransactionQuantity(timePayrollTransaction, payrollProduct, employmentToUse, timePayrollTransaction.TimeBlockDate, timePayrollTransaction.Quantity, vacationGroups: vacationGroups);

                // Accounting
                DateTime accountingDate = timePayrollTransaction.TimeBlockDate.Date;
                if (payrollProduct.AverageCalculated)
                {
                    accountingDate = GetPayrollAccountingDateIfEmployeeNotEmployedOnTransactionDate(timePeriod, employee, accountingDate);
                }

                ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, accountingDate, payrollProduct);

                newTransactions.Add(timePayrollTransaction);

                result = Save();
                if (!result.Success)
                    return result;

                #region PayrollProductChain/Fixed Accounting/Distribution

                //Must be done after accounting is set on parent transaction
                result = CreateTransactionsFromPayrollProductChain(timePayrollTransaction, employee, timePayrollTransaction.TimeBlockDate, out List<TimePayrollTransaction> childTransactions);
                if (!result.Success)
                    return result;

                newTransactions.AddRange(childTransactions);

                if (fixedPayrollRowToUse.Distribute)
                {
                    int distributionPayrollProductId = GetCompanyIntSettingFromCache(CompanySettingType.PayrollAccountingDistributionPayrollProduct);

                    //distribute only the transaction that is generated from fixedPayrollRowToUse
                    result = CreateDistributedTransactions(timePayrollTransactions.Where(x => x.ProductId == distributionPayrollProductId).ToList(), timePayrollScheduleTransactions.Where(x => x.ProductId == distributionPayrollProductId && x.Type.HasValue && x.Type.Value == (int)SoeTimePayrollScheduleTransactionType.Absence).ToList(), timePayrollTransaction, employee, timePayrollTransaction.TimeBlockDate, out List<TimePayrollTransaction> distributedTransactions);
                    if (!result.Success)
                        return result;

                    newTransactions.AddRange(distributedTransactions);
                }
                else
                {
                    result = CreateFixedAccountingTransactions(newTransactions, employee, timePayrollTransaction.TimeBlockDate, out List<TimePayrollTransaction> fixedAccountingTransactions, employment: employmentToUse);
                    if (!result.Success)
                        return result;

                    newTransactions.AddRange(fixedAccountingTransactions);
                }

                #endregion

                #region Amounts

                SetTimePayrollTransactionAmountsForFixedPayrollRows(monthlySalaryDateFrom, monthlySalaryDateTo, timePayrollTransaction.TimeBlockDate.Date, employee, employmentToUse, fixedPayrollRowToUse, newTransactions);

                #endregion

                #region Rounding

                result = CreateRoundingTransactions(newTransactions, new List<TimePayrollScheduleTransaction>(), newTransactions.GetTimeBlockDates(), employee, timePeriod, transactionTimeBlockDate);
                if (!result.Success)
                    return result;          
                if (result.Value is List<TimePayrollTransaction> timePayrollTransactionsRounding)
                    newTransactions.AddRange(timePayrollTransactionsRounding);

                #endregion

                result = Save();
                if (!result.Success)
                    return result;

                timePayrollTransactionsForPeriod.AddRange(newTransactions);

            }

            #endregion

            result = Save();
            if (!result.Success)
                return result;

            result.Value = timePayrollTransactionsForPeriod;
            return result;
        }       

        private List<RetroactivePayrollCalculationDTO> CalculateRetroactivePayrollFromTimePayrollTransactionsAdded(Employee employee, DateTime dateFrom, DateTime? dateTo, TimeBlockDate timeBlockDate, PayrollProduct payrollProduct, List<TimePayrollTransaction> addedTimePayrollTransactionsForDayAndProduct)
        {
            List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();

            addedTimePayrollTransactionsForDayAndProduct = addedTimePayrollTransactionsForDayAndProduct.Where(x => x.EmployeeId == employee.EmployeeId && x.TimePeriodId.HasValue && x.IsAdded && CalendarUtility.IsDatesOverlappingNullable(dateFrom, dateTo, x.AddedDateFrom, x.AddedDateTo)).ToList();
            if (!addedTimePayrollTransactionsForDayAndProduct.Any())
                return new List<RetroactivePayrollCalculationDTO>();

            foreach (var addedTimePayrollTransactionsByUnitPrice in addedTimePayrollTransactionsForDayAndProduct.GroupBy(x => x.UnitPrice))
            {
                foreach (var transaction in addedTimePayrollTransactionsByUnitPrice)
                {
                    if (!transaction.AddedDateFrom.HasValue || !transaction.AddedDateTo.HasValue)
                        continue;

                    RetroactivePayrollCalculationDTO retroCalculation = new RetroactivePayrollCalculationDTO(payrollProduct.ProductId, timeBlockDate.Date, TermGroup_PayrollResultType.Quantity, addedTimePayrollTransactionsByUnitPrice.ToList());

                    if (transaction.IsSpecifiedUnitPrice)
                    {
                        retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.SpecifiedUnitPrice;
                    }
                    else
                    {
                        Employment employment = employee.GetEmployment(transaction.AddedDateTo.Value) ?? employee.GetLastEmployment();
                        if (employment != null)
                        {
                            bool transactionIsOverlappedByRetroInterval = CalendarUtility.IsCurrentOverlappedByNew(dateFrom, dateTo ?? DateTime.MaxValue, transaction.AddedDateFrom.Value, transaction.AddedDateTo.Value);
                            if (transactionIsOverlappedByRetroInterval)
                            {
                                PayrollPriceFormulaResultDTO formulaResult = EvaluatePayrollPriceFormula(transaction.AddedDateTo.Value, employee, employment, payrollProduct);
                                if (formulaResult != null)
                                {
                                    retroCalculation.RetroUnitPrice = formulaResult.Amount;
                                    retroCalculation.TransactionUnitPrice = addedTimePayrollTransactionsByUnitPrice.Key ?? 0;
                                }
                                else
                                {
                                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.FormulaNotFound;
                                }
                            }
                            else
                            {
                                retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.RetroDontOverlapPeriod;
                            }
                        }
                        else
                        {
                            retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.EmploymentNotFound;
                        }
                    }

                    retroCalculations.Add(retroCalculation);
                }
            }

            return retroCalculations;
        }
        private List<RetroactivePayrollCalculationDTO> CalculateRetroactivePayrollFromTimePayrollTransactionsFixed(Employee employee, Employment employment, DateTime dateFrom, DateTime? dateTo, TimeBlockDate timeBlockDate, PayrollProduct payrollProduct, List<TimePayrollTransaction> fixedTimePayrollTransactionsForDayAndProduct)
        {
            List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();

            fixedTimePayrollTransactionsForDayAndProduct = fixedTimePayrollTransactionsForDayAndProduct.Where(x => x.EmployeeId == employee.EmployeeId && x.TimePeriodId.HasValue && x.IsFixed).ToList();
            if (!fixedTimePayrollTransactionsForDayAndProduct.Any())
                return new List<RetroactivePayrollCalculationDTO>();

            foreach (var transactionsByPeriod in fixedTimePayrollTransactionsForDayAndProduct.GroupBy(x => x.TimePeriodId))
            {
                TimePeriod timePeriod = GetTimePeriodFromCache(transactionsByPeriod.Key.Value);
                if (timePeriod == null || !timePeriod.HasPayrollDates())
                    continue;

                foreach (var transactionsByUnitPrice in transactionsByPeriod.GroupBy(x => x.UnitPrice))
                {
                    decimal transactionUnitPrice = transactionsByUnitPrice.Key ?? 0;

                    foreach (var transactionsByIsSpecifiedUnitPrice in transactionsByUnitPrice.GroupBy(x => x.IsSpecifiedUnitPrice))
                    {
                        bool isSpecifiedUnitPrice = transactionsByIsSpecifiedUnitPrice.Key;

                        RetroactivePayrollCalculationDTO retroCalculation = new RetroactivePayrollCalculationDTO(payrollProduct.ProductId, timeBlockDate.Date, TermGroup_PayrollResultType.Quantity, transactionsByIsSpecifiedUnitPrice.ToList());
                        bool periodIsOverlappedByRetroInterval = CalendarUtility.IsCurrentOverlappedByNew(dateFrom, dateTo ?? DateTime.MaxValue, timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value);

                        if (isSpecifiedUnitPrice)
                        {
                            retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.SpecifiedUnitPrice;
                        }
                        else
                        {
                            if (payrollProduct.AverageCalculated)
                            {
                                PayrollPriceFormulaResultDTO formulaResult = CalculatePayrollPriceFormula(timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value, timeBlockDate.Date, employee, employment, payrollProduct);
                                if (formulaResult != null)
                                {
                                    if (periodIsOverlappedByRetroInterval)
                                    {
                                        retroCalculation.RetroUnitPrice = formulaResult.Amount;
                                        retroCalculation.TransactionUnitPrice = transactionUnitPrice;
                                    }
                                    else
                                    {
                                        decimal difference = formulaResult.Amount - (transactionsByUnitPrice.Key ?? 0);
                                        decimal unitprice = (difference / ((decimal)(timePeriod.PayrollStopDate.Value.Date.AddDays(1) - timePeriod.PayrollStartDate.Value.Date).TotalDays)) * ((decimal)(timePeriod.PayrollStopDate.Value.Date.AddDays(1) - dateFrom.Date).TotalDays);
                                        retroCalculation.RetroUnitPrice = unitprice;
                                        retroCalculation.IsPartOfPayrollPeriodCalculated = true;
                                        retroCalculation.TransactionUnitPrice = 0;
                                    }
                                }
                                else
                                {
                                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.FormulaNotFound;
                                }
                            }
                            else
                            {
                                if (employment != null)
                                {
                                    if (periodIsOverlappedByRetroInterval)
                                    {
                                        PayrollPriceFormulaResultDTO formulaResult = CalculatePayrollPriceFormula(null, null, timeBlockDate.Date, employee, employment, payrollProduct);
                                        if (formulaResult != null)
                                        {
                                            retroCalculation.RetroUnitPrice = formulaResult.Amount;
                                            retroCalculation.TransactionUnitPrice = transactionsByUnitPrice.Key ?? 0;
                                        }
                                        else
                                        {
                                            retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.FormulaNotFound;
                                        }
                                    }
                                    else
                                    {
                                        retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.RetroDontOverlapPeriod;
                                    }
                                }
                                else
                                {
                                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.EmploymentNotFound;
                                }
                            }
                        }

                        retroCalculations.Add(retroCalculation);
                    }
                }
            }

            return retroCalculations;
        }
        private List<RetroactivePayrollCalculationDTO> CalculateRetroactivePayrollFromTimePayrollTransactions(Employee employee, Employment employment, TimeBlockDate timeBlockDate, PayrollProduct payrollProduct, List<TimePayrollTransaction> timePayrollTransactionsForDayAndProduct)
        {
            List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();
            timePayrollTransactionsForDayAndProduct = timePayrollTransactionsForDayAndProduct.Where(x => x.EmployeeId == employee.EmployeeId && !x.IsAdded && !x.IsFixed).ToList();
            if (!timePayrollTransactionsForDayAndProduct.Any())
                return new List<RetroactivePayrollCalculationDTO>();

            foreach (var transactionsByUnitPrice in timePayrollTransactionsForDayAndProduct.GroupBy(x => x.UnitPrice))
            {
                decimal transactionUnitPrice = transactionsByUnitPrice.Key ?? 0;

                RetroactivePayrollCalculationDTO retroCalculation = new RetroactivePayrollCalculationDTO(payrollProduct.ProductId, timeBlockDate.Date, (TermGroup_PayrollResultType)payrollProduct.ResultType, transactionsByUnitPrice.ToList());
                if (employment != null)
                {
                    PayrollPriceFormulaResultDTO formulaResult = CalculatePayrollPriceFormula(null, null, timeBlockDate.Date, employee, employment, payrollProduct);
                    if (formulaResult != null)
                    {
                        TimePayrollTransaction timePayrollTransaction = retroCalculation.TimePayrollTransactionBasis.FirstOrDefault(i => i.TimePayrollTransactionExtended != null);

                        decimal? unitPrice = formulaResult.Amount;
                        decimal? amount = 0; //do not calculate amount
                        decimal transactionQuantity = 0; //do not calculate amount
                        decimal? transactionQuantityCalendarDays = timePayrollTransaction != null ? timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays : (decimal?)null;
                        decimal? transactionCalenderDayFactor = timePayrollTransaction != null ? timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor : (decimal?)null;
                        string accountingString = (employment.FixedAccounting && timePayrollTransaction != null) ? timePayrollTransaction.GetAccountingString(GetAccountDimsFromCache()) : "";
                        string accountStdId = (employment.FixedAccounting && timePayrollTransaction != null) ? timePayrollTransaction.AccountStdId.ToString() : "";

                        CalculateTimePayrollTransactionUnitPriceAndAmounts(transactionQuantity, transactionQuantityCalendarDays, transactionCalenderDayFactor, retroCalculation.ResultType, timePayrollTransactionsForDayAndProduct, payrollProduct, timeBlockDate, employment, formulaResult, accountingString, accountStdId, ref unitPrice, ref amount);
                        retroCalculation.TransactionUnitPrice = transactionUnitPrice;
                        retroCalculation.RetroUnitPrice = unitPrice ?? 0;

                        //Set it to quantity so retroamount will be calculated correctly
                        if (payrollProduct.IsVacationCompensation() || payrollProduct.IsVacationAdditionOrSalaryPrepaymentPaid() || payrollProduct.IsVacationAdditionOrSalaryVariablePrepaymentPaid())
                            retroCalculation.ResultType = TermGroup_PayrollResultType.Quantity;
                    }
                    else
                    {
                        retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.FormulaNotFound;
                    }
                }
                else
                {
                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.EmploymentNotFound;
                }

                retroCalculations.Add(retroCalculation);
            }

            return retroCalculations;
        }
        private List<RetroactivePayrollCalculationDTO> CalculateRetroactivePayrollFromTimePayrollTransactionsAlreadyRetro(Employee employee, Employment employment, TimeBlockDate timeBlockDate, PayrollProduct payrollProduct, List<TimePayrollTransaction> timePayrollTransactionsForDayAndProduct)
        {
            List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();

            timePayrollTransactionsForDayAndProduct = timePayrollTransactionsForDayAndProduct.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
            if (!timePayrollTransactionsForDayAndProduct.Any())
                return new List<RetroactivePayrollCalculationDTO>();

            foreach (var transactionsByUnitPrice in timePayrollTransactionsForDayAndProduct.GroupBy(x => x.UnitPrice))
            {
                decimal transactionUnitPrice = transactionsByUnitPrice.Key ?? 0;
                RetroactivePayrollCalculationDTO retroCalculation = new RetroactivePayrollCalculationDTO(payrollProduct.ProductId, timeBlockDate.Date, (TermGroup_PayrollResultType)payrollProduct.ResultType, transactionsByUnitPrice.ToList(), isReversed: true);

                if (employment != null)
                {
                    retroCalculation.TransactionUnitPrice = transactionUnitPrice;
                    retroCalculation.RetroUnitPrice = 0;
                }
                else
                {
                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.EmploymentNotFound;
                }

                retroCalculations.Add(retroCalculation);
            }

            return retroCalculations;
        }
        private List<RetroactivePayrollCalculationDTO> CalculateRetroactivePayrollFromTimePayrollScheduleTransactions(Employee employee, Employment employment, TimeBlockDate timeBlockDate, PayrollProduct payrollProduct, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsForDayAndProduct)
        {
            List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();

            timePayrollScheduleTransactionsForDayAndProduct = timePayrollScheduleTransactionsForDayAndProduct.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
            if (!timePayrollScheduleTransactionsForDayAndProduct.Any())
                return new List<RetroactivePayrollCalculationDTO>();

            foreach (var transactionsByUnitPrice in timePayrollScheduleTransactionsForDayAndProduct.GroupBy(x => x.UnitPrice))
            {
                decimal transactionUnitPrice = transactionsByUnitPrice.Key ?? 0;
                RetroactivePayrollCalculationDTO retroCalculation = new RetroactivePayrollCalculationDTO(payrollProduct.ProductId, timeBlockDate.Date, (TermGroup_PayrollResultType)payrollProduct.ResultType, transactionsByUnitPrice.ToList());

                if (employment != null)
                {
                    PayrollPriceFormulaResultDTO formulaResult = CalculatePayrollPriceFormula(null, null, timeBlockDate.Date, employee, employment, payrollProduct);
                    if (formulaResult != null)
                    {
                        retroCalculation.RetroUnitPrice = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                        retroCalculation.TransactionUnitPrice = transactionUnitPrice;
                    }
                    else
                    {
                        retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.FormulaNotFound;
                    }
                }
                else
                {
                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.EmploymentNotFound;
                }

                retroCalculations.Add(retroCalculation);
            }

            return retroCalculations;
        }
        private List<RetroactivePayrollCalculationDTO> CalculateRetroactivePayrollFromTimePayrollScheduleTransactionsAlreadyRetro(Employee employee, Employment employment, TimeBlockDate timeBlockDate, PayrollProduct payrollProduct, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsForDayAndProduct)
        {
            List<RetroactivePayrollCalculationDTO> retroCalculations = new List<RetroactivePayrollCalculationDTO>();

            timePayrollScheduleTransactionsForDayAndProduct = timePayrollScheduleTransactionsForDayAndProduct.Where(x => x.EmployeeId == employee.EmployeeId).ToList();
            if (!timePayrollScheduleTransactionsForDayAndProduct.Any())
                return new List<RetroactivePayrollCalculationDTO>();

            foreach (var transactionsByUnitPrice in timePayrollScheduleTransactionsForDayAndProduct.GroupBy(x => x.UnitPrice))
            {
                decimal transactionUnitPrice = transactionsByUnitPrice.Key ?? 0;
                RetroactivePayrollCalculationDTO retroCalculation = new RetroactivePayrollCalculationDTO(payrollProduct.ProductId, timeBlockDate.Date, (TermGroup_PayrollResultType)payrollProduct.ResultType, transactionsByUnitPrice.ToList(), isReversed: true);

                if (employment != null)
                {
                    retroCalculation.TransactionUnitPrice = transactionUnitPrice;
                    retroCalculation.RetroUnitPrice = 0;
                }
                else
                {
                    retroCalculation.ErrorCode = (int)TermGroup_SoeRetroactivePayrollOutcomeErrorCode.EmploymentNotFound;
                }

                retroCalculations.Add(retroCalculation);
            }

            return retroCalculations;
        }
        private void SetTimePayrollTransactionAmountsForFixedPayrollRows(DateTime? payrollStartDate, DateTime? payrollStopDate, DateTime transactionDate, Employee employee, Employment employment, FixedPayrollRowDTO fixedPayrollRowToUse, List<TimePayrollTransaction> timePayrollTransactions)
        {
            foreach (var timePayrollTransactionByProduct in timePayrollTransactions.GroupBy(x => x.ProductId))
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactionByProduct.Key);
                if (payrollProduct == null)
                    return;

                foreach (var timePayrollTransaction in timePayrollTransactionByProduct)
                {
                    //if unitprice is given use it or else it should be calculated
                    if (fixedPayrollRowToUse.IsSpecifiedUnitPrice)
                    {
                        timePayrollTransaction.UnitPrice = fixedPayrollRowToUse.UnitPrice;
                        timePayrollTransaction.Amount = Decimal.Round(timePayrollTransaction.Quantity * timePayrollTransaction.UnitPrice.Value, 2, MidpointRounding.AwayFromZero);
                        timePayrollTransaction.VatAmount = Decimal.Round(timePayrollTransaction.Quantity * fixedPayrollRowToUse.VatAmount, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        PayrollPriceFormulaResultDTO formulaResult = CalculatePayrollPriceFormula(payrollStartDate, payrollStopDate, transactionDate, employee, employment, payrollProduct);
                        if (formulaResult != null)
                        {
                            CreateTimePayrollTransactionExtended(timePayrollTransaction, employee.EmployeeId, actorCompanyId);

                            SetTimePayrollTransactionFormulas(timePayrollTransaction, formulaResult);
                            timePayrollTransaction.UnitPrice = Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero);
                            timePayrollTransaction.Amount = Decimal.Round((timePayrollTransaction.Quantity * timePayrollTransaction.UnitPrice.Value), 2, MidpointRounding.AwayFromZero);//quantity from FixedPayrollRow is never time                 
                            timePayrollTransaction.State = (int)SoeEntityState.Active;

                            SetCreatedProperties(timePayrollTransaction.TimePayrollTransactionExtended);
                        }
                    }

                    SetTimePayrollTransactionCurrencyAmounts(timePayrollTransaction);
                    SetModifiedProperties(timePayrollTransaction);
                }
            }
        }

        #endregion

        #region MassRegistrationTemplate

        private ActionResult CreateAddedTransactionsFromTemplate(MassRegistrationTemplateHeadDTO template, List<MassRegistrationTemplateRowDTO> rows, bool deleteTransactions, bool decideTimePeriod, int timePeriodId)
        {
            ActionResult result = new ActionResult();

            List<TimePeriodHead> timePeriodHeadCache = new List<TimePeriodHead>();

            #region Perform

            foreach (var templateRow in rows)
            {
                if (templateRow.EmployeeId <= 0 || templateRow.ProductId <= 0 || !templateRow.PaymentDate.HasValue || !templateRow.DateFrom.HasValue || !templateRow.DateTo.HasValue)
                    continue;

                Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(templateRow.EmployeeId, getHidden: false);
                if (employee == null)
                    continue;

                TimePeriod timePeriod = null;

                if (decideTimePeriod)
                {
                    PayrollGroup payrollGroup = employee.GetPayrollGroup(templateRow.PaymentDate.Value, GetPayrollGroupsFromCache()) ?? employee.GetLastPayrollGroup();
                    if (payrollGroup == null)
                        continue;

                    int timePeriodHeadId = payrollGroup.TimePeriodHeadId ?? GetCompanyIntSettingFromCache(CompanySettingType.TimeDefaultTimePeriodHead);
                    if (timePeriodHeadId == 0)
                        continue;

                    TimePeriodHead timePeriodHead = timePeriodHeadCache.FirstOrDefault(x => x.TimePeriodHeadId == timePeriodHeadId);
                    if (timePeriodHead == null)
                    {
                        timePeriodHead = GetTimePeriodHeadWithPeriods(timePeriodHeadId);
                        if (timePeriodHead != null)
                            timePeriodHeadCache.Add(timePeriodHead);
                    }
                    if (timePeriodHead == null)
                        continue;

                    timePeriod = timePeriodHead.TimePeriod.Where(x => x.State == (int)SoeEntityState.Active).ToList().GetTimePeriod(templateRow.PaymentDate.Value);
                    if (timePeriod == null)
                        continue;

                    if (IsEmployeeTimePeriodLockedForChanges(templateRow.EmployeeId, timePeriodId: timePeriod.TimePeriodId))
                        continue;

                }
                else
                {
                    timePeriod = GetTimePeriodFromCache(timePeriodId);
                    if (timePeriod == null)
                        continue;
                }

                if (deleteTransactions)
                {
                    result = SetTimePayrollTransactionsForMassRegistrationToDeleted(templateRow.EmployeeId, timePeriod.TimePeriodId, templateRow.MassRegistrationTemplateRowId, saveChanges: true);
                    if (!result.Success)
                        return result;
                }

                AttestPayrollTransactionDTO transactionDTO = new AttestPayrollTransactionDTO()
                {
                    PayrollProductId = templateRow.ProductId,
                    AddedDateFrom = templateRow.DateFrom,
                    AddedDateTo = templateRow.DateTo,
                    VatAmount = 0,
                    Quantity = templateRow.Quantity,
                    UnitPrice = templateRow.UnitPrice,
                    Amount = templateRow.Quantity * templateRow.UnitPrice,
                    IsSpecifiedUnitPrice = templateRow.IsSpecifiedUnitPrice,
                    Comment = template.Comment,
                };

                var accountSettings = new List<AccountingSettingDTO>();

                if (templateRow.Dim1Id != 0)
                {
                    accountSettings.Add(new AccountingSettingDTO
                    {
                        DimNr = Constants.ACCOUNTDIM_STANDARD,
                        Account1Id = templateRow.Dim1Id,
                    });
                }
                if (templateRow.Dim2Id != 0)
                {
                    accountSettings.Add(new AccountingSettingDTO
                    {
                        Account1Id = templateRow.Dim2Id,
                    });
                }
                if (templateRow.Dim3Id != 0)
                {
                    accountSettings.Add(new AccountingSettingDTO
                    {
                        Account1Id = templateRow.Dim3Id,
                    });
                }
                if (templateRow.Dim4Id != 0)
                {
                    accountSettings.Add(new AccountingSettingDTO
                    {
                        Account1Id = templateRow.Dim4Id,
                    });
                }
                if (templateRow.Dim5Id != 0)
                {
                    accountSettings.Add(new AccountingSettingDTO
                    {
                        Account1Id = templateRow.Dim5Id,
                    });
                }
                if (templateRow.Dim6Id != 0)
                {
                    accountSettings.Add(new AccountingSettingDTO
                    {
                        Account1Id = templateRow.Dim6Id,
                    });
                }

                if (!accountSettings.Any())
                    accountSettings = null;

                result = SaveAddedTransaction(transactionDTO, accountSettings, null, templateRow.EmployeeId, timePeriod.TimePeriodId, templateRow.MassRegistrationTemplateRowId, true);
                if (!result.Success)
                    return result;
            }

            return result;

            #endregion
        }
        private ActionResult ReCreateAddedTransactionsFromTemplate(int employeeId, TimePeriod timePeriod, List<TimePayrollTransaction> timePayrollTransactionsForPeriod)
        {
            ActionResult result = new ActionResult();

            if (timePeriod == null || !timePeriod.PayrollStartDate.HasValue)
                return result;

            List<MassRegistrationTemplateHeadDTO> templates = GetMassRegistrationTemplatesForPayrollCalculationFromCache().Where(x => x.RecurringDateTo.Value.Date >= timePeriod.PayrollStartDate.Value.Date).ToList();
            if (!templates.Any())
                return result;

            List<TimePayrollTransaction> employeeTemplateTransactions = timePayrollTransactionsForPeriod.Where(x => x.MassRegistrationTemplateRowId.HasValue).ToList();

            foreach (var template in templates)
            {
                var employeeTemplateRows = template.Rows.Where(x => x.EmployeeId == employeeId).ToList();
                foreach (var row in employeeTemplateRows)
                {
                    var timepayrollTransactionTemplateRows = employeeTemplateTransactions.Where(x => x.MassRegistrationTemplateRowId == row.MassRegistrationTemplateRowId).ToList();
                    result = SetTimePayrollTransactionsToDeleted(timepayrollTransactionTemplateRows, saveChanges: false);
                    if (!result.Success)
                        return result;

                    timePayrollTransactionsForPeriod = timePayrollTransactionsForPeriod.Where(x => !(timepayrollTransactionTemplateRows.Select(t => t.TimePayrollTransactionId).Contains(x.TimePayrollTransactionId))).ToList();
                }

                result = CreateAddedTransactionsFromTemplate(template, employeeTemplateRows.Where(x => x.State == SoeEntityState.Active).ToList(), false, false, timePeriod.TimePeriodId);
                if (!result.Success)
                    return result;
            }

            result = Save();
            if (!result.Success)
                return result;

            return result;
        }

        #endregion

        #region NetSalary

        private ActionResult CreateNetSalaryAndDistressAmountTransaction(decimal netSalaryAmount, List<TimePayrollTransaction> timePayrollTransactionsForPeriod, PayrollProduct payrollProductNetSalary, PayrollProduct payrollProductNetSalaryRound, PayrollProduct payrollProductSalaryDistressAmount, TimePeriod timePeriod, Employee employee, EmployeeTimePeriod employeeTimePeriod, TimeBlockDate lastTimeBlockDateInPeriod, int attestStateId, bool deleteTransactionIfZero)
        {
            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (employeeTimePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeTimePeriod");
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
            AttestStateDTO attestState = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestState == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            TimePayrollTransaction timePayrollTransactionNetSalary = CreateOrUpdateTimePayrollTransaction(payrollProductNetSalary, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, attestStateId, 1, netSalaryAmount, netSalaryAmount, 0, String.Empty, timePayrollTransactionsForPeriod);
            if (timePayrollTransactionNetSalary != null)
            {
                if (timePayrollTransactionNetSalary.Amount == 0 && deleteTransactionIfZero)
                {
                    SetTimePayrollTransactionToDeleted(timePayrollTransactionNetSalary);
                }
                else
                {
                    #region Deduction - SalaryDistress

                    if (payrollProductSalaryDistressAmount != null)
                    {
                        if (!employee.EmployeeTaxSE.IsLoaded)
                            employee.EmployeeTaxSE.Load();

                        EmployeeTaxSE employeeTaxSE = employee.EmployeeTaxSE.FirstOrDefault(i => i.Year == timePeriod.PaymentDate.Value.Year);
                        if (employeeTaxSE != null && employeeTaxSE.SalaryDistressAmountType != (int)TermGroup_EmployeeTaxSalaryDistressAmountType.NotSelected)
                        {
                            //Distress reserved amount (Förebehållsbelopp)
                            decimal distressReservedAmount = employeeTaxSE.SalaryDistressReservedAmount ?? 0;
                            if (distressReservedAmount > 0)
                            {
                                decimal calculatedDistressAmount = 0;

                                if (netSalaryAmount - distressReservedAmount > 0)
                                {
                                    //Distress amount (Utmätningsbelopp)
                                    if (employeeTaxSE.SalaryDistressAmountType == (int)TermGroup_EmployeeTaxSalaryDistressAmountType.FixedAmount)
                                    {
                                        //Fixed amount, but not less than reserved amount
                                        if (employeeTaxSE.SalaryDistressAmount.HasValue && employeeTaxSE.SalaryDistressAmount.Value > 0)
                                        {
                                            calculatedDistressAmount = employeeTaxSE.SalaryDistressAmount.Value;
                                            if ((netSalaryAmount - calculatedDistressAmount) < distressReservedAmount)
                                                calculatedDistressAmount = netSalaryAmount - distressReservedAmount;
                                        }
                                    }
                                    else if (employeeTaxSE.SalaryDistressAmountType == (int)TermGroup_EmployeeTaxSalaryDistressAmountType.AllSalary)
                                    {
                                        //All salary above reserved amount
                                        calculatedDistressAmount = netSalaryAmount - distressReservedAmount;
                                    }
                                }

                                //Round to even int (down)
                                calculatedDistressAmount = Decimal.Floor(calculatedDistressAmount);

                                //Cannot be negative
                                if (calculatedDistressAmount < 0)
                                    calculatedDistressAmount = 0;

                                //Deduct from net salary
                                timePayrollTransactionNetSalary.Amount -= calculatedDistressAmount;

                                //Negate
                                calculatedDistressAmount = Decimal.Negate(calculatedDistressAmount);

                                //Create distress amount transaction
                                CreateOrUpdateTimePayrollTransaction(payrollProductSalaryDistressAmount, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, attestStateId, 1, calculatedDistressAmount, calculatedDistressAmount, 0, String.Empty, timePayrollTransactionsForPeriod);
                            }
                        }
                    }

                    #endregion

                    #region Rounding

                    PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProductNetSalary, employee, lastTimeBlockDateInPeriod);
                    if (payrollProductSetting != null && (TermGroup_PayrollProductCentRoundingType)payrollProductSetting.CentRoundingType != TermGroup_PayrollProductCentRoundingType.None && timePayrollTransactionNetSalary.Amount.HasValue)
                    {
                        decimal amount = timePayrollTransactionNetSalary.Amount.Value;
                        decimal roundedAmount = this.Round(amount, (TermGroup_PayrollProductCentRoundingType)payrollProductSetting.CentRoundingType, (TermGroup_PayrollProductCentRoundingLevel)payrollProductSetting.CentRoundingLevel);
                        decimal difference = roundedAmount - amount;
                        if (difference != 0)
                        {
                            TimePayrollTransaction timePayrollTransaction = CreateTimePayrollTransaction(payrollProductNetSalaryRound, lastTimeBlockDateInPeriod, 1, difference, 0, difference, String.Empty, attestState.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId);

                            //Accounting
                            ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, lastTimeBlockDateInPeriod.Date, payrollProductNetSalaryRound);

                            timePayrollTransaction.IsCentRounding = true;

                            //Add the difference to the net salary transaction
                            timePayrollTransactionNetSalary.Amount += difference;
                            timePayrollTransactionNetSalary.UnitPrice = timePayrollTransactionNetSalary.Amount;

                        }
                    }

                    #endregion
                }
                ActionResult result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.NetSalary, timePayrollTransactionNetSalary.Amount ?? 0, false);
                if (!result.Success)
                    return result;
            }

            return Save();
        }

        #endregion

        #region QualifyingDeduction

        private ActionResult CreateQualifyingDeductionTransactions(TimeEngineTemplate template, List<ApplyAbsenceDayBase> allAbsenceDays)
        {
            if (!template.Outcome.TimePayrollTransactions.Any(t => t.IsAbsenceSickOrWorkInjury()))
                return new ActionResult(true);

            ActionResult result = new ActionResult();

            #region Prereq

            var (absenceDays, absenceSickIwhStandbyDays) = ApplyAbsenceDayBase.Parse(allAbsenceDays);

            ApplyAbsenceDay absenceDay = GetAbsenceDay(absenceDays);
            PayrollProduct qualifyingDeductionProduct = GetPayrollProductQualifyingDeduction();
            if (!DoEvaluateQualifyingDeduction(template, absenceDay, qualifyingDeductionProduct))
                return result;

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(template.EmployeeId, getHidden: false);
            if (employee == null)
                return result;

            Employment employment = employee.GetEmployment(template.Date);
            if (employment == null)
                return null;

            SicknessPeriod sicknessPeriod = GetSicknessPeriod(template, employee, absenceDay, absenceSickIwhStandbyDays);
            if (sicknessPeriod == null)
                return result;

            List<QualifyingDeductionPeriod> qualifyingDeductionPeriods = sicknessPeriod.GetPeriods(template.Date);
            if (qualifyingDeductionPeriods.IsNullOrEmpty())
                return result;

            #endregion

            #region Create transactions

            if (qualifyingDeductionProduct != null)
            {
                foreach (QualifyingDeductionPeriod qualifyingDeductionPeriod in qualifyingDeductionPeriods)
                {
                    if (qualifyingDeductionPeriod.SicknessSalaryTimeCodeTransaction == null || qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction == null)
                        continue;
                    if (qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction?.TimeBlock?.State == (int)SoeEntityState.Deleted)
                        continue;

                    int quantity = qualifyingDeductionPeriod.Length;
                    decimal amount = 0, vatAmount = 0, unitPrice = 0;
                    string comment = "";

                    TimeCodeTransaction newTimeCodeTransaction = CopyTimeCodeTransaction(qualifyingDeductionPeriod.SicknessSalaryTimeCodeTransaction, qualifyingDeductionPeriod.StartTime, qualifyingDeductionPeriod.StopTime, quantity);
                    if (newTimeCodeTransaction == null)
                        continue;

                    TimePayrollTransaction timePayrollTransaction = CreateTimePayrollTransaction(qualifyingDeductionProduct, template.Identity.TimeBlockDate, quantity, amount, vatAmount, unitPrice, comment, qualifyingDeductionPeriod.AttestStateId, qualifyingDeductionPeriod.TimePeriodId, qualifyingDeductionPeriod.EmployeeId);
                    if (timePayrollTransaction == null)
                        continue;

                    timePayrollTransaction.TimeCodeTransaction = newTimeCodeTransaction;

                    // Connect to same TimeBlock
                    if (qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction.TimeBlockId.HasValue && qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction.TimeBlockId > 0)
                        timePayrollTransaction.TimeBlockId = qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction.TimeBlockId;
                    else if (base.IsEntityAvailableInContext(entities, qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction.TimeBlock))
                        timePayrollTransaction.TimeBlock = qualifyingDeductionPeriod.SicknessSalaryTimePayrollTransaction.TimeBlock;

                    CreateTimePayrollTransactionExtended(timePayrollTransaction, timePayrollTransaction.EmployeeId, actorCompanyId);
                    SetTimePayrollTransactionQuantity(timePayrollTransaction, qualifyingDeductionProduct, employment, template.Identity.TimeBlockDate, timePayrollTransaction.Quantity);

                    // Accounting
                    ApplyAccountingOnTimePayrollTransaction(timePayrollTransaction, employee, template.Date, qualifyingDeductionProduct, newTimeCodeTransaction.TimeBlock);

                    template.Outcome.TimeCodeTransactions.Add(newTimeCodeTransaction);
                    template.Outcome.TimePayrollTransactions.Add(timePayrollTransaction);
                }
            }

            #endregion

            #region Turn iwh time during qualifying deduction period

            if (sicknessPeriod.TryGetPeriodTimes(template.Date, out DateTime? startTime, out DateTime? stopTime))
            {
                List<TimeCodeTransaction> newTimeCodeTransactions = new List<TimeCodeTransaction>();
                List<TimeCodeTransaction> sickDuringIwhTimeCodeTransactions = template.Outcome.TimeCodeTransactions.Where(i => i.IsSickDuringIwhOrStandbyTransaction).OrderBy(i => i.Start).ToList();
                foreach (var originalTimeCodeTransaction in sickDuringIwhTimeCodeTransactions)
                {
                    if (template.Outcome.UseStandby && sicknessPeriod.HasNoneQualifyingStandbyInterval(template.Date, originalTimeCodeTransaction.Start, originalTimeCodeTransaction.Stop))
                        continue;

                    if (CalendarUtility.GetOverlappingDates(originalTimeCodeTransaction.Start, originalTimeCodeTransaction.Stop, startTime.Value, stopTime.Value, out DateTime newStart, out DateTime newStop))
                    {
                        decimal quantity = (int)newStop.Subtract(newStart).TotalMinutes;

                        TimeCodeTransaction newTimeCodeTransaction = CopyTimeCodeTransaction(originalTimeCodeTransaction, newStart, newStop, quantity);
                        if (newTimeCodeTransaction == null)
                            continue;

                        newTimeCodeTransaction.Quantity = decimal.Negate(quantity);
                        newTimeCodeTransactions.Add(newTimeCodeTransaction);

                        List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
                        foreach (TimePayrollTransaction timePayrollTransaction in originalTimeCodeTransaction.TimePayrollTransaction)
                        {
                            TimePayrollTransaction newTransaction = CopyTimePayrollTransaction(timePayrollTransaction, timePayrollTransaction.TimeBlockDate, timePayrollTransaction.TimeBlockId, employment, setAccounting: true);
                            if (newTransaction == null)
                                continue;

                            //Set properties that is not set in CopyTimePayrollTransaction                            
                            newTransaction.TimePeriodId = timePayrollTransaction.TimePeriodId;
                            newTransaction.TimeBlockDateId = timePayrollTransaction.TimeBlockDateId;
                            newTransaction.Quantity = decimal.Negate(quantity);
                            newTransaction.TimeCodeTransaction = newTimeCodeTransaction;
                            newTransactions.Add(newTransaction);

                        }
                        template.Outcome.TimePayrollTransactions.AddRange(newTransactions);
                    }
                }
                template.Outcome.TimeCodeTransactions.AddRange(newTimeCodeTransactions);
            }

            #endregion

            result = Save();

            return result;
        }
        private ActionResult CreateQualifyingDeductionTransactions(TimeEngineTemplate template, List<ApplyAbsenceDayBase> applyAbsenceDays, List<TimeTransactionItem> timeTransactionsItemsForDay)
        {
            if (!template.Outcome.TimePayrollTransactions.Any(t => t.IsAbsenceSickOrWorkInjury()))
                return new ActionResult(true);

            ActionResult result = new ActionResult();

            #region Prereq

            List<ApplyAbsenceDay> absenceDays = new List<ApplyAbsenceDay>();
            List<ApplyAbsenceSickIwhStandbyDay> absenceSickIwhStandbyDays = new List<ApplyAbsenceSickIwhStandbyDay>();
            foreach (ApplyAbsenceDayBase day in applyAbsenceDays)
            {
                if (day is ApplyAbsenceDay applyAbsenceDay)
                    absenceDays.Add(applyAbsenceDay);                
                else if (day is ApplyAbsenceSickIwhStandbyDay applyAbsenceSickIwhStandbyDay)
                    absenceSickIwhStandbyDays.Add(applyAbsenceSickIwhStandbyDay);
            }

            ApplyAbsenceDay absenceDay = GetAbsenceDay(absenceDays);
            PayrollProduct qualifyingDeductionProduct = GetPayrollProductQualifyingDeduction();
            if (!DoEvaluateQualifyingDeduction(template, absenceDay, qualifyingDeductionProduct, timeTransactionsItemsForDay))
                return result;

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(template.EmployeeId, getHidden: false);
            if (employee == null)
                return result;

            Employment employment = employee.GetEmployment(template.Date);
            if (employment == null)
                return null;

            SicknessPeriod sicknessPeriod = GetSicknessPeriod(template, employee, absenceDay, absenceSickIwhStandbyDays, timeTransactionsItemsForDay);
            if (sicknessPeriod == null)
                return result;

            List<QualifyingDeductionPeriod> qualifyingDeductionPeriods = sicknessPeriod.GetPeriods(template.Date);
            if (qualifyingDeductionPeriods.IsNullOrEmpty())
                return result;

            #endregion

            #region Create transactions

            if (qualifyingDeductionProduct != null)
            {
                foreach (var qualifyingDeductionPeriod in qualifyingDeductionPeriods)
                {
                    if (qualifyingDeductionPeriod.SicknessSalaryTimeTransactionItem == null)
                        continue;

                    TimeTransactionItem sicknessSalaryTimeTransactionItem = qualifyingDeductionPeriod.SicknessSalaryTimeTransactionItem;
                    if (sicknessSalaryTimeTransactionItem == null)
                        continue;

                    TimeCodeTransaction sicknessSalaryTimeCodeTransaction = template.Outcome.TimeCodeTransactions.FirstOrDefault(x => x.Guid == sicknessSalaryTimeTransactionItem.GuidInternalFK);
                    if (sicknessSalaryTimeCodeTransaction == null)
                        continue;

                    int quantity = qualifyingDeductionPeriod.Length;
                    TimeCode timeCode = GetTimeCodeFromCache(sicknessSalaryTimeCodeTransaction.TimeCodeId);
                    TimeBlock timeBlock = template?.Identity?.TimeBlocks?.FirstOrDefault(i => i.GuidId == sicknessSalaryTimeCodeTransaction.GuidTimeBlock);
                    AttestStateDTO attestState = GetAttestStateFromCache(sicknessSalaryTimeTransactionItem.AttestStateId);

                    TimeCodeTransaction newTimeCodeTransaction = CopyTimeCodeTransaction(sicknessSalaryTimeCodeTransaction, qualifyingDeductionPeriod.StartTime, qualifyingDeductionPeriod.StopTime, quantity);
                    if (newTimeCodeTransaction == null)
                        continue;
                    if (newTimeCodeTransaction.TimeBlock == null)
                        newTimeCodeTransaction.TimeBlock = timeBlock;

                    TimeTransactionItem timePayrollTransaction = CreateTimeTransactionItem(qualifyingDeductionProduct, employee, timeCode, template.Identity.TimeBlockDate, attestState, quantity, newTimeCodeTransaction.Guid, newTimeCodeTransaction.GuidTimeBlock, timeBlock, SoeTimeTransactionType.TimePayroll, setAccounting: true);
                    if (timePayrollTransaction == null)
                        continue;

                    template.Outcome.TimeCodeTransactions.Add(newTimeCodeTransaction);
                    timeTransactionsItemsForDay.Add(timePayrollTransaction);
                }
            }

            #endregion

            #region Turn iwh time during qualifying deduction period

            if (sicknessPeriod.TryGetPeriodTimes(template.Date, out DateTime? startTime, out DateTime? stopTime))
            {
                List<TimeCodeTransaction> sickDuringIwhOrStandbyTimeCodeTransactions = template.Outcome.TimeCodeTransactions.Where(i => i.IsSickDuringIwhOrStandbyTransaction).OrderBy(i => i.Start).ToList();
                foreach (TimeCodeTransaction originalTimeCodeTransaction in sickDuringIwhOrStandbyTimeCodeTransactions)
                {
                    if (!originalTimeCodeTransaction.Guid.HasValue)
                        continue;
                    if (template.Outcome.UseStandby && sicknessPeriod.HasNoneQualifyingStandbyInterval(template.Date, startTime.Value, stopTime.Value))
                        continue;

                    TimeBlock timeBlock = template?.Identity?.TimeBlocks?.FirstOrDefault(i => i.GuidId == originalTimeCodeTransaction.GuidTimeBlock);

                    if (CalendarUtility.GetOverlappingDates(originalTimeCodeTransaction.Start, originalTimeCodeTransaction.Stop, startTime.Value, stopTime.Value, out DateTime newStart, out DateTime newStop))
                    {
                        decimal quantity = (int)newStop.Subtract(newStart).TotalMinutes;

                        TimeCodeTransaction newTimeCodeTransaction = CopyTimeCodeTransaction(originalTimeCodeTransaction, newStart, newStop, quantity);
                        if (newTimeCodeTransaction == null)
                            continue;
                        if (newTimeCodeTransaction.TimeBlock == null)
                            newTimeCodeTransaction.TimeBlock = timeBlock;

                        template.Outcome.TimeCodeTransactions.Add(newTimeCodeTransaction);

                        List<TimeTransactionItem> timeTransactionsItems = timeTransactionsItemsForDay.Where(i => i.GuidInternalFK.HasValue && i.GuidInternalFK.Value == originalTimeCodeTransaction.Guid.Value).ToList();
                        foreach (TimeTransactionItem timeTransactionsItem in timeTransactionsItems)
                        {
                            TimeTransactionItem newTransactionItem = CopyTimeTransactionItem(timeTransactionsItem, timeTransactionsItem.TimeBlockId, setAccounting: true);
                            if (newTransactionItem == null)
                                continue;

                            //Connect to timeblock
                            newTransactionItem.GuidTimeBlockFK = timeTransactionsItem.GuidTimeBlockFK;
                            if (timeBlock != null)
                                newTransactionItem.TimeBlockId = timeBlock.TimeBlockId;

                            //Connect to timecodetransaction
                            newTransactionItem.GuidInternalFK = newTimeCodeTransaction.Guid;
                            newTransactionItem.Quantity = decimal.Negate(quantity);

                            timeTransactionsItemsForDay.Add(newTransactionItem);
                        }
                    }
                }
            }

            #endregion

            return result;
        }
        private ApplyAbsenceDay GetAbsenceDay(List<ApplyAbsenceDay> absenceDays)
        {
            return absenceDays?.FirstOrDefault(a =>
                a.AbsenceDayNumber > 0 ||
                (a.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Sick || a.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_WorkInjury));
        }

        #endregion

        #region PayrollProductChain

        #region TimePayrollTransaction

        private ActionResult CreateTransactionsFromPayrollProductChain(TimeEngineTemplate template)
        {
            if (!UsePayroll())
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(template.EmployeeId, getHidden: false);
            if (employee == null)
                return new ActionResult(true);

            int? payrollGroupId = employee.GetPayrollGroupId(template.Date);
            if (!payrollGroupId.HasValue)
                return new ActionResult(true);

            List<TimePayrollTransaction> childTransactions = new List<TimePayrollTransaction>();
            bool? hasEmployeeRightToSicknessSalary = null;
            bool saveNeeded = false;
            var transactionsGroupedByProduct = template.Outcome.TimePayrollTransactions.GroupBy(x => x.ProductId).ToList();
            foreach (var transactionGroup in transactionsGroupedByProduct)
            {
                List<PayrollProduct> payrollProductChain = new List<PayrollProduct>();

                int productId = transactionGroup.Key;
                ActionResult result = GetPayrollProductChain(productId, payrollGroupId.Value, payrollProductChain);
                if (!result.Success)// the chain is wrongly configured
                    return new ActionResult(true);

                if (payrollProductChain.Any())
                {
                    saveNeeded = true;
                    payrollProductChain.RemoveAt(0); //the first one is the parent
                    if (!hasEmployeeRightToSicknessSalary.HasValue)
                        hasEmployeeRightToSicknessSalary = HasEmployeeRightToSicknessSalaryFromCache(employee, template.Date);

                    foreach (TimePayrollTransaction parent in transactionGroup.ToList())
                    {
                        childTransactions.AddRange(CreateChildTransactions(parent, payrollProductChain, employee, hasEmployeeRightToSicknessSalary.Value));
                    }
                }
            }

            template.Outcome.TimePayrollTransactions.AddRange(childTransactions);

            return saveNeeded ? Save() : new ActionResult(true);
        }
        private ActionResult CreateTransactionsFromPayrollProductChain(TimePayrollTransaction timePayrollTransaction, Employee employee, TimeBlockDate timeBlockDate, out List<TimePayrollTransaction> childTransactions)
        {
            childTransactions = new List<TimePayrollTransaction>();

            if (!UsePayroll())
                return new ActionResult(true);

            int? payrollGroupId = employee.GetPayrollGroupId(timeBlockDate.Date);
            if (!payrollGroupId.HasValue)
                return new ActionResult(true);

            List<PayrollProduct> payrollProductChain = new List<PayrollProduct>();

            ActionResult result = GetPayrollProductChain(timePayrollTransaction.ProductId, payrollGroupId.Value, payrollProductChain);

            // Check if the chain is wrongly configured
            if (!result.Success)
                return new ActionResult(true);

            // The first one is the parent
            if (payrollProductChain.Any())
                payrollProductChain.RemoveAt(0);

            childTransactions.AddRange(CreateChildTransactions(timePayrollTransaction, payrollProductChain, employee));

            result = Save();

            return result;
        }
        private List<TimePayrollTransaction> CreateChildTransactions(TimePayrollTransaction firstParent, List<PayrollProduct> payrollProductChain, Employee employee, bool hasEmployeeRightToSicknessSalary = true)
        {
            List<TimePayrollTransaction> timePayrollTransactions = new List<TimePayrollTransaction>();

            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            firstParent.IncludedInPayrollProductChain = payrollProductChain.Any();

            TimePayrollTransaction newParent = null;
            foreach (PayrollProduct product in payrollProductChain)
            {
                if (!hasEmployeeRightToSicknessSalary && product.IsAbsence_SicknessSalary())
                    continue;

                newParent = CreateTimePayrollTransactionFromParent(product, newParent ?? firstParent, employee, attestStateInitial.AttestStateId, true);
                if (newParent != null)
                    timePayrollTransactions.Add(newParent);
            }

            return timePayrollTransactions;
        }

        #endregion

        #region TimeTransactionItem

        private ActionResult CreateTransactionsFromPayrollProductChain(List<TimeTransactionItem> transactionItems, int employeeId, TimeBlockDate timeBlockDate)
        {
            if (!UsePayroll())
                return new ActionResult(true);

            ActionResult result = new ActionResult();

            if (timeBlockDate == null)
                return new ActionResult(true);

            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
            if (employee == null)
                return new ActionResult(true);

            int? payrollGroupId = employee.GetPayrollGroupId(timeBlockDate.Date);
            if (!payrollGroupId.HasValue)
                return new ActionResult(true);

            var transactionsGroupedByProduct = transactionItems.Where(x => x.TransactionType == SoeTimeTransactionType.TimePayroll).GroupBy(x => x.ProductId).ToList();
            List<TimeTransactionItem> childTransactions = new List<TimeTransactionItem>();

            foreach (var transactionGroup in transactionsGroupedByProduct)
            {
                List<PayrollProduct> payrollProductChain = new List<PayrollProduct>();

                int productId = transactionGroup.Key;
                result = GetPayrollProductChain(productId, payrollGroupId.Value, payrollProductChain);
                if (!result.Success)// the chain is wrongly configured
                    return new ActionResult(true);

                if (payrollProductChain.Any())
                    payrollProductChain.RemoveAt(0); //the first one is the parent

                foreach (TimeTransactionItem parent in transactionGroup.ToList())
                {
                    childTransactions.AddRange(CreateChildTransactions(parent, payrollProductChain, employee, timeBlockDate));
                }
            }

            transactionItems.AddRange(childTransactions);

            return result;
        }
        private List<TimeTransactionItem> CreateChildTransactions(TimeTransactionItem firstParent, List<PayrollProduct> payrollProductChain, Employee employee, TimeBlockDate timeBlockDate)
        {
            List<TimeTransactionItem> timeTimeTransactionItems = new List<TimeTransactionItem>();

            if (firstParent != null && payrollProductChain != null)
            {
                firstParent.IncludedInPayrollProductChain = payrollProductChain.Any();

                TimeTransactionItem newParent = null;
                foreach (var product in payrollProductChain)
                {
                    newParent = CreateTimeTransactionItemFromParent(product, newParent ?? firstParent, employee, timeBlockDate, true);
                    timeTimeTransactionItems.Add(newParent);
                }
            }

            return timeTimeTransactionItems;
        }
        private void SetPayrollProductChainParent(List<AttestPayrollTransactionDTO> transactions, List<Tuple<string, TimePayrollTransaction>> payrollTransactionMapping)
        {
            foreach (var childTransactionItem in transactions.Where(x => x.IncludedInPayrollProductChain && !string.IsNullOrEmpty(x.ParentGuidId) && !string.IsNullOrEmpty(x.GuidId)))
            {
                var parentTimeTransactionItem = transactions.FirstOrDefault(x => !string.IsNullOrEmpty(x.GuidId) && x.GuidId == childTransactionItem.ParentGuidId);
                if (parentTimeTransactionItem != null)
                {
                    var childTuple = payrollTransactionMapping.FirstOrDefault(x => x.Item1 == childTransactionItem.GuidId);
                    if (childTuple != null)
                    {
                        var parentTuple = payrollTransactionMapping.FirstOrDefault(x => x.Item1 == parentTimeTransactionItem.GuidId);
                        if (parentTuple != null)
                            childTuple.Item2.Parent = parentTuple.Item2;
                    }
                }
            }
        }
        private void SetPayrollProductChainParent(List<TimeTransactionItem> timeTransactionItems, List<Tuple<Guid, TimePayrollTransaction>> payrollTransactionMapping)
        {
            foreach (var childTransactionItem in timeTransactionItems.Where(x => !x.ManuallyAdded && x.IncludedInPayrollProductChain && x.ParentGuidId.HasValue && x.GuidId.HasValue))
            {
                var parentTimeTransactionItem = timeTransactionItems.FirstOrDefault(x => x.GuidId.HasValue && x.GuidId.Value == childTransactionItem.ParentGuidId.Value);
                if (parentTimeTransactionItem != null)
                {
                    var childTuple = payrollTransactionMapping.FirstOrDefault(x => x.Item1 == childTransactionItem.GuidId.Value);
                    if (childTuple != null)
                    {
                        var parentTuple = payrollTransactionMapping.FirstOrDefault(x => x.Item1 == parentTimeTransactionItem.GuidId.Value);
                        if (parentTuple != null)
                            childTuple.Item2.Parent = parentTuple.Item2;
                    }
                }
            }
        }
        private void SetPayrollProductChainParent(List<TimeTransactionItem> timeTransactionItems, List<Tuple<Guid, TimePayrollScheduleTransaction>> scheduleTransactionMapping)
        {
            foreach (var childTransactionItem in timeTransactionItems.Where(x => !x.ManuallyAdded && x.IncludedInPayrollProductChain && x.ParentGuidId.HasValue && x.GuidId.HasValue))
            {
                var parentTimeTransactionItem = timeTransactionItems.FirstOrDefault(x => x.GuidId.HasValue && x.GuidId.Value == childTransactionItem.ParentGuidId.Value);
                if (parentTimeTransactionItem != null)
                {
                    var childTuple = scheduleTransactionMapping.FirstOrDefault(x => x.Item1 == childTransactionItem.GuidId.Value);
                    if (childTuple != null)
                    {
                        var parentTuple = scheduleTransactionMapping.FirstOrDefault(x => x.Item1 == parentTimeTransactionItem.GuidId.Value);
                        if (parentTuple != null)
                            childTuple.Item2.Parent = parentTuple.Item2;
                    }
                }
            }
        }

        #endregion

        private ActionResult GetPayrollProductChain(int payrollProductId, int? payrollGroupId, List<PayrollProduct> payrollProductChain)
        {
            var payrollProductWithSettings = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(payrollProductId);
            if (payrollProductWithSettings != null)
                payrollProductChain.Add(payrollProductWithSettings);
            else
                return new ActionResult(false);

            PayrollProductSetting payrollProductSetting = null;
            if (payrollGroupId.HasValue)
                payrollProductSetting = payrollProductWithSettings.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && i.PayrollGroupId.HasValue && i.PayrollGroupId.Value == payrollGroupId.Value);
            if (payrollProductSetting == null)
                payrollProductSetting = payrollProductWithSettings.PayrollProductSetting.FirstOrDefault(i => i.State == (int)SoeEntityState.Active && !i.PayrollGroupId.HasValue);

            if (payrollProductSetting != null && payrollProductSetting.ChildProductId.HasValue)
            {
                if (payrollProductChain.Any(x => x.ProductId == payrollProductSetting.ChildProductId.Value))
                {
                    payrollProductWithSettings = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(payrollProductSetting.ChildProductId.Value);
                    payrollProductChain.Add(payrollProductWithSettings);//need for the error message
                                                                        //The payrollproduct chain is inifinte, it is wrongly configured!
                    return new ActionResult(false);
                }
                else
                {
                    var result = GetPayrollProductChain(payrollProductSetting.ChildProductId.Value, payrollGroupId, payrollProductChain);
                    if (!result.Success)
                        return result;
                }
            }

            return new ActionResult();
        }

        #endregion

        #region Recalculate period

        private ReCalculatePayrollPeriodSysPayrollPriceDTO SetUpReCalculatePayrollPeriodSysPayrollPriceDTO(ReCalculatePayrollPeriodCompanyDTO companyDTO, TimePeriod timePeriod)
        {
            return new ReCalculatePayrollPeriodSysPayrollPriceDTO()
            {
                BaseAmount = PayrollManager.GetBaseAmountSE(entities, actorCompanyId, timePeriod.PaymentDate.Value, companyDTO.SysCountryId),
                SysPayrollPriceVacationDayPercent = PayrollManager.GetSysPayrollPriceAmount(entities, actorCompanyId, (int)TermGroup_SysPayrollPrice.SE_Vacation_VacationDayPercent, timePeriod.PaymentDate.Value, companyDTO.SysCountryId)
            };
        }
        private ActionResult ReCalculatePayrollPeriod(ReCalculatePayrollPeriodCompanyDTO companyDTO, List<int> employeeIds, int timePeriodId, bool ignoreEmploymentHasEnded, bool includeScheduleTransactions, bool doLogProgressInfo, out string baseMessage, out string errorMessage)
        {
            baseMessage = string.Empty;
            errorMessage = string.Empty;

            TimePeriod timePeriod = GetTimePeriodFromCache(timePeriodId);
            if (timePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingTimePeriodPaymentDate);

            ActionResult result = new ActionResult();
            StringBuilder errorMessageBuilder = new StringBuilder();

            try
            {
                ReCalculatePayrollPeriodSysPayrollPriceDTO sysPayrollPriceDTO = SetUpReCalculatePayrollPeriodSysPayrollPriceDTO(companyDTO, timePeriod);
                companyDTO.HolidaySalaryHolidaysCurrentPeriod = GetHolidaySalaryHolidays(timePeriod.StartDate, timePeriod.StopDate);
                bool continueIfNotSucces = employeeIds.Count > 1;
                int employeeCounter = 1;
                int subsequentErrorCounter = 0;

                foreach (int employeeId in employeeIds)
                {
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
                    if (employee == null)
                        continue;

                    if (doLogProgressInfo)
                        baseMessage = String.Format("Räknar om anställd {0} ({1} av {2})", employee.EmployeeNr, employeeCounter, employeeIds.Count);

                    result = ReCalculatePayrollPeriod(employee, timePeriod, ignoreEmploymentHasEnded, includeScheduleTransactions, companyDTO, sysPayrollPriceDTO);
                    if (!result.Success)
                    {
                        if (continueIfNotSucces)
                        {
                            errorMessageBuilder.Append(employee.EmployeeNr + ": " + ((string.IsNullOrEmpty(result.ErrorMessage)) ? GetText(8819, "Beräkning misslyckades") : result.ErrorMessage) + "\n");
                            if (subsequentErrorCounter < 9)
                            {
                                subsequentErrorCounter++;
                                continue;
                            }
                            else
                            {
                                errorMessageBuilder.Append(GetText(8818, "Löneberäkningen har avbrutits pga för många efterföljande fel.") + "\n");
                                return result;
                            }
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        subsequentErrorCounter = 0;
                    }

                    employeeCounter++;
                }
            }
            finally
            {
                errorMessage = errorMessageBuilder.ToString();
            }

            return result;
        }
        private ActionResult ReCalculatePayrollPeriod(Employee employee, TimePeriod timePeriod, bool ignoreEmploymentHasEnded, bool includeScheduleTransactions, ReCalculatePayrollPeriodCompanyDTO companyDTO, ReCalculatePayrollPeriodSysPayrollPriceDTO sysPayrollPriceDTO)
        {
            ActionResult result = new ActionResult();

            try
            {
                StartWatch("Employee", append: true);

                #region Prereq

                StartWatch("PrereqEmployee", append: true);

                if (IsEmployeeTimePeriodLockedForChanges(employee.EmployeeId, timePeriodId: timePeriod.TimePeriodId))
                    return new ActionResult((int)ActionResultSave.NothingSaved, GetText(91947, "Löneberäkning kunde inte genomföras. Perioden är inte öppen"));

                var vacationGroup = employee.GetVacationGroup(timePeriod.StartDate, timePeriod.StopDate, companyDTO.VacationGroups, forward: false);
                var vacationGroupSE = vacationGroup != null ? GetVacationGroupSEFromCache(vacationGroup.VacationGroupId) : null;
                bool createVacationCompensation = vacationGroup != null && vacationGroup.Type == (int)TermGroup_VacationGroupType.DirectPayment;
                bool applyEmploymentHasEnded = ignoreEmploymentHasEnded && (employee.GetEmployment(timePeriod.StartDate, timePeriod.StopDate) == null) && (employee.GetEmployment(timePeriod.PayrollStartDate.Value, timePeriod.PayrollStopDate.Value) == null);

                // Get EmployeeTaxSE for employee
                EmployeeTaxSEDTO empTax = EmployeeManager.GetEmployeeTaxSEDTO(entities, employee.EmployeeId, timePeriod.PaymentDate.Value.Year);
                if (empTax == null && !applyEmploymentHasEnded)
                    return new ActionResult((int)ActionResultSave.NothingSaved, string.Format(GetText(8721, "Skatteuppgifter för {0} saknas.\nDetta åtgärdas under Anställd - Skatter och Avgifter"), timePeriod.PaymentDate.Value.Year.ToString()));

                bool createSINKTaxInsteadOfTableTax = false;
                bool createASINKTaxInsteadOfASINKTax = false;
                PayrollProduct payrollProductSINKTax = null;
                PayrollProduct payrollProductASINKTax = null;
                if (empTax != null && empTax.Type == TermGroup_EmployeeTaxType.Sink && empTax.SinkType != (int)TermGroup_EmployeeTaxSinkType.NotSelected)
                {
                    if (empTax.SinkType == TermGroup_EmployeeTaxSinkType.Normal || empTax.SinkType == TermGroup_EmployeeTaxSinkType.NoTax)
                    {
                        createSINKTaxInsteadOfTableTax = true;
                        payrollProductSINKTax = GetPayrollProductSINKTax();
                        if (payrollProductSINKTax == null)
                            return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductSINKTax, GetText(9310, "Löneart för SINK saknas"));
                    }
                    else if (empTax.SinkType == TermGroup_EmployeeTaxSinkType.AthletsArtistSailors)
                    {
                        createASINKTaxInsteadOfASINKTax = true;
                        payrollProductASINKTax = GetPayrollProductASINKTax();
                        if (payrollProductASINKTax == null)
                            return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductASINKTax, GetText(9311, "Löneart för A-SINK saknas"));
                    }
                }

                StopWatch("PrereqEmployee");

                #endregion

                using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                {
                    InitTransaction(transaction);

                    #region EmployeeTimePeriod

                    StartWatch("EmployeeTimePeriod", append: true);

                    EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriodWithValueAndSettings(timePeriod.TimePeriodId, employee.EmployeeId);
                    if (employeeTimePeriod == null)
                    {
                        employeeTimePeriod = new EmployeeTimePeriod()
                        {
                            Status = (int)SoeEmployeeTimePeriodStatus.Open,

                            //Set FK
                            EmployeeId = employee.EmployeeId,
                            ActorCompanyId = actorCompanyId,

                            //References
                            TimePeriod = timePeriod,
                        };
                        entities.EmployeeTimePeriod.AddObject(employeeTimePeriod);
                        SetCreatedProperties(employeeTimePeriod);
                    }
                    else
                    {
                        SetModifiedProperties(employeeTimePeriod);
                    }

                    StopWatch("EmployeeTimePeriod");

                    #endregion

                    #region TimeBlockDates

                    StartWatch("TimeBlockDates", append: true);

                    List<TimeBlockDate> transactionDates = new List<TimeBlockDate>();
                    List<TimeBlockDate> timeBlockDatesInPeriod = GetTimeBlockDatesForPeriod(employee.EmployeeId, timePeriod.TimePeriodId);
                    DateTime? fallbackEmploymentValidationDate = null;

                    int days = timePeriod.StopDate.Subtract(timePeriod.StartDate).Days + 1;
                    DateTime date = timePeriod.StartDate.Date;
                    for (int day = 1; day <= days; day++)
                    {
                        if (timeBlockDatesInPeriod.Any(x => x.Date.Date == date))
                        {
                            date = date.AddDays(1);
                            continue;
                        }

                        TimeBlockDate timeBlockDate = CreateTimeBlockDate(employee.EmployeeId, date);
                        if (timeBlockDate != null)
                            timeBlockDatesInPeriod.Add(timeBlockDate);

                        date = date.AddDays(1);
                    }

                    result = Save();
                    if (!result.Success)
                        return result;

                    List<TimeBlockDate> timeBlockDatesInEmployment = employee.GetTimeBlockDatesInEmployment(timeBlockDatesInPeriod);
                    if (!timeBlockDatesInEmployment.Any())
                    {
                        if (applyEmploymentHasEnded)
                        {
                            timeBlockDatesInEmployment.Add(timeBlockDatesInPeriod.OrderBy(x => x.Date).LastOrDefault());
                        }
                        else if (timePeriod.PayrollStopDate.HasValue && timePeriod.PayrollStopDate.Value != timePeriod.StopDate)// Fix: innevarande månad problem (anställningen börjar i augusti och man ska ha lön i augusti men avräkningen är juli)
                        {
                            fallbackEmploymentValidationDate = timePeriod.PayrollStopDate;
                            timeBlockDatesInEmployment.Add(timeBlockDatesInPeriod.OrderBy(x => x.Date).LastOrDefault());
                        }
                        else
                        {
                            timeBlockDatesInEmployment.Add(timeBlockDatesInPeriod.OrderBy(x => x.Date).LastOrDefault());
                        }
                    }

                    TimeBlockDate lastTimeBlockDateInPeriod = timeBlockDatesInEmployment.OrderBy(x => x.Date).LastOrDefault();

                    StopWatch("TimeBlockDates");

                    #endregion

                    #region Get existing Period transactions

                    StartWatch("GetExistingPeriodTransactions", "hämtar transaktioner", append: true);

                    List<TimePayrollTransaction> reusableTransactionsForPeriod = GetTimePayrollTransactionsWithAccountInternals(employee.EmployeeId, timePeriod.TimePeriodId).GetPayrollCalculationTransactions(true);

                    StopWatch("GetExistingPeriodTransactions");

                    #endregion

                    #region Check locked period

                    result = ValidatePayrollLockedAttestStates(reusableTransactionsForPeriod);
                    if (!result.Success)
                    {
                        result.ErrorMessage = GetText(8690, "Du måste öppna upp perioden för att kunna göra en omräkning");
                        return result;
                    }

                    #endregion

                    #region Delete transactions

                    StartWatch("DeleteTransactions", "tar bort transaktioner", append: true);

                    result = SetTimePayrollTransactionsForRecalculateToDeleted(reusableTransactionsForPeriod);
                    if (!result.Success)
                        return result;

                    StopWatch("DeleteTransactions");

                    #endregion

                    #region Recreate Added Template Transactions

                    StartWatch("AddedTemplateTransactions", "kontrollerar massregistrering", append: true);

                    result = ReCreateAddedTransactionsFromTemplate(employee.EmployeeId, timePeriod, reusableTransactionsForPeriod);
                    if (!result.Success)
                        return result;

                    reusableTransactionsForPeriod = reusableTransactionsForPeriod.GetPayrollCalculationTransactions(false);

                    StopWatch("AddedTemplateTransactions");

                    #endregion

                    #region Get a fresh batch of existing transactions

                    StartWatch("GetExistingTransactions", "hämtar transaktioner", append: true);

                    List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(employee.EmployeeId, timePeriod.StartDate, timePeriod.StopDate, timePeriod).Where(x => !x.PayrollStartValueRowId.HasValue).ToList();

                    //Decide transactiondates
                    transactionDates.AddRange(timeBlockDatesInEmployment);
                    var timeBlockDatesForPeriodTransactions = GetTimeBlockDates(employee.EmployeeId, timePayrollTransactions.Where(x => x.TimePeriodId.HasValue).Select(x => x.TimeBlockDateId).ToList());
                    foreach (var timeBlockDate in timeBlockDatesForPeriodTransactions)
                    {
                        if (!transactionDates.Any(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId))
                            transactionDates.Add(timeBlockDate);
                    }

                    StopWatch("GetExistingTransactions");

                    #endregion

                    #region Schedule transactions

                    List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = new List<TimePayrollScheduleTransaction>();
                    if (!timePeriod.ExtraPeriod)
                    {
                        StartWatch("SaveTimePayrollScheduleTransactions", "beräknar schematransaktioner", append: true);

                        if (includeScheduleTransactions)
                        {
                            //Create scheduletransactions and calculate amounts
                            result = SaveTimePayrollScheduleTransactions(employee, timeBlockDatesInEmployment, timePeriod, createTaxDebet: true, createSupplementCharge: true);
                            if (!result.Success)
                                return result;

                        }
                        else
                        {
                            #region Handle only absence scheduletransactions

                            PayrollProduct payrollProductSupplementChargeDebet = GetPayrollProductSupplementChargeDebet();
                            if (payrollProductSupplementChargeDebet == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, "payrollProductSupplementChargeDebet");

                            //Get all absence transactions
                            List<TimePayrollScheduleTransaction> existingAbsenceScheduleTransactions = GetTimePayrollScheduleTransactionsWithAccountingAndDim(employee.EmployeeId, timeBlockDatesInEmployment.OrderBy(i => i.Date).First().Date, timeBlockDatesInEmployment.OrderByDescending(i => i.Date).First().Date, timePeriod.TimePeriodId, SoeTimePayrollScheduleTransactionType.Absence);

                            //Delete employment tax - they will be recalculated
                            result = SetTimePayrollScheduleTransactionsToDeleted(existingAbsenceScheduleTransactions.Where(x => x.IsEmploymentTaxDebit()).ToList(), saveChanges: true, excludeEmploymentTaxAndSupplementCharge: false);
                            if (!result.Success)
                                return result;

                            //Delete supplementcharge- they will be recalculated
                            result = SetTimePayrollScheduleTransactionsToDeleted(existingAbsenceScheduleTransactions.Where(x => x.IsSupplementChargeDebit()).ToList(), saveChanges: true, excludeEmploymentTaxAndSupplementCharge: false);
                            if (!result.Success)
                                return result;

                            var timeBlockDatesForPeriodScheduleTransactions = GetTimeBlockDates(employee.EmployeeId, existingAbsenceScheduleTransactions.Where(x => x.TimePeriodId.HasValue && !x.IsEmploymentTaxDebit() && !x.IsSupplementChargeDebit()).Select(x => x.TimeBlockDateId).ToList());
                            foreach (var timeBlockDate in timeBlockDatesForPeriodScheduleTransactions)
                            {
                                if (!transactionDates.Any(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId))
                                    transactionDates.Add(timeBlockDate);
                            }

                            //Calculate amounts and recreate employment tax and supplementcharge
                            foreach (TimeBlockDate timeBlockDate in transactionDates.OrderBy(i => i.Date))
                            {
                                List<TimePayrollScheduleTransaction> transactions = existingAbsenceScheduleTransactions.Where(i => !i.IsEmploymentTaxDebit() && !i.IsSupplementChargeDebit() && i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();

                                //Set amounts                                 
                                SetTimePayrollScheduleTransactionAmounts(transactions.Where(i => !i.IsRetroTransaction()).ToList(), employee, timeBlockDate);

                                #region Save

                                result = Save();
                                if (!result.Success)
                                    return result;

                                #endregion

                                #region EmploymentTax (handled later, together with payrollTransactions)

                                //var newTransactions = CreateEmploymentTaxDebetScheduleTransactions(employee, timePeriod, timeBlockDate, payrollProductEmploymentTaxDebet, 0, transactions.GetGrossSalaryAndBenefitTransactions().ToList());
                                //result = Save();
                                //if (!result.Success)
                                //    return result;

                                #endregion

                                #region Create SupplementCharge transactions

                                CreateSupplementChargeDebetScheduleTransactions(employee, timePeriod, timeBlockDate, payrollProductSupplementChargeDebet, 0, transactions.GetGrossSalaryAndBenefitTransactions());
                                result = Save();
                                if (!result.Success)
                                    return result;

                                #endregion
                            }

                            #endregion
                        }

                        StopWatch("SaveTimePayrollScheduleTransactions");

                        StartWatch("GetTimePayrollScheduleTransactions", "hämtar schematransaktioner", append: true);

                        timePayrollScheduleTransactions = GetTimePayrollScheduleTransactionsWithAccountingAndDim(employee.EmployeeId, timeBlockDatesInEmployment.OrderBy(i => i.Date).First().Date, timeBlockDatesInEmployment.OrderByDescending(i => i.Date).First().Date, timePeriod.TimePeriodId);

                        StopWatch("GetTimePayrollScheduleTransactions");
                    }

                    #region Add timeblockdates from absence schedule transactions  - very rare situation (se item 38123)

                    List<int> timeblockDateIdsForAbsenceScheduleTransactions = timePayrollScheduleTransactions.Where(x => x.TimePeriodId.HasValue && x.Type == (int)SoeTimePayrollScheduleTransactionType.Absence).Select(x => x.TimeBlockDateId).Distinct().ToList();
                    List<int> missingTimeBlockDateIds = timeblockDateIdsForAbsenceScheduleTransactions.Where(x => !transactionDates.Any(y => y.TimeBlockDateId == x)).ToList();
                    List<TimeBlockDate> missingTimeBlockDates = GetTimeBlockDates(employee.EmployeeId, missingTimeBlockDateIds);
                    transactionDates.AddRange(missingTimeBlockDates);

                    #endregion

                    #endregion

                    #region Set transaction amounts

                    StartWatch("SaveTimePayrollTransactionAmounts", "beräknar pris", append: true);

                    result = SaveTimePayrollTransactionAmounts(transactionDates, timePayrollTransactions: timePayrollTransactions, collectTransactions: false, timePeriod: timePeriod);
                    if (!result.Success)
                        return result;

                    StopWatch("SaveTimePayrollTransactionAmounts");

                    #endregion

                    #region Update transaction types

                    StartWatch("UpdatetransactionTypes", "uppdaterar typer", append: true);

                    result = UpdateTransactionPayrollTypeLevels(timePayrollTransactions);
                    if (!result.Success)
                        return result;

                    StopWatch("UpdatetransactionTypes");

                    #endregion

                    #region Weekend salary transactions

                    StartWatch("CreateHolidaySalaryTransactions", "beräknar helglöntransaktioner", append: true);

                    result = CreateHolidaySalaryTransactions(companyDTO, employee, timePeriod, transactionDates);
                    if (!result.Success)
                        return result;

                    if (result.Value is List<TimePayrollTransaction> holidaySalaryTransactions)
                        timePayrollTransactions.AddRange(holidaySalaryTransactions);

                    StopWatch("CreateHolidaySalaryTransactions");

                    #endregion

                    #region Rounding transactions

                    StartWatch("CreateRoundingTransactions", "beräknar avrundningstransaktioner", append: true);

                    result = CreateRoundingTransactions(timePayrollTransactions, timePayrollScheduleTransactions, transactionDates, employee, timePeriod, lastTimeBlockDateInPeriod);
                    if (!result.Success)
                        return result;

                    if (result.Value is List<TimePayrollTransaction> roundingTransactions)
                        timePayrollTransactions.AddRange(roundingTransactions);

                    StopWatch("CreateRoundingTransactions");

                    #endregion

                    #region Fixed transactions

                    StartWatch("FixedPayrollRows", "beräknar fasta lönerader", append: true);
                    if (!applyEmploymentHasEnded)
                    {
                        result = CreateTimePayrollTransactionsFromFixedPayrollRows(timePayrollTransactions, timePayrollScheduleTransactions, employee.EmployeeId, timePeriod.TimePeriodId, lastTimeBlockDateInPeriod, companyDTO, false);
                        if (!result.Success)
                            return result;

                        if (result.Value is List<TimePayrollTransaction> fixedTransactions)
                            timePayrollTransactions.AddRange(fixedTransactions);
                    }
                    StopWatch("FixedPayrollRows");

                    #endregion

                    #region EmployeeVehicle

                    StartWatch("EmployeeVehicle", "beräknar transaktioner för tjänstebil", append: true);

                    result = CreateEmployeeVehicleTransactions(timePeriod, lastTimeBlockDateInPeriod, employee);
                    if (!result.Success)
                        return result;

                    if (result.Value is List<TimePayrollTransaction> vechicleTransactions)
                        timePayrollTransactions.AddRange(vechicleTransactions);

                    StopWatch("EmployeeVehicle");

                    #endregion

                    #region Prepaid Vacation

                    StartWatch("CheckPrePaidVacation", "motbokar förutbetald semester", append: true);

                    result = CheckPrePaidVacation(timePayrollTransactions, timePeriod, employee, lastTimeBlockDateInPeriod, companyDTO);
                    if (!result.Success)
                        return result;

                    if (result.Value is List<TimePayrollTransaction> prePaidVacationTransactions)
                        timePayrollTransactions.AddRange(prePaidVacationTransactions);

                    StopWatch("CheckPrePaidVacation");

                    #endregion

                    #region EmploymentTax and SupplementCharge transactions

                    StartWatch("EmploymentTaxAndTransactions", "beräknar arbetsgivaravgift", append: true);

                    result = ReCalculateEmploymentTaxDebet(employee, transactionDates, timePeriod, reusableTransactionsForPeriod.Where(x => x.IsEmploymentTax()).ToList(), timePayrollTransactions, includeScheduleTransactions ? new List<TimePayrollScheduleTransaction>() : timePayrollScheduleTransactions.Where(x => x.Type == (int)SoeTimePayrollScheduleTransactionType.Absence).ToList(), false, applyEmploymentHasEnded, sysCountryId: companyDTO.SysCountryId, payrollProductEmploymentTaxDebet: companyDTO.PayrollProductEmploymentTaxDebet); //Transactions has alreday been deleted
                    if (!result.Success)
                        return result;

                    if (result.Value is List<TimePayrollTransaction> employmentTaxTimePayrollTransactions)
                        timePayrollTransactions.AddRange(employmentTaxTimePayrollTransactions);
                    if (result.Value2 is List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsEmploymentTax)
                        timePayrollScheduleTransactions.AddRange(timePayrollScheduleTransactionsEmploymentTax);

                    decimal accEmpTaxBasis = result.DecimalValue;

                    StopWatch("EmploymentTaxAndTransactions");

                    StartWatch("SupplementChargeTransactions", "beräknar påslag", append: true);

                    result = ReCalculateSupplementChargeDebet(employee, transactionDates, timePeriod, reusableTransactionsForPeriod.Where(x => x.IsSupplementCharge()).ToList(), timePayrollTransactions, false, companyDTO.PayrollProductSupplementChargeDebet); //Transactions has alreday been deleted
                    if (!result.Success)
                        return result;

                    if (result.Value is List<TimePayrollTransaction> supplementChargeDebetTransactions)
                        timePayrollTransactions.AddRange(supplementChargeDebetTransactions);

                    StopWatch("SupplementChargeTransactions");

                    #endregion

                    #region Calculate period amounts

                    StartWatch("CalculatePeriodAmounts", "beräknar belopp", append: true);

                    PayrollAmountsDTO payrollAmountsDTO = CalculatePayrollAmounts(employee, employeeTimePeriod, transactionDates, timePayrollTransactions, timePayrollScheduleTransactions, fallbackEmploymentValidationDate);

                    decimal tableTaxTransactionsAmount = payrollAmountsDTO.TableTaxTransactionsAmount;
                    decimal oneTimeTaxTransactionsAmount = payrollAmountsDTO.OneTimeTaxTransactionsAmount;
                    decimal optionalTaxAmount = payrollAmountsDTO.OptionalTaxAmount;
                    decimal employmentTaxDebitTransactionsAmount = payrollAmountsDTO.EmploymentTaxDebitTransactionsAmount;
                    decimal employmentTaxBasisTransactionsAmount = payrollAmountsDTO.EmploymentTaxBasisTransactionsAmount;
                    decimal supplementChargeDebitTransactionsAmount = payrollAmountsDTO.SupplementChargeDebitTransactionsAmount;
                    decimal grosSalaryAmount = payrollAmountsDTO.GrosSalaryAmount;
                    decimal compensationAmount = payrollAmountsDTO.CompensationAmount;
                    decimal deductionAmount = payrollAmountsDTO.DeductionAmount;
                    decimal benefitAmount = payrollAmountsDTO.BenefitAmount;
                    decimal unionFeePromotedAmount = payrollAmountsDTO.UnionFeePromotedAmount;
                    decimal benefitOtherAmount = payrollAmountsDTO.BenefitOtherAmount;
                    decimal benefitPropertyNotHouseAmount = payrollAmountsDTO.BenefitPropertyNotHouseAmount;
                    decimal benefitPropertyHouseAmount = payrollAmountsDTO.BenefitPropertyHouseAmount;
                    decimal benefitFuelAmount = payrollAmountsDTO.BenefitFuelAmount;
                    decimal benefitROTAmount = payrollAmountsDTO.BenefitROTAmount;
                    decimal benefitRUTAmount = payrollAmountsDTO.BenefitRUTAmount;
                    decimal benefitFoodtherAmount = payrollAmountsDTO.BenefitFoodtherAmount;
                    decimal benefitBorrowedComputerAmount = payrollAmountsDTO.BenefitBorrowedComputerAmount;
                    decimal benefitParkingAmount = payrollAmountsDTO.BenefitParkingAmount;
                    decimal benefitInterestAmount = payrollAmountsDTO.BenefitInterestAmount;
                    decimal benefitCompanyCarAmount = payrollAmountsDTO.BenefitCompanyCarAmount;

                    //UnionFees                    
                    result = CreateUnionFee(timePeriod.PaymentDate.Value, unionFeePromotedAmount, employee, companyDTO.PayrollProductUnionFees, timePeriod, lastTimeBlockDateInPeriod, companyDTO);
                    if (!result.Success)
                        return result;

                    decimal unionFeeAmount = 0;
                    if (result.Value is List<TimePayrollTransaction> unionFeeTransactions)
                    {
                        timePayrollTransactions.AddRange(unionFeeTransactions);
                        unionFeeAmount = unionFeeTransactions.Sum(x => x.Amount ?? 0);
                        deductionAmount += timePeriod.ExtraPeriod ? 0 : unionFeeAmount;
                    }

                    //Vacation compensation
                    VacationCompensationAccountingDistributionDTO vacationCompensationAccountingDistributionDTO = new VacationCompensationAccountingDistributionDTO();

                    if (createVacationCompensation && payrollAmountsDTO.VacationSalaryPromotedTransactions.Sum(x => x.Amount) > 0)
                    {
                        DateTime earliestDate = payrollAmountsDTO.VacationSalaryPromotedTransactions.OrderBy(x => x.Date).First().Date;
                        var employeeCalenderDTOs = EmployeeManager.GetEmploymentCalenderDTOs(employee, timePeriod.StartDate, timePeriod.StopDate.AddDays(1), GetEmployeeGroupsFromCache(), GetPayrollGroupsFromCache(), GetPayrollPriceTypesWithPeriodsFromCache(), vacationGroups: GetVacationGroupsWithSEFromCache(), annualLeaveGroups: GetAnnualLeaveGroupsFromCache());
                        if (!employeeCalenderDTOs.IsVacationGroupCoherent())
                        {
                            if (employeeCalenderDTOs.Count > 1 && employeeCalenderDTOs.Take(employeeCalenderDTOs.Count - 1).ToList().IsVacationGroupCoherent() && timePayrollTransactions.Any(a => a.IsMonthlySalary()))
                                payrollAmountsDTO.VacationSalaryPromotedTransactions = payrollAmountsDTO.VacationSalaryPromotedTransactions.Where(w => !w.IsMonthlySalary()).ToList();


                            if (earliestDate < timePeriod.StartDate)
                            {
                                var employeeCalenderDTOsOutsidePeriod = EmployeeManager.GetEmploymentCalenderDTOs(employee, earliestDate, timePeriod.StartDate.AddDays(-1), GetEmployeeGroupsFromCache(), GetPayrollGroupsFromCache(), GetPayrollPriceTypesWithPeriodsFromCache(), vacationGroups: GetVacationGroupsWithSEFromCache(), annualLeaveGroups: GetAnnualLeaveGroupsFromCache());
                                employeeCalenderDTOs.AddRange(employeeCalenderDTOsOutsidePeriod);
                            }

                            var periods = employeeCalenderDTOs.GetVacationGroupPeriods();
                            var validPeriods = periods.Where(w => w.Item1 == vacationGroupSE.VacationGroupId).ToList();

                            if (validPeriods.Any())
                            {
                                var validTransactions = new List<PayrollCalculationTransaction>();
                                foreach (var period in validPeriods)
                                {
                                    var validTimeBlockDates = transactionDates.Filter(transactionDates.First().EmployeeId, period.Item2, period.Item3);
                                    validTransactions.AddRange(payrollAmountsDTO.VacationSalaryPromotedTransactions.Where(x => validTimeBlockDates.Select(y => y.TimeBlockDateId).ToList().Contains(x.TimeBlockDateId)).ToList());                             
                                }

                                payrollAmountsDTO.VacationSalaryPromotedTransactions.Clear();
                                payrollAmountsDTO.VacationSalaryPromotedTransactions.AddRange(validTransactions);
                            }
                        }

                        #region Calculate vacation compensation

                        foreach (var group in payrollAmountsDTO.VacationSalaryPromotedTransactions.GroupBy(x => x.InternalAccountingString))
                        {
                            decimal vacationSalaryPromotedGroupAmount = group.Sum(x => x.Amount);
                            decimal vacationCompensationGroupAmount = 0;
                            if (vacationGroupSE != null && vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.DirectPayment_AccordingToCollectiveAgreement && vacationGroupSE.VacationDayPercent.HasValue)
                                vacationCompensationGroupAmount = Decimal.Multiply(vacationSalaryPromotedGroupAmount, Decimal.Divide(vacationGroupSE.VacationDayPercent.Value, 100));
                            else
                                vacationCompensationGroupAmount = Decimal.Multiply(vacationSalaryPromotedGroupAmount, sysPayrollPriceDTO.SysPayrollPriceVacationDayPercent);

                            vacationCompensationGroupAmount = Decimal.Round(vacationCompensationGroupAmount, 2, MidpointRounding.AwayFromZero);
                            vacationCompensationAccountingDistributionDTO.AddDistribution(vacationCompensationGroupAmount, group.First().AccountInternal);
                        }

                        #endregion

                        tableTaxTransactionsAmount += vacationCompensationAccountingDistributionDTO.VacationCompensationAmount;
                        grosSalaryAmount += vacationCompensationAccountingDistributionDTO.VacationCompensationAmount;
                    }

                    if (empTax != null && (empTax.Type == TermGroup_EmployeeTaxType.SideIncomeTax || empTax.Type == TermGroup_EmployeeTaxType.Adjustment || empTax.Type == TermGroup_EmployeeTaxType.Sink))
                    {
                        tableTaxTransactionsAmount += oneTimeTaxTransactionsAmount;
                        oneTimeTaxTransactionsAmount = 0;
                    }

                    if (companyDTO.PayrollProductGrossSalaryRound != null && (grosSalaryAmount >= (-0.99M) && grosSalaryAmount <= (0.99M)))
                    {
                        decimal grosSalaryAmountNegated = decimal.Negate(grosSalaryAmount);
                        grosSalaryAmount += grosSalaryAmountNegated; //sets grossalary to 0;

                        TimePayrollTransaction timePayrollTransactionGrossalaryRound = CreateOrUpdateTimePayrollTransaction(companyDTO.PayrollProductGrossSalaryRound, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, grosSalaryAmountNegated, grosSalaryAmountNegated, 0, String.Empty, reusableTransactionsForPeriod);
                        if (timePayrollTransactionGrossalaryRound == null)
                            return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                        timePayrollTransactionGrossalaryRound.IsCentRounding = true;
                        timePayrollTransactions.Add(timePayrollTransactionGrossalaryRound);
                    }


                    //Table tax
                    decimal tableTaxAmount = CalculateTaxSE(employee, timePeriod.PaymentDate.Value, tableTaxTransactionsAmount, sysPayrollPriceDTO.BaseAmount, applyEmploymentHasEnded, oneTimeTaxTransactionsAmount) * -1;

                    //Onetime tax
                    decimal oneTimeTaxAmount = CalculateOneTimeTaxSE(employee, lastTimeBlockDateInPeriod.Date, timePeriod.PaymentDate.Value, oneTimeTaxTransactionsAmount, applyEmploymentHasEnded, tableTaxTransactionsAmount) * -1;

                    //Employment tax                                
                    decimal employmentTaxCreditAmount = employmentTaxDebitTransactionsAmount * -1;

                    //Supplement charge                                
                    decimal supplementChargeCreditAmount = supplementChargeDebitTransactionsAmount * -1;

                    //Net salary
                    decimal netSalaryAmount = Decimal.Round(grosSalaryAmount + tableTaxAmount + oneTimeTaxAmount + compensationAmount + deductionAmount, 2, MidpointRounding.AwayFromZero);

                    #region Avoid negative net salary when optionaltax is greater then net salary

                    decimal netSalaryWithoutCompensation = netSalaryAmount - (compensationAmount);
                    if (netSalaryWithoutCompensation < Math.Abs(optionalTaxAmount))
                    {
                        var optionalTaxTransaction = timePayrollTransactions.FirstOrDefault(x => x.IsFixed && x.IsOptionalTax());
                        if (optionalTaxTransaction != null)
                        {
                            decimal diff = Math.Abs(optionalTaxAmount) - netSalaryWithoutCompensation;
                            optionalTaxAmount += diff;

                            optionalTaxTransaction.Quantity = 1; //make sure it is 1, 
                            optionalTaxTransaction.UnitPrice = Decimal.Round(optionalTaxAmount, 2, MidpointRounding.AwayFromZero);
                            optionalTaxTransaction.Amount = Decimal.Round(optionalTaxTransaction.Quantity * optionalTaxTransaction.UnitPrice.Value, 2, MidpointRounding.AwayFromZero);
                            SetTimePayrollTransactionCurrencyAmounts(optionalTaxTransaction);
                        }
                    }
                    netSalaryAmount += optionalTaxAmount;
                    if (netSalaryAmount > 0 && netSalaryAmount < 1)
                        netSalaryAmount = 0;

                    #endregion

                    StopWatch("CalculatePeriodAmounts");

                    #endregion

                    #region VacationCompensation

                    StartWatch("CreateVacationCompensationTransaction", "beräknar semesterersättning", append: true);

                    if (createVacationCompensation)
                    {
                        result = CreateVacationCompensationDirectPaymentTransaction(vacationCompensationAccountingDistributionDTO, reusableTransactionsForPeriod, companyDTO.PayrollProductVacationCompensationDR, timePeriod, employee, employeeTimePeriod, lastTimeBlockDateInPeriod, companyDTO.InitialAttestStateId, accEmpTaxBasis, ref employmentTaxCreditAmount);
                        if (!result.Success)
                            return result;
                    }

                    StopWatch("CreateVacationCompensationTransaction");

                    #endregion

                    #region Save period amounts

                    StartWatch("SavePeriodAmounts", "sparar belopp", append: true);

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.SINKTax, createSINKTaxInsteadOfTableTax ? tableTaxAmount : 0, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.ASINKTax, createASINKTaxInsteadOfASINKTax ? tableTaxAmount : 0, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.TableTax, (!createSINKTaxInsteadOfTableTax && !createASINKTaxInsteadOfASINKTax) ? tableTaxAmount : 0, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.OneTimeTax, oneTimeTaxAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.OptionalTax, optionalTaxAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.EmploymentTaxCredit, employmentTaxCreditAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.EmploymentTaxBasis, employmentTaxBasisTransactionsAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.SupplementChargeCredit, supplementChargeCreditAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.GrossSalary, grosSalaryAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.Benefit, benefitAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.Compensation, compensationAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.Deduction, deductionAmount, false);
                    if (!result.Success)
                        return result;

                    result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.UnionFee, unionFeeAmount, false);
                    if (!result.Success)
                        return result;

                    StopWatch("SavePeriodAmounts");

                    #endregion

                    #region Create period tax transactions

                    StartWatch("CreatePeriodTaxTransactions", "skapar transaktioner för skatt", append: true);

                    TimePayrollTransaction timePayrollTransactionTax = null;
                    PayrollProduct taxProduct = companyDTO.PayrollProductTableTax;
                    if (createSINKTaxInsteadOfTableTax)
                        taxProduct = payrollProductSINKTax;
                    else if (createASINKTaxInsteadOfASINKTax)
                        taxProduct = payrollProductASINKTax;

                    //Tax (TableTax, SINKTax or ASINKTax)
                    timePayrollTransactionTax = CreateOrUpdateTimePayrollTransaction(taxProduct, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, tableTaxAmount, tableTaxAmount, 0, String.Empty, reusableTransactionsForPeriod);
                    if (timePayrollTransactionTax == null)
                        return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                    if (timePayrollTransactionTax.Amount == 0)
                    {
                        result = SetTimePayrollTransactionToDeleted(timePayrollTransactionTax, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        timePayrollTransactions.Add(timePayrollTransactionTax);
                    }

                    //OneTimeTax
                    TimePayrollTransaction timePayrollTransactionOneTimeTax = CreateOrUpdateTimePayrollTransaction(companyDTO.PayrollProductOneTimeTax, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, oneTimeTaxAmount, oneTimeTaxAmount, 0, String.Empty, reusableTransactionsForPeriod);
                    if (timePayrollTransactionOneTimeTax == null)
                        return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                    if (timePayrollTransactionOneTimeTax.Amount == 0)
                    {
                        result = SetTimePayrollTransactionToDeleted(timePayrollTransactionOneTimeTax, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        timePayrollTransactions.Add(timePayrollTransactionOneTimeTax);
                    }

                    //EmploymentTax Debit
                    TimePayrollTransaction timePayrollTransactionEmploymentTaxCredit = CreateOrUpdateTimePayrollTransaction(companyDTO.PayrollProductEmploymentTaxCredit, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, employmentTaxCreditAmount, employmentTaxCreditAmount, 0, String.Empty, reusableTransactionsForPeriod);
                    if (timePayrollTransactionEmploymentTaxCredit == null)
                        return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                    if (timePayrollTransactionEmploymentTaxCredit.Amount == 0)
                    {
                        result = SetTimePayrollTransactionToDeleted(timePayrollTransactionEmploymentTaxCredit, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        timePayrollTransactions.Add(timePayrollTransactionEmploymentTaxCredit);
                    }

                    //Supplement charge Debit
                    TimePayrollTransaction timePayrollTransactionSupplementChargeCredit = CreateOrUpdateTimePayrollTransaction(companyDTO.PayrollProductSupplementChargeCredit, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, supplementChargeCreditAmount, supplementChargeCreditAmount, 0, String.Empty, reusableTransactionsForPeriod);
                    if (timePayrollTransactionSupplementChargeCredit == null)
                        return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                    if (timePayrollTransactionSupplementChargeCredit.Amount == 0)
                    {
                        result = SetTimePayrollTransactionToDeleted(timePayrollTransactionSupplementChargeCredit, saveChanges: false);
                        if (!result.Success)
                            return result;
                    }
                    else
                    {
                        timePayrollTransactions.Add(timePayrollTransactionSupplementChargeCredit);
                    }

                    StopWatch("CreatePeriodTaxTransactions");

                    #endregion

                    #region Benefit Invert

                    StartWatch("CreateBenefitInvertTransactions", "skapar motbokning av förmån", append: true);

                    Dictionary<TermGroup_SysPayrollType, decimal> amounts = new Dictionary<TermGroup_SysPayrollType, decimal>()
                    {
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_Other, benefitOtherAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyNotHouse, benefitPropertyNotHouseAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_PropertyHouse, benefitPropertyHouseAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_Fuel, benefitFuelAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_ROT, benefitROTAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_RUT, benefitRUTAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_Food, benefitFoodtherAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_BorrowedComputer, benefitBorrowedComputerAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_Parking, benefitParkingAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_Interest, benefitInterestAmount },
                        { TermGroup_SysPayrollType.SE_Benefit_Invert_CompanyCar, benefitCompanyCarAmount },
                    };

                    result = CreateBenefitInvertTransactions(reusableTransactionsForPeriod, timePeriod, lastTimeBlockDateInPeriod, employee, amounts);
                    if (!result.Success)
                        return result;

                    StopWatch("CreateBenefitInvertTransactions");

                    #endregion

                    #region Save

                    StartWatch("Save", "sparar", append: true);

                    result = Save();
                    if (!result.Success)
                        return result;

                    StopWatch("Save");

                    #endregion

                    #region NetSalary

                    StartWatch("CreateNetSalaryTransaction", "beräknar nettolön", append: true);

                    result = CreateNetSalaryAndDistressAmountTransaction(netSalaryAmount, reusableTransactionsForPeriod, companyDTO.PayrollProductNetSalary, companyDTO.PayrollProductNetSalaryRound, companyDTO.PayrollProductSalaryDistressAmount, timePeriod, employee, employeeTimePeriod, lastTimeBlockDateInPeriod, companyDTO.InitialAttestStateId, !timePayrollTransactions.Any());
                    if (!result.Success)
                        return result;

                    StopWatch("CreateNetSalaryTransaction");

                    #endregion

                    this.DeActivateWarningPayrollPeriodHasChanged(employeeTimePeriod);
                    this.RecalculatePayrollControlFunctions(employee, timePeriod, employeeTimePeriod);
                    
                    if (result.Success)
                        this.currentTransaction.Complete();
                }

                StopWatch("Employee");
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                LogError(ex);
            }
            finally
            {
                if (!result.Success)
                    LogTransactionFailed(this.ToString());
            }

            return result;
        }
        private ActionResult SetUpReCalculatePayrollPeriodCompanyDTO(ReCalculatePayrollPeriodCompanyDTO companyDTO, List<int> employeeIds = null)
        {
            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            companyDTO.InitialAttestStateId = attestStateInitialPayroll.AttestStateId;

            List<Tuple<int?, int?, int?, int?>> mandatoryProducts = new List<Tuple<int?, int?, int?, int?>>()
            {
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_Tax, (int?)TermGroup_SysPayrollType.SE_Tax_TableTax, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_Tax, (int?)TermGroup_SysPayrollType.SE_Tax_OneTimeTax, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_EmploymentTaxCredit, (int?)null, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_SupplementChargeCredit, (int?)null, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_EmploymentTaxDebit, (int?)null, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_SupplementChargeDebit, (int?)null, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_NetSalary, (int?)TermGroup_SysPayrollType.SE_NetSalary_Paid, (int?)null, (int?)null),
                Tuple.Create((int?)TermGroup_SysPayrollType.SE_NetSalary, (int?)TermGroup_SysPayrollType.SE_NetSalary_Rounded, (int?)null, (int?)null),
            };

            ActionResult result = CheckMandatoryPayrollProductByLevels(mandatoryProducts);
            if (!result.Success)
                return new ActionResult((int)ActionResultSave.EntityNotFound, result.ErrorMessage);

            companyDTO.PayrollProductTableTax = GetPayrollProductTableTax();
            if (companyDTO.PayrollProductTableTax == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductTableTax);

            companyDTO.PayrollProductOneTimeTax = GetPayrollProductOneTimeTax();
            if (companyDTO.PayrollProductOneTimeTax == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductOneTimeTax);

            companyDTO.PayrollProductEmploymentTaxDebet = GetPayrollProductEmploymentTaxDebet();
            companyDTO.PayrollProductEmploymentTaxCredit = GetPayrollProductEmploymentTaxCredit();
            if (companyDTO.PayrollProductEmploymentTaxCredit == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductEmploymentTax);

            companyDTO.PayrollProductSupplementChargeDebet = GetPayrollProductSupplementChargeDebet();
            companyDTO.PayrollProductSupplementChargeCredit = GetPayrollProductSupplementChargeCredit();
            if (companyDTO.PayrollProductSupplementChargeCredit == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductSupplementCharge);

            companyDTO.PayrollProductNetSalary = GetPayrollProductNetSalaryPayed();
            if (companyDTO.PayrollProductNetSalary == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductNetSalary);

            companyDTO.PayrollProductNetSalaryRound = GetPayrollProductNetSalaryRound();
            if (companyDTO.PayrollProductNetSalaryRound == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPayrollProductNetSalaryRound);

            //Do not validate if is null
            companyDTO.PayrollProductVacationCompensationDR = GetPayrollProductVacationCompensationDirectPaid();
            companyDTO.PayrollProductSalaryDistressAmount = GetPayrollProductSalaryDistressAmount();
            companyDTO.PayrollProductWeekendSalary = GetPayrollProductWeekendSalary();
            companyDTO.VacationGroups = GetVacationGroupsWithSEFromCache();
            companyDTO.PayrollProductUnionFees = GetPayrollProductUnionFees();
            companyDTO.PayrollProductMonthlySalaries = GetPayrollProductMonthlySalaries();
            companyDTO.SysCountryId = GetSysCountryFromCache();

            int productId = GetCompanyIntSettingFromCache(CompanySettingType.PayrollGrossalaryRoundingPayrollProduct);
            if (productId != 0)
                companyDTO.PayrollProductGrossSalaryRound = GetPayrollProductWithSettingsAndAccountInternals(productId);

            //Pre load: uses CompanyCacheItem
            InitEvaluatePriceFormulaInputDTO(employeeIds: employeeIds);
            AddFixedPayrollRowsToCache(employeeIds, GetFixedPayrollRows(employeeIds).ToDTOs(false).ToList());

            return result;
        }
        private ActionResult UpdateTransactionPayrollTypeLevels(List<TimePayrollTransaction> timePayrollTransactions)
        {
            StartWatch("UpdatetransactionTypesGetAndSet", append: true);
            foreach (var productsGrouping in timePayrollTransactions.Where(x => !x.IsRetroTransaction()).GroupBy(i => i.ProductId))
            {
                PayrollProduct payrollProduct = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(productsGrouping.Key);
                if (payrollProduct == null)
                    continue;

                foreach (var transaction in productsGrouping)
                {
                    transaction.SysPayrollTypeLevel1 = payrollProduct.SysPayrollTypeLevel1;
                    transaction.SysPayrollTypeLevel2 = payrollProduct.SysPayrollTypeLevel2;
                    transaction.SysPayrollTypeLevel3 = payrollProduct.SysPayrollTypeLevel3;
                    transaction.SysPayrollTypeLevel4 = payrollProduct.SysPayrollTypeLevel4;
                }
            }
            StopWatch("UpdatetransactionTypesGetAndSet");

            StartWatch("UpdatetransactionTypesSave", append: true);

            ActionResult result = Save();

            StopWatch("UpdatetransactionTypesSave");

            return result;
        }
        private ActionResult CheckPrePaidVacation(List<TimePayrollTransaction> periodTransactions, TimePeriod timePeriod, Employee employee, TimeBlockDate lastTimeBlockDateInPeriod, ReCalculatePayrollPeriodCompanyDTO companyDTO)
        {
            ActionResult result = new ActionResult(true);

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            Employment employment = employee.GetEmployment(timePeriod.StartDate, timePeriod.StopDate, false);
            if (employment != null)
            {
                VacationGroup vacationGroup = employment.GetVacationGroup(timePeriod.StopDate, companyDTO.VacationGroups);
                VacationGroupSE vacationGroupSE = vacationGroup != null ? GetVacationGroupSE(vacationGroup.VacationGroupId) : null;
                if (vacationGroupSE == null)
                    return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8719, "Semesteravtal saknas. kan ej slutföra motbokning av förutbetald semester"));

                AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
                if (attestStateInitialPayroll == null)
                    return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

                if (!periodTransactions.Any(x => x.IsAbsenceVacation()))// if no vacation is reported this period, dont continue
                    return result;

                EmployeeVacationSE employeeVacation = EmployeeManager.GetLatestEmployeeVacationSE(entities, employee.EmployeeId);
                if (employeeVacation == null)
                    return result;

                var vacationDto = periodTransactions.ToVacationDaysCalculationDTO(vacationGroupSE.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours);
                List<TimePayrollTransaction> vacationYearEndPrePaidTransactions = GetTimePayrollTransactionsForEmployeeAndVacationYearEndFromDate(employee.EmployeeId, timePeriod.StopDate.AddDays(1));

                #region VacationAddition or VacationSalary

                if (vacationGroupSE.IsInvertVacationAddition() || vacationGroupSE.IsInvertVacationSalary())
                {
                    decimal vacationYearEndPrePaidQuantity = vacationYearEndPrePaidTransactions.GetVacationPrepaymentTransactions().GetQuantityVacationDays(false);
                    decimal prepaidQuantity = (employeeVacation.PaidVacationAllowance.GetValueOrDefault() + vacationYearEndPrePaidQuantity + vacationDto.PeriodVacationPrepaymentPaid - employeeVacation.UsedDaysPaid.GetValueOrDefault());
                    if (prepaidQuantity > 0)
                    {
                        var prepaymentInvertProduct = GetPayrollProductPrepaymentInvert();
                        if (prepaymentInvertProduct == null)
                            return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8718, "Löneart för motbokning av förutbetald semester saknas"));

                        List<Tuple<int, decimal, decimal>> transactionsToInvert = new List<Tuple<int, decimal, decimal>>();
                        List<TimePayrollTransaction> transactionBasisForInvert = new List<TimePayrollTransaction>();
                        if (vacationGroupSE.IsInvertVacationAddition())
                            transactionBasisForInvert = periodTransactions.Where(x => x.IsVacationAddition()).ToList();
                        else if (vacationGroupSE.IsInvertVacationSalary())
                            transactionBasisForInvert = periodTransactions.Where(x => x.IsVacationSalary()).ToList();

                        if (transactionBasisForInvert.Count != 0)
                        {
                            foreach (var transactionsByDate in transactionBasisForInvert.GroupBy(x => x.TimeBlockDateId))
                            {
                                if (prepaidQuantity == 0)
                                    break;

                                var transaction = transactionsByDate.FirstOrDefault();
                                if (transaction == null)
                                    continue;

                                decimal vacationDays = transaction.GetQuantityVacationDays(false);
                                decimal unitPrice = 0;
                                foreach (var item in transactionsByDate)
                                {
                                    if (transaction.UnitPrice.HasValue)
                                        unitPrice += Math.Round((item.Quantity / 60) * item.UnitPrice.Value, 2);
                                }
                                unitPrice = Math.Round(unitPrice / vacationDays, 2);

                                if (vacationDays <= prepaidQuantity)
                                {
                                    transactionsToInvert.Add(Tuple.Create(transaction.TimePayrollTransactionId, vacationDays, unitPrice));
                                    prepaidQuantity -= vacationDays;
                                }
                                else if (vacationDays > prepaidQuantity)
                                {
                                    transactionsToInvert.Add(Tuple.Create(transaction.TimePayrollTransactionId, prepaidQuantity, unitPrice));
                                    prepaidQuantity = 0;
                                }
                            }

                            foreach (var transactionsToInvertByUnitPrice in transactionsToInvert.GroupBy(x => x.Item3))
                            {
                                decimal unitPrice = transactionsToInvertByUnitPrice.Key;
                                var groupItems = transactionsToInvertByUnitPrice.ToList();
                                decimal quantity = decimal.Negate(groupItems.Select(x => x.Item2).Sum());
                                decimal amount = Decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

                                TimePayrollTransaction invertTransaction = CreateTimePayrollTransaction(prepaymentInvertProduct, lastTimeBlockDateInPeriod, quantity, amount, 0, unitPrice, String.Empty, attestStateInitialPayroll.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId);

                                //Accounting
                                ApplyAccountingOnTimePayrollTransaction(invertTransaction, employee, lastTimeBlockDateInPeriod.Date, prepaymentInvertProduct, setAccountInternal: true);
                                newTransactions.Add(invertTransaction);
                            }

                            result = SaveChanges(entities);
                        }
                    }
                }

                #endregion

                #region VacationAddition Variable

                if (vacationGroupSE.IsInvertVacationAdditionVariable())
                {
                    decimal vacationYearEndPrePaidQuantityVariable = vacationYearEndPrePaidTransactions.GetVacationPrepaymentVariableTransactions().GetQuantityVacationDays(false);
                    decimal prepaidQuantityVariable = (employeeVacation.PaidVacationVariableAllowance.GetValueOrDefault() + vacationYearEndPrePaidQuantityVariable + vacationDto.PeriodVariablePrepaymentPaid - employeeVacation.UsedDaysPaid.GetValueOrDefault());
                    if (prepaidQuantityVariable > 0)
                    {
                        List<Tuple<int, decimal, decimal>> transactionsToInvert = new List<Tuple<int, decimal, decimal>>();
                        List<TimePayrollTransaction> transactionBasisForInvert = periodTransactions.Where(x => x.IsVacationAdditionVariable()).ToList();

                        if (transactionBasisForInvert.Count != 0)
                        {
                            var prepaymentVariableInvertProduct = GetPayrollProductVariablePrepaymentInvert();
                            if (prepaymentVariableInvertProduct == null)
                                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8834, "Löneart för motbokning av förutbetald rörligt semestertillägg saknas"));

                            foreach (var transactionsByDate in transactionBasisForInvert.GroupBy(x => x.TimeBlockDateId))
                            {
                                if (prepaidQuantityVariable == 0)
                                    break;

                                var transaction = transactionsByDate.FirstOrDefault();
                                if (transaction == null)
                                    continue;

                                decimal vacationDays = transaction.GetQuantityVacationDays(false);
                                decimal unitPrice = 0;
                                foreach (var item in transactionsByDate)
                                {
                                    if (transaction.UnitPrice.HasValue)
                                        unitPrice += Math.Round((item.Quantity / 60) * item.UnitPrice.Value, 2);
                                }
                                unitPrice = Math.Round(unitPrice / vacationDays, 2);

                                if (vacationDays <= prepaidQuantityVariable)
                                {
                                    transactionsToInvert.Add(Tuple.Create(transaction.TimePayrollTransactionId, vacationDays, unitPrice));
                                    prepaidQuantityVariable -= vacationDays;
                                }
                                else if (vacationDays > prepaidQuantityVariable)
                                {
                                    transactionsToInvert.Add(Tuple.Create(transaction.TimePayrollTransactionId, prepaidQuantityVariable, unitPrice));
                                    prepaidQuantityVariable = 0;
                                }
                            }

                            foreach (var transactionsToInvertByUnitPrice in transactionsToInvert.GroupBy(x => x.Item3))
                            {
                                decimal unitPrice = transactionsToInvertByUnitPrice.Key;
                                var groupItems = transactionsToInvertByUnitPrice.ToList();
                                decimal quantity = decimal.Negate(groupItems.Select(x => x.Item2).Sum());
                                decimal amount = Decimal.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

                                TimePayrollTransaction invertTransaction = CreateTimePayrollTransaction(prepaymentVariableInvertProduct, lastTimeBlockDateInPeriod, quantity, amount, 0, unitPrice, String.Empty, attestStateInitialPayroll.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId);

                                //Accounting
                                ApplyAccountingOnTimePayrollTransaction(invertTransaction, employee, lastTimeBlockDateInPeriod.Date, prepaymentVariableInvertProduct, setAccountInternal: true);
                                newTransactions.Add(invertTransaction);
                            }

                            result = SaveChanges(entities);
                        }
                    }

                }

                #endregion
            }

            result.Value = newTransactions;
            return result;
        }

        #endregion

        #region Recalculate control functions

        private ActionResult RecalculatePayrollControlFunctions(Employee employee, TimePeriod timePeriod, EmployeeTimePeriod employeeTimePeriod = null)
        {            
            if (employeeTimePeriod != null && timePeriod.TimePeriodId != employeeTimePeriod.TimePeriodId)
                return new ActionResult(false, (int)ActionResultSave.IncorrectInput, GetText(8604, "Vald period och perioduppsättning matchar inte."));

            if (employeeTimePeriod == null)
                employeeTimePeriod = this.GetEmployeeTimePeriodWithOutcome(timePeriod.TimePeriodId, employee.EmployeeId);
            else if (!employeeTimePeriod.PayrollControlFunctionOutcome.IsLoaded)
                employeeTimePeriod.PayrollControlFunctionOutcome.Load();

            //If no period exists, do nothing. 
            if (employeeTimePeriod == null)
                return new ActionResult(true);

            if (!employee.EmployeePosition.IsLoaded)
                employee.EmployeePosition.Load();


            List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForDayWithExtendedAndTimeCodeAndAccounting(employee.EmployeeId, timePeriod.StartDate, timePeriod.StopDate, timePeriod).Where(x => !x.PayrollStartValueRowId.HasValue).ToList();
            //Get all absence scheduletransactions
            List<TimePayrollScheduleTransaction> scheduleTransactions = timePeriod.ExtraPeriod ? new List<TimePayrollScheduleTransaction>() : GetTimePayrollScheduleTransactionsWithAccountingAndDim(employee.EmployeeId, timePeriod.StartDate, timePeriod.StopDate, timePeriod.TimePeriodId, SoeTimePayrollScheduleTransactionType.Absence);
            List<PayrollProduct> payrollProducts = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactions, scheduleTransactions);

            //Filter out transactions with products that are not used in payroll
            timePayrollTransactions = timePayrollTransactions.Where(x => payrollProducts.Where(y => y.UseInPayroll).Any(z => z.ProductId == x.ProductId)).ToList();
            scheduleTransactions = scheduleTransactions.Where(x => payrollProducts.Where(y => y.UseInPayroll).Any(z => z.ProductId == x.ProductId)).ToList();

            PayrollCalculationPeriodSumDTO periodSumDTO = CalculateSum(timePayrollTransactions, scheduleTransactions);

            EmployeeVacationPeriodDTO vacationPeriodDTO = PayrollManager.GetEmployeeVacationPeriod(entities, actorCompanyId, employee.EmployeeId, timePeriod.StartDate, timePeriod.StopDate, timePeriod, timePayrollTransactions);

            RecalculatePayrollControlFunctions payrollControlFunctions = new RecalculatePayrollControlFunctions(actorCompanyId, employee, employeeTimePeriod, timePeriod);

            ActionResult validateEmployeeAccountsResult = ActorManager.ValidateEmployeeAccounts(entities, actorCompanyId, employee.EmployeeId, false, false);

            List<PayrollControlFunction> result = payrollControlFunctions.Run(periodSumDTO, vacationPeriodDTO, validateEmployeeAccountsResult);
            AddOutcomeToDatabase(result);

            return Save();
        }

        private void AddOutcomeToDatabase(List<PayrollControlFunction> result)
        {
            foreach (var controlFunction in result)
            {
                //Handle outcome and changes

                if (controlFunction.WarningOutcome != null)
                {
                    foreach (var change in controlFunction.Changes)
                    {
                        SetCreatedProperties(change);
                        change.PayrollControlFunctionOutcome = controlFunction.WarningOutcome;
                        entities.PayrollControlFunctionOutcomeChange.AddObject(change);
                    }

                    if (controlFunction.WarningOutcome.PayrollControlFunctionOutcomeId == 0)
                    {
                        SetCreatedProperties(controlFunction.WarningOutcome);
                        entities.PayrollControlFunctionOutcome.AddObject(controlFunction.WarningOutcome);
                    }
                    else
                    {
                        if (controlFunction.Changes.Any())
                            SetModifiedProperties(controlFunction.WarningOutcome);
                    }
                }
            }
        }

        private PayrollCalculationPeriodSumDTO CalculateSum(List<TimePayrollTransaction> timePayrollTransactions, List<TimePayrollScheduleTransaction> scheduleTransactions)
        {
            PayrollCalculationPeriodSumDTO sum = new PayrollCalculationPeriodSumDTO();
            sum.TransactionNet = timePayrollTransactions.Where(x => x.Amount.HasValue && x.IsNetSalaryPaid()).Sum(x => x.Amount.Value);

            List<PayrollCalculationPeriodSumItemDTO> transactions = new List<PayrollCalculationPeriodSumItemDTO>();

            timePayrollTransactions.ForEach(x => transactions.Add(PayrollCalculationPeriodSumItemDTO.Create(x.SysPayrollTypeLevel1, x.SysPayrollTypeLevel2, x.SysPayrollTypeLevel3, x.SysPayrollTypeLevel4, x.Amount)));
            scheduleTransactions.ForEach(x => transactions.Add(PayrollCalculationPeriodSumItemDTO.Create(x.SysPayrollTypeLevel1, x.SysPayrollTypeLevel2, x.SysPayrollTypeLevel3, x.SysPayrollTypeLevel4, x.Amount)));

            PayrollRulesUtil.CalculateSum(sum, transactions);

            return sum;
        }

        private ActionResult ActivateWarningPayrollPeriodHasChanged(int employeeId, int timePeriodId)
        {
            return ActivateWarningPayrollPeriodHasChanged(new List<int> { employeeId }, timePeriodId);
        }
        private ActionResult ActivateWarningPayrollPeriodHasChanged(List<int> employeeIds, int timePeriodId)
        {
            if (employeeIds.IsNullOrEmpty() ||  timePeriodId == 0)
                return new ActionResult(true);

            List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriodsWithOutcome(timePeriodId, employeeIds);
            return ActivateWarningPayrollPeriodHasChanged(employeeTimePeriods);
        }

        private ActionResult ActivateWarningPayrollPeriodHasChanged(Employee employee,  List<DateTime> dates)
        {         
            if (employee == null || dates.IsNullOrEmpty())
                return new ActionResult(true);

            List<TimePeriod> timeperiods = new List<TimePeriod>();
            foreach (var date in dates)
            {
                var timePeriod = GetTimePeriodForEmployee(employee, date);
                if (timePeriod != null && !timeperiods.Any(x => x.TimePeriodId == timePeriod.TimePeriodId))
                    timeperiods.Add(timePeriod);
            }
                        
            List<EmployeeTimePeriod> employeeTimePeriods = GetEmployeeTimePeriodsWithOutcome(timeperiods, employee.EmployeeId);            
            return ActivateWarningPayrollPeriodHasChanged(employeeTimePeriods);
        }

        private ActionResult ActivateWarningPayrollPeriodHasChanged(List<EmployeeTimePeriod> employeeTimePeriods)
        {
            PayrollPeriodHasChangedWarning warningPayrollPeriodHasChanged = new PayrollPeriodHasChangedWarning(actorCompanyId, employeeTimePeriods);
            List<PayrollControlFunction> result = warningPayrollPeriodHasChanged.Activate();
            AddOutcomeToDatabase(result);

            return Save();
        }

        private ActionResult DeActivateWarningPayrollPeriodHasChanged(EmployeeTimePeriod employeeTimePeriod)
        {            
            if (employeeTimePeriod == null)
                return new ActionResult(true);

            if (!employeeTimePeriod.PayrollControlFunctionOutcome.IsLoaded)
                employeeTimePeriod.PayrollControlFunctionOutcome.Load();

            PayrollPeriodHasChangedWarning warningPayrollPeriodHasChanged = new PayrollPeriodHasChangedWarning(actorCompanyId, employeeTimePeriod);
            List<PayrollControlFunction> result = warningPayrollPeriodHasChanged.DeActivate();
            AddOutcomeToDatabase(result);

            return Save();
        }

        #endregion

        #region Recalculate amounts

        private PayrollAmountsDTO CalculatePayrollAmounts(Employee employee, EmployeeTimePeriod employeeTimePeriod, List<TimeBlockDate> transactionDates, List<TimePayrollTransaction> timePayrollTransactionsInput, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactionsInput, DateTime? fallbackEmploymentValidationDate)
        {
            PayrollAmountsDTO payrollAmounts = new PayrollAmountsDTO();

            List<PayrollProduct> payrollProductsAll = new List<PayrollProduct>();
            payrollProductsAll.AddRange(GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactionsInput));
            payrollProductsAll.AddRange(GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollScheduleTransactionsInput));

            List<PayrollCalculationTransaction> timePayrollTransactions = timePayrollTransactionsInput.ToPayrollCalculationTransactions();
            List<PayrollCalculationTransaction> scheduleTransactions = timePayrollScheduleTransactionsInput.ToPayrollCalculationTransactions();
            List<PayrollCalculationTransaction> periodTransactions = new List<PayrollCalculationTransaction>();

            foreach (TimeBlockDate timeBlockDate in transactionDates)
            {
                EmployeeGroup employeeGroup = GetEmployeeGroup(employee, timeBlockDate.Date);
                if (employeeGroup == null && fallbackEmploymentValidationDate.HasValue)
                    employeeGroup = employee.GetEmployeeGroup(fallbackEmploymentValidationDate.Value, GetEmployeeGroupsFromCache());

                #region Decide which timepayrolltransactions should be included

                var timePayrollTransactionsForDate = timePayrollTransactions.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                periodTransactions.AddRange(timePayrollTransactionsForDate);

                #endregion

                #region Decide which schedule transactions should be included

                var timePayrollScheduleTransactionsForDate = scheduleTransactions.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                var timePayrollScheduleTransactionsForDateTypeAbcense = timePayrollScheduleTransactionsForDate.Where(x => x.ScheduleType == SoeTimePayrollScheduleTransactionType.Absence).ToList();

                if ((employeeGroup != null && !employeeGroup.AutogenTimeblocks && !timePayrollTransactionsForDate.Any()) || timePayrollScheduleTransactionsForDateTypeAbcense.Any())
                {
                    if (timePayrollScheduleTransactionsForDateTypeAbcense.Any())
                        periodTransactions.AddRange(timePayrollScheduleTransactionsForDateTypeAbcense);
                    else
                        periodTransactions.AddRange(timePayrollScheduleTransactionsForDate);
                }

                #endregion
            }

            #region Calculate sums

            foreach (var payrollCalculationTransactionsForDate in periodTransactions.GroupBy(x => x.TimeBlockDateId))
            {
                var payrollCalculationTransactionsForProductAndDate = payrollCalculationTransactionsForDate.GroupBy(i => i.ProductId).ToList();
                foreach (var payrollCalculationTransactionForProductAndDate in payrollCalculationTransactionsForProductAndDate)
                {
                    //PayrollProduct
                    PayrollProduct payrollProduct = payrollProductsAll.FirstOrDefault(x => x.ProductId == payrollCalculationTransactionForProductAndDate.Key);
                    if (payrollProduct == null)
                        continue;
                    if (!payrollProduct.UseInPayroll)
                        continue;

                    PayrollCalculationTransaction firstTransaction = payrollCalculationTransactionForProductAndDate.FirstOrDefault();
                    if (firstTransaction == null)
                        continue;

                    //PayrollProductSetting for Employee
                    PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, firstTransaction.Date);
                    if (payrollProductSetting == null && fallbackEmploymentValidationDate.HasValue)
                        payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, fallbackEmploymentValidationDate.Value);

                    //Special Settings for EmployeePeriod
                    EmployeeTimePeriodProductSetting employeePeriodPayrollProductSetting = employeeTimePeriod.GetSetting(payrollProduct.ProductId);

                    foreach (var transactionsByType in payrollCalculationTransactionForProductAndDate.GroupBy(x => x.Levels))
                    {
                        foreach (var transaction in transactionsByType)
                        {
                            //Sum
                            decimal sum = transaction.Amount;

                            //Tax - depending on TaxCalculationType
                            if (payrollProductSetting != null)
                            {
                                if (payrollProduct.IsTaxBasis())
                                {
                                    if (employeePeriodPayrollProductSetting != null)
                                    {
                                        if (employeePeriodPayrollProductSetting.TaxCalculationType == (int)TermGroup_PayrollProductTaxCalculationType.OneTimeTax)
                                            payrollAmounts.OneTimeTaxTransactionsAmount += sum;
                                        else if (employeePeriodPayrollProductSetting.TaxCalculationType == (int)TermGroup_PayrollProductTaxCalculationType.TableTax)
                                            payrollAmounts.TableTaxTransactionsAmount += sum;
                                    }
                                    else
                                    {
                                        if (transaction.IsRetroTransaction || payrollProductSetting.TaxCalculationType == (int)TermGroup_PayrollProductTaxCalculationType.OneTimeTax)
                                            payrollAmounts.OneTimeTaxTransactionsAmount += sum;
                                        else if (payrollProductSetting.TaxCalculationType == (int)TermGroup_PayrollProductTaxCalculationType.TableTax)
                                            payrollAmounts.TableTaxTransactionsAmount += sum;
                                    }
                                }

                                if (payrollProductSetting.VacationSalaryPromoted)
                                    payrollAmounts.VacationSalaryPromotedTransactions.Add(transaction);

                                if (payrollProductSetting.UnionFeePromoted)
                                    payrollAmounts.UnionFeePromotedAmount += sum;
                            }
                            else if (transaction.IsRetroTransaction)
                            {
                                payrollAmounts.OneTimeTaxTransactionsAmount += sum;
                            }

                            //VoluntaryTax
                            if (transaction.IsOptionalTax())
                                payrollAmounts.OptionalTaxAmount += sum;

                            //Supplementcharge
                            if (transaction.IsSupplementChargeDebit()) //Prevent rouding errors
                                payrollAmounts.SupplementChargeDebitTransactionsAmount += sum;

                            //EmploymentTax
                            if (transaction.IsEmploymentTaxDebit()) //Prevent rounding errors
                                payrollAmounts.EmploymentTaxDebitTransactionsAmount += sum;

                            if (transaction.IsEmploymentTaxBasis())
                                payrollAmounts.EmploymentTaxBasisTransactionsAmount += sum;

                            //GrossSalary
                            if (transaction.IsGrossSalary() || transaction.IsOccupationalPension())
                                payrollAmounts.GrosSalaryAmount += sum;
                            else if (transaction.IsCostDeduction())
                                payrollAmounts.GrosSalaryAmount -= sum;

                            //Compensation
                            if (transaction.IsCompensation())
                                payrollAmounts.CompensationAmount += sum;

                            #region Benefit

                            //Benefit
                            if (transaction.IsBenefit())
                                payrollAmounts.BenefitAmount += sum;

                            //Benefit other
                            if (transaction.IsBenefitOther())
                                payrollAmounts.BenefitOtherAmount += sum;

                            //Benefit property not house
                            if (transaction.IsBenefitPropertyNotHouse())
                                payrollAmounts.BenefitPropertyNotHouseAmount += sum;

                            //Benefit property house
                            if (transaction.IsBenefitPropertyHouse())
                                payrollAmounts.BenefitPropertyHouseAmount += sum;

                            //Benefit fuel
                            if (transaction.IsBenefitFuel())
                                payrollAmounts.BenefitFuelAmount += sum;

                            //Benefit ROT
                            if (transaction.IsBenefitROT())
                                payrollAmounts.BenefitROTAmount += sum;

                            //Benefit RUT
                            if (transaction.IsBenefitRUT())
                                payrollAmounts.BenefitRUTAmount += sum;

                            //Benefit Food
                            if (transaction.IsBenefitFood())
                                payrollAmounts.BenefitFoodtherAmount += sum;

                            //Benefit Borrowed computer
                            if (transaction.IsBenefitBorrowedComputer())
                                payrollAmounts.BenefitBorrowedComputerAmount += sum;

                            //Benefit parking
                            if (transaction.IsBenefitParking())
                                payrollAmounts.BenefitParkingAmount += sum;

                            //Benefit interest
                            if (transaction.IsBenefitInterest())
                                payrollAmounts.BenefitInterestAmount += sum;

                            //Benefit company car
                            if (transaction.IsBenefitCompanyCar())
                                payrollAmounts.BenefitCompanyCarAmount += sum;

                            #endregion

                            //Deduction
                            if (transaction.IsDeduction())
                            {
                                payrollAmounts.DeductionAmount += sum;
                                if (transaction.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_Deduction_SalaryDistressAmount)
                                    payrollAmounts.DeductionSalaryDistressAmount += sum;
                                if (transaction.IsDeductionCarBenefit())
                                    payrollAmounts.DeductionCarBenefit += sum;
                            }

                        }
                    }

                }
            }

            #endregion

            return payrollAmounts;
        }

        #endregion

        #region Reverse/Assign

        private ActionResult ReverseTransactions(List<TimeBlock> timeBlocks, List<TimePayrollScheduleTransaction> absenceScheduleTransactions = null)
        {
            if (timeBlocks.IsNullOrEmpty())
                return new ActionResult();

            List<TimePayrollTransaction> reversedPayrollTransactions = new List<TimePayrollTransaction>();
            List<TimePayrollScheduleTransaction> reversedScheduleTransactions = new List<TimePayrollScheduleTransaction>();

            #region Prereq

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            AttestStateDTO attestStateResultingPayroll = GetAttestStateFromCache(GetCompanyIntSettingFromCache(CompanySettingType.SalaryExportPayrollResultingAttestStatus));
            AttestStateDTO SalaryPaymentExportFileCreatedAttestState = GetAttestStateFromCache(GetCompanyIntSettingFromCache(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId));
            if (attestStateResultingPayroll == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8517, "Atteststatus - lägsta nivå saknas"));

            #endregion

            #region Perform

            DateTime batchReversedDate = DateTime.Now;

            foreach (EntityCollection<TimePayrollTransaction> timeBlockPayrollTransactions in timeBlocks.Select(t => t.TimePayrollTransaction))
            {
                if (!timeBlockPayrollTransactions.IsLoaded)
                    timeBlockPayrollTransactions.Load();

                List<TimePayrollTransaction> timePayrollTransactions = timeBlockPayrollTransactions.Where(i => !i.ReversedDate.HasValue && i.State == (int)SoeEntityState.Active).ToList();
                if (!timePayrollTransactions.Any())
                    continue;

                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                {
                    #region TimePayrollTransaction

                    if (timePayrollTransaction.AttestStateId != attestStateResultingPayroll.AttestStateId && timePayrollTransaction.AttestStateId != SalaryPaymentExportFileCreatedAttestState.AttestStateId)
                        return new ActionResult((int)ActionResultSave.TimeEngineReverseTransactionsError, GetText(5923, "Endast transaktioner som överförda till lön kan vändas"));

                    #region Add new transaction

                    TimePayrollTransaction reversedTimePayrollTransaction = new TimePayrollTransaction()
                    {
                        Exported = false, //Do not copy
                        ManuallyAdded = timePayrollTransaction.ManuallyAdded,
                        AutoAttestFailed = timePayrollTransaction.AutoAttestFailed,
                        IsPreliminary = timePayrollTransaction.IsPreliminary,
                        ModifiedWithNoCheckes = timePayrollTransaction.ModifiedWithNoCheckes,
                        Comment = timePayrollTransaction.Comment,
                        State = (int)SoeEntityState.Active,
                        SysPayrollTypeLevel1 = timePayrollTransaction.SysPayrollTypeLevel1,
                        SysPayrollTypeLevel2 = timePayrollTransaction.SysPayrollTypeLevel2,
                        SysPayrollTypeLevel3 = timePayrollTransaction.SysPayrollTypeLevel3,
                        SysPayrollTypeLevel4 = timePayrollTransaction.SysPayrollTypeLevel4,

                        //Reversed fields
                        Quantity = Decimal.Negate(timePayrollTransaction.Quantity),
                        Amount = timePayrollTransaction.Amount.HasValue ? Decimal.Negate(timePayrollTransaction.Amount.Value) : (decimal?)null,
                        AmountCurrency = timePayrollTransaction.AmountCurrency.HasValue ? Decimal.Negate(timePayrollTransaction.AmountCurrency.Value) : (decimal?)null,
                        AmountEntCurrency = timePayrollTransaction.AmountEntCurrency.HasValue ? Decimal.Negate(timePayrollTransaction.AmountEntCurrency.Value) : (decimal?)null,
                        AmountLedgerCurrency = timePayrollTransaction.AmountLedgerCurrency.HasValue ? Decimal.Negate(timePayrollTransaction.AmountLedgerCurrency.Value) : (decimal?)null,
                        VatAmount = timePayrollTransaction.VatAmount.HasValue ? Decimal.Negate(timePayrollTransaction.VatAmount.Value) : (decimal?)null,
                        VatAmountCurrency = timePayrollTransaction.VatAmountCurrency.HasValue ? Decimal.Negate(timePayrollTransaction.VatAmountCurrency.Value) : (decimal?)null,
                        VatAmountEntCurrency = timePayrollTransaction.VatAmountEntCurrency.HasValue ? Decimal.Negate(timePayrollTransaction.VatAmountEntCurrency.Value) : (decimal?)null,
                        VatAmountLedgerCurrency = timePayrollTransaction.VatAmountLedgerCurrency.HasValue ? Decimal.Negate(timePayrollTransaction.VatAmountLedgerCurrency.Value) : (decimal?)null,
                        ReversedDate = batchReversedDate, //Dont set IsReversed

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = timePayrollTransaction.EmployeeId,
                        TimeBlockDateId = timePayrollTransaction.TimeBlockDateId,
                        ProductId = timePayrollTransaction.ProductId,
                        AccountStdId = timePayrollTransaction.AccountStdId,
                        AttestStateId = attestStateInitialPayroll.AttestStateId,
                        TimeCodeTransactionId = null, //Set below
                        TimeBlockId = null,//Only connected to day, to prevent it from being removed when any other function is applied (ex, restore)
                        EmployeeChildId = timePayrollTransaction.EmployeeChildId,
                    };
                    SetCreatedProperties(reversedTimePayrollTransaction);
                    entities.TimePayrollTransaction.AddObject(reversedTimePayrollTransaction);
                    reversedPayrollTransactions.Add(reversedTimePayrollTransaction);

                    #region Copy extended

                    if (timePayrollTransaction.IsExtended)
                    {
                        if (!timePayrollTransaction.TimePayrollTransactionExtendedReference.IsLoaded)
                            timePayrollTransaction.TimePayrollTransactionExtendedReference.Load();

                        if (timePayrollTransaction.TimePayrollTransactionExtended != null)
                        {
                            CreateTimePayrollTransactionExtended(reversedTimePayrollTransaction, reversedTimePayrollTransaction.EmployeeId, actorCompanyId);
                            if (reversedTimePayrollTransaction.TimePayrollTransactionExtended != null)
                            {
                                reversedTimePayrollTransaction.TimePayrollTransactionExtended.TimeUnit = timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit;
                                reversedTimePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = Decimal.Negate(timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays);
                                reversedTimePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = Decimal.Negate(timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays);
                                reversedTimePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = Decimal.Negate(timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor);
                            }
                        }
                    }

                    #endregion

                    if (!timePayrollTransaction.AccountInternal.IsLoaded)
                        timePayrollTransaction.AccountInternal.Load();

                    //Set AccountInternal
                    if (reversedTimePayrollTransaction.AccountInternal == null)
                        reversedTimePayrollTransaction.AccountInternal = new EntityCollection<AccountInternal>();
                    AddAccountInternalsToTimePayrollTransaction(reversedTimePayrollTransaction, timePayrollTransaction.AccountInternal);

                    #endregion

                    #region Reverse existing transaction

                    timePayrollTransaction.IsReversed = true;
                    timePayrollTransaction.ReversedDate = batchReversedDate;
                    timePayrollTransaction.TimeBlockId = null;//Only connected to day, to prevent it from being removed when any other function is applied (ex, restore)
                    SetModifiedProperties(timePayrollTransaction);

                    #endregion

                    #endregion

                    #region TimeCodeTransaction

                    if (!timePayrollTransaction.TimeCodeTransactionReference.IsLoaded)
                        timePayrollTransaction.TimeCodeTransactionReference.Load();

                    TimeCodeTransaction timeCodeTransaction = timePayrollTransaction.TimeCodeTransaction;
                    if (timeCodeTransaction != null && timeCodeTransaction.State == (int)SoeEntityState.Active && !timePayrollTransaction.TimeCodeTransaction.IsReversed)
                    {
                        #region Add new transaction

                        TimeCodeTransaction reversedTimeCodeTransaction = new TimeCodeTransaction()
                        {
                            Type = timeCodeTransaction.Type,
                            Start = timeCodeTransaction.Start,
                            Stop = timeCodeTransaction.Stop,
                            DoNotChargeProject = timeCodeTransaction.DoNotChargeProject,
                            Comment = timeCodeTransaction.Comment,
                            ExternalComment = timeCodeTransaction.ExternalComment,
                            State = (int)SoeEntityState.Active,

                            //Reversed fields
                            Quantity = Decimal.Negate(timeCodeTransaction.Quantity),
                            InvoiceQuantity = timeCodeTransaction.InvoiceQuantity,
                            Amount = timeCodeTransaction.Amount.HasValue ? Decimal.Negate(timeCodeTransaction.Amount.Value) : (decimal?)null,
                            AmountCurrency = timeCodeTransaction.AmountCurrency.HasValue ? Decimal.Negate(timeCodeTransaction.AmountCurrency.Value) : (decimal?)null,
                            AmountEntCurrency = timeCodeTransaction.AmountEntCurrency.HasValue ? Decimal.Negate(timeCodeTransaction.AmountEntCurrency.Value) : (decimal?)null,
                            AmountLedgerCurrency = timeCodeTransaction.AmountLedgerCurrency.HasValue ? Decimal.Negate(timeCodeTransaction.AmountLedgerCurrency.Value) : (decimal?)null,
                            Vat = timeCodeTransaction.Vat.HasValue ? Decimal.Negate(timeCodeTransaction.Vat.Value) : (decimal?)null,
                            VatCurrency = timeCodeTransaction.VatCurrency.HasValue ? Decimal.Negate(timeCodeTransaction.VatCurrency.Value) : (decimal?)null,
                            VatEntCurrency = timeCodeTransaction.VatEntCurrency.HasValue ? Decimal.Negate(timeCodeTransaction.VatEntCurrency.Value) : (decimal?)null,
                            VatLedgerCurrency = timeCodeTransaction.VatLedgerCurrency.HasValue ? Decimal.Negate(timeCodeTransaction.VatLedgerCurrency.Value) : (decimal?)null,
                            ReversedDate = batchReversedDate, //Dont set IsReversed

                            //Set FK
                            TimeCodeId = timeCodeTransaction.TimeCodeId,
                            TimeRuleId = timeCodeTransaction.TimeRuleId,
                            CustomerInvoiceRowId = timeCodeTransaction.CustomerInvoiceRowId,
                            ProjectId = timeCodeTransaction.ProjectId,
                            ProjectInvoiceDayId = timeCodeTransaction.ProjectInvoiceDayId,
                            SupplierInvoiceId = timeCodeTransaction.SupplierInvoiceId,
                            TimeSheetWeekId = timeCodeTransaction.TimeSheetWeekId,
                            TimeBlockId = null,//Only connected to day, to prevent it from being removed when any other function is applied (ex, restore)
                            TimeBlockDateId = timeCodeTransaction.TimeBlockDateId,
                            ProjectTimeBlockId = timeCodeTransaction.ProjectTimeBlockId
                        };
                        SetCreatedProperties(reversedTimeCodeTransaction);
                        entities.TimeCodeTransaction.AddObject(reversedTimeCodeTransaction);

                        reversedTimePayrollTransaction.TimeCodeTransaction = reversedTimeCodeTransaction;

                        #endregion

                        #region Reverse existing transaction

                        timeCodeTransaction.IsReversed = true;
                        timeCodeTransaction.ReversedDate = batchReversedDate;
                        timeCodeTransaction.TimeBlockId = null;//Only connected to day, to prevent it from being removed when any other function is applied (ex, restore)
                        SetModifiedProperties(timeCodeTransaction);

                        #endregion
                    }

                    #endregion
                }
            }

            if (!absenceScheduleTransactions.IsNullOrEmpty() && UsePayroll())
            {
                absenceScheduleTransactions = absenceScheduleTransactions.Where(i => !i.ReversedDate.HasValue && i.Type == (int)SoeTimePayrollScheduleTransactionType.Absence).ToList();
                foreach (var absenceScheduleTransaction in absenceScheduleTransactions)
                {
                    #region Add new transaction

                    TimePayrollScheduleTransaction reversedTimePayrollScheduleTransaction = new TimePayrollScheduleTransaction()
                    {
                        State = (int)SoeEntityState.Active,
                        SysPayrollTypeLevel1 = absenceScheduleTransaction.SysPayrollTypeLevel1,
                        SysPayrollTypeLevel2 = absenceScheduleTransaction.SysPayrollTypeLevel2,
                        SysPayrollTypeLevel3 = absenceScheduleTransaction.SysPayrollTypeLevel3,
                        SysPayrollTypeLevel4 = absenceScheduleTransaction.SysPayrollTypeLevel4,
                        Type = absenceScheduleTransaction.Type,
                        TimeBlockStartTime = absenceScheduleTransaction.TimeBlockStartTime,
                        TimeBlockStopTime = absenceScheduleTransaction.TimeBlockStopTime,

                        //Reversed fields
                        Quantity = Decimal.Negate(absenceScheduleTransaction.Quantity),
                        Amount = absenceScheduleTransaction.Amount.HasValue ? Decimal.Negate(absenceScheduleTransaction.Amount.Value) : (decimal?)null,
                        AmountCurrency = absenceScheduleTransaction.AmountCurrency.HasValue ? Decimal.Negate(absenceScheduleTransaction.AmountCurrency.Value) : (decimal?)null,
                        AmountEntCurrency = absenceScheduleTransaction.AmountEntCurrency.HasValue ? Decimal.Negate(absenceScheduleTransaction.AmountEntCurrency.Value) : (decimal?)null,
                        AmountLedgerCurrency = absenceScheduleTransaction.AmountLedgerCurrency.HasValue ? Decimal.Negate(absenceScheduleTransaction.AmountLedgerCurrency.Value) : (decimal?)null,
                        VatAmount = absenceScheduleTransaction.VatAmount.HasValue ? Decimal.Negate(absenceScheduleTransaction.VatAmount.Value) : (decimal?)null,
                        VatAmountCurrency = absenceScheduleTransaction.VatAmountCurrency.HasValue ? Decimal.Negate(absenceScheduleTransaction.VatAmountCurrency.Value) : (decimal?)null,
                        VatAmountEntCurrency = absenceScheduleTransaction.VatAmountEntCurrency.HasValue ? Decimal.Negate(absenceScheduleTransaction.VatAmountEntCurrency.Value) : (decimal?)null,
                        VatAmountLedgerCurrency = absenceScheduleTransaction.VatAmountLedgerCurrency.HasValue ? Decimal.Negate(absenceScheduleTransaction.VatAmountLedgerCurrency.Value) : (decimal?)null,
                        ReversedDate = batchReversedDate, //Dont set IsReversed

                        //Set FK
                        ActorCompanyId = actorCompanyId,
                        EmployeeId = absenceScheduleTransaction.EmployeeId,
                        TimeBlockDateId = absenceScheduleTransaction.TimeBlockDateId,
                        ProductId = absenceScheduleTransaction.ProductId,
                        AccountStdId = absenceScheduleTransaction.AccountStdId,

                    };
                    SetCreatedProperties(reversedTimePayrollScheduleTransaction);
                    entities.TimePayrollScheduleTransaction.AddObject(reversedTimePayrollScheduleTransaction);
                    reversedScheduleTransactions.Add(reversedTimePayrollScheduleTransaction);

                    if (!absenceScheduleTransaction.AccountInternal.IsLoaded)
                        absenceScheduleTransaction.AccountInternal.Load();

                    //Set AccountInternal
                    if (reversedTimePayrollScheduleTransaction.AccountInternal == null)
                        reversedTimePayrollScheduleTransaction.AccountInternal = new EntityCollection<AccountInternal>();
                    AddAccountInternalsToTimePayrollScheduleTransaction(reversedTimePayrollScheduleTransaction, absenceScheduleTransaction.AccountInternal);

                    #endregion

                    #region Reverse existing transaction

                    absenceScheduleTransaction.IsReversed = true;
                    absenceScheduleTransaction.ReversedDate = batchReversedDate;
                    SetModifiedProperties(absenceScheduleTransaction);

                    #endregion
                }
            }

            ActionResult result = Save();
            result.Value = reversedPayrollTransactions;
            result.Value2 = reversedScheduleTransactions;

            #endregion

            return result;
        }

        private ActionResult AssignPayrollTransactionsToTimePeriod(List<int> timePayrollTransactionIds, List<int> timePayrollScheduleTransactionIds, TimePeriodDTO timePeriodItem, Employee employee)
        {
            ActionResult result;
            bool createdExtraPeriod = false;

            #region TimePeriod

            TimePeriod timePeriod;

            if (timePeriodItem.TimePeriodHeadId == 0)
            {
                #region Create TimePeriod

                TimePeriodHead timePeriodHead = GetDefaultTimePeriodHeadWithPeriods(employee.GetPayrollGroupId());
                if (timePeriodHead == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, "TimePeriodHead");

                timePeriod = new TimePeriod()
                {
                    Name = timePeriodItem.Name,
                    PaymentDate = timePeriodItem.PaymentDate,
                    StartDate = timePeriodItem.StartDate,
                    StopDate = timePeriodItem.StopDate,
                    PayrollStartDate = null,
                    PayrollStopDate = null,
                    ExtraPeriod = timePeriodItem.ExtraPeriod,

                    //References
                    TimePeriodHead = timePeriodHead,
                };
                SetCreatedProperties(timePeriod);
                entities.TimePeriod.AddObject(timePeriod);

                createdExtraPeriod = true;

                if (!HasEmployeePlacement(employee.EmployeeId, timePeriod))
                    return new ActionResult((int)ActionResultSave.TimePeriodEmployeeNotPlaced, GetText(8693, "Period hittades inte"));

                result = Save();
                if (!result.Success)
                    return result;

                #endregion
            }
            else
            {
                #region Use existing TimePeriod

                timePeriod = GetTimePeriodWithHead(timePeriodItem.TimePeriodId);
                if (timePeriod == null)
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));

                if (IsEmployeeTimePeriodLockedForChanges(employee.EmployeeId, timePeriodId: timePeriod.TimePeriodId))
                    return new ActionResult((int)ActionResultSave.TimePeriodIsLocked, GetText(8689, "Du måste öpnna upp perioden för att kunna lägga till en transaktion."));

                #endregion
            }

            #endregion

            #region TimePayrollTransaction

            foreach (var timePayrollTransactionId in timePayrollTransactionIds)
            {
                TimePayrollTransaction timePayrollTransaction = GetTimePayrollTransactionWithTimeBlockDateAndTimePeriod(timePayrollTransactionId);
                if (timePayrollTransaction == null || timePayrollTransaction.TimeBlockDate == null)
                    continue;

                //Update TimePayrollTransaction
                timePayrollTransaction.TimePeriod = timePeriod;
                SetModifiedProperties(timePayrollTransaction);
            }

            result = Save();
            if (!result.Success)
                return result;

            #endregion

            #region TimePayrollTransaction

            List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions = GetTimePayrollScheduleTransactions(employee.EmployeeId, timePayrollScheduleTransactionIds);
            foreach (var timePayrollScheduleTransaction in timePayrollScheduleTransactions)
            {
                //Update TimePayrollScheduleTransaction
                timePayrollScheduleTransaction.TimePeriod = timePeriod;
                SetModifiedProperties(timePayrollScheduleTransaction);
            }

            result = Save();
            if (!result.Success)
                return result;

            #endregion

            if (result.Success)
            {
                result.BooleanValue = createdExtraPeriod;
                ActivateWarningPayrollPeriodHasChanged(employee.EmployeeId, timePeriod.TimePeriodId);
            }

            return result;
        }

        #endregion

        #region SupplementCharge

        private ActionResult ReCalculateSupplementChargeDebet(Employee employee, List<TimeBlockDate> timeBlockDates, TimePeriod timePeriod, List<TimePayrollTransaction> deletedTransactions, List<TimePayrollTransaction> periodTransactions, bool deleteOldTransactons, PayrollProduct payrollProductSupplementChargeDebet)
        {
            if (!UsePayroll())
                return new ActionResult(true);
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            if (payrollProductSupplementChargeDebet == null)
                payrollProductSupplementChargeDebet = GetPayrollProductSupplementChargeDebet();
            if (payrollProductSupplementChargeDebet == null)
                return new ActionResult(true);

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            ActionResult result = new ActionResult();
            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();

            foreach (TimeBlockDate timeBlockDate in timeBlockDates)
            {
                result = ReCalculateSupplementChargeDebet(employee, timeBlockDate, timePeriod, deletedTransactions.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList(), periodTransactions.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList(), payrollProductSupplementChargeDebet, attestStateInitialPayroll.AttestStateId, deleteOldTransactons);
                if (!result.Success)
                    return result;

                if (result.Value is List<TimePayrollTransaction> supplementChargeDebetTransactions)
                    newTransactions.AddRange(supplementChargeDebetTransactions);
            }

            result.Value = newTransactions;
            return result;
        }
        private ActionResult ReCalculateSupplementChargeDebet(Employee employee, TimeBlockDate timeBlockDate, TimePeriod timePeriod, List<TimePayrollTransaction> deletedTransactions, List<TimePayrollTransaction> transactionsForDate, PayrollProduct payrollProductSupplementChargeDebet, int attestStateId, bool deleteOldTransactons)
        {
            if (!UsePayroll())
                return new ActionResult(true);
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            List<TimePayrollTransaction> reusableTransactions = deletedTransactions ?? new List<TimePayrollTransaction>();

            #region Get existing supplementCharge transactions and delete them

            if (deleteOldTransactons)
            {
                List<TimePayrollTransaction> supplementChargeTransactionsForDate = GetTimePayrollTransactionsForDay(employee.EmployeeId, timeBlockDate.TimeBlockDateId, TermGroup_SysPayrollType.SE_SupplementChargeDebit, timePeriod);
                foreach (TimePayrollTransaction supplementChargeTransaction in supplementChargeTransactionsForDate)
                {
                    ChangeEntityState(supplementChargeTransaction, SoeEntityState.Deleted);
                }
                reusableTransactions = supplementChargeTransactionsForDate;
            }

            #endregion

            #region Get transactions that supplement charge will be calculated on

            List<TimePayrollTransaction> grossSalaryAndBenefitsTransactions = null;
            if (transactionsForDate != null)
                grossSalaryAndBenefitsTransactions = transactionsForDate
                    .Where(x => x.IsSupplementChargeBasis()).ToList();
            else
                grossSalaryAndBenefitsTransactions = GetTimePayrollTransactionsForDayWithAccountInternal(employee.EmployeeId, timeBlockDate.TimeBlockDateId, timePeriod)
                    .Where(x => x.IsSupplementChargeBasis()).ToList();

            List<TimePayrollTransaction> transactionBasisForSupplementCharge = new List<TimePayrollTransaction>();
            List<PayrollProduct> products = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(grossSalaryAndBenefitsTransactions);
            foreach (TimePayrollTransaction grossSalaryAndBenefitsTransaction in grossSalaryAndBenefitsTransactions)
            {
                PayrollProduct payrollProduct = products.FirstOrDefault(x => x.ProductId == grossSalaryAndBenefitsTransaction.ProductId);
                if (payrollProduct == null)
                    continue;

                PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, timeBlockDate);
                if (payrollProductSetting != null && payrollProductSetting.CalculateSupplementCharge)
                    transactionBasisForSupplementCharge.Add(grossSalaryAndBenefitsTransaction);
            }

            #endregion

            #region Find matching transactions

            while (transactionBasisForSupplementCharge.Any())
            {
                TimePayrollTransaction firstSupplementChargeTransaction = transactionBasisForSupplementCharge.FirstOrDefault();

                List<TimePayrollTransaction> matchingTransactions = new List<TimePayrollTransaction>();

                foreach (var item in transactionBasisForSupplementCharge.ToList())
                {
                    if (firstSupplementChargeTransaction != null && firstSupplementChargeTransaction.AccountInternal.ToList().Match(item.AccountInternal.ToList()))
                        matchingTransactions.Add(item);
                }

                if (matchingTransactions.Any())
                {
                    matchingTransactions.ForEach(i => transactionBasisForSupplementCharge.Remove(i));

                    decimal supplementChargeTransactionsAmount = matchingTransactions.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
                    if (supplementChargeTransactionsAmount != 0)
                    {
                        //Calculate supplement charge
                        decimal supplementChargeAmount = PayrollManager.CalculateSupplementChargeSE(entities, actorCompanyId, timePeriod.PaymentDate.Value, supplementChargeTransactionsAmount, employee, true, payrollGroupAccountStds: GetPayrollGroupAccountsFromCache());
                        supplementChargeAmount = Decimal.Round(supplementChargeAmount, 2, MidpointRounding.AwayFromZero);
                        if (supplementChargeAmount == 0)
                            continue;

                        //Create or reuse transaction
                        TimePayrollTransaction supplementChargeTransaction = CreateOrUpdateTimePayrollTransaction(payrollProductSupplementChargeDebet, timeBlockDate, employee, timePeriod.TimePeriodId, attestStateId, 1, supplementChargeAmount, supplementChargeAmount, 0, String.Empty, reusableTransactions.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList(), applyAccounting: false);
                        if (supplementChargeTransaction.TimePayrollTransactionId != 0 && reusableTransactions.Any(x => x.TimePayrollTransactionId == supplementChargeTransaction.TimePayrollTransactionId))
                            reusableTransactions.RemoveAll(s => s.TimePayrollTransactionId == supplementChargeTransaction.TimePayrollTransactionId);

                        // Accounting
                        ApplyAccountingOnTimePayrollTransaction(supplementChargeTransaction, employee, timeBlockDate.Date, payrollProductSupplementChargeDebet, setAccountInternal: false);
                        supplementChargeTransaction.AccountInternal?.Clear();
                        AddAccountInternalsToTimePayrollTransaction(supplementChargeTransaction, firstSupplementChargeTransaction.AccountInternal);

                        newTransactions.Add(supplementChargeTransaction);
                    }
                }
            }

            #endregion

            ActionResult result = Save();
            if (!result.Success)
                return result;

            result.Value = newTransactions;
            return result;
        }
        private ActionResult CreateSupplementChargeDebetScheduleTransactions(Employee employee, TimeBlockDate timeBlockDate, TimePeriod timePeriod, SoeTimePayrollScheduleTransactionType? transactionType, PayrollProduct payrollProductSupplementChargeDebet, int creditAccountId)
        {
            if (!UsePayroll())
                return new ActionResult(true);
            if (payrollProductSupplementChargeDebet == null)
                return new ActionResult(true);
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            //Get transactions that supplement charge will be calculated on
            List<TimePayrollScheduleTransaction> grossSalaryAndBenefitsTransactions = GetTimePayrollScheduleTransactionsWithAccountingAndDim(employee.EmployeeId, timeBlockDate.TimeBlockDateId, timePeriod, transactionType).GetGrossSalaryAndBenefitTransactions();
            CreateSupplementChargeDebetScheduleTransactions(employee, timePeriod, timeBlockDate, payrollProductSupplementChargeDebet, creditAccountId, grossSalaryAndBenefitsTransactions);

            return Save();
        }
        private void CreateSupplementChargeDebetScheduleTransactions(Employee employee, TimePeriod timePeriod, TimeBlockDate timeBlockDate, PayrollProduct payrollProductSupplementChargeDebet, int creditAccountId, List<TimePayrollScheduleTransaction> grossSalaryAndBenefitsTransactions)
        {
            List<TimePayrollScheduleTransaction> newTransactions = new List<TimePayrollScheduleTransaction>();

            #region Get transactions that supplement charge will be calculated on

            List<TimePayrollScheduleTransaction> transactionBasisForSupplementCharge = new List<TimePayrollScheduleTransaction>();

            List<PayrollProduct> products = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(grossSalaryAndBenefitsTransactions);
            foreach (TimePayrollScheduleTransaction transaction in grossSalaryAndBenefitsTransactions)
            {
                PayrollProduct payrollProduct = products.FirstOrDefault(x => x.ProductId == transaction.ProductId);
                if (payrollProduct == null)
                    continue;

                PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, timeBlockDate);
                if (payrollProductSetting != null && payrollProductSetting.CalculateSupplementCharge)
                    transactionBasisForSupplementCharge.Add(transaction);
            }

            #endregion

            #region Find matching transactions

            foreach (var transactionBasisForSupplementChargeByType in transactionBasisForSupplementCharge.GroupBy(x => x.Type))
            {
                List<TimePayrollScheduleTransaction> supplementChargeTransactions = transactionBasisForSupplementChargeByType.ToList();
                while (supplementChargeTransactions.Any())
                {
                    TimePayrollScheduleTransaction firstSupplementChargeTransaction = supplementChargeTransactions.FirstOrDefault();
                    List<TimePayrollScheduleTransaction> matchingTransactions = new List<TimePayrollScheduleTransaction>();
                    foreach (TimePayrollScheduleTransaction supplementChargeTransaction in supplementChargeTransactions.ToList())
                    {
                        if (firstSupplementChargeTransaction != null && firstSupplementChargeTransaction.AccountInternal.ToList().Match(supplementChargeTransaction.AccountInternal.ToList()))
                            matchingTransactions.Add(supplementChargeTransaction);
                    }

                    matchingTransactions.ForEach(i => supplementChargeTransactions.Remove(i));
                    if (matchingTransactions.Any())
                    {
                        decimal supplementChargeTransactionsAmount = matchingTransactions.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
                        if (supplementChargeTransactionsAmount != 0)
                        {
                            //Calculate employment tax
                            decimal supplementChargeAmount = PayrollManager.CalculateSupplementChargeSE(entities, actorCompanyId, timePeriod.PaymentDate.Value, supplementChargeTransactionsAmount, employee, true, payrollGroupAccountStds: GetPayrollGroupAccountsFromCache());
                            supplementChargeAmount = Decimal.Round(supplementChargeAmount, 2, MidpointRounding.AwayFromZero);
                            if (supplementChargeAmount == 0)
                                continue;

                            //Create supplementcharge TimePayrollScheduleTransaction
                            TimePayrollScheduleTransaction employmentTaxScheduleTransaction = CreateTimePayrollScheduleTransaction(payrollProductSupplementChargeDebet, timeBlockDate, 1, supplementChargeAmount, 0, supplementChargeAmount, transactionBasisForSupplementChargeByType.Key ?? 0, employee.EmployeeId, creditAccountId, timePeriod.TimePeriodId);
                            if (employmentTaxScheduleTransaction != null)
                            {
                                ApplyAccountingOnTimePayrollScheduleTransaction(employmentTaxScheduleTransaction, employee, timeBlockDate.Date, payrollProductSupplementChargeDebet, setAccountStd: true, setAccountInternal: false);
                                AddAccountInternalsToTimePayrollScheduleTransaction(employmentTaxScheduleTransaction, firstSupplementChargeTransaction?.AccountInternal);
                                newTransactions.Add(employmentTaxScheduleTransaction);
                            }
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Tax

        private ActionResult ReCalculateEmploymentTaxDebet(Employee employee, List<TimeBlockDate> timeBlockDates, TimePeriod timePeriod, List<TimePayrollTransaction> deletedPayrollTransactions, List<TimePayrollTransaction> payrollTransactionsForPeriod, List<TimePayrollScheduleTransaction> scheduleTransactionsForPeriod, bool deleteOldTransactons, bool applyEmploymentHasEnded = false, int sysCountryId = 0, PayrollProduct payrollProductEmploymentTaxDebet = null)
        {
            if (!UsePayroll())
                return new ActionResult(true);
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            if (payrollProductEmploymentTaxDebet == null)
                payrollProductEmploymentTaxDebet = GetPayrollProductEmploymentTaxDebet();
            if (payrollProductEmploymentTaxDebet == null)
                return new ActionResult(true);

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            ActionResult result = new ActionResult();
            List<TimePayrollTransaction> newPayrollTransactions = new List<TimePayrollTransaction>();
            List<TimePayrollScheduleTransaction> newScheduleTransactions = new List<TimePayrollScheduleTransaction>();

            decimal accEmpTaxBasis = GetEmploymentBasisAmountFromOtherPaymentsSameMonth(employee, timePeriod);

            foreach (var timeBlockDate in timeBlockDates.OrderBy(x => x.Date))
            {
                List<TimePayrollScheduleTransaction> currentScheduleTransactionsForDate = scheduleTransactionsForPeriod.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).GetGrossSalaryAndBenefitTransactions();
                List<TimePayrollScheduleTransaction> newScheduleTransactionsForDate = CreateEmploymentTaxDebetScheduleTransactions(employee, timeBlockDate, timePeriod, currentScheduleTransactionsForDate, payrollProductEmploymentTaxDebet, 0, ref accEmpTaxBasis);

                result = Save();
                if (!result.Success)
                    return result;

                newScheduleTransactions.AddRange(newScheduleTransactionsForDate);

                List<TimePayrollTransaction> currentPayrollTransactionsForDate = payrollTransactionsForPeriod.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                List<TimePayrollTransaction> deletedPayrollTransactionsForDate = deletedPayrollTransactions.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();

                result = ReCalculateEmploymentTaxDebet(employee, timeBlockDate, timePeriod, deletedPayrollTransactionsForDate, currentPayrollTransactionsForDate, payrollProductEmploymentTaxDebet, attestStateInitialPayroll.AttestStateId, deleteOldTransactons, ref accEmpTaxBasis, applyEmploymentHasEnded, sysCountryId: sysCountryId);
                if (!result.Success)
                    return result;

                if (result.Value is List<TimePayrollTransaction> employmentTaxDebetTransactions)
                    newPayrollTransactions.AddRange(employmentTaxDebetTransactions);
            }

            result.Value = newPayrollTransactions;
            result.Value2 = newScheduleTransactions;
            result.DecimalValue = accEmpTaxBasis;
            return result;
        }
        private ActionResult ReCalculateEmploymentTaxDebet(Employee employee, TimeBlockDate timeBlockDate, TimePeriod timePeriod, List<TimePayrollTransaction> deletedTransactions, List<TimePayrollTransaction> payrollTransactionsForDate, PayrollProduct payrollProductEmploymentTaxDebit, int attestStateId, bool deleteOldTransactons, ref decimal accEmpTaxBasis, bool applyEmploymentHasEnded = false, int sysCountryId = 0)
        {
            if (!UsePayroll())
                return new ActionResult(true);
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            List<TimePayrollTransaction> newPayrollTransactions = new List<TimePayrollTransaction>();
            List<TimePayrollTransaction> reusablePayrollTransactions = deletedTransactions ?? new List<TimePayrollTransaction>();

            #region Perform

            if (deleteOldTransactons)
            {
                //Get existing employmentTax transactions and delete them
                List<TimePayrollTransaction> currentEmploymentTaxTransactions = GetTimePayrollTransactionsForDay(employee.EmployeeId, timeBlockDate.TimeBlockDateId, TermGroup_SysPayrollType.SE_EmploymentTaxDebit, timePeriod);
                foreach (TimePayrollTransaction currentEmploymentTaxTransaction in currentEmploymentTaxTransactions)
                {
                    ChangeEntityState(currentEmploymentTaxTransaction, SoeEntityState.Deleted);
                }

                reusablePayrollTransactions = currentEmploymentTaxTransactions;
            }

            //Get transactions that employment tax will be calculated on
            List<TimePayrollTransaction> payrollTransactionBasisForEmploymentTax = new List<TimePayrollTransaction>();
            if (payrollTransactionsForDate != null)
                payrollTransactionBasisForEmploymentTax = payrollTransactionsForDate.Where(x => x.IsEmploymentTaxBasis()).ToList();
            else
                payrollTransactionBasisForEmploymentTax = GetTimePayrollTransactionsForDayWithAccountInternal(employee.EmployeeId, timeBlockDate.TimeBlockDateId, timePeriod)
                    .Where(x => x.IsEmploymentTaxBasis()).ToList();

            List<PayrollProduct> payrollProducts = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(payrollTransactionBasisForEmploymentTax);
            foreach (TimePayrollTransaction payrollTransaction in payrollTransactionBasisForEmploymentTax.ToList())
            {
                PayrollProduct product = payrollProducts.FirstOrDefault(x => x.ProductId == payrollTransaction.ProductId);
                if (product != null && !product.UseInPayroll)
                    payrollTransactionBasisForEmploymentTax.Remove(payrollTransaction);
            }

            //Find macthing transactions
            while (payrollTransactionBasisForEmploymentTax.Any())
            {
                List<TimePayrollTransaction> matchingPayrollTransactions = new List<TimePayrollTransaction>();
                TimePayrollTransaction firstPayrollTransaction = payrollTransactionBasisForEmploymentTax.FirstOrDefault();
                foreach (TimePayrollTransaction payrollTransaction in payrollTransactionBasisForEmploymentTax.ToList())
                {
                    if (firstPayrollTransaction != null && firstPayrollTransaction.AccountInternal.ToList().Match(payrollTransaction.AccountInternal.ToList()))
                        matchingPayrollTransactions.Add(payrollTransaction);
                }

                if (matchingPayrollTransactions.Any())
                {
                    matchingPayrollTransactions.ForEach(i => payrollTransactionBasisForEmploymentTax.Remove(i));

                    decimal employmentTaxTransactionsAmount = matchingPayrollTransactions.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
                    if (employmentTaxTransactionsAmount != 0)
                    {
                        //Calculate employment tax
                        decimal employmentTaxAmountDebit = PayrollManager.CalculateEmploymentTaxSE(entities, actorCompanyId, timePeriod.PaymentDate.Value, employmentTaxTransactionsAmount, employee, applyEmploymentHasEnded, sysCountryId, accEmpTaxBasis: accEmpTaxBasis);
                        employmentTaxAmountDebit = Decimal.Round(employmentTaxAmountDebit, 2, MidpointRounding.AwayFromZero);
                        accEmpTaxBasis += employmentTaxTransactionsAmount;
                        if (employmentTaxAmountDebit == 0)
                            continue;

                        //Create or reuse transaction
                        TimePayrollTransaction employmentTaxTransaction = CreateOrUpdateTimePayrollTransaction(payrollProductEmploymentTaxDebit, timeBlockDate, employee, timePeriod.TimePeriodId, attestStateId, 1, employmentTaxAmountDebit, employmentTaxAmountDebit, 0, String.Empty, reusablePayrollTransactions.Where(x => x.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList(), applyAccounting: false);
                        if (employmentTaxTransaction.TimePayrollTransactionId != 0 && reusablePayrollTransactions.Any(x => x.TimePayrollTransactionId == employmentTaxTransaction.TimePayrollTransactionId))
                            reusablePayrollTransactions.RemoveAll(s => s.TimePayrollTransactionId == employmentTaxTransaction.TimePayrollTransactionId);

                        //Accounting
                        ApplyAccountingOnTimePayrollTransaction(employmentTaxTransaction, employee, timeBlockDate.Date, payrollProductEmploymentTaxDebit, setAccountInternal: false);
                        employmentTaxTransaction.AccountInternal?.Clear();
                        AddAccountInternalsToTimePayrollTransaction(employmentTaxTransaction, firstPayrollTransaction.AccountInternal);

                        newPayrollTransactions.Add(employmentTaxTransaction);
                    }
                }
            }

            ActionResult result = Save();
            if (!result.Success)
                return result;

            result.Value = newPayrollTransactions;

            #endregion

            return result;
        }
        private ActionResult CalculateEmploymentTaxDebet(Employee employee, TimeBlockDate timeBlockDate, TimePeriod timePeriod, List<TimePayrollTransaction> payrollTransactionBasisForEmploymentTax, decimal accEmpTaxBasis)
        {
            #region Prereq

            if (!UsePayroll())
                return new ActionResult(true);

            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            PayrollProduct payrollProductEmploymentTaxDebit = GetPayrollProductEmploymentTaxDebet();
            if (payrollProductEmploymentTaxDebit == null)
                return new ActionResult(true);

            AttestStateDTO attestStateInitialPayroll = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitialPayroll == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);
            int attestStateId = attestStateInitialPayroll.AttestStateId;

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();

            #endregion

            #region Perform

            //Find macthing transactions
            while (payrollTransactionBasisForEmploymentTax.Any())
            {
                var firstItem = payrollTransactionBasisForEmploymentTax.FirstOrDefault();
                List<TimePayrollTransaction> matchingTransactions = new List<TimePayrollTransaction>();

                foreach (var item in payrollTransactionBasisForEmploymentTax.ToList())
                {
                    if (firstItem != null && firstItem.AccountInternal.ToList().Match(item.AccountInternal.ToList()))
                        matchingTransactions.Add(item);
                }

                matchingTransactions.ForEach(i => payrollTransactionBasisForEmploymentTax.Remove(i));

                if (matchingTransactions.Any())
                {
                    decimal employmentTaxTransactionsAmount = matchingTransactions.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
                    if (employmentTaxTransactionsAmount != 0)
                    {
                        //Calculate employment tax
                        decimal employmentTaxAmountDebit = PayrollManager.CalculateEmploymentTaxSE(entities, actorCompanyId, timePeriod.PaymentDate.Value, employmentTaxTransactionsAmount, employee, accEmpTaxBasis: accEmpTaxBasis);
                        employmentTaxAmountDebit = Decimal.Round(employmentTaxAmountDebit, 2, MidpointRounding.AwayFromZero);
                        accEmpTaxBasis += employmentTaxTransactionsAmount;
                        if (employmentTaxAmountDebit == 0)
                            continue;

                        //Create employmenttax transaction
                        TimePayrollTransaction employmentTaxTransaction = CreateTimePayrollTransaction(payrollProductEmploymentTaxDebit, timeBlockDate, 1, employmentTaxAmountDebit, 0, employmentTaxAmountDebit, String.Empty, attestStateId, timePeriod.TimePeriodId, employee.EmployeeId);

                        //Accounting
                        ApplyAccountingOnTimePayrollTransaction(employmentTaxTransaction, employee, timeBlockDate.Date, payrollProductEmploymentTaxDebit, setAccountInternal: false);
                        AddAccountInternalsToTimePayrollTransaction(employmentTaxTransaction, firstItem?.AccountInternal);

                        newTransactions.Add(employmentTaxTransaction);
                    }
                }
            }

            ActionResult result = Save();
            if (!result.Success)
                return result;

            result.Value = newTransactions;

            #endregion

            return result;
        }
        private ActionResult CreateEmploymentTaxDebetScheduleTransactions(Employee employee, TimeBlockDate timeBlockDate, TimePeriod timePeriod, SoeTimePayrollScheduleTransactionType? transactionType, PayrollProduct payrollProductEmploymentTaxDebet, int creditAccountId)
        {
            if (!UsePayroll())
                return new ActionResult(true);
            if (payrollProductEmploymentTaxDebet == null)
                return new ActionResult(true);
            if (!timePeriod.PaymentDate.HasValue)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingPaymentDate);

            //Get transactions that employment tax will be calculated on
            List<TimePayrollScheduleTransaction> scheduleTransactionBasisForEmploymentTax = GetTimePayrollScheduleTransactionsWithAccountingAndDim(employee.EmployeeId, timeBlockDate.TimeBlockDateId, timePeriod, transactionType).Where(x => x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary || x.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_Benefit).ToList();

            decimal accEmpTaxBasis = GetEmploymentBasisAmountFromOtherPaymentsSameMonth(employee, timePeriod);
            CreateEmploymentTaxDebetScheduleTransactions(employee, timeBlockDate, timePeriod, scheduleTransactionBasisForEmploymentTax, payrollProductEmploymentTaxDebet, creditAccountId, ref accEmpTaxBasis);

            return Save();
        }
        private List<TimePayrollScheduleTransaction> CreateEmploymentTaxDebetScheduleTransactions(Employee employee, TimeBlockDate timeBlockDate, TimePeriod timePeriod, List<TimePayrollScheduleTransaction> scheduleTransactionBasisForEmploymentTax, PayrollProduct payrollProductEmploymentTaxDebet, int creditAccountId, ref decimal accEmpTaxBasis)
        {
            List<TimePayrollScheduleTransaction> newTransactions = new List<TimePayrollScheduleTransaction>();

            List<PayrollProduct> payrollProducts = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(scheduleTransactionBasisForEmploymentTax);
            foreach (TimePayrollScheduleTransaction scheduleTransaction in scheduleTransactionBasisForEmploymentTax.ToList())
            {
                PayrollProduct product = payrollProducts.FirstOrDefault(x => x.ProductId == scheduleTransaction.ProductId);
                if (product != null && !product.UseInPayroll)
                    scheduleTransactionBasisForEmploymentTax.Remove(scheduleTransaction);
            }

            foreach (var scheduleTransactionsByType in scheduleTransactionBasisForEmploymentTax.GroupBy(x => x.Type))
            {
                List<TimePayrollScheduleTransaction> scheduleTransactionBasisForEmploymentTaxGroup = scheduleTransactionsByType.ToList();
                while (scheduleTransactionBasisForEmploymentTaxGroup.Any())
                {
                    TimePayrollScheduleTransaction firstScheduleTransaction = scheduleTransactionBasisForEmploymentTaxGroup.FirstOrDefault();
                    List<TimePayrollScheduleTransaction> matchingScheduleTransactions = new List<TimePayrollScheduleTransaction>();
                    foreach (TimePayrollScheduleTransaction scheduleTransaction in scheduleTransactionBasisForEmploymentTaxGroup.ToList())
                    {
                        if (firstScheduleTransaction != null && firstScheduleTransaction.AccountInternal.ToList().Match(scheduleTransaction.AccountInternal.ToList()))
                            matchingScheduleTransactions.Add(scheduleTransaction);
                    }

                    matchingScheduleTransactions.ForEach(i => scheduleTransactionBasisForEmploymentTaxGroup.Remove(i));
                    if (matchingScheduleTransactions.Any())
                    {
                        decimal employmentTaxTransactionsAmount = matchingScheduleTransactions.Where(i => i.Amount.HasValue).Sum(i => i.Amount.Value);
                        if (employmentTaxTransactionsAmount != 0)
                        {
                            //Calculate employment tax
                            decimal employmentTaxAmountCredit = PayrollManager.CalculateEmploymentTaxSE(entities, actorCompanyId, timePeriod.PaymentDate.Value, employmentTaxTransactionsAmount, employee, accEmpTaxBasis: accEmpTaxBasis);
                            accEmpTaxBasis += employmentTaxTransactionsAmount;
                            employmentTaxAmountCredit = Decimal.Round(employmentTaxAmountCredit, 2, MidpointRounding.AwayFromZero);
                            if (employmentTaxAmountCredit == 0)
                                continue;

                            //Create employmenttax TimePayrollScheduleTransaction
                            TimePayrollScheduleTransaction scheduleTransactionEmploymentTax = CreateTimePayrollScheduleTransaction(payrollProductEmploymentTaxDebet, timeBlockDate, 1, employmentTaxAmountCredit, 0, employmentTaxAmountCredit, scheduleTransactionsByType.Key ?? 0, employee.EmployeeId, creditAccountId, timePeriod.TimePeriodId);
                            if (scheduleTransactionEmploymentTax != null)
                            {
                                ApplyAccountingOnTimePayrollScheduleTransaction(scheduleTransactionEmploymentTax, employee, timeBlockDate.Date, payrollProductEmploymentTaxDebet, setAccountStd: true, setAccountInternal: false);
                                AddAccountInternalsToTimePayrollScheduleTransaction(scheduleTransactionEmploymentTax, firstScheduleTransaction?.AccountInternal);
                                newTransactions.Add(scheduleTransactionEmploymentTax);
                            }
                        }
                    }
                }
            }

            return newTransactions;
        }
        private decimal GetEmploymentBasisAmountFromOtherPaymentsSameMonth(Employee employee, TimePeriod timePeriod)
        {
            decimal accEmpTaxBasis = 0;
            if (timePeriod.PaymentDate.HasValue && PayrollManager.Apply19to23Rule(timePeriod.PaymentDate.Value, EmployeeManager.GetEmployeeAge(employee, timePeriod.PaymentDate)))
            {
                List<EmployeeTimePeriod> employeePeriodsForMonth = TimePeriodManager.GetLockedEmployeeTimePeriodsSameMonth(entities, employee.EmployeeId, actorCompanyId, timePeriod.PaymentDate.Value);
                if (!employeePeriodsForMonth.IsNullOrEmpty())
                {
                    foreach (var employeTimePeriod in employeePeriodsForMonth.Where(w => w.TimePeriodId != timePeriod.TimePeriodId))
                    {
                        var employeeTimePeriodWithValues = TimePeriodManager.GetEmployeeTimePeriodWithValues(entities, employeTimePeriod.TimePeriodId, employee.EmployeeId, actorCompanyId);
                        if (employeeTimePeriodWithValues != null)
                            accEmpTaxBasis += employeeTimePeriodWithValues.GetEmploymentTaxBasisSum();
                    }
                }
            }
            return accEmpTaxBasis;
        }
        private int CalculateTaxSE(Employee employee, DateTime date, decimal amount, decimal baseAmount = 0, bool applyEmploymentHasEnded = false, decimal oneTimeTaxTransactionsAmount = 0)
        {
            int sysCountryId = GetSysCountryFromCache();
            return PayrollManager.CalculateTaxSE(entities, actorCompanyId, date, amount, employee, baseAmount, applyEmploymentHasEnded, sysCountryId, oneTimeTaxTransactionsAmount);
        }
        private int CalculateOneTimeTaxSE(Employee employee, DateTime transactionDate, DateTime calculationDate, decimal amount, bool applyEmploymentHasEnded = false, decimal tableTaxTransactionsAmount = 0)
        {
            EvaluatePayrollPriceFormulaInputDTO priceFormulaInputDTO = GetEvaluatePriceFormulaInputDTOFromCache();
            return (int)PayrollManager.CalculateOneTimeTaxSE(entities, actorCompanyId, transactionDate, calculationDate, amount, employee, applyEmploymentHasEnded: applyEmploymentHasEnded, iDTO: priceFormulaInputDTO, tableTaxTransactionsAmount);
        }

        #endregion

        #region TimePayrollTransaction rounding

        private ActionResult CreateRoundingTransactions(List<TimePayrollTransaction> timePayrollTransactions, List<TimePayrollScheduleTransaction> timePayrollScheduleTransactions, List<TimeBlockDate> timeBlockDates, Employee employee, TimePeriod timePeriod, TimeBlockDate lastDateInPeriod)
        {
            AttestStateDTO attestState = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestState == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            List<PayrollProductRoundingDTO> roundingDTOs = new List<PayrollProductRoundingDTO>();
            List<PayrollProductSetting> settings = new List<PayrollProductSetting>();
            List<PayrollProduct> payrollProductsAll = GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollTransactions);
            payrollProductsAll.AddRange(GetPayrollProductWithSettingsAndAccountInternalsAndStdsFromCache(timePayrollScheduleTransactions));

            foreach (var timeBlockDate in timeBlockDates)
            {
                var timePayrollTransactionsForDate = timePayrollTransactions.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                var timePayrollScheduleTransactionsForDate = timePayrollScheduleTransactions.Where(i => i.TimeBlockDateId == timeBlockDate.TimeBlockDateId).ToList();
                var timePayrollScheduleTransactionsForDateTypeAbcense = timePayrollScheduleTransactionsForDate.Where(x => x.Type.HasValue && x.Type.Value == (int)SoeTimePayrollScheduleTransactionType.Absence).ToList();

                EmployeeGroup employeeGroup = GetEmployeeGroup(employee, timeBlockDate.Date);
                if (employeeGroup == null)
                    continue;

                if ((!employeeGroup.AutogenTimeblocks && !timePayrollTransactionsForDate.Any()) || timePayrollScheduleTransactionsForDateTypeAbcense.Any())
                {
                    #region Calculate sums from scheduletransactions

                    List<IGrouping<int, TimePayrollScheduleTransaction>> timePayrollScheduleTransactionsForProductAndDate = null;
                    if (timePayrollScheduleTransactionsForDateTypeAbcense.Any())
                        timePayrollScheduleTransactionsForProductAndDate = timePayrollScheduleTransactionsForDateTypeAbcense.GroupBy(i => i.ProductId).ToList();
                    else
                        timePayrollScheduleTransactionsForProductAndDate = timePayrollScheduleTransactionsForDate.GroupBy(i => i.ProductId).ToList();

                    foreach (var timePayrollScheduleTransactionForProductAndDate in timePayrollScheduleTransactionsForProductAndDate)
                    {
                        var payrollProduct = payrollProductsAll.FirstOrDefault(x => x.ProductId == timePayrollScheduleTransactionForProductAndDate.Key);
                        if (payrollProduct == null)
                            continue;

                        //PayrollProductSetting for Employee
                        PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, timeBlockDate);
                        if (payrollProductSetting != null)
                        {
                            if (!settings.Any(x => x.PayrollProductSettingId == payrollProductSetting.PayrollProductSettingId))
                                settings.Add(payrollProductSetting);

                            foreach (var transaction in timePayrollScheduleTransactionForProductAndDate)
                            {
                                roundingDTOs.Add(new PayrollProductRoundingDTO()
                                {
                                    PayrollProductSettingId = payrollProductSetting.PayrollProductSettingId,
                                    ProductId = payrollProduct.ProductId,
                                    Amount = transaction.Amount ?? 0,
                                    Quantity = transaction.Quantity,
                                    AccountStdId = transaction.AccountStdId,
                                    AccountInternal = transaction.AccountInternal.ToList(),
                                });
                            }
                        }
                    }

                    #endregion
                }

                #region Calculate sums from payrolltransactions

                var timePayrollTransactionsForProductAndDate = timePayrollTransactionsForDate.GroupBy(i => i.ProductId).ToList();
                foreach (var timePayrollTransactionForProductAndDate in timePayrollTransactionsForProductAndDate)
                {
                    var payrollProduct = payrollProductsAll.FirstOrDefault(x => x.ProductId == timePayrollTransactionForProductAndDate.Key);
                    if (payrollProduct == null)
                        continue;

                    //PayrollProductSetting for Employee
                    PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employee, timeBlockDate);
                    if (payrollProductSetting != null)
                    {
                        if (!settings.Any(x => x.PayrollProductSettingId == payrollProductSetting.PayrollProductSettingId))
                            settings.Add(payrollProductSetting);

                        foreach (var transaction in timePayrollTransactionForProductAndDate)
                        {
                            roundingDTOs.Add(new PayrollProductRoundingDTO()
                            {
                                PayrollProductSettingId = payrollProductSetting.PayrollProductSettingId,
                                ProductId = payrollProduct.ProductId,
                                Amount = transaction.Amount ?? 0,
                                Quantity = transaction.Quantity,
                                AccountStdId = transaction.AccountStdId,
                                AccountInternal = transaction.AccountInternal.ToList(),
                            });

                        }
                    }
                }

                #endregion
            }

            ActionResult result = CreateQuantityRoundingTransactions(timePeriod, employee, lastDateInPeriod, attestState, roundingDTOs, settings, payrollProductsAll);
            if (!result.Success)
                return result;

            if (result.Value is List<TimePayrollTransaction> roundingTransactions)
                newTransactions.AddRange(roundingTransactions);

            result = CreateCentRoundingTransactions(timePeriod, employee, lastDateInPeriod, attestState, roundingDTOs, settings, payrollProductsAll);
            if (!result.Success)
                return result;

            if (result.Value is List<TimePayrollTransaction> centRoundingTransactions)
                newTransactions.AddRange(centRoundingTransactions);

            result = Save();
            result.Value = newTransactions;
            return result;
        }
        private ActionResult CreateCentRoundingTransactions(TimePeriod timePeriod, Employee employee, TimeBlockDate lastDateInPeriod, AttestStateDTO attestState, List<PayrollProductRoundingDTO> roundingDTOs, List<PayrollProductSetting> settings, List<PayrollProduct> payrollProducts)
        {
            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            ActionResult result = new ActionResult();

            var roundingDTOsGroupedBySetting = roundingDTOs.GroupBy(x => x.PayrollProductSettingId).ToList();
            foreach (var roundingDTOsSettingsGroup in roundingDTOsGroupedBySetting)
            {
                int settingId = roundingDTOsSettingsGroup.Key;
                var setting = settings.FirstOrDefault(x => x.PayrollProductSettingId == settingId);
                if (setting != null && (TermGroup_PayrollProductCentRoundingType)setting.CentRoundingType != TermGroup_PayrollProductCentRoundingType.None)
                {
                    var roundingDTOsGroupedByProduct = roundingDTOsSettingsGroup.GroupBy(x => x.ProductId).ToList();
                    foreach (var roundingDTOsProductGroup in roundingDTOsGroupedByProduct)
                    {
                        int productId = roundingDTOsProductGroup.Key;
                        var payrollProduct = payrollProducts.FirstOrDefault(x => x.ProductId == productId);
                        decimal amount = roundingDTOsProductGroup.Sum(x => x.Amount);
                        decimal roundedAmount = this.Round(amount, (TermGroup_PayrollProductCentRoundingType)setting.CentRoundingType, (TermGroup_PayrollProductCentRoundingLevel)setting.CentRoundingLevel);
                        decimal difference = roundedAmount - amount;
                        if (difference == 0)
                            continue;

                        var roundingDTOsGroupedByAccounts = roundingDTOsProductGroup.GroupBy(r => r.AccountingString);
                        var accountsAmountTuples = new List<Tuple<PayrollProductRoundingDTO, decimal>>();
                        foreach (var roundingDTOsAccountsGroup in roundingDTOsGroupedByAccounts)
                        {
                            var dto = roundingDTOsAccountsGroup.FirstOrDefault();
                            decimal sum = roundingDTOsAccountsGroup.Sum(x => x.Amount);
                            accountsAmountTuples.Add(Tuple.Create(dto, sum));

                        }
                        var accountsAmountTuple = accountsAmountTuples.OrderByDescending(x => x.Item2).First();
                        if (accountsAmountTuple != null)
                        {
                            var transaction = CreateTimePayrollTransaction(payrollProduct, lastDateInPeriod, 1, difference, 0, difference, String.Empty, attestState.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId, accountsAmountTuple.Item1.AccountStdId);
                            AddAccountInternalsToTimePayrollTransaction(transaction, accountsAmountTuple.Item1.AccountInternal);
                            transaction.IsCentRounding = true;
                            newTransactions.Add(transaction);
                        }
                    }
                }
            }
            result.Value = newTransactions;
            return result;
        }
        private ActionResult CreateQuantityRoundingTransactions(TimePeriod timePeriod, Employee employee, TimeBlockDate lastDateInPeriod, AttestStateDTO attestState, List<PayrollProductRoundingDTO> roundingDTOs, List<PayrollProductSetting> settings, List<PayrollProduct> payrollProducts)
        {
            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            ActionResult result = new ActionResult();

            var roundingDTOsGroupedBySetting = roundingDTOs.GroupBy(x => x.PayrollProductSettingId).ToList();
            foreach (var roundingDTOsSettingsGroup in roundingDTOsGroupedBySetting)
            {
                int settingId = roundingDTOsSettingsGroup.Key;
                var setting = settings.FirstOrDefault(x => x.PayrollProductSettingId == settingId);
                if (setting != null && (TermGroup_PayrollProductQuantityRoundingType)setting.QuantityRoundingType != TermGroup_PayrollProductQuantityRoundingType.None)
                {
                    var roundingDTOsGroupedByProduct = roundingDTOsSettingsGroup.GroupBy(x => x.ProductId).ToList();
                    foreach (var roundingDTOsProductGroup in roundingDTOsGroupedByProduct)
                    {
                        int productId = roundingDTOsProductGroup.Key;
                        var payrollProduct = payrollProducts.FirstOrDefault(x => x.ProductId == productId);
                        decimal quantity = roundingDTOsProductGroup.Sum(x => x.Quantity);
                        decimal roundedQuantity = this.Round(quantity, (TermGroup_PayrollProductQuantityRoundingType)setting.QuantityRoundingType, setting.QuantityRoundingMinutes);
                        decimal difference = roundedQuantity - quantity;
                        if (difference == 0)
                            continue;

                        var roundingDTOsGroupedByAccounts = roundingDTOsProductGroup.GroupBy(r => r.AccountingString);
                        var accountsQuantityTuples = new List<Tuple<PayrollProductRoundingDTO, decimal>>();
                        foreach (var roundingDTOsAccountsGroup in roundingDTOsGroupedByAccounts)
                        {
                            var dto = roundingDTOsAccountsGroup.FirstOrDefault();
                            decimal sum = roundingDTOsAccountsGroup.Sum(x => x.Quantity);
                            accountsQuantityTuples.Add(Tuple.Create(dto, sum));

                        }
                        var accountsQuantityTuple = accountsQuantityTuples.OrderByDescending(x => x.Item2).First();
                        if (accountsQuantityTuple != null)
                        {
                            var timePayrollTransaction = CreateTimePayrollTransaction(payrollProduct, lastDateInPeriod, difference, 0, 0, 0, String.Empty, attestState.AttestStateId, timePeriod.TimePeriodId, employee.EmployeeId, accountsQuantityTuple.Item1.AccountStdId);
                            result = SaveTimePayrollTransactionAmounts(lastDateInPeriod, timePayrollTransaction);
                            if (!result.Success)
                                return result;

                            AddAccountInternalsToTimePayrollTransaction(timePayrollTransaction, accountsQuantityTuple.Item1.AccountInternal);
                            timePayrollTransaction.IsQuantityRounding = true;
                            newTransactions.Add(timePayrollTransaction);
                        }
                    }
                }
            }
            result.Value = newTransactions;
            return result;
        }
        private decimal Round(decimal value, TermGroup_PayrollProductCentRoundingType centRoundingType, TermGroup_PayrollProductCentRoundingLevel centRoundingLevel)
        {
            if (centRoundingType == TermGroup_PayrollProductCentRoundingType.None || centRoundingLevel == TermGroup_PayrollProductCentRoundingLevel.None)
                return value;

            decimal level = 1;
            bool decimalRound = false;

            switch (centRoundingLevel)
            {
                case TermGroup_PayrollProductCentRoundingLevel.Thousands:
                    level = 1000;
                    break;
                case TermGroup_PayrollProductCentRoundingLevel.Hundred:
                    level = 100;
                    break;
                case TermGroup_PayrollProductCentRoundingLevel.Ten:
                    level = 10;
                    break;
                case TermGroup_PayrollProductCentRoundingLevel.One:
                    level = 1;
                    break;
                case TermGroup_PayrollProductCentRoundingLevel.OneDecimal:
                    decimalRound = true;
                    level = 10;
                    break;
                case TermGroup_PayrollProductCentRoundingLevel.TwoDecimals:
                    decimalRound = true;
                    level = 100;
                    break;
            }

            switch (centRoundingType)
            {
                case TermGroup_PayrollProductCentRoundingType.Mathematical:

                    if (decimalRound)
                        value = Decimal.Divide(Math.Round(Decimal.Multiply(value, level), MidpointRounding.AwayFromZero), level);
                    else
                        value = Decimal.Multiply(Math.Round(Decimal.Divide(value, level), MidpointRounding.AwayFromZero), level);

                    break;
                case TermGroup_PayrollProductCentRoundingType.Up:

                    if (decimalRound)
                        value = Decimal.Divide(Math.Ceiling(Decimal.Multiply(value, level)), level);
                    else
                        value = Decimal.Multiply(Math.Ceiling(Decimal.Divide(value, level)), level);

                    break;
                case TermGroup_PayrollProductCentRoundingType.Down:

                    if (decimalRound)
                        value = Decimal.Divide(Math.Floor(Decimal.Multiply(value, level)), level);
                    else
                        value = Decimal.Multiply(Math.Floor(Decimal.Divide(value, level)), level);

                    break;
                default:
                    break;
            }

            return value;
        }
        private decimal Round(decimal value, TermGroup_PayrollProductQuantityRoundingType quantityRoundingType, int quantityRoundingLevel)
        {
            if (quantityRoundingType == TermGroup_PayrollProductQuantityRoundingType.None || quantityRoundingLevel == 0)
                return value;

            switch (quantityRoundingType)
            {
                case TermGroup_PayrollProductQuantityRoundingType.Up:

                    value = Decimal.Multiply(Math.Ceiling(Decimal.Divide(value, quantityRoundingLevel)), quantityRoundingLevel);

                    break;
                case TermGroup_PayrollProductQuantityRoundingType.Down:

                    value = Decimal.Multiply(Math.Floor(Decimal.Divide(value, quantityRoundingLevel)), quantityRoundingLevel);

                    break;
                default:
                    break;
            }

            return value;
        }

        #endregion

        #region UnionFee        

        private ActionResult CreateUnionFee(DateTime paymentDate, decimal unionFeePromotedAmount, Employee employee, List<PayrollProduct> payrollProductUnionFees, TimePeriod timePeriod, TimeBlockDate lastTimeBlockDateInPeriod, ReCalculatePayrollPeriodCompanyDTO companyDTO)
        {
            if (timePeriod.ExtraPeriod)
                return new ActionResult(true);

            ActionResult result = new ActionResult(true);
            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();
            List<Tuple<int, PayrollProduct, decimal>> unionFeeIdsAndAmounts = new List<Tuple<int, PayrollProduct, decimal>>();
            List<EmployeeUnionFee> employeeUnionFees = GetEmployeeUnionFees(employee.EmployeeId);
            List<EmployeeUnionFee> currentEmployeeUnionFees = employeeUnionFees.Where(x => (!x.FromDate.HasValue || x.FromDate.Value.Date <= paymentDate.Date) && (!x.ToDate.HasValue || x.ToDate.Value.Date >= paymentDate.Date)).OrderBy(x => x.Created).ToList();
            if (currentEmployeeUnionFees.IsNullOrEmpty())
                return result;

            foreach (var employeeUnionFee in currentEmployeeUnionFees)
            {
                UnionFee unionFee = GetUnionFeeWithPriceTypeAndPeriods(employeeUnionFee.UnionFeeId);
                if (unionFee == null)
                    continue;

                decimal fixedAmount = PayrollManager.GetPayrollPriceTypeAmount(unionFee.PayrollPriceTypeFixedAmount, paymentDate);
                decimal percent = PayrollManager.GetPayrollPriceTypeAmount(unionFee.PayrollPriceTypePercent, paymentDate);
                decimal percentAmount = (percent / 100) * unionFeePromotedAmount;
                decimal ceilingAmount = PayrollManager.GetPayrollPriceTypeAmount(unionFee.PayrollPriceTypePercentCeiling, paymentDate);

                decimal unionFeeAmount = 0;
                if (percentAmount > fixedAmount)
                {
                    //use percent amount
                    if (percentAmount > ceilingAmount)
                        unionFeeAmount = ceilingAmount;
                    else
                        unionFeeAmount = percentAmount;
                }
                else
                {
                    //use fixed fixedamount = minimum amount
                    unionFeeAmount = fixedAmount;
                }

                unionFeeAmount = decimal.Round(unionFeeAmount, 2, MidpointRounding.AwayFromZero);
                var payrollProductUnionFee = unionFee.PayrollProductId.HasValue ? payrollProductUnionFees.FirstOrDefault(x => x.ProductId == unionFee.PayrollProductId.Value) : payrollProductUnionFees.FirstOrDefault();
                unionFeeIdsAndAmounts.Add(Tuple.Create(unionFee.UnionFeeId, payrollProductUnionFee, -1 * unionFeeAmount));
            }

            foreach (var tuple in unionFeeIdsAndAmounts)
            {
                int unionFeeId = tuple.Item1;
                PayrollProduct payrollProductUnionFee = tuple.Item2;
                decimal currentUnionFeeAmount = tuple.Item3;

                if (unionFeeId == 0 || payrollProductUnionFee == null || currentUnionFeeAmount == 0)
                    continue;

                //UnionFee
                TimePayrollTransaction timePayrollTransactionUnionFee = CreateOrUpdateTimePayrollTransaction(payrollProductUnionFee, lastTimeBlockDateInPeriod, employee, timePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, currentUnionFeeAmount, currentUnionFeeAmount, 0, String.Empty);
                if (timePayrollTransactionUnionFee == null)
                    return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                timePayrollTransactionUnionFee.UnionFeeId = unionFeeId;
                if (timePayrollTransactionUnionFee.Amount == 0)
                {
                    result = SetTimePayrollTransactionToDeleted(timePayrollTransactionUnionFee, saveChanges: false);
                    if (!result.Success)
                        return result;
                }
                else
                {
                    newTransactions.Add(timePayrollTransactionUnionFee);
                }
            }

            if (newTransactions.Any())
            {
                #region Rounding
                foreach (var transactionsByUnionFee in newTransactions.Where(w => w.UnionFeeId.HasValue).GroupBy(g => g.UnionFeeId.Value))
                {
                    int unionFeeId = transactionsByUnionFee.Key;
                    var transactions = transactionsByUnionFee.ToList();

                    result = CreateRoundingTransactions(transactions, new List<TimePayrollScheduleTransaction>(), new List<TimeBlockDate>() { lastTimeBlockDateInPeriod }, employee, timePeriod, lastTimeBlockDateInPeriod);
                    if (!result.Success)
                        return result;

                    //Add roundingtransactions                
                    if (result.Value is List<TimePayrollTransaction>)
                    {
                        foreach (var resultRow in result.Value as List<TimePayrollTransaction>)
                        {
                            resultRow.UnionFeeId = unionFeeId;
                            newTransactions.Add(resultRow);
                        }
                    }
                }

                #endregion
            }
          
            result = Save();
            if (!result.Success)
                return result;

            result.Value = newTransactions;
            return result;
        }

        #endregion

        #region VacationCompensation

        private ActionResult CreateVacationCompensationDirectPaymentTransaction(VacationCompensationAccountingDistributionDTO vacationCompensationAccountingDistributionDTO, List<TimePayrollTransaction> timePayrollTransactionsForPeriod, PayrollProduct payrollProductVacationCompensation, TimePeriod timePeriod, Employee employee, EmployeeTimePeriod employeeTimePeriod, TimeBlockDate lastTimeBlockDateInPeriod, int attestStateId, decimal accEmpTaxBasis, ref decimal periodEmploymentTaxCredit)
        {
            if (payrollProductVacationCompensation == null)
                return new ActionResult(true);

            if (employee == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, GetText(8540, "Anställd kunde inte hittas"));
            if (employeeTimePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityIsNull, "EmployeeTimePeriod");

            AttestStateDTO attestState = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestState == null)
                return new ActionResult((int)ActionResultSave.PayrollCalculationMissingAttestStateReg);

            ActionResult result = SaveEmployeeTimePeriodValue(employeeTimePeriod, SoeEmployeeTimePeriodValueType.VacationCompensation, vacationCompensationAccountingDistributionDTO.VacationCompensationAmount, false);
            if (!result.Success)
                return result;

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();

            foreach (var item in vacationCompensationAccountingDistributionDTO.Items.Where(x => x.Amount != 0))
            {
                TimePayrollTransaction timePayrollTransactionVacationCompensation = CreateOrUpdateTimePayrollTransaction(payrollProductVacationCompensation, lastTimeBlockDateInPeriod, employee, employeeTimePeriod.TimePeriodId, attestStateId, 1, item.Amount, item.Amount, 0, String.Empty, timePayrollTransactionsForPeriod, removeFromReusableCollection: true, applyAccounting: false);
                if (timePayrollTransactionVacationCompensation != null)
                {
                    ApplyAccountingOnTimePayrollTransaction(timePayrollTransactionVacationCompensation, employee, lastTimeBlockDateInPeriod.Date, payrollProductVacationCompensation, setAccountInternal: false);
                    timePayrollTransactionVacationCompensation.AccountInternal?.Clear();

                    AddAccountInternalsToTimePayrollTransaction(timePayrollTransactionVacationCompensation, item.AccountInternal);
                    newTransactions.Add(timePayrollTransactionVacationCompensation);
                }
            }

            if (newTransactions.Any())
            {
                #region Rounding

                result = CreateRoundingTransactions(newTransactions, new List<TimePayrollScheduleTransaction>(), new List<TimeBlockDate>() { lastTimeBlockDateInPeriod }, employee, timePeriod, lastTimeBlockDateInPeriod);
                if (!result.Success)
                    return result;

                if (result.Value is List<TimePayrollTransaction> roundingTransactions)
                    newTransactions.AddRange(roundingTransactions);

                #endregion

                #region Employment tax debit and credit (see item 20450)

                //Create employmenttax debit transactions
                result = CalculateEmploymentTaxDebet(employee, lastTimeBlockDateInPeriod, timePeriod, newTransactions, accEmpTaxBasis);
                if (!result.Success)
                    return result;

                //Sum employmenttax debit amount and added to the employmenttax credit for the period
                if (result.Value is List<TimePayrollTransaction> employmentTaxDebetTransactions)
                {
                    decimal employmentTaxDebit = employmentTaxDebetTransactions.Where(x => x.Amount.HasValue).Sum(x => x.Amount.Value);
                    periodEmploymentTaxCredit += (employmentTaxDebit * -1);
                }

                #endregion

            }
            result = Save();

            return result;
        }

        #endregion

        #region WeekendSalary

        private ActionResult CreateHolidaySalaryTransactions(ReCalculatePayrollPeriodCompanyDTO companyDTO, Employee employee, TimePeriod timePeriod, List<TimeBlockDate> timeBlockDates)
        {
            if (timePeriod.ExtraPeriod || !companyDTO.HolidaySalaryHolidaysCurrentPeriod.Any())
                return new ActionResult(true);

            if (companyDTO.PayrollProductWeekendSalary == null)
                return new ActionResult(GetText(8937, "Löneart för helglön saknas"));

            List<TimePayrollTransaction> newTransactions = new List<TimePayrollTransaction>();

            foreach (HolidayDTO holidayDTO in companyDTO.HolidaySalaryHolidaysCurrentPeriod)
            {
                Employment employment = employee.GetEmployment(holidayDTO.Date);
                if (employment == null)
                    continue;

                TimeBlockDate timeBlockDate = timeBlockDates.FirstOrDefault(x => x.Date == holidayDTO.Date);
                if (timeBlockDate == null)
                    continue;

                int employeeGroupId = employee.GetEmployeeGroupId(timeBlockDate.Date);
                EmployeeGroup employeeGroup = GetEmployeeGroupsWithWeekendSalaryDayTypesFromCache().FirstOrDefault(x => x.EmployeeGroupId == employeeGroupId);
                if (employeeGroup == null)
                    continue;

                if (!employeeGroup.EmployeeGroupDayType.Any(x => x.IsHolidaySalary && x.DayTypeId == holidayDTO.DayTypeId && x.State == (int)SoeEntityState.Active))
                    continue;

                List<TimeBlock> timeBlocksForDay = TimeBlockManager.GetTimeBlocks(entities, employee.EmployeeId, holidayDTO.Date, holidayDTO.Date);
                int? wholedayDeviationTimeDeviationCauseId = timeBlocksForDay.GetWholedayDeviationTimeDeviationCauseId();
                if (wholedayDeviationTimeDeviationCauseId.HasValue)
                    continue;

                PayrollPriceFormulaResultDTO formulaResult = EvaluatePayrollPriceFormula(timeBlockDate.Date, employee, employment, companyDTO.PayrollProductWeekendSalary);
                decimal amount = formulaResult != null ? Decimal.Round(formulaResult.Amount, 2, MidpointRounding.AwayFromZero) : 0;
                TimePayrollTransaction newTimePayrollTransaction = CreateOrUpdateTimePayrollTransaction(companyDTO.PayrollProductWeekendSalary, timeBlockDate, employee, timePeriod.TimePeriodId, companyDTO.InitialAttestStateId, 1, amount, amount, 0, String.Empty);
                if (newTimePayrollTransaction == null)
                    return new ActionResult((int)ActionResultSave.PayrollCalculationPayrollTransactionCouldNotBeCreated);

                SetTimePayrollTransactionFormulas(newTimePayrollTransaction, formulaResult);

                newTransactions.Add(newTimePayrollTransaction);

            }

            ActionResult result = Save();
            if (!result.Success)
                return result;

            result.Value = newTransactions;
            return result;

        }

        #endregion
    }
}
