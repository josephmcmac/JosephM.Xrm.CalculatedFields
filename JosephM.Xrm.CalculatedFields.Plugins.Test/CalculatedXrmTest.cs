using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Services;
using JosephM.Xrm.CalculatedFields.Plugins.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    [TestClass]
    public class CalculatedXrmTest : XrmTest
    {
        //USE THIS IF NEED TO VERIFY SCRIPTS FOR A PARTICULAR SECURITY ROLE
        //private XrmService _xrmService;
        //public override XrmService XrmService
        //{
        //    get
        //    {
        //        if (_xrmService == null)
        //        {
        //            var xrmConnection = new XrmConfiguration()
        //            {
        //                AuthenticationProviderType = XrmConfiguration.AuthenticationProviderType,
        //                DiscoveryServiceAddress = XrmConfiguration.DiscoveryServiceAddress,
        //                OrganizationUniqueName = XrmConfiguration.OrganizationUniqueName,
        //                Username = "",
        //                Password = ""
        //            };
        //            _xrmService = new XrmService(xrmConnection);
        //        }
        //        return _xrmService;
        //    }
        //}

        protected override IEnumerable<string> EntitiesToDelete
        {
            get
            {
                return new string[0];
            }
        }

        private CalculatedSettings _settings;
        public CalculatedSettings CalculatedSettings
        {
            get
            {
                if (_settings == null)
                    _settings = new CalculatedSettings(XrmService);
                return _settings;
            }
        }

        private CalculatedService _service;
        public CalculatedService CalculatedService
        {
            get
            {
                if (_service == null)
                    _service = new CalculatedService(XrmService, CalculatedSettings, LocalisationService);
                return _service;
            }
        }

        private CalculatedSharepointService _sharePointService;
        public CalculatedSharepointService CalculatedSharepointService
        {
            get
            {
                if (_sharePointService == null)
                    _sharePointService = new CalculatedSharepointService(XrmService, new CalculatedSharePointSettings(XrmService));
                return _sharePointService;
            }
        }

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                    _localisationService = new LocalisationService(new LocalisationSettings(XrmService, CurrentUserId));
                return _localisationService;
            }
        }

        public Guid ServiceCalendarId => new Guid("6a2c3c2f-a2ec-ea11-8143-000c290a70aa");
        //var calendar = XrmService.Retrieve(Entities.calendar, calendarId);//var calendarRules = ((EntityCollection)calendar.GetField("calendarrules")).Entities;//calendarRules.First().SetField(Fields.calendarrule_.starttime, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));//XrmService.Update(calendar);
    }
}