using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_BuildingCategory : WealthNode
    {
        private static HashSet<DesignationCategoryDef> openCategories = new HashSet<DesignationCategoryDef>();

        private readonly DesignationCategoryDef def;
        private readonly List<WealthNode_Building> subNodes;

        public WealthNode_BuildingCategory(Map map, int level, DesignationCategoryDef def) : base(map, level)
        {
            this.def = def;
            subNodes = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.designationCategory == def && ThingRequestGroup.BuildingArtificial.Includes(d)).Select(d => new WealthNode_Building(map, level + 1, d)).ToList();
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
