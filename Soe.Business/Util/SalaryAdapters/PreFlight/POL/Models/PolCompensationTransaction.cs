using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models
{
    public class PolCompensationTransaction : PolTransactionBase
    {
        public PolCompensationTransaction() { }
        public PolCompensationTransaction(SalaryExportCompany company, SalaryExportEmployee employee, SalaryExportTransaction transaction)
        {
            Foretag = int.Parse(company.CompanyCode);
            AnstNr = int.Parse(employee.EmployeeNr);
            Timmar = transaction.Hours;
            Belopp = transaction.Amount;
            Period = transaction.Date.AddMonths(1);
            kostnadsstalle = transaction.CostAllocation.Costcenter;
            project = transaction.CostAllocation.Project;
            arbeteNr = transaction.Date.ToString("yyMMdd");
            Loneart = int.Parse(transaction.Code);
        }

        private string ForetagFormatted => Foretag.ToString().Length < 2 ? Foretag.ToString().PadLeft(2, '0') : Foretag.ToString().Substring(Foretag.ToString().Length - 2, 2);

        // Startposition 3, längd 2, numeriskt
        [Required]
        public int TransTyp { get; set; } = 52;

        // Startposition 18, längd 3, numeriskt
        [Required]
        [Range(1, 999)]
        public int Loneart { get; set; }
        private string loneartFormatted => Loneart.ToString().PadLeft(3, '0');

        // Startposition 21, längd 10
        public string arbeteNr { get; set; } = string.Empty.PadRight(10, ' ');
        private string arbeteNrFormatted => arbeteNr.PadRight(10, ' ');

        // Startposition 129, längd 15
        public string kostnadsstalle { get; set; } = string.Empty.PadRight(15, ' ');
        private string kostnadsstalleFormatted => kostnadsstalle.PadRight(15, ' ');

        // Startposition 129+15, längd 15
        public string project { get; set; } = string.Empty.PadRight(15, ' ');
        private string projectFormatted => project.PadRight(15, ' ');

        // Startposition 41, längd 1
        public string Atart { get; set; } = " "; // Blankt eller minus
        private string atartFormatted
        {
            get
            {
                if (Timmar < 0)
                {
                    return "-";
                }
                return " ";
            }
        }

        // Startposition 42, längd 5, numeriskt
        [Range(-99999.99, 99999.99)]
        public decimal Timmar { get; set; }
        private string timmarFormatted => Timmar.ToString("000.00").Replace(".", "").Replace(",","");

        // Startposition 47, längd 5, numeriskt
        [Range(-999.99, 999.99)]
        public decimal APris { get; set; }
        private string aPrisFormatted => APris != 0 ? APris.ToString("0.00").Replace(".", "").PadRight(5, ' ') : string.Empty.PadRight(5, ' ');

        // Startposition 52, längd 10, numeriskt
        [Range(-9999999.99, 9999999.99)]
        public decimal Belopp { get; set; }
        private string beloppFormatted => Belopp != 0 ? Belopp.ToString("0.00").Replace(".", "").PadRight(10, ' ') : string.Empty.PadRight(10, ' ');

        public string ToPolString()
        {
            var sb = new StringBuilder();
            sb.Append(ForetagFormatted);
            sb.Append(TransTyp.ToString().PadLeft(2, '0'));
            sb.Append(periodFormatted);
            sb.Append(anstNrFormatted);
            sb.Append(loneartFormatted);
            sb.Append(arbeteNrFormatted);
            // sb.Append(kostnadsstalleFormatted);
            sb.Append(string.Empty.PadRight(10, ' ')); // Pad to 41 (zero based index
            sb.Append(atartFormatted);
            sb.Append(timmarFormatted);
            sb.Append(aPrisFormatted);
            sb.Append(beloppFormatted);
            // Append Kostnadsställe again on column 128.
            sb.Append(string.Empty.PadRight(128 - sb.Length, ' ')); // Pad to 129 (zero based index)
            sb.Append(kostnadsstalleFormatted);
            sb.Append(projectFormatted);
            var result = sb.ToString().Trim();
            result = result.Trim();
            result += "\r\n";
            return result;
        }

        public bool IsValid()
        {
            return Validator.TryValidateObject(this, new ValidationContext(this), null, true);
        }
    }
}
