using Microsoft.VisualStudio.TestTools.UnitTesting;
using Schema;
using System;

namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    [TestClass]
    public class CalculatedServiceTests : CalculatedXrmTest
    {
        [TestMethod]
        public void CalculatedServiceTimeTakenTest()
        {
            var calendarId = ServiceCalendarId;
            //work days

            var wednesdayNoon1 = new DateTime(2020, 7, 29, 12, 0, 0, DateTimeKind.Unspecified);
            var thursdayNoon = new DateTime(2020, 7, 30, 12, 0, 0, DateTimeKind.Unspecified);
            var thursdayNoonUtc = LocalisationService.ConvertTargetToUtc(thursdayNoon);
            var fridayNoon = new DateTime(2020, 7, 31, 12, 0, 0, DateTimeKind.Unspecified);
            var fridayNoonUtc = LocalisationService.ConvertTargetToUtc(fridayNoon);
            var saturdayNoon = new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Unspecified);
            var saturdayNoonUtc = LocalisationService.ConvertTargetToUtc(saturdayNoon);
            var sundayNoon = new DateTime(2020, 8, 2, 12, 0, 0, DateTimeKind.Unspecified);
            var sundayNoonUtc = LocalisationService.ConvertTargetToUtc(sundayNoon);
            var mondayNoon = new DateTime(2020, 8, 3, 12, 0, 0, DateTimeKind.Unspecified);
            var mondayNoonUtc = LocalisationService.ConvertTargetToUtc(mondayNoon);
            var tuesdayNoon = new DateTime(2020, 8, 4, 12, 0, 0, DateTimeKind.Unspecified);
            var tuesdayNoonUtc = LocalisationService.ConvertTargetToUtc(tuesdayNoon);
            var wednesdayNoon2 = new DateTime(2020, 8, 5, 12, 0, 0, DateTimeKind.Unspecified);

            var thursdayAm = new DateTime(2020, 7, 30, 3, 0, 0, DateTimeKind.Unspecified);
            var thursdayAmUtc = LocalisationService.ConvertTargetToUtc(thursdayAm);
            var fridayAm = new DateTime(2020, 7, 31, 3, 0, 0, DateTimeKind.Unspecified);
            var fridayAmUtc = LocalisationService.ConvertTargetToUtc(fridayAm);
            var mondayAm = new DateTime(2020, 8, 3, 3, 0, 0, DateTimeKind.Unspecified);
            var mondayAmUtc = LocalisationService.ConvertTargetToUtc(mondayAm);
            var tuesdayAm = new DateTime(2020, 8, 4, 3, 0, 0, DateTimeKind.Unspecified);
            var tuesdayAmUtc = LocalisationService.ConvertTargetToUtc(tuesdayAm);

            var thursdayPm = new DateTime(2020, 7, 30, 21, 0, 0, DateTimeKind.Unspecified);
            var thursdayPmUtc = LocalisationService.ConvertTargetToUtc(thursdayPm);
            var fridayPm = new DateTime(2020, 7, 31, 21, 0, 0, DateTimeKind.Unspecified);
            var fridayPmUtc = LocalisationService.ConvertTargetToUtc(fridayPm);
            var mondayPm = new DateTime(2020, 8, 3, 21, 0, 0, DateTimeKind.Unspecified);
            var mondayPmUtc = LocalisationService.ConvertTargetToUtc(mondayPm);
            var tuesdayPm = new DateTime(2020, 8, 4, 21, 0, 0, DateTimeKind.Unspecified);
            var tuesdayPmUtc = LocalisationService.ConvertTargetToUtc(tuesdayPm);

            var wednesdayEnd = new DateTime(2020, 7, 29, 17, 0, 0, DateTimeKind.Unspecified);
            var thursdayStart = new DateTime(2020, 7, 30, 8, 30, 0, DateTimeKind.Unspecified);
            var thursdayEnd = new DateTime(2020, 7, 30, 17, 0, 0, DateTimeKind.Unspecified);
            var fridayStart = new DateTime(2020, 7, 31, 8, 30, 0, DateTimeKind.Unspecified);
            var fridayEnd = new DateTime(2020, 7, 31, 17, 0, 0, DateTimeKind.Unspecified);
            var fridayEndUtc = LocalisationService.ConvertTargetToUtc(fridayEnd);
            var mondayStart = new DateTime(2020, 8, 3, 8, 30, 0, DateTimeKind.Unspecified);
            var mondayStartUtc = LocalisationService.ConvertTargetToUtc(mondayStart);
            var mondayEnd = new DateTime(2020, 8, 3, 17, 0, 0, DateTimeKind.Unspecified);
            var tuesdayStart = new DateTime(2020, 8, 4, 8, 30, 0, DateTimeKind.Unspecified);
            var tuesdayEnd = new DateTime(2020, 8, 4, 17, 0, 0, DateTimeKind.Unspecified);
            var wednesdayStart = new DateTime(2020, 8, 5, 8, 30, 0, DateTimeKind.Unspecified);

            //work days

            //thursday noon ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(thursdayNoon, thursdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayNoon, fridayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayNoon, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(2, CalculatedService.GetTimeTaken(thursdayNoon, mondayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(3, CalculatedService.GetTimeTaken(thursdayNoon, tuesdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //friday noon ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(fridayNoon, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(fridayNoon, mondayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //satuyrday noon ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(saturdayNoon, sundayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(saturdayNoon, mondayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(saturdayNoon, tuesdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //thursday am ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(thursdayAm, thursdayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayAm, fridayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayAm, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(2, CalculatedService.GetTimeTaken(thursdayAm, mondayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(3, CalculatedService.GetTimeTaken(thursdayAm, tuesdayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //friday am ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(fridayAm, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(fridayAm, mondayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //saturday am ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(saturdayNoon, sundayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(saturdayNoon, mondayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(saturdayNoon, tuesdayAm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //thursday pm ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(thursdayPm, thursdayPm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayPm, fridayPm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayPm, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(2, CalculatedService.GetTimeTaken(thursdayPm, mondayPm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(3, CalculatedService.GetTimeTaken(thursdayPm, tuesdayPm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //friday pm ++
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(fridayPm, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(fridayPm, mondayPm, OptionSets.CalculatedField.TimeTakenMeasure.WorkDays, ServiceCalendarId));

            //days
            Assert.AreEqual(0, CalculatedService.GetTimeTaken(thursdayPm, thursdayPm, OptionSets.CalculatedField.TimeTakenMeasure.Days, ServiceCalendarId));
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(thursdayPm, fridayPm, OptionSets.CalculatedField.TimeTakenMeasure.Days, ServiceCalendarId));
            Assert.AreEqual(2, CalculatedService.GetTimeTaken(thursdayPm, saturdayNoon, OptionSets.CalculatedField.TimeTakenMeasure.Days, ServiceCalendarId));
            Assert.AreEqual(4, CalculatedService.GetTimeTaken(thursdayPm, mondayPm, OptionSets.CalculatedField.TimeTakenMeasure.Days, ServiceCalendarId));
            Assert.AreEqual(5, CalculatedService.GetTimeTaken(thursdayPm, tuesdayPm, OptionSets.CalculatedField.TimeTakenMeasure.Days, ServiceCalendarId));

            //work minutes
            Assert.AreEqual(30, CalculatedService.GetTimeTaken(fridayEndUtc.AddMinutes(-15), mondayStart.AddMinutes(15), OptionSets.CalculatedField.TimeTakenMeasure.WorkMinutes, calendarId));

            //work hours
            Assert.AreEqual(1, CalculatedService.GetTimeTaken(fridayEndUtc.AddMinutes(-45), mondayStart.AddMinutes(45), OptionSets.CalculatedField.TimeTakenMeasure.WorkHours, calendarId));

            //work minutes
            Assert.AreEqual(3840, CalculatedService.GetTimeTaken(fridayEndUtc.AddMinutes(-15), mondayStart.AddMinutes(15), OptionSets.CalculatedField.TimeTakenMeasure.Minutes, calendarId));

            //work hours
            Assert.AreEqual(64, CalculatedService.GetTimeTaken(fridayEndUtc.AddMinutes(-30), mondayStart.AddMinutes(30), OptionSets.CalculatedField.TimeTakenMeasure.Hours, calendarId));
            Assert.AreEqual(65, CalculatedService.GetTimeTaken(fridayEndUtc.AddMinutes(-45), mondayStart.AddMinutes(45), OptionSets.CalculatedField.TimeTakenMeasure.Hours, calendarId));
        }

        [TestMethod]
        public void CalculatedServiceAddTimeTest()
        {
            var calendarId = ServiceCalendarId;
            //work days

            var wednesdayNoon1 = new DateTime(2020, 7, 29, 12, 0, 0, DateTimeKind.Unspecified);
            var thursdayNoon = new DateTime(2020, 7, 30, 12, 0, 0, DateTimeKind.Unspecified);
            var thursdayNoonUtc = LocalisationService.ConvertTargetToUtc(thursdayNoon);
            var fridayNoon = new DateTime(2020, 7, 31, 12, 0, 0, DateTimeKind.Unspecified);
            var fridayNoonUtc = LocalisationService.ConvertTargetToUtc(fridayNoon);
            var saturdayNoon = new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Unspecified);
            var saturdayNoonUtc = LocalisationService.ConvertTargetToUtc(saturdayNoon);
            var sundayNoon = new DateTime(2020, 8, 2, 12, 0, 0, DateTimeKind.Unspecified);
            var sundayNoonUtc = LocalisationService.ConvertTargetToUtc(sundayNoon);
            var mondayNoon = new DateTime(2020, 8, 3, 12, 0, 0, DateTimeKind.Unspecified);
            var mondayNoonUtc = LocalisationService.ConvertTargetToUtc(mondayNoon);
            var tuesdayNoon = new DateTime(2020, 8, 4, 12, 0, 0, DateTimeKind.Unspecified);
            var tuesdayNoonUtc = LocalisationService.ConvertTargetToUtc(tuesdayNoon);
            var wednesdayNoon2 = new DateTime(2020, 8, 5, 12, 0, 0, DateTimeKind.Unspecified);

            var thursdayAm = new DateTime(2020, 7, 30, 3, 0, 0, DateTimeKind.Unspecified);
            var thursdayAmUtc = LocalisationService.ConvertTargetToUtc(thursdayAm);
            var fridayAm = new DateTime(2020, 7, 31, 3, 0, 0, DateTimeKind.Unspecified);
            var fridayAmUtc = LocalisationService.ConvertTargetToUtc(fridayAm);
            var mondayAm = new DateTime(2020, 8, 3, 3, 0, 0, DateTimeKind.Unspecified);
            var mondayAmUtc = LocalisationService.ConvertTargetToUtc(mondayAm);
            var tuesdayAm = new DateTime(2020, 8, 4, 3, 0, 0, DateTimeKind.Unspecified);
            var tuesdayAmUtc = LocalisationService.ConvertTargetToUtc(tuesdayAm);

            var thursdayPm = new DateTime(2020, 7, 30, 21, 0, 0, DateTimeKind.Unspecified);
            var thursdayPmUtc = LocalisationService.ConvertTargetToUtc(thursdayPm);
            var fridayPm = new DateTime(2020, 7, 31, 21, 0, 0, DateTimeKind.Unspecified);
            var fridayPmUtc = LocalisationService.ConvertTargetToUtc(fridayPm);
            var mondayPm = new DateTime(2020, 8, 3, 21, 0, 0, DateTimeKind.Unspecified);
            var mondayPmUtc = LocalisationService.ConvertTargetToUtc(mondayPm);
            var tuesdayPm = new DateTime(2020, 8, 4, 21, 0, 0, DateTimeKind.Unspecified);
            var tuesdayPmUtc = LocalisationService.ConvertTargetToUtc(tuesdayPm);

            var wednesdayEnd = new DateTime(2020, 7, 29, 17, 0, 0, DateTimeKind.Unspecified);
            var thursdayStart = new DateTime(2020, 7, 30, 8, 30, 0, DateTimeKind.Unspecified);
            var thursdayEnd = new DateTime(2020, 7, 30, 17, 0, 0, DateTimeKind.Unspecified);
            var fridayStart = new DateTime(2020, 7, 31, 8, 30, 0, DateTimeKind.Unspecified);
            var fridayEnd = new DateTime(2020, 7, 31, 17, 0, 0, DateTimeKind.Unspecified);
            var fridayEndUtc = LocalisationService.ConvertTargetToUtc(fridayEnd);
            var mondayStart = new DateTime(2020, 8, 3, 8, 30, 0, DateTimeKind.Unspecified);
            var mondayStartUtc = LocalisationService.ConvertTargetToUtc(mondayStart);
            var mondayEnd = new DateTime(2020, 8, 3, 17, 0, 0, DateTimeKind.Unspecified);
            var tuesdayStart = new DateTime(2020, 8, 4, 8, 30, 0, DateTimeKind.Unspecified);
            var tuesdayEnd = new DateTime(2020, 8, 4, 17, 0, 0, DateTimeKind.Unspecified);
            var wednesdayStart = new DateTime(2020, 8, 5, 8, 30, 0, DateTimeKind.Unspecified);

            //basic noon to noon over weekend
            Assert.AreEqual(tuesdayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(fridayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));
            Assert.AreEqual(mondayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(thursdayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));
            Assert.AreEqual(tuesdayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, 3, calendarId)));
            Assert.AreEqual(thursdayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, -3, calendarId)));
            Assert.AreEqual(wednesdayNoon2, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(saturdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(wednesdayNoon1, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(sundayNoonUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));

            //am to am over weekend
            Assert.AreEqual(tuesdayAm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayAmUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(fridayAm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayAmUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));
            Assert.AreEqual(mondayAm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayAmUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(thursdayAm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayAmUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));
            Assert.AreEqual(tuesdayAm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayAmUtc, OptionSets.CalculatedField.TimeType.WorkDays, 3, calendarId)));
            Assert.AreEqual(thursdayAm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayAmUtc, OptionSets.CalculatedField.TimeType.WorkDays, -3, calendarId)));

            //pm to pm over weekend
            Assert.AreEqual(tuesdayPm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayPmUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(fridayPm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayPmUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));
            Assert.AreEqual(mondayPm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayPmUtc, OptionSets.CalculatedField.TimeType.WorkDays, 2, calendarId)));
            Assert.AreEqual(thursdayPm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayPmUtc, OptionSets.CalculatedField.TimeType.WorkDays, -2, calendarId)));
            Assert.AreEqual(tuesdayPm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayPmUtc, OptionSets.CalculatedField.TimeType.WorkDays, 3, calendarId)));
            Assert.AreEqual(thursdayPm, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayPmUtc, OptionSets.CalculatedField.TimeType.WorkDays, -3, calendarId)));

            //work hours

            //noon to noonish
            Assert.AreEqual(tuesdayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayNoonUtc, OptionSets.CalculatedField.TimeType.WorkHours, 17, calendarId)));
            Assert.AreEqual(fridayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkHours, -17, calendarId)));
            Assert.AreEqual(mondayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkHours, 17, calendarId)));
            Assert.AreEqual(thursdayNoon, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayNoonUtc, OptionSets.CalculatedField.TimeType.WorkHours, -17, calendarId)));
            Assert.AreEqual(tuesdayNoon.AddMinutes(30), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkHours, 26, calendarId)));
            Assert.AreEqual(thursdayNoon.AddMinutes(-30), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayNoonUtc, OptionSets.CalculatedField.TimeType.WorkHours, -26, calendarId)));

            //am
            Assert.AreEqual(mondayEnd, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayAmUtc, OptionSets.CalculatedField.TimeType.WorkHours, 17, calendarId)));
            Assert.AreEqual(fridayStart, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayAmUtc, OptionSets.CalculatedField.TimeType.WorkHours, -17, calendarId)));
            Assert.AreEqual(fridayEnd, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayAmUtc, OptionSets.CalculatedField.TimeType.WorkHours, 17, calendarId)));
            Assert.AreEqual(thursdayStart, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayAmUtc, OptionSets.CalculatedField.TimeType.WorkHours, -17, calendarId)));
            Assert.AreEqual(tuesdayStart.AddMinutes(30), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayAmUtc, OptionSets.CalculatedField.TimeType.WorkHours, 26, calendarId)));
            Assert.AreEqual(wednesdayEnd.AddMinutes(-30), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayAmUtc, OptionSets.CalculatedField.TimeType.WorkHours, -26, calendarId)));

            //pm
            Assert.AreEqual(tuesdayEnd, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayPmUtc, OptionSets.CalculatedField.TimeType.WorkHours, 17, calendarId)));
            Assert.AreEqual(mondayStart, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayPmUtc, OptionSets.CalculatedField.TimeType.WorkHours, -17, calendarId)));
            Assert.AreEqual(mondayEnd, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayPmUtc, OptionSets.CalculatedField.TimeType.WorkHours, 17, calendarId)));
            Assert.AreEqual(fridayStart, LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayPmUtc, OptionSets.CalculatedField.TimeType.WorkHours, -17, calendarId)));
            Assert.AreEqual(wednesdayStart.AddMinutes(30), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(thursdayPmUtc, OptionSets.CalculatedField.TimeType.WorkHours, 26, calendarId)));
            Assert.AreEqual(thursdayEnd.AddMinutes(-30), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(tuesdayPmUtc, OptionSets.CalculatedField.TimeType.WorkHours, -26, calendarId)));

            //work minutes
            Assert.AreEqual(mondayStart.AddMinutes(15), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(fridayEndUtc.AddMinutes(-15), OptionSets.CalculatedField.TimeType.WorkMinutes, 30, calendarId)));
            Assert.AreEqual(fridayEnd.AddMinutes(-15), LocalisationService.ConvertToTargetTime(CalculatedService.AddTime(mondayStartUtc.AddMinutes(15), OptionSets.CalculatedField.TimeType.WorkMinutes, -30, calendarId)));
        }
    }
}