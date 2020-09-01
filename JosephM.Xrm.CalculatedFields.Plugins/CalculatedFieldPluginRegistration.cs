using JosephM.Xrm.CalculatedFields.Plugins.Plugins;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Schema;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins
{
    /// <summary>
    /// This is the class for registering plugins in CRM
    /// Each entity plugin type needs to be instantiated in the CreateEntityPlugin method
    /// </summary>
    public class CalculatedFieldPluginRegistration : XrmPluginRegistration
    {
        public override XrmPlugin CreateEntityPlugin(string entityType, bool isRelationship, IServiceProvider serviceProvider)
        {
            switch (entityType)
            {
                case Entities.jmcg_calculatedfield: return new CalculatedFieldPlugin();
            }
            return null;
        }
    }
}
