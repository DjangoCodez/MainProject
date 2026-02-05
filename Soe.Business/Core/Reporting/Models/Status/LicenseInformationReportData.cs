using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.Reporting.Models.Interface;
using SoftOne.Soe.Business.Core.Reporting.Models.Status.Models;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Manage
{
    public class LicenseInformationReportData : BaseReportDataManager, IReportDataModel
    {
        private readonly LicenseInformationReportDataInput _reportDataInput;
        private readonly LicenseInformationReportDataOutput _reportDataOutput;

        bool loadSessions => _reportDataInput.Columns.Any(a => a.Column == TermGroup_LicenseInformationMatrixColumns.LastLogin);

        public LicenseInformationReportData(ParameterObject parameterObject, LicenseInformationReportDataInput reportDataInput) : base(parameterObject)
        {
            _reportDataInput = reportDataInput;
            _reportDataOutput = new LicenseInformationReportDataOutput(reportDataInput);
        }

        public LicenseInformationReportDataOutput CreateOutput(CreateReportResult reportResult)
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

            _reportDataOutput.ResultItems = new List<LicenseInformationItem>();

            var databases = ConfigurationSetupUtil.GetSysCompDbsOfSameType(false);
            var connections = ConfigurationSetupUtil.GetEntityConnectionsForDbsOfTheSameType();
            Dictionary<SysCompDBDTO, string> connectionsDict = databases.ToDictionary(d => d, d => connections.FirstOrDefault(f => f.DataSource == d.Name)?.ConnectionString);
            ConcurrentBag<LicenseInformationItem> items = new ConcurrentBag<LicenseInformationItem>();

            Parallel.ForEach(connectionsDict, db =>
            {
                using (CompEntities entities = new CompEntities(db.Value))
                {
                    Dictionary<int, UserSession> lastLoginPerLicense = new Dictionary<int, UserSession>();
                    var licenses = entities.License.ToList();

                    if (loadSessions)
                    {
                        // in the the table USer there are users (users have a licenseid), in the table usersessions there are sessions. I want to to know the last login per license
                        lastLoginPerLicense = entities.UserSession
                                       .GroupBy(g => g.User.LicenseId)
                                       .Select(s => new { LicenseId = s.Key, LastLogin = s.OrderByDescending(m => m.Login).FirstOrDefault() })
                                       .ToDictionary(d => d.LicenseId, d => d.LastLogin);
                    }

                    foreach (var license in licenses)
                    {
                        LicenseInformationItem item = new LicenseInformationItem()
                        {
                            Database = db.Key.Name,
                            LicenseNr = license.LicenseNr,
                            LicenseName = license.Name,
                        };

                        if (loadSessions && lastLoginPerLicense.TryGetValue(license.LicenseId, out UserSession lastLogin))
                            item.LastLogin = lastLogin.Login;

                        items.Add(item);
                    }
                }
            });

            #endregion

            _reportDataOutput.ResultItems = items.OrderBy(o => o.Database).ThenBy(t => t.LicenseNr).ToList();
            return new ActionResult();
        }
    }

    public class LicenseInformationReportDataReportDataField
    {
        public MatrixColumnSelectionDTO Selection { get; set; }
        public TermGroup_LicenseInformationMatrixColumns Column { get; set; }
        public string ColumnKey { get; set; }

        public int Sort
        {
            get
            {
                return Selection?.Sort ?? 0;
            }
        }

        public LicenseInformationReportDataReportDataField(MatrixColumnSelectionDTO columnSelectionDTO)
        {
            this.Selection = columnSelectionDTO;
            this.ColumnKey = Selection?.Field;
            this.Column = Selection?.Field != null ? EnumUtility.GetValue<TermGroup_LicenseInformationMatrixColumns>(ColumnKey.FirstCharToUpperCase()) : TermGroup_LicenseInformationMatrixColumns.LicenseName;
        }
    }

    public class LicenseInformationReportDataInput
    {
        public CreateReportResult ReportResult { get; set; }
        public List<LicenseInformationReportDataReportDataField> Columns { get; set; }

        public LicenseInformationReportDataInput(CreateReportResult reportResult, List<LicenseInformationReportDataReportDataField> columns)
        {
            this.ReportResult = reportResult;
            this.Columns = columns;
        }
    }

    public class LicenseInformationReportDataOutput : IReportDataOutput
    {
        public ActionResult Result { get; set; }
        public List<LicenseInformationItem> ResultItems { get; set; }
        public LicenseInformationReportDataInput Input { get; set; }

        public LicenseInformationReportDataOutput(LicenseInformationReportDataInput input)
        {
            this.ResultItems = new List<LicenseInformationItem>();
            this.Input = input;
        }
    }
}
