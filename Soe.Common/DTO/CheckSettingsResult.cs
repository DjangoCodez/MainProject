using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class CheckSettingsResult
    {
        #region Public properties

        public int Sort { get; set; }

        public TermGroup_CheckSettingsArea Area { get; set; }
        public string AreaName { get; set; }
        public string Setting { get; set; }

        public bool IsSelected { get; set; }
        public bool IsRunning { get; set; }

        public TermGroup_CheckSettingsResultType ResultType { get; set; }
        public string Description { get; set; }
        public string Adjustment { get; set; }

        #endregion

        #region Constructors

        public CheckSettingsResult()
        {
        }

        public CheckSettingsResult(TermGroup_CheckSettingsArea area, string areaName)
        {
            Init(area, areaName, 0, String.Empty);
        }

        public CheckSettingsResult(TermGroup_CheckSettingsArea area, string areaName, int sort, string setting)
        {
            Init(area, areaName, sort, setting);
        }

        private void Init(TermGroup_CheckSettingsArea area, string areaName, int sort, string setting)
        {
            Sort = sort;

            Area = area;
            AreaName = areaName;

            Setting = setting;

            IsSelected = true;
            IsRunning = false;

            ResultType = TermGroup_CheckSettingsResultType.NotChecked;
        }

        #endregion
    }
}
