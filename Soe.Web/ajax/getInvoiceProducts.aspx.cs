using SoftOne.Soe.Business.Core;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getInvoiceProducts : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["c"], out int actorCompanyId))
            {
                string prod = QS["prod"];
                bool onlyFixed = Convert.ToBoolean(QS["of"]);
                ProductManager pm = new ProductManager(ParameterObject);
                Queue q = new Queue();
                foreach (var p in pm.GetInvoiceProductsBySearch(actorCompanyId, prod, onlyFixed))
                {
                    q.Enqueue(new
                    {
                        Found = true,
                        ProductId = p.ProductId,
                        Number = p.Number,
                        Name = p.Name
                    });
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
