using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class LoginManagerTests
    {
        [TestMethod()]
        public void ValidateLoginSoftOneIdTest()
        {
            LoginManager lm = new LoginManager(null);
           var user =  lm.ValidateLoginSoftOneId("101", "txl", "Txl123");
        }
    }
}