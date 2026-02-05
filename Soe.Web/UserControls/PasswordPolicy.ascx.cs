using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class PasswordPolicy : ControlBase
    {
        public void Populate()
        {
            SettingManager sm = new SettingManager(PageBase.ParameterObject);
            int passwordMinLength = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMinLength, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
            if (passwordMinLength == 0)
                passwordMinLength = Constants.PASSWORD_DEFAULT_MIN_LENGTH;
            int passwordMaxLength = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.CorePasswordMaxLength, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
            if (passwordMaxLength == 0)
                passwordMaxLength = Constants.PASSWORD_DEFAULT_MAX_LENGTH;

            InstructionList.HeaderText = PageBase.GetText(1583, "Lösenordspolicy");
            InstructionList.Instructions = new List<string>()
			{
				String.Format(PageBase.GetText(1584, "Måste vara mellan {0} och {1} tecken"), passwordMinLength, passwordMaxLength),
				PageBase.GetText(1585, "Måste innehålla både bokstäver och siffror"),
				PageBase.GetText(1586, "Måste ha minst en stor bokstav"),
			};
        }
    }
}