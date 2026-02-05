using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.SoftOne_Stage
{

    public class StageSyncDTO
    {
        public Guid CompanyApiKey { get; set; }
        public List<StageSyncItemDTO> StageSyncItemDTOs { get; set; }
    }

    public class StageSyncItemDTO
    {
        public StageSyncItemType StageSyncItemType { get; set; }
    }

    public enum StageSyncItemType
    {
        Vouchers = 1,
        Offers = 2,
        Contracts = 3,
        Orders = 4,
        Customerinvoices = 5,
        Customers = 6,
        Suppliers = 7,
        SupplierInvoices = 8,
        TimeCodeTransactions = 9,
        TimePayrollTransactions = 10,
        Schedule = 11,
    }

}
