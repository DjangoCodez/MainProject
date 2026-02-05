using SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy
{
	public class DepreciationReportData : EconomyReportDataManager, IReportDataModel
	{
		private readonly DepreciationReportDataOutput _reportDataOutput;

		public DepreciationReportData(ParameterObject parameterObject, DepreciationReportDataInput reportDataInput) : base(parameterObject)
		{
			_reportDataOutput = new DepreciationReportDataOutput(reportDataInput);
		}

		public DepreciationReportDataOutput CreateOutput(CreateReportResult reportResult)
		{
			base.reportResult = reportResult;

			_reportDataOutput.Result = LoadData();
			if (!_reportDataOutput.Result.Success)
				return _reportDataOutput;

			return _reportDataOutput;
		}

		public ActionResult LoadData()
		{
			TryGetDateFromSelection(reportResult, out DateTime selectionDate, "date");
			TryGetIdsFromSelection(reportResult, out List<int> categories, "categories");
			TryGetIdsFromSelection(reportResult, out List<int> statuses, "statuses");
			TryGetIdFromSelection(reportResult, out int? prognoseType, "prognoseType");
			TryGetIdFromSelection(reportResult, out int? periods, "periods");
			if (periods is null || periods == 0)
				throw new ArgumentException("Periods must be specified for depreciation prognose.");

			var (endDate, monthsInPeriodUnit) = CalculatePeriodEnd(selectionDate, prognoseType, periods);
			var inventories = DepreciationManager.GetInventoryForDepreciations(
				base.ActorCompanyId,
				endDate,
				selectionDate,
				statuses,
				categories
			).ToList();

			var missingDepreciations = DepreciationManager.GenerateMissingDepreciations(inventories, endDate);
			foreach(var inventory in inventories)
			{
				var depreciations = inventory.InventoryLog.ToList();
				bool success = missingDepreciations.TryGetValue(inventory.InventoryId, out var preliminaryDepreciations);
				if (success)
				{
					depreciations.AddRange(preliminaryDepreciations);
					inventory.InventoryLog = depreciations;
				}
			}
			AddDepreciationsToInventories(inventories, selectionDate, monthsInPeriodUnit, (int)periods);
			_reportDataOutput.DepreciationInventories.AddRange(inventories);

			return new ActionResult();
		}

		private static (DateTime, int) CalculatePeriodEnd(DateTime date, int? timeUnit, int? periods)
		{
			if (!periods.HasValue)
				return (date, 1);
			if (!timeUnit.HasValue)
				timeUnit = 1; // Default to months

			int monthsInTimeUnit = 1;
			switch ((TermGroup_PrognoseInterval)timeUnit)
			{
				case TermGroup_PrognoseInterval.Quarter:
					monthsInTimeUnit = 3;
					break;
				case TermGroup_PrognoseInterval.Season:
					monthsInTimeUnit = 6;
					break;
				case TermGroup_PrognoseInterval.Year:
					monthsInTimeUnit = 12;
					break;
				default:
					monthsInTimeUnit = 1;
					break;
			}

			var endDate = date.AddMonths(monthsInTimeUnit * (int)periods);
			return (endDate, monthsInTimeUnit);
		}

		private static PeriodUnitDTO[] CreatePeriodUnits(DateTime dateFrom, int periodDurationInMonths, int periods)
		{
			var result = new PeriodUnitDTO[periods];
			for (int i = 0; i < periods; i++)
			{
				var periodStart = dateFrom.AddMonths(periodDurationInMonths * i);
				var periodEnd = periodStart.AddMonths(periodDurationInMonths).AddTicks(-1);

				result[i] = new PeriodUnitDTO
				{
					DateFrom = periodStart,
					DateTo = periodEnd,
					Name = $"{periodStart:yyyy-MM-dd} - {periodEnd:yyyy-MM-dd}"
				};
			}
			return result;
		}

		private static void AddDepreciationsToInventories(List<DepreciationInventoryDTO> inventories, DateTime selectionDate, int monthsInPeriodUnit, int periods)
		{
			var periodUnits = CreatePeriodUnits(selectionDate, monthsInPeriodUnit, periods);

			foreach (var inventory in inventories)
			{
				int pi = 0;
				inventory.PeriodUnits = periodUnits.Copy();

				decimal bookValue = inventory.WriteOffAmount;
				var depreciations = inventory.InventoryLog.OrderBy(p => p.Date).ToArray();

				foreach (var periodUnit in inventory.PeriodUnits)
				{
					decimal sum = 0;

					while (pi < depreciations.Length && depreciations[pi].Date < periodUnit.DateFrom)
					{
						bookValue -= depreciations[pi].Amount;
						pi++;
					}

					while (pi < depreciations.Length && depreciations[pi].Date < periodUnit.DateTo)
					{
						sum += depreciations[pi].Amount;
						pi++;
					}

					bookValue -= sum;
					periodUnit.BookValue = bookValue;
					periodUnit.DepreciationAmount = sum;
				}
				inventory.TotalDepreciationAmount = inventory.PeriodUnits.Sum(pu => pu.DepreciationAmount) + inventory.WriteOffSum;
				inventory.RemainingValue = bookValue - inventory.WriteOffSum;
			}
		}
	}

	#region Classes
	public class DepreciationReportDataReportDataField
	{
		public MatrixColumnSelectionDTO Selection { get; set; }
		public TermGroup_DepreciationMatrixColumns Column { get; set; }
		public string ColumnKey { get; set; }

		public int Sort
		{
			get
			{
				return Selection?.Sort ?? 0;
			}
		}

		public DepreciationReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
		{
			this.Selection = columnSelectionDTO;
			this.ColumnKey = Selection?.Field;
			this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_DepreciationMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_DepreciationMatrixColumns.Unknown;
		}
	}

	public class DepreciationReportDataInput
	{
		public CreateReportResult ReportResult { get; set; }
		public List<DepreciationReportDataReportDataField> Columns { get; set; }

		public DepreciationReportDataInput(CreateReportResult reportResult, List<DepreciationReportDataReportDataField> columns)
		{
			this.ReportResult = reportResult;
			this.Columns = columns;
		}
	}

	public class DepreciationReportDataOutput : IReportDataOutput
	{
		public ActionResult Result { get; set; }
		public List<DepreciationInventoryDTO> DepreciationInventories { get; set; }
		public DepreciationReportDataInput Input { get; set; }

		public DepreciationReportDataOutput(DepreciationReportDataInput input)
		{
			this.DepreciationInventories = new List<DepreciationInventoryDTO>();
			this.Input = input;
		}
	}
	#endregion
}
