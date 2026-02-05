using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getCustomerByNumber: JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string customerNr = QS["cnr"];
            if (!string.IsNullOrEmpty(customerNr))
            {
                CustomerManager cm = new CustomerManager(ParameterObject);
                List<Customer> customers = cm.GetCustomersByCustomerNumber(SoeCompany.ActorCompanyId, customerNr, 20, null);

                Queue q = new Queue();
                
                if (customers != null)
                {
                    foreach (var customer in customers)
                    {
                        q.Enqueue(new
                        {
                            Found = true,
                            ActorCustomerId = customer.ActorCustomerId,
                            CustomerNr = customer.CustomerNr,
                            CustomerName = customer.Name,
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
