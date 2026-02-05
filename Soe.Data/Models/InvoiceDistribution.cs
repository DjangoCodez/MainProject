using SoftOne.Soe.Common.DTO;
using System;
using System.Linq.Expressions;


namespace SoftOne.Soe.Data
{
    public static partial class EntityExtensions
    {
        public readonly static Expression<Func<InvoiceDistribution, InvoiceDistributionDTO>> GetInvoiceDistributionInvoiceDTO =
        id => new InvoiceDistributionDTO
        {
            InvoiceDistributionId = id.InvoiceDistributionId,
            OriginId = id.OriginId,
            Guid = id.Guid,
            Message = id.Msg,
            Status = id.DistributionStatus,
            Type = id.DistributionType,
            CreatedBy = id.CreatedBy,
            Created = id.Created,
            Modified = id.Modified,
            SeqNr = id.Origin.Invoice.SeqNr.ToString(),
            CustomerName = id.Origin.Invoice.Actor.Customer.Name,
            CustomerNr = id.Origin.Invoice.Actor.Customer.CustomerNr,
            OriginTypeId = id.Origin.Type,
        };
        public readonly static Expression<Func<InvoiceDistribution, InvoiceDistributionDTO>> GetInvoiceDistributionPurchaseDTO =
        id => new InvoiceDistributionDTO
        {
            InvoiceDistributionId = id.InvoiceDistributionId,
            OriginId = id.OriginId,
            Guid = id.Guid,
            Message = id.Msg,
            Status = id.DistributionStatus,
            Type = id.DistributionType,
            CreatedBy = id.CreatedBy,
            Created = id.Created,
            Modified = id.Modified,
            SeqNr = id.Origin.Purchase.PurchaseNr,
            CustomerName = id.Origin.Purchase.Supplier.Name,
            CustomerNr = id.Origin.Purchase.Supplier.SupplierNr,
            OriginTypeId = id.Origin.Type,
        };
    }
}
