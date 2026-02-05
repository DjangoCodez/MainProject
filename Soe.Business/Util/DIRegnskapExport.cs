using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Util
{
    public class DIRegnskapExport : NorwegianExportBase
    {
        #region Enums
        private enum Bundles
        {
            Invoice = 1,
            Payment = 4,
        }

        private enum VoucherKinds
        {
            Other = 1,
            CreditInvoice = 2,
            BankAccountPayment = 4,
            CustomerPayment = 6,
        }

        #endregion

        #region Fields

        private bool isPayment = false;

        #endregion

        #region Nodes

        private List<DIRegnskapCustomer> customers = new List<DIRegnskapCustomer>();
        private List<DIRegnskapTransaction> transactions = new List<DIRegnskapTransaction>();

        #endregion

        #region Constructor

        public DIRegnskapExport(Company company, bool isPayment) : base(company)
        {
            this.company = company;
            this.isPayment = isPayment;
        }

        #endregion Constructor

        #region Public Methods

        public XDocument ToXml()
        {
            #region Prereq

            //LineItemCountNumeric = invoiceLines.Count;

            #endregion

            XElement rootElement = new XElement("DISYSTEMER");
            var doc = new XDocument(new XDeclaration("1.0", "ISO-8859-1", "yes"), rootElement);
            var defaultValue = new XElement("DefaultValue");
            rootElement.Add(defaultValue);
            defaultValue.Add(new XElement("Operation", "ChangeOrNew"));
            defaultValue.Add(new XElement("Control", "Yes"));
            defaultValue.Add(new XElement("System", "DI-Regnskap"));
            defaultValue.Add(new XElement("Register", "Transaction"));

            // Customers
            foreach (var item in this.customers)
            {
                item.AddNode(ref rootElement);
            }

            // Transaction
            foreach (var item in this.transactions)
            {
                item.AddNode(ref rootElement);
            }

            return doc;
        }

        public bool Validate(out string errorMsg)
        {
            errorMsg = string.Empty;
            foreach (var item in this.customers)
            {
                if (!item.Validate(ref errorMsg))
                    return false;
            }

            foreach (var item in this.transactions)
            {
                if (!item.Validate(ref errorMsg))
                    return false;

                foreach (var voucher in item.Vouchers)
                {
                    if (!item.Validate(ref errorMsg))
                        return false;
                }
            }

            errorMsg = this.errorMessage + errorMsg;

            return true;
        }

        #endregion

        #region Private Methods

        protected override bool Populate(CustomerInvoice invoice, Customer customer, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<PaymentInformationRowDTO> paymentInformations, string kidNr, int? paymentSeqNr = null)
        {
            DIRegnskapCustomer diCustomer;

            #region Customer
            
            if (!customers.Any(c => c.ActorCustomerId == customer.ActorCustomerId))
            {
                ContactAddressRow customerPostalCode = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalCode);
                ContactAddressRow customerPostalAddress = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.PostalAddress);
                ContactAddressRow customerAddressStreetName = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.StreetAddress) ?? customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Address);
                ContactAddressRow customerCountry = customerBillingAddress.GetRow(TermGroup_SysContactAddressRowType.Country);
                ContactECom customerPhone = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneJob || i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.PhoneMobile);
                ContactECom customerFax = customerContactEcoms.FirstOrDefault(i => i.SysContactEComTypeId == (int)TermGroup_SysContactEComType.Fax);

                int postalCode = 0;
                if (customerPostalCode != null && !string.IsNullOrEmpty(customerPostalCode.Text))
                    int.TryParse(customerPostalCode.Text.RemoveWhiteSpace('-', '.'), out postalCode);

                var pgNumber = paymentInformations.FirstOrDefault(pi => pi.SysPaymentTypeId == (int)TermGroup_SysPaymentType.PG);
                int pgNumberInt = 0;
                if (pgNumber != null)
                    int.TryParse(pgNumber.PaymentNr.RemoveWhiteSpace('-', '.'), out pgNumberInt);

                var bgNumber = paymentInformations.FirstOrDefault(pi => pi.SysPaymentTypeId == (int)TermGroup_SysPaymentType.BG);
                int bgNumberInt = 0;
                if (bgNumber != null)
                    int.TryParse(bgNumber.PaymentNr.RemoveWhiteSpace('-', '.'), out bgNumberInt);

                string country = string.Empty;
                if (customer.SysCountryId > 0)
                    country = ((TermGroup_Country)customer.SysCountryId).ToString() + (customerCountry != null ? customerCountry.Text ?? string.Empty : string.Empty);

                int customerNr;
                int.TryParse(customer.CustomerNr.RemoveWhiteSpace('-','.',','), out customerNr);
                string customerName = customer.Name.Replace("\x0D0A", " ");
                customerName = customerName.Replace("\x0D", " ");
                customerName = customerName.Replace("\x0A", " ");
                if (customerName.Length > 30)
                    customerName = customerName.Substring(0, 30);
                diCustomer = new DIRegnskapCustomer()
                {
                    ActorCustomerId = customer.ActorCustomerId,
                    City = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : String.Empty,
                    Country = customerCountry != null && !string.IsNullOrEmpty(customerCountry.Text) ? customerCountry.Text : string.Empty,
                    Address1 = customerAddressStreetName != null && !string.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : string.Empty,
                    AccountName = customerName,
                    TaxCode = customer.VatType == (int)TermGroup_InvoiceVatType.NoVat ? 0 : 1,
                    Phone = customerPhone ?.Text ?? string.Empty,
                    Fax = customerFax?.Text ?? string.Empty,
                    AccountNumber = customerNr,
                };

                if (!customer.ActorReference.IsLoaded)
                    customer.ActorReference.Load();

                var contactPerson = customer.Actor.ActiveContactPersons.FirstOrDefault();
                if (contactPerson != null)
                    diCustomer.Ref1 = contactPerson.Name;

                if (postalCode != 0)
                    diCustomer.PostalCode = postalCode;
                if (pgNumberInt > 0)
                    diCustomer.PostalGiroAccountNumber = pgNumberInt;
                if (bgNumberInt > 0)
                    diCustomer.BankTransferAccountNumber = bgNumberInt;
                //if (accountNumber > 0)
                //    diCustomer.AccountNumber = accountNumber;
                if (customer.CreditLimit.HasValue)
                    diCustomer.CreditLimit = customer.CreditLimit.Value;

                // Add customer
                this.customers.Add(diCustomer);
            }
            else
            {
                diCustomer = customers.FirstOrDefault(c => c.ActorCustomerId == customer.ActorCustomerId);
            }

            #endregion

            #region Transaction

            #region Voucher
            var vouchers = new List<DIRegnskapVoucher>();
            var accountingRows = new List<AccountingRowDTO>();
            int productClaimId = 0;
            var dim1IdVatRateMapping = new Dictionary<int, decimal>();
            var paymentAccountingRowDtoPaymentRowMapping = new Dictionary<AccountingRowDTO, PaymentRow>();
            decimal vatAmount = 0;

            // Check if this is a payment
            if (this.isPayment)
            {
                if (!invoice.PaymentRow.IsLoaded)
                    invoice.PaymentRow.Load();

                var paymentRow = invoice.PaymentRow.FirstOrDefault(p => p.SeqNr == paymentSeqNr);

                if (paymentRow == null)
                    return false;

                if (!paymentRow.PaymentAccountRow.IsLoaded)
                    paymentRow.PaymentAccountRow.Load();

                AccountManager acm = new AccountManager(null);
                List<AccountDim> dims = acm.GetAccountDimsByCompany();
                foreach (var item in paymentRow.PaymentAccountRow)
                {
                    var dto = item.ToDTO(dims);
                    accountingRows.Add(dto);

                    paymentAccountingRowDtoPaymentRowMapping.Add(dto, paymentRow);

                    if (invoice.IsCredit && item.DebitRow)
                    {
                        productClaimId = dto.Dim1Id;
                    }
                    else if(!invoice.IsCredit && item.CreditRow)
                    {
                        productClaimId = dto.Dim1Id;
                    }
                }
                
            }
            else
            {
                foreach (var invoiceRow in invoice.CustomerInvoiceRow)
                {
                    foreach (var item in invoiceRow.CustomerInvoiceAccountRow)
                    {
                        if (item.State == 2)
                            continue;
                        var dto = item.AccountingRowDTO(invoiceRow);
                        if (dto.IsVatRow)
                        {
                            vatAmount += dto.AmountCurrency;
                            continue;
                        }

                        accountingRows.Add(dto);

                        // Get the claim row id (used for later)
                        if (invoiceRow.ProductId == null && invoiceRow.Type == (int)SoeInvoiceRowType.AccountingRow && !invoiceRow.IsCentRoundingRow && !dto.IsHouseholdRow && dto.IsClaimRow && !dto.IsVatRow)
                        {
                            if (invoice.IsCredit && item.CreditRow)
                                productClaimId = dto.Dim1Id;
                            else if (!invoice.IsCredit && item.DebitRow)
                                productClaimId = dto.Dim1Id;
                        }

                        // TODO change this when the new VAT system is implemented
                        if (dto.Dim1Id != productClaimId && !dim1IdVatRateMapping.ContainsKey(dto.Dim1Id))
                        {
                            dim1IdVatRateMapping.Add(dto.Dim1Id, invoiceRow.VatRate);
                        }
                    }
                }
            }

            if (productClaimId == 0)
            {
                
            }

            var groupedRows = (from r in accountingRows
                               group r by new
                               {
                                   r.Dim1Id,
                                   r.Dim1Nr,
                                   r.Dim2Id,
                                   r.Dim2Nr,
                                   r.Dim3Id,
                                   r.Dim3Nr,
                                   r.Dim4Id,
                                   r.Dim4Nr,
                                   r.Dim5Id,
                                   r.Dim5Nr,
                                   r.Dim6Id,
                                   r.Dim6Nr,
                                   r.Type,
                                   r.IsCentRoundingRow,
                               } into g
                               select g);

            foreach (var g in groupedRows)
	        {
                decimal amount = g.Sum(r => r.DebitAmount) - g.Sum(r => r.CreditAmount);
                var v = new DIRegnskapVoucher()
                {
                    Amount = Math.Abs(amount),
                    DueDate = invoice.DueDate.HasValue ? ToDiRegnskapDateTime(invoice.DueDate.Value) : string.Empty,
                    VoucherText =  invoice.Origin.Description,
                };

                if (isPayment)
                {
                    // Payment specific fields
                    var pr = paymentAccountingRowDtoPaymentRowMapping[g.First()];
                    v.VoucherNumber = pr.SeqNr;
                    v.VoucherDate = ToDiRegnskapDateTime(pr.PayDate);
                    v.VoucherText = "Innbet. faktura";
                    v.VoucherKind = (int)VoucherKinds.BankAccountPayment;
                }
                else
                {
                    // Invoice specific fields
                    v.VoucherNumber = int.Parse(invoice.InvoiceNr.RemoveWhiteSpace());
                    v.VoucherDate = invoice.VoucherDate.HasValue ? ToDiRegnskapDateTime(invoice.VoucherDate.Value) : string.Empty;
                    v.VoucherKind = invoice.IsCredit ? (int)VoucherKinds.CreditInvoice : (int)VoucherKinds.Other; 
                    v.Kid = kidNr;
                }
                
                int accountNr;
                int.TryParse(g.Key.Dim1Nr ?? g.Key.Dim2Nr ?? g.Key.Dim3Nr ?? g.Key.Dim4Nr ?? g.Key.Dim5Nr ?? g.Key.Dim6Nr, out accountNr);

                if (productClaimId == g.Key.Dim1Id)
                {
                    accountNr = diCustomer.AccountNumber;

                    if (this.isPayment)
                    {
                        v.SupplierInvoiceNumber = int.Parse(invoice.InvoiceNr.RemoveWhiteSpace());
                        v.VoucherKind = (int)VoucherKinds.CustomerPayment;
                    }
                }
                else if (g.FirstOrDefault() != null && g.First().IsCentRoundingRow)
                {
                    // Cent rounding has always vat 0%
                    v.TaxCode = VoucherTaxCode.P0;
                }
                else
                {
                    // Set taxcode
                    decimal vatRate;
                    if (dim1IdVatRateMapping.TryGetValue(g.Key.Dim1Id, out vatRate))
                    {
                        if (!v.SetTaxCodeFromVatRate(vatRate))
                        {
                            string cultureCode = Thread.CurrentThread.CurrentCulture.Name;
                            var msg = TermCacheManager.Instance.GetText(9063, (int)TermGroup.General, "Faktura {0}: Momssatsen {1}% är inte tillåten för att få importeras till DI-regnskap", cultureCode) + Environment.NewLine;
                            errorMessage += string.Format(msg, invoice.InvoiceNr, vatRate);
                        }
                    }

                    v.Amount = v.Amount + Math.Abs(vatAmount);
                }

                if (amount > 0)
                    v.DebitAccountNumber = accountNr;
                else
                    v.CreditAccountNumber = accountNr;

                vouchers.Add(v);
	        }

            #endregion
            
            string accountPeriod  = invoice.VoucherDate.HasValue ? ToDiRegnskapDateTime(CalendarUtility.GetLastDateOfMonth(invoice.VoucherDate.Value)) : string.Empty;
            string bundleDate = invoice.InvoiceDate.HasValue ? ToDiRegnskapDateTime(CalendarUtility.GetLastDateOfMonth(invoice.InvoiceDate.Value)) : string.Empty;
            
            // Merge invoices with the same bundle date to one transaction.
            DIRegnskapTransaction transaction = transactions.FirstOrDefault(t => t.BundleDate == bundleDate);
            if (transaction != null)
            {
                transaction.Vouchers.AddRange(vouchers);
            }
            else
            {
                transaction = new DIRegnskapTransaction()
                {
                    AccountPeriod = accountPeriod,
                    BundleNumber = 0,
                    BundleDate = bundleDate,
                    BundleDescription = isPayment ? "Betalinger" : "Fakturar_Kredit",
                    Bundle = isPayment ? (int)Bundles.Payment : (int)Bundles.Invoice, 
                    Vouchers = vouchers,
                };

                this.transactions.Add(transaction);
            }

            #endregion

            return true;
        }

        #region HelperMethods

        private string ToDiRegnskapDateTime(DateTime dateTime)
        {
            return dateTime.ToString("ddMMyyyy");
        }

        #endregion

        #endregion

        #region Inner Classes

        #region DIRegnskapTransaction
        public class DIRegnskapTransaction : DIRegnskapBase
        {
            #region Fields
            private BaseNode<string> accountPeriod = new BaseNode<string>("AccountingPeriod", 8);
            private BaseNode<int> bundleNumber = new BaseNode<int>("BundleNumber", 5);
            private BaseNode<string> bundleDate = new BaseNode<string>("BundleDate", 8);
            private BaseNode<int> bundle = new BaseNode<int>("Bundle", 2);
            private BaseNode<string> bundleDescription = new BaseNode<string>("BundleDescription", 15);
            #endregion

            #region Properties

            protected override string NodeName
            {
                get { return "Transaction"; }
            }

            public List<DIRegnskapVoucher> Vouchers { get; set; }

            /// <summary>
            /// Regnskapsperiode for bunten (DDMMYY eller DDMMYYYY). Må stemme overens med regnskapsperioder definert i DI-Business.
            /// </summary>
            public string AccountPeriod { get { return accountPeriod.Value; } set { accountPeriod.Value = value; } }

            /// <summary>
            /// Buntnummer. Hvis dette settes til null, finner systemet neste ledige buntnummer og fyller ut dette i bunthodet.
            /// </summary>
            public int BundleNumber { get { return bundleNumber.Value; } set { bundleNumber.Value = value; } }

            /// <summary>
            /// Fylles ut med etableringsdato (ddmmyyyy) for bunten. Eks. 15082001 = den 15 august 2001
            /// </summary>
            public string BundleDate { get { return bundleDate.Value; } set { bundleDate.Value = value; } }

            /// <summary>
            /// Angir buntart i henhold til definisjoner i DI-Regnskap.
            /// </summary>
            public int Bundle{ get { return bundle.Value; } set { bundle.Value = value; } }

            /// <summary>
            /// Fritekst for beskrivelse av bunten. Kommer kun ut på kontrolliste ved oppdatering/utskrift, og ved opphenting av ikke-oppdaterte bunter.
            /// </summary>
            public string BundleDescription { get { return bundleDescription.Value; } set { bundleDescription.Value = value; } }

            #endregion

            public override XElement AddNode(ref XElement rootElement)
            {
                var node = base.AddNode(ref rootElement);

                foreach (var item in this.Vouchers)
                {
                    item.AddNode(ref node);
                }

                return node;
            }

            public override List<BaseNode> GetFields()
            {
                return new List<BaseNode>()
                {
                    accountPeriod,
                    bundleNumber,
                    bundleDate,
                    bundle,
                    bundleDescription,
                };
            }
        }

        #endregion

        #region DIRegnskapCustomer
        public class DIRegnskapCustomer : DIRegnskapBase
        {
            #region Fields
            private BaseNode<int> accountNumber = new BaseNode<int>("AccountNumber", 8);
            private BaseNode<string> country = new BaseNode<string>("Country", 30);
            private BaseNode<string> city = new BaseNode<string>("City", 26);
            private BaseNode<int> postalCode = new BaseNode<int>("PostalCode", 4);
            private BaseNode<string> address1 = new BaseNode<string>("Address1", 30);
            private BaseNode<string> address2 = new BaseNode<string>("Address2", 30);
            private BaseNode<string> accountName = new BaseNode<string>("AccountName", 30);
            private BaseNode<string> phone = new BaseNode<string>("Phone", 20);
            private BaseNode<string> fax = new BaseNode<string>("Fax", 20);
            private BaseNode<int> postalGiroAccountNumber = new BaseNode<int>("PostalGiroAccountNumber", 11);
            private BaseNode<int> bankTransferAccountNumber = new BaseNode<int>("BankTransferAccountNumber", 11);
            private BaseNode<int> creditLimit = new BaseNode<int>("CreditLimit", 11);
            private BaseNode<int> taxCode = new BaseNode<int>("TaxCode", 1);
            private BaseNode<string> ref1 = new BaseNode<string>("Ref1", 30);
            #endregion

            #region Properties

            #region NonXMLProperties

            protected override string NodeName
            {
                get { return "Customer"; }
            }

            public int ActorCustomerId { get; set; }

            #endregion

            #region XMLProperties

            /// <summary>
            /// For reskontro, trenger du ikke å oppgi hovedbokskontonummer, da dette bestemmes av kontoavgrensninger definert i DI-Business
            /// </summary>
            public int AccountNumber { get { return accountNumber.Value; } set { accountNumber.Value = value; } }

            /// <summary>
            /// The name of the customer
            /// </summary>
            public string AccountName { get { return accountName.Value; } set { accountName.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens gate-/besøksadresse
            /// </summary>
            public string Address1 { get { return address1.Value; } set { address1.Value = value; } }
            /// <summary>
            /// Kundens/leverandørens postboksadresse
            /// </summary>
            public string Address2 { get { return address2.Value; } set { address2.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens land. Angis med landkode+mellomrom+landets navn. Eksempel ”SE SVERIGE”.
            /// </summary>
            public string Country { get { return country.Value; } set { country.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens poststed.
            /// </summary>
            public string City { get { return city.Value; } set { city.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens postnummer, 4  tegn
            /// </summary>
            public int PostalCode { get { return postalCode.Value; } set { postalCode.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens telefonnummer.
            /// </summary>
            public string Phone { get { return phone.Value; } set { phone.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens telefaxnummer.
            /// </summary>
            public string Fax { get { return fax.Value; } set { fax.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens postgironummer. Må være et gyldig postgironummer.
            /// </summary>
            public int PostalGiroAccountNumber { get { return postalGiroAccountNumber.Value; } set { postalGiroAccountNumber.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens bankgironummer. Må være et gyldig bankgironummer.
            /// </summary>
            public int BankTransferAccountNumber { get{ return bankTransferAccountNumber.Value;} set{ bankTransferAccountNumber.Value = value;} }

            /// <summary>
            /// Angir kundens kredittgrense.
            /// </summary>
            public int CreditLimit { get { return creditLimit.Value; } set { creditLimit.Value = value; } }

            /// <summary>
            /// 0: Avgiftsfri, 1: Avgiftspliktig
            /// </summary>
            public int TaxCode { get { return taxCode.Value; } set { taxCode.Value = value; } }

            /// <summary>
            /// Kundens/leverandørens referanseperson nr. 1.
            /// </summary>
            public string Ref1 { get { return ref1.Value; } set { ref1.Value = value; } }

            #endregion

            #endregion

            #region Constructor

            public DIRegnskapCustomer()
            {
                
            }

            #endregion

            public override List<BaseNode> GetFields()
            {
                return new List<BaseNode>()
                {
                    accountNumber,
                    accountName,
                    address1,
                    address2,
                    postalCode,
                    city,
                    country,
                    taxCode,
                    creditLimit,
                    phone,
                    fax,
                    postalGiroAccountNumber,
                    bankTransferAccountNumber,
                    ref1,
                };
            }
        }
        #endregion

        #region DIRegnskapVoucher
        public class DIRegnskapVoucher : DIRegnskapBase
        {
            #region Fields
            private BaseNode<int> voucherNumber = new BaseNode<int>("VoucherNumber", 6);
            private BaseNode<int> voucherKind = new BaseNode<int>("VoucherKind", 2);
            private BaseNode<string> voucherText = new BaseNode<string>("VoucherText", 15);
            private BaseNode<string> voucherDate = new BaseNode<string>("VoucherDate", 8);
            private BaseNode<string> dueDate = new BaseNode<string>("DueDate", 8);
            private BaseNode<string> kid = new BaseNode<string>("Kid", 25);
            private BaseNode<int> debitAccountNumber = new BaseNode<int>("DebitAccountNumber", 8);
            private BaseNode<int> creditAccountNumber = new BaseNode<int>("CreditAccountNumber", 8);
            private BaseNode<decimal> amount = new BaseNode<decimal>("Amount", 9, 2);
            private BaseNode<int> taxCode = new BaseNode<int>("TaxCode", 2);
            private BaseNode<int> supplierInvoiceNumber = new BaseNode<int>("SupplierInvoiceNumber", 9);
            #endregion

            #region Properties
            protected override string NodeName
            {
                get { return "Voucher"; }
            }

            /// <summary>
            /// Bilagsnummer. Nummerserien bør samordnes med andre bilagsartserier som er i bruk. Skal uansett ikke være 0.
            /// </summary>
            public int VoucherNumber { get { return voucherNumber.Value; } set { voucherNumber.Value = value; } }

            /// <summary>
            /// Angir bilagsart i henhold til definisjoner i DI-Business.
            /// </summary>
            public int VoucherKind { get { return voucherKind.Value; } set { voucherKind.Value = value; } }

            public string VoucherText { get { return voucherText.Value; } set { voucherText.Value = value; } }

            /// <summary>
            /// Posteringsdato (ddmmyyyy) for bilaget. Eks. 15092001 = 15. september 2001.
            /// </summary>
            public string VoucherDate { get { return voucherDate.Value; } set { voucherDate.Value = value; } }

            public string DueDate { get { return dueDate.Value; } set { dueDate.Value = value; } }

            /// <summary>
            /// Kan fylles ut med bankgiroens KID-felt i forbindelse med remitteringsrutinen i DI-Regnskap. 
            /// I tilfelle det brukes, skal det fylles ut med alle tall, ikke kontrolltegnet, og følge vanlige definerte nummerregler.
            /// </summary>
            public string Kid { get { return kid.Value; } set { kid.Value = value; } }

            /// <summary>
            /// Debet kontonummer for føringen. Eventuell reskontro fylles ut direkte uten hovedbokskonto.
            /// </summary>
            public int DebitAccountNumber { get { return debitAccountNumber.Value; } set { debitAccountNumber.Value = value; } }

            /// <summary>
            /// Kredit kontonummer for føringen. Eventuell reskontro fylles ut direkte uten hovedbokskonto.
            /// </summary>
            public int CreditAccountNumber { get { return creditAccountNumber.Value; } set { creditAccountNumber.Value = value; } }

            /// <summary>
            /// eks. 123456789.12. Brutto posteringsbeløp.
            /// </summary>
            public decimal Amount { get { return amount.Value; } set { amount.Value = Math.Round(value, 2); } }

            /// <summary>
            /// standard Tax code er: 1 for 25%, 9 for 0, 11 for 15%, 21 for 8%
            /// </summary>
            public VoucherTaxCode TaxCode { get { return (VoucherTaxCode)taxCode.Value; } set { taxCode.Value = (int)value; } }

            public int SupplierInvoiceNumber { get { return supplierInvoiceNumber.Value; } set { supplierInvoiceNumber.Value = value; } }

            #endregion

            //[Obsolete("TODO change this when the new VAT system is implemented", false)]
            public bool SetTaxCodeFromVatRate(decimal vatRate)
            {
                VoucherTaxCode code;
                if (NorwegianExportBase.TryGetTaxCodeFromVatRate(vatRate, out code))
                {
                    this.TaxCode = code;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override List<BaseNode> GetFields()
            {
                return new List<BaseNode>()
                {
                    voucherKind,
                    voucherNumber,
                    voucherText,
                    voucherDate,
                    supplierInvoiceNumber,
                    dueDate,
                    kid,
                    debitAccountNumber,
                    creditAccountNumber,
                    amount,
                    taxCode,
                };
            }
        }
        #endregion

        #region DIRegnskapBase
        public abstract class DIRegnskapBase
        {
            protected abstract string NodeName { get; }

            public abstract List<BaseNode> GetFields();

            public virtual bool Validate(ref string errorMsg)
            {
                bool validated = true;
                foreach (var item in this.GetFields())
                {
                    if (item.Value != null && item.Value.ToString().Length > item.Length)
                    {
                        errorMsg += GetErrorMsg(item.Key, item.Value.ToString().Length, item.Length);
                        validated = false;
                    }

                    if (item.Length2 >= 0)
                    {
                        var decimals = item.Value.ToString().Split(',', '.').LastOrDefault();
                        if (decimals.Length > item.Length2)
                        {
                            errorMsg += GetErrorMsg(item.Key, decimals.Length, item.Length2);
                            validated = false;
                        }
                    }
                }

                return validated;
            }

            public virtual XElement AddNode(ref XElement rootElement)
            {
                XElement node = new XElement(this.NodeName);

                foreach (var item in this.GetFields())
                {
                    if(item.Value != null)
                        node.Add(new XElement(item.Key, item.Value));
                }

                rootElement.Add(node);
                return node;
            }

            private string GetErrorMsg(string name, int length, int maximumLength)
            {
                string cultureCode = Thread.CurrentThread.CurrentCulture.Name;
                var msg = TermCacheManager.Instance.GetText(9063, (int)TermGroup.General, "{0} är {1} tecken långt men får maximalt vara {2} tecken långt för att få importeras till DI-regnskap", cultureCode);
                return string.Format(msg, name, length, maximumLength) + Environment.NewLine;
            }
        }
        #endregion

        #region BaseNode
        public class BaseNode
        {
            public string Key { get; set; }
            public virtual object Value { get; set; }
            public int Length { get; set; }
            public int Length2 { get; set; }
            
            public BaseNode(string key, int length, int length2)
            {
                this.Key = key;
                this.Length = length;
                this.Length2 = length2;
            }
        }
        #endregion BaseNode

        #region BaseNode<T>
        public class BaseNode<T> : BaseNode
        {
            public new T Value
            {
                get
                {
                    return (T)base.Value;
                }
                set
                {
                    base.Value = value;
                }
            }

            public BaseNode(string key, int length, int length2 = -1) : base(key, length, length2) 
            { }
        }
        #endregion

        #endregion
    }
}
