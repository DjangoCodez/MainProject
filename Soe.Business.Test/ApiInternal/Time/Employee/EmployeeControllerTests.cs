using Microsoft.VisualStudio.TestTools.UnitTesting;
using Soe.Api.Internal.Controllers.WebApiExternal.Economy;
using Soe.Api.Internal.Controllers.WebApiExternal.Employee;
using SoftOne.Soe.Business.Core.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SoftOne.Soe.Business.Core;
using Soe.Api.Internal;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core.SysService;

using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using System.Threading;
using SoftOne.Soe.Data;
using System.Web.Http.Controllers;
using SoftOne.Soe.Business.Util.API.Models;
using System.Net.Http;
using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;

namespace Soe.Business.Test.ApiInternal.Time.Employee
{
    [TestClass]
    public class EmployeeControllerTests : TestBase
    {
        [TestMethod]
        public void TestGetEmployees()
        {
            #region setup
            ConfigurationSetupUtil.Init();
            var actorCompanyId = 90;
            var userId = 74;
            var licenseId = 3;
            var roleId = 21;
            var parameterObject = GetParameterObject(actorCompanyId, userId, roleId);
            var itoken = Guid.NewGuid().ToString();
            var connectUtil = new ConnectUtil(parameterObject);
            var settingManager = new SettingManager(parameterObject);
            var sysServiceManager = new SysServiceManager(parameterObject);
            var httpConfiguration = new HttpConfiguration();

            #endregion

            #region Get Api Keys
            var companyApiKey = settingManager.GetStringSetting(SoftOne.Soe.Common.Util.SettingMainType.Company, (int)CompanySettingType.CompanyAPIKey, userId, actorCompanyId, licenseId);
            var connectApiKey = SysMiscConnector.GetConnectApiKey().FirstOrDefault().Value;
            #endregion


            EmployeeController controller = new EmployeeController(new WebApiInternalParamObject() { Token = itoken });
            controller.Request = new HttpRequestMessage();
            controller.Configuration = httpConfiguration;

            #region Fake UserSession
            var loginToken = connectUtil.CreateToken(companyApiKey, connectApiKey, false, ref userId);
            using (CompEntities entities = new CompEntities())
            {
                var user = entities.User.FirstOrDefault(x => x.UserId == userId);

                UserSession userSession = new UserSession()
                {
                    Login = DateTime.Now,
                    RemoteLogin = false,
                    MobileLogin = false,
                    Token = loginToken,
                    User = user
                };
                entities.UserSession.AddObject(userSession);
                entities.SaveChanges();
            }
            #endregion

            var employeesResult = controller.GetEmployees(Guid.Parse(companyApiKey), Guid.Parse(connectApiKey), loginToken);
            var response = employeesResult.ExecuteAsync(CancellationToken.None).Result;
            var employees = (List<EmployeeIODTO>)response.Content.ReadAsAsync(typeof(List<EmployeeIODTO>)).Result;

            #region Assert

            Assert.IsTrue(employees.Count > 0);

            #endregion

        }
    }
}
