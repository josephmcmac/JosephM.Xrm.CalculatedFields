using JosephM.Xrm.CalculatedFields.Plugins.Rollups;
using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;


namespace JosephM.Xrm.CalculatedFields.Plugins.Test
{
    [TestClass]
    public class CalculatedFieldsTests : CalculatedXrmTest
    {
        [TestMethod]
        public void CalculatedFieldsOrderDependeciesTests()
        {
            DeleteAllCalculatedFields();

            //string with created on
            CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Concatenate) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, Fields.jmcg_testentity_.jmcg_string },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_string },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1, Fields.jmcg_testentity_.createdon },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(OptionSets.CalculatedField.SeparatorType.Space) },
                });

            //create name with string (created on), time taken and jmcg_dateaddresultutc
            CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Concatenate) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, Fields.jmcg_testentity_.jmcg_name },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_name },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenmeasure, new OptionSetValue(OptionSets.CalculatedField.TimeTakenMeasure.WorkMinutes) },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1, Fields.jmcg_testentity_.jmcg_string },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield2, Fields.jmcg_testentity_.jmcg_timetakenutc },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield3, Fields.jmcg_testentity_.jmcg_dateaddresultutc },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(OptionSets.CalculatedField.SeparatorType.Space) },
                });

            //create time taken field = created on -> date add result (10)
            CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.TimeTaken) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, Fields.jmcg_testentity_.jmcg_timetakenutc },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_timetakenutc },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenmeasure, new OptionSetValue(OptionSets.CalculatedField.TimeTakenMeasure.Minutes) },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenstartfield, Fields.jmcg_testentity_.createdon },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenendfield, Fields.jmcg_testentity_.jmcg_dateaddresultutc },
                    { Fields.jmcg_calculatedfield_.jmcg_calendarid, ServiceCalendarId.ToString() },
                });

            //create add time result = created on + 10 minutes
            CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.AddTime) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, Fields.jmcg_testentity_.jmcg_dateaddresultutc },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_dateaddresultutc },
                    { Fields.jmcg_calculatedfield_.jmcg_timetype, new OptionSetValue(OptionSets.CalculatedField.TimeType.Minutes) },
                    { Fields.jmcg_calculatedfield_.jmcg_timeamount, 10 },
                    { Fields.jmcg_calculatedfield_.jmcg_addtimetofield, Fields.jmcg_testentity_.createdon }
                });

            var testRecord = CreateTestRecord(Entities.jmcg_testentity);
            var name = testRecord.GetStringField(Fields.jmcg_testentity_.jmcg_name);
            Assert.IsNotNull(testRecord.GetField(Fields.jmcg_testentity_.jmcg_string));
            Assert.IsNotNull(testRecord.GetField(Fields.jmcg_testentity_.jmcg_dateaddresultutc));
            Assert.IsNotNull(testRecord.GetField(Fields.jmcg_testentity_.jmcg_timetakenutc));
            Assert.AreEqual(XrmService.GetFieldAsDisplayString(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_string, testRecord.GetField(Fields.jmcg_testentity_.jmcg_string), LocalisationService) + " 10 " + XrmService.GetFieldAsDisplayString(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_dateaddresultutc, testRecord.GetField(Fields.jmcg_testentity_.jmcg_dateaddresultutc), LocalisationService), name);
        }

        [TestMethod]
        public void CalculatedFieldsTimeTakenTests()
        {
            DeleteAllCalculatedFields();

            //create time taken field for work minutes
            CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.TimeTaken) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, Fields.jmcg_testentity_.jmcg_timetakenutc },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_timetakenutc },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenmeasure, new OptionSetValue(OptionSets.CalculatedField.TimeTakenMeasure.WorkMinutes) },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenstartfield, Fields.jmcg_testentity_.jmcg_timetakenstartutc },
                    { Fields.jmcg_calculatedfield_.jmcg_timetakenendfield, Fields.jmcg_testentity_.jmcg_timetakenendutc },
                    { Fields.jmcg_calculatedfield_.jmcg_calendarid, ServiceCalendarId.ToString() },
                });

            var fridayEnd = new DateTime(2020, 7, 31, 17, 0, 0, DateTimeKind.Unspecified);
            var fridayEndUtc = LocalisationService.ConvertTargetToUtc(fridayEnd);
            var mondayStart = new DateTime(2020, 8, 3, 8, 30, 0, DateTimeKind.Unspecified);
            var mondayStartUtc = LocalisationService.ConvertTargetToUtc(mondayStart);
            var mondayEnd = new DateTime(2020, 8, 3, 17, 0, 0, DateTimeKind.Unspecified);
            var tuesdayStart = new DateTime(2020, 8, 4, 8, 30, 0, DateTimeKind.Unspecified);
            var tuesdayStartUtc = LocalisationService.ConvertTargetToUtc(tuesdayStart);

            //create test record and verify calculated
            var testRecord = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_timetakenstartutc, fridayEnd.AddMinutes(-15) },
                { Fields.jmcg_testentity_.jmcg_timetakenendutc, mondayStart.AddMinutes(15) }
            });
            Assert.AreEqual(30, testRecord.GetInt(Fields.jmcg_testentity_.jmcg_timetakenutc));

            //change end and verify updated
            testRecord.SetField(Fields.jmcg_testentity_.jmcg_timetakenendutc, tuesdayStartUtc.AddMinutes(15));
            testRecord = UpdateFieldsAndRetreive(testRecord, Fields.jmcg_testentity_.jmcg_timetakenendutc);
            Assert.AreEqual(540, testRecord.GetInt(Fields.jmcg_testentity_.jmcg_timetakenutc));

            //change start and verify updated
            testRecord.SetField(Fields.jmcg_testentity_.jmcg_timetakenstartutc, tuesdayStart);
            testRecord = UpdateFieldsAndRetreive(testRecord, Fields.jmcg_testentity_.jmcg_timetakenstartutc);
            Assert.AreEqual(15, testRecord.GetInt(Fields.jmcg_testentity_.jmcg_timetakenutc));
        }

        [TestMethod]
        public void CalculatedFieldsAddTimeTests()
        {
            DeleteAllCalculatedFields();

            var configs = new[]
            {
                new TestAddTimeConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_dateaddresultutc, Fields.jmcg_testentity_.jmcg_dateaddtoutc, OptionSets.CalculatedField.TimeType.WorkDays, 1, ServiceCalendarId, true),
                new TestAddTimeConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_dateaddresultdateonly, Fields.jmcg_testentity_.jmcg_dateaddtodateonly, OptionSets.CalculatedField.TimeType.WorkDays, 1, ServiceCalendarId, false),
                new TestAddTimeConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_dateaddresultnotimezone, Fields.jmcg_testentity_.jmcg_dateaddtonotimezone, OptionSets.CalculatedField.TimeType.WorkDays, 1, ServiceCalendarId, true),
            };

            var calculatedRecords = new List<Entity>();
            foreach (var config in configs)
            {
                calculatedRecords.Add(CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.AddTime) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, config.Field },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, config.EntityType },
                    { Fields.jmcg_calculatedfield_.jmcg_field, config.Field },
                    { Fields.jmcg_calculatedfield_.jmcg_addtimetofield, config.FieldSource },
                    { Fields.jmcg_calculatedfield_.jmcg_timetype, new OptionSetValue(config.TimeType) },
                    { Fields.jmcg_calculatedfield_.jmcg_timeamount, config.TimeAmount },
                    { Fields.jmcg_calculatedfield_.jmcg_calendarid, config.CalendarId.ToString() },
                }));
            }


            var fridayNoon = new DateTime(2020, 7, 31, 12, 0, 0, DateTimeKind.Unspecified);
            var fridayNoonUtc = LocalisationService.ConvertTargetToUtc(fridayNoon);
            var mondayNoon = new DateTime(2020, 8, 3, 12, 0, 0, DateTimeKind.Unspecified);
            var mondayNoonUtc = LocalisationService.ConvertTargetToUtc(mondayNoon);
            var tuesdayNoon = new DateTime(2020, 8, 4, 12, 0, 0, DateTimeKind.Unspecified);
            var tuesdayNoonUtc = LocalisationService.ConvertTargetToUtc(tuesdayNoon);
            var fridayStart = new DateTime(2020, 7, 31, 0, 0, 0, DateTimeKind.Unspecified);
            var fridayStartUtc = LocalisationService.ConvertTargetToUtc(fridayNoon);
            var mondayStart = new DateTime(2020, 8, 3, 0, 0, 0, DateTimeKind.Unspecified);
            var mondayStartUtc = LocalisationService.ConvertTargetToUtc(mondayNoon);

            var testEntity = new Entity(Entities.jmcg_testentity);
            foreach (var config in configs)
            {
                testEntity.SetField(config.FieldSource, config.UseTime ? fridayNoon : fridayStart);
            }
            testEntity = CreateAndRetrieve(testEntity);
            foreach (var config in configs)
            {
                var addedTime = testEntity.GetDateTimeField(config.Field);
                Assert.IsTrue(addedTime.HasValue);
                if (config.UseTime)
                {
                    if (addedTime.Value.Kind == DateTimeKind.Utc)
                    {
                        Assert.AreEqual(mondayNoonUtc, addedTime.Value);
                    }
                    else
                    {
                        //Assert.AreEqual(mondayNoon, addedTime.Value);
                    }
                }
                else
                {
                    //Assert.AreEqual(mondayStart, addedTime.Value);
                }
            }
            foreach (var config in configs)
            {
                testEntity.SetField(config.FieldSource, config.UseTime ? mondayNoonUtc : mondayStartUtc);
            }
            testEntity = UpdateFieldsAndRetreive(testEntity, configs.Select(c => c.FieldSource).ToArray());
            foreach (var config in configs)
            {
                var addedTime = testEntity.GetDateTimeField(config.Field);
                Assert.IsTrue(addedTime.HasValue);
                if (config.UseTime)
                {
                    if (addedTime.Value.Kind == DateTimeKind.Utc)
                    {
                        Assert.AreEqual(tuesdayNoonUtc, addedTime.Value);
                    }
                    else
                    {
                        //Assert.AreEqual(mondayNoon, addedTime.Value);
                    }
                }
                else
                {
                    //Assert.AreEqual(mondayStart, addedTime.Value);
                }
            }

            foreach (var config in configs)
            {
                testEntity.SetField(config.FieldSource, null);
            }
            testEntity = UpdateFieldsAndRetreive(testEntity, configs.Select(c => c.FieldSource).ToArray());
            foreach (var config in configs)
            {
                var addedTime = testEntity.GetDateTimeField(config.Field);
                Assert.IsFalse(addedTime.HasValue);
            }
        }

        [TestMethod]
        public void CalculatedFieldsRollupFirstTests()
        {
            //these also need refreshing when the order field changed so have specific test for them
            DeleteAllCalculatedFields();

            var firstRollup = CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Rollup) },
                    { Fields.jmcg_calculatedfield_.jmcg_rolluptype, new OptionSetValue(OptionSets.CalculatedField.RollupType.First) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, "Testing First Orders" },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_firsttarget },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup, Entities.jmcg_testentitytwo },
                    { Fields.jmcg_calculatedfield_.jmcg_fieldrolledup, Fields.jmcg_testentitytwo_.jmcg_firstsource},
                    { Fields.jmcg_calculatedfield_.jmcg_fieldreferencing, Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(OptionSets.CalculatedField.SeparatorType.Comma) },
                    { Fields.jmcg_calculatedfield_.jmcg_rollupfilter,  "<filter type=\"and\"><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"new_boolean\" operator=\"eq\" value=\"1\" /></filter>" },
                    { Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfield, Fields.jmcg_testentitytwo_.new_date },
                    { Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype, new OptionSetValue(OptionSets.CalculatedField.OrderRollupByFieldOrderType.Ascending) }
                });

            var firstRollupLookup = CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Rollup) },
                    { Fields.jmcg_calculatedfield_.jmcg_rolluptype, new OptionSetValue(OptionSets.CalculatedField.RollupType.First) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, "Testing First Orders" },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup, Entities.jmcg_testentitytwo },
                    { Fields.jmcg_calculatedfield_.jmcg_fieldrolledup, Fields.jmcg_testentitytwo_.jmcg_testentitytwoid},
                    { Fields.jmcg_calculatedfield_.jmcg_fieldreferencing, Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(OptionSets.CalculatedField.SeparatorType.Comma) },
                    { Fields.jmcg_calculatedfield_.jmcg_rollupfilter,  "<filter type=\"and\"><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"new_boolean\" operator=\"eq\" value=\"1\" /></filter>" },
                    { Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfield, Fields.jmcg_testentitytwo_.new_date },
                    { Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype, new OptionSetValue(OptionSets.CalculatedField.OrderRollupByFieldOrderType.Ascending) }
                });

            var target = CreateTestRecord(Entities.jmcg_testentity);

            //create one matching and verify populated
            var source1 = CreateTestRecord(Entities.jmcg_testentitytwo, new Dictionary<string, object>
            {
                { Fields.jmcg_testentitytwo_.jmcg_name, "Testing Order" },
                { Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference, target.ToEntityReference() },
                { Fields.jmcg_testentitytwo_.new_boolean, true },
                { Fields.jmcg_testentitytwo_.new_date, LocalisationService.TodayUnspecifiedType },
                { Fields.jmcg_testentitytwo_.jmcg_firstsource,  50 },
            });
            target = Refresh(target);
            Assert.AreEqual(50, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source1.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //create one with earlier date and verify populated
            var source2 = CreateTestRecord(Entities.jmcg_testentitytwo, new Dictionary<string, object>
            {
                { Fields.jmcg_testentitytwo_.jmcg_name, "Testing Order" },
                { Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference, target.ToEntityReference() },
                { Fields.jmcg_testentitytwo_.new_boolean, true },
                { Fields.jmcg_testentitytwo_.new_date, LocalisationService.TodayUnspecifiedType.AddDays(-1) },
                { Fields.jmcg_testentitytwo_.jmcg_firstsource,  100 },
            });
            target = Refresh(target);
            Assert.AreEqual(100, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source2.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //set other date earlier and verify updated
            source1.SetField(Fields.jmcg_testentitytwo_.new_date, LocalisationService.TodayUnspecifiedType.AddDays(-2));
            source1 = UpdateFieldsAndRetreive(source1, Fields.jmcg_testentitytwo_.new_date);
            target = Refresh(target);
            Assert.AreEqual(50, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source1.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //unmatch and verify updated
            source1.SetField(Fields.jmcg_testentitytwo_.new_boolean, false);
            source1 = UpdateFieldsAndRetreive(source1, Fields.jmcg_testentitytwo_.new_boolean);
            target = Refresh(target);
            Assert.AreEqual(100, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source2.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //rematch and verify updated
            source1.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            source1 = UpdateFieldsAndRetreive(source1, Fields.jmcg_testentitytwo_.new_boolean);
            target = Refresh(target);
            Assert.AreEqual(50, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source1.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //delete and verify updated
            XrmService.Delete(source1);
            target = Refresh(target);
            Assert.AreEqual(100, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source2.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //change to descending order
            firstRollup.SetOptionSetField(Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype, OptionSets.CalculatedField.OrderRollupByFieldOrderType.Descending);
            firstRollup = UpdateFieldsAndRetreive(firstRollup, Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype);
            firstRollupLookup.SetOptionSetField(Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype, OptionSets.CalculatedField.OrderRollupByFieldOrderType.Descending);
            firstRollupLookup = UpdateFieldsAndRetreive(firstRollupLookup, Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype);

            source1 = CreateTestRecord(Entities.jmcg_testentitytwo, new Dictionary<string, object>
            {
                { Fields.jmcg_testentitytwo_.jmcg_name, "Testing Order" },
                { Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference, target.ToEntityReference() },
                { Fields.jmcg_testentitytwo_.new_boolean, true },
                { Fields.jmcg_testentitytwo_.new_date, LocalisationService.TodayUnspecifiedType },
                { Fields.jmcg_testentitytwo_.jmcg_firstsource,  50 },
            });
            target = Refresh(target);
            Assert.AreEqual(50, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source1.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //change amount and verify updated
            source1.SetField(Fields.jmcg_testentitytwo_.jmcg_firstsource, 75);
            source1 = UpdateFieldsAndRetreive(source1, Fields.jmcg_testentitytwo_.jmcg_firstsource);
            target = Refresh(target);
            Assert.AreEqual(75, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source1.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //set other date later and verify updated
            source2.SetField(Fields.jmcg_testentitytwo_.new_date, LocalisationService.TodayUnspecifiedType.AddDays(2));
            source2 = UpdateFieldsAndRetreive(source2, Fields.jmcg_testentitytwo_.new_date);
            target = Refresh(target);
            Assert.AreEqual(100, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source2.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));

            //change amount on not matched and verify not updated
            source1.SetField(Fields.jmcg_testentitytwo_.jmcg_firstsource, 25);
            source1 = UpdateFieldsAndRetreive(source1, Fields.jmcg_testentitytwo_.jmcg_firstsource);
            target = Refresh(target);
            Assert.AreEqual(100, target.GetInt(Fields.jmcg_testentity_.jmcg_firsttarget));
            Assert.AreEqual(source2.Id, target.GetLookupGuid(Fields.jmcg_testentity_.jmcg_firsttargettestentitytwo));
        }

        [TestMethod]
        public void CalculatedFieldsRollupRecalculateAllTest()
        {
            DeleteAllCalculatedFields();

            var configs = InitialiseTestEntityTwotoOneConfigs();

            var testEntityTarget1 = CreateTestRecord(configs.TypeRolledUpTo);
            var testEntityTarget2 = CreateTestRecord(configs.TypeRolledUpTo);
            var testEntityTarget3 = CreateTestRecord(configs.TypeRolledUpTo);

            var testEntitySource1 = new Entity(configs.TypeRolledUp);
            testEntitySource1.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget1);
            testEntitySource1.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource1.SetField(config.FieldFrom, config.ValueRollup1);
            }
            testEntitySource1 = CreateAndRetrieve(testEntitySource1);

            var testEntitySource1Inactive = new Entity(configs.TypeRolledUp);
            testEntitySource1Inactive.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget1);
            testEntitySource1Inactive.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource1Inactive.SetField(config.FieldFrom, config.ValueRollup1);
            }
            testEntitySource1Inactive = CreateAndRetrieve(testEntitySource1Inactive);
            XrmService.SetState(testEntitySource1Inactive.LogicalName, testEntitySource1Inactive.Id, OptionSets.TestEntityTwo.Status.Inactive);

            var testEntitySource2 = new Entity(configs.TypeRolledUp);
            testEntitySource2.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget2);
            testEntitySource2.SetField(Fields.jmcg_testentitytwo_.new_boolean, false);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource2.SetField(config.FieldFrom, config.ValueRollup1);
            }
            testEntitySource2 = CreateAndRetrieve(testEntitySource2);

            var calculatedFields = CreateCalculatedFieldsForConfigs(configs);

            foreach (var calculated in calculatedFields)
            {
                calculated.SetField(Fields.jmcg_calculatedfield_.jmcg_recalculateall, true);
                var updated = UpdateFieldsAndRetreive(calculated, Fields.jmcg_calculatedfield_.jmcg_recalculateall);

                WaitTillTrue(() => !Refresh(calculated).GetBoolean(Fields.jmcg_calculatedfield_.jmcg_isrecalculating), 60);
            }

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);
            testEntityTarget3 = Refresh(testEntityTarget3);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget2.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget3.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget2.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget3.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                    Assert.IsNull(testEntityTarget3.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                    Assert.IsNull(testEntityTarget3.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                    Assert.IsNull(testEntityTarget3.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                    Assert.IsNull(testEntityTarget3.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    var convertValue = testEntityTarget2.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    convertValue = testEntityTarget3.GetField(config.FieldTo);
                    if (convertValue is Money money2)
                    {
                        convertValue = money2.Value;
                    }
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                    Assert.IsNull(testEntityTarget3.GetField(config.FieldTo));
                }
            }
        }

        [TestMethod]
        public void CalculatedFieldsRollupTypesTest()
        {
            DeleteAllCalculatedFields();

            var configs = InitialiseTestEntityTwotoOneConfigs();

            CreateCalculatedFieldsForConfigs(configs);

            //create a target record and verify fields initialised
            var testEntityTarget1 = CreateTestRecord(configs.TypeRolledUpTo);
            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(0, testEntityTarget1.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsFalse(testEntityTarget1.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    var convertValue = testEntityTarget1.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
            }

            //create a rollup record and verify rollup calculated
            var testEntitySource1 = new Entity(configs.TypeRolledUp);
            testEntitySource1.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget1);
            testEntitySource1.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource1.SetField(config.FieldFrom, config.ValueRollup1);
            }
            testEntitySource1 = CreateAndRetrieve(testEntitySource1);

            testEntityTarget1 = Refresh(testEntityTarget1);
            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
            }

            //create a 2nd rollup record and verify rollup calculated
            var testEntitySource2 = new Entity(configs.TypeRolledUp);
            testEntitySource2.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget1);
            testEntitySource2.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource2.SetField(config.FieldFrom, config.ValueRollup2);
            }
            testEntitySource2 = CreateAndRetrieve(testEntitySource2);

            testEntityTarget1 = Refresh(testEntityTarget1);
            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(2, testEntityTarget1.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.AreEqual(4, int.Parse(testEntityTarget1.GetField(config.FieldTo).ToString().Substring(0, 1)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1.ToString() + "," + config.ValueRollup2.ToString(), testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(XrmEntity.SumFields(new[] { config.ValueRollup1, config.ValueRollup2 }), testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
            }

            //create a second target record, switch one of the rollups to it and verify
            var testEntityTarget2 = CreateTestRecord(configs.TypeRolledUpTo);

            testEntitySource2.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget2);
            testEntitySource2 = UpdateFieldsAndRetreive(testEntitySource2, configs.TypeRolledUpReferenceField);

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(1, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsTrue(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
            }

            //deactivate and verify no longer rolled up
            XrmService.SetState(testEntitySource2.LogicalName, testEntitySource2.Id, OptionSets.TestEntityTwo.Status.Inactive);

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    var convertValue = testEntityTarget2.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
            }

            //reactivate and verify rolled up
            XrmService.SetState(testEntitySource2.LogicalName, testEntitySource2.Id, OptionSets.TestEntityTwo.Status.Active);
            testEntitySource2 = Refresh(testEntitySource2);

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(1, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsTrue(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
            }

            //unmatch filter and change value and verify not rolled up
            testEntitySource2.SetField(Fields.jmcg_testentitytwo_.new_boolean, false);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource2.SetField(config.FieldFrom, config.ValueRollup1);
            }
            testEntitySource2 = UpdateFieldsAndRetreive(testEntitySource2, configs.TestRollupConfigs.Select(c => c.FieldFrom).Union(new[] { Fields.jmcg_testentitytwo_.new_boolean }).ToArray());

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    var convertValue = testEntityTarget2.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
            }

            //rematch filter and change value and verify rolled up
            testEntitySource2.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource2.SetField(config.FieldFrom, config.ValueRollup2);
            }
            testEntitySource2 = UpdateFieldsAndRetreive(testEntitySource2, configs.TestRollupConfigs.Select(c => c.FieldFrom).Union(new[] { Fields.jmcg_testentitytwo_.new_boolean }).ToArray());

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(1, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsTrue(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget2.GetField(config.FieldTo)));
                }
            }

            //relink and change value and verify all on target 1, none target 2
            testEntitySource2.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget1);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource2.SetField(config.FieldFrom, config.ValueRollup1);
            }
            testEntitySource2 = UpdateFieldsAndRetreive(testEntitySource2, configs.TestRollupConfigs.Select(c => c.FieldFrom).Union(new[] { configs.TypeRolledUpReferenceField }).ToArray());

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(2, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1.ToString() + "," + config.ValueRollup1.ToString(), testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(XrmEntity.SumFields(new[] { config.ValueRollup1, config.ValueRollup1 }), testEntityTarget1.GetField(config.FieldTo)));
                    var convertValue = testEntityTarget2.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
            }

            //relink and unmatch value
            testEntitySource2.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget2);
            testEntitySource2.SetField(Fields.jmcg_testentitytwo_.new_boolean, false);
            testEntitySource2 = UpdateFieldsAndRetreive(testEntitySource2, configs.TypeRolledUpReferenceField, Fields.jmcg_testentitytwo_.new_boolean);

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    var convertValue = testEntityTarget2.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
            }
            //relink, match & change value
            testEntitySource2.SetLookupField(configs.TypeRolledUpReferenceField, testEntityTarget1);
            testEntitySource2.SetField(Fields.jmcg_testentitytwo_.new_boolean, true);
            foreach (var config in configs.TestRollupConfigs)
            {
                testEntitySource2.SetField(config.FieldFrom, config.ValueRollup2);
            }
            testEntitySource2 = UpdateFieldsAndRetreive(testEntitySource2, configs.TestRollupConfigs.Select(c => c.FieldFrom).Union(new[] { configs.TypeRolledUpReferenceField, Fields.jmcg_testentitytwo_.new_boolean }).ToArray());

            testEntityTarget1 = Refresh(testEntityTarget1);
            testEntityTarget2 = Refresh(testEntityTarget2);

            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(2, testEntityTarget1.GetInt(config.FieldTo));
                    Assert.AreEqual(0, testEntityTarget2.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                    Assert.IsFalse(testEntityTarget2.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup2, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.AreEqual(4, int.Parse(testEntityTarget1.GetField(config.FieldTo).ToString().Substring(0, 1)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1.ToString() + "," + config.ValueRollup2.ToString(), testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(XrmEntity.SumFields(new[] { config.ValueRollup1, config.ValueRollup2 }), testEntityTarget1.GetField(config.FieldTo)));
                    var convertValue = testEntityTarget2.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                    Assert.IsNull(testEntityTarget2.GetField(config.FieldTo));
                }
            }

            //deactivate and verify removed
            XrmService.SetState(testEntitySource2.LogicalName, testEntitySource2.Id, OptionSets.TestEntityTwo.Status.Inactive);
            testEntityTarget1 = Refresh(testEntityTarget1);
            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
            }

            //delete already deactivated and verify no change
            XrmService.Delete(testEntitySource2);
            testEntityTarget1 = Refresh(testEntityTarget1);
            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(1, testEntityTarget1.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsTrue(testEntityTarget1.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsTrue(XrmEntity.FieldsEqual(config.ValueRollup1, testEntityTarget1.GetField(config.FieldTo)));
                }
            }

            //delete rolled up and at 0 day
            XrmService.Delete(testEntitySource1);
            testEntityTarget1 = Refresh(testEntityTarget1);
            foreach (var config in configs.TestRollupConfigs)
            {
                if (config.RollupType == RollupType.Count)
                {
                    Assert.AreEqual(0, testEntityTarget1.GetInt(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Exists)
                {
                    Assert.IsFalse(testEntityTarget1.GetBoolean(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Max)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Mean)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Min)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.SeparatedStrings)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
                else if (config.RollupType == RollupType.Sum)
                {
                    var convertValue = testEntityTarget1.GetField(config.FieldTo);
                    if (convertValue is Money money)
                    {
                        convertValue = money.Value;
                    }
                    Assert.AreEqual(0, Convert.ToInt32(convertValue));
                }
                else if (config.RollupType == RollupType.First)
                {
                    Assert.IsNull(testEntityTarget1.GetField(config.FieldTo));
                }
            }
        }

        [TestMethod]
        public void CalculatedFieldsConcatenateAnnotationsPluginTest()
        {
            DeleteAllCalculatedFields();

            var concatenator = CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Concatenate) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, "Test Concatenatation" },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, Entities.jmcg_testentity },
                    { Fields.jmcg_calculatedfield_.jmcg_field, Fields.jmcg_testentity_.jmcg_name },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1, Fields.jmcg_testentity_.jmcg_string },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1prepend, "Name" },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1prependspaced, true },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1append, "-" },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1appendspaced, true },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield2, Fields.jmcg_testentity_.jmcg_date },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield2formatstring, "yyyy-MM-dd" },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield2prepend, "(" },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield2append, ")" },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield3, Fields.jmcg_testentity_.jmcg_integer },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield4, Fields.jmcg_testentity_.jmcg_account },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield5, Fields.jmcg_testentity_.jmcg_boolean },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield6, Fields.jmcg_testentity_.createdon },{ Fields.jmcg_calculatedfield_.jmcg_concatenatefield6formatstring, "dd/MM/yyyy" },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(OptionSets.CalculatedField.SeparatorType.Space) },
                });

            var testRecord = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>()
            {
                { Fields.jmcg_testentity_.jmcg_string, "Joseph" },
                { Fields.jmcg_testentity_.jmcg_date, DateTime.UtcNow },
                { Fields.jmcg_testentity_.jmcg_integer, 100 },
                { Fields.jmcg_testentity_.jmcg_account, TestContactAccount.ToEntityReference() },
                { Fields.jmcg_testentity_.jmcg_boolean, true },
            });
            var calculatedName = testRecord.GetStringField(Fields.jmcg_testentity_.jmcg_name);
            var expectedName = $"Name Joseph - ({LocalisationService.TargetToday.ToString("yyyy-MM-dd")}) 100 {TestContactAccount.GetStringField(Fields.account_.name)} Yes {LocalisationService.TargetToday.ToString("dd/MM/yyyy")}";
            Assert.AreEqual(expectedName, calculatedName);

            var testRecordNonePopulated = CreateTestRecord(Entities.jmcg_testentity);
        }

        [TestMethod]
        public void CalculatedFieldsConcatenatePluginTest()
        {
            DeleteAllCalculatedFields();

            var configs = new[]
            {
                new TestConcatenateConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, new[]
                {
                    Fields.jmcg_testentity_.jmcg_account, Fields.jmcg_testentity_.jmcg_date, Fields.jmcg_testentity_.jmcg_money, Fields.jmcg_testentity_.jmcg_decimal, Fields.jmcg_testentity_.jmcg_picklist,
                    Fields.jmcg_testentity_.jmcg_boolean
                }, new object[]
                {
                    TestContactAccount.ToEntityReference(), LocalisationService.TodayUnspecifiedType, new Money((decimal)111.11), (decimal)111.1, new OptionSetValue(OptionSets.TestEntity.Picklist.Option1), true
                }, new object[]
                {
                    TestContactAccount.ToEntityReference(), LocalisationService.TodayUnspecifiedType.AddDays(1), new Money((decimal)222.22), (decimal)222.2, new OptionSetValue(OptionSets.TestEntity.Picklist.Option2), false
                }, OptionSets.CalculatedField.SeparatorType.OtherString, ":", true, true, false)
            };

            var concatenateFieldRecord = CreateCalculatedFieldsForConfig(configs).First(); ;

            foreach (var config in configs)
            {
                var indexes = new List<int>();
                var target = new Entity(config.EntityType);
                for (var i = 0; i < config.ConcatenateField.Length; i++)
                {
                    indexes.Add(i);
                }
                foreach (var index in indexes)
                {
                    target.SetField(config.ConcatenateField[index], config.ConcatenateValues1[index]);
                }
                target = CreateAndRetrieve(target);

                var concatenated = target.GetStringField(config.Field);
                var values = indexes.Select(i => XrmService.GetFieldAsDisplayString(config.EntityType, config.ConcatenateField[i], config.ConcatenateValues1[i], LocalisationService));
                Assert.AreEqual(string.Join(" : ", values), concatenated);

                foreach (var index in indexes)
                {
                    target.SetField(config.ConcatenateField[index], config.ConcatenateValues2[index]);
                }
                target = UpdateFieldsAndRetreive(target, config.ConcatenateField);
                var concatenated2 = target.GetStringField(config.Field);
                values = indexes.Select(i => XrmService.GetFieldAsDisplayString(config.EntityType, config.ConcatenateField[i], config.ConcatenateValues2[i], LocalisationService));
                Assert.AreEqual(string.Join(" : ", values), concatenated2);

                target.SetField(config.ConcatenateField[3], null);
                target = UpdateFieldsAndRetreive(target, config.ConcatenateField[3]);
                var concatenated3 = target.GetStringField(config.Field);
                var valueList = indexes.Select(i => XrmService.GetFieldAsDisplayString(config.EntityType, config.ConcatenateField[i], config.ConcatenateValues2[i], LocalisationService))
                    .ToList();
                valueList.RemoveAt(3);
                Assert.AreEqual(string.Join(" : ", valueList), concatenated3);
            }
        }

        [TestMethod]
        public void CalculatedFieldsConcatenateCalculationTest()
        {
            DeleteAllCalculatedFields();

            var configs = new[]
            {
                new TestConcatenateConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, new[]
                {
                    Fields.jmcg_testentity_.jmcg_string, Fields.jmcg_testentity_.jmcg_date, Fields.jmcg_testentity_.jmcg_stringmultiline, Fields.jmcg_testentity_.jmcg_integer, Fields.jmcg_testentity_.jmcg_picklist
                }, new object[]
                {
                    "FAKE",null,"FAKE",1,null
                }, null, OptionSets.CalculatedField.SeparatorType.Pipe, ":", true, true, false)
            };

            var concatenateFieldRecord = CreateCalculatedFieldsForConfig(configs).First();

            var indexes = new List<int>();
            var target = new Entity(configs.First().EntityType);
            for (var i = 0; i < 5; i++)
            {
                indexes.Add(i);
            }
            foreach (var index in indexes)
            {
                target.SetField(configs.First().ConcatenateField[index], configs.First().ConcatenateValues1[index]);
            }
            target = CreateAndRetrieve(target);

            var loadConfig = CalculatedService.LoadCalculatedFieldConfig(concatenateFieldRecord);
            var calculatedValue = CalculatedService.GetNewValue(loadConfig, target.GetField);
            Assert.AreEqual("FAKE | FAKE | 1", calculatedValue);

            concatenateFieldRecord.SetField(Fields.jmcg_calculatedfield_.jmcg_separatorspacebefore, false);
            loadConfig = CalculatedService.LoadCalculatedFieldConfig(concatenateFieldRecord);
            calculatedValue = CalculatedService.GetNewValue(loadConfig, target.GetField);
            Assert.AreEqual("FAKE| FAKE| 1", calculatedValue);

            concatenateFieldRecord.SetField(Fields.jmcg_calculatedfield_.jmcg_separatorspaceafter, false);
            loadConfig = CalculatedService.LoadCalculatedFieldConfig(concatenateFieldRecord);
            calculatedValue = CalculatedService.GetNewValue(loadConfig, target.GetField);
            Assert.AreEqual("FAKE|FAKE|1", calculatedValue);

            concatenateFieldRecord.SetField(Fields.jmcg_calculatedfield_.jmcg_includeifempty, true);
            loadConfig = CalculatedService.LoadCalculatedFieldConfig(concatenateFieldRecord);
            calculatedValue = CalculatedService.GetNewValue(loadConfig, target.GetField);
            Assert.AreEqual("FAKE||FAKE|1|", calculatedValue);

            concatenateFieldRecord.SetOptionSetField(Fields.jmcg_calculatedfield_.jmcg_separatortype, OptionSets.CalculatedField.SeparatorType.OtherString);
            loadConfig = CalculatedService.LoadCalculatedFieldConfig(concatenateFieldRecord);
            calculatedValue = CalculatedService.GetNewValue(loadConfig, target.GetField);
            Assert.AreEqual("FAKE::FAKE:1:", calculatedValue);
        }

        [TestMethod]
        public void CalculatedFieldsConcatenateRecalculateAllTest()
        {
            DeleteAllCalculatedFields();

            var configs = new[]
            {
                new TestConcatenateConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, new[]
                {
                    Fields.jmcg_testentity_.jmcg_account, Fields.jmcg_testentity_.jmcg_date, Fields.jmcg_testentity_.jmcg_money, Fields.jmcg_testentity_.jmcg_decimal, Fields.jmcg_testentity_.jmcg_picklist
                }, new object[]
                {
                    TestContactAccount.ToEntityReference(), LocalisationService.TodayUnspecifiedType, new Money((decimal)111.11), (decimal)111.1, new OptionSetValue(OptionSets.TestEntity.Picklist.Option1)
                }, new object[]
                {
                    TestContactAccount.ToEntityReference(), LocalisationService.TodayUnspecifiedType.AddDays(1), new Money((decimal)222.22), (decimal)222.2, new OptionSetValue(OptionSets.TestEntity.Picklist.Option2)
                }, OptionSets.CalculatedField.SeparatorType.OtherString, ":", true, true, false)
            };

            foreach (var config in configs)
            {
                var indexes = new List<int>();
                for (var i = 0; i < 5; i++)
                {
                    indexes.Add(i);
                }

                var target1 = new Entity(config.EntityType);
                foreach (var index in indexes)
                {
                    target1.SetField(config.ConcatenateField[index], config.ConcatenateValues1[index]);
                }
                target1 = CreateAndRetrieve(target1);

                var target2 = new Entity(config.EntityType);
                foreach (var index in indexes)
                {
                    target2.SetField(config.ConcatenateField[index], config.ConcatenateValues2[index]);
                }
                target2 = CreateAndRetrieve(target2);

                var concatenateFieldRecord = CreateCalculatedFieldsForConfig(configs).First();

                concatenateFieldRecord.SetField(Fields.jmcg_calculatedfield_.jmcg_recalculateall, true);
                var updated = UpdateFieldsAndRetreive(concatenateFieldRecord, Fields.jmcg_calculatedfield_.jmcg_recalculateall);

                WaitTillTrue(() => !Refresh(concatenateFieldRecord).GetBoolean(Fields.jmcg_calculatedfield_.jmcg_isrecalculating), 60);

                target1 = Refresh(target1);
                var concatenated = target1.GetStringField(config.Field);
                var values = indexes.Select(i => XrmService.GetFieldAsDisplayString(config.EntityType, config.ConcatenateField[i], config.ConcatenateValues1[i], LocalisationService));
                Assert.AreEqual(string.Join(" : ", values), concatenated);

                target2 = Refresh(target2);
                var concatenated2 = target2.GetStringField(config.Field);
                values = indexes.Select(i => XrmService.GetFieldAsDisplayString(config.EntityType, config.ConcatenateField[i], config.ConcatenateValues2[i], LocalisationService));
                Assert.AreEqual(string.Join(" : ", values), concatenated2);
            }
        }

        private IEnumerable<Entity> CreateCalculatedFieldsForConfig(TestConcatenateConfig[] configs)
        {
            var results = new List<Entity>();
            foreach (var config in configs)
            {
                results.Add(CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Concatenate) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, config.Field },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, config.EntityType },
                    { Fields.jmcg_calculatedfield_.jmcg_field, config.Field },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield1, config.ConcatenateField != null && config.ConcatenateField.Length > 0 ? config.ConcatenateField[0] : null },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield2, config.ConcatenateField != null && config.ConcatenateField.Length > 1 ? config.ConcatenateField[1] : null },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield3, config.ConcatenateField != null && config.ConcatenateField.Length > 2 ? config.ConcatenateField[2] : null },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield4, config.ConcatenateField != null && config.ConcatenateField.Length > 3 ? config.ConcatenateField[3] : null },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield5, config.ConcatenateField != null && config.ConcatenateField.Length > 4 ? config.ConcatenateField[4] : null },
                    { Fields.jmcg_calculatedfield_.jmcg_concatenatefield6, config.ConcatenateField != null && config.ConcatenateField.Length > 5 ? config.ConcatenateField[5] : null },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(config.SeparatorType) },
                    { Fields.jmcg_calculatedfield_.jmcg_separatorstring, config.SeparatorString },
                    { Fields.jmcg_calculatedfield_.jmcg_separatorspacebefore, config.PrefixSpace },
                    { Fields.jmcg_calculatedfield_.jmcg_separatorspaceafter, config.SuffixSpace },
                    { Fields.jmcg_calculatedfield_.jmcg_includeifempty, config.IncludeEmpty }
                }));
            }
            return results;
        }

        private static TestRollupsConfig InitialiseTestEntityTwotoOneConfigs()
        {
            var typeRolledUpTo = Entities.jmcg_testentity;
            var typeRolledUp = Entities.jmcg_testentitytwo;
            var typeRolledUpReferenceField = Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference;
            var filterXml = "<filter type=\"and\"><condition attribute=\"statecode\" operator=\"eq\" value=\"0\" /><condition attribute=\"new_boolean\" operator=\"eq\" value=\"1\" /></filter>";

            //create configs for our tests to cover various rollup types
            return new TestRollupsConfig(typeRolledUpTo, typeRolledUp, typeRolledUpReferenceField, filterXml, new[]
             {
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_integersumtarget, Fields.jmcg_testentitytwo_.jmcg_integersumsource, 2, 7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_decimalsumtarget, Fields.jmcg_testentitytwo_.jmcg_decimalsumsource, (decimal)2222.2, (decimal)7777.7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_doublesumtarget, Fields.jmcg_testentitytwo_.jmcg_doublesumsource, (double)2.222, (double)7.777),
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_moneysumtarget, Fields.jmcg_testentitytwo_.jmcg_moneysumsource, new Money((decimal)2.22), new Money((decimal)7.77)),
                new TestRollupsConfig.TestRollupConfig(RollupType.Min, Fields.jmcg_testentity_.jmcg_integermintarget, Fields.jmcg_testentitytwo_.jmcg_integerminsource, 2, 7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Min, Fields.jmcg_testentity_.jmcg_decimalmintarget, Fields.jmcg_testentitytwo_.jmcg_decimalminsource, (decimal)2.2, (decimal)7.7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Min, Fields.jmcg_testentity_.jmcg_doublemintarget, Fields.jmcg_testentitytwo_.jmcg_doubleminsource, (double)2.222, (double)7.777),
                new TestRollupsConfig.TestRollupConfig(RollupType.Min, Fields.jmcg_testentity_.jmcg_moneymintarget, Fields.jmcg_testentitytwo_.jmcg_moneyminsource, new Money((decimal)2.22), new Money((decimal)7.77)),
                new TestRollupsConfig.TestRollupConfig(RollupType.Min, Fields.jmcg_testentity_.jmcg_datemintarget, Fields.jmcg_testentitytwo_.jmcg_dateminsource, new DateTime(2020,1,1, 0,0,0, DateTimeKind.Utc), new DateTime(2020,2,2, 0,0,0, DateTimeKind.Utc)),
                new TestRollupsConfig.TestRollupConfig(RollupType.Max, Fields.jmcg_testentity_.jmcg_integermaxtarget, Fields.jmcg_testentitytwo_.jmcg_integermaxsource, 2, 7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Max, Fields.jmcg_testentity_.jmcg_decimalmaxtarget, Fields.jmcg_testentitytwo_.jmcg_decimalmaxsource, (decimal)2.2, (decimal)7.7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Max, Fields.jmcg_testentity_.jmcg_doublemaxtarget, Fields.jmcg_testentitytwo_.jmcg_doublemaxsource, (double)2.222, (double)7.777),
                new TestRollupsConfig.TestRollupConfig(RollupType.Max, Fields.jmcg_testentity_.jmcg_moneymaxtarget, Fields.jmcg_testentitytwo_.jmcg_moneymaxsource, new Money((decimal)2.22), new Money((decimal)7.77)),
                new TestRollupsConfig.TestRollupConfig(RollupType.Max, Fields.jmcg_testentity_.jmcg_datemaxtarget, Fields.jmcg_testentitytwo_.jmcg_datemaxsource, new DateTime(2020,1,1, 0,0,0, DateTimeKind.Utc), new DateTime(2020,2,2, 0,0,0, DateTimeKind.Utc)),
                new TestRollupsConfig.TestRollupConfig(RollupType.Exists, Fields.jmcg_testentity_.jmcg_existstarget, Fields.jmcg_testentitytwo_.jmcg_name, "Testing 1", "Testing 2"),
                new TestRollupsConfig.TestRollupConfig(RollupType.Count, Fields.jmcg_testentity_.jmcg_counttarget, Fields.jmcg_testentitytwo_.jmcg_name, "Testing 1", "Testing 2"),
                new TestRollupsConfig.TestRollupConfig(RollupType.SeparatedStrings, Fields.jmcg_testentity_.jmcg_separatedstringtarget, Fields.jmcg_testentitytwo_.jmcg_separatedstringsource, "WTF", "WTS"),
                new TestRollupsConfig.TestRollupConfig(RollupType.First, Fields.jmcg_testentity_.jmcg_firsttarget, Fields.jmcg_testentitytwo_.jmcg_firstsource, 10, 20),
            });
        }

        private IEnumerable<Entity> CreateCalculatedFieldsForConfigs(TestRollupsConfig configs)
        {
            var results = new List<Entity>();

            //create calculated field records for each config
            foreach (var config in configs.TestRollupConfigs)
            {
                results.Add(CreateTestRecord(Entities.jmcg_calculatedfield, new Dictionary<string, object>
                {
                    { Fields.jmcg_calculatedfield_.jmcg_type, new OptionSetValue(OptionSets.CalculatedField.Type.Rollup) },
                    { Fields.jmcg_calculatedfield_.jmcg_rolluptype, new OptionSetValue((int)config.RollupType) },
                    { Fields.jmcg_calculatedfield_.jmcg_name, config.FieldTo },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytype, configs.TypeRolledUpTo },
                    { Fields.jmcg_calculatedfield_.jmcg_field, config.FieldTo },
                    { Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup, configs.TypeRolledUp },
                    { Fields.jmcg_calculatedfield_.jmcg_fieldrolledup, config.FieldFrom },
                    { Fields.jmcg_calculatedfield_.jmcg_fieldreferencing, configs.TypeRolledUpReferenceField },
                    { Fields.jmcg_calculatedfield_.jmcg_separatortype, new OptionSetValue(OptionSets.CalculatedField.SeparatorType.Comma) },
                    { Fields.jmcg_calculatedfield_.jmcg_rollupfilter, configs.FilterXml },
                    { Fields.jmcg_calculatedfield_.jmcg_orderrollupbyfieldordertype, new OptionSetValue(OptionSets.CalculatedField.OrderRollupByFieldOrderType.Ascending) }
                }));
            }
            return results;
        }

        [TestMethod]
        public void CalculatedFieldsRegistrationsTest()
        {
            DeleteAllCalculatedFields();

            CreateCalculatedFieldsForConfigs(new TestRollupsConfig(Entities.jmcg_testentity, Entities.jmcg_testentitytwo, Fields.jmcg_testentitytwo_.jmcg_testentityrollupreference, null, new[]
            {
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_integersumtarget, Fields.jmcg_testentitytwo_.jmcg_integersumsource, 2, 7),
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_decimalsumtarget, Fields.jmcg_testentitytwo_.jmcg_decimalsumsource, (decimal)2.2, (decimal)7.7),
            }));

            var calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(4, calculateFieldsEvents.Count());

            CreateCalculatedFieldsForConfigs(new TestRollupsConfig(Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Fields.jmcg_testentitythree_.jmcg_testentitytwo, null, new[]
            {
                new TestRollupsConfig.TestRollupConfig(RollupType.Count, Fields.jmcg_testentitytwo_.new_integer, Fields.jmcg_testentitythree_.jmcg_testentitytwo, 2, 7),
            }));

            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(8, calculateFieldsEvents.Count());

            CreateCalculatedFieldsForConfigs(new TestRollupsConfig(Entities.jmcg_testentity, Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_parenttestentity, null, new[]
            {
                new TestRollupsConfig.TestRollupConfig(RollupType.Count, Fields.jmcg_testentity_.jmcg_integer, Fields.jmcg_testentity_.jmcg_parenttestentity, 2, 7),
            }));

            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(11, calculateFieldsEvents.Count());

            CreateCalculatedFieldsForConfig(new[]
            {
                new TestConcatenateConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, new[]
                {
                    Fields.jmcg_testentity_.jmcg_string
                }, null, null, OptionSets.CalculatedField.SeparatorType.Pipe, ":", true, true, false),
                new TestConcatenateConfig(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_string, new[]
                {
                    Fields.jmcg_testentity_.jmcg_boolean
                }, null, null, OptionSets.CalculatedField.SeparatorType.Pipe, ":", true, true, false)
            });

            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(12, calculateFieldsEvents.Count());

            CreateCalculatedFieldsForConfig(new[]
            {
                new TestConcatenateConfig(Entities.jmcg_testentitytwo, Fields.jmcg_testentitytwo_.jmcg_name, new[]
                {
                    Fields.jmcg_testentitytwo_.new_boolean
                }, null, null, OptionSets.CalculatedField.SeparatorType.Pipe, ":", true, true, false),
            });

            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(13, calculateFieldsEvents.Count());

            var calculatedFields = XrmService.RetrieveAllEntityType(Entities.jmcg_calculatedfield);
            Assert.AreEqual(7, calculatedFields.Count());

            var twoToOneRollups = calculatedFields.Where(r => r.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup) == Entities.jmcg_testentitytwo).ToArray();

            XrmService.Delete(twoToOneRollups[0]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(13, calculateFieldsEvents.Count());

            XrmService.SetState(twoToOneRollups[1].LogicalName, twoToOneRollups[1].Id, OptionSets.CalculatedField.Status.Inactive);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(10, calculateFieldsEvents.Count());

            XrmService.SetState(twoToOneRollups[1].LogicalName, twoToOneRollups[1].Id, OptionSets.CalculatedField.Status.Active);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(13, calculateFieldsEvents.Count());

            XrmService.Delete(twoToOneRollups[1]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(10, calculateFieldsEvents.Count());

            var OneToOneRollups = calculatedFields.Where(r => r.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup) == Entities.jmcg_testentity).ToArray();

            XrmService.Delete(OneToOneRollups[0]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(7, calculateFieldsEvents.Count());

            var ThreeToTwoRollups = calculatedFields.Where(r => r.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytyperolledup) == Entities.jmcg_testentitythree).ToArray();

            XrmService.Delete(ThreeToTwoRollups[0]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(4, calculateFieldsEvents.Count());

            var concatOne = calculatedFields.Where(r => r.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype) == Entities.jmcg_testentity && r.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type) == OptionSets.CalculatedField.Type.Concatenate).ToArray();

            XrmService.Delete(concatOne[0]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(4, calculateFieldsEvents.Count());

            XrmService.Delete(concatOne[1]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(2, calculateFieldsEvents.Count());

            var concatTwo = calculatedFields.Where(r => r.GetStringField(Fields.jmcg_calculatedfield_.jmcg_entitytype) == Entities.jmcg_testentitytwo && r.GetOptionSetValue(Fields.jmcg_calculatedfield_.jmcg_type) == OptionSets.CalculatedField.Type.Concatenate).ToArray();

            XrmService.Delete(concatTwo[0]);
            calculateFieldsEvents = CalculatedService.GetCalculateFieldsEvents();
            Assert.AreEqual(0, calculateFieldsEvents.Count());
        }

        private void DeleteAllCalculatedFields()
        {
            //delete all existing calculations
            var calculatedFIelds = XrmService.RetrieveAllEntityType(Entities.jmcg_calculatedfield);
            foreach (var calculatedField in calculatedFIelds)
            {
                XrmService.Delete(calculatedField);
            }
        }

        public class TestRollupsConfig
        {
            public TestRollupsConfig(string typeRolledUpTo, string typeRolledUp, string typeRolledUpReferenceField, string filterXml, TestRollupConfig[] testRollupConfigs)
            {
                TypeRolledUpTo = typeRolledUpTo;
                TypeRolledUp = typeRolledUp;
                TypeRolledUpReferenceField = typeRolledUpReferenceField;
                FilterXml = filterXml;
                TestRollupConfigs = testRollupConfigs;
            }

            public string TypeRolledUpTo { get; }
            public string TypeRolledUp { get; }
            public string TypeRolledUpReferenceField { get; }
            public string FilterXml { get; }
            public TestRollupConfig[] TestRollupConfigs { get; }

            public class TestRollupConfig
            {
                public TestRollupConfig(RollupType rollupType, string fieldTo, string fieldFrom, object valueRollup1, object valueRollup2)
                {
                    RollupType = rollupType;
                    FieldTo = fieldTo;
                    FieldFrom = fieldFrom;
                    ValueRollup1 = valueRollup1;
                    ValueRollup2 = valueRollup2;
                }

                public RollupType RollupType
                {
                    get; set;
                }

                public string FieldTo
                {
                    get; set;
                }

                public string FieldFrom
                {
                    get; set;
                }

                public object ValueRollup1
                {
                    get; set;
                }

                public object ValueRollup2
                {
                    get; set;
                }
            }
        }

        public class TestConcatenateConfig
        {
            public TestConcatenateConfig(string entityType, string field, string[] concatenateField, object[] concatenateValues1, object[] concatenateValues2, int separatorType, string separatorString, bool prefixSpace, bool suffixSpace, bool includeEmpty)
            {
                EntityType = entityType;
                Field = field;
                ConcatenateField = concatenateField;
                ConcatenateValues1 = concatenateValues1;
                ConcatenateValues2 = concatenateValues2;
                SeparatorType = separatorType;
                PrefixSpace = prefixSpace;
                SeparatorString = separatorString;
                SuffixSpace = suffixSpace;
                IncludeEmpty = includeEmpty;
            }

            public string EntityType { get; set; }
            public string Field { get; set; }
            public string[] ConcatenateField { get; set; }
            public object[] ConcatenateValues1 { get; set; }
            public object[] ConcatenateValues2 { get; set; }
            public int SeparatorType { get; set; }
            public bool PrefixSpace { get; set; }
            public string SeparatorString { get; set; }
            public bool SuffixSpace { get; set; }
            public bool IncludeEmpty { get; set; }
        }

        public class TestAddTimeConfig
        {
            public TestAddTimeConfig(string entityType, string field, string fieldSource, int timeType, int timeAmount, Guid calendarId, bool useTime)
            {
                EntityType = entityType;
                Field = field;
                FieldSource = fieldSource;
                TimeType = timeType;
                TimeAmount = timeAmount;
                CalendarId = calendarId;
                UseTime = useTime;
            }

            public string EntityType { get; set; }
            public string Field { get; set; }
            public string FieldSource { get; set; }
            public int TimeType { get; set; }
            public int TimeAmount { get; set; }
            public Guid CalendarId { get; set; }
            public bool UseTime { get; set; }
        }
    }
}