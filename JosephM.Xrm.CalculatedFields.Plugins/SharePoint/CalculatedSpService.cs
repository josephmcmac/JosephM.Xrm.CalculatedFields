using System.Collections.Generic;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;

namespace JosephM.Xrm.CalculatedFields.Plugins.SharePoint
{
    public class CalculatedSharepointService : SharePointService
    {
        public CalculatedSharepointService(XrmService xrmService, ISharePointSettings sharepointSettings)
            : base(sharepointSettings, xrmService)
        {
        }

        public override IEnumerable<SharepointFolderConfig> SharepointFolderConfigs
        {
            get
            {

                return new SharepointFolderConfig[]
                {
                };
            }
        }
    }
}
