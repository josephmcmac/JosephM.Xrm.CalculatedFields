using JosephM.Xrm.CalculatedFields.Plugins.Workflows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    [TestClass]
    public class AddWeekDaysTests : CalculatedXrmTest
    {
        [TestMethod]
        public void AddWeekDaysTest()
        {
            var workflowActivity = CreateWorkflowInstance<AddWeekDaysInstance>();
            var thursday = new DateTime(2020, 12, 17);
            var friday = thursday.AddDays(1);
            var saturday = friday.AddDays(1);
            var sunday = saturday.AddDays(1);
            var monday = sunday.AddDays(1);
            var tuesday = monday.AddDays(1);

            Assert.AreEqual(thursday, workflowActivity.Calculate(friday, -1));
            Assert.AreEqual(thursday, workflowActivity.Calculate(saturday, -1));
            Assert.AreEqual(thursday, workflowActivity.Calculate(sunday, -1));
            Assert.AreEqual(friday, workflowActivity.Calculate(monday, -1));

            Assert.AreEqual(friday, workflowActivity.Calculate(friday, 0));
            Assert.AreEqual(monday, workflowActivity.Calculate(saturday, 0));
            Assert.AreEqual(monday, workflowActivity.Calculate(sunday, 0));
            Assert.AreEqual(monday, workflowActivity.Calculate(monday, 0));

            Assert.AreEqual(monday, workflowActivity.Calculate(friday, 1));
            Assert.AreEqual(tuesday, workflowActivity.Calculate(saturday, 1));
            Assert.AreEqual(tuesday, workflowActivity.Calculate(sunday, 1));
            Assert.AreEqual(tuesday, workflowActivity.Calculate(monday, 1));

            Assert.AreEqual(monday.AddDays(-7), workflowActivity.Calculate(friday, -4));
            Assert.AreEqual(friday.AddDays(7), workflowActivity.Calculate(monday, 4));

            Assert.AreEqual(monday.AddDays(-7), workflowActivity.Calculate(monday, -5));
            Assert.AreEqual(monday.AddDays(7), workflowActivity.Calculate(monday, 5));
            Assert.AreEqual(friday.AddDays(-7), workflowActivity.Calculate(friday, -5));
            Assert.AreEqual(friday.AddDays(7), workflowActivity.Calculate(friday, 5));
        }
    }
}