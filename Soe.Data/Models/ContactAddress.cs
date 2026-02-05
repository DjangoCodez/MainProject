using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;


namespace SoftOne.Soe.Data
{
    public partial class ContactAddress : ICreatedModified
    {
        public string Address { get; set; }
    }

    public partial class ContactAddressRow : ICreatedModified
    {

    }

    public static partial class EntityExtensions
    {
        #region ContactAddress

        public static ContactAddress GetLastCreatedOrModified(this List<ContactAddress> addresses)
        {
            if (addresses.IsNullOrEmpty())
                return null;

            var lastCreated = addresses.OrderByDescending(o => o.Created).FirstOrDefault();
            var lastModified = addresses.OrderByDescending(o => o.Modified).FirstOrDefault();
            bool useLastModified = lastModified?.Modified != null && lastCreated?.Created != null && lastModified.Modified.Value > lastCreated.Created.Value;
            return useLastModified ? lastModified : lastCreated;
        }

        public static bool AddressExists(this IEnumerable<ContactAddress> contactAddresses, TermGroup_SysContactAddressType contactAddressType)
        {
            return contactAddresses?.Any(ca => ca.SysContactAddressTypeId == (int)contactAddressType) ?? false;
        }

        public static bool AddressExists(this IEnumerable<ContactAddress> contactAddresses, TermGroup_SysContactAddressRowType contactAddressRowType, string addressText)
        {
            return contactAddresses?.Any(a => a.ContactAddressRow.Any(r => r.SysContactAddressRowTypeId == (int)contactAddressRowType && r.Text.ToLower() == addressText.ToLower())) ?? false;
        }

        public static int? ReturnExistingContactAddressId(this IEnumerable<ContactAddress> addresses, TermGroup_SysContactAddressRowType contactAddressRowType, string addressText)
        {
            return addresses?.FirstOrDefault(a => a.ContactAddressRow.Any(r => r.SysContactAddressRowTypeId == (int)contactAddressRowType && r.Text.ToLower() == addressText.ToLower()))?.ContactAddressId;
        }

        public static ContactAddress GetAddress(this IEnumerable<ContactAddress> l, TermGroup_SysContactAddressType addressType)
        {
            return l?.FirstOrDefault(i => i.SysContactAddressTypeId == (int)addressType);
        }

        public static ContactAddressRow GetRow(this ContactAddress e, TermGroup_SysContactAddressRowType rowType)
        {
            return e?.ContactAddressRow.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)rowType);
        }

        public static string GetRowText(this ContactAddress e, TermGroup_SysContactAddressRowType rowType)
        {
            return e?.GetRow(rowType)?.Text;
        }

        #endregion

        #region ContactAddressRow

        public static string GetContactAddressRowText(this IEnumerable<ContactAddressRow> contactAddressRows, TermGroup_SysContactAddressType addressType, TermGroup_SysContactAddressRowType addressRowType, int contactAddressId = 0)
        {
            if (contactAddressRows.IsNullOrEmpty())
                return string.Empty;

            // If specified, use ContactAddressId
            ContactAddressRow contactAddressRow = null;
            if (contactAddressId != 0)
                contactAddressRow = contactAddressRows.FirstOrDefault(i => i.ContactAddress != null && i.ContactAddress.SysContactAddressTypeId == (int)addressType && i.SysContactAddressRowTypeId == (int)addressRowType && i.ContactAddress.ContactAddressId == contactAddressId);
            else
                contactAddressRow = contactAddressRows.FirstOrDefault(i => i.ContactAddress != null && i.ContactAddress.SysContactAddressTypeId == (int)addressType && i.SysContactAddressRowTypeId == (int)addressRowType);

            return contactAddressRow?.Text ?? string.Empty;
        }

        public static ContactAddressRow GetRow(this List<ContactAddressRow> l, TermGroup_SysContactAddressRowType rowType)
        {
            return l?.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)rowType);
        }

        #endregion
    }
}
