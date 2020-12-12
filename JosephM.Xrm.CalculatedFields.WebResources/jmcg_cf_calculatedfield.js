/// <reference path="jmcg_cf_pageutility.js" />
/// <reference path="jmcg_cf_webserviceutility.js" />

CalculatedJs = new Object();
CalculatedJs.options = {
    Type: {
        AddTime: 0,
        Concatenate: 1,
        Rollup: 2,
        TimeTaken: 3
    },
    RollupType: {
        Count: 0,
        Exists: 1,
        First: 2,
        Max: 3,
        Mean: 4,
        Min: 5,
        SeparatedStrings: 6,
        Sum: 7
    },
    SeparatorType: {
        Comma: 0,
        Hyphen: 1,
        NewLine: 2,
        OtherString: 5,
        Pipe: 3,
        Space: 4
    },
    TimeType: {
        Days: 2,
        Hours: 1,
        Minutes: 0,
        Months: 3,
        WorkDays: 6,
        WorkHours: 5,
        WorkMinutes: 4
    },
    TimeTakenMeasure: {
        Days: 2,
        Hours: 1,
        Minutes: 0,
        WorkDays: 5,
        WorkHours: 4,
        WorkMinutes: 3
    }
};


CalculatedJs.RunOnLoad = function () {
    calculatedPageUtility.CommonForm(CalculatedJs.RunOnChange, CalculatedJs.RunOnSave);

    CalculatedJs.PopulateTypeLists(["jmcg_entitytypeselectionfield", "jmcg_entitytyperolledupselectionfield"]);
    CalculatedJs.AddFieldSelectionPicklist(null, "jmcg_entitytype", "jmcg_fieldselectionfield", "jmcg_field", ["String", "Integer", "Money", "Decimal", "Double", "Boolean", "Memo", "DateTime"]);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Rollup, "jmcg_entitytyperolledup", "jmcg_fieldreferencingselectionfield", "jmcg_fieldreferencing",["Lookup", "Customer", "Owner"]);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Rollup, "jmcg_entitytyperolledup", "jmcg_fieldrolledupselectionfield", "jmcg_fieldrolledup", ["String", "Integer", "Money", "Decimal", "Double", "Boolean", "UniqueIdentifier", "Lookup", "Memo", "DateTime", "Lookup", "Customer", "Owner" ]);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Rollup, "jmcg_entitytyperolledup", "jmcg_orderrollupbyfieldselectionfield", "jmcg_orderrollupbyfield", null);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Concatenate, "jmcg_entitytype", "jmcg_concatenatefield1selectionfield", "jmcg_concatenatefield1", null);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Concatenate, "jmcg_entitytype", "jmcg_concatenatefield2selectionfield", "jmcg_concatenatefield2", null);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Concatenate, "jmcg_entitytype", "jmcg_concatenatefield3selectionfield", "jmcg_concatenatefield3", null);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Concatenate, "jmcg_entitytype", "jmcg_concatenatefield4selectionfield", "jmcg_concatenatefield4", null);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.Concatenate, "jmcg_entitytype", "jmcg_concatenatefield5selectionfield", "jmcg_concatenatefield5", null);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.AddTime, "jmcg_entitytype", "jmcg_addtimetofieldselectionfield", "jmcg_addtimetofield", ["DateTime"]);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.TimeTaken, "jmcg_entitytype", "jmcg_timetakenstartfieldselectionfield", "jmcg_timetakenstartfield", ["DateTime"]);
    CalculatedJs.AddFieldSelectionPicklist(CalculatedJs.options.Type.TimeTaken, "jmcg_entitytype", "jmcg_timetakenendfieldselectionfield", "jmcg_timetakenendfield", ["DateTime"]);

    CalculatedJs.RefreshVisibility();
};

CalculatedJs.RunOnChange = function (fieldName) {
    switch (fieldName) {
        case "jmcg_type":
            CalculatedJs.RefreshVisibility();
            CalculatedJs.InitialiseFetchFilter();
            break;
        case "jmcg_rolluptype":
            CalculatedJs.RefreshVisibility();
            break;
        case "jmcg_separatortype":
            CalculatedJs.RefreshVisibility();
            break;
        case "jmcg_timetype":
            CalculatedJs.RefreshVisibility();
            break;
        case "jmcg_timetakenmeasure":
            CalculatedJs.RefreshVisibility();
            break;
        case "jmcg_entitytypeselectionfield":
            CalculatedJs.SetEntitySelection("jmcg_entitytypeselectionfield", "jmcg_entitytype");
            break;
        case "jmcg_entitytyperolledupselectionfield":
            CalculatedJs.SetEntitySelection("jmcg_entitytyperolledupselectionfield", "jmcg_entitytyperolledup");
            break;
        case "jmcg_entitytype":
            CalculatedJs.RefreshVisibility();
            break;
        case "jmcg_entitytyperolledup":
            CalculatedJs.RefreshVisibility();
            break;
    }
};

CalculatedJs.RunOnSave = function () {
};

CalculatedJs.RefreshVisibility = function () {
    var entityTypePopulated = calculatedPageUtility.GetFieldValue("jmcg_entitytype") != null;
    calculatedPageUtility.SetFieldDisabled("jmcg_fieldselectionfield", !entityTypePopulated);
    var type = calculatedPageUtility.GetFieldValue("jmcg_type");
    var isRollup = type == CalculatedJs.options.Type.Rollup;
    calculatedPageUtility.SetFieldVisibility("jmcg_rolluptype", isRollup);
    calculatedPageUtility.SetFieldMandatory("jmcg_rolluptype", isRollup);
    calculatedPageUtility.SetSectionVisibility("secRollup", isRollup);
    calculatedPageUtility.SetFieldMandatory("jmcg_entitytyperolledup", isRollup);
    calculatedPageUtility.SetFieldMandatory("jmcg_fieldreferencing", isRollup);
    calculatedPageUtility.SetFieldMandatory("jmcg_fieldrolledup", isRollup);
    var isConcatenate = type == CalculatedJs.options.Type.Concatenate;
    calculatedPageUtility.SetSectionVisibility("secConcatenate", isConcatenate);
    calculatedPageUtility.SetFieldMandatory("jmcg_concatenatefield1", isConcatenate);
    var rollupType = calculatedPageUtility.GetFieldValue("jmcg_rolluptype");
    var rollupSeparatedString = isRollup && rollupType == CalculatedJs.options.RollupType.SeparatedStrings;
    var separatorInContext = isConcatenate || rollupSeparatedString;
    calculatedPageUtility.SetSectionVisibility("secSeparator", separatorInContext);
    calculatedPageUtility.SetFieldMandatory("jmcg_separatortype", separatorInContext);
    var isOtherSeparator = calculatedPageUtility.GetFieldValue("jmcg_separatortype") == CalculatedJs.options.SeparatorType.OtherString;
    calculatedPageUtility.SetFieldVisibility("jmcg_separatorstring", separatorInContext && isOtherSeparator);
    calculatedPageUtility.SetFieldMandatory("jmcg_separatorstring", separatorInContext && isOtherSeparator);
    var rollupFirst = isRollup && rollupType == CalculatedJs.options.RollupType.First;
    calculatedPageUtility.SetFieldVisibility("jmcg_orderrollupbyfieldselectionfield", rollupFirst);
    calculatedPageUtility.SetFieldMandatory("jmcg_orderrollupbyfieldselectionfield", rollupFirst);
    calculatedPageUtility.SetFieldVisibility("jmcg_orderrollupbyfield", rollupFirst);
    calculatedPageUtility.SetFieldMandatory("jmcg_orderrollupbyfield", rollupFirst);
    calculatedPageUtility.SetFieldVisibility("jmcg_orderrollupbyfieldordertype", rollupFirst);
    calculatedPageUtility.SetFieldMandatory("jmcg_orderrollupbyfieldordertype", rollupFirst);
    var isAddTime = type == CalculatedJs.options.Type.AddTime;
    calculatedPageUtility.SetSectionVisibility("secAddTime", isAddTime);
    calculatedPageUtility.SetFieldMandatory("jmcg_timetype", isAddTime);
    calculatedPageUtility.SetFieldMandatory("jmcg_timeamount", isAddTime);
    var isTimeTaken = type == CalculatedJs.options.Type.TimeTaken;
    calculatedPageUtility.SetSectionVisibility("secTimeTaken", isTimeTaken);
    calculatedPageUtility.SetFieldMandatory("jmcg_timetakenstartfield", isTimeTaken);
    calculatedPageUtility.SetFieldMandatory("jmcg_timetakenendfield", isTimeTaken);
    calculatedPageUtility.SetFieldMandatory("jmcg_timetakenmeasure", isTimeTaken);
    var timeType = calculatedPageUtility.GetFieldValue("jmcg_timetype");
    var isWorkTimeType = timeType == CalculatedJs.options.TimeType.WorkDays
        || timeType == CalculatedJs.options.TimeType.WorkHours
        || timeType == CalculatedJs.options.TimeType.WorkMinutes;
    var timeTakenMeasure = calculatedPageUtility.GetFieldValue("jmcg_timetakenmeasure");
    var isWorkTimeMeasure = timeTakenMeasure == CalculatedJs.options.TimeTakenMeasure.WorkDays
        || timeTakenMeasure == CalculatedJs.options.TimeTakenMeasure.WorkHours
        || timeTakenMeasure == CalculatedJs.options.TimeTakenMeasure.WorkMinutes;
    calculatedPageUtility.SetSectionVisibility("secWorkCalendar", isWorkTimeType || isWorkTimeMeasure);
    calculatedPageUtility.SetFieldMandatory("jmcg_calendarid", isWorkTimeType || isWorkTimeMeasure);

};

CalculatedJs.AddFieldSelectionPicklist = function (calculationType, entityField, fieldSelectionField, targetField, validTypes) {
    var selectionConfig = {
        entityField: entityField,
        fieldSelectionField: fieldSelectionField,
        targetField: targetField,
        validTypes: validTypes,
        calculationType: calculationType,
        loadSelectionsFunction: function () {
            var entityType = calculatedPageUtility.GetFieldValue(entityField);
            var processResults = function (results) {
                var newArray = new Array();
                var ignoreFields = [];
                for (var j = 0; j < results.length; j++) {
                    if (validTypes == null || calculatedPageUtility.ArrayContains(validTypes, results[j].FieldType)
                        && !calculatedPageUtility.ArrayContains(ignoreFields, results[j].LogicalName)
                        && results[j].Createable == true) {
                        newArray.push(results[j]);
                    }
                }
                CalculatedJs.FieldLists[fieldSelectionField] = newArray;
                var fieldOptions = new Array();
                fieldOptions.push(new calculatedPageUtility.PicklistOption(0, "Select to change the field below"));
                for (var i = 1; i <= CalculatedJs.FieldLists[fieldSelectionField].length; i++) {
                    fieldOptions.push(new calculatedPageUtility.PicklistOption(i, CalculatedJs.FieldLists[fieldSelectionField][i - 1]["DisplayName"]));
                }
                calculatedPageUtility.SetPicklistOptions(fieldSelectionField, fieldOptions);
                calculatedPageUtility.SetFieldValue(fieldSelectionField, 0);
            };
            if (entityType != null) {
                calculatedServiceUtility.GetFieldMetadata(entityType, processResults);
            }
        },
        applySelected: function () {
            var selectedoption = Xrm.Page.getAttribute(fieldSelectionField).getSelectedOption();
            if (selectedoption != null && parseInt(selectedoption.value) != 0) {
                var value = selectedoption.value;
                var selectedField = CalculatedJs.FieldLists[fieldSelectionField][parseInt(value) - 1];
                var selectedFieldName = selectedField["LogicalName"];
                calculatedPageUtility.SetFieldValue(targetField, selectedFieldName);
                calculatedPageUtility.SetFieldValue(fieldSelectionField, 0);
            }
        },
        setFieldEnabled: function () {
            var entityFieldPopulated = calculatedPageUtility.GetFieldValue(entityField) != null
                && calculatedPageUtility.GetFieldValue(entityField) != "";
            var correctCalculationType = calculationType == null || calculatedPageUtility.GetFieldValue("jmcg_type") == calculationType;
            calculatedPageUtility.SetFieldDisabled(fieldSelectionField, !(entityFieldPopulated && correctCalculationType));
        }
    };

    selectionConfig.loadSelectionsFunction();
    calculatedPageUtility.AddOnChange(entityField, selectionConfig.setFieldEnabled);
    calculatedPageUtility.AddOnChange("jmcg_type", selectionConfig.setFieldEnabled);
    calculatedPageUtility.AddOnChange(entityField, selectionConfig.loadSelectionsFunction);
    calculatedPageUtility.AddOnChange(fieldSelectionField, selectionConfig.applySelected);
};

CalculatedJs.InitialiseFetchFilter = function () {
    var initialFetchFilter = '<filter type="and">\n  <condition attribute="statecode" operator="eq" value="0" />\n</filter>';
    var type = calculatedPageUtility.GetFieldValue("jmcg_type");
    var isRollup = type == CalculatedJs.options.Type.Rollup;
    var filter = calculatedPageUtility.GetFieldValue("jmcg_rollupfilter");
    if (isRollup && (filter == null || filter == "")) {
        calculatedPageUtility.SetFieldValue("jmcg_rollupfilter", initialFetchFilter);
    }
}

CalculatedJs.EntityTypes = null;
CalculatedJs.PopulateTypeLists = function (fields) {
    var compare = function (a, b) {
        if (a.DisplayName < b.DisplayName)
            return -1;
        if (a.DisplayName > b.DisplayName)
            return 1;
        return 0;
    };

    var processResults = function (results) {
        results.sort(compare);
        CalculatedJs.EntityTypes = results;
        var entityOptions = new Array();
        entityOptions.push(new calculatedPageUtility.PicklistOption(0, "Select to change the selected entity type"));
        for (var i = 1; i <= results.length; i++) {
            entityOptions.push(new calculatedPageUtility.PicklistOption(i, results[i - 1]["DisplayName"]));
        }
        for (var j = 0; j <= fields.length; j++) {
            calculatedPageUtility.SetPicklistOptions(fields[j], entityOptions);
            calculatedPageUtility.SetFieldValue(fields[j], 0);
        }
    };
    calculatedServiceUtility.GetAllEntityMetadata(processResults);
};

CalculatedJs.SetEntitySelection = function (selectionField, entityField) {
    var selectedoption = Xrm.Page.getAttribute(selectionField).getSelectedOption();
    if (selectedoption != null && parseInt(selectedoption.value) != 0) {
        var value = selectedoption.value;
        var selectedEntity = CalculatedJs.EntityTypes[parseInt(value) - 1];
        var selectedEntityName = selectedEntity["LogicalName"];
        calculatedPageUtility.SetFieldValue(entityField, selectedEntityName);
        calculatedPageUtility.SetFieldValue(selectionField, 0);
    }
};

CalculatedJs.FieldLists = new Array();
CalculatedJs.PopulateFieldsList = function (entityField, fieldSelectionField, validTypes) {
    var entityType = calculatedPageUtility.GetFieldValue(entityField);
    var processResults = function (results) {
        var newArray = new Array();
        var ignoreFields = [];
        for (var j = 0; j < results.length; j++) {
            if (calculatedPageUtility.ArrayContains(validTypes, results[j].FieldType)
                && !calculatedPageUtility.ArrayContains(ignoreFields, results[j].LogicalName)
                && results[j].Createable == true) {
                newArray.push(results[j]);
            }
        }
        CalculatedJs.FieldLists[fieldSelectionField] = newArray;
        var fieldOptions = new Array();
        fieldOptions.push(new calculatedPageUtility.PicklistOption(0, "Select to change the field below"));
        for (var i = 1; i <= CalculatedJs.FieldLists[fieldSelectionField].length; i++) {
            fieldOptions.push(new calculatedPageUtility.PicklistOption(i, CalculatedJs.FieldLists[fieldSelectionField][i - 1]["DisplayName"]));
        }
        calculatedPageUtility.SetPicklistOptions(fieldSelectionField, fieldOptions);
        calculatedPageUtility.SetFieldValue(fieldSelectionField, 0);
    };
    if (entityType != null) {
        calculatedServiceUtility.GetFieldMetadata(entityType, processResults);
    }
};