using SoftOne.Soe.Common.Attributes;
﻿using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class AccountingSettingDTO
    {
        #region Variables

        public int Type { get; set; }
        public int DimNr { get; set; }
        public string DimName { get; set; }

        public int Account1Id { get; set; }
        public string Account1Nr { get; set; }
        public string Account1Name { get; set; }

        public int Account2Id { get; set; }
        public string Account2Nr { get; set; }
        public string Account2Name { get; set; }

        public int Account3Id { get; set; }
        public string Account3Nr { get; set; }
        public string Account3Name { get; set; }

        public int Account4Id { get; set; }
        public string Account4Nr { get; set; }
        public string Account4Name { get; set; }

        public int Account5Id { get; set; }
        public string Account5Nr { get; set; }
        public string Account5Name { get; set; }

        public int Account6Id { get; set; }
        public string Account6Nr { get; set; }
        public string Account6Name { get; set; }

        public int Account7Id { get; set; }
        public string Account7Nr { get; set; }
        public string Account7Name { get; set; }

        public int Account8Id { get; set; }
        public string Account8Nr { get; set; }
        public string Account8Name { get; set; }

        public int Account9Id { get; set; }
        public string Account9Nr { get; set; }
        public string Account9Name { get; set; }

        public int Account10Id { get; set; }
        public string Account10Nr { get; set; }
        public string Account10Name { get; set; }

        public decimal Percent1 { get; set; }

        public decimal Percent2 { get; set; }

        public decimal Percent3 { get; set; }

        public decimal Percent4 { get; set; }

        public decimal Percent5 { get; set; }

        public decimal Percent6 { get; set; }

        public decimal Percent7 { get; set; }

        public decimal Percent8 { get; set; }

        public decimal Percent9 { get; set; }

        public decimal Percent10 { get; set; }

        #endregion

        #region Static methods

        public static IEnumerable<AccountingSettingDTO> Copy(IEnumerable<AccountingSettingDTO> accountSettings)
        {
            foreach (AccountingSettingDTO accountSetting in accountSettings)
            {
                yield return AccountingSettingDTO.Copy(accountSetting);
            }
        }

        public static AccountingSettingDTO Copy(AccountingSettingDTO accountSetting)
        {
            return new AccountingSettingDTO()
            {
                Type = accountSetting.Type,
                DimNr = accountSetting.DimNr,
                DimName = accountSetting.DimName,

                Account1Id = accountSetting.Account1Id,
                Account1Nr = accountSetting.Account1Nr,
                Account1Name = accountSetting.Account1Name,
                Percent1 = accountSetting.Percent1,

                Account2Id = accountSetting.Account2Id,
                Account2Nr = accountSetting.Account2Nr,
                Account2Name = accountSetting.Account2Name,
                Percent2 = accountSetting.Percent2,

                Account3Id = accountSetting.Account3Id,
                Account3Nr = accountSetting.Account3Nr,
                Account3Name = accountSetting.Account3Name,
                Percent3 = accountSetting.Percent3,

                Account4Id = accountSetting.Account4Id,
                Account4Nr = accountSetting.Account4Nr,
                Account4Name = accountSetting.Account4Name,
                Percent4 = accountSetting.Percent4,

                Account5Id = accountSetting.Account5Id,
                Account5Nr = accountSetting.Account5Nr,
                Account5Name = accountSetting.Account5Name,
                Percent5 = accountSetting.Percent5,

                Account6Id = accountSetting.Account6Id,
                Account6Nr = accountSetting.Account6Nr,
                Account6Name = accountSetting.Account6Name,
                Percent6 = accountSetting.Percent6,

                Account7Id = accountSetting.Account7Id,
                Account7Nr = accountSetting.Account7Nr,
                Account7Name = accountSetting.Account7Name,
                Percent7 = accountSetting.Percent7,

                Account8Id = accountSetting.Account8Id,
                Account8Nr = accountSetting.Account8Nr,
                Account8Name = accountSetting.Account8Name,
                Percent8 = accountSetting.Percent8,

                Account9Id = accountSetting.Account9Id,
                Account9Nr = accountSetting.Account9Nr,
                Account9Name = accountSetting.Account9Name,
                Percent9 = accountSetting.Percent9,

                Account10Id = accountSetting.Account10Id,
                Account10Nr = accountSetting.Account10Nr,
                Account10Name = accountSetting.Account10Name,
                Percent10 = accountSetting.Percent10,
            };
        }

        #endregion
    }
}
