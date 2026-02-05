using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeVehicle : ICreatedModified, IState
    {
        public decimal TaxableValue { get; set; }
        public DateTime CalculatedFromDate
        {
            get
            {
                return this.FromDate ?? DateTime.MinValue;
            }
        }
        public DateTime CalculatedToDate
        {
            get
            {
                return this.ToDate ?? DateTime.MaxValue;
            }
        }
    }

    public partial class EmployeeVehicleEquipment : ICreatedModified, IState
    {
        public DateTime CalculatedFromDate
        {
            get
            {
                return this.FromDate ?? DateTime.MinValue;
            }
        }
        public DateTime CalculatedToDate
        {
            get
            {
                return this.ToDate ?? DateTime.MaxValue;
            }
        }
    }

    public partial class EmployeeVehicleDeduction : ICreatedModified, IState
    {
        public DateTime CalculatedFromDate
        {
            get
            {
                return this.FromDate ?? DateTime.MinValue;
            }
        }
    }

    public partial class EmployeeVehicleTax : ICreatedModified, IState
    {
        public DateTime CalculatedFromDate
        {
            get
            {
                return this.FromDate ?? DateTime.MinValue;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeVehicle

        public static EmployeeVehicleDTO ToDTO(this EmployeeVehicle e, bool includeDeduction, bool includeEquipment, bool includeTax)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeEquipment && !e.EmployeeVehicleEquipment.IsLoaded)
                    {
                        e.EmployeeVehicleEquipment.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.EmployeeVehicleEquipment");
                    }

                    if (includeDeduction && !e.EmployeeVehicleDeduction.IsLoaded)
                    {
                        e.EmployeeVehicleDeduction.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.EmployeeVehicleDeduction");
                    }

                    if (includeTax && !e.EmployeeVehicleTax.IsLoaded)
                    {
                        e.EmployeeVehicleTax.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.EmployeeVehicleTax");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            EmployeeVehicleDTO dto = new EmployeeVehicleDTO()
            {
                EmployeeVehicleId = e.EmployeeVehicleId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                Year = e.Year,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                SysVehicleTypeId = e.SysVehicleTypeId,
                Type = (TermGroup_VehicleType)e.Type,
                LicensePlateNumber = e.LicensePlateNumber,
                ModelCode = e.ModelCode,
                VehicleMake = e.VehicleMake,
                VehicleModel = e.VehicleModel,
                RegisteredDate = e.RegisteredDate,
                FuelType = (TermGroup_SysVehicleFuelType)e.FuelType,
                HasExtensiveDriving = e.HasExtensiveDriving,
                Price = e.Price,
                PriceAdjustment = e.PriceAdjustment,
                CodeForComparableModel = e.CodeForComparableModel,
                ComparablePrice = e.ComparablePrice,
                TaxableValue = e.TaxableValue,

                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                BenefitValueAdjustment = e.BenefitValueAdjustment
            };

            // Relations
            if (includeDeduction)
            {
                dto.Deduction = new List<EmployeeVehicleDeductionDTO>();
                if (e.EmployeeVehicleDeduction != null && e.EmployeeVehicleDeduction.Where(x => x.State == (int)SoeEntityState.Active).ToList().Count > 0)
                    dto.Deduction = e.EmployeeVehicleDeduction.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }
            if (includeEquipment)
            {
                dto.Equipment = new List<EmployeeVehicleEquipmentDTO>();
                if (e.EmployeeVehicleEquipment != null && e.EmployeeVehicleEquipment.Where(x => x.State == (int)SoeEntityState.Active).ToList().Count > 0)
                    dto.Equipment = e.EmployeeVehicleEquipment.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }
            if (includeTax)
            {
                dto.Tax = new List<EmployeeVehicleTaxDTO>();
                if (e.EmployeeVehicleTax != null && e.EmployeeVehicleTax.Where(x => x.State == (int)SoeEntityState.Active).ToList().Count > 0)
                    dto.Tax = e.EmployeeVehicleTax.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            return dto;
        }

        public static IEnumerable<EmployeeVehicleDTO> ToDTOs(this IEnumerable<EmployeeVehicle> l, bool includeDeduction, bool includeEquipment, bool includeTax)
        {
            var dtos = new List<EmployeeVehicleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeDeduction, includeEquipment, includeTax));
                }
            }
            return dtos;
        }

        public static EmployeeVehicleDeductionDTO ToDTO(this EmployeeVehicleDeduction e)
        {
            if (e == null)
                return null;

            return new EmployeeVehicleDeductionDTO()
            {
                EmployeeVehicleDeductionId = e.EmployeeVehicleDeductionId,
                EmployeeVehicleId = e.EmployeeVehicleId,
                FromDate = e.FromDate,
                Price = e.Price,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static IEnumerable<EmployeeVehicleDeductionDTO> ToDTOs(this IEnumerable<EmployeeVehicleDeduction> l)
        {
            var dtos = new List<EmployeeVehicleDeductionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeVehicleEquipmentDTO ToDTO(this EmployeeVehicleEquipment e)
        {
            if (e == null)
                return null;

            return new EmployeeVehicleEquipmentDTO()
            {
                EmployeeVehicleEquipmentId = e.EmployeeVehicleEquipmentId,
                EmployeeVehicleId = e.EmployeeVehicleId,
                Description = e.Description,
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                Price = e.Price,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static IEnumerable<EmployeeVehicleEquipmentDTO> ToDTOs(this IEnumerable<EmployeeVehicleEquipment> l)
        {
            var dtos = new List<EmployeeVehicleEquipmentDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeVehicleTaxDTO ToDTO(this EmployeeVehicleTax e)
        {
            if (e == null)
                return null;

            return new EmployeeVehicleTaxDTO()
            {
                EmployeeVehicleTaxId = e.EmployeeVehicleTaxId,
                EmployeeVehicleId = e.EmployeeVehicleId,
                FromDate = e.FromDate,
                Amount = e.Amount,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static IEnumerable<EmployeeVehicleTaxDTO> ToDTOs(this IEnumerable<EmployeeVehicleTax> l)
        {
            var dtos = new List<EmployeeVehicleTaxDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static EmployeeVehicleGridDTO ToGridDTO(this EmployeeVehicle e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.EmployeeReference.IsLoaded)
                    {
                        e.EmployeeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.EmployeeReference");
                    }
                    if (e.Employee != null && !e.Employee.ContactPersonReference.IsLoaded)
                    {
                        e.Employee.ContactPersonReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.Employee.ContactPersonReference");
                    }
                    if (!e.EmployeeVehicleEquipment.IsLoaded)
                    {    e.EmployeeVehicleEquipment.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.EmployeeVehicleEquipment");
                    }
                    if (!e.EmployeeVehicleDeduction.IsLoaded)
                    {   e.EmployeeVehicleDeduction.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeVehicle.cs e.EmployeeVehicleDeduction");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            return new EmployeeVehicleGridDTO()
            {
                EmployeeVehicleId = e.EmployeeVehicleId,
                EmployeeId = e.EmployeeId,
                EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty,
                EmployeeName = e.Employee?.Name ?? string.Empty,
                LicensePlateNumber = e.LicensePlateNumber,
                VehicleMakeAndModel = $"{e.VehicleMake} {e.VehicleModel}",
                FromDate = e.FromDate,
                ToDate = e.ToDate,
                Price = e.Price,
                TaxableValue = e.TaxableValue,
                NetSalaryDeduction = e.EmployeeVehicleDeduction?.Where(ed => ed.State == (int)SoeEntityState.Active).Sum(ed => ed.Price) ?? 0,
                EquipmentSum = e.EmployeeVehicleEquipment?.Where(eq => eq.State == (int)SoeEntityState.Active).Sum(eq => eq.Price) ?? 0,
            };
        }

        public static IEnumerable<EmployeeVehicleGridDTO> ToGridDTOs(this IEnumerable<EmployeeVehicle> l)
        {
            var dtos = new List<EmployeeVehicleGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static List<EmployeeVehicle> Get(this List<EmployeeVehicle> l, IEnumerable<int> employeeVehicleIds)
        {
            var l2 = new List<EmployeeVehicle>();
            foreach (int employeeVehicleId in employeeVehicleIds)
            {
                if (!l2.Any(i => i.EmployeeVehicleId == employeeVehicleId))
                    l2.Add(l.FirstOrDefault(i => i.EmployeeVehicleId == employeeVehicleId));
            }
            return l2;
        }

        #endregion
    }
}
