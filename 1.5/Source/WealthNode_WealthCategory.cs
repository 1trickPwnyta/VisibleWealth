using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_WealthCategory : WealthNode
    {
        private static HashSet<WealthCategory> openCategories = new HashSet<WealthCategory>();

        private readonly WealthCategory category;
        private readonly List<WealthNode> subNodes;

        public WealthNode_WealthCategory(Map map, int level, WealthCategory category) : base(map, level)
        {
            this.category = category;
            switch (category)
            {
                case WealthCategory.Items:
                    subNodes = ThingCategoryDefOf.Root.childCategories.Select(d => new WealthNode_ResourceCategory(map, level + 1, d) as WealthNode).ToList();
                    break;
                case WealthCategory.Buildings:
                    subNodes = new List<WealthNode>();
                    subNodes.AddRange(DefDatabase<DesignationCategoryDef>.AllDefsListForReading.Select(d => new WealthNode_BuildingCategory(map, level + 1, d) as WealthNode));
                    subNodes.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.designationCategory == null && ThingRequestGroup.BuildingArtificial.Includes(d)).Select(d => new WealthNode_Building(map, level + 1, d)));
                    break;
            }
            Open = openCategories.Contains(category);
        }

        public override string Text => category.Label();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => true;

        public override float Value => subNodes.Sum(n => n.Value);

        public override void OnOpen()
        {
            openCategories.Add(category);
        }

        public override void OnClose()
        {
            openCategories.Remove(category);
        }
    }
}
