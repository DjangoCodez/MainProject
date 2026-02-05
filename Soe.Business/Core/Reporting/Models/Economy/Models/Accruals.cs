using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models
{
	public class AccrualSource
	{
		public DateTime Date { get; set; }
		public long SeqNr { get; set; }

		public AccrualSource(DateTime? date = null, long? seqNr = null)
		{
			Date = date ?? new DateTime(1900, 1, 1);
			SeqNr = seqNr ?? 0;
		}
	}

	public class AccrualSourceKey
	{
		public int HeadId { get; set; }
		public int? CustomerInvoiceId { get; set; }
		public int? SupplierInvoiceId { get; set; }
		public int? VoucherHeadId { get; set; }

		public AccrualSourceKey(int headId, int? customerInvoiceId = null, int? supplierInvoiceId = null, int? voucherHeadId = null)
		{
			HeadId = headId;
			CustomerInvoiceId = customerInvoiceId;
			SupplierInvoiceId = supplierInvoiceId;
			VoucherHeadId = voucherHeadId;
		}

		public (int, int, int) ToTuple()
		{
			return (HeadId, CustomerInvoiceId ?? SupplierInvoiceId ?? 0, VoucherHeadId ?? 0);
		}
	}
}
