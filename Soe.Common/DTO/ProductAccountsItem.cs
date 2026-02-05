
namespace SoftOne.Soe.Common.DTO
{
    public class ProductAccountsItem
    {
        public int RowId { get; set; }
        public int ProductId { get; set; }

        public int SalesAccountDim1Id { get; set; }
        public string SalesAccountDim1Nr { get; set; }
        public string SalesAccountDim1Name { get; set; }
        public int SalesAccountDim2Id { get; set; }
        public string SalesAccountDim2Nr { get; set; }
        public string SalesAccountDim2Name { get; set; }
        public int SalesAccountDim3Id { get; set; }
        public string SalesAccountDim3Nr { get; set; }
        public string SalesAccountDim3Name { get; set; }
        public int SalesAccountDim4Id { get; set; }
        public string SalesAccountDim4Nr { get; set; }
        public string SalesAccountDim4Name { get; set; }
        public int SalesAccountDim5Id { get; set; }
        public string SalesAccountDim5Nr { get; set; }
        public string SalesAccountDim5Name { get; set; }
        public int SalesAccountDim6Id { get; set; }
        public string SalesAccountDim6Nr { get; set; }
        public string SalesAccountDim6Name { get; set; }

        public int PurchaseAccountDim1Id { get; set; }
        public string PurchaseAccountDim1Nr { get; set; }
        public string PurchaseAccountDim1Name { get; set; }
        public int PurchaseAccountDim2Id { get; set; }
        public string PurchaseAccountDim2Nr { get; set; }
        public string PurchaseAccountDim2Name { get; set; }
        public int PurchaseAccountDim3Id { get; set; }
        public string PurchaseAccountDim3Nr { get; set; }
        public string PurchaseAccountDim3Name { get; set; }
        public int PurchaseAccountDim4Id { get; set; }
        public string PurchaseAccountDim4Nr { get; set; }
        public string PurchaseAccountDim4Name { get; set; }
        public int PurchaseAccountDim5Id { get; set; }
        public string PurchaseAccountDim5Nr { get; set; }
        public string PurchaseAccountDim5Name { get; set; }
        public int PurchaseAccountDim6Id { get; set; }
        public string PurchaseAccountDim6Nr { get; set; }
        public string PurchaseAccountDim6Name { get; set; }

        public int VatAccountDim1Id { get; set; }
        public string VatAccountDim1Nr { get; set; }
        public string VatAccountDim1Name { get; set; }

        public decimal VatRate { get; set; }
    }

    public class GetProductAccountItem
    {
        public int RowId { get; set; }
        public int ProductId { get; set; }
        public bool GetSalesAccounts { get; set; }
        public bool GetPurchaseAccounts { get; set; }
    }
}
