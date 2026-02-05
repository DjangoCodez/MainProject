using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.ExportFiles.Common;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class Bygglosen : ExportFilesBase
    {
        #region Ctor

        public Bygglosen(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        public string CreateBygglosenFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetBoolFromSelection(reportResult, out bool setAsFinal, "setAsFinal");
            TryGetBoolFromSelection(reportResult, out bool removePrevSubmittedData, "removePrevSubmittedData");

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var bygglosen = GetBygglosenDTO(entities, reportResult.ActorCompanyId, reportResult.UserId, reportResult.RoleId, selectionTimePeriodIds, employees, setAsFinal);
            string lastFileIdentifier = string.Empty;

            #endregion

            #region Create File

            if (removePrevSubmittedData)
            {
                var lastEventHistory = GeneralManager.GetLastEventHistory(entities, TermGroup_EventHistoryType.ByggLosenNyckel, Company.ActorCompanyId, Company.ActorCompanyId);

                if (lastEventHistory?.StrData != null)
                {
                    lastFileIdentifier = lastEventHistory.StrData;
                }
            }

            var document = bygglosen.GetDocument(lastFileIdentifier);

            string validationErrors = ValidateXDocument(document);

            var directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string fileName = IOUtil.FileNameSafe("Bygglosen" + "_" + company.Name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss"));
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".xml";

            try
            {
                if (string.IsNullOrEmpty(validationErrors))
                {
                    File.WriteAllText(filePath, document.ToString(), Encoding.UTF8);

                    if (setAsFinal && !bygglosen.EventHistories.IsNullOrEmpty())
                        GeneralManager.SaveEventHistories(entities, bygglosen.EventHistories, Company.ActorCompanyId);
                }
                else
                {
                    validationErrors = "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + validationErrors + Environment.NewLine + Environment.NewLine + "####################  VALIDATION ERROR IN FILE ###########################" + Environment.NewLine + Environment.NewLine + document.ToString();
                    File.WriteAllText(filePath, validationErrors + document.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        private string ValidateXDocument(XDocument document)
        {
            bool errors = false;
            string message = "";
            string path = ConfigSettings.SOE_SERVER_DIR_REPORT_EXTERNAL_BYGGLOSEN_PHYSICAL + "bygglosen_2_2.xsd";
            string file = File.ReadAllText(path);

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(file)));
            document.Validate(schemas, (o, e) =>
            {
                message = e.Message;
                errors = true;
            });

            if (errors)
            {
                return message;
            }
            return "";
        }

        public BygglosenDTO GetBygglosenDTO(CompEntities entities, int actorCompanyId, int userId, int roleId, List<int> selectionTimePeriodIds, List<Employee> employees, bool setAsFinal)
        {
            BygglosenDTO dto = new BygglosenDTO();

            List<TimePeriod> timePeriods = TimePeriodManager.GetTimePeriods(selectionTimePeriodIds, actorCompanyId).Where(x => x.PayrollStopDate.HasValue).ToList();
            if (!timePeriods.Any())
                return dto;

            var kommunkoder = SettingManager.GetSysParameters(1);
            var befattningar = EmployeeManager.GetEmployeePositionsForCompany(entities, actorCompanyId, loadSysPosition: true);
            var payrollGroups = GetPayrollGroupsFromCache(entities, CacheConfig.Company(actorCompanyId));
            var arbetsplatsnr = string.Empty;

            foreach (var timePeriod in timePeriods)
            {

                DateTime dateFrom = timePeriod.StartDate;
                DateTime dateTo = timePeriod.StopDate;

                var allTransactions = TimeTransactionManager.GetTimePayrollStatisticsSmallDTOs_new(entities, actorCompanyId, employees, new List<int>() { timePeriod.TimePeriodId }, setPensionCompany: true, ignoreAccounting: true);
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, roleId, actorCompanyId);
                string avtalsnummer = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportKPAAgreementNumber, userId, actorCompanyId, 0);

                foreach (var employee in employees)
                {
                    var transactions = allTransactions.Where(w => w.EmployeeId == employee.EmployeeId).ToList();
                    if (transactions.IsNullOrEmpty())
                        continue;

                    var employment = employee.GetEmployment(dateFrom, dateTo);
                    var payrollGroup = employment.GetPayrollGroup(dateFrom, dateTo, payrollGroups);

                    decimal avtaladManadslon = 0;
                    decimal grundlonPerTimma = 0;
                    decimal utbNivaPerTimma = 0;

                    TermGroup_PayrollExportSalaryType salaryType = EmployeeManager.GetEmployeeSalaryType(employee, dateFrom, dateTo);

                    #region LoneTyp

                    int loneTyp = 0;

                    if (employee.BygglosenSalaryType > 0)
                    {
                        loneTyp = employee.BygglosenSalaryType;
                    }
                    else if (payrollGroup != null && employment != null)
                    {
                        var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.BygglosenSalaryType);
                        if (setting != null && setting.IntData.HasValue)
                            loneTyp = setting.IntData.Value;
                    }

                    #endregion

                    #region Avtal månadslön
                    if (loneTyp == 1)
                    {
                        if (employee.BygglosenSalaryFormula != 0 && salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                        {
                            PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, dateTo, null, null, employee.BygglosenSalaryFormula);
                            if (result != null)
                                avtaladManadslon = Convert.ToInt64(result.Amount);
                        }
                        else if (payrollGroup != null && employment != null && salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                        {
                            var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.BygglosenSalaryFormula);
                            if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                            {
                                PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, dateTo, null, null, setting.IntData.Value);
                                if (result != null)
                                    avtaladManadslon = Convert.ToInt64(result.Amount);
                            }
                        }
                    }
                    else
                    {
                        if (employee.BygglosenSalaryFormula != 0 && employment != null)
                        {
                            grundlonPerTimma = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, dateTo, null, null, employee.BygglosenSalaryFormula)?.Amount ?? 0;
                        }
                        else if (payrollGroup != null && employment != null) 
                        {
                            var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.BygglosenSalaryFormula);
                            if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                                grundlonPerTimma = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, dateTo, null, null, setting.IntData.Value)?.Amount ?? 0;
                        }
                        if (employee.BygglosenAgreedHourlyPayLevel.HasValue && employee.BygglosenAgreedHourlyPayLevel.Value != 0)
                        {
                            utbNivaPerTimma = employee.BygglosenAgreedHourlyPayLevel ?? 0;
                        }
                        else
                        {
                            var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.BygglosenAgreedHourlyPayLevel);
                            if (setting != null && setting.DecimalData.HasValue && setting.DecimalData.Value != 0)
                                utbNivaPerTimma = setting.DecimalData.Value;
                        }
                    }

                    #endregion

                    #region Kommunkod

                    string kommunkod = "";

                    if (!string.IsNullOrEmpty(employee.BygglosenMunicipalCode))
                        kommunkod = employee.BygglosenMunicipalCode;

                    else if (!kommunkoder.IsNullOrEmpty())
                        kommunkod = SettingManager.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.PayrollExportCommunityCode, userId, actorCompanyId, 0);

                    #endregion

                    #region Fördelningstal

                    int fordelningskod = 0;

                    if (!string.IsNullOrEmpty(employee.BygglosenAllocationNumber))
                    {
                        int bygglosenAllocationNumber = 0;
                        int.TryParse(employee.BygglosenAllocationNumber, out bygglosenAllocationNumber);
                        if (bygglosenAllocationNumber != 0 && bygglosenAllocationNumber <= 100)
                            fordelningskod = bygglosenAllocationNumber;
                    }
                    else if (payrollGroup != null && employment != null)
                    {
                        var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.BygglosenAllocationNumber);
                        if (setting != null)
                        {
                            int.TryParse(setting.StrData, out int payrollGroupbygglosenAllocationNumber);
                            if (payrollGroupbygglosenAllocationNumber != 0 && payrollGroupbygglosenAllocationNumber <= 100)
                            {
                                fordelningskod = payrollGroupbygglosenAllocationNumber;
                            }
                        }
                            
                    }
                    #endregion

                    #region Arbetsplatsnr

                    if (!string.IsNullOrEmpty(employee.BygglosenWorkPlaceNumber))
                    {
                        arbetsplatsnr = employee.BygglosenWorkPlaceNumber;
                    }
                    else if (payrollGroup != null && employment != null)
                    {
                        var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.ByggLosenWorkPlaceNr);
                        if (!string.IsNullOrEmpty(setting?.StrData))
                            arbetsplatsnr = setting.StrData;
                    }
                        
                    #endregion

                    #region Yrkeskod

                    int yrkeskod = 0;

                    var employeePositions = befattningar.Where(w => w.EmployeeId == employee.EmployeeId && w.Default).ToList();

                    if (!employeePositions.IsNullOrEmpty())
                    {
                        var sysCode = employeePositions.FirstOrDefault().Position.SysPositionCode;
                        int.TryParse(sysCode, out yrkeskod);
                    }

                    #endregion

                    #region Avtalsområde

                    int avtalsomrade = 0;

                    if (!string.IsNullOrEmpty(employee.BygglosenAgreementArea))
                    {
                        int.TryParse(employee.BygglosenAgreementArea, out avtalsomrade);
                    }
                    else if (payrollGroup != null && employment != null)
                    {
                        var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.BygglosenAgreementArea);
                        if (!string.IsNullOrEmpty(setting?.StrData))
                            int.TryParse(setting.StrData, out avtalsomrade);
                    }

                    #endregion

                    int yrkesKategori = 0;
                    int.TryParse(employee.BygglosenProfessionCategory, out yrkesKategori);

                    #region Salary

                    decimal salary = 0;
                    decimal workTime = decimal.Divide(transactions.Where(w => w.IsWorkTime() && !w.IsScheduleTransaction).Sum(s => s.Quantity), 60);

                    if (salaryType == TermGroup_PayrollExportSalaryType.Hourly)
                    {
                        salary = transactions.Where(w => w.IsHourlySalary() && !w.IsScheduleTransaction).Sum(s => s.Amount);
                    }
                    else if (salaryType == TermGroup_PayrollExportSalaryType.Monthly)
                    {
                        decimal absence = transactions.Where(w => (w.IsAbsence()) && !w.IsScheduleTransaction).Sum(s => s.Amount);
                        salary = transactions.Where(w => w.IsMonthlySalary() && !w.IsScheduleTransaction).Sum(s => s.Amount) + absence;
                    }

                    #endregion

                    #region Supplemenets

                    decimal roleSupplement = transactions.Where(w => w.IsRoleSupplement()).Sum(s => s.Amount);
                    decimal activitySupplement = transactions.Where(w => w.IsActivitySupplement()).Sum(s => s.Amount);
                    decimal competenceSupplement = transactions.Where(w => w.IsCompetenceSupplement()).Sum(s => s.Amount);
                    decimal responsibilitySupplement = transactions.Where(w => w.IsResponsibilitySupplement()).Sum(s => s.Amount);

                    #endregion

                    #region PayedoutExcess

                    decimal utbetaltOverskott = transactions.Where(w => w.IsBygglosenPaidoutExcess() && !w.IsScheduleTransaction).Sum(s => s.Amount);

                    #endregion

                    var employeeDTO = new BygglosenEmployeeDTO()
                    {
                        Arbetsplatsnr = arbetsplatsnr,
                        UtlanadTillOrgnr = StringUtility.RemoveDash(ExportFilesHelper.FillWithZerosBeginning(10, employee.BygglosenLendedToOrgNr)),
                        Anstallningsnummer = employee.EmployeeNr,
                        LoneperiodStartdatum = dateFrom,
                        LoneperiodensSlutdatum = dateTo,
                        Personnummer = employee.SocialSec,
                        EmployeeId = employee.EmployeeId,
                        Foretagsnamn = Company.Name,
                        Organisationsnummer = StringUtility.RemoveDash(Company.OrgNr),
                        Namn = employee.Name,
                        Arbetadetimmar = Math.Round(workTime, 2),
                        Lonesumma = salary,
                        AvtaladManadslon = avtaladManadslon,
                        OBTillagg = transactions.Where(w => w.IsGrossSalary() && w.IsOBAddition() && !w.IsScheduleTransaction).Sum(s => s.Amount),
                        OvertidsTillagg = transactions.Where(w => w.IsGrossSalary() && w.IsOverTimeAddition()).Sum(s => s.Amount),
                        OvertidsTimmar = decimal.Divide(transactions.Where(w => w.IsGrossSalary() && w.IsOvertimeCompensation()).Sum(s => s.Quantity), 60),
                        Lonetyp = loneTyp,
                        AvtalsOmrade = avtalsomrade,
                        ByggarbetsplatsensLanOchKommun = kommunkod,
                        ByggarbetsplatsensPostort = "",
                        Fordelningstal = fordelningskod,
                        Yrkeskod = yrkeskod,
                        Yrkeskategori = yrkesKategori,

                        //Prestationslön
                        GrundlonPerTimma = Math.Round(grundlonPerTimma, 2),
                        UtbNivaPerTimma =Math.Round(utbNivaPerTimma, 2),
                        UtbetaltOverskott = Math.Round(utbetaltOverskott, 2),

                        //Tillägg
                        Rolltillagg = Math.Round(roleSupplement,2),
                        Aktivitetstillagg = Math.Round(activitySupplement, 2),
                        Kompetenstillagg = Math.Round(competenceSupplement, 2),
                        Ansvarstillagg = Math.Round(responsibilitySupplement, 2),
                    };

                    dto.BygglosenEmployees.Add(employeeDTO);
                }
            }

            if (setAsFinal)
                dto.EventHistories.Add(new EventHistoryDTO(Company.ActorCompanyId, TermGroup_EventHistoryType.ByggLosenNyckel, SoeEntityType.FileIdentifier, Company.ActorCompanyId, userId: UserId, stringValue: StringUtility.FillWithZerosBeginning(13, CalendarUtility.ToFileFriendlyDateTime(DateTime.Now), true), dateValue: DateTime.Now));

            return dto;
        }

    }

    public class BygglosenDTO
    {
        public BygglosenDTO()
        {
            BygglosenEmployees = new List<BygglosenEmployeeDTO>();
            EventHistories = new List<EventHistoryDTO>();
        }

        public List<EventHistoryDTO> EventHistories { get; set; }
        public List<BygglosenEmployeeDTO> BygglosenEmployees { get; set; }

        private string GetThisFileIdentifier()
        {
            var fileIdentifierEventHistory = this.EventHistories.FirstOrDefault(f => f.Type == TermGroup_EventHistoryType.ByggLosenNyckel);

            if (fileIdentifierEventHistory != null && !string.IsNullOrEmpty(fileIdentifierEventHistory.StringValue))
                return fileIdentifierEventHistory.StringValue;

            return StringUtility.FillWithZerosBeginning(13, CalendarUtility.ToFileFriendlyDateTime(DateTime.Now), true);
        }

        public XDocument GetDocument(string lastFileIdentifier)
        {
            lastFileIdentifier = string.IsNullOrEmpty(lastFileIdentifier) ? null : StringUtility.FillWithZerosBeginning(13, lastFileIdentifier, true);
            var lista_lonegranskning = new Lista_lonegranskning();
            lista_lonegranskning.Lonesystem = "SoftOne GO";
            lista_lonegranskning.KorrigeringAvFil = lastFileIdentifier;
            lista_lonegranskning.Korrigeringskod = GetThisFileIdentifier();

            List<Lista_lonegranskningLonegranskning> lonegransknings = new List<Lista_lonegranskningLonegranskning>();
            foreach (var grouped in BygglosenEmployees.GroupBy(g => $"{g.AvtalsOmrade}#{g.LoneperiodStartdatum}#{g.Lonetyp}"))
            {
                var first = grouped.First();

                var lonegranskning = new Lista_lonegranskningLonegranskning()
                {
                    LoneperiodStartdatum = Convert.ToInt32(first.LoneperiodStartdatum.ToString("yyyyMMdd")),
                    LoneperiodSlutdatum = Convert.ToInt32(first.LoneperiodensSlutdatum.ToString("yyyyMMdd")),
                    Avtalsomrade = first.AvtalsOmrade,
                    Foretagsnamn = first.Foretagsnamn,
                    LanOchKommun = first.ByggarbetsplatsensLanOchKommun,
                    Lonetyp = first.Lonetyp,
                    Organisationsnummer = first.Organisationsnummer,
                    Postort = first.ByggarbetsplatsensPostort
                };

                List<Lista_lonegranskningLonegranskningPerson> list = new List<Lista_lonegranskningLonegranskningPerson>();

                foreach (var employee in grouped)
                {
                    var person = new Lista_lonegranskningLonegranskningPerson()
                    {
                        Arbetsplatsnr = employee.Arbetsplatsnr,
                        UtlanadTillOrgnr = employee.UtlanadTillOrgnr,
                        Anstallningsnummer = employee.Anstallningsnummer,
                        ArbetadeTimmar = employee.Arbetadetimmar,
                        AvtalsenligManadslon = employee.AvtaladManadslon,
                        Fordelningstal = employee.Fordelningstal,
                        Lonesumma = employee.Lonesumma,
                        Namn = employee.Namn,
                        OBTillagg = employee.OBTillagg,
                        ArbetsplatsnrSpecified = false,
                        UtlanadTillOrgnrSpecified = false,
                        GrundlonPerTimmaSpecified = false,
                        UtbNivaPerTimmaSpecified = false,
                        GrundlonPerTimma = employee.GrundlonPerTimma,
                        UtbNivaPerTimma = employee.UtbNivaPerTimma,
                        UtbetaltOverskott = employee.UtbetaltOverskott,
                        Personnummer = StringUtility.SocialSecYYMMDDXXXX(employee.Personnummer),
                        UtbetaltOverskottSpecified = false,
                        Overtidstillagg = employee.OvertidsTillagg,
                        Overtidstimmar = employee.OvertidsTimmar,
                        Rolltillagg = employee.Rolltillagg,
                        Aktivitetstillagg = employee.Aktivitetstillagg,
                        Kompetenstillagg = employee.Kompetenstillagg,
                        Ansvarstillagg = employee.Ansvarstillagg,
                        Yrkeskod = employee.Yrkeskod,
                        Yrkeskategori = employee.Yrkeskategori,
                    };

                    if (employee.Lonetyp > 1)
                    {
                        person.GrundlonPerTimmaSpecified = true;
                        person.UtbNivaPerTimmaSpecified = true;
                        person.UtbetaltOverskottSpecified = true;
                        person.UtlanadTillOrgnrSpecified = true;
                        person.ArbetsplatsnrSpecified = true;

                    }
                   
                    list.Add(person);
                }

                lonegranskning.Personer = list.ToArray();
                lonegransknings.Add(lonegranskning);
            }

            lista_lonegranskning.Lonegranskning = lonegransknings.ToArray();

            XDocument doc = new XDocument();

            using (var writer = doc.CreateWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Lista_lonegranskning));
                serializer.Serialize(writer, lista_lonegranskning);
            }



            //using (var writer = doc.CreateWriter())
            //{
            //    // write xml into the writer
            //    var serializer = new XmlSerializer(lista_lonegranskning.GetType());
            //    serializer.WriteObject(writer, lista_lonegranskning);
            //}
            return doc;

        }
    }


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    [DataContract]
    public class Lista_lonegranskning
    {

        private string korrigeringskodField;

        private string korrigeringAvFilField;

        private string lonesystemField;

        private Lista_lonegranskningLonegranskning[] lonegranskningField;

        /// <remarks/>
        public string Korrigeringskod
        {
            get
            {
                return this.korrigeringskodField;
            }
            set
            {
                this.korrigeringskodField = value;
            }
        }

        public bool ShouldSerializeKorrigeringAvFil()
        {
            return !string.IsNullOrEmpty(KorrigeringAvFil);
        }

        /// <remarks/>
        [XmlElement (IsNullable=true)]
        public string KorrigeringAvFil
        {
            get
            {
                return this.korrigeringAvFilField;
            }
            set
            {
                this.korrigeringAvFilField = value;
            }
        }

        /// <remarks/>
        public string Lonesystem
        {
            get
            {
                return this.lonesystemField;
            }
            set
            {
                this.lonesystemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Lonegranskning")]
        public Lista_lonegranskningLonegranskning[] Lonegranskning
        {
            get
            {
                return this.lonegranskningField;
            }
            set
            {
                this.lonegranskningField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [DataContract]
    public class Lista_lonegranskningLonegranskning
    {

        private string organisationsnummerField;

        private string foretagsnamnField;

        private int loneperiodStartdatumField;

        private int loneperiodSlutdatumField;

        private int avtalsomradeField;

        private string postortField;

        private Lista_lonegranskningLonegranskningPerson[] personerField;

        /// <remarks/>
        public string Organisationsnummer
        {
            get
            {
                return this.organisationsnummerField;
            }
            set
            {
                this.organisationsnummerField = value;
            }
        }

        /// <remarks/>
        public string Foretagsnamn
        {
            get
            {
                return this.foretagsnamnField;
            }
            set
            {
                this.foretagsnamnField = value;
            }
        }

        /// <remarks/>
        public int LoneperiodStartdatum
        {
            get
            {
                return this.loneperiodStartdatumField;
            }
            set
            {
                this.loneperiodStartdatumField = value;
            }
        }

        /// <remarks/>
        public int LoneperiodSlutdatum
        {
            get
            {
                return this.loneperiodSlutdatumField;
            }
            set
            {
                this.loneperiodSlutdatumField = value;
            }
        }

        /// <remarks/>
        public int Avtalsomrade
        {
            get
            {
                return this.avtalsomradeField;
            }
            set
            {
                this.avtalsomradeField = value;
            }
        }

        /// <remarks/>
        public int Lonetyp { get; set; }

        /// <remarks/>
        public string LanOchKommun { get; set; }

        /// <remarks/>
        public string Postort
        {
            get
            {
                return this.postortField;
            }
            set
            {
                this.postortField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Person", IsNullable = false)]
        public Lista_lonegranskningLonegranskningPerson[] Personer
        {
            get
            {
                return this.personerField;
            }
            set
            {
                this.personerField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [DataContract]
    public class Lista_lonegranskningLonegranskningPerson
    {

        private string arbetsplatsnrField;

        private string utlanadTillOrgnrField;

        private string anstallningsnummerField;

        private string personnummerField;

        private string namnField;

        private int fordelningstalField;

        private decimal arbetadeTimmarField;

        private decimal grundlonPerTimmaField;

        private bool grundlonPerTimmaFieldSpecified;

        private decimal utbNivaPerTimmaField;

        private bool utbNivaPerTimmaFieldSpecified;

        private decimal utbetaltOverskottField;

        private bool utbetaltOverskottFieldSpecified;

        private decimal lonesummaField;

        private decimal oBTillaggField;

        private decimal avtalsenligManadslonField;

        private decimal overtidstimmarField;

        private decimal overtidstillaggField;

        private int yrkeskodField;


        /// <remarks/>
        public string Arbetsplatsnr
        {
            get
            {
                return this.arbetsplatsnrField;
            }
            set
            {
                this.arbetsplatsnrField = value;
            }
        }

        /// <remarks/>
        public string UtlanadTillOrgnr
        {
            get
            {
                return this.utlanadTillOrgnrField;
            }
            set
            {
                this.utlanadTillOrgnrField = value;
            }
        }

        /// <remarks/>
        public string Anstallningsnummer
        {
            get
            {
                return this.anstallningsnummerField;
            }
            set
            {
                this.anstallningsnummerField = value;
            }
        }

        /// <remarks/>
        public string Personnummer
        {
            get
            {
                return this.personnummerField;
            }
            set
            {
                this.personnummerField = value;
            }
        }

        /// <remarks/>
        public string Namn
        {
            get
            {
                return this.namnField;
            }
            set
            {
                this.namnField = value;
            }
        }

        /// <remarks/>
        public int Fordelningstal
        {
            get
            {
                return this.fordelningstalField;
            }
            set
            {
                this.fordelningstalField = value;
            }
        }

        /// <remarks/>
        public decimal ArbetadeTimmar
        {
            get
            {
                return this.arbetadeTimmarField;
            }
            set
            {
                this.arbetadeTimmarField = value;
            }
        }

        /// <remarks/>
        public decimal GrundlonPerTimma
        {
            get
            {
                return this.grundlonPerTimmaField;
            }
            set
            {
                this.grundlonPerTimmaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GrundlonPerTimmaSpecified
        {
            get
            {
                return this.grundlonPerTimmaFieldSpecified;
            }
            set
            {
                this.grundlonPerTimmaFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal UtbNivaPerTimma
        {
            get
            {
                return this.utbNivaPerTimmaField;
            }
            set
            {
                this.utbNivaPerTimmaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UtbNivaPerTimmaSpecified
        {
            get
            {
                return this.utbNivaPerTimmaFieldSpecified;
            }
            set
            {
                this.utbNivaPerTimmaFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UtlanadTillOrgnrSpecified { get; set; }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ArbetsplatsnrSpecified { get; set; }
        /// <remarks/>
        public decimal UtbetaltOverskott
        {
            get
            {
                return this.utbetaltOverskottField;
            }
            set
            {
                this.utbetaltOverskottField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UtbetaltOverskottSpecified
        {
            get
            {
                return this.utbetaltOverskottFieldSpecified;
            }
            set
            {
                this.utbetaltOverskottFieldSpecified = value;
            }
        }

        /// <remarks/>
        public decimal Lonesumma
        {
            get
            {
                return this.lonesummaField;
            }
            set
            {
                this.lonesummaField = value;
            }
        }

        /// <remarks/>
        public decimal OBTillagg
        {
            get
            {
                return this.oBTillaggField;
            }
            set
            {
                this.oBTillaggField = value;
            }
        }

        /// <remarks/>
        public decimal AvtalsenligManadslon
        {
            get
            {
                return this.avtalsenligManadslonField;
            }
            set
            {
                this.avtalsenligManadslonField = value;
            }
        }

        /// <remarks/>
        public decimal Overtidstimmar
        {
            get
            {
                return this.overtidstimmarField;
            }
            set
            {
                this.overtidstimmarField = value;
            }
        }

        /// <remarks/>
        public decimal Overtidstillagg
        {
            get
            {
                return this.overtidstillaggField;
            }
            set
            {
                this.overtidstillaggField = value;
            }
        }
        public decimal Rolltillagg { get; set; }
        public decimal Aktivitetstillagg { get; set; }
        public decimal Kompetenstillagg { get; set; }
        public decimal Ansvarstillagg { get; set; }

        /// <remarks/>
        public int Yrkeskod
        {
            get
            {
                return this.yrkeskodField;
            }
            set
            {
                this.yrkeskodField = value;
            }
        }
        /// <remarks/>
        public int Yrkeskategori { get; set; }
    
    }

    public class BygglosenEmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string Anstallningsnummer { get; set; }
        public string Organisationsnummer { get; set; }
        public string Foretagsnamn { get; set; }
        public DateTime LoneperiodStartdatum { get; set; }
        public DateTime LoneperiodensSlutdatum { get; set; }
        public string Personnummer { get; set; }
        public string Namn { get; set; }
        public int AvtalsOmrade { get; set; }
        public int Fordelningstal { get; set; }
        public int Lonetyp { get; set; }
        public decimal Arbetadetimmar { get; set; }
        public decimal Lonesumma { get; set; }
        public decimal AvtaladManadslon { get; set; }
        public decimal OvertidsTimmar { get; set; }
        public decimal OvertidsTillagg { get; set; }
        public string ByggarbetsplatsensLanOchKommun { get; set; }
        public string ByggarbetsplatsensPostort { get; set; }
        public int Yrkeskod { get; set; }
        public int Yrkeskategori { get; set; }
        public decimal OBTillagg { get; set; }
        public string Arbetsplatsnr { get; set; }
        public string UtlanadTillOrgnr { get; set; }
        public decimal GrundlonPerTimma { get; set; }
        public decimal UtbNivaPerTimma { get; set; }
        public decimal UtbetaltOverskott { get; set; }

        public decimal Rolltillagg { get; set; }
        public decimal Aktivitetstillagg { get; set; }
        public decimal Kompetenstillagg { get; set; }
        public decimal Ansvarstillagg { get; set; }
    }
}
