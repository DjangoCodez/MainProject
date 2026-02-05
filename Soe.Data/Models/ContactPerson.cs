using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class ContactPerson : ICreatedModified, IState
    {
        public string Name
        {
            get 
            {
                return $"{this.FirstName} {this.LastName}";
            }
        }
        public string PositionName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region ContactPerson

        public static ContactPersonDTO ToDTO(this ContactPerson e)
        {
            if (e == null)
                return null;

            var dto = new ContactPersonDTO
            {
                ActorContactPersonId = e.ActorContactPersonId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                PositionName = e.PositionName,
                Description = e.Description,
                SocialSec = e.SocialSec,
                Sex = (TermGroup_Sex)e.Sex,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
            };

            ActorConsent actorConsent = e.Actor?.ActorConsent.FirstOrDefault(a => a.ConsentType == (int)ActorConsentType.Unspecified);
            if (actorConsent != null)
            {
                dto.HasConsent = actorConsent.HasConsent;
                dto.ConsentDate = actorConsent.ConsentDate;
                dto.ConsentModified = actorConsent.ConsentModified;
                dto.ConsentModifiedBy = actorConsent.ConsentModifiedBy;
            }

            return dto;
        }

        public static ContactPerson FromDTO(this ContactPersonDTO e)
        {
            return new ContactPerson()
            {
                ActorContactPersonId = e.ActorContactPersonId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                Description = e.Description,
                SocialSec = e.SocialSec,
                Sex = (int)e.Sex,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (int)e.State
            };
        }

        public static ContactPersonGridDTO ToGridDTO(this GetContactPersons_Result e)
        {
            if (e == null)
                return null;

            return new ContactPersonGridDTO
            {
                ActorContactPersonId = e.ActorContactPersonId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FirstAndLastName = e.FirstAndLastName,
                CategoryString = e.CategoryString,
                Position = e.Position,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Email = e.Email,
                PhoneNumber = e.Telephone,
                HasConsent = e.HasConsent.GetValueOrDefault(),
                ConsentDate = e.ConsentDate,
                ConsentModified = e.ConsentModified,
                ConsentModifiedBy = e.ConsentModifiedBy,
                CustomerId = e.CustomerId,
                CustomerNr = e.CustomerNr,
                CustomerName = e.CustomerName,
                SupplierId = e.SupplierId,
                SupplierNr = e.SupplierNr,
                SupplierName = e.SupplierName
            };
        }

        public static IEnumerable<ContactPersonDTO> ToDTOs(this IEnumerable<ContactPerson> l)
        {
            var dtos = new List<ContactPersonDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ContactPersonDTO ToDTO(this GetContactPersons_Result e)
        {
            if (e == null)
                return null;

            return new ContactPersonDTO
            {
                ActorContactPersonId = e.ActorContactPersonId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Email = e.Email,
                PhoneNumber = e.Telephone,
                HasConsent = e.HasConsent.GetValueOrDefault(),
                ConsentDate = e.ConsentDate,
                ConsentModified = e.ConsentModified,
                ConsentModifiedBy = e.ConsentModifiedBy,

            };
        }

        public static IEnumerable<ContactPersonGridDTO> ToDTOs(this IEnumerable<GetContactPersons_Result> l)
        {
            var dtos = new List<ContactPersonGridDTO>();
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
