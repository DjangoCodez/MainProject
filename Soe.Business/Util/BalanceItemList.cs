using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util
{
    public class BalanceItemList
	{
		public int Status { get; set; }
		public List<AccountYearBalanceHead> BalanceList { get; set; }
	}
}
