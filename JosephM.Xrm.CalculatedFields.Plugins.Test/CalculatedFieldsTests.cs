﻿using JosephM.Xrm.CalculatedFields.Plugins.Rollups;
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

            foreach(var calculated in calculatedFields)
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
            }
        }

        [TestMethod]
        public void CalculatedFieldsConcatenatePluginTest()
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

            var concatenateFieldRecord = CreateCalculatedFieldsForConfig(configs).First(); ;

            foreach (var config in configs)
            {
                var indexes = new List<int>();
                var target = new Entity(config.EntityType);
                for (var i = 0; i < 5; i++)
                {
                    indexes.Add(i);
                }
                foreach(var index in indexes)
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
                new TestRollupsConfig.TestRollupConfig(RollupType.Sum, Fields.jmcg_testentity_.jmcg_decimalsumtarget, Fields.jmcg_testentitytwo_.jmcg_decimalsumsource, (decimal)2.2, (decimal)7.7),
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
                    { Fields.jmcg_calculatedfield_.jmcg_rollupfilter, configs.FilterXml }
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
            public TestRollupsConfig(string typeRolledUpTo, string typeRolledUp, string typeRolledUpReferenceField, string filterXml,TestRollupConfig[] testRollupConfigs)
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
    }
}