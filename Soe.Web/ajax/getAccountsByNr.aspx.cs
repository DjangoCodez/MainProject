using SoftOne.Soe.Business.Core;
using System;
using System.Collections;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAccountsByNr : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["dim"], out int accountDimId))
            {
                string acc = QS["acc"];
                Queue q = new Queue();
                if (!String.IsNullOrEmpty(acc))
                {
                    foreach (var a in AccountManager.GetAccountsByAccountNr(acc, accountDimId))
                    {
                        q.Enqueue(new
                        {
                            a.AccountId,
                            a.AccountNr,
                            a.Name
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
