using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class MassRegistrationTemplateHead : ICreatedModified, IState
    {
        public int? Dim1Id { get; set; }
        public int? Dim2Id { get; set; }
        public int? Dim3Id { get; set; }
        public int? Dim4Id { get; set; }
        public int? Dim5Id { get; set; }
        public int? Dim6Id { get; set; }
        public bool HasCreatedTransactions { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region MassRegistrationTemplateHead

        public static MassRegistrationTemplateHeadDTO ToDTO(this MassRegistrationTemplateHead e, bool loadRelations, List<MassRegistrationTemplateRow> loadedRows = null, List<AccountDimDTO> dims = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && loadRelations)
                {
                    if (!e.PayrollProductReference.IsLoaded)
                    {
                        e.PayrollProductReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs e.PayrollProductReference");
                    }
                    if (!e.AccountStdReference.IsLoaded)
                    {
                        e.AccountStdReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs e.AccountStdReference");
                    }

                    if (e.AccountStd != null)
                    {
                        if (!e.AccountStd.AccountReference.IsLoaded)
                        {
                            e.AccountStd.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs e.AccountStd.AccountReference");
                        }
                        if (e.AccountStd.Account != null && !e.AccountStd.Account.AccountDimReference.IsLoaded)
                        {
                            e.AccountStd.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs e.AccountStd.Account.AccountDimReference");
                        }
                    }

                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs e.AccountInternal");
                    }

                    foreach (var accountInternal in e.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                        {
                            accountInternal.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs accountInternal.AccountReference");
                        }
                        if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                        {
                            accountInternal.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("MassRegistration.cs accountInternal.Account.AccountDimReference");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            MassRegistrationTemplateHeadDTO dto = new MassRegistrationTemplateHeadDTO()
            {
                MassRegistrationTemplateHeadId = e.MassRegistrationTemplateHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Dim1Id = e.AccountId,
                Dim1Nr = e.AccountStd?.Account?.AccountNr ?? string.Empty,
                Dim1Name = e.AccountStd?.Account?.Name ?? string.Empty,
                Dim2Id = e.Dim2Id,
                Dim3Id = e.Dim3Id,
                Dim4Id = e.Dim4Id,
                Dim5Id = e.Dim5Id,
                Dim6Id = e.Dim6Id,
                PayrollProductId = e.PayrollProductId,
                Name = e.Name,
                IsRecurring = e.IsRecurring,
                RecurringDateTo = e.RecurringDateTo,
                InputType = (TermGroup_MassRegistrationInputType)e.InputType,
                Comment = e.Comment,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                Quantity = e.Quantity,
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
                UnitPrice = e.UnitPrice,
                StopOnProduct = e.StopOnProduct,
                StopOnDateFrom = e.StopOnDateFrom,
                StopOnDateTo = e.StopOnDateTo,
                StopOnQuantity = e.StopOnQuantity,
                StopOnIsSpecifiedUnitPrice = e.StopOnIsSpecifiedUnitPrice,
                StopOnUnitPrice = e.StopOnUnitPrice,
                PaymentDate = e.PaymentDate,
                StopOnPaymentDate = e.StopOnPaymentDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            dto.AccountInternals = e.AccountInternal.ToDTOs();
            if (loadedRows != null)
                dto.Rows = loadedRows.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs(loadRelations, dims).ToList();
            else
                dto.Rows = e.MassRegistrationTemplateRow.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs(loadRelations, dims).ToList();

            dto.HasCreatedTransactions = e.HasCreatedTransactions;

            return dto;
        }

        public static IEnumerable<MassRegistrationTemplateHeadDTO> ToDTOs(this IEnumerable<MassRegistrationTemplateHead> l, bool loadRelations)
        {
            var dtos = new List<MassRegistrationTemplateHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(loadRelations));
                }
            }
            return dtos;
        }

        public static MassRegistrationGridDTO ToGridDTO(this MassRegistrationTemplateHead e)
        {
            if (e == null)
                return null;

            return new MassRegistrationGridDTO()
            {
                MassRegistrationTemplateHeadId = e.MassRegistrationTemplateHeadId,
                Name = e.Name,
                IsRecurring = e.IsRecurring,
                RecurringDateTo = e.RecurringDateTo,
                State = (SoeEntityState)e.State
            };
        }

        public static List<MassRegistrationGridDTO> ToGridDTOs(this IEnumerable<MassRegistrationTemplateHead> l)
        {
            var dtos = new List<MassRegistrationGridDTO>();
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

        #region MassRegistrationTemplateRow

        public static MassRegistrationTemplateRowDTO ToDTO(this MassRegistrationTemplateRow e, bool loadRelations, List<AccountDimDTO> dims)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && loadRelations)
                {
                    if (!e.EmployeeReference.IsLoaded)
                        e.EmployeeReference.Load();
                    if (!e.PayrollProductReference.IsLoaded)
                        e.PayrollProductReference.Load();
                    if (!e.AccountStdReference.IsLoaded)
                        e.AccountStdReference.Load();

                    if (e.AccountStd != null)
                    {
                        if (!e.AccountStd.AccountReference.IsLoaded)
                            e.AccountStd.AccountReference.Load();
                        if (e.AccountStd.Account != null && !e.AccountStd.Account.AccountDimReference.IsLoaded)
                            e.AccountStd.Account.AccountDimReference.Load();
                    }

                    if (!e.AccountInternal.IsLoaded)
                        e.AccountInternal.Load();

                    foreach (var accountInternal in e.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                            accountInternal.AccountReference.Load();
                        if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                            accountInternal.Account.AccountDimReference.Load();
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            MassRegistrationTemplateRowDTO dto = new MassRegistrationTemplateRowDTO()
            {
                MassRegistrationTemplateRowId = e.MassRegistrationTemplateRowId,
                MassRegistrationTemplateHeadId = e.MassRegistrationTemplateHeadId,
                EmployeeId = e.EmployeeId,
                ProductId = e.PayrollProductId,
                Dim1Id = e.AccountId ?? 0,
                Dim1Name = e.AccountStd?.Account?.Name ?? string.Empty,
                Dim1Nr = e.AccountStd?.Account?.AccountNr ?? string.Empty,
                PaymentDate = e.PaymentDate,
                DateFrom = e.DateFrom,
                DateTo = e.DateTo,
                Quantity = e.Quantity,
                IsSpecifiedUnitPrice = e.IsSpecifiedUnitPrice,
                UnitPrice = e.UnitPrice,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Internal accounts (dim 2-6)
            if (e.AccountInternal != null && dims != null)
            {
                int dimCounter = 2;
                foreach (AccountDimDTO dim in dims.Where(a => a.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).OrderBy(o => o.AccountDimNr))
                {
                    AccountInternal accountInternal = e.AccountInternal.FirstOrDefault(w => w.Account.AccountDimId == dim.AccountDimId);

                    if (dimCounter == 2)
                    {
                        dto.Dim2DimNr = accountInternal?.Account.AccountDim.AccountDimNr ?? 0;
                        dto.Dim2Id = accountInternal?.Account.AccountId ?? 0;
                        dto.Dim2Nr = accountInternal?.Account.AccountNr ?? string.Empty;
                        dto.Dim2Name = accountInternal?.Account.Name ?? string.Empty;
                    }
                    else if (dimCounter == 3)
                    {
                        dto.Dim3DimNr = accountInternal?.Account.AccountDim.AccountDimNr ?? 0;
                        dto.Dim3Id = accountInternal?.Account.AccountId ?? 0;
                        dto.Dim3Nr = accountInternal?.Account.AccountNr ?? string.Empty;
                        dto.Dim3Name = accountInternal?.Account.Name ?? string.Empty;
                    }
                    else if (dimCounter == 4)
                    {
                        dto.Dim4DimNr = accountInternal?.Account.AccountDim.AccountDimNr ?? 0;
                        dto.Dim4Id = accountInternal?.Account.AccountId ?? 0;
                        dto.Dim4Nr = accountInternal?.Account.AccountNr ?? string.Empty;
                        dto.Dim4Name = accountInternal?.Account.Name ?? string.Empty;
                    }
                    else if (dimCounter == 5)
                    {
                        dto.Dim5DimNr = accountInternal?.Account.AccountDim.AccountDimNr ?? 0;
                        dto.Dim5Id = accountInternal?.Account.AccountId ?? 0;
                        dto.Dim5Nr = accountInternal?.Account.AccountNr ?? string.Empty;
                        dto.Dim5Name = accountInternal?.Account.Name ?? string.Empty;
                    }
                    else if (dimCounter == 6)
                    {
                        dto.Dim6DimNr = accountInternal?.Account.AccountDim.AccountDimNr ?? 0;
                        dto.Dim6Id = accountInternal?.Account.AccountId ?? 0;
                        dto.Dim6Nr = accountInternal?.Account.AccountNr ?? string.Empty;
                        dto.Dim6Name = accountInternal?.Account.Name ?? string.Empty;
                    }

                    dimCounter++;
                }
            }

            dto.EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty;
            dto.EmployeeName = e.Employee?.Name ?? string.Empty;
            dto.ProductName = e.PayrollProduct?.Name ?? string.Empty;
            dto.ProductNr = e.PayrollProduct?.Number ?? string.Empty;

            return dto;
        }

        public static IEnumerable<MassRegistrationTemplateRowDTO> ToDTOs(this IEnumerable<MassRegistrationTemplateRow> l, bool loadRelations, List<AccountDimDTO> dims = null)
        {
            var dtos = new List<MassRegistrationTemplateRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(loadRelations, dims));
                }
            }
            return dtos;
        }

        #endregion
    }
}
