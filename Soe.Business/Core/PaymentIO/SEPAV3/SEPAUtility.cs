using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    public class Element : XElement
    {
        internal static readonly XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";
        internal static readonly XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        internal Element(string tagName, params object[] content) : base(ns + tagName, content)
        {
        }
    }
   
    public class SEPANode<PT> : SEPABase where PT : SEPABase
    {
        internal PT Parent { get; private set; }

        protected SEPANode(PT parent) : base(parent.entities, parent.contactManager, parent.countryCurrencyManager, parent.paymentManager, parent.sysCountries, parent.sysCurrencies, parent.paymentBank)
        {
            this.Parent = parent;
        }

        protected Element CreateBICElement(PaymentInformationRow paymentInformationRow)
        {
            return new Element("BIC", GetBIC(paymentInformationRow));
        }

        protected string GetBIC(PaymentInformationRow paymentInformationRow)
        {
            string bic = "NDEASESS"; //For now....

            if (paymentInformationRow.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG)
            {
                bic = "NDEASESS"; //PG is nordea....
            }
            else if (paymentInformationRow.PaymentNr.Contains('/'))
            {
                bic = paymentInformationRow.PaymentNr.Split('/')[0].TrimEnd();
            }
            else if (!string.IsNullOrEmpty(paymentInformationRow.BIC))
            {
                bic = paymentInformationRow.BIC;
            }

            return bic;
        }

        protected Element CreateBGAccountElement(string paymentNr)
        {
            paymentNr = paymentNr.RemoveWhiteSpaceAndHyphen();
            return new Element("Id", new Element("Othr", new Element("Id", paymentNr), new Element("SchmeNm", new Element("Prtry", "BGNR"))));
        }

        protected Element CreateIBANorBBANElement(string paymentNr)
        {

            paymentNr = new string(paymentNr.Where(c => !char.IsWhiteSpace(c)).ToArray());

            if (paymentNr.Contains('/'))
                paymentNr = paymentNr.Split('/')[1];
                       
            int n;
            bool isIBAN = !int.TryParse(paymentNr.Substring(0, 2), out n);
            paymentNr = paymentNr.RemoveWhiteSpaceAndHyphen();
            var idElement = new Element("Id");

            if (isIBAN)
            {
                idElement.Add(new Element("IBAN", paymentNr));
            }
            else
            {
                idElement.Add(
                                new Element("Othr",
                                new Element("Id", paymentNr),
                                new Element("SchmeNm", new Element("Cd", "BBAN"))
                                )
                             );
            }

            return idElement;
        }                       
                                   
        protected Element CreatePaymentTypeElement()
        {
            return new Element("PmtTpInf",
                        new Element("InstrPrty", "NORM"),
                        new Element("SvcLvl", new Element("Cd", "NURG") ),
                        new Element("CtgyPurp", new Element("Cd", "SUPP") )
                        );
        }

        protected Element CreateCountryCodeElement(string code)
        {
            return new Element("Ctry", code);
        }
    }

    public class SEPABase
    {
        internal const string ISODateFormat = "yyyy'-'MM'-'dd";
        internal const string ISOTimeFormat = "hh':'mm";

        internal CompEntities entities { get; private set; }
        internal ContactManager contactManager { get; private set; }
        internal CountryCurrencyManager countryCurrencyManager { get; private set; }
        internal PaymentManager paymentManager { get; private set; }
        internal List<SysCountry> sysCountries { get; private set; }
        internal List<SysCurrency> sysCurrencies { get; private set; }
        internal int paymentBank { get; private set; }
        public static string FormatAmount(decimal amount)
        {
            CultureInfo enUsCulture = CultureInfo.GetCultureInfo("en-US");
            return amount.ToString("##.00", enUsCulture);
        }

        public static string FormatCreditAmount(decimal amount)
        {
            //Only positive values goes thrue, type of invoice is in STR segment
            amount = amount * -1;
            CultureInfo enUsCulture = CultureInfo.GetCultureInfo("en-US");
            return amount.ToString("##.00", enUsCulture);
        }

        public static bool IsNordea(string bic)
        {
            return !string.IsNullOrEmpty(bic) && bic.ToUpper() == "NDEASESS";
        }

        public static bool IsSwedbank(string bic)
        {
            return !string.IsNullOrEmpty(bic) && bic.ToUpper() == "SWEDSESS";
        }

        public static bool IsSEB(string bic)
        {
            return !string.IsNullOrEmpty(bic) && bic.ToUpper() == "ESSESESS";
        }

        protected SEPABase(CompEntities entities, ContactManager contactManager, CountryCurrencyManager countryCurrencyManager, PaymentManager paymentManager, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies, int foreignBank)
        {
            this.entities = entities;
           
            this.contactManager = contactManager;
            this.countryCurrencyManager = countryCurrencyManager;
            this.paymentManager = paymentManager;
            this.sysCountries = sysCountries;
            this.sysCurrencies = sysCurrencies;
            this.paymentBank = foreignBank;
        }

        protected List<ContactAddressRow> GetAddressParts(int actorId, TermGroup_SysContactAddressType addressType)
        {
            Contact contactPreferences = this.contactManager.GetContactFromActor(this.entities, actorId);
            return this.contactManager.GetContactAddressRows(this.entities, contactPreferences.ContactId, (int)addressType);
        }

        protected string GetCurrencyCode(int? sysCurrencyId)
        {
            string code = "";
            if (sysCurrencyId.HasValue)
            {
                SysCurrency sysCurrency = sysCurrencies.FirstOrDefault(i => i.SysCurrencyId == sysCurrencyId);
                if (sysCurrency != null)
                {
                    code = sysCurrency.Code;
                }
            }
            return code;
        }

        protected string GetCountryCode(int sysCountryId)
        {
            return sysCountries.Where(i => i.SysCountryId == sysCountryId).Select(i => i.Code).FirstOrDefault();
        }
    }
}
