using SoftOne.Soe.Business.Util.API.AvionData;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core
{
    public class ExternalCompanySearchManager : ManagerBase
    {
        #region Variables

        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly PRHCompanySearchManager _prhComopanySearchManager;

        #endregion

        #region Ctor
        public ExternalCompanySearchManager(ParameterObject parameterObject) : base(parameterObject)
        {
            this._prhComopanySearchManager = new PRHCompanySearchManager(parameterObject);
        }
        #endregion

        public List<ExternalCompanyResultDTO> GetExternalComanyResultDTOs(ExternalCompanySearchProvider provider, ExternalCompanyFilterDTO filter)
        {
            List<ExternalCompanyResultDTO> result = new List<ExternalCompanyResultDTO>();
            switch (provider)
            {
                case ExternalCompanySearchProvider.PRH:
                    result = _prhComopanySearchManager.GetExternalComanyResultDTOsFromPrh(filter);
                    break;
            }
            return result;
        }
    }
}
