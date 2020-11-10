using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Services;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;

namespace JosephM.Xrm.CalculatedFields.Plugins.Workflows
{
    /// <summary>
    /// class for shared services or settings objects for workflow activities
    /// </summary>
    public abstract class CalculatedWorkflowActivity<T> : XrmWorkflowActivityInstance<T>
        where T : XrmWorkflowActivityRegistration
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
                    _localisationService = new LocalisationService(new LocalisationSettings(XrmService, InitiatingUserId));
                }
                return _localisationService;
            }
        }
    }
}
