using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Services;
using JosephM.Xrm.CalculatedFields.Plugins.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                    _localisationService = new LocalisationService(new LocalisationSettings(XrmService));
                return _localisationService;
            }
        }
    }
}