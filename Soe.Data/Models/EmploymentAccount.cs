using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class EmploymentAccountStd
    {
        public bool IsFixedAccounting
        {
            get
            {
                return (this.Type == (int)EmploymentAccountType.Fixed1 ||
                       this.Type == (int)EmploymentAccountType.Fixed2 ||
                       this.Type == (int)EmploymentAccountType.Fixed3 ||
                       this.Type == (int)EmploymentAccountType.Fixed4 ||
                       this.Type == (int)EmploymentAccountType.Fixed5 ||
                       this.Type == (int)EmploymentAccountType.Fixed6 ||
                       this.Type == (int)EmploymentAccountType.Fixed7 ||
                       this.Type == (int)EmploymentAccountType.Fixed8);
            }
        }
        public string GetAccountingIdString(string fallbackAccountId = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.AccountId.IsNullOrEmpty() ? fallbackAccountId : this.AccountId.ToString());

            if (this.AccountInternal != null)
            {
                foreach (var accountInternal in this.AccountInternal.OrderBy(x => x.AccountId).ToList())
                {
                    if (sb.Length > 0)
                        sb.Append(",");

                    sb.Append(accountInternal.AccountId);
                }
            }
            return sb.ToString();
        }
    }

    public static partial class EntityExtensions
    {
        #region EmploymentAccountStd

        public static EmploymentAccountStd GetAccount(this IEnumerable<EmploymentAccountStd> l, int employeeId, EmploymentAccountType employeeAccountType, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            var accounts = (from eas in l
                            where eas.Type == (int)employeeAccountType &&
                            eas.Employment != null &&
                            eas.Employment.EmployeeId == employeeId &&
                            eas.Employment.State == (int)SoeEntityState.Active
                            select eas).ToList();

            return accounts.GetAccount(date.Value);
        }

        public static EmploymentAccountStd GetAccount(this IEnumerable<EmploymentAccountStd> l, DateTime date)
        {
            return l?.FirstOrDefault(eas =>
                eas.Employment != null &&
                (!eas.Employment.DateFrom.HasValue || CalendarUtility.GetBeginningOfDay(eas.Employment.DateFrom.Value, 0) <= date) &&
                (!eas.Employment.DateTo.HasValue || CalendarUtility.GetEndOfDay(eas.Employment.DateTo.Value, 0) >= date));
        }

        #endregion
    }
}
