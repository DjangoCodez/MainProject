using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.SysService.Tests
{
    [TestClass()]
    public class SysCompanyConnectorTests
    {
   //    var x = typeof(System.Data.Entity.SqlServer.SqlProviderServices);

        [TestMethod()]
        public void SaveSysCompanyDTOsTest()
        {
            try
            {
                SettingManager sm = new SettingManager(null);
                SysServiceManager ssm = new SysServiceManager(null);
                if (sm.GetBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.SyncToSysService, 0, 0, 0))
                {
                    var sysCompDbId = ssm.GetSysCompDBId();

                    if (sysCompDbId != 0 && sysCompDbId != null)
                    {
                        CompanyManager cm = new CompanyManager(null);
                        var companies = cm.GetCompanies(false);
                        EdiManager em = new EdiManager(null);
                        List<SysCompanyDTO> sysCompanyDTOs = new List<SysCompanyDTO>();

                        foreach (var company in companies.Where(c => c.State == (int)SoeEntityState.Active))
                        {
                            var dto = ssm.CreateSysCompanyDTO(company.ActorCompanyId, sysCompDbId);
                            sysCompanyDTOs.Add(dto);
                            ssm.SaveSysCompany(dto, 1);
                        }

                       // ActionResult saveResult = ssm.SaveSysCompanies(sysCompanyDTOs);
                    }
                }
            }
            catch (Exception)
            {
                Assert.Fail();
            }          

        }
    }
}