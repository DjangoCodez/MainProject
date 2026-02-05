using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Util;

namespace SoftOne.Soe.Web.soe.billing.order.status
{
    public partial class _default : PageBase
    {
        #region Variables
        
        private AccountManager am;
        protected SoeModule TargetSoeModule = SoeModule.Billing;
        protected Feature FeatureEdit = Feature.Billing_Order_Status;
        public int employeeId = 0;
        public int accountYearId;
        public bool accountYearIsOpen;
        public string userName;
        public int invoiceId = -1;
        public string invoiceNr;
        public int customerId;
        public int projectId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Order_Status;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init
            
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            if (int.TryParse(QS["recordId"], out int recordId))
                ExportUtil.DownladDataStorageFromDb(ParameterObject.ActorCompanyId, recordId);

            EmployeeManager em = new EmployeeManager(ParameterObject);
            Employee employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
            if (employee != null)
                employeeId = employee.EmployeeId;

            userName = base.SoeUser?.Name ?? string.Empty;

            #endregion            

            #region Legacy Navigation

            //Int32.TryParse(QS["invoiceId"], out invoiceId);
            invoiceId = string.IsNullOrEmpty(QS["invoiceId"]) ? -1 : Int32.Parse(QS["invoiceId"]);
            invoiceNr = QS["invoiceNr"] ?? "-1";
            customerId = string.IsNullOrEmpty(QS["customerId"]) ? 0 : Int32.Parse(QS["customerId"]);
            projectId = string.IsNullOrEmpty(QS["projectId"]) ? 0 : Int32.Parse(QS["projectId"]);

            #endregion
        }
    }
}
