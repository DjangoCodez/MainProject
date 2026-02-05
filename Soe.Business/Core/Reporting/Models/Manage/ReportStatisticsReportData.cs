using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Manage.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage
{
    public class ReportStatisticsReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly ReportStatisticsReportDataOutput _reportDataOutput;
        private readonly ReportStatisticsReportDataInput _reportDataInput;

        public ReportStatisticsReportData(ParameterObject parameterObject, ReportStatisticsReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new ReportStatisticsReportDataOutput(reportDataInput);
        }
  
        public ReportStatisticsReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();

            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Prereq

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(true);

            #endregion

            #region Init repository

            base.InitPersonalDataEmployeeReportRepository();
            int langId = GetLangId();
            Dictionary<int, string> sysReport = base.GetTermGroupDict(TermGroup.SysReportTemplateType, langId);
            #endregion

            MatrixColumnsSelectionDTO matrixColumnsSelection = TryGetMatrixColumnsSelectionDTOFromSelection(reportResult);
            bool isInsight = matrixColumnsSelection.InsightId != 0 ;
            try
            {

                using (CompEntities entities = new CompEntities())
                {

                    List<ReportPrintoutDTO> reports = ReportManager.GetReportPrintoutStatisticsForPeriod(entities, base.ActorCompanyId, selectionDateFrom, selectionDateTo);

                    List<ReportStatisticsItem> returnedItems = new List<ReportStatisticsItem>();

                    foreach (ReportPrintoutDTO report in reports.OrderBy(a => a.Created))
                    {

                        ReportStatisticsItem item = new ReportStatisticsItem
                        {
                            ReportName = report.ReportName ?? string.Empty,
                            SystemReportName = GetValueFromDict((int)report.SysReportTemplateType, sysReport),
                            DelTime = report.DeliveredTime.HasValue ? (decimal)(report.DeliveredTime.Value - report.Created).TotalSeconds : 0,
                            AmountPrintOut = 1,
                            AverageTime = 0,
                            MedianTime = 0,
                            Period = report.Created.ToString("yyyy-MM"),
                            AmountOfUniqueUsers = 1,
                            UserId = report.UserId,
                            AmountOfFailed = report.ResultMessage,
                            Date = report.Created.ToString("yyyy-MM-dd")
                        };

                        returnedItems.Add(item);
                    }
                    if (isInsight)
                    {
                        _reportDataOutput.ReportStatisticsItems = returnedItems.OrderBy(o => o.Date).ThenBy(t => t.ReportName).ToList();
                    }
                    else
                    {
                        List<ReportStatisticsItem> mergedItems = new List<ReportStatisticsItem>();
                        var columns = _reportDataInput.Columns.Select(s => s.Column).ToList();

                        foreach (var grouped in returnedItems.GroupBy(g => g.GroupOn(columns)))
                        {
                            var first = grouped.First();
                            var mergedItem = first;

                            mergedItem.AmountOfUniqueUsers = grouped.GroupBy(s => s.UserId).Count();
                            mergedItem.AverageTime = (int)grouped.Sum(s => s.DelTime) / grouped.Count();
                            mergedItem.AmountOfFailed = grouped.Sum(s => s.AmountOfFailed);
                            mergedItem.AmountPrintOut = grouped.Sum(s => s.AmountPrintOut);
                            mergedItem.MedianTime = (int)NumberUtility.GetMedianValue(grouped.Select(s => s.DelTime).ToList());

                            mergedItems.Add(mergedItem);
                        }

                        _reportDataOutput.ReportStatisticsItems = mergedItems.OrderBy(o => o.ReportName).ThenBy(t => t.Date).ToList();
                    }
                   
                }
                

            }
            catch (Exception ex)
            {
                LogError(ex, log);
                return new ActionResult(ex);
            }

            #region Close repository

            base.personalDataRepository.GenerateLogs();

            #endregion

            return new ActionResult();
        }

        private string GetValueFromDict(int? key, Dictionary<int, string> dict)
        {
            if (!key.HasValue || dict.Count == 0)
                return string.Empty;

            dict.TryGetValue(key.Value, out string value);

            if (value != null)
                return value;

            return string.Empty;
        }
    }

    public class ReportStatisticsReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_ReportStatisticsMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public ReportStatisticsReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_ReportStatisticsMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_ReportStatisticsMatrixColumns.Unknown;
        }
    }

    public class ReportStatisticsReportDataInput
    {
        public readonly CreateReportResult ReportResult;
        public List<ReportStatisticsReportDataField> Columns { get; set; }

        public ReportStatisticsReportDataInput(CreateReportResult reportResult, List<ReportStatisticsReportDataField> ReportStatisticsReportDataFields)
        {
            this.ReportResult = reportResult;
            this.Columns = ReportStatisticsReportDataFields;
        }
    }

    public class ReportStatisticsReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<ReportStatisticsItem> ReportStatisticsItems { get; set; }
        public ReportStatisticsReportDataInput Input { get; set; }

        public ReportStatisticsReportDataOutput(ReportStatisticsReportDataInput input)
        {
            this.ReportStatisticsItems = new List<ReportStatisticsItem>();
            this.Input = input;
        }
    }
}
