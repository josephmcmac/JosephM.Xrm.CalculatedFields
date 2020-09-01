using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace JosephM.Xrm.CalculatedFields.Plugins.Rollups
{
    /// <summary>
    /// Configuration object for a custom rollup/Rollup field
    /// Use the RollupService to process the Rollup in plugins
    /// For each configured type you will need plugin triggers registered for
    /// The type containing the rollup field on preoperation create - to initialise its value
    /// The type containing the field Rollupd on postevent create synch, postevent update synch, postevent delete synch - to process changes
    /// the preimage will need to contain all dependency fields
    /// </summary>
    public class LookupRollup
    {
        public LookupRollup(string recordTypeWithRollup, string rollupField, string recordTypeRolledup,
            string fieldRolledUp, RollupType rollupType, string lookupName, Type objectType, string separatorString)
        {
            LookupName = lookupName;
            ObjectType = objectType;
            SeparatorString = separatorString;
            RecordTypeWithRollup = recordTypeWithRollup;
            RollupField = rollupField;
            RecordTypeRolledup = recordTypeRolledup;
            RollupType = rollupType;
            FieldRolledup = fieldRolledUp;
            if (ObjectType != null)
            {
                if(rollupType == RollupType.Count
                    || rollupType == RollupType.Exists
                    || rollupType == RollupType.Sum)
                if (ObjectType == typeof(decimal))
                    NullAmount = (decimal)0;
                else if (ObjectType == typeof(int))
                    NullAmount = (int)0;
                else if (ObjectType == typeof(Money))
                    NullAmount = new Money(0);
                else if (ObjectType == typeof(bool))
                    NullAmount = false;
            }
        }

        public Type ObjectType { get; set; }
        public string SeparatorString { get; set; }
        public string LookupName { get; set; }
        public string RecordTypeWithRollup { get; set; }
        public string RollupField { get; set; }
        public string RecordTypeRolledup { get; set; }
        public RollupType RollupType { get; set; }
        public FilterExpression Filter { get; set; }
        public string FilterXml { get; set; }
        public string FieldRolledup { get; set; }
        public object NullAmount { get; set; }
    }
}