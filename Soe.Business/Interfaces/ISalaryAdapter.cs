using System.Xml.Linq;

namespace SoftOne.Soe.Business.Interfaces
{
    interface ISalaryAdapter
    {
        /// <summary>
        /// Create the Xml representation of salary data
        /// </summary>
        /// <returns></returns>
        byte[] TransformSalary(XDocument baseXml);
    }

    interface ISalarySplittedFormatAdapter : ISalaryAdapter
    {
        /// <summary>
        /// Crate the Xml representation of schedule, extends ISalary for those adapters where salary and schedule are separate files
        /// </summary>
        /// <param name="baseXml"></param>
        /// <returns></returns>
        byte[] TransformSchedule(XDocument baseXml);
    }
}
