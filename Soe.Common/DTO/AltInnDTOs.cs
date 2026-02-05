using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public enum AltInnAuthMethods
    {
        SMSPin = 0,
        AltinnPin = 1,
        TaxPin = 2,
    }

    public enum AltInnError
    {
        PinHasExpired = 989,
    }

    public class AltInnUser
    {
        /// <summary>
        /// Fødselsnummer til bruker i sluttbrukersystemet som skal autentiseres
        /// </summary>
        public string UserSSN { get; set; }
        /// <summary>
        /// Passordet person har registrert for sin bruker i Altinn
        /// </summary>
        public string UserPassword { get; set; }
        /// <summary>
        /// Id som unikt identifiserer sluttbrukersystemet i Altinn
        /// </summary>
        public string EndUserSystemId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EndUserSystemPassword { get; set; }
        /// <summary>
        /// Angir hvilken engangskodetype bruker ønskes utfordret på
        /// </summary>
        public AltInnAuthMethods LogInMethod { get; set; }
        public string OrginizationNumber { get; set; }

        /// <summary>
        /// Pin code sended by sms to users phone.
        /// </summary>
        public string UserPinCode { get; set; }
    }

    public class AltInFormRF002Input
    {
        public string Comment { get; set; }
        public AltInnPeriodTypeEnum PeriodType { get; set; }
        public int Period { get; set; }
        public int Year { get; set; }
        public bool IsComplete { get; set; }
    }

    public class AltInnReciept
    {
        public int ReceiptId { get; set; }
        public string ReceiptText { get; set; }
        public int ReceiptStatusCode { get; set; }
        public string ReceiptStatusCodeText { get; set; }
        public DateTime LastChanged { get; set; }

        public string ErrorMessage { get; set; }
        public int ErrorNumber { get; set; }
        public bool Success { get; set; }
        public List<AltInnReciept> SubReceipts { get; set; }
    }
}
