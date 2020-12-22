using JosephM.Xrm.CalculatedFields.Plugins.Workflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    [TestClass]
    public class GetFirstOfNextMonthTests : CalculatedXrmTest
    {
        [TestMethod]
        public void GetFirstOfMonthTestsTest()
        {
            var workflowActivity = CreateWorkflowInstance<GetFirstOfNextMonthInstance>();
            Assert.AreEqual(new DateTime(2021, 1, 1), workflowActivity.GetFirstOfNextMonth(new DateTime(2020, 12, 1)));
            Assert.AreEqual(new DateTime(2021, 1, 1), workflowActivity.GetFirstOfNextMonth(new DateTime(2020, 12, 15)));
            Assert.AreEqual(new DateTime(2021, 1, 1), workflowActivity.GetFirstOfNextMonth(new DateTime(2020, 12, 31)));
        }
    }
}