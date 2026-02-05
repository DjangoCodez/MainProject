using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models
{
    public class PolTransactionBase
    {
        public PolTransactionBase()
        {
            validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        }
        // Startposition 1, längd 2, numeriskt
        [Required]
        [Range(1, 99)]
        public int Foretag { get; set; }

        // Startposition 5, längd 3, numeriskt
        // Utbetalningsmånad, PaymentMonth
        // ÅMM
        [Required]
        public DateTime Period { get; set; }
        protected string periodFormatted
        {
            get
            {
                // Get Last digit of year 
                var year = Period.Year.ToString().Substring(3, 1);
                var month = Period.Month.ToString().PadLeft(2, '0');
                return year + month;
            }
        }

        // Startposition 8, längd 10, numeriskt
        [Required]
        [Range(1, 9999999999)]
        public long AnstNr { get; set; }
        protected string anstNrFormatted => AnstNr.ToString().PadLeft(10, '0');

        public ICollection<System.ComponentModel.DataAnnotations.ValidationResult> validationResults { get; set; }
        public string GetValidationErrors()
        {
            var sb = new StringBuilder();
            foreach (var validationResult in validationResults)
            {
                sb.AppendLine(validationResult.ErrorMessage);
            }
            return sb.ToString();
        }

    }
}
