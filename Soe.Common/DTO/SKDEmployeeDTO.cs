using SoftOne.Soe.Common.Interfaces.Common;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class SKDDTO
    {
        public decimal LonBrutto { get; set; }
        public decimal Forman { get; set; }
        public decimal AvdrKostn { get; set; }
        public decimal SumUlagAvg { get; set; }
        public decimal UlagAvgHel { get; set; }
        public decimal UlagSkLonSarsk { get; set; }
        public decimal UlagAvgHelLessThanLimit { get; set; }
        public decimal UlagAvgHelFromPreviousPeriods { get; set; }
        public decimal AvgHel { get; set; }
        public decimal AvgHelLessThanLimit { get; set; }
        public decimal AvgHelFromPreviousPeriods { get; set; }
        public decimal UlagAvgAldersp { get; set; }
        public decimal UlagAvgAlderspLessThanLimit { get; set; }
        public decimal UlagAvgAlderspFromPreviousPeriods { get; set; }
        public decimal UlagUngdom { get; set; }
        public decimal UlagUngdomLessThanLimit { get; set; }
        public decimal UlagUngdomFromPreviousPeriods { get; set; }
        public decimal AvgAldersp { get; set; }
        public decimal AvgAlderspLessThanLimit { get; set; }
        public decimal AvgAlderspFromPreviousPeriods { get; set; }
        public decimal UlagAlderspSkLon { get; set; }
        public decimal UlagAlderspSkLonLessThanLimit { get; set; }
        public decimal UlagAlderspSkLonFromPreviousPeriods { get; set; }
        public decimal AvgAlderspSkLon { get; set; }
        public decimal AvgUngdom { get; set; }
        public decimal SkLonSarsk { get; set; }
        public decimal AvgUngdomLessThanLimit { get; set; }
        public decimal AvgUngdomFromPreviousPeriods { get; set; }
        public decimal UlagAvgAmbassad { get; set; }
        public decimal AvgAmbassad { get; set; }
        public bool KodAmerika { get; set; }
        public decimal UlagAvgAmerika { get; set; }
        public decimal AvgAmerika { get; set; }
        public decimal UlagStodForetag { get; set; }
        public decimal AvdrStodForetag { get; set; }
        public decimal UlagStodUtvidgat { get; set; }
        public decimal AvdrStodUtvidgat { get; set; }
        public decimal SumAvgBetala { get; set; }
        public decimal UlagSkAvdrLon { get; set; }
        public decimal UlagSkAvdrLonFromPreviousPeriods { get; set; }
        public decimal SkAvdrLon { get; set; }
        public decimal UlagSkAvdrPension { get; set; }
        public decimal SkAvdrPension { get; set; }
        public decimal UlagSkAvdrRanta { get; set; }
        public decimal SkAvdrRanta { get; set; }
        public decimal UlagSumSkAvdr { get; set; }
        public decimal SumSkAvdr { get; set; }
        public decimal SjukLonKostnEhs { get; set; }
        public string TextUpplysningAg { get; set; }
        public decimal SumAvg { get; set; } //Only in printed report, box 78?

        public List<SKDEmployeeTransactionDTO> Transactions { get; set; }
    }

    public class SKDEmployeeTransactionDTO : IPayrollType
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string Type { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public string TimePeriodName { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public string SysPayrollTypeLevel1Name { get; set; }
        public string SysPayrollTypeLevel2Name { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public string SysPayrollTypeLevel4Name { get; set; }
    }
}
