﻿using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
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

        public DateTime AddTime(DateTime dateAddTo, int timeType, int timeAmount, Guid? workCalendarId)
        {
            var startTimeUtc = dateAddTo.Kind == DateTimeKind.Utc
                ? dateAddTo
                : LocalisationService.ConvertTargetToUtc(dateAddTo);

            var somewhereInTimeUtc = startTimeUtc;

            switch (timeType)
            {
                case OptionSets.CalculatedField.TimeType.Minutes:
                    {
                        somewhereInTimeUtc = somewhereInTimeUtc.AddMinutes(timeAmount);
                        break;
                    }
                case OptionSets.CalculatedField.TimeType.Hours:
                    {
                        somewhereInTimeUtc = somewhereInTimeUtc.AddHours(timeAmount);
                        break;
                    }
                case OptionSets.CalculatedField.TimeType.Days:
                    {
                        somewhereInTimeUtc = LocalisationService.ConvertTargetToUtc(LocalisationService.ConvertToTargetTime(somewhereInTimeUtc).AddDays(timeAmount));
                        break;
                    }
                case OptionSets.CalculatedField.TimeType.Months:
                    {
                        somewhereInTimeUtc = LocalisationService.ConvertTargetToUtc(LocalisationService.ConvertToTargetTime(somewhereInTimeUtc).AddMonths(timeAmount));
                        break;
                    }
                case OptionSets.CalculatedField.TimeType.WorkMinutes:
                case OptionSets.CalculatedField.TimeType.WorkHours:
                case OptionSets.CalculatedField.TimeType.WorkDays:
                    {
                        if (!workCalendarId.HasValue)
                        {
                            throw new ArgumentNullException(nameof(workCalendarId), "Required for work time type " + timeType);
                        }
                        var bufferMinutes = 0;
                        switch (timeType)
                        {
                            case OptionSets.CalculatedField.TimeType.WorkMinutes:
                                {
                                    bufferMinutes = timeAmount;
                                    break;
                                }
                            case OptionSets.CalculatedField.TimeType.WorkHours:
                                {
                                    bufferMinutes = timeAmount * 60;
                                    break;
                                }
                            case OptionSets.CalculatedField.TimeType.WorkDays:
                                {
                                    bufferMinutes = timeAmount * 60 * 24;
                                    break;
                                }
                            default:
                                {
                                    throw new InvalidPluginExecutionException("Not implemented for time type of " + timeType);
                                }
                        }
                        var isNegative = timeAmount < 0;
                        bufferMinutes = (bufferMinutes * 2) + (30 * (isNegative ? -1 : 1) * 60 * 24); //double days + 30?
                        var outerBoundUtc = startTimeUtc.AddMinutes(bufferMinutes);
                        var rangeStartUtc = isNegative ? outerBoundUtc : startTimeUtc.AddDays(-1);
                        var rangeEndUtc = isNegative ? startTimeUtc.AddDays(1) : outerBoundUtc;
                        var request = new Microsoft.Crm.Sdk.Messages.ExpandCalendarRequest()
                        {
                            CalendarId = workCalendarId.Value,
                            Start = rangeStartUtc,
                            End = rangeEndUtc
                        };
                        var response = (Microsoft.Crm.Sdk.Messages.ExpandCalendarResponse)XrmService.Execute(request);

                        var publicHolidays = GetPublicHolidays(rangeStartUtc, rangeEndUtc, workCalendarId.Value);
                        Func<DateTime, bool> getIsPublicHoliday = (DateTime utc) =>
                        {
                            var vicTime = LocalisationService.ConvertToTargetTime(utc);
                            return publicHolidays.Any(h => h.Year == vicTime.Year && h.Month == vicTime.Month && h.Day == vicTime.Day);
                        };

                        var sortTimeSlots = isNegative
                            ? response.result.Where(t => t.Start.HasValue && t.End.HasValue).OrderByDescending(t => t.Start).ToArray()
                            : response.result.Where(t => t.Start.HasValue && t.End.HasValue).OrderBy(t => t.Start).ToArray();

                        var daysCounted = new List<Tuple<int, int, int>>();
                        Func<DateTime, bool> dayAlreadyCounted = (dt) =>
                        {
                            var tuple = new Tuple<int, int, int>(dt.Year, dt.Month, dt.Day);
                            if (daysCounted.Any(t => t.Item1 == tuple.Item1
                 && t.Item2 == tuple.Item2
                 && t.Item3 == tuple.Item3))
                            {
                                return true;
                            }
                            else
                            {
                                daysCounted.Add(tuple);
                                return false;
                            }
                        };


                        var minutesRemaining = Math.Abs(timeType == OptionSets.CalculatedField.TimeType.WorkMinutes
                            ? timeAmount
                            : 60 * timeAmount);
                        foreach (var timeSlot in sortTimeSlots)
                        {
                            if (timeType == OptionSets.CalculatedField.TimeType.WorkDays)
                            {
                                var somewhereInTimeLocal = LocalisationService.ConvertToTargetTime(somewhereInTimeUtc);
                                var spanStartLocal = LocalisationService.ConvertToTargetTime(timeSlot.Start.Value);
                                var isSameDay = somewhereInTimeLocal.Year == spanStartLocal.Year
                                    && somewhereInTimeLocal.Month == spanStartLocal.Month
                                    && somewhereInTimeLocal.Day == spanStartLocal.Day;
                                var isGreaterThanStart = somewhereInTimeLocal > spanStartLocal;

                                var countDay = !getIsPublicHoliday(spanStartLocal)
                                    && (daysCounted.Any()
                                    || ((isNegative && (isSameDay || spanStartLocal < somewhereInTimeUtc))
                                        || (!isNegative && (isSameDay || spanStartLocal > somewhereInTimeUtc))));

                                var dayCounted = countDay && dayAlreadyCounted(spanStartLocal);

                                if (daysCounted.Count == Math.Abs(timeAmount) + 1)
                                {
                                    var resultLocal = new DateTime(spanStartLocal.Year, spanStartLocal.Month, spanStartLocal.Day, somewhereInTimeLocal.Hour, somewhereInTimeLocal.Minute, somewhereInTimeLocal.Second, DateTimeKind.Unspecified);
                                    return dateAddTo.Kind == DateTimeKind.Utc
                                            ? LocalisationService.ConvertTargetToUtc(resultLocal)
                                            : resultLocal;
                                }
                            }
                            else
                            {
                                var spanEndLocal = LocalisationService.ConvertToTargetTime(timeSlot.End.Value);
                                var isPublicHoliday = getIsPublicHoliday(spanEndLocal);
                                if (!isPublicHoliday)
                                {
                                    if (isNegative)
                                    {
                                        while (somewhereInTimeUtc > timeSlot.End.Value)
                                        {
                                            somewhereInTimeUtc = somewhereInTimeUtc.AddMinutes(-1);
                                        }
                                        while (somewhereInTimeUtc > timeSlot.Start.Value)
                                        {
                                            somewhereInTimeUtc = somewhereInTimeUtc.AddMinutes(-1);
                                            minutesRemaining--;
                                            if (minutesRemaining <= 0)
                                            {
                                                return dateAddTo.Kind == DateTimeKind.Utc
                                                    ? somewhereInTimeUtc
                                                    : LocalisationService.ConvertToTargetTime(somewhereInTimeUtc);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        while (somewhereInTimeUtc < timeSlot.Start.Value)
                                        {
                                            somewhereInTimeUtc = somewhereInTimeUtc.AddMinutes(1);
                                        }
                                        while (somewhereInTimeUtc < timeSlot.End.Value)
                                        {
                                            somewhereInTimeUtc = somewhereInTimeUtc.AddMinutes(1);
                                            minutesRemaining--;
                                            if (minutesRemaining <= 0)
                                            {
                                                return dateAddTo.Kind == DateTimeKind.Utc
                                                    ? somewhereInTimeUtc
                                                    : LocalisationService.ConvertToTargetTime(somewhereInTimeUtc);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
            return dateAddTo.Kind == DateTimeKind.Utc
                ? somewhereInTimeUtc
                : LocalisationService.ConvertToTargetTime(somewhereInTimeUtc);
        }

        public IEnumerable<DateTime> GetPublicHolidays(DateTime startDateUtc, DateTime endDateUtc, Guid calendarId)
        {
            var vicStartTime = LocalisationService.ConvertToTargetTime(startDateUtc).AddDays(-30);
            var vicEndTime = LocalisationService.ConvertToTargetTime(endDateUtc).AddDays(30);
            var myStart = new DateTime(vicStartTime.Year, vicStartTime.Month, vicStartTime.Day, vicStartTime.Hour, vicStartTime.Minute, vicStartTime.Second, DateTimeKind.Utc);
            var myEnd = new DateTime(vicEndTime.Year, vicEndTime.Month, vicEndTime.Day, vicEndTime.Hour, vicEndTime.Minute, vicEndTime.Second, DateTimeKind.Utc);

            var query = XrmService.BuildQuery(Entities.systemuser, new[] { Fields.systemuser_.systemuserid }, null, null);
            //query.Distinct = false;
            //XrmService.BuildQuery(Entities.calendar, new[] { Fields.calendar_.calendarid }, new[] {  }, null);
            var join0 = query.AddLink(Entities.calendar, Fields.systemuser_.systemuserid, Fields.calendar_.createdby);
            join0.LinkCriteria.AddCondition(new ConditionExpression(Fields.calendar_.calendarid, ConditionOperator.Equal, calendarId));
            var join1 = join0.AddLink(Entities.calendar, Fields.calendar_.holidayschedulecalendarid, Fields.calendar_.calendarid);
            var join2 = join1.AddLink(Entities.calendarrule, Fields.calendar_.calendarid, Fields.calendarrule_.calendarid);
            join2.EntityAlias = "CR";
            join2.Columns = XrmService.CreateColumnSet(new string[] { Fields.calendarrule_.starttime, Fields.calendarrule_.effectiveintervalend });
            join2.LinkCriteria.AddCondition(new ConditionExpression(Fields.calendarrule_.starttime, ConditionOperator.GreaterThan, myStart));
            join2.LinkCriteria.AddCondition(new ConditionExpression(Fields.calendarrule_.effectiveintervalend, ConditionOperator.LessThan, myEnd));
            var publicHolidays = XrmService.RetrieveAll(query);

            var dates = new List<DateTime>();

            foreach (var holiday in publicHolidays)
            {
                var start = (DateTime?)holiday.GetField("CR." + Fields.calendarrule_.starttime);
                var end = (DateTime?)holiday.GetField("CR." + Fields.calendarrule_.effectiveintervalend);
                if (start.HasValue && end.HasValue)
                {
                    while (start < end)
                    {
                        dates.Add(start.Value);
                        start = start.Value.AddDays(1);
                    }
                }
            }

            return dates;
        }

        public void RefreshPluginRegistrations(Guid changedEntityId, bool isCurrentlyActive)
        {
            var activeCalculatedFields = XrmService.RetrieveAllAndConditions(Entities.jmcg_calculatedfield, new[]
            {
                new ConditionExpression(Fields.jmcg_calculatedfield_.statecode, ConditionOperator.Equal, OptionSets.CalculatedField.Status.Active)
            });

            var sdkMessageProcessingSteps = GetCalculateFieldsEvents();

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
                if (calculationType == OptionSets.CalculatedField.Type.Concatenate
                    || calculationType == OptionSets.CalculatedField.Type.AddTime
                    || calculationType == OptionSets.CalculatedField.Type.TimeTaken
                    || calculationType == OptionSets.CalculatedField.Type.Lookup)
                {
                    addToDictionary(targetEntity, PluginMessage.Create, PluginStage.PreOperationEvent, calculatedField);
                    addToDictionary(targetEntity, PluginMessage.Update, PluginStage.PreOperationEvent, calculatedField);
                }
                if (calculationType == OptionSets.CalculatedField.Type.Lookup)
                {
                    var referencedType = calculatedField.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtype);
                    if (referencedType != targetEntity)
                    {
                        addToDictionary(referencedType, PluginMessage.Update, PluginStage.PreOperationEvent, calculatedField);
                    }
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
            foreach (var entity in entities)
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
            var deserialisedEntities = (SerialisedEntity[])theObject;

            var entities = new List<Entity>();
            foreach (var deserialised in deserialisedEntities)
            {
                var entity = new Entity(deserialised.LogicalName);
                entity.Id = deserialised.Id;
                foreach (var keyValue in deserialised.Attributes)
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
                if (rollupCalculations.Any())
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
            var lookupCalculations = calculatedConfigs
                .Where(cf => cf.CalculatedFieldEntity.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type) == OptionSets.CalculatedField.Type.Lookup)
                .ToArray();

            var lookupGroupDictionary = new Dictionary<string, Dictionary<string, Dictionary<string, List<CalculatedFieldsConfig>>>>();
            foreach (var lookupCalculation in lookupCalculations)
            {
                var entityType = lookupCalculation.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype);
                var lookupField = lookupCalculation.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_lookupfield);
                var referencedType = lookupCalculation.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtype);
                if (!lookupGroupDictionary.ContainsKey(entityType))
                {
                    lookupGroupDictionary.Add(entityType, new Dictionary<string, Dictionary<string, List<CalculatedFieldsConfig>>>());
                }
                if (!lookupGroupDictionary[entityType].ContainsKey(lookupField))
                {
                    lookupGroupDictionary[entityType].Add(lookupField, new Dictionary<string, List<CalculatedFieldsConfig>>());
                }
                if (!lookupGroupDictionary[entityType][lookupField].ContainsKey(referencedType))
                {
                    lookupGroupDictionary[entityType][lookupField].Add(referencedType, new List<CalculatedFieldsConfig>());
                }
                lookupGroupDictionary[entityType][lookupField][referencedType].Add(lookupCalculation);
            }
            foreach (var entityTypeKey in lookupGroupDictionary.Keys)
            {
                foreach (var lookupFieldKey in lookupGroupDictionary[entityTypeKey].Keys)
                {
                    foreach (var referencedTypeKey in lookupGroupDictionary[entityTypeKey][lookupFieldKey].Keys)
                    {
                        var lookupCalculationConfigs = lookupGroupDictionary[entityTypeKey][lookupFieldKey][referencedTypeKey];
                        if (plugin.TargetType == entityTypeKey)
                        {
                            if (plugin.FieldChanging(lookupFieldKey))
                            {
                                var lookupId = plugin.GetLookupGuid(lookupFieldKey);
                                var referencedEntity = lookupId.HasValue
                                    ? XrmService.Retrieve(referencedTypeKey, lookupId.Value, lookupCalculationConfigs.Select(c => c.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield)))
                                    : null;

                                foreach(var lookupCalculationConfig in lookupCalculationConfigs)
                                {
                                    var sourceField = lookupCalculationConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield);
                                    var targetField = lookupCalculationConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field);
                                    var oldValue = plugin.GetField(targetField);
                                    var newValue = referencedEntity.GetField(sourceField);
                                    if (!XrmEntity.FieldsEqual(oldValue, newValue))
                                    {
                                        plugin.SetField(targetField, newValue);
                                    }
                                }
                            }
                        }
                        if (plugin.TargetType == referencedTypeKey)
                        {
                            if (plugin.FieldChanging(lookupCalculationConfigs.Select(c => c.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield))))
                            {
                                var referencingRecords = XrmService.RetrieveAllAndConditions(entityTypeKey, new[]
                                {
                                    new ConditionExpression(lookupFieldKey, ConditionOperator.Equal, plugin.TargetId)
                                 }, lookupCalculationConfigs.Select(c => c.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field)));

                                foreach (var referencingRecord in referencingRecords)
                                {
                                    var updateEntity = new Entity(referencingRecord.LogicalName)
                                    {
                                        Id = referencingRecord.Id
                                    };

                                    foreach (var lookupCalculationConfig in lookupCalculationConfigs)
                                    {
                                        var sourceField = lookupCalculationConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield);
                                        var targetField = lookupCalculationConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field);
                                        var oldValue = referencingRecord.GetField(targetField);
                                        var newValue = plugin.GetField(sourceField);
                                        if (!XrmEntity.FieldsEqual(oldValue, newValue))
                                        {
                                            updateEntity.SetField(targetField, newValue);
                                        }
                                    }
                                    if(updateEntity.Attributes.Any())
                                    {
                                        XrmService.Update(updateEntity);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var calculatedConfig in calculatedConfigs.Except(rollupCalculations.Union(lookupCalculations)).ToArray())
            {
                if (IsDependencyChanging(calculatedConfig, plugin))
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
                        var ignoreValuesString = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_concatenateskipvalues);
                        var ignoreValues = string.IsNullOrWhiteSpace(ignoreValuesString)
                            ? new string[0]
                            : ignoreValuesString
                                .Split(';')
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                                .Select(s => s.Trim().ToLower())
                                .ToArray();

                        var includeEmpty = calculatedConfig.CalculatedFieldEntity.GetBoolean(Fields.jmcg_calculatedfield_.jmcg_includeifempty);
                        var concatValues = new List<string>();
                        foreach (var concatField in _concatenateFiels)
                        {
                            var thisField = calculatedConfig.CalculatedFieldEntity.GetStringField(concatField.FieldName);
                            if (thisField != null)
                            {
                                var displayValue = XrmService.GetFieldAsDisplayString(entityTypeWithCalculation, thisField, getField(thisField), LocalisationService, funcOrFormat: calculatedConfig.CalculatedFieldEntity.GetStringField(concatField.FormatFieldName));
                                if (includeEmpty || !string.IsNullOrWhiteSpace(displayValue)
                                    && !(displayValue != null && ignoreValues.Contains(displayValue.ToLower())))
                                {
                                    var prependString = calculatedConfig.CalculatedFieldEntity.GetStringField(concatField.PrependStringFieldName);
                                    if(calculatedConfig.CalculatedFieldEntity.GetBoolean(concatField.PrependStringSpacedFieldName))
                                    {
                                        prependString = prependString + " ";
                                    }
                                    var appendString = calculatedConfig.CalculatedFieldEntity.GetStringField(concatField.AppendStringFieldName);
                                    if (calculatedConfig.CalculatedFieldEntity.GetBoolean(concatField.AppendStringSpacedFieldName))
                                    {
                                        appendString = " " + appendString;
                                    }
                                    displayValue = prependString + displayValue + appendString;
                                    concatValues.Add(displayValue);
                                }
                            }
                        }

                        return string.Join(GetSeparatorString(calculatedConfig.CalculatedFieldEntity), concatValues);
                    }
                case OptionSets.CalculatedField.Type.AddTime:
                    {
                        var addTimeTo = (DateTime?)getField(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_addtimetofield));
                        if (addTimeTo.HasValue)
                        {
                            var calendarId = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_calendarid);
                            return AddTime(addTimeTo.Value, calculatedConfig.CalculatedFieldEntity.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_timetype), calculatedConfig.CalculatedFieldEntity.GetInt(Fields.jmcg_calculatedfield_.jmcg_timeamount), calendarId == null ? null : (Guid?)new Guid(calendarId));
                        }
                        else
                        {
                            return null;
                        }
                    }
                case OptionSets.CalculatedField.Type.TimeTaken:
                    {
                        var start = (DateTime?)getField(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_timetakenstartfield));
                        var end = (DateTime?)getField(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_timetakenendfield));

                        if (start.HasValue && end.HasValue)
                        {
                            var calendarId = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_calendarid);
                            return GetTimeTaken(start.Value, end.Value, calculatedConfig.CalculatedFieldEntity.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_timetakenmeasure), calendarId == null ? null : (Guid?)new Guid(calendarId));
                        }
                        else
                        {
                            return null;
                        }
                    }
                case OptionSets.CalculatedField.Type.Lookup:
                    {
                        var lookupId = XrmEntity.GetLookupGuid(getField(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_lookupfield)));
                        var referencedEntity = lookupId.HasValue
                            ? XrmService.Retrieve(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtype), lookupId.Value, new[] { calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield) })
                            : null;
                        return referencedEntity.GetField(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield));
                    }
            }
            throw new NotImplementedException($"Not implemented for type {calculationType}");
        }

        public int GetTimeTaken(DateTime startTime, DateTime endTime, int timeTakenMeasure, Guid? workCalendarId)
        {
            var startTimeUtc = startTime.Kind == DateTimeKind.Utc
                            ? startTime
                            : LocalisationService.ConvertTargetToUtc(startTime);
            var endTimeUtc = endTime.Kind == DateTimeKind.Utc
                ? endTime
                : LocalisationService.ConvertTargetToUtc(endTime);

            switch (timeTakenMeasure)
            {
                case OptionSets.CalculatedField.TimeTakenMeasure.Minutes:
                    {
                        return Convert.ToInt32((endTimeUtc - startTimeUtc).TotalMinutes);
                    }
                case OptionSets.CalculatedField.TimeTakenMeasure.Hours:
                    {
                        return Convert.ToInt32((endTimeUtc - startTimeUtc).TotalHours);
                    }
                case OptionSets.CalculatedField.TimeTakenMeasure.Days:
                    {
                        var startDay = LocalisationService.ConvertToTargetTime(startTime);
                        var endDay = LocalisationService.ConvertToTargetTime(endTime);
                        var daysTaken = 0;
                        while (startDay < endDay)
                        {
                            startDay = startDay.AddDays(1);
                            daysTaken++;
                        }
                        return daysTaken;
                    }
                case OptionSets.CalculatedField.TimeTakenMeasure.WorkMinutes:
                case OptionSets.CalculatedField.TimeTakenMeasure.WorkHours:
                case OptionSets.CalculatedField.TimeTakenMeasure.WorkDays:
                    {
                        if (!workCalendarId.HasValue)
                        {
                            throw new ArgumentNullException(nameof(workCalendarId), "Required for time taken measure " + timeTakenMeasure);
                        }
                        if (startTime >= endTime)
                        {
                            return 0;
                        }
                        var startBoundUtc = startTimeUtc.AddDays(-10);
                        var endBoundUtc = endTimeUtc.AddDays(10);
                        var request = new Microsoft.Crm.Sdk.Messages.ExpandCalendarRequest()
                        {
                            CalendarId = workCalendarId.Value,
                            Start = startBoundUtc,
                            End = endBoundUtc
                        };
                        var response = (Microsoft.Crm.Sdk.Messages.ExpandCalendarResponse)XrmService.Execute(request);

                        var publicHolidays = GetPublicHolidays(startBoundUtc, endBoundUtc, workCalendarId.Value);
                        Func<DateTime, bool> getIsPublicHoliday = (DateTime utc) =>
                        {
                            var vicTime = LocalisationService.ConvertToTargetTime(utc);
                            return publicHolidays.Any(h => h.Year == vicTime.Year && h.Month == vicTime.Month && h.Day == vicTime.Day);
                        };

                        var sortTimeSlots = response.result.Where(t => t.Start.HasValue && t.End.HasValue).OrderBy(t => t.Start).ToArray();

                        if (timeTakenMeasure == OptionSets.CalculatedField.TimeTakenMeasure.WorkMinutes
                            || timeTakenMeasure == OptionSets.CalculatedField.TimeTakenMeasure.WorkHours)
                        {
                            var totalMinutes = 0;
                            foreach (var timeSlot in sortTimeSlots)
                            {
                                if (!getIsPublicHoliday(timeSlot.Start.Value))
                                {
                                    if (startTimeUtc <= timeSlot.Start)
                                    {
                                        if (endTimeUtc >= timeSlot.End)
                                        {
                                            totalMinutes += Convert.ToInt32((timeSlot.End.Value - timeSlot.Start.Value).TotalMinutes);
                                        }
                                        else if (endTimeUtc > timeSlot.Start)
                                        {
                                            totalMinutes += Convert.ToInt32((endTimeUtc - timeSlot.Start.Value).TotalMinutes);
                                        }
                                    }
                                    else if (startTimeUtc > timeSlot.Start && startTimeUtc < timeSlot.End)
                                    {
                                        if (endTimeUtc >= timeSlot.End)
                                        {
                                            totalMinutes += Convert.ToInt32((timeSlot.End.Value - startTimeUtc).TotalMinutes);
                                        }
                                        else if (endTimeUtc < timeSlot.End)
                                        {
                                            totalMinutes += Convert.ToInt32((endTime - startTimeUtc).TotalMinutes);
                                        }
                                    }
                                }
                            }
                            if (timeTakenMeasure == OptionSets.CalculatedField.TimeTakenMeasure.WorkMinutes)
                            {
                                return totalMinutes;
                            }
                            else
                            {
                                return totalMinutes / 60;
                            }
                        }
                        else
                        {
                            var daysCounted = new List<Tuple<int, int, int>>();
                            Action<DateTime> addDay = (dt) =>
                            {
                                var tuple = new Tuple<int, int, int>(dt.Year, dt.Month, dt.Day);
                                if (!daysCounted.Any(t => t.Item1 == tuple.Item1
                                && t.Item2 == tuple.Item2
                                && t.Item3 == tuple.Item3))
                                {
                                    daysCounted.Add(tuple);
                                }
                            };

                            var startTimeLocal = LocalisationService.ConvertToTargetTime(startTimeUtc);
                            var endTimeLocal = LocalisationService.ConvertToTargetTime(endTimeUtc);

                            foreach (var timeSlot in sortTimeSlots)
                            {
                                if (!getIsPublicHoliday(timeSlot.Start.Value))
                                {
                                    var localStart = LocalisationService.ConvertToTargetTime(timeSlot.Start.Value);
                                    var isInRange = startTimeLocal.Date <= localStart.Date
                                        && endTimeLocal.Date >= localStart.Date;
                                    if (isInRange)
                                    {
                                        addDay(localStart);
                                    }
                                }
                            }
                            if (!daysCounted.Any())
                            {
                                return 0;
                            }
                            else
                            {
                                var startDayTuple = new Tuple<int, int, int>(startTimeLocal.Year, startTimeLocal.Month, startTimeLocal.Day);
                                var endDayTuple = new Tuple<int, int, int>(endTimeLocal.Year, endTimeLocal.Month, endTimeLocal.Day);
                                if (!daysCounted.Contains(startDayTuple) && !daysCounted.Contains(endDayTuple))
                                {
                                    return daysCounted.Count;
                                }
                                else
                                {
                                    return daysCounted.Count - 1;
                                }
                            }
                        }
                    }
            }
            throw new InvalidPluginExecutionException("Get Time Taken Not Implemented For Time Measure: " + timeTakenMeasure);
        }

        public IEnumerable<string> GetConcatenateFields(CalculatedFieldsConfig calculatedConfig)
        {
            var concatenateFields = new List<string>();
            foreach (var field in _concatenateFiels)
            {
                concatenateFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(field.FieldName));
            }
            return concatenateFields.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        public IEnumerable<string> GetDependencyFields(CalculatedFieldsConfig calculatedConfig, string typeChanging)
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
                case OptionSets.CalculatedField.Type.AddTime:
                    {
                        dependencyFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_addtimetofield));
                        break;
                    }
                case OptionSets.CalculatedField.Type.TimeTaken:
                    {
                        dependencyFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_timetakenstartfield));
                        dependencyFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_timetakenendfield));
                        break;
                    }
                case OptionSets.CalculatedField.Type.Lookup:
                    {
                        var targetType = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype);
                        var referencedType = calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtype);
                        if(typeChanging == targetType)
                        {
                            dependencyFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_lookupfield));
                        }
                        if (typeChanging == referencedType)
                        {
                            dependencyFields.Add(calculatedConfig.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_referencedtypetargetfield));
                        }
                        break;
                    }
            }
            return dependencyFields;
        }

        private bool IsDependencyChanging(CalculatedFieldsConfig calculatedConfig, XrmEntityPlugin plugin)
        {
            var dependencyFields = GetDependencyFields(calculatedConfig, plugin.TargetType);
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
                GetSeparatorString(e),
                e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfield),
                OrderTypeOptionToSdkType(e.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype)));
            rollup.Filter = config.FilterExpression;
            rollup.FilterXml = e.GetStringField(Fields.jmcg_calculatedfield_.jmcg_rollupfilter);
            return rollup;
        }

        public string GetSeparatorString(Entity calculatedField)
        {
            var result = "";
            switch (calculatedField.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_separatortype))
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
                        result = "|";
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
            if (calculatedField.GetBoolean(Fields.jmcg_calculatedfield_.jmcg_separatorspacebefore))
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

        private OrderType OrderTypeOptionToSdkType(int optionValue)
        {
            switch (optionValue)
            {
                case OptionSets.CalculatedField.OrderRollupByFieldOrderType.Ascending:
                    {
                        return OrderType.Ascending;
                    }
                case OptionSets.CalculatedField.OrderRollupByFieldOrderType.Descending:
                    {
                        return OrderType.Descending;
                    }
                default:
                    {
                        return OrderType.Ascending;
                    }
            }
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
                case OptionSets.CalculatedField.FieldType.Lookup:
                    {
                        return typeof(EntityReference);
                    }
                case OptionSets.CalculatedField.FieldType.Money:
                    {
                        return typeof(Money);
                    }
                case OptionSets.CalculatedField.FieldType.Picklist:
                    {
                        return typeof(OptionSetValue);
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

        public class ConcatenateFieldSetting
        {
            public string FieldName { get; set; }
            public string FormatFieldName { get; set; }
            public string PrependStringFieldName { get; set; }
            public string PrependStringSpacedFieldName { get; set; }
            public string AppendStringFieldName { get; set; }
            public string AppendStringSpacedFieldName { get; set; }
        }

        private IEnumerable<ConcatenateFieldSetting> _concatenateFiels = new[]
        {
            new ConcatenateFieldSetting
            {
                FieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield1,
                FormatFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield1formatstring,
                PrependStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield1prepend,
                PrependStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield1prependspaced,
                AppendStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield1append,
                AppendStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield1appendspaced
            },
            new ConcatenateFieldSetting
            {
                FieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield2,
                FormatFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield2formatstring,
                PrependStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield2prepend,
                PrependStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield2prependspaced,
                AppendStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield2append,
                AppendStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield2appendspaced
            },
            new ConcatenateFieldSetting
            {
                FieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield3,
                FormatFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield3formatstring,
                PrependStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield3prepend,
                PrependStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield3prependspaced,
                AppendStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield3append,
                AppendStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield3appendspaced
            },
            new ConcatenateFieldSetting
            {
                FieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield4,
                FormatFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield4formatstring,
                PrependStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield4prepend,
                PrependStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield4prependspaced,
                AppendStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield4append,
                AppendStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield4appendspaced
            },
            new ConcatenateFieldSetting
            {
                FieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield5,
                FormatFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield5formatstring,
                PrependStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield5prepend,
                PrependStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield5prependspaced,
                AppendStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield5append,
                AppendStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield5appendspaced
            },
            new ConcatenateFieldSetting
            {
                FieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield6,
                FormatFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield6formatstring,
                PrependStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield6prepend,
                PrependStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield6prependspaced,
                AppendStringFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield6append,
                AppendStringSpacedFieldName = Fields.jmcg_calculatedfield_.jmcg_concatenatefield6appendspaced
            },
        };
    }
}