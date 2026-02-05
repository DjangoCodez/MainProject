using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Export
    {

    }

    public partial class ExportDefinition : ICreatedModified, IState
    {

    }

    public partial class ExportDefinitionLevelColumn : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region Export

        public static ExportDTO ToDTO(this Export e)
        {
            if (e == null)
                return null;

            ExportDTO dto = new ExportDTO()
            {
                ExportId = e.ExportId,
                ActorCompanyId = e.ActorCompanyId,
                ExportDefinitionId = e.ExportDefinitionId,
                Module = e.Module,
                Standard = e.Standard,
                Name = e.Name,
                Filename = e.Filename,
                Emailaddress = e.Emailaddress,
                Subject = e.Subject,
                AttachFile = e.AttachFile,
                SendDirect = e.SendDirect,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                Guid = e.Guid,
                SpecialFunctionality = e.SpecialFunctionality,
            };

            return dto;
        }

        public static List<ExportDTO> ToDTOs(this List<Export> l)
        {
            var dtos = new List<ExportDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;
        }

        public static ExportGridDTO ToGridDTO(this Export e)
        {
            if (e == null)
                return null;

            ExportGridDTO dto = new ExportGridDTO()
            {
                ExportId = e.ExportId,
                Name = e.Name,
            };

            return dto;
        }

        public static List<ExportGridDTO> ToGridDTOs(this List<Export> l)
        {
            var dtos = new List<ExportGridDTO>();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region ExportDefinition

        public static ExportDefinitionDTO ToDTO(this ExportDefinition e, bool? active = true)
        {
            if (e == null)
                return null;

            ExportDefinitionDTO dto = new ExportDefinitionDTO()
            {
                ExportDefinitionId = e.ExportDefinitionId,
                ActorCompanyId = e.ActorCompanyId,
                SysExportHeadId = e.SysExportHeadId,
                Name = e.Name,
                Type = e.Type,
                Separator = e.Separator,
                XmlTagHead = e.XmlTagHead,
                SpecialFunctionality = e.SpecialFunctionality,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.ReportUserSelection != null)
                dto.ReportUserSelection = e.ReportUserSelection.ToDTO();

            if (!e.ExportDefinitionLevel.IsNullOrEmpty())
                dto.ExportDefinitionLevels = e.ExportDefinitionLevel.ToList().ToDTOs(active);

            return dto;
        }

        public static List<ExportDefinitionDTO> ToDTOs(this List<ExportDefinition> l, bool? active = true)
        {
            var dtos = new List<ExportDefinitionDTO>();

            if (active == true)
                l = l.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                l = l.Where(i => i.State == (int)SoeEntityState.Inactive).ToList();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }

            return dtos;
        }

        public static ExportDefinitionGridDTO ToGridDTO(this ExportDefinition e)
        {
            if (e == null)
                return null;

            ExportDefinitionGridDTO dto = new ExportDefinitionGridDTO()
            {
                ExportDefinitionId = e.ExportDefinitionId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Type = e.Type,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            return dto;
        }

        public static List<ExportDefinitionGridDTO> ToGridDTOs(this List<ExportDefinition> l, bool? active = true)
        {
            var dtos = new List<ExportDefinitionGridDTO>();

            if (active == true)
                l = l.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                l = l.Where(i => i.State == (int)SoeEntityState.Inactive).ToList();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }

            return dtos;
        }

        #endregion

        #region ExportDefinitionLevel

        public static ExportDefinitionLevelDTO ToDTO(this ExportDefinitionLevel e, bool? active = true)
        {
            if (e == null)
                return null;

            ExportDefinitionLevelDTO dto = new ExportDefinitionLevelDTO()
            {
                ExportDefinitionLevelId = e.ExportDefinitionLevelId,
                ExportDefinitionId = e.ExportDefinitionId,
                Level = e.Level,
                Xml = e.Xml,
                UseColumnHeaders = e.UseColumnHeaders,
            };

            if (e.ExportDefinitionLevelColumn != null)
                dto.ExportDefinitionLevelColumns = e.ExportDefinitionLevelColumn.ToList().ToDTOs(active);

            return dto;
        }

        public static List<ExportDefinitionLevelDTO> ToDTOs(this List<ExportDefinitionLevel> l, bool? active = true)
        {
            var dtos = new List<ExportDefinitionLevelDTO>();

            if (active == true)
                l = l.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                l = l.Where(i => i.State == (int)SoeEntityState.Inactive).ToList();

            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(active));
                }
            }

            return dtos;
        }

        #endregion

        #region ExportDefinitionLevelColumn

        public static ExportDefinitionLevelColumnDTO ToDTO(this ExportDefinitionLevelColumn e)
        {
            if (e == null)
                return null;

            ExportDefinitionLevelColumnDTO dto = new ExportDefinitionLevelColumnDTO()
            {
                ExportDefinitionLevelColumnId = e.ExportDefinitionLevelColumnId,
                ExportDefinitionLevelId = e.ExportDefinitionLevelId,
                Name = e.Name,
                Description = e.Description,
                Key = e.Key,
                DefaultValue = e.DefaultValue,
                Position = e.Position,
                ColumnLength = e.ColumnLength,
                XmlTag = e.XmlTag,
                FillChar = e.FillChar,
                FillBeginning = e.FillBeginning,
                FormatDate = e.FormatDate,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                ConvertValue = e.ConvertValue
            };

            return dto;
        }

        public static List<ExportDefinitionLevelColumnDTO> ToDTOs(this List<ExportDefinitionLevelColumn> l, bool? active = true)
        {
            var dtos = new List<ExportDefinitionLevelColumnDTO>();

            if (active == true)
                l = l.Where(i => i.State == (int)SoeEntityState.Active).ToList();
            else if (active == false)
                l = l.Where(i => i.State == (int)SoeEntityState.Inactive).ToList();

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
