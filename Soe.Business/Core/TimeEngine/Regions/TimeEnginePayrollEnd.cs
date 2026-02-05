using log4net.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Tasks

        /// <summary>
        /// Creates a vacation year end
        /// </summary>
        /// <returns>Output DTO</returns>
        private SaveVacationYearEndOutputDTO TaskSaveVacationYearEnd()
        {
            var (iDTO, oDTO) = InitTask<SaveVacationYearEndInputDTO, SaveVacationYearEndOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.ContentTypeIds == null)
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
                    oDTO.Result = ValidationSaveVacationYearEnd(base.ActorCompanyId, iDTO.Date, iDTO.ContentType, iDTO.ContentTypeIds);
                    if (!oDTO.Result.Success)
                        return oDTO;

                    oDTO.Result = PrintEmployeeVacationDebtReport(out ReportPrintoutDTO dto, CalendarUtility.GetEndOfDay(iDTO.Date), iDTO.ContentType, contentTypeIds: iDTO.ContentTypeIds);
                    if (!oDTO.Result.Success)
                        return oDTO;

                    oDTO.Result = CreateVacationYearEnd(null, SoeVacationYearEndType.VacationYearEnd, iDTO.ContentType, dto.ReportPrintoutId, iDTO.Date);
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

                    if (oDTO.Result.Success && oDTO.Result.Value is List<TimeEngineVacationYearEndEmployee>)
                    {
                        var handledEmployees = oDTO.Result.Value as List<TimeEngineVacationYearEndEmployee>;
                        Task.Run(() => CompEntitiesProvider.RunWithTaskScopedReadOnlyEntities(() => RecalculateVacationTransactionsAsync(handledEmployees.Where(e => e.HasSucceeded && e.HasValidVacationGroup).ToList())));

                        oDTO.Details = new VacationYearEndResultDTO()
                        {
                            Result = oDTO.Result,
                            EmployeeResults = handledEmployees.Select(row => new VacationYearEndEmployeeResultDTO()
                            {
                                EmployeeId = row.EmployeeId,
                                EmployeeNrAndName = row.Employee.EmployeeNrAndName,
                                Message = row.HasSucceeded ? row.Result.InfoMessage : row.Result.ErrorMessage,
                                Status = row.Status,
                                StatusName = GetText((int)row.Status, (int)TermGroup.VacationYearEndResult),
                                VacationGroupName = row.VacationGroup.Name,
                            }).Sort()
                        };
                        oDTO.Details.Result.Value = null; //Clean
                    }
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Deletes a vacation year end
        /// </summary>
        /// <returns>Output DTO</returns>
        private DeleteVacationYearEndOutputDTO TaskDeleteVacationYearEnd()
        {
            var (iDTO, oDTO) = InitTask<DeleteVacationYearEndInputDTO, DeleteVacationYearEndOutputDTO>();
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
                    using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                    {
                        InitTransaction(transaction);

                        oDTO.Result = DeleteVacationYearEnd(iDTO.VacationYearEndHeadId, SoeVacationYearEndType.VacationYearEnd);

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
        /// Creates final salary
        /// </summary>
        /// <returns>Output DTO</returns>
        private CreateFinalSalaryOutputDTO TaskCreateFinalSalary()
        {
            var (iDTO, oDTO) = InitTask<CreateFinalSalaryInputDTO, CreateFinalSalaryOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null || iDTO.EmployeeIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Validate PayrollProducts

                List<Tuple<int?, int?, int?, int?>> mandatoryProducts = new List<Tuple<int?, int?, int?, int?>>()
                {
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Earned, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Paid, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Advance, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear2, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear3, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear4, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear5, (int?)null),
                    Tuple.Create((int?)TermGroup_SysPayrollType.SE_GrossSalary, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, (int?)TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdue, (int?)null),
                };
                oDTO.Result = CheckMandatoryPayrollProductByLevels(mandatoryProducts);
                if (!oDTO.Result.Success)
                    return oDTO;

                #endregion

                #region Validate Employees

                List<int> handledEmployeeIds = new List<int>();
                List<string> employeeErrors = new List<string>();
                Dictionary<DateTime, List<Employment>> dateEmploymentMapping = new Dictionary<DateTime, List<Employment>>();

                foreach (int employeeId in iDTO.EmployeeIds)
                {
                    Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
                    if (employee == null)
                    {
                        employeeErrors.Add(GetText(10083, "Anställd hittades inte"));
                        continue;
                    }

                    Employment employment = employee.Employment.GetApplyFinalSalaryEmployment();
                    if (employment == null || !employment.DateTo.HasValue)
                    {
                        employeeErrors.Add($"{employee.EmployeeNrAndName}. {GetText(10084, "Anställning hittades inte")}");
                        continue;
                    }

                    DateTime date = employment.DateTo.Value.Date;

                    TimePeriod timePeriod = iDTO.TimePeriodId.HasValue ? GetTimePeriodFromCache(iDTO.TimePeriodId.Value) : null;
                    if (timePeriod != null && timePeriod.PayrollStopDate.HasValue && timePeriod.PayrollStopDate.Value < date)
                    {
                        employeeErrors.Add(String.Format(GetText(12007, "Anställd {0} slutar {1} och ska slutavräknas i en senare period"), employee.EmployeeNrAndName, date.ToShortDateString()));
                        continue;
                    }

                    if (HasVacationYearEndRowOnDate(employee.EmployeeId, date, SoeVacationYearEndType.FinalSalary))
                    {
                        employeeErrors.Add(String.Format(GetText(11693, "Anställd {0} slutar {1} och är redan slutavräknad"), employee.EmployeeNrAndName, date.ToShortDateString()));
                        continue;
                    }

                    if (iDTO.TimePeriodId.HasValue)
                    {
                        List<TimePayrollTransaction> reusableTransactionsForPeriod = GetTimePayrollTransactions(employee.EmployeeId, iDTO.TimePeriodId.Value).GetPayrollCalculationTransactions();
                        oDTO.Result = ValidatePayrollLockedAttestStates(reusableTransactionsForPeriod);
                        if (!oDTO.Result.Success)
                        {
                            employeeErrors.Add(String.Format(GetText(11955, "Anställd {0}, perioden måste låsas upp för att kunna slutavräknas"), employee.EmployeeNrAndName));
                            continue;
                        }
                    }

                    if (!dateEmploymentMapping.ContainsKey(date))
                        dateEmploymentMapping.Add(date, new List<Employment>());
                    dateEmploymentMapping[date].Add(employment);
                }

                #endregion

                #region Print EmployeeVacationDebtReport

                oDTO.Result = PrintEmployeeVacationDebtReport(out Dictionary<DateTime, ReportPrintoutDTO> dateReportMapping, dateEmploymentMapping, createReport: iDTO.CreateReport, isFinalSalary: true, timePeriodId: iDTO.TimePeriodId);
                if (!oDTO.Result.Success)
                    return oDTO;

                #endregion

                try
                {
                    if (dateReportMapping.Any())
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            #region Perform

                            foreach (var pair in dateReportMapping)
                            {
                                DateTime date = pair.Key;
                                int reportPrintoutId = pair.Value.ReportPrintoutId;

                                oDTO.Result = CreateVacationYearEnd(transaction, SoeVacationYearEndType.FinalSalary, TermGroup_VacationYearEndHeadContentType.Employee, reportPrintoutId, date, iDTO.TimePeriodId);
                                if (oDTO.Result.Success && dateEmploymentMapping.ContainsKey(date))
                                {
                                    foreach (Employment employment in dateEmploymentMapping[date])
                                    {
                                        employment.FinalSalaryStatus = (int)SoeEmploymentFinalSalaryStatus.AppliedFinalSalary;
                                        SetModifiedProperties(employment);

                                        handledEmployeeIds.Add(employment.EmployeeId);
                                    }

                                    oDTO.Result = Save();
                                    if (!oDTO.Result.Success)
                                        return oDTO;
                                }
                                else
                                {
                                    employeeErrors.Add(oDTO.Result.ErrorMessage);
                                }
                            }

                            #endregion

                            TryCommit(oDTO);
                        }
                    }
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                        oDTO.Result.Keys = handledEmployeeIds;
                    else
                        LogTransactionFailed(this.ToString());

                    if (employeeErrors.Any())
                    {
                        if (iDTO.EmployeeIds.Count == 1)
                            employeeErrors.Insert(0, GetText(12009, "Anställd kunde inte slutavräknas"));
                        else
                            employeeErrors.Insert(0, !dateEmploymentMapping.Any() ? GetText(12008, "Inga anställda kunde slutavräknas") : GetText(12006, "Följande anställda kunde inte slutavräknas"));
                        oDTO.Result.Strings = employeeErrors;
                    }

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Deletes final salary
        /// </summary>
        /// <returns>Output DTO</returns>
        private DeleteFinalSalaryOutputDTO TaskDeleteFinalSalary()
        {
            var (iDTO, oDTO) = InitTask<DeleteFinalSalaryInputDTO, DeleteFinalSalaryOutputDTO>();
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

                List<int> handledEmployeeIds = new List<int>();
                List<string> employeeErrors = new List<string>();

                try
                {
                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        using (TransactionScope transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            #region Prereq
                            
                            Employee employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId, getHidden: false);
                            if (employee == null)
                            {
                                employeeErrors.Add(GetText(10083, "Anställd hittades inte"));
                                continue;
                            }

                            TimePeriod timePeriod = GetTimePeriodFromCache(iDTO.TimePeriodId);
                            if (timePeriod == null)
                            {
                                employeeErrors.Add($"{employee.EmployeeNrAndName}. {GetText(8693, "Period hittades inte")}");
                                continue;
                            }

                            Employment employment = employee.Employment.GetAppliedFinalSalaryEmployment();
                            if (employment == null)
                            {
                                employeeErrors.Add($"{employee.EmployeeNrAndName}. {GetText(10084, "Anställning hittades inte")}");
                                continue;
                            }

                            #endregion

                            #region Perform

                            VacationYearEndRow vacationYearEndRow = GetLatestVacationYearEndRowWithHead(employee.EmployeeId, SoeVacationYearEndType.FinalSalary);
                            if (vacationYearEndRow == null)
                            {
                                employeeErrors.Add($"{employee.EmployeeNrAndName}. {GetText(94047, "Slutlön hittades inte")}");
                                continue;
                            }

                            oDTO.Result = DeleteVacationYearEnd(vacationYearEndRow.VacationYearEndHeadId, SoeVacationYearEndType.FinalSalary, iDTO.TimePeriodId, employeeId: employee.EmployeeId);
                            if (!oDTO.Result.Success)
                            {
                                employeeErrors.Add($"{employee.EmployeeNrAndName}. {oDTO.Result.ErrorMessage}");
                                continue;
                            }

                            employment.FinalSalaryStatus = (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary;
                            SetModifiedProperties(employment);

                            oDTO.Result = Save();
                            if (!oDTO.Result.Success)
                            {
                                employeeErrors.Add($"{employee.EmployeeNrAndName}. {oDTO.Result.ErrorMessage}");
                                continue;
                            }

                            handledEmployeeIds.Add(employeeId);

                            #endregion

                            TryCommit(oDTO);
                        }
                    }                                             
                }
                catch (Exception ex)
                {
                    oDTO.Result.Exception = ex;
                    LogError(ex);
                }
                finally
                {
                    if (oDTO.Result.Success)
                        oDTO.Result.Keys = handledEmployeeIds;
                    else
                        LogTransactionFailed(this.ToString());

                    if (employeeErrors.Any())
                    {
                        if (iDTO.EmployeeIds.Count == 1)
                            employeeErrors.Insert(0, GetText(94048, "Slutlön kunde inte backas"));
                        else
                            employeeErrors.Insert(0, !handledEmployeeIds.Any() ? GetText(94049, "Slutlön kunde inte backas för valda anställda") : GetText(94050, "Slutlön kunde inte backas för följande anställda"));
                        oDTO.Result.Strings = employeeErrors;
                    }

                    entities.Connection.Close();
                }
            }

            return oDTO;
        }

        /// <summary>
        /// Validates VacationYearEnd content
        /// </summary>
        /// <returns></returns>
        private ValidateVacationYearEndOutputDTO TaskValidateVacationYearEnd()
        {
            var (iDTO, oDTO) = InitTask<ValidateVacationYearEndInputDTO, ValidateVacationYearEndOutputDTO>();
            if (!oDTO.Result.Success)
                return oDTO;

            if (iDTO == null)
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.InsufficientInput);
                return oDTO;
            }
            if (iDTO.VacationGroupIds.IsNullOrEmpty() && iDTO.EmployeeIds.IsNullOrEmpty())
            {
                oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, GetText(12075, "Välj semesteravtal eller anställda"));
                return oDTO;
            }

            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);
                List<string> invalidated = new List<string>();

                if (!iDTO.EmployeeIds.IsNullOrEmpty())
                {
                    DateTime? vacationYearEndDate = null;
                    foreach (int employeeId in iDTO.EmployeeIds)
                    {
                        Employee employee = GetEmployeeWithContactPersonFromCache(employeeId, getHidden: false);
                        if (employee == null)
                            continue;

                        VacationGroupDTO currentVacationGroup = GetVacationGroupFromCache(employee.EmployeeId, iDTO.Date);

                        if (currentVacationGroup == null)
                        {
                            // Retrieve the employee's previous employment record based on the given date.
                            var lastEmployment = employee.GetPrevEmployment(iDTO.Date);

                            // If the previous employment exists and has an end date, update the currentVacationGroup.
                            if (lastEmployment != null && lastEmployment.GetEndDate().HasValue)
                                currentVacationGroup = GetVacationGroupFromCache(employee.EmployeeId, lastEmployment.GetEndDate().Value);

                            // If the previous employment has a final salary applied or was manually adjusted, skip the current iteration.
                            if (lastEmployment != null && lastEmployment.HasAppliedFinalSalaryOrManually())
                                continue;

                            // If there is no previous employment record, check the employee's first employment record.
                            if (lastEmployment == null)
                            {
                                var firstEmployment = employee.GetFirstEmployment();

                                // If the first employment exists and its start date is after the given date, skip the current iteration.
                                if (firstEmployment != null && firstEmployment.DateFrom > iDTO.Date)
                                    continue;
                            }
                        }

                        if (currentVacationGroup == null)
                        {
                            invalidated.Add(string.Format(GetText(12076, "Semesteravtal saknas på anställd {0}"), employee.NumberAndName) + Environment.NewLine);
                            continue;
                        }
                        currentVacationGroup.RealDateFrom = currentVacationGroup.CalculateFromDate(iDTO.Date);

                        if (!vacationYearEndDate.HasValue)
                        {
                            vacationYearEndDate = currentVacationGroup.RealDateFrom;
                        }
                        else if (vacationYearEndDate.Value != currentVacationGroup.RealDateFrom)
                        {
                            invalidated.Add(GetText(12076, "De valda anställda har semesteravtal med olika startdatum.\nDu kan bara välja anställda som tillhör avtal med samma startdatum") + Environment.NewLine);
                        }
                    }

                    if (invalidated.Any())
                    {
                        oDTO.Result = new ActionResult((int)ActionResultSave.IncorrectInput, string.Join(Environment.NewLine, invalidated));
                        return oDTO;
                    }
                }
                else
                {
                    //Nothing to validate at this point
                    oDTO.Result = new ActionResult(true);
                }
            }

            return oDTO;
        }

        #endregion

        #region VacationYearEnd

        private ActionResult CreateVacationYearEnd(TransactionScope transaction, SoeVacationYearEndType type, TermGroup_VacationYearEndHeadContentType contentType, int reportPrintoutId, DateTime inputDate, int? timePeriodId = null)
        {
            ActionResult result;

            #region Prereq

            bool hasInputTransaction = transaction != null;
            bool isFinalSalary = type == SoeVacationYearEndType.FinalSalary;
            bool isVacationYearEnd = type == SoeVacationYearEndType.VacationYearEnd;

            ReportPrintout reportPrintout = ReportManager.GetReportPrintout(entities, reportPrintoutId, base.ActorCompanyId);
            if (reportPrintout?.XML == null)
                return new ActionResult((int)ActionResultSave.VacationGroupsFailedReport, GetText(10026, "Rapport semesterskuld kunde inte skapas"));

            XDocument xdoc = XDocument.Parse(reportPrintout.XML);
            if (xdoc == null)
                return new ActionResult((int)ActionResultSave.VacationGroupsFailedReport, GetText(10026, "Rapport semesterskuld kunde inte skapas"));

            XElement rootElement = XmlUtil.GetChildElement(xdoc, "EmployeeVacationDebtReport");
            if (rootElement == null)
                return new ActionResult((int)ActionResultSave.VacationGroupsFailedReport, GetText(10026, "Rapport semesterskuld kunde inte skapas"));

            TimePeriod timePeriod = timePeriodId.HasValue ? GetTimePeriodFromCache(timePeriodId.Value) : null;
            if (timePeriod == null && isFinalSalary)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10088, "Period hittades inte"));

            AttestStateDTO attestStateInitial = GetAttestStateInitialFromCache(TermGroup_AttestEntity.PayrollTime);
            if (attestStateInitial == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10085, "Attestnivå hittades inte"));

            List<TimeAccumulator> timeAccumulatorsForFinalSalary = isFinalSalary ? GetTimeAccumulatorsForFinalSalary() : null;

            VacationYearEndHead vacationYearEndHead = null;

            #endregion

            #region Perform

            var vacationGroupsMapping = new Dictionary<VacationGroup, XElement>();
            var vacationGroupElements = XmlUtil.GetChildElements(rootElement, "VacationGroup");
            var vacationGroupNames = XmlUtil.GetChildElementValues(vacationGroupElements, "VacationGroupName");            

            foreach (var vacationGroupElement in vacationGroupElements)
            {
                if (!Int32.TryParse(XmlUtil.GetChildElementValue(vacationGroupElement, "VacationGroupId"), out int vacationGroupId) || vacationGroupId <= 0)
                    continue;
                if (vacationGroupsMapping.Any(m => m.Key.VacationGroupId == vacationGroupId))
                    continue;

                var vacationGroup = GetVacationGroupWithVacationGroupSE(vacationGroupId);
                if (vacationGroup == null || vacationGroup.VacationGroupSE.IsNullOrEmpty())
                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10033, "Semesteravtal hittades inte"));

                vacationGroupsMapping.Add(vacationGroup, vacationGroupElement);
            }

            var calculatedEmployees = new List<TimeEngineVacationYearEndEmployee>();
            var calculatedVacationGroupIds = new List<int>();

            foreach (var vacationGroupMapping in vacationGroupsMapping)
            {
                var vacationGroup = vacationGroupMapping.Key;
                var vacationGroupElement = vacationGroupMapping.Value;
                var vacationGroupSE = vacationGroup.VacationGroupSE.First();
                vacationGroup.GetActualDates(out DateTime vacationGroupFromDate, out DateTime vacationGroupToDate, out int _, out int _, inputDate);

                var calculationType = vacationGroupSE != null ? (TermGroup_VacationGroupCalculationType)vacationGroupSE.CalculationType : TermGroup_VacationGroupCalculationType.Unknown;
                bool sammanfallande = calculationType == TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement || calculationType == TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition;
                bool calculateHours = vacationGroupSE?.VacationHandleRule == (int)TermGroup_VacationGroupVacationHandleRule.Hours;
                bool useParagraph26 = vacationGroupSE.UseParagraph26();
                string userDetails = GetUserDetails();

                var employeesMapping = new Dictionary<Employee, (Employment Employment, XElement Element)>();
                var employeeElements = XmlUtil.GetChildElements(vacationGroupElement, "Employee");

                foreach (var employeeElement in employeeElements)
                {
                    if (!Int32.TryParse(XmlUtil.GetChildElementValue(employeeElement, "EmployeeId"), out int employeeId))
                        continue;
                    if (employeesMapping.Any(m => m.Key.EmployeeId == employeeId))
                        continue;

                    var employee = GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId);
                    if (employee == null)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10083, "Anställd hittades inte") + $" ({employeeId})");

                    var employment = employee.GetEmployment(vacationGroupFromDate, vacationGroupToDate, forward: false);

                    if (DoSkipEmployeeFromVacationYearEnd(employee, employment, timePeriod))
                        continue;

                    employeesMapping.Add(employee, (employment, employeeElement));
                }

                if (employeesMapping.Any())
                {
                    result = InitVacationGroupCalculation();
                    if (!result.Success)
                        return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(92026, "Semesterårsskifte kunde ej skapas för semesteravtal {0}"), vacationGroup.Name));

                    foreach (var employeeMapping in employeesMapping)
                    {
                        var calculation = new TimeEngineVacationYearEndEmployee(employeeMapping.Key, employeeMapping.Value.Employment, employeeMapping.Value.Element, vacationGroup, type, inputDate, vacationGroupFromDate, vacationGroupToDate, timePeriod);

                        bool calculationSuccess = hasInputTransaction ? TryCalculateEmployee(calculation) : TryCalculateEmployeeWithOwnTransaction(calculation);
                        calculatedEmployees.Add(calculation);

                        if (hasInputTransaction && !calculationSuccess)
                        {
                            calculation.Result.ErrorMessage = $"{calculation.Employee.EmployeeNrAndName}: {calculation.Result.ErrorMessage}";
                            return calculation.Result;
                        }
                    }

                    calculatedVacationGroupIds.Add(vacationGroup.VacationGroupId);
                }

                ActionResult InitVacationGroupCalculation()
                {
                    if (vacationYearEndHead == null)
                        vacationYearEndHead = CreateVacationYearEndHead(type, contentType, inputDate, vacationGroupNames);
                    if (!vacationYearEndHead.VacationGroup.Any(i => i.VacationGroupId == vacationGroup.VacationGroupId))
                        vacationYearEndHead.VacationGroup.Add(vacationGroup);
                    return Save();
                }

                bool TryCalculateEmployeeWithOwnTransaction(TimeEngineVacationYearEndEmployee calculation)
                {
                    try
                    {
                        using (transaction = new TransactionScope(ConfigSettings.TRANSACTIONSCOPEOPTION_DEFAULT, ConfigSettings.TRANSACTIONOPTION_DEFAULT))
                        {
                            InitTransaction(transaction);

                            if (TryCalculateEmployee(calculation))
                                CommitEmployeeCalculation(calculation);
                            else
                                RollbackEmployeeCalculation(calculation);
                        }
                        PostTransactionCleanup(calculation);
                    }
                    catch (Exception ex)
                    {
                        calculation.SetResult(new ActionResult(ex));
                        LogError(ex);

                    }
                    return calculation.Result.Success;
                }
                void CommitEmployeeCalculation(TimeEngineVacationYearEndEmployee calculation)
                {
                    SaveVacationYearEndRowStatus(calculation);
                    transaction.Complete();
                }
                void RollbackEmployeeCalculation(TimeEngineVacationYearEndEmployee calculation)
                {
                    ClearCache();
                    base.TryDetachEntitys(this.entities, calculation.Tracked);
                }
                void PostTransactionCleanup(TimeEngineVacationYearEndEmployee calculation)
                {
                    if (calculation.HasFailed)
                    {
                        calculation.SetVacationYearEndRow(this.CreateCleanupVacationYearEndRow(calculation, vacationYearEndHead));
                        SaveVacationYearEndRowStatus(calculation);
                    }
                }

                bool TryCalculateEmployee(TimeEngineVacationYearEndEmployee calculation)
                {
                    var calculationResult = CalculateEmployee(calculation);
                    calculation.SetResult(calculationResult);
                    return calculationResult.Success;
                }
                ActionResult CalculateEmployee(TimeEngineVacationYearEndEmployee calculation)
                {
                    ActionResult calculationResult;

                    try
                    {
                        var vacationDaysToPay = new List<TimeEngineVacationYearEndDayToPay>();
                        var vacationAdditionOrSalaryPrepaymentDaysToPay = new List<TimeEngineVacationYearEndDayToPay>();
                        var created = DateTime.Now;

                        #region Validation

                        if (isVacationYearEnd)
                        {
                            calculationResult = ValidateEmployeeForVacationYearEnd(calculation.Employee, calculation.Employment, vacationGroupFromDate, vacationGroupToDate);
                            if (!calculationResult.Success)
                                return calculationResult;
                        }

                        int employeeVacationSEId = XmlUtil.GetChildElementValueId(calculation.EmployeeElement, "EmployeeVacationSE", "EmployeeVacationSEId");
                        EmployeeVacationSE employeeVacationSE = GetEmployeeVacationSEDiscardedState(employeeVacationSEId);
                        if (employeeVacationSE != null)
                        {
                            calculationResult = ValidateEmployeeVacationSEForVacationYearEnd(calculation.Employee, employeeVacationSE, vacationGroupSE);
                            if (!calculationResult.Success)
                                return calculationResult;
                        }

                        #endregion

                        GetVacationYearEndFormulaValues(calculation.EmployeeElement, out decimal SLD, out decimal STRD, out decimal SR, out decimal SB, out decimal? SSG);

                        if (isVacationYearEnd)
                        {
                            #region RemainingDaysRule

                            decimal allowedToSave = 0;
                            decimal earnedDaysPaidPrevVacationYear = employeeVacationSE?.EarnedDaysPaid ?? 0;
                            decimal remainingDaysPaid = CalculateRemainingDaysPayed(employeeVacationSE, sammanfallande, SB) ?? 0;
                            decimal daysToPay = 0;

                            switch ((TermGroup_VacationGroupYearEndRemainingDaysRule)vacationGroupSE.YearEndRemainingDaysRule)
                            {
                                case TermGroup_VacationGroupYearEndRemainingDaysRule.Paid:
                                    daysToPay = remainingDaysPaid;
                                    break;
                                case TermGroup_VacationGroupYearEndRemainingDaysRule.Saved:
                                    daysToPay = 0;
                                    break;
                                case TermGroup_VacationGroupYearEndRemainingDaysRule.Over20DaysSaved:
                                    allowedToSave = earnedDaysPaidPrevVacationYear - 20;
                                    if (allowedToSave > 0)
                                    {
                                        if (allowedToSave <= remainingDaysPaid)
                                        {
                                            daysToPay = remainingDaysPaid - allowedToSave;
                                        }                                            
                                        else
                                        {
                                            daysToPay = 0;
                                            allowedToSave = remainingDaysPaid;
                                        }                                           
                                    }
                                    else
                                    {
                                        daysToPay = remainingDaysPaid;
                                        allowedToSave = 0;
                                    }
                                        
                                    break;
                            }

                            bool appliedParagraph26 = false;
                            if (useParagraph26)
                            {
                                if (vacationGroupSE.YearEndRemainingDaysRule == (int)TermGroup_VacationGroupYearEndRemainingDaysRule.Saved)
                                    appliedParagraph26 = TryApplyParagrah26Saved(calculation.Employee, vacationGroupFromDate, vacationGroupToDate, SR, ref SB, ref allowedToSave, employeeVacationSE?.RemainingDaysPaid ?? 0);
                                else
                                    appliedParagraph26 = TryApplyParagrah26(calculation.Employee, vacationGroupFromDate, vacationGroupToDate, SR, ref SB, ref daysToPay);
                            }

                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1, daysToPay));

                            #endregion

                            #region [Track] EmployeeVacationSE

                            decimal employmentRatePaid = SSG ?? XmlUtil.GetChildElementValueDecimal(calculation.EmployeeElement, "Employment", "WorkPercentage");
                            CreateEmployeeVacationSE(employmentRatePaid, allowedToSave, daysToPay, appliedParagraph26);

                            #endregion

                            #region Overdue days rule

                            decimal overdueDaysPaid = calculation.NewEmployeeVacationSE.RemainingDaysOverdue ?? 0;
                            switch ((TermGroup_VacationGroupYearEndOverdueDaysRule)vacationGroupSE.YearEndOverdueDaysRule)
                            {
                                case TermGroup_VacationGroupYearEndOverdueDaysRule.Paid:
                                    daysToPay = overdueDaysPaid;
                                    break;
                                case TermGroup_VacationGroupYearEndOverdueDaysRule.Saved:
                                    daysToPay = 0;
                                    break;
                            }

                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdue, daysToPay));

                            #endregion

                            #region SavedDaysYear1 (sammanfallande)

                            if (sammanfallande && calculation.NewEmployeeVacationSE.SavedDaysYear1.HasValue && calculation.NewEmployeeVacationSE.SavedDaysYear1.Value < 0)
                            {
                                vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1, calculation.NewEmployeeVacationSE.SavedDaysYear1.Value));
                                calculation.NewEmployeeVacationSE.SavedDaysYear1 = null;
                            }

                            #endregion

                            #region Vacation Salary Prepayment

                            if (vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw || vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement || vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition)
                            {
                                decimal vacationSalaryPrepaymentQuantity = 0;
                                if (vacationGroupSE.VacationSalaryPayoutRule == (int)TermGroup_VacationGroupVacationSalaryPayoutRule.AllBeforeVacation)
                                {
                                    vacationSalaryPrepaymentQuantity = calculation.NewEmployeeVacationSE.EarnedDaysPaid ?? 0;
                                }
                                else if (vacationGroupSE.VacationSalaryPayoutRule == (int)TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation)
                                {
                                    vacationSalaryPrepaymentQuantity = vacationGroupSE.VacationSalaryPayoutDays ?? 0;
                                    if (vacationSalaryPrepaymentQuantity > (calculation.NewEmployeeVacationSE.EarnedDaysPaid ?? 0))
                                        vacationSalaryPrepaymentQuantity = calculation.NewEmployeeVacationSE.EarnedDaysPaid ?? 0;
                                }
                                vacationAdditionOrSalaryPrepaymentDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment, TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment_Paid, vacationSalaryPrepaymentQuantity));
                            }

                            #endregion

                            #region Vacation Variable Addition Prepayment

                            if (vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw || vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement || vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition)
                                vacationAdditionOrSalaryPrepaymentDaysToPay.Add(CreateVacationVariableAdditionPrepayment(calculation.NewEmployeeVacationSE.EarnedDaysPaid ?? 0));

                            #endregion
                        }
                        else if (isFinalSalary)
                        {
                            VacationDaysCalculationDTO vacationDaysCalculation = GetVacationDaysInPeriod(timePeriod, calculateHours, calculation.EmployeeId);

                            #region Vacation Variable Addition Prepayment

                            if (vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw || vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement || vacationGroupSE.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition)
                                vacationDaysToPay.Add(CreateVacationVariableAdditionPrepayment(SB));

                            #endregion

                            #region Vacation to pay

                            bool isTransactionsAlreadyCreated = (inputDate == vacationGroupToDate) && HasVacationYearEndRowOnDate(calculation.EmployeeId, vacationGroupToDate, SoeVacationYearEndType.VacationYearEnd);
                            if (!isTransactionsAlreadyCreated)
                            {
                                if (sammanfallande)
                                {
                                    vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Earned, SB - (employeeVacationSE != null ? employeeVacationSE.UsedDaysPaid ?? 0 : 0) - vacationDaysCalculation.PeriodUsedDaysPaidCount));
                                }
                                else
                                {
                                    vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Earned, SB));
                                    vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Paid, (employeeVacationSE?.RemainingDaysPaid ?? 0) - vacationDaysCalculation.PeriodUsedDaysPaidCount));
                                }
                            }
                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear1, (employeeVacationSE?.RemainingDaysYear1 ?? 0) - vacationDaysCalculation.PeriodUsedAndCompensationDaysYear1Count));
                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear2, (employeeVacationSE?.RemainingDaysYear2 ?? 0) - vacationDaysCalculation.PeriodUsedAndCompensationDaysYear2Count));
                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear3, (employeeVacationSE?.RemainingDaysYear3 ?? 0) - vacationDaysCalculation.PeriodUsedAndCompensationDaysYear3Count));
                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear4, (employeeVacationSE?.RemainingDaysYear4 ?? 0) - vacationDaysCalculation.PeriodUsedAndCompensationDaysYear4Count));
                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedYear5, (employeeVacationSE?.RemainingDaysYear5 ?? 0) - vacationDaysCalculation.PeriodUsedAndCompensationDaysYear5Count));
                            vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdue, (employeeVacationSE?.RemainingDaysOverdue ?? 0) - vacationDaysCalculation.PeriodUsedAndCompensationDaysOverdueCount));
                            if (employeeVacationSE != null && employeeVacationSE.DebtInAdvanceAmount.HasValue && employeeVacationSE.DebtInAdvanceDueDate.HasValue && inputDate <= employeeVacationSE.DebtInAdvanceDueDate.Value && !employeeVacationSE.DebtInAdvanceDelete)
                                vacationDaysToPay.Add(new TimeEngineVacationYearEndDayToPay(TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation, TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_Advance, Decimal.One, Decimal.MinusOne * (employeeVacationSE.DebtInAdvanceAmount.Value + vacationDaysCalculation.PeriodDebtAdvanceAmount)));

                            #endregion
                        }

                        #region [Track] VacationYearEndRow and EmployeeFactors

                        CreateVacationYearEndRow();
                        CreateEmployeeFactors();

                        #endregion

                        calculationResult = Save();
                        if (!calculationResult.Success)
                            return calculationResult;

                        #region [Track] TimePayrollTransactions

                        calculationResult = CreateTransactionsForVacationDays();
                        if (!calculationResult.Success)
                            return calculationResult;

                        if (isFinalSalary)
                        {
                            calculationResult = CreateTransactionsForFinalSalary();
                            if (!calculationResult.Success)
                                return calculationResult;
                        }

                        if (isVacationYearEnd)
                        {
                            calculationResult = CreateTransactionsForAdditionPrepayment();
                            if (!calculationResult.Success)
                                return calculationResult;
                        }

                        if (isFinalSalary)
                        {
                            calculationResult = ApplyTimeWorkAccountFinalSalary(calculation);
                            if (!calculationResult.Success)
                                return calculationResult;
                        }

                        #endregion

                        if (vacationGroupFromDate != CalendarUtility.DATETIME_DEFAULT && vacationGroupFromDate <= vacationGroupToDate)
                            calculation.SetValidVacationGroup();

                        void CreateEmployeeVacationSE(decimal employmentRatePaid, decimal allowedToSave, decimal daysToPay, bool appliedParagraph26)
                        {
                            var newVacationSE = this.CreateEmployeeVacationSE(calculation.EmployeeId, employeeVacationSE, sammanfallande, SR, SB, employmentRatePaid, savedDaysYear1: appliedParagraph26 ? allowedToSave + daysToPay : (decimal?)null);
                            ChangeEntityState(employeeVacationSE, SoeEntityState.Deleted);
                            calculation.SetEmployeeVacationSE(newVacationSE, employeeVacationSE);
                        }
                        void CreateVacationYearEndRow()
                        {
                            calculation.SetVacationYearEndRow(this.CreateVacationYearEndRow(vacationYearEndHead, employeeVacationSE, calculation.NewEmployeeVacationSE, timePeriod, isFinalSalary, calculation.EmployeeId));
                        }
                        void CreateEmployeeFactors()
                        {
                            if(isFinalSalary)                            
                                calculation.AddEmployeeFactor(EmployeeFactor.Create(calculation.VacationYearEndRow, vacationGroup, TermGroup_EmployeeFactorType.VacationDayPercentFinalSalary, inputDate, SLD, userDetails, created));                            
                            else                            
                                calculation.AddEmployeeFactor(EmployeeFactor.Create(calculation.VacationYearEndRow, vacationGroup, TermGroup_EmployeeFactorType.VacationDayPercent, inputDate.AddDays(1), SLD, userDetails, created));
                                                        
                            calculation.AddEmployeeFactor(EmployeeFactor.Create(calculation.VacationYearEndRow, vacationGroup, TermGroup_EmployeeFactorType.VacationVariableAmountPerDay, isFinalSalary ? inputDate : inputDate.AddDays(1), STRD, userDetails, created));
                        }
                        TimeBlockDate GetOrCreateTimeBlockDate(DateTime date)
                        {
                            return calculation.TimeBlockDates.FirstOrDefault(t => t.Date == date) ?? calculation.AddTimeBlockDate(GetTimeBlockDateFromCache(calculation.EmployeeId, date, true));
                        }
                        TimeCodeTransaction CreateTimeCodeTransactions(int timeCodeId, TimeBlockDate timeBlockDate, decimal quantity, decimal amount)
                        {
                            TimeCodeTransaction timeCodeTransaction = CreateTimeCodeTransaction(timeCodeId, TimeCodeTransactionType.Time, quantity, timeBlockDate.Date, timeBlockDate.Date, amount, timeBlockDate: timeBlockDate);
                            calculation.AddTimeCodeTransactions(timeCodeTransaction);
                            return timeCodeTransaction;
                        }
                        List<TimePayrollTransaction> CreateTimePayrollTransactions(Employment employment, PayrollProduct payrollProduct, TimeBlockDate timeBlockDate, int? transactionTimePeriodId, decimal quantity, decimal amount, TimeCodeTransaction timeCodeTransaction = null)
                        {
                            TimePayrollTransaction newTimePayrollTransaction = CreateTimePayrollTransaction(payrollProduct, timeBlockDate, quantity, amount, 0, amount, string.Empty, attestStateInitial.AttestStateId, transactionTimePeriodId, calculation.EmployeeId, timeCodeTransaction: timeCodeTransaction, vacationYearEndRow: calculation.VacationYearEndRow);
                            CreateTimePayrollTransactionExtended(newTimePayrollTransaction, calculation.EmployeeId, actorCompanyId);
                            ApplyAccountingOnTimePayrollTransaction(newTimePayrollTransaction, calculation.Employee, timeBlockDate.Date, payrollProduct, setAccountInternal: true);

                            List<TimePayrollTransaction> timePayrollTransactions = CreateFixedAccountingTransactions(newTimePayrollTransaction, employment, calculation.Employee, timeBlockDate.Date);
                            timePayrollTransactions.Add(newTimePayrollTransaction);

                            calculation.AddTimePayrollTransactions(timePayrollTransactions.ToArray());

                            return timePayrollTransactions;
                        }
                        TimeEngineVacationYearEndDayToPay CreateVacationVariableAdditionPrepayment(decimal earnedDaysPaid)
                        {
                            TermGroup_SysPayrollType level2 = TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment;
                            TermGroup_SysPayrollType level3 = TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment_Paid;
                            decimal vacationVariablePrepaymentQuantity = 0;

                            if (vacationGroupSE.VacationVariablePayoutRule == (int)TermGroup_VacationGroupVacationSalaryPayoutRule.AllBeforeVacation)
                            {
                                vacationVariablePrepaymentQuantity = earnedDaysPaid;
                            }
                            else if (vacationGroupSE.VacationVariablePayoutRule == (int)TermGroup_VacationGroupVacationSalaryPayoutRule.PartlyPayoutBeforeVacation)
                            {
                                if (isFinalSalary)
                                {
                                    vacationVariablePrepaymentQuantity = earnedDaysPaid;
                                }
                                else
                                {
                                    vacationVariablePrepaymentQuantity = vacationGroupSE.VacationVariablePayoutDays ?? 0;
                                    if (vacationVariablePrepaymentQuantity > (earnedDaysPaid))
                                        vacationVariablePrepaymentQuantity = earnedDaysPaid;
                                }
                            }
                            else if (vacationGroupSE.VacationVariablePayoutRule == (int)TermGroup_VacationGroupVacationSalaryPayoutRule.InConjunctionWithVacation && vacationGroupSE.YearEndVacationVariableRule == (int)TermGroup_VacationGroupYearEndVacationVariableRule.Paid)
                            {
                                level2 = TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation;
                                level3 = TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdueVariable;

                                vacationVariablePrepaymentQuantity = (earnedDaysPaid) - (employeeVacationSE.PaidVacationVariableAllowance ?? 0);
                                if (vacationVariablePrepaymentQuantity < 0)
                                    vacationVariablePrepaymentQuantity = 0;
                            }
                            return new TimeEngineVacationYearEndDayToPay(level2, level3, vacationVariablePrepaymentQuantity);
                        }
                        ActionResult CreateTransactionsForVacationDays()
                        {
                            if (!vacationDaysToPay.Any(d => d.Days != 0))
                                return new ActionResult(true);

                            var localResult = new ActionResult(true);

                            DateTime transactionDate = inputDate;
                            int? transactionTimePeriodId = timePeriodId;
                            Employment employment;

                            if (isVacationYearEnd)
                            {
                                int? month = vacationGroupSE.RemainingDaysPayoutMonth.ToNullable();
                                if (!month.HasValue)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8753, "Inställning 'Kvarvarande semestersaldo att utbetala - utbetalas i' på semesteravtalet saknas"));

                                localResult = GetTimePeriodForVacationYearEnd(calculation.Employee, transactionDate, month.Value, out TimePeriod currentTimePeriod, out bool skip);
                                if (skip)
                                    return new ActionResult(true);
                                if (!localResult.Success)
                                    return localResult;

                                transactionTimePeriodId = currentTimePeriod.TimePeriodId;
                                transactionDate = currentTimePeriod.StopDate;
                                employment = calculation.Employee.GetEmployment(currentTimePeriod.StartDate, currentTimePeriod.StopDate, forward: false);
                            }
                            else
                            {
                                employment = calculation.Employee.GetEmployment(transactionDate);
                            }

                            if (employment == null)
                                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte") + ". " + calculation.Employee.EmployeeNrAndName);


                            TimeBlockDate timeBlockDate = GetOrCreateTimeBlockDate(transactionDate);

                            foreach (var vacationDayToPay in vacationDaysToPay.Where(d => d.Days != 0))
                            {
                                PayrollProduct payrollProduct = GetPayrollProductFromCache((int)TermGroup_SysPayrollType.SE_GrossSalary, (int)vacationDayToPay.Level2, (int)vacationDayToPay.Level3);
                                if (payrollProduct == null)
                                    return new ActionResult((int)ActionResultSave.EntityIsNull, string.Format(GetText(8839, "Löneart med lönetyp: {0} - {1} - {2} saknas"), GetText((int)TermGroup_SysPayrollType.SE_GrossSalary, (int)TermGroup.SysPayrollType), GetText((int)vacationDayToPay.Level2, (int)TermGroup.SysPayrollType), GetText((int)vacationDayToPay.Level3, (int)TermGroup.SysPayrollType)));

                                TimeBlockDate transactionTimeBlockDate = timeBlockDate;                                
                                PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employment.GetPayrollGroupId(transactionTimeBlockDate.Date));
                                List<TimePayrollTransaction> timePayrollTransactions = CreateTimePayrollTransactions(employment, payrollProduct, transactionTimeBlockDate, transactionTimePeriodId, vacationDayToPay.Days, vacationDayToPay.Amount);
                                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                                {
                                    if (payrollProductSetting != null)
                                        timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit = payrollProductSetting.TimeUnit;

                                    if (timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit == (int)TermGroup_PayrollProductTimeUnit.WorkDays)
                                    {
                                        timePayrollTransaction.TimePayrollTransactionExtended.QuantityWorkDays = vacationDayToPay.Days;
                                    }
                                    else
                                    {
                                        timePayrollTransaction.TimePayrollTransactionExtended.QuantityCalendarDays = vacationDayToPay.Days;
                                        timePayrollTransaction.TimePayrollTransactionExtended.CalenderDayFactor = 1;
                                    }

                                    localResult = SaveTimePayrollTransactionAmounts(transactionTimeBlockDate, timePayrollTransaction);
                                    if (!localResult.Success)
                                        return localResult;
                                }
                            }

                            return localResult;
                        }
                        ActionResult CreateTransactionsForFinalSalary()
                        {
                            var localResult = new ActionResult(true);

                            List<TimeAccumulatorItem> timeAccumulatorItems = GetTimeAccumulatorItemsForFinalSalary(timeAccumulatorsForFinalSalary, calculation.Employee, timePeriod, inputDate);
                            foreach (TimeAccumulatorItem timeAccumulatorItem in timeAccumulatorItems)
                            {
                                if (!timeAccumulatorItem.FinalSalary || !timeAccumulatorItem.TimeCodeId.HasValue)
                                    continue;

                                TimeCode timeCode = GetTimeCodeWithProductsFromCache(timeAccumulatorItem.TimeCodeId.Value);
                                if (timeCode == null)
                                    continue;

                                TimeBlockDate timeBlockDate = GetOrCreateTimeBlockDate(inputDate);

                                TimeCodeTransaction timeCodeTransaction = CreateTimeCodeTransactions(timeAccumulatorItem.TimeCodeId.Value, timeBlockDate, -timeAccumulatorItem.SumAccToday, 0);
                                if (timeCodeTransaction == null)
                                    continue;

                                foreach (TimeCodePayrollProduct timeCodePayrollProduct in timeCode.TimeCodePayrollProduct)
                                {
                                    decimal quantity = timeCodeTransaction.Quantity * timeCodePayrollProduct.Factor;
                                    List<TimePayrollTransaction> timePayrollTransactions = CreateTimePayrollTransactions(calculation.Employment, timeCodePayrollProduct.PayrollProduct, timeBlockDate, timePeriodId, quantity, 0, timeCodeTransaction: timeCodeTransaction);
                                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                                    {
                                        localResult = SaveTimePayrollTransactionAmounts(timeBlockDate, timePayrollTransaction);
                                        if (!localResult.Success)
                                            return localResult;
                                    }
                                }
                            }

                            return localResult;
                        }
                        ActionResult CreateTransactionsForAdditionPrepayment()
                        {
                            var localResult = new ActionResult(true);

                            foreach (var vacationAdditionOrSalaryPrepaymentDayToPay in vacationAdditionOrSalaryPrepaymentDaysToPay.Where(i => i.Days != 0).GroupBy(i => i.Level2))
                            {
                                int? month = null;
                                TermGroup_SysPayrollType payrollTypeLevel2 = vacationAdditionOrSalaryPrepaymentDayToPay.Key;
                                if (payrollTypeLevel2 == TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryPrepayment)
                                {
                                    month = vacationGroupSE.VacationSalaryPayoutMonth;
                                    if (!month.HasValue)
                                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8833, "Inställning 'Utbetalning av semestertillägg - utbetalas i' på semesteravtalet saknas"));
                                }
                                else if (payrollTypeLevel2 == TermGroup_SysPayrollType.SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment)
                                {
                                    month = vacationGroupSE.VacationVariablePayoutMonth;
                                    if (!month.HasValue)
                                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10251, "Inställning 'Utbetalning av rörligt semestertillägg - utbetalas i' på semesteravtalet saknas"));
                                }
                                else if (payrollTypeLevel2 == TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation)
                                {
                                    month = vacationGroupSE.RemainingDaysPayoutMonth;
                                    if (!month.HasValue)
                                        return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(8753, "Inställning 'Kvarvarande semestersaldo att utbetala - utbetalas i' på semesteravtalet saknas"));
                                }
                                if (!month.HasValue)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10251, "Inställning 'Utbetalning av rörligt semestertillägg - utbetalas i' på semesteravtalet saknas"));

                                localResult = GetTimePeriodForVacationYearEnd(calculation.Employee, inputDate, month.Value, out TimePeriod currentTimePeriod, out bool skip);
                                if (skip)
                                    continue;
                                if (!localResult.Success)
                                    return localResult;

                                DateTime transactionDate = currentTimePeriod.StopDate;
                                int? transactionTimePeriodId = currentTimePeriod.TimePeriodId;

                                Employment employment = calculation.Employee.GetEmployment(currentTimePeriod.StartDate, currentTimePeriod.StopDate, forward: false);
                                if (employment == null)
                                    return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte") + ". " + calculation.Employee.EmployeeNrAndName);

                                TimeBlockDate timeBlockDateForTransactionDate = GetOrCreateTimeBlockDate(transactionDate);

                                foreach (var vacationAdditionOrSalaryPrepaymentItem in vacationAdditionOrSalaryPrepaymentDayToPay)
                                {
                                    decimal quantityDays = vacationAdditionOrSalaryPrepaymentItem.Days;
                                    if (quantityDays == 0)
                                        continue;

                                    PayrollProduct payrollProduct = GetPayrollProductFromCache((int)TermGroup_SysPayrollType.SE_GrossSalary, (int)vacationAdditionOrSalaryPrepaymentItem.Level2, (int)vacationAdditionOrSalaryPrepaymentItem.Level3);
                                    if (payrollProduct == null)
                                        return new ActionResult((int)ActionResultSave.EntityIsNull, string.Format(GetText(8839, "Löneart med lönetyp: {0} - {1} - {2} saknas"), GetText((int)TermGroup_SysPayrollType.SE_GrossSalary, (int)TermGroup.SysPayrollType), GetText((int)vacationAdditionOrSalaryPrepaymentItem.Level2, (int)TermGroup.SysPayrollType), GetText((int)vacationAdditionOrSalaryPrepaymentItem.Level3, (int)TermGroup.SysPayrollType)));

                                    bool isSavedOverdueVariable = vacationAdditionOrSalaryPrepaymentItem.Level2 == TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation && vacationAdditionOrSalaryPrepaymentItem.Level3 == TermGroup_SysPayrollType.SE_GrossSalary_VacationCompensation_SavedOverdueVariable;
                                    TimeBlockDate timeBlockDateToUse = isSavedOverdueVariable ? GetOrCreateTimeBlockDate(calculation.VacationGroupToDate) : timeBlockDateForTransactionDate;
                                    PayrollProductSetting payrollProductSetting = GetPayrollProductSetting(payrollProduct, employment.GetPayrollGroupId(timeBlockDateToUse.Date));
                                    List<TimePayrollTransaction> timePayrollTransactions = CreateTimePayrollTransactions(employment, payrollProduct, timeBlockDateToUse, transactionTimePeriodId, quantityDays, vacationAdditionOrSalaryPrepaymentItem.Amount);

                                    foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions)
                                    {
                                        if (payrollProductSetting != null)
                                            timePayrollTransaction.TimePayrollTransactionExtended.TimeUnit = payrollProductSetting.TimeUnit;

                                        localResult = SaveTimePayrollTransactionAmounts(timeBlockDateToUse, timePayrollTransaction);
                                        if (!localResult.Success)
                                            return localResult;
                                    }
                                }
                            }

                            return localResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                        return new ActionResult(ex);
                    }
                    return calculationResult;
                }

                ActionResult SaveVacationYearEndRowStatus(TimeEngineVacationYearEndEmployee calculation)
                {
                    calculation.VacationYearEndRow.Status = (int)calculation.Status;
                    calculation.VacationYearEndRow.Message = calculation.Result.Success ? calculation.Result.InfoMessage : calculation.Result.ErrorMessage;
                    SetModifiedProperties(calculation.VacationYearEndRow);
                    return Save();
                }
            }

            if (isFinalSalary && !calculatedEmployees.Any(e => e.HasValidVacationGroup))
                return new ActionResult((int)ActionResultSave.VacationYearEndFailed, GetText(10031, "Ofullständiga inställningar på anställd"));
            if (vacationYearEndHead == null)
                return new ActionResult((int)ActionResultSave.VacationYearEndFailed, GetText(10028, "Semesterårsskifte kunde inte skapas, anställda saknas i valt semesteravtal"));

            #endregion

            #region DataStorage

            DataStorage dataStorage = GeneralManager.CreateDataStorage(entities, SoeDataStorageRecordType.VacationYearEndHead, xdoc.ToString(), reportPrintout.Data, null, null, base.ActorCompanyId, vacationYearEndHead?.VacationGroups);
            GeneralManager.CreateDataStorageRecord(entities, SoeDataStorageRecordType.VacationYearEndHead, vacationYearEndHead.VacationYearEndHeadId, null, SoeEntityType.VacationYearEndHead, dataStorage);

            result = Save();
            if (!result.Success)
                return result;

            #endregion

            result.Value = calculatedEmployees;
            return result;
        }

        private bool TryApplyParagrah26Saved(Employee employee, DateTime vacationGroupFromDate, DateTime vacationGroupToDate, decimal SR, ref decimal SB, ref decimal allowedToSave, decimal remainingDaysPaid)
        {
            if (SR >= SB && employee.GetEmploymentDays(vacationGroupFromDate, vacationGroupToDate) >= 365)
            {
                SB += remainingDaysPaid;
                if (SB > SR)
                {
                    allowedToSave = SB - SR;
                    SB = SR;
                }
                else
                    allowedToSave = 0;
                return true;
            }
            else
                return false;
        }

        private bool TryApplyParagrah26(Employee employee, DateTime vacationGroupFromDate, DateTime vacationGroupToDate, decimal SR, ref decimal SB, ref decimal daysToPay)
        {
            if (SR > SB && daysToPay > 0 && employee.GetEmploymentDays(vacationGroupFromDate, vacationGroupToDate) >= 365)
            {
                SB += daysToPay;
                if (SB > SR)
                {
                    daysToPay = SB - SR;
                    SB = SR;
                }
                else
                    daysToPay = 0;
                return true;
            }
            else
                return false;
        }

        private static void GetVacationYearEndFormulaValues(XElement employeeElement, out decimal SLD, out decimal STRD, out decimal SR, out decimal SB, out decimal? SSG)
        {
            SLD = 0;
            STRD = 0;
            SR = 0;
            SB = 0;
            SSG = null;

            List<XElement> formulaElements = XmlUtil.GetChildElements(employeeElement, "Formula");
            foreach (XElement formulaElement in formulaElements)
            {
                string formulaValue = XmlUtil.GetChildElementValue(formulaElement, "FormulaValue");
                if (!string.IsNullOrEmpty(formulaValue))
                {
                    formulaValue = formulaValue.Replace(".", ",");

                    string formulaName = XmlUtil.GetChildElementValue(formulaElement, "FormulaName");
                    if (formulaName == "SLD")
                        Decimal.TryParse(formulaValue, out SLD);
                    else if (formulaName == "SR")
                        Decimal.TryParse(formulaValue, out SR);
                    else if (formulaName == "SB")
                        Decimal.TryParse(formulaValue, out SB);
                    else if (formulaName == "STRD")
                        Decimal.TryParse(formulaValue, out STRD);
                    else if (formulaName == "SSG" && Decimal.TryParse(formulaValue, out decimal ssg))
                        SSG = ssg > 0 ? Decimal.Round(ssg * 100, 2) : 0;
                }
            }
        }

        private VacationDaysCalculationDTO GetVacationDaysInPeriod(TimePeriod timePeriod, bool calculateHours, int employeeId)
        {
            var timePayrollTransactionsForEmployeeInPeriod = TimeTransactionManager.GetTimePayrollTransactionItemsForEmployee(entities, employeeId, timePeriod);
            var timePayrollTransactionsForEmployeeAll = GetTimePayrollTransactionsWithExtendedAndTimeBlockDate(timePayrollTransactionsForEmployeeInPeriod, true);
            return timePayrollTransactionsForEmployeeAll.ToVacationDaysCalculationDTO(calculateHours);
        }

        private EmployeeVacationSE CreateEmployeeVacationSE(int employeeId, EmployeeVacationSE employeeVacationSE, bool sammanfallande, decimal SR, decimal SB, decimal employmentRatePaid, decimal? savedDaysYear1 = null)
        {
            decimal? daysOverdue = employeeVacationSE != null ? ((employeeVacationSE.RemainingDaysOverdue ?? 0M) + (employeeVacationSE.RemainingDaysYear5 ?? 0M)).ToNullable<decimal>() : null;
            if (!savedDaysYear1.HasValue)
                savedDaysYear1 = CalculateRemainingDaysPayed(employeeVacationSE, sammanfallande, SB);

            EmployeeVacationSE newVacationSE = new EmployeeVacationSE()
            {
                PrevEmployeeVacationSEId = employeeVacationSE?.EmployeeVacationSEId,

                EarnedDaysPaid = sammanfallande ? SR : SB,
                EarnedDaysUnpaid = sammanfallande ? 0 : SR - SB,
                EarnedDaysAdvance = 0,
                SavedDaysYear1 = savedDaysYear1,
                SavedDaysYear2 = employeeVacationSE?.RemainingDaysYear1,
                SavedDaysYear3 = employeeVacationSE?.RemainingDaysYear2,
                SavedDaysYear4 = employeeVacationSE?.RemainingDaysYear3,
                SavedDaysYear5 = employeeVacationSE?.RemainingDaysYear4,
                SavedDaysOverdue = daysOverdue,

                UsedDaysPaid = 0,
                PaidVacationAllowance = 0,
                PaidVacationVariableAllowance = 0,
                UsedDaysUnpaid = 0,
                UsedDaysAdvance = 0,
                UsedDaysYear1 = 0,
                UsedDaysYear2 = 0,
                UsedDaysYear3 = 0,
                UsedDaysYear4 = 0,
                UsedDaysYear5 = 0,
                UsedDaysOverdue = 0,

                RemainingDaysPaid = sammanfallande ? SR : SB,
                RemainingDaysUnpaid = sammanfallande ? 0 : SR - SB,
                RemainingDaysAdvance = 0,
                RemainingDaysYear1 = savedDaysYear1,
                RemainingDaysYear2 = employeeVacationSE?.RemainingDaysYear1,
                RemainingDaysYear3 = employeeVacationSE?.RemainingDaysYear2,
                RemainingDaysYear4 = employeeVacationSE?.RemainingDaysYear3,
                RemainingDaysYear5 = employeeVacationSE?.RemainingDaysYear4,
                RemainingDaysOverdue = daysOverdue,

                EarnedDaysRemainingHoursPaid = 0,
                EarnedDaysRemainingHoursUnpaid = 0,
                EarnedDaysRemainingHoursAdvance = 0,
                EarnedDaysRemainingHoursYear1 = employeeVacationSE?.EarnedDaysRemainingHoursPaid,
                EarnedDaysRemainingHoursYear2 = employeeVacationSE?.EarnedDaysRemainingHoursYear1,
                EarnedDaysRemainingHoursYear3 = employeeVacationSE?.EarnedDaysRemainingHoursYear2,
                EarnedDaysRemainingHoursYear4 = employeeVacationSE?.EarnedDaysRemainingHoursYear3,
                EarnedDaysRemainingHoursYear5 = employeeVacationSE?.EarnedDaysRemainingHoursYear4,
                EarnedDaysRemainingHoursOverdue = employeeVacationSE != null ? employeeVacationSE.EarnedDaysRemainingHoursOverdue + employeeVacationSE.EarnedDaysRemainingHoursYear5 : null,

                EmploymentRatePaid = employmentRatePaid,
                EmploymentRateYear1 = employeeVacationSE?.EmploymentRatePaid,
                EmploymentRateYear2 = employeeVacationSE?.EmploymentRateYear1,
                EmploymentRateYear3 = employeeVacationSE?.EmploymentRateYear2,
                EmploymentRateYear4 = employeeVacationSE?.EmploymentRateYear3,
                EmploymentRateYear5 = employeeVacationSE?.EmploymentRateYear4,
                EmploymentRateOverdue = employeeVacationSE?.EmploymentRateYear5,

                DebtInAdvanceAmount = employeeVacationSE?.DebtInAdvanceAmount,
                DebtInAdvanceDueDate = employeeVacationSE?.DebtInAdvanceDueDate,
                DebtInAdvanceDelete = employeeVacationSE?.DebtInAdvanceDelete ?? false,

                //Set FK
                EmployeeId = employeeId,
            };
            SetCreatedProperties(newVacationSE);
            entities.EmployeeVacationSE.AddObject(newVacationSE);
            return newVacationSE;
        }

        private decimal? CalculateRemainingDaysPayed(EmployeeVacationSE existingEmployeeVacationSE, bool sammanfallande, decimal SB)
        {
            if (sammanfallande)
                return SB - (existingEmployeeVacationSE?.UsedDaysPaid ?? 0);
            else
                return existingEmployeeVacationSE?.RemainingDaysPaid;
        }

        private ActionResult GetTimePeriodForVacationYearEnd(Employee employee, DateTime date, int month, out TimePeriod timePeriod, out bool doSkipEmployee)
        {
            doSkipEmployee = false;
            timePeriod = GetLatestTimePeriodForEmployee(employee, date, month);
            if (timePeriod == null)
            {
                if (employee.GetLastEmployment()?.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary)
                    doSkipEmployee = true;
                else
                    return new ActionResult((int)ActionResultSave.EntityNotFound, string.Format(GetText(12057, "Period för utbetalning hittades inte för anställd {0}. Om den anställde slutat så ska den anställde markeras för slutavräkning, och kommer då exkluderas i semesterårsskiftet"), employee.EmployeeNrAndName));
            }
            return new ActionResult(true);
        }

        private VacationYearEndHead CreateVacationYearEndHead(SoeVacationYearEndType type, TermGroup_VacationYearEndHeadContentType contentType, DateTime date, List<string> vacationGroupNames)
        {
            VacationYearEndHead vacationYearEndHead = new VacationYearEndHead()
            {
                Date = date,
                VacationGroups = vacationGroupNames?.Distinct().ToCommaSeparated(),
                Type = (int)type,
                ContentType = (int)contentType,

                //Set FK
                ActorCompanyId = base.ActorCompanyId,
            };
            SetCreatedProperties(vacationYearEndHead);
            entities.VacationYearEndHead.AddObject(vacationYearEndHead);

            return vacationYearEndHead;
        }

        private VacationYearEndRow CreateCleanupVacationYearEndRow(TimeEngineVacationYearEndEmployee calculation, VacationYearEndHead vacationYearEndHead)
        {
            if (calculation == null || vacationYearEndHead == null)
                return null;

            VacationYearEndRow vacationYearEndRow = new VacationYearEndRow()
            {
                //Set FK
                EmployeeVacationSEId = null,
                EmployeeId = calculation.EmployeeId,
                TimePeriodId = calculation.VacationYearEndRow?.TimePeriodId ?? calculation.TimePeriod?.TimePeriodId.ToNullable(),
            };
            SetCreatedProperties(vacationYearEndRow);
            vacationYearEndHead.VacationYearEndRow.Add(vacationYearEndRow);
            return vacationYearEndRow;
        }

        private VacationYearEndRow CreateVacationYearEndRow(VacationYearEndHead vacationYearEndHead, EmployeeVacationSE existingEmployeeVacationSE, EmployeeVacationSE newVacationSE, TimePeriod timePeriod, bool isFinalSalary, int employeeId)
        {
            VacationYearEndRow vacationYearEndRow = new VacationYearEndRow()
            {
                //Set references
                EmployeeVacationSE = newVacationSE ?? existingEmployeeVacationSE,

                //Set FK
                EmployeeId = employeeId,
            };
            SetCreatedProperties(vacationYearEndRow);
            vacationYearEndHead.VacationYearEndRow.Add(vacationYearEndRow);

            if (isFinalSalary)
                vacationYearEndRow.TimePeriodId = timePeriod.TimePeriodId;
            return vacationYearEndRow;
        }

        private ActionResult DeleteVacationYearEnd(int vacationYearEndHeadId, SoeVacationYearEndType type, int? timePeriodId = null, int? employeeId = null)
        {
            #region Validation

            bool isVacationYearEnd = type == SoeVacationYearEndType.VacationYearEnd;
            bool isFinalSalary = type == SoeVacationYearEndType.FinalSalary;

            if (!isVacationYearEnd && !isFinalSalary)
                return new ActionResult((int)ActionResultDelete.InsufficientInput);
            if (isFinalSalary && !timePeriodId.HasValue)
                return new ActionResult((int)ActionResultDelete.InsufficientInput);

            VacationYearEndHead head = GetVacationYearEndWithRows(vacationYearEndHeadId);
            if (head == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "VacationYearEndHead");
            if (head.VacationYearEndRow.IsNullOrEmpty())
                return new ActionResult((int)ActionResultDelete.EntityNotFound, "VacationYearEndRow");

            List<VacationYearEndRow> vacationYearEndRows = head.VacationYearEndRow.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            if (employeeId.HasValue)
                vacationYearEndRows = vacationYearEndRows.Where(i => i.EmployeeId == employeeId.Value).ToList();

            // Check if period is locked. In that case it's not possible to roll back the vacation year end.
            if (isFinalSalary && HasPeriodLockedForChanges(vacationYearEndRows.Select(r => r.EmployeeId).ToList(), head.Date.AddYears(1)))
                return new ActionResult((int)ActionResultDelete.TimePeriodLocked, GetText(11015, "Slutlön går inte att backa!\n\nEn eller flera anställda som ingår har en löneperiod som låsts efter att semesterårsskiftet skapades."));

            #endregion

            #region VacationYearEndRow

            foreach (VacationYearEndRow vacationYearEndRow in vacationYearEndRows)
            {
                #region Delete VacationYearEndRow

                // Delete VacationYearEndRow
                ChangeEntityState(vacationYearEndRow, SoeEntityState.Deleted);

                #endregion

                #region Delete EmployeeVacationSE

                if (isVacationYearEnd && vacationYearEndRow.EmployeeVacationSEId.HasValue)
                {
                    EmployeeVacationSE employeeVacationSE = GetEmployeeVacationSEDiscardedState(vacationYearEndRow.EmployeeVacationSEId.Value);
                    if (employeeVacationSE != null)
                    {
                        ChangeEntityState(employeeVacationSE, SoeEntityState.Deleted);

                        // Check previous EmployeeVacationSE
                        if (employeeVacationSE.PrevEmployeeVacationSEId.HasValue)
                        {
                            if (!employeeVacationSE.PrevEmployeeVacationSEReference.IsLoaded)
                                employeeVacationSE.PrevEmployeeVacationSEReference.Load();

                            if (employeeVacationSE.PrevEmployeeVacationSE?.State == (int)SoeEntityState.Deleted)
                            {
                                // Activate prev EmployeeVacationSE
                                ChangeEntityState(employeeVacationSE.PrevEmployeeVacationSE, SoeEntityState.Active);

                                List<EmployeeVacationSE> vacations = EmployeeManager.GetEmployeeVacationSEs(entities, employeeVacationSE.EmployeeId);
                                foreach (EmployeeVacationSE vacation in vacations)
                                {
                                    if (vacation.EmployeeVacationSEId == employeeVacationSE.PrevEmployeeVacationSE.EmployeeVacationSEId || vacation.EmployeeVacationSEId == employeeVacationSE.EmployeeVacationSEId)
                                        continue;

                                    ChangeEntityState(vacation, SoeEntityState.Deleted);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region EmployeeFactor

                List<EmployeeFactor> factors = GetEmployeeFactors(vacationYearEndRow.EmployeeId, vacationYearEndRow.VacationYearEndRowId);
                foreach (EmployeeFactor factor in factors)
                {
                    ChangeEntityState(factor, SoeEntityState.Deleted);
                }

                #endregion

                #region TimePayrollTransaction

                List<TimePayrollTransaction> timePayrollTransactions = GetTimePayrollTransactionsForCompanyAndVacationYearEnd(vacationYearEndRow.VacationYearEndRowId, timePeriodId);
                if (timePayrollTransactions.Any() && isFinalSalary)
                {
                    timePayrollTransactions = timePayrollTransactions.Where(i => i.TimePeriodId == timePeriodId.Value).ToList();
                    if (!timePayrollTransactions.Any())
                        return new ActionResult((int)ActionResultDelete.FinalSalary_TimePayrollTransactionsNotFound, GetText(11016, "Slutlön kan inte backas. Inga transaktioner hittades för vald period"));
                }

                ActionResult result = SetTimePayrollTransactionsToDeleted(timePayrollTransactions, saveChanges: false);
                if (!result.Success)
                    return result;

                foreach (TimePayrollTransaction timePayrollTransaction in timePayrollTransactions.Where(i => i.TimeCodeTransactionId.HasValue && !i.TimeWorkAccountYearOutcomeId.HasValue) )
                {
                    if (!timePayrollTransaction.TimeCodeTransactionReference.IsLoaded)
                        timePayrollTransaction.TimeCodeTransactionReference.Load();

                    result = SetTimeCodeTransactionToDeleted(timePayrollTransaction.TimeCodeTransaction, saveChanges: false);
                    if (!result.Success)
                        return result;
                }
                if (timePayrollTransactions.Any(w => w.TimeWorkAccountYearOutcomeId.HasValue))
                {
                    result = ReverseTimeWorkAccountTransactionFinalSalary(timePayrollTransactions.Where(w => w.TimeWorkAccountYearOutcomeId.HasValue).ToList());
                    if (!result.Success)
                        return result;
                }
                #endregion
            }

            #endregion

            #region Head

            if (head.VacationYearEndRow.All(i => i.State == (int)SoeEntityState.Deleted))
                ChangeEntityState(head, SoeEntityState.Deleted);

            #endregion

            #region DataStorage

            if (head.State == (int)SoeEntityState.Deleted)
            {
                DataStorageRecord record = GeneralManager.GetDataStorageRecord(entities, base.ActorCompanyId, head.VacationYearEndHeadId, SoeEntityType.VacationYearEndHead);
                if (record != null)
                {
                    DataStorage storage = GeneralManager.GetDataStorage(entities, record.DataStorageId, base.ActorCompanyId);
                    if (storage != null)
                        ChangeEntityState(storage, SoeEntityState.Deleted);
                }
            }

            #endregion

            return Save();
        }

        private Report GetEmployeeVacationDebtReport()
        {
            int defaultReportId = ReportManager.GetCompanySettingReportId(SettingMainType.Company, CompanySettingType.DefaultEmployeeVacationDebtReport, SoeReportTemplateType.EmployeeVacationDebtReport, this.actorCompanyId, this.userId);
            return ReportManager.GetReport(entities, defaultReportId, this.actorCompanyId) ?? ReportManager.GetStandardReport(base.ActorCompanyId, SoeReportTemplateType.EmployeeVacationDebtReport);
        }

        private bool DoSkipEmployeeFromVacationYearEnd(Employee employee, Employment employment, TimePeriod timePeriod)
        {
            //VacationYearEnd should not be done if Employee hasnt got employment (but finalsalary should go)
            if (employee == null || employment == null)
                return true;

            //VacationYearEnd should not be done if Employee has applied finalsalary in same period
            List<VacationYearEndRow> vacationYearEndRows = GetVacationYearEndRows(employee.EmployeeId, SoeVacationYearEndType.FinalSalary, timePeriodId: timePeriod?.TimePeriodId);
            if (vacationYearEndRows.Any(a => employee.GetEmployment(a.VacationYearEndHead.Date) != null && employee.GetEmployment(a.VacationYearEndHead.Date).EmploymentId == employment.EmploymentId))
                return true;

            return false;
        }

        private ActionResult ValidateEmployeeForVacationYearEnd(Employee employee, Employment employment, DateTime vacationGroupFromDate, DateTime vacationGroupToDate)
        {
            if (employee == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(10083, "Anställd hittades inte"));
            if (employment == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10084, "Anställning hittades inte"));
            if (employment.FinalSalaryStatus == (int)SoeEmploymentFinalSalaryStatus.ApplyFinalSalary && employment.DateTo >= vacationGroupFromDate && employment.DateTo <= vacationGroupToDate)
                return new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(0, "Anställd {0} har markerats för slutavräkning. Slutlön måste köras innan semesterårsskifte."), employee.EmployeeNrAndName));

            EmployeeTimePeriod employeeTimePeriod = GetEmployeeTimePeriodWithTimePeriod(employee.EmployeeId, vacationGroupToDate);
            if (employeeTimePeriod?.TimePeriod == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, String.Format(GetText(8751, "Anställd {0} behöver räknas om {1}"), employee.EmployeeNrAndName, vacationGroupToDate.ToShortDateString()));
            if (employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Locked && employeeTimePeriod.Status != (int)SoeEmployeeTimePeriodStatus.Paid && GetTimePayrollTransactions(employee.EmployeeId, employeeTimePeriod.TimePeriodId).Any())
                return new ActionResult((int)ActionResultSave.VacationYearEndFailed, String.Format(GetText(8748, "Semesterårsskifte kan inte köras för att inte perioden {0} är låst för anställd {1}"), employeeTimePeriod.TimePeriod.Name, employee.EmployeeNrAndName));

            return new ActionResult(true);
        }

        private ActionResult ValidateEmployeeVacationSEForVacationYearEnd(Employee employee, EmployeeVacationSE employeeVacationSE, VacationGroupSE vacationGroupSE)
        {
            if (employee == null || employeeVacationSE == null)
                return new ActionResult((int)ActionResultDelete.EntityNotFound, GetText(10083, "Anställd hittades inte"));

            if (employeeVacationSE.PaidVacationAllowance > 0 && employeeVacationSE.PaidVacationAllowance != employeeVacationSE.UsedDaysPaid && vacationGroupSE?.VacationSalaryPayoutRule != (int)TermGroup_VacationGroupVacationSalaryPayoutRule.AllBeforeVacation)
                return new ActionResult((int)ActionResultSave.VacationYearEndFailed, String.Format(GetText(8747, "Måste manuellt justera utbetalda semestertillägg på anställd {0}"), employee.EmployeeNrAndName));
            if (employeeVacationSE.PaidVacationVariableAllowance > 0 && employeeVacationSE.PaidVacationVariableAllowance != employeeVacationSE.UsedDaysPaid && vacationGroupSE?.VacationVariablePayoutRule != (int)TermGroup_VacationGroupVacationSalaryPayoutRule.AllBeforeVacation)
                return new ActionResult((int)ActionResultSave.VacationYearEndFailed, String.Format(GetText(10250, "Måste manuellt justera utbetalda rörliga semestertillägg på anställd {0}"), employee.EmployeeNrAndName));

            return new ActionResult(true);
        }

        private ActionResult PrintEmployeeVacationDebtReport(out Dictionary<DateTime, ReportPrintoutDTO> dateReportMapping, Dictionary<DateTime, List<Employment>> dateEmploymentMapping, bool createReport = true, bool isFinalSalary = false, int? timePeriodId = null)
        {
            dateReportMapping = new Dictionary<DateTime, ReportPrintoutDTO>();
            if (dateEmploymentMapping.IsNullOrEmpty())
                return new ActionResult(true);

            Report report = GetEmployeeVacationDebtReport();
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10097, "Rapport semesterskuld hittades inte"));

            ActionResult result = new ActionResult(true);
            foreach (var pair in dateEmploymentMapping)
            {
                DateTime date = pair.Key;
                List<int> employeeIds = pair.Value?.Select(i => i.EmployeeId).ToList();

                result = PrintEmployeeVacationDebtReport(out ReportPrintoutDTO dto, date, employeeIds: employeeIds, report: report, createReport: createReport, isFinalSalary: isFinalSalary, timePeriodId: timePeriodId);
                if (!result.Success)
                    return result;

                dateReportMapping.Add(date, dto);
            }
            return result;
        }

        private ActionResult PrintEmployeeVacationDebtReport(out ReportPrintoutDTO dto, DateTime date, TermGroup_VacationYearEndHeadContentType contentType, List<int> contentTypeIds, bool isFinalSalary = false, int? timePeriodId = null)
        {
            if (contentType == TermGroup_VacationYearEndHeadContentType.VacationGroup)
                return PrintEmployeeVacationDebtReport(out dto, date, vacationGroupIds: contentTypeIds, isFinalSalary: isFinalSalary, timePeriodId: timePeriodId);
            else
                return PrintEmployeeVacationDebtReport(out dto, date, employeeIds: contentTypeIds, isFinalSalary: isFinalSalary, timePeriodId: timePeriodId);
        }

        private ActionResult PrintEmployeeVacationDebtReport(out ReportPrintoutDTO dto, DateTime date, List<int> employeeIds = null, List<int> vacationGroupIds = null, Report report = null, bool createReport = true, bool isFinalSalary = false, int? timePeriodId = null)
        {
            dto = null;

            if (employeeIds.IsNullOrEmpty() && vacationGroupIds.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.EntityNotFound, $"{GetText(11873, "Rapport semesterskuld kunde inte skapas")}. {GetText(1450, "Felaktigt urval")}");

            if (report == null)
                report = GetEmployeeVacationDebtReport();
            if (report == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(10097, "Rapport semesterskuld hittades inte"));

            TermGroup_ReportExportType exportType = createReport ? TermGroup_ReportExportType.Pdf : TermGroup_ReportExportType.NoExport;
            ReportJobDefinitionDTO reportJobDefinitionDTO = new ReportJobDefinitionDTO(report.ReportId, SoeReportTemplateType.EmployeeVacationDebtReport, exportType);
            reportJobDefinitionDTO.Selections.Add(new DateSelectionDTO { Key = "date", Date = date, });
            reportJobDefinitionDTO.Selections.Add(new BoolSelectionDTO() { Key = "IsFinalSalary", Value = isFinalSalary });
            reportJobDefinitionDTO.Selections.Add(new IdSelectionDTO(timePeriodId ?? 0, "timePeriodId"));

            EmployeeSelectionDTO employeeSelection = new EmployeeSelectionDTO()
            {
                Key = "employees",
                IncludeInactive = false,
                OnlyInactive = false,
                IncludeEnded = false,
            };
            if (!employeeIds.IsNullOrEmpty())
                employeeSelection.EmployeeIds = employeeIds;
            else if (!vacationGroupIds.IsNullOrEmpty())
                reportJobDefinitionDTO.Selections.Add(new IdListSelectionDTO(vacationGroupIds, "IdListSelectionDTO", "vacationGroups"));
            reportJobDefinitionDTO.Selections.Add(employeeSelection);

            dto = ReportDataManager.PrintMigratedReportDTO(reportJobDefinitionDTO, base.ActorCompanyId, base.UserId, base.RoleId, forcePrint: true, skipApiInternal: true);
            if (dto == null)
                return new ActionResult((int)ActionResultSave.EntityNotFound, GetText(11873, "Rapport semesterskuld kunde inte skapas"));

            return new ActionResult(true);
        }

        private void RecalculateVacationTransactionsAsync(List<TimeEngineVacationYearEndEmployee> handledEmployees)
        {
            if (handledEmployees.IsNullOrEmpty())
                return;
            ClearCachedContent();

            using (CompEntities taskEntities = new CompEntities())
            {
                InitContext(taskEntities);

                #region Recalculate vacation

                List<string> unhandledVacationEmployees = new List<string>();

                foreach (var handledEmployee in handledEmployees)
                {
                    var absence = handledEmployee.GetAbsenceRecalculationDates();
                    List<TimePayrollTransaction> timePayrollTransactions = GetAbsenceTimePayrollTransactionsWithTimeBlockDateFromCache(handledEmployee.EmployeeId, absence.From, absence.To, (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation);
                    if (timePayrollTransactions.IsNullOrEmpty())
                        continue;

                    List<AttestEmployeeDaySmallDTO> items = new List<AttestEmployeeDaySmallDTO>();
                    foreach (var timePayrollTransactionsGroupedByDay in timePayrollTransactions.Where(t => t.TimeBlockDate != null).GroupBy(t => t.TimeBlockDateId))
                    {
                        TimeBlockDate timeBlockDate = timePayrollTransactionsGroupedByDay.FirstOrDefault()?.TimeBlockDate;
                        if (timeBlockDate == null)
                            continue;

                        TimeScheduleTemplatePeriod templatePeriod = GetTimeScheduleTemplatePeriodFromCache(handledEmployee.EmployeeId, timeBlockDate.Date);
                        items.Add(new AttestEmployeeDaySmallDTO(timeBlockDate.EmployeeId, timeBlockDate.Date, timeBlockDate.TimeBlockDateId, templatePeriod?.TimeScheduleTemplatePeriodId));
                    }

                    ActionResult result = ReCalculateTransactionsDiscardAttest(items, doNotRecalculateAmounts: true, skipDaysInSameAbsenceRuleRow: true);
                    if (!result.Success)
                        unhandledVacationEmployees.Add(handledEmployee.Employee.EmployeeNrAndName);
                }

                StringBuilder message = new StringBuilder();
                if (unhandledVacationEmployees.Any())
                {
                    message.Append(GetText(11510, "Följande anställda har semester i nästa semesterår som måste ses över"));
                    unhandledVacationEmployees.ForEach(str => message.Append("\r\n" + str));
                }

                //What to do with the message?

                #endregion
            }
        }

        #endregion

        #region VacationYearEndHead

        private VacationYearEndHead GetVacationYearEndWithRows(int vacationYearEndHeadId)
        {
            return (from v in entities.VacationYearEndHead.Include("VacationYearEndRow")
                    where v.VacationYearEndHeadId == vacationYearEndHeadId
                    select v).FirstOrDefault();
        }

        private List<VacationYearEndRow> GetVacationYearEndRows(int employeeId, SoeVacationYearEndType type, int? timePeriodId = null)
        {
            List<VacationYearEndRow> vacationYearEndRows = (from v in entities.VacationYearEndRow
                                                                .Include("VacationYearEndHead")
                                                            where v.EmployeeId == employeeId &&
                                                            v.State == (int)SoeEntityState.Active
                                                            select v).ToList();

            vacationYearEndRows = vacationYearEndRows.Where(i => i.VacationYearEndHead.Type == (int)type).ToList();
            if (timePeriodId.HasValue)
                vacationYearEndRows = vacationYearEndRows.Where(i => i.TimePeriodId == timePeriodId.Value).ToList();

            return vacationYearEndRows;
        }

        private List<VacationYearEndRow> GetVacationYearEndRowsForCompanyAndPeriod(int actorCompanyId, SoeVacationYearEndType type, int timePeriodId)
        {
            List<VacationYearEndRow> vacationYearEndRows = (from v in entities.VacationYearEndRow
                                                                .Include("VacationYearEndHead")
                                                            where v.TimePeriodId == timePeriodId &&
                                                            v.State == (int)SoeEntityState.Active
                                                            select v).ToList();

            vacationYearEndRows = vacationYearEndRows.Where(i => i.VacationYearEndHead.ActorCompanyId == actorCompanyId && i.VacationYearEndHead.Type == (int)type).ToList();

            return vacationYearEndRows;
        }

        private ActionResult ValidationSaveVacationYearEnd(int actorCompanyId, DateTime date, TermGroup_VacationYearEndHeadContentType contentType, List<int> contentTypeIds)
        {
            ActionResult result = new ActionResult(true);

            IQueryable<VacationYearEndHead> query = entities.VacationYearEndHead;
            query = query.Include("VacationGroup");
            if (contentType == TermGroup_VacationYearEndHeadContentType.Employee)
                query = query.Include("VacationYearEndRow");

            List<VacationYearEndHead> vacationYearEndHeads = (from v in query
                                                              where v.ActorCompanyId == actorCompanyId &&
                                                              v.Date == date &&
                                                              v.Type == (int)SoeVacationYearEndType.VacationYearEnd &&
                                                              v.State == (int)SoeEntityState.Active
                                                              select v).ToList();

            List<Employee> employees = new List<Employee>();
            List<string> messages = new List<string>();

            foreach (VacationYearEndHead vacationYearEndHead in vacationYearEndHeads)
            {
                if (contentType == TermGroup_VacationYearEndHeadContentType.VacationGroup && vacationYearEndHead.ContentType == (int)TermGroup_VacationYearEndHeadContentType.VacationGroup)
                    ValidateVacationGroups(vacationYearEndHead, filterVacationGroupIds: contentTypeIds);
                if (contentType == TermGroup_VacationYearEndHeadContentType.VacationGroup && vacationYearEndHead.ContentType == (int)TermGroup_VacationYearEndHeadContentType.Employee)
                    ValidateEmployees(vacationYearEndHead, filterVacationGroupIds: contentTypeIds);
                if (contentType == TermGroup_VacationYearEndHeadContentType.Employee && vacationYearEndHead.ContentType == (int)TermGroup_VacationYearEndHeadContentType.VacationGroup)
                    ValidateVacationGroups(vacationYearEndHead, filterVacationGroupIds: GetVacationGroupIds(contentTypeIds), filterEmployeeIds: contentTypeIds);
                if (contentType == TermGroup_VacationYearEndHeadContentType.Employee && vacationYearEndHead.ContentType == (int)TermGroup_VacationYearEndHeadContentType.Employee)
                    ValidateEmployees(vacationYearEndHead, filterVacationGroupIds: GetVacationGroupIds(contentTypeIds), filterEmployeeIds: contentTypeIds);
            }

            if (!messages.IsNullOrEmpty())
                return new ActionResult((int)ActionResultSave.VacationYearEndAlreadyCreated, string.Format(GetText(0, "Semesterårsskifte för datum {0} redan skapat för {1}"), date.ToShortDateString(), messages.ToCommaSeparated()));

            void ValidateVacationGroups(VacationYearEndHead vacationYearEndHead, List<int> filterVacationGroupIds = null, List<int> filterEmployeeIds = null)
            {
                if (vacationYearEndHead == null)
                    return;

                if (!vacationYearEndHead.VacationGroup.IsLoaded)
                    vacationYearEndHead.VacationGroup.Load();

                foreach (VacationGroup vacationGroup in vacationYearEndHead.VacationGroup)
                {
                    if (filterVacationGroupIds != null && !filterVacationGroupIds.Contains(vacationGroup.VacationGroupId))
                        continue;
                    if (!vacationYearEndHead.ContainsAnyEmployee(filterEmployeeIds))
                        continue;

                    messages.Add(CreateVacationGroupMessage(vacationGroup));
                }
            }

            void ValidateEmployees(VacationYearEndHead vacationYearEndHead, List<int> filterVacationGroupIds = null, List<int> filterEmployeeIds = null)
            {
                if (vacationYearEndHead?.VacationGroups == null)
                    return;
                if (!filterVacationGroupIds.IsNullOrEmpty() && !vacationYearEndHead.VacationGroup.Select(i => i.VacationGroupId).Intersect(filterVacationGroupIds).Any())
                    return;

                if (!vacationYearEndHead.VacationYearEndRow.IsLoaded)
                    vacationYearEndHead.VacationYearEndRow.Load();

                foreach (VacationYearEndRow vacationYearEndRow in vacationYearEndHead.VacationYearEndRow)
                {
                    if (filterEmployeeIds != null && !filterEmployeeIds.Contains(vacationYearEndRow.EmployeeId))
                        continue;
                    if (vacationYearEndRow.Status != (int)TermGroup_VacationYearEndStatus.Succeded)
                        continue;

                    messages.Add(CreateEmployeeMessage(vacationYearEndRow));
                }
            }

            string CreateVacationGroupMessage(VacationGroup vacationGroup)
            {
                return vacationGroup?.Name ?? "?";
            }

            string CreateEmployeeMessage(VacationYearEndRow vacationYearEndRow)
            {
                Employee employee = vacationYearEndRow != null ? GetEmployee(vacationYearEndRow.EmployeeId) : null;
                return employee?.NumberAndName ?? "?";
            }

            List<int> GetVacationGroupIds(List<int> employeeIds)
            {
                List<int> vacationGroupIds = new List<int>();

                List<VacationGroup> vacationGroups = GetVacationGroupsWithSEFromCache();
                foreach (int employeeId in employeeIds)
                {
                    vacationGroupIds.Add(GetEmployee(employeeId)?.GetVacationGroup(date, vacationGroups: vacationGroups)?.VacationGroupId ?? 0);
                }

                return vacationGroupIds.Where(id => id > 0).Distinct().OrderBy(id => id).ToList();
            }

            Employee GetEmployee(int employeeId)
            {
                Employee employee = employees.FirstOrDefault(i => i.EmployeeId == employeeId);
                if (employee == null)
                {
                    employee =
                        contentType == TermGroup_VacationYearEndHeadContentType.Employee ?
                        GetEmployeeWithContactPersonAndEmploymentFromCache(employeeId) :
                        GetEmployeeWithContactPersonFromCache(employeeId);

                    if (employee != null)
                        employees.Add(employee);
                }
                return employee;
            }

            return result;
        }

        private bool HasPeriodLockedForChanges(List<int> employeeIds, DateTime date)
        {
            foreach (int employeeId in employeeIds)
            {
                EmployeeTimePeriod period = GetNextEmployeeTimePeriod(employeeId, date);
                if (period != null && period.Status != (int)SoeEmployeeTimePeriodStatus.Open)
                    return true;
            }

            return false;
        }

        #endregion

        #region VacationYearEndRow

        private VacationYearEndRow GetLatestVacationYearEndRowWithHead(int employeeId, SoeVacationYearEndType type = SoeVacationYearEndType.None)
        {
            return (from v in entities.VacationYearEndRow
                        .Include("VacationYearEndHead")
                    where v.EmployeeId == employeeId &&
                    (type == SoeVacationYearEndType.None || v.VacationYearEndHead.Type == (int)type) &&
                    v.VacationYearEndHead.ActorCompanyId == actorCompanyId &&
                    v.Status == (int)TermGroup_VacationYearEndStatus.Succeded &&
                    v.State == (int)SoeEntityState.Active
                    orderby v.VacationYearEndHead.Date descending
                    select v).FirstOrDefault();
        }

        private bool HasVacationYearEndRowOnDate(int employeeId, DateTime date, SoeVacationYearEndType type)
        {
            date = CalendarUtility.GetBeginningOfDay(date);

            return (from v in entities.VacationYearEndRow
                    where v.EmployeeId == employeeId &&
                    v.VacationYearEndHead.Type == (int)type &&
                    v.VacationYearEndHead.ActorCompanyId == actorCompanyId &&
                    v.VacationYearEndHead.Date == date &&
                    v.State == (int)SoeEntityState.Active
                    select v).Any();
        }

        #endregion
    }
}
