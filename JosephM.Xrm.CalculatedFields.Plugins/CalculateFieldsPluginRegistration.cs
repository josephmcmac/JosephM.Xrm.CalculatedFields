using JosephM.Xrm.CalculatedFields.Plugins.Core;
using JosephM.Xrm.CalculatedFields.Plugins.Localisation;
using JosephM.Xrm.CalculatedFields.Plugins.Plugins;
using JosephM.Xrm.CalculatedFields.Plugins.Services;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.CalculatedFields.Plugins
{
    /// <summary>
    /// This is the class for registering plugins in CRM
    /// Each entity plugin type needs to be instantiated in the CreateEntityPlugin method
    /// </summary>
    public class CalculateFieldsPluginRegistration : XrmPluginRegistration
    {
        private readonly string _unsecureConfiguration;

        public CalculateFieldsPluginRegistration(string unsecure)
        {
            _unsecureConfiguration = unsecure;
        }

        private bool _loadedConfig;

        private object _lockObject = new object();

        public override XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship, IServiceProvider serviceProvider)
        {
            lock (_lockObject)
            {
                if (!_loadedConfig)
                {
                    var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                    var xrmService = new XrmService(factory.CreateOrganizationService(context.UserId), new LogController());
                    var calculatedService = new CalculatedService(xrmService, new CalculatedSettings(xrmService), new LocalisationService(new LocalisationSettings(xrmService, context.InitiatingUserId)));

                    var loadedToConfigs = calculatedService.DeserialiseEntities(_unsecureConfiguration)
                        .Select(calculatedService.LoadCalculatedFieldConfig)
                        .ToArray();

                    var ordered = new List<CalculatedFieldsConfig>();
                    foreach(var config in loadedToConfigs)
                    {
                        var i = 0;
                        foreach(var added in ordered.ToArray())
                        {
                            //ensure we add before this item if our target is one of its dependent fields
                            var thisTargetField = config.CalculatedFieldEntity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_field);
                            var dependentFields = calculatedService.GetDependencyFields(added, entityType);
                            if(dependentFields.Contains(thisTargetField))
                            {
                                ordered.Insert(i, config);
                                break;
                            }
                            i++;
                        }
                        if(!ordered.Contains(config))
                        {
                            ordered.Add(config);
                        }
                    }

                    Configs = ordered;

                    _loadedConfig = true;
                }
            }
            return new CalculateFieldsPlugin(Configs);
        }

        private IEnumerable<CalculatedFieldsConfig> Configs { get; set; }
    }
}
