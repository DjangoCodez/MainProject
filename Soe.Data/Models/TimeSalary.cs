using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;

namespace SoftOne.Soe.Data
{
    public partial class TimeSalaryExport : ICreatedModified, IState
    {
        public string TargetName { get; set; }
    }

    public partial class TimeSalaryPaymentExport : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public DateTime? SalarySpecificationPublishDate { get; set; }
    }

    public partial class TimeSalaryPaymentExportEmployee : ICreatedModified, IState
    {
        public string DisbursementMethodName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeSalaryExport

        public static TimeSalaryExportDTO ToDTO(this TimeSalaryExport e)
        {
            if (e == null)
                return null;

            return new TimeSalaryExportDTO()
            {
                TimeSalaryExportId = e.TimeSalaryExportId,
                ActorCompanyId = e.Company?.ActorCompanyId ?? 0,
                StartInterval = e.StartInterval,
                StopInterval = e.StopInterval,
                ExportDate = e.ExportDate,
                ExportTarget = (SoeTimeSalaryExportTarget)e.ExportTarget,
                ExportFormat = (SoeTimeSalaryExportFormat)e.ExportFormat,
                Extension = e.Extension,
                TargetName = e.TargetName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Comment = e.Comment,
            };
        }

        public static IEnumerable<TimeSalaryExportDTO> ToDTOs(this IEnumerable<TimeSalaryExport> l)
        {
            var dtos = new List<TimeSalaryExportDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeSalaryPaymentExport

        public static TimeSalaryPaymentExportDTO ToDTO(this TimeSalaryPaymentExport e, bool includeTimePeriodInfo, bool includeEmployeeInfo)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeTimePeriodInfo)
                {
                    if (!e.TimePeriodReference.IsLoaded)
                    {
                        e.TimePeriodReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeSalary.cs e.TimePeriodReference");
                    }

                    if (!e.TimePeriod.TimePeriodHeadReference.IsLoaded)
                    {
                        e.TimePeriod.TimePeriodHeadReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeSalary.cs e.TimePeriod.TimePeriodHeadReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeSalaryPaymentExportDTO dto = new TimeSalaryPaymentExportDTO()
            {
                TimeSalaryPaymentExportId = e.TimeSalaryPaymentExportId,
                ActorCompanyId = e.ActorCompanyId,
                TimePeriodId = e.TimePeriodId,
                ExportDate = e.ExportDate,
                ExportType = (TermGroup_TimeSalaryPaymentExportType)e.ExportType,
                ExportFormat = (SoeTimeSalaryPaymentExportFormat)e.ExportFormat,
                Extension = e.Extension,
                TypeName = e.TypeName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.TimePeriod != null)
            {
                dto.TimePeriodName = e.TimePeriod.Name;
                dto.PaymentDate = e.TimePeriod.PaymentDate ?? CalendarUtility.DATETIME_DEFAULT;
                dto.PaymentDateString = e.TimePeriod.PaymentDate?.ToShortDateString() ?? string.Empty;
                if (e.TimePeriod.PayrollStartDate.HasValue && e.TimePeriod.PayrollStopDate.HasValue)
                    dto.PayrollDateInterval = e.TimePeriod.PayrollStartDate.Value.ToShortDateString() + " - " + e.TimePeriod.PayrollStopDate.Value.ToShortDateString();
            }
            if (e.TimePeriod?.TimePeriodHead != null)
                dto.TimePeriodHeadName = e.TimePeriod.TimePeriodHead.Name;
            if (e.TimeSalaryPaymentExportEmployee != null)
                dto.TimeSalaryPaymentExportEmployees = e.TimeSalaryPaymentExportEmployee.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(includeEmployeeInfo, null).ToList();

            return dto;
        }

        public static IEnumerable<TimeSalaryPaymentExportDTO> ToDTOs(this IEnumerable<TimeSalaryPaymentExport> l, bool includeTimePeriodInfo, bool includeEmployeeInfo)
        {
            var dtos = new List<TimeSalaryPaymentExportDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeTimePeriodInfo, includeEmployeeInfo));
                }
            }
            return dtos;
        }

        public static TimeSalaryPaymentExportGridDTO ToGridDTO(this TimeSalaryPaymentExport e)
        {
            if (e == null)
                return null;

            TimeSalaryPaymentExportGridDTO dto = new TimeSalaryPaymentExportGridDTO()
            {
                TimeSalaryPaymentExportId = e.TimeSalaryPaymentExportId,
                ExportDate = e.ExportDate,
                ExportType = (TermGroup_TimeSalaryPaymentExportType)e.ExportType,
                TypeName = e.TypeName,
                SalarySpecificationPublishDate = e.SalarySpecificationPublishDate,
            };

            if (e.TimePeriod != null)
            {
                dto.TimePeriodName = e.TimePeriod.Name;
                dto.PaymentDate = e.TimePeriod.PaymentDate ?? CalendarUtility.DATETIME_DEFAULT;
                if (e.TimePeriod.PayrollStartDate.HasValue && e.TimePeriod.PayrollStopDate.HasValue)
                    dto.PayrollDateInterval = e.TimePeriod.PayrollStartDate.Value.ToShortDateString() + " - " + e.TimePeriod.PayrollStopDate.Value.ToShortDateString();

                if (e.TimePeriod.TimePeriodHead != null)
                    dto.TimePeriodHeadName = e.TimePeriod.TimePeriodHead.Name;
            }

            if (e.TimeSalaryPaymentExportEmployee != null)
                dto.Employees = e.TimeSalaryPaymentExportEmployee.Where(p => p.State == (int)SoeEntityState.Active).ToDTOs(true, null).ToList();

            return dto;
        }

        public static IEnumerable<TimeSalaryPaymentExportGridDTO> ToGridDTOs(this IEnumerable<TimeSalaryPaymentExport> l)
        {
            List<TimeSalaryPaymentExportGridDTO> dtos = new List<TimeSalaryPaymentExportGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeSalaryPaymentExportEmployee

        public static TimeSalaryPaymentExportEmployeeDTO ToDTO(this TimeSalaryPaymentExportEmployee e, bool includeEmployeeInfo, Employee employee)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeEmployeeInfo && employee == null)
                {
                    if (!e.EmployeeReference.IsLoaded)
                    {
                        e.EmployeeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeSalary.cs e.EmployeeReference");
                    }

                    if (!e.Employee.ContactPersonReference.IsLoaded)
                    {
                        e.Employee.ContactPersonReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeSalary.cs e.Employee.ContactPersonReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeSalaryPaymentExportEmployeeDTO dto = new TimeSalaryPaymentExportEmployeeDTO()
            {
                EmployeeId = e.EmployeeId,
                AccountNr = e.DisbursementAccountNr,
                NetAmount = e.NetAmount,
                DisbursementMethod = e.DisbursementMethod,
                DisbursementMethodName = e.DisbursementMethodName,
                PaymentRowKey = e.UniquePaymentRowKey,
                NetAmountCurrency = e.NetAmountCurrency.ToDecimal(),
            };

            if (employee != null)
            {
                dto.Name = employee.Name;
                dto.EmployeeNr = employee.EmployeeNr;
            }
            else
            {
                dto.Name = e.Employee?.Name ?? string.Empty;
                dto.EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty;
            }

            return dto;
        }

        public static IEnumerable<TimeSalaryPaymentExportEmployeeDTO> ToDTOs(this IEnumerable<TimeSalaryPaymentExportEmployee> l, bool includeEmployeeInfo, Employee employee)
        {
            var dtos = new List<TimeSalaryPaymentExportEmployeeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeEmployeeInfo, employee));
                }
            }
            return dtos;
        }

        #endregion
    }
}
