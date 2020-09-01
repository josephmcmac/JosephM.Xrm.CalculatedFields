using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;

namespace JosephM.Xrm.CalculatedFields.Plugins.Plugins
{
    public class CalculatedFieldPlugin : CalculatedEntityPluginBase
    {
        public override void GoExtention()
        {
            Validate();
            StartRecalculation();
            SetFieldType();
            RefreshPluginRegistration();
        }

        private void Validate()
        {
            if (IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PostEvent))
            {
                if (ConfigFieldChanging())
                {
                    //check valid fetch / query for rollup
                    var target = XrmService.Retrieve(TargetType, TargetId);
                    var config = CalculatedService.LoadCalculatedFieldConfig(target);
                    if (target.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type) == OptionSets.CalculatedField.Type.Rollup)
                    {
                        var rollup = CalculatedService.CreateRollup(config);
                        var query = new QueryExpression(rollup.RecordTypeWithRollup);
                        query.ColumnSet = new ColumnSet(rollup.RollupField);
                        var link = query.AddLink(rollup.RecordTypeRolledup, XrmService.GetPrimaryKey(rollup.RecordTypeWithRollup), rollup.LookupName);
                        link.Columns = new ColumnSet(rollup.FieldRolledup);
                        if (rollup.Filter != null)
                        {
                            link.LinkCriteria = rollup.Filter;
                        }
                        try
                        {
                            XrmService.Execute(new QueryExpressionToFetchXmlRequest
                            {
                                Query = query
                            });
                        }
                        catch(Exception ex)
                        {
                            throw new InvalidPluginExecutionException("There was an error validating a query for the rollups configuration - " + ex.Message, ex);
                        }
                    }
                }
            }
        }

        private void StartRecalculation()
        {
            if (IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PreOperationEvent))
            {
                if(BooleanChangingToTrue(Fields.jmcg_calculatedfield_.jmcg_recalculateall))
                {
                    SetField(Fields.jmcg_calculatedfield_.jmcg_recalculateall, false);
                    SetField(Fields.jmcg_calculatedfield_.jmcg_isrecalculating, true);
                    Context.SharedVariables.Add("STARTRECALCULATE", true);
                }
            }

            if (IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                if (Context.SharedVariables.ContainsKey("STARTRECALCULATE"))
                {
                    XrmService.StartWorkflow(CalculatedSettings.RecalculateWorkflowId, TargetId);
                }
            }
        }

        private void SetFieldType()
        {
            if(IsMessage(PluginMessage.Create, PluginMessage.Update) && IsStage(PluginStage.PreOperationEvent))
            {
                if(FieldChanging(Fields.jmcg_calculatedfield_.jmcg_entitytype, Fields.jmcg_calculatedfield_.jmcg_field))
                {
                    var entityType = GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype);
                    var fieldName = GetStringField(Fields.jmcg_calculatedfield_.jmcg_field);
                    var fieldTypeOption = -1;
                    if(!string.IsNullOrWhiteSpace(entityType) && !string.IsNullOrWhiteSpace(fieldName))
                    {
                        var fieldType = XrmService.GetFieldType(fieldName, entityType);
                        switch(fieldType)
                        {
                            case AttributeTypeCode.Boolean:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.Boolean;
                                    break;
                                }
                            case AttributeTypeCode.DateTime:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.Date;
                                    break;
                                }
                            case AttributeTypeCode.Decimal:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.Decimal;
                                    break;
                                }
                            case AttributeTypeCode.Double:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.Double;
                                    break;
                                }
                            case AttributeTypeCode.Integer:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.Integer;
                                    break;
                                }
                            case AttributeTypeCode.Money:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.Money;
                                    break;
                                }
                            case AttributeTypeCode.String:
                            case AttributeTypeCode.Memo:
                                {
                                    fieldTypeOption = OptionSets.CalculatedField.FieldType.String;
                                    break;
                                }
                            default:
                                {
                                    throw new NotSupportedException($"Calculated fields not supported for type {fieldType}");
                                }
                        }
                    }
                    if(GetOptionSet(Fields.jmcg_calculatedfield_.jmcg_fieldtype) != fieldTypeOption)
                    {
                        SetOptionSetField(Fields.jmcg_calculatedfield_.jmcg_fieldtype, fieldTypeOption);
                    }
                }
            }
        }

        private void RefreshPluginRegistration()
        {
            if(IsMessage(PluginMessage.Create, PluginMessage.Update, PluginMessage.Delete) && IsStage(PluginStage.PostEvent) && IsMode(PluginMode.Synchronous))
            {
                var refreshSdkMessageProcessingSteps = IsMessage(PluginMessage.Delete)
                    || ConfigFieldChanging();
                if (refreshSdkMessageProcessingSteps)
                {
                    var isActive = GetOptionSet(Fields.jmcg_calculatedfield_.statecode) == OptionSets.CalculatedField.Status.Active;
                    CalculatedService.RefreshPluginRegistrations(TargetId, isActive);
                }
            }
        }

        private bool ConfigFieldChanging()
        {
            return TargetEntity
                                .Attributes
                                .Keys
                                .Where(k => k.StartsWith("jmcg"))
                                .Except(new[]
                                {
                        Fields.jmcg_calculatedfield_.jmcg_recalculateall,
                        Fields.jmcg_calculatedfield_.jmcg_isrecalculating,
                        Fields.jmcg_calculatedfield_.jmcg_errorrecalculating,
                        Fields.jmcg_calculatedfield_.jmcg_lastrecalculationcreatedate
                                }).Any(FieldChanging);
        }
    }
}