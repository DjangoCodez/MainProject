using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Status.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Bridge
{
    public class VismaPayrollChangesReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly VismaPayrollChangesReportDataOutput _reportDataOutput;

        public VismaPayrollChangesReportData(ParameterObject parameterObject, VismaPayrollChangesReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataOutput = new VismaPayrollChangesReportDataOutput(reportDataInput);
        }

        public VismaPayrollChangesReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            base.reportResult = reportResult;

            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        public ActionResult LoadData()
        {
            #region Get selections

            if (!TryGetDatesFromSelection(reportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);

            #endregion

            #region  Fetch data

            _reportDataOutput.ResultItems = new List<VismaPayrollChangesItem>();

            var changes = BridgeManager.GetVismaPayrollChanges(reportResult.ActorCompanyId, selectionDateFrom, selectionDateTo);
            foreach (var change in changes)
            {
                VismaPayrollChangesItem item = new VismaPayrollChangesItem()
                {
                    VismaPayrollBatchId = change.VismaPayrollBatchId,
                    VismaPayrollChangeId = change.VismaPayrollChangeId,
                    PersonId = change.PersonId,
                    VismaPayrollEmploymentId = change.VismaPayrollEmploymentId,
                    Entity = change.Entity,
                    Info = change.Info,
                    Field = change.Field,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                    EmployerRegistrationNumber = change.EmployerRegistrationNumber,
                    PersonName = change.PersonName,
                    Time = change.Time
                };

                _reportDataOutput.ResultItems.Add(item);
            }

            #endregion

            return new ActionResult();
        }
    }

    public class VismaPayrollChangesReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_VismaPayrollChangesMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public VismaPayrollChangesReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_VismaPayrollChangesMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_VismaPayrollChangesMatrixColumns.Unknown;
        }
    }

    public class VismaPayrollChangesReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<VismaPayrollChangesReportDataField> Columns { get; set; }

        public VismaPayrollChangesReportDataInput(CreateReportResult reportResult, List<VismaPayrollChangesReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class VismaPayrollChangesReportDataOutput : IReportDataOutput
    {
        public VismaPayrollChangesReportDataOutput(VismaPayrollChangesReportDataInput input)
        {
            ResultItems = new List<VismaPayrollChangesItem>();
            Input = input;
        }
        public ActionResult Result { get; set; }
        public List<VismaPayrollChangesItem> ResultItems { get; set; }
        public VismaPayrollChangesReportDataInput Input { get; set; }
    }
}
