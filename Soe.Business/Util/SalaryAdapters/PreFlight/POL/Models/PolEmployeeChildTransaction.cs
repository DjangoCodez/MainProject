using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.Models;
using SoftOne.Soe.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models
{
    internal class PolEmployeeChildTransaction : PolEmployeeTransaction
    {
        public PolEmployeeChildTransaction(SalaryExportCompany company, SalaryExportEmployee employee, DateTime periodStartDate, SalaryExportEmployeeChild employeeChild) : base(company, employee, periodStartDate)
        {
            // SalaryExportEmployeeChild employeeChild = employee.EmployeeChildren.First();
            PersonNr = ((DateTime)employeeChild.BirthDate).ToString("yyyyMMdd") ?? DateTime.Today.ToString("yyyyMMdd");
            NamnAnt = employeeChild.Name;
            FodselDat = employeeChild.BirthDate ?? DateTime.Today;
            BarnNr = employee.EmployeeChildren.IndexOf(employeeChild);
            FledRattDatumTom = DateTime.Today;
            FpenningDatumTom = DateTime.Today;
            FledSemgrDgr = 0;
        }

        // Fältinnehåll nedan - Startposition 59
        // Startposition 1, längd 6, datumformat ÅÅMMDD
        [Required]
        public DateTime FodselDat { get; set; }
        private string fodselDatFormatted => FodselDat.ToString("yyMMdd");

        private string fodselDatNyttFormatted => "".PadLeft(6, '0');
        private string fodselDatBekrFormatted => "N";

        public DateTime FledRattDatumTom { get; set; }
        private string fledRattDatumTomFormatted => "".PadLeft(6, '0');

        public DateTime FpenningDatumTom { get; set; }
        private string fpenningDatumTomFormatted => "".PadLeft(6, '0');

        public int FledSemgrDgr { get; set; }
        private string fledSemgrDgr => "".PadLeft(3, '0');

        // Startposition 7, längd 1, numeriskt
        [Required]
        [Range(0, 9)]
        public int BarnNr { get; set; }
        private string barnNrFormatted => "".PadLeft(1, '0');

        // Startposition 8, längd 12, valfritt personnr
        [MaxLength(12)]
        public string PersonNr { get; set; }
        private string personNrFormatted => PersonNr.PadRight(12, ' ');

        // Startposition 20, längd 50, valfritt namn/anteckning
        [MaxLength(50)]
        public string NamnAnt { get; set; }
        private string namnAntFormatted => NamnAnt.PadRight(50, ' ');

        public string ToPolString()
        {
            StringBuilder sb = new StringBuilder();
            // Barns födsel (FLEDFODSEL)
            sb.Append(base.ToPolString("FLEDFODSEL"));
            sb.Append(fodselDatFormatted);
            sb.Append(fodselDatNyttFormatted);
            sb.Append(fodselDatBekrFormatted);
            sb.Append(fledRattDatumTomFormatted);
            sb.Append(fpenningDatumTomFormatted);
            sb.Append(fledSemgrDgr);
            sb.AppendLine();

            // Föräldraledig (FLEDBARN)
            sb.Append(base.ToPolString("FLEDBARN"));
            sb.Append(fodselDatFormatted);
            sb.Append(barnNrFormatted);
            sb.Append(personNrFormatted);
            sb.Append(namnAntFormatted);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
