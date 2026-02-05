using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class Contact : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region Contact

        public static ContactDTO ToDTO(this Contact e, bool includeContactAddresses, bool includeContactEComs)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeContactAddresses && !e.ContactAddress.IsLoaded)
                    {
                        e.ContactAddress.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Contact.cs e.ContactAddress");
                    }
                    if (includeContactEComs && !e.ContactECom.IsLoaded)
                    {
                        e.ContactECom.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("Contact.cs e.ContactECom");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            ContactDTO dto = new ContactDTO()
            {
                ContactId = e.ContactId,
                ActorId = e.Actor?.ActorId,
                SysContactTypeId = (TermGroup_SysContactType)e.SysContactTypeId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (includeContactAddresses)
                dto.ContactAddresses = e.ContactAddress.ToDTOs(true).ToList();
            if (includeContactEComs)
                dto.ContactEComs = e.ContactECom.ToDTOs().ToList();

            return dto;
        }

        public static IEnumerable<ContactDTO> ToDTOs(this IEnumerable<Contact> l, bool includeContactAddresses, bool includeContactEComs)
        {
            var dtos = new List<ContactDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeContactAddresses, includeContactEComs));
                }
            }
            return dtos;
        }

        public static ContactAddressDTO ToDTO(this ContactAddress e, bool includeRows)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeRows && !e.IsAdded() && e.ContactAddressId > 0 && !e.ContactAddressRow.IsLoaded)
                {
                    e.ContactAddressRow.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("Contact.cs e.ContactAddressRow");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            ContactAddressDTO dto = new ContactAddressDTO()
            {
                ContactAddressId = e.ContactAddressId,
                ContactId = e.Contact?.ContactId ?? 0,
                SysContactAddressTypeId = (TermGroup_SysContactAddressType)e.SysContactAddressTypeId,
                Name = e.Name,
                IsSecret = e.IsSecret,
                Address = e.Address,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };

            if (includeRows)
                dto.ContactAddressRows = (e.ContactAddressRow != null && e.ContactAddressRow.Count > 0) ? e.ContactAddressRow.ToDTOs().ToList() : new List<ContactAddressRowDTO>();

            return dto;
        }

        public static IEnumerable<ContactAddressDTO> ToDTOs(this IEnumerable<ContactAddress> l, bool includeRows)
        {
            var dtos = new List<ContactAddressDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        public static ContactAddressRowDTO ToDTO(this ContactAddressRow e)
        {
            if (e == null)
                return null;

            return new ContactAddressRowDTO()
            {
                RowNr = e.RowNr,
                ContactAddressId = e.ContactAddress?.ContactAddressId ?? 0,
                SysContactAddressRowTypeId = (TermGroup_SysContactAddressRowType)e.SysContactAddressRowTypeId,
                Text = e.Text,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        public static IEnumerable<ContactAddressRowDTO> ToDTOs(this IEnumerable<ContactAddressRow> l)
        {
            var dtos = new List<ContactAddressRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static ContactEComDTO ToDTO(this ContactECom e)
        {
            if (e == null)
                return null;

            return new ContactEComDTO()
            {
                ContactEComId = e.ContactEComId,
                ContactId = e.Contact?.ContactId,
                SysContactEComTypeId = (TermGroup_SysContactEComType)e.SysContactEComTypeId,
                Name = e.Name,
                Text = e.Text,
                Description = e.Description,
                IsSecret = e.IsSecret,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy
            };
        }

        public static IEnumerable<ContactEComDTO> ToDTOs(this IEnumerable<ContactECom> l)
        {
            var dtos = new List<ContactEComDTO>();
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
