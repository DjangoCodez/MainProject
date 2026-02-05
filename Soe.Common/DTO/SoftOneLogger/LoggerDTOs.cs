using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.SoftOneLogger
{
    public class PersonalDataLogBatchDTO
    {
        public long PersonalDataLogBatchId { get; set; }

        #region  Keys

        public int ActorCompanyId { get; set; }
        public int? UserId { get; set; }
        public int? RoleId { get; set; }
        public int? SupportUserId { get; set; }
        public int SysCompDBId { get; set; }
        public Guid? LoginGuid { get; set; }
        public Guid Batch { get; set; }

        #endregion

        #region Information 

        public PersonalDataBatchType BatchType { get; set; }
        public int? BatchRecordId { get; set; }
        public DateTime Created { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserName { get; set; }
        public string RequestUrl { get; set; }
        public string IPAddress { get; set; }
        public string MachineName { get; set; }
        public string ObjectName { get; set; }

        #endregion

        #region Relations

        public List<PersonalDataLogDTO> PersonalDataLog { get; set; }

        #endregion
    }

    public class PersonalDataLogDTO
    {
        #region Keys

        public long PersonalDataLogId { get; set; }

        #endregion

        #region Information

        public int RecordId { get; set; }
        public TermGroup_PersonalDataType PersonalDataType { get; set; }
        public TermGroup_PersonalDataActionType ActionType { get; set; }
        public TermGroup_PersonalDataInformationType InformationType { get; set; }

        #endregion

        #region Relations

        public PersonalDataLogBatchDTO PersonalDataLogBatch { get; set; }

        #endregion

        #region Transfer
        public string ObjectName { get; set; }

        #endregion
    }

    public class PersonalDataLogMessageDTO
    {
        public Guid Batch { get; set; }
        public int BatchNbr { get; set; }
        public DateTime TimeStamp { get; set; }
        public string UserName { get; set; }
        public string Url { get; set; }
        public string InformationTypeText { get; set; }
        public string ActionTypeText { get; set; }
        public string Message { get; set; }
    }
}
