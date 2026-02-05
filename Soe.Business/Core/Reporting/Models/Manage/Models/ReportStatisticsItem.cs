using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models
{
    public class ReportStatisticsItem
    {
        public string ReportName { get; set; }
        public string SystemReportName { get; set; }
        public int AmountPrintOut { get; set; }
        public int AverageTime { get; set; }
        public int MedianTime { get; set; }
        public String Period { get; set; }
        public int AmountOfUniqueUsers { get; set; }
        public int? AmountOfFailed { get; set; }
        public String Date { get; set; }
        public decimal DelTime { get; set; }
        public int? UserId { get; internal set; }

        public string GroupOn(List<TermGroup_ReportStatisticsMatrixColumns> columns)
        {
            var sbv = new StringBuilder();

            foreach (var column in columns)
            {
                switch (column)
                {
                    case TermGroup_ReportStatisticsMatrixColumns.ReportName:
                        sbv.Append($"#{this.ReportName}");
                        break;
                    case TermGroup_ReportStatisticsMatrixColumns.SystemReportName:
                        sbv.Append($"#{this.SystemReportName}");
                        break;
                    case TermGroup_ReportStatisticsMatrixColumns.Period:
                        sbv.Append($"#{this.Period}");
                        break;
                    case TermGroup_ReportStatisticsMatrixColumns.Date:
                        sbv.Append($"#{this.Date}");
                        break;
                    case TermGroup_ReportStatisticsMatrixColumns.DelTime:
                        sbv.Append($"#{this.DelTime}");
                        break;
                    case TermGroup_ReportStatisticsMatrixColumns.UserId:
                        sbv.Append($"#{this.UserId}");
                        break;
                    
                    case TermGroup_ReportStatisticsMatrixColumns.AmountOfFailed:
                    case TermGroup_ReportStatisticsMatrixColumns.AverageTime:
                    case TermGroup_ReportStatisticsMatrixColumns.AmountPrintOut:
                    case TermGroup_ReportStatisticsMatrixColumns.MedianTime:
                    case TermGroup_ReportStatisticsMatrixColumns.AmountOfUniqueUsers:
                        break;
                    default:
                        break;
                }
            }

            return sbv.ToString();
        }

    }

}