using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_WealthCategory : WealthNode
    {
        private static readonly HashSet<WealthCategory> openCategories = new HashSet<WealthCategory>();

        private readonly WealthCategory category;
        private readonly List<WealthNode> subNodes;

        public WealthNode_WealthCategory(Map map, int level, WealthCategory category) : base(map, level)
        {
            this.category = category;
            subNodes = new List<WealthNode>();
            switch (category)
            {
                case WealthCategory.Items:
                    subNodes.AddRange(ThingCategoryDefOf.Root.childCategories.Select(d => new WealthNode_ResourceCategory(map, level + 1, d)));
                    break;
                case WealthCategory.Buildings:
                    subNodes.AddRange(DefDatabase<DesignationCategoryDef>.AllDefsListForReading.Select(d => new WealthNode_BuildingCategory(map, level + 1, d)));
                    subNodes.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.designationCategory == null && ThingRequestGroup.BuildingArtificial.Includes(d)).Select(d => new WealthNode_Building(map, level + 1, d)));
                    break;
                case WealthCategory.Pawns:
                    foreach (PawnCategory pawnCategory in typeof(PawnCategory).GetEnumValues())
                    {
                        subNodes.Add(new WealthNode_PawnCategory(map, level + 1, pawnCategory));
                    }
                    break;
                case WealthCategory.PocketMaps:
                    subNodes.AddRange(Find.World.pocketMaps.Where(p => p.HasMap && p.sourceMap == map).Select(p => new WealthNode_PocketMap(map, level + 1, p.Map)));
                    break;
                default: throw new NotImplementedException("Invalid wealth category.");
            }
            Open = openCategories.Contains(category);
        }

        public override string Text => category.Label();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => category != WealthCategory.PocketMaps || subNodes.Any(n => n.Visible);

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
