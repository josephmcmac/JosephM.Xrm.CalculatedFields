using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using System;
using Schema;
using Microsoft.Xrm.Sdk.Query;
using JosephM.Xrm.CalculatedFields.Plugins.Rollups;
using System.Linq;
using JosephM.Xrm.CalculatedFields.Plugins.Core;
using System.Windows.Documents;
using System.Collections.Generic;

namespace JosephM.Xrm.CalculatedFields.Plugins.Workflows
{
    /// <summary>
    /// This class is for the static type required for registration of the custom workflow activity in CRM
    /// </summary>
    public class CalculatedFieldsRecalculateAllWorkflow : XrmWorkflowActivityRegistration
    {
        [Output("Is Finished")]
        public OutArgument<bool> IsFinished { get; set; }

        protected override XrmWorkflowActivityInstanceBase CreateInstance()
        {
            return new CalculatedFieldsRecalculateAllWorkflowInstance();
        }
    }

    /// <summary>
    /// This class is instantiated per execution
    /// </summary>
    public class CalculatedFieldsRecalculateAllWorkflowInstance
        : CalculatedWorkflowActivity<CalculatedFieldsRecalculateAllWorkflow>
    {
        protected override void Execute()
        {
            ActivityThisType.IsFinished.Set(ExecutionContext, DoRefresh());
        }

        private bool DoRefresh()
        {
            var target = XrmService.Retrieve(TargetType, TargetId);

            var fieldsToSet = new List<string>();
            if(!target.GetBoolean(Fields.jmcg_calculatedfield_.jmcg_isrecalculating))
            {
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_isrecalculating, true);
                fieldsToSet.Add(Fields.jmcg_calculatedfield_.jmcg_isrecalculating);
            }
            if (!string.IsNullOrWhiteSpace(target.GetStringField(Fields.jmcg_calculatedfield_.jmcg_errorrecalculating)))
            {
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_errorrecalculating, null);
                fieldsToSet.Add(Fields.jmcg_calculatedfield_.jmcg_errorrecalculating);
            }
            if(fieldsToSet.Any())
            {
                XrmService.Update(target, fieldsToSet.ToArray());
            }

            var isFinished = false;
            var createDateThreshold = target.GetDateTimeField(Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate) ?? new DateTime(1910, 1, 1);
            try
            {
                var config = CalculatedService.LoadCalculatedFieldConfig(target);
                var startedAt = DateTime.UtcNow;
                var fieldsToLoad = new List<string>();
                var calculatedFieldFieldName = target.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field);
                var calciulatedFieldEntityName = target.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype);
                fieldsToLoad.Add(calculatedFieldFieldName);
                fieldsToLoad.Add("createdon");
                fieldsToLoad.AddRange(CalculatedService.GetDependencyFields(config));

                while (true)
                {
                    if (IsSandboxIsolated && DateTime.UtcNow - startedAt > new TimeSpan(0, 0, MaxSandboxIsolationExecutionSeconds - 10))
                    {
                        break;
                    }
                    else
                    {
                        var processSetQuery = new QueryExpression(calciulatedFieldEntityName);
                        processSetQuery.ColumnSet = new ColumnSet(fieldsToLoad.ToArray());
                        processSetQuery.AddOrder("createdon", OrderType.Ascending);
                        processSetQuery.Criteria.AddCondition(new ConditionExpression("createdon", ConditionOperator.GreaterEqual, createDateThreshold));

                        var processSet = XrmService.RetrieveMultiple(processSetQuery).Entities.ToList();
                        var countThisSet = processSet.Count;

                        if (countThisSet == 0)
                        {
                            isFinished = true;
                        }
                        else
                        {
                            var countProcessed = 0;
                            ProcessWhileInSandboxLimit(startedAt, processSet.ToArray(), (entity) =>
                            {
                                try
                                {
                                    if (target.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type) == OptionSets.CalculatedField.Type.Rollup)
                                    {
                                        var rollup = CalculatedService.CreateRollup(config);
                                        var rollupService = new CalculatedRollupService(XrmService, new []
                                        {
                                            rollup
                                        });

                                        rollupService.RefreshRollup(entity.Id, rollup);
                                    }
                                    else
                                    {
                                        var oldValue = entity.GetField(calculatedFieldFieldName);
                                        var newValue = CalculatedService.GetNewValue(config, entity.GetField);
                                        if (!XrmEntity.FieldsEqual(oldValue, newValue))
                                        {
                                            entity.SetField(calculatedFieldFieldName, newValue);
                                            XrmService.Update(entity, calculatedFieldFieldName);
                                        }
                                    }
                                    createDateThreshold = entity.GetDateTimeField("createdon") ?? throw new InvalidPluginExecutionException("Error empty createdon " + entity.Id);
                                    processSet.Remove(entity);
                                    countProcessed++;
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidPluginExecutionException($"Error refreshing ecountered for ID {entity.Id}. Refresh will be discontinued", ex);
                                }
                            });

                            if (countThisSet < 5000
                                && countProcessed == countThisSet)
                            {
                                isFinished = true;
                            }
                        }
                    }
                    if (isFinished)
                    {
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_errorrecalculating, ex.XrmDisplayString().Left(10000));
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate, null);
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_isrecalculating, false);
                XrmService.Update(target, Fields.jmcg_calculatedfield_.jmcg_errorrecalculating, Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate, Fields.jmcg_calculatedfield_.jmcg_isrecalculating);
                throw ex;
            }

            if(isFinished)
            {
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate, null);
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_isrecalculating, false);
                XrmService.Update(target, Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate, Fields.jmcg_calculatedfield_.jmcg_isrecalculating);
            }
            else
            {
                target.SetField(Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate, createDateThreshold);
                XrmService.Update(target, Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate);
            }

            return isFinished;
        }
    }
}
