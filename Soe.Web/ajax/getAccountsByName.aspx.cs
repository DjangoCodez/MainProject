using SoftOne.Soe.Business.Core;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAccountsByName : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["dim"], out int accountDimId))
            {
                string acc = QS["acc"];
                Queue q = new Queue();
                if (!String.IsNullOrEmpty(acc))
                {
                    foreach (var a in AccountManager.GetAccountsBySearch(accountDimId, acc))
                    {
                        q.Enqueue(new
                        {
                            AccountId = a.AccountId,
                            AccountNr = a.AccountNr,
                            Name = a.Name
                        });
                    }
                }
                ResponseObject = q;
            }
            else
                ResponseObject = null;
        }
    }
}
