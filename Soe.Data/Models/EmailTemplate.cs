using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace SoftOne.Soe.Data
{
    public partial class EmailTemplate
    {

    }

    public static partial class EntityExtensions
    {
        #region EmailTemplate

        public static EmailTemplateDTO ToDTO(this EmailTemplate e)
        {
            if (e == null)
                return null;

            EmailTemplateDTO dto = new EmailTemplateDTO()
            {
                EmailTemplateId = e.EmailTemplateId,
                ActorCompanyId = e.Company != null ? e.Company.ActorCompanyId : 0,  // TODO: Add foreign key to model
                Type = e.Type,
                Name = e.Name,
                Subject = e.Subject,
                Body = e.Body,
                BodyIsHTML = e.BodyIsHTML
            };

            return dto;
        }

        public static IEnumerable<EmailTemplateDTO> ToDTOs(this IEnumerable<EmailTemplate> l)
        {
            var dtos = new List<EmailTemplateDTO>();
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
