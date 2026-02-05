using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    public class ApiMessageGridDTO
    {
        public int ApiMessageId { get; set; }
        public int RecordCount { get; set; }
        public string TypeName { get; set; }
        public string SourceTypeName { get; set; }
        [TsIgnore]
        public TermGroup_ApiMessageStatus Status { get; set; }
        public string StatusName { get; set; }
        public string Identifiers { get; set; }
        public string Comment { get; set; }
        public string ValidationMessage { get; set; }
        public bool HasFile { get; set; }
        public bool HasError { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public List<ApiMessageChangeGridDTO> Changes { get; set; }
    }

    public class ApiMessageChangeGridDTO
    {
        public static readonly string NOPERMISSIONINDICATOR = "*";

        public string RecordName { get; set; }
        public string Identifier { get; set; }
        public string TypeName { get; set; }
        public string FieldTypeName { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Error { get; set; }
        public bool HasError { get; set; }
        [TsIgnore]
        public bool HasPermission
        {
            get
            {
                return this.Identifier != NOPERMISSIONINDICATOR;
            }
        }
    }

    public class ApiMessageDTO
    {
        public Guid? CompanyApiKey { get; private set; }
        public Guid? ConnectApiKey { get; private set; }
        public SoeEntityType EntityType { get; private set; }
        public TermGroup_ApiMessageType Type { get; private set; }
        public TermGroup_ApiMessageSourceType SourceType { get; private set; }
        public TermGroup_ApiMessageStatus Status { get; private set; }
        public int RecordCount { get; set; }

        public ApiMessageDTO(SoeEntityType entityType, TermGroup_ApiMessageType type, TermGroup_ApiMessageSourceType sourceType)
        {
            Init(entityType, type, sourceType);
        }

        public ApiMessageDTO(Guid companyApiKey, Guid connectApiKey, SoeEntityType entityType, TermGroup_ApiMessageType type, TermGroup_ApiMessageSourceType sourceType)
        {
            this.CompanyApiKey = companyApiKey;
            this.ConnectApiKey = connectApiKey;
            Init(entityType, type, sourceType);
        }

        private void Init(SoeEntityType entityType, TermGroup_ApiMessageType type, TermGroup_ApiMessageSourceType sourceType)
        {
            this.EntityType = entityType;
            this.Type = type;
            this.SourceType = sourceType;
            this.Status = TermGroup_ApiMessageStatus.Initialized;
        }

        public void UpdateStatus(TermGroup_ApiMessageStatus status)
        {
            this.Status = status;
        }
    }

    public class ApiMessageChangeDTO
    {
        public int FieldType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public string FromValueName { get; set; }
        public string ToValueName { get; set; }

    }
}
