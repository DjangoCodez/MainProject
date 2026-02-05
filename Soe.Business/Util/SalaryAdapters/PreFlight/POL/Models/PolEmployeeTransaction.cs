using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models
{
    public class PolEmployeeTransaction
    {
        public PolEmployeeTransaction(SalaryExportCompany company, SalaryExportEmployee employee, DateTime periodStartDate)
        {
            ForetagsNr = int.Parse(company.CompanyCode);
            AnstallningsNr = employee.EmployeeNr;

            FromDatum = periodStartDate;
        }
        
        // Startposition 1, längd 3, numeriskt
        [Required]
        public int TransTyp { get; set; } = 307;
        private string transTypFormatted => TransTyp.ToString().PadLeft(3, '0');

        // Startposition 4, längd 5, numeriskt
        [Required]
        public int ForetagsNr { get; set; }
        private string foretagsNrFormatted => ForetagsNr.ToString().Length < 5 ? ForetagsNr.ToString().PadLeft(5, ' ') : ForetagsNr.ToString().Substring(ForetagsNr.ToString().Length - 5, 5).PadLeft(5, ' ');

        // Startposition 9, längd 10, numeriskt
        [Required]
        [MaxLength(10)]
        public string AnstallningsNr { get; set; }
        private string anstallningsNrFormatted => AnstallningsNr.PadLeft(10, '0');

        // Startposition 19, längd 16, strängvärde
        [Required]
        [MaxLength(16)]
        public string KolumnId { get; set; }
        private string kolumnIdFormatted => KolumnId.PadRight(16, ' ');

        // Startposition 35, längd 6, numeriskt
        [Required]
        public DateTime FromDatum { get; set; }
        private string fromDatumFormatted => FromDatum.ToString("yyMMdd");

        
        public bool Validate()
        {
            // Validate the model
            var context = new ValidationContext(this, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(this, context, results, true);
        }

        public string ToPolString(string kolumnId)
        {
            this.KolumnId = kolumnId;
            var sb = new StringBuilder();
            sb.Append(transTypFormatted);
            sb.Append(foretagsNrFormatted);
            sb.Append(anstallningsNrFormatted);
            sb.Append(kolumnIdFormatted);
            sb.Append(fromDatumFormatted);
            sb.Append(string.Empty.PadRight(6, ' ')); // date to (blank)
            sb.Append(string.Empty.PadRight(12, ' '));

            return sb.ToString();
        }
    }
}