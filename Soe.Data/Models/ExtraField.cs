using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ExtraField : ICreatedModified, IState
    {
        public List<CompTerm> Translations { get; set; }
        public int AccountDimId { get; set; }
        public string AccountDimName { get; set; }

        public List<string> ExternalCodes { get; set; } = new List<string>();
    }

    public static partial class EntityExtensions
    {
        #region ExtraField

        public static ExtraFieldDTO ToDTO(this ExtraField e)
        {
            if (e == null)
                return null;

            ExtraFieldDTO dto = new ExtraFieldDTO()
            {
                ExtraFieldId = e.ExtraFieldId,
                SysExtraFieldId = e.SysExtraFieldId,
                Entity = (SoeEntityType)e.Entity,
                Text = e.Text,
                Type = (TermGroup_ExtraFieldType)e.Type,
                ConnectedEntity = e.ConnectedEntity,
                ConnectedRecordId = e.ConnectedRecordId,
                Translations = e.Translations?.ToDTOs() ?? new List<CompTermDTO>(),
                State = (SoeEntityState)e.State,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            if (!e.ExtraFieldValue.IsNullOrEmpty())
            {
                dto.ExtraFieldValues = e.ExtraFieldValue.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            if (!e.ExtraFieldRecord.IsNullOrEmpty())
            {
                dto.ExtraFieldRecords = e.ExtraFieldRecord.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs();
            }

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes.ToList();
                dto.ExternalCodesString = string.Join("#", e.ExternalCodes);
            }

            return dto;
        }

        public static IEnumerable<ExtraFieldDTO> ToDTOs(this IEnumerable<ExtraField> l)
        {
            List<ExtraFieldDTO> dtos = new List<ExtraFieldDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ExtraFieldValueDTO ToDTO(this ExtraFieldValue e)
        {
            if (e == null)
                return null;

            ExtraFieldValueDTO dto = new ExtraFieldValueDTO()
            {
                ExtraFieldValueId = e.ExtraFieldValueId,
                ExtraFieldId = e.ExtraFieldId,
                Type = (TermGroup_ExtraFieldValueType)e.Type,
                Value = e.Value,
                Sort = e.Sort
            };

            return dto;
        }

        public static IEnumerable<ExtraFieldValueDTO> ToDTOs(this IEnumerable<ExtraFieldValue> l)
        {
            List<ExtraFieldValueDTO> dtos = new List<ExtraFieldValueDTO>();
            if (l != null)
            {
                foreach (var e in l.OrderBy(f => f.Sort).ThenBy(f => f.Value).ToList())
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ExtraFieldRecordDTO ToDTO(this ExtraFieldRecord e)
        {
            if (e == null)
                return null;

            ExtraFieldRecordDTO dto = new ExtraFieldRecordDTO()
            {
                ExtraFieldId = e.ExtraFieldId,
                ExtraFieldRecordId = e.ExtraFieldRecordId,
                ExtraFieldText = e.ExtraField?.Text ?? "",
                ExtraFieldType = e.ExtraField?.Type ?? 0,
                StrData = e.StrData,
                BoolData = e.BoolData,
                Comment = e.Comment,
                DataTypeId = e.DataTypeId,
                DateData = e.DateData,
                DecimalData = e.DecimalData,
                IntData = e.IntData,
                RecordId = e.RecordId ?? 0,
            };

            if (e.ExtraField != null && !e.ExtraField.ExtraFieldValue.IsNullOrEmpty())
                dto.ExtraFieldValues = e.ExtraField.ExtraFieldValue.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();

            return dto;
        }

        public static List<ExtraFieldRecordDTO> ToDTOs(this IEnumerable<ExtraFieldRecord> l)
        {
            List<ExtraFieldRecordDTO> dtos = new List<ExtraFieldRecordDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ExtraFieldGridDTO ToGridDTO(this ExtraField e)
        {
            if (e == null)
                return null;

            ExtraFieldGridDTO dto = new ExtraFieldGridDTO()
            {
                ExtraFieldId = e.ExtraFieldId,
                Text = e.Text,
                Type = e.Type,
                AccountDimId = e.AccountDimId,
                AccountDimName = e.AccountDimName,
                HasRecords = !e.ExtraFieldRecord.IsNullOrEmpty(),
            };

            if (!e.ExtraFieldValue.IsNullOrEmpty())
            {
                dto.ExtraFieldValues = e.ExtraFieldValue.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }

            return dto;
        }

        public static IEnumerable<ExtraFieldGridDTO> ToGridDTOs(this IEnumerable<ExtraField> l)
        {
            List<ExtraFieldGridDTO> dtos = new List<ExtraFieldGridDTO>();
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
    }
}
