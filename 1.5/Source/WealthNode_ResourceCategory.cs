using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_ResourceCategory : WealthNode
    {
        private ThingCategoryDef def;
        private List<WealthNode> subNodes;

        public WealthNode_ResourceCategory(Map map, int level, ThingCategoryDef def) : base(map, level)
        {
            this.def = def;
            subNodes = new List<WealthNode>();
            subNodes.AddRange(def.childCategories.Select(d => new WealthNode_ResourceCategory(map, level + 1, d)));
            subNodes.AddRange(def.childThingDefs.Select(d => new WealthNode_Item(map, level + 1, d)));
        }

        public override string Text => def.LabelCap;

        public override IEnumerable<WealthNode> Children => subNodes;

        public override bool Visible => subNodes.Any(n => n.Visible);

        public override float Value => subNodes.Sum(n => n.Value);

        public override Texture2D Icon => null;
    }
}
