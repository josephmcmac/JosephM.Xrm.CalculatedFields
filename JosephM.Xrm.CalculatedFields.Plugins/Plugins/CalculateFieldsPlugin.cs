using JosephM.Xrm.CalculatedFields.Plugins.Services;
using System.Collections.Generic;

namespace JosephM.Xrm.CalculatedFields.Plugins.Plugins
{
    public class CalculateFieldsPlugin : CalculatedEntityPluginBase
    {
        public CalculateFieldsPlugin(IEnumerable<CalculatedFieldsConfig> configs)
        {
            Configs = configs;
        }

        public IEnumerable<CalculatedFieldsConfig> Configs { get; }

        public override void GoExtention()
        {
            CalculatedService.ApplyCalculations(this, Configs);
        }
    }
}