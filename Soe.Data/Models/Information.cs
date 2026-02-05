using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Information : ICreatedModified, IState
    {
        public string SeverityName { get; set; }
        public bool ValidateSendPush(DateTime sendTime)
        {
            if (this.ShowInMobile && this.Notify && !this.NotificationSent.HasValue)
            {
                if (!this.ValidFrom.HasValue && !this.ValidTo.HasValue)
                    return true;

                DateTime from = this.ValidFrom ?? DateTime.MinValue;
                DateTime to = this.ValidTo ?? DateTime.MaxValue;

                //Handle that job hasnt run before ValidTo has expired. So, send push if job hasnt run if the push hasnt been sent earlier
                if (to < sendTime)
                    return true;

                if (from <= sendTime && to >= sendTime) //NOSONAR
                    return true;
            }
            return false;
        }
    }

    public static partial class EntityExtensions
    {
        #region Information

        public static InformationDTO ToDTO(this Information e, bool includeText, int? recipientUserId = null)
        {
            if (e == null)
                return null;

            return new InformationDTO()
            {
                InformationId = e.InformationId,
                ActorCompanyId = e.ActorCompanyId,
                LicenseId = e.LicenseId,
                SysLanguageId = e.SysLanguageId,
                SourceType = SoeInformationSourceType.Company,
                Type = (SoeInformationType)e.Type,
                Severity = (TermGroup_InformationSeverity)e.Severity,
                Subject = e.Subject,
                ShortText = e.ShortText,
                Folder = e.Folder,
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                StickyType = (TermGroup_InformationStickyType)e.StickyType,
                NeedsConfirmation = e.NeedsConfirmation,
                ShowInWeb = e.ShowInWeb,
                ShowInMobile = e.ShowInMobile,
                ShowInTerminal = e.ShowInTerminal,
                Notify = e.Notify,
                NotificationSent = e.NotificationSent,
                Text = includeText ? e.Text : null,
                MessageGroupIds = e.InformationMessageGroup?.Where(m => m.State == (int)SoeEntityState.Active).Select(m => m.MessageGroupId).ToList() ?? new List<int>(),
                Recipients = e.InformationRecipient?.Where(r => !recipientUserId.HasValue || r.UserId == recipientUserId.Value).ToDTOs() ?? new List<InformationRecipientDTO>(),
                HasText = !String.IsNullOrEmpty((e.Text)),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
            };
        }

        public static List<InformationDTO> ToDTOs(this IEnumerable<Information> l, bool includeText, int? recipientUserId = null)
        {
            List<InformationDTO> dtos = new List<InformationDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeText, recipientUserId));
                }
            }
            return dtos;
        }

        public static InformationGridDTO ToGridDTO(this Information e)
        {
            if (e == null)
                return null;

            return new InformationGridDTO()
            {
                InformationId = e.InformationId,
                Severity = (TermGroup_InformationSeverity)e.Severity,
                Subject = e.Subject,
                ShortText = e.ShortText,
                Folder = e.Folder,
                ValidFrom = e.ValidFrom,
                ValidTo = e.ValidTo,
                NeedsConfirmation = e.NeedsConfirmation,
                ShowInWeb = e.ShowInWeb,
                ShowInMobile = e.ShowInMobile,
                ShowInTerminal = e.ShowInTerminal,
                Notify = e.Notify,
                NotificationSent = e.NotificationSent,
                SeverityName = e.SeverityName,
            };
        }

        public static List<InformationGridDTO> ToGridDTOs(this IEnumerable<Information> l)
        {
            List<InformationGridDTO> dtos = new List<InformationGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static InformationRecipientDTO ToDTO(this InformationRecipient e)
        {
            if (e == null)
                return null;

            return new InformationRecipientDTO()
            {
                InformationRecipientId = e.InformationRecipientId,
                InformationId = e.InformationId,
                SysInformationId = e.SysInformationId,
                UserId = e.UserId,
                ReadDate = e.ReadDate,
                ConfirmedDate = e.ConfirmedDate,
                HideDate = e.HideDate,
                UserName = e.User?.LoginName
            };
        }

        public static List<InformationRecipientDTO> ToDTOs(this IEnumerable<InformationRecipient> l)
        {
            List<InformationRecipientDTO> dtos = new List<InformationRecipientDTO>();
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
