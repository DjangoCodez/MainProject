using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;

namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        #region VoucherHead

        public static VoucherHeadDTO ToDTO(this VoucherHead e, bool includeRows, bool includeRowAccounts, List<AccountDim> dims = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.VoucherSeriesReference.IsLoaded)
                    {
                        e.VoucherSeriesReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VoucherSeriesReference");
                    }
                    if (e.VoucherSeries != null && !e.VoucherSeries.VoucherSeriesTypeReference.IsLoaded)
                    {
                        e.VoucherSeries.VoucherSeriesTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VoucherSeries.VoucherSeriesTypeReference");
                    }
                    if (includeRows && !e.VoucherRow.IsLoaded)
                    {
                        e.VoucherRow.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VoucherRow");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            VoucherHeadDTO dto = new VoucherHeadDTO
            {
                VoucherHeadId = e.VoucherHeadId,
                VoucherSeriesId = e.VoucherSeriesId,
                AccountPeriodId = e.AccountPeriodId,
                VoucherNr = e.VoucherNr,
                Date = e.Date,
                Text = e.Text,
                Template = e.Template,
                TypeBalance = e.TypeBalance,
                SourceType = (TermGroup_VoucherHeadSourceType)e.SourceType,
                SourceTypeName = e.SourceTypeName,
                VatVoucher = e.VatVoucher.HasValue && e.VatVoucher.Value,
                CompanyGroupVoucher = e.CompanyGroupVoucher.HasValue && e.CompanyGroupVoucher.Value,
                Status = (TermGroup_AccountStatus)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Note = e.Note,
                ActorCompanyId = e.ActorCompanyId,
                BudgetAccountId = e.BudgetAccountId,
                AccountIds = e.AccountIds,

            };

            // Extensions
            if (e.VoucherSeries != null)
            {
                dto.VoucherSeriesTypeId = e.VoucherSeries.VoucherSeriesTypeId;
                if (e.VoucherSeries.VoucherSeriesType != null)
                    dto.VoucherSeriesTypeName = e.VoucherSeries.VoucherSeriesType.Name;
            }

            if (includeRows)
                dto.Rows = e.VoucherRow?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(includeRowAccounts, dims).ToList() ?? new List<VoucherRowDTO>();

            return dto;
        }

        public static IEnumerable<VoucherHeadDTO> ToDTOs(this IEnumerable<VoucherHead> l, bool includeRows, bool includeRowAccounts)
        {
            var dtos = new List<VoucherHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, includeRowAccounts));
                }
            }
            return dtos;
        }

        public static VoucherGridDTO ToGridDTO(this VoucherHead e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (!e.VoucherSeriesReference.IsLoaded)
                    {
                        e.VoucherSeriesReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VoucherSeriesReference");
                    }
                    if (e.VoucherSeries != null && !e.VoucherSeries.VoucherSeriesTypeReference.IsLoaded)
                    {
                        e.VoucherSeries.VoucherSeriesTypeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("e.VoucherSeries.VoucherSeriesTypeReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            VoucherGridDTO dto = new VoucherGridDTO
            {
                VoucherHeadId = e.VoucherHeadId,
                VoucherNr = e.VoucherNr,
                Date = e.Date,
                Text = e.Text,
                VatVoucher = e.VatVoucher.HasValue && e.VatVoucher.Value,
                Modified = e.Modified,
            };

            // Extensions
            if (e.VoucherSeries != null)
            {
                dto.VoucherSeriesTypeId = e.VoucherSeries.VoucherSeriesTypeId;
                if (e.VoucherSeries.VoucherSeriesType != null)
                    dto.VoucherSeriesTypeName = e.VoucherSeries.VoucherSeriesType.Name;
            }

            return dto;
        }

        public static IEnumerable<VoucherGridDTO> ToGridDTOs(this IEnumerable<VoucherHead> l)
        {
            var dtos = new List<VoucherGridDTO>();
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

        #region VoucherRow

        public static VoucherRowDTO ToDTO(this VoucherRow e, bool includeInternalAccounts, List<AccountDim> dims = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeInternalAccounts)
                    {
                        if (!e.AccountInternal.IsLoaded)
                        {
                            e.AccountInternal.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("e.AccountInternal");
                        }
                        foreach (AccountInternal accountInternal in e.AccountInternal)
                        {
                            if (!accountInternal.AccountReference.IsLoaded)
                            {
                                accountInternal.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.AccountReference");
                            }
                            if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                            {
                                accountInternal.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("accountInternal.Account.AccountDimReference");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new VoucherRowDTO
            {
                VoucherRowId = e.VoucherRowId,
                VoucherHeadId = e.VoucherHeadId,
                ParentRowId = e.ParentRowId,
                AccountDistributionHeadId = e.AccountDistributionHeadId,
                Date = e.Date,
                Text = e.Text,
                Quantity = e.Quantity,
                Amount = e.Amount,
                AmountEntCurrency = e.AmountEntCurrency,
                Merged = e.Merged,
                State = (SoeEntityState)e.State,
                RowNr = e.RowNr,
                StartDate = e.StartDate,
                NumberOfPeriods = e.NumberOfPeriods 
            };

            // Extensions
            dto.VoucherSeriesTypeNr = e.VoucherSeriesTypeNr;
            dto.VoucherSeriesTypeName = e.VoucherSeriesTypeName;
            dto.VoucherNr = e.VoucherNr;

            Account account = e.AccountStd != null && e.AccountStd.Account != null ? e.AccountStd.Account : null;
            dto.Dim1Id = account?.AccountId ?? 0;
            dto.Dim1Nr = account?.AccountNr ?? string.Empty;
            dto.Dim1Name = account?.Name ?? string.Empty;
            dto.Dim1UnitStop = e.AccountStd?.UnitStop ?? false;
            dto.Dim1AmountStop = e.AccountStd?.AmountStop ?? 1;

            if (includeInternalAccounts)
            {
                foreach (AccountInternal accInt in e.AccountInternal)
                {
                    if (accInt.Account != null && accInt.Account.AccountDim != null)
                    {
                        if (dims != null)
                        {
                            int index = 1;
                            foreach (AccountDim dim in dims)
                            {
                                if (dim.AccountDimId == accInt.Account.AccountDim.AccountDimId)
                                    break;
                                index++;
                            }

                            switch (index)
                            {
                                case 2:
                                    dto.Dim2Id = accInt.AccountId;
                                    dto.Dim2Nr = accInt.Account.AccountNr;
                                    dto.Dim2Name = accInt.Account.Name;
                                    break;
                                case 3:
                                    dto.Dim3Id = accInt.AccountId;
                                    dto.Dim3Nr = accInt.Account.AccountNr;
                                    dto.Dim3Name = accInt.Account.Name;
                                    break;
                                case 4:
                                    dto.Dim4Id = accInt.AccountId;
                                    dto.Dim4Nr = accInt.Account.AccountNr;
                                    dto.Dim4Name = accInt.Account.Name;
                                    break;
                                case 5:
                                    dto.Dim5Id = accInt.AccountId;
                                    dto.Dim5Nr = accInt.Account.AccountNr;
                                    dto.Dim5Name = accInt.Account.Name;
                                    break;
                                case 6:
                                    dto.Dim6Id = accInt.AccountId;
                                    dto.Dim6Nr = accInt.Account.AccountNr;
                                    dto.Dim6Name = accInt.Account.Name;
                                    break;
                            }
                        }
                        else
                        {
                            switch (accInt.Account.AccountDim.AccountDimNr)
                            {
                                case 2:
                                    dto.Dim2Id = accInt.AccountId;
                                    dto.Dim2Nr = accInt.Account.AccountNr;
                                    dto.Dim2Name = accInt.Account.Name;
                                    break;
                                case 3:
                                    dto.Dim3Id = accInt.AccountId;
                                    dto.Dim3Nr = accInt.Account.AccountNr;
                                    dto.Dim3Name = accInt.Account.Name;
                                    break;
                                case 4:
                                    dto.Dim4Id = accInt.AccountId;
                                    dto.Dim4Nr = accInt.Account.AccountNr;
                                    dto.Dim4Name = accInt.Account.Name;
                                    break;
                                case 5:
                                    dto.Dim5Id = accInt.AccountId;
                                    dto.Dim5Nr = accInt.Account.AccountNr;
                                    dto.Dim5Name = accInt.Account.Name;
                                    break;
                                case 6:
                                    dto.Dim6Id = accInt.AccountId;
                                    dto.Dim6Nr = accInt.Account.AccountNr;
                                    dto.Dim6Name = accInt.Account.Name;
                                    break;
                            }
                        }
                    }
                }
            }

            return dto;
        }

        public static IEnumerable<VoucherRowDTO> ToDTOs(this IEnumerable<VoucherRow> l, bool includeInternalAccounts, List<AccountDim> dims = null)
        {
            var dtos = new List<VoucherRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeInternalAccounts, dims));
                }
            }
            return dtos;
        }

        #endregion

        #region VoucherHeadIO

        public static VoucherHeadIODTO ToDTO(this VoucherHeadIO e, bool includeRows)
        {
            if (e == null)
                return null;

            VoucherHeadIODTO dto = new VoucherHeadIODTO()
            {
                VoucherHeadIOId = e.VoucherHeadIOId,
                VoucherSeriesId = e.VoucherSeriesId,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = (TermGroup_IOType)e.Type,
                Status = (TermGroup_IOStatus)e.Status,
                Source = (TermGroup_IOSource)e.Source,
                BatchId = e.BatchId,
                ErrorMessage = e.ErrorMessage,
                ImportHeadType = (TermGroup_IOImportHeadType)e.ImportHeadType,
                AccountYearId = e.AccountYearId,
                VoucherNr = e.VoucherNr,
                Date = e.Date,
                Text = e.Text,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Note = e.Note,
                VoucherSeriesName = e.VoucherSeriesName,
                StatusName = e.StatusName,
                IsVatVoucher = e.IsVatVoucher,
            };

            if (includeRows)
                dto.Rows = e.VoucherRowIO.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(true).ToList();

            return dto;
        }

        public static IEnumerable<VoucherHeadIODTO> ToDTOs(this IEnumerable<VoucherHeadIO> l, bool includeRows)
        {
            var dtos = new List<VoucherHeadIODTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        #endregion

        #region VoucherRowIO

        public static VoucherRowIODTO ToDTO(this VoucherRowIO e, bool includeInternalAccounts)
        {
            if (e == null)
                return null;


            VoucherRowIODTO dto = new VoucherRowIODTO()
            {
                VoucherRowIOId = e.VoucherRowIOId,
                VoucherHeadIOId = e.VoucherHeadIOId,
                ActorCompanyId = e.ActorCompanyId,
                Import = e.Import,
                Type = (TermGroup_IOType)e.Type,
                Status = (TermGroup_IOStatus)e.Status,
                Source = (TermGroup_IOSource)e.Source,
                BatchId = e.BatchId,
                ErrorMessage = e.ErrorMessage,
                ImportHeadType = (TermGroup_IOImportHeadType)e.ImportHeadType,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                Text = e.Text,
                Quantity = e.Quantity,
                Amount = e.Amount,
                DebetAmount = e.DebetAmount,
                CreditAmount = e.CreditAmount,
                AccountNr = e.AccountNr,
                AccountDim2Nr = e.AccountDim2Nr,
                AccountDim3Nr = e.AccountDim3Nr,
                AccountDim4Nr = e.AccountDim4Nr,
                AccountDim5Nr = e.AccountDim5Nr,
                AccountDim6Nr = e.AccountDim6Nr,
            };

            /*Account account = e.Account != null && e.AccountStd.Account != null ? e.AccountStd.Account : null;
            dto.Dim1Id = account != null ? account.AccountId : 0;
            dto.Dim1Nr = account != null ? account.AccountNr : String.Empty;
            dto.Dim1Name = account != null ? account.Name : String.Empty;
            dto.Dim1UnitStop = e.AccountStd != null ? e.AccountStd.UnitStop : false;
            dto.Dim1AmountStop = e.AccountStd != null ? e.AccountStd.AmountStop : 1;

            if (includeInternalAccounts)
            {
                foreach (AccountInternal accountInternal in e.AccountInternal)
                {
                    Account accInt = accountInternal.Account;

                    if (accInt != null && accInt.AccountDim != null)
                    {
                        switch (accInt.AccountDim.AccountDimNr)
                        {
                            case 2:
                                dto.Dim2Id = accInt.AccountId;
                                dto.Dim2Nr = accInt.AccountNr;
                                dto.Dim2Name = accInt.Name;
                                break;
                            case 3:
                                dto.Dim3Id = accInt.AccountId;
                                dto.Dim3Nr = accInt.AccountNr;
                                dto.Dim3Name = accInt.Name;
                                break;
                            case 4:
                                dto.Dim4Id = accInt.AccountId;
                                dto.Dim4Nr = accInt.AccountNr;
                                dto.Dim4Name = accInt.Name;
                                break;
                            case 5:
                                dto.Dim5Id = accInt.AccountId;
                                dto.Dim5Nr = accInt.AccountNr;
                                dto.Dim5Name = accInt.Name;
                                break;
                            case 6:
                                dto.Dim6Id = accInt.AccountId;
                                dto.Dim6Nr = accInt.AccountNr;
                                dto.Dim6Name = accInt.Name;
                                break;
                        }
                    }
                }
            }*/

            return dto;
        }

        public static IEnumerable<VoucherRowIODTO> ToDTOs(this IEnumerable<VoucherRowIO> l, bool includeInternalAccounts)
        {
            var dtos = new List<VoucherRowIODTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeInternalAccounts));
                }
            }
            return dtos;
        }


        #endregion

        #region VoucherRowHistory

        public static VoucherRowHistoryDTO ToDTO(this VoucherRowHistory e)
        {
            if (e == null)
                return null;

            VoucherRowHistoryDTO dto = new VoucherRowHistoryDTO()
            {
                VoucherRowHistoryId = e.VoucherRowHistoryId,
                VoucherRowId = e.VoucherRowId,
                AccountId = e.AccountId,
                UserId = e.UserId,
                Date = e.Date,
                Amount = e.Amount,
                AmountEntCurrency = e.AmountEntCurrency,
                Quantity = e.Quantity,
                Text = e.Text,
                EventText = e.EventText,
                EventType = e.EventType,
                FieldModified = e.FieldModified,
                VoucherHeadIdModified = e.VoucherHeadIdModified,
                AccountDimId = e.AccountDimId,
            };

            return dto;
        }

        public static IEnumerable<VoucherRowHistoryDTO> ToDTOs(this IEnumerable<VoucherRowHistory> l)
        {
            var dtos = new List<VoucherRowHistoryDTO>();
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
    }
}
