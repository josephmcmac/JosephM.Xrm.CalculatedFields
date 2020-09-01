using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace JosephM.Xrm.CalculatedFields.Plugins.Services
{
    public class CalculatedFieldsConfig
    {
        public Entity CalculatedFieldEntity { get; set; }
        public FilterExpression FilterExpression { get; set; }
    }
}
