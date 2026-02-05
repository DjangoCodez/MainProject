using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getSupplierByNumber: JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string supplierNr = QS["snr"];
            if (!string.IsNullOrEmpty(supplierNr))
            {
                SupplierManager sm = new SupplierManager(ParameterObject);
                List<Supplier> suppliers = sm.GetSuppliersBySearch(SoeCompany.ActorCompanyId, supplierNr, supplierNr, 20);

                Queue q = new Queue();
                
                if (suppliers != null)
                {
                    foreach (var supplier in suppliers)
                    {
                        q.Enqueue(new
                        {
                            Found = true,
                            ActorSupplierId = supplier.ActorSupplierId,
                            SupplierNr = supplier.SupplierNr,
                            SupplierName = supplier.Name,
                        });
                    }
                }

                ResponseObject = q;
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}
