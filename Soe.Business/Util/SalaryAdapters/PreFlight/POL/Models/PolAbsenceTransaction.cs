using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models
{
    public class PolAbsenceTransaction : PolTransactionBase
    {
        public PolAbsenceTransaction() { }

        public PolAbsenceTransaction(SalaryExportCompany company, SalaryExportEmployee employee, SalaryExportTransaction transaction)
        {
            Foretag = int.Parse(company.CompanyCode);
            AnstNr = int.Parse(employee.EmployeeNr);
            Begynnelsedatum = transaction.Date;
            KalenderDagar = null;
            Slutdatum = transaction.Date;
            Period = transaction.Date.AddMonths(1);
            OrsaksKod = transaction.Code;
            FranvaroTid = transaction.OfFullDayAbsence ? 0 : transaction.Hours;
            OnFreeDay = transaction.Hours > 0 ? false : true;
            IsPaidVacation = transaction.IsPaidVacation;
            ForPeriod = ParseForPeriod(transaction.ExternalCode);
        }

        public bool OnFreeDay { get; set; }
        public bool IsPaidVacation { get; set; }
        public bool ForPeriod { get; set; }

        private string ForetagFormatted => Foretag.ToString().Length < 2 ? Foretag.ToString().PadLeft(2, '0') : Foretag.ToString().Substring(Foretag.ToString().Length - 2, 2);

        // Startposition 3, längd 2, numeriskt
        // Konstant ”45”
        [Required]
        public int TransTyp { get; set; } = 45;

        // Startposition 18, längd 6, numeriskt
        // ÅÅMMDD Begynnelsedatum för frånvaron.
        [Required]
        public DateTime Begynnelsedatum { get; set; }
        private string begynnelsedatumFormatted => Begynnelsedatum.ToString("yyMMdd");

        // Startposition 24, längd 2, numeriskt
        // Orsakskod enligt Nya POL
        [Required]
        public string OrsaksKod { get; set; }
        [RegularExpression(@"\d{2}")]
        private string orsaksKodFormatted => OrsaksKod.PadLeft(2, '0');

        // Startposition 26, längd , numeriskt
        // Endast vid frånvaro del av dag.
        // För semester (orsak 05) kan även antal uttagna dagar anges.
        // I samband med löneberäkningen multipliceras då angivet antal med semesterkvoten enligt avdragsunderlaget för frånvaro.
        public decimal? FranvaroTid { get; set; }

        private string franvaroTidFormatted => FranvaroTid.HasValue? FranvaroTid.Value.ToString("0.00").Replace(".", "").Replace(",","").PadLeft(5, '0') : string.Empty.PadLeft(5, ' ');

        // Startposition 31, längd 2
        public string TjlFaktor { get; } = "  ";

        // Startposition 66, längd 3, numeriskt
        // Blankt då slutdatum är att föredra.
        public int? KalenderDagar { get; set; }
        private string kalenderDagarFormatted => KalenderDagar?.ToString().PadLeft(3, '0') ?? "".PadLeft(3, ' ');

        // Startposition 36, längd 6
        // ÅÅMMDD
        public DateTime? Slutdatum { get; set; }
        private string slutdatumFormatted => Slutdatum?.ToString("yyMMdd") ?? "".PadLeft(6, '0');

        // Startposition 42, längd 2, numeriskt
        [Range(0, 99)]
        public int? Procent1 { get; set; }
        private string procent1Formatted => Procent1.HasValue ? Procent1.ToString().PadLeft(2, '0') : string.Empty.PadRight(2, ' ');


        // Startposition 44, längd 1, numeriskt
        public int? FranvaroTyp { get; set; }
        private string franvaroTypFormatted => FranvaroTyp.HasValue ? FranvaroTyp.ToString().PadLeft(1, '0') : string.Empty.PadRight(2, ' ');

        // Startposition 45, längd 2, numeriskt
        [Range(0, 99)]
        public int? Procent2 { get; set; }
        private string procent2Formatted => Procent2.HasValue ? Procent2.ToString().PadLeft(2, '0') : string.Empty.PadRight(2, ' ');

        public bool Validate()
        {
            // Validate the model
            var context = new ValidationContext(this, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(this, context, results, true);
        }

        //Exempelfil:
        //034540400000028981403261100000 14032600 0000 (kompledighet hel dag 140326)
        //034540400000235961403031100400 14030300 0000 (kompledighet del av dag, 4 tim, 140303)
        //034540400000235961403080200400 14030800 0000 (sjuk del av dag, 4 tim karens, 140308)
        //034540400000235961403090200000 14031600 0000 (sjuk 140309-140316)

        public string ToPolString()
        {
            var polString = new StringBuilder();

            // We will later also let through transactions with IsPaidVacation == true when M&S tell us to (this comment can then be removed!)
            // if (!OnFreeDay || IsPaidVacation)
            // Since it can differ between collective agreements we added ForPeriod instead of IsPaidVacation. (External code: "POL:period")
            if (!OnFreeDay || ForPeriod)
            {
                polString.Append(ForetagFormatted);
                polString.Append(TransTyp.ToString().PadLeft(2, '0'));
                polString.Append(periodFormatted);
                polString.Append(anstNrFormatted);
                polString.Append(begynnelsedatumFormatted);
                polString.Append(orsaksKodFormatted);
                polString.Append(franvaroTidFormatted);
                polString.Append(TjlFaktor);
                polString.Append(kalenderDagarFormatted);
                polString.Append(slutdatumFormatted);
                if (Procent1.HasValue || FranvaroTyp.HasValue || Procent2.HasValue)
                {
                    polString.Append(procent1Formatted);
                    polString.Append(franvaroTypFormatted);
                    polString.Append(procent2Formatted);
                }
                polString.AppendLine();
            }
            
            return polString.ToString();
        }

        private bool ParseForPeriod(string externalCode)
        {
            if (externalCode.ToLower().Contains("pol:") && externalCode.Split(':')[1].Trim().ToLower() == "period")
            {
                return true;
            }
            return false;
        }
    }
}
