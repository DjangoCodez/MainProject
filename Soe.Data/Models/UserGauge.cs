using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class UserGauge
    {
        public string SysGaugeName { get; set; }
    }

    public partial class UserGaugeHead : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region

        public static UserGaugeHeadDTO ToDTO(this UserGaugeHead e)
        {
            if (e == null)
                return null;

            UserGaugeHeadDTO dto = new UserGaugeHeadDTO()
            {
                UserGaugeHeadId = e.UserGaugeHeadId,
                ActorCompanyId = e.ActorCompanyId,
                Module = (SoeModule)e.Module,
                UserId = e.UserId,
                Name = e.Name,
                Description = e.Description,
                Priority = e.Priority,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                UserGauges = e.UserGauge.ToDTOs(true),
            };

            return dto;
        }
        public static List<UserGaugeHeadDTO> ToDTOs(this IEnumerable<UserGaugeHead> l)
        {
            var dtos = new List<UserGaugeHeadDTO>();
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

        #region UserGauge

        public static UserGaugeDTO ToDTO(this UserGauge e, bool includeSettings)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeSettings && !e.IsAdded() && !e.UserGaugeSetting.IsLoaded)
                {
                    e.UserGaugeSetting.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("UserGauge.cs e.UserGaugeSetting");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            UserGaugeDTO dto = new UserGaugeDTO()
            {
                UserGaugeId = e.UserGaugeId,
                ActorCompanyId = e.ActorCompanyId,
                RoleId = e.RoleId,
                UserId = e.UserId,
                SysGaugeId = e.SysGaugeId,
                Module = (SoeModule)e.Module,
                Sort = e.Sort,
                WindowState = e.WindowState,
                SysGaugeName = e.SysGaugeName
            };

            if (includeSettings)
            {
                dto.UserGaugeSettings = new List<UserGaugeSettingDTO>();
                foreach (var setting in e.UserGaugeSetting)
                {
                    dto.UserGaugeSettings.Add(new UserGaugeSettingDTO()
                    {
                        UserGaugeSettingId = setting.UserGaugeSettingId,
                        UserGaugeId = setting.UserGaugeId,
                        DataType = setting.DataType,
                        Name = setting.Name,
                        StrData = setting.StrData,
                        IntData = setting.IntData,
                        DecimalData = setting.DecimalData,
                        BoolData = setting.BoolData,
                        DateData = setting.DateData,
                        TimeData = setting.TimeData
                    });
                }
            }

            return dto;
        }

        public static List<UserGaugeDTO> ToDTOs(this IEnumerable<UserGauge> l, bool includeSettings)
        {
            var dtos = new List<UserGaugeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeSettings));
                }
            }
            return dtos;
        }

        #endregion

        #region UserGaugeSetting

        public static UserGaugeSettingDTO ToDTO(this UserGaugeSetting e)
        {
            if (e == null)
                return null;

            return new UserGaugeSettingDTO()
            {
                UserGaugeSettingId = e.UserGaugeSettingId,
                UserGaugeId = e.UserGaugeId,
                DataType = e.DataType,
                Name = e.Name,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData
            };
        }

        public static IEnumerable<UserGaugeSettingDTO> ToDTOs(this IEnumerable<UserGaugeSetting> l)
        {
            var dtos = new List<UserGaugeSettingDTO>();
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
