using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Controls;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.UserControls
{
    public partial class AccumulatorSetting : ControlBase
    {
        #region Enums

        private enum Target
        {
            Employee = 1,
            EmployeeGroup = 2,
        }

        #endregion

        #region Variables

        private TimeAccumulatorManager tam;
        private TermManager tm;

        private Target target;
        private List<AccumulatorBalanceRuleView> rules;
        private int employeeId;
        private int employeeGroupId;
        private int actorCompanyId;
        private bool initialized;

        #endregion

        #region Init

        public void InitControl(Controls.Form Form1)
        {
            tm = new TermManager(PageBase.ParameterObject);
            tam = new TimeAccumulatorManager(PageBase.ParameterObject);

            this.SoeForm = Form1;
        }

        public void InitEmployee(Controls.Form Form1, int actorCompanyId, int employeeId)
        {
            this.target = Target.Employee;
            this.actorCompanyId = actorCompanyId;
            this.employeeId = employeeId;

            InitControl(Form1);
            initialized = true;
        }

        public void InitEmployeeGroup(Controls.Form Form1, int actorCompanyId, int employeeGroupId)
        {
            this.target = Target.EmployeeGroup;
            this.actorCompanyId = actorCompanyId;
            this.employeeGroupId = employeeGroupId;

            InitControl(Form1);
            initialized = true;
        }

        #endregion

        #region Render

        protected override void Render(HtmlTextWriter writer)
        {
            if (!initialized)
                return;

            #region Populate

            switch (target)
            {
                case Target.Employee:
                    rules = tam.GetAccumulatorBalanceRulesForEmployee(actorCompanyId, employeeId);
                    break;
                case Target.EmployeeGroup:
                    rules = tam.GetAccumulatorBalanceRulesForEmployeeGroup(actorCompanyId, employeeGroupId);
                    break;
            }

            if (rules.Any())
            {
                Dictionary<int, string> timePeriods = base.PageBase.GetGrpText(TermGroup.AccumulatorTimePeriodType, addEmptyRow: true);

                HtmlGenericControl headFieldset = new HtmlGenericControl("fieldset");
                HtmlGenericControl headLegend = new HtmlGenericControl("legend")
                {
                    InnerHtml = PageBase.GetText(4518, "Saldoregler"),
                };
                headFieldset.Controls.Add(headLegend);

                foreach (var rule in rules)
                {
                    #region Rule

                    HtmlGenericControl childFieldset = new HtmlGenericControl("fieldset");
                    HtmlGenericControl childlegend = new HtmlGenericControl("legend")
                    {
                        InnerHtml = rule.Name,
                    };
                    childFieldset.Controls.Add(childlegend);

                    Table table = new Table();
                    TableRow row = null;
                    TableCell cell1 = null;
                    TableCell cell2 = null;

                    #region TimePeriod

                    row = new TableRow();

                    #region Cell 1

                    cell1 = new TableCell();

                    SelectEntry timePeriod = new SelectEntry()
                    {
                        ID = "Period_" + rule.TimeAccumulatorId,
                        TermID = 4516,
                        DefaultTerm = "Tidsperiod",
                        FitInTable = true,
                    };
                    timePeriod.ConnectDataSource(timePeriods);

                    if (rule.Type > 0 && timePeriods.Any())
                    {
                        KeyValuePair<int, string> pair = timePeriods.FirstOrDefault(i => i.Key == rule.Type);
                        timePeriod.Value = pair.Key.ToString();
                    }

                    cell1.Controls.Add(timePeriod);
                    row.Cells.Add(cell1);

                    #endregion

                    #region Cell 2

                    cell2 = new TableCell();

                    row.Cells.Add(cell2);

                    #endregion

                    table.Rows.Add(row);

                    #endregion

                    #region Min

                    row = new TableRow();

                    #region Cell 1

                    cell1 = new TableCell();

                    TextEntry minBalance = new TextEntry()
                    {
                        ID = "MinBalance_" + rule.TimeAccumulatorId,
                        TermID = 4515,
                        DefaultTerm = "Min saldo",
                        FitInTable = true,
                        OnChange = "formatTimeEmptyIfPossible(this);",
                        OnFocus = "this.select();",
                    };

                    if (rule.MinMinutes != null)
                        minBalance.Value = GetClockFromMinutes(rule.MinMinutes.Value);

                    cell1.Controls.Add(minBalance);
                    row.Cells.Add(cell1);

                    #endregion

                    #region Cell 2

                    //cell2 = new TableCell();

                    //SelectEntry minTimeCode = new SelectEntry()
                    //{
                    //    ID = "MinTimeCode_" + rule.TimeAccumulatorId,
                    //    TermID = 4519,
                    //    DefaultTerm = "Ger tidkod",
                    //    FitInTable = true,
                    //};
                    //minTimeCode.ConnectDataSource(timeCodesDict);

                    //if (rule.MinTimeCodeId.HasValue)
                    //    minTimeCode.Value = rule.MinTimeCodeId.Value.ToString();

                    //cell2.Controls.Add(minTimeCode);
                    //row.Cells.Add(cell2);

                    #endregion

                    table.Rows.Add(row);

                    #endregion

                    #region Max

                    row = new TableRow();

                    #region Cell 1

                    cell1 = new TableCell();

                    TextEntry maxBalance = new TextEntry()
                    {
                        ID = "MaxBalance_" + rule.TimeAccumulatorId,
                        TermID = 4514,
                        DefaultTerm = "Max saldo",
                        FitInTable = true,
                        OnChange = "formatTimeEmptyIfPossible(this);",
                        OnFocus = "this.select();",
                    };

                    if (rule.MaxMinutes != null)
                        maxBalance.Value = GetClockFromMinutes(rule.MaxMinutes.Value);

                    cell1.Controls.Add(maxBalance);
                    row.Cells.Add(cell1);

                    #endregion

                    #region Cell 2

                    //cell2 = new TableCell();

                    //SelectEntry maxTimeCode = new SelectEntry()
                    //{
                    //    ID = "MaxTimeCode_" + rule.TimeAccumulatorId,
                    //    TermID = 4519,
                    //    DefaultTerm = "Ger tidkod",
                    //    FitInTable = true,
                    //};
                    //maxTimeCode.ConnectDataSource(timeCodesDict);

                    //if (rule.MaxTimeCodeId.HasValue)
                    //    maxTimeCode.Value = rule.MaxTimeCodeId.Value.ToString();

                    //cell2.Controls.Add(maxTimeCode);
                    //row.Cells.Add(cell2);

                    #endregion

                    table.Rows.Add(row);

                    #endregion

                    #region ShowOnPayrollSlip

                    #region Cell1

                    row = new TableRow();
                    cell1 = new TableCell();

                    CheckBoxEntry showOnPayrollSlip = new CheckBoxEntry()
                    {
                        ID = "ShowOnPayrollSlip_" + rule.TimeAccumulatorId,
                        TermID = 4545,
                        DefaultTerm = "Visa på lönespec",
                        FitInTable = true,
                    };

                    showOnPayrollSlip.Value = rule.ShowOnPayrollSlip == 1 ? Boolean.TrueString : Boolean.FalseString;

                    cell1.Controls.Add(showOnPayrollSlip);
                    row.Cells.Add(cell1);

                    #endregion

                    #region Cell2

                    cell2 = new TableCell();
                    row.Cells.Add(cell2);

                    #endregion

                    table.Rows.Add(row);

                    #endregion

                    childFieldset.Controls.Add(table);
                    headFieldset.Controls.Add(childFieldset);

                    #endregion
                }

                headFieldset.RenderControl(writer);
            }

            #endregion
        }

        #endregion

        #region Action-methods

        public ActionResult SaveForEmployeeGroup(NameValueCollection F, int employeeGroupId)
        {
            ActionResult result = new ActionResult(false);

            if (employeeGroupId == 0)
                return result;

            if (tam == null)
                tam = new TimeAccumulatorManager(PageBase.ParameterObject);

            List<AccumulatorSaveItem> items = new List<AccumulatorSaveItem>();

            if (rules == null)
                rules = tam.GetAccumulatorBalanceRulesForEmployeeGroup(PageBase.SoeCompany.ActorCompanyId, employeeGroupId);

            foreach (var rule in rules)
            {
                #region Init

                //Period
                int type = 0;
                int.TryParse(F["Period_" + rule.TimeAccumulatorId], out type);

                //Min minutes
                int? minMinutes = null;
                if (!string.IsNullOrEmpty(F["MinBalance_" + rule.TimeAccumulatorId]))
                    minMinutes = GetTimeInMinutes(F["MinBalance_" + rule.TimeAccumulatorId]);

                //Min TimeCode
                int? minTimeCodeId = null;
                if (!string.IsNullOrEmpty(F["MinTimeCode_" + rule.TimeAccumulatorId]))
                {
                    int tmpMinTimeCodeId;
                    int.TryParse(F["MinTimeCode_" + rule.TimeAccumulatorId], out tmpMinTimeCodeId);
                    minTimeCodeId = tmpMinTimeCodeId;
                }

                //Max minutes
                int? maxMinutes = null;
                if (!string.IsNullOrEmpty(F["MaxBalance_" + rule.TimeAccumulatorId]))
                    maxMinutes = GetTimeInMinutes(F["MaxBalance_" + rule.TimeAccumulatorId]);

                //Max TimeCode
                int? maxTimeCodeId = null;
                if (!string.IsNullOrEmpty(F["MaxTimeCode_" + rule.TimeAccumulatorId]))
                {
                    int tmpMaxTimeCodeId;
                    int.TryParse(F["MaxTimeCode_" + rule.TimeAccumulatorId], out tmpMaxTimeCodeId);
                    maxTimeCodeId = tmpMaxTimeCodeId;
                }

                bool showOnPayrollSlip = false;
                Boolean.TryParse(F["ShowOnPayrollSlip_" + rule.TimeAccumulatorId], out showOnPayrollSlip);

                #endregion

                AccumulatorSaveItem item = new AccumulatorSaveItem()
                {
                    TimeAccumulatorId = rule.TimeAccumulatorId,
                    Type = type,
                    MinMinutes = minMinutes,
                    MinTimeCodeId = minTimeCodeId,
                    MaxMinutes = maxMinutes,
                    MaxTimeCodeId = maxTimeCodeId,
                    ShowOnPayrollSlip = showOnPayrollSlip
                };

                items.Add(item);
            }

            return tam.SaveEmployeeGroupAccumulatorSettings(items, employeeGroupId);
        }

        #endregion

        #region Help-methods

        private int? GetTimeInMinutes(string time)
        {
            int minutes = 0;
            if (!int.TryParse(time, out minutes))
            {
                string[] parts = time.Split(":".ToCharArray());
                if (parts.Length == 2)
                {
                    int hours = 0;
                    int.TryParse(parts[0], out hours);
                    int.TryParse(parts[1], out minutes);
                    minutes += (hours * 60);
                }
            }

            return minutes;
        }

        private string GetClockFromMinutes(int minutes)
        {
            bool isNegative = minutes < 0;
            minutes = Math.Abs(minutes);

            int hours = minutes / 60;
            int min = minutes - (hours * 60);
            string clock = string.Empty;
            if (hours < 10)
                clock = "0";
            clock += hours.ToString() + ":";
            if (min < 10)
                clock += "0";
            clock += min.ToString();

            if (isNegative)
                clock = '-' + clock;

            return clock;
        }

        #endregion
    }
}