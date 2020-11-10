using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Services;
using JosephM.Xrm.CalculatedFields.Plugins.SharePoint;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;

namespace JosephM.Xrm.CalculatedFields.Plugins.Plugins
{
    /// <summary>
    /// class for shared services or settings objects for plugins
    /// </summary>
    public abstract class CalculatedEntityPluginBase : XrmEntityPlugin
    {
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
                {
                    _localisationService = new LocalisationService(new LocalisationSettings(XrmService, Context.InitiatingUserId));
                }
                return _localisationService;
            }
        }
    }
}
