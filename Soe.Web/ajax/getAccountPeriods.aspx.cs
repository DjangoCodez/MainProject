using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getAccountPeriods :  JsonBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            if (Int32.TryParse(QS["year"], out int accountYear))
            {
                AccountYear getAccountYear = AccountManager.GetAccountYear(accountYear);
                Dictionary<int, string> dict = AccountManager.GetAccountPeriodsInDateIntervalDict(accountYear, getAccountYear.From, getAccountYear.To);

                Queue q = new Queue();
                int i = 0;
                foreach (KeyValuePair<int, string> kvp in dict)
                {
                    q.Enqueue(new
                    {
                        Found = true,
                        Position = i,
                        AccountPeriodId = kvp.Key,
                        Interval = kvp.Value,
                    });

                    i++;
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
