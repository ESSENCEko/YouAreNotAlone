using GTA;
using GTA.Math;
using System.Collections.Generic;

namespace YouAreNotAlone
{
    public class SWAT : EmergencyCar
    {
        public SWAT(string name, Entity target) : base(name, target) { }

        public override bool IsCreatedIn(Vector3 safePosition, List<string> models) { return IsCreatedIn(safePosition, models, "SWAT"); }
    }
}