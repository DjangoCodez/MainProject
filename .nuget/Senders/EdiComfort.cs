using SoftOne.EdiAdmin.Business.FileDefinitions;
using SoftOne.Soe.EdiAdmin.Business.FileDefinitions;
using SoftOne.EdiAdmin.Business.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SoftOne.EdiAdmin.Business.Senders
{
    public class EdiComfort : EdiSenderXML<SFTIInvoiceType>
    {
        private Message message;

        protected override bool ParseInput()
        {
            // Mandatory values
            this.message = new Message()
            {
                MessageInfo = new MessageMessageInfo()
                {
                    MessageSenderId = input.SellerParty.Party.PartyIdentification.FirstOrDefault().ID.Value, //.PartyTaxScheme.First().CompanyID.Value, //.TaxScheme.ID.Value, 
                    MessageType = "INVOICE", // Only invoices is supported
                },
                Buyer = new MessageBuyer()
                {
                    BuyerId = input.BuyerParty.Party.PartyIdentification.FirstOrDefault().ID.Value,
                    BuyerName = input.BuyerParty.Party.PartyName.FirstOrDefault().Value,
                },
                Seller = new MessageSeller()
                {
                    //SellerId = input.SellerParty.Party.PartyIdentification.FirstOrDefault().ID.Value,
                    SellerName = input.SellerParty.Party.PartyName.FirstOrDefault().Value,
                },
                Head = new MessageHead()
                {
                    HeadInvoiceNumber = input.ID.Value,
                    HeadInvoiceDate = input.IssueDate.Value,
                },
                Row = new MessageRow[0],
            };

            // Optional values
            this.PopulateBuyer();

            this.PopulateSeller();

            this.PopulateHead();

            this.PopulateRows();

            this.outputs.Add(this.message);

            return true;
        }

        private void PopulateBuyer()
        {
            var buyerParty = input.BuyerParty.Party;

            if (buyerParty != null)
            {
                var buyerAddress = buyerParty.Address;
                if (buyerAddress != null)
                {
                    message.Buyer.BuyerAddress = buyerAddress.StreetName == null ? null : buyerAddress.StreetName.Value;
                    message.Buyer.BuyerPostalCode = buyerAddress.PostalZone == null ? null : buyerAddress.PostalZone.Value;
                    message.Buyer.BuyerPostalAddress = buyerAddress.CityName == null ? null : buyerAddress.CityName.Value;
                    message.Buyer.BuyerCountryCode = buyerAddress.Country == null ? null : buyerAddress.Country.IdentificationCode == null ? null : buyerAddress.Country.IdentificationCode.Value.ToString();
                }

                var contact = buyerParty.Contact;
                if (contact != null)
                {
                    message.Buyer.BuyerReference = contact.Name == null ? null : contact.Name.Value;
                }
            }

            var deliveryAddress = input.Delivery.DeliveryAddress;
            if (deliveryAddress != null)
            {
                message.Buyer.BuyerDeliveryName = deliveryAddress.Department == null ? null : deliveryAddress.Department.Value;
                message.Buyer.BuyerDeliveryAddress = deliveryAddress.StreetName == null ? null : deliveryAddress.StreetName.Value;
                message.Buyer.BuyerDeliveryPostalCode = deliveryAddress.PostalZone == null ? null : deliveryAddress.PostalZone.Value;
                message.Buyer.BuyerDeliveryPostalAddress = deliveryAddress.CityName == null ? null : deliveryAddress.CityName.Value;
                message.Buyer.BuyerDeliveryCountryCode = deliveryAddress.Country == null ? null : deliveryAddress.Country.IdentificationCode == null ? null : deliveryAddress.Country.IdentificationCode.Value.ToString();
            }
        }

        private void PopulateSeller()
        {
            if (input.SellerParty.AccountsContact != null)
            { 
                message.Seller.SellerReference = input.SellerParty.AccountsContact.Name == null ? null : input.SellerParty.AccountsContact.Name.Value;
                message.Seller.SellerReferencePhone = input.SellerParty.AccountsContact.Telephone == null ? null : input.SellerParty.AccountsContact.Telephone.Value;
            }
            var sellerAddress = input.SellerParty.Party.Address;
            if (sellerAddress != null)
            {
                message.Seller.SellerAddress = sellerAddress.StreetName == null ? null : sellerAddress.StreetName.Value;
                message.Seller.SellerPostalCode = sellerAddress.PostalZone == null ? null : sellerAddress.PostalZone.Value;
                message.Seller.SellerPostalAddress = sellerAddress.CityName == null ? null : sellerAddress.CityName.Value;
                message.Seller.SellerCountryCode = sellerAddress.Country == null ? null : sellerAddress.Country.IdentificationCode == null ? null : sellerAddress.Country.IdentificationCode.Value.ToString();

                message.Seller.SellerOrganisationNumber = sellerAddress.ID == null ? null : sellerAddress.ID.Value;
            }

            if (message.Seller.SellerOrganisationNumber == null && input.SellerParty.Party.PartyTaxScheme.Count() > 0)
                message.Seller.SellerOrganisationNumber = input.SellerParty.Party.PartyTaxScheme.FirstOrDefault().CompanyID.Value;
        }

        private void PopulateHead()
        {
            var legalTotal = input.LegalTotal;
            message.Head.HeadInvoiceGrossAmount = legalTotal.TaxInclusiveTotalAmount == null ? null : legalTotal.TaxInclusiveTotalAmount.Value.ToString();
            message.Head.HeadInvoiceNetAmount = legalTotal.TaxExclusiveTotalAmount == null ? null : legalTotal.TaxExclusiveTotalAmount.Value.ToString();

            var taxTotal = input.TaxTotal;
            if (taxTotal != null && taxTotal.Any() && taxTotal.First().TaxSubTotal.Any() && taxTotal.First().TaxSubTotal.First().TaxAmount != null)
                message.Head.HeadVatAmount = taxTotal.FirstOrDefault().TaxSubTotal.FirstOrDefault().TaxAmount.Value;

            var delivery = input.Delivery;
            message.Head.HeadDeliveryDate = delivery.ActualDeliveryDateTime == null ? null : (DateTime?)delivery.ActualDeliveryDateTime.Value;

            var paymentTerms = input.PaymentTerms;
            if (paymentTerms != null)
            {
            message.Head.HeadPaymentConditionText = paymentTerms.Note == null ? null : paymentTerms.Note.Value;
            }

            var paymentMeans = input.PaymentMeans.FirstOrDefault();
            if (paymentMeans != null)
            {
                message.Head.HeadInvoiceDueDate = paymentMeans.DuePaymentDate == null ? null : (DateTime?)paymentMeans.DuePaymentDate.Value;
            }
        }

        private void PopulateRows()
        {
            var rows = new List<MessageRow>();
            foreach (var line in input.InvoiceLine)
            {
                if (line == null)
                    continue;

                var r = new MessageRow();
                if (line.InvoicedQuantity != null)
                {
                    r.RowQuantity = line.InvoicedQuantity.Value;
                    r.RowUnitCode = line.InvoicedQuantity.quantityUnitCode;
                }

                r.RowNetAmount = line.LineExtensionAmount == null ? "0" : line.LineExtensionAmount.Value.ToString();
                decimal rowNetAmount;
                if (decimal.TryParse(r.RowNetAmount, out rowNetAmount) && r.RowQuantity > 0)
                    r.RowUnitPrice = (rowNetAmount / r.RowQuantity).ToString();

                r.RowSellerArticleDescription2 = line.Note == null ? null : line.Note.Value;
                if (line.Delivery != null)
                { 
                    r.RowDeliveryDate = line.Delivery.ActualDeliveryDateTime == null ? null : (DateTime?)line.Delivery.ActualDeliveryDateTime.Value;
                }
                r.RowBuyerArticleNumber = line.OrderLineReference.NotNull(s => s.OrderReference).NotNull(s => s.BuyersID).NotNull(s => s.Value);

                var item = line.Item;
                r.RowSellerArticleNumber = item.SellersItemIdentification.ID == null ? null : item.SellersItemIdentification.ID.Value;
                r.RowSellerArticleDescription1 = item.Description == null ? null : item.Description.Value;
                //r.RowUnitPrice = item.BasePrice.PriceAmount == null ? null : item.BasePrice.PriceAmount.Value.ToString();

                var tax = item.TaxCategory.FirstOrDefault();
                if (tax != null)
                {
                    r.RowVatPercentage = tax.Percent.Value;
                }

                rows.Add(r);
            }

            message.Row = rows.ToArray();
        }
    }
}
