using SoftOne.Soe.Business.Core.PaymentIO;
using SoftOne.Soe.Common.Util;
using System;
using System.Text;

namespace SoftOne.Soe.Business.Core.Util.ICA
{
    public class ICACustomerBalanceItem
    {
        #region Members

        public string CustomerNr { get; set; }      // Startpos 0, length 4
        public string Name { get; set; }            // Startpos 4, length 30
        public string AddressCO { get; set; }       // Startpos 34, length 30 - FÖRE?
        public string Address { get; set; }         // Startpos 64, length 30
        public string PostalCode { get; set; }      // Startpos 94, length 6
        public string PostalAddress { get; set; }   // Startpos 101, length 23
        public string PhoneNr { get; set; }         // Startpos 124, length 12
        public string FaxNr { get; set; }           // Startpos 136, length 12
        public int Discount { get; set; }        // Startpos 148, length 3 - EFTER?
        public decimal InLedger { get; set; }       // Startpos 152, length 11 - FÖRE?
        public bool CancellationFlag { get; set; }// Startpos 163, length 1 - FÖRE?

        public bool IsMyStore { get; set; }

        #endregion

        #region Constructors

        public ICACustomerBalanceItem()
        {

        }

        /// <summary>
        /// init for export
        /// </summary>
        /// <param name="transactionCode"></param>
        /// <param name="bgCode"></param>
        /// <param name="invoiceNumber"></param>
        /// <param name="amount"></param>
        /// <param name="date"></param>
        public ICACustomerBalanceItem(string customerNr, string name, string addressCo, string address, string postalcode, string postaladdress, string phoneNr, string faxNr, int discount, decimal amount, bool cancel)
        {
            CustomerNr = customerNr;
            Name = name;
            AddressCO = addressCo;
            Address = address;
            PostalCode = PostalCode;
            PostalAddress = postaladdress;
            PhoneNr = phoneNr;
            FaxNr = faxNr;
            Discount = discount;
            InLedger = amount;
            CancellationFlag = cancel;
        }
        /// <summary>
        /// init for import
        /// </summary>
        /// <param name="item"></param>
        public ICACustomerBalanceItem(string item) 
        {
            /*TransactionCode = Utilities.GetNumeric(item,0,2);
            if(TransactionCode != (int)LbTransactionCodeDomestic.PlusgiroPost)
                CheckNum = Utilities.HasCheckNumber(item.Substring(2, 10)); 
            BgCode = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item), 2, 10);
            InvoiceNumber = item.Substring(12, 25);
            Amount = Utilities.GetNumeric(Utilities.RemoveLeadingZeros(item), 37, 12);
            Information = item.Substring(60, 20);

            switch (TransactionCode) //TODO: this section needs to be verified against specification, example file differs
            { 
                case 14:
                case 15:
                case 54:
                    Date = item.Substring(49, 6); //added to parse example file ^^
                    PaymentTypeCode = item.Substring(49, 1);
                    ReferalBankAccountNumber = Utilities.GetNumeric(item,50,10);
                    break;
                case 16:
                case 17:
                    Date = item.Substring(49, 6); 
                    PaymentTypeCode = item.Substring(55, 1);
                    break;
            }*/
        }

        #endregion

        #region Help methods
        /// <summary>
        /// Serialize object for export
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder(165);

            if (CustomerNr == null)
                CustomerNr = String.Empty;

            //if (CustomerNr.Trim().Length > 10)
            //    sb.Append(CustomerNr.Trim().Substring(0, 10));
            //else
            //    sb.Append(Utilities.AddPadding(CustomerNr.Trim(), 10));

            if (IsMyStore)
            {
                if (CustomerNr.Trim().Length > 10)
                    sb.Append(CustomerNr.Trim().Substring(0, 10));
                else
                    sb.Append(Utilities.AddPadding(CustomerNr.Trim(), 10));
            }
            else
            {
                if (CustomerNr.Trim().Length > 4)
                    sb.Append(CustomerNr.Trim().Substring(0, 4));
                else
                    sb.Append(Utilities.AddPadding(CustomerNr.Trim(), 4));
            }

            if (Name == null)
                Name = String.Empty;

            string name = Name.Trim().Replace("\r", " ");
            if (name.Length > 30)
                sb.Append(name.Substring(0, 30));
            else
                sb.Append(Utilities.AddPadding(name, 30));

            if (AddressCO == null)
                AddressCO = String.Empty;

            if (AddressCO.Length > 30)
                sb.Append(AddressCO.Substring(0, 30));
            else
                sb.Append(Utilities.AddPadding(AddressCO, 30));

            if (Address == null)
                Address = String.Empty;

            if (Address.Length > 30)
                sb.Append(Address.Substring(0, 30));
            else
                sb.Append(Utilities.AddPadding(Address, 30));

            if (PostalCode == null)
                PostalCode = String.Empty;

            if (PostalCode.Length > 6)
                sb.Append(PostalCode.Substring(0, 6));
            else
                sb.Append(Utilities.AddPadding(PostalCode, 6));

            if (PostalAddress == null)
                PostalAddress = String.Empty;
            else
                PostalAddress = " " + PostalAddress;

            if (PostalAddress.Length > 23)
                sb.Append(PostalAddress.Substring(0, 23) + " ");
            else
                sb.Append(Utilities.AddPadding(PostalAddress, 24));

            if (PhoneNr == null)
                PhoneNr = String.Empty;

            string phone = PhoneNr.RemoveWhiteSpace();
            if (phone.Length > 12)
                sb.Append(phone.Substring(0, 12));
            else
                sb.Append(Utilities.AddPadding(phone, 12));

            if (FaxNr == null)
                FaxNr = String.Empty;

            string fax = FaxNr.RemoveWhiteSpace();
            if (fax.Length > 12)
                sb.Append(fax.Substring(0, 12));
            else
                sb.Append(Utilities.AddPadding(fax, 12));

            if (Discount.ToString().Length > 3)
                sb.Append(Discount.ToString().Substring(0, 3));
            else
                sb.Append(Utilities.AddLeadingZeroes(Discount.ToString(), 3));

            string ledger = InLedger.ToString().Replace(",", "").Replace("-", "");
  
            if (ledger.Length > 11)
                sb.Append((InLedger >= 0 ? "+" : "-") + ledger.Substring(0, 11));
            else
                sb.Append((InLedger < 0 ? "-" : "+") + Utilities.AddLeadingZeroes(ledger, 11));


            sb.Append(CancellationFlag ? "1" : "0");

            return sb.ToString();
        }

        #endregion
    }
}
