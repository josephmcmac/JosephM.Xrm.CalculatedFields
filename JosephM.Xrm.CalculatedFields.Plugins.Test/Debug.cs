using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Schema;
using System;

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

            //var holidays = CalculatedService.GetPublicHolidays(new DateTime(2020, 4, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 4, 18, 0, 0, 0, DateTimeKind.Utc), ServiceCalendarId);
            //var huh = holidays;

            //var calendar = XrmService.Retrieve(Entities.calendar, new Guid("b311004f-a2ec-ea11-8143-000c290a70aa"));

            //var huh = calendar;

            //var calendarId = new Guid("6a2c3c2f-a2ec-ea11-8143-000c290a70aa");

            //var dayUtc = DateTime.UtcNow.AddMonths(1);
            //var dayLocal = LocalisationService.ConvertToTargetTime(dayUtc);

            //var calc1 = LocalisationService.ConvertToTargetTime(CalculatedService.AddCalendarTime(dayUtc, OptionSets.CalculatedField.TimeType.WorkMinutes, 10, calendarId));
            //var calc2 = LocalisationService.ConvertToTargetTime(CalculatedService.AddCalendarTime(dayUtc, OptionSets.CalculatedField.TimeType.WorkHours, 10, calendarId));
            //var calc3 = LocalisationService.ConvertToTargetTime(CalculatedService.AddCalendarTime(dayUtc, OptionSets.CalculatedField.TimeType.WorkDays, 10, calendarId));



            //var calc1b = LocalisationService.ConvertToTargetTime(CalculatedService.AddCalendarTime(dayUtc, OptionSets.CalculatedField.TimeType.WorkMinutes, -10, calendarId));
            //var calc2b = LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(dayUtc, OptionSets.CalculatedField.TimeType.WorkHours, -10, calendarId));
            //var calc3b = LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(dayUtc, OptionSets.CalculatedField.TimeType.WorkDays, -10, calendarId));

            //var huh = dayUtc;

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