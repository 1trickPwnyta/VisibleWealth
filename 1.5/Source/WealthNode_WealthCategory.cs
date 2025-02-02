using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_WealthCategory : WealthNode
    {
        private List<WealthNode_ResourceCategory> subNodes;

        public WealthNode_WealthCategory(Map map, int level, WealthCategory category) : base(map, level)
        {
            switch (category)
            {
                case WealthCategory.Items:
                    subNodes = ThingCategoryDefOf.Root.childCategories.Select(d => new WealthNode_ResourceCategory(map, level + 1, d)).ToList();
                    break;
            }
        }

        public override string Text => "VisibleWealth_ItemsWealth".Translate();

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => true;

        public override float Value => subNodes.Sum(n => n.Value);

        public override Texture2D Icon => null;
    }
}
