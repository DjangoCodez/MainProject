using SoftOne.Soe.Business.ArbetsgivarintygNu;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class ArbetsgivarintygPunktNu
    {
        public string ApiNyckel;
        public string ArbetsgivarId;
        public EndpointAddress endPoint;
        public ArbetsgivarintygServiceClient client;

        public ArbetsgivarintygPunktNu(bool isTest, string apiNyckel, string arbetsgivarId)
        {
            var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
            if (isTest)
            {
                ApiNyckel = "SNybvKQ6fgcz35cw2xB7LaJ5IWR7W8";
                ArbetsgivarId = "ftg4431";
                endPoint = new EndpointAddress("https://arbetsgivarintyg-test.rd.sverigesakassor.se/HR_SystemService/ArbetsgivarintygService.svc");

                //https://arbetsgivarintyg-test.rd.sverigesakassor.se för att logga in emot
            }
            else
            {
                ApiNyckel = apiNyckel;
                ArbetsgivarId = arbetsgivarId;
                endPoint = new EndpointAddress("https://aip.sverigesakassor.se/HR_SystemService/ArbetsgivarintygService.svc");
            }

            client = new ArbetsgivarintygServiceClient(binding, endPoint);
        }


        public ActionResult SendArbetsgivarIntyg(CertificateOfEmployment certificateOfEmployment)
        {
            var request = InitRequest();

            List<ApiCertificate> arbetsgivarIntyg = new List<ApiCertificate>();

            foreach (var certificateOfEmploymentEmployee in certificateOfEmployment.CertificateOfEmploymentEmployees)
            {
                var intyg = InitArbetsgivarintyg(certificateOfEmployment);
                AddArbetstagareInformation(certificateOfEmploymentEmployee, intyg);
                AddAnstallningInformation(certificateOfEmploymentEmployee, intyg);
                arbetsgivarIntyg.Add(intyg);
            }
            request.Request.Arbetsgivarintyg = arbetsgivarIntyg.ToArray();

            var response = client.SaveArbetsgivarintygService(request);
            return new ActionResult(response.Response.ResultMessage);

        }

        private ImportRequest InitServerRequest()
        {
            ImportRequest request = new ImportRequest();
            request.Autentisering = new ApiAuthentication() { SkickatTidpunkt = DateTime.Now, ApiNyckel = this.ApiNyckel, ArbetsgivareId = this.ArbetsgivarId, SkickatTidpunktSpecified = true };
            request.AGPVersion = new APIAGPVersion() { Version = "1" };
            request.SoftwareInfo = new APISoftwareInfo() { Supplier = "SoftOne", Build = "1.0", Version = "." };
            return request;
        }

        private ArbetsgivarintygServiceRequest InitRequest()
        {
            ArbetsgivarintygServiceRequest request = new ArbetsgivarintygServiceRequest();
            request.Request = new ImportRequest();
            request.Request.Autentisering = new ApiAuthentication() { SkickatTidpunkt = DateTime.Now, ApiNyckel = this.ApiNyckel, ArbetsgivareId = this.ArbetsgivarId, SkickatTidpunktSpecified = true };
            request.Request.AGPVersion = new APIAGPVersion() { Version = "1" };
            request.Request.SoftwareInfo = new APISoftwareInfo() { Supplier = "SoftOne", Build = "1.0", Version = "." };
            return request;
        }

        private ApiCertificate InitArbetsgivarintyg(CertificateOfEmployment certificateOfEmployment)
        {
            ApiCertificate arbetsgivarIntyg = new ApiCertificate();
            Adress adress = new Adress() { Gatuadress = certificateOfEmployment.Street, Postnummer = certificateOfEmployment.ZipCode, Ort = certificateOfEmployment.City, CareOf = certificateOfEmployment.CareOf };
            string namn = certificateOfEmployment.CompanyName;
            string epost = certificateOfEmployment.Email;
            string organisationsnummer = certificateOfEmployment.OrgNr;
            string telefon = certificateOfEmployment.Telephone;

            arbetsgivarIntyg.Arbetsgivare = new ApiCertificate_CompanyBlock()
            {
                Adress = adress,
                Namn = namn,
                Epostadress = !string.IsNullOrEmpty(epost) ? epost : null,
                Organisationsnummer = organisationsnummer,
                Telefonnummer = !string.IsNullOrEmpty(telefon) ? telefon : null,
            };

            return arbetsgivarIntyg;
        }

        private void AddArbetstagareInformation(CertificateOfEmploymentEmployee certificateOfEmploymentEmployee, ApiCertificate arbetsgivarIntyg)
        {
            arbetsgivarIntyg.Arbetstagare = new ApiCertificate_WorkerBlock()
            {
                Fornamn = certificateOfEmploymentEmployee.FirstName,
                Efternamn = certificateOfEmploymentEmployee.LastName,
                Personnummer = certificateOfEmploymentEmployee.SocialSec,
                Telefonnummer = !string.IsNullOrEmpty(certificateOfEmploymentEmployee.Phone) ? certificateOfEmploymentEmployee.Phone : null,
                Epostadress = !string.IsNullOrEmpty(certificateOfEmploymentEmployee.Email) ? certificateOfEmploymentEmployee.Email : null,
            };

            if (!string.IsNullOrEmpty(certificateOfEmploymentEmployee.Phone))
                arbetsgivarIntyg.Arbetstagare.SkickaSMS = true;
        }

        private void AddAnstallningInformation(CertificateOfEmploymentEmployee certificateOfEmploymentEmployee, ApiCertificate arbetsgivarIntyg)
        {
            CertificateOfEmploymentEmployment certificateOfEmploymentMergedEmployment = certificateOfEmploymentEmployee.CertificateOfEmploymentMergedEmployments.LastOrDefault();
            arbetsgivarIntyg.Anstallning = new ApiCertificate_EmploymentBlock()
            {
                AnstallningstidPeriod = new DatumOmfattning() { Start = certificateOfEmploymentMergedEmployment.DateFrom, Slut = certificateOfEmploymentMergedEmployment.DateTo != CalendarUtility.DATETIME_DEFAULT ? certificateOfEmploymentMergedEmployment.DateTo : (DateTime?)null, StartSpecified = true, SlutSpecified = certificateOfEmploymentMergedEmployment.DateTo != CalendarUtility.DATETIME_DEFAULT ? true : false },
                Befattning = certificateOfEmploymentMergedEmployment.PositionName,
                ArbetstidOmfattning = certificateOfEmploymentMergedEmployment.WorkPercent,
                ArbetstidOmfattningSpecified = true,
                FortfarandeAnstalld = certificateOfEmploymentMergedEmployment.DateTo == CalendarUtility.DATETIME_DEFAULT,
                FortfarandeAnstalldSpecified = certificateOfEmploymentMergedEmployment.DateTo == CalendarUtility.DATETIME_DEFAULT,
                TjanstledigSpecified = false,
                //TODO                   
            };      
            
            if (!arbetsgivarIntyg.Anstallning.FortfarandeAnstalld.Value)
            {
                arbetsgivarIntyg.Anstallning.ArbetstidOmfattning = null;
                arbetsgivarIntyg.Anstallning.ArbetstidOmfattningSpecified = false;
            }

            arbetsgivarIntyg.Upphorandeorsak = new ApiCertificate_ReasonEmploymentEndedBlock()
            {
                Orsak = GetOrsak(certificateOfEmploymentMergedEmployment),
                OrsakSpecified = GetOrsak(certificateOfEmploymentMergedEmployment) != Orsak.Ingen ? true : false,
                AnnanOrsak = GetOrsak(certificateOfEmploymentMergedEmployment) == Orsak.Ingen ? certificateOfEmploymentMergedEmployment.EndReasonName : null,
            };

            arbetsgivarIntyg.Lon = new ApiCertificate_SalaryBlock()
            {
                Ar = certificateOfEmploymentMergedEmployment.PayrollYear,
                ArSpecified = true,
                TypAvLon = certificateOfEmploymentMergedEmployment.SalaryType == TermGroup_PayrollExportSalaryType.Monthly ? LoneTyp.Manadslon : LoneTyp.Timlon,
                TypAvLonSpecified = true,
                Belopp = certificateOfEmploymentMergedEmployment.PayrollAmount,
                BeloppSpecified = true,
                Overtidstillagg = certificateOfEmploymentMergedEmployment.OverTimePerHour,
                OvertidstillaggSpecified = certificateOfEmploymentMergedEmployment.OverTimePerHour != 0,
                Mertidstillagg = certificateOfEmploymentMergedEmployment.AddedTimePerHour,
                MertidstillaggSpecified = certificateOfEmploymentMergedEmployment.AddedTimePerHour != 0,
                VarierandeTimlon = certificateOfEmploymentMergedEmployment.OverTimeOrAddedTimeHasDifferentHourlySalary,
                VarierandeTimlonSpecified = true,
            };

            List<ApiCertificate_SalaryBlock_ComplementWorkedHoursRow> otherRows = new List<ApiCertificate_SalaryBlock_ComplementWorkedHoursRow>();
            foreach (var other in certificateOfEmploymentMergedEmployment.OtherGroupTrans.Where(w => w.OtherPayrollType != OtherPayrollType.Inget && w.Amount != 0).GroupBy(g => $"{g.DateFrom.Year}#{g.DateFrom.Month}#{g.OtherPayrollType}#{g.ShowHours}").OrderByDescending(o => o.Key))
            {
                var first = other.First();
                var row = new ApiCertificate_SalaryBlock_ComplementWorkedHoursRow()
                {
                    Ar = first.DateFrom.Year,
                    ArSpecified = true,
                    Manad = first.DateFrom.Month,
                    ManadSpecified = true,
                    Belopp = other.Sum(s => s.Amount),
                    BeloppSpecified = true,
                    Lonetillaggstyp = (LoneTyper)first.OtherPayrollType,
                    LonetillaggstypSpecified = true,
                };

                if (first.ShowHours)
                {
                    row.TimmarSpecified = true;
                    row.Timmar = decimal.Divide(other.Sum(s => s.Quantity), 60);
                }

                otherRows.Add(row);
            }

            foreach (var other in certificateOfEmploymentMergedEmployment.OtherGroupTrans.Where(w => w.OtherPayrollType == OtherPayrollType.Inget && w.Amount != 0).OrderByDescending(o => o.DateFrom))
            {
                var row = new ApiCertificate_SalaryBlock_ComplementWorkedHoursRow()
                {
                    Ar = other.DateFrom.Year,
                    ArSpecified = true,
                    Manad = other.DateFrom.Month,
                    ManadSpecified = true,
                    Belopp = other.Amount,
                    BeloppSpecified = true,
                    Lonetillaggstyp = LoneTyper.AndraFormaner,
                    LonetillaggstypSpecified = true,
                    Beskrivning = other.Name,
                };

                if (other.ShowHours)
                {
                    row.TimmarSpecified = true;
                    row.Timmar = decimal.Divide(other.Quantity, 60);
                }

                otherRows.Add(row);
            }
            arbetsgivarIntyg.Lon.AndraLonetillaggLista = otherRows.ToArray();
            arbetsgivarIntyg.Arbetstid = new ApiCertificate_WorkingHoursBlock();

            if (certificateOfEmploymentMergedEmployment.WorkPercent == 100)
            {
                arbetsgivarIntyg.Arbetstid.ArbetstidHeltidTimmar = decimal.Divide(certificateOfEmploymentMergedEmployment.WorkTime, 60);
                arbetsgivarIntyg.Arbetstid.ArbetstidHeltidTimmarSpecified = true;
                arbetsgivarIntyg.Arbetstid.Typ = ArbetstidTyp.Heltid;
            }
            else if (certificateOfEmploymentMergedEmployment.WorkPercent < 100 && certificateOfEmploymentMergedEmployment.EmploymentType == TermGroup_EmploymentType.SE_Permanent)
            {
                arbetsgivarIntyg.Arbetstid.ArbetstidDeltidTimmar = decimal.Divide(certificateOfEmploymentMergedEmployment.WorkTime, 60);
                arbetsgivarIntyg.Arbetstid.ProcentAvHeltid = certificateOfEmploymentMergedEmployment.WorkPercent;
                arbetsgivarIntyg.Arbetstid.ProcentAvHeltidSpecified = true;
                arbetsgivarIntyg.Arbetstid.ArbetstidDeltidTimmarSpecified = true;
                arbetsgivarIntyg.Arbetstid.Typ = ArbetstidTyp.Deltid;
            }
            else
            {
                arbetsgivarIntyg.Arbetstid.Typ = ArbetstidTyp.VarierandeArbetstid;
            }

            arbetsgivarIntyg.Arbetstid.TypSpecified = true;

            arbetsgivarIntyg.SpeciellAnstallningsinformation = new ApiCertificate_SpecificEmploymentInformationBlock()
            {
                Skiftarbete = certificateOfEmploymentMergedEmployment.VariableWeekTime,
                SkiftarbeteSpecified = true,
                AnstalldBemanning = false, //TODO
                AnstalldBemanningSpecified = true,
            };

            arbetsgivarIntyg.Anstallningsform = new ApiCertificate_EmploymentFormBlock()
            {
                Typ = GetAnstallningsformTyp(certificateOfEmploymentMergedEmployment),
            };

            if (arbetsgivarIntyg.Anstallningsform.Typ == AnstallningsformTyp.Provanstallning && certificateOfEmploymentMergedEmployment.DateTo != CalendarUtility.DATETIME_DEFAULT)
            {
                arbetsgivarIntyg.Anstallningsform.ProvanstallningSlutdatum = certificateOfEmploymentMergedEmployment.DateTo;
                arbetsgivarIntyg.Anstallningsform.ProvanstallningSlutdatumSpecified = true;
            }
            else if (arbetsgivarIntyg.Anstallningsform.Typ == AnstallningsformTyp.Tidsbegransad && certificateOfEmploymentMergedEmployment.DateTo != CalendarUtility.DATETIME_DEFAULT)
            {
                arbetsgivarIntyg.Anstallningsform.TidsbegransadAnstallningSlutdatum = certificateOfEmploymentMergedEmployment.DateTo;
                arbetsgivarIntyg.Anstallningsform.TidsbegransadAnstallningSlutdatumSpecified = true;
            }

            List<ApiCertificate_WorkedHoursBlock_WorkedHoursRow> arbetadTidManader = new List<ApiCertificate_WorkedHoursBlock_WorkedHoursRow>();
            foreach (var group in certificateOfEmploymentMergedEmployment.GroupTrans)
            {
                arbetadTidManader.Add(new ApiCertificate_WorkedHoursBlock_WorkedHoursRow()
                {
                    Manad = group.DateFrom.Month,
                    ManadSpecified = true,
                    Ar = group.DateFrom.Year,
                    ArSpecified = true,
                    ArbetadeTimmar = decimal.Round(decimal.Divide(group.WorkSum, 60),2),
                    ArbetadeTimmarSpecified = true,
                    Franvaro = decimal.Round(decimal.Divide(group.AbsenceSum, 60), 2),
                    FranvaroSpecified = group.AbsenceSum != 0,
                    Mertid = decimal.Round(decimal.Divide(group.AddedTimeSum, 60), 2),
                    MertidSpecified = group.AddedTimeSum != 0,
                    Overtid = decimal.Round(decimal.Divide(group.OverTimeSum, 60),2),
                    OvertidSpecified = group.OverTimeSum != 0
                });
            }

            if (!certificateOfEmploymentMergedEmployment.GroupTrans.IsNullOrEmpty())
            {
                arbetsgivarIntyg.ArbetadTid = new ApiCertificate_WorkedHoursBlock()
                {
                    ArbetadTidManader = arbetadTidManader.ToArray(),
                    ArbetadTidDatumOmfattning = new DatumOmfattning() { Start = certificateOfEmploymentMergedEmployment.GroupTrans.First().DateFrom, Slut = certificateOfEmploymentMergedEmployment.GroupTrans.Last().DateFrom.AddMonths(1).AddDays(-1), StartSpecified = true, SlutSpecified = true },

                };
            }
        }

        public static int GetOrsakNumber(CertificateOfEmploymentEmployment certificateOfEmploymentMergedEmployment)
        {
            return (int)GetOrsak(certificateOfEmploymentMergedEmployment);
        }
        private static Orsak GetOrsak(CertificateOfEmploymentEmployment certificateOfEmploymentMergedEmployment)
        {
            if (!string.IsNullOrEmpty(certificateOfEmploymentMergedEmployment.EndReasonName))
            {
                var reason = certificateOfEmploymentMergedEmployment.EndReasonName.ToLower();

                if (reason.Contains("brist"))
                    return Orsak.UppsagdArbetsbrist;

                if (reason.Contains("begr") && reason.Contains("tid"))
                    return Orsak.TidsbegransadAnstallning;

                if (reason.Contains("prov") && reason.Contains("givar"))
                    return Orsak.SlutProvanstallningArbetsgivareBeslut;

                if (reason.Contains("prov") && reason.Contains("anst"))
                    return Orsak.SlutProvanstallningAnstalldBeslut;

                if (reason.Contains("konku"))
                    return Orsak.Konkurs;

                if (reason.Contains("egen"))
                    return Orsak.EgenBegaran;
            }
            return Orsak.Ingen;

        }

        private AnstallningsformTyp GetAnstallningsformTyp(CertificateOfEmploymentEmployment certificateOfEmploymentMergedEmployment)
        {
            switch (certificateOfEmploymentMergedEmployment.EmploymentType)
            {
                case TermGroup_EmploymentType.Unknown:
                    return AnstallningsformTyp.Ingen;
                case TermGroup_EmploymentType.SE_Probationary:
                    return AnstallningsformTyp.Provanstallning;
                case TermGroup_EmploymentType.SE_Substitute:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_SubstituteVacation:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_Permanent:
                    return AnstallningsformTyp.Tillsvidare;
                case TermGroup_EmploymentType.SE_FixedTerm:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_Seasonal:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_SpecificWork:
                    return AnstallningsformTyp.Behovsanstalld;
                case TermGroup_EmploymentType.SE_Trainee:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_NormalRetirementAge:
                    return AnstallningsformTyp.Tillsvidare;
                case TermGroup_EmploymentType.SE_CallContract:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_LimitedAfterRetirementAge:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_FixedTerm14days:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_Apprentice:
                    return AnstallningsformTyp.Tidsbegransad;
                case TermGroup_EmploymentType.SE_SpecialFixedTerm:
                    return AnstallningsformTyp.Tidsbegransad;
                default:
                    return AnstallningsformTyp.Ingen;
            }
        }
    }
}


