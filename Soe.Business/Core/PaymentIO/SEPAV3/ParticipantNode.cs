using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    internal class ParticipantNode : SEPANode<SEPABase>
    {
        public readonly PostalAddress PostalAddress;
        private readonly string Name, CountryCode, OrgNr;
        private readonly PaymentMethod paymentMethod;
        private readonly List<PaymentInformationRow> paymentInformationRows;

        internal ParticipantNode(SEPABase parent, string name, string orgNr, SysCountry country, List<ContactAddressRow> addressParts, PaymentMethod paymentMethod, List<PaymentInformationRow> paymentInformationRows) : base(parent)
        {
            Name = name.Trim();
            OrgNr = orgNr;
            CountryCode = country?.Code;
            PostalAddress = new PostalAddress(this, country, addressParts);

            this.paymentMethod = paymentMethod;
            this.paymentInformationRows = paymentInformationRows;
        }

        internal Element CreateNodeInitgPty(string customerNr, string idSchemeName)
        {
            Element node = new Element("InitgPty", new Element("Nm", this.Name.Truncate(35).XmlEncode()));
            
            if (string.IsNullOrEmpty(customerNr))
            {
                customerNr = this.paymentMethod.CustomerNr;
            }

            if (!string.IsNullOrEmpty(customerNr))
            {
                node.Add(this.CreateCustNrElement(customerNr, idSchemeName)); // SEB uses BANK, Nordea uses CUST
            }
            else if (this.paymentMethod.SysPaymentMethodId == (int)TermGroup_SysPaymentMethod.ISO20022 && !string.IsNullOrEmpty(this.paymentMethod.PaymentInformationRow.PayerBankId))
            {
                node.Add(this.CreateBankIdElement(this.paymentMethod.PaymentInformationRow.PayerBankId));
            }
            return node;
        }

        internal Element CreateNodeDbtr()
        {
            Element node = new Element("Dbtr", new Element("Nm", this.Name.Truncate(35).XmlEncode()));
            node.Add(this.PostalAddress.CreateNode());
            var bankId = !string.IsNullOrEmpty(this.paymentMethod.PaymentInformationRow.PayerBankId) ? this.paymentMethod.PaymentInformationRow.PayerBankId : OrgNr?.Replace("-", "");
            if (!string.IsNullOrEmpty(bankId))
            {
                node.Add(this.CreateBankIdElement(bankId));
            }
            return node;
        }

        internal Element CreateNodeCdtr()
        {
            var name = StringUtility.RemoveNonTextCharacters(this.Name)?.Truncate(35);
            Element node = new Element("Cdtr", new Element("Nm", name.XmlEncode()));
            if (this.PostalAddress.HasAddressLines())
            {
                node.Add(this.PostalAddress.CreateNode());
            }
            else if (!string.IsNullOrEmpty(this.CountryCode))
            {
                node.Add(CreatePstlAdrCountryElement());
            }
            return node;
        }

        private Element CreateBankIdElement(string bankNr)
        {
            return new Element("Id", new Element("OrgId", 
                new Element("Othr", new Element("Id", bankNr),
                new Element("SchmeNm", new Element("Cd", "BANK")))));
        }
        private Element CreateCustNrElement(string custNr, string schemeName)
        {
            if (!string.IsNullOrEmpty(custNr))
            {
                return new Element("Id", new Element("OrgId",
                    new Element("Othr", new Element("Id", custNr),
                    new Element("SchmeNm", new Element("Cd", schemeName ?? "CUST")))));
            }
            return null;
        }

        public Element CreateCreditorAgentElement(PaymentRow paymentRow, string debtorAgentBIC)
        {
            Element finInstnIdElement = new Element("FinInstnId");

            if (paymentRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BIC)
            {
                PaymentInformationRow paymentInformationRow = this.paymentInformationRows.FirstOrDefault((p) => p.PaymentNr == paymentRow.PaymentNr);

                if (paymentInformationRow == null)
                {
                    throw new ActionFailedException(this.Parent.paymentManager.GetText(7600, (int)TermGroup.General, "Betalkonto saknas") + ": " +  paymentRow.PaymentNr);
                }

                //international payment
                var BIC = string.IsNullOrEmpty(paymentInformationRow.BIC) ? GetBICValue(paymentRow) : paymentInformationRow.BIC;
                    
                finInstnIdElement.Add(new Element("BIC", BIC));
                if (!paymentInformationRow.ClearingCode.IsNullOrEmpty())
                {
                    finInstnIdElement.Add(CreateClrSysMmbIdElement(paymentInformationRow.ClearingCode, paymentInformationRow.CurrencyAccount));
                }
                else
                {
                    //SEPA payment (payment number saved as foreign bank account)
                    finInstnIdElement.Add(CreatePstlAdrCountryElement());
                }
            }
            else 
            {
                //domestic or SEPA payment
                var BIC = GetBICValue(paymentRow, debtorAgentBIC);
                var mbId = GetMmbIdValue(paymentRow);

                if (!string.IsNullOrEmpty(BIC))
                {
                    finInstnIdElement.Add(new Element("BIC", BIC));
                }
                
                if (!string.IsNullOrEmpty(mbId) && (!IsSwedbank(debtorAgentBIC) || (IsSwedbank(debtorAgentBIC) && string.IsNullOrEmpty(BIC)) ))
                {
                    if (!string.IsNullOrEmpty(BIC) || !IsNordea(debtorAgentBIC))
                    {
                        finInstnIdElement.Add(CreateClrSysMmbIdElement("SESBA", mbId));
                    }
                }
            }

            return finInstnIdElement.Elements().Any() ? new Element("CdtrAgt", finInstnIdElement) : null;
        }

        private Element CreatePstlAdrCountryElement()
        {
            return new Element("PstlAdr", CreateCountryCodeElement(this.CountryCode));
        }

        private string GetBICValue(PaymentRow paymentRow, string debtorAgentBIC = null)
        {
            if (paymentRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG && (debtorAgentBIC == null || !IsSwedbank(debtorAgentBIC)))
            {
                //Payment type PostGiro gets always NDEASESS as BIC
                return "NDEASESS";
            }
            else if (!string.IsNullOrEmpty(paymentRow.PaymentNr))
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

        private string GetMmbIdValue(PaymentRow paymentRow)
        {
            switch ((TermGroup_SysPaymentType)paymentRow.SysPaymentTypeId)
            {
                case TermGroup_SysPaymentType.BG:
                    return "9900"; //Bankgiro
                case TermGroup_SysPaymentType.PG:
                    return "9960"; //Plusgiro
                default:
                    return "";
            }
        }

        public Element CreateClrSysMmbIdElement(string clrSysId, string mmbId)
        {
            return new Element("ClrSysMmbId",
                            new Element("ClrSysId", new Element("Cd", clrSysId)),
                            new Element("MmbId", mmbId));
        }
    }

    internal class PostalAddress : SEPANode<SEPABase>
    {
        private readonly string streetName;
        private readonly string postalCode;
        private readonly string city;
        private readonly string countryCode;

        internal PostalAddress(SEPABase parent, SysCountry country, List<ContactAddressRow> addressParts) : base(parent)
        {
            this.streetName = addressParts.GetRow(TermGroup_SysContactAddressRowType.Address)?.Text?.Trim();
            if (string.IsNullOrEmpty(this.streetName))
            {
                this.streetName = addressParts.GetRow(TermGroup_SysContactAddressRowType.StreetAddress)?.Text?.Trim();
            }
            this.postalCode = addressParts.GetRow(TermGroup_SysContactAddressRowType.PostalCode)?.Text?.Trim();
            this.city = addressParts.GetRow(TermGroup_SysContactAddressRowType.PostalAddress)?.Text?.Trim();
            //this.countryCode = addressParts.GetRow(TermGroup_SysContactAddressRowType.Country)?.Text;
            //if (string.IsNullOrEmpty(this.countryCode))
            //{
                countryCode = country.Code;
            //}

            this.CheckArguments();
        }

        internal bool HasAddressLines()
        {
            return !string.IsNullOrEmpty(streetName) && !string.IsNullOrEmpty(postalCode);
        }

        internal Element CreateNode()
        {
            var pstlAdr = new Element("PstlAdr");

            if (!string.IsNullOrEmpty(streetName))
            {
                pstlAdr.Add(new Element("StrtNm", streetName));
            }
            if (!string.IsNullOrEmpty(postalCode))
            {
                pstlAdr.Add(new Element("PstCd", postalCode));
            }
            if (!string.IsNullOrEmpty(city))
            {
                pstlAdr.Add(new Element("TwnNm", city));
            }
            if (!string.IsNullOrEmpty(countryCode))
            {
                pstlAdr.Add(new Element("Ctry", countryCode));
            }

            return pstlAdr;
        }

        private void CheckArguments()
        {
            if (string.IsNullOrEmpty(this.countryCode))
            {
                throw new ActionFailedException((int)ActionResultSave.PaymentCountryCodeMissing);
            }
        }
    }
}
