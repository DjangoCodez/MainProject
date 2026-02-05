using SoftOne.Soe.Business.Interfaces;
using SoftOne.Soe.Common.DTO.CustomerInvoice;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.SupplierAgreement
{
    public interface ISupplierAgreementWithNetPrices: ISupplierAgreement
    {
        List<WholsellerNetPriceRowDTO> ToNetPrices();
        bool HasNetPrice { get; }
        SoeWholeseller SysWholeSeller { get; }
    }
}
