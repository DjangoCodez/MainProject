using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using SoftOne.Soe.Business.Core.Template.Models.Time;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.WebApiInternal.Template
{
    public class TimeTemplateConnector : ConnectorBase
    {

        public List<DayTypeCopyItem> GetDayTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<DayTypeCopyItem> dayTypeCopyItems = new List<DayTypeCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/DayTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        dayTypeCopyItems = JsonConvert.DeserializeObject<List<DayTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return dayTypeCopyItems;
        }
        public List<TimeHalfDayCopyItem> GetTimeHalfDayCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<TimeHalfDayCopyItem> timeHalfDayCopyItems = new List<TimeHalfDayCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeHalfDayCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        timeHalfDayCopyItems = JsonConvert.DeserializeObject<List<TimeHalfDayCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return timeHalfDayCopyItems;
        }

        public List<HolidayCopyItem> GetHolidayCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<HolidayCopyItem> holidayCopyItems = new List<HolidayCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/HolidayCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        holidayCopyItems = JsonConvert.DeserializeObject<List<HolidayCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return holidayCopyItems;
        }

        public List<TimePeriodHeadCopyItem> GetTimePeriodHeadCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<TimePeriodHeadCopyItem> timePeriodHeadCopyItems = new List<TimePeriodHeadCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimePeriodHeadCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        timePeriodHeadCopyItems = JsonConvert.DeserializeObject<List<TimePeriodHeadCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return timePeriodHeadCopyItems;
        }

        public List<PositionCopyItem> GetPositionCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<PositionCopyItem> positionCopyItems = new List<PositionCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/PositionCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        positionCopyItems = JsonConvert.DeserializeObject<List<PositionCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return positionCopyItems;
        }

        public List<PayrollPriceTypeCopyItem> GetPayrollPriceTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<PayrollPriceTypeCopyItem> payrollPriceTypeCopyItems = new List<PayrollPriceTypeCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/PayrollPriceTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        payrollPriceTypeCopyItems = JsonConvert.DeserializeObject<List<PayrollPriceTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return payrollPriceTypeCopyItems;
        }


        public List<PayrollPriceFormulaCopyItem> GetPayrollPriceFormulaCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<PayrollPriceFormulaCopyItem> payrollPriceFormulaCopyItems = new List<PayrollPriceFormulaCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/PayrollPriceFormulaCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        payrollPriceFormulaCopyItems = JsonConvert.DeserializeObject<List<PayrollPriceFormulaCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return payrollPriceFormulaCopyItems;
        }

        public List<VacationGroupCopyItem> GetVacationGroupCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<VacationGroupCopyItem> vacationGroupCopyItems = new List<VacationGroupCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/VacationGroupCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        vacationGroupCopyItems = JsonConvert.DeserializeObject<List<VacationGroupCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return vacationGroupCopyItems;
        }

        public List<TimeScheduleTypeCopyItem> GetTimeScheduleTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<TimeScheduleTypeCopyItem> timeScheduleTypeCopyItems = new List<TimeScheduleTypeCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeScheduleTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        timeScheduleTypeCopyItems = JsonConvert.DeserializeObject<List<TimeScheduleTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return timeScheduleTypeCopyItems;
        }

        public List<ShiftTypeCopyItem> GetShiftTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<ShiftTypeCopyItem> shiftTypeCopyItems = new List<ShiftTypeCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/ShiftTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        shiftTypeCopyItems = JsonConvert.DeserializeObject<List<ShiftTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return shiftTypeCopyItems;
        }

        public List<SkillCopyItem> GetSkillCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<SkillCopyItem> skillCopyItems = new List<SkillCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/SkillCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        skillCopyItems = JsonConvert.DeserializeObject<List<SkillCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return skillCopyItems;
        }

        public List<ScheduleCycleCopyItem> GetScheduleCycleCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<ScheduleCycleCopyItem> scheduleCycleCopyItems = new List<ScheduleCycleCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/ScheduleCycleCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        scheduleCycleCopyItems = JsonConvert.DeserializeObject<List<ScheduleCycleCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return scheduleCycleCopyItems;
        }

        public List<FollowUpTypeCopyItem> GetFollowUpTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<FollowUpTypeCopyItem> followUpTypeCopyItems = new List<FollowUpTypeCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/FollowUpTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        followUpTypeCopyItems = JsonConvert.DeserializeObject<List<FollowUpTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return followUpTypeCopyItems;
        }

        public List<InvoiceProductCopyItem> GetInvoiceProductCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<InvoiceProductCopyItem> copyItems = new List<InvoiceProductCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/InvoiceProductCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<InvoiceProductCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<PayrollProductCopyItem> GetPayrollProductCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<PayrollProductCopyItem> copyItems = new List<PayrollProductCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/PayrollProductCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<PayrollProductCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<PayrollGroupCopyItem> GetPayrollGroupCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<PayrollGroupCopyItem> copyItems = new List<PayrollGroupCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/PayrollGroupCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<PayrollGroupCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<TimeCodeCopyItem> GetTimeCodeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeCodeCopyItem> copyItems = new List<TimeCodeCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeCodeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeCodeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<TimeBreakTemplateCopyItem> GetTimeBreakTemplateCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeBreakTemplateCopyItem> copyItems = new List<TimeBreakTemplateCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeBreakTemplateCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeBreakTemplateCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }
        public List<TimeCodeBreakGroupCopyItem> GetTimeCodeBreakGroupCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeCodeBreakGroupCopyItem> copyItems = new List<TimeCodeBreakGroupCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeCodeBreakGroupCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeCodeBreakGroupCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }
        public List<TimeCodeRankingGroupCopyItem> GetTimeCodeRankingGroupCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeCodeRankingGroupCopyItem> copyItems = new List<TimeCodeRankingGroupCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeCodeRankingGroupCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeCodeRankingGroupCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }
        public List<TimeDeviationCauseCopyItem> GetTimeDeviationCauseCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeDeviationCauseCopyItem> copyItems = new List<TimeDeviationCauseCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeDeviationCauseCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeDeviationCauseCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<EmploymentTypeCopyItem> GetEmploymentTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<EmploymentTypeCopyItem> copyItems = new List<EmploymentTypeCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/EmploymentTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<EmploymentTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<EmployeeGroupCopyItem> GetEmployeeGroupCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<EmployeeGroupCopyItem> copyItems = new List<EmployeeGroupCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/EmployeeGroupCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<EmployeeGroupCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }


        public List<TimeAccumulatorCopyItem> GetTimeAccumulatorCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeAccumulatorCopyItem> copyItems = new List<TimeAccumulatorCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeAccumulatorCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeAccumulatorCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<TimeRuleCopyItem> GetTimeRuleCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeRuleCopyItem> copyItems = new List<TimeRuleCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeRuleCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeRuleCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<TimeAbsenceRuleCopyItem> GetTimeAbsenceRuleCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeAbsenceRuleCopyItem> copyItems = new List<TimeAbsenceRuleCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeAbsenceRuleCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeAbsenceRuleCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<TimeAttestRuleCopyItem> GetTimeAttestRuleCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<TimeAttestRuleCopyItem> copyItems = new List<TimeAttestRuleCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/TimeAttestRuleCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<TimeAttestRuleCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<EmployeeCollectiveAgreementCopyItem> GetEmployeeCollectiveAgreementCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<EmployeeCollectiveAgreementCopyItem> copyItems = new List<EmployeeCollectiveAgreementCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/EmployeeCollectiveAgreementCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<EmployeeCollectiveAgreementCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }

        public List<EmployeeTemplateCopyItem> GetEmployeeTemplateCopyItems(int sysCompDbId, int actorCompanyId)
        {
            List<EmployeeTemplateCopyItem> copyItems = new List<EmployeeTemplateCopyItem>();

            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Time/EmployeeTemplateCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        copyItems = JsonConvert.DeserializeObject<List<EmployeeTemplateCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return copyItems;
        }
    }
}
