using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Soe.Business.Test.Util.WebApiInternal
{
    [TestClass]
    public class ConnectorBaseTests
    {
        //[TestMethod]
        public void CreateRequest_generates_a_request_with_a_authorization_header()
        {
            var req = SoftOne.Soe.Business.Util.WebApiInternal.ConnectorBase.CreateRequest("xxx", RestSharp.Method.Get);

            var authHeader = req.Parameters.FirstOrDefault(x => x.Name == "Authorization");

            Assert.IsNotNull(authHeader);
            Assert.AreEqual(RestSharp.ParameterType.HttpHeader, authHeader.Type);
            Assert.IsNotNull(authHeader.Value);
            Assert.IsTrue(((string)authHeader.Value).StartsWith("Bearer "));
        }

        //[TestMethod]
        public void CreateRequest_uses_cached_access_token()
        {
            var req = SoftOne.Soe.Business.Util.WebApiInternal.ConnectorBase.CreateRequest("xxx", RestSharp.Method.Get);
            var req2 = SoftOne.Soe.Business.Util.WebApiInternal.ConnectorBase.CreateRequest("xxx", RestSharp.Method.Get);

            var authHeader = req.Parameters.FirstOrDefault(x => x.Name == "Authorization");
            var authHeader2 = req2.Parameters.FirstOrDefault(x => x.Name == "Authorization");
            
            Assert.AreEqual(authHeader.Value, authHeader2.Value);
        }

        [TestMethod()]
        [Ignore] //not implemented
        public void DummyTest()
        {
            Assert.IsTrue(true);
        }
    }
}
