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
    public class AddWeekDays : XrmWorkflowActivityRegistration
    {
        [Input("Date Add To")]
        public InArgument<DateTime> DateAddTo { get; set; }

        [Input("Week Days To Add")]
        public InArgument<int> WeekDaysToAdd { get; set; }

        [Output("Result")]
        public OutArgument<DateTime> Result { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new AddWeekDaysInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class AddWeekDaysInstance
        : CalculatedWorkflowActivity<AddWeekDays>
    {
        protected override void Execute()
        {
            var dateAddTo = ActivityThisType.DateAddTo.Get(ExecutionContext);
            var daysToAdd = ActivityThisType.WeekDaysToAdd.Get(ExecutionContext);
            var result = Calculate(dateAddTo, daysToAdd);
            ActivityThisType.Result.Set(ExecutionContext, result);
        }

        public object Calculate(DateTime dateAddTo, int daysToAdd)
        {
            var wasUtc = dateAddTo.Kind == DateTimeKind.Utc;
            var local = wasUtc
                ? LocalisationService.ConvertToTargetTime(dateAddTo)
                : dateAddTo;
            var unit = daysToAdd < 0 ? -1 : 1;
            var daysLeft = Math.Abs(daysToAdd);

            while(local.DayOfWeek == DayOfWeek.Saturday || local.DayOfWeek == DayOfWeek.Sunday)
            {
                local = local.AddDays(unit);
            }

            while(daysLeft > 0)
            {
                local = local.AddDays(unit);
                if(local.DayOfWeek != DayOfWeek.Saturday && local.DayOfWeek != DayOfWeek.Sunday)
                {
                    daysLeft--;
                }
            }
            return wasUtc
                ? LocalisationService.ConvertTargetToUtc(local)
                : local;
        }
    }
}
