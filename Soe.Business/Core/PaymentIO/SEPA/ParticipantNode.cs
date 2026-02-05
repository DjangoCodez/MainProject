using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    internal class ParticipantNode : SEPANode<SEPABase>
    {
        private readonly string name, orgNr, bankId;
        private readonly PostalAddress postalAddress;
        private readonly List<PaymentInformationRow> paymentInformationRows;
        private const string DEFAULT_COMPANY_NAME = "SoftOne Oy";

        internal ParticipantNode(SEPABase parent, string name, SysCountry country, string orgNr, List<ContactAddressRow> addressParts, List<PaymentInformationRow> paymentInformationRows, string bankId = null) : base(parent)
        {
            this.name = name;
            this.orgNr = orgNr;
            this.postalAddress = new PostalAddress(this, country, addressParts);
            this.bankId = bankId;
            this.paymentInformationRows = paymentInformationRows;
        }

        internal Element CreateNode(bool paymentBankConnected)
        {
            Element node = new Element("InitgPty", new Element("Nm", (paymentBankConnected ? DEFAULT_COMPANY_NAME : this.name).Truncate(35).XmlEncode()));
            return node;
        }


        internal Element CreateNodeBank()
        {
            Element node = new Element("Dbtr", new Element("Nm", this.name.Truncate(35).XmlEncode()));
            if (postalAddress.HasAddressLines())
            {
                node.Add(postalAddress.CreateNode());
            }
            node.Add(this.CreateBankIdElement());
            return node;
        }

        private string GetBICValue(PaymentRow paymentRow)
        {
            if (!string.IsNullOrEmpty(paymentRow.PaymentNr))
            {
                string[] BICandIBAN = paymentRow.PaymentNr.Split('/');

                if (BICandIBAN.Length > 1 && BICandIBAN[1].Length > 0)
                    return BICandIBAN[0];
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        internal Element CreateCreditorAgentElement(PaymentRow paymentRow)
        {
            Element finInstnIdElement = new Element("FinInstnId");

            if (paymentRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                PaymentInformationRow paymentInformationRow = this.paymentInformationRows.FirstOrDefault((p) => p.PaymentNr == paymentRow.PaymentNr);
                //international payment
                var BIC = paymentInformationRow != null && !string.IsNullOrEmpty(paymentInformationRow.BIC) ? paymentInformationRow.BIC : GetBICValue(paymentRow);

                if (!string.IsNullOrEmpty(BIC))
                {
                    finInstnIdElement.Add(new Element("BIC", BIC));
                }
            }

            return finInstnIdElement.Elements().Any() ? new Element("CdtrAgt", finInstnIdElement) : null;
        }

        internal Element CreateNodeCdtr()
        {
            Element node = new Element("Cdtr", new Element("Nm", this.name));
            if (postalAddress.HasAddressLines())
            {
                node.Add(postalAddress.CreateNode());
            }
            return node;
        }

        private Element CreateBankIdElement()
        {
            if (bankId != null)
            {
                if (bankId != string.Empty)
                    return new Element("Id", new Element("OrgId", new Element("BkPtyId", bankId)));
                else
                    return new Element("Id", new Element("OrgId", new Element("BkPtyId", "0" + FormatOrgNumber(orgNr))));
            }
            else
                return new Element("Id", new Element("OrgId", new Element("BkPtyId", "0" + FormatOrgNumber(orgNr))));
        }
    }

    internal class PostalAddress : SEPANode<SEPABase>
    {
        private readonly SysCountry country;
        private readonly ContactAddressRow address;
        private readonly ContactAddressRow addressCO;
        private readonly ContactAddressRow addressStreetName;
        private readonly ContactAddressRow postalCode;
        private readonly ContactAddressRow postalAddress;

        internal PostalAddress(SEPABase parent, SysCountry country, List<ContactAddressRow> addressParts)
            : base(parent)
        {
            this.country = country;
            this.addressStreetName = addressParts.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.StreetAddress);
            this.postalCode = addressParts.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalCode);
            this.postalAddress = addressParts.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.PostalAddress);
            this.address = addressParts.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.Address);
            this.addressCO = addressParts.FirstOrDefault(i => i.SysContactAddressRowTypeId == (int)TermGroup_SysContactAddressRowType.AddressCO);
            this.CheckArguments();
        }

        internal bool HasAddressLines()
        {
            return this.GetAddressLines().Any();
        }

        internal Element CreateNode()
        {
            return new Element("PstlAdr", this.CreateAddressElements().AppendElement(this.GetCountryCodeElement()).ToArray());
        }

        private void CheckArguments()
        {
            if (this.country == null)
            {
                throw new ActionFailedException((int)ActionResultSave.PaymentCountryMissing);
            }
            if (string.IsNullOrEmpty(this.country.Code))
            {
                throw new ActionFailedException((int)ActionResultSave.PaymentCountryCodeMissing);
            }
        }

        /// <summary>
        /// Returns up to five address-elements
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Object> CreateAddressElements()
        {
            return this.GetAddressLines().Take(5).Select(x => new Element("AdrLine", x)).OfType<Object>();
        }

        private Element GetCountryCodeElement()
        {
            return new Element("Ctry", country.Code);
        }

        private IEnumerable<string> GetAddressLines()
        {
            if (address != null)
            {
                yield return address.Text;
            }
            if (addressCO != null)
            {
                yield return "C/O " + addressCO.Text;
            }
            if (addressStreetName != null)
            {
                yield return addressStreetName.Text;
            }
            if (postalCode != null && postalAddress != null)
            {
                yield return postalCode.Text + " " + postalAddress.Text;
            }
            else if (postalCode != null)
            {
                yield return postalCode.Text;
            }
            else if (postalAddress != null)
            {
                yield return postalAddress.Text;
            }
        }
    }
}
