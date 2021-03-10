using IdleBusiness.Data;
using IdleBusiness.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IdleBusinessTests.IntegrationTests
{
    [TestClass]
    public class CronHelperTests
    {
        // This is not really a test, but it's really hard to test changes from the hangfire server
        // since you don't have control of whether it queues job on the online server, or your local machine
        [TestMethod]
        public void RemoveEspionageInvestments_RemoveOldInvestments()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var context = ApplicationDbContextFactory.CreateDbContext(config);

            var helper = new CronHelpers(context);

            try
            {
                helper.RemoveEspionageInvestments();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void AwardInvestmentProfits_RemoveOldInvestments()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var context = ApplicationDbContextFactory.CreateDbContext(config);

            var helper = new CronHelpers(context);

            try
            {
                helper.AwardInvestmentProfits();
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
