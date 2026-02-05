using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util
{
    public class ChangeStatusGridViewParams
    {
        //Common
        public int ActorCompanyId { get; set; }
        public int UserId { get; set; }
        public SoeOriginType OriginType { get; set; }
        public SoeOriginStatusChange OriginStatusChange { get; set; }
        public int AccountYearId { get; set; }

        //PayDate
        public DateTime? BulkPayDate { get; set; }

        //PaymentMethod
        public int PaymentMethodId { get; set; }

        //Order to Invoice
        public bool MergeInvoices { get; set; }

        //Status of new invoice. False = Draft, True = Origin
        public bool SetStatusToOrigin { get; set; }

        //Claim level of invoice
        public int ClaimLevel { get; set; }
    }
}
