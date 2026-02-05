using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Time.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time
{
    public class AbsenceRequestReportData : IReportDataModel
    {
        private readonly AbsenceRequestReportDataInput _reportDataInput;
        private readonly AbsenceRequestReportDataOutput _reportDataOutput;

        private TimeScheduleManager TimeScheduleManager { get { return _reportDataInput.TimeScheduleManager; } }
        private TimeReportDataManager TimeReportDataManager { get { return _reportDataInput.TimeReportDataManager; } }
        private CreateReportResult ReportResult { get { return _reportDataInput.ReportResult; } }

        public AbsenceRequestReportData(ParameterObject parameterObject, AbsenceRequestReportDataInput reportDataInput)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new AbsenceRequestReportDataOutput(reportDataInput);
        }

        public static List<AbsenceRequestReportDataField> GetPossibleDataFields()
        {
            List<AbsenceRequestReportDataField> possibleFields = new List<AbsenceRequestReportDataField>();
            EnumUtility.GetValues<TermGroup_AbsenceRequestMatrixColumns>().ToList()
                .ForEach(f => possibleFields.Add(new AbsenceRequestReportDataField(null)
                { Column = f }));

            return possibleFields;
        }

        public AbsenceRequestReportDataOutput CreateOutput(CreateReportResult reportResult)
        {
            _reportDataOutput.Result = LoadData();
            if (!_reportDataOutput.Result.Success)
                return _reportDataOutput;

            return _reportDataOutput;
        }

        private ActionResult LoadData()
        {
            #region Prereq

            if (!TimeReportDataManager.TryGetDatesFromSelection(ReportResult, out DateTime selectionDateFrom, out DateTime selectionDateTo))
                return new ActionResult(false);
            if (!TimeReportDataManager.TryGetEmployeeIdsFromSelection(ReportResult, selectionDateFrom, selectionDateTo, out _, out List<int> selectionEmployeeIds))
                return new ActionResult(false);

            #endregion

            using (CompEntities entities = new CompEntities())
            {
                if (selectionEmployeeIds.Any())
                {
                    #region Collections

                    var input = TimeScheduleManager.GetShiftQueuesForEmployee(entities, 1, DateTime.Today);

                    #endregion

                    #region Content

                    foreach (int employeeId in selectionEmployeeIds)
                    {
                        Thread.Sleep(1);
                    }

                    #endregion
                }

                return new ActionResult();
            }
        }
    }

    public class AbsenceRequestReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_AbsenceRequestMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public AbsenceRequestReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_AbsenceRequestMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_AbsenceRequestMatrixColumns.EmployeeNr;
        }
    }

    public class AbsenceRequestReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<AbsenceRequestReportDataField> Columns { get; set; }
        public readonly EmployeeManager EmployeeManager;
        public readonly TimeScheduleManager TimeScheduleManager;
        public readonly TimeReportDataManager TimeReportDataManager;

        public AbsenceRequestReportDataInput(CreateReportResult reportResult, TimeReportDataManager timeReportDataManager, EmployeeManager employeeManager, TimeScheduleManager timeScheduleManager, List<AbsenceRequestReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
            this.TimeReportDataManager = timeReportDataManager;
            this.EmployeeManager = employeeManager;
            this.TimeScheduleManager = timeScheduleManager;
        }
    }

    public class AbsenceRequestReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public AbsenceRequestReportDataInput Input { get; set; }
        public List<AbsenceRequestItem> AbsenceRequestItems { get; set; }

        public AbsenceRequestReportDataOutput(AbsenceRequestReportDataInput input)
        {
            Input = input;
            AbsenceRequestItems = new List<AbsenceRequestItem>();
        }
    }
}

