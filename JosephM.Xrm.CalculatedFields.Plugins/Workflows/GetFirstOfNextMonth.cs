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
    public class GetFirstOfNextMonth : XrmWorkflowActivityRegistration
    {
        [Output("First Of Next Month")]
        public OutArgument<DateTime> FirstOfNextMonth { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new GetFirstOfNextMonthInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class GetFirstOfNextMonthInstance
        : CalculatedWorkflowActivity<GetFirstOfNextMonth>
    {
        protected override void Execute()
        {
            var firstOfNextMonth = GetFirstOfNextMonth(LocalisationService.TargetToday);
            ActivityThisType.FirstOfNextMonth.Set(ExecutionContext, firstOfNextMonth);
        }

        public DateTime GetFirstOfNextMonth(DateTime dayZero)
        {
            var aDayAtaTime = dayZero;
            aDayAtaTime = aDayAtaTime.AddDays(1);
            while (aDayAtaTime.Day != 1)
            {
                aDayAtaTime = aDayAtaTime.AddDays(1);
            }
            var firstOfNextMonth = aDayAtaTime;
            return firstOfNextMonth;
        }
    }
}
