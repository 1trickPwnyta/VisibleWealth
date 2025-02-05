using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VisibleWealth
{
    public class WealthNode_ResourceCategory : WealthNode
    {
        private static readonly HashSet<ThingCategoryDef> openCategories = new HashSet<ThingCategoryDef>();

        private readonly ThingCategoryDef def;
        private readonly List<WealthNode> subNodes;

        public WealthNode_ResourceCategory(Map map, int level, ThingCategoryDef def) : base(map, level)
        {
            this.def = def;
            subNodes = new List<WealthNode>();
            subNodes.AddRange(def.childCategories.Select(d => new WealthNode_ResourceCategory(map, level + 1, d)));
            subNodes.AddRange(def.childThingDefs.Select(d => new WealthNode_Item(map, level + 1, d)));
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
