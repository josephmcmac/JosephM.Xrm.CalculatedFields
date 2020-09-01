using JosephM.Xrm.CalculatedFields.Plugins.Xrm;
using System.Collections.Generic;

namespace JosephM.Xrm.CalculatedFields.Plugins.Rollups
{
    public class CalculatedRollupService : RollupService
    {
        public CalculatedRollupService(XrmService xrmService, IEnumerable<LookupRollup> rollups)
            : base(xrmService)
        {
            _rollups = rollups;
        }

        private IEnumerable<LookupRollup> _rollups;

        public override IEnumerable<LookupRollup> AllRollups => _rollups;
    }
}