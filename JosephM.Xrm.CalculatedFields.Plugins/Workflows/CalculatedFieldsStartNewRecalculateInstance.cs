using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;

namespace JosephM.Xrm.CalculatedFields.Plugins.Workflows
{
    /// <summary>
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class CalculatedFieldsStartNewRecalculateInstance : XrmWorkflowActivityRegistration
    {
        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new CalculatedFieldsStartNewRecalculateInstanceInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class CalculatedFieldsStartNewRecalculateInstanceInstance
        : CalculatedWorkflowActivity<CalculatedFieldsStartNewRecalculateInstance>
    {
        protected override void Execute()
        {
            XrmService.StartWorkflow(CalculatedSettings.RecalculateWorkflowId, TargetId);
        }
    }
}
