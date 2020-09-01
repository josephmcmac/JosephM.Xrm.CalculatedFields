using System;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;

namespace JosephM.Xrm.CalculatedFields.Plugins.SharePoint
{
    public class CalculatedSharePointSettings : ISharePointSettings
    {
        public CalculatedSharePointSettings(XrmService xrmService)
        {
            XrmService = xrmService;
        }

        private string _username;
        public string UserName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private XrmService XrmService { get; }
    }
}
