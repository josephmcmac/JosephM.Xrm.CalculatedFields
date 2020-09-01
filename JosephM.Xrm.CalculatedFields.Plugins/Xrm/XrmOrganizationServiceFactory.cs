using Microsoft.Xrm.Sdk;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins.Xrm
{
    public class XrmOrganizationServiceFactory
    {
        public IOrganizationService GetOrganisationService(IXrmConfiguration xrmConfiguration)
        {
            if (!xrmConfiguration.UseXrmToolingConnector)
            {
                return XrmConnection.GetOrgServiceProxy(xrmConfiguration);
            }
            else
            {
                throw new NotSupportedException("Tooling Conenction Not Supported In This Project");
            }
        }
    }
}