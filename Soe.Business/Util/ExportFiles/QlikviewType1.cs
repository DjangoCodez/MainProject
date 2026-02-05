using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeTree;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class QlikviewType1 : ExportFilesBase
    {
        #region Ctor

        public QlikviewType1(ParameterObject parameterObject, CreateReportResult ReportResult) : base(parameterObject, ReportResult) { }

        #endregion

        #region Public methods

        public string CreateFile(CompEntities entities)
        {
            #region Init

            if (ReportResult == null)
                return null;

            TimeTreeAttestManager am = new TimeTreeAttestManager(parameterObject);
            CompanyManager cm = new CompanyManager(parameterObject);
            EmployeeManager em = new EmployeeManager(parameterObject);

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, ReportResult);

            #endregion

            #region Prereq

            var company = cm.GetCompany(ReportResult.ActorCompanyId);

            bool selectionIncludeInactive;
            bool selectionOnlyInactive;
            bool? selectionActiveEmployees;
            TryGetIncludeInactiveFromSelection(ReportResult, out selectionIncludeInactive, out selectionOnlyInactive, out selectionActiveEmployees);

            DateTime selectionDateFrom;
            DateTime selectionDateTo;
            if (!TryGetDatesFromSelection(ReportResult, out selectionDateFrom, out selectionDateTo))
                return null;

            bool showAllEmployees;
            TryGetBoolFromSelection(ReportResult, out showAllEmployees, "showAllEmployees");

            List<Employee> employees;
            List<int> selectionEmployeeIds;
            if (!showAllEmployees)
            {
                if (!TryGetEmployeeIdsFromSelection(ReportResult, selectionDateFrom, selectionDateTo, out employees, out selectionEmployeeIds))
                    return null;

                employees = em.GetAllEmployeesByIds(entities, ReportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);
            }
            else
            {
                employees = em.GetAllEmployees(entities, ReportResult.ActorCompanyId, loadEmployment: true, getHidden: false);
            }

            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, ReportResult.RoleId, ReportResult.ActorCompanyId);

            var sb = new StringBuilder();
            sb.Append("Id;Butik;Kostnadsställe;Avdelning;Anställningsnummer;Personnummer;Namn;Datum;Tid in;Tid ut;Total tid;Schematid;Löneartikel;Löneartsbenämning;Frånvarokod;OB - typ;ÖT - typ;Lönekostnad per timme(inkl.OB och ÖT);Betald");
            sb.Append(Environment.NewLine);

            #endregion

            foreach (Employee employee in employees)
            {
                #region Employee

                //Hej Kent

                //Jag tror att vi snarast måste kolla den data vi levererar via ftp.Jag kom att tänka på att det nu även skickas data för tjänstemän, denna får absolut inte komma med.
                //Jenni och jag kommer att lägga på detta senast på måndag så någon typ av begräsning måste göras före dess.
                //Lösningen i det korta är att undanta dessa löneavtal på Mathem & Kulls
                int? payrollGroupId = employee.GetPayrollGroupId();
                if (payrollGroupId.HasValue && (payrollGroupId == 218 || payrollGroupId == 247 || payrollGroupId == 220))
                    continue;

                var input = GetAttestEmployeeInput.CreateAttestInputForWeb(ReportResult.ActorCompanyId, ReportResult.UserId, ReportResult.RoleId, employee.EmployeeId, selectionDateFrom, selectionDateTo, null, null, InputLoadType.GrossNetCost);
                input.SetOptionalParameters(doGetOnlyActive: !selectionOnlyInactive);

                var items = am.GetAttestEmployeeDays(input);
                if (items.IsNullOrEmpty())
                    continue;

                var schedules = new List<KeyValuePair<DateTime, int>>();
                foreach (var item in items)
                {
                    int scheduleTime = Convert.ToInt32(item.ScheduleTime.TotalMinutes) - (item.TimeScheduleTypeFactorMinutes);
                    schedules.Add(new KeyValuePair<DateTime, int>(item.Date, scheduleTime));
                }

                string personnummer = showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec);
                string delimiter = ";";

                foreach (var item in items)
                {
                    string accountDim1Nr = "";
                    string accountDim2Nr = "";
                    string accountDim3Nr = "";
                    int dimOrder = 1;

                    foreach (var trans in item.AttestPayrollTransactions)
                    {
                        #region Set values

                        dimOrder = 1;
                        foreach (var accountDTO in trans.AccountInternals.OrderBy(a => a.AccountDimNr))
                        {
                            if (dimOrder == 1)
                                accountDim1Nr = accountDTO.AccountNr;
                            if (dimOrder == 2)
                                accountDim2Nr = accountDTO.AccountNr;
                            if (dimOrder == 3)
                                accountDim3Nr = accountDTO.AccountNr;
                            dimOrder++;
                        }

                        DateTime startTime = trans.StartTime.HasValue ? trans.StartTime.Value : CalendarUtility.DATETIME_DEFAULT;
                        DateTime timeStart = CalendarUtility.GetActualDateTime(trans.Date, startTime);
                        string timeStartString = timeStart == CalendarUtility.DATETIME_DEFAULT ? string.Empty : CalendarUtility.ToShortDateTimeString(timeStart);

                        DateTime stopTime = trans.StopTime.HasValue ? trans.StopTime.Value : CalendarUtility.DATETIME_DEFAULT;
                        DateTime timeStop = CalendarUtility.GetActualDateTime(trans.Date, stopTime);
                        string timeStopString = timeStop == CalendarUtility.DATETIME_DEFAULT ? string.Empty : CalendarUtility.ToShortDateTimeString(timeStop);
                        string totaltidHours = (trans.Quantity / 60m).ToString("F");
                        string scheduledhours = schedules.Any(k => k.Key == trans.Date) && schedules.FirstOrDefault(k => k.Key == trans.Date).Value > 0 ? (schedules.FirstOrDefault(k => k.Key == trans.Date).Value / 60).ToString() : string.Empty;

                        string id = trans.TimeBlockDateId.ToString();
                        string butik = accountDim1Nr;
                        string kostnadsstalle = accountDim2Nr;
                        string avdelning = accountDim3Nr;
                        string anstallningsnummer = employee.EmployeeNr;
                        string namn = employee.Name;
                        string datum = trans.Date.ToShortDateString();
                        string tidin = timeStartString;
                        string tidut = timeStopString;
                        string schematid = scheduledhours;
                        string totaltid = totaltidHours;
                        string loneartikel = trans.PayrollProductNumber;
                        string loneartsbenamning = trans.PayrollProductName;
                        string franvarokod = trans.TransactionSysPayrollTypeLevel3.HasValue ? GetText(trans.TransactionSysPayrollTypeLevel3.Value, (int)TermGroup.SysPayrollType) : String.Empty;
                        string oBtyp = trans.TransactionSysPayrollTypeLevel2.HasValue && (TermGroup_SysPayrollType)trans.TransactionSysPayrollTypeLevel2.Value == TermGroup_SysPayrollType.SE_GrossSalary_OBAddition ? trans.PayrollProductFactor.ToString() : string.Empty;
                        string oTtyp = trans.TransactionSysPayrollTypeLevel2.HasValue && ((TermGroup_SysPayrollType)trans.TransactionSysPayrollTypeLevel2.Value == TermGroup_SysPayrollType.SE_GrossSalary_OvertimeAddition || (TermGroup_SysPayrollType)trans.TransactionSysPayrollTypeLevel2.Value == TermGroup_SysPayrollType.SE_GrossSalary_OvertimeCompensation) ? trans.PayrollProductFactor.ToString() : string.Empty;
                        string lonekostnadpertimme = trans.Amount.HasValue && trans.Amount.Value > 0 && trans.Quantity != 0 ? decimal.Round((((decimal)trans.Amount / (trans.Quantity)) * 60), 2).ToString() : "0";
                        string isPayed = trans.PayrollProductPayed.ToInt().ToString();

                        #endregion

                        #region Append

                        var transaction = new StringBuilder();
                        transaction.Append(id);
                        transaction.Append(delimiter);
                        transaction.Append(butik);
                        transaction.Append(delimiter);
                        transaction.Append(kostnadsstalle);
                        transaction.Append(delimiter);
                        transaction.Append(avdelning);
                        transaction.Append(delimiter);
                        transaction.Append(anstallningsnummer);
                        transaction.Append(delimiter);
                        transaction.Append(personnummer);
                        transaction.Append(delimiter);
                        transaction.Append(namn);
                        transaction.Append(delimiter);
                        transaction.Append(datum);
                        transaction.Append(delimiter);
                        transaction.Append(tidin);
                        transaction.Append(delimiter);
                        transaction.Append(tidut);
                        transaction.Append(delimiter);
                        transaction.Append(totaltid);
                        transaction.Append(delimiter);
                        transaction.Append(schematid);
                        transaction.Append(delimiter);
                        transaction.Append(loneartikel);
                        transaction.Append(delimiter);
                        transaction.Append(loneartsbenamning);
                        transaction.Append(delimiter);
                        transaction.Append(franvarokod);
                        transaction.Append(delimiter);
                        transaction.Append(oBtyp);
                        transaction.Append(delimiter);
                        transaction.Append(oTtyp);
                        transaction.Append(delimiter);
                        transaction.Append(lonekostnadpertimme);
                        transaction.Append(delimiter);
                        transaction.Append(isPayed);
                        transaction.Append(delimiter);
                        transaction.Append(Environment.NewLine);

                        sb.Append(transaction.ToString());

                        #endregion
                    }

                    personalDataRepository.AddEmployeeSocialSec(employee);
                }

                #endregion
            }

            #region Create File

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = "Qlikview" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss");
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                SysLogManager slm = new SysLogManager(parameterObject);
                slm.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        #endregion
    }
}



