using SoftOne.Soe.Common.Attributes;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class CsrResponseDTO
    {
        public CsrResponseDTO() { }

        public CsrResponseDTO(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }

        // Response from Skatteverket
        public string personnummer { get; set; }
        public string skatteform { get; set; }
        public string skattetabell { get; set; }
        public int staendejamkningprocent { get; set; }
        public string felkod { get; set; }
        public string felmeddelande { get; set; }
        public int procentbeslut { get; set; }
        public string giltigtom { get; set; }
        public string giltigfrom { get; set; }

        // Extensions
        public int Year { get; set; }
        public string ErrorMessage { get; set; }
        public int EmployeeId { get; set; }
    }

    public class CSRMultiPersonRequest
    {
        public List<string> Personnummer { get; set; }
    }


    public class SkatteAvdragFleraPersoner
    {
        public SkatteAvdragFleraPersoner()
        {
            personnummer = new List<string>();
        }
        public List<string> personnummer { get; set; }
    }

}
