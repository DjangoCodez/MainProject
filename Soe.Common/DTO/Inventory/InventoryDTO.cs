using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.Inventory
{
	public interface IInventory
	{
		int InventoryId { get; set; }
		string InventoryNumber { get; set; }
		string InventoryName { get; set; }
		int InventoryStatus { get; set; }
		string InventoryCategories { get; set; }
		TermGroup_InventoryWriteOffMethodType InventoryWriteOffMethodType { get; set; }
		TermGroup_InventoryWriteOffMethodPeriodType InventoryWriteOffPeriodType { get; set; }
		decimal WriteOffAmount { get; set; }
		decimal WriteOffSum { get; set; }
		decimal WriteOffRemainingAmount { get; set; }
		DateTime? WriteOffDate { get; set; }
		decimal AnnualPercentage { get; set; }
		int NumberOfPeriods { get; set; }
		int PreviouslyDepreciatedPeriods { get; set; }
		IEnumerable<IInventoryLog> InventoryLog { get; set; }
	}

	public interface IInventoryLog
	{
		DateTime Date { get; set; }
		decimal Amount { get; set; }
		TermGroup_InventoryLogType Type { get; set; }
	}

	public static class InventoryDTOExtensions
	{
		public static bool IsRegular(this IInventoryLog log)
		{
			return log.Type == TermGroup_InventoryLogType.WriteOff;
		}

		public static bool UsesLinearDepreciation(this IInventory i)
		{
			return i.InventoryWriteOffMethodType == TermGroup_InventoryWriteOffMethodType.AccordingToTheBooks_ComplementaryRule;
		}

		public static bool UsesNonLinearDepreciation(this IInventory i)
		{
			return i.InventoryWriteOffMethodType == TermGroup_InventoryWriteOffMethodType.AccordingToTheBooks_MainRule;
		}	

		public static bool UsesMonthlyDepreciation(this IInventory i)
		{
			return i.InventoryWriteOffPeriodType == TermGroup_InventoryWriteOffMethodPeriodType.Period;
		}

		public static bool UsesYearlyDepreciation(this IInventory i)
		{
			return i.InventoryWriteOffPeriodType == TermGroup_InventoryWriteOffMethodPeriodType.Year;
		}
	}
}
