using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    #region DTO

    public class SCB_SLPPayrollStatisticsFileHeadDTO
    {
        public int NumberOfEmployees { get; set; }
        public DateTime Date { get; set; }
        public List<SCB_SLPPayrollStatisticsFileRowDTO> SCBPayrollStatisticsFileRowDTOs { get; set; }
    }

    public class SCB_SLPPayrollStatisticsFileRowDTO
    {
        public string Period { get; set; }
        public string Delagarnummer { get; set; }
        public string Arbetsplatsnummer { get; set; }
        public string Organisationsnummer { get; set; }
        public string Forbundsnummer { get; set; }
        public string Avtalskod { get; set; }
        public string Personnummer { get; set; }
        public string Anstnummer { get; set; }
        public string Namn { get; set; }
        public string Personalkategori { get; set; }
        public string Arbetstidsart { get; set; }
        public string Yrkeskod { get; set; }
        public string Forbundsspecifikkod { get; set; }
        public string Loneform { get; set; }
        public string AntalanstalldaCFARnr { get; set; }
        public string CFARnummer { get; set; }
        public string Helglon { get; set; }
        public string Reserv { get; set; }
        public string Falt1a { get; set; }
        public string Falt1b { get; set; }
        public string Falt2a { get; set; }
        public string Falt2b { get; set; }
        public string Falt3a { get; set; }
        public string Falt3b { get; set; }
        public string Falt4a { get; set; }
        public string Falt4b { get; set; }
        public string Falt5a { get; set; }
        public string Falt5b { get; set; }
        public string Falt6a { get; set; }
        public string Falt6b { get; set; }
        public string Falt7a { get; set; }
        public string Falt7b { get; set; }
        public string Falt8a { get; set; }
        public string Falt8b { get; set; }
        public string Falt9a { get; set; }
        public string Falt9b { get; set; }
        public string Falt10aa { get; set; }
        public string Falt10ab { get; set; }
        public string Falt10ba { get; set; }
        public string Falt10bb { get; set; }
        public string Falt10ca { get; set; }
        public string Falt10cb { get; set; }
        public string Falt11a { get; set; }
        public string Falt11b { get; set; }
        public string Falt12a { get; set; }
        public string Falt12b { get; set; }
        public string Falt13a { get; set; }
        public string Falt13b { get; set; }
        public string Falt14a { get; set; }
        public string Falt14b { get; set; }
        public string Falt15aa { get; set; }
        public string Falt15ab { get; set; }
        public string Falt15ba { get; set; }
        public string Falt15bb { get; set; }
        public string Falt16a { get; set; }
        public string Falt16b { get; set; }
        public string Falt17a { get; set; }
        public string Falt17b { get; set; }
        public string Falt18a { get; set; }
        public string Falt18b { get; set; }
        public string Falt19a { get; set; }
        public string Falt19b { get; set; }
        public string Falt20a { get; set; }
        public string Falt20b { get; set; }
        public string Falt21a { get; set; }
        public string Falt21b { get; set; }
        public string Falt22a { get; set; }
        public string Falt22b { get; set; }
        public string Falt23a { get; set; }
        public string Falt23b { get; set; }
        public string Falt24a { get; set; }
        public string Falt24b { get; set; }
        public string Falt25a { get; set; }
        public string Falt25b { get; set; }
        public string Falt26a { get; set; }
        public string Falt26b { get; set; }
        public string Falt27a { get; set; }
        public string Falt27b { get; set; }
        public string Falt28a { get; set; }
        public string Falt28b { get; set; }
        public string Falt29a { get; set; }
        public string Falt29b { get; set; }
        public string Falt30a { get; set; }
        public string Falt30b { get; set; }
        public string Falt31a { get; set; }
        public string Falt31b { get; set; }
        public string Falt32a { get; set; }
        public string Falt32b { get; set; }
        public string Falt33a { get; set; }
        public string Falt33b { get; set; }
        public string Falt34a { get; set; }
        public string Falt34b { get; set; }
        public string Falt35a { get; set; }
        public string Falt35b { get; set; }
        public string Falt36a { get; set; }
        public string Falt36b { get; set; }
        public string Falt37a { get; set; }
        public string Falt37b { get; set; }
        public string Falt38a { get; set; }
        public string Falt38b { get; set; }
        public string Falt39a { get; set; }
        public string Falt39b { get; set; }
        public string Falt40a { get; set; }
        public string Falt40b { get; set; }
        public string Falt41a { get; set; }
        public string Falt41b { get; set; }
        public string Falt42a { get; set; }
        public string Falt42b { get; set; }
        public string Falt43a { get; set; }
        public string Falt43b { get; set; }
        public string Falt44a { get; set; }
        public string Falt44b { get; set; }
        public string Falt45a { get; set; }
        public string Falt45b { get; set; }
        public string Falt46a { get; set; }
        public string Falt46b { get; set; }

        public List<SCBPayrollStatisticsTransactionDTO> scbPayrollStatisticsTransactionDTOs { get; set; }

    }

    public class SCBPayrollStatisticsTransactionDTO : IPayrollType
    {
        public DateTime Date { get; set; }
        public string EmployeeNr { get; set; }
        public string Type { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public string ProductNr { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
    public class SCB_KSPPayrollStatisticsFileHeadDTO
    {
        public int TotaltAntalAnstallda { get; set; }
        public int AntalAnstalldaMan { get; set; }
        public int AntalAnstalldaKvinnor { get; set; }
        public int TotaltAntalTillsvidareAnstallda { get; set; }
        public int AntalTillsvidareAnstalldaMan { get; set; }
        public int AntalTillsvidareAnstalldaKvinnor { get; set; }
        public int TotaltAntalVisstidsAnstallda { get; set; }
        public int AntalVisstidsAnstalldaMan { get; set; }
        public int AntalVisstidsAnstalldaKvinnor { get; set; }
        public int TotaltAntalHeldagsfranvarande { get; set; }
        public int AntalHeldagsfranvarandeMan { get; set; }
        public int AntalHeldagsfranvarandeKvinnor { get; set; }
        public int TotaltAntalHeldagsfranvarandeSjukdomArbetsskada { get; set; }
        public int AntalHeldagsfranvarandeSjukdomArbetsskadaMan { get; set; }
        public int AntalHeldagsfranvarandeSjukdomArbetsskadaKvinnor { get; set; }
        public int TotaltAntalHeldagsfranvarandeSemester { get; set; }
        public int AntalHeldagsfranvarandeSemesterMan { get; set; }
        public int AntalHeldagsfranvarandeSemesterKvinnor { get; set; }
        public int TotaltAntalHeldagsfranvarandeOvrigFranvaro { get; set; }
        public int AntalHeldagsfranvarandeOvrigFranvaroMan { get; set; }
        public int AntalHeldagsfranvarandeOvrigFranvaroKvinnor { get; set; }
        public int NyAnstallda { get; set; }
        public int NyAnstalldaMan { get; set; }
        public int NyAnstalldaKvinnor { get; set; }
        public int NyAnstalldaVissTidTotalt { get; set; }
        public int NyAnstalldaVissTidMan { get; set; }
        public int NyAnstalldaVissTidKvinnor { get; set; }
        public int NyAnstalldaTillsvidareTotalt { get; set; }
        public int NyAnstalldaTillsvidareMan { get; set; }
        public int NyAnstalldaTillsvidareKvinnor { get; set; }
        public int Avgangna { get; set; }
        public int AvgangnaMan { get; set; }
        public int AvgangnaKvinnor { get; set; }
        public int AvgangnaVissTidTotalt { get; set; }
        public int AvgangnaVissTidMan { get; set; }
        public int AvgangnaVissTidKvinnor { get; set; }
        public int AvgangnaTillsvidareTotalt { get; set; }
        public int AvgangnaTillsvidareMan { get; set; }
        public int AvgangnaTillsvidareKvinnor { get; set; }
        public DateTime Date { get; set; }
        public List<SCBPayrollStatisticsTransactionDTO> SCBPayrollStatisticsTransactionDTO { get; set; }
    }
    public class SCB_KSPPayrollStatisticsFileRowTO
    {

    }

    public class SCB_KSJUPayrollStatisticsFileHeadDTO
    {
        public int NumberOfEmployees { get; set; }
        public DateTime Date { get; set; }
        public List<SCB_KSJUPayrollStatisticsFileRowDTO> SCB_KSJUPayrollStatisticsFileRowDTOs { get; set; }
    }
    public class SCB_KSJUPayrollStatisticsFileRowDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string PeOrgNr { get; set; }
        public string PersonNr { get; set; }
        public DateTime SjukFrom { get; set; }
        public DateTime SjukTom { get; set; }
        public int HelAntDagar { get; set; }
        public int DelAntDagar { get; set; }
        public int AntDagar { get; set; }
        public bool Korrigeringsuppgift { get; set; }
        public string Cfar { get; set; }
    }


    public class SCB_KLPPayrollStatisticsFileHeadDTO
    {
        public decimal NumberOfEmployees { get; set; }
        public DateTime Date { get; set; }
        public decimal OverenskommenManadslon { get; set; }
        public decimal Utbetaldlon { get; set; }
        public decimal AntalHeltidsTjanster { get; set; }
        public decimal DaravOvertidstillagg { get; set; }
        public decimal ArbetadeTimmar { get; set; }
        public decimal AvtaladeTimmar { get; set; }
        public decimal Permittering { get; set; }
        public decimal DaravOvertidstimmar { get; set; }
        public decimal Retrolon { get; set; }
        public decimal Sjuklon { get; set; }
        public decimal RorligaTillagg { get; set; }
        public decimal RorligaTillaggTidigarePerioder { get; set; }

        public DateTime? RetrolonFran { get; set; }
        public DateTime? RetrolonTill { get; set; }
        public string UtbetalningsManad { get; set; }

        public List<SCB_KLPPayrollStatisticsFileRowDTO> SCBPayrollStatisticsFileRowDTOs { get; set; }

        public SCB_KLPPayrollStatisticsFileHeadDTO TimAvlonade
        {
            get
            {
                var timavlonade = this.SCBPayrollStatisticsFileRowDTOs.Where(w => w.IsTimavlonad).ToList();
                var retroFran = timavlonade.Where(w => w.Retrolon > 0 && w.RetrolonFran.HasValue);
                var retroTill = timavlonade.Where(w => w.Retrolon > 0 && w.RetrolonTill.HasValue);
                return new SCB_KLPPayrollStatisticsFileHeadDTO()
                {
                    NumberOfEmployees = timavlonade.Count,
                    Sjuklon = timavlonade.Sum(s => s.Sjuklon),
                    ArbetadeTimmar = timavlonade.Sum(s => s.ArbetadeTimmar),
                    AvtaladeTimmar = timavlonade.Sum(s => s.AvtaladeTimmar),
                    DaravOvertidstillagg = timavlonade.Sum(s => s.DaravOvertidstillagg),
                    DaravOvertidstimmar = timavlonade.Sum(s => s.DaravOvertidstimmar),
                    Date = this.Date,
                    OverenskommenManadslon = timavlonade.Sum(s => s.OverenskommenManadslon),
                    Retrolon = timavlonade.Sum(s => s.Retrolon),
                    RetrolonFran = retroFran.IsNullOrEmpty() ? null : retroFran.Min(m => m.RetrolonFran),
                    RetrolonTill = retroFran.IsNullOrEmpty() ? null : retroFran.Min(m => m.RetrolonTill),
                    RorligaTillagg = timavlonade.Sum(s => s.RorligaTillagg) > 0 ? timavlonade.Sum(s => s.RorligaTillagg) : 0,
                    RorligaTillaggTidigarePerioder = timavlonade.Sum(s => s.RorligaTillaggTidigarePerioder) > 0 ? timavlonade.Sum(s => s.RorligaTillaggTidigarePerioder) : 0,
                    Utbetaldlon = timavlonade.Sum(s => s.Utbetaldlon),
                };
            }
        }
        public SCB_KLPPayrollStatisticsFileHeadDTO ManAvlonade
        {
            get
            {
                var mansavlonade = this.SCBPayrollStatisticsFileRowDTOs.Where(w => w.IsManadsavlonad).ToList();
                var retroFran = mansavlonade.Where(w => w.Retrolon > 0 && w.RetrolonFran.HasValue);
                var retroTill = mansavlonade.Where(w => w.Retrolon > 0 && w.RetrolonTill.HasValue);
                return new SCB_KLPPayrollStatisticsFileHeadDTO()
                {
                    NumberOfEmployees = mansavlonade.Count,
                    Sjuklon = mansavlonade.Sum(s => s.Sjuklon),
                    ArbetadeTimmar = mansavlonade.Sum(s => s.ArbetadeTimmar),
                    AvtaladeTimmar = mansavlonade.Sum(s => s.AvtaladeTimmar),
                    DaravOvertidstillagg = mansavlonade.Sum(s => s.DaravOvertidstillagg),
                    DaravOvertidstimmar = mansavlonade.Sum(s => s.DaravOvertidstimmar),
                    Date = this.Date,
                    OverenskommenManadslon = mansavlonade.Sum(s => s.OverenskommenManadslon),
                    Retrolon = mansavlonade.Sum(s => s.Retrolon),
                    RetrolonFran = retroFran.IsNullOrEmpty() ? null : retroFran.Min(m => m.RetrolonFran),
                    RetrolonTill = retroFran.IsNullOrEmpty() ? null : retroFran.Min(m => m.RetrolonTill),
                    RorligaTillagg = mansavlonade.Sum(s => s.RorligaTillagg) > 0 ? mansavlonade.Sum(s => s.RorligaTillagg) : 0,
                    RorligaTillaggTidigarePerioder = mansavlonade.Sum(s => s.RorligaTillaggTidigarePerioder) > 0 ? mansavlonade.Sum(s => s.RorligaTillaggTidigarePerioder) : 0,
                    Utbetaldlon = mansavlonade.Sum(s => s.Utbetaldlon)

                };
            }
        }
        public SCB_KLPPayrollStatisticsFileHeadDTO TjanstemanAvlonade
        {
            get
            {
                var tjansteman = this.SCBPayrollStatisticsFileRowDTOs.Where(w => w.isTjansteman).ToList();
                var retroFran = tjansteman.Where(w => w.Retrolon > 0 && w.RetrolonFran.HasValue);
                var retroTill = tjansteman.Where(w => w.Retrolon > 0 && w.RetrolonTill.HasValue);
                return new SCB_KLPPayrollStatisticsFileHeadDTO()
                {
                    NumberOfEmployees = tjansteman.Count,
                    Sjuklon = tjansteman.Sum(s => s.Sjuklon),
                    ArbetadeTimmar = tjansteman.Sum(s => s.ArbetadeTimmar),
                    AvtaladeTimmar = tjansteman.Sum(s => s.AvtaladeTimmar),
                    DaravOvertidstillagg = tjansteman.Sum(s => s.DaravOvertidstillagg),
                    DaravOvertidstimmar = tjansteman.Sum(s => s.DaravOvertidstimmar),
                    Date = this.Date,
                    OverenskommenManadslon = tjansteman.Sum(s => s.OverenskommenManadslon),
                    Retrolon = tjansteman.Sum(s => s.Retrolon),
                    RetrolonFran = retroFran.IsNullOrEmpty() ? null : retroFran.Min(m => m.RetrolonFran),
                    RetrolonTill = retroFran.IsNullOrEmpty() ? null : retroFran.Min(m => m.RetrolonTill),
                    RorligaTillagg = tjansteman.Sum(s => s.RorligaTillagg) > 0 ? tjansteman.Sum(s => s.RorligaTillagg) : 0,
                    RorligaTillaggTidigarePerioder = tjansteman.Sum(s => s.RorligaTillaggTidigarePerioder) > 0 ? tjansteman.Sum(s => s.RorligaTillaggTidigarePerioder) : 0,
                    Utbetaldlon = tjansteman.Sum(s => s.Utbetaldlon),
                    AntalHeltidsTjanster = tjansteman.Sum(s => decimal.Divide(s.Sysselsattningsgrad, 100))
                };
            }
        }
    }

    public class SCB_KLPPayrollStatisticsFileRowDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string PersonNr { get; set; }
        public string Typ { get; set; }
        public string Loneform { get; set; }
        public string PersonalKategori { get; set; }
        public string ArbetsKategori { get; set; }
        public decimal Sysselsattningsgrad { get; set; }
        public decimal OverenskommenManadslon { get; set; }
        public decimal Utbetaldlon { get; set; }
        public decimal DaravOvertidstillagg { get; set; }
        public decimal ArbetadeTimmar { get; set; }
        public decimal AvtaladeTimmar { get; set; }
        public decimal Permittering { get; set; }
        public decimal DaravOvertidstimmar { get; set; }
        public decimal Retrolon { get; set; }
        public DateTime? RetrolonFran { get; set; }
        public DateTime? RetrolonTill { get; set; }
        public decimal Sjuklon { get; set; }
        public decimal RorligaTillagg { get; set; }
        public decimal RorligaTillaggTidigarePerioder { get; set; }
        public string Period { get; set; }

        public bool IsTimavlonad
        {
            get
            {
                return Loneform == "3" && IsArbetare;
            }
        }

        public bool IsManadsavlonad
        {
            get
            {
                return IsArbetare && !IsTimavlonad;
            }
        }
        public bool IsArbetare
        {
            get
            {
                if (PersonalKategori.Equals("1") ||
                         PersonalKategori.Equals("3") ||
                         PersonalKategori.Equals("4") ||
                         PersonalKategori.Equals("5") ||
                         PersonalKategori.Equals("6"))
                {
                    return true;
                }
                else
                    return false;
            }
        }

        public bool isTjansteman
        {
            get
            {
                return !IsArbetare;
            }
        }
        public List<SCBPayrollStatisticsTransactionDTO> SCBPayrollStatisticsTransactionDTOs { get; set; }

    }

    #endregion

    public class SCBStatisticsFiles : ExportFilesBase
    {
        #region Ctor

        public SCBStatisticsFiles(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject, reportResult)
        {
            this.reportResult = reportResult;
        }

        #endregion

        #region Public methods

        public SCB_KSPPayrollStatisticsFileHeadDTO CreateSCB_KSPPayrollStatisticsFileHeadDTO(CompEntities entities)
        {
            var scb_KSPPayrollStatisticsFileHeadDTO = new SCB_KSPPayrollStatisticsFileHeadDTO
            {
                SCBPayrollStatisticsTransactionDTO = new List<SCBPayrollStatisticsTransactionDTO>()
            };

            #region Prereq

            TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date");

            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDate, selectionDate, out List<Employee> employees, out _))
                return null;

            scb_KSPPayrollStatisticsFileHeadDTO.Date = selectionDate;
            DateTime date = selectionDate;
            DateTime beginningOfMonth = CalendarUtility.GetBeginningOfMonth(date);
            DateTime endOfMonth = CalendarUtility.GetEndOfMonth(date);
            List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.ActorCompanyId);
            List<AccountDimDTO> companyAccountDims = AccountManager.GetAccountDimsByCompany(entities, reportResult.ActorCompanyId).ToDTOs();
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, base.ActorCompanyId);

            List<Employee> anstallda = new List<Employee>();
            List<Employee> tillsvidareAnstallda = new List<Employee>();
            List<Employee> visstidsAnstallda = new List<Employee>();

            foreach (Employee employee in employees)
            {
                #region Employee

                Employment employment = employee.GetEmployment(date);
                if (employment == null)
                    continue;

                anstallda.Add(employee);

                if (employment.GetEmploymentType(employmentTypes) == (int)TermGroup_EmploymentType.SE_Permanent)
                {
                    #region Permanent

                    SCBPayrollStatisticsTransactionDTO dto = new SCBPayrollStatisticsTransactionDTO()
                    {
                        ProductNr = string.Empty,
                        Type = "TotaltAntalTillsvidareAnstallda",
                        EmployeeNr = employee.EmployeeNr,
                        Amount = 0,
                        Quantity = 0
                    };
                    scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.Add(dto);
                    tillsvidareAnstallda.Add(employee);
                    #endregion
                }
                else if (employment.GetEmploymentType(employmentTypes) != (int)TermGroup_EmploymentType.SE_Permanent && employment.GetEmploymentType(employmentTypes) != (int)TermGroup_EmploymentType.SE_Trainee)
                {
                    #region Trainee

                    SCBPayrollStatisticsTransactionDTO dto = new SCBPayrollStatisticsTransactionDTO()
                    {
                        ProductNr = string.Empty,
                        Type = "TotaltAntalVisstidsAnstallda",
                        EmployeeNr = employee.EmployeeNr,
                        Amount = 0,
                        Quantity = 0
                    };
                    scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.Add(dto);
                    visstidsAnstallda.Add(employee);

                    #endregion
                }

                #endregion
            }

            #endregion

            #region Sums

            scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalAnstallda = anstallda.Count;
            scb_KSPPayrollStatisticsFileHeadDTO.AntalAnstalldaMan = anstallda.Count(x => x.ContactPerson != null && x.ContactPerson.Sex == (int)TermGroup_Sex.Male);
            scb_KSPPayrollStatisticsFileHeadDTO.AntalAnstalldaKvinnor = anstallda.Count(x => x.ContactPerson != null && x.ContactPerson.Sex == (int)TermGroup_Sex.Female);
            scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalTillsvidareAnstallda = tillsvidareAnstallda.Count;
            scb_KSPPayrollStatisticsFileHeadDTO.AntalTillsvidareAnstalldaMan = tillsvidareAnstallda.Count(x => x.ContactPerson != null && x.ContactPerson.Sex == (int)TermGroup_Sex.Male);
            scb_KSPPayrollStatisticsFileHeadDTO.AntalTillsvidareAnstalldaKvinnor = tillsvidareAnstallda.Count(x => x.ContactPerson != null && x.ContactPerson.Sex == (int)TermGroup_Sex.Female);
            scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalVisstidsAnstallda = visstidsAnstallda.Count;
            scb_KSPPayrollStatisticsFileHeadDTO.AntalVisstidsAnstalldaMan = visstidsAnstallda.Count(x => x.ContactPerson != null && x.ContactPerson.Sex == (int)TermGroup_Sex.Male);
            scb_KSPPayrollStatisticsFileHeadDTO.AntalVisstidsAnstalldaKvinnor = visstidsAnstallda.Count(x => x.ContactPerson != null && x.ContactPerson.Sex == (int)TermGroup_Sex.Female);

            #endregion

            entities.CommandTimeout = 300;

            foreach (Employee anstalld in anstallda)
            {
                #region Employee

                var input = GetAttestEmployeeInput.CreatePayrollInputForWeb(reportResult.ActorCompanyId, parameterObject.UserId, 0, anstalld.EmployeeId, date.Date, date);
                input.SetOptionalParameters(companyHolidays, companyAccountDims);
                var items = TimeTreeAttestManager.GetAttestEmployeeDays(input);
                var first = items.FirstOrDefault();
                if (first != null && first.IsWholedayAbsence)
                {
                    var item = items.FirstOrDefault();
                    if (item != null && item.IsWholedayAbsence)
                    {
                        var transactions = item.AttestPayrollTransactions;
                        if (!transactions.IsNullOrEmpty() && transactions.Sum(t => t.Quantity) != 0)
                        {
                            foreach (var group in transactions.GroupBy(g => g.Date))
                            {
                                var transaction = group.FirstOrDefault(f => f.IsAbsence());
                                if (transaction == null || transaction.IsAbsenceVacationNoVacationDaysDeducted())
                                    continue;

                                if (transaction.IsAbsenceSickOrWorkInjury())
                                {
                                    #region Sick

                                    scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalHeldagsfranvarandeSjukdomArbetsskada++;
                                    scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalHeldagsfranvarande++;
                                    if (anstalld.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    {
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeSjukdomArbetsskadaMan++;
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeMan++;
                                    }
                                    else if (anstalld.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    {
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeSjukdomArbetsskadaKvinnor++;
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeKvinnor++;
                                    }

                                    scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(anstalld, transactions, "TotaltAntalHeldagsfranvarandeSjukdomArbetsskada"));

                                    #endregion
                                }
                                else if (transaction.IsAbsenceVacation())
                                {
                                    #region Vacation

                                    scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalHeldagsfranvarandeSemester++;
                                    scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalHeldagsfranvarande++;
                                    if (anstalld.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    {
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeSemesterMan++;
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeMan++;
                                    }
                                    else if (anstalld.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    {
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeSemesterKvinnor++;
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeKvinnor++;
                                    }
                                    scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(anstalld, transactions, "TotaltAntalHeldagsfranvarandeSemester"));

                                    #endregion
                                }
                                else if (transaction.IsAbsence())
                                {
                                    #region Other absence

                                    scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalHeldagsfranvarandeOvrigFranvaro++;
                                    scb_KSPPayrollStatisticsFileHeadDTO.TotaltAntalHeldagsfranvarande++;
                                    if (anstalld.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    {
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeOvrigFranvaroMan++;
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeMan++;
                                    }
                                    else if (anstalld.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    {
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeOvrigFranvaroKvinnor++;
                                        scb_KSPPayrollStatisticsFileHeadDTO.AntalHeldagsfranvarandeKvinnor++;
                                    }
                                    scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(anstalld, transactions, "TotaltAntalHeldagsfranvarandeOvrigFranvaro"));

                                    #endregion
                                }
                            }
                        }
                    }
                }

                #endregion
            }

            foreach (Employee employee in employees)
            {
                #region Employee

                Employment firstEmployment = employee.GetFirstEmployment();
                if (firstEmployment != null)
                {
                    DateTime? startDate = firstEmployment.DateFrom;
                    if (startDate != null && startDate >= beginningOfMonth && startDate <= endOfMonth)
                    {
                        #region Employed
                        if (firstEmployment.GetEmploymentType(employmentTypes) != (int)TermGroup_EmploymentType.SE_Trainee)
                        {
                            SCBPayrollStatisticsTransactionDTO dto = new SCBPayrollStatisticsTransactionDTO()
                            {
                                ProductNr = string.Empty,
                                Type = "NyAnstallda",
                                EmployeeNr = employee.EmployeeNr,
                                Amount = 0,
                                Quantity = 0
                            };

                            scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.Add(dto);
                            scb_KSPPayrollStatisticsFileHeadDTO.NyAnstallda++;

                            if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaMan++;
                            else if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaKvinnor++;

                            int employmentType = firstEmployment.GetEmploymentType(employmentTypes);
                            if (employmentType == (int)TermGroup_EmploymentType.SE_Permanent)
                            {
                                scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaTillsvidareTotalt++;
                                if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaTillsvidareMan++;
                                else if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaTillsvidareKvinnor++;
                            }
                            else
                            {
                                if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Unknown && string.IsNullOrEmpty(employee.SocialSec))
                                    employee.ContactPerson.Sex = (int)CalendarUtility.GetSexFromSocialSecNr(employee.SocialSec);

                                scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaVissTidTotalt++;
                                if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaVissTidMan++;
                                else if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    scb_KSPPayrollStatisticsFileHeadDTO.NyAnstalldaVissTidKvinnor++;
                            }
                        }

                        #endregion
                    }
                }

                Employment lastEmployment = employee.GetLastEmployment();
                if (lastEmployment != null)
                {
                    DateTime? endDate = lastEmployment.GetEndDate();
                    if (endDate.HasValue && endDate.Value >= beginningOfMonth && endDate.Value <= endOfMonth)
                    {
                        #region Ended

                        if (firstEmployment.GetEmploymentType(employmentTypes) != (int)TermGroup_EmploymentType.SE_Trainee)
                        {
                            SCBPayrollStatisticsTransactionDTO dto = new SCBPayrollStatisticsTransactionDTO()
                            {
                                ProductNr = string.Empty,
                                Type = "Avgangna",
                                EmployeeNr = employee.EmployeeNr,
                                Amount = 0,
                                Quantity = 0
                            };

                            scb_KSPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsTransactionDTO.Add(dto);
                            scb_KSPPayrollStatisticsFileHeadDTO.Avgangna++;

                            if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaMan++;
                            else if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaKvinnor++;

                            if (lastEmployment.GetEmploymentType(employmentTypes) == (int)TermGroup_EmploymentType.SE_Permanent)
                            {
                                scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaTillsvidareTotalt++;
                                if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaTillsvidareMan++;
                                else if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaTillsvidareKvinnor++;
                            }
                            else
                            {
                                scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaVissTidTotalt++;
                                if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Male)
                                    scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaVissTidMan++;
                                else if (employee.ContactPerson.Sex == (int)TermGroup_Sex.Female)
                                    scb_KSPPayrollStatisticsFileHeadDTO.AvgangnaVissTidKvinnor++;
                            }
                        }

                        #endregion
                    }
                }

                #endregion
            }

            return scb_KSPPayrollStatisticsFileHeadDTO;
        }

        public SCB_KLPPayrollStatisticsFileHeadDTO CreateSCB_KLPPayrollStatisticsFileHeadDTO(CompEntities entities)
        {
            SCB_KLPPayrollStatisticsFileHeadDTO scb_KLPPayrollStatisticsFileHeadDTO = new SCB_KLPPayrollStatisticsFileHeadDTO
            {
                Date = DateTime.Now.Date,
                SCBPayrollStatisticsFileRowDTOs = new List<SCB_KLPPayrollStatisticsFileRowDTO>(),
            };

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out _, out _, out var selectedTimePeriods, alwaysLoadPeriods: true);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            List<PayrollGroup> payrollGroups = PayrollManager.GetPayrollGroups(entities, reportResult.ActorCompanyId, true, true, loadSettings: true);
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(entities, base.ActorCompanyId);
            List<TimePayrollStatisticsDTO> transactions = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds);

            int langId = GetLangId();
            Dictionary<int, string> loneFormDict = base.GetTermGroupDict(TermGroup.PayrollReportsSalaryType, langId, includeKey: true);
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

            #endregion

            foreach (Employee employee in employees)
            {
                #region Employee

                if (employee.PartnerInCloseCompany)
                    continue;

                SCB_KLPPayrollStatisticsFileRowDTO rowDTO = new SCB_KLPPayrollStatisticsFileRowDTO
                {
                    SCBPayrollStatisticsTransactionDTOs = new List<SCBPayrollStatisticsTransactionDTO>(),
                    EmployeeId = employee.EmployeeId,
                    EmployeeName = employee.Name,
                    EmployeeNr = employee.EmployeeNr,
                    PersonNr = showSocialSec ? employee.SocialSec : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                    Typ = "Okänd",
                    Period = "",
                };
                personalDataRepository.AddEmployeeSocialSec(employee);

                List<TimePayrollStatisticsDTO> employeeTransactions = transactions.Where(t => t.EmployeeId == employee.EmployeeId).OrderByDescending(t => t.TimeBlockDate).ToList();
                if (employeeTransactions.Any())
                {
                    #region SCB_KLPPayrollStatisticsFileRowDTO 

                    DateTime date = employeeTransactions.FirstOrDefault().TimeBlockDate;
                    Employment employment = employee.GetEmployment(date);
                    if (employment == null)
                        continue;

                    int employmentType = employment.GetEmploymentType(employmentTypes);
                    if (employmentType == (int)TermGroup_EmploymentType.SE_Trainee || employmentType == (int)TermGroup_EmploymentType.Unknown)
                        continue;

                    bool isWorker = false;
                    bool isTimavlonadTjansteman = false;
                    decimal fullTime = 0;

                    #region PayrollGroup

                    PayrollGroup payrollGroup = employment.GetPayrollGroup(date, payrollGroups);
                    if (payrollGroup != null)
                    {
                        rowDTO.Typ = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.PayrollReportsSalaryType)?.IntData.ToString() ?? rowDTO.Typ;
                        rowDTO.Loneform = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.PayrollReportsSalaryType)?.IntData.ToString() ?? rowDTO.Typ;
                        rowDTO.PersonalKategori = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.PayrollReportsPersonalCategory)?.IntData.ToString() ?? rowDTO.Typ;
                        rowDTO.ArbetsKategori = payrollGroup.PayrollGroupSetting?.FirstOrDefault(t => t.Type == (int)PayrollGroupSettingType.PayrollReportsWorkTimeCategory)?.IntData.ToString() ?? rowDTO.Typ;

                        string loneform = string.Empty;
                        if (Int32.TryParse(rowDTO.Loneform, out int loneFormId))
                            loneform = loneFormDict.GetValue(loneFormId);

                        var monthlyWorkTimeSetting = payrollGroup.PayrollGroupSetting?.FirstOrDefault(f => f.State == (int)SoeEntityState.Active && f.Type == (int)PayrollGroupSettingType.MonthlyWorkTime);
                        if (monthlyWorkTimeSetting?.DecimalData != null)
                            fullTime = monthlyWorkTimeSetting.DecimalData.Value;

                        // 1   Arbetare
                        // 2   Övriga (t.ex.tjänstemän)för Stål - och metallförbundet
                        // 3   Arbetare röda avtalet
                        // 4   Arbetare blå avtalet
                        // 5   Arbetare elavtalet
                        // 6   Medarbetare gröna avtalet
                        // 7   SIF
                        // 8   CF
                        // 9   LEDARNA
                        string personalKategori = string.Empty;
                        if (rowDTO.PersonalKategori.Equals("1") || rowDTO.PersonalKategori.Equals("3") || rowDTO.PersonalKategori.Equals("4") || rowDTO.PersonalKategori.Equals("5") || rowDTO.PersonalKategori.Equals("6"))
                        {
                            personalKategori = "Arbetare";
                            isWorker = true;
                        }
                        else
                        {
                            personalKategori = "Tjänstemän";
                            if (personalKategori == "Tjänstemän" && rowDTO.Loneform == "3")
                                isTimavlonadTjansteman = true;
                        }

                        //  1  Månadslön
                        //  2 Veckolön
                        //  3 Timlön
                        rowDTO.Typ = $"{personalKategori} - {loneform}";

                    }

                    #endregion

                    #region Employment

                    if (employment != null)
                    {
                        #region SSG

                        // Loop all employments for specified vacation year
                        decimal allssgs = 0;
                        decimal nrOfDays = 0;
                        List<DateTime> CheckedDates = new List<DateTime>();
                        foreach (var timePeriod in selectedTimePeriods.Where(t => selectionTimePeriodIds.Contains(t.TimePeriodId)))
                        {
                            DateTime fromDate = CalendarUtility.GetBeginningOfMonth(timePeriod.PaymentDate);
                            DateTime toDate = CalendarUtility.GetEndOfMonth(timePeriod.PaymentDate);

                            DateTime lookDate = fromDate;

                            while (lookDate <= toDate)
                            {
                                if (!CheckedDates.Any(c => c == lookDate))
                                {
                                    var loopEmployment = employee.GetEmployment(lookDate);
                                    if (loopEmployment != null)
                                    {
                                        allssgs += loopEmployment.GetPercent(lookDate);
                                    }

                                    nrOfDays++;
                                    CheckedDates.Add(lookDate);
                                }
                                lookDate = lookDate.AddDays(1);
                            }
                        }

                        decimal SSG = 0;
                        if (nrOfDays > 0)
                            SSG = Decimal.Divide(allssgs, nrOfDays);

                        #endregion

                        var bruttoloniPerioden = employeeTransactions.Where(t => t.IsGrossSalary());

                        // Utbetald lön för arbetade timmar före skatt och andra avdrag Summera den utbetalda lönen som betalats ut till timavlönade arbetare för månaden.
                        // Lönen som redovisas ska vara lön före skatt och innan eventuella avdrag. Lönen ska motsvara redovisat antal arbetade timmar.
                        // För timavlönade som erhåller lön med en månads fördröjning ska också det utbetalda beloppet redovisas med fördröjning.

                        //Ska ingå:  Tidlön, provision / prestations / premielön, fasta tillägg  Tillägg för obekväm och förskjuten arbetstid  
                        // Övertidsersättning(inklusive övertidstillägg)  Ackordstillägg, skifttillägg, skiftformstillägg, risktillägg, smutstillägg, väntetidstillägg, 
                        // gångtidstillägg och restidstillägg mellan två arbetsplatser 
                        // Ska inte ingå:  Semesterlön, semesterersättning eller annan ledighet med lön  Helglön, helgersättning för ej arbetad tid  Lönetillägg för annan period än redovisningsperioden  Kostnadsersättningar, 
                        // t.ex.traktamente eller verktygsersättning  Permitteringslön / avgångsvederlag  Sjuklön  Retroaktiv lön och engångsbelopp som betalas ut efter avslutade centrala eller lokala löneförhandlingar 
                        //  Bonus som inte stäms av och betalas ut månatligen

                        var utbetaldlon = bruttoloniPerioden.Where(t =>
                            !t.IsVacationSalary() &&
                            !t.IsVacationAddition() &&
                            !t.IsVacationAdditionVariable() &&
                            !t.IsVacationCompensation() &&
                            !t.IsGrossSalaryCarAllowanceFlat() &&
                            !t.IsGrossSalaryWeekendSalary() &&
                            !t.IsAbsence_SicknessSalary() &&
                            !t.IsGrossSalaryAllowanceStandard() &&
                            !t.IsGrossSalaryLayOffSalary() &&
                            !t.IsGrossSalaryRetroactive() &&
                            !t.IsGrossSalaryTravelTime() &&
                            !t.IsGrossSalaryEarnedHolidayPayment());

                        if (isWorker)
                            utbetaldlon = utbetaldlon.Where(t => !t.IsGrossSalaryCommision()).ToList();
                        rowDTO.Utbetaldlon = utbetaldlon.Sum(s => s.Amount);

                        if (utbetaldlon != null && utbetaldlon.Any())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, utbetaldlon.ToList(), "Utbetaldlon", merge: false));

                        // Därav övertidstillägg Redovisa utbetalt övertidstillägg.Övertidstillägget ska motsvara redovisade övertidstimmar.Observera att det är övertidstillägg som efterfrågas och inte övertidsersättning, 
                        // grundlönen för övertiden ska därmed inte inkluderas. Mer -/ fyllnadstid ska inte ingå.För timavlönade som erhåller övertidstillägg med en månads fördröjning ska också det utbetalda beloppet redovisas med fördröjning.

                        var daravOvertidstillagg = bruttoloniPerioden.Where(t => t.IsOverTimeAddition());
                        rowDTO.DaravOvertidstillagg = daravOvertidstillagg.Sum(t => t.Amount);

                        // Antal arbetade timmar Summera antalet faktiskt arbetade timmar, räkna med mertid, övertid och obekväm tid.Endast tid då arbete utförts ska ingå.Semester, sjukfrånvaro, vård av sjukt barn,
                        // jour -och beredskapstid etc. ska inte ingå.Timmarna ska motsvara den lönesumma som redovisas under ”Utbetald lön för arbetade timmar före skatt och andra avdrag”. För timavlönade som erhåller lön med en månads fördröjning ska också antal arbetade timmar redovisas med fördröjning.

                        var arbetadeTimmar = employeeTransactions.Where(t => (t.IsWorkTime() || t.IsAddedTime()) && !t.IsFromOtherPeriod).ToList();
                        arbetadeTimmar.AddRange(daravOvertidstillagg);
                        var permittering = employeeTransactions.Where(t => t.IsAbsencePayedAbsence() && !t.IsFromOtherPeriod).ToList();
                        rowDTO.ArbetadeTimmar = arbetadeTimmar.Sum(t => t.Quantity / 60) - permittering.Sum(t => t.Quantity / 60);
                        rowDTO.Permittering = permittering.Sum(t => t.Quantity / 60);

                        if (!arbetadeTimmar.IsNullOrEmpty())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, arbetadeTimmar.ToList(), "ArbetadeTimmar", merge: false));

                        if (!permittering.IsNullOrEmpty())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, permittering.ToList(), "Permittering", merge: false));

                        // Därav övertidstimmar Redovisa antalet utbetalda övertidstimmar. Timmarna ska motsvara redovisat övertidstillägg.För timavlönade som erhåller övertidstillägg 
                        // med en månads fördröjning ska också ”Därav överidstimmar” redovisas med fördröjning.Räkna inte med mer-/ fyllnadstid.Övertid som tas ut i ledighet eller sparas i ”övertidsbank” ska inte ingå, utan endast tas med i ”Antal arbetade timmar”.  

                        var daravOvertidstimmar = daravOvertidstillagg;
                        rowDTO.DaravOvertidstimmar = daravOvertidstimmar.Sum(t => t.Quantity / 60);

                        if (daravOvertidstimmar != null && daravOvertidstimmar.Any())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, daravOvertidstimmar.ToList(), "DaravOvertidstimmar", merge: false));

                        // Utbetald retroaktiv lön Summera den retroaktiva lönesumma som betalats ut för månaden. Summan ska endast avse den retroaktiva löneökning som betalats ut 
                        // efter avslutade löneförhandlingar.Andra typer av utbetalningar som sker i efterskott, t.ex.bonus och lönekorrigeringar ska redovisas under ”Rörliga tillägg och ersättningar avseende tidigare perioder”. 

                        var retroLon = employeeTransactions.Where(t => t.IsGrossSalaryRetroactive());
                        rowDTO.RetrolonFran = retroLon.OrderBy(o => o.TimeBlockDate.Date).FirstOrDefault()?.TimeBlockDate.Date;
                        rowDTO.RetrolonTill = retroLon.OrderByDescending(o => o.TimeBlockDate.Date).FirstOrDefault()?.TimeBlockDate.Date;
                        rowDTO.Retrolon = retroLon.Sum(t => t.Amount);

                        if (retroLon != null && retroLon.Any())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, retroLon.ToList(), "Retrolon", merge: false));

                        // Period som den retroaktiva utbetalningen avser Ange den tidsperiod som den retroaktiva lönen avser. Retroaktiv lön kan inte avse den aktuella månaden.  Exempel: Nya löner gäller från och med april.Nya lönenivåer 
                        // är inte klara till denna tidpunkt utan betalas först ut i samband med junilönen samtidigt som retroaktiv lön betalas ut. Perioden för retroaktiv lön blir april – maj.

                        DateTime? firstDate = retroLon.FirstOrDefault()?.TimeBlockDate;
                        DateTime? lastDate = retroLon.LastOrDefault()?.TimeBlockDate;
                        string period = "";

                        if (firstDate.HasValue && lastDate.HasValue)
                        {
                            period = $"{firstDate.Value.ToShortDateString()} - {lastDate.Value.ToShortDateString()}";
                        }
                        rowDTO.Period = period;

                        // Rörliga tillägg 
                        List<int> retroIds = retroLon.Select(r => r.TimePayrollTransactionId).ToList();
                        var overtimeIds = daravOvertidstillagg.Select(s => s.TimePayrollTransactionId);
                        var rorligaTillagg = bruttoloniPerioden.Where(t => !t.IsFromOtherPeriod && !overtimeIds.Contains(t.TimePayrollTransactionId) && !retroIds.Contains(t.TimePayrollTransactionId) &&
                            !t.IsVacationSalary() &&
                            !t.IsVacationAddition() &&
                            !t.IsVacationAdditionVariable() &&
                            !t.IsVacationCompensation() &&
                            !t.IsGrossSalaryCarAllowanceFlat() &&
                            !t.IsGrossSalaryWeekendSalary() &&
                            !t.IsAbsence_SicknessSalary() &&
                            !t.IsGrossSalaryAllowanceStandard() &&
                            !t.IsGrossSalaryLayOffSalary() &&
                            !t.IsGrossSalaryRetroactive() &&
                            !t.IsGrossSalaryTravelTime() &&
                            !t.IsGrossSalaryEarnedHolidayPayment() &&
                            !t.IsMonthlySalary() &&
                            !t.IsAbsence());

                        rowDTO.RorligaTillagg = rorligaTillagg.Sum(t => t.Amount);
                        if (rorligaTillagg != null && rorligaTillagg.Any())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, rorligaTillagg.ToList(), "RorligaTillagg", merge: false));

                        // Rörliga tillägg och ersättningar avseende tidigare perioder Summera alla utbetalda rörliga lönetillägg som på grund av tidsförskjutningar, exempelvis långa ackord, inte kunnat redovisas vid tidigare redovisningstillfällen. 
                        // Ersättningar som avser fler månader än aktuell månad ska även ingå, exempelvis kvartalsbonus. Här ska även lönekorrigeringar som gjorts som inte är till följd av nya löneavtal redovisas.Retroaktiva löner som betalas ut efter avslutade löneförhandlingar ska redovisas under ”Utbetald retroaktiv lön”.  
                        var rorligaTillaggTidigarePerioder = bruttoloniPerioden.Where(t => t.IsFromOtherPeriod && !retroIds.Contains(t.TimePayrollTransactionId) &&
                            !t.IsVacationSalary() &&
                            !t.IsVacationAddition() &&
                            !t.IsVacationAdditionVariable() &&
                            !t.IsVacationCompensation() &&
                            !t.IsGrossSalaryCarAllowanceFlat() &&
                            !t.IsGrossSalaryWeekendSalary() &&
                            !t.IsAbsence_SicknessSalary() &&
                            !t.IsGrossSalaryAllowanceStandard() &&
                            !t.IsGrossSalaryLayOffSalary() &&
                            !t.IsGrossSalaryRetroactive() &&
                            !t.IsGrossSalaryTravelTime() &&
                            !t.IsGrossSalaryEarnedHolidayPayment());

                        rowDTO.RorligaTillaggTidigarePerioder = rorligaTillaggTidigarePerioder.Sum(t => t.Amount);
                        if (rorligaTillaggTidigarePerioder != null && rorligaTillaggTidigarePerioder.Any())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, rorligaTillaggTidigarePerioder.ToList(), "RorligaTillaggTidigarePerioder", merge: false));

                        //Sjuklön Summera den utbetalda sjuklönen som avser månaden. Sjuklönen ska avse de första 14 dagarna, dvs.arbetsgivarperioden.Observera att det är sjuklön som efterfrågas, inte sjuklöneavdrag. 
                        //måste dock dra av karensavdraget

                        var sjuklon = bruttoloniPerioden.Where(t => t.IsAbsence_SicknessSalary_Day2_14() && !t.IsFromOtherPeriod).ToList();
                        sjuklon.AddRange(bruttoloniPerioden.Where(t => t.IsAbsence_SicknessSalary_Deduction() && !t.IsFromOtherPeriod).ToList());
                        rowDTO.Sjuklon = sjuklon.Sum(t => t.Amount);

                        if (sjuklon != null && sjuklon.Any())
                            rowDTO.SCBPayrollStatisticsTransactionDTOs.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, sjuklon.ToList(), "Sjuklon", merge: false));

                        // Tjänstemän och månadsavlönade
                        // Antal anställda
                        // Redovisa antalet anställda timavlönade arbetare som arbetat och fått utbetald lön för månaden.
                        // Eventuella personer som finns med i anställningsregistret men som inte arbetat och fått utbetald lön ska inte ingå. 

                        rowDTO.Sysselsattningsgrad = SSG;

                        // Tjänstemän och månadsavlönade
                        // Överenskommen månadslön inklusive fasta tillägg Summera de anställdas överenskomna månadslöner (lön enligt anställningsavtal). Räkna även med frånvarande, t.ex.sjukskrivna och tjänstlediga.För deltidsanställda ska deltidslönen anges.
                        // Ta med fasta tillägg som är knutna till befattning eller individ.Lönen som redovisas ska vara lönen före skatt och innan eventuella avdrag. Ta t.ex.inte med semestertillägg, övertidstillägg, restidsersättning, jour - och beredskapsersättning.
                        // Överenskommen månadslön ska vara lika mellan månaderna om det inte sker förändringar av personalstrukturen och / eller av anställningsavtal

                        rowDTO.OverenskommenManadslon = 0;

                        var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                        if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                        {
                            PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, reportResult.ActorCompanyId, employee, employment, null, date, null, null, setting.IntData.Value);
                            if (result != null)
                                rowDTO.OverenskommenManadslon = result.Amount;
                        }

                        // Tjänstemän och månadsavlönade
                        // Antal avtalade timmar Summera de anställdas avtalade arbetstimmar enligt anställningsavtal. Räkna även med frånvarande, t.ex.sjukskrivna och tjänstlediga.Med avtalade arbetstimmar avses normal arbetstid helgfria veckor 
                        // enligt anställningsavtal. För deltidsanställda ska avtalade timmar enligt deltidstjänstgöringen anges.  Timmarna ska vara lika mellan månaderna om det inte sker förändringar av personalstrukturen och/ eller av 
                        // anställningsavtal. Antalet arbetsdagar under månaden ska inte påverka redovisningen. Räkna med att varje månad innehåller 4,3 veckor, dvs.summa avtalade arbetstimmar per månad = veckoarbetstid(för samtliga) x 4,3.

                        var weekMinutes = employment.GetWorkTimeWeek();
                        rowDTO.AvtaladeTimmar = Decimal.Round(Decimal.Multiply(Decimal.Divide(weekMinutes, 60), Convert.ToDecimal(4.3)), 2);
                    }

                    #endregion

                    if (isTimavlonadTjansteman)
                    {
                        rowDTO.AvtaladeTimmar = rowDTO.AvtaladeTimmar - rowDTO.DaravOvertidstimmar;
                        rowDTO.Sysselsattningsgrad = decimal.Multiply(100, decimal.Divide(rowDTO.ArbetadeTimmar, (fullTime != 0 ? fullTime : 173)));
                        rowDTO.DaravOvertidstimmar = 0;
                        rowDTO.OverenskommenManadslon = rowDTO.Utbetaldlon;
                    }

                    scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Add(rowDTO);

                    #endregion
                }

                #endregion
            }

            #region Sums

            scb_KLPPayrollStatisticsFileHeadDTO.ArbetadeTimmar = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.ArbetadeTimmar);
            scb_KLPPayrollStatisticsFileHeadDTO.Permittering = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.Permittering);
            scb_KLPPayrollStatisticsFileHeadDTO.AvtaladeTimmar = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.AvtaladeTimmar);
            scb_KLPPayrollStatisticsFileHeadDTO.DaravOvertidstillagg = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.DaravOvertidstillagg);
            scb_KLPPayrollStatisticsFileHeadDTO.DaravOvertidstimmar = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.DaravOvertidstimmar);
            scb_KLPPayrollStatisticsFileHeadDTO.OverenskommenManadslon = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.OverenskommenManadslon);
            scb_KLPPayrollStatisticsFileHeadDTO.Retrolon = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.Retrolon);
            scb_KLPPayrollStatisticsFileHeadDTO.RorligaTillagg = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.RorligaTillagg);
            scb_KLPPayrollStatisticsFileHeadDTO.RorligaTillaggTidigarePerioder = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.RorligaTillaggTidigarePerioder);
            scb_KLPPayrollStatisticsFileHeadDTO.Sjuklon = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.Sjuklon);
            scb_KLPPayrollStatisticsFileHeadDTO.Utbetaldlon = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.Utbetaldlon);
            scb_KLPPayrollStatisticsFileHeadDTO.NumberOfEmployees = scb_KLPPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Sum(s => s.Sysselsattningsgrad) / 100;
            var manad = selectedTimePeriods.OrderBy(o => o.PaymentDate).FirstOrDefault();
            scb_KLPPayrollStatisticsFileHeadDTO.UtbetalningsManad = manad != null ? (manad.PaymentDate.Value.Year + manad.PaymentDate.Value.Month.ToString().PadLeft(2, '0')) : "";

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return scb_KLPPayrollStatisticsFileHeadDTO;
        }

        public SCB_KSJUPayrollStatisticsFileHeadDTO CreateSCB_KSJUPayrollStatisticsFileHeadDTO(CompEntities entities, List<Employee> employees, DateTime selectionDateFrom, DateTime selectionDateTo)
        {
            var scb_KSJUPayrollStatisticsFileHeadDTO = new SCB_KSJUPayrollStatisticsFileHeadDTO
            {
                SCB_KSJUPayrollStatisticsFileRowDTOs = new List<SCB_KSJUPayrollStatisticsFileRowDTO>()
            };

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            entities.CommandTimeout = 300;
            var transactionsOnCompany = entities.TimePayrollTransaction.Where(w => w.ActorCompanyId == reportResult.ActorCompanyId && w.TimeBlockDate.Date >= selectionDateFrom && w.TimeBlockDate.Date <= selectionDateTo && w.State == (int)SoeEntityState.Active).ToList();
            if (!transactionsOnCompany.Any())
                return scb_KSJUPayrollStatisticsFileHeadDTO;

            transactionsOnCompany = transactionsOnCompany.Where(a => a.IsAbsence_SicknessSalary_Day2_14() || a.IsAbsenceSickDayQualifyingDay()).ToList();
            if (!transactionsOnCompany.Any())
                return scb_KSJUPayrollStatisticsFileHeadDTO;

            List<int> employeeIds = transactionsOnCompany.Select(s => s.EmployeeId).Distinct().ToList();
            employees = employees.Where(w => employeeIds.Contains(w.EmployeeId)).ToList();

            #endregion

            if (employees.Any())
            {
                Company company = CompanyManager.GetCompany(entities, reportResult.ActorCompanyId);
                List<HolidayDTO> companyHolidays = CalendarManager.GetHolidaysByCompany(entities, reportResult.ActorCompanyId);
                List<AccountDimDTO> companyAccountDims = AccountManager.GetAccountDimsByCompany(entities, reportResult.ActorCompanyId).ToDTOs();
                bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);

                foreach (Employee employee in employees)
                {
                    #region Employee

                    #region Unmerged

                    List<SCB_KSJUPayrollStatisticsFileRowDTO> unmerged = new List<SCB_KSJUPayrollStatisticsFileRowDTO>();

                    var input = GetAttestEmployeeInput.CreateAttestInputForWeb(reportResult.ActorCompanyId, parameterObject.UserId, 0, employee.EmployeeId, selectionDateFrom, selectionDateTo);
                    input.SetOptionalParameters(companyHolidays, companyAccountDims);
                    var items = TimeTreeAttestManager.GetAttestEmployeeDays(input);
                    foreach (var item in items)
                    {
                        #region SCB_KSJUPayrollStatisticsFileRowDTO

                        SCB_KSJUPayrollStatisticsFileRowDTO rowDTO = new SCB_KSJUPayrollStatisticsFileRowDTO();

                        var sickTransactions = item.AttestPayrollTransactions.Where(a => a.IsAbsence_SicknessSalary_Day2_14() || a.IsAbsenceSickDayQualifyingDay()).ToList();
                        if (!sickTransactions.Any())
                            continue;

                        var helAntDagar = 0;
                        var delAntDagar = 0;
                        var scheduleTime = (int)item.ScheduleTime.TotalMinutes;
                        if (scheduleTime != 0)
                        {
                            if (sickTransactions.Sum(t => t.Quantity) >= scheduleTime)
                                helAntDagar++;
                            else
                                delAntDagar++;
                        }

                        rowDTO.EmployeeId = employee.EmployeeId;
                        rowDTO.EmployeeNr = employee.EmployeeNr;
                        rowDTO.EmployeeName = employee.Name;
                        rowDTO.PersonNr = showSocialSec ? StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec);
                        rowDTO.PeOrgNr = OrgNrWith16(company.OrgNr);
                        rowDTO.SjukFrom = item.Date;
                        rowDTO.SjukTom = item.Date;
                        rowDTO.HelAntDagar = helAntDagar;
                        rowDTO.DelAntDagar = delAntDagar;
                        rowDTO.AntDagar = helAntDagar + delAntDagar;
                        rowDTO.Cfar = employee.PayrollStatisticsCFARNumber.HasValue ? employee.PayrollStatisticsCFARNumber.Value.ToString() : string.Empty;
                        rowDTO.Korrigeringsuppgift = false;

                        personalDataRepository.AddEmployeeSocialSec(employee);
                        unmerged.Add(rowDTO);

                        #endregion

                    }

                    if (!unmerged.Any())
                        continue;

                    #endregion

                    #region Merge

                    DateTime? lastDate = null;
                    DateTime endDate = unmerged.OrderBy(u => u.SjukFrom).LastOrDefault().SjukFrom;
                    SCB_KSJUPayrollStatisticsFileRowDTO mergedRowDTO = new SCB_KSJUPayrollStatisticsFileRowDTO();

                    foreach (var item in unmerged.OrderBy(u => u.SjukFrom).ToList())
                    {
                        if (lastDate == null || mergedRowDTO == null)
                        {
                            mergedRowDTO = CloneSCB_KSJUPayrollStatisticsFileRowDTO(item);
                            lastDate = item.SjukFrom;
                            if (unmerged.Count == 1)
                                scb_KSJUPayrollStatisticsFileHeadDTO.SCB_KSJUPayrollStatisticsFileRowDTOs.Add(CloneSCB_KSJUPayrollStatisticsFileRowDTO(mergedRowDTO));
                            continue;
                        }

                        if (lastDate == item.SjukFrom.AddDays(-1))
                        {
                            mergedRowDTO.SjukTom = item.SjukTom;
                            mergedRowDTO.HelAntDagar += item.HelAntDagar;
                            mergedRowDTO.DelAntDagar += item.DelAntDagar;
                            mergedRowDTO.AntDagar += item.AntDagar;
                            lastDate = item.SjukFrom;

                            if (item.SjukFrom == endDate)
                                scb_KSJUPayrollStatisticsFileHeadDTO.SCB_KSJUPayrollStatisticsFileRowDTOs.Add(CloneSCB_KSJUPayrollStatisticsFileRowDTO(mergedRowDTO));
                        }
                        else
                        {
                            scb_KSJUPayrollStatisticsFileHeadDTO.SCB_KSJUPayrollStatisticsFileRowDTOs.Add(CloneSCB_KSJUPayrollStatisticsFileRowDTO(mergedRowDTO));
                            mergedRowDTO = CloneSCB_KSJUPayrollStatisticsFileRowDTO(item);
                            lastDate = item.SjukFrom;
                        }
                    }

                    #endregion

                    #endregion
                }
            }

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return scb_KSJUPayrollStatisticsFileHeadDTO;
        }

        public SCB_SLPPayrollStatisticsFileHeadDTO CreateSCB_SLPPayrollStatisticsFileHeadDTO(CompEntities entities, List<Employee> employees, bool isSCB, DateTime selectionDateFrom, DateTime selectionDateTo, List<int> selectionTimePeriodIds, StatisticFileType statisticFileType = StatisticFileType.none)
        {
            var scbPayrollStatisticsFileHeadDTO = new SCB_SLPPayrollStatisticsFileHeadDTO
            {
                SCBPayrollStatisticsFileRowDTOs = new List<SCB_SLPPayrollStatisticsFileRowDTO>()
            };

            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            isSCB = isSCB || statisticFileType == StatisticFileType.SCB;
            DateTime date = selectionDateTo;
            bool showSocialSec = FeatureManager.HasRolePermission(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, Permission.Readonly, reportResult.RoleId, reportResult.ActorCompanyId);
            LogCollector.LogCollector.LogInfo($"SCB_SLPPayrollStatisticsFileHeadDTO Start interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} actorCompanyId {reportResult.ActorCompanyId} number of employees {employees.Count}");
            List<TimePayrollStatisticsDTO> timePayrollTransactionItems = TimeTransactionManager.GetTimePayrollStatisticsDTOs(entities, reportResult.ActorCompanyId, employees, selectionTimePeriodIds);
            LogCollector.LogCollector.LogInfo($"SCB_SLPPayrollStatisticsFileHeadDTO End interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} timePayrollTransactionItems {timePayrollTransactionItems.Count} actorCompanyId {reportResult.ActorCompanyId}");
            List<VacationGroup> vacationGroups = PayrollManager.GetVacationGroups(reportResult.ActorCompanyId);
            Company company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            List<int> companyExportSettings = new List<int>
            {
                (int)CompanySettingType.PayrollExportSNKFOMemberNumber,
                (int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber,
                (int)CompanySettingType.PayrollExportSNKFOAffiliateNumber,
                (int)CompanySettingType.PayrollExportSNKFOAgreementNumber,
                (int)CompanySettingType.PayrollExportCommunityCode,
                (int)CompanySettingType.PayrollExportSCBWorkSite,
                (int)CompanySettingType.PayrollExportCFARNumber
            };

            Dictionary<int, object> dictCompanySettings = SettingManager.GetUserCompanySettings(SettingMainType.Company, companyExportSettings, 0, company.ActorCompanyId, 0);
            string period = selectionDateFrom.Year.ToString();
            string delagarnummer = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOMemberNumber);
            string arbetsplatsnummer = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOWorkPlaceNumber);
            string forbundsnummer = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOAffiliateNumber);
            string avtalskod = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOAgreementNumber);
            using var entitiesReadonly = CompEntitiesProvider.LeaseReadOnlyContext();
            var payrollProducts = base.GetPayrollProductsFromCache(entitiesReadonly, CacheConfig.Company(this.ActorCompanyId));
            var payedProducts = payrollProducts.Where(w => w.Payed).Select(s => s.ProductId).ToList();
            bool additionLevel3 = false;

            if (statisticFileType == StatisticFileType.Fremia && payrollProducts.Any())
            {
                additionLevel3 = payrollProducts.ToList().Any(w =>
                 w.SysPayrollTypeLevel1 == (int)TermGroup_SysPayrollType.SE_GrossSalary &&
                 w.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime &&
                 (w.SysPayrollTypeLevel2 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Compensation || w.SysPayrollTypeLevel3 == (int)TermGroup_SysPayrollType.SE_GrossSalary_AdditionalTime_Addition));
            }

            #endregion

            int employeeCounter = 0;

            foreach (Employee employee in employees)
            {
                #region Employee

                Employment employment = employee.GetEmployment(date);
                if (employment == null)
                    continue;

                employeeCounter++;

                if (employeeCounter % 20 == 0)
                    LogCollector.LogCollector.LogInfo($"SCB_SLPPayrollStatisticsFileHeadDTO actorCompanyId {reportResult.ActorCompanyId} EmployeeCounter: {employeeCounter}/{employees.Count}");

                List<EmployeePosition> employeePositions = EmployeeManager.GetEmployeePositions(entities, employee.EmployeeId, loadSysPosition: true);
                EmployeePosition defaultEmployeePosition = null;
                if (employeePositions.Any(w => w.Default))
                    defaultEmployeePosition = employeePositions.FirstOrDefault(f => f.Default);

                PayrollGroup payrollGroup = employment.GetPayrollGroup(date);

                #region Get values from Group

                string groupPersonalkategori = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PayrollReportsPersonalCategory))?.ToString();
                string groupArbetstidsart = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PayrollReportsWorkTimeCategory))?.ToString();
                string groupLonefom = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PayrollReportsSalaryType))?.ToString();
                string groupForbundsnummer = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.PartnerNumber))?.ToString();
                string groupAvtalskod = PayrollManager.GetPayrollGroupSettingValue(PayrollManager.GetPayrollGroupSetting(payrollGroup.PayrollGroupId, PayrollGroupSettingType.AgreementCode))?.ToString();

                // 1   Arbetare
                // 2   Övriga (t.ex.tjänstemän)för Stål - och metallförbundet
                // 3   Arbetare röda avtalet
                // 4   Arbetare blå avtalet
                // 5   Arbetare elavtalet
                // 6   Medarbetare gröna avtalet
                // 7   SIF
                // 8   CF
                // 9   LEDARNA

                if (string.IsNullOrEmpty(groupPersonalkategori))
                    continue;

                if (groupPersonalkategori.Equals("1") ||
                    groupPersonalkategori.Equals("3") ||
                    groupPersonalkategori.Equals("4") ||
                    groupPersonalkategori.Equals("5") ||
                    groupPersonalkategori.Equals("6"))
                {
                    groupPersonalkategori = "1";
                }
                else
                    groupPersonalkategori = "2";

                if (!string.IsNullOrEmpty(groupForbundsnummer))
                    forbundsnummer = groupForbundsnummer;

                if (!string.IsNullOrEmpty(groupAvtalskod))
                    avtalskod = groupAvtalskod;

                #endregion

                #region Get values from Employee

                string employeePersonalkategori = employee.PayrollStatisticsPersonalCategory.HasValue ? employee.PayrollStatisticsPersonalCategory.Value.ToString() : string.Empty;
                string employeeArbetstidsart = employee.PayrollStatisticsWorkTimeCategory.HasValue ? employee.PayrollStatisticsWorkTimeCategory.Value.ToString() : string.Empty;
                string employeeLoneform = employee.PayrollStatisticsSalaryType.HasValue ? employee.PayrollStatisticsSalaryType.Value.ToString() : string.Empty;
                string employeeArbetsplatsnummer = employee.PayrollStatisticsWorkPlaceNumber.HasValue && employee.PayrollStatisticsWorkPlaceNumber != 0 ? employee.PayrollStatisticsWorkPlaceNumber.ToString() : arbetsplatsnummer;
                string employeeCfarnummer = employee.PayrollStatisticsCFARNumber.HasValue ? employee.PayrollStatisticsCFARNumber.Value.ToString() : string.Empty;

                #endregion

                #region SCB_SLPPayrollStatisticsFileRowDTO

                var scbPayrollStatisticsFileRowDTO = new SCB_SLPPayrollStatisticsFileRowDTO
                {
                    Period = period,
                    Delagarnummer = isSCB ? string.Empty : delagarnummer,
                    Arbetsplatsnummer = isSCB ? string.Empty : employeeArbetsplatsnummer,
                    Organisationsnummer = OrgNrWithout16(company.OrgNr),
                    Forbundsnummer = isSCB ? string.Empty : forbundsnummer,
                    Avtalskod = isSCB ? string.Empty : avtalskod,
                    Personnummer = showSocialSec ? StringUtility.SocialSecYYYYMMDDXXXX(employee.SocialSec) : StringUtility.SocialSecYYMMDD_Dash_Stars(employee.SocialSec),
                    Anstnummer = employee.EmployeeNr,
                    Namn = employee.Name,
                    Personalkategori = !(string.IsNullOrEmpty(employeePersonalkategori) || employeePersonalkategori == "0") ? employeePersonalkategori : groupPersonalkategori,
                    Arbetstidsart = !string.IsNullOrEmpty(employeeArbetstidsart) ? employeeArbetstidsart : groupArbetstidsart,
                    Yrkeskod = defaultEmployeePosition != null && defaultEmployeePosition.Position.SysPositionCode != null ? defaultEmployeePosition.Position.SysPositionCode : String.Empty,
                    Forbundsspecifikkod = string.Empty,
                    Loneform = !string.IsNullOrEmpty(employeeLoneform) && employeeLoneform != "0" ? employeeLoneform : groupLonefom,
                    AntalanstalldaCFARnr = isSCB ? string.Empty : employees.Count.ToString(),
                    CFARnummer = employeeCfarnummer.Replace("-", ""),
                    Helglon = "0",
                    Reserv = "000000"
                };

                var loneform = !string.IsNullOrEmpty(employeeLoneform) ? employeeLoneform : groupLonefom;
                bool isHourlyPay = loneform != null && loneform.Equals("3");


                if (statisticFileType != StatisticFileType.none)
                {
                    switch (statisticFileType)
                    {
                        case StatisticFileType.SCB:
                            SetValueForSCB(entities, reportResult.ActorCompanyId, scbPayrollStatisticsFileRowDTO, employee, employment, date, isHourlyPay, timePayrollTransactionItems.Where(e => e.EmployeeId == employee.EmployeeId).ToList(), vacationGroups, payedProducts);
                            break;
                        case StatisticFileType.SN:
                            SetValueForSN(entities, reportResult.ActorCompanyId, scbPayrollStatisticsFileRowDTO, employee, employment, date, isHourlyPay, timePayrollTransactionItems.Where(e => e.EmployeeId == employee.EmployeeId).ToList(), vacationGroups, payedProducts);
                            break;
                        case StatisticFileType.Fremia:
                            SetValueForFremia(entities, reportResult.ActorCompanyId, scbPayrollStatisticsFileRowDTO, employee, employment, date, isHourlyPay, timePayrollTransactionItems.Where(e => e.EmployeeId == employee.EmployeeId).ToList(), vacationGroups, payedProducts, additionLevel3);
                            break;
                    }
                }
                else
                {
                    if (isSCB)
                        SetValueForSCB(entities, reportResult.ActorCompanyId, scbPayrollStatisticsFileRowDTO, employee, employment, date, isHourlyPay, timePayrollTransactionItems.Where(e => e.EmployeeId == employee.EmployeeId).ToList(), vacationGroups, payedProducts);
                    else
                        SetValueForSN(entities, reportResult.ActorCompanyId, scbPayrollStatisticsFileRowDTO, employee, employment, date, isHourlyPay, timePayrollTransactionItems.Where(e => e.EmployeeId == employee.EmployeeId).ToList(), vacationGroups, payedProducts);
                }
                personalDataRepository.AddEmployeeSocialSec(employee);
                scbPayrollStatisticsFileHeadDTO.SCBPayrollStatisticsFileRowDTOs.Add(scbPayrollStatisticsFileRowDTO);

                #endregion

                #endregion
            }

            LogCollector.LogCollector.LogInfo($"SCB_SLPPayrollStatisticsFileHeadDTO End interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} actorCompanyId {reportResult.ActorCompanyId} number of employees {employees.Count}");

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return scbPayrollStatisticsFileHeadDTO;
        }

        public List<SCBPayrollStatisticsTransactionDTO> CreateSCB_PayrollStatisticsTransactionDTO(Employee employee, List<TimePayrollStatisticsDTO> timePayrollTransactionItems, string type, bool merge = true)
        {
            List<SCBPayrollStatisticsTransactionDTO> dtos = new List<SCBPayrollStatisticsTransactionDTO>();
            List<SCBPayrollStatisticsTransactionDTO> mergedDtos = new List<SCBPayrollStatisticsTransactionDTO>();

            foreach (var item in timePayrollTransactionItems)
            {
                SCBPayrollStatisticsTransactionDTO dto = new SCBPayrollStatisticsTransactionDTO()
                {
                    ProductNr = item.PayrollProductNumber,
                    Date = item.TimeBlockDate,
                    Type = type,
                    EmployeeNr = employee.EmployeeNr,
                    Amount = item.Amount,
                    Quantity = item.Quantity,
                    Name = item.PayrollProductName,
                    SysPayrollTypeLevel1 = item.SysPayrollTypeLevel1,
                    SysPayrollTypeLevel2 = item.SysPayrollTypeLevel2,
                    SysPayrollTypeLevel3 = item.SysPayrollTypeLevel3,
                    SysPayrollTypeLevel4 = item.SysPayrollTypeLevel4,
                };

                dtos.Add(dto);
            }
            if (merge)
            {
                foreach (var group in dtos.GroupBy(t => t.ProductNr))
                {
                    SCBPayrollStatisticsTransactionDTO dto = new SCBPayrollStatisticsTransactionDTO()
                    {
                        ProductNr = group.FirstOrDefault().ProductNr,
                        Name = group.FirstOrDefault().Name,
                        Type = type,
                        EmployeeNr = employee.EmployeeNr,
                        Amount = group.Sum(a => a.Amount),
                        Quantity = group.Sum(a => a.Quantity),
                    };

                    mergedDtos.Add(dto);
                }

                return mergedDtos;
            }
            else
                return dtos;
        }

        public List<SCBPayrollStatisticsTransactionDTO> CreateSCB_PayrollStatisticsTransactionDTO(Employee employee, List<AttestPayrollTransactionDTO> attestPayrollTransactionDTOs, string type)
        {
            List<TimePayrollStatisticsDTO> timePayrollTransactionItems = new List<TimePayrollStatisticsDTO>();

            foreach (var item in attestPayrollTransactionDTOs)
            {
                TimePayrollStatisticsDTO timePayrollTransactionItem = new TimePayrollStatisticsDTO()
                {
                    PayrollProductNumber = item.PayrollProductNumber,
                    Amount = item.Amount ?? 0,
                    Quantity = item.Quantity
                };

                timePayrollTransactionItems.Add(timePayrollTransactionItem);
            }

            return CreateSCB_PayrollStatisticsTransactionDTO(employee, timePayrollTransactionItems, type);
        }

        public string CreateSCBKLPFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Prereq

            Company company = CompanyManager.GetCompany(reportResult.ActorCompanyId);
            var dto = CreateSCB_KLPPayrollStatisticsFileHeadDTO(entities);

            #endregion

            #region Create file

            string fileName = IOUtil.FileNameSafe(company.Name + "_SCB_KLP_" + CalendarUtility.ToFileFriendlyDateTime(dto.Date) + "_" + CalendarUtility.ToFileFriendlyDateTime(DateTime.Now));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                var sb = new StringBuilder();
                //TODO

                sb.Append("datum;" + dto.Date.ToString("yyyyMMdd") + Environment.NewLine);
                sb.Append("System;SoftOne GO" + Environment.NewLine);
                sb.Append("Version;1" + Environment.NewLine);
                sb.Append("OrgNummer;" + StringUtility.OrgNrWithout16(company.OrgNr) + Environment.NewLine);
                sb.Append("UtbManad;" + dto.UtbetalningsManad + Environment.NewLine);
                sb.Append("ATFinns;" + (dto.TimAvlonade.NumberOfEmployees > 0 ? "1" : "2") + Environment.NewLine);
                sb.Append("AMFinns;" + (dto.ManAvlonade.NumberOfEmployees > 0 ? "1" : "2") + Environment.NewLine);
                sb.Append("TMTFinns;" + (dto.TjanstemanAvlonade.NumberOfEmployees > 0 ? "1" : "2") + Environment.NewLine);
                sb.Append("AtUtbLon;" + Convert.ToInt32(dto.TimAvlonade.Utbetaldlon) + Environment.NewLine);
                sb.Append("AtOvtTlg;" + Convert.ToInt32(dto.TimAvlonade.DaravOvertidstillagg) + Environment.NewLine);
                sb.Append("AtArbTim;" + Convert.ToInt32(dto.TimAvlonade.ArbetadeTimmar) + Environment.NewLine);
                sb.Append("AtOvtTim;" + Convert.ToInt32(dto.TimAvlonade.DaravOvertidstimmar) + Environment.NewLine);
                sb.Append("AtRetLonS;" + Convert.ToInt32(dto.TimAvlonade.Retrolon) + Environment.NewLine);
                sb.Append("AtRetLonF;" + (dto.TimAvlonade.RetrolonFran.HasValue ? dto.TimAvlonade.RetrolonFran.Value.ToString("yyyyMMdd") : "") + Environment.NewLine);
                sb.Append("AtRetLonT;" + (dto.TimAvlonade.RetrolonTill.HasValue ? dto.TimAvlonade.RetrolonTill.Value.ToString("yyyyMMdd") : "") + Environment.NewLine);
                sb.Append("AtTidpLon;" + Convert.ToInt32(dto.TimAvlonade.RorligaTillaggTidigarePerioder) + Environment.NewLine);
                sb.Append("AtSjukLon;" + Convert.ToInt32(dto.TimAvlonade.Sjuklon) + Environment.NewLine);
                sb.Append("AtAnt;" + Convert.ToInt32(dto.TimAvlonade.NumberOfEmployees) + Environment.NewLine);
                sb.Append("AmManLon;" + Convert.ToInt32(dto.ManAvlonade.OverenskommenManadslon) + Environment.NewLine);
                sb.Append("AmAvtTim;" + Convert.ToInt32(dto.ManAvlonade.AvtaladeTimmar) + Environment.NewLine);
                sb.Append("AmRorLon;" + Convert.ToInt32(dto.ManAvlonade.RorligaTillagg) + Environment.NewLine);
                sb.Append("AmOvtTlg;" + Convert.ToInt32(dto.ManAvlonade.DaravOvertidstillagg) + Environment.NewLine);
                sb.Append("AmArbTim;" + Convert.ToInt32(dto.ManAvlonade.ArbetadeTimmar) + Environment.NewLine);
                sb.Append("AmOvtTim;" + Convert.ToInt32(dto.ManAvlonade.DaravOvertidstimmar) + Environment.NewLine);
                sb.Append("AmRetLonS;" + Convert.ToInt32(dto.ManAvlonade.Retrolon) + Environment.NewLine);
                sb.Append("AmRetLonF;" + (dto.ManAvlonade.RetrolonFran.HasValue ? dto.ManAvlonade.RetrolonFran.Value.ToString("yyyyMMdd") : "") + Environment.NewLine);
                sb.Append("AmRetLonT;" + (dto.ManAvlonade.RetrolonTill.HasValue ? dto.ManAvlonade.RetrolonTill.Value.ToString("yyyyMMdd") : "") + Environment.NewLine);
                sb.Append("AmTidpLon;" + Convert.ToInt32(dto.ManAvlonade.RorligaTillaggTidigarePerioder) + Environment.NewLine);
                sb.Append("AmSjukLon;" + Convert.ToInt32(dto.ManAvlonade.Sjuklon) + Environment.NewLine);
                sb.Append("AmAnt;" + Convert.ToInt32(dto.ManAvlonade.NumberOfEmployees) + Environment.NewLine);
                sb.Append("TmTrAntH;" + (dto.TjanstemanAvlonade.AntalHeltidsTjanster != 0 ? (decimal.Round(dto.TjanstemanAvlonade.AntalHeltidsTjanster, 2).ToString().Replace(".", ",")) : "0") + Environment.NewLine);
                sb.Append("TmTrManL;" + Convert.ToInt32(dto.TjanstemanAvlonade.OverenskommenManadslon) + Environment.NewLine);
                sb.Append("TmTAvtTi;" + Convert.ToInt32(dto.TjanstemanAvlonade.AvtaladeTimmar) + Environment.NewLine);
                sb.Append("TmTrRorL;" + Convert.ToInt32(dto.TjanstemanAvlonade.RorligaTillagg) + Environment.NewLine);
                sb.Append("TmTOvTlg;" + Convert.ToInt32(dto.TjanstemanAvlonade.DaravOvertidstillagg) + Environment.NewLine);
                sb.Append("TmTArbTi;" + Convert.ToInt32(dto.TjanstemanAvlonade.ArbetadeTimmar) + Environment.NewLine);
                sb.Append("TmTOvtTi;" + Convert.ToInt32(dto.TjanstemanAvlonade.DaravOvertidstimmar) + Environment.NewLine);
                sb.Append("TmTRetLS;" + Convert.ToInt32(dto.TjanstemanAvlonade.Retrolon) + Environment.NewLine);
                sb.Append("TmTRetLF;" + (dto.TjanstemanAvlonade.RetrolonFran.HasValue ? dto.TjanstemanAvlonade.RetrolonFran.Value.ToString("yyyyMMdd") : "") + Environment.NewLine);
                sb.Append("TmTRetLT;" + (dto.TjanstemanAvlonade.RetrolonTill.HasValue ? dto.TjanstemanAvlonade.RetrolonTill.Value.ToString("yyyyMMdd") : "") + Environment.NewLine);
                sb.Append("TmTTidpL;" + Convert.ToInt32(dto.TjanstemanAvlonade.RorligaTillaggTidigarePerioder) + Environment.NewLine);
                sb.Append("TmTSjukL;" + Convert.ToInt32(dto.TjanstemanAvlonade.Sjuklon) + Environment.NewLine);
                sb.Append("TmTAnt;" + Convert.ToInt32(dto.TjanstemanAvlonade.NumberOfEmployees) + Environment.NewLine);

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            return filePath;
        }

        public string CreateSCBKSJUFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Prereq

            Company company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionDateFrom, selectionDateTo, out _, out List<int> selectionEmployeeIds))
                return null;

            var employees = EmployeeManager.GetAllEmployeesByIds(entities, reportResult.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var dto = CreateSCB_KSJUPayrollStatisticsFileHeadDTO(entities, employees, selectionDateFrom, selectionDateTo);
            var susFormat = selectionDateFrom.Year >= 2024;

            #endregion

            #region Create file

            string fileName = IOUtil.FileNameSafe(company.Name + "_SCB_KSJU_" + CalendarUtility.ToFileFriendlyDateTime(selectionDateFrom) + " - " + CalendarUtility.ToFileFriendlyDateTime(selectionDateTo));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            try
            {
                var sb = new StringBuilder();

                foreach (var item in dto.SCB_KSJUPayrollStatisticsFileRowDTOs)
                {

                    string row =  susFormat 
                                ?
                                SetString(item.PeOrgNr, 12, "0") +
                                SetString(item.PersonNr, 12, "0") +
                                SetString(GetYearMonthDay(item.SjukFrom), 8, "0") +
                                SetString(GetYearMonthDay(item.SjukTom), 8, "0") +
                                SetString(item.AntDagar.ToString(), 2, "0")
                                :
                                SetString(item.PeOrgNr, 12, "0") +
                                SetString(item.PersonNr, 12, "0") +
                                SetString(GetYearMonthDay(item.SjukFrom), 8, "0") +
                                SetString(GetYearMonthDay(item.SjukTom), 8, "0") +
                                SetString(item.HelAntDagar.ToString(), 2, "0") +
                                SetString(item.DelAntDagar.ToString(), 2, "0") +
                                SetString(item.Korrigeringsuppgift.ToInt().ToString(), 1, "0");

                    sb.Append(row);
                    sb.Append(Environment.NewLine);
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }

            #endregion

            return filePath;
        }
        public string CreateFremia_PayrollStatisticsFile(CompEntities entities)
        {
            #region Init

            #endregion

            #region Prereq

            var company = CompanyManager.GetCompany(reportResult.ActorCompanyId);

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            LogCollector.LogCollector.LogInfo($"CreateFremia_PayrollStatisticsFile Start interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} actorCompanyId {reportResult.ActorCompanyId} number of employees {employees.Count}");
            var dto = CreateSCB_SLPPayrollStatisticsFileHeadDTO(entities, employees, false, selectionDateFrom, selectionDateTo, selectionTimePeriodIds, StatisticFileType.Fremia);
            LogCollector.LogCollector.LogInfo($"CreateFremia_PayrollStatisticsFile End interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} actorCompanyId {reportResult.ActorCompanyId} number of employees {employees.Count}");

            #endregion

            #region Create file

            StreamWriter sw = null;
            string fileName = IOUtil.FileNameSafe(company.Name + " Fremia stat" + selectionDateFrom.ToShortDateString().Replace("-", "") + " - " + selectionDateTo.ToShortDateString().Replace("-", ""));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            // Settings to be fetched for Company
            List<int> companyExportSettings = new List<int>
            {
                (int)CompanySettingType.PayrollExportCFARNumber
            };

            Dictionary<int, object> dictCompanySettings = SettingManager.GetUserCompanySettings(SettingMainType.Company, companyExportSettings, 0, company.ActorCompanyId, 0);
            string payrollExportCFARNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportCFARNumber).ToString();

            try
            {
                FileStream file = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                sw = new StreamWriter(file, Encoding.GetEncoding(437));

                foreach (var item in dto.SCBPayrollStatisticsFileRowDTOs)
                {
                    //     var                                                                             Counted  Pos  Explanation
                    string f1 = SetString(item.Period, 4, "0");                                         // Field 1, 1-4, Period
                    string f2 = SetString("", 7, "0");                                                  // Field 2, 5-11, Delägarnummer fylles ej (blank eller nollutfyllnad)
                    string f3 = SetString("", 3, "0");                                                  // Field 3, 12-14, Arbetsplatnummer Fylles ej (blank eller nollutfyllnad)
                    string f4 = SetString(item.Organisationsnummer.Replace("-", ""), 10, "0");          // Field 4, 15-24, Orgno 10 tkn
                    string f5 = SetString("", 2, "0");                                                  // Field 5, 25-26, Förbundsnummer Ifylles ej (blank eller nollutfyllnad)
                    string f6 = SetString(item.Avtalskod, 3, "0");                                      // Field 6, 27-29, Avtalskod Är avtalskoden mer än 3 positioner  använd styrkod 940
                    string f7 = SetString(item.Personnummer, 12, "0");                                  // Field 7, 30-41, Social Security Number
                    string f8 = SetString("", 1, "0");                                                  // Field 8  42-42, Ifylles ej (blank eller nollutfyllnad)
                    string f9 = SetString(item.Arbetstidsart, 1, "0");                                  // Field 9, 43-43, Arbetstidsart
                    string f10 = SetString(item.Yrkeskod, 4, "0", fromRight: false) + "00";             // Field 10, 44-49, yrkeskod
                    string f11 = "";
                    string f12 = SetString(item.Loneform, 1, "0");                                      // Field 12, 50, Instructionen fält 5
                    string f13 = SetString("", 5, "0");                                                 // Field 13, 51-55, Antall anställda CFAR
                    string f14 = SetString((!string.IsNullOrEmpty(item.CFARnummer) ? item.CFARnummer : payrollExportCFARNumber.Replace("-", "")), 8, "0");   // Field 14, 56-63, CFAR företag
                    string f15 = SetString(item.Helglon == "0" ? "2" : "1", 1, "0");                    // Field 15, 64, Helglön, 2 = Nej    
                    string f16 = SetString(item.Reserv, 6, "0");                                        // Field 16, 65-70, Reserv (noll)

                    //Postbeskrivning rörlig del, styrkod och värden redovisas i position 71 - 400
                    string falt1a = SetString(item.Falt1a, 3, "0");
                    string falt1b = SetString(item.Falt1b, 7, "0");
                    string falt2a = SetString(item.Falt2a, 3, "0");
                    string falt2b = SetString(item.Falt2b, 7, "0");
                    string falt3a = SetString(item.Falt3a, 3, "0");
                    string falt3b = SetString(item.Falt3b, 7, "0");
                    string falt4a = SetString(item.Falt4a, 3, "0");
                    string falt4b = SetString(item.Falt4b, 7, "0");
                    string falt5a = SetString(item.Falt5a, 3, "0");
                    string falt5b = SetString(item.Falt5b, 7, "0");
                    string falt6a = SetString(item.Falt6a, 3, "0");
                    string falt6b = SetString(item.Falt6b, 7, "0");
                    string falt7a = SetString(item.Falt7a, 3, "0");
                    string falt7b = SetString(item.Falt7b, 7, "0");
                    string falt8a = SetString(item.Falt8a, 3, "0");
                    string falt8b = SetString(item.Falt8b, 7, "0");
                    string falt9a = SetString(item.Falt9a, 3, "0");
                    string falt9b = SetString(item.Falt9b, 7, "0");
                    string falt10aa = SetString(item.Falt10aa, 3, "0");
                    string falt10ab = SetString(item.Falt10ab, 7, "0");
                    string falt11a = SetString(item.Falt11a, 3, "0");
                    string falt11b = SetString(item.Falt11b, 7, "0");
                    string falt12a = SetString(item.Falt12a, 3, "0");
                    string falt12b = SetString(item.Falt12b, 7, "0");
                    string falt13a = SetString(item.Falt13a, 3, "0");
                    string falt13b = SetString(item.Falt13b, 7, "0");
                    string falt14a = SetString(item.Falt14a, 3, "0");
                    string falt14b = SetString(item.Falt14b, 7, "0");
                    string falt15aa = SetString(item.Falt15aa, 3, "0");
                    string falt15ab = SetString(item.Falt15ab, 7, "0");
                    string falt15ba = SetString(item.Falt15ba, 3, "0");
                    string falt15bb = SetString(item.Falt15bb, 7, "0");
                    string falt16a = SetString(item.Falt16a, 3, "0");
                    string falt16b = SetString(item.Falt16b, 7, "0");
                    string falt17a = SetString(item.Falt17a, 3, "0");
                    string falt17b = SetString(item.Falt17b, 7, "0");
                    string falt18a = SetString(item.Falt18a, 3, "0");
                    string falt18b = SetString(item.Falt18b, 7, "0");
                    string falt19a = SetString(item.Falt19a, 3, "0");
                    string falt19b = SetString(item.Falt19b, 7, "0");
                    string falt20a = SetString(item.Falt20a, 3, "0");
                    string falt20b = SetString(item.Falt20b, 7, "0");
                    string falt21a = SetString(item.Falt21a, 3, "0");
                    string falt21b = SetString(item.Falt21b, 7, "0");
                    string falt22a = SetString(item.Falt22a, 3, "0");
                    string falt22b = SetString(item.Falt22b, 7, "0");
                    string falt23a = SetString(item.Falt23a, 3, "0");
                    string falt23b = SetString(item.Falt23b, 7, "0");
                    string falt24a = SetString(item.Falt24a, 3, "0");
                    string falt24b = SetString(item.Falt24b, 7, "0");
                    string falt25a = SetString(item.Falt25a, 3, "0");
                    string falt25b = SetString(item.Falt25b, 7, "0");
                    string falt26a = SetString(item.Falt26a, 3, "0");
                    string falt26b = SetString(item.Falt26b, 7, "0");
                    string falt27a = SetString(item.Falt27a, 3, "0");
                    string falt27b = SetString(item.Falt27b, 7, "0");
                    string falt28a = SetString(item.Falt28a, 3, "0");
                    string falt28b = SetString(item.Falt28b, 7, "0");
                    string falt29a = SetString(item.Falt29a, 3, "0");
                    string falt29b = SetString(item.Falt29b, 7, "0");
                    string falt30a = SetString(item.Falt30a, 3, "0");
                    string falt30b = SetString(item.Falt30b, 7, "0");
                    string falt31a = SetString(item.Falt31a, 3, "0");
                    string falt31b = SetString(item.Falt31b, 7, "0");
                    string falt32a = SetString(item.Falt32a, 3, "0");
                    string falt32b = SetString(item.Falt32b, 7, "0");
                    string falt33a = SetString(item.Falt33a, 3, "0");
                    string falt33b = SetString(item.Falt33b, 7, "0");
                    string falt34a = SetString(item.Falt34a, 3, "0");
                    string falt34b = SetString(item.Falt34b, 7, "0");
                    string falt35a = SetString(item.Falt35a, 3, "0");
                    string falt35b = SetString(item.Falt35b, 7, "0");
                    string falt36a = SetString(item.Falt36a, 3, "0");
                    string falt36b = SetString(item.Falt36b, 7, "0");
                    string falt37a = SetString(item.Falt37a, 3, "0");
                    string falt37b = SetString(item.Falt37b, 7, "0");
                    string falt38a = SetString(item.Falt38a, 3, "0");
                    string falt38b = SetString(item.Falt38b, 7, "0");
                    string falt39a = SetString(item.Falt39a, 3, "0");
                    string falt39b = SetString(item.Falt39b, 7, "0");
                    string falt40a = SetString(item.Falt40a, 3, "0");
                    string falt40b = SetString(item.Falt40b, 7, "0");
                    string falt41a = SetString(item.Falt41a, 3, "0");
                    string falt41b = SetString(item.Falt41b, 7, "0");
                    string falt42a = SetString(item.Falt42a, 3, "0");
                    string falt42b = SetString(item.Falt42b, 7, "0");
                    string falt43a = SetString(item.Falt43a, 3, "0");
                    string falt43b = SetString(item.Falt43b, 7, "0");
                    string falt44a = SetString(item.Falt44a, 3, "0");
                    string falt44b = SetString(item.Falt44b, 7, "0");
                    string falt45a = SetString(item.Falt45a, 3, "0");
                    string falt45b = SetString(item.Falt45b, 7, "0");
                    string falt46a = SetString(item.Falt46a, 3, "0");
                    string falt46b = SetString(item.Falt46b, 7, "0");

                    string info = f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15 + f16;

                    string falten = falt1a + falt1b + falt2a + falt2b + falt3a + falt3b + falt4a + falt4b + falt5a + falt5b + falt6a + falt6b + falt7a + falt7b + falt8a + falt8b + falt9a + falt9b + falt10aa + falt10ab
                        + falt11a + falt11b + falt12a + falt12b + falt13a + falt13b + falt14a + falt14b + falt15aa + falt15ab + falt15ba + falt15bb + falt16a + falt16b + falt17a + falt17b + falt18a + falt18b + falt19a + falt19b + falt20a + falt20b
                        + falt21a + falt21b + falt22a + falt22b + falt23a + falt23b + falt24a + falt24b + falt25a + falt25b + falt26a + falt26b + falt27a + falt27b + falt28a + falt28b + falt29a + falt29b + falt30a + falt30b
                        + falt31a + falt31b + falt32a + falt32b + falt33a + falt33b + falt34a + falt34b + falt35a + falt35b + falt36a + falt36b + falt37a + falt37b + falt38a + falt38b + falt39a + falt39b + falt40a + falt40b
                        + falt41a + falt41b + falt42a + falt42b + falt43a + falt43b + falt44a + falt44b + falt45a + falt45b + falt46a + falt46b;

                    sw.WriteLine((info + falten).Substring(0, 400));
                }
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }
            finally
            {
                sw?.Close();
            }

            #endregion

            return filePath;
        }

        public string CreateSN_PayrollStatisticsFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            LogCollector.LogCollector.LogInfo($"CreateSN_PayrollStatisticsFile Start interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} actorCompanyId {reportResult.ActorCompanyId} number of employees {employees.Count}");
            var dto = CreateSCB_SLPPayrollStatisticsFileHeadDTO(entities, employees, false, selectionDateFrom, selectionDateTo, selectionTimePeriodIds);
            LogCollector.LogCollector.LogInfo($"CreateSN_PayrollStatisticsFile End interval {selectionDateFrom.ToShortDateString()}-{selectionDateTo.ToShortDateString()} actorCompanyId {reportResult.ActorCompanyId} number of employees {employees.Count}");

            #endregion

            #region Create file

            StreamWriter sw = null;
            string fileName = IOUtil.FileNameSafe(Company.Name + " SN Medstat " + selectionDateFrom.ToShortDateString().Replace("-", "") + " - " + selectionDateTo.ToShortDateString().Replace("-", ""));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            // Settings to be fetched for Company
            List<int> companyExportSettings = new List<int>
            {
                (int)CompanySettingType.PayrollExportForaAgreementNumber,
                (int)CompanySettingType.PayrollExportITP1Number,
                (int)CompanySettingType.PayrollExportITP2Number,
                (int)CompanySettingType.PayrollExportKPAAgreementNumber,
                (int)CompanySettingType.PayrollExportSNKFOMemberNumber,
                (int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber,
                (int)CompanySettingType.PayrollExportSNKFOAffiliateNumber,
                (int)CompanySettingType.PayrollExportSNKFOAgreementNumber,
                (int)CompanySettingType.PayrollExportCommunityCode,
                (int)CompanySettingType.PayrollExportSCBWorkSite,
                (int)CompanySettingType.PayrollExportCFARNumber
            };

            Dictionary<int, object> dictCompanySettings = SettingManager.GetUserCompanySettings(SettingMainType.Company, companyExportSettings, 0, Company.ActorCompanyId, 0);

            string payrollExportForaAgreementNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportForaAgreementNumber).ToString();
            string payrollExportITP1Number = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportITP1Number).ToString();
            string payrollExportITP2Number = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportITP2Number).ToString();
            string payrollExportKPAAgreementNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportKPAAgreementNumber).ToString();
            string payrollExportSNKFOMemberNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOMemberNumber).ToString();
            string payrollExportSNKFOWorkPlaceNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOWorkPlaceNumber).ToString();
            string payrollExportSNKFOAffiliateNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOAffiliateNumber).ToString();
            string payrollExportSNKFOAgreementNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOAgreementNumber).ToString();
            string payrollExportCommunityCode = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportCommunityCode).ToString();
            string payrollExportSCBWorkSite = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSCBWorkSite).ToString();
            string payrollExportCFARNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportCFARNumber).ToString();

            try
            {
                FileStream file = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                sw = new StreamWriter(file, Encoding.GetEncoding(437));

                foreach (var item in dto.SCBPayrollStatisticsFileRowDTOs)
                {
                    if (string.IsNullOrEmpty(item.Falt11b) || Convert.ToInt32(NumberUtility.ToDecimalWithComma(item.Falt11b)) == 0)
                        continue;

                    if (string.IsNullOrEmpty(item.Falt10ab) || Convert.ToInt32(NumberUtility.ToDecimalWithComma(item.Falt10ab)) == 0)
                        continue;

                    // Gather items to file structure http://www.scb.se/Statistik/AM/AM0103/_dokument/Uppgiftslamnare/Postbeskrivning.pdf

                    //     var                                                       Counted  Pos  Explanation
                    string f1 = SetString(item.Period, 4, "0");                    // Field 1, 1-4, Period
                    string f2 = SetString(item.Delagarnummer, 7, "0");             // Field 2, 5-11, Delägarnummer (Company, Affiliate)
                    string f3 = SetString(item.Arbetsplatsnummer, 3, "0");         // Field 3, 12-14, Arbetsplatnummer i Svenskt Näringsliv
                    string f4 = SetString(item.Organisationsnummer.Replace("-", ""), 10, "0");                     // Field 4, 15-24, Orgno
                    string f5 = SetString(item.Forbundsnummer, 2, "0");                                  // Field 5, 25-26, Förbundsnummer
                    string f6 = SetString(item.Avtalskod, 3, "0");  // Field 6, 27-29, Avtalskod
                    string f7 = SetString(item.Personnummer, 12, "0");                     // Field 7, 30-41, Social Security Number
                    string f8 = SetString(item.Personalkategori, 1, "0");                     // Field 8, 42, Instr. fälr 4
                    string f9 = SetString(item.Arbetstidsart, 1, "0");                     // Field 9, 43, Instr. fält 15
                    string f10 = SetString(item.Yrkeskod, 6, "0", fromRight: false);                     // Field 10, 44-49, Instr. fält 2
                    string f11 = "";
                    string f12 = SetString(item.Loneform, 1, "0");                     // Field 12, 50, Instructionen fält 5
                    string f13 = SetString("", 5, "0");                          // Field 13, 51-55, Antall anställda CFAR
                    string f14 = SetString((!string.IsNullOrEmpty(item.CFARnummer) ? item.CFARnummer : payrollExportCFARNumber.Replace("-", "")), 8, "0");   // Field 14, 56-63, CFAR företag
                    string f15 = SetString(item.Helglon, 0, "0");                                          // Field 15, 64, Helglön, 2 = Nej    
                    string f16 = SetString(item.Reserv, 6, "0");                     // Field 16, 65-70, Reserv (noll)
                                                                                     //Postbeskrivning rörlig del, styrkod och värden redovisas i position 71 - 300
                    string falt1a = SetString(item.Falt1a, 3, "0");
                    string falt1b = SetString(item.Falt1b, 7, "0");
                    string falt2a = SetString(item.Falt2a, 3, "0");
                    string falt2b = SetString(item.Falt2b, 7, "0");
                    string falt3a = SetString(item.Falt3a, 3, "0");
                    string falt3b = SetString(item.Falt3b, 7, "0");
                    string falt4a = SetString(item.Falt4a, 3, "0");
                    string falt4b = SetString(item.Falt4b, 7, "0");
                    string falt5a = SetString(item.Falt5a, 3, "0");
                    string falt5b = SetString(item.Falt5b, 7, "0");
                    string falt6a = SetString(item.Falt6a, 3, "0");
                    string falt6b = SetString(item.Falt6b, 7, "0");
                    string falt7a = SetString(item.Falt7a, 3, "0");
                    string falt7b = SetString(item.Falt7b, 7, "0");
                    string falt8a = SetString(item.Falt8a, 3, "0");
                    string falt8b = SetString(item.Falt8b, 7, "0");
                    string falt9a = SetString(item.Falt9a, 3, "0");
                    string falt9b = SetString(item.Falt9b, 7, "0");
                    string falt10aa = SetString(item.Falt10aa, 3, "0");
                    string falt10ab = SetString(item.Falt10ab, 7, "0");
                    string falt11a = SetString(item.Falt11a, 3, "0");
                    string falt11b = SetString(item.Falt11b, 7, "0");
                    string falt12a = SetString(item.Falt12a, 3, "0");
                    string falt12b = SetString(item.Falt12b, 7, "0");
                    string falt13a = SetString(item.Falt13a, 3, "0");
                    string falt13b = SetString(item.Falt13b, 7, "0");
                    string falt14a = SetString(item.Falt14a, 3, "0");
                    string falt14b = SetString(item.Falt14b, 7, "0");
                    string falt15aa = SetString(item.Falt15aa, 3, "0");
                    string falt15ab = SetString(item.Falt15ab, 7, "0");
                    string falt15ba = SetString(item.Falt15ba, 3, "0");
                    string falt15bb = SetString(item.Falt15bb, 7, "0");
                    string falt16a = SetString(item.Falt16a, 3, "0");
                    string falt16b = SetString(item.Falt16b, 7, "0");
                    string falt17a = SetString(item.Falt17a, 3, "0");
                    string falt17b = SetString(item.Falt17b, 7, "0");
                    string falt18a = SetString(item.Falt18a, 3, "0");
                    string falt18b = SetString(item.Falt18b, 7, "0");
                    string falt19a = SetString(item.Falt19a, 3, "0");
                    string falt19b = SetString(item.Falt19b, 7, "0");
                    string falt20a = SetString(item.Falt20a, 3, "0");
                    string falt20b = SetString(item.Falt20b, 7, "0");
                    string falt21a = SetString(item.Falt21a, 3, "0");
                    string falt21b = SetString(item.Falt21b, 7, "0");
                    string falt22a = SetString(item.Falt22a, 3, "0");
                    string falt22b = SetString(item.Falt22b, 7, "0");
                    string falt23a = SetString(item.Falt23a, 3, "0");
                    string falt23b = SetString(item.Falt23b, 7, "0");
                    string falt24a = SetString(item.Falt24a, 3, "0");
                    string falt24b = SetString(item.Falt24b, 7, "0");
                    string falt25a = SetString(item.Falt25a, 3, "0");
                    string falt25b = SetString(item.Falt25b, 7, "0");
                    string falt26a = SetString(item.Falt26a, 3, "0");
                    string falt26b = SetString(item.Falt26b, 7, "0");
                    string falt27a = SetString(item.Falt27a, 3, "0");
                    string falt27b = SetString(item.Falt27b, 7, "0");
                    string falt28a = SetString(item.Falt28a, 3, "0");
                    string falt28b = SetString(item.Falt28b, 7, "0");
                    string falt29a = SetString(item.Falt29a, 3, "0");
                    string falt29b = SetString(item.Falt29b, 7, "0");
                    string falt30a = SetString(item.Falt30a, 3, "0");
                    string falt30b = SetString(item.Falt30b, 7, "0");
                    string falt31a = SetString(item.Falt31a, 3, "0");
                    string falt31b = SetString(item.Falt31b, 7, "0");
                    string falt32a = SetString(item.Falt32a, 3, "0");
                    string falt32b = SetString(item.Falt32b, 7, "0");
                    string falt33a = SetString(item.Falt33a, 3, "0");
                    string falt33b = SetString(item.Falt33b, 7, "0");
                    string falt34a = SetString(item.Falt34a, 3, "0");
                    string falt34b = SetString(item.Falt34b, 7, "0");
                    string falt35a = SetString(item.Falt35a, 3, "0");
                    string falt35b = SetString(item.Falt35b, 7, "0");
                    string falt36a = SetString(item.Falt36a, 3, "0");
                    string falt36b = SetString(item.Falt36b, 7, "0");
                    string falt37a = SetString(item.Falt37a, 3, "0");
                    string falt37b = SetString(item.Falt37b, 7, "0");
                    string falt38a = SetString(item.Falt38a, 3, "0");
                    string falt38b = SetString(item.Falt38b, 7, "0");
                    string falt39a = SetString(item.Falt39a, 3, "0");
                    string falt39b = SetString(item.Falt39b, 7, "0");
                    string falt40a = SetString(item.Falt40a, 3, "0");
                    string falt40b = SetString(item.Falt40b, 7, "0");
                    string falt41a = SetString(item.Falt41a, 3, "0");
                    string falt41b = SetString(item.Falt41b, 7, "0");
                    string falt42a = SetString(item.Falt42a, 3, "0");
                    string falt42b = SetString(item.Falt42b, 7, "0");
                    string falt43a = SetString(item.Falt43a, 3, "0");
                    string falt43b = SetString(item.Falt43b, 7, "0");
                    string falt44a = SetString(item.Falt44a, 3, "0");
                    string falt44b = SetString(item.Falt44b, 7, "0");
                    string falt45a = SetString(item.Falt45a, 3, "0");
                    string falt45b = SetString(item.Falt45b, 7, "0");
                    string falt46a = SetString(item.Falt46a, 3, "0");
                    string falt46b = SetString(item.Falt46b, 7, "0");

                    string info = f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15 + f16;

                    string falten = falt1a + falt1b + falt2a + falt2b + falt3a + falt3b + falt4a + falt4b + falt5a + falt5b + falt6a + falt6b + falt7a + falt7b + falt8a + falt8b + falt9a + falt9b + falt10aa + falt10ab
                        + falt11a + falt11b + falt12a + falt12b + falt13a + falt13b + falt14a + falt14b + falt15aa + falt15ab + falt15ba + falt15bb + falt16a + falt16b + falt17a + falt17b + falt18a + falt18b + falt19a + falt19b + falt20a + falt20b
                        + falt21a + falt21b + falt22a + falt22b + falt23a + falt23b + falt24a + falt24b + falt25a + falt25b + falt26a + falt26b + falt27a + falt27b + falt28a + falt28b + falt29a + falt29b + falt30a + falt30b
                        + falt31a + falt31b + falt32a + falt32b + falt33a + falt33b + falt34a + falt34b + falt35a + falt35b + falt36a + falt36b + falt37a + falt37b + falt38a + falt38b + falt39a + falt39b + falt40a + falt40b
                        + falt41a + falt41b + falt42a + falt42b + falt43a + falt43b + falt44a + falt44b + falt45a + falt45b + falt46a + falt46b;

                    sw.WriteLine((info + falten).Substring(0, 400));
                }
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }
            finally
            {
                sw?.Close();
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        public string CreateSCB_PayrollStatisticsFile(CompEntities entities)
        {
            #region Init repository

            PersonalDataEmployeeReportRepository personalDataRepository = new PersonalDataEmployeeReportRepository(parameterObject, reportResult);

            #endregion

            #region Prereq

            if (!TryGetIdsFromSelection(reportResult, out List<int> selectionTimePeriodIds, "periods"))
                return null;
            if (!TryGetEmployeeIdsFromSelection(reportResult, selectionTimePeriodIds, out List<Employee> employees, out List<int> selectionEmployeeIds))
                return null;

            TryGetDatesFromSelection(reportResult, selectionTimePeriodIds, out DateTime selectionDateFrom, out DateTime selectionDateTo);

            if (employees == null)
                employees = EmployeeManager.GetAllEmployeesByIds(entities, base.ActorCompanyId, selectionEmployeeIds, loadEmployment: true);

            var dto = CreateSCB_SLPPayrollStatisticsFileHeadDTO(entities, employees, true, selectionDateFrom, selectionDateTo, selectionTimePeriodIds);

            #endregion

            #region Create file

            StreamWriter sw = null;
            string fileName = IOUtil.FileNameSafe(Company.Name + " SCB Statistik " + selectionDateFrom.ToShortDateString().Replace("-", "") + " - " + selectionDateTo.ToShortDateString().Replace("-", ""));
            DirectoryInfo directory = Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL + Guid.NewGuid().ToString());
            string filePath = StringUtility.GetValidFilePath(directory.FullName) + fileName + ".txt";

            // Settings to be fetched for Company
            List<int> companyExportSettings = new List<int>
            {
                (int)CompanySettingType.PayrollExportForaAgreementNumber,
                (int)CompanySettingType.PayrollExportITP1Number,
                (int)CompanySettingType.PayrollExportITP2Number,
                (int)CompanySettingType.PayrollExportKPAAgreementNumber,
                (int)CompanySettingType.PayrollExportSNKFOMemberNumber,
                (int)CompanySettingType.PayrollExportSNKFOWorkPlaceNumber,
                (int)CompanySettingType.PayrollExportSNKFOAffiliateNumber,
                (int)CompanySettingType.PayrollExportSNKFOAgreementNumber,
                (int)CompanySettingType.PayrollExportCommunityCode,
                (int)CompanySettingType.PayrollExportSCBWorkSite,
                (int)CompanySettingType.PayrollExportCFARNumber
            };

            Dictionary<int, object> dictCompanySettings = SettingManager.GetUserCompanySettings(SettingMainType.Company, companyExportSettings, 0, Company.ActorCompanyId, 0);

            string payrollExportForaAgreementNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportForaAgreementNumber).ToString();
            string payrollExportITP1Number = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportITP1Number).ToString();
            string payrollExportITP2Number = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportITP2Number).ToString();
            string payrollExportKPAAgreementNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportKPAAgreementNumber).ToString();
            string payrollExportSNKFOMemberNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOMemberNumber).ToString();
            string payrollExportSNKFOWorkPlaceNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOWorkPlaceNumber).ToString();
            string payrollExportSNKFOAffiliateNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOAffiliateNumber).ToString();
            string payrollExportSNKFOAgreementNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSNKFOAgreementNumber).ToString();
            string payrollExportCommunityCode = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportCommunityCode).ToString();
            string payrollExportSCBWorkSite = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportSCBWorkSite).ToString();
            string payrollExportCFARNumber = SettingsUtility.GetStringCompanySetting(dictCompanySettings, CompanySettingType.PayrollExportCFARNumber).ToString();

            try
            {
                FileStream file = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
                sw = new StreamWriter(file, Encoding.GetEncoding(437));

                foreach (var item in dto.SCBPayrollStatisticsFileRowDTOs)
                {

                    if (string.IsNullOrEmpty(item.Falt9b) || Convert.ToInt32(NumberUtility.ToDecimalWithComma(item.Falt9b)) == 0)
                        continue;

                    if (string.IsNullOrEmpty(item.Falt6b) || Convert.ToInt32(NumberUtility.ToDecimalWithComma(item.Falt6b)) == 0)
                        continue;

                    // Gather items to file structure http://www.scb.se/Statistik/AM/AM0103/_dokument/Uppgiftslamnare/Postbeskrivning.pdf

                    //     var                                                       Counted  Pos  Explanation
                    string f1 = SetString(item.Period, 4, "0");                    // Field 1, 1-4, Period
                    string f2 = SetString(item.Delagarnummer, 7, "0");             // Field 2, 5-11, Delägarnummer (Company, Affiliate)
                    string f3 = SetString(item.Arbetsplatsnummer, 3, "0");         // Field 3, 12-14, Arbetsplatnummer i Svenskt Näringsliv
                    string f4 = SetString(item.Organisationsnummer.Replace("-", ""), 10, "0");                     // Field 4, 15-24, Orgno
                    string f5 = SetString(item.Forbundsnummer, 2, "0");                                  // Field 5, 25-26, Förbundsnummer
                    string f6 = SetString(item.Avtalskod, 3, "0");  // Field 6, 27-29, Avtalskod
                    string f7 = SetString(item.Personnummer, 12, "0");                     // Field 7, 30-41, Social Security Number
                    string f8 = SetString(item.Personalkategori, 1, "0");                     // Field 8, 42, Instr. fälr 4
                    string f9 = SetString(item.Arbetstidsart, 1, "0");                     // Field 9, 43, Instr. fält 15
                    string f10 = SetString(item.Yrkeskod, 4, "0", fromRight: false);                     // Field 10, 44-47, Instr. fält 2
                    string f11 = SetString(item.Forbundsspecifikkod, 2, "0");                     // Field 11, 48-49, Förbundspecifik kod
                    string f12 = SetString(item.Loneform, 1, "0");                     // Field 12, 50, Instructionen fält 5
                    string f13 = SetString(item.AntalanstalldaCFARnr, 5, "0");                          // Field 13, 51-55, Antall anställda CFAR
                    string f14 = SetString((!string.IsNullOrEmpty(item.CFARnummer) ? item.CFARnummer : payrollExportCFARNumber), 8, "0");   // Field 14, 56-63, CFAR företag
                    string f15 = SetString(item.Helglon == "0" ? "2" : "1", 1, "0");                                          // Field 15, 64, Helglön, 2 = Nej    
                    string f16 = SetString(item.Reserv, 6, "0");                     // Field 16, 65-70, Reserv (noll)
                                                                                     //Postbeskrivning rörlig del, styrkod och värden redovisas i position 71 - 300
                    string falt1a = SetString(item.Falt1a, 3, "0");
                    string falt1b = SetString(item.Falt1b, 7, "0");
                    string falt2a = SetString(item.Falt2a, 3, "0");
                    string falt2b = SetString(item.Falt2b, 7, "0");
                    string falt3a = SetString(item.Falt3a, 3, "0");
                    string falt3b = SetString(item.Falt3b, 7, "0");
                    string falt4a = SetString(item.Falt4a, 3, "0");
                    string falt4b = SetString(item.Falt4b, 7, "0");
                    string falt5a = SetString(item.Falt5a, 3, "0");
                    string falt5b = SetString(item.Falt5b, 7, "0");
                    string falt6a = SetString(item.Falt6a, 3, "0");
                    string falt6b = SetString(item.Falt6b, 7, "0");
                    string falt7a = SetString(item.Falt7a, 3, "0");
                    string falt7b = SetString(item.Falt7b, 7, "0");
                    string falt8a = SetString(item.Falt8a, 3, "0");
                    string falt8b = SetString(item.Falt8b, 7, "0");
                    string falt9a = SetString(item.Falt9a, 3, "0");
                    string falt9b = SetString(item.Falt9b, 7, "0");
                    string falt10aa = SetString(item.Falt10aa, 3, "0");
                    string falt10ab = SetString(item.Falt10ab, 7, "0");
                    string falt10ba = SetString(item.Falt10ba, 3, "0");
                    string falt10bb = SetString(item.Falt10bb, 7, "0");
                    string falt10ca = SetString(item.Falt10ca, 3, "0");
                    string falt10cb = SetString(item.Falt10cb, 7, "0");
                    string falt11a = SetString(item.Falt11a, 3, "0");
                    string falt11b = SetString(item.Falt11b, 7, "0");
                    string falt12a = SetString(item.Falt12a, 3, "0");
                    string falt12b = SetString(item.Falt12b, 7, "0");
                    string falt13a = SetString(item.Falt13a, 3, "0");
                    string falt13b = SetString(item.Falt13b, 7, "0");
                    string falt14a = SetString(item.Falt14a, 3, "0");
                    string falt14b = SetString(item.Falt14b, 7, "0");
                    string falt15a = SetString(item.Falt15aa, 3, "0");
                    string falt15b = SetString(item.Falt15ab, 7, "0");
                    string falt16a = SetString(item.Falt16a, 3, "0");
                    string falt16b = SetString(item.Falt16b, 7, "0");
                    string falt17a = SetString(item.Falt17a, 3, "0");
                    string falt17b = SetString(item.Falt17b, 7, "0");
                    string falt18a = SetString(item.Falt18a, 3, "0");
                    string falt18b = SetString(item.Falt18b, 7, "0");
                    string falt19a = SetString(item.Falt19a, 3, "0");
                    string falt19b = SetString(item.Falt19b, 7, "0");
                    string falt20a = SetString(item.Falt20a, 3, "0");
                    string falt20b = SetString(item.Falt20b, 7, "0");
                    string falt21a = SetString(item.Falt21a, 3, "0");
                    string falt21b = SetString(item.Falt21b, 7, "0");
                    string falt22a = SetString(item.Falt22a, 3, "0");
                    string falt22b = SetString(item.Falt22b, 7, "0");
                    string falt23a = SetString(item.Falt23a, 3, "0");
                    string falt23b = SetString(item.Falt23b, 7, "0");
                    string falt24a = SetString(item.Falt24a, 3, "0");
                    string falt24b = SetString(item.Falt24b, 7, "0");
                    string falt25a = SetString(item.Falt25a, 3, "0");
                    string falt25b = SetString(item.Falt25b, 7, "0");
                    string falt26a = SetString(item.Falt26a, 3, "0");
                    string falt26b = SetString(item.Falt26b, 7, "0");
                    string falt27a = SetString(item.Falt27a, 3, "0");
                    string falt27b = SetString(item.Falt27b, 7, "0");
                    string falt28a = SetString(item.Falt28a, 3, "0");
                    string falt28b = SetString(item.Falt28b, 7, "0");
                    string falt29a = SetString(item.Falt29a, 3, "0");
                    string falt29b = SetString(item.Falt29b, 7, "0");
                    string falt30a = SetString(item.Falt30a, 3, "0");
                    string falt30b = SetString(item.Falt30b, 7, "0");
                    string falt31a = SetString(item.Falt31a, 3, "0");
                    string falt31b = SetString(item.Falt31b, 7, "0");
                    string falt32a = SetString(item.Falt32a, 3, "0");
                    string falt32b = SetString(item.Falt32b, 7, "0");
                    string falt33a = SetString(item.Falt33a, 3, "0");
                    string falt33b = SetString(item.Falt33b, 7, "0");
                    string falt34a = SetString(item.Falt34a, 3, "0");
                    string falt34b = SetString(item.Falt34b, 7, "0");
                    string falt35a = SetString(item.Falt35a, 3, "0");
                    string falt35b = SetString(item.Falt35b, 7, "0");
                    string falt36a = SetString(item.Falt36a, 3, "0");
                    string falt36b = SetString(item.Falt36b, 7, "0");
                    string falt37a = SetString(item.Falt37a, 3, "0");
                    string falt37b = SetString(item.Falt37b, 7, "0");
                    string falt38a = SetString(item.Falt38a, 3, "0");
                    string falt38b = SetString(item.Falt38b, 7, "0");
                    string falt39a = SetString(item.Falt39a, 3, "0");
                    string falt39b = SetString(item.Falt39b, 7, "0");
                    string falt40a = SetString(item.Falt40a, 3, "0");
                    string falt40b = SetString(item.Falt40b, 7, "0");
                    string falt41a = SetString(item.Falt41a, 3, "0");
                    string falt41b = SetString(item.Falt41b, 7, "0");
                    string falt42a = SetString(item.Falt42a, 3, "0");
                    string falt42b = SetString(item.Falt42b, 7, "0");
                    string falt43a = SetString(item.Falt43a, 3, "0");
                    string falt43b = SetString(item.Falt43b, 7, "0");
                    string falt44a = SetString(item.Falt44a, 3, "0");
                    string falt44b = SetString(item.Falt44b, 7, "0");
                    string falt45a = SetString(item.Falt45a, 3, "0");
                    string falt45b = SetString(item.Falt45b, 7, "0");
                    string falt46a = SetString(item.Falt46a, 3, "0");
                    string falt46b = SetString(item.Falt46b, 7, "0");

                    string info = f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15 + f16;

                    string falten = falt1a + falt1b + falt2a + falt2b + falt3a + falt3b + falt4a + falt4b + falt5a + falt5b + falt6a + falt6b + falt7a + falt7b + falt8a + falt8b + falt9a + falt9b + falt10aa + falt10ab + falt10ba + falt10bb + falt10ca + falt10cb
                        + falt11a + falt11b + falt12a + falt12b + falt13a + falt13b + falt14a + falt14b + falt15a + falt15b + falt16a + falt16b + falt17a + falt17b + falt18a + falt18b + falt19a + falt19b + falt20a + falt20b
                        + falt21a + falt21b + falt22a + falt22b + falt23a + falt23b + falt24a + falt24b + falt25a + falt25b + falt26a + falt26b + falt27a + falt27b + falt28a + falt28b + falt29a + falt29b + falt30a + falt30b
                        + falt31a + falt31b + falt32a + falt32b + falt33a + falt33b + falt34a + falt34b + falt35a + falt35b + falt36a + falt36b + falt37a + falt37b + falt38a + falt38b + falt39a + falt39b + falt40a + falt40b
                        + falt41a + falt41b + falt42a + falt42b + falt43a + falt43b + falt44a + falt44b + falt45a + falt45b + falt46a + falt46b;

                    sw.WriteLine((info + falten).Substring(0, 300));
                }
            }
            catch (Exception ex)
            {
                SysLogManager.AddSysLog(ex, log4net.Core.Level.Error);
            }
            finally
            {
                sw?.Close();
            }

            #endregion

            #region Close repository

            personalDataRepository.GenerateLogs();

            #endregion

            return filePath;
        }

        #endregion

        #region Help-methods

        private SCB_KSJUPayrollStatisticsFileRowDTO CloneSCB_KSJUPayrollStatisticsFileRowDTO(SCB_KSJUPayrollStatisticsFileRowDTO from)
        {
            var rowDTO = new SCB_KSJUPayrollStatisticsFileRowDTO
            {
                EmployeeNr = from.EmployeeNr,
                EmployeeName = from.EmployeeName,
                EmployeeId = from.EmployeeId,
                PersonNr = from.PersonNr,
                PeOrgNr = from.PeOrgNr,
                SjukFrom = from.SjukFrom,
                SjukTom = from.SjukTom,
                HelAntDagar = from.HelAntDagar,
                DelAntDagar = from.DelAntDagar,
                AntDagar = from.AntDagar,
                Cfar = from.Cfar,
                Korrigeringsuppgift = from.Korrigeringsuppgift
            };

            return rowDTO;
        }

        private void SetValueForSN(CompEntities entities, int actorCompanyId, SCB_SLPPayrollStatisticsFileRowDTO scbPayrollStatisticsFileRowDTO, Employee employee, Employment employment, DateTime date, bool isHourlyPay, List<TimePayrollStatisticsDTO> timePayrollTransactionItems, List<VacationGroup> vacationGroups, List<int> payedProducts)
        {
            List<SCBPayrollStatisticsTransactionDTO> list = new List<SCBPayrollStatisticsTransactionDTO>();
            scbPayrollStatisticsFileRowDTO.scbPayrollStatisticsTransactionDTOs = new List<SCBPayrollStatisticsTransactionDTO>();

            if (!string.IsNullOrEmpty(employee.WorkPlaceSCB))
                scbPayrollStatisticsFileRowDTO.Arbetsplatsnummer = employee.WorkPlaceSCB;

            string falt11a = "001";
            string falt11b = string.Empty;
            string falt14a = "002";
            string falt14b = string.Empty;
            string falt7a = "003";
            string falt7b = string.Empty;
            string falt8a = "004";
            string falt8b = string.Empty;
            string falt10aa = "051";
            string falt10ab = string.Empty;
            string falt10ba = string.Empty;
            string falt10bb = string.Empty;
            string falt10ca = string.Empty;
            string falt10cb = string.Empty;
            string falt15aa = "052";
            string falt15ab = string.Empty;
            string falt15ba = "053";
            string falt15bb = string.Empty;
            string falt16a = "054";
            string falt16b = string.Empty;
            string falt18a = "055";
            string falt18b = string.Empty;
            string falt17a = "056";
            string falt17b = string.Empty;
            string falt12a = "058";
            string falt12b = string.Empty;
            string falt9a = "600";
            string falt9b = string.Empty;
            string falt13a = "601";
            string falt13b = string.Empty;
            string falt20a = "810";
            string falt20b = string.Empty;
            string falt19a = "000";
            string falt19b = string.Empty;
            string falt21a = "700";
            string falt21b = string.Empty;
            string falt22a = "057";
            string falt22b = string.Empty;

            var totaltArbetadtid = timePayrollTransactionItems.Where(t => payedProducts.Contains(t.PayrollProductId) && !t.IsScheduleTransaction && !t.IsFromOtherPeriod);

            decimal totaltArbetadtidSumma = 0;
            if (totaltArbetadtid != null && totaltArbetadtid.Any())
            {
                totaltArbetadtidSumma = totaltArbetadtid.Sum(t => decimal.Divide(t.Quantity, 60));
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltArbetadtid.ToList(), "totaltArbetadtid"));
            }

            falt11b = Convert.ToInt32(totaltArbetadtidSumma).ToString();

            //(1) Bruttolön (2) Övertidsersättning och (1) Bruttolön (2) Mertid/fyllnadstid
            var totaltMerOverTid = timePayrollTransactionItems.Where(t => t.IsAddedOrOverTime() && !t.IsFromOtherPeriod);
            decimal totaltMerOverTidSumma = 0;
            if (totaltMerOverTid != null && totaltMerOverTid.Any())
            {
                totaltMerOverTidSumma = totaltMerOverTid.Sum(t => decimal.Divide(t.Quantity, 60));
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltMerOverTid.ToList(), "totaltMerOverTid"));
                totaltArbetadtidSumma = totaltArbetadtidSumma + totaltMerOverTidSumma;
                falt11b = Convert.ToInt32(totaltArbetadtidSumma).ToString();
            }
            if (totaltMerOverTidSumma > 0 && totaltMerOverTidSumma < 1)
                totaltMerOverTidSumma = 1;

            falt14b = Convert.ToInt32(totaltMerOverTidSumma).ToString();

            var veckoarbetstid = employment.GetWorkTimeWeek(date);
            decimal totaltVeckoarbetstid = 0;
            if (veckoarbetstid != 0)
                totaltVeckoarbetstid = decimal.Divide(veckoarbetstid, 60);
            falt7b = totaltVeckoarbetstid.ToString("F").Replace(".", ",");

            var veckoarbetstidHel = employment.GetEmployeeGroup(date).RuleWorkTimeWeek;
            decimal totaltVeckoarbetstidHel = 0;
            if (veckoarbetstidHel != 0)
                totaltVeckoarbetstidHel = decimal.Divide(veckoarbetstidHel, 60);
            falt8b = totaltVeckoarbetstidHel.ToString("F").Replace(".", ",");

            decimal lon = 0;
            PayrollGroup payrollGroup = employment.GetPayrollGroup(date);
            bool variableHourlyPay = false;
            if (payrollGroup != null)
            {
                if (!payrollGroup.PayrollGroupSetting.IsLoaded)
                    payrollGroup.PayrollGroupSetting.Load();

                if (!payrollGroup.PayrollGroupSetting.IsNullOrEmpty())
                {
                    var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                    if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                    {
                        PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, null, date, null, null, setting.IntData.Value);
                        if (result != null)
                        {
                            if (!isHourlyPay)
                                lon = Convert.ToInt32(result.Amount);
                            else
                            {
                                if (timePayrollTransactionItems.Where(w => w.IsHourlySalary()).GroupBy(g => g.PayrollProductId).Count() > 1)
                                {
                                    var hourlyPayPerHour = timePayrollTransactionItems.Where(w => w.IsHourlySalary()).Select(s => decimal.Divide(s.Amount, decimal.Multiply(60, s.Quantity))).ToList();

                                    if (hourlyPayPerHour.Min() + 2 < (int)hourlyPayPerHour.Max())
                                        variableHourlyPay = true;
                                }
                                lon = Convert.ToInt32(result.Amount * 100);
                            }
                        }
                    }
                }
            }

            if (!variableHourlyPay)
                falt10ab = lon.ToString("#").Replace(".", ",");
            else
                falt13b = lon.ToString("#").Replace(".", ",");

            //(1) Bruttolön (2) Övertidstillägg
            var totaltOverTidsTillagg = timePayrollTransactionItems.Where(t => t.IsOverTimeAddition() && !t.IsFromOtherPeriod);
            decimal totaltOverTidsTillaggSumma = 0;
            if (totaltOverTidsTillagg != null && totaltOverTidsTillagg.Any())
            {
                totaltOverTidsTillaggSumma = decimal.Divide(totaltOverTidsTillagg.Sum(t => t.Amount), 100);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltOverTidsTillagg.ToList(), "totaltOverTidsTillagg"));
            }

            falt15ab = Convert.ToInt32(totaltOverTidsTillaggSumma).ToString();

            //(1) Bruttolön(2) Övertidsersättning
            var totaltOverTidsErsattning = timePayrollTransactionItems.Where(t => t.IsOvertimeCompensation() && !t.IsFromOtherPeriod);
            decimal totaltOverTidsErsattningSumma = 0;
            if (totaltOverTidsErsattning != null && totaltOverTidsErsattning.Any())
                totaltOverTidsErsattningSumma = totaltOverTidsErsattning.Sum(t => t.Amount);
            falt15bb = Convert.ToInt32(totaltOverTidsErsattningSumma).ToString();

            //(1) Bruttolön (2) OB-tillägg
            var totaltOBSkiftTillagg = timePayrollTransactionItems.Where(t => t.IsOBAddition() && !t.IsFromOtherPeriod);
            decimal totaltOBSkiftTillaggSumma = 0;
            if (totaltOBSkiftTillagg != null && totaltOBSkiftTillagg.Any())
            {
                totaltOBSkiftTillaggSumma = totaltOBSkiftTillagg.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltOBSkiftTillagg.ToList(), "totaltOBSkiftTillagg"));
            }

            falt16b = Convert.ToInt32(totaltOBSkiftTillaggSumma).ToString();

            //(1) Förmån + Ersättning
            var totaltJourBeredskap = timePayrollTransactionItems.Where(t => t.IsBenefitAndNotInvert() && !t.IsFromOtherPeriod);
            decimal totaltJourBeredskapSumma = 0;
            if (totaltJourBeredskap != null && totaltJourBeredskap.Any())
            {
                totaltJourBeredskapSumma = totaltJourBeredskap.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltJourBeredskap.ToList(), "totaltJourBeredskap"));
            }
            falt18b = Convert.ToInt32(totaltJourBeredskapSumma).ToString();

            //(1) Förmån
            var totaltForman = timePayrollTransactionItems.Where(t => t.IsBenefitAndNotInvert() && !t.IsFromOtherPeriod);
            decimal totaltFormanSumma = 0;
            if (totaltForman != null && totaltForman.Any())
                totaltFormanSumma = totaltForman.Sum(t => t.Amount);

            falt22b = Convert.ToInt32(totaltFormanSumma).ToString();

            //(1) Bruttolön (2) Ackord
            var totaltPrestationslon = timePayrollTransactionItems.Where(t => t.IsContract() && !t.IsFromOtherPeriod);
            decimal totaltPrestationslonSumma = 0;
            if (totaltPrestationslon != null && totaltPrestationslon.Any())
            {
                totaltPrestationslonSumma = totaltPrestationslon.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltPrestationslon.ToList(), "totaltPrestationslon"));
            }
            falt17b = Convert.ToInt32(totaltPrestationslonSumma).ToString();

            var vacationGroup = employment.GetVacationGroup(date, vacationGroups);
            if (vacationGroup != null && !vacationGroup.VacationGroupSE.IsNullOrEmpty())
            {
                var vacationCalculationContainer = PayrollManager.CalculateVacationSR(entities, actorCompanyId, date, vacationGroup, vacationGroup.VacationGroupSE.FirstOrDefault(), employee, employment);
                decimal semesterratt = 0;
                if (vacationCalculationContainer != null && vacationCalculationContainer.FormulaResult != null && vacationCalculationContainer.FormulaResult.FirstOrDefault() != null)
                    semesterratt = vacationCalculationContainer.FormulaResult.FirstOrDefault().Value;
                falt9b = Convert.ToInt32(semesterratt).ToString();
            }
            //Jobbstatus
            var employeeSettings = EmployeeManager.GetEmployeeSettings(actorCompanyId, employee.EmployeeId, date, date, TermGroup_EmployeeSettingType.Reporting, TermGroup_EmployeeSettingType.Reporting_SN, TermGroup_EmployeeSettingType.Reporting_SN_Jobstatus).FirstOrDefault();
            if (employeeSettings == null && payrollGroup != null)
            {
                var jobStatus = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollReportsJobStatus);
                if (jobStatus != null && jobStatus.IntData.HasValue && jobStatus.IntData.Value != 0)
                {
                    falt20b = jobStatus.IntData.Value.ToString();
                }
            }
            else if (employeeSettings.IntData.HasValue)
            {
                falt20b = employeeSettings.IntData.Value.ToString();
            }
          
            //(1) Tillsvidare (2) Tidsbegränsad
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(reportResult.ActorCompanyId);
            bool tillsvidareAnstalld = employment.GetEmploymentType(employmentTypes) == (int)TermGroup_EmploymentType.SE_Permanent;

            falt21b = tillsvidareAnstalld ? "1" : "2";

            scbPayrollStatisticsFileRowDTO.scbPayrollStatisticsTransactionDTOs.AddRange(list);

            scbPayrollStatisticsFileRowDTO.Falt1a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt1b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt2a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt2b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt3a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt3b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt4a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt4b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt5a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt5b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt6a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt6b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt7a = falt7a;
            scbPayrollStatisticsFileRowDTO.Falt7b = falt7b;
            scbPayrollStatisticsFileRowDTO.Falt8a = falt8a;
            scbPayrollStatisticsFileRowDTO.Falt8b = falt8b;
            scbPayrollStatisticsFileRowDTO.Falt9a = falt9a;
            scbPayrollStatisticsFileRowDTO.Falt9b = falt9b;
            scbPayrollStatisticsFileRowDTO.Falt10aa = falt10aa;
            scbPayrollStatisticsFileRowDTO.Falt10ab = falt10ab;
            scbPayrollStatisticsFileRowDTO.Falt10ba = falt10ba;
            scbPayrollStatisticsFileRowDTO.Falt10bb = falt10bb;
            scbPayrollStatisticsFileRowDTO.Falt10ca = falt10ca;
            scbPayrollStatisticsFileRowDTO.Falt10cb = falt10cb;
            scbPayrollStatisticsFileRowDTO.Falt11a = falt11a;
            scbPayrollStatisticsFileRowDTO.Falt11b = falt11b;
            scbPayrollStatisticsFileRowDTO.Falt12a = falt12a;
            scbPayrollStatisticsFileRowDTO.Falt12b = falt12b;
            scbPayrollStatisticsFileRowDTO.Falt13a = falt13a;
            scbPayrollStatisticsFileRowDTO.Falt13b = falt13b;
            scbPayrollStatisticsFileRowDTO.Falt14a = falt14a;
            scbPayrollStatisticsFileRowDTO.Falt14b = falt14b;
            scbPayrollStatisticsFileRowDTO.Falt15aa = falt15aa;
            scbPayrollStatisticsFileRowDTO.Falt15ab = falt15ab;
            scbPayrollStatisticsFileRowDTO.Falt15ba = falt15ba;
            scbPayrollStatisticsFileRowDTO.Falt15bb = falt15bb;
            scbPayrollStatisticsFileRowDTO.Falt16a = falt16a;
            scbPayrollStatisticsFileRowDTO.Falt16b = falt16b;
            scbPayrollStatisticsFileRowDTO.Falt17a = falt17a;
            scbPayrollStatisticsFileRowDTO.Falt17b = falt17b;
            scbPayrollStatisticsFileRowDTO.Falt18a = falt18a;
            scbPayrollStatisticsFileRowDTO.Falt18b = falt18b;
            scbPayrollStatisticsFileRowDTO.Falt19a = falt19a;
            scbPayrollStatisticsFileRowDTO.Falt19b = falt19b;
            scbPayrollStatisticsFileRowDTO.Falt20a = falt20a;
            scbPayrollStatisticsFileRowDTO.Falt20b = falt20b;
            scbPayrollStatisticsFileRowDTO.Falt21a = falt21a;
            scbPayrollStatisticsFileRowDTO.Falt21b = falt21b;
            scbPayrollStatisticsFileRowDTO.Falt22a = falt22a;
            scbPayrollStatisticsFileRowDTO.Falt22b = falt22b;
            scbPayrollStatisticsFileRowDTO.Falt23a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt23b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt24a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt24b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt25a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt25b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt26a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt26b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt27a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt27b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt28a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt28b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt29a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt29b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt30a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt30b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt31a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt31b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt32a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt32b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt33a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt33b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt34a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt34b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt35a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt35b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt36a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt36b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt37a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt37b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt38a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt38b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt39a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt39b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt40a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt40b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt41a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt41b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt42a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt42b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt43a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt43b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt44a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt44b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt45a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt45b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt46a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt46b = string.Empty;
        }
        private void SetValueForFremia(CompEntities entities, int actorCompanyId, SCB_SLPPayrollStatisticsFileRowDTO scbPayrollStatisticsFileRowDTO, Employee employee, Employment employment, DateTime date, bool isHourlyPay, List<TimePayrollStatisticsDTO> timePayrollTransactionItems, List<VacationGroup> vacationGroups, List<int> payedProducts, bool additionLevel3 = false)
        {
            List<SCBPayrollStatisticsTransactionDTO> list = new List<SCBPayrollStatisticsTransactionDTO>();
            scbPayrollStatisticsFileRowDTO.scbPayrollStatisticsTransactionDTOs = new List<SCBPayrollStatisticsTransactionDTO>();

            if (!string.IsNullOrEmpty(employee.WorkPlaceSCB))
                scbPayrollStatisticsFileRowDTO.Arbetsplatsnummer = employee.WorkPlaceSCB;

            bool Avtalsomrade = scbPayrollStatisticsFileRowDTO.Avtalskod.Length > 3;    //Fler än 3 tecken i avtalskod betyder att avtalskod är ändrad


            string falt1a = string.Empty;       //934  Kommunkod - Behöver inte anges när man har CFAR nr.
            string falt1b = string.Empty;
            string falt2a = Avtalsomrade ? "940" : string.Empty;
            string falt2b = Avtalsomrade ? scbPayrollStatisticsFileRowDTO.Avtalskod : string.Empty;
            string falt3a = "937";
            string falt3b = scbPayrollStatisticsFileRowDTO.Yrkeskod;
            string falt4a = "997";
            string falt4b = string.Empty;
            string falt5a = "996";
            string falt5b = string.Empty;
            string falt6a = "450";
            string falt6b = string.Empty;
            string falt7a = "003";
            string falt7b = string.Empty;
            string falt8a = "004";
            string falt8b = string.Empty;
            string falt10aa = "051";
            string falt10ab = string.Empty;
            string falt10ba = string.Empty;
            string falt10bb = string.Empty;
            string falt10ca = string.Empty;
            string falt10cb = string.Empty;
            string falt11a = "001";
            string falt11b = string.Empty;
            string falt14a = "002";
            string falt14b = string.Empty;
            string falt15aa = "052";
            string falt15ab = string.Empty;
            string falt15ba = "053";
            string falt15bb = string.Empty;
            string falt16a = "054";
            string falt16b = string.Empty;
            string falt18a = "055";
            string falt18b = string.Empty;
            string falt17a = "056";
            string falt17b = string.Empty;
            string falt12a = "601";
            string falt12b = string.Empty;
            string falt9a = "600";
            string falt9b = string.Empty;
            string falt13a = "058";
            string falt13b = string.Empty;
            string falt19a = "930";
            string falt19b = string.Empty;
            string falt20a = "057";
            string falt20b = string.Empty;
            string falt21a = "700";
            string falt21b = string.Empty;

            var totaltArbetadtid = timePayrollTransactionItems.Where(t => payedProducts.Contains(t.PayrollProductId) && !t.IsAddedOrOverTime() && !t.IsScheduleTransaction && !t.IsFromOtherPeriod);

            decimal totaltArbetadtidSumma = 0;
            if (totaltArbetadtid != null && totaltArbetadtid.Any())
            {
                totaltArbetadtidSumma = totaltArbetadtid.Sum(t => decimal.Divide(t.Quantity, 60));
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltArbetadtid.ToList(), "totaltArbetadtid"));
            }

            falt11b = Convert.ToInt32(totaltArbetadtidSumma).ToString();

            //(1) Bruttolön (2) Övertidsersättning och (1) Bruttolön (2) Mertid/fyllnadstid
            var totaltMerOverTid = timePayrollTransactionItems.Where(t => t.IsAddedOrOverTime() && !t.IsFromOtherPeriod);
            decimal totaltMerOverTidSumma = 0;
            if (totaltMerOverTid != null && totaltMerOverTid.Any())
            {
                totaltMerOverTidSumma = totaltMerOverTid.Sum(t => decimal.Divide(t.Quantity, 60));
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltMerOverTid.ToList(), "totaltMerOverTid"));
                totaltArbetadtidSumma = totaltArbetadtidSumma + totaltMerOverTidSumma;
                falt11b = Convert.ToInt32(totaltArbetadtidSumma).ToString();
            }
            if (totaltMerOverTidSumma > 0 && totaltMerOverTidSumma < 1)
                totaltMerOverTidSumma = 1;

            falt14b = Convert.ToInt32(Decimal.Round(totaltMerOverTidSumma)).ToString();

            int veckoarbetstid = employment.GetWorkTimeWeek(date);
            decimal totaltVeckoarbetstid = 0;
            if (veckoarbetstid != 0)
                totaltVeckoarbetstid = decimal.Divide(veckoarbetstid, 60);
            falt7b = totaltVeckoarbetstid.ToString("F").Replace(".", ",");

            int veckoarbetstidHel = employment.GetEmployeeGroup(date).RuleWorkTimeWeek;
            decimal totaltVeckoarbetstidHel = 0;
            if (veckoarbetstidHel != 0)
                totaltVeckoarbetstidHel = decimal.Divide(veckoarbetstidHel, 60);
            falt8b = totaltVeckoarbetstidHel.ToString("F").Replace(".", ",");

            decimal lon = 0;
            PayrollGroup payrollGroup = employment.GetPayrollGroup(date);
            bool variableHourlyPay = false;
            if (payrollGroup != null)
            {
                if (!payrollGroup.PayrollGroupSetting.IsLoaded)
                    payrollGroup.PayrollGroupSetting.Load();

                if (!payrollGroup.PayrollGroupSetting.IsNullOrEmpty())
                {
                    var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                    if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                    {
                        PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, null, date, null, null, setting.IntData.Value);
                        if (result != null)
                        {
                            if (!isHourlyPay)
                                lon = Convert.ToInt32(result.Amount);
                            else
                            {
                                if (timePayrollTransactionItems.Where(w => w.IsHourlySalary()).GroupBy(g => g.PayrollProductId).Count() > 1)
                                {
                                    var hourlyPayPerHour = timePayrollTransactionItems.Where(w => w.IsHourlySalary()).Select(s => decimal.Divide(s.Amount, decimal.Multiply(60, s.Quantity))).ToList();

                                    if (hourlyPayPerHour.Min() + 2 < (int)hourlyPayPerHour.Max())
                                        variableHourlyPay = true;
                                }
                                lon = result.Amount;
                            }
                        }
                    }
                }
            }

            if (!variableHourlyPay)
            {
                if(isHourlyPay)
                    falt10ab = totaltArbetadtidSumma > 0 ? lon.ToString("F").Replace(".", ",") : "0";
                else
                    falt10ab = lon.ToString("#").Replace(".", ",");
            }
            else
                falt13b = lon.ToString("F").Replace(".", ",");

            //(1) Bruttolön (2) Övertidstillägg
            var totaltOverTidsTillagg = timePayrollTransactionItems.Where(t => (t.IsOverTimeAddition() || t.IsAddedTimeAddition() ) && !t.IsFromOtherPeriod);
            decimal totaltOverTidsTillaggSumma = 0;
            if (totaltOverTidsTillagg != null && totaltOverTidsTillagg.Any())
            {
                totaltOverTidsTillaggSumma = totaltOverTidsTillagg.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltOverTidsTillagg.ToList(), "totaltOverTidsTillagg"));
            }
            if(totaltOverTidsTillaggSumma > 0 && totaltOverTidsTillaggSumma < 1)
                totaltOverTidsTillaggSumma = 1;

            falt15ab = totaltOverTidsTillaggSumma.ToString("F").Replace(".", ",");

            //(1) Bruttolön(2) Övertidsersättning
            var totaltOverTidsErsattning = timePayrollTransactionItems.Where(t => (t.IsOvertimeCompensation() || t.IsAddedTimeCompensation() || (!additionLevel3 && t.IsAddedTime()) ) && !t.IsFromOtherPeriod);
            decimal totaltOverTidsErsattningSumma = 0;
            if (totaltOverTidsErsattning != null && totaltOverTidsErsattning.Any())
                totaltOverTidsErsattningSumma = totaltOverTidsErsattning.Sum(t => t.Amount);

            if (totaltOverTidsErsattningSumma > 0 && totaltOverTidsErsattningSumma < 1)
                totaltOverTidsErsattningSumma = 1;

            falt15bb = totaltOverTidsErsattningSumma.ToString("F").Replace(".", ",");

            //(1) Bruttolön (2) OB-tillägg
            var totaltOBSkiftTillagg = timePayrollTransactionItems.Where(t => t.IsOBAddition() && !t.IsFromOtherPeriod);
            decimal totaltOBSkiftTillaggSumma = 0;
            if (totaltOBSkiftTillagg != null && totaltOBSkiftTillagg.Any())
            {
                totaltOBSkiftTillaggSumma = totaltOBSkiftTillagg.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltOBSkiftTillagg.ToList(), "totaltOBSkiftTillagg"));
            }

            falt16b = totaltOBSkiftTillaggSumma.ToString("F").Replace(".", ",");

            //(1) Bruttolön (2) Ackord
            var totaltPrestationslon = timePayrollTransactionItems.Where(t => t.IsContract());
            decimal totaltPrestationslonSumma = 0;
            if (totaltPrestationslon != null && totaltPrestationslon.Any())
            {
                totaltPrestationslonSumma = totaltPrestationslon.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltPrestationslon.ToList(), "totaltPrestationslon"));
            }
            falt12b = totaltPrestationslonSumma.ToString("F").Replace(".", ",");

            //(1) Förmån men inte (2) Motbokning förmån eller (2) (1) Bruttolön Jour
            var totaltJourBeredskap = timePayrollTransactionItems.Where(t => t.IsDutyAndBenefitNotInvert() && !t.IsFromOtherPeriod);
            decimal totaltJourBeredskapSumma = 0;
            if (totaltJourBeredskap != null && totaltJourBeredskap.Any())
            {
                totaltJourBeredskapSumma = totaltJourBeredskap.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltJourBeredskap.ToList(), "totaltJourBeredskap"));
            }
            falt18b = totaltJourBeredskapSumma.ToString("F").Replace(".", ",");
            //(1) Förmån men inte (2) Motbokning förmån eller (2) 
            var totaltForman = timePayrollTransactionItems.Where(t => t.IsBenefitAndNotInvert() && !t.IsFromOtherPeriod);
            decimal totaltFormanSumma = 0;
            if (totaltForman != null && totaltForman.Any())
            {
                totaltFormanSumma = totaltForman.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltForman.ToList(), "totaltForman"));
            }
            falt20b = totaltFormanSumma.ToString("F").Replace(".", ",");

            var vacationGroup = employment.GetVacationGroup(date, vacationGroups);
            if (vacationGroup != null && !vacationGroup.VacationGroupSE.IsNullOrEmpty())
            {
                var vacationCalculationContainer = PayrollManager.CalculateVacationSR(entities, actorCompanyId, date, vacationGroup, vacationGroup.VacationGroupSE.FirstOrDefault(), employee, employment);
                decimal semesterratt = 0;
                if (vacationCalculationContainer != null && vacationCalculationContainer.FormulaResult != null && vacationCalculationContainer.FormulaResult.FirstOrDefault() != null)
                    semesterratt = vacationCalculationContainer.FormulaResult.FirstOrDefault().Value;
                falt9b = Convert.ToInt32(semesterratt).ToString();
            }

            //Syssesättningsgrad 2 decimaler
            var sysGrad = employment.GetPercent(date);
            falt4b = sysGrad.ToString("F").Replace(".", ",");

            //Lönen (1) som heltidslön, (2) som deltidslön, (3) som lön per timme, (4) som utbetalda timmar
            if (isHourlyPay)
                falt5b = "3";
            else if (Convert.ToInt32(sysGrad) >= 100)
                falt5b = "1";
            else
                falt5b = "2";

            //(1) Frånvarande
            var wokrTime = timePayrollTransactionItems.Where(t => t.IsWorkTime() && !t.IsScheduleTransaction && !t.IsFromOtherPeriod).Sum(t => decimal.Divide(t.Quantity, 60));
            var absence = timePayrollTransactionItems.Where(t => (t.IsAbsenceSick() || t.IsLeaveOfAbsence() || t.IsParentalLeave() ) && !t.IsScheduleTransaction && !t.IsFromOtherPeriod).Sum(t => decimal.Divide(t.Quantity, 60)); 
            if (wokrTime == 0 && absence > 0)
                falt19b = "1";
            else
                falt19b = "0";

            //(1) Tillsvidare (2) Tidsbegränsad
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(reportResult.ActorCompanyId);
            bool tillsvidareAnstalld = employment.GetEmploymentType(employmentTypes) == (int)TermGroup_EmploymentType.SE_Permanent;

            falt21b = tillsvidareAnstalld ? "1" : "2";

            scbPayrollStatisticsFileRowDTO.scbPayrollStatisticsTransactionDTOs.AddRange(list);

            scbPayrollStatisticsFileRowDTO.Falt1a = falt1a;
            scbPayrollStatisticsFileRowDTO.Falt1b = falt1b;
            scbPayrollStatisticsFileRowDTO.Falt2a = falt2a;
            scbPayrollStatisticsFileRowDTO.Falt2b = falt2b;
            scbPayrollStatisticsFileRowDTO.Falt3a = falt3a;
            scbPayrollStatisticsFileRowDTO.Falt3b = falt3b;
            scbPayrollStatisticsFileRowDTO.Falt4a = falt4a;
            scbPayrollStatisticsFileRowDTO.Falt4b = falt4b;
            scbPayrollStatisticsFileRowDTO.Falt5a = falt5a;
            scbPayrollStatisticsFileRowDTO.Falt5b = falt5b;
            scbPayrollStatisticsFileRowDTO.Falt6a = falt6a;
            scbPayrollStatisticsFileRowDTO.Falt6b = falt6b;
            scbPayrollStatisticsFileRowDTO.Falt7a = falt7a;
            scbPayrollStatisticsFileRowDTO.Falt7b = falt7b;
            scbPayrollStatisticsFileRowDTO.Falt8a = falt8a;
            scbPayrollStatisticsFileRowDTO.Falt8b = falt8b;
            scbPayrollStatisticsFileRowDTO.Falt9a = falt9a;
            scbPayrollStatisticsFileRowDTO.Falt9b = falt9b;
            scbPayrollStatisticsFileRowDTO.Falt10aa = falt10aa;
            scbPayrollStatisticsFileRowDTO.Falt10ab = falt10ab;
            scbPayrollStatisticsFileRowDTO.Falt10ba = falt10ba;
            scbPayrollStatisticsFileRowDTO.Falt10bb = falt10bb;
            scbPayrollStatisticsFileRowDTO.Falt10ca = falt10ca;
            scbPayrollStatisticsFileRowDTO.Falt10cb = falt10cb;
            scbPayrollStatisticsFileRowDTO.Falt11a = falt11a;
            scbPayrollStatisticsFileRowDTO.Falt11b = falt11b;
            scbPayrollStatisticsFileRowDTO.Falt12a = falt12a;
            scbPayrollStatisticsFileRowDTO.Falt12b = falt12b;
            scbPayrollStatisticsFileRowDTO.Falt13a = falt13a;
            scbPayrollStatisticsFileRowDTO.Falt13b = falt13b;
            scbPayrollStatisticsFileRowDTO.Falt14a = falt14a;
            scbPayrollStatisticsFileRowDTO.Falt14b = falt14b;
            scbPayrollStatisticsFileRowDTO.Falt15aa = falt15aa;
            scbPayrollStatisticsFileRowDTO.Falt15ab = falt15ab;
            scbPayrollStatisticsFileRowDTO.Falt15ba = falt15ba;
            scbPayrollStatisticsFileRowDTO.Falt15bb = falt15bb;
            scbPayrollStatisticsFileRowDTO.Falt16a = falt16a;
            scbPayrollStatisticsFileRowDTO.Falt16b = falt16b;
            scbPayrollStatisticsFileRowDTO.Falt17a = falt17a;
            scbPayrollStatisticsFileRowDTO.Falt17b = falt17b;
            scbPayrollStatisticsFileRowDTO.Falt18a = falt18a;
            scbPayrollStatisticsFileRowDTO.Falt18b = falt18b;
            scbPayrollStatisticsFileRowDTO.Falt19a = falt19a;
            scbPayrollStatisticsFileRowDTO.Falt19b = falt19b;
            scbPayrollStatisticsFileRowDTO.Falt20a = falt20a;
            scbPayrollStatisticsFileRowDTO.Falt20b = falt20b;
            scbPayrollStatisticsFileRowDTO.Falt21a = falt21a;
            scbPayrollStatisticsFileRowDTO.Falt21b = falt21b;
            scbPayrollStatisticsFileRowDTO.Falt22a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt22b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt23a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt23b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt24a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt24b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt25a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt25b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt26a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt26b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt27a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt27b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt28a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt28b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt29a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt29b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt30a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt30b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt31a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt31b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt32a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt32b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt33a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt33b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt34a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt34b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt35a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt35b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt36a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt36b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt37a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt37b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt38a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt38b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt39a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt39b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt40a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt40b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt41a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt41b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt42a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt42b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt43a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt43b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt44a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt44b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt45a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt45b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt46a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt46b = string.Empty;
        }

        private void SetValueForSCB(CompEntities entities, int actorCompanyId, SCB_SLPPayrollStatisticsFileRowDTO scbPayrollStatisticsFileRowDTO, Employee employee, Employment employment, DateTime date, bool isHourlyPay, List<TimePayrollStatisticsDTO> timePayrollTransactionItems, List<VacationGroup> vacationGroups, List<int> payedProducts)
        {
            List<SCBPayrollStatisticsTransactionDTO> list = new List<SCBPayrollStatisticsTransactionDTO>();
            scbPayrollStatisticsFileRowDTO.scbPayrollStatisticsTransactionDTOs = new List<SCBPayrollStatisticsTransactionDTO>();

            string falt9a = "001";
            string falt9b = string.Empty;
            string falt7a = "003";
            string falt7b = string.Empty;
            string falt8a = "004";
            string falt8b = string.Empty;
            string falt6a = "051";
            string falt6b = string.Empty;
            string falt10aa = "002";
            string falt10ab = string.Empty;
            string falt10ba = "052";
            string falt10bb = string.Empty;
            string falt10ca = "053";
            string falt10cb = string.Empty;
            string falt12a = "054";
            string falt12b = string.Empty;
            string falt13a = "055";
            string falt13b = string.Empty;
            string falt14a = "057";
            string falt14b = string.Empty;
            string falt11a = "058";
            string falt11b = string.Empty;
            string falt15a = "700";
            string falt15b = string.Empty;
            string falt16a = "600";
            string falt16b = string.Empty;

            var totaltArbetadtid = timePayrollTransactionItems.Where(t => payedProducts.Contains(t.PayrollProductId) && !t.IsScheduleTransaction && !t.IsFromOtherPeriod);

            decimal totaltArbetadtidSumma = 0;
            if (totaltArbetadtid != null && totaltArbetadtid.Any())
            {
                totaltArbetadtidSumma = totaltArbetadtid.Sum(t => decimal.Divide(t.Quantity, 60));
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltArbetadtid.ToList(), "totaltArbetadtid"));
            }
            falt9b = Convert.ToInt32(totaltArbetadtidSumma).ToString();
            //(1) Bruttolön (2) Övertidsersättning och (1) Bruttolön (2) Mertid/fyllnadstid
            var totaltMerOverTid = timePayrollTransactionItems.Where(t => t.IsAddedOrOverTime());
            decimal totaltMerOverTidSumma = 0;
            if (totaltMerOverTid != null && totaltMerOverTid.Any())
            {
                totaltMerOverTidSumma = totaltMerOverTid.Sum(t => decimal.Divide(t.Quantity, 60));
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltMerOverTid.ToList(), "totaltMerOverTid"));
                totaltArbetadtidSumma = totaltArbetadtidSumma + totaltMerOverTidSumma;
                falt9b = Convert.ToInt32(totaltArbetadtidSumma).ToString();
            }
            falt10ab = totaltMerOverTidSumma.ToString("F").Replace(".", ",");

            var veckoarbetstid = employment.GetWorkTimeWeek(date);
            decimal totaltVeckoarbetstid = 0;
            if (veckoarbetstid != 0)
                totaltVeckoarbetstid = decimal.Divide(veckoarbetstid, 60);
            falt7b = totaltVeckoarbetstid.ToString("F").Replace(".", ",");

            var veckoarbetstidHel = employment.GetEmployeeGroup(date).RuleWorkTimeWeek;
            decimal totaltVeckoarbetstidHel = 0;
            if (veckoarbetstidHel != 0)
                totaltVeckoarbetstidHel = decimal.Divide(veckoarbetstidHel, 60);
            falt8b = totaltVeckoarbetstidHel.ToString("F").Replace(".", ",");

            decimal lon = 0;
            PayrollGroup payrollGroup = employment.GetPayrollGroup(date);

            if (payrollGroup != null)
            {
                if (!payrollGroup.PayrollGroupSetting.IsLoaded)
                    payrollGroup.PayrollGroupSetting.Load();

                if (!payrollGroup.PayrollGroupSetting.IsNullOrEmpty())
                {
                    var setting = payrollGroup.PayrollGroupSetting.FirstOrDefault(p => p.Type == (int)PayrollGroupSettingType.PayrollFormula);
                    if (setting != null && setting.IntData.HasValue && setting.IntData.Value != 0)
                    {
                        PayrollPriceFormulaResultDTO result = PayrollManager.EvaluatePayrollPriceFormula(entities, actorCompanyId, employee, employment, null, date, null, null, setting.IntData.Value);
                        if (result != null)
                        {
                            if (!isHourlyPay)
                                lon = Convert.ToInt32(result.Amount);
                            else
                                lon = Convert.ToInt32(result.Amount * 100);
                        }
                    }
                }
            }

            falt6b = lon.ToString("#").Replace(".", ",");

            //(1) Bruttolön (2) Övertidstillägg
            var totaltOverTidsTillagg = timePayrollTransactionItems.Where(t => t.IsOverTimeAddition());
            decimal totaltOverTidsTillaggSumma = 0;
            if (totaltOverTidsTillagg != null && totaltOverTidsTillagg.Any())
            {
                totaltOverTidsTillaggSumma = totaltOverTidsTillagg.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltOverTidsTillagg.ToList(), "totaltOverTidsTillagg"));
            }

            falt10bb = totaltOverTidsTillaggSumma.ToString("F").Replace(".", ",");

            //(1) Bruttolön(2) Övertidsersättning
            var totaltOverTidsErsattning = timePayrollTransactionItems.Where(t => t.IsOvertimeCompensation());
            decimal totaltOverTidsErsattningSumma = 0;
            if (totaltOverTidsErsattning != null && totaltOverTidsErsattning.Any())
                totaltOverTidsErsattningSumma = totaltOverTidsErsattning.Sum(t => t.Amount);
            falt10cb = totaltOverTidsErsattningSumma.ToString("F").Replace(".", ",");

            //(1) Bruttolön (2) OB-tillägg
            var totaltOBSkiftTillagg = timePayrollTransactionItems.Where(t => t.IsOBAddition());
            decimal totaltOBSkiftTillaggSumma = 0;
            if (totaltOBSkiftTillagg != null && totaltOBSkiftTillagg.Any())
            {
                totaltOBSkiftTillaggSumma = totaltOBSkiftTillagg.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltOBSkiftTillagg.ToList(), "totaltOBSkiftTillagg"));
            }

            falt12b = totaltOBSkiftTillaggSumma.ToString("F").Replace(".", ",");

            //(1) Förmån men inte (2) Motbokning förmån eller (2) (1) Bruttolön Jour
            var totaltJourBeredskap = timePayrollTransactionItems.Where(t => t.IsDutyAndBenefitNotInvert());
            decimal totaltJourBeredskapSumma = 0;
            if (totaltJourBeredskap != null && totaltJourBeredskap.Any())
            {
                totaltJourBeredskapSumma = totaltJourBeredskap.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltJourBeredskap.ToList(), "totaltJourBeredskap"));
            }
            falt13b = totaltJourBeredskapSumma.ToString("F").Replace(".", ",");

            //(1) Förmån men inte (2) Motbokning förmån
            var totaltJourBeredskapDaravFormaner = timePayrollTransactionItems.Where(t => t.IsBenefitAndNotInvert());
            decimal totaltJourBeredskapDaravFormanerSumma = 0;
            if (totaltJourBeredskapDaravFormaner != null && totaltJourBeredskapDaravFormaner.Any())
            {
                totaltJourBeredskapDaravFormanerSumma = totaltJourBeredskapDaravFormaner.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltJourBeredskapDaravFormaner.ToList(), "totaltJourBeredskapDaravFormaner"));
            }
            falt14b = totaltJourBeredskapDaravFormanerSumma.ToString("F").Replace(".", ",");

            //(1) Bruttolön (2) Ackord
            var totaltPrestationslon = timePayrollTransactionItems.Where(t => t.IsContract());
            decimal totaltPrestationslonSumma = 0;
            if (totaltPrestationslon != null && totaltPrestationslon.Any())
            {
                totaltPrestationslonSumma = totaltPrestationslon.Sum(t => t.Amount);
                list.AddRange(CreateSCB_PayrollStatisticsTransactionDTO(employee, totaltPrestationslon.ToList(), "totaltPrestationslon"));
            }
            falt11b = totaltPrestationslonSumma.ToString("F").Replace(".", ",");

            var vacationGroup = employment.GetVacationGroup(date, vacationGroups);
            if (vacationGroup != null && !vacationGroup.VacationGroupSE.IsNullOrEmpty())
            {
                var vacationCalculationContainer = PayrollManager.CalculateVacationSR(entities, actorCompanyId, date, vacationGroup, vacationGroup.VacationGroupSE.FirstOrDefault(), employee, employment);
                decimal semesterratt = 0;
                if (vacationCalculationContainer != null && vacationCalculationContainer.FormulaResult != null && vacationCalculationContainer.FormulaResult.FirstOrDefault() != null)
                    semesterratt = vacationCalculationContainer.FormulaResult.FirstOrDefault().Value;

                falt16b = Convert.ToInt32(semesterratt).ToString();
            }

            //(1) Tillsvidare (2) Tidsbegränsad
            List<EmploymentTypeDTO> employmentTypes = EmployeeManager.GetEmploymentTypes(reportResult.ActorCompanyId);
            bool tillsvidareAnstalld = employment.GetEmploymentType(employmentTypes) == (int)TermGroup_EmploymentType.SE_Permanent;

            falt15b = tillsvidareAnstalld ? "1" : "2";

            scbPayrollStatisticsFileRowDTO.scbPayrollStatisticsTransactionDTOs.AddRange(list);

            scbPayrollStatisticsFileRowDTO.Falt1a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt1b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt2a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt2b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt3a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt3b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt4a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt4b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt5a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt5b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt6a = falt6a;
            scbPayrollStatisticsFileRowDTO.Falt6b = falt6b;
            scbPayrollStatisticsFileRowDTO.Falt7a = falt7a;
            scbPayrollStatisticsFileRowDTO.Falt7b = falt7b;
            scbPayrollStatisticsFileRowDTO.Falt8a = falt8a;
            scbPayrollStatisticsFileRowDTO.Falt8b = falt8b;
            scbPayrollStatisticsFileRowDTO.Falt9a = falt9a;
            scbPayrollStatisticsFileRowDTO.Falt9b = falt9b;
            scbPayrollStatisticsFileRowDTO.Falt10aa = falt10aa;
            scbPayrollStatisticsFileRowDTO.Falt10ab = falt10ab;
            scbPayrollStatisticsFileRowDTO.Falt10ba = falt10ba;
            scbPayrollStatisticsFileRowDTO.Falt10bb = falt10bb;
            scbPayrollStatisticsFileRowDTO.Falt10ca = falt10ca;
            scbPayrollStatisticsFileRowDTO.Falt10cb = falt10cb;
            scbPayrollStatisticsFileRowDTO.Falt11a = falt11a;
            scbPayrollStatisticsFileRowDTO.Falt11b = falt11b;
            scbPayrollStatisticsFileRowDTO.Falt12a = falt12a;
            scbPayrollStatisticsFileRowDTO.Falt12b = falt12b;
            scbPayrollStatisticsFileRowDTO.Falt13a = falt13a;
            scbPayrollStatisticsFileRowDTO.Falt13b = falt13b;
            scbPayrollStatisticsFileRowDTO.Falt14a = falt14a;
            scbPayrollStatisticsFileRowDTO.Falt14b = falt14b;
            scbPayrollStatisticsFileRowDTO.Falt15aa = falt15a;
            scbPayrollStatisticsFileRowDTO.Falt15ab = falt15b;
            scbPayrollStatisticsFileRowDTO.Falt16a = falt16a;
            scbPayrollStatisticsFileRowDTO.Falt16b = falt16b;
            scbPayrollStatisticsFileRowDTO.Falt17a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt17b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt18a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt18b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt19a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt19b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt20a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt20b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt21a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt21b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt22a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt22b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt23a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt23b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt24a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt24b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt25a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt25b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt26a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt26b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt27a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt27b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt28a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt28b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt29a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt29b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt30a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt30b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt31a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt31b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt32a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt32b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt33a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt33b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt34a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt34b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt35a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt35b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt36a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt36b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt37a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt37b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt38a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt38b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt39a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt39b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt40a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt40b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt41a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt41b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt42a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt42b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt43a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt43b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt44a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt44b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt45a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt45b = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt46a = string.Empty;
            scbPayrollStatisticsFileRowDTO.Falt46b = string.Empty;
        }

        private string SetString(string str, int limit = 0, string fillchar = " ", bool fromRight = true)
        {
            // Fill zeroes or empties before value
            string mystring = !string.IsNullOrEmpty(str) ? str : string.Empty;
            mystring = mystring.Trim();

            //Remove ,
            mystring = mystring.Replace(",", "");

            for (int i = 0; i < limit; i++)
            {
                if (mystring.Length < limit)
                {
                    mystring = fillchar + mystring;
                }
            }

            if (fromRight && mystring.Length > limit && limit > 0)
                return mystring.Substring(mystring.Length - limit);
            else if (!fromRight && mystring.Length > limit && limit > 0)
                return mystring.Substring(0, limit);

            return mystring;
        }

        #endregion
    }
}
