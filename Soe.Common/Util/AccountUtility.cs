using SoftOne.Soe.Common.DTO;
using System;

namespace SoftOne.Soe.Common.Util
{
    public static class AccountUtility
    {
        public static void SetRowItemAccounts(AccountingRowDTO rowItem, AccountDTO account, bool isSalesRow, bool setInternalAccount)
        {
            // Set standard account
            rowItem.Dim1Id = account != null ? account.AccountId : 0;
            rowItem.Dim1Nr = account != null ? account.AccountNr : String.Empty;
            rowItem.Dim1Name = account != null ? account.Name : String.Empty;
            rowItem.Dim1Disabled = false;
            rowItem.Dim1Mandatory = true;
            rowItem.Dim1Stop = true;
            rowItem.QuantityStop = account != null ? account.UnitStop : false;
            rowItem.Unit = account != null ? account.Unit : String.Empty;
            rowItem.AmountStop = account != null ? account.AmountStop : 1;
            rowItem.RowTextStop = account != null ? account.RowTextStop : true;

            if (!setInternalAccount)
                return;

            // Clear internal accounts
            rowItem.Dim2Id = 0;
            rowItem.Dim2Nr = String.Empty;
            rowItem.Dim2Name = String.Empty;
            rowItem.Dim2Disabled = false;
            rowItem.Dim2Mandatory = false;
            rowItem.Dim2Stop = false;
            rowItem.Dim3Id = 0;
            rowItem.Dim3Nr = String.Empty;
            rowItem.Dim3Name = String.Empty;
            rowItem.Dim3Disabled = false;
            rowItem.Dim3Mandatory = false;
            rowItem.Dim3Stop = false;
            rowItem.Dim4Id = 0;
            rowItem.Dim4Nr = String.Empty;
            rowItem.Dim4Name = String.Empty;
            rowItem.Dim4Disabled = false;
            rowItem.Dim4Mandatory = false;
            rowItem.Dim4Stop = false;
            rowItem.Dim5Id = 0;
            rowItem.Dim5Nr = String.Empty;
            rowItem.Dim5Name = String.Empty;
            rowItem.Dim5Disabled = false;
            rowItem.Dim5Mandatory = false;
            rowItem.Dim5Stop = false;
            rowItem.Dim6Id = 0;
            rowItem.Dim6Nr = String.Empty;
            rowItem.Dim6Name = String.Empty;
            rowItem.Dim6Disabled = false;
            rowItem.Dim6Mandatory = false;
            rowItem.Dim6Stop = false;

            // Set internal accounts
            if (account != null && account.AccountInternals != null)
            {
                // Get internal accounts from the account
                foreach (AccountInternalDTO accInt in account.AccountInternals)
                {
                    switch (accInt.AccountDimNr)
                    {
                        case (2):
                            rowItem.Dim2Id = accInt != null ? accInt.AccountId : 0;
                            rowItem.Dim2Nr = accInt != null ? accInt.AccountNr : String.Empty;
                            rowItem.Dim2Name = accInt != null ? accInt.Name : String.Empty;
                            rowItem.Dim2Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim2Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            rowItem.Dim2Stop = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                            break;
                        case (3):
                            rowItem.Dim3Id = accInt != null ? accInt.AccountId : 0;
                            rowItem.Dim3Nr = accInt != null ? accInt.AccountNr : String.Empty;
                            rowItem.Dim3Name = accInt != null ? accInt.Name : String.Empty;
                            rowItem.Dim3Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim3Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            rowItem.Dim3Stop = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                            break;
                        case (4):
                            rowItem.Dim4Id = accInt != null ? accInt.AccountId : 0;
                            rowItem.Dim4Nr = accInt != null ? accInt.AccountNr : String.Empty;
                            rowItem.Dim4Name = accInt != null ? accInt.Name : String.Empty;
                            rowItem.Dim4Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim4Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            rowItem.Dim4Stop = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                            break;
                        case (5):
                            rowItem.Dim5Id = accInt != null ? accInt.AccountId : 0;
                            rowItem.Dim5Nr = accInt != null ? accInt.AccountNr : String.Empty;
                            rowItem.Dim5Name = accInt != null ? accInt.Name : String.Empty;
                            rowItem.Dim5Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim5Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            rowItem.Dim5Stop = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                            break;
                        case (6):
                            rowItem.Dim6Id = accInt != null ? accInt.AccountId : 0;
                            rowItem.Dim6Nr = accInt != null ? accInt.AccountNr : String.Empty;
                            rowItem.Dim6Name = accInt != null ? accInt.Name : String.Empty;
                            rowItem.Dim6Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim6Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            rowItem.Dim6Stop = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Stop;
                            break;
                    }
                }
            }
        }

        public static void SetRowItemAccounts(TimeTransactionItem rowItem, AccountDTO account)
        {
            // Set standard account
            rowItem.Dim1Id = account?.AccountId ?? 0;
            rowItem.Dim1Nr = account?.AccountNr ?? string.Empty;
            rowItem.Dim1Name = account?.Name ?? string.Empty;
            rowItem.Dim1Disabled = false;
            rowItem.Dim1Mandatory = true;

            // Clear internal accounts
            rowItem.Dim2Id = 0;
            rowItem.Dim2Nr = string.Empty;
            rowItem.Dim2Name = string.Empty;
            rowItem.Dim2Disabled = false;
            rowItem.Dim2Mandatory = false;
            rowItem.Dim3Id = 0;
            rowItem.Dim3Nr = string.Empty;
            rowItem.Dim3Name = string.Empty;
            rowItem.Dim3Disabled = false;
            rowItem.Dim3Mandatory = false;
            rowItem.Dim4Id = 0;
            rowItem.Dim4Nr = string.Empty;
            rowItem.Dim4Name = string.Empty;
            rowItem.Dim4Disabled = false;
            rowItem.Dim4Mandatory = false;
            rowItem.Dim5Id = 0;
            rowItem.Dim5Nr = string.Empty;
            rowItem.Dim5Name = string.Empty;
            rowItem.Dim5Disabled = false;
            rowItem.Dim5Mandatory = false;
            rowItem.Dim6Id = 0;
            rowItem.Dim6Nr = string.Empty;
            rowItem.Dim6Name = string.Empty;
            rowItem.Dim6Disabled = false;
            rowItem.Dim6Mandatory = false;

            // Set internal accounts
            if (account?.AccountInternals != null)
            {
                foreach (AccountInternalDTO accInt in account.AccountInternals)
                {
                    if (accInt == null)
                        continue;
                    switch (accInt.AccountDimNr)
                    {
                        case (2):
                            rowItem.Dim2Id = accInt.AccountId;
                            rowItem.Dim2Nr = accInt.AccountNr;
                            rowItem.Dim2Name = accInt.Name;
                            rowItem.Dim2Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim2Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            break;
                        case (3):
                            rowItem.Dim3Id = accInt.AccountId;
                            rowItem.Dim3Nr = accInt.AccountNr;
                            rowItem.Dim3Name = accInt.Name;
                            rowItem.Dim3Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim3Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            break;
                        case (4):
                            rowItem.Dim4Id = accInt.AccountId;
                            rowItem.Dim4Nr = accInt.AccountNr;
                            rowItem.Dim4Name = accInt.Name;
                            rowItem.Dim4Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim4Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            break;
                        case (5):
                            rowItem.Dim5Id = accInt.AccountId;
                            rowItem.Dim5Nr = accInt.AccountNr;
                            rowItem.Dim5Name = accInt.Name;
                            rowItem.Dim5Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim5Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            break;
                        case (6):
                            rowItem.Dim6Id = accInt.AccountId;
                            rowItem.Dim6Nr = accInt.AccountNr;
                            rowItem.Dim6Name = accInt.Name;
                            rowItem.Dim6Disabled = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                            rowItem.Dim6Mandatory = accInt.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                            break;
                    }
                }
            }
        }

        public static void SetRowItemAccounts(AccountDistributionRowDTO rowItem, AccountDTO account, int dimNr, string keepAccountNr, string keepAccountName)
        {
            int accountId = account?.AccountId ?? 0;
            string accountNr = account?.AccountNr ?? string.Empty;
            string accountName = account?.Name ?? string.Empty;

            if (accountId == 0)
            {
                accountNr = keepAccountNr;
                accountName = keepAccountName;
            }

            switch (dimNr)
            {
                case (1):
                    // User selected same account, no action
                    if (account != null && account.AccountId == rowItem.Dim1Id)
                        return;

                    // Set standard account
                    rowItem.Dim1Id = accountId;
                    rowItem.Dim1Nr = accountNr;
                    rowItem.Dim1Name = accountName;
                    rowItem.Dim1Disabled = false;
                    rowItem.Dim1Mandatory = true;

                    if (account?.AccountInternals != null)
                    {
                        foreach (AccountInternalDTO acc in account.AccountInternals)
                        {
                            switch (acc.AccountDimNr)
                            {
                                case (2):
                                    rowItem.Dim2Id = acc.AccountId;
                                    rowItem.Dim2Nr = acc.AccountNr;
                                    rowItem.Dim2Name = acc.Name;
                                    rowItem.Dim2Disabled = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                    rowItem.Dim2Mandatory = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                    break;
                                case (3):
                                    rowItem.Dim3Id = acc.AccountId;
                                    rowItem.Dim3Nr = acc.AccountNr;
                                    rowItem.Dim3Name = acc.Name;
                                    rowItem.Dim3Disabled = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                    rowItem.Dim3Mandatory = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                    break;
                                case (4):
                                    rowItem.Dim4Id = acc.AccountId;
                                    rowItem.Dim4Nr = acc.AccountNr;
                                    rowItem.Dim4Name = acc.Name;
                                    rowItem.Dim4Disabled = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                    rowItem.Dim4Mandatory = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                    break;
                                case (5):
                                    rowItem.Dim5Id = acc.AccountId;
                                    rowItem.Dim5Nr = acc.AccountNr;
                                    rowItem.Dim5Name = acc.Name;
                                    rowItem.Dim5Disabled = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                    rowItem.Dim5Mandatory = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                    break;
                                case (6):
                                    rowItem.Dim6Id = acc.AccountId;
                                    rowItem.Dim6Nr = acc.AccountNr;
                                    rowItem.Dim6Name = acc.Name;
                                    rowItem.Dim6Disabled = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Warn;
                                    rowItem.Dim6Mandatory = acc.MandatoryLevel == (int)TermGroup_AccountMandatoryLevel.Mandatory;
                                    break;
                            }
                        }
                    }
                    break;
                case (2):
                    if (account != null && (account.AccountId == rowItem.Dim2Id && account.AccountId != 0))
                        return;

                    rowItem.Dim2Id = accountId;
                    rowItem.Dim2Nr = accountNr;
                    rowItem.Dim2Name = accountName;
                    rowItem.Dim2KeepSourceRowAccount = account != null && accountId == 0;
                    break;
                case (3):
                    if (account != null && (account.AccountId == rowItem.Dim3Id && account.AccountId != 0))
                        return;

                    rowItem.Dim3Id = accountId;
                    rowItem.Dim3Nr = accountNr;
                    rowItem.Dim3Name = accountName;
                    rowItem.Dim3KeepSourceRowAccount = account != null && accountId == 0;
                    break;
                case (4):
                    if (account != null && (account.AccountId == rowItem.Dim4Id && account.AccountId != 0))
                        return;

                    rowItem.Dim4Id = accountId;
                    rowItem.Dim4Nr = accountNr;
                    rowItem.Dim4Name = accountName;
                    rowItem.Dim4KeepSourceRowAccount = account != null && accountId == 0;
                    break;
                case (5):
                    if (account != null && (account.AccountId == rowItem.Dim5Id && account.AccountId != 0))
                        return;

                    rowItem.Dim5Id = accountId;
                    rowItem.Dim5Nr = accountNr;
                    rowItem.Dim5Name = accountName;
                    rowItem.Dim5KeepSourceRowAccount = account != null && accountId == 0;
                    break;
                case (6):
                    if (account != null && (account.AccountId == rowItem.Dim6Id && account.AccountId != 0))
                        return;

                    rowItem.Dim6Id = accountId;
                    rowItem.Dim6Nr = accountNr;
                    rowItem.Dim6Name = accountName;
                    rowItem.Dim6KeepSourceRowAccount = account != null && accountId == 0;
                    break;
            }
        }
    }
}
