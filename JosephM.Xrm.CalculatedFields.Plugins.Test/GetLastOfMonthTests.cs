using JosephM.Xrm.CalculatedFields.Plugins.Workflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    [TestClass]
    public class GetLastOfMonthTests : CalculatedXrmTest
    {
        [TestMethod]
        public void GetLastOfMonthTestsTest()
        {
            var workflowActivity = CreateWorkflowInstance<GetLastOfMonthInstance>();
            Assert.AreEqual(new DateTime(2020, 12, 31), workflowActivity.GetLastOfMonth(new DateTime(2020, 12, 1)));
            Assert.AreEqual(new DateTime(2020, 12, 31), workflowActivity.GetLastOfMonth(new DateTime(2020, 12, 15)));
            Assert.AreEqual(new DateTime(2020, 12, 31), workflowActivity.GetLastOfMonth(new DateTime(2020, 12, 31)));
        }
    }
}