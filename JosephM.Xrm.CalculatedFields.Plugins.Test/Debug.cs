using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Schema;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;

namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    //this class just for general debug purposes
    [TestClass]
    public class DebugTests : CalculatedXrmTest
    {
        [TestMethod]
        public void Debug()
        {
            var me = XrmService.WhoAmI();

            //var entity = XrmService.GetFirst(Entities.jmcg_calculatedfield);

            //var filter = entity.GetStringField(Fields.jmcg_calculatedfield_.jmcg_filter);

            //var xmlDocument = new XmlDocument();
            //xmlDocument.LoadXml(filter);

            //using (var stringWriter = new StringWriter())
            //{
            //    using (var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
            //    {
            //        xmlDocument.WriteTo(xmlTextWriter);
            //        xmlTextWriter.Flush();
            //        var xml = stringWriter.GetStringBuilder().ToString();
            //        if (xml.StartsWith("<?xml"))
            //        {
            //            var endCharIndex = xml.IndexOf('>');
            //            xml = xml.Substring(endCharIndex + 1).TrimStart();
            //        }
            //    }
            //}

            //CalculatedService.RefreshPluginRegistrations(entity.Id, true);
        }
    }
}