using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_BuildingCategory : WealthNode
    {
        private static readonly HashSet<DesignationCategoryDef> openCategories = new HashSet<DesignationCategoryDef>();

        private readonly DesignationCategoryDef def;
        private readonly List<WealthNode> subNodes;

        public WealthNode_BuildingCategory(Map map, int level, DesignationCategoryDef def) : base(map, level)
        {
            this.def = def;
            subNodes = new List<WealthNode>();
            subNodes.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.designationCategory == def && ThingRequestGroup.BuildingArtificial.Includes(d)).Select(d => new WealthNode_Building(map, level + 1, d)));
            if (def == DesignationCategoryDefOf.Floors)
            {
                subNodes.AddRange(DefDatabase<TerrainDef>.AllDefsListForReading.Where(d => ((float[])typeof(WealthWatcher).Field("cachedTerrainMarketValue").GetValue(map.wealthWatcher))[d.index] > 0f).Select(d => new WealthNode_Floor(map, level + 1, d)));
            }
            Open = openCategories.Contains(def);
        }

        public override string Text => def.LabelCap;

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float Value => subNodes.Sum(n => n.Value);

        public override void OnOpen()
        {
            openCategories.Add(def);
        }

        public override void OnClose()
        {
            openCategories.Remove(def);
        }
    }
}
