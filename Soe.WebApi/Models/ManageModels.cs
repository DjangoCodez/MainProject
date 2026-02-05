using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Attributes;

namespace Soe.WebApi.Models
{
    public class SysScheduledJobModel
    {
        [Required]
        public int SysScheduledJobId { get; set; }
        public SysScheduledJobDTO SysScheduledJob { get; set; }
    }

    public class GDPRHandleInfoModel
    {
        public DateTime? Date { get; set; }
        [Required]
        public List<int> Customers { get; set; }
        [Required]
        public List<int> Suppliers { get; set; }
        [Required]
        public List<int> ContactPersons { get; set; }
    }

    public class MultipleChoiceAnswerModel
    {
        public CheckListMultipleChoiceAnswerHeadDTO AnswerHead { get; set; }
        public List<CheckListMultipleChoiceAnswerRowDTO> AnswerRows { get; set; }
    }

    public class SaveDocumentModel
    {
        [Required]
        public DocumentDTO Document { get; set; }
        public byte[] FileData { get; set; }
    }

    public class SetDocumentAsReadModel
    {
        [Required]
        public int DataStorageId { get; set; }
        public bool Confirmed { get; set; }
    }

    public class SetInformationAsReadModel
    {
        [Required]
        public int InformationId { get; set; }
        [Required]
        public int SysInformationId { get; set; }
        public bool Confirmed { get; set; }
        public bool Hidden { get; set; }
    }

    public class SaveUserCompanyRoleDelegateHistoryModel
    {
        public UserCompanyRoleDelegateHistoryUserDTO TargetUser { get; set; }
        public int SourceUserId { get; set; }
    }

    public class SysTermSuggestionModel
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public int PrimaryLanguageId { get; set; }
        [Required]
        public int SecondaryLanguageId { get; set; }
    }

    public class CopyTemplateCompanyModel
    {
        [Required]
        public CopyFromTemplateCompanyInputDTO Dto { get; set; }
        [Required]
        public List<SmallGenericType> Items { get; set; }
    }

    public class SaveDocumentSigningAnswerModel
    {
        public int AttestWorkFlowRowId { get; set; }
        public SigneeStatus SigneeStatus { get; set; }
        public string Comment { get; set; }
    }

    public class CancelDocumentSigningModel
    {
        public int AttestWorkFlowHeadId { get; set; }
        public string Comment { get; set; }
    }

    [TSInclude]
    public class  SoeBankerAuthorizationRequestModel
    {
        [Required]
        public List<int> OnboardingRequestIds { get; set; }
    }

}