using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ExtraFieldDTO
    {
        public ExtraFieldDTO()
        {
            Translations = new List<CompTermDTO>();
            ExtraFieldRecords = new List<ExtraFieldRecordDTO>();
            ExtraFieldValues = new List<ExtraFieldValueDTO>();
        }
        public int ExtraFieldId { get; set; }
        public int? SysExtraFieldId { get; set; }
        public SoeEntityType Entity { get; set; }
        public string Text { get; set; }
        public TermGroup_ExtraFieldType Type { get; set; }
        public int? ConnectedEntity { get; set; }
        public int? ConnectedRecordId { get; set; }
        public List<CompTermDTO> Translations { get; set; }
        public List<ExtraFieldRecordDTO> ExtraFieldRecords { get; set; }
        public List<string> ExternalCodes { get; set; } = new List<string>();
        public List<ExtraFieldValueDTO> ExtraFieldValues { get; set; }

        public string ExternalCodesString { get; set; }
        public int? CategoryGroupId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class ExtraFieldValueDTO
    {
        public int ExtraFieldValueId { get; set; }
        public int ExtraFieldId { get; set; }

        public TermGroup_ExtraFieldValueType Type { get; set; }
        public string Value { get; set; }
        public int Sort { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class ExtraFieldGridDTO
    {
        public int ExtraFieldId { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
        public int AccountDimId { get; set; }
        public string AccountDimName { get; set; }
        public bool HasRecords { get; set; }
        public int? SysExtraFieldId { get; set; }

        // Extensions
        public List<ExtraFieldValueDTO> ExtraFieldValues { get; set; }
    }

    [TSInclude]
    public class ExtraFieldRecordDTO
    {
        public int ExtraFieldRecordId { get; set; }
        public int ExtraFieldId { get; set; }
        public int DataTypeId { get; set; }
        public string StrData { get; set; }
        public int? IntData { get; set; }
        public bool? BoolData { get; set; }
        public DateTime? DateData { get; set; }
        public decimal? DecimalData { get; set; }
        public string Comment { get; set; }
        public int RecordId { get; set; }

        public string ExtraFieldText { get; set; }
        public int ExtraFieldType { get; set; }

        public List<ExtraFieldValueDTO> ExtraFieldValues { get; set; }

        public string Value
        {
            get
            {
                string value = "";

                switch (this.DataTypeId)
                {
                    case (int)SettingDataType.Boolean:
                        if (this.BoolData.HasValue)
                            value = this.BoolData.Value.ToString();
                        break;
                    case (int)SettingDataType.Decimal:
                        if (this.DecimalData.HasValue)
                            value = this.DecimalData.Value.ToString("0.0000");
                        break;
                    case (int)SettingDataType.Integer:
                        if (this.IntData.HasValue)
                            value = this.IntData.Value.ToString();
                        break;
                    case (int)SettingDataType.String:
                        if (!String.IsNullOrEmpty(this.StrData))
                            value = this.StrData.ToString();
                        break;
                    case (int)SettingDataType.Date:
                        if (this.DateData.HasValue)
                            value = this.DateData.ToShortDateString();
                        break;
                    default:
                        break;
                }

                return value;
            }
        }

    }
}
