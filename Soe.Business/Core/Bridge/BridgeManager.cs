using Bridge.Shared.Models;
using Bridge.Shared.Models.Visma;
using Bridge.Shared.Util;
using Newtonsoft.Json;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Bridge;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core
{
    public class BridgeManager : ManagerBase
    {

        public BridgeManager(ParameterObject parameterObject) : base(parameterObject)
        {
            BridgeConnector._isTest = SettingManager.isTest();
        }

        public List<EmployeeChangeIODTO> GetEmployeeChangesFromVismaPayroll(int actorCompanyId, List<ScheduledJobSetting> settings)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var vismaPayrollBridgeRecieveRequest = new VismaPayrollBridgeRecieveRequest();
            var allEmployees = EmployeeManager.GetAllEmployees(actorCompanyId, null, true, loadEmployeeAccounts: true);
            vismaPayrollBridgeRecieveRequest.ExistingEmployees = GetExistingEmployees(actorCompanyId, allEmployees);
            vismaPayrollBridgeRecieveRequest.OrganisationNumbers.Add(company.OrgNr);
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            return BridgeConnector.GetHrPlusEmployeeChangeIODTOs(vismaPayrollBridgeRecieveRequest);
        }

        public string SyncVismaEmployments(int actorCompanyId, int batchId, List<ScheduledJobSetting> settings)
        {
            var log = new StringBuilder();
            List<VismaGoEmploymentDTO> vismaEmployments = GetVismaGoEmploymentDTOs(actorCompanyId, batchId, settings);
            log.AppendLine($"=== VISMA → GO SYNCHRONISERING (Visma är master) – {vismaEmployments.Count} anställningar ===");
            vismaEmployments.ForEach(v => { v.FoundExistingEmployment = false; v.Note = null; });

            using (CompEntities entities = new CompEntities())
            {
                entities.CommandTimeout = 300;
                var employees = EmployeeManager.GetAllEmployees(entities, actorCompanyId, null, true, loadEmployeeAccounts: true);
                var allEmployees = new Dictionary<string, Employee>();

                foreach (var employee in employees)
                    if (!allEmployees.ContainsKey(employee.EmployeeNr))
                        allEmployees.Add(employee.EmployeeNr, employee);

                var vismaByEmpNr = vismaEmployments
                    .GroupBy(v => v.EmployeeNr)
                    .ToDictionary(g => g.Key, g => g.ToList());

                int totalProcessed = 0;
                int updatedDatesAndCode = 0;
                int onlyUpdatedCode = 0;
                int onlyUpdatedDates = 0;
                int createdNew = 0;
                int cleanedDuplicates = 0;
                int deletedIdenticalDates = 0;

                // === HJÄLPFUNKTIONS-LAMBDAS ===
                bool SameInterval(Employment e, VismaGoEmploymentDTO v) =>
                    e.DateFrom.HasValue &&
                    e.DateFrom.Value.Date == v.DateFrom.Date &&
                    (e.DateTo?.Date == v.DateTo?.Date);

                bool StartMatches(Employment e, VismaGoEmploymentDTO v) =>
                    e.DateFrom.HasValue && e.DateFrom.Value.Date == v.DateFrom.Date;

                bool EndMatches(Employment e, VismaGoEmploymentDTO v) =>
                    e.DateTo.HasValue && v.DateTo.HasValue && e.DateTo.Value.Date == v.DateTo.Value.Date;

                Employment PickBestByDates(IEnumerable<Employment> candidates, VismaGoEmploymentDTO v)
                {
                    var exact = candidates.FirstOrDefault(e => SameInterval(e, v));
                    if (exact != null) return exact;

                    var start = candidates.FirstOrDefault(e => StartMatches(e, v));
                    if (start != null) return start;

                    var end = candidates.FirstOrDefault(e => EndMatches(e, v));
                    if (end != null) return end;

                    return candidates.OrderByDescending(e => e.DateFrom ?? DateTime.MinValue).FirstOrDefault();
                }

                foreach (var kvp in vismaByEmpNr)
                {
                    string empNr = kvp.Key;
                    var vismaList = kvp.Value;

                    if (!allEmployees.TryGetValue(empNr, out var employee))
                    {
                        log.AppendLine($"[EmployeeNr: {empNr}] Finns INTE i GO → hoppar över");
                        vismaList.ForEach(v => v.Note = "Anställd finns ej i GO");
                        continue;
                    }

                    string header = $"[{empNr}] {employee.FirstName} {employee.LastName}";
                    var goEmployments = employee.Employment.Where(e => e.State != 2).ToList();

                    foreach (var visma in vismaList.OrderBy(v => v.DateFrom))
                    {
                        totalProcessed++;

                        string vFrom = visma.DateFrom.ToString("yyyy-MM-dd");
                        string vTo = visma.DateTo?.ToString("yyyy-MM-dd") ?? "null";
                        string code = string.IsNullOrWhiteSpace(visma.ExternalCode) ? "<ingen kod>" : visma.ExternalCode;

                        log.AppendLine($"{header} → Visma: {code} | {vFrom} – {vTo}");

                        Employment goMatch = null;
                        string matchType = "";

                        // === 1. PRIORITET: EXTERNALCODE (Visma är master) ===
                        if (!string.IsNullOrWhiteSpace(visma.ExternalCode))
                        {
                            var withCode = goEmployments
                                .Where(e => string.Equals(e.GetExternalCode(), visma.ExternalCode, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (withCode.Any())
                            {
                                if (withCode.Count == 1)
                                {
                                    goMatch = withCode[0];
                                    matchType = "ExternalCode";
                                }
                                else // > 1
                                {
                                    goMatch = PickBestByDates(withCode, visma);
                                    matchType = "ExternalCode + djup datum-matchning";

                                    log.AppendLine($"  → Flera GO-anställningar med ExternalCode '{visma.ExternalCode}' ({withCode.Count} st). Väljer EmploymentId {goMatch?.EmploymentId}");

                                    foreach (var dup in withCode.Where(e => e.EmploymentId != goMatch.EmploymentId))
                                    {
                                        if (SameInterval(dup, visma))
                                        {
                                            dup.State = 2;
                                            deletedIdenticalDates++;
                                            log.AppendLine($"    → Identiskt intervall → State=2 (borttagen) på EmploymentId {dup.EmploymentId}");
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrWhiteSpace(dup.GetExternalCode()))
                                            {
                                                dup.OriginalExternalCode = null;
                                                dup.ApplyEmploymentChanges(dup.DateTo);
                                                cleanedDuplicates++;
                                                log.AppendLine($"    → Dubblettkod → tar bort ExternalCode på EmploymentId {dup.EmploymentId}");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // === 2. FALLBACK: EXAKT DATUMINTERVALL (när GO saknar kod) ===
                        if (goMatch == null)
                        {
                            var exactByDate = goEmployments.Where(e => SameInterval(e, visma)).ToList();

                            if (exactByDate.Count == 1)
                            {
                                goMatch = exactByDate[0];
                                matchType = "exakt datumintervall";
                            }
                            else if (exactByDate.Count > 1)
                            {
                                var keep = exactByDate.OrderByDescending(e => e.DateFrom ?? DateTime.MinValue).First();
                                goMatch = keep;
                                matchType = "exakt datumintervall (med rensning)";

                                log.AppendLine($"  → Flera GO med identiskt intervall ({exactByDate.Count}). Behåller EmploymentId {keep.EmploymentId}");

                                foreach (var dup in exactByDate.Where(e => e.EmploymentId != keep.EmploymentId))
                                {
                                    dup.State = 2;
                                    deletedIdenticalDates++;
                                    log.AppendLine($"    → State=2 på EmploymentId {dup.EmploymentId}");
                                }
                            }
                        }

                        // === 3. UPPDATERA MATCHAD ANSTÄLLNING (Visma är master!) ===
                        if (goMatch != null)
                        {
                            bool datesChanged = !goMatch.DateFrom.HasValue ||
                                               goMatch.DateFrom.Value.Date != visma.DateFrom.Date ||
                                               (goMatch.DateTo?.Date != visma.DateTo?.Date);

                            bool codeMissing = string.IsNullOrWhiteSpace(goMatch.GetExternalCode()) &&
                                              !string.IsNullOrWhiteSpace(visma.ExternalCode);

                            bool codeDiffers = !string.IsNullOrWhiteSpace(visma.ExternalCode) &&
                                              !string.IsNullOrWhiteSpace(goMatch.GetExternalCode()) &&
                                              !string.Equals(goMatch.GetExternalCode(), visma.ExternalCode, StringComparison.OrdinalIgnoreCase);

                            if (datesChanged || codeMissing || codeDiffers)
                            {
                                // Logga exakt vad som ändras
                                var changes = new List<string>();
                                if (datesChanged) changes.Add("datum");
                                if (codeMissing || codeDiffers) changes.Add("ExternalCode");

                                log.AppendLine($"  → UPPDATERAR {string.Join(" + ", changes)} → EmploymentId {goMatch.EmploymentId}");

                                goMatch.DateFrom = visma.DateFrom;
                                goMatch.DateTo = visma.DateTo;
                                goMatch.OriginalExternalCode = visma.ExternalCode; // Visma vinner alltid
                                goMatch.ApplyEmploymentChanges(visma.DateTo);

                                if (datesChanged && (codeMissing || codeDiffers)) updatedDatesAndCode++;
                                else if (datesChanged) onlyUpdatedDates++;
                                else onlyUpdatedCode++;

                                employee.ExternalCode += "_u";

                                // check for overlaps with other employments after date change
                                if (goEmployments.Any(e => e.EmploymentId != goMatch.EmploymentId &&
                                                     e.DateFrom < (goMatch.DateTo ?? DateTime.MaxValue) &&
                                                     goMatch.DateFrom < (e.DateTo ?? DateTime.MaxValue)))
                                {
                                    log.AppendLine($"    → VARNING: Överlappande anställning upptäckt efter datumändring!");
                                    // check it there now is two with open end date
                                    var openEndings = goEmployments.Where(e => (e.DateTo == null || goMatch.DateTo == null)).ToList();

                                    //if (openEndings.Count > 1)
                                    //{
                                    //    log.AppendLine($"    → KRITISK VARNING: Två anställningar med öppet slutdatum efter ändring!");

                                    //    // set end date on the older one
                                    //    var toClose = openEndings.OrderBy(e => e.DateFrom ?? DateTime.MinValue).First();
                                    //    var next = openEndings.OrderBy(e => e.DateFrom ?? DateTime.MinValue).Skip(1).First();
                                    //    toClose.DateTo = next.DateFrom?.AddDays(-1);
                                    //    toClose.ApplyEmploymentChanges(toClose.DateTo);
                                    //    SetModifiedProperties(toClose);
                                    //    log.AppendLine($"      → Sätter slutdatum på EmploymentId {toClose.EmploymentId} till {toClose.DateTo:yyyy-MM-dd} pga flera öppna anställningar");
                                    //}
                                }

                                SetModifiedProperties(employee);
                                SetModifiedProperties(goMatch);
                            }
                            else
                            {
                                log.AppendLine($"  → Redan identisk med Visma (match: {matchType})");
                            }

                            visma.FoundExistingEmployment = true;
                            visma.Note = matchType;
                        }
                        // === 4. INGEN MATCH → SKAPA NY (om anställningen börjar snart) ===
                        else if (visma.DateFrom < CalendarUtility.GetBeginningOfMonth(DateTime.Today)) // Lite längre fram
                        {

                            if (goEmployments.Any() && employee.State != 1)
                            {
                                var template = goEmployments.OrderByDescending(e => e.DateFrom ?? DateTime.MinValue).First();

                                // Check if it will overlap any existing

                                if (!goEmployments.Any(a =>
                                                        a.DateFrom < (visma.DateTo ?? DateTime.MaxValue) &&
                                                        visma.DateFrom < (a.DateTo ?? DateTime.MaxValue)))
                                {

                                    var newEmp = new Employment
                                    {
                                        EmployeeId = employee.EmployeeId,
                                        ActorCompanyId = actorCompanyId,
                                        OriginalPayrollGroupId = template.GetPayrollGroupId(),
                                        OriginalEmployeeGroupId = template.GetEmployeeGroupId(),
                                        OriginalType = 0,
                                        OriginalName = template.GetName(),
                                        OriginalWorkTimeWeek = template.GetWorkTimeWeek(),
                                        OriginalPercent = template.GetPercent(),
                                        OriginalBaseWorkTimeWeek = template.GetBaseWorkTimeWeek(),
                                        OriginalFullTimeWorkTimeWeek = template.GetFullTimeWorkTimeWeek() ?? 0,
                                        Created = DateTime.UtcNow,
                                        CreatedBy = "VismaSync" + DateTime.Today.ToString("yyDDMM"),
                                        DateFrom = visma.DateFrom,
                                        DateTo = visma.DateTo,
                                        OriginalExternalCode = visma.ExternalCode
                                    };

                                    employee.ExternalCode += "_a";
                                    SetModifiedProperties(employee);
                                    entities.Employment.AddObject(newEmp);
                                    goEmployments.Add(newEmp);
                                    createdNew++;
                                    visma.FoundExistingEmployment = true;
                                    visma.Note = "skapad ny anställning";

                                    log.AppendLine($"  → SKAPAR NY ANSTÄLLNING | {code} | {vFrom} – {vTo}");
                                }
                                else
                                {
                                    log.AppendLine($"  → Överlappande anställning upptäckt → kan inte skapa ny.Visma {visma.DateFrom}-{visma.DateTo}");
                                }
                            }
                            else
                            {
                                visma.Note = "ingen mall att kopiera";
                                log.AppendLine($"  → Ingen befintlig anställning → kan inte skapa ny (ingen mall)");
                            }
                        }
                        else
                        {
                            visma.Note = "ej matchad (framtida)";
                            log.AppendLine($"  → Ej matchad – framtida anställning (startar {visma.DateFrom:yyyy-MM-dd})");
                        }
                    }

                    if (goEmployments.Any(e => goEmployments.Any(a => a.EmploymentId != e.EmploymentId && e.DateFrom < (a.DateTo ?? DateTime.MaxValue) && a.DateFrom < (e.DateTo ?? DateTime.MaxValue))))
                    {
                        log.AppendLine($"    → VARNING: Överlappande anställning upptäckt efter datumändring!");
                        // check it there now is two with open end date
                        var openEndings = goEmployments.Where(e => e.DateTo == null).ToList();

                        if (openEndings.Count > 1)
                        {
                            log.AppendLine($"    → KRITISK VARNING: flera anställningar med öppet slutdatum efter kontroll!");

                            foreach (var toClose in openEndings.OrderBy(e => e.DateFrom ?? DateTime.MinValue))
                            {
                                var next = openEndings.OrderBy(e => e.DateFrom ?? DateTime.MinValue).Skip(1).FirstOrDefault();
                                if (next != null && next.DateFrom < DateTime.Today)
                                {
                                    toClose.DateTo = next.DateFrom?.AddDays(-1);
                                    toClose.ApplyEmploymentChanges(toClose.DateTo);
                                    SetModifiedProperties(toClose);
                                    log.AppendLine($"      → Sätter slutdatum på EmploymentId {toClose.EmploymentId} till {toClose.DateTo:yyyy-MM-dd} pga flera öppna anställningar");
                                }
                                else
                                    log.AppendLine($"      → Nästa anställning startar inte ännu ({next.DateFrom:yyyy-MM-dd}) – ingen ändring gjord.");
                            }
                        }
                    }
                }

                int saved = entities.SaveChanges();


                log.AppendLine($"Databas: {saved} ändringar sparade.");

                log.AppendLine("=== SAMMANFATTNING ===");
                log.AppendLine($"Bearbetade Visma-anställningar            : {totalProcessed}");
                log.AppendLine($"Uppdaterade datum + ExternalCode          : {updatedDatesAndCode}");
                log.AppendLine($"Uppdaterade endast datum                  : {onlyUpdatedDates}");
                log.AppendLine($"Uppdaterade endast ExternalCode           : {onlyUpdatedCode}");
                log.AppendLine($"Skapade nya anställningar                 : {createdNew}");
                log.AppendLine($"Rensade dubblett-ExternalCode            : {cleanedDuplicates}");
                log.AppendLine($"Markerade som borttagna (State=2)         : {deletedIdenticalDates}");
                log.AppendLine($"=== Synkronisering klar ===");

                var result = log.ToString();
                LogInfo(result);
                return result;
            }
        }

        public List<VismaGoEmploymentDTO> GetVismaGoEmploymentDTOs(int actorCompanyId, int batchId, List<ScheduledJobSetting> settings)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var vismaPayrollBridgeRecieveRequest = new VismaPayrollBridgeRecieveRequest();
            vismaPayrollBridgeRecieveRequest.OrganisationNumbers.Add(company.OrgNr);
            vismaPayrollBridgeRecieveRequest.BridgeConditions.Add(new BridgeCondition() { Field = "BatchId", Value = batchId.ToString() });
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            return BridgeConnector.GetVismaGoEmploymentDTOs(vismaPayrollBridgeRecieveRequest);
        }

        public List<EmployeeChangeIODTO> GetEmployeeChangesFromAgda(int actorCompanyId, List<ScheduledJobSetting> settings)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var adgaPayrollBridgeRecieveRequest = new AgdaPayrollBridgeRecieveRequest();
            var allEmployees = EmployeeManager.GetAllEmployees(actorCompanyId, null, true, loadEmployeeAccounts: true);
            adgaPayrollBridgeRecieveRequest.OrganisationNumbers.Add(StringUtility.Orgnr16XXXXXX_Dash_XXXX(company.OrgNr));
            adgaPayrollBridgeRecieveRequest.ExistingEmployees = GetExistingEmployees(actorCompanyId, allEmployees);
            adgaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            adgaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            adgaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            adgaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData;
            foreach (var setting in settings.Where(w => w.Type == (int)TermGroup_ScheduledJobSettingType.ExportKey))
            {
                var arr = setting.StrData.Split('#');
                adgaPayrollBridgeRecieveRequest.BridgeConditions.Add(new BridgeCondition() { Field = arr[0], Value = arr[1] });
            }
            return BridgeConnector.GetAgdaEmployeeChangeIODTOs(adgaPayrollBridgeRecieveRequest);
        }

        public List<StaffingNeedsFrequencyIODTO> GetICAStoreDataFrequencies(int actorCompanyId, List<ScheduledJobSettingDTO> settings, DateTime startDate, DateTime stopDate)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var ICAStoreDataBridgeRecieveRequest = new ICAStoreDataBridgeRecieveRequest();
            ICAStoreDataBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            ICAStoreDataBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == SettingDataType.String)?.StrData;
            ICAStoreDataBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Secret = settings.FirstOrDefault(s => s.Type == TermGroup_ScheduledJobSettingType.BridgeCredentialSecret && s.DataType == SettingDataType.String)?.StrData;
            ICAStoreDataBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Tenent = settings.FirstOrDefault(s => s.Type == TermGroup_ScheduledJobSettingType.BridgeCredentialTenent && s.DataType == SettingDataType.String)?.StrData;
            ICAStoreDataBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.TokenEndPoint = settings.FirstOrDefault(s => s.Type == TermGroup_ScheduledJobSettingType.BridgeCredentialTokenEndPoint && s.DataType == SettingDataType.String)?.StrData;
            ICAStoreDataBridgeRecieveRequest.FromDate = startDate;
            ICAStoreDataBridgeRecieveRequest.ToDate = stopDate;

            return BridgeConnector.GetICAStoreDataFrequencies(ICAStoreDataBridgeRecieveRequest, actorCompanyId);
        }

        public ActionResult SendVismaHrplusAbsence(List<ScheduledJobSetting> settings, LongTermAbsenceOutput longTermAbsenceOutput, int actorCompanyId)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);

            VismaHrPlusAbsenceBridgeSendRequest sendRequest = new VismaHrPlusAbsenceBridgeSendRequest();
            sendRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.UniqueKey = ConfigurationSetupUtil.GetTestPrefix() + company.CompanyGuid.ToString();
            sendRequest.Base64 = Base64Util.GetBase64StringFromData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(longTermAbsenceOutput)));

            string defaultFileName = "absenceexport_{Company.CompanyNr}_{DateTime.Now(yyyyMMdd)}.txt";
            ScheduledJobSetting fileNameSetting = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupFileName && s.DataType == (int)SettingDataType.String && !string.IsNullOrEmpty(s.StrData));
            sendRequest.Name = BridgeManager.ParseFileName(actorCompanyId, fileNameSetting?.StrData ?? defaultFileName);

            return BridgeConnector.SendVismaHrPlusAbsence(sendRequest);
        }

        public List<TimeBalanceIODTO> GetTimeBalancesIOsFromVismaPayroll(int actorCompanyId, List<ScheduledJobSetting> settings)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var vismaPayrollBridgeRecieveRequest = new VismaPayrollBridgeRecieveRequest();
            var allEmployees = EmployeeManager.GetAllEmployees(actorCompanyId, true, true, loadEmployeeAccounts: true);
            vismaPayrollBridgeRecieveRequest.ExistingEmployees = GetExistingEmployees(actorCompanyId, allEmployees);
            vismaPayrollBridgeRecieveRequest.OrganisationNumbers.Add(company.OrgNr);
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeFileInformations = new List<BridgeFileInformation>() { new BridgeFileInformation() { MatchExpression = "SALDO" } };
            return BridgeConnector.GetVismaTimeBalancesIOs(vismaPayrollBridgeRecieveRequest);
        }

        public List<TimeBalanceIODTO> GetLasBalancesFromVismaPayroll(int actorCompanyId, List<ScheduledJobSetting> settings)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            var vismaPayrollBridgeRecieveRequest = new VismaPayrollBridgeRecieveRequest();
            var allEmployees = EmployeeManager.GetAllEmployees(actorCompanyId, true, true, loadEmployeeAccounts: true);
            vismaPayrollBridgeRecieveRequest.ExistingEmployees = GetExistingEmployees(actorCompanyId, allEmployees);
            vismaPayrollBridgeRecieveRequest.OrganisationNumbers.Add(company.OrgNr);
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeFileInformations = new List<BridgeFileInformation>() { new BridgeFileInformation() { MatchExpression = "LAS" } };
            return BridgeConnector.GetVismaTimeBalancesIOs(vismaPayrollBridgeRecieveRequest);
        }

        public List<VismaPayrollChangesDTO> GetVismaPayrollChanges(int actorCompanyId, DateTime startDate, DateTime stopDate)
        {
            var company = CompanyManager.GetCompany(actorCompanyId);
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var jobHeads = entitiesReadOnly.ScheduledJobHead.Include("ScheduledJobSetting").Where(f => f.ActorCompanyId == actorCompanyId).ToList();

            List<ScheduledJobSetting> settings = new List<ScheduledJobSetting>();
            foreach (var jobHead in jobHeads)
            {
                var typeSetting = jobHead.ScheduledJobSetting.FirstOrDefault(a => a.Type == (int)TermGroup_ScheduledJobSettingType.BridgeJobType && a.DataType == (int)SettingDataType.Integer && a.IntData.HasValue && a.IntData.Value != (int)TermGroup_BridgeJobType.Unknown && a.State == (int)SoeEntityState.Active);
                if (typeSetting == null)
                    continue;

                TermGroup_BridgeJobType type = (TermGroup_BridgeJobType)typeSetting.IntData;

                if (type == TermGroup_BridgeJobType.VismaPayroll)
                {
                    settings = jobHead.ScheduledJobSetting.Where(a => a.State == (int)SoeEntityState.Active).ToList();
                    break;
                }
            }


            var vismaPayrollBridgeRecieveRequest = new VismaPayrollBridgeRecieveRequest();
            vismaPayrollBridgeRecieveRequest.FromDate = startDate;
            vismaPayrollBridgeRecieveRequest.ToDate = stopDate;
            vismaPayrollBridgeRecieveRequest.OrganisationNumbers.Add(company.OrgNr);
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            return BridgeConnector.GetVismaPayrollChangesDTOs(vismaPayrollBridgeRecieveRequest);
        }

        public List<VismaPayrollEmploymentDTO> GetVismaPayrollEmploymentsForEmployeeNr(string employeeNr, List<ScheduledJobSetting> settings)
        {
            VismaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest = new VismaPayrollBridgeRecieveRequest();
            vismaPayrollBridgeRecieveRequest.ExistingEmployees = new List<ExistingEmployee>() { new ExistingEmployee { EmployeeNr = employeeNr } };
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";//https://coop-stage.bluegarden.se/odata/";
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            vismaPayrollBridgeRecieveRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            return BridgeConnector.GetVismaPayrollEmploymentsForEmployeeNr(vismaPayrollBridgeRecieveRequest);
        }

        private List<ExistingEmployee> GetExistingEmployees(int actorCompanyId, List<Employee> employees)
        {
            List<ExistingEmployee> existingEmployees = new List<ExistingEmployee>();
            var existingIOs = GetExistingEmployeeIOs(actorCompanyId, employees);
            var employeeDict = employees
                .GroupBy(g => g.EmployeeId)
                .ToDictionary(k => k.Key.ToString(), v => v.First());

            foreach (var employeeIO in existingIOs)
            {
                Employee employee = null;
                ExistingEmployeeState existingEmployeeState = ExistingEmployeeState.Active;

                if (employeeDict.ContainsKey(employeeIO.EmployeeId))
                {
                    employee = employeeDict[employeeIO.EmployeeId];
                    if (employee.State != (int)SoeEntityState.Active)
                    {
                        existingEmployeeState =
                            employee.State == (int)SoeEntityState.Inactive ? ExistingEmployeeState.InActive :
                            employee.State == (int)SoeEntityState.Deleted ? ExistingEmployeeState.Deleted :
                            ExistingEmployeeState.Other;
                    }
                }

                var existingEmployee = new ExistingEmployee
                {
                    EmployeeNr = employeeIO.EmployeeNr,
                    EmployeeId = employeeIO.EmployeeId.ToString(),
                    ExternalCode = employeeIO.ExternalCode,
                    LastUpdated = employeeIO.LastUpdated,
                    State = existingEmployeeState,
                    ParentEmployeeNr = employeeIO.ParentEmployeeNr,
                };

                foreach (var employment in employeeIO.ExistingEmployments)
                {

                    var existingEmployment = new ExistingEmployment()
                    {
                        StartDate = employment.StartDate,
                        StopDate = employment.StopDate,
                        EmployeeGroupExternalCodes = employment.EmployeeGroupExternalCodes,
                        PayrollGroupExternalCodes = employment.PayrollGroupExternalCodes,
                        EmploymentTypeExternalCodes = employment.EmploymentTypeExternalCodes,
                        ExternalCode = employment.ExternalCode,
                        WorkTimeWeeks = new List<ExistingEmploymentWorkTimeWeek>()
                    };

                    if (!employment.StopDate.HasValue || employment.StartDate > DateTime.Today.AddYears(-1))
                    {
                        var matchingEmployment = employee.GetEmployment(employment.StartDate);

                        if (matchingEmployment != null)
                        {
                            var workTimeWeekChanges = matchingEmployment.GetDataChanges(TermGroup_EmploymentChangeFieldType.WorkTimeWeek);
                            var employmentRateChanges = matchingEmployment.GetDataChanges(TermGroup_EmploymentChangeFieldType.Percent);
                            var workweekTimes = new List<ExistingEmploymentWorkTimeWeek>();

                            foreach (var change in workTimeWeekChanges.Where(w => w.EmploymentChangeBatch?.FromDate != null).OrderByDescending(o => o.EmploymentChangeBatch.Created))
                            {
                                if (workweekTimes.Any(a => a.Start == change.EmploymentChangeBatch.FromDate))
                                    continue;

                                var matchingRateChange = employmentRateChanges.FirstOrDefault(f => f.EmploymentChangeBatch.FromDate == change.EmploymentChangeBatch.FromDate);
                                int.TryParse(change.ToValue, out int result);
                                var rate = NumberUtility.GetNullableDecimalFromString(matchingRateChange?.ToValue);

                                workweekTimes.Add(new ExistingEmploymentWorkTimeWeek()
                                {
                                    Start = change.EmploymentChangeBatch.FromDate.Value,
                                    Minutes = result,
                                    EmploymentRate = rate,
                                });
                            }
                            existingEmployment.WorkTimeWeeks = workweekTimes;
                        }
                    }

                    existingEmployee.ExistingEmployments.Add(existingEmployment);
                }

                foreach (var employment in employeeIO.ExistingEmployeeAccounts)
                {
                    existingEmployee.ExistingEmployeeAccounts.Add(new ExistingEmployeeAccount()
                    {
                        StartDate = employment.StartDate,
                        StopDate = employment.StopDate,
                        AccountExternalCodes = employment.AccountExternalCodes,
                        AccountNr = employment.AccountNr,
                        IsDefault = employment.IsDefault,
                        ParentAccountNr = employment.ParentAccountNr,
                    });
                }

                foreach (var employeeEmployer in employeeIO.ExistingEmployeeEmployers)
                {
                    existingEmployee.ExistingEmployeeEmployers.Add(new ExistingEmployeeEmployer()
                    {
                        StartDate = employeeEmployer.StartDate,
                        StopDate = employeeEmployer.StopDate,
                        EmployerRegistrationNumber = employeeEmployer.EmployerRegistrationNumber,
                    });
                }

                existingEmployees.Add(existingEmployee);
            }

            return existingEmployees;
        }

        public List<ExistingEmployeeIO> GetExistingEmployeeIOs(int actorCompanyId, List<Employee> employees)
        {
            List<ExistingEmployeeIO> existingEmployees = new List<ExistingEmployeeIO>();

            using var entities = CompEntitiesProvider.LeaseReadOnlyContext();
            var employeeGroups = base.GetEmployeeGroupsFromCache(entities, CacheConfig.Company(actorCompanyId), true);
            var payrollGroups = base.GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId), true);
            var employmentTypes = base.GetEmploymentTypesFromCache(entities, CacheConfig.Company(actorCompanyId), TermGroup_Languages.Swedish);
            var accounts = base.GetAccountInternalsFromCache(entities, CacheConfig.Company(actorCompanyId));
            var employers = base.GetEmployersFromCache(entities, CacheConfig.Company(actorCompanyId));
            var employeeEmployers = base.GetEmployeeEmployersForCompanyFromCache(entities, CacheConfig.Company(actorCompanyId));

            foreach (var employee in employees)
            {
                var parentEmployeeNr = employee.ParentId.HasValue ? employees.FirstOrDefault(f => f.EmployeeId == employee.ParentId.Value)?.EmployeeNr : null;

                var existingEmployee = new ExistingEmployeeIO()
                {
                    EmployeeNr = employee.EmployeeNr,
                    EmployeeId = employee.EmployeeId.ToString(),
                    ExternalCode = employee.ExternalCode,
                    LastUpdated = employee.Modified ?? employee.Created ?? DateTime.Now.AddYears(-1),
                    ParentEmployeeNr = parentEmployeeNr
                };

                foreach (var employment in employee.GetActiveEmployments())
                {
                    var date = employment.GetEmploymentDate() ?? employment.GetEndDate() ?? CalendarUtility.DATETIME_DEFAULT;
                    var employeeGroup = employment.GetEmployeeGroup(date, employeeGroups);
                    var payrollGroup = employment.GetPayrollGroup(date, payrollGroups);
                    var employmentType = employment.GetEmploymentTypeDTO(employmentTypes, date);

                    List<string> employmentTypeCodes = new List<string>
                    {
                        employmentType.EmploymentTypeId.ToString(),
                        employmentType.Name.ToString()
                    };
                    if (employmentType.Name.Length > 3)
                        employmentTypeCodes.Add(employmentType.Name.Substring(0, 4));
                    if (!string.IsNullOrEmpty(employmentType.Code))
                        employmentTypeCodes.Add(employmentType.Code);

                    existingEmployee.ExistingEmployments.Add(new ExistingEmploymentIO()
                    {
                        StartDate = employment.GetEmploymentDate() ?? CalendarUtility.DATETIME_DEFAULT,
                        StopDate = employment.GetEndDate(),
                        EmployeeGroupExternalCodes = employeeGroup?.ExternalCodes ?? new List<string>(),
                        PayrollGroupExternalCodes = payrollGroup?.ExternalCodes ?? new List<string>(),
                        EmploymentTypeExternalCodes = employmentTypeCodes,
                        ExternalCode = employment.GetExternalCode(date)
                    });
                }

                foreach (var employeeAccount in employee.EmployeeAccount?.ToList() ?? new List<EmployeeAccount>())
                {
                    if (employeeAccount?.AccountId == null || employeeAccount.State != (int)SoeEntityState.Active)
                        continue;

                    var account = accounts.FirstOrDefault(f => f.AccountId == employeeAccount.AccountId);
                    if (account == null)
                        continue;

                    var parentAccount = employeeAccount.ParentEmployeeAccountId.HasValue ? accounts.FirstOrDefault(f => f.AccountId == employeeAccount.ParentEmployeeAccountId) : null;

                    existingEmployee.ExistingEmployeeAccounts.Add(new ExistingEmployeeAccountIO()
                    {
                        StartDate = employeeAccount.DateFrom,
                        StopDate = employeeAccount.DateTo,
                        AccountExternalCodes = account?.ExternalCode != null ? new List<string>() { account.ExternalCode } : new List<string>(),
                        AccountNr = account?.AccountNr ?? string.Empty,
                        ParentAccountNr = parentAccount?.AccountNr ?? string.Empty,
                        IsDefault = employeeAccount.Default
                    });
                }

                foreach (var employeeEmployer in employeeEmployers.Where(w => w.EmployeeId == employee.EmployeeId) ?? new List<EmployeeEmployer>())
                {
                    var employer = employers.FirstOrDefault(f => f.EmployerId == employeeEmployer.EmployerId);
                    if (employer == null)
                        continue;
                    existingEmployee.ExistingEmployeeEmployers.Add(new ExistingEmployeeEmployerIO()
                    {
                        StartDate = employeeEmployer.DateFrom,
                        StopDate = employeeEmployer.DateTo,
                        EmployerRegistrationNumber = employer.OrgNr
                    });
                }

                existingEmployees.Add(existingEmployee);
            }
            return existingEmployees;
        }

        public ActionResult FTPUpload(List<ScheduledJobSetting> settings, FileExportResult fileExportResult)
        {
            FTPBridgeSendRequest sendRequest = new FTPBridgeSendRequest();
            sendRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.Base64 = fileExportResult.Base64Data;
            sendRequest.Name = fileExportResult.FileName;

            return BridgeConnector.FTPUpload(sendRequest);
        }

        public ActionResult FTPDelete(List<ScheduledJobSetting> settings, BridgeFileInformation bridgeFileInformation)
        {
            FTPBridgeDeleteRequest deleteRequest = new FTPBridgeDeleteRequest();
            deleteRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            deleteRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            deleteRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            deleteRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;

            deleteRequest.BridgeFileInformations.Add(bridgeFileInformation);

            var response = BridgeConnector.FTPDeleteFiles(deleteRequest);
            return new ActionResult() { Success = response.Success, ErrorMessage = response.ErrorMessage };
        }

        public ActionResult FTPMove(List<ScheduledJobSetting> settings, FileExportResult fileExportResult, BridgeFileInformation bridgeFileInformation)
        {
            FTPBridgeMoveRequest moveRequest = new FTPBridgeMoveRequest();
            FTPBridgeDeleteRequest DeleteRequest = new FTPBridgeDeleteRequest();
            DeleteRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            DeleteRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            DeleteRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            DeleteRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            DeleteRequest.BridgeFileInformations.Add(bridgeFileInformation);
            moveRequest.FTPBridgeDeleteRequest = DeleteRequest;

            FTPBridgeSendRequest sendRequest = new FTPBridgeSendRequest();
            sendRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.Base64 = fileExportResult.Base64Data;
            sendRequest.Name = fileExportResult.FileName;
            moveRequest.FTPBridgeSendRequest = sendRequest;
            var response = BridgeConnector.FTPMoveFile(moveRequest);
            return new ActionResult() { Success = response.Success, ErrorMessage = response.ErrorMessage };
        }


        public List<BridgeFileInformation> FTPGetFiles(List<ScheduledJobSetting> settings)
        {
            FTPBridgeRecieveRequest request = new FTPBridgeRecieveRequest();
            request.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            request.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            request.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            request.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData;

            var matchExpressionSetting = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeFileInformationMatchExpression && s.DataType == (int)SettingDataType.String);

            if (matchExpressionSetting != null)
                request.BridgeFileInformations.Add(new BridgeFileInformation() { MatchExpression = matchExpressionSetting.StrData });

            return BridgeConnector.FTPGetFiles(request)?.BridgeFileInformations ?? new List<BridgeFileInformation>();
        }

        public ActionResult SSHUpload(List<ScheduledJobSetting> settings, FileExportResult fileExportResult)
        {
            SSHBridgeSendRequest sendRequest = new SSHBridgeSendRequest();
            sendRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.Base64 = fileExportResult.Base64Data;
            sendRequest.Name = fileExportResult.FileName;

            return BridgeConnector.SSHUpload(sendRequest);
        }

        public ActionResult SSHMove(List<ScheduledJobSetting> settings, FileExportResult fileExportResult, BridgeFileInformation bridgeFileInformation)
        {
            SSHBridgeMoveRequest moveRequest = new SSHBridgeMoveRequest();
            SSHBridgeDeleteRequest DeleteRequest = new SSHBridgeDeleteRequest();
            DeleteRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            DeleteRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            DeleteRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            DeleteRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            DeleteRequest.BridgeFileInformations.Add(bridgeFileInformation);
            moveRequest.SSHBridgeDeleteRequest = DeleteRequest;

            SSHBridgeSendRequest sendRequest = new SSHBridgeSendRequest();
            sendRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPathTransfer && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeSetup.ImportKey = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupImportKey && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.BridgeConfiguration.BridgeSetup.ImportSettings = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupImportSettings && s.DataType == (int)SettingDataType.String)?.StrData;
            sendRequest.Base64 = fileExportResult.Base64Data;
            sendRequest.Name = Path.GetFileName(fileExportResult.FileName);
            moveRequest.SSHBridgeSendRequest = sendRequest;
            var response = BridgeConnector.SSHMoveFile(moveRequest);
            return new ActionResult() { Success = response.Success, ErrorMessage = response.ErrorMessage };
        }

        public ActionResult SSHDelete(List<ScheduledJobSetting> settings, BridgeFileInformation bridgeFileInformation)
        {
            SSHBridgeDeleteRequest DeleteRequest = new SSHBridgeDeleteRequest();
            DeleteRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            DeleteRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            DeleteRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            DeleteRequest.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            DeleteRequest.BridgeFileInformations.Add(bridgeFileInformation);

            var response = BridgeConnector.SSHDeleteFiles(DeleteRequest);
            return new ActionResult() { Success = response.Success, ErrorMessage = response.ErrorMessage };
        }

        public List<BridgeFileInformation> SSHGetFiles(List<ScheduledJobSetting> settings)
        {
            SSHBridgeRecieveRequest request = new SSHBridgeRecieveRequest();
            request.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            request.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData;
            request.BridgeConfiguration.BridgeCredentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            request.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData;
            request.BridgeConfiguration.BridgeSetup.ImportKey = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupImportKey && s.DataType == (int)SettingDataType.String)?.StrData;
            request.BridgeConfiguration.BridgeSetup.ImportSettings = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupImportSettings && s.DataType == (int)SettingDataType.String)?.StrData;

            var matchExpressionSetting = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeFileInformationMatchExpression && s.DataType == (int)SettingDataType.String);

            if (matchExpressionSetting != null)
                request.BridgeFileInformations.Add(new BridgeFileInformation() { MatchExpression = matchExpressionSetting.StrData });

            return BridgeConnector.SSHGetFiles(request)?.BridgeFileInformations ?? new List<BridgeFileInformation>();
        }

        public ActionResult AzureStorageUpload(List<ScheduledJobSetting> settings, FileExportResult fileExportResult)
        {
            AzureStorageBridgeSendRequest sendRequest = new AzureStorageBridgeSendRequest();
            sendRequest.BridgeConfiguration.BridgeSetup.Address = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupAddress && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Container = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupContainer && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeSetup.Path = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeSetupPath && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.User = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialUser && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.Secret = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialSecret && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.TokenEndPoint = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialTokenEndPoint && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.GrantType = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialGrantType && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.Tenent = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialTenent && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.Token = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialToken && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.BridgeConfiguration.BridgeCredentials.ConnectionString = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialConnectionString && s.DataType == (int)SettingDataType.String)?.StrData ?? "";
            sendRequest.Base64 = fileExportResult.Base64Data;
            sendRequest.Name = fileExportResult.FileName;
            return BridgeConnector.AzureStorageUpload(sendRequest);
        }

        public bool EncryptionUpdate(CompEntities entities, List<ScheduledJobSetting> settings)
        {
            var updated = false;
            BridgeCredentials credentials = new BridgeCredentials();
            credentials.Secret = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialSecret && s.DataType == (int)SettingDataType.String)?.StrData;
            credentials.Password = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.StrData;
            credentials.ConnectionString = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialConnectionString && s.DataType == (int)SettingDataType.String)?.StrData;

            if (!credentials.IsEncrypted)
            {
                var password = credentials.Password;
                var connectionString = credentials.ConnectionString;
                var secret = credentials.Secret;

                var updateCredentials = BridgeConnector.Encrypt(credentials);

                if (password != updateCredentials.Password)
                {
                    var existingSettingId = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String)?.ScheduledJobSettingId ?? 0;
                    var setting = entities.ScheduledJobSetting.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialPassword && s.DataType == (int)SettingDataType.String && s.ScheduledJobSettingId == existingSettingId);

                    if (setting != null && setting.StrData != updateCredentials.Password)
                    {
                        setting.StrData = updateCredentials.Password;
                        SaveChanges(entities);
                        updated = true;
                    }
                }
                if (secret != updateCredentials.Secret)
                {
                    var existingSettingId = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialSecret && s.DataType == (int)SettingDataType.String)?.ScheduledJobSettingId ?? 0;
                    var setting = entities.ScheduledJobSetting.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialSecret && s.DataType == (int)SettingDataType.String && s.ScheduledJobSettingId == existingSettingId);

                    if (setting != null && setting.StrData != updateCredentials.Secret)
                    {
                        setting.StrData = updateCredentials.Secret;
                        SaveChanges(entities);
                        updated = true;
                    }
                }
                if (connectionString != updateCredentials.ConnectionString)
                {
                    var existingSettingId = settings.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialConnectionString && s.DataType == (int)SettingDataType.String)?.ScheduledJobSettingId ?? 0;
                    var setting = entities.ScheduledJobSetting.FirstOrDefault(s => s.Type == (int)TermGroup_ScheduledJobSettingType.BridgeCredentialConnectionString && s.DataType == (int)SettingDataType.String && s.ScheduledJobSettingId == existingSettingId);

                    if (setting != null && setting.StrData != updateCredentials.ConnectionString)
                    {
                        setting.StrData = updateCredentials.ConnectionString;
                        SaveChanges(entities);
                        updated = true;
                    }
                }
            }

            return updated;
        }
        public string ParseFileName(int actorCompanyId, string fileName)
        {
            var parsedName = fileName;

            if (!string.IsNullOrEmpty(fileName))
            {
                if (fileName.Contains("{Company."))
                {
                    var company = CompanyManager.GetCompany(actorCompanyId);

                    if (fileName.Contains("{Company.Name}"))
                        parsedName = parsedName.Replace("{Company.Name}", company.Name);

                    if (fileName.Contains("{Company.CompanyNr}"))
                        parsedName = parsedName.Replace("{Company.CompanyNr}", company.CompanyNr.HasValue ? company.CompanyNr.Value.ToString() : "");
                }

                if (parsedName.Contains("{DateTime.Now("))
                {
                    var arrWItheEnds = parsedName.Split('}');

                    foreach (var part in arrWItheEnds.Where(w => w.Contains("{DateTime.Now(")))
                    {
                        var varCurly = part.Split('{');

                        foreach (var replaceThisPart in varCurly.Where(w => w.Contains("DateTime.Now(")))
                        {
                            var arrOnParentes = replaceThisPart.Split('(');

                            if (arrOnParentes.Any())
                            {
                                var last = arrOnParentes.Last();
                                var formatValue = last.Replace(")", "").Trim();

                                if (!string.IsNullOrEmpty(formatValue))
                                {
                                    formatValue = DateTime.Now.ToString(formatValue);
                                    parsedName = parsedName.Replace("{" + replaceThisPart + "}", formatValue);
                                }
                            }
                        }
                    }
                }
            }
            return parsedName;
        }
    }
}
