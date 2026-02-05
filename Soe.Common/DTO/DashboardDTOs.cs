using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeRequestsGaugeDTO
    {
        public int RequestId { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public TermGroup_EmployeeRequestType EmployeeRequestType { get; set; }
        public string EmployeeRequestTypeName { get; set; }
        public DateTime AppliedDate { get; set; }
    }

    public interface IMapDTO
    {
        MapItemType Type { get; set; }
        int ID { get; set; }
        string Name { get; set; }
        decimal Longitude { get; set; }
        decimal Latitude { get; set; }
        string Address { get; set; }
    }

    public class BaseMapDTO : IMapDTO
    {
        public MapItemType Type { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public string Address { get; set; }

        public BaseMapDTO()
        {
            Type = MapItemType.Default;
        }
    }

    public class MapGaugeDTO : BaseMapDTO
    {
        public DateTime TimeStamp { get; set; }

        public MapGaugeDTO()
        {
            Type = MapItemType.Employee;
        }
    }

    public class OrderMapDTO : BaseMapDTO
    {
        public int? OrderNr { get; set; }
        public string CustomerName { get; set; }
        public string WorkingDescription { get; set; }

        // Relations
        public List<OrderMapShiftDTO> Shifts { get; set; }

        public OrderMapDTO()
        {
            Type = MapItemType.Order;
        }
    }

    public class OrderMapShiftDTO
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string ShiftTypeName { get; set; }
    }

    public class MyShiftsGaugeDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftStatus ShiftStatus { get; set; }
        public string ShiftStatusName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus ShiftUserStatus { get; set; }
        public string ShiftUserStatusName { get; set; }
    }

    public class OpenShiftsGaugeDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public int OpenType { get; set; }           // 1 = Open, 2 = Unwanted
        public string OpenTypeName { get; set; }    // Populated on client side
        public int NbrInQueue { get; set; }
        public bool IamInQueue { get; set; }
        public string Link { get; set; }
    }

    public class WantedShiftsGaugeDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public int EmployeeId { get; set; }
        public string Employee { get; set; }
        public int OpenType { get; set; }           // 1 = Open, 2 = Unwanted
        public string OpenTypeName { get; set; }    // Populated on client side
        public string EmployeesInQueue { get; set; }
        public string Link { get; set; }
    }
}
