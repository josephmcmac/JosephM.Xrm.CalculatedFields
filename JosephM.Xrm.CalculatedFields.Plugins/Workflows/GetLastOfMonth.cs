using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace JosephM.Xrm.CalculatedFields.Plugins.Workflows
{
    /// <summary>
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class GetLastOfMonth : XrmWorkflowActivityRegistration
    {
        [Output("Last Of Month")]
        public OutArgument<DateTime> LastOfMonth { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new GetLastOfMonthInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class GetLastOfMonthInstance
        : CalculatedWorkflowActivity<GetLastOfMonth>
    {
        protected override void Execute()
        {
            var firstOfNextMonth = GetLastOfMonth(LocalisationService.TargetToday);
            ActivityThisType.LastOfMonth.Set(ExecutionContext, firstOfNextMonth);
        }

        public DateTime GetLastOfMonth(DateTime dayZero)
        {
            var aDayAtaTime = dayZero;
            if(aDayAtaTime.Day == 1)
            {
                aDayAtaTime = aDayAtaTime.AddDays(1);
            }
            while (aDayAtaTime.Day != 1)
            {
                aDayAtaTime = aDayAtaTime.AddDays(1);
            }
            var firstOfNextMonth = aDayAtaTime;
            return firstOfNextMonth.AddDays(-1);
        }
    }
}
