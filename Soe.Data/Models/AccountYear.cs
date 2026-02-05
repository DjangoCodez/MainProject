using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AccountYear : ICreatedModified
    {
        public string StatusText { get; set; }
    }

    public partial class AccountYearBalanceHead : ICreatedModified
    {
        public string AccountTypeName { get; set; }
        public bool isDiffRow { get; set; }
        public string InternalIds { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region AccountYear

        public static AccountYearDTO ToDTO(this AccountYear e, bool setNoOfPeriods = false, bool getPeriods = false, bool doVoucherCheck = false)
        {
            if (e == null)
                return null;

            AccountYearDTO dto = new AccountYearDTO()
            {
                AccountYearId = e.AccountYearId,
                ActorCompanyId = e.ActorCompanyId,
                From = e.From,
                To = e.To,
                Status = (TermGroup_AccountStatus)e.Status,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                StatusText = e.StatusText,
                YearFromTo = e.From.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + " - " + e.To.ToString("yyyyMMdd", CultureInfo.CurrentCulture),
            };

            if (setNoOfPeriods)
                dto.NoOfPeriods = e.AccountPeriod.Count;

            if (getPeriods)
            {
                dto.Periods = new List<AccountPeriodDTO>();
                foreach (var period in e.AccountPeriod)
                {
                    if (!period.HasVouchers.HasValue && doVoucherCheck && !period.VoucherHead.IsLoaded)
                    {
                        period.VoucherHead.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("AccountYear.cs period.VoucherHead");
                    }

                    dto.Periods.Add(period.ToDTO(doVoucherCheck));
                }
            }

            return dto;
        }

        public static IEnumerable<AccountYearDTO> ToDTOs(this IEnumerable<AccountYear> l, bool setNoOfPeriods = false, bool getPeriods = false)
        {
            return l?.Select(s => s.ToDTO(setNoOfPeriods, getPeriods)).ToList() ?? new List<AccountYearDTO>();
        }

        public static string GetFromToShortString(this AccountYear e)
        {
            return e.From.ToString("yyyyMM") + "-" + e.To.ToString("yyyyMM");
        }

        #endregion

        #region AccountYearBalanceHead

        public static AccountYearBalanceHeadDTO ToDTO(this AccountYearBalanceHead e)
        {
            if (e == null)
                return null;

            #region Load references
            if (!e.AccountStdReference.IsLoaded)
            {
                e.AccountStdReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("AccountYear.cs e.AccountStdReference");
            }
            if (!e.AccountYearReference.IsLoaded)
            {
                e.AccountYearReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("AccountYear.cs e.AccountYearReference");
            }
            #endregion

            AccountYearBalanceHeadDTO dto = new AccountYearBalanceHeadDTO()
            {
                AccountYearBalanceHeadId = e.AccountYearBalanceHeadId,
                AccountYearId = e.AccountYear.AccountYearId,
                AccountId = e.AccountStd.AccountId,
                Balance = e.Balance,
                BalanceEntCurrency = e.BalanceEntCurrency,
                Quantity = e.Quantity,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };

            if (!e.AccountInternal.IsNullOrEmpty())
            {
                dto.rows = new List<AccountYearBalanceRowDTO>();
                foreach (var a in e.AccountInternal)
                {
                    dto.rows.Add(new AccountYearBalanceRowDTO()
                    {
                        AccountId = a.AccountId,
                        AccountYearBalanceRowId = e.AccountYearBalanceHeadId,
                    });
                }
            }

            return dto;
        }

        public static IEnumerable<AccountYearBalanceHeadDTO> ToDTOs(this IEnumerable<AccountYearBalanceHead> l)
        {
            var dtos = new List<AccountYearBalanceHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static AccountYearBalanceFlatDTO ToFlatDTO(this AccountYearBalanceHead e, List<AccountDim> accountDims, bool loadReferences = true)
        {
            if (e == null)
                return null;

            #region Load references
            if (loadReferences)
            {
                if (!e.AccountStdReference.IsLoaded)
                {
                    e.AccountStdReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountYear.cs e.AccountStdReference");
                }
                if (!e.AccountYearReference.IsLoaded)
                {
                    e.AccountYearReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountYear.cs e.AccountYearReference");
                }
            }
            #endregion

            AccountYearBalanceFlatDTO dto = new AccountYearBalanceFlatDTO()
            {
                AccountYearBalanceHeadId = e.AccountYearBalanceHeadId,
                AccountYearId = e.AccountYear.AccountYearId,
                Balance = e.Balance,
                BalanceEntCurrency = e.BalanceEntCurrency,
                Quantity = e.Quantity,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                isDiffRow = e.isDiffRow,
            };

            if (e.Balance < 0)
                dto.CreditAmount = e.Balance * -1;
            else
                dto.DebitAmount = e.Balance;

            // Accounts
            AccountStd accStd = e.AccountStd;
            if (accStd != null)
            {
                dto.Dim1Id = accStd.AccountId;
                dto.Dim1Nr = accStd.Account?.AccountNr ?? string.Empty;
                dto.Dim1Name = accStd.Account?.Name ?? string.Empty;
                dto.Dim1TypeName = e.AccountTypeName;
            }

            if (!e.AccountInternal.IsNullOrEmpty())
            {
                foreach (AccountInternal accountInternal in e.AccountInternal.Where(a => a.Account != null))
                {
                    int position = 2;
                    foreach (AccountDim accountDim in accountDims)
                    {
                        if (accountInternal.Account.AccountDimId == accountDim.AccountDimId)
                            dto.SetAccountValues(position, accountInternal.AccountId, accountInternal.Account.AccountNr, accountInternal.Account.Name);
                        position++;
                    }
                }
            }

            return dto;
        }

        public static IEnumerable<AccountYearBalanceFlatDTO> ToFlatDTOs(this IEnumerable<AccountYearBalanceHead> l, List<AccountDim> dims, bool checkReferences = true)
        {
            var dtos = new List<AccountYearBalanceFlatDTO>();
            if (l != null)
            {
                var counter = 1;
                foreach (var e in l)
                {
                    var dto = e.ToFlatDTO(dims, checkReferences);
                    dto.RowNr = counter;
                    dtos.Add(dto);
                    counter++;
                }
            }
            return dtos;
        }

        #endregion
    }
}
