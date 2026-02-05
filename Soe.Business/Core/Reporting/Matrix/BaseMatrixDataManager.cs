using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix
{
    public class BaseMatrixDataManager : BaseReportDataManager
    {
        public BaseMatrixDataManager(ParameterObject parameterObject) : base(parameterObject) { }

        protected List<TermGroup_InsightChartTypes> BarChartTypes
        {
            get
            {
                return new List<TermGroup_InsightChartTypes>
                {
                    TermGroup_InsightChartTypes.Column,
                    TermGroup_InsightChartTypes.Bar
                };
            }
        }

        protected List<TermGroup_InsightChartTypes> PieChartTypes
        {
            get
            {
                return new List<TermGroup_InsightChartTypes>
                {
                    TermGroup_InsightChartTypes.Pie,
                    TermGroup_InsightChartTypes.Doughnut
                };
            }
        }

        protected List<TermGroup_InsightChartTypes> LineChartTypes
        {
            get
            {
                return new List<TermGroup_InsightChartTypes>
                {
                    TermGroup_InsightChartTypes.Line
                };
            }
        }

        protected List<TermGroup_InsightChartTypes> AreaChartTypes
        {
            get
            {
                return new List<TermGroup_InsightChartTypes>
                {
                    TermGroup_InsightChartTypes.Area
                };
            }
        }

        protected List<TermGroup_InsightChartTypes> TreemapChartTypes
        {
            get
            {
                return new List<TermGroup_InsightChartTypes>
                {
                    TermGroup_InsightChartTypes.Treemap
                };
            }
        }

        protected List<TermGroup_InsightChartTypes> SimpleOneColumnChartTypes
        {
            get
            {
                List<TermGroup_InsightChartTypes> types = new List<TermGroup_InsightChartTypes>();
                types.AddRange(PieChartTypes);
                types.AddRange(BarChartTypes);
                return types;
            }
        }

        protected int GetAccountLevel(int actorCompanyId)
        {
         
            int defaultEmployeeAccountDimEmployeeAccountDimId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.DefaultEmployeeAccountDimEmployee, 0, actorCompanyId, 0);
            List<Account> companyAccounts = AccountManager.GetAccountsByCompany(actorCompanyId, loadAccount: true, loadAccountDim: true);
            List<AccountDim> accountDims = companyAccounts.Select(a => a.AccountDim).DistinctBy(a => a.AccountDimId).ToList();
            if (accountDims.Any(a => a.Level > 0))
                accountDims.CalculateLevels();
            return accountDims.OrderBy(o => o.Level).ThenBy(o => o.AccountDimNr).FirstOrDefault(d => d.AccountDimId == defaultEmployeeAccountDimEmployeeAccountDimId)?.AccountDimNr ?? 0;
           
        }
    }
}
