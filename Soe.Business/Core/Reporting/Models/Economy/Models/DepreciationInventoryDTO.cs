using SoftOne.Soe.Common.DTO.Inventory;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models
{
	public class DepreciationInventoryDTO : IInventory
	{
		public int InventoryId { get; set; }
		public string InventoryNumber { get; set; }
		public string InventoryName { get; set; }
		public int InventoryStatus { get; set; }
		public string InventoryStatusName { get; set; }
		public string InventoryCategories { get; set; }
		public TermGroup_InventoryWriteOffMethodType InventoryWriteOffMethodType { get; set; }
		public TermGroup_InventoryWriteOffMethodPeriodType InventoryWriteOffPeriodType { get; set; }
		public decimal WriteOffAmount { get; set; }
		public decimal WriteOffSum { get; set; }
		public decimal WriteOffRemainingAmount { get; set; }
		public DateTime? WriteOffDate { get; set; }
		public decimal AnnualPercentage { get; set; }
		public int NumberOfPeriods { get; set; }
		public int PreviouslyDepreciatedPeriods { get; set; }
		public decimal TotalDepreciationAmount { get; set; } = 0;
		public decimal RemainingValue { get; set; } = 0;
		public IEnumerable<IInventoryLog> InventoryLog { get; set; }
		public PeriodUnitDTO[] PeriodUnits { get; set; }
	}

	public class InventoryLogDTO : IInventoryLog
	{
		public int InventoryId { get; set; }
		public DateTime Date { get; set; }
		public decimal Amount { get; set; }
		public TermGroup_InventoryLogType Type { get; set; }
	}

	public class PeriodUnitDTO
	{
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public string Name { get; set; }
		public decimal DepreciationAmount { get; set; } = 0m;
		public decimal BookValue { get; set; } = 0m;

		public PeriodUnitDTO Copy()
		{
			return new PeriodUnitDTO
			{
				DateFrom = this.DateFrom,
				DateTo = this.DateTo,
				Name = this.Name,
				DepreciationAmount = this.DepreciationAmount,
				BookValue = this.BookValue
			};
		}
	}

	public static class PeriodUnitDTOExtensions
	{
		/// <summary>
		/// Deep-copy the sequence of PeriodUnit into a new array
		/// </summary>
		public static PeriodUnitDTO[] Copy(this PeriodUnitDTO[] source)
		{
			if (source == null)
				return Array.Empty<PeriodUnitDTO>();

			var newArray = new PeriodUnitDTO[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				newArray[i] = source[i]?.Copy() ?? new PeriodUnitDTO();
			}
			return newArray;
		}
	}
}
