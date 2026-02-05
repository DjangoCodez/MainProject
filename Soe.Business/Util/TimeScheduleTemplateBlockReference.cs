using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business
{
    public static class TimeScheduleTemplateBlockReferenceExtensions
    {
        public static List<TimeScheduleTemplateBlock> GetTemplateBlocks(this List<TimeScheduleTemplateBlockReference> l, bool onlyAutogen = false)
        {
            List<TimeScheduleTemplateBlock> templateBlocks = new List<TimeScheduleTemplateBlock>();
            foreach (TimeScheduleTemplateBlockReference e in l)
            {
                if (onlyAutogen && !e.AutogenTimeblocks)
                    continue;

                templateBlocks.AddRange(e.TemplateBlocks);
            }
            return templateBlocks.OrderBy(i => i.Date).ToList();
        }
    }

    public class TimeScheduleTemplateBlockReference
    {
        #region Properties

        public int EmployeeGroupId { get; set; }
        public bool AutogenTimeblocks { get; set; }
        public List<TimeScheduleTemplateBlock> TemplateBlocks { get; set; }

        #endregion

        #region Ctor

        public TimeScheduleTemplateBlockReference(EmployeeGroup employeeGroup, List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (employeeGroup == null)
                return;

            this.EmployeeGroupId = employeeGroup.EmployeeGroupId;
            this.AutogenTimeblocks = employeeGroup.AutogenTimeblocks;
            this.TemplateBlocks = templateBlocks != null ? templateBlocks : new List<TimeScheduleTemplateBlock>();
        }

        #endregion

        #region Public methods

        public void Update(List<TimeScheduleTemplateBlock> templateBlocks)
        {
            if (this.TemplateBlocks == null)
                this.TemplateBlocks = new List<TimeScheduleTemplateBlock>();
            if (templateBlocks != null)
                this.TemplateBlocks.AddRange(templateBlocks);
        }

        #endregion

        #region Private methods

        #endregion
    }
}
