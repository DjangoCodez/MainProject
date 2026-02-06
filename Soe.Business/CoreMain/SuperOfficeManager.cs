using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Core
{
    public class SuperOfficeManager : ManagerBase
    {
        #region Variables

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //private string customerlog;

        #endregion

        #region Ctor

        public SuperOfficeManager(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        public ActionResult Sync(int actorCompanyId, int? paramCustomerId, string countryName, int? daysBack)
        {
            paramCustomerId = 0;
            daysBack = 0;
            return new ActionResult() { IntegerValue = actorCompanyId, StringValue = countryName, IntegerValue2 = paramCustomerId.Value, Value = daysBack };
        }
    }
}