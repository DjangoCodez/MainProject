using Newtonsoft.Json;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZXing.QrCode;

namespace SoftOne.Soe.Business.Util.QR
{
    public class QRCode
    {
        public string CreateInvoiceQR(string invoiceReference, string address, DateTime invoiceDate, DateTime dueDate, decimal totalAmountCurreny, decimal vatAmountCurrency, string sellerName, string currency, string orgNr, TermGroup_SysPaymentType paymentType, string paymentNr)
        {
            string value = string.Empty;

            if (paymentType == TermGroup_SysPaymentType.PG)
                paymentNr = "Pg: " + paymentNr;

            if (paymentType == TermGroup_SysPaymentType.Bank)
                paymentNr = "Account: " + paymentNr;

            if (paymentType == TermGroup_SysPaymentType.BG)
                paymentNr = "Bg: " + paymentNr;

            UsingQRDTO qr = new UsingQRDTO()
            {
                uqr = 2,
                tp = 1,
                nme = sellerName,
                cur = currency,
                cid = orgNr,
                idt = invoiceDate.ToShortDateString().Replace("-", ""),
                ddt = dueDate.ToShortDateString().Replace("-", ""),
                due = totalAmountCurreny,
                vat = vatAmountCurrency,
                pt = paymentNr,
                adr = address,
                iref = invoiceReference,
                

            };
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            value = JsonConvert.SerializeObject(qr, settings).ToString();

            return CreateQR(value);

        }
        public string CreateQR(string text)
        {
            byte[] image = null;

            QRCodeWriter qr = new QRCodeWriter();

            var matrix = qr.encode(text, ZXing.BarcodeFormat.QR_CODE, 400, 400);

            ZXing.BarcodeWriter w = new ZXing.BarcodeWriter();

            w.Format = ZXing.BarcodeFormat.QR_CODE;

            Bitmap img = w.Write(matrix);

            image = ToByteArray(img, ImageFormat.Bmp);

            string dirPhysical = ConfigSettings.SOE_SERVER_DIR_TEMP_LOGO_PHYSICAL;
            string pathPhysical = dirPhysical + Guid.NewGuid() + ".png";

            img.Save(pathPhysical, System.Drawing.Imaging.ImageFormat.Png);
            img.Dispose();

            return pathPhysical;
        }
        private byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }

    public class UsingQRDTO
    {
        //UsingQR Version (Number)
        public int uqr { get; set; }

        //tp Type(Number)
        //Format:
        //1
        //Description:
        //1. 1 = Payment information/Invoice.Indicates that the QR code is dedicated for
        //sending payment information.
        //2. 2 = Credit invoice.Cannot be used by banking apps.
        //3. 3 = Cash paid invoice.Cannot be used by banking apps.
        //4. Also indicates which JSON keys are mandatory.See chapter 5.

        public int tp { get; set; }

        //“nme” Name(String)
        //Format:
        //Test company
        //Description:
        //1. Company name of the sending party.

        public string nme { get; set; }

        //“cc” Country Code(String)
        //Format:
        //DK
        //Description:
        //1. This is the country of the sending party.
        //2. Country code must adhere to ISO standard for country codes
        //a.Used if pt is:
        //i.IBAN
        //ii. BBAN, and bc contains a Bank code
        public string cc { get; set; }

        // “cid” Company ID(String)
        //Format:
        //555555-5555
        //998870283
        //88644072
        //131052-308T
        //Description
        //1. Corporate identification number of sending party.
        //2. Allowed characters include hyphens, blank spaces and latin characters.

        public string cid { get; set; }

        //“iref” Invoice reference(String)
        //Format:
        //1000000000132
        //Page 7
        //Description:
        //1. Invoice reference number is typically an OCR/KID number or an invoice number
        //depending on the sending system.
        //2. Invoice reference length can vary according to national standards.
        //3. If the invoice reference is an OCR/KID number, the last number is always a
        //checksum calculated with MOD10 or MOD11.

        public string iref { get; set; }

        //“cr” Credit invoice reference(String)
        //Format:
        //101
        //Description:
        //1. A reference to the invoice being credited.
        //2. Only used when tp = 2.


        public string cr { get; set; }

        //“idt” Invoice Date(String)
        //Format:
        //20130403
        //Description:
        //1. The creation date on the invoice.
        //2. Date format is ISO without hyphen in the format YYYYMMDD.

        public string idt { get; set; }

        //“ddt” Due date(String)
        //Format:
        //20130425
        //Description:
        //1. The due date on the invoice.
        //2. Date format is ISO without hyphen in the format YYYYMMDD.

        public string ddt { get; set; }

        //“due” Due amount(Number)
        //Format:
        //450.5
        //-450.5
        //Description:
        //1. This is the total amount on invoice(The amount to be paid). That includes any
        //VAT.
        //2. The amount can be negative if tp = 2 or 3. In this case, the hyphen shall be directly
        //before the amount.
        //3. If tp = 1, the due amount must always be positive.

        public decimal due { get; set; }

        //“cur” Currency(String)
        //Format:
        //SEK
        //Description:
        //1. Currency format on the invoice in ISO 4217 standard.
        //2. If the invoice is domestic this field can be omitted.

        public string cur { get; set; }

        //“vat” Total VAT amount(Number)
        //Format:
        //100.25
        //-100.25
        //Description:
        //1. The total VAT amount on the invoice.
        //2. The amount can be negative if tp = 2 or 3. In that case, the hyphen shall be directly
        //before the amount.
        //3. If tp = 1, the due amount must always be positive.

        public decimal vat { get; set; }

        //“pt” Payment type(String)
        //Format:
        //IBAN
        //BBAN
        //BG
        //PG
        //Description:
        //1. The field describes the preferred payment method and the type of account
        //contained in the acc field.
        //2. Valid types are:
        //IBAN(International Bank Account Number)
        //BBAN(Basic Bank Account Number)
        //BG(Bankgiro) (Only used in Sweden)
        //PG(Nordea/Plusgiro) (Only used in Sweden)
        //a.If pt = IBAN, write IBAN number in acc, BIC in bc and company address in
        //adr
        //b.If pt = BBAN
        //i.And Invoice is domestic: Write account number in acc and bank
        //name in BIC form in bc.
        //ii.And Invoice is international: Write account number in acc , bank
        //code in bc and Company address in adr
        //c. If pt = BG, write Bankgiro number in acc
        //d. If pt = PG, write Plusgiro number in acc

        public string pt { get; set; }

        //“acc” Account(String)
        //Format:
        //IBAN: SE48600000000000658159712
        //IBAN: DK7030004073013887
        //Bg: 885-8383
        //Pg: 176099-0
        //Account: 8169-5 9139876057
        //Account: 6000658159712
        //Description:
        //1. Contains the deposit account on the invoice sender in the format specified in pt.
        //Page 11
        //2. Formats include BG(Sweden), PG(Sweden), IBAN(International), BBAN
        //(International)
        //3. The type of the account is defined in the field pt.


        public string acc { get; set; }

        //“bc” Bank code(String)
        //Format:
        //HANDSESS
        //ACIXUS33XXX
        //Description:
        //1. Bank code field can contain different bank codes according to the rules in the field
        //pt.
        //2. Bank code can be BIC/SWIFT or Bank code.
        //3. If the invoice requires payment to a domestic account (BBAN is used both for
        //foreign and domestic payments), write BIC here.BIC will be used to identify the
        //bank.


        public string bc { get; set; }

        //“adr” Address(String)
        //Format:
        //10500 Solna
        //Description:
        //1. Sender party address.
        //2. Address is required for certain foreign payments(defined by thse pt value).
        //3. Address is composed of postcode and city

        public string adr { get; set; }
    }
    
}

