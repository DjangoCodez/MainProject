using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models
{
    public class PolScheduleTransaction : PolTransactionBase
    {
        public PolScheduleTransaction() { }

        public PolScheduleTransaction(SalaryExportCompany company, SalaryExportEmployee employee, SalaryExportSchedule schedule)
        {
            Foretag = int.Parse(company.CompanyCode);
            AnstNr = int.Parse(employee.EmployeeNr);
            Begynnelsedatum = schedule.Date;
            Dagkod = schedule.ScheduleHours;
            Period = schedule.Date.AddMonths(1);
        }

        private string ForetagFormatted => Foretag.ToString().Length < 2 ? Foretag.ToString().PadLeft(2, '0') : Foretag.ToString().Substring(Foretag.ToString().Length - 2, 2);

        // Startposition 3, längd 2, numeriskt
        [Required]
        public int TransTyp { get; set; } = 43;

        // Startposition 18, längd 6, numeriskt
        [Required]
        public DateTime Begynnelsedatum { get; set; }
        private string begynnelsedatumFormatted => Begynnelsedatum.ToString("yyMMdd");

        // Startposition 24, längd 4
        public string Arbetstidsschema { get; set; } = "    "; // 4 spaces

        // Startposition 28, längd 6
        public DateTime? Slutdatum { get; set; }
        private string slutdatumFormatted => Slutdatum?.ToString("yyMMdd") ?? Begynnelsedatum.ToString("yyMMdd");

        /// <summary>
        /// Schemat decimalt i timmar, ex 8 timmar och 30 minuter = 08,50
        /// Startposition 34, längd 5, numeriskt
        /// Max 7 (mån-sön) x 5 tecken motsvarande den aktuella dagkoden enligt tabell 22, Dagkoder i LA (Styrdata > Schema, dagkoder > Dagkoder). Arbetsfri dag anges med 5 asterisker (*).</summary>
        public decimal Dagkod { get; set; }
        public string dagkodFormatted
        {
            get
            {
                if (Dagkod == 0)
                {
                    return "*****";
                }

                // 5 tecken. 8 timmar ska bli 08,00
                return Dagkod.ToString("00.00").Replace(".", ",");
            }
        }

        // Exempel 1: Rapportering av dagkod för en hel månad, med en vecka per rad.
        // Dagkoden som används i detta exempel reflekterar arbetstiden för arbetade dagar,
        // dvs 08,00 (fem tecken). Månaden inleds med två arbetsfria dagar, lör-sön 170301-170302.
        //03434040000080007170301    170302********** (här rapporteras två arbetsfria dagar)
        //03434040000080007170303    17030908,0008,0008,0008,0008,00**********
        //03434040000080007170310    17031608,0008,0008,0008,0008,00**********
        //03434040000080007170317    17032308,0008,0008,0008,0008,00**********
        //03434040000080007170324    17033008,0008,0008,0008,0008,00**********
        //03434040000080007170331    17033108,00

        //Exempel 2: Rapportering av dagkod dag för dag, med en kalenderdag per rad.
        //Dagkoden som används i detta exempel är 160 + två blanktecken (mellanslag),
        //dvs i denna dagkod finns ingen direkt koppling till arbetstiden.
        //03434040000080007170301    170301***** (här rapporteras en arbetsfri dag)
        //03434040000080007170302    170302160<blankblank>
        //03434040000080007170303    170303160<blankblank>
        //03434040000080007170304    170304160<blankblank>
        //03434040000080007170305    170305160<blankblank>

        public string ToPolString()
        {
            var sb = new StringBuilder();
            sb.Append(ForetagFormatted);
            sb.Append(TransTyp.ToString().PadLeft(2, '0'));
            sb.Append(periodFormatted);
            sb.Append(anstNrFormatted);
            sb.Append(begynnelsedatumFormatted);
            sb.Append(Arbetstidsschema);
            sb.Append(slutdatumFormatted);
            sb.Append(dagkodFormatted);
            sb.AppendLine();
            return sb.ToString();
        }

        public string ToPolStringForOneWeek(DateTime begynnelsedatum, DateTime slutdatum, List<PolScheduleTransaction> polScheduleTransactions)
        {
            var sb = new StringBuilder();
            sb.Append(ForetagFormatted);
            sb.Append(TransTyp.ToString().PadLeft(2, '0'));
            sb.Append(periodFormatted);
            sb.Append(anstNrFormatted);
            sb.Append(begynnelsedatum.ToString("yyMMdd"));
            sb.Append("    ");
            sb.Append(slutdatum.ToString("yyMMdd"));
            var dates = CalendarUtility.GetDates(begynnelsedatum, slutdatum);

            foreach (var date in dates)
            {
                if (date == this.Begynnelsedatum)
                    sb.Append(dagkodFormatted);
                else
                {
                    var matched = polScheduleTransactions.FirstOrDefault(x => x.Begynnelsedatum == date);

                    if (matched != null)
                        sb.Append(matched.dagkodFormatted);
                    else
                        sb.Append(new PolScheduleTransaction().dagkodFormatted);
                }                
            }
            return sb.ToString();   
        }

        public bool IsValid()
        {
            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateObject(this, new ValidationContext(this), validationResults, true))
            {
                return false;
            }
            return true;
        }

        public bool PolStringIsValid(string polString)
        {
            if (polString.Length != 63)
            {
                return false;
            }
            return true;
        }

        public static Dictionary<DateTime, List<PolScheduleTransaction>> GetTransactionsForWeeks(List<PolScheduleTransaction> transactions)
        {
            return transactions.GroupBy(x => CalendarUtility.GetBeginningOfWeek(x.Begynnelsedatum)).ToDictionary(k => k.Key, v => v.ToList());
        }

        public static string GetPolStringForOneWeek(List<PolScheduleTransaction> polScheduleTransactionsForOneWeek)
        {
            var result = polScheduleTransactionsForOneWeek.First().ToPolString();
            foreach (var day in polScheduleTransactionsForOneWeek.Skip(1))
            {
                result = result + day.dagkodFormatted;
            }

            return result;
        }

    }
}
