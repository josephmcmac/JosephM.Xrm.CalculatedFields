using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Services;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using System;

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

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                {
                    Guid? userId = null;
                    if (IsMessage(PluginMessage.Create))
                    {
                        userId = GetLookupGuid("createdonbehalfby");
                        if (!userId.HasValue)
                        {
                            userId = GetLookupGuid("createdby");
                        }
                    }
                    else if (IsMessage(PluginMessage.Update))
                    {
                        userId = GetLookupGuid("modifiedby");
                    }
                    if(!userId.HasValue)
                    {
                        userId = Context.InitiatingUserId;
                    }
                    _localisationService = new LocalisationService(new LocalisationSettings(XrmService, userId.Value));
                }
                return _localisationService;
            }
        }
    }
}
