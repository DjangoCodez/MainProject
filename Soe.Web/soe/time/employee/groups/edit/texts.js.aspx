<%@ Page Language="C#" AutoEventWireup="true" Inherits="SoftOne.Soe.Web.PageBase" %>
<%@ Import Namespace="SoftOne.Soe.Business.Core" %>
<%@ Import Namespace="SoftOne.Soe.Common.Util" %>
<%@ Import Namespace="SoftOne.Soe.Data" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "text/javascript";
        
        AccountManager am = new AccountManager(ParameterObject);
        SettingManager sm = new SettingManager(ParameterObject);
                
        AccountDim dim = am.GetAccountDimStd(SoeCompany.ActorCompanyId);        
        if (dim != null)
            Response.Write("var stdDimID=" + dim.AccountDimId + ";");
        else
            Response.Write("var stdDimID='';");
        Response.Write("eval('accountSearch.init(true)');");

        string defaultCostAccountNr = am.GetAccountNr(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupCost, UserId, SoeCompany.ActorCompanyId, 0), true);
        string defaultIncomeAccountNr = am.GetAccountNr(SoeCompany.ActorCompanyId, sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountEmployeeGroupIncome, UserId, SoeCompany.ActorCompanyId, 0), true);

        Response.Write("var defaultCostAccountNr = eval('" + defaultCostAccountNr + "');");
        Response.Write("var defaultIncomeAccountNr = eval('" + defaultIncomeAccountNr + "');");
        Response.Write("fixInfoLabel('CostAccount',accountSearch.displayDefault);");
        Response.Write("fixInfoLabel('IncomeAccount',accountSearch.displayDefault);");
    }
</script>

