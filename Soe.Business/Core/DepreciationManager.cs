using NodaTime;
using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Common.DTO.Inventory;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SoftOne.Soe.Business.Core
{
	public class DepreciationManager : ManagerBase
	{
		public DepreciationManager(ParameterObject parameterObject) : base(parameterObject) { }

		public List<DepreciationInventoryDTO> GetInventoryForDepreciations(int actorCompanyId, DateTime toDate, DateTime? fromDate = null, List<int> statuses = null, IEnumerable<int> categories = null)
		{
			var includedCategoryIds = categories?.ToArray() ?? Array.Empty<int>();
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            var excludedInventoryIds = Array.Empty<int>();
			if (includedCategoryIds.Length > 0)
				excludedInventoryIds = entitiesReadOnly.CompanyCategoryRecord
					.Where(ccr => 
						ccr.ActorCompanyId == actorCompanyId &&
						ccr.Entity == (int)SoeCategoryRecordEntity.Inventory &&
						!includedCategoryIds.Contains(ccr.CategoryId))
					.Select(s => s.RecordId)
					.ToArray();

			var query = entitiesReadOnly.Inventory
				.AsNoTracking()
				.Where(i =>
					i.ActorCompanyId == actorCompanyId &&
					i.State == (int)SoeEntityState.Active &&
					!excludedInventoryIds.Contains(i.InventoryId)
				).Select(i => new DepreciationInventoryDTO()
				{
					InventoryId = i.InventoryId,
					InventoryNumber = i.InventoryNr,
					InventoryName = i.Name,
					InventoryStatus = i.Status,
					WriteOffAmount = i.WriteOffAmount,
					WriteOffSum = i.WriteOffSum,
					WriteOffDate = i.WriteOffDate ?? i.PurchaseDate,
					WriteOffRemainingAmount = i.WriteOffRemainingAmount,
					InventoryWriteOffMethodType = (TermGroup_InventoryWriteOffMethodType)i.InventoryWriteOffMethod.Type,
					InventoryWriteOffPeriodType = (TermGroup_InventoryWriteOffMethodPeriodType)i.InventoryWriteOffMethod.PeriodType,
					AnnualPercentage = i.InventoryWriteOffMethod.YearPercent,
					NumberOfPeriods = i.PeriodValue,
					PreviouslyDepreciatedPeriods = i.WriteOffPeriods,
					InventoryCategories = ""
				});
			if (statuses != null && statuses.Count > 0)
				query = query.Where(i => statuses.Contains(i.InventoryStatus));

			var result = query.ToList();

			var inventoryLogDict = GetInventoryLogDict(result, excludedInventoryIds, toDate);
			var inventoryCategoryDict = GetCategoriesDict(result, includedCategoryIds);
			var statusDict = TermManager.GetTermGroupDict(TermGroup.InventoryStatus);
			foreach (var inventory in result)
			{
				inventory.InventoryCategories = inventoryCategoryDict.ContainsKey(inventory.InventoryId) ? inventoryCategoryDict[inventory.InventoryId] : "";
				inventory.InventoryStatusName = statusDict.ContainsKey(inventory.InventoryStatus) ? statusDict[inventory.InventoryStatus] : "";
				inventory.InventoryLog = inventoryLogDict.ContainsKey(inventory.InventoryId) ? inventoryLogDict[inventory.InventoryId] : new List<InventoryLogDTO>();
			}

			return result;
		}

		private Dictionary<int, string> GetCategoriesDict(List<DepreciationInventoryDTO> depreciationInventories, int[] includedCategoryIds)
		{
			if (includedCategoryIds == null || includedCategoryIds.Length == 0)
				return new Dictionary<int, string>();
			if (depreciationInventories == null || depreciationInventories.Count == 0)
				return new Dictionary<int, string>();

			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			return entitiesReadOnly.CompanyCategoryRecord
				.AsNoTracking()
				.Where(ccr => ccr.Category != null && includedCategoryIds.Contains(ccr.Category.CategoryId))
				.Select(ccr => new { InventoryId = ccr.RecordId, CategoryName = ccr.Category.Name })
				.Distinct()
				.AsEnumerable()
				.GroupBy(g => g.InventoryId)
				.ToDictionary(
					g => g.Key,
					g => string.Join(", ", g.Select(c => c.CategoryName).ToArray())
				);
		}

		private Dictionary<int, List<InventoryLogDTO>> GetInventoryLogDict(List<DepreciationInventoryDTO> depreciationInventories, int[] excludedInventoryIds, 
			DateTime toDate, DateTime? fromDate = null)
		{
			if (depreciationInventories == null || depreciationInventories.Count == 0)
				return new Dictionary<int, List<InventoryLogDTO>>();
			if (excludedInventoryIds == null)
				excludedInventoryIds = Array.Empty<int>();

			var includedInventoryIds = depreciationInventories.Select(di => di.InventoryId).ToArray();

			int[] inventoryLogTypes = new int[]
			{
				(int)TermGroup_InventoryLogType.WriteOff,
				(int)TermGroup_InventoryLogType.OverWriteOff,
				(int)TermGroup_InventoryLogType.WriteUp,
				(int)TermGroup_InventoryLogType.WriteDown
			};
			using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
			var logQuery = entitiesReadOnly.InventoryLog
				.AsNoTracking()
				.Where(log =>
					includedInventoryIds.Contains(log.InventoryId) &&
					log.Date < toDate &&
					inventoryLogTypes.Contains(log.Type) &&
					!excludedInventoryIds.Contains(log.InventoryId))
				.Select(log => new InventoryLogDTO()
				{
					InventoryId = log.InventoryId,
					Date = log.Date,
					Amount = log.Amount,
					Type = (TermGroup_InventoryLogType)log.Type
				});

			if (fromDate != null)
				logQuery = logQuery.Where(il => il.Date >= fromDate);

			return logQuery
				.GroupBy(g => g.InventoryId)
				.ToDictionary(g => g.Key, g => g.ToList());
		}

		public static Dictionary<int, List<InventoryLogDTO>> GenerateMissingDepreciations(IEnumerable<IInventory> inventories, DateTime endDate)
		{
			var result = new Dictionary<int, List<InventoryLogDTO>>();
			if (inventories is null || inventories.Count() == 0)
				return result;

			foreach (var inventory in inventories)
			{
				if (!Validated(inventory))
					continue;

				bool usesMonthlyDepreciation = inventory.UsesMonthlyDepreciation();
				bool usesLinearDepreciation = inventory.UsesLinearDepreciation();
				var depreciationAmount = usesLinearDepreciation
					? CalculateLinearDepreciationAmount(inventory.WriteOffAmount, inventory.NumberOfPeriods)
					: 0;

				var bookValue = inventory.WriteOffAmount - inventory.WriteOffSum;
				var asPlannedDepreciations = inventory.InventoryLog.Where(d => d.IsRegular());
				bookValue -= asPlannedDepreciations.Sum(d => d.Amount);
				bookValue -= inventory.InventoryLog.Where(d => !d.IsRegular()).Sum(d => d.Amount);

				var depreciations = new List<InventoryLogDTO>();

				var date = usesMonthlyDepreciation ?
					inventory.WriteOffDate.Value.AddMonths(inventory.PreviouslyDepreciatedPeriods) :
					inventory.WriteOffDate.Value.AddYears(inventory.PreviouslyDepreciatedPeriods);
				var monthIncrement = usesMonthlyDepreciation ? 1 : 12;
				while (date < endDate)
				{
					if (bookValue <= 0)
						break;

					if (asPlannedDepreciations.Any(d => SameMonth(d.Date, date)))
					{
						date = date.AddMonths(monthIncrement);
						continue;
					}

					if (!usesLinearDepreciation)
						depreciationAmount = CalculateNonLinearDepreciationAmount(
							inventory.WriteOffDate.Value, 
							date,
							inventory.WriteOffAmount, 
							inventory.AnnualPercentage / 100m, 
							usesMonthlyDepreciation);

					if (!usesLinearDepreciation && depreciationAmount + 1 > bookValue) // Don't leave 0.x
						depreciationAmount = bookValue;
					else if (usesLinearDepreciation && bookValue - depreciationAmount < inventory.NumberOfPeriods) // Handle 0.x handed over from previous periods.
						depreciationAmount = bookValue;

					if (depreciationAmount > bookValue)
						depreciationAmount = bookValue;

					bookValue -= depreciationAmount;
					depreciations.Add(new InventoryLogDTO
					{
						Date = date,
						Amount = depreciationAmount,
						Type = TermGroup_InventoryLogType.WriteOff
					});

					date = date.AddMonths(monthIncrement);
				}
				result.Add(inventory.InventoryId, depreciations);
			}
			return result;
		}

		private static bool SameMonth(DateTime a, DateTime b)
			=> a.Year == b.Year && a.Month == b.Month;

		private static decimal CalculateLinearDepreciationAmount(decimal writeOffAmount, int numberOfPeriods)
			=> Math.Round(writeOffAmount / numberOfPeriods, 2);

		/// <summary>
		/// Calculates the depreciation amount for non-linear depreciation
		/// </summary>
		/// <param name="depreciationStartDate">The inventory's depreciation start date</param>
		/// <param name="currentDepreciationDate">The date for which you are looking to create a depreciation</param>
		/// <param name="assetValue">The Starting value of the asset, before any depreciations were done</param>
		/// <param name="annualPercentage">The percentage the asset should be depricated by each year, e.g. 0.3 for 30%</param>
		/// <param name="usesMonthlyDepreciation">Whether depreciation occurs each month, if false calculation will return yearly basis</param>
		/// <returns>Depreciation amount for a period of given year in the asset's life-cycle</returns>
		private static decimal CalculateNonLinearDepreciationAmount(DateTime depreciationStartDate, DateTime currentDepreciationDate, 
			decimal assetValue, decimal annualPercentage, bool usesMonthlyDepreciation)
		{
			var year = GetDepreciationYear(depreciationStartDate, currentDepreciationDate);
			var depreciation = assetValue * annualPercentage * NumberUtility.Power(1 - annualPercentage, year - 1);

			if (usesMonthlyDepreciation)
				depreciation /= 12;

			return Math.Round(depreciation, 2);
		}

		private static int DateDiffMonths(DateTime fromDate, DateTime toDate)
			=> (toDate.Year - fromDate.Year) * 12 + (toDate.Month - fromDate.Month);

		private static int GetDepreciationYear(DateTime depreciationStartDate, DateTime currentDepreciationDate)
			=> DateDiffMonths(depreciationStartDate, currentDepreciationDate) / 12 + 1;

		private static bool Validated(IInventory inventory)
		{
			return
				inventory.WriteOffDate.HasValue &&
				(
					inventory.UsesLinearDepreciation() && inventory.NumberOfPeriods > inventory.PreviouslyDepreciatedPeriods  ||
					inventory.UsesNonLinearDepreciation()
				) &&
				(
					inventory.UsesMonthlyDepreciation() ||
					inventory.UsesYearlyDepreciation()
				);
		}
	}
}
