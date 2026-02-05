using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ContactAddressItem
    {
        public ContactAddressItem() { }
        public ContactAddressItem(ContactAddressItemType type)
        {
            this.ContactAddressItemType = type;
            this.SysContactAddressTypeId = (int)type;
            this.SysContactEComTypeId = (int)type;
            if (this.ContactAddressItemType == Util.ContactAddressItemType.AddressBilling ||
                this.ContactAddressItemType == Util.ContactAddressItemType.AddressBoardHQ ||
                this.ContactAddressItemType == Util.ContactAddressItemType.AddressDelivery ||
                this.ContactAddressItemType == Util.ContactAddressItemType.AddressDistribution ||
                this.ContactAddressItemType == Util.ContactAddressItemType.AddressVisiting)
            {
                this.IsAddress = true;
            }
        }

        // Common
        public int ContactId { get; set; }
        public ContactAddressItemType ContactAddressItemType { get; set; }
        public bool IsSecret { get; set; }
        public bool IsAddress { get; set; } // If true, Address else ECom
        public string TypeName { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string DisplayAddress { get; set; }

        // Address fields
        public int ContactAddressId { get; set; }
        public int SysContactAddressTypeId { get; set; }
        public string AddressName { get; set; }
        public string Address { get; set; }
        public bool AddressIsSecret { get; set; }
        public string AddressCO { get; set; }
        public bool AddressCOIsSecret { get; set; }
        public string PostalCode { get; set; }
        public string PostalAddress { get; set; }
        public bool PostalIsSecret { get; set; }
        public string Country { get; set; }
        public bool CountryIsSecret { get; set; }
        public string StreetAddress { get; set; }
        public bool StreetAddressIsSecret { get; set; }
        public string EntranceCode { get; set; }
        public bool EntranceCodeIsSecret { get; set; }

        // ECom fields
        public int ContactEComId { get; set; }
        public int SysContactEComTypeId { get; set; }
        public string EComText { get; set; }
        public string EComDescription { get; set; }
        public bool EComIsSecret { get; set; }

        public string ToAddressString()
        {
            return $"{this.Address} {this.AddressCO} {this.PostalCode} {this.PostalAddress} {this.Country}";
        }
        public string ToEComString()
        {
            return $"{this.EComText}";
        }

        public void SetDisplayAddress()
        {
            switch (this.ContactAddressItemType)
            {
                case ContactAddressItemType.AddressVisiting:
                    this.DisplayAddress = $"{this.StreetAddress}, {this.PostalCode} {this.PostalAddress}";
                    break;
                case ContactAddressItemType.AddressBoardHQ:
                    this.DisplayAddress = this.PostalAddress;
                    break;
                case ContactAddressItemType.AddressDelivery:
                    this.DisplayAddress = $"{this.AddressName}, {this.Address}, {this.PostalCode} {this.PostalAddress}";
                    break;
                case ContactAddressItemType.ClosestRelative:
                    this.DisplayAddress = this.EComText;
                    if (!string.IsNullOrEmpty(this.EComDescription))
                        this.DisplayAddress += " " + this.EComDescription.Replace(";", ", ");
                    break;
                default:
                    if (this.IsAddress)
                        this.DisplayAddress = $"{this.Address}, {this.PostalCode} {this.PostalAddress}";
                    else
                        this.DisplayAddress = $"{this.EComText}";
                    break;
            }
        }
    }
}
