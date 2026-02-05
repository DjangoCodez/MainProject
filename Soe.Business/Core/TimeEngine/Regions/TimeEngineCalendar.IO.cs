using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class ImportHolidaysInputDTO : TimeEngineInputDTO
    {
        public List<HolidayDTO> Holidays { get; set; }
        public bool UpdateSchedules { get; set; }
        public ImportHolidaysInputDTO(List<HolidayDTO> holidays, bool updateSchedules)
        {
            this.Holidays = holidays;
            this.UpdateSchedules = updateSchedules;
        }
        public override int? GetIntervalCount()
        {
            return Holidays?.Count();
        }
    }
    public class CalculateDayTypeForEmployeeInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public int EmployeeId { get; set; }
        public bool DoNotCheckHoliday { get; set; }
        public List<HolidayDTO> CompanyHolidays { get; set; }
        public CalculateDayTypeForEmployeeInputDTO(DateTime date, int employeeId, bool doNotCheckHoliday, List<HolidayDTO> companyHolidays = null)
        {
            this.Date = date;
            this.EmployeeId = employeeId;
            this.DoNotCheckHoliday = doNotCheckHoliday;
            this.CompanyHolidays = companyHolidays;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CompanyHolidays?.Count();
        }
    }
    public class CalculateDayTypesForEmployeeInputDTO : TimeEngineInputDTO
    {
        public List<DateTime> Dates { get; set; }
        public int EmployeeId { get; set; }
        public bool DoNotCheckHoliday { get; set; }
        public List<HolidayDTO> CompanyHolidays { get; set; }
        public CalculateDayTypesForEmployeeInputDTO(List<DateTime> dates, int employeeId, bool doNotCheckHoliday, List<HolidayDTO> companyHolidays = null)
        {
            this.Dates = dates;
            this.EmployeeId = employeeId;
            this.DoNotCheckHoliday = doNotCheckHoliday;
            this.CompanyHolidays = companyHolidays;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return Dates?.Count();
        }
    }
    public class CalculateDayTypeForEmployeesInputDTO : TimeEngineInputDTO
    {
        public List<GrossNetCostDTO> Dtos { get; set; }
        public bool DoNotCheckHoliday { get; set; }
        public List<HolidayDTO> CompanyHolidays { get; set; }
        public List<DayType> CompanyDayTypes { get; set; }
        public Employee Employee { get; set; }
        public CalculateDayTypeForEmployeesInputDTO(List<GrossNetCostDTO> dtos, bool doNotCheckHoliday, List<HolidayDTO> companyHolidays = null, List<DayType> companyDayTypes = null, Employee employee = null)
        {
            this.Dtos = dtos;
            this.DoNotCheckHoliday = doNotCheckHoliday;
            this.CompanyHolidays = companyHolidays;
            this.CompanyDayTypes = companyDayTypes;
            this.Employee = employee;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return Dtos?.Count();
        }
    }
    public class SaveUniqueDayInputDTO : TimeEngineInputDTO
    {
        private List<Tuple<int, int, bool>> tuples;
        public List<Tuple<int, int, bool>> Tuples
        {
            get
            {
                if (tuples == null)
                    tuples = new List<Tuple<int, int, bool>>();
                return tuples;
            }
            set
            {
                tuples = value;
            }
        }
        public bool CreateTimeBlocksAndTransactionsAsync { get; set; }
        public SaveUniqueDayInputDTO(int holidayId, int dayTypeId, bool removeOnly)
        {
            Init(Tuple.Create<int, int, bool>(holidayId, dayTypeId, removeOnly));
        }
        public SaveUniqueDayInputDTO(Tuple<int, int, bool> tuple, bool createTimeBlocksAndTransactionsAsync = true)
        {
            Init(tuple, createTimeBlocksAndTransactionsAsync);
        }
        public SaveUniqueDayInputDTO(List<Tuple<int, int, bool>> tuples, bool createTimeBlocksAndTransactionsAsync = true)
        {
            Init(tuples, createTimeBlocksAndTransactionsAsync);
        }
        private void Init(Tuple<int, int, bool> tuple, bool createTimeBlocksAndTransactionsAsync = true)
        {
            this.Init(tuple.ObjToList(), createTimeBlocksAndTransactionsAsync);
        }
        private void Init(List<Tuple<int, int, bool>> tuples, bool createTimeBlocksAndTransactionsAsync = true)
        {
            this.Tuples = tuples;
            this.CreateTimeBlocksAndTransactionsAsync = createTimeBlocksAndTransactionsAsync;
        }
        public override int? GetIntervalCount()
        {
            return Tuples?.Count();
        }
    }
    public class UpdateUniqueDayFromHalfDayInputDTO : TimeEngineInputDTO
    {
        public int TimeHalfDayId { get; set; }
        public int DayTypeId { get; set; }
        public UpdateUniqueDayFromHalfDayInputDTO(int timeHalfDayId, int dayTypeId)
        {
            this.TimeHalfDayId = timeHalfDayId;
            this.DayTypeId = dayTypeId;
        }
    }
    public class SaveUniqueDayFromHalfDayInputDTO : TimeEngineInputDTO
    {
        public int TimeHalfdayId { get; set; }
        public bool RemoveOnly { get; set; }
        public SaveUniqueDayFromHalfDayInputDTO(int timeHalfdayId, bool removeOnly)
        {
            this.TimeHalfdayId = timeHalfdayId;
            this.RemoveOnly = removeOnly;
        }
    }
    public class AddUniqueDayFromHolidayInputDTO : TimeEngineInputDTO
    {
        public int HolidayId { get; set; }
        public int DayTypeId { get; set; }
        public AddUniqueDayFromHolidayInputDTO(int holidayId, int dayTypeId)
        {
            this.HolidayId = holidayId;
            this.DayTypeId = dayTypeId;
        }
    }
    public class DeleteUniqueDayFromHolidayInputDTO : TimeEngineInputDTO
    {
        public int HolidayId { get; set; }
        public int DayTypeId { get; set; }
        public DateTime? OldDateToDelete { get; set; }
        public bool CreateTimeBlocksAndTransactionsAsync { get; set; }
        public DeleteUniqueDayFromHolidayInputDTO(int holidayId, int dayTypeId, DateTime? oldDate, bool createTimeBlocksAndTransactionsAsync = true)
        {
            this.HolidayId = holidayId;
            this.DayTypeId = dayTypeId;
            this.OldDateToDelete = oldDate;
            this.CreateTimeBlocksAndTransactionsAsync = createTimeBlocksAndTransactionsAsync;
        }
    }
    public class UpdateUniqueDayFromHolidayInputDTO : TimeEngineInputDTO
    {
        public int HolidayId { get; set; }
        public int DayTypeId { get; set; }
        public DateTime? OldDateToDelete { get; set; }
        public UpdateUniqueDayFromHolidayInputDTO(int holidayId, int dayTypeId, DateTime? oldDateToDelete)
        {
            this.HolidayId = holidayId;
            this.DayTypeId = dayTypeId;
            this.OldDateToDelete = oldDateToDelete;
        }
    }
    public class CreateTransactionsForEarnedHolidayInputDTO : TimeEngineInputDTO
    {
        public int Year { get; set; }
        public List<int> EmployeeIds { get; set; }
        public int HolidayId { get; set; }
        public CreateTransactionsForEarnedHolidayInputDTO(int holidayId, List<int> employeeIds, int year)
        {
            this.HolidayId = holidayId;
            this.EmployeeIds = employeeIds;
            this.Year = year;
        }
    }
    public class DeleteTransactionsForEarnedHolidayInputDTO : TimeEngineInputDTO
    {
        public int Year { get; set; }
        public List<int> EmployeeIds { get; set; }
        public int HolidayId { get; set; }
        public DeleteTransactionsForEarnedHolidayInputDTO(int holidayId, List<int> employeeIds, int year)
        {
            this.HolidayId = holidayId;
            this.EmployeeIds = employeeIds;
            this.Year = year;
        }
        public override int? GetIdCount()
        {
            return EmployeeIds?.Count();
        }
    }

    #endregion

    #region Output

    public class ImportHolidaysOutputDTO : TimeEngineOutputDTO { }
    public class CalculateDayTypeForEmployeeOutputDTO : TimeEngineOutputDTO
    {
        public DayType DayType { get; set; }
    }
    public class CalculateDayTypesForEmployeeOutputDTO : TimeEngineOutputDTO
    {
        public Dictionary<DateTime, DayType> DayTypes { get; set; }
    }
    public class SaveUniqueDayOutputDTO : TimeEngineOutputDTO { }
    public class UpdateUniqueDayFromHalfDayOutputDTO : TimeEngineOutputDTO { }
    public class SaveUniqueDayFromHalfDayOutputDTO : TimeEngineOutputDTO { }
    public class AddUniqueDayFromHolidayOutputDTO : TimeEngineOutputDTO { }
    public class DeleteUniqueDayFromHolidayOutputDTO : TimeEngineOutputDTO { }
    public class UpdateUniqueDayFromHolidayOutputDTO : TimeEngineOutputDTO { }
    public class CreateTransactionsForEarnedHolidayOutputDTO : TimeEngineOutputDTO { }
    public class DeleteTransactionsForEarnedHolidayOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
