using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.API.AvionData;
using SoftOne.Soe.Business.Util.API.AvionData.Models;

namespace Soe.Business.Test.Util.API
{
    [TestClass()]
    public class AvionDataConnectorTests
    {
        AvionConnector Connector;
        [TestInitialize]
        public void SetUp()
        {
            Connector = new AvionConnector();
        }
        [TestMethod]
        public void SearchAvionCompaniesTest()
        {
            // Arrange
            
            var searchCriteria = new CompanySearchFilter
            {
                Name = "Avion"
            };
            // Act
            var response = Connector.SearchCompanies(searchCriteria);
            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
            Assert.IsTrue(response.Result.Count < 100);
        }

        [TestMethod]
        public void SearchAvionCompaniesTest2()
        {
            // Arrange
            var searchCriteria = new CompanySearchFilter
            {
                Name = "Finn-"
            };
            // Act
            var response = Connector.SearchCompanies(searchCriteria);
            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess);
            Assert.IsTrue(response.Result.Count > 100);
        }

        [TestCleanup]
        public void CleanUp()
        {
            Connector = null;
        }
    }
}
