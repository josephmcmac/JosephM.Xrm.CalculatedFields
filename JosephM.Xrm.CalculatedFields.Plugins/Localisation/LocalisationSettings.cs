﻿using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins.Localisation
{
    public class LocalisationSettings : ILocalisationSettings
    {
        public LocalisationSettings(XrmService xrmService, Guid userId)
        {
            XrmService = xrmService;
            UserId = userId;
        }

        public string TargetTimeZoneId
        {
            get
            {
                return TimeZone.GetStringField(Fields.timezonedefinition_.standardname);
            }
        }

        private int? _userTimeZoneCode;
        private int UserTimeZoneCode
        {
            get
            {
                if (!_userTimeZoneCode.HasValue)
                {
                    var userSettingsQuery = new QueryExpression(Entities.usersettings);
                    userSettingsQuery.ColumnSet = new ColumnSet(Fields.usersettings_.timezonecode);
                    var userJoin = userSettingsQuery.AddLink(Entities.systemuser, Fields.usersettings_.systemuserid, Fields.systemuser_.systemuserid);
                    userJoin.LinkCriteria.AddCondition(Fields.systemuser_.systemuserid, ConditionOperator.Equal, UserId);
                    var userSettings = XrmService.RetrieveFirst(userSettingsQuery);
                    if (userSettings == null)
                        throw new NullReferenceException($"Error getting {XrmService.GetEntityDisplayName(Entities.usersettings)} for user: {UserId}");
                    if (userSettings.GetField(Fields.usersettings_.timezonecode) == null)
                        throw new NullReferenceException($"Error {XrmService.GetFieldLabel(Fields.usersettings_.timezonecode, Entities.usersettings)} is empty in the {XrmService.GetEntityDisplayName(Entities.usersettings)} record");


                    _userTimeZoneCode = userSettings.GetInt(Fields.usersettings_.timezonecode);
                }
                return _userTimeZoneCode.Value;
            }
        }

        private Entity _timeZone;
        private Entity TimeZone
        {
            get
            {
                if (_timeZone == null)
                {
                    _timeZone = XrmService.GetFirst(Entities.timezonedefinition, Fields.timezonedefinition_.timezonecode, UserTimeZoneCode, new[] { Fields.timezonedefinition_.standardname });
                }
                return _timeZone;
            }
        }

        public XrmService XrmService { get; private set; }
        public Guid UserId { get; }
    }
}
