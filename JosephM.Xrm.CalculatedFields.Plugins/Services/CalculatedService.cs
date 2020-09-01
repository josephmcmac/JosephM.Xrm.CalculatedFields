using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Rollups;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace JosephM.Xrm.CalculatedFields.Plugins.Services
{
    /// <summary>
    /// A service class for performing logic
    /// </summary>
    public class CalculatedService
    {
        private XrmService XrmService { get; set; }
        private CalculatedSettings CalculatedSettings { get; set; }
        public LocalisationService LocalisationService { get; }

        public CalculatedService(XrmService xrmService, CalculatedSettings settings, LocalisationService localisationService)
        {
            XrmService = xrmService;
            CalculatedSettings = settings;
            LocalisationService = localisationService;
        }

        public void RefreshPluginRegistrations(Guid changedEntityId, bool isCurrentlyActive)
        {
            //todo delete, inactive
            var activeCalculatedFields = XrmService.RetrieveAllAndConditions(Entities.jmcg_calculatedfield, new[]
            {
                new ConditionExpression(Fields.jmcg_calculatedfield_.statecode, ConditionOperator.Equal, OptionSets.CalculatedField.Status.Active)
            });

            IEnumerable<Entity> sdkMessageProcessingSteps = GetCalculateFieldsEvents();

            var removeIfNotUpdated = !isCurrentlyActive
                ? sdkMessageProcessingSteps
                .Where(sdk => sdk.GetStringField(Fields.sdkmessageprocessingstep_.configuration).Contains(changedEntityId.ToString()))
                .ToList()
                : new List<Entity>();

            //messages - entity type -> message -> stage
            var messages = new Dictionary<string, Dictionary<string, Dictionary<int, List<Entity>>>>();
            Action<string, string, int, Entity> addToDictionary = (type, message, stage, entity) =>
            {
                if (!messages.ContainsKey(type))
                {
                    messages.Add(type, new Dictionary<string, Dictionary<int, List<Entity>>>());
                }
                if (!messages[type].ContainsKey(message))
                {
                    messages[type].Add(message, new Dictionary<int, List<Entity>>());
                }
                if (!messages[type][message].ContainsKey(stage))
                {
                    messages[type][message].Add(stage, new List<Entity>());
                }
                messages[type][message][stage].Add(entity);
            };

            foreach (var calculatedField in activeCalculatedFields)
            {
                var targetEntity = calculatedField.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype);
                var calculationType = calculatedField.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type);
                if (calculationType == OptionSets.CalculatedField.Type.Rollup)
                {
                    addToDictionary(targetEntity, PluginMessage.Create, PluginStage.PreOperationEvent, calculatedField);

                    var rolledUpType = calculatedField.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup);

                    addToDictionary(rolledUpType, PluginMessage.Create, PluginStage.PostEvent, calculatedField);
                    addToDictionary(rolledUpType, PluginMessage.Update, PluginStage.PostEvent, calculatedField);
                    addToDictionary(rolledUpType, PluginMessage.Delete, PluginStage.PostEvent, calculatedField);
                }
                if (calculationType == OptionSets.CalculatedField.Type.Concatenate)
                {
                    addToDictionary(targetEntity, PluginMessage.Create, PluginStage.PreOperationEvent, calculatedField);
                    addToDictionary(targetEntity, PluginMessage.Update, PluginStage.PreOperationEvent, calculatedField);
                }
            }

            //only update those for this entity
            foreach (var type in messages)
            {
                foreach (var message in type.Value)
                {
                    foreach (var stage in message.Value)
                    {
                        var matchingMessages = sdkMessageProcessingSteps
                            .Where(m => m.GetOptionSetValue(Fields.sdkmessageprocessingstep_.stage) == stage.Key
                                && m.GetStringField("FILTER." + Fields.sdkmessagefilter_.primaryobjecttypecode) == type.Key
                                && m.GetStringField("MESSAGE." + Fields.sdkmessage_.name) == message.Key);

                        var sdkMessage = matchingMessages.Any()
                            ? matchingMessages.First()
                            : new Entity(Entities.sdkmessageprocessingstep);

                        if (stage.Value.Any(v => v.Id == changedEntityId
                            || (sdkMessage.Id != Guid.Empty && sdkMessage.GetStringField(Fields.sdkmessageprocessingstep_.configuration).Contains(changedEntityId.ToString()))))
                        {
                            var entityListXml = SerialiseToString(stage.Value);
                            if (sdkMessage.Id == Guid.Empty)
                            {
                                sdkMessage.SetField(Fields.sdkmessageprocessingstep_.configuration, entityListXml);
                                sdkMessage.SetLookupField(Fields.sdkmessageprocessingstep_.plugintypeid, GetPluginType());
                                sdkMessage.SetOptionSetField(Fields.sdkmessageprocessingstep_.stage, stage.Key);
                                sdkMessage.SetLookupField(Fields.sdkmessageprocessingstep_.sdkmessagefilterid, GetPluginFilter(type.Key, message.Key));
                                sdkMessage.SetLookupField(Fields.sdkmessageprocessingstep_.sdkmessageid, GetPluginMessage(message.Key));
                                sdkMessage.SetField(Fields.sdkmessageprocessingstep_.rank, 1);
                                sdkMessage.SetField(Fields.sdkmessageprocessingstep_.name, $"Calculate For {type.Key} {message.Key} {stage.Key}");
                                sdkMessage.Id = XrmService.Create(sdkMessage);

                                if (message.Key == PluginMessage.Update || message.Key == PluginMessage.Delete)
                                {
                                    var imageRecord = new Entity(Entities.sdkmessageprocessingstepimage);
                                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.name, "PreImage");
                                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.entityalias, "PreImage");
                                    imageRecord.SetField(Fields.sdkmessageprocessingstepimage_.messagepropertyname, "Target");
                                    imageRecord.SetLookupField(Fields.sdkmessageprocessingstepimage_.sdkmessageprocessingstepid, sdkMessage);
                                    imageRecord.SetOptionSetField(Fields.sdkmessageprocessingstepimage_.imagetype, OptionSets.SdkMessageProcessingStepImage.ImageType.PreImage);
                                    XrmService.Create(imageRecord);
                                }
                            }
                            else
                            {
                                sdkMessage.SetField(Fields.sdkmessageprocessingstep_.configuration, entityListXml);
                                XrmService.Update(sdkMessage);
                                removeIfNotUpdated.Remove(sdkMessage);
                            }
                        }
                    }
                }
            }

            foreach (var remove in removeIfNotUpdated)
            {
                XrmService.Delete(remove);
            }
        }

        public IEnumerable<Entity> GetCalculateFieldsEvents()
        {
            var sdkMessageProcessingStepsQuery = XrmService.BuildQuery(Entities.sdkmessageprocessingstep, null, null);
            var pluginTypeJoin = sdkMessageProcessingStepsQuery.AddLink(Entities.plugintype, Fields.sdkmessageprocessingstep_.plugintypeid, Fields.plugintype_.plugintypeid);
            pluginTypeJoin.LinkCriteria.AddCondition(new ConditionExpression(Fields.plugintype_.typename, ConditionOperator.Equal, PluginQualifiedName));
            var pluginFilterJoin = sdkMessageProcessingStepsQuery.AddLink(Entities.sdkmessagefilter, Fields.sdkmessageprocessingstep_.sdkmessagefilterid, Fields.sdkmessagefilter_.sdkmessagefilterid);
            pluginFilterJoin.EntityAlias = "FILTER";
            pluginFilterJoin.Columns = new ColumnSet(Fields.sdkmessagefilter_.primaryobjecttypecode);
            var pluginFilterMessage = pluginFilterJoin.AddLink(Entities.sdkmessage, Fields.sdkmessagefilter_.sdkmessageid, Fields.sdkmessage_.sdkmessageid);
            pluginFilterMessage.EntityAlias = "MESSAGE";
            pluginFilterMessage.Columns = new ColumnSet(Fields.sdkmessage_.name);
            var sdkMessageProcessingSteps = XrmService.RetrieveAll(sdkMessageProcessingStepsQuery);
            return sdkMessageProcessingSteps;
        }

        private string SerialiseToString(List<Entity> entities)
        {
            var serialiseEntities = new List<SerialisedEntity>();
            foreach(var entity in entities)
            {
                var serialise = new SerialisedEntity();
                serialise.Id = entity.Id;
                serialise.LogicalName = entity.LogicalName;
                serialise.Attributes = entity.Attributes.ToDictionary(kv => kv.Key,
                    kv =>
                    {
                        if (kv.Value == null)
                            return null;
                        if (KnownSerialisationTypes.Any(t => t == kv.Value.GetType()))
                            return kv.Value;
                        return kv.Value.ToString();
                    });
                serialiseEntities.Add(serialise);
            }

            var serializer = new DataContractJsonSerializer(typeof(SerialisedEntity[]), KnownSerialisationTypes);
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, serialiseEntities.ToArray());
                return Encoding.Default.GetString(stream.ToArray());
            }

            //var serializer = new DataContractSerializer(typeof(Entity[]));
            //var settings = new XmlWriterSettings { Indent = true };

            //var stringBuilder = new StringBuilder();
            //using (var w = XmlWriter.Create(stringBuilder, settings))
            //{
            //    serializer.WriteObject(w, entities.ToArray());
            //}
            //return stringBuilder.ToString();
        }

        private IEnumerable<Type> KnownSerialisationTypes
        {
            get
            {
                return new[]
                {
                    typeof(EntityReference),
                    typeof(OptionSetValue),
                    typeof(Money),
                    typeof(DateTime),
                    typeof(int),
                    typeof(decimal),
                    typeof(Guid),
                    typeof(bool),
                };
            }
        }

        public Entity[] DeserialiseEntities(string serialised)
        {
            object theObject;
            var serializer = new DataContractJsonSerializer(typeof(SerialisedEntity[]), KnownSerialisationTypes);
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(serialised)))
            {
                theObject = serializer.ReadObject(stream);
            }
            var deserialisedEntities = (SerialisedEntity[]) theObject;

            var entities = new List<Entity>();
            foreach(var deserialised in deserialisedEntities)
            {
                var entity = new Entity(deserialised.LogicalName);
                entity.Id = deserialised.Id;
                foreach(var keyValue in deserialised.Attributes)
                {
                    entity[keyValue.Key] = keyValue.Value;
                }
                entities.Add(entity);
            }
            return entities.ToArray();
        }

        private string PluginQualifiedName
        {
            get { return "JosephM.Xrm.CalculatedFields.Plugins.CalculateFieldsPluginRegistration"; }
        }

        public Entity GetPluginType()
        {
            var entity = XrmService.GetFirst(Entities.plugintype, Fields.plugintype_.typename, PluginQualifiedName);
            if (entity == null)
                throw new NullReferenceException(string.Format("No {0} Exists With {1} = {2}",
                    XrmService.GetEntityDisplayName(Entities.plugintype), XrmService.GetFieldLabel(Fields.plugintype_.typename, Entities.plugintype),
                    PluginQualifiedName));
            return entity;
        }

        public Entity GetPluginFilter(string entityType, string message)
 
        {
            var pluginFilters = XrmService.RetrieveAllAndConditions(Entities.sdkmessagefilter, new[]
            {
                new ConditionExpression(Fields.sdkmessagefilter_.primaryobjecttypecode, ConditionOperator.Equal,
                    XrmService.GetEntityMetadata(entityType).ObjectTypeCode),
                new ConditionExpression(Fields.sdkmessagefilter_.sdkmessageid, ConditionOperator.Equal, GetPluginMessage(message).Id)
            });

            if (pluginFilters.Count() != 1)
                throw new InvalidPluginExecutionException(string.Format(
                    "Error Getting {0} for {1} {2} and type {3}",
                    XrmService.GetEntityDisplayName(Entities.sdkmessagefilter), XrmService.GetEntityDisplayName(Entities.sdkmessage), message,
                    XrmService.GetEntityDisplayName(entityType)));
            return pluginFilters.First();
        }

        public Entity GetPluginMessage(string message)
        {
            return XrmService.GetFirst(Entities.sdkmessage, Fields.sdkmessage_.name, message);
        }

        public void ApplyCalculations(XrmEntityPlugin plugin, IEnumerable<CalculatedFieldsConfig> calculatedConfigs)
        {
            var rollupCalculations = calculatedConfigs
                .Where(cf => cf.CalculatedFieldEntity.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type) == OptionSets.CalculatedField.Type.Rollup)
                .ToArray();
            {
                if(rollupCalculations.Any())
                {
                    var rollups = new List<LookupRollup>();
                    foreach (var config in calculatedConfigs)
                    {
                        var rollup = CreateRollup(config);
                        rollups.Add(rollup);
                    }
                    var rollupService = new CalculatedRollupService(XrmService, rollups);
                    rollupService.SetInitialValues(plugin);
                    rollupService.ExecuteDependencyPlugin(plugin);
                }
            }

            foreach(var calculatedConfig in calculatedConfigs)
            {
                if(IsDependencyChanging(calculatedConfig, plugin))
                {
                    var fieldCalculated = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field);
                    var oldValue = plugin.GetField(fieldCalculated);
                    var newValue = GetNewValue(calculatedConfig, plugin.GetField);
                    if (!XrmEntity.FieldsEqual(oldValue, newValue))
                    {
                        plugin.SetField(fieldCalculated, newValue);
                    }
                }
            }
        }

        public object GetNewValue(CalculatedFieldsConfig calculatedConfig, Func<string, object> getField)
        {
            var calculationType = calculatedConfig.CalculatedFieldEntity.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type);
            var entityTypeWithCalculation = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype);

            switch (calculationType)
            {
                case OptionSets.CalculatedField.Type.Concatenate:
                    {
                        var includeEmpty = calculatedConfig.CalculatedFieldEntity.GetBoolean(Fields.jmcg_calculatedfield_.jmcg_includeifempty);
                        var concatFields = GetConcatenateFields(calculatedConfig);
                        var concatValues = concatFields
                            .Select(cf => XrmService.GetFieldAsDisplayString(entityTypeWithCalculation, cf, getField(cf), LocalisationService))
                            .Where(v => includeEmpty || !string.IsNullOrWhiteSpace(v))
                            .ToArray();
                        return string.Join(GetSeparatorString(calculatedConfig.CalculatedFieldEntity), concatValues);
                    }
            }
            throw new NotImplementedException($"Not implemented for type {calculationType}");
        }

        public IEnumerable<string> GetConcatenateFields(CalculatedFieldsConfig calculatedConfig)
        {
            var concatenateFields = new List<string>();
            concatenateFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_concatenatefield1));
            concatenateFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_concatenatefield2));
            concatenateFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_concatenatefield3));
            concatenateFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_concatenatefield4));
            concatenateFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_concatenatefield5));
            return concatenateFields.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        public IEnumerable<string> GetDependencyFields(CalculatedFieldsConfig calculatedConfig)
        {
            var calculationType = calculatedConfig.CalculatedFieldEntity.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type);
            var dependencyFields = new List<string>();
            switch (calculationType)
            {
                case OptionSets.CalculatedField.Type.Concatenate:
                    {
                        dependencyFields.AddRange(GetConcatenateFields(calculatedConfig));
                        break;
                    }
            }
            return dependencyFields;
        }

        private bool IsDependencyChanging(CalculatedFieldsConfig calculatedConfig, XrmEntityPlugin plugin)
        {
            var dependencyFields = GetDependencyFields(calculatedConfig);
            return dependencyFields.Any() && plugin.FieldChanging(dependencyFields);
        }

        public LookupRollup CreateRollup(CalculatedFieldsConfig config)
        {
            var e = config.CalculatedFieldEntity;
            var rollup = new LookupRollup(e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype),
                e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field),
                e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup),
                e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_fieldrolledup),
                (RollupType)e.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_rolluptype),
                e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_fieldreferencing),
                FieldTypeOptionToClrTytpe(e.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_fieldtype)),
                GetSeparatorString(e));
            rollup.Filter = config.FilterExpression;
            rollup.FilterXml = e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_rollupfilter);
            return rollup;
        }

        public string GetSeparatorString(Entity calculatedField)
        {
            var result = "";
            switch(calculatedField.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_separatortype))
            {
                case OptionSets.CalculatedField.SeparatorType.Comma:
                    {
                        result = ",";
                        break;
                    }
                case OptionSets.CalculatedField.SeparatorType.Hyphen:
                    {
                        result = "-";
                        break;
                    }
                case OptionSets.CalculatedField.SeparatorType.NewLine:
                    {
                        result = Environment.NewLine;
                        break;
                    }
                case OptionSets.CalculatedField.SeparatorType.Pipe:
                    {
                        result ="|";
                        break;
                    }
                case OptionSets.CalculatedField.SeparatorType.Space:
                    {
                        result = " ";
                        break;
                    }
                case OptionSets.CalculatedField.SeparatorType.OtherString:
                    {
                        result = calculatedField.GetStringField(Fields.jmcg_calculatedfield_.jmcg_separatorstring);
                        break;
                    }
            }
            if(calculatedField.GetBoolean(Fields.jmcg_calculatedfield_.jmcg_separatorspacebefore))
            {
                result = " " + result;
            }
            if (calculatedField.GetBoolean(Fields.jmcg_calculatedfield_.jmcg_separatorspaceafter))
            {
                result = result + " ";
            }
            return result;
        }
        public CalculatedFieldsConfig LoadCalculatedFieldConfig(Entity calculatedFieldEntity)
        {
            var config = new CalculatedFieldsConfig
            {
                CalculatedFieldEntity = calculatedFieldEntity,
            };
            var filterXml = calculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_rollupfilter);
            var rollupEntityType = calculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup);

            if (!string.IsNullOrWhiteSpace(rollupEntityType)
                && !string.IsNullOrWhiteSpace(filterXml))
            {

                var fetchXml = "<fetch distinct=\"true\" no-lock=\"false\" mapping=\"logical\"><entity name=\"" + rollupEntityType + "\">" + filterXml + "</entity></fetch>";
                var response = (Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionResponse)XrmService.Execute(new Microsoft.Crm.Sdk.Messages.FetchXmlToQueryExpressionRequest
                {
                    FetchXml = fetchXml
                });

                config.FilterExpression = response.Query.Criteria;
            }
            return config;
        }

        private Type FieldTypeOptionToClrTytpe(int optionValue)
        {
            switch (optionValue)
            {
                case OptionSets.CalculatedField.FieldType.Boolean:
                    {
                        return typeof(bool);
                    }
                case OptionSets.CalculatedField.FieldType.Date:
                    {
                        return typeof(DateTime);
                    }
                case OptionSets.CalculatedField.FieldType.Decimal:
                    {
                        return typeof(decimal);
                    }
                case OptionSets.CalculatedField.FieldType.Double:
                    {
                        return typeof(double);
                    }
                case OptionSets.CalculatedField.FieldType.Integer:
                    {
                        return typeof(int);
                    }
                case OptionSets.CalculatedField.FieldType.Money:
                    {
                        return typeof(Money);
                    }
                case OptionSets.CalculatedField.FieldType.String:
                    {
                        return typeof(string);
                    }
                default:
                    {
                        throw new InvalidPluginExecutionException($"Field Type not implemented for option value {optionValue}");
                    }
            }
        }

        public class SerialisedEntity
        {
            public string LogicalName { get; set; }
            public Guid Id { get; set; }
            public Dictionary<string, object> Attributes { get; set; }
        }
    }
}



















