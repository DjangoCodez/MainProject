using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.PaymentIO.SEPAV3
{
    /// <summary>
    /// Result from ConvertSupplierStreamToEntity containing the converted payment imports.
    /// Callers are responsible for persisting via AddPaymentsImports.
    /// </summary>
    public class ConvertSupplierStreamResult
    {
        public ActionResult Result { get; set; }
        public List<PaymentImportIO> PaymentImports { get; set; }
        public DateTime? ImportDate { get; set; }

        public ConvertSupplierStreamResult()
        {
            Result = new ActionResult(true);
            PaymentImports = new List<PaymentImportIO>();
        }
    }
}
