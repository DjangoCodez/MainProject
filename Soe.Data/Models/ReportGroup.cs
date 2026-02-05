using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ReportGroup : ICreatedModified, IState
    {
        public string TemplateType { get; set; }
        public string NameAndDescription
        {
            get
            {
                return !string.IsNullOrEmpty(this.Description) ? (this.Name + " (" + this.Description + ")") : this.Name;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region ReportGroup

        public static ReportGroupDTO ToDTO(this ReportGroup e)
        {
            if (e == null)
                return null;

            ReportGroupDTO dto = new ReportGroupDTO()
            {
                ReportGroupId = e.ReportGroupId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Description = e.Description,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                InvertRow = e.InvertRow,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
            if (e.ReportGroupHeaderMapping != null)
                dto.ReportGroupHeaderMappings = e.ReportGroupHeaderMapping.ToDTOs(false).ToList();

            if (e.ReportGroupHeaderMapping != null)
                dto.ReportHeaders = e.ReportGroupHeaderMapping
                    .Select(m => m.ReportHeader)
                    .ToList()
                    .ToDTOs()
                    .ToList();

            return dto;
        }

        public static ReportGroupDTO ToDTO(this SysReportGroup e)
        {
            if (e == null)
                return null;

            ReportGroupDTO dto = new ReportGroupDTO()
            {
                ReportGroupId = e.SysReportGroupId,
                Name = e.Name,
                Description = e.Description,
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                InvertRow = e.InvertRow,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
            if (e.SysReportGroupHeaderMapping != null)
                dto.ReportGroupHeaderMappings = e.SysReportGroupHeaderMapping.ToDTOs(false).ToList();

            if (e.SysReportGroupHeaderMapping != null)
                dto.ReportHeaders = e.SysReportGroupHeaderMapping
                    .Select(m => m.SysReportHeader)
                    .ToList()
                    .ToDTOs()
                    .ToList();

            return dto;
        }

        public static SysReportGroup FromDTOToSys(this ReportGroupDTO e)
        {
            if (e == null)
                return null;

            SysReportGroup reportGroup = new SysReportGroup()
            {
                SysReportGroupId = e.ReportGroupId,
                Name = e.Name,
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                Description = e.Description,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                InvertRow = e.InvertRow,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (int)e.State,
            };


            return reportGroup;
        }

        public static ReportGroup FromDTO(this ReportGroupDTO e)
        {
            if (e == null)
                return null;

            ReportGroup reportGroup = new ReportGroup()
            {
                ReportGroupId = e.ReportGroupId,
                Company = e.ActorCompanyId > 0 ? new Company() { ActorCompanyId = e.ActorCompanyId } : null,
                Name = e.Name,
                Description = e.Description,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                InvertRow = e.InvertRow,
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                Module = (int)e.Module,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (int)e.State,
            };


            return reportGroup;
        }

        public static List<ReportGroupDTO> ToDTOs(this List<SysReportGroup> l)
        {
            var dtos = new List<ReportGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<ReportGroupDTO> ToDTOs(this List<ReportGroup> l)
        {
            var dtos = new List<ReportGroupDTO>();
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

        #region ReportHeader
        public static ReportHeaderDTO ToDTO(this SysReportHeader e)
        {
            if (e == null)
                return null;

            ReportHeaderDTO dto = new ReportHeaderDTO()
            {
                ReportHeaderId = e.SysReportHeaderId,
                //ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                //Module = (SoeModule)e.,
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                Name = e.Name,
                Description = e.Description,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                ShowRow = e.ShowRow,
                ShowZeroRow = e.ShowZeroRow,
                InvertRow = e.InvertRow,
                DoNotSummarizeOnGroup = e.DoNotSummarizeOnGroup,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.SysReportGroupHeaderMapping != null)
            {
                dto.ReportGroupHeaderMappings = e.SysReportGroupHeaderMapping.ToDTOs(false);
            }

            if (e.SysReportHeaderInterval != null)
            {
                dto.ReportHeaderIntervals = e.SysReportHeaderInterval.ToDTOs();
            }

            // Extensions
            //dto.TemplateType = e.TemplateType;

            return dto;
        }

        public static ReportHeaderDTO ToDTO(this ReportHeader e)
        {
            if (e == null)
                return null;

            ReportHeaderDTO dto = new ReportHeaderDTO()
            {
                ReportHeaderId = e.ReportHeaderId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                Module = (SoeModule)e.Module,
                Name = e.Name,
                Description = e.Description,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                ShowRow = e.ShowRow,
                ShowZeroRow = e.ShowZeroRow,
                InvertRow = e.InvertRow,
                DoNotSummarizeOnGroup = e.DoNotSummarizeOnGroup,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            // Extensions
            dto.TemplateType = e.TemplateType;


            if (e.ReportGroupHeaderMapping != null)
            {
                dto.ReportGroupHeaderMappings = e.ReportGroupHeaderMapping.ToDTOs(false);
            }

            if (e.ReportHeaderInterval != null)
            {
                dto.ReportHeaderIntervals = e.ReportHeaderInterval.ToDTOs();
            }

            return dto;
        }
        public static SysReportHeader FromDTOToSys(this ReportHeaderDTO e)
        {
            if (e == null)
                return null;

            SysReportHeader entity = new SysReportHeader()
            {
                SysReportHeaderId = e.ReportHeaderId,
                Name = e.Name,
                Description = e.Description,
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                ShowRow = e.ShowRow,
                ShowZeroRow = e.ShowZeroRow,
                DoNotSummarizeOnGroup = e.DoNotSummarizeOnGroup,
                InvertRow = e.InvertRow,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (int)e.State
            };

            if (e.ReportGroupHeaderMappings != null)
            {
                //entity.SysReportGroupHeaderMapping = e.ReportGroupHeaderMappings.FromDTOs();
            }

            // Extensions
            //dto.TemplateType = e.TemplateType;

            return entity;
        }

        public static ReportHeader FromDTO(this ReportHeaderDTO e)
        {
            if (e == null)
                return null;

            ReportHeader entity = new ReportHeader()
            {
                ReportHeaderId = e.ReportHeaderId,
                //Company = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                TemplateType = e.TemplateType,
                TemplateTypeId = e.TemplateTypeId,
                Module = (int)e.Module,
                Name = e.Name,
                Description = e.Description,
                ShowLabel = e.ShowLabel,
                ShowSum = e.ShowSum,
                ShowRow = e.ShowRow,
                ShowZeroRow = e.ShowZeroRow,
                InvertRow = e.InvertRow,
                DoNotSummarizeOnGroup = e.DoNotSummarizeOnGroup,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (int)e.State
            };

            // Extensions
            entity.TemplateType = e.TemplateType;

            return entity;
        }
        public static List<ReportHeaderDTO> ToDTOs(this List<SysReportHeader> l)
        {
            var dtos = new List<ReportHeaderDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<ReportHeaderDTO> ToDTOs(this List<ReportHeader> l)
        {
            var dtos = new List<ReportHeaderDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ReportGroupMappingDTO ToDTO(this ReportGroupMapping e)
        {
            if (e == null)
                return null;

            var mapping = new ReportGroupMappingDTO()
            {
                ReportId = e.ReportId,
                ReportGroupId = e.ReportGroupId,
                Order = e.Order
            };

            if (e.ReportGroup != null)
                mapping.ReportGroup = e.ReportGroup.ToDTO();

            return mapping;
        }

        public static ReportGroupMappingDTO ToDTO(this SysReportGroupMapping e)
        {
            if (e == null)
                return null;

            var mapping = new ReportGroupMappingDTO()
            {
                ReportId = e.SysReportTemplateId,
                ReportGroupId = e.SysReportGroupId,
                Order = e.Order
            };

            if (e.SysReportGroup != null)
                mapping.ReportGroup = e.SysReportGroup.ToDTO();

            return mapping;
        }
        public static ReportGroupMapping FromDTO(this ReportGroupMappingDTO e)
        {
           if (e == null)
                return null;

            var mapping = new ReportGroupMapping()
            {
                ReportId = e.ReportId,
                ReportGroupId = e.ReportGroupId,
                Order = e.Order
            };

            return mapping;
        }
        
        public static SysReportGroupMapping FromDTOToSys(this ReportGroupMappingDTO e)
        {
            if (e == null) 
                return null;

            var mapping = new SysReportGroupMapping()
            {
                SysReportTemplateId = e.ReportId,
                SysReportGroupId = e.ReportGroupId,
                Order = e.Order
            };
            return mapping;
        }

        public static List<ReportGroupMappingDTO> ToDTOs(this List<SysReportGroupMapping> l)
        {
            var dtos = new List<ReportGroupMappingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<ReportGroupMappingDTO> ToDTOs(this List<ReportGroupMapping> l)
        {
            var dtos = new List<ReportGroupMappingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
        public static ReportGroupHeaderMappingDTO ToDTO(this ReportGroupHeaderMapping e, bool setReportAndGroup)
        {
            if (e == null)
                return null;

            return new ReportGroupHeaderMappingDTO()
            {
                ReportHeaderId = e.ReportHeaderId,
                ReportGroupId = e.ReportGroupId,
                ReportHeader = setReportAndGroup ? e.ReportHeader.ToDTO() : null,
                ReportGroup = setReportAndGroup ? e.ReportGroup.ToDTO() : null,
                Order = e.Order
            };
        }
        public static ReportGroupHeaderMappingDTO ToDTO(this SysReportGroupHeaderMapping e, bool setReportAndGroup)
        {
            if (e == null)
                return null;

            return new ReportGroupHeaderMappingDTO()
            {
                ReportHeaderId = e.SysReportHeaderId,
                ReportGroupId = e.SysReportGroupId,
                ReportGroup = setReportAndGroup ? e.SysReportGroup.ToDTO() : null,
                ReportHeader = setReportAndGroup ? e.SysReportHeader.ToDTO() : null,
                Order = e.Order
            };
        }

        public static SysReportGroupHeaderMapping FromDTOToSys(this ReportGroupHeaderMappingDTO e)
        {
            if (e == null)
                return null;

            return new SysReportGroupHeaderMapping()
            {
                SysReportHeaderId = e.ReportHeaderId,
                SysReportGroupId = e.ReportGroupId,
                //SysReportGroup = e.ReportGroup.FromDTOToSys(),
                //SysReportHeader = e.ReportHeader.FromDTOToSys(),
                Order = e.Order
            };
        }
        public static ReportGroupHeaderMapping FromDTO(this ReportGroupHeaderMappingDTO e)
        {
            if (e == null)
                return null;

            return new ReportGroupHeaderMapping()
            {
                ReportHeaderId = e.ReportHeaderId,
                ReportGroupId = e.ReportGroupId,
                //ReportHeader = e.ReportHeader.FromDTO(),
                //ReportGroup = e.ReportGroup.FromDTO(),
                Order = e.Order
            };
        }

        public static List<ReportGroupHeaderMappingDTO> ToDTOs(this IEnumerable<SysReportGroupHeaderMapping> l, bool setGroupAndHeader)
        {
            var dtos = new List<ReportGroupHeaderMappingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setGroupAndHeader));
                }
            }
            return dtos;
        }

        public static List<ReportGroupHeaderMappingDTO> ToDTOs(this IEnumerable<ReportGroupHeaderMapping> l, bool setGroupAndHeader)
        {
            var dtos = new List<ReportGroupHeaderMappingDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(setGroupAndHeader));
                }
            }
            return dtos;
        }



        public static ReportHeaderIntervalDTO ToDTO(this SysReportHeaderInterval e) {
            if (e == null)
                return null;

            return new ReportHeaderIntervalDTO()
            {
                ReportHeaderIntervalId = e.SysReportHeaderIntervalId,
                ReportHeaderId = e.SysReportHeaderId,
                IntervalFrom = e.IntervalFrom,
                IntervalTo = e.IntervalTo,
                SelectValue = e.SelectValue,
            };
        }

        public static ReportHeaderIntervalDTO ToDTO(this ReportHeaderInterval e)
        {
            if (e == null)
                return null;

            return new ReportHeaderIntervalDTO()
            {
                ReportHeaderIntervalId = e.ReportHeaderIntervalId,
                ReportHeaderId = e.ReportHeader != null ? e.ReportHeader.ReportHeaderId : 0,
                IntervalFrom = e.IntervalFrom,
                IntervalTo = e.IntervalTo,
                SelectValue = e.SelectValue,
            };
        }

        public static SysReportHeaderInterval FromDTOToSys(this ReportHeaderIntervalDTO e, SysReportHeader header)
        {
            if (e == null)
                return null;

            return new SysReportHeaderInterval()
            {
                SysReportHeaderIntervalId = e.ReportHeaderIntervalId,
                SysReportHeader = header,
                SysReportHeaderId = e.ReportHeaderId,
                IntervalFrom = e.IntervalFrom,
                IntervalTo = e.IntervalTo,
                SelectValue = e.SelectValue,
            };
        }

        public static ReportHeaderInterval FromDTO(this ReportHeaderIntervalDTO e, ReportHeader header)
        {
            if (e == null)
                return null;

            return new ReportHeaderInterval()
            {
                ReportHeaderIntervalId = e.ReportHeaderIntervalId,
                ReportHeader = header,
                IntervalFrom = e.IntervalFrom,
                IntervalTo = e.IntervalTo,
                SelectValue = e.SelectValue,
            };
        }

        public static List<SysReportHeaderInterval> FromDTOsToSys(this IEnumerable<ReportHeaderIntervalDTO> l, SysReportHeader header)
        {
            var dtos = new List<SysReportHeaderInterval>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.FromDTOToSys(header));
                }
            }
            return dtos;
        }


        public static List<ReportHeaderInterval> FromDTOs(this IEnumerable<ReportHeaderIntervalDTO> l, ReportHeader header)
        {
            var dtos = new List<ReportHeaderInterval>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.FromDTO(header));
                }
            }
            return dtos;
        }

        public static List<ReportHeaderIntervalDTO> ToDTOs(this IEnumerable<SysReportHeaderInterval> l)
        {
            var dtos = new List<ReportHeaderIntervalDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }


        public static List<ReportHeaderIntervalDTO> ToDTOs(this IEnumerable<ReportHeaderInterval> l)
        {
            var dtos = new List<ReportHeaderIntervalDTO>();
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
