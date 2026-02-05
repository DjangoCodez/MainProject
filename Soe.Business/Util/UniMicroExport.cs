using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Util
{
    public class UniMicroExport : NorwegianExportBase
    {
        #region Fields

        private IList<UniMicroCustomer> customers;
        private IList<UniMicroTransaction> transactions;

        #endregion

        #region Enums

        public enum RecTypes
        {
            Customers = 30,
            Transactions = 60,
        }

        #endregion

        #region Constructor

        public UniMicroExport(Company company) : base(company)
        {
            this.company = company;

            this.customers = new List<UniMicroCustomer>();
            this.transactions = new List<UniMicroTransaction>();
        }

        #endregion Constructor

        #region Methods 

        protected override bool Populate(CustomerInvoice invoice, Customer customer, List<ContactAddressRow> customerBillingAddress, List<ContactAddressRow> customerDeliveryAddress, List<ContactECom> customerContactEcoms, List<PaymentInformationRowDTO> paymentInformations, string kidNr, int? paymentSeqNr = null)
        {
            UniMicroCustomer uniMicroCustomer;
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
                int.TryParse(customer.CustomerNr.RemoveWhiteSpace('-', '.', ','), out customerNr);

                uniMicroCustomer = new UniMicroCustomer()
                {
                    ActorCustomerId = customer.ActorCustomerId,
                    City = customerPostalAddress != null && !String.IsNullOrEmpty(customerPostalAddress.Text) ? customerPostalAddress.Text : String.Empty,
                    Country = customerCountry != null && !string.IsNullOrEmpty(customerCountry.Text) ? customerCountry.Text : string.Empty,
                    Address1 = customerAddressStreetName != null && !string.IsNullOrEmpty(customerAddressStreetName.Text) ? customerAddressStreetName.Text : string.Empty,
                    Name = customer.Name,
                    Phone = customerPhone?.Text ?? string.Empty,
                    Fax = customerFax?.Text ?? string.Empty,
                    CustomerNr = customerNr,
                };

                if (!customer.ActorReference.IsLoaded)
                    customer.ActorReference.Load();

                if (postalCode != 0)
                    uniMicroCustomer.PostalCode = postalCode.ToString();
                if (pgNumberInt > 0)
                    uniMicroCustomer.PostalGiroAccountNumber = pgNumberInt;
                if (bgNumberInt > 0)
                    uniMicroCustomer.BankTransferAccountNumber = bgNumberInt;

                if (!customer.PaymentConditionReference.IsLoaded)
                    customer.PaymentConditionReference.Load();
                if (customer.PaymentCondition != null && customer.PaymentCondition.Days > 0)
                    uniMicroCustomer.CreditDays = customer.PaymentCondition.Days;
                else if (invoice.PaymentConditionId > 0)
                {
                    if (!invoice.PaymentConditionReference.IsLoaded)
                        invoice.PaymentConditionReference.Load();

                    uniMicroCustomer.CreditDays = invoice.PaymentCondition.Days;
                }
                else
                {
                    this.errorMessage = "Invoice credit days could not be found";
                }

                // Add customer
                this.customers.Add(uniMicroCustomer);
            }
            else
            {
                uniMicroCustomer = this.customers.FirstOrDefault(c => c.ActorCustomerId == customer.ActorCustomerId);
            }

            #endregion Customer

            #region Transaction

            #region Voucher
            var accountingRows = new List<AccountingRowDTO>();
            int productClaimId = 0;
            var dim1IdVatRateMapping = new Dictionary<int, decimal>();
            decimal vatAmount = 0;

            string accountPeriod = invoice.VoucherDate.HasValue ? ToUniMicroDate(CalendarUtility.GetLastDateOfMonth(invoice.VoucherDate.Value)) : string.Empty;
            string bundleDate = invoice.InvoiceDate.HasValue ? ToUniMicroDate(CalendarUtility.GetLastDateOfMonth(invoice.InvoiceDate.Value)) : string.Empty;

            foreach (var invoiceRow in invoice.CustomerInvoiceRow)
            {
                foreach (var item in invoiceRow.CustomerInvoiceAccountRow)
                {
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
                var trans = new UniMicroTransaction()
                {
                    Amount = amount,
                    DueDate = ToUniMicroDate(invoice.DueDate),
                    Date = ToUniMicroDate(invoice.InvoiceDate),
                    VoucherDate = ToUniMicroDate(invoice.DueDate),
                    InvoiceNr = invoice.InvoiceNr.RemoveWhiteSpace(),
                    Period = accountPeriod,
                    Kid = kidNr,
                    Text = invoice.InvoiceNr + "    " +  uniMicroCustomer.Name,
                };

                int accountNr;
                int.TryParse(g.Key.Dim1Nr ?? g.Key.Dim2Nr ?? g.Key.Dim3Nr ?? g.Key.Dim4Nr ?? g.Key.Dim5Nr ?? g.Key.Dim6Nr, out accountNr);

                if (productClaimId == g.Key.Dim1Id)
                {
                    accountNr = uniMicroCustomer.CustomerNr;
                }
                else if (g.FirstOrDefault() != null && g.First().IsCentRoundingRow)
                {
                    // Cent rounding has always vat 0%
                    //trans.TaxCode = VoucherTaxCode.P0;
                }
                else
                {
                    //Set taxcode
                    //decimal vatRate;
                    //if (dim1IdVatRateMapping.TryGetValue(g.Key.Dim1Id, out vatRate))
                    //{
                    //    if (!trans.SetTaxCodeFromVatRate(vatRate))
                    //    {
                    //        string cultureCode = Thread.CurrentThread.CurrentCulture.Name;
                    //        var msg = TermCacheManager.Instance.GetText(9063, (int)TermGroup.General, "Faktura {0}: Momssatsen {1}% är inte tillåten för att få importeras till DI-regnskap", cultureCode, null) + Environment.NewLine;
                    //        errorMessage += string.Format(msg, invoice.InvoiceNr, vatRate);
                    //    }
                    //}

                    trans.Amount = trans.Amount + vatAmount;
                }

                trans.AccountNr = accountNr;

                this.transactions.Add(trans);
            }

            #endregion

            #endregion

            return true;
        }

        public bool Validate(out string errMsg)
        {
            errMsg = this.errorMessage;
            return this.errorMessage == string.Empty;
        }

        public byte[] ToCSV()
        {
            StringBuilder exportFile = new StringBuilder();

            foreach (var item in this.customers)
            {
                exportFile.AppendLine(item.ToCSV());
            }

            foreach (var item in this.transactions)
            {
                exportFile.AppendLine(item.ToCSV());
            }

            return Constants.ENCODING_IBM865.GetBytes(exportFile.ToString());
        }

        #region HelperMethods

        private string ToUniMicroDate(DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToString("ddMMyy") : string.Empty;
        }

        #endregion

        #endregion

        #region Inner Classes

        public class UniMicroCustomer : UniMicroBase
        {
            #region Properties

            #region NonExportProperties

            public int ActorCustomerId { get; set; }

            public override RecTypes RecType { get { return RecTypes.Customers; } }

            #endregion

            /// <summary>
            /// 2 Kontonr
            /// </summary>
            public int CustomerNr { get; set; }

            /// <summary>
            /// 3. Navn
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 4. Adresse
            /// </summary>
            public string Address1 { get; set; }

            public string Address2 { get; set; }

            /// <summary>
            /// 6. Postnr
            /// </summary>
            public string PostalCode { get; set; }

            /// <summary>
            /// 7	Poststed
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// 22  Land
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// 14 Telefon
            /// </summary>
            public string Phone { get; set; }

            /// <summary>
            /// 15  Telefax
            /// </summary>
            public string Fax { get; set; }

            public int PostalGiroAccountNumber { get; set; }

            public int BankTransferAccountNumber { get; set; }

            /// <summary>
            /// 10	Kredittdager (Betalningsvillkor)
            /// </summary>
            public int CreditDays { get; set; }

            #endregion Properties

            protected override List<string> GetFields()
            {
                
                return new List<string>()
                {
                    this.RecTypeInt.ToString(), // 1	RECTYPE =30
                    this.CustomerNr.ToString(), // 2	Kontonr.
                    this.Name, // 3	Navn
                    this.Address1.Truncate(25, true), // 4	Adresse
                    this.Address2.Truncate(25, true), // 5	Adresse 2
                    this.PostalCode.Truncate(6, true), // 6	Postnr.
                    this.City, // 7	Poststed
                    string.Empty, // 8	Saldo i fjor
                    string.Empty, //this.PostalGiroAccountNumber.ToString(), // 9	Gironr.
                    this.CreditDays.ToString(), // 10	Kredittdager, must be set
                    string.Empty, // 11	Kjedenr.
                    string.Empty, // 12	Kategori
                    string.Empty, // 13	øk.kategori
                    this.Phone, // 14	Telefon
                    this.Fax, // 15	Telefaks
                    string.Empty, // 16	BLANK
                    string.Empty, // 17	Omsetning i år
                    string.Empty, // 18	Rabatt
                    this.Fax, // 19	Telefaks
                    "3", // 20	MVA nummer, hardcoded to 3 at the moment
                    string.Empty, // 21	Landkode
                    this.Country, // 22	Land
                    string.Empty, // 21	Valutakode
                };
            }
        }

        public class UniMicroTransaction : UniMicroBase
        {
            #region Properties
            public override RecTypes RecType { get { return RecTypes.Transactions; } }
            public string InvoiceNr { get; set; }
            public string Date { get; set; }
            public decimal Amount { get; set; }
            public string DueDate { get; set; }
            public string VoucherDate { get; set; }
            public string Kid { get; set; }
            public string Period { get; set; }
            /// <summary>
            /// Accountnr is the customernr if it is the product claim row.
            /// </summary>
            public int AccountNr { get; set; }
            public string Text { get; set; }
            #endregion Properties

            protected override List<string> GetFields()
            {
                return new List<string>()
                {
                   this.RecTypeInt.ToString(), //1	RECTYPE=60
                   this.InvoiceNr, //2	Bilagsnr.
                   this.Date, //3	Dato
                   this.AccountNr.ToString().Truncate(10, true), //4	Kontonr
                   string.Empty, //5	avdeling (Dim2)
                   string.Empty, //6	Prosjekt  (Dim1)
                   "3", //7	Momskode hardcoded to 3 at the moment
                   Text.Truncate(40, true), //8	Tekst
                   this.Amount.ToString(), //9	Beløp
                   string.Empty, //10	Operatør (signatur)
                   string.Empty, //11	Dim5
                   string.Empty, //12	Dim6
                   string.Empty, //13	Dim7
                   string.Empty, //14	Dim8
                   this.Period, //15	Periode
                   this.InvoiceNr, //16	Fakturanr
                   this.DueDate, //17	FORFALLSDATO
                   string.Empty, //18	Valutatype
                   string.Empty, //19	Valutabeløp
                   string.Empty, //20	Funksjon/Ansvar(dim3)
                   string.Empty, //21	Område (dim4)
                };
            }
        }

        public abstract class UniMicroBase
        {
            public abstract RecTypes RecType { get; }
            public int RecTypeInt { get { return (int)this.RecType; } }
            protected abstract List<string> GetFields();

            protected UniMicroBase()
            {
                // RecType is always the first field
                // fields = new List<string>() { this.RecType.ToString() };
            }

            public string ToCSV()
            {
                var fields = new List<string>();
                foreach (var item in this.GetFields())
                {
                    if (item == null)
                        fields.Add(string.Empty);
                    else if (item.Contains(','))
                        fields.Add(item.Replace(',', '.'));
                    else
                        fields.Add(item);
                }

                return string.Join(",", fields);
            }
        }

        #endregion
    }
}
