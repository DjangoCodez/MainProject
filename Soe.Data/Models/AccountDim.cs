using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class AccountDim : ICreatedModified, IState
    {
        public int SysSieDimNrOrAccountDimNr
        {
            get
            {
                return this.SysSieDimNr.HasValue ? this.SysSieDimNr.Value : this.AccountDimNr;
            }
        }
        public bool IsStandard
        {
            get
            {
                return this.AccountDimNr == Constants.ACCOUNTDIM_STANDARD;
            }
        }
        public bool IsInternal
        {
            get
            {
                return this.AccountDimNr != Constants.ACCOUNTDIM_STANDARD;
            }
        }

        public int Level { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region AccountDim

        public static AccountDimDTO ToDTO(this AccountDim e, bool includeAccounts = false, bool includeInternalAccounts = false)
        {
            if (e == null)
                return null;

            #region Try load
            try
            {
                if (e.Parent == null && !e.IsAdded() && !e.ParentReference.IsLoaded)
                {
                    e.ParentReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountDim.cs e.ParentReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }
            #endregion

            return new AccountDimDTO()
            {
                AccountDimId = e.AccountDimId,
                ActorCompanyId = e.ActorCompanyId,
                SysAccountStdTypeParentId = e.SysAccountStdTypeParentId,
                ParentAccountDimId = e.Parent?.AccountDimId,
                SysSieDimNr = e.SysSieDimNr,
                AccountDimNr = e.AccountDimNr,
                Name = e.Name,
                ShortName = e.ShortName,
                MinChar = e.MinChar,
                MaxChar = e.MaxChar,
                LinkedToProject = e.LinkedToProject,
                LinkedToShiftType = e.LinkedToShiftType,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                UseInSchedulePlanning = e.UseInSchedulePlanning,
                ExcludeinAccountingExport = e.ExcludeinAccountingExport,
                ExcludeinSalaryReport = e.ExcludeinSalaryExport,
                UseVatDeduction = e.UseVatDeduction,
                MandatoryInOrder = e.MandatoryInOrder,
                MandatoryInCustomerInvoice = e.MandatoryInCustomerInvoice,
                ParentAccountDimName = e.Parent != null ? e.Parent.Name : String.Empty,
                OnlyAllowAccountsWithParent = e.OnlyAllowAccountsWithParent,
                Accounts = includeAccounts ? e.Account?.Where(i => i.State == (int)SoeEntityState.Active).ToDTOs(includeInternalAccounts: includeInternalAccounts) : null,
            };
        }

        public static List<AccountDimDTO> ToDTOs(this IEnumerable<AccountDim> l, bool includeAccounts = false, bool loadInternalAccounts = false)
        {
            var dtos = new List<AccountDimDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccounts, loadInternalAccounts));
                }
            }
            return dtos;
        }

        public static List<AccountDimGridDTO> ToGridDTOs(this IEnumerable<AccountDim> l)
        {
            var dtos = new List<AccountDimGridDTO>();
            if (!(l is null))
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        private static AccountDimGridDTO ToGridDTO(this AccountDim e)
        {
            if (e is null)
                return null;

            return new AccountDimGridDTO
            {
                AccountDimId = e.AccountDimId,
                AccountDimNr = e.AccountDimNr,
                ActorCompanyId = e.ActorCompanyId,
                ShortName = e.ShortName,
                Name = e.Name,
                ParentAccountDimName = e.Parent != null ? e.Parent.Name : String.Empty,
                State = (SoeEntityState)e.State,
                SysSieDimNr = e.SysSieDimNr,
                ExcludeinAccountingExport = e.ExcludeinAccountingExport,
                ExcludeinSalaryReport = e.ExcludeinSalaryExport,
                UseInSchedulePlanning = e.UseInSchedulePlanning,
                OnlyAllowAccountsWithParent = e.OnlyAllowAccountsWithParent,
            };
        }

        public static AccountDimSmallDTO ToSmallDTO(this AccountDim e, bool includeAccounts = false, bool includeInternalAccounts = false, bool includeInactives = false)
        {
            if (e == null)
                return null;

            #region Try load
            try
            {
                if (!e.IsAdded() && includeAccounts && !e.Account.IsLoaded)
                {
                    e.Account.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("AccountDim.cs e.Account");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }
            #endregion

            AccountDimSmallDTO dto = new AccountDimSmallDTO()
            {
                AccountDimId = e.AccountDimId,
                AccountDimNr = e.AccountDimNr,
                Name = e.Name,
                ParentAccountDimId = e.Parent?.AccountDimId,
                LinkedToShiftType = e.LinkedToShiftType,
                MandatoryInOrder = e.MandatoryInOrder,  
                MandatoryInCustomerInvoice = e.MandatoryInCustomerInvoice,  
                Level = e.Level,
            };

            if (includeAccounts && !e.Account.IsNullOrEmpty())
            {
                if (includeInactives)
                    dto.Accounts = e.Account.Where(i => i.State == (int)SoeEntityState.Active || i.State == (int)SoeEntityState.Inactive).OrderBy(i => i.AccountNrSort).ToDTOs(includeInternalAccounts: includeInternalAccounts);
                else
                    dto.Accounts = e.Account.Where(i => i.State == (int)SoeEntityState.Active).OrderBy(i => i.AccountNrSort).ToDTOs(includeInternalAccounts: includeInternalAccounts);
            }

            return dto;
        }

        public static IEnumerable<AccountDimSmallDTO> ToSmallDTOs(this IEnumerable<AccountDim> l, bool includeAccounts = false, bool includeInternalAccounts = false, bool includeInactives = false)
        {
            var dtos = new List<AccountDimSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO(includeAccounts, includeInternalAccounts, includeInactives));
                }
            }
            return dtos;
        }

        public static List<AccountDim> GetInternals(this List<AccountDim> l)
        {
            return l?.Where(i => i.AccountDimNr != Constants.ACCOUNTDIM_STANDARD).ToList() ?? new List<AccountDim>();
        }

        public static List<AccountDim> GetChildrensByName(this AccountDim e, List<AccountDim> accountDims)
        {
            if (e == null || accountDims == null)
                return new List<AccountDim>();
            return accountDims.Where(i => i.Parent != null && i.Parent.AccountDimId == e.AccountDimId).OrderBy(i => i.Name).ToList();
        }

        public static AccountDim GetStandard(this IEnumerable<AccountDim> l)
        {
            return l?.FirstOrDefault(ad => ad.AccountDimNr == Constants.ACCOUNTDIM_STANDARD);
        }

        public static AccountDim GetByNr(this IEnumerable<AccountDim> accountDims, int accountDimNr)
        {
            return accountDims?.FirstOrDefault(i => i.AccountDimNr == accountDimNr);
        }

        public static void CalculateLevels(this List<AccountDim> l)
        {
            if (l.IsNullOrEmpty())
                return;

            l = l.Where(i => i.IsInternal && i.State == (int)SoeEntityState.Active).ToList();

            //Can only have 1 highest, otherwise invalid setup
            if (l.Count(i => i.Parent == null) != 1)
                return;

            AccountDim accountDimTopLevel = l.FirstOrDefault(i => i.Parent == null);
            l.CalculateLevels(accountDimTopLevel, 1);
        }

        public static void CalculateLevels(this List<AccountDim> l, AccountDim parentAccountDim, int level)
        {
            if (l.IsNullOrEmpty() || parentAccountDim == null)
                return;

            parentAccountDim.Level = level;

            level++;
            foreach (AccountDim accountDimChild in parentAccountDim.GetChildrensByName(l))
            {
                l.CalculateLevels(accountDimChild, level);
            }
        }

        #endregion
    }
}
