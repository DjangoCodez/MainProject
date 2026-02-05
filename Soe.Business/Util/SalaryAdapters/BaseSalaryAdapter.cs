using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.SalaryAdapters
{
    public class BaseSalaryAdapter
    {
        protected string FillStrWithZeros(int nrOfZeros)
        {
            return FillWithChar(nrOfZeros, '0');
        }

        protected string FillStrWithZeros(int nrOfZeros, string value, bool beginning = false)
        {
            return FillWithChar('0', nrOfZeros, value, true, beginning);
        }

        protected string FillStrWithBlanks(int nrOfBlanks)
        {
            return FillWithChar(nrOfBlanks, ' ');
        }

        protected string FillStrWithBlanks(int nrOfZeros, string value, bool beginning = false)
        {
            return FillWithChar(' ', nrOfZeros, value, true, beginning);
        }

        protected string FillWithChar(Char character, int targetSize, string originValue, bool truncate = false, bool beginning = false)
        {
            if (targetSize > originValue.Length)
            {
                string newChars = string.Empty;
                int diff = targetSize - originValue.Length;
                for (int i = 0; i < diff; i++)
                {
                    newChars += character;
                }
                if (beginning)
                    return (newChars + originValue);
                else
                    return (originValue + newChars);
            }
            else if (truncate && targetSize < originValue.Length)
                return originValue.Substring(0, targetSize - 1);
            else
                return originValue;
        }

        protected string FillWithChar(int length, char character)
        {
            string value = string.Empty;
            for (int i = 0; i < length; i++)
            {
                value += character;
            }

            return value;
        }

        /// <summary>
        /// Format 8 hours => 800 and 7 hours and 30 minutes => 750
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        protected string GetTimeFromMinutes(decimal minutes)
        {
            decimal value = minutes;
            if (value != 0)
                value /= 60;
            value = Math.Round(value, 2, MidpointRounding.ToEven);
            return value.ToString().Replace(".", "").Replace(",", "");
        }

        protected string GetAccountNr(TermGroup_SieAccountDim accountDim, List<AccountInternal> internalAccounts)
        {
            foreach (AccountInternal internalAccount in internalAccounts)
            {
                if (internalAccount.Account != null && internalAccount.Account.AccountDim != null && internalAccount.Account.AccountDim.SysSieDimNr.HasValue)
                {
                    if (internalAccount.Account.AccountDim.SysSieDimNr.Value == (int)accountDim)
                        return internalAccount.Account.AccountNr;
                }
            }
            return "";
        }
    }

    public static class SalaryAdapterExtension
    {
        public static bool IsInTimeBox(this TransactionItem e, bool mergeOnlyHourlySalaryInTimeBox, bool noTimeBoxOnPresence)
        {
            if (noTimeBoxOnPresence)
                return false;

            if (mergeOnlyHourlySalaryInTimeBox)
                return PayrollRulesUtil.IsHourlySalary(e.SysPayrollTypeLevel1, e.SysPayrollTypeLevel2, e.SysPayrollTypeLevel3, e.SysPayrollTypeLevel4);

            if (!e.IsAbsence() && e.Amount == 0 && !e.IsRegistrationQuantity)
                return true;

            return false;
        }
    }
}
