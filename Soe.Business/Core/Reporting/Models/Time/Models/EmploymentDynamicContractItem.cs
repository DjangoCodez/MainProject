using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmploymentDynamicContractItem
    {

        EmployeeTemplateEditDTO _employeeTemplateEditDTO;
        public EmploymentDynamicContractItem(EmployeeTemplateEditDTO employeeTemplateEditDTO)
        {
            _employeeTemplateEditDTO = employeeTemplateEditDTO;
        }

        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }

        public XElement GetElement(int employeeXmlId)
        {
            return _employeeTemplateEditDTO.ToXElement(employeeXmlId);
        }
    }
}
