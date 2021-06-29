using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JosephM.Xrm.CalculatedFields.Plugins.Rollups
{
    public abstract class RollupService
    {
        public XrmService XrmService { get; }

        public abstract IEnumerable<LookupRollup> AllRollups { get; }

        public RollupService(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        public void SetInitialValues(XrmEntityPlugin plugin)
        {
            //a count Rollup should be initialised to zero when the record created
            if (plugin.MessageName == PluginMessage.Create
                && plugin.Stage == PluginStage.PreOperationEvent)
            {
                var RollupsToProcess = GetRollupsForRollupType(plugin.TargetType)
                    .Where(a => AllowsDifferenceChange(a.RollupType))
                    .ToArray();
                foreach (var Rollup in RollupsToProcess)
                {
                    if (Rollup.NullAmount != null)
                        plugin.SetField(Rollup.RollupField, Rollup.NullAmount);
                }
            }
        }

        public void RefreshRollup(Guid id, LookupRollup rollup)
        {
            var newValue = GetRollup(rollup, id);
            XrmService.SetFieldIfChanging(rollup.RecordTypeWithRollup, id, rollup.RollupField, newValue);
        }

        private bool AllowsDifferenceChange(RollupType type)
        {
            return new[] { RollupType.Count, RollupType.Sum }.Contains(type);
        }

        /// <summary>
        /// Processes plugin for a type type rolled up
        /// </summary>
        /// <param name="plugin"></param>
        public void ExecuteRollupPlugin(XrmEntityPlugin plugin)
        {
            if (plugin.IsMessage(PluginMessage.Create, PluginMessage.Update, PluginMessage.Delete)
                && plugin.IsStage(PluginStage.PostEvent)
                && plugin.IsMode(PluginMode.Synchronous))
            {
                var rollupsToProcess = GetRollupsForRolledupType(plugin.TargetType).ToArray();
                var dictionaryForDifferences = new Dictionary<string, Dictionary<Guid, List<UpdateMeta>>>();

                Action<string, Guid, string, object, LookupRollup> addValueToApply = (type, id, field, val, rollup) =>
                {
                    if (!dictionaryForDifferences.ContainsKey(type))
                        dictionaryForDifferences.Add(type, new Dictionary<Guid, List<UpdateMeta>>());
                    if (!dictionaryForDifferences[type].ContainsKey(id))
                        dictionaryForDifferences[type].Add(id, new List<UpdateMeta>());
                    dictionaryForDifferences[type][id].Add(new UpdateMeta(field, rollup, val));
                };

                foreach (var rollup in rollupsToProcess)
                {
                    //capture required facts in the plugin context to process our ifs and elses
                    var metConditionsBefore = XrmEntity.MeetsFilter(plugin.GetFieldFromPreImage, rollup.Filter);
                    var meetsConditionsAfter = plugin.MessageName == PluginMessage.Delete
                        ? false
                        : XrmEntity.MeetsFilter(plugin.GetField, rollup.Filter);
                    var linkedIdBefore = XrmEntity.GetLookupType(plugin.GetFieldFromPreImage(rollup.LookupName)) == rollup.RecordTypeWithRollup
                        ? plugin.GetLookupGuidPreImage(rollup.LookupName)
                        : null;
                    var linkedIdAfter = plugin.MessageName == PluginMessage.Delete || XrmEntity.GetLookupType(plugin.GetField(rollup.LookupName)) != rollup.RecordTypeWithRollup
                        ? null
                        : plugin.GetLookupGuid(rollup.LookupName);
                    var isValueChanging = rollup.FieldRolledup != null && plugin.FieldChanging(rollup.FieldRolledup);
                    var isOrderByChanging = rollup.OrderByField != null && plugin.FieldChanging(rollup.OrderByField);

                    if (AllowsDifferenceChange(rollup.RollupType))
                    {
                        //this covers scenarios which require changing the value in a parent record
                        if (linkedIdBefore.HasValue && linkedIdBefore == linkedIdAfter)
                        {
                            //the same record linked before and after
                            if (metConditionsBefore && meetsConditionsAfter)
                            {
                                //if part of Rollup before and after
                                if (isValueChanging)
                                {
                                    //and the value is changing then apply difference
                                    if (rollup.RollupType == RollupType.Sum)
                                    {
                                        addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, GetDifferenceToApply(plugin.GetFieldFromPreImage(rollup.FieldRolledup), plugin.GetField(rollup.FieldRolledup)), rollup);
                                    }
                                    else if (rollup.RollupType == RollupType.Count)
                                    {
                                        //for count only adjust if changing between null and not null
                                        if (plugin.GetFieldFromPreImage(rollup.FieldRolledup) == null)
                                        {
                                            addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, 1, rollup);
                                        }
                                        else if (plugin.GetField(rollup.FieldRolledup) == null)
                                        {
                                            addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, -1, rollup);
                                        }
                                    }
                                }
                            }
                            if (!metConditionsBefore && meetsConditionsAfter)
                            {
                                //if was not part of Rollup before but is now apply the entire value
                                if (rollup.RollupType == RollupType.Sum)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, plugin.GetField(rollup.FieldRolledup), rollup);
                                }
                                else if (rollup.RollupType == RollupType.Count)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, 1, rollup);
                                }
                            }
                            if (metConditionsBefore && !meetsConditionsAfter)
                            {
                                //if was part of Rollup before but not now apply the entire value negative
                                if (rollup.RollupType == RollupType.Sum)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, GetNegative(plugin.GetFieldFromPreImage(rollup.FieldRolledup)), rollup);
                                }
                                else if (rollup.RollupType == RollupType.Count)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, -1, rollup);
                                }
                            }
                        }
                        else
                        {
                            //different parent linked before and after
                            if (linkedIdBefore.HasValue && metConditionsBefore)
                            {
                                //if was part of previous linked records Rollup then negate the previous value
                                if (rollup.RollupType == RollupType.Sum)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdBefore.Value, rollup.RollupField, GetNegative(plugin.GetFieldFromPreImage(rollup.FieldRolledup)), rollup);
                                }
                                else if (rollup.RollupType == RollupType.Count)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdBefore.Value, rollup.RollupField, -1, rollup);
                                }
                            }
                            if (linkedIdAfter.HasValue && meetsConditionsAfter)
                            {
                                //if part of new linked records Rollup then apply the entire value
                                if (rollup.RollupType == RollupType.Sum)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, plugin.GetField(rollup.FieldRolledup), rollup);
                                }
                                else if (rollup.RollupType == RollupType.Count)
                                {
                                    addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, 1, rollup);
                                }
                            }
                        }
                    }
                    else
                    {
                        //these ones just recalculate on the parent record(s)
                        var isDependencyChanging = false;
                        switch (plugin.MessageName)
                        {
                            case PluginMessage.Delete:
                                {
                                    isDependencyChanging = linkedIdBefore.HasValue && metConditionsBefore;
                                    break;
                                }
                            case PluginMessage.Update:
                                {
                                    if (linkedIdBefore != linkedIdAfter || isValueChanging)
                                        isDependencyChanging = true;
                                    else if (isOrderByChanging)
                                        isDependencyChanging = true;
                                    else
                                        isDependencyChanging = metConditionsBefore != meetsConditionsAfter;
                                    break;
                                }
                            case PluginMessage.Create:
                                {
                                    isDependencyChanging = linkedIdAfter.HasValue
                                        && (rollup.FieldRolledup == null || isValueChanging)
                                        && meetsConditionsAfter;
                                    break;
                                }
                        }
                        if (isDependencyChanging)
                        {
                            var processPreReferenced = false;
                            var processPostReferenced = false;
                            //If they aren't the same do both
                            if (!XrmEntity.FieldsEqual(linkedIdBefore, linkedIdAfter))
                            {
                                processPreReferenced = true;
                                processPostReferenced = true;
                            }
                            //else just do the first not null one
                            else
                            {
                                if (linkedIdBefore.HasValue)
                                    processPreReferenced = true;
                                else
                                    processPostReferenced = true;
                            }
                            if (processPreReferenced && linkedIdBefore.HasValue)
                            {
                                addValueToApply(rollup.RecordTypeWithRollup, linkedIdBefore.Value, rollup.RollupField, null, rollup);
                            }
                            if (processPostReferenced && linkedIdAfter.HasValue)
                            {
                                addValueToApply(rollup.RecordTypeWithRollup, linkedIdAfter.Value, rollup.RollupField, null, rollup);
                            }
                        }
                    }
                }

                //apply all required changes to parents we captured
                //type -> ids -> fields . values
                foreach (var item in dictionaryForDifferences)
                {
                    var targetType = item.Key;
                    foreach (var idUpdates in item.Value)
                    {
                        var id = idUpdates.Key;
                        //lock the parent record then retreive it
                        plugin.XrmService.SetField(targetType, id, "modifiedon", DateTime.UtcNow);
                        var fieldsForUpdating = idUpdates.Value.Select(kv => kv.FieldName).ToArray();
                        var targetRecord = plugin.XrmService.Retrieve(targetType, id, idUpdates.Value.Select(kv => kv.FieldName));
                        //update the fields
                        foreach (var fieldUpdate in idUpdates.Value)
                        {
                            if (AllowsDifferenceChange(fieldUpdate.Rollup.RollupType))
                            {
                                targetRecord.SetField(fieldUpdate.FieldName, XrmEntity.SumFields(new[] { fieldUpdate.DifferenceValue, targetRecord.GetField(fieldUpdate.FieldName) }));
                            }
                            else
                            {
                                targetRecord.SetField(fieldUpdate.FieldName, GetRollup(fieldUpdate.Rollup, id));
                            }
                        }
                        plugin.XrmService.Update(targetRecord, fieldsForUpdating);
                    }
                }
            }
        }

        public class UpdateMeta
        {
            public UpdateMeta(string field, LookupRollup rollup, object differenceValue)
            {
                FieldName = field;
                DifferenceValue = differenceValue;
                Rollup = rollup;
            }
            public string FieldName { get; set; }
            public LookupRollup Rollup { get; set; }
            public object DifferenceValue { get; set; }
        }

        public IEnumerable<LookupRollup> GetRollupsForRolledupType(string entityType)
        {
            return AllRollups
                .Where(a => a.RecordTypeRolledup == entityType)
                .ToArray();
        }

        public IEnumerable<LookupRollup> GetRollupsForRollupType(string entityType)
        {
            return AllRollups
                .Where(a => a.RecordTypeWithRollup == entityType)
                .ToArray();
        }

        protected string FetchAlias
        {
            get { return "Rollupvalue"; }
        }

        public object GetRollup(LookupRollup rollup, Guid id)
        {
            object newValue = null;

            switch (rollup.RollupType)
            {
                case RollupType.Exists:
                    {
                        //if the Rollup returns a result > 0 then one exists
                        var fetch = GetLookupFetch(rollup, id);
                        var result = XrmService.Fetch(fetch);
                        newValue = result.Any() &&
                               XrmEntity.GetInt(result.First().GetField(FetchAlias)) > 0;
                        break;
                    }
                case RollupType.Count:
                    {
                        var result = XrmService.Fetch(GetLookupFetch(rollup, id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetField(FetchAlias);
                        break;
                    }
                case RollupType.Sum:
                    {
                        var result = XrmService.Fetch(GetLookupFetch(rollup, id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetField(FetchAlias);
                        break;
                    }
                case RollupType.Min:
                    {
                        var query = GetRollupQueryForLookup(rollup, id);
                        query.AddOrder(rollup.FieldRolledup, OrderType.Ascending);
                        var minRecord = XrmService.RetrieveFirst(query);
                        newValue = minRecord.GetField(rollup.FieldRolledup);
                        break;
                    }
                case RollupType.Max:
                    {
                        var query = GetRollupQueryForLookup(rollup, id);
                        query.AddOrder(rollup.FieldRolledup, OrderType.Descending);
                        var maxRecord = XrmService.RetrieveFirst(query);
                        newValue = maxRecord.GetField(rollup.FieldRolledup);
                        break;
                    }
                case RollupType.Mean:
                    {
                        var result = XrmService.Fetch(GetLookupFetch(rollup, id));
                        if (result.Any())
                            newValue = result.ElementAt(0).GetField(FetchAlias);
                        break;
                    }
                case RollupType.SeparatedStrings:
                    {
                        var query = GetRollupQueryForLookup(rollup, id);

                        query.AddOrder(rollup.FieldRolledup, OrderType.Ascending);
                        var records = XrmService.RetrieveAll(query);
                        var labels =
                            records.Select(e => e.GetField(rollup.FieldRolledup)).
                                ToArray();
                        newValue = string.Join(rollup.SeparatorString, labels);
                        break;
                    }
                case RollupType.First:
                    {
                        var query = GetRollupQueryForLookup(rollup, id);
                        query.AddOrder(rollup.OrderByField, rollup.OrderType);
                        query.AddOrder("createdon", OrderType.Descending);
                        var record = XrmService.RetrieveFirst(query);
                        newValue = record.GetField(rollup.FieldRolledup);
                        if(newValue is Guid g && rollup.ObjectType == typeof(EntityReference))
                        {
                            newValue = new EntityReference(rollup.RecordTypeRolledup, g);
                        }
                        break;
                    }
            }
            if (newValue == null && rollup.NullAmount != null)
                newValue = rollup.NullAmount;
            if (newValue != null && rollup.ObjectType != null)
            {
                if (rollup.ObjectType == typeof(decimal))
                {
                    newValue = Convert.ToDecimal(newValue.ToString());
                }
            }
            return newValue;
        }

        private QueryExpression GetRollupQueryForLookup(LookupRollup rollup, Guid id)
        {
            var query = new QueryExpression(rollup.RecordTypeRolledup);
           
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(rollup.LookupName, ConditionOperator.Equal, id);
            if (rollup.Filter != null)
            {
                query.Criteria.AddFilter(rollup.Filter);
            }
            query.ColumnSet.AddColumn(rollup.LookupName);
            if (rollup.FieldRolledup != null)
            {
                query.ColumnSet.AddColumn(rollup.FieldRolledup);
                query.Criteria.AddCondition(new ConditionExpression(rollup.FieldRolledup, ConditionOperator.NotNull));
            }
            return query;
        }

        public string GetLookupFetch(LookupRollup rollup, Guid id)
        {
            string RollupFieldNode;
            switch (rollup.RollupType)
            {
                case RollupType.Exists:
                    {
                        RollupFieldNode = "<attribute name=\"" + rollup.FieldRolledup +
                                             "\" aggregate=\"count\" distinct = \"true\" alias=\"" + FetchAlias + "\"/>";
                        break;
                    }
                case RollupType.Count:
                    {
                        RollupFieldNode = "<attribute name=\"" + rollup.FieldRolledup + "\" aggregate=\"count\" alias=\"" +
                                             FetchAlias + "\"/>";
                        break;
                    }
                case RollupType.Sum:
                    {
                        RollupFieldNode = "<attribute name=\"" + rollup.FieldRolledup + "\" aggregate=\"sum\" alias=\"" +
                                             FetchAlias + "\"/>";
                        break;
                    }
                case RollupType.Mean:
                    {
                        RollupFieldNode = "<attribute name=\"" + rollup.FieldRolledup + "\" aggregate=\"avg\" alias=\"" +
                                             FetchAlias + "\"/>";
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Fetch Rollup not implemented for " + rollup.RollupType);
                    }
            }
            return
                "<fetch version=\"1.0\" aggregate=\"true\" output-format=\"xml-platform\" mapping=\"logical\" distinct=\"false\">"
                + "<entity name=\"" + rollup.RecordTypeRolledup + "\">"
                + RollupFieldNode
                + "<filter type=\"and\">"
                + "<condition attribute=\"" + rollup.LookupName + "\" operator=\"eq\" uiname=\"\" uitype=\"" + rollup.RecordTypeWithRollup +
                "\" value=\"" + id + "\" />"
                + (rollup.FieldRolledup != null ? GetConditionFetchNode(new ConditionExpression(rollup.FieldRolledup, ConditionOperator.NotNull)) : null)
                + rollup.FilterXml
                + "</filter>"
                + "</entity>"
                + "</fetch>";
        }

        protected static string GetConditionFetchNode(ConditionExpression condition)
        {
            var conditionOperatorString = "";
            switch (condition.Operator)
            {
                case ConditionOperator.Equal:
                    {
                        conditionOperatorString = "eq";
                        break;
                    }
                case ConditionOperator.NotEqual:
                    {
                        conditionOperatorString = "ne";
                        break;
                    }
                case ConditionOperator.In:
                    {
                        conditionOperatorString = "in";

                        break;
                    }
                case ConditionOperator.OnOrBefore:
                    {
                        conditionOperatorString = "on-or-before";
                        break;
                    }
                case ConditionOperator.OnOrAfter:
                    {
                        conditionOperatorString = "on-or-after";
                        break;
                    }
                case ConditionOperator.NotNull:
                    {
                        conditionOperatorString = "not-null";
                        break;
                    }
            }
            if (string.IsNullOrWhiteSpace(conditionOperatorString))
                throw new InvalidPluginExecutionException(
                    string.Format("Error Getting Condition Operator String For Operator Type {0}",
                        condition.Operator));
            if (condition.Operator == ConditionOperator.In)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Format("<condition attribute=\"{0}\" operator=\"{1}\" >",
                    condition.AttributeName,
                    conditionOperatorString));
                if (condition.Values != null)
                {
                    foreach (var value in condition.Values)
                    {
                        if (value is IEnumerable<object>)
                        {
                            foreach (var nestValue in (IEnumerable<object>)value)
                            {
                                stringBuilder.Append(string.Format("<value>{0}</value>", nestValue));
                            }
                        }
                        else
                            stringBuilder.Append(string.Format("<value>{0}</value>", value));
                    }
                }
                stringBuilder.Append("</condition>");
                return stringBuilder.ToString();
            }
            if (condition.Values == null || condition.Values.Count == 0)
            {
                return string.Format("<condition attribute=\"{0}\" operator=\"{1}\" />", condition.AttributeName, conditionOperatorString);
            }
            else
            {
                var fetchValue = condition.Values[0];
                if (fetchValue is DateTime)
                {
                    if (condition.Operator == ConditionOperator.OnOrAfter
                        || condition.Operator == ConditionOperator.OnOrBefore)
                    {
                        fetchValue = ((DateTime)fetchValue).ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        fetchValue = ((DateTime)fetchValue).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }
                }

                return string.Format("<condition attribute=\"{0}\" operator=\"{1}\" value=\"{2}\" />", condition.AttributeName,
                    conditionOperatorString, fetchValue);
            }
        }

        private static object GetDifferenceToApply(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
            {
                return null;
            }
            if (oldValue is int || newValue is int)
            {
                var differenceCalc = newValue == null ? 0 : (int)newValue;
                if (oldValue != null)
                    differenceCalc = differenceCalc - (int)oldValue;
                return differenceCalc;
            }
            if (oldValue is decimal || newValue is decimal)
            {
                var differenceCalc = newValue == null ? 0 : (decimal)newValue;
                if (oldValue != null)
                    differenceCalc = differenceCalc - (decimal)oldValue;
                return differenceCalc;
            }
            if (oldValue is Money || newValue is Money)
            {
                var differenceCalc = newValue == null ? (decimal)0 : ((Money)newValue).Value;
                if (oldValue != null)
                    differenceCalc = differenceCalc - ((Money)oldValue).Value;
                return new Money(differenceCalc);
            }
            throw new NotImplementedException("The GetDifferenceToApply Method Is Not Implemented For The Type " + oldValue == null ? newValue.GetType().Name : oldValue.GetType().Name);
        }

        private static object GetNegative(object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is int eger)
            {
                return -1 * eger;
            }
            if (value is decimal places)
            {
                return -1 * places;
            }
            if (value is Money sotheysay)
            {
                return new Money(-1 * (sotheysay).Value);
            }
            if (value is double take)
            {
                return -1 * take;
            }
            throw new NotImplementedException("The GetNegative Method Is Not Implemented For The Type " + value.GetType().Name);
        }
    }
}