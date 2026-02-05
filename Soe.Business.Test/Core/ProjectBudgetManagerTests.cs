using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoftOne.Soe.Business.Core.Tests
{
    [TestClass()]
    public class ProjectBudgetManagerTests : TestBase
    {
        [TestMethod()]
        public void CreateProjectBudgetForecastFromProject()
        {
            int projectId = 0;
            var projectBudgetManager = new ProjectBudgetManager(null);
            var result = projectBudgetManager.CreateForecastFromProject(projectId);
        }

        [TestMethod()]
        public void CreateProjectBudgetForecastFromBudget()
        {
            int projectBudgetId = 2861;
            var projectBudgetManager = new ProjectBudgetManager(null);
            var result = projectBudgetManager.CreateForecastFromProjectBudget(projectBudgetId);
        }

        [TestMethod()]
        public void MigrateProjectBudget()
        {
            int projectBudgetId = 2321;
            var projectBudgetManager = new ProjectBudgetManager(null);
            var result = projectBudgetManager.MigrateBudgetHeadIncludingRows (projectBudgetId);

            Assert.IsTrue(result.Success);
        }

        [TestMethod()]
        public void GetBudgetsToMigrate()
        {
            var projectBudgetManager = new ProjectBudgetManager(null);
            var ids = projectBudgetManager.GetBudgetHeadsToMigrateForCompany(7);

            Assert.IsTrue(ids.Count > 0);
        }
    }
}