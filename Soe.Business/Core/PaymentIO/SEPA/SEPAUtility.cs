using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPA
{
    public class Element : XElement
    {
        internal static readonly XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.02";
        internal static readonly XNamespace ns_xsi = "http://www.w3.org/2001/XMLSchema-instance";

        internal Element(string tagName, params object[] content)
            : base(ns + tagName, content)
        {
        }
    }
    /*
    public class Attribute : XAttribute
    {
        internal Attribute(string attrName, object value)
            : base(Element.ns + attrName, value)
        {
        }
    }
    */
    public class SEPANode<PT> : SEPABase
        where PT : SEPABase
    {
        internal PT Parent { get; private set; }

        protected SEPANode(PT parent)
            : base(parent.entities, parent.companyManager, parent.contactManager, parent.paymentManager, parent.sysCountries, parent.sysCurrencies)
        {
            this.Parent = parent;
        }

        protected object TryCreateBICElement(int actorId, string errorMessage, PaymentInformationRow paymentInformationRow = null)
        {
            try
            {
                return this.CreateBICElement(actorId, paymentInformationRow);
            }
            catch (ActionFailedException ex)
            {
                if (ex.ErrorNumber == (int)ActionResultSave.PaymentInvalidBICIBAN)
                {
                    throw new ActionFailedException(ex.ErrorNumber, errorMessage, ex);
                }
                throw;
            }
        }

        protected object TryCreateBICElementFromPaymentNr(string paymentNr, string errorMessage)
        {
            try
            {
                string[] bicAndIBAN = paymentNr.Split('/');
                return new Element("IBAN", bicAndIBAN[0].Trim());
            }
            catch (ActionFailedException ex)
            {
                if (ex.ErrorNumber == (int)ActionResultSave.PaymentInvalidBICIBAN)
                {
                    throw new ActionFailedException(ex.ErrorNumber, errorMessage, ex);
                }
                throw;
            }
        }

        protected Element TryCreateIBANElement(int actorId, string errorMessage, int sysPaymentTypeId, PaymentInformationRow paymentInformationRow = null)
        {
            try
            {
                return this.CreateIBANElement(actorId, sysPaymentTypeId, paymentInformationRow);
            }
            catch (ActionFailedException ex)
            {
                if (ex.ErrorNumber == (int)ActionResultSave.PaymentInvalidBICIBAN)
                {
                    throw new ActionFailedException(ex.ErrorNumber, errorMessage, ex);
                }
                throw;
            }
        }

        protected Element TryCreateIBANElementFromPaymentNr(string paymentNr, string errorMessage)
        {
            paymentNr = new string(paymentNr.Where(c => !char.IsWhiteSpace(c)).ToArray());

            try
            {
                string[] bicAndIBAN = paymentNr.Split('/');
                if (bicAndIBAN.Count() == 1)
                    return new Element("IBAN", bicAndIBAN[0].Trim());
                else
                    return new Element("IBAN", bicAndIBAN[1].Trim());
            }
            catch (ActionFailedException ex)
            {
                if (ex.ErrorNumber == (int)ActionResultSave.PaymentInvalidBICIBAN)
                {
                    throw new ActionFailedException(ex.ErrorNumber, errorMessage, ex);
                }
                throw;
            }
        }

        private Element CreateBICElement(int actorId, PaymentInformationRow paymentInformationRow = null)
        {
            return new Element("FinInstnId", new Element("BIC", this.GetBIC(actorId, paymentInformationRow)));
        }

        private Element CreateIBANElement(int actorId, int sysPaymentTypeId, PaymentInformationRow paymentInformationRow = null)
        {
            return new Element("IBAN", this.GetIBAN(actorId, sysPaymentTypeId, paymentInformationRow));
        }

        /// <summary>
        /// TOTO: Genereate BIC code from actor's bank
        /// </summary>
        /// <returns></returns>
        protected string GetBIC(int actorId, PaymentInformationRow paymentInformationRow = null)
        {
            return GetBICAndIBAN(actorId, 0, paymentInformationRow)[0].Trim();
        }

        /// <summary>
        /// TOTO: Genereate IBAN code from actor's bank account number
        /// </summary>
        /// <returns></returns>
        protected string GetIBAN(int actorId, int sysPaymentTypeId, PaymentInformationRow paymentInformationRow = null)
        {
            return GetBICAndIBAN(actorId, sysPaymentTypeId, paymentInformationRow)[1].Trim();
        }

        protected string[] GetBICAndIBAN(int actorId, int sysPaymentTypeId = 0, PaymentInformationRow paymentInformationRow = null)
        {
            if (paymentInformationRow != null)
            {
                if (paymentInformationRow.PaymentNr.Contains('/'))
                {
                    return paymentInformationRow.PaymentNr.Split('/');
                }
                else
                {
                    return new string[]{paymentInformationRow.BIC, paymentInformationRow.PaymentNr};
                }
            }

            string[] bicAndIBAN = this.GetBICSlashIBAN(actorId, sysPaymentTypeId).Split('/');
            if (bicAndIBAN.Length != 2)
            {
                //No need for / anymore, BIC is not mandatory anymore 01/2016 Kai
                //throw new ActionFailedException((int)ActionResultSave.PaymentInvalidBICIBAN);
                //throw new ActionFailedException(0, "Malformed BIC / IBAN string. BIC AND IBAN-codes should be separated with '/'");
            }
            return bicAndIBAN;
        }

        protected string GetBICSlashIBAN(int actorId, int sysPaymentTypeId = 0)
        {
            try
            {
                //return this.GetPaymentInformation(actorId, Utilities.GetPaymentType(sysPaymentTypeId));
                return this.GetPaymentInformation(actorId, TermGroup_SysPaymentType.BIC);
            }
            catch (ActionFailedException ex)
            {
                ex.ToString(); //prevent compiler warning
                throw new ActionFailedException((int)ActionResultSave.PaymentInvalidBICIBAN);
            }
        }

        protected string GetAccountNr(int actorId)
        {
            try
            {
                string pgNumber = this.GetPaymentInformation(actorId, TermGroup_SysPaymentType.PG);
                if (string.IsNullOrEmpty(pgNumber))
                {
                    return this.GetPaymentInformation(actorId, TermGroup_SysPaymentType.BG);
                }
                return pgNumber;
            }
            catch (ActionFailedException ex)
            {
                ex.ToString(); //prevent compiler warning
                throw new ActionFailedException((int)ActionResultSave.PaymentInvalidAccountNumber);
            }
        }

        /// <summary>
        /// TODO: lookup or calculate BIC for suplier (what information on supplier is needed?)
        /// </summary>
        /// <param name="actorId"></param>
        /// <param name="paymentType"></param>
        /// <returns></returns>
        protected string GetPaymentInformation(int actorId, TermGroup_SysPaymentType paymentType)
        {
            PaymentInformation paymentInformation = paymentManager.GetPaymentInformationFromActor(this.entities, actorId, true, false);
            PaymentInformationRow paymentInformationRow = paymentInformation.ActivePaymentInformationRows.FirstOrDefault(i => i.SysPaymentTypeId == (int)paymentType && i.Default);
            if (paymentInformationRow == null)
            {
                throw new ActionFailedException(0, "Payment information of type " + paymentType + " missing for supplier with actorId: " + actorId);
            }
            return paymentInformationRow.PaymentNr.Contains('/') ? paymentInformationRow.PaymentNr : paymentInformationRow.BIC +"/" + paymentInformationRow.PaymentNr;
        }

        protected Element CreatePaymentTypeElement()
        {
            return new Element("PmtTpInf", 
                new Element("InstrPrty", "NORM"),
                new Element("SvcLvl", new Element("Cd", "SEPA"))
                );
        }
    }

    public class SEPABase
    {
        internal const string ISODateFormat = "yyyy'-'MM'-'dd";
        internal const string ISOTimeFormat = "hh':'mm";

        internal CompEntities entities { get; private set; }
        internal CompanyManager companyManager { get; private set; }
        internal ContactManager contactManager { get; private set; }
        internal PaymentManager paymentManager { get; private set; }
        internal List<SysCountry> sysCountries { get; private set; }
        internal List<SysCurrency> sysCurrencies { get; private set; }

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

        protected SEPABase(CompEntities entities, CompanyManager companyManager, ContactManager contactManager, PaymentManager paymentManager, List<SysCountry> sysCountries, List<SysCurrency> sysCurrencies)
        {
            this.entities = entities;
            this.companyManager = companyManager;
            this.contactManager = contactManager;
            this.paymentManager = paymentManager;
            this.sysCountries = sysCountries;
            this.sysCurrencies = sysCurrencies;
        }

        protected List<ContactAddressRow> GetAddressParts(int actorCompanyId, TermGroup_SysContactAddressType addressType)
        {
            Contact contactPreferences = this.contactManager.GetContactFromActor(this.entities, actorCompanyId);
            return this.contactManager.GetContactAddressRows(this.entities, contactPreferences.ContactId, (int)addressType);
        }

        protected string GetCode(Currency currency)
        {
            string code = "";
            if (currency != null)
            {
                SysCurrency sysCurrency = sysCurrencies.FirstOrDefault(i => i.SysCurrencyId == currency.SysCurrencyId);
                if (sysCurrency != null)
                    code = sysCurrency.Code;
            }
            return code;
        }

        internal static string FormatOrgNumber(string orgNumber)
        {
            return orgNumber.Replace("-", "").Replace(".", "").Replace(" ", "");
        }
    }
}
