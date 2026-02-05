using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeTerminal : ICreatedModified, IState
    {
        public string TypeName { get; set; }
    }

    public partial class TimeTerminalSetting : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeTerminal

        public static TimeTerminalDTO ToDTO(this TimeTerminal e, bool includeCompanyName = false, bool includeSettings = false, bool checkVersion = false, List<TimeTerminalSettingType> validSettingTypes = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeCompanyName && !e.CompanyReference.IsLoaded)
                    {
                        e.CompanyReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeTerminal.cs e.CompanyReference");
                    }
                    if (includeSettings && !e.TimeTerminalSetting.IsLoaded)
                    {
                        e.TimeTerminalSetting.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeTerminal.cs e.TimeTerminalSetting");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeTerminalDTO dto = new TimeTerminalDTO()
            {
                TimeTerminalId = e.TimeTerminalId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TimeTerminalType)e.Type,
                TypeName = e.TypeName,
                Name = e.Name,
                MacAddress = e.MacAddress,
                MacName = e.MacName,
                MacNumber = e.MacNumber,
                Registered = e.Registered,
                TerminalVersion = e.TerminalVersion,
                TerminalDbSchemaVersion = e.TerminalDbSchemaVersion,
                LastSync = e.LastSync,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                TimeTerminalGuid = e.TimeTerminalGuid,
            };

            if (includeCompanyName && e.Company != null)
                dto.CompanyName = e.Company.Name;
            if (includeSettings)
                dto.TimeTerminalSettings = !e.TimeTerminalSetting.IsNullOrEmpty() ? e.TimeTerminalSetting.Where(s => s.State == (int)SoeEntityState.Active && (validSettingTypes.IsNullOrEmpty() || validSettingTypes.Contains((TimeTerminalSettingType)s.Type))).ToDTOs(false).ToList() : new List<TimeTerminalSettingDTO>();

            if (checkVersion)
            {
                if (dto.Type == TimeTerminalType.TimeSpot || dto.Type == TimeTerminalType.WebTimeStamp)
                {
                    dto.Name = dto.Name + " (Denna tjänst är stängd)";
                }
                else if (dto.Type == TimeTerminalType.XETimeStamp && !string.IsNullOrEmpty(dto.TerminalVersion))
                {
                    dto.Name = dto.Name + " (Denna terminal stängs 28 maj – kontakta SoftOne omgående via support@softone.se)";
                }
            }

            return dto;
        }

        public static List<TimeTerminalDTO> ToDTOs(this IEnumerable<TimeTerminal> l, bool includeCompanyName, bool includeSettings, bool checkVersion)
        {
            var dtos = new List<TimeTerminalDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeCompanyName, includeSettings, checkVersion));
                }
            }
            return dtos;
        }

        public static TimeTerminalSettingDTO ToDTO(this TimeTerminalSetting e)
        {
            if (e == null)
                return null;

            return new TimeTerminalSettingDTO()
            {
                TimeTerminalSettingId = e.TimeTerminalSettingId,
                TimeTerminalId = e.TimeTerminal?.TimeTerminalId ?? 0,
                ParentId = e.ParentId,
                Type = (TimeTerminalSettingType)e.Type,
                DataType = (TimeTerminalSettingDataType)e.DataType,
                Name = e.Name,
                StrData = e.StrData,
                IntData = e.IntData,
                DecimalData = e.DecimalData,
                BoolData = e.BoolData,
                DateData = e.DateData,
                TimeData = e.TimeData,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static List<TimeTerminalSettingDTO> ToDTOs(this IEnumerable<TimeTerminalSetting> l, bool loopingChildren)
        {
            List<TimeTerminalSettingDTO> dtos = new List<TimeTerminalSettingDTO>();
            if (l != null)
            {
                foreach (var e in l.Where(x => !x.ParentId.HasValue || loopingChildren).ToList())
                {
                    TimeTerminalSettingDTO dto = e.ToDTO();
                    List<TimeTerminalSetting> children = l.Where(s => s.Type == e.Type && s.ParentId.HasValue && s.ParentId.Value == e.TimeTerminalSettingId).ToList();
                    if (children.Any())
                        dto.Children = children.ToDTOs(true);
                    dtos.Add(dto);
                }
            }
            return dtos;
        }

        #endregion
    }
}
